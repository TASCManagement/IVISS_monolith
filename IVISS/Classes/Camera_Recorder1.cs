using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using SpinnakerNET;
using SpinnakerNET.GenApi;
using SpinnakerNET.Video;

using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.CompilerServices;
//using System.Drawing;

namespace IVISS
{
    class Camera_Recorder
    {
        // Use the following enum and global static variable to select the type
        // of video file to be created and saved.
        enum VideoType
        {
            Uncompressed,
            Mjpg,
            H264
        }

        static VideoType chosenFileType = VideoType.Mjpg;

        long image_offset_x = 0;    // 424;
        long image_offset_y = 500;    // 600;

        long image_width = 2048;
        long image_height = 80;

        float recording_frame_rate = 610.0f;

        float recording_duration = 10.0f;        // Record for 5.0 seconds

        float video_frame_rate = 30.0f;         // Frame rate for saving video

        double exposure_time = 1300; //1200;     // Time is us (micro-second)
        double gain = 24; //12;                   // Gain in db

        int bitrate = 500 * 1000;     // in bits per second (10 Mbits/sec). For h.264 only
        int mjpeg_quality = 75;       // For MJPEG conversion only

        float gamma = 0.5f; ///////////////////////////////////////////////////////////////////////////////////////////////////// Fix gamma too! /////////////////////////////////////////////////////////////

        String recordingPath = "C:\\IVISSTemp\\";      // Choosing a new path

        /*
        long image_width = 2048;
        long image_height = 1536;
        long image_offset_x = 0;
        long image_offset_y = 0;

        float recording_frame_rate = 30.0f;

        float recording_duration = 2.0f;    // Record for 5.0 seconds

        float video_frame_rate = 30.0f;     // Frame rate for saving video

        double exposure_time = 15000;       // Time is us (micro-second)
        double gain = 0;                   // Gain in db

        int bitrate = 15 * 1000 * 1000;     // in bits per second (10 Mbits/sec)
        int mjpeg_quality = 90;
        */

        // Variable for detecting dropped frames
        ulong old_frame_id = 1;
        ulong old_cycleCount = 0;
        ulong dropped_frames = 0;

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        List<IManagedCamera> camList;

        IManagedCamera theCamera;
        
        BlockingCollection<IManagedImage> images = new BlockingCollection<IManagedImage>();

        Object lock_obj = new Object();

        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        int recording_number = 0;

        // If the recording should be saved in the same folder with different video names
        // .. or diffferent folders with differents folders names
        bool seperate_folder = false;       

        // The constructor
        public Camera_Recorder()
        {
            
        }

        /**
         *      Order of function calls:
         *      
         *      1) "Constructor"
         *      2) "ConnectCamera"              // Just finds the list of available cameras. Can be done based on IP address
         *      3) "StartCamera"                // This is the functions that actually connects to the camera, an then we get access to the camera functions and settings, even if we haven't started acquiring frames
         *                                      // Done by calling "cam.Init()"
         *      4) "InitializeSettings"         // This function changes the camera settings, before we start acquiring frames
         *      5) "StartReceivingFrames"       // We start acquiring frames in this function
         *                                      // Done by calling "cam.BeginAcquisition"
         *      5) "StopReceivingFrames"                 // Stops receiving frames by calling "cam.EndAcquisition()"
         *      6) "DisconnectCamera"           // Disconnects camera by calling "cam.DeInit()"
         */


        // Just finds the list of available cameras. Can be done based on IP address
        public int ConnectCamera()
        {
            // Todo 3: Start a camera based on its IP address, or its "Serial Number". Perhaps the serial number is more reliable

            // Todo 6: Perhaps some exceptions need to be handled in this function

            // Retrieve singleton reference to system object
            ManagedSystem system = new ManagedSystem();

            // Retrieve list of cameras from the system
            camList = system.GetCameras();

            Console.WriteLine("Number of cameras detected: {0}\n", camList.Count);

            // Finish if there are no cameras
            if (camList.Count == 0)
            {
                // Clear camera list before releasing system
                camList.Clear();

                // Release system
                system.Dispose();

                Console.WriteLine("Not enough cameras!");

                return -1;
            }

            this.theCamera = camList[0];

            // Clear camera list before releasing system
            camList.Clear();

            // Release system
            system.Dispose();

            return 1;
        }

