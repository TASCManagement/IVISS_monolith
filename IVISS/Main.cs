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
using System.Diagnostics;
using System.Drawing.Imaging;
using ConfigurationUtility;
using System.Net;

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

        public event EventHandler FormLoaded;
        public event EventHandler FormIsClosing;

        private string m_RecordingPath = @"C:\IVISSTemp\m1.avi";
        private string m_SelectedImage = string.Empty;
        private bool m_FOB = false;

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
        private bool b_ALPRProcessing = true;

        //============================================================================================

        private string m_PlateColor;
        private string m_PlateSubColor;
        private string m_Origin;
        private int m_TotalFrameDriver;
        private string m_IVISSRecordingPath;

        private string m_StitchPath;
        private string m_ComparisonPath;
        private string m_DestinationDir;
        private Image imgCompositeImage;

        //============================================================================================

        private string Client_ID = string.Empty;
        VideoFileWriter writer = new VideoFileWriter();
        DateTime firstRecordedFrameTimeStamp, nextWrittenFrameTimeStamp;

        System.Threading.Timer timerALPR;

        public Main()
        {
            InitializeComponent();

            // Show license renewal dialog
            this.ShowLicenseRenewal();

            // On Cancel Close App
            frmLoginV1 frm = new frmLoginV1();
            if (frm.ShowDialog() == DialogResult.Cancel)
            {
                Environment.Exit(0);
            }

            presenter = new MainPresenter(this);

            //******************************* LIVE ENTRY ACCURACY BOX ***********************************
            SetLiveAccuracyBox();
            //*******************************************************************************************

            //*********************************** FILL SETTINGS *****************************************
            Task t2 = Task.Run(() =>
            {
                // Fill database settings
                this.FillSettings();

                // Connect NCD
                this.NCDConnect();
            });
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
            //Task tNCD = Task.Run(() =>
            //{
            //    this.NCDConnect();
            //});
            //*******************************************************************************************

            if (!Global.DEMO)
            {
                anprThread = new ANPR_Thread();
                anprThread.anpr_result += AnprThread_anpr_result;

                timerALPR = new System.Threading.Timer(new TimerCallback(LoopLiveALPR), null, 0, Global.LOOP_INTERVAL); // Loop Interval is in millisecond
            }
            //this.lblGate.Text = Global.m_Gate_Name;
        }

        private void LoopLiveALPR(object obj)
        {
            //int index = 0;

            if (Global.ENTRY_LOOP_SENSOR)
            {
                //if (global.b_Connected[global.LIVE_ALPR])
                //{
                string address = string.Format("http://" + Global.ALPR_CAMERA_HOST[Global.ENTRY_ALPR] + "/trigger/gpiotrigger?getgpin&wfilter=1");
                string response = string.Empty;

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        response = client.DownloadString(address);
                    }

                }
                catch (Exception ex) { }

                Global.b_StartCapturing[Global.ENTRY_ALPR] = (response.IndexOf("1") != -1);
            }
            else
            {
                Global.b_StartCapturing[Global.ENTRY_ALPR] = true;
            }
        }

        private void ShowLicenseRenewal()
        {
            var directory = Global.LIC_DIRECTORY;
            int diff = 0;

            if (File.Exists(directory + "sys.dll"))
            {
                try
                {
                    var txt = StringCipher.Decrypt(File.ReadAllText(directory + "sys.dll"), Global.PASSPHRASE);

                    if (txt != "1")
                    {
                        try
                        {
                            if (DateTime.Now > Convert.ToDateTime(txt))
                            {
                                // set to 1 if license has expired
                                File.WriteAllText(directory + "sys.dll", StringCipher.Encrypt("1", Global.PASSPHRASE));
                            }
                            else
                            {
                                // start giving message from 30 days prior to expiration
                                // from 4-6 months give no message but give license renewal dialog
                                diff = ((Convert.ToDateTime(txt).AddDays(-60)) - DateTime.Now).Days;

                                if (diff >= 0 && diff <= Global.RENEWAL_MESSAGE_DAYS)
                                {
                                    Global.ShowMessage("License is going to expire in " + diff + " days", false);
                                }
                                else if (diff >= Global.RENEWAL_GRACE_DAYS && diff < 0)
                                {
                                    var frmLR = new frmLicRenewal();
                                    if (frmLR.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                    }
                                }
                                return;
                            }
                        }
                        catch (Exception ex)
                        { }
                    }

                }
                catch (Exception e)
                {
                    //MessageBox.Show("Invalid License");
                    Global.ShowMessage("Invalid License Key", false);
                }

                var frm = new frmLicRenewalV1();
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    //disconnectButton_Click(null, null);
                    //m_bLeaveMsg = false;

                    //after 6 months after renewal dialog close the exe
                    //this.Close();
                }
            }
            else
            {
                //var dt = StringCipher.Encrypt(DateTime.Now.AddDays(global.RENEWAL_LICENSE_DAYS).ToString(), global.PASSPHRASE);
                //if (directory.Length > 0)
                //{
                //    Directory.CreateDirectory(directory);
                //}
                //File.WriteAllText(directory + "sys.dll", dt);

                var frm = new frmLicRenewalV1();
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    //disconnectButton_Click(null, null);
                    //m_bLeaveMsg = false;

                    //after 6 months after renewal dialog close the exe
                    //this.Close();
                }

                //Global.ShowMessage("Invalid License Key", false);
            }
        }

        private void AnprThread_anpr_result(ANPR_RESULT_STRUCT result)
        {
            //if (!b_ALPRProcessing)
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

                txtLiveLPEnglish.Text = text.PureAscii().Trim();
                txtLiveLPArabic.Text = text.PureUnicode().Trim();

                this.plateColor = plateColor;
                this.plateSubColor = plateSubColor;
                this.origin = origin;

                this.accuracy = accuracy;

                // this.lblLiveAccuracy.Text = accuracy + "%";
                // this.pBoxLiveALPRBar.BackgroundImage = Global.GetAccuracyBitmap(accuracy);

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

                b_ALPRProcessing = false;

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

            //if (authorization.Length > 0)
            //    lblVisitorClassificationLive.Text = authorization;
            //else
            //    lblVisitorClassificationLive.Text = "VISITOR";

            SetAuthorizationBox(authorization);
        }

        private void RunIPCameras()
        {
            //********************************************************* Driver Entry *************************************************************
            //************************************************************************************************************************************
            // create video source
            //MJPEGStream mjpegSourceDriver = new MJPEGStream(@"http://" + Global.mDriverCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view&Resolution=1920x1080");
            MJPEGStream mjpegSourceDriver = new MJPEGStream(@"http://" + Global.mDriverCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view");

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
        
        public void LoadCompositeImage(string destDir)
        {
            this.m_DestinationDir = destDir;

            BeginInvoke((MethodInvoker)delegate
            {
                tmCompositeImage.Start();

                pBoxComparison.SizeMode = PictureBoxSizeMode.CenterImage;
                pBoxComparison.Image = Image.FromFile("loader.gif");
            });
        }

        public void LoadImageComparison(string dir)
        {
            try
            {
                if (File.Exists(dir + @"\outPutVer.jpg"))
                {
                    pBoxStitch.SizeMode = PictureBoxSizeMode.StretchImage;

                    //pBoxComparison.Image = Image.FromFile(this.m_DestinationDir + @"\outPutVer.jpg");
                    using (FileStream fs = new FileStream(dir + @"\outPutVer.jpg", FileMode.Open))
                    {
                        pBoxStitch.Image = Image.FromStream(fs);
                        fs.Close();
                    }

                    tmCompositeImage.Stop();
                }
            }
            catch (Exception ex)
            {

            }
        }

        #region VIDEO_SOURCE_PLAYER

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

        private void vspLP_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (image != null && Global.b_StartCapturing[Global.ENTRY_ALPR]) // && !m_Anpr_Active
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

        #endregion 

        private void SetLiveAccuracyBox()
        {
            /*
            var pos = this.PointToScreen(lblVisitorClassificationLive.Location);
            pos = pBoxAuthorizationLive.PointToClient(pos);

            pBoxAuthorizationLive.Controls.Add(lblVisitorClassificationLive);
            lblVisitorClassificationLive.Location = new Point(pos.X + 15, 150);
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
            */
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
                ////this.ncd1.PortName = "COM" + Global.mNCDPortNo;
                this.ncd1.IPAddress = Global.mIPAddress;

                if (!String.IsNullOrEmpty(Global.mListenPort))
                    this.ncd1.Port = int.Parse(Global.mListenPort);

                ////this.ncdComponent1.PortName = "COM3";
                //this.ncd1.BaudRate = 115200;

                BeginInvoke((MethodInvoker)delegate
                {
                    this.ncd1.UsingComPort = false;
                    this.ncd1.OpenPort();       // open the port

                    if (!this.ncd1.IsOpen)
                    {
                        MessageBox.Show("Failed to open Relay Contoller");
                        //Application.Exit();
                    }
                    else
                    {
                        MessageBox.Show("Successfully connected!");
                    }
                });

                //if (this.ncd1.IsOpen)
                //{
                //    timer1.Interval = 100;      // set the interval
                //    timer1.Enabled = true;      // start the timer
                //}

                this.ncd1.ProXR.RelayBanks.SelectBank(1);   // select the first bank
                //NCDLib.WriteBytes(ncd1, )
                //NCDLib.WriteBytesAPI(ncd1, UsingAPI, 254, 116, SetBank.Value);
                //ncd1.WriteBytesAPI(ncd1, true, 256, 116, 1);

                //var NCDLib1 = new NCDLib();
                
                //ncd1.SetTcpWritePace(0);
                //ncd1.WriteBytes();

                //byte[] cmd = { 254, 8 };
                //ncd1.WriteBytesAPI(cmd);

                //ncd1.ProXR.RelayBanks.TurnOnAllRelays();
            }
            catch(Exception ex)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    MessageBox.Show(ex.ToString());
                });
            }
        }

        private void TurnLC(string portNo, Boolean On)
        {
            try
            {
                if (On)
                {
                    this.ncd1.ProXR.RelayBanks.TurnOnRelay(Convert.ToByte(portNo));
                    //NCDLib.WriteBytesAPI(ref ncd1, true, 254, 116, 1);
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

        private void Main_Load(object sender, EventArgs e)
        {
            if (Global.DEMO)
            {
                m_IVISSRecordingPath = @"C:\IVISSTemp";
                pBoxStitch.Image = Image.FromFile(this.m_IVISSRecordingPath + @"\outPutVer.jpg");
                pBoxStitch.BackgroundImageLayout = ImageLayout.Zoom;

                //Image flipImage = pBoxStitch.Image;
                //flipImage.RotateFlip(RotateFlipType.Rotate90FlipY);
                //pBoxStitch.Image = flipImage;

                //pBoxComparison.Image = Image.FromFile(@"C:\IVISSTemp\outPutVer.jpg");

                this.m_ComparisonPath = @"C:\IVISSTemp";
                pBoxComparison.Image = Image.FromFile(this.m_ComparisonPath + "\\" + "photoshop2.jpg");
                pBoxComparison.BackgroundImageLayout = ImageLayout.Stretch;

                vspDriver.BackgroundImage = Image.FromFile(@"C:\IVISSTemp\driver.bmp");
                vspScene.BackgroundImage = Image.FromFile(@"C:\IVISSTemp\scene.bmp");
                vspLP.BackgroundImage = Image.FromFile(@"C:\IVISSTemp\lpnum.bmp");

                this.txtLiveLPEnglish.Text = "6RBZ328";
            }

            //lblLiveAccuracy.Text = "99%";

            lblRelay1.Text = Global.mlblRelay1;
            lblRelay1Arab.Text = Global.mlblRelayArab1;

            lblRelay2.Text = Global.mlblRelay2;
            lblRelay2Arab.Text = Global.mlblRelayArab2;

            lblRelay3.Text = Global.mlblRelay3;
            lblRelay3Arab.Text = Global.mlblRelayArab3;

            lblRelay4.Text = Global.mlblRelay4;
            lblRelay4Arab.Text = Global.mlblRelayArab4;

            // Load presenter form loaded
            FormLoaded(sender, e);
            
            if (!Global.DEMO)
                this.RunIPCameras();

            this.KeyUp += Main_KeyUp;
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                this.txtLiveLPEnglish.Clear();
                this.txtLiveLPArabic.Clear();

                b_ALPRProcessing = true;
            }
        }

        #region PROPERTIES
        public void BindData(DataTable dt)
        {
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    dgView.DataSource = dt;
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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

        public string accuracy { set; get; }

        public string origin { set; get; }

        public string plateColor { set; get; }

        public string plateSubColor { set; get; }

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

        public bool auto
        {
            set { this.m_Auto = value; }
            get { return this.m_Auto; }
        }

        #endregion

        #region BUTTON_EVENTS

        private void btnSearchRecords_Click(object sender, EventArgs e)
        {
            BtnSearchRecords(sender, e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Start Recording
        private void btnRecordingOff_Click(object sender, EventArgs e)
        {
            this.StartRecording();

            BtnRecordingOff(sender, e);
        }

        public bool StopRecording()
        {
            try
            {
                m_Recording = false;

                //this.BeginInvoke((MethodInvoker)delegate
                //{
                    writer.Close();

                    //MessageBox.Show("writer.Close();");
                    // On-screen display
                    //ShowStatus();

                    // red button
                    btnRecordingOn.Visible = false;
                    // green button
                    btnRecordingOff.Visible = true;
                //});
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

            return true;
        }

        // Stop Recording
        private void btnRecordingOn_Click(object sender, EventArgs e)
        {
            this.StopRecording();

            BtnRecordingOn(sender, e);
        }

        public bool StartRecording()
        {
            try
            {
                //t = TimeSpan.Zero;

                // Delete Stitch Image and Translation.Dat
                Task tDel = Task.Run(() =>
                {
                    if (File.Exists(Global.TEMP_FOLDER + "\\" + "outPutVer.jpg"))
                        File.Delete(Global.TEMP_FOLDER + "\\" + "outPutVer.jpg");

                    if (File.Exists(Global.TEMP_FOLDER + "\\" + "translation.dat"))
                        File.Delete(Global.TEMP_FOLDER + "\\" + "translation.dat");
                });

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

                //this.BeginInvoke((MethodInvoker)delegate
                //{
                    firstRecordedFrameTimeStamp = DateTime.MinValue;
                    nextWrittenFrameTimeStamp = DateTime.MinValue;

                    writer.Open(Global.TEMP_FOLDER + "\\driver.avi", Global.DRIVER_CAM_WIDTH, Global.DRIVER_CAM_HEIGHT, 30, VideoCodec.WMV2, 10000 * 1000); // 

                    //*****************************************************************************************************************************************************

                    Task tSnap = Task.Run(() => { TakeSnapshots(); });

                    //start recording driver camera
                    m_Recording = true;

                    //lpNumEnglish = this.txtLiveLPEnglish.Text;
                    //lpNumArabic = this.txtLiveLPArabic.Text;

                    // red button
                    btnRecordingOn.Visible = true;
                    // green button
                    btnRecordingOff.Visible = false;
                //});
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        private void TakeSnapshots(bool exit = false)
        {
            if (!exit)
            {
                try
                {
                    // Entry Driver
                    BeginInvoke((MethodInvoker)delegate
                    {
                        using (Bitmap bitmap = Global.PrintWindow(vspDriver.Handle))
                        {
                            bitmap.Save(Global.TEMP_FOLDER + "\\Driver.bmp", ImageFormat.Bmp);
                        }

                    });
                }
                catch (Exception ex)
                {
                }

                try
                {
                    //// Entry ALPR
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    //    using (Bitmap bitmap = global.PrintWindow(pBoxLiveLP.Handle))
                    //    {
                    //        bitmap.Save(global.TEMP_FOLDER + "\\LpNum.bmp", ImageFormat.Bmp);
                    //    }

                    //});
                }
                catch (Exception ex)
                {
                }
            }
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            BtnSettings(sender, e);
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

        #endregion

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

                        // Load image comparison
                        if (presenter.LPExists())
                            this.LoadImageComparison(presenter.SelectImageComparison());
                        else
                            pBoxStitch.Image = null;

                        //if (accuracy != null && accuracy.Length > 0)
                        //    lblLiveAccuracy.Text = accuracy + "%";
                        //else
                        //    lblLiveAccuracy.Text = "99%";

                        this.lblVisitorClassificationLive.Text = (authorization == "VISITOR") ? "    " + authorization : authorization;

                        this.SetAuthorizationBox(authorization);

                        //// Driver 
                        //if (File.Exists(path + @"\Driver.bmp"))
                        //{
                        //    using (FileStream fs = new FileStream(path + @"\Driver.bmp", FileMode.Open))
                        //    {
                        //        vspDriver.BackgroundImage = Image.FromStream(fs);
                        //        fs.Close();
                        //    }
                        //}
                        //else
                        //{
                        //    vspDriver.BackgroundImage = null;
                        //}

                        //// Scene Camera
                        //if (File.Exists(path + @"\Scene.bmp"))
                        //{
                        //    using (FileStream fs = new FileStream(path + @"\Scene.bmp", FileMode.Open))
                        //    {
                        //        vspScene.BackgroundImage = Image.FromStream(fs);
                        //        fs.Close();
                        //    }
                        //}
                        //else
                        //{
                        //    vspScene.BackgroundImage = null;
                        //}

                        //// License Plate Num
                        //if (File.Exists(path + @"\LpNum.bmp"))
                        //{
                        //    using (FileStream fs = new FileStream(path + @"\LpNum.bmp", FileMode.Open))
                        //    {
                        //        vspLP.BackgroundImage = Image.FromStream(fs);
                        //        fs.Close();
                        //    }
                        //}
                        //else
                        //{
                        //    vspLP.BackgroundImage = null;
                        //}

                        //============================================================================

                        if (File.Exists(path + @"\outPutVer.jpg"))
                        {
                            using (FileStream fs = new FileStream(path + @"\outPutVer.jpg", FileMode.Open))
                            {
                                imgCompositeImage = Image.FromStream(fs);
                                pBoxComparison.Image = imgCompositeImage;
                                fs.Close();
                            }
                        }
                        else
                        {
                            this.pBoxComparison.Image = null;
                        }

                        //============================================================================

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

                    //if (m_TotalFrameDriver > 0)
                    //{
                    //    CloseFilesDriver();
                    //    m_TotalFrameDriver = 0;
                    //    tbDriver.Value = 1;
                    //}

                    //m_TotalFrameDriver = OpenFilesDriver(path);

                    //if (m_TotalFrameDriver > 0)
                    //{
                    //    BeginInvoke((MethodInvoker)delegate
                    //    {
                    //        tbDriver.SetRange(1, m_TotalFrameDriver);
                    //    });
                    //}

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
                /*
                pBoxAuthorizationLive.BackgroundImage = (Bitmap)IVISS.Properties.Resources.red_card;
                pBoxLiveALPRBar.BackColor = Color.FromArgb(235, 107, 97);
                this.lblLiveAccuracy.BackColor = Color.FromArgb(235, 107, 97);
                this.lblLiveOCRAccuracy.BackColor = Color.FromArgb(235, 107, 97);
                */

                lblVisitorClassificationLive.BackColor = Color.FromArgb(241, 79, 73);

                //// Play Sound
                //if (play)
                //    this.PlaySound();
            }
            else if (authorization == "AUTHORIZED") //green
            {
                /*
                pBoxAuthorizationLive.BackgroundImage = (Bitmap)IVISS.Properties.Resources.green_card;
                pBoxLiveALPRBar.BackColor = Color.FromArgb(121, 246, 152);
                this.lblLiveAccuracy.BackColor = Color.FromArgb(121, 246, 152);
                this.lblLiveOCRAccuracy.BackColor = Color.FromArgb(121, 246, 152);
                */

                lblVisitorClassificationLive.BackColor = Color.FromArgb(0, 204, 79);

                //this.lblVisitorClassification.ForeColor = Color.White;
                //this.lblOCRAccuracy.ForeColor = Color.White;
                //this.lblPlayAccuracy.ForeColor = Color.White;
            }
            else
            {
                /*
                pBoxAuthorizationLive.BackgroundImage = (Bitmap)IVISS.Properties.Resources.yellow_card;
                pBoxLiveALPRBar.BackColor = Color.FromArgb(252, 255, 132);
                this.lblLiveAccuracy.BackColor = Color.FromArgb(252, 255, 132);
                this.lblLiveOCRAccuracy.BackColor = Color.FromArgb(252, 255, 132);
                */

                lblVisitorClassificationLive.BackColor = Color.FromArgb(238, 218, 23);
            }

            if (authorization.Length > 0)
                lblVisitorClassificationLive.Text = authorization;
            else
                lblVisitorClassificationLive.Text = "VISITOR";
        }
  
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {

            ncd1.ClosePort();

            CloseVideoSourceDriver();
            CloseVideoSourceScene();
            CloseVideoSourceLP();

            anprThread.StopThread();

            FormIsClosing(sender, e);
        }

        private void btnClickSnapshot_Click(object sender, EventArgs e)
        {
            //btnClickSnapshot.Visible = false;
            //tbDriver.Visible = true;
        }

        private void tbDriver_Scroll(object sender, EventArgs e)
        {
            if (m_TotalFrameDriver > 0)
            {
                SeekDriver((IntPtr)tbDriver.Value);
            }
        }

        private void vspDriver_Click(object sender, EventArgs e)
        {
            if (m_TotalFrameDriver > 0)
            {
                SaveDriver((IntPtr)tbDriver.Value);

                var path = (String.IsNullOrEmpty(m_IVISSRecordingPath)) ? Global.LAST_FOLDER : m_IVISSRecordingPath;

                System.Threading.Thread.Sleep(200);

                if (File.Exists(path + "\\frame.ppm"))
                {
                    File.Delete(path + "\\frame.ppm");
                }

                //MessageBox.Show(path);
            }

            using (Graphics g = vspDriver.CreateGraphics())
            {
                using (Font myFont = new Font("Arial", 10))
                {
                    g.DrawString("Driver snapshot taken", myFont, Brushes.White, new PointF(2f, vspDriver.Height - 20)); //new Point(2, 2)
                }
            }

            //tbDriver.Visible = false;
            //btnClickSnapshot.Visible = true;
        }

        private void dgView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //dgView.ReadOnly = true;
                m_IVISSRecordingPath = string.Empty;

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
                        var frm = new frmOverlay();
                        frm.plate = plate;
                        frm.plateArab = plateArab;
                        frm.recordingPath = path;
                        frm.Show();

                        m_IVISSRecordingPath = path;

                        //MessageBox.Show(path);
                        //m_IVISS_Plate_Number = plate;
                        //m_IVISS_Plate_DateTime = datetime;

                        if (Directory.Exists(path))
                        {

                            //******************************* reset FOD buttons *********************************
                            Task T1 = Task.Run(() =>
                            {
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    TurnOffConfidenceBtns();
                                    btnOriginal.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
                                });
                            });
                            //***********************************************************************************
                            SetFilesToOpen(path, plate, plateArab, classification, accuracy, gate, visitorAccess);
                        }
                        else
                        {
                            MessageBox.Show("Path not found!");
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { /* dgView.ReadOnly = false; */ }
        }

        private void btnClickSnapshot_Click_1(object sender, EventArgs e)
        {
            //btnClickSnapshot.Visible = false;
            //tbDriver.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            btnRecordingOff_Click(null, null);
        }

        private void picCompositeImage_Click(object sender, EventArgs e)
        {
            var frm = new frmViewImage();

            if (!m_FOB)
            {
                frm.imgPath = m_IVISSRecordingPath + @"\outPutVer.jpg";
            }
            else
            {
                frm.imgPath = m_SelectedImage;
            }

            frm.ShowDialog();
        }

        private void picOriginalImage_Click(object sender, EventArgs e)
        {
            var frm = new frmViewImage();
            frm.imgPath = m_ComparisonPath + @"\outPutVer.jpg";
            frm.ShowDialog();
        }

        private void picDriver_Click(object sender, EventArgs e)
        {
            string path = m_IVISSRecordingPath + "\\" + "Driver.bmp";
            var frm = new frmViewImageFullScreen();
            frm.imgPath = path;
            frm.ShowDialog();
        }

        private void TurnOffConfidenceBtns()
        {
            foreach (Button btn in pnlConfidence.Controls.OfType<Button>())
                if (btn.Tag != null && btn.Tag.ToString() == "confidence")
                    btn.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green;

            this.tBarBrightness.Value = 20;

            //foreach (Control c in pnlConfidence.Controls)
            //{
            //    if (c is Button)
            //    {
            //        if (c.Tag != null && c.Tag.ToString() == "confidence")
            //            c.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green;
            //    }
            //}
        }

        private void btnHigh_Click(object sender, EventArgs e)
        {
            Task T1 = Task.Run(() => 
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    TurnOffConfidenceBtns();
                    btnHigh.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
                });
            });

            m_FOB = true;

            //Task T2 = Task.Run(() =>
            //{
                m_SelectedImage = GetFilePath() + "\\_high.jpg";

                if (File.Exists(m_SelectedImage))
                {
                    using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                    {
                        //BeginInvoke((MethodInvoker)delegate
                        //{
                            pBoxComparison.Image = Image.FromStream(fs);
                        //});

                        fs.Close();
                    }
                }
                //else
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        pBoxComparison.BackgroundImage = null;
                //    });
                //}
            //});
        }

        private void btnMedium_Click(object sender, EventArgs e)
        {
            Task T1 = Task.Run(() =>
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    TurnOffConfidenceBtns();
                    btnMedium.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
                });
            });

            m_FOB = true;

            //Task T2 = Task.Run(() =>
            //{
                m_SelectedImage = GetFilePath() + "\\_medium.jpg";

                if (File.Exists(m_SelectedImage))
                {
                    using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                    {
                        //BeginInvoke((MethodInvoker)delegate
                        //{
                            pBoxComparison.Image = Image.FromStream(fs);
                        //});

                        fs.Close();
                    }
                }
                //else
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        pBoxComparison.BackgroundImage = null;
                //    });
                //}
            //});
        }

        private void btnLow_Click(object sender, EventArgs e)
        {
            Task T1 = Task.Run(() =>
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    TurnOffConfidenceBtns();
                    btnLow.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
                });
            });

            m_FOB = true;

            //Task T2 = Task.Run(() =>
            //{
                m_SelectedImage = GetFilePath() + "\\_low.jpg";

                if (File.Exists(m_SelectedImage))
                {
                    using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                    {
                        //BeginInvoke((MethodInvoker)delegate
                        //{
                            pBoxComparison.Image = Image.FromStream(fs);
                        //});

                        fs.Close();
                    }
                }
                //else
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        pBoxComparison.BackgroundImage = null;
                //    });
                //}
            //});
        }

        private void pBoxComparison_Click(object sender, EventArgs e)
        {

        }

        private void btnOriginal_Click(object sender, EventArgs e)
        {
            Task T1 = Task.Run(() =>
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    TurnOffConfidenceBtns();
                    btnOriginal.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
                });
            });

            m_FOB = false;

            //Task T2 = Task.Run(() =>
            //{
                m_SelectedImage = GetFilePath() + "\\outPutVer.jpg";

                if (File.Exists(m_SelectedImage))
                {
                    using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                    {
                        //BeginInvoke((MethodInvoker)delegate
                        //{
                            pBoxComparison.Image = Image.FromStream(fs);
                        //});

                        fs.Close();
                    }
                }
                //else
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        pBoxComparison.BackgroundImage = null;
                //    });
                //}
            //});
        }

        private void btnAuto_Click(object sender, EventArgs e)
        {
            if (!m_Auto)
            {
                btnAuto.BackgroundImage = (Bitmap)IVISS.Properties.Resources.auto;
            }
            else
            {
                btnAuto.BackgroundImage = (Bitmap)IVISS.Properties.Resources.auto_grey;
            }

            m_Auto = !m_Auto;

            presenter.SetRecordingMode();
            //ShowStatus();
        }

        private void tmCompositeImage_Tick(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(this.m_DestinationDir + @"\outPutVer.jpg"))
                {
                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                    //pBoxComparison.Image = Image.FromFile(this.m_DestinationDir + @"\outPutVer.jpg");
                    using (FileStream fs = new FileStream(this.m_DestinationDir + @"\outPutVer.jpg", FileMode.Open))
                    {
                        imgCompositeImage = Image.FromStream(fs);
                        pBoxComparison.Image = imgCompositeImage;
                        fs.Close();
                    }

                    tmCompositeImage.Stop();
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //LoadCompositeImage();
        }

        public static Bitmap AdjustBrightness(Bitmap Image, int Value)
        {
            if (Image != null)
            {
                Bitmap TempBitmap = Image;

                Bitmap NewBitmap = new Bitmap(TempBitmap.Width, TempBitmap.Height);

                Graphics NewGraphics = Graphics.FromImage(NewBitmap);

                float FinalValue = (float)Value / 255.0f;

                float[][] FloatColorMatrix ={

                    new float[] {1, 0, 0, 0, 0},

                    new float[] {0, 1, 0, 0, 0},

                    new float[] {0, 0, 1, 0, 0},

                    new float[] {0, 0, 0, 1, 0},

                    new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                };

                ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);

                ImageAttributes Attributes = new ImageAttributes();

                Attributes.SetColorMatrix(NewColorMatrix);

                NewGraphics.DrawImage(TempBitmap, new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, GraphicsUnit.Pixel, Attributes);

                Attributes.Dispose();

                NewGraphics.Dispose();

                return NewBitmap;
            }
            else
            {
                return null;
            }
        }

        private void tBarBrightness_Scroll(object sender, EventArgs e)
        {
            pBoxComparison.Image = AdjustBrightness((Bitmap)imgCompositeImage, tBarBrightness.Value);
        }

        private void dgView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void picLicensePlate_Click(object sender, EventArgs e)
        {
            string path = m_IVISSRecordingPath + "\\" + "LpNum.bmp";
            var frm = new frmViewImageFullScreen();
            frm.imgPath = path;
            frm.ShowDialog();
        }

        private void btnAFOD_Click(object sender, EventArgs e)
        {
            var defImage = presenter.SelectImageComparison().Replace(@"\",@"\\") + @"\\";
            var curImage = m_IVISSRecordingPath.Replace(@"\", @"\\") + @"\\";

            //MessageBox.Show("\"" + defImage + "\"" + " " + "\"" + "outPutVer.jpg" + "\"" + " " + "\"" + curImage + "\"" + " " + "\"" + "outPutVer.jpg" + "\"");

            Task tFOD = Task.Run(() =>
            {
                ProcessStartInfo _processStartInfo = new ProcessStartInfo();
                _processStartInfo.WorkingDirectory = Application.StartupPath + @"\FOD\";
                _processStartInfo.FileName = @"ForeignObjectDetection.exe";
                _processStartInfo.UseShellExecute = true;
                _processStartInfo.CreateNoWindow = true;
                _processStartInfo.Arguments = "\"" + defImage + "\"" + " " + "outPutVer.jpg" + " " + "\"" + curImage + "\"" + " " + "outPutVer.jpg"; //"C:\\IVISSTemp\\"; //"\"" + destination_dir + "\"";
                _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _processStartInfo.CreateNoWindow = true;

                Process myProcess = Process.Start(_processStartInfo);
            });
        }

        private void lblVisitorClassificationLive_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            btnRecordingOff_Click(null, null);
        }

        private string GetFilePath()
        {
            return (String.IsNullOrEmpty(m_IVISSRecordingPath)) ? "C:\\IVISSTemp" : m_IVISSRecordingPath;
        }

        public void RunFODAsync(string caseimage, string referenceimage,bool IsManual)
        {
            throw new NotImplementedException();
        }

        public void LoadImageComparisonWithFOD(string destDir)
        {
            throw new NotImplementedException();
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
