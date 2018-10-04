using IVISS.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace IVISS.Model
{
    class MainModel
    {
        public string lpNumArabic { set; get; }
        public string lpNumEnglish { set; get; }
        public string recordingPath { set; get; }
        public DataTable ReturnGridSource()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("License", typeof(String));
                dt.Columns.Add("ArabicLicense", typeof(String));
                dt.Columns.Add("Date", typeof(DateTime));
                dt.Columns.Add("Gate", typeof(String));
                dt.Columns.Add("Status", typeof(String));
                dt.Columns.Add("Path", typeof(String));
                dt.Columns.Add("Accuracy", typeof(String));
                dt.Columns.Add("Classification", typeof(String));

                //dgView.DataSource = dt;

                int gateNo = Global.m_Gate_No;

                using (IVISSEntities db = new IVISSEntities())
                {
                    // .Take(5)
                    // where objDetails.gate_no == gateNo

                    var detailQuery = from objDetails in db.Details  
                                      where objDetails.visitor_license_number != ""
                                      orderby objDetails.visitor_entry_time descending
                                      select new { objDetails.visitor_license_number, objDetails.visitor_license_number_arabic, objDetails.visitor_entry_date, objDetails.visitor_entry_time, objDetails.visitor_iviss_recording, objDetails.visitor_license_accuracy, objDetails.visitor_access_status, objDetails.visitor_access_gate, objDetails.visitor_authorization };

                    detailQuery = detailQuery.Take(6);

                    DataRow dr;

                    foreach (var detail in detailQuery)
                    {
                        dr = dt.NewRow();
                        dr["License"] = detail.visitor_license_number;
                        dr["ArabicLicense"] = detail.visitor_license_number_arabic;
                        dr["Date"] = detail.visitor_entry_time; //detail.visitor_entry_date.ToShortDateString() + " " + detail.visitor_entry_time.ToShortTimeString(); 
                        dr["Gate"] = detail.visitor_access_gate;
                        dr["Status"] = detail.visitor_access_status; //(detail.visitor_license_number.Trim().ToLower()=="xac5831")?"DENIED":"ALLOWED";
                        dr["Path"] = detail.visitor_iviss_recording;
                        dr["Accuracy"] = detail.visitor_license_accuracy;
                        dr["Classification"] = detail.visitor_authorization;

                        dt.Rows.Add(dr);
                    }
                }

                return dt;
            }
            catch (Exception ex) 
            {
                return new DataTable();
            }
        }
        public SystemSetting FillSettings()
        {
            using (var db = new IVISSEntities())
            {
                var query = (from settings in db.SystemSettings
                             select settings).FirstOrDefault();

                return query;
            }
        }
    }
}
