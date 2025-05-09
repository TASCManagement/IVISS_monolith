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
//using ConfigurationUtility;
using System.Net;
using MaterialSkin.Controls;
using MaterialSkin;
using DotImaging;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Python.Runtime;
using Newtonsoft.Json.Linq;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Media;
using ClosedXML.Excel;
using fod;
using System.Net.Sockets;
using System.Text.RegularExpressions;

//using Microsoft.SqlServer.Management.Smo;


namespace IVISS
{
    public partial class MainV1 : MaterialForm, IMain, ISystemSettings, IUserManagement, IReports, ISearchRecords
    {
        #region Declaration

        Int32 port = 13000;
        IPAddress localAddr = System.Net.IPAddress.Parse("127.0.0.1");

        TcpListener server = null;

        // TcpListener server = new TcpListener(port);


        bool serverstarted = false;

        String cameraSerial = "17165437";
        NetworkStream stream_commands = null;
        NetworkStream stream_responses = null;

        Byte[] bytes = new Byte[256];       // Receiving data
        String data = null;
        byte[] msg;                         // Sending data

        bool rec_started = false;
        bool rec_ended = false;


        DataTable dtTheme = new DataTable("ThemeTable");
        DataColumn dcTheme = new DataColumn("Theme");
        DataColumn dcLanguage = new DataColumn("Language");


        DataTable dtAutoFOD = new DataTable("AutoFOD");
        DataColumn dcAutoEnabled = new DataColumn("AutoFODEnabled");




        MaterialSkinManager _skinmanager;
        private int _lastFormSize;

        private float _fontsize { get; set; }

        private System.Drawing.SizeF _formSize { get; set; }


        MainPresenter presenter;
        SystemSettingsPresenter settingspresenter;
        ReportsPresenter reportspresenter;

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

        bool isPlayable = false;

        Image OriginalImage;


        private Stopwatch stopwatch = new Stopwatch();
        // private int frameCounter = 0;
        // private const int MaxFramesPerSecond = 5; // Set the max frames per second
        private object lockObject = new object(); // Ensure thread safety
      

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
        ANPR_Thread anprThreadALPR;

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
        private Image imgCompositeImageBrightness;

        //============================================================================================

        private string Client_ID = string.Empty;
        VideoFileWriter writer = new VideoFileWriter();
        DateTime firstRecordedFrameTimeStamp, nextWrittenFrameTimeStamp;

        System.Threading.Timer timerALPR;

        private long? StartTick;

        //----------------------------Zoom on Mouse hover-----------------------
        private Bitmap OriginalImage_zoom, ShadedImage;
        private int SmallWidth, SmallHeight;
        private double ScaleX, ScaleY;
        // Display a closeup of this area.
        Rectangle ViewingRectangle;
        private SoundPlayer Player = new SoundPlayer();

        public static bool isAutoStitchFinished = true;

        bool firsttimeviewdetails = true;

        bool cameraResetStarted = false;


        frmVisitorSearch frm = null;


        int frameCounter = 0;
        int skipAlprFrames = 5;

        DataTable dtAdditionalALPR;

        #endregion


        List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>> image_array = null;

