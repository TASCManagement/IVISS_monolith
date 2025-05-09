using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IVISS.View;
using IVISS.Presenter;
using MaterialSkin.Controls;

namespace IVISS
{
    public partial class frmSystemSettings : Form, ISystemSettings
    {
        SystemSettingsPresenter presenter;

        public frmSystemSettings()
        {
            InitializeComponent();

            
            presenter = new SystemSettingsPresenter(this);
            this.Cursor = Cursors.Default;
        }

        public string Relay1CaptionEnglish { get => this.txtRelay1Caption.Text; set => this.txtRelay1Caption.Text = value; }
        public string Relay1CaptionArabic { get => this.txtRelay1CaptionArab.Text; set => this.txtRelay1CaptionArab.Text = value; }
        public string Relay1Port { get => this.txtRelay1Port.Text; set => this.txtRelay1Port.Text = value; }
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

        public bool AIEnabled { get => this.ALPREntryLoopToggle.Checked; set => this.ALPREntryLoopToggle.Checked = value; }
        public string IPAddress { get => this.txtIPAddress.Text; set => this.txtIPAddress.Text = value; }
        public string ListenPort { get => this.txtListenPort.Text; set => this.txtListenPort.Text = value; }
        public MaterialForm systemSettingForm { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler BtnUpdate;

        private void btnSettingsUpdate_Click(object sender, EventArgs e)
        {
            BtnUpdate(sender, e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
