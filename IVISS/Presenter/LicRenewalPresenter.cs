using IVISS.Utility;
using IVISS.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IVISS.Presenter
{
    class LicRenewalPresenter
    {
        ILicRenewal view;

        public LicRenewalPresenter(ILicRenewal lrView)
        {
            view = lrView;

            view.BtnSave += view_BtnSave;
        }

        private void view_BtnSave(object sender, EventArgs e)
        {
            var license = view.serialNo; // StringCipher.Decrypt(this.txtSerialNo.Text, Shared.PASSPHRASE);

            File.WriteAllText(Global.LIC_DIRECTORY + "sys.dll", license);

            MessageBox.Show("Please restart the application", Global.COMPANY_NAME);

            view.CloseForm();
        }

    }
}
