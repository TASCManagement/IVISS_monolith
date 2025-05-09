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
using IVISS.Utility;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IVISS
{
    public partial class frmSearchRecords : Form, ISearchRecords 
    {
        public event EventHandler BtnResetSearchRecords;
        public event EventHandler BtnSearchSearchRecords;
        public event EventHandler ChkSetDefault;

        SearchRecordsPresenter presenter;

        private bool m_LicensePlateSearchRecords;
        private bool m_DateSearchRecords;
        private bool m_TimeSearchRecords;

        private bool m_AdditionalALPRSearchRecords;
        //private string m_Path;

        public frmSearchRecords()
        {
            InitializeComponent();

            presenter = new SearchRecordsPresenter(this);
        }

        public bool isLicensePlateSearchRecords { get => m_LicensePlateSearchRecords; set => m_LicensePlateSearchRecords = value; }
        public bool isDateSearchRecords { get => m_DateSearchRecords; set => m_DateSearchRecords = value; }
        public bool isTimeSearchRecords { get => m_TimeSearchRecords; set => m_TimeSearchRecords = value; }

        public bool isAdditionalALPR { get => m_AdditionalALPRSearchRecords; set => m_AdditionalALPRSearchRecords = value; }

        public string recordingPathSearchRecords { set;  get; }

        public string lpNumEngSearchRecords
        {
            get { return txtLicensePlate.Text; }
            set { txtLicensePlate.Text = value; }
        }

        public string lpNumArabSearchRecords
        {
            get { return txtLicensePlateArab.Text; }
            set { txtLicensePlateArab.Text = value; }
        }

        public DateTime fromTimeSearchRecords
        {
            get { return dtpFromTime.Value; }
            set { dtpFromTime.Value = value; }
        }

        public DateTime toTimeSearchRecords
        {   
            get { return dtpToTime.Value; }
            set { dtpToTime.Value = value; }
        }

        public DateTime fromDateSearchRecords
        {
            get { return dtpFromDate.Value; }
            set { dtpFromDate.Value = value; }
        }

        public DateTime toDateSearchRecords
        {
            get { return dtpToDate.Value; }
            set { dtpToDate.Value = value; }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            BtnSearchSearchRecords(sender, e);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            BtnResetSearchRecords(sender, e);
        }


        public void BindDataSearchReords(DataTable dt)
        {
            dgView.DataSource = dt;
        }

        private void btnLicensePlate_Click(object sender, EventArgs e)
        {
            if (!m_LicensePlateSearchRecords)
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

            m_LicensePlateSearchRecords = !m_LicensePlateSearchRecords;
        }

        private void btnDate_Click(object sender, EventArgs e)
        {
            if (!m_DateSearchRecords)
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

            m_DateSearchRecords = !m_DateSearchRecords;
        }

        private void btnTime_Click(object sender, EventArgs e)
        {
            if (!m_TimeSearchRecords)
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

            m_TimeSearchRecords = !m_TimeSearchRecords;
        }

        private void dgView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            /*
            if (e.RowIndex > 0)
            {
                string path = dgView.Rows[e.RowIndex].Cells["Path"].Value.ToString();

                if (File.Exists(path + @"\outPutVer.jpg"))
                    pBoxStitch.Image = Image.FromFile(path + @"\outPutVer.jpg");
                else
                    pBoxStitch.Image = null;
            }
            */
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

      

        private void frmSearchRecords_Load(object sender, EventArgs e)
        {
            if (Global.DEMO)
            {
                pBoxStitch.Image = Image.FromFile(@"C:\IVISSTemp\outPutVer.jpg");

                this.txtLicensePlate.Text = "6RBZ328";
            }

            btnStitch.Visible = false;
        }

        private void btnStitch_Click(object sender, EventArgs e)
        {

            Task tStitch = Task.Run(() =>
            {
                ProcessStartInfo _processStartInfo = new ProcessStartInfo();
                _processStartInfo.WorkingDirectory = Application.StartupPath + @"\stitch\";
                _processStartInfo.FileName = @"stitch.exe";
                _processStartInfo.UseShellExecute = true;
                //_processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _processStartInfo.CreateNoWindow = true;
                _processStartInfo.Arguments = "\"" + recordingPathSearchRecords + "\""; 
                _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process myProcess = Process.Start(_processStartInfo);
            });

            btnStitch.Visible = false;
            tmComposite.Start();

            pBoxStitch.SizeMode = PictureBoxSizeMode.CenterImage;
            pBoxStitch.Image = Image.FromFile("loader.gif");
        }

        private void tmComposite_Tick(object sender, EventArgs e)
        {
            if (File.Exists(recordingPathSearchRecords + @"\outPutVer.jpg"))
            {
                pBoxStitch.SizeMode = PictureBoxSizeMode.StretchImage;
                pBoxStitch.Image = Image.FromFile(recordingPathSearchRecords + @"\outPutVer.jpg");

                tmComposite.Stop();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var licenseNumber = txtLicensePlate.Text;
                var licenseNumberArabic = txtLicensePlateArab.Text;

                using (var db = new IVISSEntities())
                {
                    var query = (from d in db.Details
                                 where d.visitor_iviss_recording == recordingPathSearchRecords
                                 select d).FirstOrDefault();

                    if (query != null)
                    {
                        query.visitor_license_number = licenseNumber;
                        query.visitor_license_number_arabic = licenseNumberArabic;

                        db.SaveChanges();
                    }
                }

                btnUpdate.Visible = false;
                this.txtLicensePlate.Text = "";
                this.txtLicensePlateArab.Text = "";

                Global.ShowMessage("License plate updated successfully", false);

                btnSearch_Click(null, null);
            }
            catch(Exception ex)
            {
                Global.ShowMessage("An error occurred. Please try again later", false);
            }
        }

        private void txtLicensePlateArab_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtLicensePlateArab.Text = f.m_LicensePlate;
        }

        private void dgView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnUpdate.Visible = true;

                recordingPathSearchRecords = dgView.Rows[e.RowIndex].Cells["Path"].Value.ToString();
                //chkSetDefault.Checked = (dgView.Rows[e.RowIndex].Cells["IsDefault"].Value.ToString() == "1") ? true : false;
                lblStatus.Text = (dgView.Rows[e.RowIndex].Cells["IsDefault"].Value.ToString() == "1") ? "Default" : "";
                lpNumEngSearchRecords = dgView.Rows[e.RowIndex].Cells["License"].Value.ToString();
                lpNumArabSearchRecords = dgView.Rows[e.RowIndex].Cells["Arabic"].Value.ToString();

                // Stitch 
                if (File.Exists(recordingPathSearchRecords + @"\outPutVer.jpg"))
                {
                    if (btnStitch.Visible)
                        btnStitch.Visible = false;

                    using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\outPutVer.jpg", FileMode.Open))
                    {
                        pBoxStitch.Image = Image.FromStream(fs);
                        fs.Close();
                    }
                }
                else
                {
                    pBoxStitch.Image = null;
                    btnStitch.Visible = true;
                }

                // Driver 
                if (File.Exists(recordingPathSearchRecords + @"\Driver.bmp"))
                {
                    using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\Driver.bmp", FileMode.Open))
                    {
                        pBoxDriver.Image = Image.FromStream(fs);
                        fs.Close();
                    }
                }
                else
                {
                    pBoxDriver.Image = null;
                }

                // LP Num 
                if (File.Exists(recordingPathSearchRecords + @"\LPNum.bmp"))
                {
                    using (FileStream fs = new FileStream(recordingPathSearchRecords + @"\LPNum.bmp", FileMode.Open))
                    {
                        pBoxLP.Image = Image.FromStream(fs);
                        fs.Close();
                    }
                }
              
            }
        }

        private void chkSetDefault_CheckedChanged(object sender, EventArgs e)
        {
            ChkSetDefault(sender, e);
            //btnSearch_Click(null, null);
        }


    }
}
