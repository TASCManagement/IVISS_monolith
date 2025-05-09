using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVISS.View;
using IVISS.Model;
using System.Windows.Forms;

namespace IVISS.Presenter
{
    class SearchRecordsPresenter
    {
        ISearchRecords view;
        SearchRecordsModel model;

        public SearchRecordsPresenter(ISearchRecords mainView)
        {
            view = mainView;

            model = new SearchRecordsModel();

            view.BtnResetSearchRecords += view_BtnReset;
            view.BtnSearchSearchRecords += view_BtnSearch;
            view.ChkSetDefault += View_ChkSetDefault;
        }

        private void View_ChkSetDefault(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            var chk = (CheckBox)sender;

            model.LpNumEnglish = view.lpNumEngSearchRecords;
            model.LpNumArabic = view.lpNumArabSearchRecords;
            model.RecordingPath = view.recordingPathSearchRecords;

            model.SetDefault(chk.Checked);
            //MessageBox.Show(chk.Checked.ToString());
        }

        void view_BtnSearch(object sender, EventArgs e)
        {
            //BeginInvoke((MethodInvoker)delegate
            //{
            //    dgView.DataSource = dt;
            //});

            model.IsDate = view.isDateSearchRecords;
            model.IsLicensePlate = view.isLicensePlateSearchRecords;
            model.IsTime = view.isTimeSearchRecords;

            model.LpNumArabic = view.lpNumArabSearchRecords;
            model.LpNumEnglish = view.lpNumEngSearchRecords;

            model.FromDate = view.fromDateSearchRecords;
            model.ToDate = view.toDateSearchRecords;

            model.FromTime = view.fromTimeSearchRecords;
            model.ToTime = view.toTimeSearchRecords;

            model.isAdditionalALPR = view.isAdditionalALPR;

            view.BindDataSearchReords(model.ReturnGridSource());
        }

        void view_BtnReset(object sender, EventArgs e)
        {
            view.BindDataSearchReords(model.ResetGridSource());
            //throw new NotImplementedException();
        }
    }
}
