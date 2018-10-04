using System;
using System.Collections.Generic;
using System.ComponentModel;
using gx;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IVISS.View;
using IVISS.Presenter;
using IVISS.Classes;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using IVISS.Utility;
using AForge.Video;
using AForge.Video.FFMPEG;
using System.Runtime.InteropServices;

namespace IVISS
{
    public partial class Main : Form, IMain
    {
        
        MainPresenter presenter;

        public event EventHandler BtnLights;
        public event EventHandler BtnAirClean;
        public event EventHandler BtnCamera;
        public event EventHandler BtnSearchRecords;
        public event EventHandler BtnRecordingOn;
        public event EventHandler BtnRecordingOff;

        public event EventHandler BtnSettings;

        private string m_RecordingPath = @"C:\IVISSTemp\m1.avi";

        //=======================================================================
        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenFilesDriver(string filePath);

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseFilesDriver();

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitializeDriver(IntPtr hwnd, IntPtr width, IntPtr height);

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SeekDriver(IntPtr frameNo);

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SaveDriver(IntPtr frameNo);
        //=======================================================================

        //====================================== NCD Component ========================================

        private bool m_Lights = false;
        private bool m_Camera = false;
        private bool m_Recording = false;
        private bool m_Auto = false;
        private bool m_Live = false;
        private bool m_Overlay = false;

        //============================================================================================

        ANPR_Thread anprThread;
        private bool m_Anpr_Active = false;

        //============================================================================================

        private string m_PlateColor;
        private string m_PlateSubColor;
        private string m_Origin;
        private string m_Accuracy;

        private int m_TotalFrame_Driver;
        private string m_IVISS_Recording_Path;

        //============================================================================================

        private string Client_ID = string.Empty;

        VideoFileWriter writer = new VideoFileWriter();
        DateTime firstRecordedFrameTimeStamp, nextWrittenFrameTimeStamp;

        public Main()
        {
            InitializeComponent();

            presenter = new MainPresenter(this);

            // On Cancel Close App
            frmLogin frm = new frmLogin();
            if (frm.ShowDialog() == DialogResult.Cancel)
            {
                Environment.Exit(0);
            }

            //******************************* LIVE ENTRY ACCURACY BOX ***********************************
            SetLiveAccuracyBox();
            //*******************************************************************************************

            //*********************************** FILL SETTINGS *****************************************
            Task t2 = Task.Run(() => FillSettings());
            //*******************************************************************************************

            //*********************************** READ CLIENT ID ****************************************
            Task t3 = Task.Run(() =>
            {
                if (File.Exists("ClientID.txt"))
                {
                    Client_ID = File.ReadAllText("ClientID.txt");
                }
            });
            //*******************************************************************************************

            //********************************** CONNECT TO RELAY ***************************************
            Task tNCD = Task.Run(() =>
            {
                this.NCDConnect();
            });
            //*******************************************************************************************

            anprThread = new ANPR_Thread();
            anprThread.anpr_result += AnprThread_anpr_result;
        }