        // If error is returned after the function call, then don't start the camera
        public int StartCamera()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /// //////////////////////////////////////////////////////////

            int result = 0;

            try
            {
                // Initialize camera
                this.theCamera.Init();

                // Initialize cameras settings
                InitializeSettings();
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                ShowLineNumber("");
                result = -1;
            }

            /// //////////////////////////////////////////////////////////

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            stopWatch.Reset();

            // Format and display the TimeSpan value. 
            string t = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            Console.WriteLine("Elapsed time for theCamera.Init(): " + t);

            return result;
        }

        //! Done
        public int InitializeSettings()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /// //////////////////////////////////////////////////////////

            int result = 0;

            // Todo 9: Load all of these settings from the 'UserSet', instead of manually setting each one individually

            try
            {
                IManagedCamera cam = this.theCamera;

                // Retrieve GenICam nodemap
                INodeMap nodeMap = theCamera.GetNodeMap();

                /// //////////////////////////////////////////////////////////

                //! Gettings a list of all the settings of the camera

                /*
                    var a = nodeMap.GetEnumerator();

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"list.txt"))
                    {
                        do
                        {
                            try
                            {
                                file.WriteLine("Key: {0} \t Value: {1} \t Type: {2}", a.Current.Key.ToString().PadRight(70, ' '), a.Current.Value.ToString().PadRight(30, ' '), a.Current.Value.GetType());
                            }
                            catch (Exception e)
                            {
                                file.WriteLine("Exception: Key: {0}", a.Current.Key);
                            }
                        } while (a.MoveNext());
                    }
                    */

                /// //////////////////////////////////////////////////////////

                // Set acquisition mode to continuous
                IEnum iAcquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");
                if (iAcquisitionMode == null || !iAcquisitionMode.IsWritable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (node retrieval). Aborting...\n");
                    return -1;
                }

                IEnumEntry iAcquisitionModeContinuous = iAcquisitionMode.GetEntryByName("Continuous");
                if (iAcquisitionModeContinuous == null || !iAcquisitionMode.IsReadable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (entry retrieval). Aborting...\n");
                    return -1;
                }

                iAcquisitionMode.Value = iAcquisitionModeContinuous.Value;

                Console.WriteLine("Acquisition mode set to continuous...");

                /// //////////////////////////////////////////////////////////

                // Enable change of acquisition frame-rate
                BoolNode iAcquisitionFrameRateEnable = nodeMap.GetNode<BoolNode>("AcquisitionFrameRateEnable");
                if (iAcquisitionFrameRateEnable == null || !iAcquisitionFrameRateEnable.IsWritable)
                {
                    Console.WriteLine("Unable to set acquisition frame-rate enable to true (node retrieval). Aborting...\n");
                    return -1;
                }
                else
                {
                    iAcquisitionFrameRateEnable.Value = true;
                }

                /// //////////////////////////////////////////////////////////

                // Disable ISP (Image Signle Processing)
                BoolNode iIspEnable = nodeMap.GetNode<BoolNode>("IspEnable");
                if (iIspEnable == null || !iIspEnable.IsWritable)
                {
                    Console.WriteLine("Unable to set ISP to false (node retrieval). Aborting...\n");
                    return -1;
                }
                else
                {
                    iIspEnable.Value = false;
                }

                /// //////////////////////////////////////////////////////////

                cam.PixelFormat.Value = PixelFormatEnums.BayerRG8.ToString();

                // Set up 'Exposure'
                cam.ExposureAuto.Value = ExposureAutoEnums.Off.ToString();
                cam.ExposureMode.Value = ExposureModeEnums.Timed.ToString(); ;
                cam.ExposureTime.Value = exposure_time;   // 900 us or 0.9 ms

                // Set up 'Gain'
                cam.GainAuto.Value = GainAutoEnums.Off.ToString();
                //cam.GainAutoBalance.Value = GainAutoBalanceEnums.Off.ToString();
                cam.Gain.Value = gain;

                // Set up 'White Balance'
                cam.BalanceWhiteAuto.Value = BalanceWhiteAutoEnums.Off.ToString();
                cam.BalanceRatioSelector.Value = BalanceRatioSelectorEnums.Red.ToString();
                cam.BalanceRatio.Value = 1.18;

                // Set up 'Gamma'
                cam.GammaEnable.Value = true;
                cam.Gamma.Value = gamma;

                // Set up 'Black Level'


                // Set up image dimensions
                cam.OffsetX.Value = image_offset_x;
                cam.OffsetY.Value = image_offset_y;

                cam.Width.Value = image_width;
                cam.Height.Value = image_height;



                // Set up 'AcquisitionFrameRate'. It can only be set if above conditions satisfy
                // i.e. ISP is turned off, and
                // Auto-exposure, auto-gain and auto-white balance are turned off, and
                // Image size is smaal enough, and
                // Exposure time is small enough, and
                // There is enough bandwidth
                cam.AcquisitionFrameRate.Value = recording_frame_rate;

                Console.WriteLine("Frame rate set to: " + cam.AcquisitionFrameRate.Value);

                // Todo 3: Set this to a fixed value after researching optimal value
                /*  
                 * // This is set automatically in newer versions
                // Setting up number of image buffers
                INodeMap sNodeMap = cam.GetTLStreamNodeMap();
                IInteger streamNode = sNodeMap.GetNode<IInteger>("StreamDefaultBufferCount");
                long bufferCount = streamNode.Value;
                streamNode.Value = 200;
                */

                /// //////////////////////////////////////////////////////////

            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                ShowLineNumber("");
                result = -1;
            }

