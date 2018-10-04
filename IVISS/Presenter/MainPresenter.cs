using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVISS.View;
using IVISS.Model;
using System.Windows.Forms;
using IVISS.Utility;
using System.IO;
using System.Threading.Tasks;
using IVISS.Classes;
using System.Diagnostics;

namespace IVISS.Presenter
{
    class MainPresenter
    {
        IMain view;
        MainModel model;
        Camera_Recorder recorder;

        public MainPresenter(IMain mainView)
        {
            view = mainView;
            model = new MainModel();

            view.BtnLights += view_BtnLights;
            view.BtnCamera += view_BtnCamera;
            view.BtnSearchRecords += view_BtnSearchRecords;
            view.BtnRecordingOn += view_BtnRecordingOn;
            view.BtnRecordingOff += view_BtnRecordingOff;
            view.BtnSettings += view_BtnSettings;
           

            Task T = Task.Factory.StartNew(() => {
                view.BindData(model.ReturnGridSource());
            });

            // recording related code
            // //TODO:
            //recorder = new Camera_Recorder();
            //this.ConnectCamera();
        }

        void view_BtnSettings(object sender, EventArgs e)
        {
            var frm = new frmSettings();
            frm.Show();
        }

        void view_BtnSearchRecords(object sender, EventArgs e)
        {
            //view.BindData(model.ReturnGridSource());
            var frm = new frmSearchRecords();
            frm.Show();
        }

        void view_BtnLights(object sender, EventArgs e)
        {
            MessageBox.Show("view_BtnLights");
        }

        void view_BtnCamera(object sender, EventArgs e)
        {
            MessageBox.Show("view_BtnCamera");
        }

        void view_BtnRecordingOn(object sender, EventArgs e)
        {
            //MessageBox.Show("Recording Stop");
            StopRecord();

            // run stitch.exe
            Task tStitch = Task.Run(() =>
            {
                ProcessStartInfo _processStartInfo = new ProcessStartInfo();
                _processStartInfo.WorkingDirectory = Application.StartupPath + @"\stitch\";
                _processStartInfo.FileName = @"stitch.exe";
                _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _processStartInfo.CreateNoWindow = true;

                Process myProcess = Process.Start(_processStartInfo);
            });

            // run stitch code from within the app 
            /*
            Stitch st = new Stitch();
            st.recordingPath = view.recordingPath;
            view.stitchImage = st.Create();
            */
        }

        void view_BtnRecordingOff(object sender, EventArgs e)
        {
            //MessageBox.Show("Recording Start");
            StartRecord();
            
            
        }

        #region RECORDING_CODE
        private bool ConnectCamera()
        {
            // Finds the list of cameras and gets a reference to the camera
            if (recorder.ConnectCamera() < 0)
            {
                //Console.WriteLine("Error finding a camera");
                MessageBox.Show("Error finding a camera");
                return false;
            }

            // Connects to the referenced camera and initialize camera settings
            if (recorder.StartCamera() < 0)
            {
                //Console.WriteLine("Error starting or Initializing camera");
                MessageBox.Show("Error starting or Initializing camera");
                return false;
            }

            return true;
        }

        private bool DisconnectCamera()
        {
            // Disconnects the camera
            if (recorder.DisconnectCamera() < 0)
            {
                //Console.WriteLine("Error disconnecting a camera");
                MessageBox.Show("Error disconnecting a camera");
                return false;
            }

            return true;
        }

        private bool StartRecord()
        {
            // Starts receiving frames untill stops function is called
            // Starts conversion of images-to-video right away
            if (recorder.StartReceivingFrames_2() < 0)
            {
                //Console.WriteLine("Error receiving frames");
                MessageBox.Show("Error receiving frames");
                return false;
            }
            return true;
        }

        private bool StopRecord()
        {
            // This line blocks until the conversion of video
            //? Or perhaps it should start another recording, should it?
            if (recorder.StopReceivingFrames() < 0)
            {
                //Console.WriteLine("Error while stopping the frame acquisition");
                //Console.WriteLine("Disconnecting the camera");
                MessageBox.Show("Error while stopping the frame acquisition" + Environment.NewLine + "Disconnecting the camera");
                return false;
            }
            return true;
        }

        #endregion

        public void Save()
        {
            model.lpNumArabic = view.lpNumArabic;
            model.lpNumEnglish = view.lpNumEnglish;
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



                var frm = new frmLicRenewal();
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

                Global.ShowMessage("Invalid License Key", false);
            }
        }

