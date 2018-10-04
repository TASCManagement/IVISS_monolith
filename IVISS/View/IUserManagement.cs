using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.View
{
    interface IUserManagement
    {
        event EventHandler BtnSave;
        event EventHandler BtnReset;
        event EventHandler BtnDelete;

        event EventHandler RdoManager;
        event EventHandler RdoGuard;

        event EventHandler FormLoad;

        void SetGridSource(DataTable dt);
        void Reset();

        string firstName { set; get; }
        string middleName { set; get; }
        string lastName { set; get; }
        string phone { set; get; }
        string id { set; get; }
        string password { set; get; }
        string selectedID { set; get; }

        bool managerChecked { set; get; }
        bool guardChecked { set; get; }

        string saveBtnCaption { set; get; }
    }
}
