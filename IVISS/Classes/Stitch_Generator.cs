using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Threading;

namespace IVISS.Classes
{
    class Stitch_Generator
    {
        
        Bitmap stitchedImage;

        private BlockingCollection<Bitmap> image_queue;

        Image<Gray, byte> W1;
        Image<Gray, byte> W2;
        Image<Gray, byte> W3;
        Image<Gray, byte> W4;

        int[] x_dat;
        int[] y_dat;

        int w, h;

        PictureBox pictureBox;

        Object lock_obj = new Object();

        // Constructor
        public Stitch_Generator()
        {
            // TODO: pictureBox = pictureBox_In;

            // Initilaizes and start the thing
            image_queue = new BlockingCollection<Bitmap>();

            //TODO 5: Coonvert to 2d arrays for better access
            W1 = new Image<Gray, byte>("W1.pgm");
            W2 = new Image<Gray, byte>("W2.pgm");
            W3 = new Image<Gray, byte>("W3.pgm");
            W4 = new Image<Gray, byte>("W4.pgm");

            w = W1.Width;
            h = W1.Height;

            //pictureBox.Image = W1.Bitmap;

            //TODO 5: Coonvert to 2d arrays for better access
            var length = W1.Height * W1.Width;
            x_dat = new int[length];
            y_dat = new int[length];

            using (var reader = new BinaryReader(File.Open("XLow.dat", FileMode.Open)))
            {
                for (int i = 0; i < length; i++)
                    x_dat[i] = reader.ReadInt32();
            }

            using (var reader = new BinaryReader(File.Open("YLow.dat", FileMode.Open)))
            {
                for (int i = 0; i < length; i++)
                    y_dat[i] = reader.ReadInt32();
            }

            Task.Factory.StartNew(() => {
                start_processing();
            });
        }

        private void start_processing()
        {
            Task.Factory.StartNew(() => {
                //undistort_all_frames();
                blend_images();
            });
        }

        public void AddImage(Bitmap img)
        {
            image_queue.Add(img);
        }

