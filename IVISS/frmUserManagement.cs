using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IVISS.View;
using IVISS.Presenter;
using IVISS.Utility;

namespace IVISS
{
    public partial class frmUserManagement : Form, IUserManagement
    {
        public event EventHandler BtnSave;
        public event EventHandler BtnReset;
        public event EventHandler BtnDelete;
        public event EventHandler RdoManager;
        public event EventHandler RdoGuard;
        public event EventHandler FormLoad;

        UserManagementPresenter presenter;

        private string m_SelectedID = string.Empty;

        public string firstName { set { this.txtFirstName.Text = value; } get { return this.txtFirstName.Text; }}
        public string middleName
        {
            set { this.txtMiddleName.Text = value; }
            get { return this.txtMiddleName.Text; }
        }
        public string lastName
        {
            set { this.txtLastName.Text = value; }
            get { return this.txtLastName.Text; }
        }
        public string phone
        {
            set { this.txtPhone.Text = value; }
            get { return this.txtPhone.Text; }
        }
        public string id
        {
            set { this.txtID.Text = value; }
            get { return this.txtID.Text; }
        }
        public string password
        {
            set { this.txtPassword.Text = value; }
            get { return this.txtPassword.Text; }
        }

        public string selectedID
        {
            set { this.m_SelectedID = value; }
            get { return this.m_SelectedID; }
        }

        public bool managerChecked
        {
            set { this.rdoManager.Checked   = value; }
            get { return this.rdoManager.Checked; }
        }

        public bool guardChecked
        {
            set { this.rdoGuard.Checked = value; }
            get { return this.rdoGuard.Checked; }
        }

        public string saveBtnCaption
        {
            set { this.btnSave.Text = value; }
            get { return this.btnSave.Text; }
        }

        public frmUserManagement()
        {
            InitializeComponent();

            presenter = new UserManagementPresenter(this);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnSave.Text = "SAVE";

            this.txtFirstName.Clear();
            this.txtMiddleName.Clear();
            this.txtLastName.Clear();

            this.txtPhone.Clear();
            this.txtID.Clear();
            this.txtPassword.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            BtnSave(sender, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            BtnDelete(sender, e);
        }

        private void rdoManager_CheckedChanged(object sender, EventArgs e)
        {
            RdoManager(sender, e);
        }

        public void SetGridSource(DataTable dt)
        {
            //BeginInvoke((MethodInvoker)delegate
            //{
                dgGuards.DataSource = dt;
            //});
        }

        private void rdoGuard_CheckedChanged(object sender, EventArgs e)
        {
            RdoGuard(sender, e);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgGuards_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    m_SelectedID = this.txtID.Text = dgGuards.Rows[e.RowIndex].Cells[0].Value.ToString();
                    this.txtFirstName.Text = dgGuards.Rows[e.RowIndex].Cells[1].Value.ToString();
                    this.txtMiddleName.Text = dgGuards.Rows[e.RowIndex].Cells[2].Value.ToString();
                    this.txtLastName.Text = dgGuards.Rows[e.RowIndex].Cells[3].Value.ToString();
                    this.txtPassword.Text = StringCipher.Decrypt(dgGuards.Rows[e.RowIndex].Cells[4].Value.ToString(), Global.PASSPHRASE);
                    this.txtPhone.Text = dgGuards.Rows[e.RowIndex].Cells[5].Value.ToString();
                    //MessageBox.Show(s);

                    btnSave.Text = "UPDATE";
                }
            }
            catch (Exception ex) { /* MessageBox.Show(ex.ToString()); */ }
            finally { /* dgView.ReadOnly = false; */ }
        }

        public void Reset()
        {
            btnSave.Text = "SAVE";

            this.txtFirstName.Clear();
            this.txtMiddleName.Clear();
            this.txtLastName.Clear();

            this.txtPhone.Clear();
            this.txtID.Clear();
            this.txtPassword.Clear();

        }

        private void frmUserManagement_Load(object sender, EventArgs e)
        {
            FormLoad(sender, e);
        }
    }
}
