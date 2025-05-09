using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVISS
{
    public partial class EnhancedImageForm : MetroForm
    {
        #region Declaration
        Image OriginalImage;

        #endregion
        public EnhancedImageForm()
        {
            InitializeComponent();
        }

        public void ShowForm(Image img)
        {
            //this.kpImageViewer1.Zoom = 18;
          //  this.TopMost = true;
         
            this.kpImageViewer1.Image = new Bitmap(img);

            


            this.Show();

         
        }

        private void EnhancedImageForm_Shown(object sender, EventArgs e)
        {
           // this.kpImageViewer1.Zoom = 18;
        }

        private void EnhancedImageForm_Load(object sender, EventArgs e)
        {
            System.Drawing.Rectangle workingRectangle =    Screen.PrimaryScreen.WorkingArea;
            this.Height = workingRectangle.Height - 20;
            this.Width = workingRectangle.Width - 120;
            this.kpImageViewer1.Zoom = 38;

            OriginalImage = (Bitmap)(this.kpImageViewer1.Image.Clone());
        }
       
     
      

    }

   
}
