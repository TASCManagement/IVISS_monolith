using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVISS.View;
using IVISS.Model; 

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

            view.BtnReset += view_BtnReset;
            view.BtnSearch += view_BtnSearch;
        }

        void view_BtnSearch(object sender, EventArgs e)
        {
            //BeginInvoke((MethodInvoker)delegate
            //{
            //    dgView.DataSource = dt;
            //});

            model.IsDate = view.isDate;
            model.IsLicensePlate = view.isLicensePlate;
            model.IsTime = view.isTime;

            model.LpNumArabic = view.lpNumArab;
            model.LpNumEnglish = view.lpNumEng;

            model.FromDate = view.fromDate;
            model.ToDate = view.toDate;

            model.FromTime = view.fromTime;
            model.ToTime = view.toTime;

            view.BindData(model.ReturnGridSource());
        }

        void view_BtnReset(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


    }
}
