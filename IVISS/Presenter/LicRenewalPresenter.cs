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
            //   var license = view.serialNo; 

            try
            {
                var license = StringCipher.Decrypt(view.serialNo, Global.PASSPHRASE);

                DateTime dt = DateTime.Now.AddDays(int.Parse(license));

                string encrypteddate = StringCipher.Encrypt(dt.ToString(), Global.PASSPHRASE);
                File.WriteAllText(Global.LIC_DIRECTORY + "sys.dll", encrypteddate);

                MessageBox.Show("Please restart the application", Global.COMPANY_NAME);

                view.CloseForm();
            }
            catch (Exception)
            {

                Global.ShowMessage("Invalid License Key", false);
            }
           
        }

    }
}
