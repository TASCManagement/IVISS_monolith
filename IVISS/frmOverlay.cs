using IVISS.Utility;
using MaterialSkin.Controls;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmOverlay : MetroForm
    {
        public string plate { set; get; }
        public string plateArab { set; get; }
        public string recordingPath { set; get; }

        //=======================================================================
        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenFilesDriver(string filePath);

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseFilesDriver();

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitializeDriver(IntPtr hwnd, IntPtr width, IntPtr height);

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SeekDriver(IntPtr frameNo);

        [DllImport("DRIVERDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SaveDriver(IntPtr frameNo);
        //=======================================================================

        private Image imgCompositeImage, imgComparison;
        private int m_TotalFrameDriver;

        public frmOverlay()
        {
            InitializeComponent();
        }

        private void vspLP_Click(object sender, EventArgs e)
        {

        }

        private void frmOverlay_Load(object sender, EventArgs e)
        {
            //Task T = Task.Run(() => 
            //{
            var comparisonPath = this.SelectImageComparison();
                
                // Stitch
                if (File.Exists(comparisonPath + @"\outPutVer.jpg"))
                {
                    using (FileStream fs = new FileStream(comparisonPath + @"\outPutVer.jpg", FileMode.Open))
                    {
                        //BeginInvoke((MethodInvoker)delegate
                        //{
                            imgComparison = Image.FromStream(fs);
                            pBoxStitchViewDetails.Image = imgComparison;
                        //});

                        fs.Close();
                    }
                }
                else
                {
                    //BeginInvoke((MethodInvoker)delegate
                    //{
                        this.pBoxStitchViewDetails.Image = null;
                    //});
                }
            //});
            
            // Driver 
            if (File.Exists(recordingPath + @"\Driver.bmp"))
            {
                using (FileStream fs = new FileStream(recordingPath + @"\Driver.bmp", FileMode.Open))
                {
                    vspDriverViewDetails.BackgroundImage = Image.FromStream(fs);
                    fs.Close();
                }
            }
            else
            {
                vspDriverViewDetails.BackgroundImage = null;
            }

            // Scene Camera
            if (File.Exists(recordingPath + @"\Scene.bmp"))
            {
                using (FileStream fs = new FileStream(recordingPath + @"\Scene.bmp", FileMode.Open))
                {
                    vspSceneViewDetails.BackgroundImage = Image.FromStream(fs);
                    fs.Close();
                }
            }
            else
            {
                vspSceneViewDetails.BackgroundImage = null;
            }

            // License Plate Num
            if (File.Exists(recordingPath + @"\LpNum.bmp"))
            {
                using (FileStream fs = new FileStream(recordingPath + @"\LpNum.bmp", FileMode.Open))
                {
                    vspLPViewDetails.BackgroundImage = Image.FromStream(fs);
                    fs.Close();
                }
            }
            else
            {
                vspLPViewDetails.BackgroundImage = null;
            }

            // Stitch
            if (File.Exists(recordingPath + @"\outPutVer.jpg"))
            {
                using (FileStream fs = new FileStream(recordingPath + @"\outPutVer.jpg", FileMode.Open))
                {
                    imgCompositeImage = Image.FromStream(fs);
                    pBoxComparisonViewDetails.Image = imgCompositeImage;
                    fs.Close();
                }
            }
            else
            {
                this.pBoxComparisonViewDetails.Image = null;
            }

            //******************************************************************************************

            InitializeDriver((IntPtr)vspDriverViewDetails.Handle, (IntPtr)this.vspDriverViewDetails.Width, (IntPtr)this.vspDriverViewDetails.Height);

            if (m_TotalFrameDriver > 0)
            {
                CloseFilesDriver();
                m_TotalFrameDriver = 0;
                tbDriver.Value = 1;
            }

            m_TotalFrameDriver = OpenFilesDriver(recordingPath);

            if (m_TotalFrameDriver > 0)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbDriver.Minimum = 1;
                    tbDriver.Maximum = m_TotalFrameDriver;
                  
                });
            }

            //******************************************************************************************
        }

        public string SelectImageComparison()
        {
            var licensePlate = plate;
            var licensePlateArab = plateArab;

            using (IVISSEntities db = new IVISSEntities())
            {

                var detail = from d in db.Details
                             select d;

                if (licensePlate.Trim().Length > 0)
                    detail = detail.Where(d => d.visitor_license_number == licensePlate);
                else
                    detail = detail.Where(d => d.visitor_license_number_arabic == licensePlateArab);

                var detailDefault = detail.Where(d => d.is_default == 1);

                var vi = detailDefault.FirstOrDefault();

                if (detailDefault.Count() == 0)
                {
                    vi = detail.FirstOrDefault();
                }

                if (vi != null)
                {
                    return vi.visitor_iviss_recording;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private void picDriver_Click(object sender, EventArgs e)
        {
            string path = recordingPath + "\\" + "Driver.bmp";
            var frm = new frmViewImageV1();
            frm.imgPath = path;
            frm.ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void btnClickSnapshot_Click(object sender, EventArgs e)
        {
            btnClickSnapshot.Visible = false;
            tbDriver.Visible = true;
        }

        private void vspDriver_Click(object sender, EventArgs e)
        {
            if (m_TotalFrameDriver > 0)
            {
                SaveDriver((IntPtr)tbDriver.Value);

                var path = (String.IsNullOrEmpty(recordingPath)) ? Global.LAST_FOLDER : recordingPath;

                System.Threading.Thread.Sleep(200);

                if (File.Exists(path + "\\frame.ppm"))
                {
                    File.Delete(path + "\\frame.ppm");
                }

                //MessageBox.Show(path);
            }

            using (Graphics g = vspDriverViewDetails.CreateGraphics())
            {
                using (Font myFont = new Font("Arial", 10))
                {
                    g.DrawString("Driver snapshot taken", myFont, Brushes.White, new PointF(2f, vspDriverViewDetails.Height - 20)); //new Point(2, 2)
                }
            }

            tbDriver.Visible = false;
            btnClickSnapshot.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void tbDriver_Scroll(object sender, ScrollEventArgs e)
        {

            if (m_TotalFrameDriver > 0)
            {
                SeekDriver((IntPtr)tbDriver.Value);
            }
        }


        private void pic_MouseHover(object sender, EventArgs e)
        {
            ((PictureBox)sender).BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // ((PictureBox)sender).Cursor= Cursors.Hand;
        }


        private void pic_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).BorderStyle = System.Windows.Forms.BorderStyle.None;
            //((PictureBox)sender).Cursor = Cursors.No;
        }

        private void pic_MouseHover(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            EnhancedImageForm frm = new EnhancedImageForm();
            frm.ShowForm(pBoxComparisonViewDetails.Image);
        }

        private void picLicensePlate_Click(object sender, EventArgs e)
        {
            string path = recordingPath + "\\" + "LpNum.bmp";
            var frm = new frmViewImageV1();
            frm.imgPath = path;
            frm.ShowDialog();
        }
    }
}