        #region PROPERTIES
        public void BindData(DataTable dt)
        {
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    dgView.DataSource = dt;

                    Application.DoEvents();
                    GC.Collect();
                    Application.DoEvents();
                });
            }
            catch (Exception ex)
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

        // No point of passing a bool if all you do is return it...
        private bool CheckDatabase(string databaseName)
        {
            try
            {

                try
                {
                    string root = Global.TEMP_FOLDER;
                    string ivissroot = @"C:\IVISS";
                    string subdir = @"C:\IVISS\Gate";
                    // If directory does not exist, create it. 
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }

                    if (!Directory.Exists(ivissroot))
                    {
                        Directory.CreateDirectory(ivissroot);
                    }

                    if (!Directory.Exists(subdir))
                    {
                        Directory.CreateDirectory(subdir);
                    }
                }
                catch (Exception)
                {


                }

                // You know it's a string, use var
                var connString = "Server=.\\SQLEXPRESS;Integrated Security=SSPI;database=master";
                // Note: It's better to take the connection string from the config file.

                var cmdText = "select count(*) from master.dbo.sysdatabases where name=@database";

                using (var sqlConnection = new SqlConnection(connString))
                {
                    using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                    {
                        // Use parameters to protect against Sql Injection
                        sqlCmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;

                        // Open the connection as late as possible
                        sqlConnection.Open();
                        // count(*) will always return an int, so it's safe to use Convert.ToInt32

                        return Convert.ToInt32(sqlCmd.ExecuteScalar()) == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.ShowMessage(ex.Message, false);
                return false;
            }


        }


        private void CreateAdditionalALPRTable()
        {
            try
            {

                string createAdditionalALPRTable = "CREATE TABLE [dbo].[AdditionalALPR]([AdditionalALPRId] [int] IDENTITY(1,1) NOT NULL,[ALPRIP] [varchar](50) NULL,[LoopEnabled] [bit] NULL,[GateName] [varchar](200) NULL,[AIEnabled] [bit] NULL,CONSTRAINT [PK_AdditionalALPR] PRIMARY KEY CLUSTERED (	[AdditionalALPRId] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]";

                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";



                using (var sqlConnection = new SqlConnection(connString))
                {
                    sqlConnection.Open();
                    using (var sqlCmd1 = new SqlCommand(createAdditionalALPRTable, sqlConnection))
                    {
                        sqlCmd1.ExecuteNonQuery();
                    }

                    sqlConnection.Close();
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);

            }
        }


        private bool DeleteVisitorColumnsExist()
        {
            SqlConnection sqlConnection = null;
            try
            {
                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                var cmdText = "select isDeleted from Visitor";

                using (sqlConnection = new SqlConnection(connString))
                {
                    using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                    {


                        // Open the connection as late as possible
                        sqlConnection.Open();
                        // count(*) will always return an int, so it's safe to use Convert.ToInt32
                        var returnValue = sqlCmd.ExecuteScalar();


                        if (returnValue != null)
                            return true;
                        else
                        {
                            using (var sqlCmd1 = new SqlCommand("Alter table Visitor Add isDeleted tinyint DEFAULT 0;", sqlConnection))
                            {
                                sqlCmd1.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";
                    using (sqlConnection = new SqlConnection(connString))
                    {

                        try
                        {
                            using (var sqlCmd1 = new SqlCommand("Alter table Visitor Add isDeleted tinyint DEFAULT 0;", sqlConnection))
                            {
                                sqlConnection.Open();


                                sqlCmd1.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex1)
                        {
                            MessageBox.Show(ex1.Message);

                        }


                    }
                }

                return false;

            }

            return false;

        }

        private bool ExtraColumnsExist()
        {

            SqlConnection sqlConnection = null;
            try
            {
                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                var cmdText = "select AIEnabled from SystemSettings";

                using (sqlConnection = new SqlConnection(connString))
                {
                    using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                    {


                        // Open the connection as late as possible
                        sqlConnection.Open();
                        // count(*) will always return an int, so it's safe to use Convert.ToInt32
                        var returnValue = sqlCmd.ExecuteScalar();


                        if (returnValue != null)
                            return true;
                        else
                        {
                            using (var sqlCmd1 = new SqlCommand("Alter table SystemSettings Add AIEnabled bit null;", sqlConnection))
                            {
                                sqlCmd1.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return true;
                MessageBox.Show(ex.Message);
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";
                    using (sqlConnection = new SqlConnection(connString))
                    {

                        using (var sqlCmd1 = new SqlCommand("Alter table SystemSettings Add AIEnabled bit null;", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCmd1.ExecuteNonQuery();
                        }
                    }
                }

                return false;

            }

            return false;

        }

        private bool LaserTriggerEExist()
        {
           
            SqlConnection sqlConnection = null;
            try
            {
                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                var cmdText = "select LaserTriggerEnabled from SystemSettings";

                using (sqlConnection = new SqlConnection(connString))
                {
                    using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                    {


                        // Open the connection as late as possible
                        sqlConnection.Open();
                        // count(*) will always return an int, so it's safe to use Convert.ToInt32
                        var returnValue = sqlCmd.ExecuteScalar();


                        if (returnValue != null)
                            return true;
                        else
                        {
                            using (var sqlCmd1 = new SqlCommand("Alter table SystemSettings Add LaserTriggerEnabled bit null;", sqlConnection))
                            {
                                sqlCmd1.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return true;
                MessageBox.Show(ex.Message);
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";
                    using (sqlConnection = new SqlConnection(connString))
                    {

                        using (var sqlCmd1 = new SqlCommand("Alter table SystemSettings Add LaserTriggerEnabled bit null;", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCmd1.ExecuteNonQuery();
                        }
                    }
                }

                return false;

            }

            return false;

        }


        private bool AdditionalLaserTriggerEExist()
        {

            SqlConnection sqlConnection = null;
            try
            {
                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                var cmdText = "select LaserTriggerEnabled from AdditionalALPR";

                using (sqlConnection = new SqlConnection(connString))
                {
                    using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                    {


                        // Open the connection as late as possible
                        sqlConnection.Open();
                        // count(*) will always return an int, so it's safe to use Convert.ToInt32
                        var returnValue = sqlCmd.ExecuteScalar();


                        if (returnValue != null)
                            return true;
                        else
                        {
                            using (var sqlCmd1 = new SqlCommand("Alter table AdditionalALPR Add LaserTriggerEnabled bit null;", sqlConnection))
                            {
                                sqlCmd1.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";
                    using (sqlConnection = new SqlConnection(connString))
                    {

                        using (var sqlCmd1 = new SqlCommand("Alter table AdditionalALPR Add LaserTriggerEnabled bit null;", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCmd1.ExecuteNonQuery();
                        }
                    }
                }

                return false;

            }

            return false;

        }


        private bool IsPrimaryColumnInDetailExist()
        {
            SqlConnection sqlConnection = null;
            try
            {
                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                var cmdText = "select is_primary from Detail";

                using (sqlConnection = new SqlConnection(connString))
                {
                    using (var sqlCmd = new SqlCommand(cmdText, sqlConnection))
                    {


                        // Open the connection as late as possible
                        sqlConnection.Open();
                        // count(*) will always return an int, so it's safe to use Convert.ToInt32
                        var returnValue = sqlCmd.ExecuteScalar();


                        if (returnValue != null)
                            return true;
                        else
                        {
                            using (var sqlCmd1 = new SqlCommand("Alter table Detail Add is_primary tinyint null;", sqlConnection))
                            {
                                sqlCmd1.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return true;
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";
                    using (sqlConnection = new SqlConnection(connString))
                    {

                        using (var sqlCmd1 = new SqlCommand("Alter table Detail Add is_primary tinyint null;", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCmd1.ExecuteNonQuery();
                        }
                    }
                }

                return false;

            }

            return false;

        }

        private bool ChangeGateDataType()
        {
            SqlConnection sqlConnection = null;
            try
            {
                var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";



                using (sqlConnection = new SqlConnection(connString))
                {



                    // Open the connection as late as possible
                    sqlConnection.Open();



                    using (var sqlCmd1 = new SqlCommand("ALTER TABLE SystemSettings ALTER COLUMN gate_no varchar(20); ALTER TABLE Detail ALTER COLUMN gate_no varchar(20);", sqlConnection))
                    {
                        sqlCmd1.ExecuteNonQuery();
                    }


                }
            }
            catch (Exception)
            {
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    var connString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";
                    using (sqlConnection = new SqlConnection(connString))
                    {

                        using (var sqlCmd1 = new SqlCommand("Alter table SystemSettings Add AIEnabled bit null;", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCmd1.ExecuteNonQuery();
                        }
                    }
                }

                return false;

            }

            return false;

        }
        public MainV1()
        {
            try
            {

           
            if (CheckDatabase("IVISS_Client"))
            {

               
            }
            else
            {
                Process.Start(Application.StartupPath + "\\" + "ConfigureSettingsAfterSetup.exe");
               
            }


                //if (ExtraColumnsExist())
                //{

                //}

                //if (ChangeGateDataType())
                //{

                //}

                //if (DeleteVisitorColumnsExist())
                //{

                //}

                //if (LaserTriggerEExist())
                //{

                //}
                //if (IsPrimaryColumnInDetailExist())
                //{

                //}
                //CreateAdditionalALPRTable();


                //if (AdditionalLaserTriggerEExist())
                //{

                //}

                stopwatch.Start();


                dtAutoFOD.Columns.Add(dcAutoEnabled);



            dtTheme.Columns.Add(dcTheme);
            dtTheme.Columns.Add(dcLanguage);

            _skinmanager = MaterialSkinManager.Instance;
            _skinmanager.AddFormToManage(this);
            _skinmanager.Theme = MaterialSkinManager.Themes.LIGHT;

             

            if (File.Exists(Application.StartupPath + "\\Theme.xml"))
            {
                UpdateTheme(Application.StartupPath + "\\Theme.xml");
            }
            else
                _skinmanager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

           
            using (var db = new IVISSEntities())
            {

                    try
                    {
                        var query = (from settings in db.SystemSettings
                                     select settings).FirstOrDefault();

                        if (query == null)
                        {
                            Global.LicenseNo = "";
                        }
                        else
                        {
                            Global.LicenseNo = query.LicenseNo;

                        }
                    }
                    catch (Exception ex)
                    {
                       // MessageBox.Show("exx");

                        MessageBox.Show(ex.InnerException.ToString());
                    }
               
            }

              

            //  _skinmanager.AddFormToManage(this);
            presenter = new MainPresenter(this);


               

                ProcessKeyLok();

               

                // Show license renewal dialog
                this.ShowLicenseRenewal();

            // On Cancel Close App
            frmLoginV1 frm = new frmLoginV1();
            if (frm.ShowDialog() == DialogResult.Cancel)
            {
                Environment.Exit(0);
            }




            //******************************* LIVE ENTRY ACCURACY BOX ***********************************
            SetLiveAccuracyBox();
            //*******************************************************************************************

            //*********************************** FILL SETTINGS *****************************************
            Task t2 = Task.Run(() =>
            {
                // Fill database settings
                this.FillSettings();

                // Connect NCD
                // this.NCDConnect();
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

            Task t4 = Task.Run(() =>
            {
                // Fill database settings
                this.FillAdditionalALPR();

                // Connect NCD
                // this.NCDConnect();
            });
            {
                anprThread = new ANPR_Thread();
                anprThread.anpr_result += AnprThread_anpr_result;


                anprThreadALPR = new ANPR_Thread();
                anprThreadALPR.anpr_result += anprThreadALPR_anpr_result;


                //timerALPR = new System.Threading.Timer(new TimerCallback(LoopLiveALPR), null, 0, Global.LOOP_INTERVAL); // Loop Interval is in millisecond
            }
            //this.lblGate.Text = Global.m_Gate_Name;
            InitializeComponent();
            this.Player.LoadCompleted += new AsyncCompletedEventHandler(Player_LoadCompleted);
            this.Resize += new EventHandler(Form_Resize);
            _lastFormSize = GetFormArea(this.Size);

            _dgv_Column_Adjust(dgView, false);

            if (Global.IsRelayAllowed == true)
            {
                //********************************** CONNECT TO RELAY ***************************************
                Task tNCD = Task.Run(() =>
                {
                    this.NCDConnect();
                });
                //*******************************************************************************************
            }

              

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            //  this.AutoScaleMode = AutoScaleMode.Dpi;

        }



        #region ExistingMethods

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
                                    var frmLR = new frmLicRenewalV1();
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
                var dt = StringCipher.Encrypt(DateTime.Now.AddDays(Global.RENEWAL_LICENSE_DAYS).ToString(), Global.PASSPHRASE);
                if (directory.Length > 0)
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(directory + "sys.dll", dt);

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
            try
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
            catch (Exception)
            {


            }


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

            SetAuthorizationBox(authorization, true);
            isPlayable = true;
        }

        private void RunIPCameras()
        {
            if (Global.IsDriverAllowed)
            {
                //********************************************************* Driver Entry *************************************************************
                //************************************************************************************************************************************
                // create video source
                //MJPEGStream mjpegSourceDriver = new MJPEGStream(@"http://" + Global.mDriverCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view&Resolution=1920x1080");
                MJPEGStream mjpegSourceDriver = new MJPEGStream(@"http://" + Global.mDriverCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view");

              

                // http://192.168.1.145/stw-cgi/image.cgi?msubmenu=camera&action=set&DayNightMode=BW

                //IR change functionality
                //http://192.168.1.145/stw-cgi/image.cgi?msubmenu=camera&action=set&IR=Auto1

                if (Global.mDriverCamPassword != null && Global.mDriverCamPassword.Length > 0)
                {
                    mjpegSourceDriver.Login = "admin";
                    mjpegSourceDriver.Password = Global.mDriverCamPassword;
                }
                // open it
                OpenVideoSourceDriver(mjpegSourceDriver);
            }
            //************************************************************************************************************************************

            //********************************************************* Scene Cam Entry *************************************************************
            //************************************************************************************************************************************
            if (Global.SCENE_CAM)
            {
                if (Global.IsSceneAllowed == true)
                {
                    // create video source
                    MJPEGStream mjpegSourceScene = new MJPEGStream(@"http://" + Global.mSceneCamIP + "/stw-cgi/video.cgi?msubmenu=stream&action=view&Resolution=1920x1080");

                    if (Global.mScenecamPassword != null && Global.mScenecamPassword.Length > 0)
                    {
                        mjpegSourceScene.Login = "admin";
                        mjpegSourceScene.Password = Global.mScenecamPassword; //Tasc12345
                    }
                    // open it
                    OpenVideoSourceScene(mjpegSourceScene);
                }
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

            BeginInvoke((MethodInvoker)delegate
            {

                tmCompositeImage.Start();

                pBoxComparison.SizeMode = PictureBoxSizeMode.CenterImage;
                pBoxComparison.Image = Image.FromFile("loader.gif");
            });
        }

        public static string refImage = "";

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


        public void LoadImageComparisonWithFOD()
        {
            try
            {
                if (refImage != "")
                {
                    if (File.Exists(refImage))
                    {
                        imgLoadFOD.Visible = true;
                        Application.DoEvents();
                        imgLoadFOD.Image = Image.FromFile("loadingnew.gif");

                        Application.DoEvents();
                        RunFODAsync(refImage, m_DestinationDir + "\\" + "outPutVer.jpg", false);
                        refImage = "";
                    }
                }
            }
            catch (Exception ex)
            {
                refImage = "";
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

                if (Global.lstAdditionalALPR.Count() > 0)
                {
                    CloseVideoSourceLPGate2();
                    MJPEGStream mjpegSourceALPR = new MJPEGStream("http://" + Global.lstAdditionalALPR.FirstOrDefault().ALPRIP + ":9901");

                    vspLPGate2.VideoSource = mjpegSourceALPR;
                    vspLPGate2.Start();
                }

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
        private void CloseVideoSourceLPGate2()
        {
            if (vspLPGate2.VideoSource != null)
            {
                vspLPGate2.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!vspLPGate2.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (vspLPGate2.IsRunning)
                {
                    vspLPGate2.Stop();
                }



                vspLPGate2.VideoSource = null;
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

            BeginInvoke((MethodInvoker)delegate
            {
                lblCurrentTime.Text = DateTime.Now.ToString();
            });

            try
            {
                if (Global.AIEnabled == true)
                {
                    string address1 = string.Format("http://" + Global.ALPR_CAMERA_HOST[Global.ENTRY_ALPR] + "/vvq?wfilter=1&getlast_trigger_perc");
                    string response = string.Empty;

                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;
                        response = client.DownloadString(address1);



                        if (response.Contains("last_trigger_perc=0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0"))

                        {

                        }
                        else
                        {
                            if (image != null) // && !m_Anpr_Active
                            {
                                anprThread.SetImage(new Bitmap(image));
                            }
                        }

                    }
                }

                else
                {
                    //if (m_Anpr_Active == false)
                    //{
                    //    try
                    //    {


                    //        string address = string.Format("http://" + Global.ALPR_CAMERA_HOST[Global.ENTRY_ALPR] + "/trigger/gpiotrigger?getgpin&wfilter=1");
                    //        string response = string.Empty;

                    //        using (WebClient client = new WebClient())
                    //        {
                    //            client.Proxy = null;
                    //            response = client.DownloadString(address);
                    //            if (response.Contains("gpin=0") || response.Contains("gpin=-1"))

                    //            {

                    //            }
                    //            else
                    //            {
                    //                anprThread.SetImage(new Bitmap(image));
                    //            }

                    //        }





                    //    }
                    //    catch (Exception ex)
                    //    {

                    //        var test = ex;
                    //    }
                    //}
                    //else
                    //{
                    if (Global.ENTRY_LOOP_SENSOR == false)
                    {
                        if(frameCounter == skipAlprFrames)
                        {
                            anprThread.SetImage(new Bitmap(image)); // Process the frame
                            frameCounter = 0;
                        }
                        else
                        {
                            frameCounter++;
                        }
                          

                        //----------------------Code for old camera ------------------------------

                        //lock (this.lockObject)
                        //{
                        //    if (this.stopwatch.Elapsed.TotalSeconds >= 1)
                        //    {
                        //        this.frameCounter = 0;
                        //        this.stopwatch.Restart();
                        //    }
                        //    if (this.frameCounter < 3)
                        //    {
                        //        this.anprThread.SetImage(new Bitmap(image));
                        //        this.frameCounter++;
                        //    }
                        //}


                        //----------------------Code for old camera ------------------------------


                        ////----------------------Code for new cameras-----------------------------
                        //if (image != null) // && !m_Anpr_Active
                        //{
                        //    //Task.Delay(2000).Wait();

                        //    anprThread.SetImage(new Bitmap(image));

                        //}
                        ////----------------------Code for new cameras-----------------------------
                    }
                    else
                    {
                        try
                        {


                            string address1 = string.Format("http://" + Global.ALPR_CAMERA_HOST[Global.ENTRY_ALPR] + "/trigger/gpiswtrigger?wfilter=1&output=0&sendtrigger=1");
                            string response1 = string.Empty;




                            string address = string.Format("http://" + Global.ALPR_CAMERA_HOST[Global.ENTRY_ALPR] + "/trigger/gpiotrigger?getgpin&wfilter=1");
                            string response = string.Empty;

                            using (WebClient client = new WebClient())
                            {
                                client.Proxy = null;
                                response = client.DownloadString(address);
                                if (response.Contains("gpin=0") || response.Contains("gpin=-1"))

                                {

                                }
                                else
                                {
                                    anprThread.SetImage(new Bitmap(image));
                                }

                            }





                        }
                        catch (Exception ex)
                        {

                            var test = ex;
                        }
                    }
                    // }
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

        private void FillAdditionalALPR()
        {
            presenter.FillAdditionalALPR();
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
            catch (Exception ex)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    // MessageBox.Show(ex.ToString());
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
            else
            {
                // Global.ShowMessage("USB Lock Verified successfully");
                keylok.GetDongleType();

                if (keylok.DongleType == 1)
                {
                    // buttonExecCode.Enabled = true;
                    // panelExe.Enabled = true;
                }

                keylok.AuthorizeRead();
                ReadPermissionsFromDongle();

            }
        }



        private void ReadPermissionsFromDongle()
        {
            int reg, regDriver, regRelay;


            try
            {
                reg = int.Parse("1");
            }
            catch (Exception)
            {
                MessageBox.Show("register must be in the range 0-55");
                return;
            }

            try
            {
                regDriver = int.Parse("2");
            }
            catch (Exception)
            {
                MessageBox.Show("register must be in the range 0-55");
                return;
            }

            try
            {
                regRelay = int.Parse("3");
            }
            catch (Exception)
            {
                MessageBox.Show("register must be in the range 0-55");
                return;
            }



            try
            {

                if (keylok.ReadRegister(reg) == 111)
                {
                    Global.IsSceneAllowed = true;
                }
                else
                {
                    Global.IsSceneAllowed = false;
                }


                if (keylok.ReadRegister(reg) == 444)
                {
                    Global.IsAdditionalALPRAllowed = true;
                }
                else
                {
                    Global.IsAdditionalALPRAllowed = false;
                }


                if (keylok.ReadRegister(regDriver) == 222)
                {
                    Global.IsDriverAllowed = true;
                }
                else
                {
                    Global.IsDriverAllowed = false;
                }


                if (keylok.ReadRegister(regRelay) == 333)
                {
                    Global.IsRelayAllowed = true;
                }
                else
                {
                    Global.IsRelayAllowed = false;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
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

        private void StartAutoRecording()
        {

            var dispatcherOp = base.Invoke(new MethodInvoker(() =>
            {
                imgRecording.Visible = true;
                btnRecordingOff.PerformClick();
            }));

        }


        private void StopAutoRecording()
        {

            var dispatcherOp = base.Invoke(new MethodInvoker(() =>
            {
                btnRecordingOn.PerformClick();
                imgRecording.Visible = false;
            }));

        }

        private void SetAutoModeAfterReset()
        {
            var dispatcherOp = base.Invoke(new MethodInvoker(() =>
            {
                btnAuto.Checked = true;
                lblFODError.Text = "";
            }));
        }

        // Start Recording
        private void btnRecordingOff_Click(object sender, EventArgs e)
        {

            if (cameraResetStarted == true)
            {

                lblFODError.Text = "Camera reset in progress.please wait";
                return;
            }


            if (m_Auto != true)
            {
                msg = System.Text.Encoding.ASCII.GetBytes("R");

                stream_commands.Write(msg, 0, msg.Length);
            }
            this.pBoxStitch.Image = null;
            //
            var f = this.m_DestinationDir;
            this.pBoxComparison.Image = null;
            this.btnOriginal.PerformClick();


            this.StartRecording();

            BtnRecordingOff(sender, e);
        }

        public bool StopRecording()
        {
            Thread.Sleep(500);
            bool flag;
            try
            {
                this.m_Recording = false;
                var dispatcherOp = base.Invoke(new MethodInvoker(() =>
                {
                    this.writer.Close();


                    SetAuthorizationBoxLive(txtLiveLPEnglish.Text, txtLiveLPArabic.Text);
                    Application.DoEvents();
                    GC.Collect();
                    Application.DoEvents();

                    if (auto == false || chkFODEnabled.Checked == false)
                    {
                        this.btnRecordingOn.Visible = false;
                        this.btnRecordingOff.Visible = true;
                    }


                    presenter.ProcessView_BtnRecordingOn();
                    pBoxComparison.Dock = DockStyle.None;
                }));



                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                flag = false;
                Application.DoEvents();
                GC.Collect();
                Application.DoEvents();
            }
            return flag;

            //try
            //{
            //    m_Anpr_Active = false;
            //    m_Recording = false;

            //    Invoke((MethodInvoker)delegate
            //    {
            //        writer.Close();

            //        //MessageBox.Show("writer.Close();");
            //        // On-screen display
            //        //ShowStatus();

            //        // red button
            //        btnRecordingOn.Visible = false;
            //        // green button
            //        btnRecordingOff.Visible = true;
            //    });
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    return false;
            //}

            //return true;
        }

        // Stop Recording
        private void btnRecordingOn_Click(object sender, EventArgs e)
        {

            if (cameraResetStarted == true)
            {

                lblFODError.Text = "Camera reset in progress.please wait";
                return;
            }

            if (m_Auto != true)
            {
                msg = System.Text.Encoding.ASCII.GetBytes("P");

                stream_commands.Write(msg, 0, msg.Length);
            }
            this.StopRecording();



            BtnRecordingOn(sender, e);
        }

        public bool StartRecording()
        {

            BeginInvoke((MethodInvoker)delegate
            {

                lblFODError.Text = "";
                pBoxComparison.Dock = DockStyle.Fill;
                pBoxComparison.SizeMode = PictureBoxSizeMode.CenterImage;
                pBoxComparison.Image = Image.FromFile("loader.gif");
            });

            //m_Anpr_Active = true;
            bool flag;
            try
            {
                Task.Run(() =>
                {
                    if (File.Exists(string.Concat(Global.TEMP_FOLDER, "\\outPutVer.jpg")))
                    {
                        File.Delete(string.Concat(Global.TEMP_FOLDER, "\\outPutVer.jpg"));
                    }
                    if (File.Exists(string.Concat(Global.TEMP_FOLDER, "\\translation.dat")))
                    {
                        File.Delete(string.Concat(Global.TEMP_FOLDER, "\\translation.dat"));
                    }
                });
                base.BeginInvoke(new MethodInvoker(() =>
                {
                    this.firstRecordedFrameTimeStamp = DateTime.MinValue;
                    this.nextWrittenFrameTimeStamp = DateTime.MinValue;
                    this.m_Recording = true;
                    writer.Open(Global.TEMP_FOLDER + "\\driver.avi", Global.DRIVER_CAM_WIDTH, Global.DRIVER_CAM_HEIGHT, 30, AForge.Video.FFMPEG.VideoCodec.MPEG4, 1000000);


                    //  writer.Open(Global.TEMP_FOLDER + "\\driver.avi", Global.DRIVER_CAM_WIDTH, Global.DRIVER_CAM_HEIGHT, 30, VideoCodec.WMV2, 10000 * 1000);

                    //this.writer.Open(string.Concat(Global.TEMP_FOLDER, "\\driver.avi"), 1920, 1080, 30);
                    Task.Run(() => this.TakeSnapshots(false));

                    this.btnRecordingOn.Visible = true;
                    this.btnRecordingOff.Visible = false;
                }));
                return true;
            }
            catch (Exception exception)
            {
                flag = false;
            }
            return flag;
            //try
            //{
            //    m_Anpr_Active = true;
            //    //t = TimeSpan.Zero;

            //    // Delete Stitch Image and Translation.Dat
            //    Task tDel = Task.Run(() =>
            //    {
            //        if (File.Exists(Global.TEMP_FOLDER + "\\" + "outPutVer.jpg"))
            //            File.Delete(Global.TEMP_FOLDER + "\\" + "outPutVer.jpg");

            //        if (File.Exists(Global.TEMP_FOLDER + "\\" + "translation.dat"))
            //            File.Delete(Global.TEMP_FOLDER + "\\" + "translation.dat");
            //    });

            //    // ********************** EMGU ************************************************************************************************************************
            //    //int FrameRate = 15; //Set the framerate manually as a camera would retun 0 if we use GetCaptureProperty()
            //    ////Set up a video writer component
            //    ///*                                        ---USE----
            //    ///* VideoWriter(string fileName, int compressionCode, int fps, int width, int height, bool isColor)
            //    // *
            //    // * Compression code. 
            //    // *      Usually computed using CvInvoke.CV_FOURCC. On windows use -1 to open a codec selection dialog. 
            //    // *      On Linux, use CvInvoke.CV_FOURCC('I', 'Y', 'U', 'V') for default codec for the specific file name. 
            //    // * 
            //    // * Compression code. 
            //    // *      -1: allows the user to choose the codec from a dialog at runtime 
            //    // *       0: creates an uncompressed AVI file (the filename must have a .avi extension) 
            //    // *
            //    // * isColor.
            //    // *      true if this is a color video, false otherwise
            //    // */
            //    //VW = new VideoWriter("C:\\vittemp\\driver.avi", CvInvoke.CV_FOURCC('W', 'M', 'V', '3'), (int)FrameRate, DRIVER_CAM_WIDTH, DRIVER_CAM_HEIGHT, true);
            //    //*****************************************************************************************************************************************************

            //    //***********************  AFORGE *********************************************************************************************************************

            //    Invoke((MethodInvoker)delegate
            //     {
            //         firstRecordedFrameTimeStamp = DateTime.MinValue;
            //         nextWrittenFrameTimeStamp = DateTime.MinValue;

            //         writer.Open(Global.TEMP_FOLDER + "\\driver.avi", Global.DRIVER_CAM_WIDTH, Global.DRIVER_CAM_HEIGHT, 30, AForge.Video.FFMPEG.VideoCodec.WMV2, 10000 * 1000); // 

            //        //*****************************************************************************************************************************************************

            //        Task tSnap = Task.Run(() => { TakeSnapshots(); });

            //        //start recording driver camera
            //        m_Recording = true;

            //        //lpNumEnglish = this.txtLiveLPEnglish.Text;
            //        //lpNumArabic = this.txtLiveLPArabic.Text;

            //        // red button
            //        btnRecordingOn.Visible = true;
            //        // green button
            //        btnRecordingOff.Visible = false;
            //     });
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}

            //return true;
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
                            bitmap.Save(Global.TEMP_FOLDER + "\\Driver.jpg", ImageFormat.Jpeg);
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

        private void btnOpen1_CheckedChanged(object sender, EventArgs e)
        {
            if (btnOpen1.Checked)
            {
                TurnLC(Global.mNCDPort1, true);
            }
            else
            {
                TurnLC(Global.mNCDPort1, false);
            }


        }

        private void btnOpen2_CheckedChanged(object sender, EventArgs e)
        {
            if (btnOpen2.Checked)
            {
                TurnLC(Global.mNCDPort2, true);
            }
            else
            {
                TurnLC(Global.mNCDPort2, false);
            }


        }

        private void btnOpen3_CheckedChanged(object sender, EventArgs e)
        {
            if (btnOpen3.Checked)
            {
                TurnLC(Global.mNCDPort3, true);
            }
            else
            {
                TurnLC(Global.mNCDPort3, false);
            }


        }


        private void btnOpen4_CheckedChanged(object sender, EventArgs e)
        {
            if (btnOpen4.Checked)
            {
                TurnLC(Global.mNCDPort4, true);
            }
            else
            {
                TurnLC(Global.mNCDPort4, false);
            }


        }



        #endregion

        #endregion



        #region ResizeControl

        private int GetFormArea(Size size)
        {
            return size.Height * size.Width;
        }

        private void Form_Resize(object sender, EventArgs e)
        {



            Control control = (Control)sender;


            float scaleFactor = (float)GetFormArea(control.Size) / (float)_lastFormSize;

            ResizeFont(this.Controls, scaleFactor);






            _lastFormSize = GetFormArea(control.Size);

        }

        private void ResizeFont(Control.ControlCollection coll, float scaleFactor)
        {
            foreach (Control c in coll)
            {
                if (c.Name == "crystalReportViewer1")
                {

                }

                else
                {


                    if (c.HasChildren)
                    {
                        ResizeFont(c.Controls, scaleFactor);
                    }
                    else
                    {

                        //if (c.GetType().ToString() == "System.Windows.Form.Label")
                        if (true)
                        {
                            // scale font
                            c.Font = new Font(c.Font.FontFamily.Name, c.Font.Size * scaleFactor, c.Font.Style, c.Font.Unit);
                        }

                    }


                }


            }
        }

        private void _dgv_Column_Adjust(DataGridView dgv, bool _showRowHeader) //if you have Datagridview 
                                                                               //and want to resize the column base on its dimension.
        {


            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                if (dgv.Columns[i].HeaderText != "Plate Number")
                {
                    dgv.Columns[i].HeaderCell.Style.Font = new Font(dgView.Font.FontFamily.Name, dgView.Columns[i].Width / 15, dgView.Font.Style, dgView.Font.Unit);
                }
                else
                {
                    dgv.Columns[i].HeaderCell.Style.Font = new Font(dgView.Font.FontFamily.Name, dgView.Columns[i].Width / 26, dgView.Font.Style, dgView.Font.Unit);
                }
            }
        }

        #endregion

        #region Methods

        public void processResponses(NetworkStream stream_receiver)
        {
            try
            {


                // Buffer for reading data
                Byte[] bytes = new Byte[256];       // Receiving data
                String msg = null;

                while (true)
                {
                    Int32 bytes_read = stream_receiver.Read(bytes, 0, bytes.Length);

                    msg = System.Text.Encoding.ASCII.GetString(bytes, 0, bytes_read);

                    if (msg.Contains("--Interupt from client-- Recording started"))
                    {
                        if (m_Auto == true)
                        {
                            StartAutoRecording();
                        }
                        rec_started = true;
                        rec_ended = false;

                    }
                    else if (msg.Contains("Error finding a camera"))
                    {
                        var dispatcherOp = base.Invoke(new MethodInvoker(() =>
                        {
                            Global.ShowMessage("Under Vehicle Camera Not Found,Please check serial number", false);
                        }));
                    }

                    else if (msg.Contains("--Interupt from client-- Recording ended"))
                    {
                        if (m_Auto == true)
                        {
                            StopAutoRecording();
                        }
                        rec_started = false;
                        rec_ended = true;

                    }


                    else if (msg.Contains("Error reseting a camera") || msg.Contains("Error finding a camera") || msg.Contains("Error starting or Initializing camera"))
                    {
                        lblFODError.Text = "Error resting camera...";
                        cameraResetStarted = false;


                    }
                    else if (msg.ToLower().Contains("reset complete"))
                    {
                        cameraResetStarted = false;
                        SetAutoModeAfterReset();

                    }
                    else
                    {
                        rec_started = false;
                        rec_ended = false;
                    }
                    Console.WriteLine("Received: {0}", msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);

            }
        }

        public async Task StartListening()
        {
            try
            {
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();


            }
            catch (Exception)
            {

                serverstarted = false;
            }




            StartClientListening();


            TcpClient client_commands = await server.AcceptTcpClientAsync();
            Console.WriteLine("Sender Connected!");
            stream_commands = client_commands.GetStream();

            // 'client_receiver' is used to recieve responses from the client
            TcpClient client_responses = await server.AcceptTcpClientAsync();
            Console.WriteLine("Receiver Connected!");




            stream_responses = client_responses.GetStream();

            serverstarted = true;



            await Task.Factory.StartNew(() => processResponses(stream_responses));



            //NetworkStream ns = handler.GetStream();
            //StreamReader sr = new StreamReader(ns);
            //String message = await sr.ReadLineAsync();

            //Console.WriteLine(message);






        }

        private void StartClientListening()
        {
            var camserial = Global.LicenseNo;

            Task tStitch = Task.Run(() =>
            {
                ProcessStartInfo _processStartInfo = new ProcessStartInfo();
                _processStartInfo.WorkingDirectory = Application.StartupPath + @"\Camer_Recording\";
                _processStartInfo.FileName = @"Camera_Recording.exe";
                _processStartInfo.UseShellExecute = true;

                _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _processStartInfo.CreateNoWindow = true;
                _processStartInfo.Arguments = "\"" + camserial + "\""; //"C:\\IVISSTemp\\"; //"\"" + destination_dir + "\"";
                //_processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process myProcess = Process.Start(_processStartInfo);




            });

        }
        private void Main_Load(object sender, EventArgs e)
        {

            dtAdditionalALPR = new DataTable();

            dtAdditionalALPR.Columns.Add("License", typeof(String));
            dtAdditionalALPR.Columns.Add("ArabicLicense", typeof(String));
            dtAdditionalALPR.Columns.Add("Date", typeof(DateTime));
            dtAdditionalALPR.Columns.Add("Gate", typeof(String));
            dtAdditionalALPR.Columns.Add("Status", typeof(String));
            dtAdditionalALPR.Columns.Add("Path", typeof(String));
            dtAdditionalALPR.Columns.Add("Accuracy", typeof(String));
            dtAdditionalALPR.Columns.Add("Classification", typeof(String));

            try
            {
                foreach (Process proc in Process.GetProcessesByName("Camera_Recording"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            StartListening();

            lblLoggedInUser.Text = "Current Logged in User: " + Global.USER_NAME;
            lblLoggedInUser.ForeColor = Color.Silver;
            //TabMain.TabPages.Remove(tpViewDetail);
            // TabMain.TabPages.Remove(tpManageImage);

            //TabMain.TabPages["tpViewDetail"].Hide();
            btnAuto.Checked = true;

            if (File.Exists(Application.StartupPath + "\\AutoFODEnabled.xml"))
            {
                dtAutoFOD.Clear();

                dtAutoFOD.ReadXml(Application.StartupPath + "\\AutoFODEnabled.xml");

                if (dtAutoFOD.Rows.Count > 0)
                {

                    var EnabledValue = dtAutoFOD.Rows[0]["AutoFODEnabled"].ToString();

                    if (EnabledValue == "1")
                        chkFODEnabled.Checked = true;
                    else
                        chkFODEnabled.Checked = false;
                }
            }
            else

            {

                DataRow drowAuto = dtAutoFOD.NewRow();
                drowAuto["AutoFODEnabled"] = "1";
                dtAutoFOD.Rows.Add(drowAuto);

                dtAutoFOD.WriteXml((Application.StartupPath + "\\AutoFODEnabled.xml"));


            }

            if (Global.USER_TYPE != "ADMIN")
            {
                chkSaveVideo.Visible = false;
            }


            if (Global.USER_TYPE == "GUARD")
            {
                TabSettings.TabPages.RemoveByKey("tpSettings_UserManagement");
                //TabSettings.TabPages.RemoveByKey("tpVistorDataEntry_Settings");
                TabSettings.TabPages.RemoveByKey("tpSettings_Settings");
                materialRaisedButton1.Enabled = false;
                txtSerialNoSettings.Enabled = false;
                //TabSettings.TabPages["tpSettings_UserManagement"].Hide();
                // TabSettings.TabPages["tpVistorDataEntry_Settings"].Hide();


            }
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

                vspDriver.BackgroundImage = Image.FromFile(@"C:\IVISSTemp\Driver.jpg");
                vspScene.BackgroundImage = Image.FromFile(@"C:\IVISSTemp\scene.bmp");
                vspLP.BackgroundImage = Image.FromFile(@"C:\IVISSTemp\lpnum.bmp");

                this.txtLiveLPEnglish.Text = "6RBZ328";
            }

            //lblLiveAccuracy.Text = "99%";

          

            if (Global.ProfileMode == "English")
            {
                rdoEnglish.Checked = true;
            }
            else
            {
                rdoBoth.Checked = true;
            }

            if (Global.IsRelayAllowed == false)
            {
                btnOpen1.Enabled = false;
                btnOpen2.Enabled = false;
                btnOpen3.Enabled = false;
                btnOpen4.Enabled = false;

                txtRelay1Port.Enabled = false;
           //     txtRelay1Port.Visible = false;

               // txtRelay1Port.Text = "";
                txtRelay1Caption.Enabled = false;
               // txtRelay1Caption.Visible = false;
              // txtRelay1Caption.Text = "";
                txtRelay1CaptionArab.Enabled = false;
              //  txtRelay1CaptionArab.Visible = false;
              //  txtRelay1CaptionArab.Text = "";

                txtRelay1ArabicPort.Enabled = false;
              //  txtRelay1ArabicPort.Visible = false;

              //  txtRelay1ArabicPort.Text = "";

                txtRelay2Port.Enabled = false;
               // txtRelay2Port.Visible = false;

                txtRelay2ArabicPort.Enabled = false;
              //  txtRelay2ArabicPort.Visible = false;

                txtRelay2Caption.Enabled = false;
              //  txtRelay2Caption.Visible = false;

                txtRelay2CaptionArab.Enabled = false;
              //  txtRelay2CaptionArab.Visible = false;


              //  txtRelay2Port.Text = "";

              //  txtRelay2Port.Visible = false;

             //   txtRelay2ArabicPort.Text = "";
             //   txtRelay2ArabicPort.Visible = false;
            //    txtRelay2Caption.Text = "";
             //   txtRelay2Caption.Visible = false;
              //  txtRelay2CaptionArab.Text = "";
             //   txtRelay2CaptionArab.Visible = false;


                txtRelay3Port.Enabled = false;
              // txtRelay3Port.Visible = false;

                txtRelay3ArabicPort.Enabled = false;
              //  txtRelay3ArabicPort.Visible = false;


                txtRelay3Caption.Enabled = false;
             //   txtRelay3Caption.Visible = false;

                txtRelay3CaptionArab.Enabled = false;
             //   txtRelay3CaptionArab.Visible = false;


             //   txtRelay3Port.Text = "";
            //    txtRelay3Port.Visible = false;

           //     txtRelay3ArabicPort.Text = "";
           //     txtRelay3ArabicPort.Visible = false;

            //    txtRelay3Caption.Text = "";
            //    txtRelay3Caption.Visible = false;

              //  txtRelay3CaptionArab.Text = "";
            //    txtRelay3CaptionArab.Visible = false;


                txtRelay4Port.Enabled = false;
            //    txtRelay4Port.Visible = false;

                txtRelay4ArabicPort.Enabled = false;
             //   txtRelay4ArabicPort.Visible = false;


                txtRelay4Caption.Enabled = false;
             //   txtRelay4Caption.Visible = false;

                txtRelay4CaptionArab.Enabled = false;
              //  txtRelay4CaptionArab.Visible = false;


                txtIPAddress.Enabled = false;
              //  txtIPAddress.Visible = false;

                txtListenPort.Enabled = false;
              //  txtListenPort.Visible = false;


            }

            if (Global.IsDriverAllowed == false)
            {
                txtEntryDriverIP.Enabled = false;
              //  txtEntryDriverIP.Visible = false;
                txtExitDriverIP.Enabled = false;
             //   txtExitDriverIP.Visible = false;
                txtDriverCamPassword.Enabled = false;
              //  txtDriverCamPassword.Visible = false;

                btnCaptureDriverSearch.Enabled = false;
                metroTrackBar1.Enabled = false;

            }

            if (Global.IsSceneAllowed == false)
            {
                txtEntrySceneIP.Enabled = false;

                txtEntrySceneIP.Visible = false;

                txtSceneCamPassword.Enabled = false;
                txtSceneCamPassword.Visible = false;
            }

            if(Global.IsAdditionalALPRAllowed==false)
            {
                btnAdditionalALPR.Visible = false;
                btnAdditionalALPR.Enabled = false;


                btnAddMoreALPR.Visible = false;
                btnAddMoreALPR.Enabled = false;

                label90.Visible = false;
                
            }




            lblRelay1.Text = Global.mlblRelay1;
            lblRelay1Arab.Text = Global.mlblRelayArab1;

            lblRelay2.Text = Global.mlblRelay2;
            lblRelay2Arab.Text = Global.mlblRelayArab2;

            lblRelay3.Text = Global.mlblRelay3;
            lblRelay3Arab.Text = Global.mlblRelayArab3;

            lblRelay4.Text = Global.mlblRelay4;
            lblRelay4Arab.Text = Global.mlblRelayArab4;


            FillGateForReport();



            ChangeThemeColors();
            txtSerialNoSettings.Text = Global.LicenseNo;
            // Load presenter form loaded
            FormLoaded(sender, e);

            if (!Global.DEMO)
                this.RunIPCameras();

            this.KeyUp += Main_KeyUp;


            AdditionalALPRGridSource();
        }

        private void FillGateForReport()
        {
            using (IVISSEntities db = new IVISSEntities())
            {

                var res = db.Details.Select(o => o.gate_no).Distinct();
                cboGate.Items.Add("");
                foreach (var s in res)
                {
                    cboGate.Items.Add(s);
                }
            }


        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F9)
            {
                this.txtLiveLPEnglish.Clear();
                this.txtLiveLPArabic.Clear();

                b_ALPRProcessing = true;
                Task T = Task.Run(() =>
                {
                    presenter.RefreshGrid();
                });
            }
            if (e.Control && e.KeyCode == Keys.F8)
            {
                if (this.Player != null)
                    this.Player.Stop();
            }
        }

        private void tbDriver_Scroll(object sender, EventArgs e)
        {

        }

        public string SelectImageComparison(string plate, string plateArab)
        {
            var licensePlate = plate;
            var licensePlateArab = plateArab;

            using (IVISSEntities db = new IVISSEntities())
            {

                var detail = from d in db.Details
                             select d;

                if (licensePlate.Trim().Length > 0)
                    detail = detail.Where(d => d.visitor_license_number == licensePlate);
                else
                    detail = detail.Where(d => d.visitor_license_number_arabic == licensePlateArab);

                var detailDefault = detail.Where(d => d.is_default == 1);

                var vi = detailDefault.FirstOrDefault();

                if (detailDefault.Count() == 0)
                {
                    vi = detail.FirstOrDefault();
                }

                if (vi != null)
                {
                    return vi.visitor_iviss_recording;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        private void FillDetailsTab(string plate, string arabplate, string path)
        {


            //Task T = Task.Run(() => 
            //{
            var comparisonPath = this.SelectImageComparison(plate, arabplate);

            // Stitch
            if (File.Exists(comparisonPath + @"\outPutVer.jpg"))
            {
                using (FileStream fs = new FileStream(comparisonPath + @"\outPutVer.jpg", FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    var imgComparison = Image.FromStream(fs);


                    pBoxStitchViewDetails.Image = imgComparison;
                    //});

                    fs.Close();
                }
            }
            else
            {
                //BeginInvoke((MethodInvoker)delegate
                //{
                this.pBoxStitchViewDetails.Image = null;
                //});
            }
            //});

            // Driver 
            if (File.Exists(path + @"\Driver.jpg"))
            {
                using (FileStream fs = new FileStream(path + @"\Driver.jpg", FileMode.Open))
                {
                    if (Global.IsDriverAllowed == true)
                        vspDriverViewDetails.BackgroundImage = Image.FromStream(fs);

                    fs.Close();
                }
            }
            else
            {
                vspDriverViewDetails.BackgroundImage = null;
            }

            // Scene Camera
            if (File.Exists(path + @"\Scene.bmp"))
            {
                using (FileStream fs = new FileStream(path + @"\Scene.bmp", FileMode.Open))
                {
                    if (Global.IsSceneAllowed == true)
                        vspSceneViewDetails.BackgroundImage = Image.FromStream(fs);

                    fs.Close();
                }
            }
            else
            {
                vspSceneViewDetails.BackgroundImage = null;
            }

            // License Plate Num
            if (File.Exists(path + @"\LpNum.bmp"))
            {
                using (FileStream fs = new FileStream(path + @"\LpNum.bmp", FileMode.Open))
                {
                    vspLPViewDetails.BackgroundImage = Image.FromStream(fs);
                    fs.Close();
                }
            }
            else
            {
                vspLPViewDetails.BackgroundImage = null;
            }

            // Stitch
            if (File.Exists(path + @"\outPutVer.jpg"))
            {
                if (firsttimeviewdetails == true)
                {
                    chkImageModeViewDetails.Checked = true;
                    firsttimeviewdetails = false;
                    chkImageModeViewDetails.Checked = false;
                }
                if (chkImageModeViewDetails.Checked)
                {
                    pnlViewComparisonImage.Size = new Size(464, 866);
                    pnlViewComparisonImage.VerticalScroll.Value = 0;
                    pBoxComparisonViewDetails.Size = new Size(464, 866);
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    using (FileStream fs = new FileStream(path + @"\outPutVer.jpg", FileMode.Open))
                    {



                        imgCompositeImage = Image.FromStream(fs);
                        int new_width = imgCompositeImage.Width * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;
                        int new_height = imgCompositeImage.Height * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;

                        Size new_size = new Size(464, new_height);

                        imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));



                        //pBoxComparisonViewDetails.Image = imgCompositeImage;
                        fs.Close();
                    }

                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                }


                pBoxComparisonViewDetails.Image = imgCompositeImage;
            }
            else
            {
                this.pBoxComparisonViewDetails.Image = null;
            }

            Task.Delay(500);
            btnOriginalViewDetails.PerformClick();

            //******************************************************************************************
            ReInitializeZoom();

        }

        private string GetAuthorization(string plate, string classification)
        {
            try
            {
                using (IVISSEntities db = new IVISSEntities())
                {
                    var authorizationfound = (from v in db.Visitors where v.visitor_license_plate == plate select v).ToList();
                    if (authorizationfound == null)
                    {
                        return classification;
                    }
                    else if (authorizationfound.Count() == 0)
                    {
                        return classification;
                    }
                    else
                    {
                        return authorizationfound.FirstOrDefault().visitor_authorization;
                    }
                }
            }
            catch (Exception)
            {

                return classification;
            }

        }

        private void dgView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (TabMain.TabPages.IndexOf(tpViewDetail) > 0)
            {
                if (TabMain.TabPages["tpManageImage"] != null)
                {
                    //TabMain.TabPages["tpManageImage"].Text = "";

                }
                Application.DoEvents();
                TabMain.SelectedIndex = 3;
                Application.DoEvents();

            }
            else
            {
                if (TabMain.TabPages["tpManageImage"] != null)
                {
                    //TabMain.TabPages["tpManageImage"].Text = "";

                }
                Application.DoEvents();
                TabMain.TabPages.Add(tpViewDetail);

                TabMain.SelectedIndex = 3;
                Application.DoEvents();

            }
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

                    var authorization = GetAuthorization(plate, classification);
                    dividerViewDetails.Left = btnOriginalViewDetails.Left;

                    if (path != null && path.Length > 0)
                    {
                        //var frm = new frmOverlay();
                        //frm.plate = plate;
                        //frm.plateArab = plateArab;
                        //frm.recordingPath = path;

                        FillDetailsTab(plate, plateArab, path);
                        // frm.Show();

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
                                    //btnOriginal.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;

                                });
                            });
                            //***********************************************************************************
                            SetFilesToOpen(path, plate, plateArab, authorization, accuracy, gate, visitorAccess);
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

        private void SetFilesToOpen(string path, string plate, string plateArab, string authorization, string accuracy, string gate, string visitorStatus)
        {
            InitializeDriver((IntPtr)vspDriver.Handle, (IntPtr)this.vspDriver.Width, (IntPtr)this.vspDriver.Height);

            Task t = Task.Run(() =>
            {
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {


                        lpNumEnglish = plate;
                        lpNumArabic = plateArab;

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


                        this.txtLiveLPEnglish.Text = "";
                        this.txtLiveLPArabic.Text = ""; //Regex.Replace(plateArab, @"[^\u0020-\u007E]", string.Empty);

                        //// Driver 
                        //if (File.Exists(path + @"\Driver.jpg"))
                        //{
                        //    using (FileStream fs = new FileStream(path + @"\Driver.jpg", FileMode.Open))
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



                                int new_width = 363;// imgCompositeImage.Width * pBoxComparison.Width / imgCompositeImage.Width;
                                int new_height = imgCompositeImage.Height * pBoxComparison.Width / imgCompositeImage.Width;

                                Size new_size = new Size(new_width, new_height);

                                imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));
                                pBoxComparison.Dock = DockStyle.None;

                                if (chkImageMode.Checked)
                                {
                                    pnlpBoxComparison.Size = new Size(363, 830);
                                    pnlpBoxComparison.VerticalScroll.Value = 0;
                                    pBoxComparison.Size = new Size(363, 830);
                                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                                }
                                else

                                {
                                    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                                }

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


        public void LoadAsyncSound()
        {
            try
            {
                // Replace this file name with a valid file name.
                this.Player.SoundLocation = @"C:\windows\media\tada.wav";
                this.Player.LoadAsync();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error loading sound");
            }
        }

        void Player_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (Player.IsLoadCompleted)
            {
                try
                {
                    this.Player.PlayLooping();
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message, "Error playing sound");
                }
            }
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


                //this.PlaySound();
            }
            else if (authorization == "AUTHORIZED") //green
            {
                if (this.Player != null)
                    this.Player.Stop();
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
                if (this.Player != null)
                    this.Player.Stop();

                lblVisitorClassificationLive.BackColor = Color.FromArgb(238, 218, 23);
            }

            if (authorization.Length > 0)
                lblVisitorClassificationLive.Text = authorization;
            else
                lblVisitorClassificationLive.Text = "VISITOR";
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {

            TurnLC(Global.mNCDPort1, false);
            TurnLC(Global.mNCDPort2, false);
            TurnLC(Global.mNCDPort3, false);
            TurnLC(Global.mNCDPort4, false);

            ncd1.ClosePort();

            CloseVideoSourceDriver();
            CloseVideoSourceScene();
            CloseVideoSourceLP();
            CloseVideoSourceLPGate2();

            anprThread.StopThread();
            anprThreadALPR.StopThread();


            try
            {
                foreach (Process proc in Process.GetProcessesByName("Camera_Recording"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            FormIsClosing(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            btnRecordingOff_Click(null, null);
        }

        private void picCompositeImage_Click(object sender, EventArgs e)
        {

        }

        private void picOriginalImage_Click(object sender, EventArgs e)
        {
            var frm = new frmViewImageV1();
            frm.imgPath = m_ComparisonPath + @"\outPutVer.jpg";
            frm.ShowDialog();
        }

        private void picDriver_Click(object sender, EventArgs e)
        {
            try
            {
                picLoadingViewDetails.Visible = true;
                Application.DoEvents();
                picLoadingViewDetails.Image = Image.FromFile("loadingnew.gif");

                Application.DoEvents();
                Application.DoEvents();
                if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
                {
                    Application.DoEvents();
                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }
                else
                {
                    Application.DoEvents();
                    TabMain.TabPages.Add(tpManageImage);

                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }


                string path = m_IVISSRecordingPath + "\\" + "Driver.jpg";
                this.kpImageViewer1.Image = null;
                OriginalImage = null;

                this.kpImageViewer1.Image = new Bitmap(path);

                System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

                this.kpImageViewer1.Zoom = 38;

                OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

                picLoadingViewDetails.Visible = false;
            }
            catch (Exception)
            {


            }




            //var frm = new frmViewImageFullScreenV1();
            //frm.imgPath = path;
            //frm.ShowDialog();
        }

        private void TurnOffConfidenceBtns()
        {
            //foreach (Button btn in pnlConfidence.Controls.OfType<MaterialFlatButton>())
            //    if (btn.Tag != null && btn.Tag.ToString() == "confidence")
            //        btn.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green;

            dividerImageType.Left = btnOriginal.Left;
            // this.tBarBrightness.Value = 20;

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
            dividerImageType.Left = btnHigh.Left;

            m_FOB = true;

            var path = GetFilePath();

            if (path.ToLower().Contains(Global.TEMP_FOLDER.ToLower()))
                return;
            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = path + "\\_high.jpg";

            if (File.Exists(m_SelectedImage))
            {
                if (chkImageMode.Checked)
                {
                    pnlpBoxComparison.Size = new Size(363, 830);
                    pnlpBoxComparison.VerticalScroll.Value = 0;
                    pBoxComparison.Size = new Size(363, 830);
                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                }

                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{

                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = 363;// imgCompositeImage.Width * pBoxComparison.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparison.Width / imgCompositeImage.Width;

                    Size new_size = new Size(new_width, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));


                    pBoxComparison.Image = imgCompositeImage;

                    //pBoxComparison.Image = Image.FromStream(fs);
                    //});

                    fs.Close();
                }
                // pBoxComparison.Image = AdjustBrightness((Bitmap)imgCompositeImage, tBarBrightness.Value);

                pBoxComparison.Image = imgCompositeImage;

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
            dividerImageType.Left = btnMedium.Left;
            m_FOB = true;
            var path = GetFilePath();

            if (path.ToLower().Contains(Global.TEMP_FOLDER.ToLower()))
                return;
            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\_medium.jpg";

            if (File.Exists(m_SelectedImage))
            {
                if (chkImageMode.Checked)
                {
                    pnlpBoxComparison.Size = new Size(363, 830);
                    pnlpBoxComparison.VerticalScroll.Value = 0;
                    pBoxComparison.Size = new Size(363, 830);
                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                }
                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = 363;// imgCompositeImage.Width * pBoxComparison.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparison.Width / imgCompositeImage.Width;

                    Size new_size = new Size(new_width, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));


                    pBoxComparison.Image = imgCompositeImage;
                    // pBoxComparison.Image = Image.FromStream(fs);
                    //});

                    fs.Close();
                }
                // pBoxComparison.Image = AdjustBrightness((Bitmap)imgCompositeImage, tBarBrightness.Value);
                pBoxComparison.Image = imgCompositeImage;

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
            dividerImageType.Left = btnLow.Left;

            m_FOB = true;

            var path = GetFilePath();

            if (path.ToLower().Contains(Global.TEMP_FOLDER.ToLower()))
                return;

            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\_low.jpg";

            if (File.Exists(m_SelectedImage))
            {
                if (chkImageMode.Checked)
                {
                    pnlpBoxComparison.Size = new Size(363, 830);
                    pnlpBoxComparison.VerticalScroll.Value = 0;
                    pBoxComparison.Size = new Size(363, 830);
                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                }
                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = 363;// imgCompositeImage.Width * pBoxComparison.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparison.Width / imgCompositeImage.Width;

                    Size new_size = new Size(new_width, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));


                    pBoxComparison.Image = imgCompositeImage;
                    // pBoxComparison.Image = Image.FromStream(fs);
                    //});

                    fs.Close();
                }
                // pBoxComparison.Image = AdjustBrightness((Bitmap)imgCompositeImage, tBarBrightness.Value);

                pBoxComparison.Image = imgCompositeImage;

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

        private void btnOriginal_Click_1(object sender, EventArgs e)
        {
            dividerImageType.Left = btnOriginal.Left;

            m_FOB = false;

            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\outPutVer.jpg";

            if (File.Exists(m_SelectedImage))
            {
                if (chkImageMode.Checked)
                {
                    pnlpBoxComparison.Size = new Size(363, 830);
                    pnlpBoxComparison.VerticalScroll.Value = 0;
                    pBoxComparison.Size = new Size(363, 830);
                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                }

                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    if (chkImageMode.Checked)
                    {
                        pnlpBoxComparison.Size = new Size(363, 830);
                        pnlpBoxComparison.VerticalScroll.Value = 0;
                        pBoxComparison.Size = new Size(363, 830);
                        pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                    }
                    else

                    {
                        pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                    }
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    imgCompositeImage = Image.FromStream(fs);

                    int new_width = 363;// imgCompositeImage.Width * pBoxComparison.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparison.Width / imgCompositeImage.Width;

                    if (new_height == 0) return;

                    Size new_size = new Size(new_width, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));

                    if (chkImageMode.Checked)
                    {
                        pnlpBoxComparison.Size = new Size(363, 830);
                        pnlpBoxComparison.VerticalScroll.Value = 0;
                        pBoxComparison.Size = new Size(363, 830);
                        pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                    }
                    else

                    {
                        if (chkImageMode.Checked)
                        {
                            pnlpBoxComparison.Size = new Size(363, 830);
                            pnlpBoxComparison.VerticalScroll.Value = 0;
                            pBoxComparison.Size = new Size(363, 830);
                            pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                        }
                        else

                        {
                            pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                        }
                    }
                    pBoxComparison.Image = imgCompositeImage;

                    //});

                    fs.Close();
                }
                //   pBoxComparison.Image = AdjustBrightness((Bitmap)imgCompositeImage, tBarBrightness.Value);

                pBoxComparison.Image = imgCompositeImage;


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

        private void btnAuto_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (btnAuto.Checked == true)
                {
                    btnRecordingOff.Width = 0;
                    btnRecordingOn.Width = 0;
                    lblRecord.Visible = false;


                    if (serverstarted == true)
                    {
                        try
                        {
                            btnRecordingOn.PerformClick();
                        }
                        catch (Exception)
                        {


                        }




                        msg = System.Text.Encoding.ASCII.GetBytes("A");

                        stream_commands.Write(msg, 0, msg.Length);
                    }
                    m_Auto = true;
                    txtLiveLPEnglish.Enabled = false;
                    txtLiveLPArabic.Enabled = false;

                }
                else
                {
                    btnRecordingOff.Width = 52;
                    btnRecordingOn.Width = 52;
                    lblRecord.Visible = true;


                    msg = System.Text.Encoding.ASCII.GetBytes("M");
                    stream_commands.Write(msg, 0, msg.Length);

                    m_Auto = false;

                    txtLiveLPEnglish.Text = "";
                    txtLiveLPArabic.Text = "";



                    txtLiveLPEnglish.Enabled = true;
                    txtLiveLPArabic.Enabled = true;


                }



                presenter.SetRecordingMode();
            }
            catch (Exception)
            {

            }


        }

        private void tmCompositeImage_Tick(object sender, EventArgs e)
        {


            if (File.Exists(Global.TEMP_FOLDER + @"\outPutVer.jpg"))
            {
                try
                {
                    Task.Delay(500).Wait();

                    this.m_DestinationDir = presenter.SaveData();
                    //// Play Sound
                    if (isPlayable == true)
                    {
                        if (lblVisitorClassificationLive.Text.ToUpper().Contains("PROHIBITED"))
                        {
                            lblVisitorClassificationLive.BackColor = Color.FromArgb(241, 79, 73);
                            this.LoadAsyncSound();
                        }
                        isPlayable = false;
                    }
                    if (File.Exists(this.m_DestinationDir + @"\outPutVer.jpg"))
                    {
                        //pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;


                        //pBoxComparison.Image = Image.FromFile(this.m_DestinationDir + @"\outPutVer.jpg");
                        using (FileStream fs = new FileStream(this.m_DestinationDir + @"\outPutVer.jpg", FileMode.Open))
                        {

                            imgCompositeImage = Image.FromStream(fs);
                            int new_width = 363;// imgCompositeImage.Width * pBoxComparison.Width / imgCompositeImage.Width;
                            int new_height = imgCompositeImage.Height * pBoxComparison.Width / imgCompositeImage.Width;

                            Size new_size = new Size(new_width, new_height);




                            imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));
                            pBoxComparison.Dock = DockStyle.None;

                            if (chkImageMode.Checked)
                            {
                                pnlpBoxComparison.Size = new Size(363, 830);
                                pnlpBoxComparison.VerticalScroll.Value = 0;
                                pBoxComparison.Size = new Size(363, 830);
                                pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                            }
                            else

                            {
                                pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                            }

                            pBoxComparison.Image = imgCompositeImage;





                            //if (chkImageMode.Checked)
                            //{
                            //    pnlpBoxComparison.Size = new Size(363, 830);
                            //    pnlpBoxComparison.VerticalScroll.Value = 0;
                            //    pBoxComparison.Size = new Size(363, 830);
                            //    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                            //}
                            //else

                            //{
                            //    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                            //}

                            //pBoxComparison.Image = imgCompositeImage;
                            fs.Close();
                        }





                        tmCompositeImage.Stop();


                        if (chkFODEnabled.Checked == true)
                            tmFOD.Start();
                    }
                    //isAutoStitchFinished = false;
                }
                catch (Exception ex)
                {
                    tmCompositeImage.Stop();
                }
            }



        }

        private void tmFOD_Tick(object sender, EventArgs e)
        {

            LoadImageComparisonWithFOD();
            tmFOD.Stop();

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
        private void tBarBrightness_Scroll(object sender, ScrollEventArgs e)
        {
            pBoxComparison.Image = AdjustBrightness((Bitmap)imgCompositeImage, tBarBrightness.Value);

        }


        private void picLicensePlate_Click(object sender, EventArgs e)
        {

            try
            {
                picLoadingViewDetails.Visible = true;
                Application.DoEvents();
                picLoadingViewDetails.Image = Image.FromFile("loadingnew.gif");

                Application.DoEvents();
                Application.DoEvents();
                if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
                {
                    Application.DoEvents();
                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }
                else
                {
                    Application.DoEvents();
                    TabMain.TabPages.Add(tpManageImage);

                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }


                string path = m_IVISSRecordingPath + "\\" + "LpNum.bmp";
                this.kpImageViewer1.Image = null;
                OriginalImage = null;

                this.kpImageViewer1.Image = new Bitmap(path);

                System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

                this.kpImageViewer1.Zoom = 38;

                OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

                picLoadingViewDetails.Visible = false;
            }
            catch (Exception)
            {


            }

            //string path = m_IVISSRecordingPath + "\\" + "LpNum.bmp";
            //var frm = new frmViewImageFullScreenV1();
            //frm.imgPath = path;
            //frm.ShowDialog();
        }


        public static byte[] ImageToByteArray(Image imageToConvert)
        {
            using (var ms = new MemoryStream())
            {
                Bitmap bmp = new Bitmap(imageToConvert);
                bmp.Save(ms, ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        public Image ConvertByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        public async void RunFODAsync(string referenceimage, string caseimage, bool IsManual)
        {


            //  string referenceimage = @"C:\Users\User\Desktop\FOD Manual Test\9-18-2020 17-34-40.145\outPutVer.jpg";
            //  string caseimage = @"C:\Users\User\Desktop\FOD Manual Test\9-18-2020 17-37-10.157\outPutVer.jpg";
            DateTime dtFrom = DateTime.Now;
            tmFOD.Stop();


            if (IsManual == false)
            {
                //using (var db = new IVISSEntities())
                //{
                //    var visitors = (from v in db.Visitors
                //                    where v.visitor_license_plate == txtLiveLPEnglish.Text
                //                    select v).FirstOrDefault();

                //    if (visitors != null)
                //    {

                //        if (visitors.visitor_classification.Contains("EMPLOYEE"))
                //        {
                //            tmFOD.Stop();
                //            lblFODError.Text = "FOD not needed for employee";
                //            //  Global.ShowMessage(json.SelectToken("error").ToString() + ":Unable to run FOD,Please check");
                //            Application.DoEvents();
                //            GC.Collect();
                //            imgLoadFOD.Visible = false;
                //            Application.DoEvents();
                //            this.btnRecordingOn.Visible = false;
                //            this.btnRecordingOff.Visible = true;
                //            return;
                //        }
                //        else
                //        {

                //        }
                //    }
                //    else
                //    {

                //    }
                //}
            }
            else
            {

                if (File.Exists(m_IVISSRecordingPath + "\\_high.jpg"))
                {
                    tmFOD.Stop();
                    lblFODError.Text = "FOD already generated for this record";
                    //  Global.ShowMessage(json.SelectToken("error").ToString() + ":Unable to run FOD,Please check");
                    Application.DoEvents();
                    GC.Collect();
                    imgLoadFOD.Visible = false;
                    Application.DoEvents();
                    this.btnRecordingOn.Visible = false;
                    this.btnRecordingOff.Visible = true;

                    Application.DoEvents();
                    GC.Collect();
                    Application.DoEvents();
                    return;
                }
            }

            Task t = Task.Run(() =>
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        try
                        {
                            if (referenceimage != "")
                                this.LoadImageComparison(System.IO.Path.GetDirectoryName(referenceimage));
                            else
                                pBoxStitch.Image = null;
                        }
                        catch (Exception)
                        {


                        }


                        fod_api f = new fod_api();
                        String jsonString = f.fod_process(referenceimage, caseimage);

                        try
                        {
                            JObject json = JObject.Parse(jsonString);
                            if (json.SelectToken("high").ToString() != "200")
                            {
                                string base64High = json.SelectToken("high").ToString();
                                string base64Medium = json.SelectToken("medium").ToString();
                                string base64Low = json.SelectToken("low").ToString();

                                byte[] returnedbytesHigh = System.Convert.FromBase64String(base64High);

                                Image imgHigh = ConvertByteArrayToImage(returnedbytesHigh);


                                byte[] returnedbytesMedium = System.Convert.FromBase64String(base64Medium);

                                Image imgMedium = ConvertByteArrayToImage(returnedbytesMedium);


                                byte[] returnedbytesLow = System.Convert.FromBase64String(base64Low);

                                Image imgLow = ConvertByteArrayToImage(returnedbytesLow);

                                if (m_DestinationDir != null)
                                    m_IVISSRecordingPath = m_DestinationDir;

                                //Bitmap newBitmap;
                                string highpath = m_IVISSRecordingPath + "\\_high.jpg";
                                string mediumpath = m_IVISSRecordingPath + "\\_medium.jpg";
                                string lowpath = m_IVISSRecordingPath + "\\_low.jpg";


                                using (var ms = new MemoryStream(returnedbytesHigh))
                                {
                                    Bitmap bmp = new Bitmap(ms);

                                    bmp.Save(highpath, ImageFormat.Jpeg);

                                }

                                using (var ms = new MemoryStream(returnedbytesMedium))
                                {
                                    Bitmap bmp = new Bitmap(ms);
                                    bmp.Save(mediumpath, ImageFormat.Jpeg);

                                }

                                using (var ms = new MemoryStream(returnedbytesLow))
                                {
                                    Bitmap bmp = new Bitmap(ms);
                                    bmp.Save(lowpath, ImageFormat.Jpeg);

                                }










                                // imgLow.Save(lowpath, System.Drawing.Imaging.ImageFormat.Jpeg);

                                //using (Image newImage = Image.FromStream(memoryStream))
                                //{
                                //    pBoxComparison.Image = new Bitmap(newImage);
                                Application.DoEvents();
                                GC.Collect();
                                imgLoadFOD.Visible = false;
                                Application.DoEvents();
                                this.btnRecordingOn.Visible = false;
                                this.btnRecordingOff.Visible = true;
                                var from = dtFrom;
                                var current_time = DateTime.Now;
                                // lblFODError.Text = dtFrom + "----" + current_time;
                                //}
                            }

                            else
                            {
                                tmFOD.Stop();
                                lblFODError.Text = "Unable to run FOD With Errorcode=200";
                                m_IVISSRecordingPath = "";
                                //  Global.ShowMessage(json.SelectToken("error").ToString() + ":Unable to run FOD,Please check");
                                Application.DoEvents();
                                GC.Collect();
                                imgLoadFOD.Visible = false;
                                Application.DoEvents();
                                this.btnRecordingOn.Visible = false;
                                this.btnRecordingOff.Visible = true;

                            }
                            // dynamic storydata = JsonConvert.DeserializeObject(jsonString);




                        }
                        catch (Exception ex)
                        {
                            tmFOD.Stop();
                            lblFODError.Text = "Unable to run FOD with inner exception";
                            m_IVISSRecordingPath = "";
                            // Global.ShowMessage(ex.Message);
                            Application.DoEvents();
                            GC.Collect();
                            imgLoadFOD.Visible = false;
                            Application.DoEvents();
                            this.btnRecordingOn.Visible = false;
                            this.btnRecordingOff.Visible = true;
                        }

                        //using (Py.GIL())
                        //{
                        //    //var converter = new NewMyDictConverter();
                        //    dynamic fod = Py.Import("fod_python.bin.run_FOD");
                        //    string jsonString = fod.start(referenceimage, caseimage);
                        //    try
                        //    {
                        //        JObject json = JObject.Parse(jsonString);
                        //        if (json.SelectToken("error").ToString() == "200")
                        //        {
                        //            string base64High = json.SelectToken("high").ToString();
                        //            string base64Medium = json.SelectToken("medium").ToString();
                        //            string base64Low = json.SelectToken("low").ToString();

                        //            byte[] returnedbytesHigh = System.Convert.FromBase64String(base64High);

                        //            Image imgHigh = ConvertByteArrayToImage(returnedbytesHigh);


                        //            byte[] returnedbytesMedium = System.Convert.FromBase64String(base64Medium);

                        //            Image imgMedium = ConvertByteArrayToImage(returnedbytesMedium);


                        //            byte[] returnedbytesLow = System.Convert.FromBase64String(base64Low);

                        //            Image imgLow = ConvertByteArrayToImage(returnedbytesLow);

                        //            // if (m_IVISSRecordingPath== "C:\\IVISSTemp")                          
                        //            m_IVISSRecordingPath = m_DestinationDir;

                        //            //Bitmap newBitmap;
                        //            string highpath = GetFilePath() + "\\_high.jpg";
                        //            string mediumpath = GetFilePath() + "\\_medium.jpg";
                        //            string lowpath = GetFilePath() + "\\_low.jpg";


                        //            using (var ms = new MemoryStream(returnedbytesHigh))
                        //            {
                        //                Bitmap bmp = new Bitmap(ms);

                        //                bmp.Save(highpath, ImageFormat.Jpeg);

                        //            }

                        //            using (var ms = new MemoryStream(returnedbytesMedium))
                        //            {
                        //                Bitmap bmp = new Bitmap(ms);
                        //                bmp.Save(mediumpath, ImageFormat.Jpeg);

                        //            }

                        //            using (var ms = new MemoryStream(returnedbytesLow))
                        //            {
                        //                Bitmap bmp = new Bitmap(ms);
                        //                bmp.Save(lowpath, ImageFormat.Jpeg);

                        //            }










                        //            // imgLow.Save(lowpath, System.Drawing.Imaging.ImageFormat.Jpeg);

                        //            //using (Image newImage = Image.FromStream(memoryStream))
                        //            //{
                        //            //    pBoxComparison.Image = new Bitmap(newImage);
                        //            Application.DoEvents();

                        //            imgLoadFOD.Visible = false;
                        //            Application.DoEvents();

                        //            var from = dtFrom;
                        //            var current_time = DateTime.Now;
                        //            lblFODError.Text = dtFrom + "----" + current_time;
                        //            //}
                        //        }

                        //        else
                        //        {
                        //            tmFOD.Stop();
                        //            lblFODError.Text = "Unable to run FOD With Errorcode=200";
                        //            //  Global.ShowMessage(json.SelectToken("error").ToString() + ":Unable to run FOD,Please check");
                        //            Application.DoEvents();

                        //            imgLoadFOD.Visible = false;
                        //            Application.DoEvents();


                        //        }
                        //        // dynamic storydata = JsonConvert.DeserializeObject(jsonString);




                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        tmFOD.Stop();
                        //        lblFODError.Text = "Unable to run FOD with inner exception";
                        //        // Global.ShowMessage(ex.Message);
                        //        Application.DoEvents();

                        //        imgLoadFOD.Visible = false;
                        //        Application.DoEvents();
                        //    }

                        //    //Console.WriteLine(jsonString);

                        //}
                    }
                    catch (Exception ex)
                    {
                        tmFOD.Stop();
                        lblFODError.Text = "Unable to run FOD,Check stitched image";
                        m_IVISSRecordingPath = "";
                        Application.DoEvents();
                        GC.Collect();
                        imgLoadFOD.Visible = false;
                        Application.DoEvents();

                        this.btnRecordingOn.Visible = false;
                        this.btnRecordingOff.Visible = true;
                    }
                });

            });






            // FileInfo fi1 = new FileInfo(@"C:\Users\User\Desktop\badcase2.jpg");
            //var bytes = File.ReadAllBytes(sample_image);
            //var base64 = Convert.ToBase64String(bytes);




            //var bytes2 = ImageToByteArray(pBoxStitch.Image);
            //var base642 = Convert.ToBase64String(bytes2);

            //////#region working code
            //////var bytes = File.ReadAllBytes(sample_image);
            //////var base64 = Convert.ToBase64String(bytes);



            //////var bytes2 = File.ReadAllBytes(recordingpath);
            //////var base642 = Convert.ToBase64String(bytes2);



            //////using (var client = new HttpClient())
            //////{
            //////    client.BaseAddress = new Uri(page);
            //////    client.DefaultRequestHeaders.Accept.Clear();
            //////    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            //////    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //////    //HttpContent stringContent = new StringContent(base64attachment);


            //////    byte[] attachmentInBytes = Convert.FromBase64String(base64);
            //////    HttpContent stringContent = new ByteArrayContent(attachmentInBytes);

            //////    byte[] attachmentInBytes2 = Convert.FromBase64String(base642);
            //////    HttpContent stringContent2 = new ByteArrayContent(attachmentInBytes2);



            //////    using (var formData = new MultipartFormDataContent())
            //////    {
            //////        formData.Add(stringContent, "sample_image", "outPutVer.jpg");
            //////        formData.Add(stringContent2, "reference_image", "TempOutver.jpg");

            //////        var response = await client.PostAsync(page, formData);
            //////        if (response.IsSuccessStatusCode)
            //////        {
            //////            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");



            //////            var jsonString = await response.Content.ReadAsStringAsync();

            //////            dynamic storydata = JsonConvert.DeserializeObject(jsonString);

            //////            foreach (var d in storydata)
            //////            {
            //////                try
            //////                {
            //////                    if (d.Path == "result")
            //////                    {
            //////                        var actualdata = d.Value;

            //////                        string base64Encoded = actualdata;

            //////                        byte[] returnedbytes = System.Convert.FromBase64String(base64Encoded);

            //////                        //Bitmap newBitmap;
            //////                        using (MemoryStream memoryStream = new MemoryStream(returnedbytes))
            //////                        using (Image newImage = Image.FromStream(memoryStream))
            //////                        {
            //////                            pBoxStitch.Image = new Bitmap(newImage);
            //////                            Application.DoEvents();

            //////                            imgLoadFOD.Visible = false;
            //////                            Application.DoEvents();
            //////                        }
            //////                    }

            //////                    else if (d.Path == "error")
            //////                    {
            //////                        var actualdata = d.Value;

            //////                        Global.ShowMessage(actualdata.ToString(), false);

            //////                        Application.DoEvents();

            //////                        imgLoadFOD.Visible = false;
            //////                        Application.DoEvents();
            //////                    }
            //////                }
            //////                catch (Exception ex)
            //////                {
            //////                    Application.DoEvents();

            //////                    imgLoadFOD.Visible = false;
            //////                    Application.DoEvents();

            //////                }

            //////            }



            //////            //Bitmap newBitmap;
            //////            //using (MemoryStream memoryStream = new MemoryStream(returnedbytes))
            //////            //using (Image newImage = Image.FromStream(memoryStream))
            //////            //    pBoxStitch.Image = new Bitmap(newImage);


            //////        }
            //////        else
            //////        {
            //////            Application.DoEvents();

            //////            imgLoadFOD.Visible = false;
            //////            Application.DoEvents();
            //////        }


            //////    }
            //////}
            //////#endregion
            //FileInfo fi2 = new FileInfo(@"C:\Users\User\Desktop\case2.jpg");
            ////string fileName = fi1.Name;
            //byte[] fileContents2 = File.ReadAllBytes(fi2.FullName);



            //System.Net.Http.HttpRequestMessage request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, page);

            //MultipartFormDataContent form = new MultipartFormDataContent();
            //request.Content = form;

            //HttpContent content1 = new ByteArrayContent(fileContents1);
            //content1.Headers.ContentDisposition = new ContentDispositionHeaderValue("sample_image");
            //content1.Headers.ContentDisposition.FileName = fi1.Name;
            //form.Add(content1);

            //HttpContent content2 = new ByteArrayContent(fileContents2);
            //content2.Headers.ContentDisposition = new ContentDispositionHeaderValue("reference_image");
            //content2.Headers.ContentDisposition.FileName = fi2.Name;
            ////}



            //form.Add(content2);

            //using (HttpClient client = new HttpClient())
            //{


            //    Task<HttpResponseMessage> httpRequest = client.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            //    HttpResponseMessage httpResponse =  httpRequest.Result;


            ////    await client.SendAsync(request).ContinueWith((response) =>
            ////{
            ////    var res = response.Result;
            ////});

            //}



        }

        public async void RunFODAsyncOLD(string referenceimage, string caseimage)
        {


            //string caseimage1 = @"C:\Users\User\Desktop\badcase.jpg";
            //string referenceimage1 = @"C:\Users\User\Desktop\case1.jpg";

            tmFOD.Stop();

            Task t = Task.Run(() =>
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        try
                        {
                            if (referenceimage != "")
                                this.LoadImageComparison(System.IO.Path.GetDirectoryName(referenceimage));
                            else
                                pBoxStitch.Image = null;
                        }
                        catch (Exception)
                        {


                        }



                        using (Py.GIL())
                        {
                            //var converter = new NewMyDictConverter();
                            dynamic fod = Py.Import("fod_python.bin.run_FOD");
                            string jsonString = fod.start(referenceimage, caseimage);
                            try
                            {
                                JObject json = JObject.Parse(jsonString);
                                if (json.SelectToken("error").ToString() == "200")
                                {
                                    string base64High = json.SelectToken("high").ToString();
                                    string base64Medium = json.SelectToken("medium").ToString();
                                    string base64Low = json.SelectToken("low").ToString();

                                    byte[] returnedbytesHigh = System.Convert.FromBase64String(base64High);

                                    Image imgHigh = ConvertByteArrayToImage(returnedbytesHigh);


                                    byte[] returnedbytesMedium = System.Convert.FromBase64String(base64Medium);

                                    Image imgMedium = ConvertByteArrayToImage(returnedbytesMedium);


                                    byte[] returnedbytesLow = System.Convert.FromBase64String(base64Low);

                                    Image imgLow = ConvertByteArrayToImage(returnedbytesLow);

                                    // if (m_IVISSRecordingPath== "C:\\IVISSTemp")                          
                                    m_IVISSRecordingPath = m_DestinationDir;

                                    //Bitmap newBitmap;
                                    string highpath = GetFilePath() + "\\_high.jpg";
                                    string mediumpath = GetFilePath() + "\\_medium.jpg";
                                    string lowpath = GetFilePath() + "\\_low.jpg";


                                    using (var ms = new MemoryStream(returnedbytesHigh))
                                    {
                                        Bitmap bmp = new Bitmap(ms);

                                        bmp.Save(highpath, ImageFormat.Jpeg);

                                    }

                                    using (var ms = new MemoryStream(returnedbytesMedium))
                                    {
                                        Bitmap bmp = new Bitmap(ms);
                                        bmp.Save(mediumpath, ImageFormat.Jpeg);

                                    }

                                    using (var ms = new MemoryStream(returnedbytesLow))
                                    {
                                        Bitmap bmp = new Bitmap(ms);
                                        bmp.Save(lowpath, ImageFormat.Jpeg);

                                    }










                                    // imgLow.Save(lowpath, System.Drawing.Imaging.ImageFormat.Jpeg);

                                    //using (Image newImage = Image.FromStream(memoryStream))
                                    //{
                                    //    pBoxComparison.Image = new Bitmap(newImage);
                                    Application.DoEvents();

                                    imgLoadFOD.Visible = false;
                                    Application.DoEvents();


                                    //}
                                }

                                else
                                {
                                    tmFOD.Stop();
                                    lblFODError.Text = "Unable to run FOD With Errorcode=200";
                                    //  Global.ShowMessage(json.SelectToken("error").ToString() + ":Unable to run FOD,Please check");
                                    Application.DoEvents();

                                    imgLoadFOD.Visible = false;
                                    Application.DoEvents();


                                }
                                // dynamic storydata = JsonConvert.DeserializeObject(jsonString);




                            }
                            catch (Exception ex)
                            {
                                tmFOD.Stop();
                                lblFODError.Text = "Unable to run FOD with inner exception";
                                // Global.ShowMessage(ex.Message);
                                Application.DoEvents();

                                imgLoadFOD.Visible = false;
                                Application.DoEvents();
                            }

                            //Console.WriteLine(jsonString);

                        }
                    }
                    catch (Exception ex)
                    {
                        tmFOD.Stop();
                        lblFODError.Text = "Unable to run FOD,Check stitched image";
                        //Global.ShowMessage(ex.Message);
                        imgLoadFOD.Visible = false;
                        Application.DoEvents();
                    }
                });
            });






            // FileInfo fi1 = new FileInfo(@"C:\Users\User\Desktop\badcase2.jpg");
            //var bytes = File.ReadAllBytes(sample_image);
            //var base64 = Convert.ToBase64String(bytes);




            //var bytes2 = ImageToByteArray(pBoxStitch.Image);
            //var base642 = Convert.ToBase64String(bytes2);

            //////#region working code
            //////var bytes = File.ReadAllBytes(sample_image);
            //////var base64 = Convert.ToBase64String(bytes);



            //////var bytes2 = File.ReadAllBytes(recordingpath);
            //////var base642 = Convert.ToBase64String(bytes2);



            //////using (var client = new HttpClient())
            //////{
            //////    client.BaseAddress = new Uri(page);
            //////    client.DefaultRequestHeaders.Accept.Clear();
            //////    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            //////    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //////    //HttpContent stringContent = new StringContent(base64attachment);


            //////    byte[] attachmentInBytes = Convert.FromBase64String(base64);
            //////    HttpContent stringContent = new ByteArrayContent(attachmentInBytes);

            //////    byte[] attachmentInBytes2 = Convert.FromBase64String(base642);
            //////    HttpContent stringContent2 = new ByteArrayContent(attachmentInBytes2);



            //////    using (var formData = new MultipartFormDataContent())
            //////    {
            //////        formData.Add(stringContent, "sample_image", "outPutVer.jpg");
            //////        formData.Add(stringContent2, "reference_image", "TempOutver.jpg");

            //////        var response = await client.PostAsync(page, formData);
            //////        if (response.IsSuccessStatusCode)
            //////        {
            //////            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");



            //////            var jsonString = await response.Content.ReadAsStringAsync();

            //////            dynamic storydata = JsonConvert.DeserializeObject(jsonString);

            //////            foreach (var d in storydata)
            //////            {
            //////                try
            //////                {
            //////                    if (d.Path == "result")
            //////                    {
            //////                        var actualdata = d.Value;

            //////                        string base64Encoded = actualdata;

            //////                        byte[] returnedbytes = System.Convert.FromBase64String(base64Encoded);

            //////                        //Bitmap newBitmap;
            //////                        using (MemoryStream memoryStream = new MemoryStream(returnedbytes))
            //////                        using (Image newImage = Image.FromStream(memoryStream))
            //////                        {
            //////                            pBoxStitch.Image = new Bitmap(newImage);
            //////                            Application.DoEvents();

            //////                            imgLoadFOD.Visible = false;
            //////                            Application.DoEvents();
            //////                        }
            //////                    }

            //////                    else if (d.Path == "error")
            //////                    {
            //////                        var actualdata = d.Value;

            //////                        Global.ShowMessage(actualdata.ToString(), false);

            //////                        Application.DoEvents();

            //////                        imgLoadFOD.Visible = false;
            //////                        Application.DoEvents();
            //////                    }
            //////                }
            //////                catch (Exception ex)
            //////                {
            //////                    Application.DoEvents();

            //////                    imgLoadFOD.Visible = false;
            //////                    Application.DoEvents();

            //////                }

            //////            }



            //////            //Bitmap newBitmap;
            //////            //using (MemoryStream memoryStream = new MemoryStream(returnedbytes))
            //////            //using (Image newImage = Image.FromStream(memoryStream))
            //////            //    pBoxStitch.Image = new Bitmap(newImage);


            //////        }
            //////        else
            //////        {
            //////            Application.DoEvents();

            //////            imgLoadFOD.Visible = false;
            //////            Application.DoEvents();
            //////        }


            //////    }
            //////}
            //////#endregion
            //FileInfo fi2 = new FileInfo(@"C:\Users\User\Desktop\case2.jpg");
            ////string fileName = fi1.Name;
            //byte[] fileContents2 = File.ReadAllBytes(fi2.FullName);



            //System.Net.Http.HttpRequestMessage request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, page);

            //MultipartFormDataContent form = new MultipartFormDataContent();
            //request.Content = form;

            //HttpContent content1 = new ByteArrayContent(fileContents1);
            //content1.Headers.ContentDisposition = new ContentDispositionHeaderValue("sample_image");
            //content1.Headers.ContentDisposition.FileName = fi1.Name;
            //form.Add(content1);

            //HttpContent content2 = new ByteArrayContent(fileContents2);
            //content2.Headers.ContentDisposition = new ContentDispositionHeaderValue("reference_image");
            //content2.Headers.ContentDisposition.FileName = fi2.Name;
            ////}



            //form.Add(content2);

            //using (HttpClient client = new HttpClient())
            //{


            //    Task<HttpResponseMessage> httpRequest = client.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            //    HttpResponseMessage httpResponse =  httpRequest.Result;


            ////    await client.SendAsync(request).ContinueWith((response) =>
            ////{
            ////    var res = response.Result;
            ////});

            //}



        }

        private string GetDefaultOfSameVehicle(string licenseplate)
        {
            return presenter.GetDefaultOfSameVehicle(licenseplate);
        }

        private void RunBatchFile()
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.Arguments = @"& '" + Application.StartupPath + "\\runfodscript.ps1'";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            //Assert.IsTrue(output.Contains("StringToBeVerifiedInAUnitTest"));

            string errors = process.StandardError.ReadToEnd();
            //Assert.IsTrue(string.IsNullOrEmpty(errors));
        }

        private void btnAFOD_Click(object sender, EventArgs e)
        {
            string path = GetDefaultOfSameVehicle(txtLiveLPEnglish.Text);

            if (path == "")
            {
                Global.ShowMessage("FOD can not be run against this image", false);

                return;
            }





            string recordingpath = "";
            if (pBoxComparison.Image != null)
            {
                try
                {
                    // pBoxStitch.Image.Save(Global.TEMP_FOLDER + "\\TempOutver.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                    using (Bitmap bm = new Bitmap(pBoxComparison.Image))
                    {
                        bm.Save(@Global.TEMP_FOLDER + "\\TempOutver.jpg", ImageFormat.Jpeg);
                    }

                    recordingpath = Global.TEMP_FOLDER + "\\TempOutver.jpg";
                }
                catch (Exception ex)
                {

                    recordingpath = "";
                }


                if (recordingpath == "")
                {
                    Global.ShowMessage("Could not find outputver.jpg,FOD can not be run against this image", false);

                    return;
                }
            }
            else

            {
                Global.ShowMessage("Could not find outputver.jpg,FOD can not be run against this image", false);

                return;
            }



            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://127.0.0.1:5000/health");
            //request.Method = "GET";
            //String test = String.Empty;
            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    Stream dataStream = response.GetResponseStream();
            //    StreamReader reader = new StreamReader(dataStream);
            //    test = reader.ReadToEnd();
            //    reader.Close();
            //    dataStream.Close();

            //    if (test.Contains("false"))
            //    {
            //        Global.ShowMessage("FOD service not running, restarting server..Please wait", false);

            //        RunBatchFile();
            //    }
            //}
            imgLoadFOD.Visible = true;
            Application.DoEvents();
            imgLoadFOD.Image = Image.FromFile("loadingnew.gif");

            Application.DoEvents();

            string referenceimage = path + @"\outPutVer.jpg";

            using (FileStream fs = new FileStream(recordingpath, FileMode.Open))
            {
                imgCompositeImageBrightness = Image.FromStream(fs);

                fs.Close();
            }
            lblFODError.Text = "";
            Application.DoEvents();
            GC.Collect();
            Application.DoEvents();


            RunFODAsync(referenceimage, recordingpath, true);



            return;
            try
            {
                var defImage = presenter.SelectImageComparison().Replace(@"\", @"\\") + @"\\";
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
            catch (Exception)
            {


            }

        }


        private string GetFilePath()
        {

            return (String.IsNullOrEmpty(m_IVISSRecordingPath)) ? "C:\\IVISSTemp" : m_IVISSRecordingPath;
        }

        private void dgView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            //if (e.Column.HeaderText != "Plate Number")
            //{
            //    e.Column.HeaderCell.Style.Font = new Font(dgView.Font.FontFamily.Name, e.Column.Width / 15, dgView.Font.Style, dgView.Font.Unit);
            //}
            //else
            //{
            //    e.Column.HeaderCell.Style.Font = new Font(dgView.Font.FontFamily.Name, e.Column.Width / 26, dgView.Font.Style, dgView.Font.Unit);
            //}

        }

        private void pic_MouseHover(object sender, EventArgs e)
        {
            ((PictureBox)sender).BorderStyle = BorderStyle.FixedSingle;
            // ((PictureBox)sender).Cursor= Cursors.Hand;
        }

        private void TableLayoutPanelSettings_Paint(object sender, PaintEventArgs e)
        {

        }

        private void metroPanel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pic_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).BorderStyle = BorderStyle.None;
            //((PictureBox)sender).Cursor = Cursors.No;
        }


        #endregion

        #region ThemeSelection

        private void btnIndigo_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Indigo";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");

            _skinmanager.ColorScheme = new ColorScheme(Primary.Indigo400, Primary.Indigo400, Primary.BlueGrey500, Accent.Indigo400, TextShade.WHITE);
            Global.CurrentTheme = "Indigo";
            ChangeThemeColors();
        }

        private void btnPurple_Click(object sender, EventArgs e)
        {

            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Purple";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");

            _skinmanager.ColorScheme = new ColorScheme(Primary.Purple400, Primary.Purple300, Primary.Purple400, Accent.Purple400, TextShade.WHITE);
            Global.CurrentTheme = "Purple";
            ChangeThemeColors();
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Green";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");

            _skinmanager.ColorScheme = new ColorScheme(Primary.Green400, Primary.Green400, Primary.Green400, Accent.Green400, TextShade.WHITE);
            Global.CurrentTheme = "Green";
            ChangeThemeColors();
        }

        private void btnInfoDark_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "InfoDark";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.InfoDark, Primary.InfoDark, Primary.InfoDark, Accent.Blue200, TextShade.WHITE);
            Global.CurrentTheme = "InfoDark";
            ChangeThemeColors();
        }

        private void btnRed_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Red";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.Red300, Primary.Red300, Primary.Red300, Accent.Red400, TextShade.WHITE);
            Global.CurrentTheme = "Red";
            ChangeThemeColors();
        }

        private void btnOrange_Click(object sender, EventArgs e)
        {

            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Orange";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.DeepOrange300, Primary.DeepOrange200, Primary.DeepOrange300, Accent.Orange400, TextShade.WHITE);
            Global.CurrentTheme = "Orange";
            ChangeThemeColors();

        }

        private void btnYellow_Click(object sender, EventArgs e)
        {

            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "LightBlue";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.LightBlue600, Primary.LightBlue600, Primary.LightBlue900, Accent.Blue700, TextShade.WHITE);
            Global.CurrentTheme = "LightBlue";
            ChangeThemeColors();
        }

        private void btnDarkGray_Click(object sender, EventArgs e)
        {

            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "DarkGrey";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.Grey600, Primary.Grey600, Primary.Grey600, Accent.Blue700, TextShade.WHITE);
            Global.CurrentTheme = "DarkGrey";
            ChangeThemeColors();
        }

        private void btnGray_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Grey";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.Grey500, Primary.Grey400, Primary.Grey600, Accent.Blue700, TextShade.WHITE);
            Global.CurrentTheme = "Grey";
            ChangeThemeColors();
        }

        private void btnTeal_Click(object sender, EventArgs e)
        {

            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Teal";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");

            _skinmanager.ColorScheme = new ColorScheme(Primary.Teal500, Primary.Teal500, Primary.Teal500, Accent.Teal700, TextShade.WHITE);
            Global.CurrentTheme = "Teal";
            ChangeThemeColors();
        }

        private void panel1_Click(object sender, EventArgs e)
        {

            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "BlueGrey";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");

            _skinmanager.ColorScheme = new ColorScheme(Primary.BlueGrey700, Primary.BlueGrey600, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            Global.CurrentTheme = "BlueGrey";
            ChangeThemeColors();
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Default";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");

            rdoBoth.Checked = true;
            _skinmanager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            Global.CurrentTheme = "Default";
            ChangeThemeColors();
        }

        private void btnCyan_Click(object sender, EventArgs e)
        {
            dtTheme.Rows.Clear();
            DataRow drow = dtTheme.NewRow();
            drow["Theme"] = "Cyan";

            if (rdoBoth.Checked == true)
            {
                drow["Language"] = "ArabicAndEnglish";
            }
            else
            {
                drow["Language"] = "English";
            }
            dtTheme.Rows.Add(drow);
            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");
            _skinmanager.ColorScheme = new ColorScheme(Primary.Cyan400, Primary.Cyan400, Primary.Cyan400, Accent.Cyan400, TextShade.WHITE);
            Global.CurrentTheme = "Cyan";
            ChangeThemeColors();
        }

        private void UpdateTheme(string Filename)
        {
            dtTheme.Clear();

            dtTheme.ReadXml(Filename);

            if (dtTheme.Rows.Count > 0)
            {

                var ThemeName = dtTheme.Rows[0]["Theme"].ToString();
                var Language = dtTheme.Rows[0]["Language"].ToString();

                Global.ProfileMode = Language;
                if (ThemeName == "Indigo")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Indigo400, Primary.Indigo400, Primary.BlueGrey500, Accent.Indigo400, TextShade.WHITE);
                    Global.CurrentTheme = "Indigo";

                }
                if (ThemeName == "Purple")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Purple400, Primary.Purple300, Primary.Purple400, Accent.Purple400, TextShade.WHITE);
                    Global.CurrentTheme = "Purple";


                }
                if (ThemeName == "Green")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Green400, Primary.Green400, Primary.Green400, Accent.Green400, TextShade.WHITE);
                    Global.CurrentTheme = "Green";
                }

                if (ThemeName == "InfoDark")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.InfoDark, Primary.InfoDark, Primary.InfoDark, Accent.Blue200, TextShade.WHITE);
                    Global.CurrentTheme = "InfoDark";
                }

                if (ThemeName == "Red")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Red300, Primary.Red300, Primary.Red300, Accent.Red400, TextShade.WHITE);
                    Global.CurrentTheme = "Red";
                }

                if (ThemeName == "Orange")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.DeepOrange300, Primary.DeepOrange200, Primary.DeepOrange300, Accent.Orange400, TextShade.WHITE);
                    Global.CurrentTheme = "Orange";
                }

                if (ThemeName == "LightBlue")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.LightBlue600, Primary.LightBlue600, Primary.LightBlue900, Accent.Blue700, TextShade.WHITE);
                    Global.CurrentTheme = "LightBlue";
                }

                if (ThemeName == "DarkGrey")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Grey600, Primary.Grey600, Primary.Grey600, Accent.Blue700, TextShade.WHITE);
                    Global.CurrentTheme = "DarkGrey";
                }

                if (ThemeName == "Grey")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Grey500, Primary.Grey400, Primary.Grey600, Accent.Blue700, TextShade.WHITE);
                    Global.CurrentTheme = "Grey";
                }

                if (ThemeName == "Teal")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Teal500, Primary.Teal500, Primary.Teal500, Accent.Teal700, TextShade.WHITE);
                    Global.CurrentTheme = "Teal";
                }

                if (ThemeName == "BlueGrey")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.BlueGrey700, Primary.BlueGrey600, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
                    Global.CurrentTheme = "BlueGrey";
                }

                if (ThemeName == "Default")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
                    Global.CurrentTheme = "Default";
                }

                if (ThemeName == "Cyan")
                {
                    _skinmanager.ColorScheme = new ColorScheme(Primary.Cyan400, Primary.Cyan400, Primary.Cyan400, Accent.Cyan400, TextShade.WHITE);
                    Global.CurrentTheme = "Cyan";
                }







            }
        }

        private void ChangeThemeColors()
        {
            dtTheme.Clear();


            if (Global.CurrentTheme != "")
            {

                var ThemeName = Global.CurrentTheme;


                if (ThemeName == "Indigo")
                {
                    UpdateColorControls(this, "#3949AB", MetroFramework.MetroColorStyle.Blue);


                }
                if (ThemeName == "Purple")
                {
                    UpdateColorControls(this, "#9400D3", MetroFramework.MetroColorStyle.Purple);



                }
                if (ThemeName == "Green")
                {
                    UpdateColorControls(this, "#388E3C", MetroFramework.MetroColorStyle.Green);

                }

                if (ThemeName == "InfoDark")
                {

                    UpdateColorControls(this, "#0099CC", MetroFramework.MetroColorStyle.Blue);


                }

                if (ThemeName == "Red")
                {

                    UpdateColorControls(this, "#D50000", MetroFramework.MetroColorStyle.Red);


                }

                if (ThemeName == "Orange")
                {

                    UpdateColorControls(this, "#FF8A65", MetroFramework.MetroColorStyle.Orange);


                }

                if (ThemeName == "LightBlue")
                {

                    UpdateColorControls(this, "#0099CC", MetroFramework.MetroColorStyle.Blue);

                }

                if (ThemeName == "DarkGrey")
                {
                    UpdateColorControls(this, "#212121", MetroFramework.MetroColorStyle.Black);


                }

                if (ThemeName == "BlueGrey")
                {

                    UpdateColorControls(this, "#0099CC", MetroFramework.MetroColorStyle.Blue);


                }



                if (ThemeName == "Grey")
                {
                    UpdateColorControls(this, "#212121", MetroFramework.MetroColorStyle.Black);

                }

                if (ThemeName == "Teal")
                {
                    UpdateColorControls(this, "#388E3C", MetroFramework.MetroColorStyle.Green);


                }

                if (ThemeName == "LightGrey")
                {
                    UpdateColorControls(this, "#212121", MetroFramework.MetroColorStyle.Black);
                }

                if (ThemeName == "Default")
                {
                    UpdateColorControls(this, "#0099CC", MetroFramework.MetroColorStyle.Blue);

                }

                if (ThemeName == "Cyan")
                {
                    UpdateColorControls(this, "#0099CC", MetroFramework.MetroColorStyle.Blue);
                }


                if (Global.ProfileMode == "English")
                    SetEnglishProfile();
                else
                    SetBothlishProfile();



            }
        }



        public void UpdateColorControls(Control myControl, string HexCode, MetroFramework.MetroColorStyle MetroColor)
        {
            dividerImageType.BackColor = System.Drawing.ColorTranslator.FromHtml(HexCode);
            lblEntry.ForeColor = System.Drawing.ColorTranslator.FromHtml(HexCode);

            foreach (Control subC in myControl.Controls)
            {

                UpdateColorControls(subC, HexCode, MetroColor);


            }

            if (myControl is MetroFramework.Controls.MetroToggle)
            {
                MetroFramework.Controls.MetroToggle toggle = (MetroFramework.Controls.MetroToggle)myControl;
                toggle.Style = MetroColor;
            }
            if (myControl is DataGridView)
            {
                DataGridView MyDgv = (DataGridView)myControl;
                MyDgv.DefaultCellStyle.SelectionBackColor = System.Drawing.ColorTranslator.FromHtml(HexCode);
            }

            TabMain.Style = MetroColor;

        }

        private void rdoEnglish_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoEnglish.Checked)
            {
                SetEnglishProfile();
            }
        }

        private void rdoBoth_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoBoth.Checked)
            {
                SetBothlishProfile();
            }
        }

        private void SetEnglishProfile()
        {

            Global.ProfileMode = "English";

            //Dashboard tab
            txtLiveLPArabic.Visible = false;
            lblLiveLPArabic.Visible = false;
            lblRelay1Arab.Visible = false;
            lblRelay2Arab.Visible = false;
            lblRelay3Arab.Visible = false;
            lblRelay4Arab.Visible = false;
            dgView.Columns["ArabicLicense"].Visible = false;

            //Settings tab

            pnlArabicCaptionSettings.Visible = false;
            lblLicensePlateArab.Visible = false;
            txtLicensePlateArab.Visible = false;
            txtLPNumArabic.Visible = false;

            //Search tab
            lblLicenseArabicSearchRecords.Visible = false;
            txtLicenseArabicSearchRecords.Visible = false;
            dgSearchRecords.Columns["Arabic"].Visible = false;

            dtTheme.Clear();

            dtTheme.ReadXml(Application.StartupPath + "\\Theme.xml");

            dtTheme.Rows[0].BeginEdit();

            dtTheme.Rows[0]["Language"] = "English";

            dtTheme.Rows[0].EndEdit();

            dtTheme.AcceptChanges();




            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");


        }


        private void SetBothlishProfile()
        {

            Global.ProfileMode = "Both";

            //Dashboard tab
            txtLiveLPArabic.Visible = true;
            lblLiveLPArabic.Visible = true;
            lblRelay1Arab.Visible = true;
            lblRelay2Arab.Visible = true;
            lblRelay3Arab.Visible = true;
            lblRelay4Arab.Visible = true;
            dgView.Columns["ArabicLicense"].Visible = true;

            //Settings tab

            pnlArabicCaptionSettings.Visible = true;
            lblLicensePlateArab.Visible = true;
            txtLicensePlateArab.Visible = true;
            txtLPNumArabic.Visible = true;

            //Search tab
            lblLicenseArabicSearchRecords.Visible = true;
            txtLicenseArabicSearchRecords.Visible = true;
            dgSearchRecords.Columns["Arabic"].Visible = true;

            dtTheme.Clear();

            dtTheme.ReadXml(Application.StartupPath + "\\Theme.xml");

            dtTheme.Rows[0].BeginEdit();

            dtTheme.Rows[0]["Language"] = "Both";

            dtTheme.Rows[0].EndEdit();

            dtTheme.AcceptChanges();




            dtTheme.WriteXml(Application.StartupPath + "\\Theme.xml");


        }



        #endregion

        #region Settings
        private void txtRelay1Port_TextChanged(object sender, EventArgs e)
        {
            txtRelay1ArabicPort.Text = txtRelay1Port.Text;
        }


        private void txtRelay2Port_TextChanged(object sender, EventArgs e)
        {
            txtRelay2ArabicPort.Text = txtRelay2Port.Text;
        }

        private void txtRelay3Port_TextChanged(object sender, EventArgs e)
        {
            txtRelay3ArabicPort.Text = txtRelay3Port.Text;
        }

        private void txtRelay4Port_TextChanged(object sender, EventArgs e)
        {
            txtRelay4ArabicPort.Text = txtRelay4Port.Text;
        }



        public string Relay1CaptionEnglish { get => this.txtRelay1Caption.Text; set => this.txtRelay1Caption.Text = value; }
        public string Relay1CaptionArabic { get => this.txtRelay1CaptionArab.Text; set => this.txtRelay1CaptionArab.Text = value; }
        public string Relay1Port
        {
            get => this.txtRelay1Port.Text;
            set => this.txtRelay1Port.Text = value;
        }
        public string Relay2CaptionEnglish { get => this.txtRelay2Caption.Text; set => this.txtRelay2Caption.Text = value; }
        public string Relay2CaptionArabic { get => this.txtRelay2CaptionArab.Text; set => this.txtRelay2CaptionArab.Text = value; }
        public string Relay2Port { get => this.txtRelay2Port.Text; set => this.txtRelay2Port.Text = value; }
        public string Relay3CaptionEnglish { get => this.txtRelay3Caption.Text; set => this.txtRelay3Caption.Text = value; }
        public string Relay3CaptionArabic { get => this.txtRelay3CaptionArab.Text; set => this.txtRelay3CaptionArab.Text = value; }
        public string Relay3Port { get => this.txtRelay3Port.Text; set => this.txtRelay3Port.Text = value; }
        public string Relay4CaptionEnglish { get => this.txtRelay4Caption.Text; set => this.txtRelay4Caption.Text = value; }
        public string Relay4CaptionArabic { get => this.txtRelay4CaptionArab.Text; set => this.txtRelay4CaptionArab.Text = value; }
        public string Relay4Port { get => this.txtRelay4Port.Text; set => this.txtRelay4Port.Text = value; }
        public string AlprIP { get => this.txtEntryLPIP.Text; set => this.txtEntryLPIP.Text = value; }
        public string DriverIP { get => this.txtEntryDriverIP.Text; set => this.txtEntryDriverIP.Text = value; }
        public string SceneIP { get => this.txtEntrySceneIP.Text; set => this.txtEntrySceneIP.Text = value; }
        public string DriverCamPassword { get => this.txtDriverCamPassword.Text; set => this.txtDriverCamPassword.Text = value; }
        public string SceneCamPassword { get => this.txtSceneCamPassword.Text; set => this.txtSceneCamPassword.Text = value; }
        public string GateName { get => this.txtGateName.Text; set => this.txtGateName.Text = value; }
        public string ComPort { get => this.txtIPAddress.Text; set => this.txtIPAddress.Text = value; }
        public bool ALPRLoop { get => this.ALPREntryLoopToggle.Checked; set => this.ALPREntryLoopToggle.Checked = value; }

        public bool AIEnabled
        {
            get => this.chkAI.Checked;

            set => this.chkAI.Checked = value;
        }

        public string IPAddress { get => this.txtIPAddress.Text; set => this.txtIPAddress.Text = value; }
        public string ListenPort { get => this.txtListenPort.Text; set => this.txtListenPort.Text = value; }

        public MaterialForm systemSettingForm { get => this; set => throw new NotImplementedException(); }



        public event EventHandler BtnUpdate;

        private void btnSettingsClear_Click(object sender, EventArgs e)
        {
            frmSystemSettings sett = new frmSystemSettings();
            sett.ShowDialog();
        }

        private void TabSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabSettings.SelectedTab.Name == "tpSettings_Settings")
            {
                if (settingspresenter == null)
                {
                    settingspresenter = new SystemSettingsPresenter(this);

                }
            }
            if (TabSettings.SelectedTab.Name == "tpSettings_UserManagement")
            {
                if (userpresenter == null)
                {
                    userpresenter = new UserManagementPresenter(this);
                    FormLoadUser(sender, e);
                }

            }

            if (TabSettings.SelectedTab.Name == "tpSettings_Report")
            {
                if (reportspresenter == null)
                {
                    reportspresenter = new ReportsPresenter(this);
                    lblDateFromDisabled.Text = DateTime.Now.Date.ToString("MM/dd/yyyy");
                    lblDateToDisabled.Text = DateTime.Now.Date.ToString("MM/dd/yyyy");
                    lblTimeFromDisabled.Text = DateTime.Now.ToString("hh:mm:ss tt");
                    lblTimeToDisabled.Text = DateTime.Now.ToString("hh:mm:ss tt");
                }

            }


        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabMain.SelectedTab.Name == "tpSettings")
            {
                if (settingspresenter == null)
                    settingspresenter = new SystemSettingsPresenter(this);


            }
            if (TabMain.SelectedTab.Name == "tpSearch")
            {
                if (searchrecordpresenter == null)
                {
                    searchrecordpresenter = new SearchRecordsPresenter(this);

                    if (Global.DEMO)
                    {
                        pBoxStitchSearchRecords.Image = Image.FromFile(@"C:\IVISSTemp\outPutVer.jpg");

                        this.txtLicenseSearchRecords.Text = "6RBZ328";
                    }

                    btnStitch.Visible = false;
                    txtLicenseSearchRecords.Focus();
                    if (chkLicensePlateSearchRecords.Checked)
                        isLicensePlateSearchRecords = true;

                    lblFromSearchRecords.Text = DateTime.Now.Date.ToString("MM/dd/yyyy");
                    lblToSearchRecords.Text = DateTime.Now.Date.ToString("MM/dd/yyyy");
                    lblTimeFromSearchRecords.Text = DateTime.Now.ToString("hh:mm:ss tt");
                    lblTimeToSearchRecords.Text = DateTime.Now.ToString("hh:mm:ss tt");
                }

            }








        }

        private void btnSettingsUpdate_Click(object sender, EventArgs e)
        {
            BtnUpdate(sender, e);
        }





        #endregion

        #region UserManagement

        public event EventHandler BtnSave;
        public event EventHandler BtnReset;
        public event EventHandler BtnDelete;
        public event EventHandler RdoManager;
        public event EventHandler RdoGuard;
        public event EventHandler FormLoadUser;

        UserManagementPresenter userpresenter;

        private string m_SelectedID = string.Empty;

        public string firstName { set { this.txtFirstName.Text = value; } get { return this.txtFirstName.Text; } }
        public string middleName
        {
            set { this.txtMiddleName.Text = value; }
            get { return this.txtMiddleName.Text; }
        }
        public string lastName
        {
            set { this.txtLastName.Text = value; }
            get { return this.txtLastName.Text; }
        }
        public string phone
        {
            set { this.txtPhone.Text = value; }
            get { return this.txtPhone.Text; }
        }
        public string id
        {
            set { this.txtID.Text = value; }
            get { return this.txtID.Text; }
        }
        public string password
        {
            set { this.txtPassword.Text = value; }
            get { return this.txtPassword.Text; }
        }

        public string selectedID
        {
            set { this.m_SelectedID = value; }
            get { return this.m_SelectedID; }
        }

        public bool managerChecked
        {
            set { this.rdoManager.Checked = value; }
            get { return this.rdoManager.Checked; }
        }

        public bool guardChecked
        {
            set { this.rdoGuard.Checked = value; }
            get { return this.rdoGuard.Checked; }
        }




        public MaterialForm userManagementForm { get => this; set => throw new NotImplementedException(); }

        private void btnSaveUserManagement_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtFirstName.Text == "" || txtPassword.Text == "" || txtID.Text == "")
                {
                    Global.ShowMessage("Please enter required fields", false);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                BtnSave(sender, e);
            }
            catch { }
            finally { this.Cursor = Cursors.Default; }
        }

        private void btnUserManagementClear_Click(object sender, EventArgs e)
        {

            ResetUser();


        }

        public void ResetUser()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                btnSaveUserManagement.Text = "SAVE";
                lblCurrentUserMode.Text = "Current Mode: New User";

                this.txtFirstName.Clear();
                this.txtMiddleName.Clear();
                this.txtLastName.Clear();

                this.txtPhone.Clear();
                this.txtID.Clear();
                this.txtPassword.Clear();
            });


        }

        private void btnNewUser_Click(object sender, EventArgs e)
        {
            ResetUser();
            txtFirstName.Focus();
        }


        private void btnUserManagementDelete_Click(object sender, EventArgs e)
        {

            BtnDelete(sender, e);
        }

        public string saveBtnCaption
        {
            set { this.btnSaveUserManagement.Text = value; }
            get { return this.btnSaveUserManagement.Text; }
        }


        private void rdoManager_CheckedChanged(object sender, EventArgs e)
        {
            RdoManager(sender, e);
        }

        public void SetGridSource(DataTable dt)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                dgGuards.DataSource = dt;
            });
        }

        private void dgGuards_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    m_SelectedID = this.txtID.Text = dgGuards.Rows[e.RowIndex].Cells[0].Value.ToString();
                    this.txtFirstName.Text = dgGuards.Rows[e.RowIndex].Cells[1].Value.ToString();
                    this.txtMiddleName.Text = dgGuards.Rows[e.RowIndex].Cells[2].Value.ToString();
                    this.txtLastName.Text = dgGuards.Rows[e.RowIndex].Cells[3].Value.ToString();
                    this.txtPassword.Text = StringCipher.Decrypt(dgGuards.Rows[e.RowIndex].Cells[4].Value.ToString(), Global.PASSPHRASE);
                    this.txtPhone.Text = dgGuards.Rows[e.RowIndex].Cells[5].Value.ToString();
                    //MessageBox.Show(s);

                    btnSaveUserManagement.Text = "UPDATE";
                    lblCurrentUserMode.Text = "Current Mode: Edit/Delete";
                }
            }
            catch (Exception ex) { /* MessageBox.Show(ex.ToString()); */ }
            finally { /* dgView.ReadOnly = false; */ }
        }

        private void rdoGuard_CheckedChanged(object sender, EventArgs e)
        {
            RdoGuard(sender, e);
        }
        #endregion

        #region Reports

        private bool m_LicensePlate;
        private bool m_Date;
        private bool m_Time;
        private bool m_AdditionalALPR;

        public bool IsLicensePlate { get => m_LicensePlate; set => m_LicensePlate = value; }
        public bool IsDate { get => m_Date; set => m_Date = value; }
        public bool IsTime { get => m_Time; set => m_Time = value; }
        public string LpNumEng { get => txtLicensePlate.Text; set => txtLicensePlate.Text = value; }
        public string LpNumArab { get => txtLicensePlateArab.Text; set => txtLicensePlateArab.Text = value; }
        public DateTime FromTime { get => dtpFromTime.Value; set => dtpFromTime.Value = value; }
        public DateTime ToTime { get => dtpToTime.Value; set => dtpToTime.Value = value; }
        public DateTime FromDate { get => dtpFromDate.Value; set => dtpFromDate.Value = value; }
        public DateTime ToDate { get => dtpToDate.Value; set => dtpToDate.Value = value; }
        public string Gate_Name { get => cboGate.Text; set => cboGate.Text = value; }

        public bool IsAdditionalALPR { get => m_AdditionalALPR; set => m_AdditionalALPR = value; }



        public event EventHandler btnResetReports;

        public event EventHandler BtnReport;

        private void btnReport_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                BtnReport(sender, e);

                if (Global.ProfileMode == "English")
                {
                    this.crystalReportViewer1.ReportSource = ReportsPresenter.mainReportObjectEng;
                }
                else
                {
                    this.crystalReportViewer1.ReportSource = ReportsPresenter.mainReportObject;
                }
                int exportFormatFlags = (int)(CrystalDecisions.Shared.ViewerExportFormats.PdfFormat); //| CrystalDecisions.Shared.ViewerExportFormats.ExcelFormat
                crystalReportViewer1.AllowedExportFormats = exportFormatFlags;
            }
            catch (Exception ex) { }
            finally { this.Cursor = Cursors.Default; }
        }





        private void btnReportClear_Click(object sender, EventArgs e)
        {
            btnLicensePlate.Checked = false;
            chkDate.Checked = false;
            chkTime.Checked = false;
            btnResetReports(sender, e);
        }

        private void btnLicensePlate_CheckedChanged(object sender, EventArgs e)
        {

            if (btnLicensePlate.Checked)
            {
                m_LicensePlate = true;
                this.txtLicensePlate.Enabled = true;
                this.txtLicensePlateArab.Enabled = true;

                this.txtLicensePlate.Visible = true;
                this.txtLicensePlateArab.Visible = true;
            }
            else
            {
                m_LicensePlate = false;
                this.txtLicensePlate.Enabled = false;
                this.txtLicensePlateArab.Enabled = false;

                this.txtLicensePlate.Visible = false;
                this.txtLicensePlateArab.Visible = false;
            }
        }

        private void chkDate_CheckedChanged(object sender, EventArgs e)
        {

            if (chkDate.Checked)
            {
                m_Date = true;
                this.dtpFromDate.Enabled = true;
                this.dtpToDate.Enabled = true;

                this.dtpFromDate.Visible = true;
                this.dtpToDate.Visible = true;


                lblDateFromDisabled.Visible = false;
                lblDateToDisabled.Visible = false;
            }
            else
            {
                m_Date = false;
                this.dtpFromDate.Enabled = false;
                this.dtpToDate.Enabled = false;

                this.dtpFromDate.Visible = false;
                this.dtpToDate.Visible = false;

                lblDateFromDisabled.Text = dtpFromDate.Text;
                lblDateToDisabled.Text = dtpToDate.Text;

                lblDateFromDisabled.Visible = true;
                lblDateToDisabled.Visible = true;
            }
        }

        private void btnClickSnapshot_Click_1(object sender, EventArgs e)
        {
            btnClickSnapshot.Visible = false;
            tbDriver.Visible = true;
        }

        private void tbDriver_Scroll(object sender, ScrollEventArgs e)
        {
            if (m_TotalFrameDriver > 0)
            {
                SeekDriver((IntPtr)tbDriver.Value);
            }
        }

        private void chkTime_CheckedChanged(object sender, EventArgs e)
        {
            m_Time = true;
            if (chkTime.Checked)
            {
                this.dtpFromTime.Enabled = true;
                this.dtpToTime.Enabled = true;

                this.dtpFromTime.Visible = true;
                this.dtpToTime.Visible = true;

                lblTimeFromDisabled.Visible = false;
                lblTimeToDisabled.Visible = false;
            }
            else
            {
                this.dtpFromTime.Enabled = false;
                this.dtpToTime.Enabled = false;

                this.dtpFromTime.Visible = false;
                this.dtpToTime.Visible = false;


                lblTimeFromDisabled.Visible = true;
                lblTimeToDisabled.Visible = true;
            }
        }


        #endregion

        #region VisitorDataEntry
        private string m_Picture;
        private int m_VisitorID = 0;

        private void btnSaveDataEntry_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                {
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                if (m_VisitorID > 0)
                {
                    using (var db = new IVISSEntities())
                    {
                        var visitors = (from v in db.Visitors
                                        where v.visitor_id == m_VisitorID
                                        select v).FirstOrDefault();

                        if (visitors != null)
                        {

                            visitors.visitor_license_plate = this.txtLPNumEnglish.Text;
                            visitors.visitor_license_plate_arabic = this.txtLPNumArabic.Text;

                            visitors.visitor_authorization = this.cboAuthorization.Text;
                            visitors.visitor_classification = this.cboClassification.Text;


                            visitors.visitor_first = this.txtFirstNameDataEntry.Text;
                            visitors.visitor_middle = this.txtMiddleNameDataEntry.Text;
                            visitors.visitor_last = this.txtLastNameDataEntry.Text;

                            visitors.visitor_address = this.txtAddress.Text;
                            visitors.visitor_city = this.txtCity.Text;
                            visitors.visitor_country = this.txtCountry.Text;
                            visitors.visitor_email = this.txtEmail.Text;
                            visitors.visitor_phone_1 = this.txtPhone1.Text;
                            visitors.visitor_phone_2 = this.txtPhone2.Text;
                            visitors.visitor_region = this.txtRegion.Text;

                            //visitors.visitor_manager = 

                            visitors.uploaded = 0;

                            if (m_Picture != null && m_Picture.Length > 0)
                            {
                                visitors.visitor_image = m_Picture;
                            }

                            db.SaveChanges();

                            m_VisitorID = 0;
                        }

                        Global.ShowMessage("Record Updated Successfully", false);

                    }
                }
                else
                {
                    try
                    {
                        using (var db = new IVISSEntities())
                        {
                            var v = new Visitor();


                            v.visitor_license_plate = this.txtLPNumEnglish.Text;
                            v.visitor_license_plate_arabic = this.txtLPNumArabic.Text;

                            v.visitor_classification = this.cboClassification.Text;
                            v.visitor_authorization = this.cboAuthorization.Text;
                            v.visitor_middle = this.txtMiddleNameDataEntry.Text;
                            v.visitor_first = this.txtFirstNameDataEntry.Text;

                            v.visitor_last = this.txtLastNameDataEntry.Text;
                            v.visitor_address = this.txtAddress.Text;
                            v.visitor_city = this.txtCity.Text;
                            v.visitor_region = this.txtRegion.Text;
                            v.visitor_country = this.txtCountry.Text;
                            v.visitor_phone_1 = this.txtPhone1.Text;
                            v.visitor_phone_2 = this.txtPhone2.Text;

                            v.visitor_email = this.txtEmail.Text;

                            v.uploaded = 0;

                            //v.visitor_manager = (int)this.cboManager.SelectedValue;
                            //v.visitor_organization = (int)this.cboOrganization.SelectedValue;
                            //v.visitor_facility = (int)this.cboFacility.SelectedValue;

                            if (m_Picture != null && m_Picture.Length > 0)
                            {
                                v.visitor_image = m_Picture;
                            }

                            db.Visitors.Add(v);
                            db.SaveChanges();


                        }
                        Global.ShowMessage("Record Saved Successfully", false);
                    }
                    catch (Exception oex)
                    {
                        Global.ShowMessage("Visitor First/Last Name cannot be left blanked or use a different value for Visitor First/Last Name", false);
                        Global.AppendString(oex.ToString());
                        return;
                    }

                }

                ClearTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        private void ClearTextBoxes()
        {
            lblCurrentVisitorMode.Text = "Current Mode: New Visitor";
            Action<Control.ControlCollection> func = null;

            //func = (controls) =>
            //{
            //    foreach (Control control in controls)
            //        if (control is TextBox)
            //            (control as TextBox).Clear();
            //        else
            //            func(control.Controls);
            //};

            txtLPNumEnglish.Clear();
            txtLPNumArabic.Clear();
            txtCountry.Clear();
            txtFirstNameDataEntry.Clear();
            txtPhone1.Clear();
            txtMiddleNameDataEntry.Clear();
            txtManager.Clear();
            txtOrganization.Clear();
            txtFacility.Clear();
            txtPhone2.Clear();
            txtLastNameDataEntry.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtRegion.Clear();
            txtCity.Clear();
            txtLPNumEnglish.Focus();

            pBoxPicture.BackgroundImage = null;

            this.cboAuthorization.SelectedIndex = 0;
            this.cboClassification.SelectedIndex = 0;

            m_Picture = "";
        }

        private void btnSearchDataEntry_Click(object sender, EventArgs e)
        {




            if (frm == null)
            {

                frm = new frmVisitorSearch();

                frm.FormClosing += new FormClosingEventHandler(VistorEntryDialog_FormClosing);
                frm.Show();
            }
            else

            {
                frm.FormClosing -= new FormClosingEventHandler(VistorEntryDialog_FormClosing);
                frm.Close();
                frm = null;

                btnSearchDataEntry.PerformClick();

            }

        }

        private void VistorEntryDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (((IVISS.frmVisitorSearch)sender).Visitor != null)
            {

                try
                {
                    m_VisitorID = int.Parse(((IVISS.frmVisitorSearch)sender).Visitor);

                    using (var db = new IVISSEntities())
                    {
                        var visitors = (from v in db.Visitors
                                        where v.visitor_id == m_VisitorID
                                        select v).FirstOrDefault();

                        if (visitors != null)
                        {

                            this.txtLPNumEnglish.Text = visitors.visitor_license_plate;
                            this.txtLPNumArabic.Text = visitors.visitor_license_plate_arabic;

                            this.cboAuthorization.Text = visitors.visitor_authorization.Trim();
                            this.cboClassification.Text = visitors.visitor_classification.Trim();



                            this.txtFirstNameDataEntry.Text = visitors.visitor_first;
                            this.txtMiddleNameDataEntry.Text = visitors.visitor_middle;
                            this.txtLastNameDataEntry.Text = visitors.visitor_last;

                            this.txtAddress.Text = visitors.visitor_address;
                            this.txtCity.Text = visitors.visitor_city;
                            this.txtCountry.Text = visitors.visitor_country;
                            this.txtEmail.Text = visitors.visitor_email;
                            this.txtPhone1.Text = visitors.visitor_phone_1;
                            this.txtPhone2.Text = visitors.visitor_phone_2;
                            this.txtRegion.Text = visitors.visitor_region;
                            m_Picture = visitors.visitor_image;


                            if (m_Picture != null && m_Picture.Length > 0)
                            {
                                pBoxPicture.BackgroundImage = Image.FromFile(m_Picture);
                            }
                            lblCurrentVisitorMode.Text = "Current Mode: Edit Visitor";
                        }
                    }
                }
                catch (Exception ex)
                { }



            }

            frm.FormClosing -= new FormClosingEventHandler(VistorEntryDialog_FormClosing);

            frm = null;
            //your code goes here
            //optionally, you can get or set e.Cancel which gets or sets a value indicating that the event should be cancelled; in this case the form won't close if you cancel it here
            //or, you can check e.CloseReason which gets a value that indicates why the form is being closed (this is an enum Systems.Windows.Forms.CloseReason)
        }

        private void btnResetDataEntry_Click(object sender, EventArgs e)
        {
            m_VisitorID = 0;
            ClearTextBoxes();
        }

        private void txtLPNumArabic_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtLPNumArabic.Text = f.m_LicensePlate;
        }

        private void btnChangePicture_Click(object sender, EventArgs e)
        {
            try
            {
                //OpenFileDialog openFileDialog1 = new OpenFileDialog();
                oFD.InitialDirectory = @"C:\";
                oFD.RestoreDirectory = true;
                oFD.Title = "Browse Image";
                oFD.DefaultExt = "jpg";
                oFD.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                if (oFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var sourceFile = oFD.FileName;
                    pBoxPicture.BackgroundImage = Image.FromFile(sourceFile); //m_Picture

                    if (!Directory.Exists("Pictures"))
                    {
                        Directory.CreateDirectory("Pictures");
                    }

                    var ticks = (int)DateTime.Now.Ticks;
                    var filename = ticks + ".jpg";
                    m_Picture = "Pictures\\" + filename;

                    File.Copy(sourceFile, m_Picture);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void metroPanel25_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool ValidateForm()
        {
            if (this.txtLPNumEnglish.Text.Length == 0 && this.txtLPNumArabic.Text.Length == 0)
            {
                Global.ShowMessage("License plate cannot be blank", false);

                if (this.txtLPNumEnglish.Text.Length == 0)
                    this.txtLPNumEnglish.Focus();

                if (this.txtLPNumArabic.Text.Length == 0)
                    this.txtLPNumArabic.Focus();

                return false;
            }

            if (this.txtFirstNameDataEntry.Text.Length == 0)
            {
                Global.ShowMessage("First name cannot be blank", false);
                this.txtFirstName.Focus();
                return false;
            }

            if (this.txtLastNameDataEntry.Text.Length == 0)
            {
                Global.ShowMessage("Last name cannot be blank", false);
                this.txtLastName.Focus();
                return false;
            }

            return true;
        }
        #endregion


        #region SearchRecords
        public event EventHandler BtnResetSearchRecords;
        public event EventHandler BtnSearchSearchRecords;
        public event EventHandler ChkSetDefault;

        SearchRecordsPresenter searchrecordpresenter;

        private bool m_LicensePlateSearchRecords;
        private bool m_DateSearchRecords;
        private bool m_TimeSearchRecords;
        private bool m_AdditionalALPRSearchRecords;

        public bool isLicensePlateSearchRecords { get => m_LicensePlateSearchRecords; set => m_LicensePlateSearchRecords = value; }
        public bool isDateSearchRecords { get => m_DateSearchRecords; set => m_DateSearchRecords = value; }
        public bool isTimeSearchRecords { get => m_TimeSearchRecords; set => m_TimeSearchRecords = value; }

        public bool isAdditionalALPR { get => m_AdditionalALPRSearchRecords; set => m_AdditionalALPRSearchRecords = value; }

        public string recordingPathSearchRecords { set; get; }

        public string lpNumEngSearchRecords
        {
            get { return txtLicenseSearchRecords.Text; }
            set { txtLicenseSearchRecords.Text = value; }
        }

        public string lpNumArabSearchRecords
        {
            get { return txtLicenseArabicSearchRecords.Text; }
            set { txtLicenseArabicSearchRecords.Text = value; }
        }

        public DateTime fromTimeSearchRecords
        {
            get { return dtpTimeFromSearchRecords.Value; }
            set { dtpTimeFromSearchRecords.Value = value; }
        }

        public DateTime toTimeSearchRecords
        {
            get { return dtpTimeToSearchRecords.Value; }
            set { dtpTimeToSearchRecords.Value = value; }
        }

        public DateTime fromDateSearchRecords
        {
            get { return dtpFromSearchRecords.Value; }
            set { dtpFromSearchRecords.Value = value; }
        }

        public DateTime toDateSearchRecords
        {
            get { return dtpToSearchRecords.Value; }
            set { dtpToSearchRecords.Value = value; }
        }

        public bool userManagemenAlreadyLoaded { get; set; }

        private void btnUpdateSearchRecords_Click(object sender, EventArgs e)
        {
            try
            {
                var licenseNumber = txtLicenseSearchRecords.Text;
                var licenseNumberArabic = txtLicenseArabicSearchRecords.Text;

                using (var db = new IVISSEntities())
                {
                    //db.Details.ToList().Where(x => x.visitor_license_number == licenseNumber).ToList().ForEach(a => a.is_default = 0);
                    //db.SaveChanges();
                    var query = (from d in db.Details
                                 where d.visitor_iviss_recording == recordingPathSearchRecords
                                 select d).FirstOrDefault();

                    if (query != null)
                    {
                        //query.visitor_license_number = licenseNumber;
                        //query.visitor_license_number_arabic = licenseNumberArabic;

                        var isdefault = 0;
                        if (chkSetDefault.Checked)
                        {

                            isdefault = 1;
                        }
                        else
                            isdefault = 0;

                        //db.Entry(query).State = EntityState.Modified;

                        //db.SaveChanges();
                        string strsql = "";
                        int noOfDetails = 0;
                        if (chkSetDefault.Checked)
                        {
                            strsql = "Update Detail set is_default=0 where visitor_license_number='" + licenseNumber + "'";
                            noOfDetails = db.Database.ExecuteSqlCommand(strsql);
                        }


                        if (noOfDetails >= 0)
                        {
                            strsql = "Update Detail set visitor_license_number='" + licenseNumber + "',visitor_license_number_arabic=N'" + licenseNumberArabic + "',is_default=" + isdefault + " where visitor_iviss_recording='" + recordingPathSearchRecords + "'";
                            int noOfRowUpdated = db.Database.ExecuteSqlCommand(strsql);

                            btnUpdateSearchRecords.Visible = false;
                            this.txtLicenseSearchRecords.Text = "";
                            this.txtLicenseArabicSearchRecords.Text = "";

                            Global.ShowMessage("License plate updated successfully", false);

                            btnSearchSearchRecords_Click(null, null);
                        }
                        else
                        {
                            Global.ShowMessage("Unable to update License plate", false);
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                Global.ShowMessage("An error occurred. Please try again later", false);
            }
        }

        private void btnSearchSearchRecords_Click(object sender, EventArgs e)
        {
            BtnSearchSearchRecords(sender, e);
        }

        private void btnResetSearchRecords_Click(object sender, EventArgs e)
        {
            chkLicensePlateSearchRecords.Checked = false;
            chkDateSearchRecords.Checked = false;
            chkTimeSearchRecords.Checked = false;
            txtLicenseArabicSearchRecords.Clear();
            txtLicenseSearchRecords.Clear();
            dtpFromSearchRecords.Value = DateTime.Now;
            dtpToSearchRecords.Value = DateTime.Now;

            dtpTimeFromSearchRecords.Value = DateTime.Now;
            dtpTimeToSearchRecords.Value = DateTime.Now;
            pBoxStitchSearchRecords.Image = null;
            pBoxDriverSearchRecords.Image = null;
            pBoxLPSearchRecords.Image = null;


            BtnResetSearchRecords(sender, e);
        }



        private void chkLicensePlateSearchRecords_CheckedChanged(object sender, EventArgs e)
        {




            if (chkLicensePlateSearchRecords.Checked)
            {

                this.txtLicenseSearchRecords.Enabled = true;
                this.txtLicenseArabicSearchRecords.Enabled = true;

                this.txtLicenseSearchRecords.Visible = true;
                this.txtLicenseArabicSearchRecords.Visible = true;
                m_LicensePlateSearchRecords = true;

            }
            else
            {

                this.txtLicenseSearchRecords.Enabled = false;
                this.txtLicenseArabicSearchRecords.Enabled = false;

                this.txtLicenseSearchRecords.Visible = false;
                this.txtLicenseArabicSearchRecords.Visible = false;

                m_LicensePlateSearchRecords = false;

            }


        }

        private void chkDateSearchRecords_CheckedChanged(object sender, EventArgs e)
        {








            if (chkDateSearchRecords.Checked)
            {

                this.dtpFromSearchRecords.Enabled = true;
                this.dtpToSearchRecords.Enabled = true;

                this.dtpFromSearchRecords.Visible = true;
                this.dtpToSearchRecords.Visible = true;

                lblFromSearchRecords.Visible = false;
                lblToSearchRecords.Visible = false;

                m_DateSearchRecords = true;

            }
            else
            {

                this.dtpFromSearchRecords.Enabled = false;
                this.dtpToSearchRecords.Enabled = false;

                this.dtpFromSearchRecords.Visible = false;
                this.dtpToSearchRecords.Visible = false;

                lblFromSearchRecords.Text = dtpFromSearchRecords.Text;
                lblToSearchRecords.Text = dtpToSearchRecords.Text;

                lblFromSearchRecords.Visible = true;
                lblToSearchRecords.Visible = true;

                m_DateSearchRecords = false;

            }
        }

        private void chkTimeSearchRecords_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTimeSearchRecords.Checked)
            {

                this.dtpTimeFromSearchRecords.Enabled = true;
                this.dtpTimeToSearchRecords.Enabled = true;

                this.dtpTimeFromSearchRecords.Visible = true;
                this.dtpTimeToSearchRecords.Visible = true;


                lblTimeFromSearchRecords.Visible = false;
                lblTimeToSearchRecords.Visible = false;

                m_TimeSearchRecords = true;
            }
            else
            {
                this.dtpTimeFromSearchRecords.Enabled = false;
                this.dtpTimeToSearchRecords.Enabled = false;
                this.dtpTimeFromSearchRecords.Visible = false;
                this.dtpTimeToSearchRecords.Visible = false;

                lblTimeFromSearchRecords.Text = dtpTimeFromSearchRecords.Text;
                lblTimeToSearchRecords.Text = dtpTimeToSearchRecords.Text;


                lblTimeFromSearchRecords.Visible = true;
                lblTimeToSearchRecords.Visible = true;

                m_TimeSearchRecords = false;
            }


        }

        private void dgSearchRecords_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    // chkSetDefault.Checked = false;

                    try
                    {
                        metroTrackBar1.Value = 1;
                    }
                    catch (Exception)
                    {

                    }


                    btnUpdateSearchRecords.Visible = true;

                    recordingPathSearchRecords = dgSearchRecords.Rows[e.RowIndex].Cells["PathSearch"].Value.ToString();
                    chkSetDefault.Checked = (dgSearchRecords.Rows[e.RowIndex].Cells["IsDefault"].Value.ToString() == "1") ? true : false;
                    lblStatus.Text = (dgSearchRecords.Rows[e.RowIndex].Cells["IsDefault"].Value.ToString() == "1") ? "Default" : "";
                    lpNumEngSearchRecords = dgSearchRecords.Rows[e.RowIndex].Cells["LicenseSearch"].Value.ToString();
                    lpNumArabSearchRecords = dgSearchRecords.Rows[e.RowIndex].Cells["Arabic"].Value.ToString();

                    // Stitch 
                    if (File.Exists(recordingPathSearchRecords + @"\outPutVer.jpg"))
                    {
                        if (btnStitch.Visible)
                            btnStitch.Visible = false;

                        using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\outPutVer.jpg", FileMode.Open))
                        {
                            pBoxStitchSearchRecords.Image = Image.FromStream(fs);
                            fs.Close();
                        }
                    }
                    else
                    {
                        pBoxStitchSearchRecords.Image = null;
                        btnStitch.Visible = true;
                    }

                    // Driver 
                    if (File.Exists(recordingPathSearchRecords + @"\Driver.jpg"))
                    {
                        using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\Driver.jpg", FileMode.Open))
                        {
                            if (Global.IsDriverAllowed == true)
                                pBoxDriverSearchRecords.Image = Image.FromStream(fs);
                            fs.Close();
                        }
                    }
                    else
                    {
                        pBoxDriverSearchRecords.Image = null;
                    }

                    // LP Num 
                    if (File.Exists(recordingPathSearchRecords + @"\LPNum.bmp"))
                    {
                        using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\LPNum.bmp", FileMode.Open))
                        {
                            pBoxLPSearchRecords.Image = Image.FromStream(fs);
                            fs.Close();
                        }
                    }
                    else if (File.Exists(recordingPathSearchRecords + @"\LpNumAdditionalALPR.png"))
                    {
                        using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\LpNumAdditionalALPR.png", FileMode.Open))
                        {
                            pBoxLPSearchRecords.Image = Image.FromStream(fs);
                            fs.Close();
                        }
                    }

                    else
                    {
                        pBoxLPSearchRecords.Image = null;
                    }
                    if (Global.IsDriverAllowed == true)
                    {
                        if (File.Exists(recordingPathSearchRecords + @"\Driver.avi"))
                        {
                            //VideoFileReader reader = new VideoFileReader();
                            // open video file
                            // reader.Open(recordingPathSearchRecords + @"\Driver.avi");

                            // int.Parse(reader.FrameCount.ToString()) - 1;
                            // reader.Close();
                            // reader.Dispose();
                            // capture = new DotImaging.FileCapture(recordingPathSearchRecords + @"\Driver.avi");
                            image_array = null;
                            metroTrackBar1.Value = 0;
                            GC.Collect();
                            //capture = new Emgu.CV.Capture(recordingPathSearchRecords + @"\Driver.avi");


                            GetVideoFrames(recordingPathSearchRecords + @"\Driver.avi");
                            // int framecount = image_array.Count;// (int)Math.Floor(capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT)); //get count of frames   
                            // double framerate = capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS); //get fps
                            //metroTrackBar1.Maximum = framecount - 1;
                            // capture.Open();
                        }
                    }

                    //if (dgSearchRecords.Rows[e.RowIndex].Cells["IsDefault"].Value.ToString() == "1")
                    //{
                    //    chkSetDefault.Checked = true;
                    //    //Task.Delay(100);
                    //}
                    //else
                    //{

                    //    chkSetDefault.Checked = false;
                    //    Thread.Sleep(100);
                    //}





                }
            }
            catch (Exception ex)
            {

                var f = ex;
            }


        }





        private Task<List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>>> GetVideoFrames(string Filename)
        {


            try
            {
                return Task.Run(() =>
                {
                    var capture = new Emgu.CV.Capture(recordingPathSearchRecords + @"\Driver.avi");
                    image_array = new List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>>();
                    Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> frame = null;
                    int frameNumber = 0;
                    do
                    {
                        frame = capture.QueryFrame();
                        if (frame != null)
                        {
                            ++frameNumber;
                            if (image_array == null)
                            {
                                image_array = new List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>>();
                            }
                            image_array.Add(frame.Copy());
                        }
                    }
                    while (frame != null);
                    if (image_array.Count() > 0)
                    {
                        if (image_array.Count == 1)
                        {
                            metroTrackBar1.Maximum = 1;
                        }
                        else

                        {
                            metroTrackBar1.Maximum = image_array.Count - 1;
                        }
                    }
                    else
                    {
                        metroTrackBar1.Maximum = 1;
                    }
                    if (capture != null)
                    {
                        capture.Dispose();
                    }
                    return image_array;
                });
            }
            catch (Exception)
            {
                return Task.Run(() =>
                {
                    image_array = new List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>>();
                    return image_array;
                });
            }




















            //image_array = new List<Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>>();
            //capture = new Emgu.CV.Capture(Filename);

            //bool Reading = true;

            //while (Reading)
            //{
            //    Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> frame = capture.QueryFrame();
            //    if (frame != null)
            //    {
            //        image_array.Add(frame.Copy());
            //    }
            //    else
            //    {
            //        Reading = false;
            //    }
            //}

            //await Task.Delay(10000); // 1 second delay

            //return image_array;
        }

        private void chkSetDefault_CheckedChanged(object sender, EventArgs e)
        {
            // ChkSetDefault(sender, e);
        }

        private void btnStitch_Click(object sender, EventArgs e)
        {
            Task tStitch = Task.Run(() =>
            {
                ProcessStartInfo _processStartInfo = new ProcessStartInfo();
                _processStartInfo.WorkingDirectory = Application.StartupPath + @"\stitch\";
                _processStartInfo.FileName = @"stitch.exe";
                _processStartInfo.UseShellExecute = true;
                //_processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _processStartInfo.CreateNoWindow = true;
                _processStartInfo.Arguments = "\"" + recordingPathSearchRecords + "\"";
                _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process myProcess = Process.Start(_processStartInfo);
            });

            btnStitch.Visible = false;
            tmComposite.Start();

            pBoxStitchSearchRecords.SizeMode = PictureBoxSizeMode.CenterImage;
            pBoxStitchSearchRecords.Image = Image.FromFile("loadingnew.gif");
            // pBoxStitchSearchRecords.Image = Image.FromFile("loader.gif");
            // pBoxStitchSearchRecords.Image = Image.FromFile("loading.gif");
        }

        private void tmComposite_Tick(object sender, EventArgs e)
        {
            if (File.Exists(recordingPathSearchRecords + @"\outPutVer.jpg"))
            {
                pBoxStitchSearchRecords.SizeMode = PictureBoxSizeMode.StretchImage;
                pBoxStitchSearchRecords.Image = Image.FromFile(recordingPathSearchRecords + @"\outPutVer.jpg");

                tmComposite.Stop();
            }
        }

        private void txtLicenseArabicSearchRecords_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtLicenseArabicSearchRecords.Text = f.m_LicensePlate;
        }

        private void metroTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (Global.IsDriverAllowed == false) return;
            if (image_array != null)
            {

                var frame = GetFrame(metroTrackBar1.Value);
                //capture.Seek(metroTrackBar1.Value, SeekOrigin.Begin);
                //Bgr<byte>[,] frame = null;
                //capture.ReadTo(ref frame);
                if (frame != null)
                    pBoxDriverSearchRecords.Image = frame.ToBitmap();
                else
                {

                }
            }
        }

        public Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> GetFrame(int frameNum)
        {
            if (image_array != null)
            {
                if (frameNum == 0)
                {
                    return image_array[frameNum];
                }
                else
                {
                    return image_array[frameNum - 1];
                }
            }
            else
            {
                return null;
            }

            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, frameNum);
            // return capture.QueryFrame();
        }
        //AForge.Imaging.Filters.Grayscale grayscale = AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709;
        private void vspDriver_NewFrame_1(object sender, ref Bitmap image)
        {


            if (m_Recording)
            {

                long currentTick = DateTime.Now.Ticks;
                StartTick = StartTick ?? currentTick;
                var frameOffset = new TimeSpan(currentTick - StartTick.Value);
                try
                {


                    //Bitmap grayBitmap = grayscale.Apply((Bitmap)image.Clone());
                    // writer.WriteVideoFrame(grayBitmap, frameOffset);

                    writer.WriteVideoFrame((Bitmap)image.Clone(), frameOffset);
                }
                catch (Exception ex)
                {

                }

            }
            else
            {


            }

        }



        private void btnCaptureDriverSearch_Click(object sender, EventArgs e)
        {
            if (Global.IsDriverAllowed == false)
                return;
            if (pBoxDriverSearchRecords.Image != null)
            {
                Bitmap bitmap = new Bitmap(pBoxDriverSearchRecords.Image);

                if (System.IO.File.Exists(recordingPathSearchRecords + @"\Driver.jpg"))
                    System.IO.File.Delete(recordingPathSearchRecords + @"\Driver.jpg");


                bitmap.Save(recordingPathSearchRecords + @"\Driver.jpg");
                // Dispose of the image files.
                bitmap.Dispose();
                Global.ShowMessage("New Image Captured Successfully", false);

            }

        }



        private void btnEnhanceCompositeImage_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {



            if (pBoxComparison.Image == null)
            {

                //string url = "http://192.168.1.145/stw-cgi/image.cgi?msubmenu=camera&action=set&DayNightMode=BW";
                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                //req.UseDefaultCredentials = true;
                //req.Credentials = new NetworkCredential("admin", Global.mDriverCamPassword);



                //using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                //{


                //    using (Stream responseStream = response.GetResponseStream())
                //    {
                //        // Get a reader capable of reading the response stream
                //        using (StreamReader myStreamReader = new StreamReader(responseStream, Encoding.UTF8))
                //        {
                //            // Read stream content as string
                //            string responseJSON = myStreamReader.ReadToEnd();

                //            // Assuming the response is in JSON format, deserialize it
                //            // creating an instance of TData type (generic type declared before).

                //        }
                //    }

                //}



                Global.ShowMessage("No valid image found", false);
                return;
            }
            picLoadingViewDetails.Visible = true;
            Application.DoEvents();
            picLoadingViewDetails.Image = Image.FromFile("loadingnew.gif");

            Application.DoEvents();
            Application.DoEvents();
            if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
            {
                Application.DoEvents();
                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }
            else
            {
                Application.DoEvents();
                TabMain.TabPages.Add(tpManageImage);

                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }



            this.kpImageViewer1.Image = null;
            OriginalImage = null;

            this.kpImageViewer1.Image = new Bitmap(pBoxComparison.Image);

            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            this.kpImageViewer1.Zoom = 38;

            OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

            picLoadingViewDetails.Visible = false;
            //EnhancedImageForm frm = new EnhancedImageForm();
            //frm.ShowForm(pBoxComparison.Image);
        }


        private void pBoxComparison_Paint(object sender, PaintEventArgs e)
        {
            try
            {

                //e.Graphics.ScaleTransform(float.Parse(ScaleX.ToString()), float.Parse(ScaleY.ToString()));


                //e.Graphics.DrawImage(OriginalImage, ViewingRectangle, ViewingRectangle, GraphicsUnit.Pixel);


            }
            catch (Exception)
            {


            }
        }

        private void pBoxComparison_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {

        }

        private void dgSearchRecords_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {


            if (pBoxComparison.Image == null)
            {
                Global.ShowMessage("No valid image found", false);
                return;
            }
            picLoadingViewDetails.Visible = true;
            Application.DoEvents();
            picLoadingViewDetails.Image = Image.FromFile("loadingnew.gif");

            Application.DoEvents();

            //Application.DoEvents();
            this.kpImageViewer1.Image = null;
            OriginalImage = null;
            Application.DoEvents();
            if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
            {
                Application.DoEvents();
                TabMain.TabPages["tpManageImage"].Text = "Image";
                //   tpManageImage.Enabled = true;

                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }
            else
            {
                Application.DoEvents();
                TabMain.TabPages.Add(tpManageImage);
                TabMain.TabPages["tpManageImage"].Text = "Image";
                // tpManageImage.Enabled = true;
                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }




            this.kpImageViewer1.Image = new Bitmap(pBoxComparison.Image);

            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            // this.kpImageViewer1.Zoom = 38;

            OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

            picLoadingViewDetails.Visible = false;
        }

        private void TabMain_Selecting(object sender, TabControlCancelEventArgs e)
        {
            try
            {
                if (e.TabPage == tpManageImage)
                {
                    if (tpManageImage.Text == "")
                        e.Cancel = true;
                }
            }
            catch (Exception)
            {


            }

        }

        public void BindDataSearchReords(DataTable dt)
        {
            dgSearchRecords.DataSource = dt;
        }

        #endregion

        #region ZoomonMouseOverViewDetails

        Rectangle ViewingRectangle_zoom;


        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void ReInitializeZoom()
        {

            if (chkImageModeViewDetails.Checked)
            {

                OriginalImage_zoom = (Bitmap)pBoxComparisonViewDetails.Image;
                picCloseup.Image = OriginalImage_zoom;
                picCloseup.SizeMode = PictureBoxSizeMode.AutoSize;
                if (OriginalImage_zoom == null) return;

                // Make a shaded version of the image.
                ShadedImage = new Bitmap(OriginalImage_zoom);

                using (Graphics gr = Graphics.FromImage(ShadedImage))
                {
                    using (var br = new SolidBrush(Color.Transparent))
                    {
                        Rectangle rect = new Rectangle(0, 0, ShadedImage.Width, ShadedImage.Height);
                        gr.FillRectangle(br, rect);
                    }
                }

                // Get scale factors to map from big scale to small scale.
                ScaleX = System.Convert.ToSingle(panCloseup.ClientSize.Width) / (double)OriginalImage_zoom.Width;
                ScaleY = System.Convert.ToSingle(panCloseup.ClientSize.Height) / (double)OriginalImage_zoom.Height;

                // See how big the closeup is on the small scale.
                SmallWidth = System.Convert.ToInt32(ScaleX * pBoxComparisonViewDetails.ClientSize.Width);
                SmallHeight = System.Convert.ToInt32(ScaleY * pBoxComparisonViewDetails.ClientSize.Height);



                return;
            }


            Bitmap bmp = null;
            OriginalImage_zoom = (Bitmap)pBoxComparisonViewDetails.Image;

            if (OriginalImage_zoom != null)
            {
                if (chkImageModeViewDetails.Checked)
                {

                    picCloseup.Image = OriginalImage_zoom;
                }
                else
                {
                    bmp = ResizeImage(OriginalImage_zoom, OriginalImage_zoom.Width * 2, OriginalImage_zoom.Height);
                    picCloseup.Image = bmp;
                    picCloseup.Refresh();
                    panCloseup.Refresh();
                    bmp = null;
                }
            }





            //if (chkImageModeViewDetails.Checked)
            //{
            //    picCloseup.SizeMode = PictureBoxSizeMode.StretchImage;
            //}
            //else
            //{
            //    picCloseup.SizeMode = PictureBoxSizeMode.AutoSize;
            //}


            if (OriginalImage_zoom == null) return;

            // Make a shaded version of the image.
            ShadedImage = new Bitmap(OriginalImage_zoom);

            using (Graphics gr = Graphics.FromImage(ShadedImage))
            {
                using (var br = new SolidBrush(Color.Transparent))
                {
                    Rectangle rect = new Rectangle(0, 0, ShadedImage.Width, ShadedImage.Height);
                    gr.FillRectangle(br, rect);
                }
            }

            // Get scale factors to map from big scale to small scale.
            ScaleX = System.Convert.ToSingle(panCloseup.ClientSize.Width) / (double)OriginalImage_zoom.Width;
            ScaleY = System.Convert.ToSingle(panCloseup.ClientSize.Height) / (double)OriginalImage_zoom.Height;

            // See how big the closeup is on the small scale.
            SmallWidth = System.Convert.ToInt32(ScaleX * pBoxComparisonViewDetails.ClientSize.Width);
            SmallHeight = System.Convert.ToInt32(ScaleY * pBoxComparisonViewDetails.ClientSize.Height);
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            if (presenter.UpDateSerial(txtSerialNoSettings.Text) == true)
            {
                Global.ShowMessage("Serial updated successfully,Please restart application to see effect", false);
            }
        }

        private void MainV1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            try
            {
                picLoadingViewDetails.Visible = true;
                Application.DoEvents();
                picLoadingViewDetails.Image = Image.FromFile("loadingnew.gif");

                Application.DoEvents();
                Application.DoEvents();
                if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
                {
                    Application.DoEvents();
                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }
                else
                {
                    Application.DoEvents();
                    TabMain.TabPages.Add(tpManageImage);

                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }


                // string path = m_IVISSRecordingPath + "\\" + "LpNum.bmp";
                this.kpImageViewer1.Image = null;
                OriginalImage = null;

                this.kpImageViewer1.Image = new Bitmap(vspDriverViewDetails.BackgroundImage);

                System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

                this.kpImageViewer1.Zoom = 38;

                OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

                picLoadingViewDetails.Visible = false;
            }
            catch (Exception)
            {


            }
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            try
            {
                picLoadingViewDetails.Visible = true;
                Application.DoEvents();
                picLoadingViewDetails.Image = Image.FromFile("loadingnew.gif");

                Application.DoEvents();
                Application.DoEvents();
                if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
                {


                    Application.DoEvents();
                    TabMain.SelectedIndex = 4;

                    Application.DoEvents();
                }
                else
                {
                    Application.DoEvents();
                    TabMain.TabPages.Add(tpManageImage);

                    TabMain.SelectedIndex = 4;
                    Application.DoEvents();
                }


                // string path = m_IVISSRecordingPath + "\\" + "LpNum.bmp";
                this.kpImageViewer1.Image = null;
                OriginalImage = null;

                this.kpImageViewer1.Image = new Bitmap(vspLPViewDetails.BackgroundImage);

                System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

                this.kpImageViewer1.Zoom = 38;

                OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

                picLoadingViewDetails.Visible = false;
            }
            catch (Exception)
            {


            }
        }

        private void txtRelay1CaptionArab_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtRelay1CaptionArab.Text = f.m_LicensePlate;
        }

        private void txtRelay2CaptionArab_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtRelay2CaptionArab.Text = f.m_LicensePlate;
        }

        private void txtRelay3CaptionArab_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtRelay3CaptionArab.Text = f.m_LicensePlate;
        }

        private void txtRelay4CaptionArab_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtRelay4CaptionArab.Text = f.m_LicensePlate;
        }

        private void txtLiveLPArabic_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtLiveLPArabic.Text = f.m_LicensePlate;
        }

        private void txtLicensePlateArab_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtLicensePlateArab.Text = f.m_LicensePlate;
        }

        private void btnBulkUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Browse Vistor File(.xlsx)";
            openFileDialog1.DefaultExt = "xlsx";
            openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                UploadExcelVisitorToDB(openFileDialog1.FileName);

            }
            else
            {
                lblUploadProgress.Text = "Click On Bulk Upload to Start Upload Process..";
            }


        }

        public void UploadExcelVisitorToDB(string filename)
        {
            bool res = true;
            string error_msg = "";
            string errorVehicleNo = "";
            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                try
                {

                    // Open the Excel file using ClosedXML.
                    // Keep in mind the Excel file cannot be open when trying to read it
                    using (XLWorkbook workBook = new XLWorkbook(filename))
                    {
                        //Read the first Sheet from Excel file.
                        IXLWorksheet workSheet = workBook.Worksheet(1);

                        //Create a new DataTable.
                        DataTable dt = new DataTable();

                        //Loop through the Worksheet rows.
                        bool firstRow = true;
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            //Use the first row to add columns to DataTable.
                            if (firstRow)
                            {
                                foreach (IXLCell cell in row.Cells())
                                {
                                    dt.Columns.Add(cell.Value.ToString());
                                }
                                firstRow = false;
                            }
                            else
                            {
                                //Add rows to DataTable.
                                dt.Rows.Add();
                                int i = 0;



                                //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))

                                foreach (IXLCell cell in row.Cells(1, dt.Columns.Count))
                                {
                                    dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                    i++;
                                }
                            }
                        }

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            lblUploadProgress.Invoke((MethodInvoker)delegate
                            {
                                lblUploadProgress.Text = "Saving Record # " + (i + 1).ToString();
                            });
                            string platenumber = dt.Rows[i]["visitor_license_plate"].ToString();
                            using (var db = new IVISSEntities())
                            {



                                string sql = "update Visitor set visitor_license_plate_arabic=N'" + dt.Rows[i]["visitor_license_plate_arabic"].ToString().Trim() + "'," +
                            "visitor_authorization='" + dt.Rows[i]["visitor_authorization"].ToString().Trim() + "',visitor_classification='" + dt.Rows[i]["visitor_classification"].ToString().Trim() + "'" +
                            ",visitor_first='" + dt.Rows[i]["visitor_first"].ToString().Trim() + "',visitor_middle='" + dt.Rows[i]["visitor_middle"].ToString().Trim() + "'," +
                            "visitor_last='" + dt.Rows[i]["visitor_last"].ToString().Trim() + "'," +
                            "visitor_address='" + dt.Rows[i]["visitor_address"].ToString().Trim() + "',visitor_city='" + dt.Rows[i]["visitor_city"].ToString().Trim() + "',visitor_country='" + dt.Rows[i]["visitor_country"].ToString().Trim() + "'" +
                            ",visitor_email='" + dt.Rows[i]["visitor_email"].ToString().Trim() + "'," +
                            "visitor_phone_1='" + dt.Rows[i]["visitor_phone_1"].ToString().Trim() + "',visitor_phone_2='" + dt.Rows[i]["visitor_phone_2"].ToString().Trim() + "'" +
                            ",visitor_region='" + dt.Rows[i]["visitor_region"].ToString().Trim() + "',visitor_image='" + dt.Rows[i]["visitor_image"].ToString().Trim() + "'" +
                            " where visitor_license_plate='" + dt.Rows[i]["visitor_license_plate"].ToString().Trim() + "' IF @@ROWCOUNT = 0 " +
                            "insert into Visitor(visitor_license_plate,visitor_license_plate_arabic,visitor_authorization,visitor_classification,visitor_first,visitor_middle,visitor_last,visitor_address,visitor_city,visitor_country,visitor_email,visitor_phone_1,visitor_phone_2,visitor_region,visitor_image,uploaded)  " +
                                    "     values('" + dt.Rows[i]["visitor_license_plate"].ToString().Trim() + "',N'" + dt.Rows[i]["visitor_license_plate_arabic"].ToString().Trim() + "','" + dt.Rows[i]["visitor_authorization"].ToString().Trim() + "'," +
                                    "'" + dt.Rows[i]["visitor_classification"].ToString().Trim() + "','" + dt.Rows[i]["visitor_first"].ToString().Trim() + "','" + dt.Rows[i]["visitor_middle"].ToString().Trim() + "'," +
                                    "'" + dt.Rows[i]["visitor_last"].ToString().Trim() + "','" + dt.Rows[i]["visitor_address"].ToString().Trim() + "','" + dt.Rows[i]["visitor_city"].ToString().Trim() + "','" + dt.Rows[i]["visitor_country"].ToString().Trim() + "'," +
                                    "'" + dt.Rows[i]["visitor_email"].ToString().Trim() + "','" + dt.Rows[i]["visitor_phone_1"].ToString().Trim() + "','" + dt.Rows[i]["visitor_phone_2"].ToString().Trim() + "','" + dt.Rows[i]["visitor_region"].ToString().Trim() + "'," +
                                    "'" + dt.Rows[i]["visitor_image"].ToString().Trim() + "',1)";
                                int noOfRowInserted = db.Database.ExecuteSqlCommand(sql);

                                if (noOfRowInserted <= 0)
                                {
                                    res = false;
                                    errorVehicleNo = errorVehicleNo + dt.Rows[i]["visitor_license_plate"].ToString().Trim() + ",";
                                }

                                //txtFirstNameDataEntry.Invoke((MethodInvoker)delegate
                                //{
                                //    txtFirstNameDataEntry.Text = sql.ToString();
                                //});










                            }



                        }
                    }


                }
                catch (Exception ex)
                {
                    lblUploadProgress.Invoke((MethodInvoker)delegate
                    {
                        lblUploadProgress.Text = ex.ToString();
                    });
                    res = false;
                }
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                if (res == true)
                    lblUploadProgress.Text = "Process completed successfully..";
                else
                    lblUploadProgress.Text = "Process completed with errors..Vehicles with error are given below \n" + errorVehicleNo;
            };
            worker.RunWorkerAsync();
        }

        private void myLongRunningTask()
        {

        }

        private void vspDriver_Click(object sender, EventArgs e)
        {

        }

        private void dgView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void chkAI_CheckedChanged(object sender, EventArgs e)
        {
            Global.AIEnabled = chkAI.Checked;
        }

        private void TableLayoutMain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnDriverZoomSearch_Click(object sender, EventArgs e)
        {

            if (pBoxDriverSearchRecords.Image == null)
            {



                Global.ShowMessage("No valid image found", false);
                return;
            }


            Application.DoEvents();
            Application.DoEvents();
            if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
            {
                Application.DoEvents();
                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }
            else
            {
                Application.DoEvents();
                TabMain.TabPages.Add(tpManageImage);

                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }



            this.kpImageViewer1.Image = null;
            OriginalImage = null;

            this.kpImageViewer1.Image = new Bitmap(pBoxDriverSearchRecords.Image);

            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            this.kpImageViewer1.Zoom = 100;

            OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());


        }

        private void btnLPZoomSearch_Click(object sender, EventArgs e)
        {
            if (pBoxLPSearchRecords.Image == null)
            {



                Global.ShowMessage("No valid image found", false);
                return;
            }


            Application.DoEvents();
            Application.DoEvents();
            if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
            {
                Application.DoEvents();
                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }
            else
            {
                Application.DoEvents();
                TabMain.TabPages.Add(tpManageImage);

                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }



            this.kpImageViewer1.Image = null;
            OriginalImage = null;

            this.kpImageViewer1.Image = new Bitmap(pBoxLPSearchRecords.Image);

            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            this.kpImageViewer1.Zoom = 100;

            OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());
        }

        private void btnStitchZoomSearch_Click(object sender, EventArgs e)
        {

            if (pBoxStitchSearchRecords.Image == null)
            {



                Global.ShowMessage("No valid image found", false);
                return;
            }


            Application.DoEvents();
            Application.DoEvents();
            if (TabMain.TabPages.IndexOf(tpManageImage) > 0)
            {
                Application.DoEvents();
                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }
            else
            {
                Application.DoEvents();
                TabMain.TabPages.Add(tpManageImage);

                TabMain.SelectedIndex = 4;
                Application.DoEvents();
            }



            this.kpImageViewer1.Image = null;
            OriginalImage = null;

            this.kpImageViewer1.Image = new Bitmap(pBoxStitchSearchRecords.Image);

            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            this.kpImageViewer1.Zoom = 20;

            OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());

        }

        private void btnDatabaseBackup_Click(object sender, EventArgs e)
        {
            lblDatabaseOperationProgress.Text = "Backing up Database..Please wait..";
            var dispatcherOp = base.Invoke(new MethodInvoker(() =>
            {
                // read connectionstring from config file
                var connectionString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                try
                {
                    BackupfolderDialog.ShowNewFolderButton = true;
                    // Show the FolderBrowserDialog.  
                    DialogResult result = BackupfolderDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {


                        var backupFolder = BackupfolderDialog.SelectedPath + "\\";

                        // read connectionstring from config file


                        // read backup folder from config file ("C:/temp/")


                        var sqlConStrBuilder = new SqlConnectionStringBuilder(connectionString);

                        // set backupfilename (you will get something like: "C:/temp/MyDatabase-2013-12-07.bak")
                        var backupFileName = String.Format(backupFolder + DateTime.Now.ToString("MM-dd-yyyyHmm") + ".bak",
                            backupFolder, sqlConStrBuilder.InitialCatalog,
                            DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

                        using (var connection = new SqlConnection(sqlConStrBuilder.ConnectionString))
                        {
                            var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                                sqlConStrBuilder.InitialCatalog, backupFileName);

                            using (var command = new SqlCommand(query, connection))
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                        }

                        lblDatabaseOperationProgress.Text = "                    Backup completed successfully                  ";
                        //Global.ShowMessage("Backup completed successfully", false);



                    }
                    else
                    {
                        lblDatabaseOperationProgress.Text = "";
                    }

                }
                catch (Exception ex)
                {

                    lblDatabaseOperationProgress.Text = "           Backup requires external drive           ";// ex.Message.ToString();


                }

            }));
            // read backup folder from config file ("C:/temp/")

        }

        private void txtGateName_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            //{
            //    e.Handled = true;
            //}

            //// only allow one decimal point
            //if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            //{
            //    e.Handled = true;
            //}
        }

        private void btnStartAuto_Click(object sender, EventArgs e)
        {
            msg = System.Text.Encoding.ASCII.GetBytes("T");

            stream_commands.Write(msg, 0, msg.Length);
            // presenter.StartTrigger();
        }

        private void btnStopAuto_Click(object sender, EventArgs e)
        {
            msg = System.Text.Encoding.ASCII.GetBytes("F");

            stream_commands.Write(msg, 0, msg.Length);
            //presenter.StopTrigger();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string path = @"C:\IVISS_affraz\";
            //Stitch_Generator_2 st_gn = new Stitch_Generator_2(path + "m1.avi");
            Stitch_Generator st_gn = new Stitch_Generator(path + "m1.avi");
            Bitmap image = st_gn.StitchVideo();
            image.Save(path + "output.jpg");

            Console.WriteLine("");

            Application.DoEvents();
            GC.Collect();
            Application.DoEvents();
        }

        private void btnHighViewDetails_Click(object sender, EventArgs e)
        {

            dividerViewDetails.Left = btnHighViewDetails.Left;
            m_FOB = true;

            var path = GetFilePath();

            if (path.ToLower().Contains(Global.TEMP_FOLDER.ToLower()))
                return;

            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\_high.jpg";

            if (File.Exists(m_SelectedImage))
            {


                if (chkImageModeViewDetails.Checked)
                {
                    pnlViewComparisonImage.Size = new Size(464, 866);
                    pnlViewComparisonImage.VerticalScroll.Value = 0;
                    pBoxComparisonViewDetails.Size = new Size(464, 866);
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                }

                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{


                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = imgCompositeImage.Width * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;

                    Size new_size = new Size(464, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));

                    // pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                    pBoxComparisonViewDetails.Image = imgCompositeImage;

                    //pBoxComparison.Image = Image.FromStream(fs);
                    //});

                    fs.Close();
                }
                pBoxComparisonViewDetails.Image = (Bitmap)imgCompositeImage;

            }

            ReInitializeZoom();
        }

        private void btnOriginalViewDetails_Click(object sender, EventArgs e)
        {
            dividerViewDetails.Left = btnOriginalViewDetails.Left;
            m_FOB = false;

            var path = GetFilePath();

            if (path.ToLower().Contains(Global.TEMP_FOLDER.ToLower()))
                return;

            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\outPutVer.jpg";

            if (File.Exists(m_SelectedImage))
            {

                if (chkImageModeViewDetails.Checked)
                {
                    pnlViewComparisonImage.Size = new Size(464, 866);
                    pnlViewComparisonImage.VerticalScroll.Value = 0;
                    pBoxComparisonViewDetails.Size = new Size(464, 866);
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                }
                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    imgCompositeImage = Image.FromStream(fs);
                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = imgCompositeImage.Width * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;

                    Size new_size = new Size(464, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));

                    //pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                    pBoxComparisonViewDetails.Image = imgCompositeImage;
                    //});

                    fs.Close();
                }
                pBoxComparisonViewDetails.Image = (Bitmap)imgCompositeImage;


            }

            ReInitializeZoom();
        }

        private void btnMediumViewDetails_Click(object sender, EventArgs e)
        {
            dividerViewDetails.Left = btnMediumViewDetails.Left;
            m_FOB = true;

            var path = GetFilePath();

            if (path.ToLower().Contains(Global.TEMP_FOLDER.ToLower()))
                return;

            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\_medium.jpg";

            if (File.Exists(m_SelectedImage))
            {

                if (chkImageModeViewDetails.Checked)
                {
                    pnlViewComparisonImage.Size = new Size(464, 866);
                    pnlViewComparisonImage.VerticalScroll.Value = 0;
                    pBoxComparisonViewDetails.Size = new Size(464, 866);
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                }
                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    imgCompositeImage = Image.FromStream(fs);
                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = imgCompositeImage.Width * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;

                    Size new_size = new Size(464, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));

                    //pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                    pBoxComparisonViewDetails.Image = imgCompositeImage;
                    // pBoxComparison.Image = Image.FromStream(fs);
                    //});

                    fs.Close();
                }
                pBoxComparisonViewDetails.Image = (Bitmap)imgCompositeImage;

            }

            ReInitializeZoom();
        }

        private void btnLowViewDetails_Click(object sender, EventArgs e)
        {

            dividerViewDetails.Left = btnLowViewDetails.Left;
            if (GetFilePath().Contains("IVISSTemp")) return;
            m_FOB = true;

            //Task T2 = Task.Run(() =>
            //{
            m_SelectedImage = GetFilePath() + "\\_low.jpg";

            if (File.Exists(m_SelectedImage))
            {

                if (chkImageModeViewDetails.Checked)
                {
                    pnlViewComparisonImage.Size = new Size(464, 866);
                    pnlViewComparisonImage.VerticalScroll.Value = 0;
                    pBoxComparisonViewDetails.Size = new Size(464, 866);
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                }

                using (FileStream fs = new FileStream(m_SelectedImage, FileMode.Open))
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    imgCompositeImage = Image.FromStream(fs);
                    imgCompositeImage = Image.FromStream(fs);
                    int new_width = imgCompositeImage.Width * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;
                    int new_height = imgCompositeImage.Height * pBoxComparisonViewDetails.Width / imgCompositeImage.Width;

                    Size new_size = new Size(464, new_height);

                    imgCompositeImage = (Image)(new Bitmap(imgCompositeImage, new_size));

                    // pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                    pBoxComparisonViewDetails.Image = imgCompositeImage;
                    // pBoxComparison.Image = Image.FromStream(fs);
                    //});

                    fs.Close();
                }
                pBoxComparisonViewDetails.Image = (Bitmap)imgCompositeImage;

            }

            ReInitializeZoom();
        }

        private void btnRestoreDatabase_Click(object sender, EventArgs e)
        {
            lblDatabaseOperationProgress.Text = "Restoring Database..Please wait..";
            var dispatcherOp = base.Invoke(new MethodInvoker(() =>
            {
                // read connectionstring from config file
                var connectionString = "data source=.\\SQLEXPRESS;initial catalog=IVISS_Client;Integrated Security=True;";

                try
                {
                    RestoreBackupDialog.Filter = "SQL SERVER database backup files|*.bak|All files (*.*)|*.*";

                    // Show the FolderBrowserDialog.  
                    DialogResult result = RestoreBackupDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {


                        var backupFolder = BackupfolderDialog.SelectedPath + "\\";

                        // read connectionstring from config file





                        var sqlConStrBuilder = new SqlConnectionStringBuilder(connectionString);


                        var backupFileName = RestoreBackupDialog.FileName;

                        using (var connection = new SqlConnection(sqlConStrBuilder.ConnectionString))
                        {



                            connection.Open();

                            string UseMaster = "USE master";
                            SqlCommand UseMasterCommand = new SqlCommand(UseMaster, connection);
                            UseMasterCommand.ExecuteNonQuery();

                            string Alter1 = @"ALTER DATABASE [IVISS_Client] SET Single_User WITH Rollback Immediate";
                            SqlCommand Alter1Cmd = new SqlCommand(Alter1, connection);
                            Alter1Cmd.ExecuteNonQuery();

                            string Restore = @"RESTORE DATABASE [IVISS_Client] FROM DISK = N'" + backupFileName + @"' WITH  FILE = 1,  NOUNLOAD,  STATS = 10";
                            SqlCommand RestoreCmd = new SqlCommand(Restore, connection);
                            RestoreCmd.ExecuteNonQuery();

                            string Alter2 = @"ALTER DATABASE [IVISS_Client] SET Multi_User";
                            SqlCommand Alter2Cmd = new SqlCommand(Alter2, connection);
                            Alter2Cmd.ExecuteNonQuery();






                            //var query = String.Format("USE [master] ALTER DATABASE [IVISS_Client] SET Single_User WITH Rollback Immediate GO RESTORE DATABASE {0} FROM DISK='{1}'",
                            //    "IVISS_Client", backupFileName);

                            //using (var command = new SqlCommand(query, connection))
                            //{
                            //    connection.Open();
                            //    command.ExecuteNonQuery();
                            //}

                            lblDatabaseOperationProgress.Text = "Restore completed successfully";

                        }





                    }
                    else
                    {
                        lblDatabaseOperationProgress.Text = "";
                    }

                }
                catch (Exception ex)
                {

                    lblDatabaseOperationProgress.Text = ex.Message.ToString();


                }
            }));



            // read backup folder from config file ("C:/temp/")
        }

        private void chkFODEnabled_CheckedChanged(object sender, EventArgs e)
        {
            dtAutoFOD.Rows.Clear();

            if (File.Exists(Application.StartupPath + "\\AutoFODEnabled.xml"))
            {


                DataRow drowAuto = dtAutoFOD.NewRow();

                if (chkFODEnabled.Checked)
                {
                    drowAuto["AutoFODEnabled"] = "1";
                    chkFODEnabled.Text = "Auto AFOD Enabled";
                }
                else
                {
                    drowAuto["AutoFODEnabled"] = "0";
                    chkFODEnabled.Text = "Auto AFOD Disabled";
                }

                dtAutoFOD.Rows.Add(drowAuto);

                dtAutoFOD.WriteXml((Application.StartupPath + "\\AutoFODEnabled.xml"));

            }
            else

            {




            }

        }

        private void btnNewDataEntry_Click(object sender, EventArgs e)
        {
            m_VisitorID = 0;
            ClearTextBoxes();
            txtLPNumEnglish.Focus();
        }

        private void btnStitchZoomSearch_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(btnStitchZoomSearch, "View Details");
        }

        private void btnDriverZoomSearch_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(btnDriverZoomSearch, "View Details");
        }

        private void btnLPZoomSearch_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(btnLPZoomSearch, "View Details");
        }

        private void txtPhone_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {

            // Only allow control characters, digits, decimal point, plus and minus symbol.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)
                 && (e.KeyChar != '+') && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }
            else
            {
                // is a digit or backspace - ignore digits if length is alreay 10 - allow backspace
                if (Char.IsDigit(e.KeyChar))
                {

                }
            }
        }

        private void dtpFromDate_Layout(object sender, LayoutEventArgs e)
        {

        }

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            lblDateFromDisabled.Text = dtpFromDate.Text;
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            lblDateToDisabled.Text = dtpToDate.Text;
        }

        private void dtpFromTime_ValueChanged(object sender, EventArgs e)
        {
            lblTimeFromDisabled.Text = dtpFromTime.Text;
        }

        private void dtpToTime_ValueChanged(object sender, EventArgs e)
        {
            lblTimeToDisabled.Text = dtpToTime.Text;
        }

        private void txtPhone1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow control characters, digits, decimal point, plus and minus symbol.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)
                 && (e.KeyChar != '+') && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }
            else
            {
                // is a digit or backspace - ignore digits if length is alreay 10 - allow backspace
                if (Char.IsDigit(e.KeyChar))
                {

                }
            }
        }

        private void txtPhone2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow control characters, digits, decimal point, plus and minus symbol.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)
                 && (e.KeyChar != '+') && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }
            else
            {
                // is a digit or backspace - ignore digits if length is alreay 10 - allow backspace
                if (Char.IsDigit(e.KeyChar))
                {

                }
            }
        }

        private void txtFirstNameDataEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsLetter(e.KeyChar))
            //{
            //    e.Handled = true;
            //}
        }

        private void txtLiveLPEnglish_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkImageMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkImageMode.Checked)
                {
                    pnlpBoxComparison.Size = new Size(363, 830);
                    pnlpBoxComparison.VerticalScroll.Value = 0;
                    pBoxComparison.Size = new Size(363, 830);
                    pBoxComparison.SizeMode = PictureBoxSizeMode.StretchImage;

                }
                else

                {
                    pBoxComparison.SizeMode = PictureBoxSizeMode.AutoSize;
                }
            }
            catch (Exception)
            {


            }

        }

        private void chkImageModeViewDetails_CheckedChanged(object sender, EventArgs e)
        {


            try
            {
                if (chkImageModeViewDetails.Checked)
                {



                    pnlViewComparisonImage.Size = new Size(464, 866);
                    pnlViewComparisonImage.VerticalScroll.Value = 0;
                    pBoxComparisonViewDetails.Size = new Size(464, 866);
                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.StretchImage;

                    //panCloseup.Height = 866;
                    //panCloseup.Width = 464;

                    picCloseup.SizeMode = PictureBoxSizeMode.AutoSize;

                    panCloseup.HorizontalScroll.Value = 0;


                }
                else

                {
                    //picCloseup.Width = pBoxComparisonViewDetails.Width*2;

                    //picCloseup.Height = pBoxComparisonViewDetails.Height;

                    //panCloseup.Width = pBoxComparisonViewDetails.Width * 2;
                    // panCloseup.Height = pBoxComparisonViewDetails.Height;

                    pBoxComparisonViewDetails.SizeMode = PictureBoxSizeMode.AutoSize;
                }

                ReInitializeZoom();
            }

            catch (Exception)
            {


            }
        }

        private void chkSaveVideo_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSaveVideo.Checked)
            {
                msg = System.Text.Encoding.ASCII.GetBytes("S");  //enable save

                stream_commands.Write(msg, 0, msg.Length);
            }
            else
            {
                msg = System.Text.Encoding.ASCII.GetBytes("U"); // disable save

                stream_commands.Write(msg, 0, msg.Length);

            }
        }

        private void btnResetCamera_Click(object sender, EventArgs e)
        {



            var frm = new frmMessageBox();
            frm.StatusMessage = "Are you sure you want to reset the camera?";


            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cameraResetStarted = true;
                msg = System.Text.Encoding.ASCII.GetBytes("X");  //enable save

                stream_commands.Write(msg, 0, msg.Length);


            }
            else
            {
                cameraResetStarted = false;
            }

            if (cameraResetStarted == true)
            {

                lblFODError.Text = "Please wait..Reseting camera";

            }

        }

        private void btnDeleteVisitorSearch_Click(object sender, EventArgs e)
        {
            var frm = new frmMessageBox();
            frm.StatusMessage = "Are you sure you want to delete this visitor?";


            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {



                if (m_VisitorID > 0)
                {
                    try
                    {
                        using (var db = new IVISSEntities())
                        {
                            var visitors = (from v in db.Visitors
                                            where v.visitor_id == m_VisitorID
                                            select v).FirstOrDefault();

                            if (visitors != null)
                            {
                                visitors.isDeleted = 1;
                                db.SaveChanges();

                                m_VisitorID = 0;

                                Global.ShowMessage("Visitor deleted successfully", false);
                                ClearTextBoxes();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Global.ShowMessage("Unable to delete visitor", false);

                    }

                }



            }
        }

        #region Additional ALPR

        bool isCollapse = false; // panel status (0 collapse / 1 expansion)
        string currentNumberPlateAdditionalALPR = "";

        DateTime dtLastAdditionalALPRPlateTime = DateTime.Now;

        private void btnAdditionalALPR_Click(object sender, EventArgs e)
        {


            if (isCollapse)
            {
                this.pnlAdditionalALPR.Hide();
                isCollapse = !isCollapse;
            }
            else
            {
                this.pnlAdditionalALPR.Show();
                isCollapse = !isCollapse;
            }
        }

        private void btnAddMoreALPR_Click(object sender, EventArgs e)
        {
            frmAddAdditionALPRSettings frm = new frmAddAdditionALPRSettings();
            frm.Show();
        }



        private void vspLPAdditionalALPR_NewFrame(object sender, ref Bitmap image)
        {


            try
            {

                if (Global.lstAdditionalALPR.Count() > 0)
                {
                    if (Global.lstAdditionalALPR.FirstOrDefault().AIEnabled == true)
                    {

                        string address1 = string.Format("http://" + Global.lstAdditionalALPR.FirstOrDefault().ALPRIP + "/vvq?wfilter=1&getlast_trigger_perc");
                        string response = string.Empty;

                        using (WebClient client = new WebClient())
                        {
                            client.Proxy = null;
                            response = client.DownloadString(address1);



                            if (response.Contains("last_trigger_perc=0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0"))

                            {

                            }
                            else
                            {
                                if (image != null) // && !m_Anpr_Active
                                {
                                    //timerAdditionalALPR.Start();

                                    anprThreadALPR.SetImage(new Bitmap(image));
                                }
                            }

                        }
                    }
                    else
                    {
                        if (Global.lstAdditionalALPR.FirstOrDefault().LoopEnabled == false)
                        {
                            if (image != null) // && !m_Anpr_Active
                            {
                                //Task.Delay(2000).Wait();
                                //timerAdditionalALPR.Start();


                                anprThreadALPR.SetImage(new Bitmap(image));

                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                //MessageBox.Show(e.ToString());
                Global.WriteLog("vspExitALPRAdditional_NewFrame: " + ex.ToString());
            }
        }

        private void anprThreadALPR_anpr_result(ANPR_RESULT_STRUCT result)
        {
            try
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

                    txtLPAdditionalALPR.Text = text.PureAscii().Trim();
                    txtLPAdditionalALPRArabic.Text = text.PureUnicode().Trim();



                    //////this.plateColor = plateColor;
                    //////this.plateSubColor = plateSubColor;
                    //////this.origin = origin;

                    //////this.accuracy = accuracy;

                    // this.lblLiveAccuracy.Text = accuracy + "%";
                    // this.pBoxLiveALPRBar.BackgroundImage = Global.GetAccuracyBitmap(accuracy);

                    //Task t = Task.Run(() =>
                    //{
                    // this.SetAuthorizationBoxLive(txtLiveLPEnglish.Text, txtLiveLPArabic.Text);
                    //});

                    Task tALPRImg = Task.Run(() =>
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            //if (currentNumberPlateAdditionalALPR == txtLPAdditionalALPR.Text)
                            //{

                            //}
                            //else

                            //{
                            SaveGate2Data(result);
                            //    currentNumberPlateAdditionalALPR = txtLPAdditionalALPR.Text;

                            //}
                        });
                    });



                    //////saving to database
                    ////if (!b_m_Auto)
                    ////{
                    ////    //disconnectButton_Click(null,null);
                    ////}

                    ////if (b_m_Auto)
                    ////    ThreadPool.QueueUserWorkItem(o => AddtoDatabase(platetextLabel.Text, lblPlatePrefix.Text, lblLPOrigin.Text, lblStatus.Text, lblPlateColor.Text, lblPlateSubColor.Text, lblAccuracy.Text));
                });
            }
            catch (Exception)
            {


            }


        }

        private void SaveGate2Data(ANPR_RESULT_STRUCT result)
        {

            try
            {

                TimeSpan difference = DateTime.Now.Subtract(dtLastAdditionalALPRPlateTime);

                double totalSeconds = difference.TotalSeconds;

                if (txtLPAdditionalALPR.Text == currentNumberPlateAdditionalALPR && totalSeconds < 60)
                {
                    return;
                }



                int vID = 0;
                string authorization = "";
                string classification = "";
                string date = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.FFF"); //DateTime.Now.ToShortDateString();
                string time = DateTime.Now.ToString("HH:mm:ss.FFF"); //DateTime.Now.ToLongTimeString();
                bool addnew = false;

                if (Directory.Exists(Global.VIDEO_FOLDER + "\\AdditionalALPR"))
                {

                }
                else
                    Directory.CreateDirectory(Global.VIDEO_FOLDER + "\\AdditionalALPR");

                var destinationDir = Global.VIDEO_FOLDER + "\\AdditionalALPR" + "\\" + Convert.ToDateTime(date).ToShortDateString().Replace("/", "-") + " " + time.Replace(":", "-");

                Directory.CreateDirectory(destinationDir);
                using (IVISSEntities db = new IVISSEntities())
                {
                    var visitor = from v in db.Visitors
                                  where v.visitor_license_plate == txtLPAdditionalALPR.Text
                                  select v;


                    //if (!String.IsNullOrEmpty(txtLPAdditionalALPRArabic.Text))
                    //    visitor = visitor.Where(p => p.visitor_license_plate_arabic == txtLPAdditionalALPRArabic.Text);

                    //else
                    //    visitor = visitor.Where(p => p.visitor_license_plate == txtLPAdditionalALPR.Text && p.visitor_license_plate_arabic == txtLPAdditionalALPRArabic.Text);

                    var result1 = visitor.Select(v => new { v.visitor_classification, v.visitor_authorization, v.visitor_id }).FirstOrDefault();

                    if (result1 != null)
                    {
                        authorization = result1.visitor_authorization;
                        classification = result1.visitor_classification;
                        vID = result1.visitor_id;


                    }
                    else
                    {
                        addnew = true;


                    }
                }

                if (addnew == true)
                {

                    vID = AddVisitorAdditionalALPR();
                }

                lblVisitorClassificationAdditionalALPR.Text = authorization;

                if (vID == 0) return;

                using (IVISSEntities db = new IVISSEntities())
                {

                    var d = new Detail();

                    if (d != null)
                    {
                        d.visitor_id = vID;
                        d.visitor_vehicle_license_plate = 1;
                        d.visitor_entry_date = Convert.ToDateTime(date);
                        d.visitor_entry_time = Convert.ToDateTime(time);

                        d.visitor_license_number = txtLPAdditionalALPR.Text;
                        d.visitor_license_number_arabic = txtLPAdditionalALPRArabic.Text;

                        d.visitor_exit_gate = 1;
                        d.visitor_entry_gate = 1;

                        d.visitor_license_prefix = "";
                        d.visitor_license_region = "";

                        d.visitor_license_country = "";
                        d.visitor_license_back_color = "White";
                        d.visitor_license_fore_color = "White";

                        d.visitor_license_type = "";
                        d.visitor_iviss_recording = destinationDir;

                        d.visitor_access_gate = "Entry";
                        // d.visitor_license_accuracy = accuracy;

                        d.visitor_authorization = (authorization.Length > 0) ? authorization : "VISITOR";

                        var gate = (from g in db.AdditionalALPRs select g).FirstOrDefault();

                        if (gate != null)
                        {
                            d.gate_no = gate.GateName;
                        }
                        else
                        {

                            d.gate_no = "";
                        }

                        d.is_default = 0;

                        d.is_primary = 0;

                        db.Details.Add(d);
                        db.SaveChanges();

                        result.image.Save(destinationDir + "\\LpNumAdditionalALPR.png");


                        var dr = dtAdditionalALPR.NewRow();
                        dr["License"] = txtLPAdditionalALPR.Text;
                        dr["ArabicLicense"] = txtLPAdditionalALPRArabic.Text;
                        dr["Date"] = time; //detail.visitor_entry_date.ToShortDateString() + " " + detail.visitor_entry_time.ToShortTimeString(); 
                        if (gate != null)
                        {
                            dr["Gate"] = gate.GateName;
                        }
                        else
                        {

                            dr["Gate"] = "";
                        }

                        dr["Status"] = ""; //(detail.visitor_license_number.Trim().ToLower()=="xac5831")?"DENIED":"ALLOWED";
                        dr["Path"] = destinationDir;
                        dr["Accuracy"] = "";
                        dr["Classification"] = "AUTHORIZED";

                        dtAdditionalALPR.Rows.InsertAt(dr, 0);

                        currentNumberPlateAdditionalALPR = txtLPAdditionalALPR.Text;
                        dtLastAdditionalALPRPlateTime = DateTime.Now;

                    }

                }

            }
            catch (Exception ex)
            {
                currentNumberPlateAdditionalALPR = "";
                var g = ex;
            }


        }

        public int AddVisitorAdditionalALPR()
        {
            try
            {


                var connString = "data source=.\\sqlexpress;initial catalog=IVISS_Client;Integrated Security=True;";


                try
                {
                    using (var sqlConnection = new SqlConnection(connString))
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Visitor(visitor_license_plate,visitor_license_plate_arabic,visitor_classification,visitor_authorization,isDeleted,uploaded,visitor_first,visitor_last) output INSERTED.visitor_id VALUES(@lp,@alp,@classi,@autho,@del,@upl,@first,@last)", sqlConnection))
                        {
                            cmd.Parameters.AddWithValue("@lp", txtLPAdditionalALPR.Text);
                            cmd.Parameters.AddWithValue("@alp", txtLPAdditionalALPRArabic.Text);
                            cmd.Parameters.AddWithValue("@classi", "VISITOR");
                            cmd.Parameters.AddWithValue("@autho", "AUTHORIZED");
                            cmd.Parameters.AddWithValue("@del", 0);
                            cmd.Parameters.AddWithValue("@upl", 0);
                            cmd.Parameters.AddWithValue("@first", txtLPAdditionalALPR.Text);
                            cmd.Parameters.AddWithValue("@last", txtLPAdditionalALPR.Text);
                            sqlConnection.Open();

                            int modified = (int)cmd.ExecuteScalar();

                            if (sqlConnection.State == System.Data.ConnectionState.Open)
                                sqlConnection.Close();

                            return modified;
                        }
                    }
                }
                catch (Exception ex)
                {

                    return 0;
                }




            }
            catch (Exception ex)
            {

                return 0;
            }
        }


        public DataTable AdditionalALPRGridSource()
        {
            try
            {








                //dgView.DataSource = dt;

                string gateNo = Global.m_Gate_No;

                using (IVISSEntities db = new IVISSEntities())
                {
                    // .Take(5)
                    // where objDetails.gate_no == gateNo
                    // where objDetails.visitor_license_number != ""

                    var detailQuery = (from objDetails in db.Details
                                       where objDetails.is_primary == 0
                                       orderby objDetails.visitor_entry_time descending
                                       select new { objDetails.visitor_license_number, objDetails.visitor_license_number_arabic, objDetails.gate_no, objDetails.visitor_entry_time, objDetails.visitor_iviss_recording, objDetails.visitor_license_accuracy, objDetails.visitor_access_status, objDetails.visitor_access_gate, objDetails.visitor_authorization }).Take(200);



                    DataRow dr;

                    foreach (var detail in detailQuery)
                    {
                        dr = dtAdditionalALPR.NewRow();
                        dr["License"] = detail.visitor_license_number;
                        dr["ArabicLicense"] = detail.visitor_license_number_arabic;
                        dr["Date"] = detail.visitor_entry_time; //detail.visitor_entry_date.ToShortDateString() + " " + detail.visitor_entry_time.ToShortTimeString(); 
                        dr["Gate"] = detail.gate_no;
                        dr["Status"] = detail.visitor_access_status; //(detail.visitor_license_number.Trim().ToLower()=="xac5831")?"DENIED":"ALLOWED";
                        dr["Path"] = detail.visitor_iviss_recording;
                        dr["Accuracy"] = detail.visitor_license_accuracy;
                        dr["Classification"] = detail.visitor_authorization;

                        dtAdditionalALPR.Rows.Add(dr);
                    }
                }

                dgAdditionalALPRGate2.DataSource = null;
                dgAdditionalALPRGate2.DataSource = dtAdditionalALPR;

                return dtAdditionalALPR;
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }


        private void dgAdditionalALPRGate2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string plate = dgAdditionalALPRGate2.Rows[e.RowIndex].Cells["LicenseAdditionalALPR"].Value.ToString();
                string path = dgAdditionalALPRGate2.Rows[e.RowIndex].Cells["PathAdditionalALPR"].Value.ToString();
                string classification = dgAdditionalALPRGate2.Rows[e.RowIndex].Cells["ClassificationAdditionalALPR"].Value.ToString();

                path = path + "\\LpNumAdditionalALPR.png";
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        //BeginInvoke((MethodInvoker)delegate
                        //{
                        var imgComparison = Image.FromStream(fs);


                        picLPAdditionalALPR.Image = imgComparison;
                        //});

                        fs.Close();
                    }
                }
                else
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                    this.picLPAdditionalALPR.Image = null;
                    //});
                }

                var authorization = GetAuthorization(plate, classification);

                this.lblVisitorClassificationAdditionalALPR.Text = (authorization == "VISITOR") ? "    " + authorization : authorization;
            }
        }


        #endregion
        private void pnleSettings3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chkAdditionalALPRSarch_CheckedChanged(object sender, EventArgs e)
        {

            m_AdditionalALPRSearchRecords = chkAdditionalALPRSarch.Checked;
        }

        private void chkAdditionalALPRReport_CheckedChanged(object sender, EventArgs e)
        {
            m_AdditionalALPR = chkAdditionalALPRReport.Checked;
        }



        private void picWhole_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // Scale so we can draw in the full scale coordinates.
                e.Graphics.ScaleTransform(float.Parse(ScaleX.ToString()), float.Parse(ScaleY.ToString()));


                // Draw the viewing area using the original image.
                e.Graphics.DrawImage(OriginalImage_zoom, ViewingRectangle_zoom, ViewingRectangle_zoom, GraphicsUnit.Pixel);
            }
            catch (Exception)
            {


            }


        }


        private void picWhole_MouseMove(System.Object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (OriginalImage_zoom == null) return;
            // OriginalImage_zoom = DirectCast(picWhole.Image, Bitmap)
            // Position picCloseup inside its parent Panel.
            double x = System.Convert.ToSingle(e.X) / (Double)pBoxComparisonViewDetails.ClientSize.Width * OriginalImage_zoom.Width - System.Convert.ToSingle(panCloseup.ClientSize.Width) / (Double)2;
            double y = System.Convert.ToSingle(e.Y) / (Double)pBoxComparisonViewDetails.ClientSize.Height * OriginalImage_zoom.Height - System.Convert.ToSingle(panCloseup.ClientSize.Height) / (Double)2;
            if ((x < 0))
                x = 0;
            if ((y < 0))
                y = 0;
            if ((x > OriginalImage_zoom.Width - panCloseup.ClientSize.Width))
                x = OriginalImage_zoom.Width - panCloseup.ClientSize.Width;
            if ((y > OriginalImage_zoom.Height - panCloseup.ClientSize.Height))
                y = OriginalImage_zoom.Height - panCloseup.ClientSize.Height;
            if (chkImageModeViewDetails.Checked == false)
            {
                if (e.X > 420)
                {
                    picCloseup.Location = new Point(-System.Convert.ToInt32(x * -27), -System.Convert.ToInt32(y));
                }
                else if (e.X > 400)
                {
                    picCloseup.Location = new Point(-System.Convert.ToInt32(x * -24), -System.Convert.ToInt32(y));
                }
                else if (e.X > 350)
                {
                    picCloseup.Location = new Point(-System.Convert.ToInt32(x * -20), -System.Convert.ToInt32(y));
                }
                else if (e.X > 220)
                {
                    picCloseup.Location = new Point(-System.Convert.ToInt32(x * -12), -System.Convert.ToInt32(y));
                }
                else
                {
                    picCloseup.Location = new Point(-System.Convert.ToInt32(x), -System.Convert.ToInt32(y));
                }
            }
            else
            {
                picCloseup.Location = new Point(-System.Convert.ToInt32(x), -System.Convert.ToInt32(y));
            }

            // Record the position we are viewing.
            // ViewingRectangle = New Rectangle(CInt(x), CInt(y),
            // panCloseup.ClientSize.Width,
            // panCloseup.ClientSize.Height)

            // Draw the closeup area.
            pBoxComparisonViewDetails.Invalidate();
        }

        private void picWhole_MouseEnter(System.Object sender, System.EventArgs e)
        {
            pBoxComparisonViewDetails.Image = ShadedImage;
            panCloseup.Visible = true;
        }

        // Use the regular image.
        private void picWhole_MouseLeave(object sender, System.EventArgs e)
        {
            pBoxComparisonViewDetails.Image = OriginalImage_zoom;
            panCloseup.Visible = false;
        }

        #endregion
    }

}