        public bool ProcessKeyLok()
        {
            UInt32 FortressDongleCount = 0;
            FortressDongleCount = keylok.GetFortressDongles();

            if (FortressDongleCount > 1)  //More than one Fortress dongle attached
            {
                //MessageBox.Show("Multiple Fortress Dongles Attached. Beginning communication with first dongle.");
                keylok.SelectTargetDongle(1);  //dongles enumerated 1-n
            }

            if (!keylok.IsPresent())
            {
                MessageBox.Show("Failed to detect IVISS Security Dongle", "IVISS");
                Environment.Exit(0);

                var msg = "Failed To Detect IVISS Security Dongle" +
                    Environment.NewLine +
                    "Please Contact TASC Management's Customer Support" +
                    Environment.NewLine +
                    "Phone: 703-310-7760" +
                    Environment.NewLine +
                    "Email: iviss@tascmanagement.com";

                Global.ShowMessageDialog(msg);
                
                if (!Global.DEVELOPMENT)
                {
                    return false;
                }
            }

            return true;
        }

        public void FillSettings()
        {
            var query = model.FillSettings();
            if (query != null)
            {
                //***************************** Live Mode Relay Ports ****************************

                if (int.TryParse(query.AirWash, out int airWashPort))
                    Global.mNCDAirWash = (airWashPort - 1).ToString();

                if (int.TryParse(query.Barrier, out int barrierPort))
                    Global.mNCDBarrier = (barrierPort - 1).ToString();

                if (int.TryParse(query.Cameras, out int cameraPort))
                    Global.mNCDCameras = (cameraPort - 1).ToString();

                if (int.TryParse(query.Lights, out int lightsPort))
                    Global.mNCDLights = (lightsPort - 1).ToString();

                //********************************************************************************

                //*************************** Ports **********************************************//

                if (int.TryParse(query.Relay1Port, out int port1))
                    Global.mNCDPort1 = (port1 - 1).ToString();

                if (int.TryParse(query.Relay2Port, out int port2))
                    Global.mNCDPort2 = (port2 - 1).ToString();

                if (int.TryParse(query.Relay3Port, out int port3))
                    Global.mNCDPort3 = (port3 - 1).ToString();

                if (int.TryParse(query.Relay4Port, out int port4))
                    Global.mNCDPort4 = (port4 - 1).ToString();

                
                Global.mlblRelay1 = query.Relay1;
                Global.mlblRelayArab1 = query.Relay1Arab;

                Global.mlblRelay2 = query.Relay2;
                Global.mlblRelayArab2 = query.Relay2Arab;

                Global.mlblRelay3 = query.Relay3;
                Global.mlblRelayArab3 = query.Relay3Arab;

                Global.mlblRelay4 = query.Relay4;
                Global.mlblRelayArab4 = query.Relay4Arab;

                Global.mALPRCamIP = query.LicenseCamIP;
                Global.mDriverCamIP = query.DriverCamIP;
                Global.mSceneCamIP = query.SceneCamIP;

                Global.mDriverCamPassword = query.DriverCamPassword;
                Global.mSceneCamIP = query.SceneCamPassword;

                /*
                global.DRIVER_REC_TIMEOUT = query.DriverRecTimeout ?? global.DRIVER_REC_TIMEOUT;

                // have to multiply by 5 to get desired results
                // global.DRIVER_REC_TIMEOUT = (global.DRIVER_REC_TIMEOUT * 5);
                // MessageBox.Show(query.DriverRecTimeout.ToString() + "-----" + global.DRIVER_REC_TIMEOUT.ToString());

                global.ENTRY_LOOP_SENSOR = this.ALPREntryLoopToggle.Checked = query.ALPREntryLoop ?? false;
                global.EXIT_LOOP_SENSOR = this.ALPRExitLoopToggle.Checked = query.ALPRExitLoop ?? false;

                if (query.RetentionDays == 15)
                {
                    this.btn15Days.BackgroundImage = (Bitmap)IVISS.Properties.Resources.radio_on;
                    this.m_15Days = true;
                }
                else if (query.RetentionDays == 30)
                {
                    this.btn30Days.BackgroundImage = (Bitmap)IVISS.Properties.Resources.radio_on;
                    this.m_30Days = true;
                }
                else if (query.RetentionDays == 60)
                {
                    this.btn60Days.BackgroundImage = (Bitmap)IVISS.Properties.Resources.radio_on;
                    this.m_60Days = true;
                }
                else if (query.RetentionDays == 90)
                {
                    this.btn90Days.BackgroundImage = (Bitmap)IVISS.Properties.Resources.radio_on;
                    this.m_90Days = true;
                }
                else if (query.RetentionDays == 120)
                {
                    this.btn120Days.BackgroundImage = (Bitmap)IVISS.Properties.Resources.radio_on;
                    this.m_120Days = true;
                }
                */
            }
        }
    }
}
