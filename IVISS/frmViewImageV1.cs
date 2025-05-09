using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmViewImageV1 : MaterialForm
    {
        public string imgPath { set; get; }

        public frmViewImageV1()
        {
            InitializeComponent();
        }

        private void frmViewImage_Load(object sender, EventArgs e)
        {
            //bm.RotateFlip(RotateFlipType.Rotate90FlipNone)
            //img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            //this.pBox.Image = img;
            try
            {
                var bm = Image.FromFile(imgPath);

                bm.RotateFlip(RotateFlipType.Rotate90FlipNone);
                this.pBox.Image = bm;
            }
            catch (Exception)
            {

               
            }

          
        }
    }
}
