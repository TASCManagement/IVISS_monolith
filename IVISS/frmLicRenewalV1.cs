using IVISS.Presenter;
using IVISS.View;
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
    public partial class frmLicRenewalV1 : MaterialForm, ILicRenewal
    {
        public event EventHandler BtnSave;
        LicRenewalPresenter presenter;
        private int _lastFormSize;

        public frmLicRenewalV1()
        {
            InitializeComponent();

            presenter = new LicRenewalPresenter(this);
            this.Resize += new EventHandler(Form_Resize);
            _lastFormSize = GetFormArea(this.Size);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            BtnSave(sender, e);
        }

        public string serialNo
        {
            set { this.txtSerialNo.Text = value; }
            get { return txtSerialNo.Text; }
        }

        public MaterialForm currentForm
        {
            get { return this; }
            set => throw new NotImplementedException();

        }

        public void CloseForm()
        {
            this.Close();
        }

        #region ResizeControls

        private int GetFormArea(Size size)
        {
            return size.Height * size.Width;
        }

        private void Form_Resize(object sender, EventArgs e)
        {



            Control control = (Control)sender;

            float scaleFactor = (float)GetFormArea(control.Size) / (float)_lastFormSize;

            ResizeFont(this.Controls, scaleFactor);

            _lastFormSize = GetFormArea(control.Size);

        }

        private void ResizeFont(Control.ControlCollection coll, float scaleFactor)
        {
            foreach (Control c in coll)
            {
                if (c.HasChildren)
                {
                    ResizeFont(c.Controls, scaleFactor);
                }
                else
                {
                    //if (c.GetType().ToString() == "System.Windows.Form.Label")
                    if (true)
                    {
                        // scale font
                        c.Font = new Font(c.Font.FontFamily.Name, c.Font.Size * scaleFactor, c.Font.Style, c.Font.Unit);
                    }
                }
            }
        }
        #endregion
    }
}
