using Emgu.CV;
using Emgu.CV.Structure;
using IVISS.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.Model
{
    class MainModel
    {
        public string lpNumArabic { set; get; }
        public string lpNumEnglish { set; get; }
        public string recordingPath { set; get; }
        public string accuracy { set; get; }
        public string origin { set; get; }
        public string plateColor { set; get; }
        public string plateSubColor { set; get; }
        public string applicationPath { set; get; }
        public bool rushMode { set; get; }
        public string destinationDir { set; get; }
        public DataTable ReturnGridSource()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("License", typeof(String));
                dt.Columns.Add("ArabicLicense", typeof(String));
                dt.Columns.Add("Date", typeof(DateTime));
                dt.Columns.Add("Gate", typeof(String));
                dt.Columns.Add("Status", typeof(String));
                dt.Columns.Add("Path", typeof(String));
                dt.Columns.Add("Accuracy", typeof(String));
                dt.Columns.Add("Classification", typeof(String));

                //dgView.DataSource = dt;

                string gateNo = Global.m_Gate_No;

                using (IVISSEntities db = new IVISSEntities())
                {
                    // .Take(5)
                    // where objDetails.gate_no == gateNo
                    // where objDetails.visitor_license_number != ""

                    var detailQuery = from objDetails in db.Details
                                      where objDetails.is_primary!=0
                                      orderby objDetails.visitor_entry_time descending
                                      select new { objDetails.visitor_license_number, objDetails.visitor_license_number_arabic, objDetails.visitor_entry_date, objDetails.visitor_entry_time, objDetails.visitor_iviss_recording, objDetails.visitor_license_accuracy, objDetails.visitor_access_status, objDetails.visitor_access_gate, objDetails.visitor_authorization };

                    detailQuery = detailQuery.Take(200);

                    DataRow dr;

                    foreach (var detail in detailQuery)
                    {
                        dr = dt.NewRow();
                        dr["License"] = detail.visitor_license_number;
                        dr["ArabicLicense"] = detail.visitor_license_number_arabic;
                        dr["Date"] = detail.visitor_entry_time; //detail.visitor_entry_date.ToShortDateString() + " " + detail.visitor_entry_time.ToShortTimeString(); 
                        dr["Gate"] = detail.visitor_access_gate;
                        dr["Status"] = detail.visitor_access_status; //(detail.visitor_license_number.Trim().ToLower()=="xac5831")?"DENIED":"ALLOWED";
                        dr["Path"] = detail.visitor_iviss_recording;
                        dr["Accuracy"] = detail.visitor_license_accuracy;
                        dr["Classification"] = detail.visitor_authorization;

                        dt.Rows.Add(dr);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }
        public SystemSetting FillSettings()
        {
            using (var db = new IVISSEntities())
            {
                var query = (from settings in db.SystemSettings

                             select settings).FirstOrDefault();

                return query;
            }
        }

        public void FillAdditionalALPR()
        {
            using (var db = new IVISSEntities())
            {
                var query = (from settings in db.AdditionalALPRs
                            
                             select settings);


                foreach(var alpr in query)
                {
                    Global.lstAdditionalALPR.Add(alpr);

                }

               
            }
        }

        

        public bool UpDateSerial(string serialno)
        {
            try
            {
                using (var db = new IVISSEntities())
                {
                    var query = (from settings in db.SystemSettings
                                 select settings).FirstOrDefault();

                    if (query != null)
                    {
                        query.LicenseNo = serialno;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        Global.ShowMessage("No settings found", false);
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {

                Global.ShowMessage(ex.Message, false);
                return false;
            }

        }

        public string GetDefaultOfSameVehicle(string licenseplate)
        {
            using (var db = new IVISSEntities())
            {
                var query = (from det in db.Details
                             where det.is_default == 1 && det.visitor_license_number == licenseplate
                             select det).FirstOrDefault();
                if (query == null)
                    return "";
                else
                {
                    if (query.visitor_license_number == licenseplate)
                        return query.visitor_iviss_recording;
                }
            }

            return "";
        }

        private void saveJpeg(string path, System.Drawing.Bitmap img, long quality)
        {
            // Encoder parameter for image quality

            System.Drawing.Imaging.EncoderParameter qualityParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            // Jpeg image codec
            System.Drawing.Imaging.ImageCodecInfo jpegCodec = this.getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        private System.Drawing.Imaging.ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        private List<Image<Bgr, Byte>> GetVideoFrames(int Time_millisecounds, string videofile)
        {
            List<Image<Bgr, Byte>> image_array = new List<Image<Bgr, Byte>>();
            System.Diagnostics.Stopwatch SW = new System.Diagnostics.Stopwatch();

            bool Reading = true;
            Capture _capture = new Capture(videofile);
            SW.Start();

            while (Reading)
            {
                // Image<Bgr, Byte> frame = _capture.QueryFrame();

                Image<Bgr, Byte> frame = _capture.QueryFrame();



                if (frame != null)
                {
                    image_array.Add(frame.Copy());
                    if (SW.ElapsedMilliseconds >= Time_millisecounds) Reading = false;
                }
                else
                {
                    Reading = false;
                }
            }

            return image_array;
        }
        public string AddToDatabase()
        {
            try
            {
                //string date = DateTime.Now.ToShortDateString();
                //string time = DateTime.Now.ToLongTimeString();

                string date = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.FFF"); //DateTime.Now.ToShortDateString();
                string time = DateTime.Now.ToString("HH:mm:ss.FFF"); //DateTime.Now.ToLongTimeString();

                var authorization = string.Empty;
                var classification = string.Empty;

                string source_dir = Global.TEMP_FOLDER;
                //string destination_dir = global.VIDEO_FOLDER + "\\" + date.Replace("/", "-") + " " + time.Replace(":", "-"); //"\\" + licensePlate + 
                destinationDir = Global.VIDEO_FOLDER + "\\" + Convert.ToDateTime(date).ToShortDateString().Replace("/", "-") + " " + time.Replace(":", "-");



                //m_IVISS_Recording_Path = destination_dir;
                int vID = 1001;

                //Task tDB = Task.Run(() =>
                //{
                using (IVISSEntities db = new IVISSEntities())
                {

                    //var visitor = (from v in db.Visitors
                    //                where v.visitor_license_plate == licensePlate 
                    //                select v).FirstOrDefault();

                    //if (visitor != null)
                    //{
                    //    authorization = visitor.visitor_authorization;
                    //    classification = visitor.visitor_classification;
                    //    vID = visitor.visitor_id;
                    //}
                    //else
                    //{
                    //    authorization = "";
                    //    classification = "VISITOR";
                    //}

                    var visitor = from v in db.Visitors
                                  where v.visitor_license_plate == lpNumEnglish
                                  select v;

                    if (!String.IsNullOrEmpty(lpNumEnglish))
                        visitor = visitor.Where(p => p.visitor_license_plate == lpNumEnglish);

                    else if (!String.IsNullOrEmpty(lpNumArabic))
                        visitor = visitor.Where(p => p.visitor_license_plate_arabic == lpNumArabic);

                    else
                        visitor = visitor.Where(p => p.visitor_license_plate == lpNumEnglish && p.visitor_license_plate_arabic == lpNumArabic);

                    var result = visitor.Select(v => new { v.visitor_classification, v.visitor_authorization, v.visitor_id }).FirstOrDefault();

                    if (result != null)
                    {
                        authorization = result.visitor_authorization;
                        classification = result.visitor_classification;
                        vID = result.visitor_id;
                    }
                    else
                    {
                        authorization = "";
                        classification = "VISITOR";
                    }


                    int accuracy;
                    if (!int.TryParse(this.accuracy, out accuracy))
                        accuracy = 0;

                    var detail = from v in db.Details
                                 where v.visitor_license_number == lpNumEnglish
                                 select v;


                    var d = new Detail();

                    if (d != null)
                    {
                        d.visitor_id = vID;
                        d.visitor_vehicle_license_plate = 1;
                        d.visitor_entry_date = Convert.ToDateTime(date);
                        d.visitor_entry_time = Convert.ToDateTime(time);

                        d.visitor_license_number = lpNumEnglish;
                        d.visitor_license_number_arabic = lpNumArabic;

                        d.visitor_exit_gate = 1;
                        d.visitor_entry_gate = 1;

                        d.visitor_license_prefix = "";
                        d.visitor_license_region = (origin != null) ? origin : String.Empty;

                        d.visitor_license_country = "";
                        d.visitor_license_back_color = (plateColor != null) ? plateColor : String.Empty;
                        d.visitor_license_fore_color = (plateSubColor != null) ? plateSubColor : String.Empty;

                        d.visitor_license_type = "";
                        d.visitor_iviss_recording = destinationDir;

                        d.visitor_access_gate = "Entry";
                        d.visitor_license_accuracy = accuracy;

                        d.visitor_authorization = (authorization.Length > 0) ? authorization : "VISITOR";

                        d.gate_no = Global.m_Gate_No;

                        if (detail != null)
                        {
                            if (detail.Count() == 0)
                            {
                                d.is_default = 1;
                            }
                        }
                        else
                        {
                            d.is_default = 1;

                        }

                        db.Details.Add(d);
                        db.SaveChanges();
                    }
                }
                //});

                //=========================================================================================================================================




                //// Create subdirectory structure in destination    
                //foreach (string dir in Directory.GetDirectories(source_dir, "*", System.IO.SearchOption.AllDirectories))
                //{
                //    Directory.CreateDirectory(destination_dir + dir.Substring(source_dir.Length));
                //}

                //foreach (string file_name in Directory.GetFiles(source_dir, "*.*", System.IO.SearchOption.AllDirectories))
                //{
                //    File.Copy(file_name, destination_dir + file_name.Substring(source_dir.Length));
                //}

                //Task t2 = Task.Run(() =>
                //{
                //if (File.Exists(source_dir + "\\" + "Brd_0_Ch_03_0.mov"))
                //    File.Copy(source_dir + "\\" + "Brd_0_Ch_03_0.mov", destination_dir + "\\" + "Brd_0_Ch_03_0.mov");
                //});

                //Task t3 = Task.Run(() =>
                //{
                //if (File.Exists(source_dir + "\\" + "Brd_0_Ch_04_0.mov"))
                //    File.Copy(source_dir + "\\" + "Brd_0_Ch_04_0.mov", destination_dir + "\\" + "Brd_0_Ch_04_0.mov");
                //});

                //Task t4 = Task.Run(() =>
                //{
                //if (File.Exists(source_dir + "\\" + "Brd_0_Ch_05_0.mov"))
                //    File.Copy(source_dir + "\\" + "Brd_0_Ch_05_0.mov", destination_dir + "\\" + "Brd_0_Ch_05_0.mov");
                //});

                //Task t5 = Task.Run(() =>
                //{
                //if (File.Exists(source_dir + "\\" + "Brd_0_Ch_08_0.mov"))
                //    File.Copy(source_dir + "\\" + "Brd_0_Ch_08_0.mov", destination_dir + "\\" + "Brd_0_Ch_08_0.mov");
                //});

                //Task t6 = Task.Run(() =>
                //{
                //if (File.Exists(source_dir + "\\" + "Brd_0_Ch_09_0.mov"))
                //    File.Copy(source_dir + "\\" + "Brd_0_Ch_09_0.mov", destination_dir + "\\" + "Brd_0_Ch_09_0.mov");
                //});


                //***************************************** Run stitch on every record ************************************************
                //if (!File.Exists(path + "Brd_0_Ch_02_0.mov"))
                //{
                //    this.ShowMessageDialog("Could not load video files or no video files found");
                //    return;
                //}

                //// if stitched image not found, create one
                //if (!File.Exists(destination_dir + "outPutVer.jpg"))
                //{
                //    Task tStitch = Task.Run(() =>
                //    {
                //        ProcessStartInfo _processStartInfo = new ProcessStartInfo();
                //        _processStartInfo.WorkingDirectory = applicationPath + @"\stitch\";
                //        _processStartInfo.FileName = @"stitch.exe";
                //        _processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                //        _processStartInfo.CreateNoWindow = true;
                //        _processStartInfo.Arguments = "\"" + destination_dir + "\"";

                //        Process myProcess = Process.Start(_processStartInfo);
                //    });
                //}
                //**********************************************************************************************************************

                if (!Directory.Exists(destinationDir))
                {
                    System.IO.Directory.CreateDirectory(destinationDir);

                    //string[] files = Directory.GetFiles(Global.TEMP_FOLDER, "outPutVer.jpg");


                    //foreach (string file in files)
                    //{

                    //    System.IO.File.Copy(file, System.IO.Path.Combine(destinationDir, "outPutVer.jpg"), true);
                    //}
                }



                //Task t1 = Task.Run(() =>
                //{
              
                //});

                //Task t2 = Task.Run(() =>
                //{
                    if (File.Exists(source_dir + "\\" + "driver.avi"))
                        File.Copy(source_dir + "\\" + "driver.avi", destinationDir + "\\" + "driver.avi");
                // });



                if (File.Exists(source_dir + "\\" + "outPutVer.jpg"))
                    File.Copy(source_dir + "\\" + "outPutVer.jpg", destinationDir + "\\" + "outPutVer.jpg");

                if (File.Exists(source_dir + "\\" + "Driver.jpg"))
                {

                    Task tdriver = Task.Run(() =>
                    {

                        List<Image<Bgr, Byte>> Image_Array = GetVideoFrames(1000, source_dir + "\\" + "driver.avi"); //10 Secounds
                        try
                        {
                            if (Image_Array.Count > 1)
                            {


                                saveJpeg(destinationDir + "\\" + "Driver.jpg", Image_Array[1].ToBitmap(), 70);
                            }
                            //Image_Array[1].Save(destinationDir + "\\" + "Driver.bmp");


                            else
                            {
                                saveJpeg(destinationDir + "\\" + "Driver.jpg", Image_Array[0].ToBitmap(), 70);
                            }
                        }
                        catch (Exception)
                        {


                        }



                    });

                    // File.Copy(source_dir + "\\" + "Driver.bmp", destinationDir + "\\" + "Driver.bmp");
                }

                if (File.Exists(source_dir + "\\" + "m1-0000.avi"))
                    File.Copy(source_dir + "\\" + "m1-0000.avi", destinationDir + "\\" + "m1-0000.avi");

                if (File.Exists(source_dir + "\\" + "Scene.bmp"))
                    File.Copy(source_dir + "\\" + "Scene.bmp", destinationDir + "\\" + "Scene.bmp");

                if (File.Exists(source_dir + "\\" + "LpNum.bmp"))
                    File.Copy(source_dir + "\\" + "LpNum.bmp", destinationDir + "\\" + "LPNum.bmp");

                if (rushMode)
                {
                    if (File.Exists(source_dir + "\\" + "translation.dat"))
                        File.Copy(source_dir + "\\" + "translation.dat", destinationDir + "\\" + "translation.dat");

                    if (File.Exists(source_dir + "\\" + "outPutVer.jpg"))
                        File.Copy(source_dir + "\\" + "outPutVer.jpg", destinationDir + "\\" + "outPutVer.jpg");
                }


                return destinationDir;
                //=========================================================================================================================================

                //ThreadPool.QueueUserWorkItem(o => RefreshGrid());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