        // todo: Use Bitmap type
        private void undistort_one_frame(Bitmap image_in, Image<Bgr, byte> CBOut, Image<Gray, byte> BOut)
        //private void undistort_one_frame(Bitmap image_in, Bitmap CBOut, Bitmap BOut)
        {
            //Image<Bgr, byte> CBOut = new Image<Bgr, byte>(W1.Width, W1.Height);       // Undistorted (color)
            //Image<Gray, byte> BOut = new Image<Gray, byte>(W1.Width, W1.Height);       // Undistorted (grey)

            Bitmap bmp = image_in;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, image_in.Width, image_in.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe 
            {
                int i, j, w1, w2, w3, w4, xl, xh, yl, yh;
                int width_orig_img = image_in.Width;

                byte* ptr = (byte*)data.Scan0;

                for (i = 0; i < h; i++)
                {
                    for (j = 0; j < w; j++)
                    {
                        //TODO 8: Coonvert to 2d arrays for better access
                        w1 = W1.Data[i, j, 0];
                        w2 = W2.Data[i, j, 0];
                        w3 = W3.Data[i, j, 0];
                        w4 = W4.Data[i, j, 0];

                        xh = y_dat[i * w + j];
                        xl = xh - 1;

                        yh = x_dat[i * w + j];
                        yl = yh - 1;

                        byte tmp;

                        tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 0] +
                                                               w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 0] +
                                                               w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 0] +
                                                               w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 0]) >> 7));
                        CBOut.Data[i, j, 0] = (byte)tmp;

                        tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 1] +
                                                               w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 1] +
                                                               w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 1] +
                                                               w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 1]) >> 7));
                        CBOut.Data[i, j, 1] = (byte)tmp;

                        tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 2] +
                                                               w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 2] +
                                                               w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 2] +
                                                               w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 2]) >> 7));
                        CBOut.Data[i, j, 2] = (byte)tmp;
                    }

                    // Convert to grey-scale image
                    // TODO 10: Make this line right (Make a grey-scale image)
                    //BOut.Data[i, j, 0] = (byte)((38 * CBOut.Data[i, j, 0] + 75 * CBOut.Data[i, j, 1] + 15 * CBOut.Data[i, j, 2]) >> 7); // >>7 is division by 128
                }
            }



            //image_in.Bitmap.UnlockBits(data);
            bmp.UnlockBits(data);

            // TODO: Uncomment (Perhaps unnedded)
            /*
            for (j = 0; j < w; j++)
            {
                CBOut.Data[0, j, 0] = CBOut.Data[1, j, 0];
                CBOut.Data[0, j, 1] = CBOut.Data[1, j, 1];
                CBOut.Data[0, j, 2] = CBOut.Data[1, j, 2];
            }
            */

            //return CBOut;
        }

        void undistort_all_frames()
        {
            // TODO 9: Use "Dataflow (Task Parallel Library)" to make it run in parallel

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Image<Bgr, byte> CBOut = new Image<Bgr, byte>(W1.Width, W1.Height);       // Undistorted (color)
            Image<Gray, byte> BOut = new Image<Gray, byte>(W1.Width, W1.Height);       // Undistorted (grey)

            var stitched_image = new Image<Bgr, byte>(W1.Width, W1.Height);

            int i = 0;

            lock(lock_obj)
            {
                // TODO 9: Run loop until queue is empty
                //for (int i = 0; i < frame_count; i++)
                while (!image_queue.IsCompleted)
                {
                    if (i % 200 == 0)
                        Console.WriteLine("\t\t\t Frames taken {0}: ", i);

                    string name1 = String.Format(@"images\{0}.jpg", i);


                    // This section works
                    // Take images from queue
                    Bitmap distorted_img = image_queue.Take();
                    //Bitmap distorted_img = new Bitmap(image_queue_2.Take());

                    // Create undistorted image
                    undistort_one_frame(distorted_img, CBOut, BOut);

                    //CBOut.Save(name1);

                    // Save both 
                    //image_color_undistorted[i] = new Image<Bgr, byte>(CBOut.Data);
                    //image_grey_undistorted[i] = new Image<Gray, byte>(BOut.Data);


                    if (i % 10 == 0)
                    {
                        //! Stack undistored frames
                        // todo 7: Memory leak in this line
                        stitched_image = stitched_image.ConcateVertical(CBOut);

                        //stitched_image.Save(name1);

                        // todo 9: Uncomment this one
                        /*
                        //! Display images
                        //pictureBox2.Image = image_color_undistorted[i].Bitmap;
                        pictureBox.Image = CBOut.Bitmap;
                        pictureBox.Refresh();
                        pictureBox.Invalidate();
                        pictureBox.Update();
                        */
                    }


                    //! Save image to file
                    //image_color_undistorted[i].Save(name1);
                    //image_grey_undistorted[i].Save(name1);

                    // Dispose original image
                    distorted_img.Dispose();


                    // todo 9: Uncomment this one
                    /*
                    // Take images from queue
                    using (var distorted_img = image_queue_2.Take())
                    {
                        //? Allocation with every loop makes things a little slow
                        // todo 7: Try to allocate just once
                        // todo 7: Convert image typ to Bitmap (this will speed things up)
                        //? This doesn't get disposed automatically. Why?
                        Image<Bgr,  byte> CBOut = new Image<Bgr,  byte>(W1.Width, W1.Height);       // Undistorted (color)
                        Image<Gray, byte> BOut  = new Image<Gray, byte>(W1.Width, W1.Height);       // Undistorted (grey)

                        // Create undistorted image
                        undistort_one_frame(distorted_img, CBOut, BOut);

                        // todo 9: Add these to another queue
                        //image_color_undistorted[i] = new Image<Bgr, byte>(CBOut.Data);
                        //image_grey_undistorted[i] = new Image<Gray, byte>(BOut.Data);

                    

                        if (i % 10 == 0)
                        {
                            //! Stack undistored frames
                            // todo 7: Memory leak in this line
                            //stitched_image = stitched_image.ConcateVertical(CBOut);

                            //! Display images
                            
                            //pictureBox2.Image = image_color_undistorted[i].Bitmap;
                            pictureBox2.Image = CBOut.Bitmap;
                            pictureBox2.Refresh();
                            pictureBox2.Invalidate();
                            pictureBox2.Update();
                            
                        }

                        //! Save image to file
                        //image_color_undistorted[i].Save(name1);
                        //image_grey_undistorted[i].Save(name1);
                    }
                    //CBOut.Dispose();
                    */

                    i++;
                }

                stitchedImage = stitched_image.ToBitmap();

            }

            sw.Stop();
            Console.WriteLine("Undistort whole video time = {0}", sw.ElapsedMilliseconds);
        }

        private void calculate_translation()
        {

        }

        // TODO 6: Perform blend instead of stack
        // TODO 6: Take images from undistorted queue
        private void blend_images()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //var stitched_image = new Image<Bgr, byte>(W1.Width, W1.Height);

            Image<Bgr, byte> stitched_image_1 = new Image<Bgr, byte>(2048, 0);
            Image<Bgr, byte> stitched_image_2 = new Image<Bgr, byte>(2048, 0);

            bool turn = true;

            int i = 0;

            lock (lock_obj)
            {
                // Runs loop until queue is empty
                //for (int i = 0; i < frame_count; i++)
                while (!image_queue.IsCompleted)
                {
                    if (i % 200 == 0)
                        Console.WriteLine("\t\t\t Frames taken {0}: ", i);

                    string name1 = String.Format(@"images\{0}.jpg", i);

                    // This section works fine
                    // Take images from queue
                    try
                    {
                        Bitmap distorted_img = image_queue.Take();
                    
                        if (i % 10 == 0)
                        {
                            //! Stack undistored frames
                            if (turn)
                            {
                                stitched_image_2 = stitched_image_1.ConcateVertical(new Image<Bgr, byte>(distorted_img));
                                stitched_image_1.Dispose();

                                turn = false;
                            }
                            else
                            {
                                stitched_image_1 = stitched_image_2.ConcateVertical(new Image<Bgr, byte>(distorted_img));
                                stitched_image_2.Dispose();

                                turn = true;
                            }
                        }
                    
                        // Dispose original image
                        distorted_img.Dispose();

                        i++;
                    }
                    catch(InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Take exception. Things will go fine");
                        break;
                    }
                }

                if (turn)
                {
                    stitchedImage = stitched_image_1.ToBitmap();
                }
                else
                {
                    stitchedImage = stitched_image_2.ToBitmap();
                }

            }

            sw.Stop();
            Console.WriteLine("Undistort whole video time = {0}", sw.ElapsedMilliseconds);
        }

        public Bitmap GetStitchResult()
        {
            // Done adding images to the queue
            image_queue.CompleteAdding();

            lock (lock_obj)
            {
                // Block until queue not empty
                while (!image_queue.IsCompleted)
                {
                    Thread.Sleep(1);
                }

                // todo 6: Dispose and reset the queue
                image_queue.Dispose();

                return stitchedImage;
            }

            ////return stitched_image.ToBitmap();
        }
    }
}
