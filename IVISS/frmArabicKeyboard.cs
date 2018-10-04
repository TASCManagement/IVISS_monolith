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
    public partial class frmArabicKeyboard : Form
    {
        public string m_LicensePlate { get; set; }
        private bool m_first = true;
        private string m_LicensePlateNumbers;
        private bool m_Language = false;

        public frmArabicKeyboard()
        {
            InitializeComponent();
        }

        public frmArabicKeyboard(bool lang)
        {
            InitializeComponent();

            m_Language = lang;

            if (lang)
                txtLicensePlate.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
        }

        private void action_Button_Click(object sender, EventArgs e)
        {
            string txt = ((Button)sender).Text;
            AppendText(txt);
        }

        private void numbers_Button_Click(object sender, EventArgs e)
        {
            string token = ((Button)sender).Text;

            //txtLicensePlate.Text = token + ('\u200E').ToString() + txtLicensePlate.Text;
            //txtLicensePlate.Text = txtLicensePlate.Text + ('\u200E').ToString() + token;

            if (m_Language)
            {
                txtLicensePlate.Text += token;
            }
            else
            {
                m_LicensePlateNumbers = m_LicensePlateNumbers + ('\u200E').ToString() + token;
                txtLicensePlate.Text = m_LicensePlateNumbers + ('\u200E').ToString() + m_LicensePlate;
            }
        }

        private void AppendText(string token)
        {
            //txtLicensePlate.Text += " " + token;
            //txtLicensePlate.Text += ('\u200E').ToString() + token;
            //txtLicensePlate.Text = txtLicensePlate.Text + ('\u200E').ToString() + token ;

            if (m_Language)
            {
                //txtLicensePlate.Text = token + txtLicensePlate.Text;
                //m_LicensePlate = token + m_LicensePlate;

                txtLicensePlate.Text += token;
            }
            else
            {
                txtLicensePlate.Text = token + ('\u200E').ToString() + txtLicensePlate.Text;
                m_LicensePlate = token + ('\u200E').ToString() + m_LicensePlate;
            }
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            //this.Close();
            this.txtLicensePlate.Clear();
            m_LicensePlateNumbers = string.Empty;
            m_LicensePlate = string.Empty;
        }

        private void btnSpace_Click(object sender, EventArgs e)
        {
            AppendText(" ");
        }

        private void btnBK_Click(object sender, EventArgs e)
        {
            int len = txtLicensePlate.Text.Length;

            if (len > 1)
            {
                string token = this.txtLicensePlate.ToString().Substring(len - 2, 2);
                m_LicensePlateNumbers = m_LicensePlateNumbers.Replace(token, string.Empty);

                this.txtLicensePlate.Text = this.txtLicensePlate.Text.Remove(len - 2);
            }
            else
            {
                m_LicensePlateNumbers = string.Empty;
                m_LicensePlate = string.Empty;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmArabicKeyboard_Load(object sender, EventArgs e)
        {
            if (m_LicensePlate != null && m_LicensePlate.Length > 0)
            {
                txtLicensePlate.Text = m_LicensePlate;
            }
        }

        private void frmArabicKeyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (txtLicensePlate.MaxLength > 0)
                m_LicensePlate = txtLicensePlate.Text;
        }
    }
}
