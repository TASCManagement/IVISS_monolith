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

namespace IVISS
{
    public partial class frmSettings : Form, ISettings
    {
        SettingsPresenter presenter;
        
        public event EventHandler BtnSystemSettings;
        public event EventHandler BtnUserManagement;
        public event EventHandler BtnReports;
        public event EventHandler BtnAccessControls;
        public event EventHandler BtnVisitorDataEntry;

        public frmSettings()
        {
            InitializeComponent();

            presenter = new SettingsPresenter(this);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSystemSettings_Click(object sender, EventArgs e)
        {
            BtnSystemSettings(sender, e);
        }

        private void btnUserManagement_Click(object sender, EventArgs e)
        {
            BtnUserManagement(sender, e);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            BtnReports(sender, e);
        }

        private void btnAccessControls_Click(object sender, EventArgs e)
        {
            BtnAccessControls(sender, e);
        }

        private void btnVisitorDataEntry_Click(object sender, EventArgs e)
        {
            BtnVisitorDataEntry(sender, e);
        }
    }
}
