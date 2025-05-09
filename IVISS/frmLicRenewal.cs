﻿using IVISS.Presenter;
using IVISS.Utility;
using IVISS.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IVISS.View;
using MaterialSkin.Controls;

namespace IVISS
{
    public partial class frmLicRenewal : Form, ILicRenewal
    {
        public event EventHandler BtnSave;
        LicRenewalPresenter presenter;

        public frmLicRenewal()
        {
            InitializeComponent();

            presenter = new LicRenewalPresenter(this);
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
            get { return null; }
            set => throw new NotImplementedException();

        }

        public void CloseForm()
        {
            this.Close();
        }
    }
}
