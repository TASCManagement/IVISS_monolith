using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace IVISS.Classes
{
    class Stitch
    {
        string name1 = @"C:\IVISSTemp\m1.avi";

        Capture _capture1 = null;

        int frame_count;

        public string recordingPath { get; set; }

        private BlockingCollection<Bitmap> image_queue;

        private void load_video_file()
        {
            try
            {
                _capture1 = new Capture(name1);

                //TODO 2: Make frame count right
                frame_count = (int)_capture1.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_COUNT) / 1;

                Console.WriteLine("Frame count 1:{0} \n", frame_count);
            }
            catch (NullReferenceException except)
            {
                //Image<Bgr, byte> image = new Image<Bgr, byte>("C:\\CombineImage\\outPutVer.ppm");
                //imageBox1.Image = image;
                return;
            }

            //stitched_image.FunctionalMode = ImageBox.FunctionalModeOption.Minimum;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //sw.Stop();
            Console.WriteLine("Initialization time = {0}", sw.ElapsedMilliseconds);
        }

        private void decode_all_frames_and_add_to_queue(Stitch_Generator stitch_obj)
        {
            Image<Bgr, byte> image1;
            Bitmap tmp_img;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < frame_count; i++)
            {
                if (i % 200 == 0)
                    Console.WriteLine("Frames added {0}: ", i);

                //! Works like a charm!
                //! (i.e. decodes at full speed, and not illegal memory accesses, and disposes everything automaticaly
                using (var image3 = _capture1.QueryFrame())
                {
                    ////var tmp_img_2 = new Bitmap(image.Bitmap);
                    var tmp_img3 = image3.ToBitmap();
                    stitch_obj.AddImage(tmp_img3);

                    /* TODO
                    if (i % 10 == 0)
                    {
                        //! Display images
                        pictureBox1.Image = image3.Bitmap;
                        pictureBox1.Refresh();
                        pictureBox1.Invalidate();
                    }
                    */ 
                }

                //System.Threading.Thread.Sleep(10);
            }

            //image_queue_2.CompleteAdding();

            sw.Stop();
            Console.WriteLine("Video decode time = {0}", sw.ElapsedMilliseconds);

            // Reset video to start
            _capture1.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
        }

        public Bitmap Create()
        {
            load_video_file();

            Stitch_Generator stitch_obj = new Stitch_Generator();

            decode_all_frames_and_add_to_queue(stitch_obj);

            Bitmap result = stitch_obj.GetStitchResult();

            //pictureBox3.Image = result;
            result.Save(@"C:\IVISSTemp\stitched_image.jpg", ImageFormat.Jpeg);

            return result;
        }
    }
}
