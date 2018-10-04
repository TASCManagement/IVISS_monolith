using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using gx;
using cm;
using System.Collections.Concurrent;

namespace IVISS.Classes
{

    /** Structure that contains ANPR result. It is used by the 'anpr_result()' event. */
    public struct ANPR_RESULT_STRUCT
    {
        /** recognized plate text */
        public string text;

        /** recognized plate text */
        public string textAscii;
        
        /** Frame of the number plate. (coordinates on the image) */
        public gxPG4 frame;
        
        /** The result is given on this image. */
        public Bitmap image;
        
        /** Number Plate Accuracy **/
        public string accuracy;
        
        /** Number Plate Origin **/
        public int lpOrigin;
       
        public string transactionStatus;

        /** Number Plate Color **/
        public string plateColor;
        
        /** Number Plate Aub Color **/
        public string plateSubColor;
    }

    /** delegate for the anpr_result event of the ANPR_Thread class
     * @param result Structure that contains the result.
     */
    public delegate void ANPR_Result(ANPR_RESULT_STRUCT result);

    /** Runs a thread that does the plate recognition. When a result is ready raises the anpr_result() event. */
    class ANPR_Thread
    {
        // declare blocking collection backed by concurrent queue
        BlockingCollection<Bitmap> q = new BlockingCollection<Bitmap>(10);

        /** this event is raised when a plate is found on the image */
        public event ANPR_Result anpr_result;

        /** cmAnpr module can be accessed through this object */
        cmAnpr anpr;

        /** Plate recognition is done in this thread. */
        Thread thread;

        /** Exit condition of the thread's loop */
        bool exit_flag;
      
        /** gxImage is needed for the FindFirst method of the cmAnpr class */
        gxImage image;

        /** Bitmap from which the gxImage was created */
        Bitmap bitmap;

        /** object used for synchronization, protects the 'image' and 'bitmap' members */
        Object sync_obj;

        /** with this event we can signal to the thread that it should run the ANPR process on the current 'image' */
        AutoResetEvent evt;

        private Object thisLock = new Object();

        public ANPR_Thread()
        {
            
            try
            {
                sync_obj = new Object();
                evt = new AutoResetEvent(false);
                anpr = new cmAnpr();
                
                //anpr = new cmAnpr();
                //anpr.SetProperty("anprname", "cmanpr-7.2.7.105");
                
                thread = new Thread(ThreadFunc);

                exit_flag = false;
                thread.Start();

            } 
            catch (gxException ex)
            {
                // create a writer and open the file
                TextWriter tw = new StreamWriter("error.txt");

                // write a line of text to the file
                tw.WriteLine(ex.ToString());

                // close the stream
                tw.Close();
            }
        }

        /** Thread Function. The ANPR process is done in this thread. */
        void ThreadFunc()
        {
            //gxImage local_image;
            //q.TryTake(out bitmap);
            //return;
            //Bitmap local_bitmap = null;

            while (!exit_flag)
            {
                // This blocks until an item appears in the queue.
                
                //local_bitmap = q.Take();

                using (Bitmap local_bitmap = q.Take())
                {

                    // Process the request here.
                    //local_bitmap = b;
                    if (local_bitmap == null)
                        return;

                    //convert Bitmap to gxImage
                    //1. save bitmap to memory stream
                    MemoryStream ms = new MemoryStream();
                    local_bitmap.Save(ms, ImageFormat.Bmp);

                    // 2. create byte array from the memory stream
                    byte[] buffer = ms.ToArray();

                    //lock (thisLock)
                    //{

                        try
                        {
                            //gxImage gximg = new gxImage();
                            using (gxImage local_image = new gxImage())
                            { 
                                // 3. load image from the byte array
                                //if (nread > 0)

                                if (buffer.Length > 0)
                                {
                                    if (!local_image.LoadFromMem(buffer, (int)GX_PIXELFORMATS.GX_BGR)) //gximg
                                    {
                                        return;
                                    }
                                }

                                //local_image = gximg;
                                Thread.Sleep(100);

                                if (local_image != null)
                                {
                                    //do the plate recognition
                                    if (anpr.FindFirst(local_image))
                                    {
                                        // plate is found, raise event                    
                                        ANPR_RESULT_STRUCT res = new ANPR_RESULT_STRUCT();

                                        //cmCharacter c = anpr.GetCharacter(0);
                                        int count = anpr.GetNCharacters();

                                        StringBuilder sb = new StringBuilder();//count * 2
                                        for (int i = 0; i < count; i++)
                                        {
                                            sb.Append((char)anpr.GetCharacter(i).code);
                                            //sb.Append('\u200E'); //unit separator
                                            //sb.Append('\u001e'); //record separator
                                            //sb.Append('\u0020'); //space
                                        }

                                        //string strNum = anpr.GetTextA();
                                        //StringBuilder sb = new StringBuilder(strNum.Length * 2);
                                        //foreach (char c in strNum)
                                        //{
                                        //    sb.Append((char)c);
                                        //    //sb.Append('\u001e');
                                        //    //sb.Append('\u0020');
                                        //}

                                        res.text = sb.ToString(); //anpr.LRText2Display(sb.ToString()); // sb.ToString(); // anpr.LRText2Display(sb.ToString());
                                        res.textAscii = anpr.GetTextA();

                                        res.plateColor = anpr.GetBkColor().ToString("X");
                                        res.plateSubColor = anpr.GetColor().ToString("X");
                                        res.accuracy = anpr.GetConfidence().ToString();
                                        res.lpOrigin = anpr.GetType();

                                        res.frame = anpr.GetFrame();
                                        res.image = (Bitmap)local_bitmap.Clone();
                                        anpr_result(res);

                                        //Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        // No Result
                                    }
                                }
                            }
                        }
                        catch (gxException e)
                        {

                        }

                        //Thread.Sleep(100);
                    //}
                   
                    // gximg.Close();
                    // local_image.Dispose();
                }
            }

            //===============================================================================================
        }

        /** Sets the 'image' and 'bitmap' variables. Signs to the thread that a new image has been set.
         * @param bm Image to be scanned for number plates
         */
        public void SetImage(Bitmap bm)
        {
            q.Add(bm);

            ////bitmap = bm;
            ////evt.Set();

            ////Monitor.Enter(sync_obj);
            //// set the new images
            ////image = gximg; // cmAnpr needs gxImage
            ////bitmap = new Bitmap(bm); // if a plate is found on the gxImage, the original bitmap is sent with the anpr_result event

            //bool lockWasTaken = false;
            //try
            //{
            //    Monitor.Enter(sync_obj, ref lockWasTaken);
            //    {
            //        //bitmap = new Bitmap(bm); 
            //        Thread.MemoryBarrier();
            //        bitmap = bm; // if a plate is found on the gxImage, the original bitmap is sent with the anpr_result event
            //    }
            //    //// signal to the thread
            //}
            //finally
            //{
            //    if (lockWasTaken) Monitor.Exit(sync_obj);
            //}

            //evt.Set();
        }

        /** Stops the ANPR thread */
        public void StopThread()
        {
            exit_flag = true;
            evt.Set();

            if (thread != null)
                if (!thread.Join(2000))
                    thread.Abort();
        }


    }

    
}
