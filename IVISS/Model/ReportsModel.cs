using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.Model
{
    class ReportsModel
    {
        public string LpNumArabic { set; get; }
        public string LpNumEnglish { set; get; }
        public string RecordingPath { set; get; }

        public bool IsLicensePlate { set; get; }

        public bool IsAdditionalALPR { set; get; }

       
        public bool IsDate { set; get; }
        public bool IsTime { set; get; }

        public DateTime ToDate { set; get; }
        public DateTime FromDate { set; get; }

        public DateTime ToTime { set; get; }
        public DateTime FromTime { set; get; }

        public string Gate_Name { set; get; }
        

        public IEnumerable<Object> GetData()
        {
            DateTime defaultDate = Convert.ToDateTime("1/1/1900");

            using (IVISSEntities db = new IVISSEntities())
            {

                var detailQuery = from d in db.Details
                                  select new
                                  {
                                      visitor_entry_date = d.visitor_entry_date,
                                      visitor_entry_time = d.visitor_entry_time,
                                      d.visitor_license_number,
                                      visitor_license_number_arabic = d.visitor_license_number_arabic ?? string.Empty,
                                      visitor_license_region = d.visitor_license_region ?? string.Empty,
                                      visitor_license_country = d.visitor_license_country ?? string.Empty,
                                      visitor_license_back_color = d.visitor_license_back_color ?? string.Empty,
                                      visitor_license_fore_color = d.visitor_license_fore_color ?? string.Empty,
                                      visitor_license_type = d.visitor_license_type ?? string.Empty,
                                      visitor_license_accuracy = d.visitor_license_accuracy ?? 0,
                                      visitor_access_gate = d.gate_no ?? string.Empty,
                                      visitor_access_status = d.visitor_access_status ?? string.Empty,
                                      visitor_license_prefix = d.visitor_license_prefix ?? string.Empty,
                                      visitor_exit_gate = d.visitor_exit_gate ?? 0,
                                      visitor_exit_time = d.visitor_exit_time ?? defaultDate,
                                      visitor_exit_date = d.visitor_exit_date ?? defaultDate,
                                      d.visitor_entry_gate,
                                      d.visitor_authorization,
                                      IsPrimary=d.is_primary??-1
                                  };

                if(Gate_Name!="")
                {
                    detailQuery = detailQuery.Where(p => p.visitor_access_gate == Gate_Name);
                }

                if (IsAdditionalALPR==false)
                {
                    detailQuery = detailQuery.Where(p => p.IsPrimary != 0);
                }

                if (IsLicensePlate)
                {
                    if (LpNumEnglish.Trim().Length > 0)
                        detailQuery = detailQuery.Where(p => p.visitor_license_number == LpNumEnglish);

                    else if (LpNumArabic.Trim().Length > 0)
                        detailQuery = detailQuery.Where(p => p.visitor_license_number_arabic.Contains(DbFunctions.AsUnicode(LpNumArabic)));
                }

                if (IsDate)
                {
                    detailQuery = detailQuery.Where(p => EntityFunctions.TruncateTime(p.visitor_entry_date) >= EntityFunctions.TruncateTime(FromDate) && EntityFunctions.TruncateTime(p.visitor_entry_date) <= EntityFunctions.TruncateTime(ToDate));
                    //detailQuery = detailQuery.Where(p => p.visitor_entry_date >= dtpFromDate.Value && p.visitor_entry_date <= dtpToDate.Value);
                }

                if (IsTime)
                {
                    //check hours
                    detailQuery = detailQuery.Where(p => p.visitor_entry_time.Hour >= FromTime.Hour && p.visitor_entry_time.Hour <= ToTime.Hour);

                    //check minutes
                    detailQuery = detailQuery.Where(p => p.visitor_entry_time.Minute >= FromTime.Minute && p.visitor_entry_time.Minute <= ToTime.Minute);
                }

                return detailQuery.ToList();
                
            }
        }
    }
}
