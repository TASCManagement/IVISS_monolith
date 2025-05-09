using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVISS.View
{
    interface ILicRenewal
    {
        event EventHandler BtnSave;
        string serialNo { set; get; }

        MaterialForm currentForm { get; set; }
        void CloseForm();
    }
}
