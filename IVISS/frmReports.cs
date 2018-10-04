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
    public partial class frmReports : Form, IReports
    {
        ReportsPresenter presenter;

        private bool m_LicensePlate;
        private bool m_Date;
        private bool m_Time;

        public bool IsLicensePlate { get => m_LicensePlate; set => m_LicensePlate = value; }
        public bool IsDate { get => m_Date; set => m_Date = value; }
        public bool IsTime { get => m_Time; set => m_Time = value; }
        public string LpNumEng { get => txtLicensePlate.Text; set => txtLicensePlate.Text = value; }
        public string LpNumArab { get => txtLicensePlateArab.Text; set => txtLicensePlateArab.Text = value; }
        public DateTime FromTime { get => dtpFromTime.Value; set => dtpFromTime.Value = value; }
        public DateTime ToTime { get => dtpToTime.Value; set => dtpToTime.Value = value; }
        public DateTime FromDate { get => dtpFromDate.Value; set => dtpFromDate.Value = value; }
        public DateTime ToDate { get => dtpToDate.Value; set => dtpToDate.Value = value; }

        public frmReports()
        {
            InitializeComponent();

            presenter = new ReportsPresenter(this);
        }

       

        public event EventHandler BtnReset;
        public event EventHandler BtnReport;

        private void btnReport_Click(object sender, EventArgs e)
        {
            this.UseWaitCursor = true;
            BtnReport(sender, e);
            this.UseWaitCursor = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnReportReset_Click(object sender, EventArgs e)
        {
            BtnReset(sender, e);
        }

        private void btnLicensePlate_Click(object sender, EventArgs e)
        {
            if (!m_LicensePlate)
            {
                btnLicensePlate.Image = (Bitmap)IVISS.Properties.Resources._checked;
                this.txtLicensePlate.Enabled = true;
                this.txtLicensePlateArab.Enabled = true;

            }
            else
            {
                btnLicensePlate.Image = (Bitmap)IVISS.Properties.Resources._unchecked;
                this.txtLicensePlate.Enabled = false;
                this.txtLicensePlateArab.Enabled = false;
            }

            m_LicensePlate = !m_LicensePlate;
        }

        private void btnDate_Click(object sender, EventArgs e)
        {
            if (!m_Date)
            {
                btnDate.Image = (Bitmap)IVISS.Properties.Resources._checked;
                this.dtpFromDate.Enabled = true;
                this.dtpToDate.Enabled = true;
            }
            else
            {
                btnDate.Image = (Bitmap)IVISS.Properties.Resources._unchecked;
                this.dtpFromDate.Enabled = false;
                this.dtpToDate.Enabled = false;
            }

            m_Date = !m_Date;
        }

        private void btnTime_Click(object sender, EventArgs e)
        {
            if (!m_Time)
            {
                btnTime.Image = (Bitmap)IVISS.Properties.Resources._checked;
                this.dtpFromTime.Enabled = true;
                this.dtpToTime.Enabled = true;
            }
            else
            {
                btnTime.Image = (Bitmap)IVISS.Properties.Resources._unchecked;
                this.dtpFromTime.Enabled = false;
                this.dtpToTime.Enabled = false;
            }

            m_Time = !m_Time;
        }
    }
}