        private void AnprThread_anpr_result(ANPR_RESULT_STRUCT result)
        {
            //if (!alpr.b_ALPRProcessing)
            //    return;

            gxPG4 frame = result.frame;
            string text = result.text;
            string textascii = result.textAscii;
            //Bitmap bm = result.image;

            //string lp = string.Empty;
            string dateTime = DateTime.Now.ToString();
            string status = "Recognized";
            string plateColor = DColor.ToName(result.plateColor);
            string plateSubColor = DColor.ToName(result.plateSubColor);
            string origin = PlateTypes.ToOrigin(result.lpOrigin);
            string accuracy = PlateTypes.ToAccuracy(result.accuracy).ToString();

            BeginInvoke((MethodInvoker)delegate
            {
                ////Draw the frame of the number plate on the image
                //Graphics g = Graphics.FromImage((Image)bm);
                //g.DrawLine(new Pen(Color.Red), frame.x1, frame.y1, frame.x2, frame.y2);
                //g.DrawLine(new Pen(Color.Red), frame.x2, frame.y2, frame.x3, frame.y3);
                //g.DrawLine(new Pen(Color.Red), frame.x3, frame.y3, frame.x4, frame.y4);
                //g.DrawLine(new Pen(Color.Red), frame.x4, frame.y4, frame.x1, frame.y1);


                // set displayed plate text
                //txtLiveLPEnglish.Text = textascii;
                //this.txtLiveLPArabic.Text = text;

                txtLiveLPEnglish.Text = text.PureAscii().Trim();
                txtLiveLPArabic.Text = text.PureUnicode().Trim();

                //txtLiveLPEnglish.Text = text.PureAscii().Trim();
                //txtLiveLPArabic.Text = text;

                this.m_PlateColor = plateColor;
                this.m_PlateSubColor = plateSubColor;
                this.m_Origin = origin;

                this.m_Accuracy = accuracy;
                this.lblLiveAccuracy.Text = accuracy + "%";
                this.pBoxLiveALPRBar.BackgroundImage = Global.GetAccuracyBitmap(accuracy);

                //Task t = Task.Run(() =>
                //{
                this.SetAuthorizationBoxLive(txtLiveLPEnglish.Text, txtLiveLPArabic.Text);
                //});

                Task tALPRImg = Task.Run(() =>
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        result.image.Save(Global.TEMP_FOLDER + "\\LpNum.bmp");
                    });
                });

                //alpr.b_ALPRProcessing = false;
                //////saving to database
                ////if (!b_m_Auto)
                ////{
                ////    //disconnectButton_Click(null,null);
                ////}

