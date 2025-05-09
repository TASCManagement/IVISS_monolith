using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.View
{
    interface IReports
    {
        bool IsLicensePlate { set; get; }
        bool IsAdditionalALPR { set; get; }
        bool IsDate { set; get; }
        bool IsTime { set; get; }

        string LpNumEng { set; get; }
        string LpNumArab { set; get; }

        string Gate_Name { set; get; }

        DateTime FromTime { set; get; }
        DateTime ToTime { set; get; }

        DateTime FromDate { set; get; }
        DateTime ToDate { set; get; }

        event EventHandler btnResetReports;
        event EventHandler BtnReport;
    }
}
