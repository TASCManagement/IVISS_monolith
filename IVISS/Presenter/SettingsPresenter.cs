using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVISS.View;

namespace IVISS.Presenter
{
    class SettingsPresenter
    {
        ISettings view;

        public SettingsPresenter(ISettings settingsView)
        {
            view = settingsView;

            view.BtnSystemSettings += view_BtnSystemSettings;
            view.BtnUserManagement += view_BtnUserManagement;
            view.BtnReports += view_BtnReports;
            view.BtnAccessControls += view_BtnAccessControls;
            view.BtnVisitorDataEntry += View_BtnVisitorDataEntry;
    
        }

        private void View_BtnVisitorDataEntry(object sender, EventArgs e)
        {
            var frm = new frmVisitorDataEntry();
            frm.Show();
        }

        void view_BtnAccessControls(object sender, EventArgs e)
        {
            var frm = new frmAccessControls();
            frm.Show();
        }

        void view_BtnReports(object sender, EventArgs e)
        {
            var frm = new frmReports();
            frm.Show();
        }

        void view_BtnUserManagement(object sender, EventArgs e)
        {
            var frm = new frmUserManagement();
            frm.Show();
        }

        void view_BtnSystemSettings(object sender, EventArgs e)
        {
            var frm = new frmSystemSettings();
            frm.Show();
        }
    }
}
