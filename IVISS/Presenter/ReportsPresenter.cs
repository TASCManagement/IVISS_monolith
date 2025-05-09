using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVISS.View;
using IVISS.Model;
using System.Data;
using IVISS.Utility;

namespace IVISS.Presenter
{
    class ReportsPresenter
    {
        IReports view;
        ReportsModel model;

        public static rptListing mainReportObject;

        public static rptListingEngOnly mainReportObjectEng;

        public ReportsPresenter(IReports repView)
        {
            view = repView;
            model = new ReportsModel();

            view.BtnReport += View_BtnReport;
            view.btnResetReports += View_BtnReset;
        }

        private void View_BtnReset(object sender, EventArgs e)
        {
            view.IsLicensePlate = false;
            view.IsDate = false;
            view.IsTime = false;
            view.LpNumArab = "";
            view.LpNumEng = "";
            view.FromDate = DateTime.Now;
            view.ToDate = DateTime.Now;
            view.Gate_Name = "";
            view.IsAdditionalALPR = false;
            view.FromTime = DateTime.Now;
            view.ToTime = DateTime.Now;

        }

        private void View_BtnReport(object sender, EventArgs e)
        {
            model.IsDate = view.IsDate;
            model.IsLicensePlate = view.IsLicensePlate;
            model.IsTime = view.IsTime;

            model.LpNumArabic = view.LpNumArab;
            model.LpNumEnglish = view.LpNumEng;

            model.FromDate = view.FromDate;
            model.ToDate = view.ToDate;

            model.FromTime = view.FromTime;
            model.ToTime = view.ToTime;
            model.Gate_Name = view.Gate_Name;
            model.IsAdditionalALPR = view.IsAdditionalALPR;
            // var rptDoc = new CrystalReport1();

            DateTime defaultDate = Convert.ToDateTime("1/1/1900");

          //  model.GetData();

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
                                        d.is_primary
                                    };

                if (Global.ProfileMode == "English")
                {
                    var rptDoc = new rptListingEngOnly();
                    rptDoc.Load(@"rptListingEngOnly.rpt");
                    rptDoc.SetDataSource(model.GetData());

                    rptDoc.SetParameterValue("gName", "");

                    mainReportObject = null;
                    mainReportObjectEng = rptDoc;
                }
                else
                {
                    var rptDoc = new rptListing();
                    rptDoc.Load(@"rptListing.rpt");
                    rptDoc.SetDataSource(model.GetData());

                    rptDoc.SetParameterValue("gName", "");

                    mainReportObject = rptDoc;
                    mainReportObjectEng = null;
                }
              

              
            }

            //rptDoc.Load(@"CrystalReport1.rpt");
            //rptDoc.SetDataSource(dt);
            //rptDoc.SetDataSource(model.GetData());

       
            //rptDoc.SetParameterValue("gName", global.m_Gate_Name);
           //var frmRptViewer = new frmRptViewer(rptDoc); // rptDoc
          //  frmRptViewer.Show();
        }

    }
}
