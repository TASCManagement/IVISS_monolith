using IVISS.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmVisitorDataEntry : Form
    {
        private string m_Picture;
        private int m_VisitorID = 0;

        public frmVisitorDataEntry()
        {
            InitializeComponent();
        }

        private bool Validate()
        {
            if (this.txtLPNumEnglish.Text.Length == 0 && this.txtLPNumArabic.Text.Length == 0)
            {
                Global.ShowMessage("License plate cannot be blank", false);

                if (this.txtLPNumEnglish.Text.Length == 0)
                    this.txtLPNumEnglish.Focus();

                if (this.txtLPNumArabic.Text.Length == 0)
                    this.txtLPNumArabic.Focus();

                return false;
            }

            if (this.txtFirstName.Text.Length == 0)
            {
                Global.ShowMessage("First name cannot be blank", false);
                this.txtFirstName.Focus();
                return false;
            }

            if (this.txtLastName.Text.Length == 0)
            {
                Global.ShowMessage("Last name cannot be blank", false);
                this.txtLastName.Focus();
                return false;
            }

            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validate())
                {
                    return;
                }

                if (m_VisitorID > 0)
                {
                    using (var db = new IVISSEntities())
                    {
                        var visitors = (from v in db.Visitors
                                        where v.visitor_id == m_VisitorID
                                        select v).FirstOrDefault();

                        if (visitors != null)
                        {

                            visitors.visitor_license_plate = this.txtLPNumEnglish.Text;
                            visitors.visitor_license_plate_arabic = this.txtLPNumArabic.Text;

                            visitors.visitor_authorization = this.cboAuthorization.Text;
                            visitors.visitor_classification = this.cboClassification.Text;

                            visitors.visitor_first = this.txtFirstName.Text;
                            visitors.visitor_middle = this.txtMiddleName.Text;
                            visitors.visitor_last = this.txtLastName.Text;

                            visitors.visitor_address = this.txtAddress.Text;
                            visitors.visitor_city = this.txtCity.Text;
                            visitors.visitor_country = this.txtCountry.Text;
                            visitors.visitor_email = this.txtEmail.Text;
                            visitors.visitor_phone_1 = this.txtPhone1.Text;
                            visitors.visitor_phone_2 = this.txtPhone2.Text;
                            visitors.visitor_region = this.txtRegion.Text;

                            //visitors.visitor_manager = 

                            visitors.uploaded = 0;

                            if (m_Picture != null && m_Picture.Length > 0)
                            {
                                visitors.visitor_image = m_Picture;
                            }

                            db.SaveChanges();

                            m_VisitorID = 0;
                        }

                        Global.ShowMessage("Record Updated Successfully", false);

                    }
                }
                else
                {
                    try
                    {
                        using (var db = new IVISSEntities())
                        {
                            var v = new Visitor();


                            v.visitor_license_plate = this.txtLPNumEnglish.Text;
                            v.visitor_license_plate_arabic = this.txtLPNumArabic.Text;

                            v.visitor_classification = this.cboClassification.Text;
                            v.visitor_authorization = this.cboAuthorization.Text;
                            v.visitor_middle = this.txtMiddleName.Text;
                            v.visitor_first = this.txtFirstName.Text;

                            v.visitor_last = this.txtLastName.Text;
                            v.visitor_address = this.txtAddress.Text;
                            v.visitor_city = this.txtCity.Text;
                            v.visitor_region = this.txtRegion.Text;
                            v.visitor_country = this.txtCountry.Text;
                            v.visitor_phone_1 = this.txtPhone1.Text;
                            v.visitor_phone_2 = this.txtPhone2.Text;

                            v.visitor_email = this.txtEmail.Text;

                            v.uploaded = 0;

                            //v.visitor_manager = (int)this.cboManager.SelectedValue;
                            //v.visitor_organization = (int)this.cboOrganization.SelectedValue;
                            //v.visitor_facility = (int)this.cboFacility.SelectedValue;

                            if (m_Picture != null && m_Picture.Length > 0)
                            {
                                v.visitor_image = m_Picture;
                            }

                            db.Visitors.Add(v);
                            db.SaveChanges();


                        }
                        Global.ShowMessage("Record Saved Successfully", false);
                    }
                    catch (Exception oex)
                    {
                        Global.ShowMessage("Visitor First/Last Name cannot be left blanked or use a different value for Visitor First/Last Name", false);
                        Global.AppendString(oex.ToString());
                        return;
                    }
                }

                ClearTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ClearTextBoxes()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };

            func(Controls);

            pBoxPicture.BackgroundImage = null;

            this.cboAuthorization.SelectedIndex = 0;
            this.cboClassification.SelectedIndex = 0;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var frm = new frmVisitorSearch();
            frm.ShowDialog();

            try
            {
                m_VisitorID = int.Parse(frm.Visitor);

                using (var db = new IVISSEntities())
                {
                    var visitors = (from v in db.Visitors
                                    where v.visitor_id == m_VisitorID
                                    select v).FirstOrDefault();

                    if (visitors != null)
                    {

                        this.txtLPNumEnglish.Text = visitors.visitor_license_plate;
                        this.txtLPNumArabic.Text = visitors.visitor_license_plate_arabic;

                        this.cboAuthorization.Text = visitors.visitor_authorization.Trim();
                        this.cboClassification.Text = visitors.visitor_classification.Trim();



                        this.txtFirstName.Text = visitors.visitor_first;
                        this.txtMiddleName.Text = visitors.visitor_middle;
                        this.txtLastName.Text = visitors.visitor_last;

                        this.txtAddress.Text = visitors.visitor_address;
                        this.txtCity.Text = visitors.visitor_city;
                        this.txtCountry.Text = visitors.visitor_country;
                        this.txtEmail.Text = visitors.visitor_email;
                        this.txtPhone1.Text = visitors.visitor_phone_1;
                        this.txtPhone2.Text = visitors.visitor_phone_2;
                        this.txtRegion.Text = visitors.visitor_region;
                        m_Picture = visitors.visitor_image;


                        if (m_Picture != null && m_Picture.Length > 0)
                        {
                            pBoxPicture.BackgroundImage = Image.FromFile(m_Picture);
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            m_VisitorID = 0;
            ClearTextBoxes();
        }

        private void txtLPNumArabic_Click(object sender, EventArgs e)
        {
            var f = new frmArabicKeyboard();
            //f.licensePlate = this.txtLicensePlateArab.Text;
            f.ShowDialog();

            if (f.m_LicensePlate != null && f.m_LicensePlate.Length > 0)
                this.txtLPNumArabic.Text = f.m_LicensePlate;
        }

        private void btnChangePicture_Click(object sender, EventArgs e)
        {
            try
            {
                if (oFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var sourceFile = oFD.FileName;
                    pBoxPicture.BackgroundImage = Image.FromFile(sourceFile); //m_Picture

                    if (!Directory.Exists("Pictures"))
                    {
                        Directory.CreateDirectory("Pictures");
                    }

                    var ticks = (int)DateTime.Now.Ticks;
                    var filename = ticks + ".jpg";
                    m_Picture = "Pictures\\" + filename;

                    File.Copy(sourceFile, m_Picture);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
