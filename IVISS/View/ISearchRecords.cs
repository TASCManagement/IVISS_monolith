using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace IVISS.View
{
    interface ISearchRecords
    {
        bool isLicensePlateSearchRecords { set; get; }
        bool isDateSearchRecords { set; get; }
        bool isTimeSearchRecords { set; get; }

        string lpNumEngSearchRecords { set; get; }
        string lpNumArabSearchRecords { set; get; }
        string recordingPathSearchRecords { set; get; }

        DateTime fromTimeSearchRecords { set; get; }
        DateTime toTimeSearchRecords { set; get; }

        DateTime fromDateSearchRecords { set; get; }
        DateTime toDateSearchRecords { set; get; }


        bool isAdditionalALPR { set; get; }

        void BindDataSearchReords(DataTable dt);

        event EventHandler BtnResetSearchRecords;
        event EventHandler BtnSearchSearchRecords;
        event EventHandler ChkSetDefault;
    }
}
