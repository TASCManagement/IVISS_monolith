using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVISS.View;
using IVISS.Model;
using IVISS.Utility;

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
                view.DriverCamPassword = model.DriverCamPassword;
                view.DriverIP = model.DriverIP;

                view.GateName = model.GateName;

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

                view.SceneCamPassword = model.SceneCamPassword;
                view.SceneIP = model.SceneIP;

                view.ALPRLoop = model.ALPRLoop;
            }
        }

        private void View_BtnUpdate(object sender, EventArgs e)
        {
            model.AlprIP = view.AlprIP;
            model.ComPort = view.ComPort;
            model.DriverCamPassword = view.DriverCamPassword;
            model.DriverIP = view.DriverIP;
            model.GateName = view.GateName;

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

            model.SceneCamPassword = view.SceneCamPassword;
            model.SceneIP = view.SceneIP;

            model.ALPRLoop = view.ALPRLoop;
         
            if (model.Update())
                Global.ShowMessageDialog("Please restart the application for the changes to take effect");
        }
    }
}
