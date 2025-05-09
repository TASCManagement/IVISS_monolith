using MaterialSkin.Controls;
using MetroFramework.Forms;
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
    public partial class frmViewImageFullScreenV1 : MetroForm
    {
        public string imgPath { set; get; }

        public frmViewImageFullScreenV1()
        {
            InitializeComponent();
        }

        private void frmViewImageFullScreen_Load(object sender, EventArgs e)
        {
            lblPath.Text = imgPath;

            if (File.Exists(imgPath))
                pBox.Image = Image.FromFile(imgPath);
        }
    }
}
