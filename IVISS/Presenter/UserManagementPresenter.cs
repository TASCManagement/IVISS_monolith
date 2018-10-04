using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVISS.View;
using IVISS.Model;
using IVISS.Utility;

namespace IVISS.Presenter
{
    class UserManagementPresenter
    {
        IUserManagement view;
        UserManagementModel model;

        public UserManagementPresenter(IUserManagement userView)
        {
            view = userView;

            model = new UserManagementModel();

            view.BtnReset += View_BtnReset;
            view.BtnSave += View_BtnSave;
            view.BtnDelete += View_BtnDelete;

            view.RdoManager += View_RdoManager;
            view.RdoGuard += View_RdoGuard;
            view.FormLoad += View_FormLoad;

            this.FillGuards();
        }

        private void View_FormLoad(object sender, EventArgs e)
        {
            //Task t1 = Task.Run(() => this.FillGuards());
            this.FillGuards();
        }

        private void View_RdoGuard(object sender, EventArgs e)
        {
            FillGuards();
        }

        private void FillGuards()
        {
            view.SetGridSource(model.FillGuards());
        }

        private void View_RdoManager(object sender, EventArgs e)
        {
            FillManagers();
        }

        private void FillManagers()
        {
            view.SetGridSource(model.FillManagers());
        }

        private void View_BtnDelete(object sender, EventArgs e)
        {
            if (CheckForDeletion())
            {
                var frm = new frmMessageBox();
                frm.StatusMessage = "Are you sure you want to delete this user?";

                //if (MessageBox.Show("Are you sure you want to delete this record?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        model.id = view.id;

                        if (view.guardChecked)
                        {
                            model.DeleteGuard();
                            Task t = Task.Run(() => FillGuards());
                        }
                        else
                        {
                            model.DeleteManager();
                            Task t = Task.Run(() => FillManagers());
                        }

                        view.Reset();
                        Global.ShowMessage("User deleted Successfully");
                    }
                    catch (Exception ex)
                    {
                        Global.ShowMessage(ex.ToString());
                    }
                }
            }
        }

        private bool CheckForDeletion()
        {
            if (view.id.Length == 0)
            {
                Global.ShowMessage("Please select a user to delete");
                return false;

            }
            return true;
        }

        private void View_BtnSave(object sender, EventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            try
            {
                model.firstName = view.firstName;
                model.middleName = view.middleName;
                model.lastName = view.lastName;
                model.phone = view.phone;
                model.id = view.id;
                model.password = view.password;
                model.selectedID = view.selectedID;

                if (view.guardChecked)
                {
                    if (view.saveBtnCaption == "SAVE")
                    {                        
                        model.InsertGuard();
                    }
                    else
                    {
                        model.UpdateGuard();   
                    }

                    Task t = Task.Run(() => FillGuards());
                }
                else
                {
                    if (view.saveBtnCaption == "SAVE")
                    {
                        model.InsertManager();
                    }
                    else
                    {
                        model.UpdateManager();
                    }

                    Task t = Task.Run(() => FillManagers());
                }

                Global.ShowMessageDialog("User saved successfully");
                view.Reset();
                
            }
            catch (Exception ex)
            {
                Global.ShowMessage(ex.ToString());
            }
        }

        private void View_BtnReset(object sender, EventArgs e)
        {
            
        }

        private bool Validate()
        {
            bool recordExists = false;

            if (view.id.Length == 0)
            {
                Global.ShowMessage("ID cannot be left blanked");
                //this.txtID.Focus();

                return false;
            }

            if (view.password.Length == 0)
            {
                Global.ShowMessage("Password cannot be left blanked");
                //this.txtPassword.Focus();

                return false;
            }

            if (recordExists)
            {
                Global.ShowMessage("Cannot create users with existing ID");
                //this.txtID.Focus();

                return false;
            }

            return true;
        }
    }
}