                ////if (b_m_Auto)
                ////    ThreadPool.QueueUserWorkItem(o => AddtoDatabase(platetextLabel.Text, lblPlatePrefix.Text, lblLPOrigin.Text, lblStatus.Text, lblPlateColor.Text, lblPlateSubColor.Text, lblAccuracy.Text));
            });
        }

        private void SetAuthorizationBoxLive(string licensePlate, string licensePlateArab)
        {

            var authorization = string.Empty;
            var classification = string.Empty;

            using (IVISSEntities db = new IVISSEntities())
            {
                //var visitor = (from v in db.Visitors
                //               where v.visitor_license_plate == licensePlate || v.visitor_license_plate_arabic == licensePlateArab
                //               select v).FirstOrDefault();

                var visitor = from v in db.Visitors
                              select v;

                if (licensePlate.Trim().Length > 0)
                    visitor = visitor.Where(o => o.visitor_license_plate == licensePlate);
                else
                    visitor = visitor.Where(o => o.visitor_license_plate_arabic == licensePlateArab);

                var vi = visitor.FirstOrDefault();

                //visitor = visitor.FirstOrDefault();

                if (vi != null)
                {
                    authorization = vi.visitor_authorization;
                    classification = vi.visitor_classification;
                }
                else
                {
                    authorization = "";
                    classification = "VISITOR";
                }
            }


            // classification
            if (authorization == "PROHIBITED") //red
            {
                pBoxAuthorizationLive.Image = (Bitmap)IVISS.Properties.Resources.red_card;
                //this.PlaySound();
            }
            else if (authorization == "AUTHORIZED") //green
            {
                pBoxAuthorizationLive.Image = (Bitmap)IVISS.Properties.Resources.green_card;
            }
            else
            {
                pBoxAuthorizationLive.Image = (Bitmap)IVISS.Properties.Resources.yellow_card;
            }

            if (authorization.Length > 0)
                lblVisitorClassificationLive.Text = authorization;
            else
                lblVisitorClassificationLive.Text = "VISITOR";
        }

        private void RunIPCameras()
        {
            //********************************************************* Driver Entry *************************************************************
            //************************************************************************************************************************************
            // create video source
            MJPEGStream mjpegSourceDriver = new MJPEGStream(@"http://" + Global.mDriverCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view&Resolution=1920x1080");

            if (Global.mDriverCamPassword!=null && Global.mDriverCamPassword.Length > 0)
            {
                mjpegSourceDriver.Login = "admin";
                mjpegSourceDriver.Password = Global.mDriverCamPassword;
            }
            // open it
            OpenVideoSourceDriver(mjpegSourceDriver);
            //************************************************************************************************************************************

            //********************************************************* Scene Cam Entry *************************************************************
            //************************************************************************************************************************************
            if (Global.SCENE_CAM)
            {
                // create video source
                MJPEGStream mjpegSourceScene = new MJPEGStream(@"http://" + Global.mSceneCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view&Resolution=1920x1080");

                if (Global.mScenecamPassword!=null && Global.mScenecamPassword.Length > 0)
                {
                    mjpegSourceScene.Login = "admin";
                    mjpegSourceScene.Password = Global.mScenecamPassword; //Tasc12345
                }
                // open it
                OpenVideoSourceScene(mjpegSourceScene);
            }
            //************************************************************************************************************************************

            //********************************************************* ALPR Entry ***************************************************************
            //************************************************************************************************************************************
            if (Global.ALPR_CAM)
            {
                // create video source
                MJPEGStream mjpegSourceALPR = new MJPEGStream("http://" + Global.mALPRCamIP + ":9901");

                //mjpegSourceALPRExit.Login = "admin";
                //mjpegSourceALPRExit.Password = "4321";

                // open it
                OpenVideoSourceLP(mjpegSourceALPR);
            }
        }

        // Open video source
        private void OpenVideoSourceDriver(IVideoSource source)
        {
            try
            {
                // set busy cursor
                this.Cursor = Cursors.WaitCursor;

                // stop current video source
                CloseVideoSourceDriver();

                // start new video source
                vspDriver.VideoSource = source;
                vspDriver.Start();

            }
            catch (Exception ex)
            {
                Global.WriteLog("OpenVideoSource: " + ex.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void OpenVideoSourceScene(IVideoSource source)
        {
            try
            {
                // set busy cursor
                this.Cursor = Cursors.WaitCursor;

                // stop current video source
                CloseVideoSourceScene();

                // start new video source
                vspScene.VideoSource = source;
                vspScene.Start();

            }
            catch (Exception ex)
            {
                Global.WriteLog("OpenVideoSource: " + ex.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void OpenVideoSourceLP(IVideoSource source)
        {
            try
            {
                // set busy cursor
                this.Cursor = Cursors.WaitCursor;

                // stop current video source
                CloseVideoSourceLP();

                // start new video source
                vspLP.VideoSource = source;
                vspLP.Start();
            }
            catch (Exception ex)
            {
                Global.WriteLog("OpenVideoSource: " + ex.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void CloseVideoSourceDriver()
        {
            if (vspDriver.VideoSource != null)
            {
                vspDriver.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!vspDriver.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (vspDriver.IsRunning)
                {
                    vspDriver.Stop();
                }

                vspDriver.VideoSource = null;
            }
        }

        private void CloseVideoSourceScene()
        {
            if (vspScene.VideoSource != null)
            {
                vspScene.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!vspScene.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (vspScene.IsRunning)
                {
                    vspScene.Stop();
                }

                vspScene.VideoSource = null;
            }
        }

        private void CloseVideoSourceLP()
        {
            if (vspLP.VideoSource != null)
            {
                vspLP.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!vspLP.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (vspLP.IsRunning)
                {
                    vspLP.Stop();
                }

                vspLP.VideoSource = null;
            }
        }

        private void SetLiveAccuracyBox()
        {
            var pos = this.PointToScreen(lblVisitorClassificationLive.Location);
            pos = pBoxAuthorizationLive.PointToClient(pos);

            //lblVisitorClassificationLive.Parent = pBoxAuthorizationLive;
            pBoxAuthorizationLive.Controls.Add(lblVisitorClassificationLive);
            lblVisitorClassificationLive.Location = new Point(pos.X + 15, 150); //pos;
            lblVisitorClassificationLive.BackColor = Color.Transparent;

            var pos1 = this.PointToScreen(pBoxLiveALPRBar.Location);
            pos1 = pBoxAuthorizationLive.PointToClient(pos1);

            pBoxLiveALPRBar.Parent = pBoxAuthorizationLive;
            pBoxLiveALPRBar.Location = new Point(pos1.X + 15, 45);
            pBoxLiveALPRBar.BackColor = Color.Transparent;

            var pos2 = this.PointToScreen(lblLiveAccuracy.Location);
            pos2 = pBoxAuthorizationLive.PointToClient(pos2);

            lblLiveAccuracy.Parent = pBoxAuthorizationLive;
            lblLiveAccuracy.Location = new Point(pos2.X + 15, 70);
            lblLiveAccuracy.BackColor = Color.Transparent;
            lblLiveAccuracy.BringToFront();

            var pos3 = this.PointToScreen(lblLiveOCRAccuracy.Location);
            pos3 = pBoxAuthorizationLive.PointToClient(pos3);

            lblLiveOCRAccuracy.Parent = pBoxAuthorizationLive;
            lblLiveOCRAccuracy.Location = new Point(pos3.X + 10, 8);
            lblLiveOCRAccuracy.BackColor = Color.Transparent;
        }

        private void FillSettings()
        {
            presenter.FillSettings();
        }
       
        #region NCD.COMPONENT

        void NCDConnect()
        {
            //using (var db = new IVISSEntities())
            //{
            //    var query = (from ss in db.SystemSettings
            //                 where ss.Id == 1
            //                 select new { ss.PortNo, ss.AirWash, ss.Barrier, ss.Cameras, ss.Lights }).FirstOrDefault();

            //    if (query != null)
            //    {
            //        this.m_NCD_PortNo = query.PortNo;
            //        this.m_NCD_AirWash = query.AirWash;
            //        this.m_NCD_Barrier = query.Barrier;
            //        this.m_NCD_Cameras = query.Cameras;
            //        this.m_NCD_Lights = query.Lights;
            //    }
            //}

            try
            {

                this.ncd1.PortName = "COM" + Global.mNCDPortNo;
                //this.ncdComponent1.PortName = "COM3";
                this.ncd1.BaudRate = 115200;

                this.ncd1.OpenPort();       // open the port

                if (!this.ncd1.IsOpen)
                {
                    //MessageBox.Show("Fail to open");
                    //Application.Exit();
                }

                //if (this.ncd1.IsOpen)
                //{
                //    timer1.Interval = 100;      // set the interval
                //    timer1.Enabled = true;      // start the timer
                //}

                this.ncd1.ProXR.RelayBanks.SelectBank(1);   // select the first bank
            }
            catch
            {

            }
        }

        private void TurnLC(string portNo, Boolean On)
        {
            try
            {
                if (On)
                {
                    this.ncd1.ProXR.RelayBanks.TurnOnRelay(Convert.ToByte(portNo));
                }
                else
                {
                    this.ncd1.ProXR.RelayBanks.TurnOffRelay(Convert.ToByte(portNo));
                }
            }
            catch
            {
            }
        }

        private void TurnRelay(string portNo, Boolean On)
        {
            try
            {
                this.ncd1.ProXR.RelayBanks.TurnOnRelay(Convert.ToByte(portNo));
                Thread.Sleep(2000);

                this.ncd1.ProXR.RelayBanks.TurnOffRelay(Convert.ToByte(portNo));
                Thread.Sleep(2000);

                this.ncd1.ProXR.RelayBanks.TurnOnRelay(Convert.ToByte(portNo));
                Thread.Sleep(2000);

                this.ncd1.ProXR.RelayBanks.TurnOffRelay(Convert.ToByte(portNo));
            }
            catch
            {
            }
        }

        #endregion

        #region KEYLOK

        private void ProcessKeyLok()
        {
            if (!presenter.ProcessKeyLok())
                Environment.Exit(0);
        }

        #endregion 

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
            //pBoxStitch.
            pBoxStitch.Image = Image.FromFile(@"C:\IVISSTemp\outPutVer.jpg");
            pBoxStitch.BackgroundImageLayout = ImageLayout.Zoom;

            //Image flipImage = pBoxStitch.Image;
            //flipImage.RotateFlip(RotateFlipType.Rotate90FlipY);
            //pBoxStitch.Image = flipImage;

            //pBoxComparison.Image = Image.FromFile(@"C:\IVISSTemp\outPutVer.jpg");
            pBoxComparison.Image = Image.FromFile(@"C:\IVISSTemp\photoshop2.jpg");
            pBoxComparison.BackgroundImageLayout = ImageLayout.Stretch;

            //pBoxDriver.Image = Image.FromFile(@"C:\IVISSTemp\driver.bmp");
            //pBoxScene.Image = Image.FromFile(@"C:\IVISSTemp\scene.bmp");
            //pBoxLP.Image = Image.FromFile(@"C:\IVISSTemp\lpnum.bmp");

            lblLiveAccuracy.Text = "99%";

            lblRelay1.Text = Global.mlblRelay1;
            lblRelay1Arab.Text = Global.mlblRelayArab1;

            lblRelay2.Text = Global.mlblRelay2;
            lblRelay2Arab.Text = Global.mlblRelayArab2;

            lblRelay3.Text = Global.mlblRelay3;
            lblRelay3Arab.Text = Global.mlblRelayArab3;

            lblRelay4.Text = Global.mlblRelay4;
            lblRelay4Arab.Text = Global.mlblRelayArab4;

            this.RunIPCameras();

        }

        private void btnSearchRecords_Click(object sender, EventArgs e)
        {
            BtnSearchRecords(sender, e);
        }

        public void BindData(DataTable dt)
        {
            dgView.DataSource = dt;
        }

        public string lpNumEnglish
        {
            set { this.txtLiveLPEnglish.Text = value; }
            get { return this.txtLiveLPEnglish.Text; }
        }

        public string lpNumArabic
        {
            set { this.txtLiveLPArabic.Text = value; }
            get { return this.txtLiveLPArabic.Text; }
        }

        public Bitmap stitchImage
        {
            set { this.pBoxStitch.Image = value; }
        }

        public Bitmap comparisonImage
        {
            set { this.pBoxComparison.Image = value; }
        }

        public string recordingPath
        {
            set { m_RecordingPath = value; }
            get { return this.m_RecordingPath; }
        }

        public void Save()
        {

        }

        private void btnRecordingOff_Click(object sender, EventArgs e)
        {
            BtnRecordingOff(sender, e);
            m_Recording = true;

            writer.Close();

            // On-screen display
            //ShowStatus();

            btnRecordingOn.Visible = true;
            btnRecordingOff.Visible = false;
        }

        private void btnRecordingOn_Click(object sender, EventArgs e)
        {
            BtnRecordingOn(sender, e);
            //StopRecord();

            //stop recording driver camera
            m_Recording = false;

            // ********************** EMGU ************************************************************************************************************************
            //int FrameRate = 15; //Set the framerate manually as a camera would retun 0 if we use GetCaptureProperty()
            ////Set up a video writer component
            ///*                                        ---USE----
            ///* VideoWriter(string fileName, int compressionCode, int fps, int width, int height, bool isColor)
            // *
            // * Compression code. 
            // *      Usually computed using CvInvoke.CV_FOURCC. On windows use -1 to open a codec selection dialog. 
            // *      On Linux, use CvInvoke.CV_FOURCC('I', 'Y', 'U', 'V') for default codec for the specific file name. 
            // * 
            // * Compression code. 
            // *      -1: allows the user to choose the codec from a dialog at runtime 
            // *       0: creates an uncompressed AVI file (the filename must have a .avi extension) 
            // *
            // * isColor.
            // *      true if this is a color video, false otherwise
            // */
            //VW = new VideoWriter("C:\\vittemp\\driver.avi", CvInvoke.CV_FOURCC('W', 'M', 'V', '3'), (int)FrameRate, DRIVER_CAM_WIDTH, DRIVER_CAM_HEIGHT, true);
            //*****************************************************************************************************************************************************

            //***********************  AFORGE *********************************************************************************************************************

            firstRecordedFrameTimeStamp = DateTime.MinValue;
            nextWrittenFrameTimeStamp = DateTime.MinValue;

            writer.Open(Global.TEMP_FOLDER + "\\driver.avi", Global.DRIVER_CAM_WIDTH, Global.DRIVER_CAM_HEIGHT, 30, VideoCodec.WMV2, 10000 * 1000); // 

            //*****************************************************************************************************************************************************

            string licensePlate = this.txtLiveLPEnglish.Text;
            string licensePlateArab = this.txtLiveLPArabic.Text;

            btnRecordingOn.Visible = false;
            btnRecordingOff.Visible = true;
        }

        private void SetFilesToOpen(string path, string plate, string plateArab, string authorization, string accuracy, string gate, string visitorStatus)
        {

            InitializeDriver((IntPtr)vspDriver.Handle, (IntPtr)this.vspDriver.Width, (IntPtr)this.vspDriver.Height);

            Task t = Task.Run(() =>
            {
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        this.txtLiveLPEnglish.Text = plate;
                        this.txtLiveLPArabic.Text = plateArab; //Regex.Replace(plateArab, @"[^\u0020-\u007E]", string.Empty);

                        if (accuracy != null && accuracy.Length > 0)
                            lblLiveAccuracy.Text = accuracy + "%";
                        else
                            lblLiveAccuracy.Text = "99%";

                        this.lblVisitorClassificationLive.Text = (authorization == "VISITOR") ? "    " + authorization : authorization;

                        this.SetAuthorizationBox(authorization);

                //if (File.Exists(path + "\\Driver.bmp"))
                //{
                //    using (FileStream fs = new FileStream(path + "\\Driver.bmp", FileMode.Open))
                //    {
                //        vspDriver.BackgroundImage = Image.FromStream(fs);
                //        fs.Close();
                //    }
                //}
                //else
                //{
                //    vspDriver.BackgroundImage = null;
                //}

                    if (File.Exists(path + "\\Driver.bmp"))
                        this.vspDriver.BackgroundImage = Image.FromFile(path + "\\Driver.bmp");
                    else
                        this.vspDriver.BackgroundImage = null;

                    if (File.Exists(path + "\\Scene.bmp"))
                        this.vspScene.BackgroundImage = Image.FromFile(path + "\\Scene.bmp");
                    else
                        this.vspScene.BackgroundImage = null;

                    if (File.Exists(path + "\\LpNum.bmp"))
                        this.vspLP.BackgroundImage = Image.FromFile(path + "\\LpNum.bmp");
                    else
                        this.vspLP.BackgroundImage = null;

                //************************************************** IF LCC SHOW LICENSE PLATE Image on Top ******************************************************
                    //if (File.Exists(path + "\\LpNum.bmp"))
                    //{
                    //    using (FileStream fs = new FileStream(path + "\\LpNum.bmp", FileMode.Open))
                    //    {
                    //        vspLP.BackgroundImage = Image.FromStream(fs);
                    //        fs.Close();
                    //    }
                    //}
                    //else
                    //{
                    //    vspLP.BackgroundImage = null;
                    //}
                //************************************************************************************************************************************************
                });

                //Initialize(hwnd, (IntPtr)m_ScreenWidth);
                //stop video if not stopped already
                //Stop();

                //******************************************** Open Driver File *******************************************************

                if (m_TotalFrame_Driver > 0)
                {
                    CloseFilesDriver();
                    m_TotalFrame_Driver = 0;
                    tbDriver.Value = 1;
                }

                m_TotalFrame_Driver = OpenFilesDriver(path);

                if (m_TotalFrame_Driver > 0)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbDriver.SetRange(1, m_TotalFrame_Driver);
                    });
                }

                //*********************************************************************************************************************

            }
                catch (Exception ex) { /*MessageBox.Show(ex.ToString());*/ }
            });

            t.Wait();
        }

        private void SetAuthorizationBox(string authorization, bool play = false)
        {
            // classification
            if (authorization == "PROHIBITED") //yellow
            {
                pBoxAuthorizationLive.BackgroundImage = (Bitmap)IVISS.Properties.Resources.red_card;

                pBoxLiveALPRBar.BackColor = Color.FromArgb(235, 107, 97);

                this.lblLiveAccuracy.BackColor = Color.FromArgb(235, 107, 97);
                this.lblLiveOCRAccuracy.BackColor = Color.FromArgb(235, 107, 97);

                lblVisitorClassificationLive.BackColor = Color.FromArgb(241, 79, 73);

                //// Play Sound
                //if (play)
                //    this.PlaySound();
            }
            else if (authorization == "AUTHORIZED") //green
            {
                pBoxAuthorizationLive.BackgroundImage = (Bitmap)IVISS.Properties.Resources.green_card;

                pBoxLiveALPRBar.BackColor = Color.FromArgb(121, 246, 152);

                this.lblLiveAccuracy.BackColor = Color.FromArgb(121, 246, 152);
                this.lblLiveOCRAccuracy.BackColor = Color.FromArgb(121, 246, 152);

                lblVisitorClassificationLive.BackColor = Color.FromArgb(0, 204, 79);

                //this.lblVisitorClassification.ForeColor = Color.White;
                //this.lblOCRAccuracy.ForeColor = Color.White;
                //this.lblPlayAccuracy.ForeColor = Color.White;
            }
            else
            {
                pBoxAuthorizationLive.BackgroundImage = (Bitmap)IVISS.Properties.Resources.yellow_card;

                pBoxLiveALPRBar.BackColor = Color.FromArgb(252, 255, 132);

                this.lblLiveAccuracy.BackColor = Color.FromArgb(252, 255, 132);
                this.lblLiveOCRAccuracy.BackColor = Color.FromArgb(252, 255, 132);
                
                lblVisitorClassificationLive.BackColor = Color.FromArgb(238, 218, 23);
            }

            if (authorization.Length > 0)
                lblVisitorClassificationLive.Text = authorization;
            else
                lblVisitorClassificationLive.Text = "VISITOR";
        }
  
        private void btnSettings_Click(object sender, EventArgs e)
        {
            BtnSettings(sender, e);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            ncd1.ClosePort();

            CloseVideoSourceDriver();
            CloseVideoSourceScene();
            CloseVideoSourceLP();

        }

        private void btnOpen1_Click(object sender, EventArgs e)
        {
            this.btnOpen1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
            this.btnClose1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;

            TurnLC(Global.mNCDPort1, true);
        }

        private void btnClose1_Click(object sender, EventArgs e)
        {
            this.btnOpen1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;
            this.btnClose1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;

            TurnLC(Global.mNCDPort1, false);
        }

        private void btnOpen2_Click(object sender, EventArgs e)
        {
            this.btnOpen2.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
            this.btnClose2.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;
            
            TurnLC(Global.mNCDPort2, true);
        }

        private void btnClose2_Click(object sender, EventArgs e)
        {
            this.btnOpen2.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;
            this.btnClose2.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;

            TurnLC(Global.mNCDPort2, false);
        }

        private void btnOpen3_Click(object sender, EventArgs e)
        {
            this.btnOpen3.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
            this.btnClose3.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;

            TurnLC(Global.mNCDPort3, true);
        }

        private void btnClose3_Click(object sender, EventArgs e)
        {
            this.btnOpen3.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;
            this.btnClose3.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;

            TurnLC(Global.mNCDPort3, false);
        }

        private void btnOpen4_Click(object sender, EventArgs e)
        {
            this.btnOpen4.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
            this.btnClose4.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;

            TurnLC(Global.mNCDPort4, true);
        }

        private void btnClose4_Click(object sender, EventArgs e)
        {
            this.btnOpen4.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;
            this.btnClose4.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;

            TurnLC(Global.mNCDPort4, false);
        }

        private void vspLP_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (image != null && Global.b_StartCapturing && !m_Anpr_Active)
                {
                    anprThread.SetImage(new Bitmap(image));
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(e.ToString());
                Global.WriteLog("vspExitALPR_NewFrame: " + ex.ToString());
            }
        }

        private void btnClickSnapshot_Click(object sender, EventArgs e)
        {
            btnClickSnapshot.Visible = false;
            tbDriver.Visible = true;
        }

        private void tbDriver_Scroll(object sender, EventArgs e)
        {
            if (m_TotalFrame_Driver > 0)
            {
                SeekDriver((IntPtr)tbDriver.Value);
            }
        }

        private void vspDriver_Click(object sender, EventArgs e)
        {
            if (m_TotalFrame_Driver > 0)
            {
                SaveDriver((IntPtr)tbDriver.Value);

                var path = (String.IsNullOrEmpty(m_IVISS_Recording_Path)) ? Global.LAST_FOLDER : m_IVISS_Recording_Path;

                System.Threading.Thread.Sleep(200);

                if (File.Exists(path + "\\frame.ppm"))
                {
                    File.Delete(path + "\\frame.ppm");
                }
            }

            using (Graphics g = vspDriver.CreateGraphics())
            {
                using (Font myFont = new Font("Arial", 10))
                {
                    g.DrawString("Driver snapshot taken", myFont, Brushes.White, new PointF(2f, vspDriver.Height - 20)); //new Point(2, 2)
                }
            }

            tbDriver.Visible = false;
            btnClickSnapshot.Visible = true;
        }

        private void dgView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //dgView.ReadOnly = true;

                if (e.RowIndex >= 0)
                {
                    string plate = dgView.Rows[e.RowIndex].Cells["License"].Value.ToString();
                    string plateArab = dgView.Rows[e.RowIndex].Cells["ArabicLicense"].Value.ToString();
                    string datetime = dgView.Rows[e.RowIndex].Cells["Date"].Value.ToString();
                    string path = dgView.Rows[e.RowIndex].Cells["Path"].Value.ToString();
                    string accuracy = dgView.Rows[e.RowIndex].Cells["Accuracy"].Value.ToString();
                    string classification = dgView.Rows[e.RowIndex].Cells["Classification"].Value.ToString();
                    string gate = dgView.Rows[e.RowIndex].Cells["Gate"].Value.ToString();
                    string visitorAccess = dgView.Rows[e.RowIndex].Cells["Status"].Value.ToString(); ; //Status


                    if (path != null && path.Length > 0)
                    {
                        m_IVISS_Recording_Path = path;
                        //m_IVISS_Plate_Number = plate;
                        //m_IVISS_Plate_DateTime = datetime;

                        if (Directory.Exists(path))
                        {
                            SetFilesToOpen(path, plate, plateArab, classification, accuracy, gate, visitorAccess);
                        }
                        else
                        {
                            MessageBox.Show("Path not found");
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { /* dgView.ReadOnly = false; */ }
        }

        private void btnClickSnapshot_Click_1(object sender, EventArgs e)
        {
            btnClickSnapshot.Visible = false;
            tbDriver.Visible = true;
        }

        private void vspDriver_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (m_Recording)
                {

                    //var frame = eventArgs.Frame;   // That's a Bitmap
                    var now = DateTime.Now;

                    if (this.firstRecordedFrameTimeStamp == DateTime.MinValue)
                        this.firstRecordedFrameTimeStamp = now;

                    if (this.nextWrittenFrameTimeStamp < now)   // this.nextWrittenFrameTimeStamp is initialized with DateTime.MinValue
                    {
                        var elapsed = now - this.firstRecordedFrameTimeStamp;

                        //this.videoWriter.WriteVideoFrame(frame.Resize(720, 480), elapsed);

                        writer.WriteVideoFrame(image, elapsed);
                        this.nextWrittenFrameTimeStamp = now.AddMilliseconds(1000 / 30);
                    }

                    //writer.WriteVideoFrame(image);

                }
            }
            catch (Exception ex)
            {
                Global.AppendString("videoSourcePlayer_NewFrame: " + ex.ToString());
                //Console.Write(ex.ToString());
            }
        }

    }

    public static class Extensions
    {
        public static string PureAscii(this string source, char nil = ' ')
        {
            var min = '\u0000';
            var max = '\u007F';
            return source.Select(c => c < min ? nil : c > max ? nil : c).ToText();
        }

        public static string PureUnicode(this string source, char nil = ' ')
        {
            // 0x0600-0x06FF	
            var min = '\u0600';
            var max = '\u06FF';
            return source.Select(c => c < min ? nil : c > max ? nil : c).ToTextUnicode();
        }

        public static string ToText(this IEnumerable<char> source)
        {
            var buffer = new StringBuilder();
            foreach (var c in source)
            {
                buffer.Append(c);
            }
            return buffer.ToString();
        }

        public static string ToTextUnicode(this IEnumerable<char> source)
        {
            var buffer = new StringBuilder();
            foreach (var c in source)
            {
                buffer.Append(c);
                buffer.Append('\u200E');

                //buffer.Append('\u001e');
                //buffer.Append('\u0020');
            }

            return buffer.ToString();

            //return Regex.Replace(buffer.ToString(), @"[^\u0600-\u06FF]", string.Empty);
        }
    }
}
