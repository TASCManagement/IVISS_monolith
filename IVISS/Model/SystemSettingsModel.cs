using IVISS.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVISS.Model
{
    class SystemSettingsModel
    {
        public string Relay1CaptionEnglish { set; get; }
        public string Relay1CaptionArabic { set; get; }
        public string Relay1Port { set; get; }
        public string Relay2CaptionEnglish { set; get; }
        public string Relay2CaptionArabic { set; get; }
        public string Relay2Port { set; get; }
        public string Relay3CaptionEnglish { set; get; }
        public string Relay3CaptionArabic { set; get; }
        public string Relay3Port { set; get; }
        public string Relay4CaptionEnglish { set; get; }
        public string Relay4CaptionArabic { set; get; }
        public string Relay4Port { set; get; }
        public string AlprIP { set; get; }
        public string DriverIP { set; get; }
        public string SceneIP { set; get; }
        public string DriverCamPassword { set; get; }
        public string SceneCamPassword { set; get; }
        public string GateName { set; get; }
        public string ComPort { set; get; }
        public string IPAddress { set; get; }
        public string ListenPort { set; get; }
        public bool ALPRLoop { set; get; }

        public bool AIEnabled { set; get; }
        public bool Update()
        {
            try
            {
                using (var db = new IVISSEntities())
                {
                    var query = (from settings in db.SystemSettings
                                 select settings).FirstOrDefault();

                    if (query != null)
                    {
                        //if (Global.IsRelayAllowed)
                        //{
                            query.Relay1 = Relay1CaptionEnglish;
                            query.Relay1Arab = Relay1CaptionArabic;
                            query.Relay1Port = Relay1Port;

                            query.Relay2 = Relay2CaptionEnglish;
                            query.Relay2Arab = Relay2CaptionArabic;
                            query.Relay2Port = Relay2Port;

                            query.Relay3 = Relay3CaptionEnglish;
                            query.Relay3Arab = Relay3CaptionArabic;
                            query.Relay3Port = Relay3Port;

                            query.Relay4 = Relay4CaptionEnglish;
                            query.Relay4Arab = Relay4CaptionArabic;
                            query.Relay4Port = Relay4Port;
                            query.IPAddress = IPAddress;
                            query.ListenPort = ListenPort;
                      //  }
                        query.LicenseCamIP = AlprIP;
                        query.DriverCamIP = DriverIP;
                        query.SceneCamIP = SceneIP;

                        query.ExitLicenseCamIP = "";
                        query.ExitDriverCamIP = "";

                        //query.ALPREntryLoop = global.ENTRY_LOOP_SENSOR = this.ALPREntryLoopToggle.Checked;
                        //query.ALPRExitLoop = global.EXIT_LOOP_SENSOR = this.ALPRExitLoopToggle.Checked;

                        query.DriverCamPassword = DriverCamPassword;
                        query.SceneCamPassword = SceneCamPassword;

                        //bool r = int.TryParse(this.txtRecTimeout.Text, out int timeout);
                        //query.DriverRecTimeout = (r) ? timeout : global.DRIVER_REC_TIMEOUT;

                        //int port = 0;
                        //bool result = int.TryParse(this.txtComPort.Text, out port);
                        //query.PortNo = ComPort;

                        query.PortNo = "";

                        query.ALPREntryLoop = ALPRLoop;
                        query.AIEnabled = AIEnabled;
                        query.gate_no = Global.m_Gate_No.ToString();

                        db.SaveChanges();
                    }
                    else
                    {
                        var newSetting = new SystemSetting();
                        newSetting.Relay1 = Relay1CaptionEnglish;
                        newSetting.Relay1Arab = Relay1CaptionArabic;
                        newSetting.Relay1Port = Relay1Port;

                        newSetting.Relay2 = Relay2CaptionEnglish;
                        newSetting.Relay2Arab = Relay2CaptionArabic;
                        newSetting.Relay2Port = Relay2Port;

                        newSetting.Relay3 = Relay3CaptionEnglish;
                        newSetting.Relay3Arab = Relay3CaptionArabic;
                        newSetting.Relay3Port = Relay3Port;

                        newSetting.Relay4 = Relay4CaptionEnglish;
                        newSetting.Relay4Arab = Relay4CaptionArabic;
                        newSetting.Relay4Port = Relay4Port;

                        newSetting.LicenseCamIP = AlprIP;
                        newSetting.DriverCamIP = DriverIP;
                        newSetting.SceneCamIP = SceneIP;

                        newSetting.ExitLicenseCamIP = "";
                        newSetting.ExitDriverCamIP = "";

                        //query.ALPREntryLoop = global.ENTRY_LOOP_SENSOR = this.ALPREntryLoopToggle.Checked;
                        //query.ALPRExitLoop = global.EXIT_LOOP_SENSOR = this.ALPRExitLoopToggle.Checked;

                        newSetting.DriverCamPassword = DriverCamPassword;
                        newSetting.SceneCamPassword = SceneCamPassword;

                        //bool r = int.TryParse(this.txtRecTimeout.Text, out int timeout);
                        //query.DriverRecTimeout = (r) ? timeout : global.DRIVER_REC_TIMEOUT;

                        //int port = 0;
                        //bool result = int.TryParse(this.txtComPort.Text, out port);
                        //query.PortNo = ComPort;

                        newSetting.PortNo = "";

                        newSetting.ALPREntryLoop = ALPRLoop;
                        newSetting.AIEnabled = AIEnabled;
                        newSetting.IPAddress = IPAddress;
                        newSetting.ListenPort = ListenPort;
                        newSetting.gate_no = Global.m_Gate_No.ToString();
                        newSetting.Id = 1;
                        db.SystemSettings.Add(newSetting);
                        db.SaveChanges();
                    }
                }

                //***************************************************** GATE.TXT *****************************************************

                // code commented for baseline system, gate set to gate1|1
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"gate.txt"))
                //{
                //    file.WriteLine(this.txtGateName.Text + "|" + global.m_Gate_No);
                //}
                //********************************************************************************************************************


                return true;

            }

            catch (DbEntityValidationException e)
            {
                string strerror = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        strerror = "prop="+ve.PropertyName + "-msg=" + ve.ErrorMessage;
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }


                MessageBox.Show(strerror);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
                //MessageBox.Show(ex.ToString());
            }
        }
         
        public bool FillSettings()
        {
            try
            {
                using (var db = new IVISSEntities())
                {
                    var query = (from settings in db.SystemSettings
                                 select settings).FirstOrDefault();

                    if (query != null)
                    {
                        ComPort = query.PortNo;

                        //***************************** Live Mode Relay Ports ****************************

                        /*
                        if (int.TryParse(query.AirWash, out int airWashPort))
                            this.m_NCD_AirWash = (airWashPort - 1).ToString();

                        if (int.TryParse(query.Barrier, out int barrierPort))
                            this.m_NCD_Barrier = (barrierPort - 1).ToString();

                        if (int.TryParse(query.Cameras, out int cameraPort))
                            this.m_NCD_Cameras = (cameraPort - 1).ToString();

                        if (int.TryParse(query.Lights, out int lightsPort))
                            this.m_NCD_Lights = (lightsPort - 1).ToString();
                        */

                        //********************************************************************************

                        //*************************** Ports **********************************************//

                        if (int.TryParse(query.Relay1Port, out int port1))
                            Relay1Port = (port1 - 1).ToString();

                        if (int.TryParse(query.Relay2Port, out int port2))
                            Relay2Port = (port2 - 1).ToString();

                        if (int.TryParse(query.Relay3Port, out int port3))
                            Relay3Port = (port3 - 1).ToString();

                        if (int.TryParse(query.Relay4Port, out int port4))
                            Relay4Port = (port4 - 1).ToString();

                        //*******************************************************************************//

                        Relay1CaptionEnglish = query.Relay1;
                        Relay1CaptionArabic = query.Relay1Arab;
                        Relay1Port = query.Relay1Port;

                        Relay2CaptionEnglish = query.Relay2;
                        Relay2CaptionArabic = query.Relay2Arab;
                        Relay2Port = query.Relay2Port;

                        Relay3CaptionEnglish = query.Relay3;
                        Relay3CaptionArabic = query.Relay3Arab;
                        Relay3Port = query.Relay3Port;

                        Relay4CaptionEnglish = query.Relay4;
                        Relay4CaptionArabic = query.Relay4Arab;
                        Relay4Port = query.Relay4Port;

                        Global.ALPR_CAMERA_HOST[Global.ENTRY_ALPR] = query.LicenseCamIP;
                        //global.ALPR_CAMERA_HOST[global.EXIT_ALPR] = this.txtExitALPRIP.Text = query.ExitLicenseCamIP;

                        DriverIP = query.DriverCamIP;
                        SceneIP = query.SceneCamIP;
                        AlprIP = query.LicenseCamIP;
                        AIEnabled = query.AIEnabled ?? false;
                        DriverCamPassword = query.DriverCamPassword;
                        SceneCamPassword = query.SceneCamPassword;

                        ALPRLoop = query.ALPREntryLoop ?? false;

                        IPAddress = query.IPAddress;
                        ListenPort = query.ListenPort;
                        GateName = query.gate_no??"0";
                      
                        if (query.gate_no== null)
                             query.gate_no = "0";

                        Global.m_Gate_No = query.gate_no.ToString();
                        /*
                        global.DRIVER_REC_TIMEOUT = query.DriverRecTimeout ?? global.DRIVER_REC_TIMEOUT;

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

                        return true;

                    }
                }
            }
            catch (Exception ex){ }
            return false;
        }
    }
}
