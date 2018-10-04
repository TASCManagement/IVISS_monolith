using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace IVISS.Utility
{
    class Global
    {
        public const bool DEVELOPMENT = true;
        public const bool LITE_COMMAND_CENTER = false;
        public const bool EXIT_GATE = false;
        public const bool KEY_LOK = true;
        public const bool SCENE_CAM = true;
        public const bool ALPR_CAM = true;
        public const bool LOG = false;

        public static string m_Gate_Name = string.Empty;
        public static int m_Gate_No = 0;
        public static string TEMP_FOLDER = @"C:\IVISSTemp";
        public static string LAST_FOLDER = @"C:\IVISSTemp";
        public static string VIDEO_DRIVE = @"C:\IVISS\";
        public static string VIDEO_FOLDER = VIDEO_DRIVE + "Gate";

        public const string COMPANY_NAME = "IVISS TASC Management";

        public const string PASSPHRASE = "!WOSKXN<<)";
        public const string LIC_DIRECTORY = "";

        public const int DRIVER_CAM_WIDTH = 1920; //640;
        public const int DRIVER_CAM_HEIGHT = 1080; //480;

        public static bool b_StartCapturing = true;

        public const int RENEWAL_LICENSE_DAYS = 365; // 6 months
        public const int RENEWAL_MESSAGE_DAYS = 30; // 1 month
        public const int RENEWAL_GRACE_DAYS = -60; // 6 months
        
        public static string mNCDPortNo;
        public static string mNCDAirWash;
        public static string mNCDBarrier;
        public static string mNCDLights;
        public static string mNCDCameras;

        public static string mNCDPort1;
        public static string mNCDPort2;
        public static string mNCDPort3;
        public static string mNCDPort4;

        public static string mDriverCamIP;
        public static string mDriverCamPassword;

        public static string mSceneCamIP;
        public static string mScenecamPassword;

        public static string mALPRCamIP;

        public static string mlblRelay1;
        public static string mlblRelayArab1;

        public static string mlblRelay2;
        public static string mlblRelayArab2;

        public static string mlblRelay3;
        public static string mlblRelayArab3;

        public static string mlblRelay4;
        public static string mlblRelayArab4;

        public static string USER_TYPE = "";

        public static string ErrMessage;

        public static void ShowMessageDialog(string msg)
        {
            var frm = new frmMessageBox();
            frm.StatusMessage = msg;
            frm.ShowDialog();

            frm.Dispose();
        }

        public static void ShowMessage(string msg, bool showCancel = true)
        {
            var frm = new frmMessageBox(showCancel);
            frm.StatusMessage = msg;
            frm.ShowDialog();
        }

        public static void WriteLog(string msg)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log(msg, w);
            }
        }

        private static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        public static void AppendString(string err)
        {
            ErrMessage = err + Environment.NewLine;
        }

        public static Bitmap GetAccuracyBitmap(string alprAcc)
        {
            int accuracy = int.Parse(alprAcc);

            if (accuracy <= 5)
                return (Bitmap)IVISS.Properties.Resources._5;

            else if (accuracy <= 10)
                return (Bitmap)IVISS.Properties.Resources._10;

            else if (accuracy <= 15)
                return (Bitmap)IVISS.Properties.Resources._15;

            else if (accuracy <= 20)
                return (Bitmap)IVISS.Properties.Resources._20;

            else if (accuracy <= 25)
                return (Bitmap)IVISS.Properties.Resources._25;

            else if (accuracy <= 30)
                return (Bitmap)IVISS.Properties.Resources._30;

            else if (accuracy <= 35)
                return (Bitmap)IVISS.Properties.Resources._35;

            else if (accuracy <= 40)
                return (Bitmap)IVISS.Properties.Resources._40;

            else if (accuracy <= 45)
                return (Bitmap)IVISS.Properties.Resources._45;

            else if (accuracy <= 50)
                return (Bitmap)IVISS.Properties.Resources._50;

            else if (accuracy <= 55)
                return (Bitmap)IVISS.Properties.Resources._55;

            else if (accuracy <= 60)
                return (Bitmap)IVISS.Properties.Resources._60;

            else if (accuracy <= 65)
                return (Bitmap)IVISS.Properties.Resources._65;

            else if (accuracy <= 70)
                return (Bitmap)IVISS.Properties.Resources._70;

            else if (accuracy <= 75)
                return (Bitmap)IVISS.Properties.Resources._75;

            else if (accuracy <= 80)
                return (Bitmap)IVISS.Properties.Resources._80;

            else if (accuracy <= 85)
                return (Bitmap)IVISS.Properties.Resources._85;

            else if (accuracy <= 90)
                return (Bitmap)IVISS.Properties.Resources._90;

            else if (accuracy <= 95)
                return (Bitmap)IVISS.Properties.Resources._95;

            else
                return (Bitmap)IVISS.Properties.Resources._95;
        }

        enum Modes
        {
            Live_Mode,
            Play_Mode,
            Access_Controls,
            Reports,
            ALPR,
            Settings
        }

    }
}
