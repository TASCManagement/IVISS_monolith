using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IVISS.Utility;
using IVISS.View;
using IVISS.Presenter;
using MaterialSkin.Controls;

namespace IVISS
{
    public partial class frmLogin : Form, ILogin 
    {
        private bool m_Disclaimer = false;
        public event EventHandler BtnOk;
        LoginPresenter presenter;

        public frmLogin()
        {
            InitializeComponent();

            presenter = new LoginPresenter(this);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BtnOk(sender, e);
            this.Cursor = Cursors.Default;
        }

        private void btnDisclaimer_Click(object sender, EventArgs e)
        {
            if (!m_Disclaimer)
            {
                btnDisclaimer.Image = (Bitmap)IVISS.Properties.Resources._checked;
                pnlLogin.Enabled = true;
                btnOK.Enabled = true;

            }
            else
            {
                btnDisclaimer.Image = (Bitmap)IVISS.Properties.Resources._unchecked;
                pnlLogin.Enabled = false;
                btnOK.Enabled = false;
            }

            m_Disclaimer = !m_Disclaimer;

        }

        private void frmLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.P)
            {
                //Do SomeThing
                // MessageBox.Show("ALP + P");

                this.Hide();

                var frm = new frmChangePassword();
                frm.ShowDialog();

                this.Show();
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.txtUsername.Focus();


            this.lblDisclaimer.Text = "***************************** IMPORTANT DISCLAIMER ***************************** " + Environment.NewLine +
                                      "                                                                                                   " + Environment.NewLine +
                                      "IVISS System is designed to aid the operator to visually inspect the under carriage of the vehicle " + Environment.NewLine +
                                      "before entering the facility. IVISS System is NOT designed to automatically identify presence of   " + Environment.NewLine +
                                      "explosives or other contraband without the interaction or visual review by the operator. The       " + Environment.NewLine +
                                      "operator assumes all responsibility of thoroughly inspecting the vehicle before letting the        " + Environment.NewLine +
                                      "vehicle into the facility. TASC Management or its affiliates shall not be held responsible of any  " + Environment.NewLine +
                                      "incident occurring due to lack of judgement by the actual operator of the IVISS System.            " + Environment.NewLine +
                                      "                                                                                                   " + Environment.NewLine +
                                      "******************************************************************************* ";

        }

        private void CapsLockCheck()
        {
            if (Control.IsKeyLocked(Keys.CapsLock))
                lblErrMsg.Text = "Caps lock is on";
            else
                lblErrMsg.Text = "";
        }

        private void txtUsername_Click(object sender, EventArgs e)
        {
            CapsLockCheck();
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            CapsLockCheck();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            CapsLockCheck();
        }

        private void txtPassword_Click(object sender, EventArgs e)
        {
            CapsLockCheck();
        }

        public string username
        {
            get { return txtUsername.Text; }
            set { txtUsername.Text = value; }
        }

        public string password
        {
            get { return txtPassword.Text; }
            set { txtPassword.Text = value; }
        }

        public string errMsg
        {
            set { lblErrMsg.Text = value;  }
        }

        public MaterialForm currentForm { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void CloseForm()
        {
            Close();
            DialogResult = DialogResult.OK;
        }

    }
}
