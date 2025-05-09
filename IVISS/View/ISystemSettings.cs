using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.View
{
    interface ISystemSettings
    {
        event EventHandler BtnUpdate;

        string Relay1CaptionEnglish { set; get; }
        string Relay1CaptionArabic { set; get; }
        string Relay1Port { set; get; }
        string Relay2CaptionEnglish { set; get; }
        string Relay2CaptionArabic { set; get; }
        string Relay2Port { set; get; }
        string Relay3CaptionEnglish { set; get; }
        string Relay3CaptionArabic { set; get; }
        string Relay3Port { set; get; }
        string Relay4CaptionEnglish { set; get; }
        string Relay4CaptionArabic { set; get; }
        string Relay4Port { set; get; }

        string AlprIP { set; get; }
        string DriverIP { set; get; }
        string SceneIP { set; get; }

        string DriverCamPassword { set; get; }
        string SceneCamPassword { set; get; }
        string GateName { set; get; }
        string ComPort { set; get; }
        bool ALPRLoop { set; get; }
        string IPAddress { set; get; }
        string ListenPort { set; get; }
        bool AIEnabled { set; get; }
        MaterialForm systemSettingForm { get; set; }

    }
}
