using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IVISS.View;
using IVISS.Presenter;
using System.IO;

namespace IVISS
{
    public partial class frmSearchRecords : Form, ISearchRecords 
    {
        public event EventHandler BtnReset;
        public event EventHandler BtnSearch;
        SearchRecordsPresenter presenter;

        private bool m_LicensePlate;
        private bool m_Date;
        private bool m_Time;

        public frmSearchRecords()
        {
            InitializeComponent();

            presenter = new SearchRecordsPresenter(this);
        }

        public bool isLicensePlate { get => m_LicensePlate; set => m_LicensePlate = value; }
        public bool isDate { get => m_Date; set => m_Date = value; }
        public bool isTime { get => m_Time; set => m_Time = value; }

        public string lpNumEng
        {
            get { return txtLicensePlate.Text; }
            set { txtLicensePlate.Text = value; }
        }

        public string lpNumArab
        {
            get { return txtLicensePlateArab.Text; }
            set { txtLicensePlateArab.Text = value; }
        }

        public DateTime fromTime
        {
            get { return dtpFromTime.Value; }
            set { dtpFromTime.Value = value; }
        }

        public DateTime toTime
        {   
            get { return dtpToTime.Value; }
            set { dtpToTime.Value = value; }
        }

        public DateTime fromDate
        {
            get { return dtpFromDate.Value; }
            set { dtpFromDate.Value = value; }
        }

        public DateTime toDate
        {
            get { return dtpToDate.Value; }
            set { dtpToDate.Value = value; }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            BtnSearch(sender, e);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            BtnReset(sender, e);
        }

        public void BindData(DataTable dt)
        {
            dgView.DataSource = dt;
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

        private void dgView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                string path = dgView.Rows[e.RowIndex].Cells["Path"].Value.ToString();

                if (File.Exists(path + @"\outPutVer.jpg"))
                    pBoxStitch.Image = Image.FromFile(path + @"\outPutVer.jpg");
                else
                    pBoxStitch.Image = null;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                string path = dgView.Rows[e.RowIndex].Cells["Path"].Value.ToString();

                if (File.Exists(path + @"\outPutVer.jpg"))
                    pBoxStitch.Image = Image.FromFile(path + @"\outPutVer.jpg");
                else
                    pBoxStitch.Image = null;
            }
        }
    }
}
