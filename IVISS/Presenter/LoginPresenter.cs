using IVISS.Model;
using IVISS.Utility;
using IVISS.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace IVISS.Presenter
{
    class LoginPresenter
    {
        ILogin view;
        LoginModel model;

        public LoginPresenter(ILogin loginView)
        {
            view = loginView;
            model = new LoginModel();

            view.BtnOk += view_BtnOk;
           
        }

        void view_BtnOk(object sender, EventArgs e)
        {
            try
            {

                if(view.password=="")
                {
                    view.errMsg = "Password cannot be left blanked";
                    return;
                }

                if (view.username == "")
                {
                    view.errMsg = "User name cannot be left blanked";
                    return;
                }

                if (ValidateControls())
                { 
                    //Abc123~!@
                    model.username = view.username;
                    model.password = StringCipher.Encrypt(view.password, Global.PASSPHRASE);

                    Global.USER_TYPE = model.Login();

                    if (Global.USER_TYPE == "ADMIN" || Global.USER_TYPE == "MANAGER" || Global.USER_TYPE == "GUARD")
                    {
                        Global.USER_NAME = view.username;
                        view.CloseForm();
                    }
                    else
                    {
                        //global.ShowMessage("Wrong username or password. Please try again");
                        view.errMsg = "Wrong username or password. Please try again";
                    }

                    //&& (p.gate_no == global.m_Gate_No) 
                }

            }
            catch (Exception ex)
            {
                //lblErrMsg.Text = "Failed to establish a connection with the database. Please verify that the \n database server is online";

                view.errMsg = ex.InnerException.ToString();
            }
            finally
            {
            }
        }

        private bool ValidateControls()
        {
            if (view.username.Length == 0)
            {
                MetroFramework.MetroMessageBox.Show(view.currentForm, "Username cannot be left blanked", Global.COMPANY_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (view.password.Length == 0)
            {
                MetroFramework.MetroMessageBox.Show(view.currentForm, "Password cannot be left blanked", Global.COMPANY_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
