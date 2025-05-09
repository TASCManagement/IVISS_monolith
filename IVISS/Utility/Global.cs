using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IVISS.Utility
{
    class Global
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        public const bool DEVELOPMENT = true;
        public const bool LITE_COMMAND_CENTER = false;
        public const bool EXIT_GATE = false;
        public const bool KEY_LOK = true;
        public const bool SCENE_CAM = true;
        public const bool ALPR_CAM = true;
        public const bool LOG = false;
        public const bool DEMO = false;


        public static string CurrentTheme = "Default";
        public static string m_Gate_Name = string.Empty;
        public static string  m_Gate_No = "0";
        public static string TEMP_FOLDER = @"C:\IVISSTemp";
        public static string LAST_FOLDER = @"C:\IVISSTemp";
        public static string VIDEO_DRIVE = @"C:\IVISS\";
        public static string VIDEO_FOLDER = VIDEO_DRIVE + "Gate";

        public static string FILE_NAME = "m1-0000.avi";

        public const string COMPANY_NAME = "IVISS TASC Management";

        public const string PASSPHRASE = "!WOSKXN<<)";
        public const string LIC_DIRECTORY = "";

        public const int DRIVER_CAM_WIDTH = 1920; //640;
        public const int DRIVER_CAM_HEIGHT = 1080; //480;

        public const int RENEWAL_LICENSE_DAYS = 365; // 6 months
        public const int RENEWAL_MESSAGE_DAYS = 30; // 1 month
        public const int RENEWAL_GRACE_DAYS = -60; // 6 months

        public const int ALPR_CAMERAS = 1;
        public const int ENTRY_ALPR = 0;
        public static int LOOP_INTERVAL = 100;
        public static bool ENTRY_LOOP_SENSOR = true;
        public static bool AIEnabled = true;
        public static bool[] b_StartCapturing = new bool[ALPR_CAMERAS];
        public static string[] ALPR_CAMERA_HOST = new string[ALPR_CAMERAS];

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

        public static string mIPAddress;
        public static string mListenPort;

        public static string USER_TYPE = "";

        public static string USER_NAME = "";

        public static string ErrMessage;

        public static string ProfileMode;

        public static string LicenseNo;

        public static bool IsAdditionalALPRAllowed = false;
        public static bool IsSceneAllowed = false;

        public static bool IsDriverAllowed = false;


        public static bool IsRelayAllowed = false;

        public static List<AdditionalALPR> lstAdditionalALPR = new List<AdditionalALPR>();

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

        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            try
            {
                RECT rc;
                GetWindowRect(hwnd, out rc);

                Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format48bppRgb);
                Graphics gfxBmp = Graphics.FromImage(bmp);
                IntPtr hdcBitmap = gfxBmp.GetHdc();

                PrintWindow(hwnd, hdcBitmap, 0);

                gfxBmp.ReleaseHdc(hdcBitmap);
                gfxBmp.Dispose();

                return bmp;
            }
            catch (Exception)
            {

                return null;
            }
          
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            public RECT(RECT Rectangle)
                : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
            {
            }
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            public int X
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Y
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Left
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Top
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Right
            {
                get { return _Right; }
                set { _Right = value; }
            }
            public int Bottom
            {
                get { return _Bottom; }
                set { _Bottom = value; }
            }
            public int Height
            {
                get { return _Bottom - _Top; }
                set { _Bottom = value + _Top; }
            }
            public int Width
            {
                get { return _Right - _Left; }
                set { _Right = value + _Left; }
            }
            public Point Location
            {
                get { return new Point(Left, Top); }
                set
                {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            public Size Size
            {
                get { return new Size(Width, Height); }
                set
                {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }

            public static implicit operator Rectangle(RECT Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }
            public static implicit operator RECT(Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new RECT((Rectangle)Object));
                }

                return false;
            }
        }

    }
}
