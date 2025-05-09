using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmViewImageFullScreen : Form
    {
        public string imgPath { set; get; }

        public frmViewImageFullScreen()
        {
            InitializeComponent();
        }

        private void frmViewImageFullScreen_Load(object sender, EventArgs e)
        {
            lblPath.Text = imgPath;

            if (File.Exists(imgPath))
                pBox.Image = Image.FromFile(imgPath);
        }

        private void pBox_MouseMove(object sender, MouseEventArgs e)
        {
            //pBox.Width = (int)(pBox.Width * zoomratio);
            //pBox.Height = (int)(pBox.Height * zoomratio);
            //pBox.Top = (int)(e.Y - zoomratio * (e.Y - pBox.Top));
            //pBox.Left = (int)(e.X - zoomratio * (e.X - pBox.Left));
        }
    }
}
