using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.View
{
    interface ISettings
    {
        event EventHandler BtnSystemSettings;
        event EventHandler BtnUserManagement;
        event EventHandler BtnReports;
        event EventHandler BtnAccessControls;
        event EventHandler BtnVisitorDataEntry;
    }
}
