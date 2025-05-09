using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVISS.View;
using IVISS.Model;
using IVISS.Utility;
using System.Windows.Forms;

namespace IVISS.Presenter
{
    class SystemSettingsPresenter
    {
        ISystemSettings view;
        SystemSettingsModel model;

        public SystemSettingsPresenter(ISystemSettings ssView)
        {
            view = ssView;

            model = new SystemSettingsModel();

            view.BtnUpdate += View_BtnUpdate;

            this.LoadSettings();
        }

        private void LoadSettings()
        {
            if (model.FillSettings())
            {
                view.AlprIP = model.AlprIP;
                view.ComPort = model.ComPort;
                view.GateName = model.GateName;

                if (Global.IsDriverAllowed == true)
                {
                    view.DriverCamPassword = model.DriverCamPassword;
                    view.DriverIP = model.DriverIP;
                }
               

               
                //if (Global.IsRelayAllowed == true)
                //{
                    view.Relay1CaptionArabic = model.Relay1CaptionArabic;
                    view.Relay1CaptionEnglish = model.Relay1CaptionEnglish;
                    view.Relay1Port = model.Relay1Port;

                    view.Relay2CaptionArabic = model.Relay2CaptionArabic;
                    view.Relay2CaptionEnglish = model.Relay2CaptionEnglish;
                    view.Relay2Port = model.Relay2Port;

                    view.Relay3CaptionArabic = model.Relay3CaptionArabic;
                    view.Relay3CaptionEnglish = model.Relay3CaptionEnglish;
                    view.Relay3Port = model.Relay3Port;

                    view.Relay4CaptionArabic = model.Relay4CaptionArabic;
                    view.Relay4CaptionEnglish = model.Relay4CaptionEnglish;
                    view.Relay4Port = model.Relay4Port;

                    view.IPAddress = model.IPAddress;
                    view.ListenPort = model.ListenPort;
                    
                //}

                if (Global.IsSceneAllowed == true)
                {
                    view.SceneCamPassword = model.SceneCamPassword;
                    view.SceneIP = model.SceneIP;
                }
                view.ALPRLoop = model.ALPRLoop;
                view.AIEnabled = model.AIEnabled;
              
            }
        }

        private void View_BtnUpdate(object sender, EventArgs e)
        {
            try
            {
                model.AlprIP = view.AlprIP;
                model.ComPort = view.ComPort;
                model.DriverCamPassword = view.DriverCamPassword;
                model.DriverIP = view.DriverIP;
                model.GateName = view.GateName;

                if (view.GateName == "" || view.GateName == null)
                    view.GateName = "0";

                Global.m_Gate_No = view.GateName;

                if (Global.IsRelayAllowed == true)
                {
                    model.Relay1CaptionArabic = view.Relay1CaptionArabic;
                    model.Relay1CaptionEnglish = view.Relay1CaptionEnglish;
                    model.Relay1Port = view.Relay1Port;

                    model.Relay2CaptionArabic = view.Relay2CaptionArabic;
                    model.Relay2CaptionEnglish = view.Relay2CaptionEnglish;
                    model.Relay2Port = view.Relay2Port;

                    model.Relay3CaptionArabic = view.Relay3CaptionArabic;
                    model.Relay3CaptionEnglish = view.Relay3CaptionEnglish;
                    model.Relay3Port = view.Relay3Port;

                    model.Relay4CaptionArabic = view.Relay4CaptionArabic;
                    model.Relay4CaptionEnglish = view.Relay4CaptionEnglish;
                    model.Relay4Port = view.Relay4Port;
                }
                if (Global.IsSceneAllowed == true)
                {
                    model.SceneCamPassword = view.SceneCamPassword;
                    model.SceneIP = view.SceneIP;
                }
                model.ALPRLoop = view.ALPRLoop;
                model.AIEnabled = view.AIEnabled;
                model.IPAddress = view.IPAddress;
                model.ListenPort = view.ListenPort;

                if (model.Update())
                {
                    if (view.GateName == "" || view.GateName==null)
                        view.GateName = "0";
                   
                    Global.m_Gate_No = view.GateName;
                    MetroFramework.MetroMessageBox.Show(view.systemSettingForm, "Please restart the application for the changes to take effect", Global.COMPANY_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
                   
                    //Global.ShowMessageDialog("Please restart the application for the changes to take effect");
                   // MessageBox.Show("Please restart the application for the changes to take effect");
                }
            }
            catch(Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(view.systemSettingForm, ex.ToString(), Global.COMPANY_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
