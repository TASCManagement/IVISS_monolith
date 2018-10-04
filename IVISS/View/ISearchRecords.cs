using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace IVISS.View
{
    interface ISearchRecords
    {
        bool isLicensePlate { set; get; }
        bool isDate { set; get; }
        bool isTime { set; get; }

        string lpNumEng { set; get; }
        string lpNumArab { set; get; }
        
        DateTime fromTime { set; get; }
        DateTime toTime { set; get; }

        DateTime fromDate { set; get; }
        DateTime toDate { set; get; }

        void BindData(DataTable dt);

        event EventHandler BtnReset;
        event EventHandler BtnSearch;
    }
}
