using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmMessageBox : Form
    {
        public frmMessageBox(bool showCancel = true)
        {
            InitializeComponent();

            if (!showCancel)
            {
                btnCancel.Visible = false;
                btnOK.Left = (this.ClientSize.Width - btnOK.Width) / 2;
                //btnOK.Top = (this.ClientSize.Height - btnOK.Height) / 2;
            }
        }

        // Declare a Name property of type string:
        public string StatusMessage { get; set; } = string.Empty;

        private void btnOK_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void frmMessageBox_Load(object sender, EventArgs e)
        {
            if (StatusMessage.Length > 0)
                this.lblStatus.Text = StatusMessage;
        }
    }
}
