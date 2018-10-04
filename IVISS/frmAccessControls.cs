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
    public partial class frmAccessControls : Form
    {
        public frmAccessControls()
        {
            InitializeComponent();
        }

        private void frmAccessControls_Load(object sender, EventArgs e)
        {

        }

        private void btnOpen1_Click(object sender, EventArgs e)
        {
            //this.btnOpen1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;
            //this.btnClose1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;

            //TurnLC(this.m_NCD_Port1, true);
        }

        private void btnClose1_Click(object sender, EventArgs e)
        {
            //this.btnOpen1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_grey;
            //this.btnClose1.BackgroundImage = (Bitmap)IVISS.Properties.Resources.medium_btn_green_pressed;

            //TurnLC(this.m_NCD_Port1, false);
        }
    }
}
