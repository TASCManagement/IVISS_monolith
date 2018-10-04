using AForge.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IVISS
{
    class Player
    {

        private static Bitmap[] videoFrame, videoFrame2, videoFrame3, videoFrame4;
        private static long frameCount = 0;
        public static int framePosition = 0;
        private static bool fwdConditionToExit=false, revConditionToExit=false;
        private static int TOTAL_CAM = 4;

        public static bool CopyVideo()
        {
            // create instance of video reader
            VideoFileReader[] reader = new VideoFileReader[TOTAL_CAM];

            for (int i = 0; i < TOTAL_CAM; i++)
                reader[i] = new VideoFileReader();

            // open video file
            reader[0].Open("C:\\vittemp\\Brd_0_Ch_02_0.mov");
            reader[1].Open("C:\\vittemp\\Brd_0_Ch_03_0.mov");
            reader[2].Open("C:\\vittemp\\Brd_0_Ch_04_0.mov");
            reader[3].Open("C:\\vittemp\\Brd_0_Ch_05_0.mov");

            videoFrame = new Bitmap[reader[0].FrameCount];
            videoFrame2 = new Bitmap[reader[1].FrameCount];
            videoFrame3 = new Bitmap[reader[2].FrameCount];
            videoFrame4 = new Bitmap[reader[3].FrameCount];

            frameCount = reader[0].FrameCount;

            new System.Threading.Thread(() =>
            {
                for (int i = 0; i < reader[0].FrameCount; i++)
                {
                    videoFrame[i] = (Bitmap)reader[0].ReadVideoFrame();
                }
                reader[0].Close();

            }).Start();

            new System.Threading.Thread(() =>
            {
                for (int i = 0; i < reader[1].FrameCount; i++)
                {
                    videoFrame2[i] = (Bitmap)reader[1].ReadVideoFrame();
                }
                reader[1].Close();

            }).Start();

            new System.Threading.Thread(() =>
            {
                for (int i = 0; i < reader[2].FrameCount; i++)
                {
                    videoFrame3[i] = (Bitmap)reader[2].ReadVideoFrame();
                }
                reader[2].Close();

            }).Start();

            new System.Threading.Thread(() =>
            {
                for (int i = 0; i < reader[3].FrameCount; i++)
                {
                    videoFrame4[i] = (Bitmap)reader[3].ReadVideoFrame();
                }
                reader[3].Close();

            }).Start();

            return true;
            //txtStatus.Text = "Copied";
        }

        public static bool Play(PictureBox pBox1, PictureBox pBox2, PictureBox pBox3, PictureBox pBox4)
        {
            fwdConditionToExit = false;
            revConditionToExit = true;
            //System.Threading.Thread.Sleep(100);

            new System.Threading.Thread(() =>
            {
                // read 100 video frames out of it
                for (int i = framePosition; i < frameCount; i++)
                {

                    if (fwdConditionToExit)
                    {
                        fwdConditionToExit = false;
                        break;
                    }

                    //videoFrame[i] = reader.ReadVideoFrame();
                    // process the frame somehow
                    // ...

                    if (i < videoFrame.Length && videoFrame[i] != null)
                        pBox1.BackgroundImage = (Bitmap)videoFrame[i].Clone();

                    if (i < videoFrame2.Length && videoFrame2[i] != null)
                        pBox2.BackgroundImage = (Bitmap)videoFrame2[i].Clone();

                    if (i < videoFrame3.Length && videoFrame3[i] != null)
                        pBox3.BackgroundImage = (Bitmap)videoFrame3[i].Clone();

                    if (i < videoFrame4.Length && videoFrame4[i] != null)
                        pBox4.BackgroundImage = (Bitmap)videoFrame4[i].Clone();

                    System.Threading.Thread.Sleep(100);
                    // dispose the frame when it is no longer required
                    framePosition = i;
                }
                //txtStatus.Text = "Play Forward Completed";

            }).Start();
            //videoFrame.Dispose();

            return true;
        }

        public static bool Rewind(PictureBox pBox1, PictureBox pBox2, PictureBox pBox3, PictureBox pBox4)
        {
            revConditionToExit = false;
            fwdConditionToExit = true;
            //System.Threading.Thread.Sleep(100);

            new System.Threading.Thread(() =>
            {
                int i = 0;

                // read 100 video frames out of it
                for (i = framePosition - 1; i >= 0; i--)
                {
                    if (revConditionToExit)
                    {
                        revConditionToExit = false;
                        break;
                    }

                    //videoFrame[i] = reader.ReadVideoFrame();
                    // process the frame somehow
                    // ...

                    if (videoFrame[i] !=null)
                        pBox1.Image = (Bitmap)videoFrame[i].Clone();

                    if (videoFrame2[i] != null)
                        pBox2.Image = (Bitmap)videoFrame2[i].Clone();

                    if (videoFrame3[i] != null)
                        pBox3.Image = (Bitmap)videoFrame3[i].Clone();

                    if (videoFrame4[i] != null)
                        pBox4.Image = (Bitmap)videoFrame4[i].Clone();

                    System.Threading.Thread.Sleep(30);

                    // dispose the frame when it is no longer required
                    framePosition = i;
                }

                //txtStatus.Text = "Play Backward Completed";

            }).Start();

            return true;
        }

        public static bool StepForward(PictureBox pBox1, PictureBox pBox2, PictureBox pBox3, PictureBox pBox4)
        {
            int i = framePosition;

            i++;

            if (i < videoFrame.Length && videoFrame[i] != null)
                pBox1.BackgroundImage = (Bitmap)videoFrame[i].Clone();

            if (i < videoFrame2.Length && videoFrame2[i] != null)
                pBox2.BackgroundImage = (Bitmap)videoFrame2[i].Clone();

            if (i < videoFrame3.Length && videoFrame3[i] != null)
                pBox3.BackgroundImage = (Bitmap)videoFrame3[i].Clone();

            if (i < videoFrame4.Length && videoFrame4[i] != null)
                pBox4.BackgroundImage = (Bitmap)videoFrame4[i].Clone();

            // dispose the frame when it is no longer required
            framePosition = i;

            return true;
        }

        public static bool StepBackward(PictureBox pBox1, PictureBox pBox2, PictureBox pBox3, PictureBox pBox4)
        {
            int i = framePosition;

            i--;

            pBox1.Image = (Bitmap)videoFrame[i].Clone();
            pBox2.Image = (Bitmap)videoFrame2[i].Clone();
            pBox3.Image = (Bitmap)videoFrame3[i].Clone();
            pBox4.Image = (Bitmap)videoFrame4[i].Clone();

            // dispose the frame when it is no longer required
            framePosition = i;

            return true;
        }

        public static void Stop()
        {
            fwdConditionToExit = true;
            revConditionToExit = true;
        }

        public static void Clear()
        {
            for (int i = 0; i < frameCount; i++)
            {
                if (i < videoFrame.Length && videoFrame[i] != null)
                    videoFrame[i].Dispose();

                if (i < videoFrame2.Length && videoFrame2[i] != null)
                    videoFrame2[i].Dispose();

                if (i < videoFrame3.Length && videoFrame3[i] != null)
                    videoFrame3[i].Dispose();

                if (i < videoFrame4.Length && videoFrame4[i] != null)
                    videoFrame4[i].Dispose();
            }
        }

    }
}
