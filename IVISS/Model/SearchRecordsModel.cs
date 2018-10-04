using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;

namespace IVISS.Model
{
    class SearchRecordsModel
    {
        public string LpNumArabic { set; get; }
        public string LpNumEnglish { set; get; }
        public string RecordingPath { set; get; }
      
        public bool IsLicensePlate { set; get; }
        public bool IsDate { set; get; }
        public bool IsTime { set; get; }

        public DateTime ToDate { set; get; }
        public DateTime FromDate { set; get; }

        public DateTime ToTime { set; get; }
        public DateTime FromTime { set; get; }
        

        public DataTable ReturnGridSource()
        {
            //string lpEng = lpNumEnglish;
            //string lpArab = lpNumArabic;
            //bool isLicensePlate;
            //bool isDate;
            //bool isTime;

            using (var db = new IVISSEntities())
            {
                var prod = from p in db.Details
                           select new
                           {
                               p.visitor_id,
                               p.visitor_license_number,
                               p.visitor_license_number_arabic,
                               p.visitor_entry_date,
                               p.visitor_entry_time,
                               p.visitor_iviss_recording,
                               p.visitor_license_accuracy,
                               p.visitor_access_gate,
                               p.visitor_authorization,
                               p.visitor_access_status,
                               p.gate_no
                           };

                if (IsLicensePlate)
                {
                    if (LpNumEnglish.Trim().Length > 0)
                        prod = prod.Where(p => p.visitor_license_number.Contains(LpNumEnglish));

                    else if (LpNumArabic.Trim().Length > 0)
                        prod = prod.Where(p => p.visitor_license_number_arabic.Contains(DbFunctions.AsUnicode(LpNumArabic))); //DbFunctions.AsUnicode(lpArab)
                }

                if (IsDate)
                {
                    prod = prod.Where(p => EntityFunctions.TruncateTime(p.visitor_entry_date) >= EntityFunctions.TruncateTime(FromDate) && EntityFunctions.TruncateTime(p.visitor_entry_date) <= EntityFunctions.TruncateTime(ToDate));
                }

                if (IsTime)
                {
                    //check hours
                    prod = prod.Where(p => p.visitor_entry_time.Hour >= FromTime.Hour && p.visitor_entry_time.Hour <= ToTime.Hour);

                    //check minutes
                    prod = prod.Where(p => p.visitor_entry_time.Minute >= FromTime.Minute && p.visitor_entry_time.Minute <= ToTime.Minute);
                }

                // Execute the query
                var prodResult = prod.ToList();


                DataTable dt = new DataTable();
                dt.Columns.Add("VisitorID", typeof(String));
                dt.Columns.Add("License", typeof(String));
                dt.Columns.Add("Arabic", typeof(String));
                dt.Columns.Add("Path", typeof(String));
                dt.Columns.Add("DateTime", typeof(DateTime));
                dt.Columns.Add("Accuracy", typeof(String));
                dt.Columns.Add("Status", typeof(String));
                dt.Columns.Add("Classification", typeof(String));
                dt.Columns.Add("GateType", typeof(String));
                dt.Columns.Add("GateName", typeof(String));

                DataRow dr;

                foreach (var p in prodResult)
                {
                    dr = dt.NewRow();

                    dr["VisitorID"] = p.visitor_id;
                    dr["License"] = p.visitor_license_number;
                    dr["Arabic"] = p.visitor_license_number_arabic;
                    dr["Path"] = p.visitor_iviss_recording;
                    dr["DateTime"] = p.visitor_entry_time;
                    dr["Accuracy"] = p.visitor_license_accuracy;
                    dr["Status"] = p.visitor_access_status ?? string.Empty;
                    dr["Classification"] = p.visitor_authorization ?? "VISITOR";
                    dr["GateType"] = p.visitor_access_gate;
                    dr["GateName"] = "Gate" + p.gate_no;

                    dt.Rows.Add(dr);
                }

                return dt;
            }
        }

    }
}