            /// ////////////////////////////////////////////////////////// 

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            stopWatch.Reset();

            // Format and display the TimeSpan value. 
            string t = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            Console.WriteLine("Elapsed time for initialize_camera(): " + t);

            return result;
        }

        public int StartReceivingFrames_2()
        {
            Task startFrames = new Task(() => StartReceivingFrames(1.0f));
            startFrames.Start();

            return 1;
        }

        public int StartReceivingFrames(float recording_duration)
        {
            Task.Factory.StartNew(() => {
                SaveListToVideo();
            });

            IManagedCamera cam = this.theCamera;

            int result = 0;

            this.recording_duration = recording_duration;

            try
            {
                // Begin acquiring images
                cam.BeginAcquisition();

                Console.WriteLine("Acquiring images...\n");

                // Retrieve and convert images
                //const int NumImages = 12000;

                int NumImages = (int)(recording_duration * recording_frame_rate);

                int imageCnt = 0;

                // Take images untill queue is empty
                while (!images.IsAddingCompleted)     
                {
                    lock (lock_obj)
                    {
                        // 'try' this, in case there's an exception when using 'cam.GetNextImage()'
                        // This exception can happen because 'cam.EndAcquisition()' has benn called
                        try
                        {
                            // Retrieve the next received images
                            using (IManagedImage rawImage = cam.GetNextImage())
                            {
                                if (imageCnt % 300 == 0 || imageCnt == NumImages - 1)
                                    Console.WriteLine("Image count: {0}", imageCnt);

                                try
                                {
                                    if (rawImage.IsIncomplete)
                                    {
                                        Console.WriteLine("Image incomplete with image status {0}...\n", rawImage.ImageStatus);
                                    }
                                    else
                                    {
                                        testFrameDrop(rawImage, imageCnt);

                                        ///////////////////////////////////////////////////////////////////////////////////////////

                                        // Todo 7: Find optimal settings for this
                                        // Deep copy image into list
                                        //images.Add(rawImage.Convert(PixelFormatEnums.Mono8));
                                        //images.Add(rawImage.Convert(PixelFormatEnums.RGB8));
                                        //images.Add(rawImage.Convert(PixelFormatEnums.BayerRG8));
                                        //images.Add(rawImage.Convert(PixelFormatEnums.RGB8, ColorProcessingAlgorithm.WEIGHTED_DIRECTIONAL_FILTER));

                                        //? Being used before the latest one
                                        //images.Add(rawImage.Convert(PixelFormatEnums.BayerRG8, ColorProcessingAlgorithm.WEIGHTED_DIRECTIONAL_FILTER));

                                        images.Add(rawImage.Convert(PixelFormatEnums.BayerRG8, ColorProcessingAlgorithm.DIRECTIONAL_FILTER));

                                        imageCnt++;
                                    }
                                }
                                catch (InvalidOperationException e)
                                {
                                    Console.WriteLine(e.Message);
                                    Console.WriteLine("Exceptional exception!");
                                    Console.WriteLine("Happened because more images cannot be added to the queue!");
                                    ShowLineNumber("");
                                    break;
                                }
                                catch (SpinnakerException ex)
                                {
                                    Console.WriteLine("Error: {0}", ex.Message);
                                    Console.WriteLine("Error_2: {0}", ex.ErrorCode);
                                    ShowLineNumber("");
                                    result = -1;
                                    break;
                                }
                            }
                        }
                        catch (InvalidOperationException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("Exceptional exception!");
                            ShowLineNumber("");
                            break;
                        }
                        catch (SpinnakerException ex)
                        {
                            Console.WriteLine("Error: {0}", ex.Message);
                            Console.WriteLine("Error_2: {0}", ex.ErrorCode);
                            ShowLineNumber("");
                            result = -1;
                            break;
                        }
                    } // Release the lock here
                }

                Console.WriteLine("Total frames recieved: {0}", imageCnt);
                Console.WriteLine("Recording duration: {0}", imageCnt/recording_frame_rate);
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine("Error_2: {0}", ex.ErrorCode);
                ShowLineNumber("");
                result = -1;
            }

            Console.WriteLine("\t End of \'StartReceivingFrames\'");

            return result;
        } // 'StartReceivingFrames' end

        public int StopReceivingFrames()
        {
            int err = 1;

            // Done adding images to the queue
            images.CompleteAdding();

            // Try to disconnect until it's done successfully
            while (true)
            {
                lock (lock_obj)
                {
                    try
                    {
                        //if (this.theCamera.IsStreaming())
                        {
                            this.theCamera.EndAcquisition();   // This line stops receiving images from the camera. But the camera isn't disconnected yet
                            //Thread.Sleep(1000);
                        }
                        break;
                    }
                    catch (SpinnakerException ex)
                    {
                        if (this.theCamera.IsValid())
                        { }

                        //DisconnectCamera();

                        Console.WriteLine("Error: {0}", ex.Message);
                        ShowLineNumber("\'EndAcquisition() exception\'");
                        err = -1;
                    }
                    finally
                    {

                    }
                }
            }

            // Waits for signal from 'waitHandle.WaitOne();'
            // .. to get the signal for the completion of video conversion
            autoResetEvent.WaitOne();

            Console.WriteLine("End of \'StopReceivingFrames\'");

            return err;
        }

        public int DisconnectCamera()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /// //////////////////////////////////////////////////////////

            int result = 0;

            try
            {
                // Deinitialize camera
                theCamera.DeInit();
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Exception during disconnecting");
                Console.WriteLine("Error: {0}", ex.Message);
                ShowLineNumber("");
                result = -1;
            }

            /// //////////////////////////////////////////////////////////

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            stopWatch.Reset();

            // Format and display the TimeSpan value. 
            string t = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            Console.WriteLine("Elapsed time for cam.DeInit(): " + t);

            return result;
        }

        // This function prepares, saves, and cleans up an video from a list of images.
        public int SaveListToVideo()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /// //////////////////////////////////////////////////////////

            int result = 0;

            //Console.WriteLine("\n\n*** CREATING VIDEO ***\n");

            try
            {
                // For saving video files in seprate directory
                // Name of new directory is based on the date and time
                // Todo 2: Use this later. For now, all video files are in same foolder but with different filenames based on the date and time

                DateTime localDate = DateTime.Now;

                //String directoryPath = String.Format("C:\\affraz_IVISS_2\\{0}-{1}-{2} {3}-{4}-{5}.{6:000}_bitrate_{7}_kbps", localDate.Month, localDate.Day, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond, bitrate / 1000);
                //String directoryPath = String.Format("C:\\affraz_IVISS_2\\{0}-{1}-{2} {3}-{4}-{5}.{6:000}_mjpeg_{7}", localDate.Month, localDate.Day, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond, mjpeg_quality);

                //string videoFilename = "SaveToAvi-CSharp-H264";
                //string videoFilename = directoryPath;

                String directoryPath = @"C:\IVISSTemp\";
                string videoFilename = directoryPath + "rec";

                // Todo 2: Don't use for now. May be in future to differentiate between recordings from different cameras
                // Retrieve device serial number for filename
                //String deviceSerialNumber = "";

                //IString iDeviceSerialNumber = nodeMapTLDevice.GetNode<IString>("DeviceSerialNumber");
                //if (iDeviceSerialNumber != null && iDeviceSerialNumber.IsReadable)
                //{
                //    deviceSerialNumber = iDeviceSerialNumber.Value;

                //    Console.WriteLine("Device serial number retrieved as {0}...", deviceSerialNumber);
                //}

                float frameRateToSet = video_frame_rate;
                Console.WriteLine("Frame rate to be set to {0}", frameRateToSet);

                using (IManagedSpinVideo video = new ManagedSpinVideo())
                {
                    // Set maximum video file size to 2GB. A new video file is generated when 2GB
                    // limit is reached. Setting maximum file size to 0 indicates no limit.
                    const uint FileMaxSize = 0;
                    video.SetMaximumFileSize(FileMaxSize);

                    // Todo 5: Do this only for MJPEG, and discard the code for others
                    switch (chosenFileType)
                    {
                        case VideoType.Mjpg:
                            //directoryPath = String.Format(recordingPath + "{0}-{1}-{2} {3}-{4}-{5}.{6:000}_mjpeg_{7}", localDate.Month, localDate.Day, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond, mjpeg_quality);
                            //videoFilename = directoryPath;


                            //// Todo 10: This naming scheme makes code crash. Use different style names
                            //videoFilename = String.Format(recordingPath + recording_number);
                            ////videoFilename = String.Format(recordingPath + "{0}-{1}-{2} {3}-{4}-{5}.{6:000}", localDate.Month, localDate.Day, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond);

                            MJPGOption mjpgOption = new MJPGOption();
                            mjpgOption.frameRate = frameRateToSet;
                            mjpgOption.quality = mjpeg_quality;
                            video.Open(videoFilename, mjpgOption);
                            break;

                        /// //////////////////////////////////////////////

                        case VideoType.Uncompressed:
                            directoryPath = String.Format(recordingPath + "{0}-{1}-{2} {3}-{4}-{5}.{6:000}_uncompressed", localDate.Month, localDate.Day, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond);
                            videoFilename = directoryPath;

                            AviOption uncompressedOption = new AviOption();
                            uncompressedOption.frameRate = frameRateToSet;
                            video.Open(videoFilename, uncompressedOption);
                            break;

                        case VideoType.H264:
                            directoryPath = String.Format(recordingPath + "{0}-{1}-{2} {3}-{4}-{5}.{6:000}_bitrate_{7}_kbps", localDate.Month, localDate.Day, localDate.Year, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond, bitrate / 1000);
                            videoFilename = directoryPath;

                            H264Option h264Option = new H264Option();
                            h264Option.frameRate = frameRateToSet;
                            h264Option.bitrate = 1000000;
                            h264Option.height = Convert.ToInt32(this.image_height);
                            h264Option.width = Convert.ToInt32(this.image_width);
                            video.Open(videoFilename, h264Option);
                            break;
                    }

                    //
                    // Construct and save video
                    //

                    int imageCnt = 0;

                    {
                        // Runs loop until queue is empty
                        //for (int i = 0; i < frame_count; i++)
                        while (!images.IsCompleted)
                        {
                            if (imageCnt % 200 == 0)
                                Console.WriteLine("\t\t\t Frames taken {0}: ", imageCnt);

                            string name1 = String.Format(@"images\{0}.jpg", imageCnt);

                            //Thread.Sleep(5);

                            // This section works fine
                            // Take images from queue
                            try
                            {
                                var one_image = images.Take();

                                video.Append(one_image);

                                //Thread.Sleep(5);      // Present only for debugging. To test if the code would break if this line is enabled

                                //if (imageCnt % (int)(recording_frame_rate / 2) == 0)
                                //    Console.WriteLine("\t\t\tAppended image {0}...", imageCnt);

                                // Dispose the taken image
                                one_image.Dispose();

                                imageCnt++;
                            }
                            catch (InvalidOperationException e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine("Exceptional exception!");
                                ShowLineNumber("");
                                break;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine("Take exception. Things will go fine");
                                ShowLineNumber("");
                                break;
                            }
                        }

                        /// //////////////////////////////////////////////////////////

                        // Close video file
                        video.Close();

                        images.Dispose();

                        images = new BlockingCollection<IManagedImage>();

                        recording_number++;

                        Console.WriteLine("End of \'SaveListTiVideo\'");
                    }
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                ShowLineNumber("");
                result = -1;
            }

            /// //////////////////////////////////////////////////////////
            
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            stopWatch.Reset();

            // Format and display the TimeSpan value. 
            string t = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds);

            Console.WriteLine("Elapsed time for saving video: " + t);

            // Added 'waitHandle.Set();' to signal the completion of video conversion
            autoResetEvent.Set();

            return result;
        }

        private void testFrameDrop(IManagedImage rawImage, int imageCnt)
        {
            double new_recording_frame_rate = 0.0;

            if ((rawImage.FrameID - old_frame_id) > 1)
            {
                // Print image information
                Console.WriteLine("Grabbed image {0}, width = {1}, height {2}, FrameID: {3}, Skipped: {4}", imageCnt, rawImage.Width, rawImage.Height, rawImage.FrameID, rawImage.FrameID - old_frame_id);
            }

            old_frame_id = rawImage.FrameID;

            ///////////////////////////////////////////////////////////////////////////////////////////

            ulong timestamp = rawImage.TimeStamp;
            ulong time_diff;
            //ulong timestamp_tick_freq = (1 << 30); //1000000000;
            ulong timestamp_tick_freq = 1000000000 / 1000;

            if (timestamp > old_cycleCount)
                time_diff = timestamp - old_cycleCount;
            else
                time_diff = timestamp_tick_freq + timestamp - old_cycleCount;

            time_diff /= 1000;

            //if (new_recording_frame_rate == 0.0f)
            if (imageCnt == 1)
            {
                new_recording_frame_rate = (double)timestamp_tick_freq / (double)time_diff;
                Console.WriteLine("New frame rate: {0}", new_recording_frame_rate);
            }

            //Console.WriteLine("Timestamp: {0} \t Time diff: {1} \t Frame rate: {2:00}", 0, time_diff, (double)timestamp_tick_freq / (double)time_diff);

            //if (time_diff[i] > 96 && total_frame_count > 1)  // for 84 fps
            //if (total_frame_count > 1)
            if (time_diff > (Math.Ceiling((double)timestamp_tick_freq / (double)new_recording_frame_rate) + 1) && imageCnt > 1)
            {
                //dropped_frames[i] += (uint)(time_diff[i] / 90.0) - 1;   // for 84 fps
                dropped_frames += (uint)(time_diff / (Math.Floor((double)timestamp_tick_freq / (double)new_recording_frame_rate) - 2)) - 1;

                Console.WriteLine("Frame number: {0000} Time_diff: {1:0000} Dropped frames: {2:0000} Frame rate: {3:00}", imageCnt, time_diff, dropped_frames, (double)timestamp_tick_freq / (double)time_diff);
            }

            old_cycleCount = timestamp;
        }

        private void ShowLineNumber(string message,
                                    [CallerLineNumber] int lineNumber = 0,
                                    [CallerMemberName] string caller = null)
        {
            Console.WriteLine(message + " at line " + lineNumber + " (" + caller + ")");
        }
    }
}
