using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmVisitorSearch : MaterialForm
    {
        private string m_Visitor = string.Empty;

        // Declare a Name property of type string:
        public string Visitor
        {
            get
            {
                return m_Visitor;
            }
            set
            {
                m_Visitor = value;
            }
        }

        public frmVisitorSearch()
        {
            InitializeComponent();
        }

        private void frmVisitorSearch_Load(object sender, EventArgs e)
        {
            this.FillGrid();
        }

        private void FillGrid()
        {
            using (var db = new IVISSEntities())
            {
                var visitors = from v in db.Visitors
                               where v.isDeleted != 1
                               orderby v.visitor_id descending
                               select new { v.visitor_id, v.visitor_first, v.visitor_middle, v.visitor_last, v.visitor_license_plate, v.visitor_license_plate_arabic };

                //if (m_LicensePlate)
                //{
                //    prod = prod.Where(p => p.visitor_license_number == lp || p.visitor_license_number_arabic == lpArab);
                //}

                //if (m_Date)
                //{
                //    prod = prod.Where(p => p.visitor_entry_date >= dtpFromDate.Value && p.visitor_entry_date <= dtpToDate.Value);
                //}

                //if (m_Time)
                //{
                //    prod = prod.Where(p => p.visitor_entry_time == p2);
                //}

                // Execute the query

                var visResult = visitors.ToList();

                //
                DataTable dt = new DataTable();

                dt.Columns.Add("VisitorID", typeof(String));
                dt.Columns.Add("FirstName", typeof(String));
                dt.Columns.Add("MiddleName", typeof(String));
                dt.Columns.Add("LastName", typeof(String));
                dt.Columns.Add("LicensePlate", typeof(String));

                DataRow dr;

                foreach (var p in visResult)
                {
                    dr = dt.NewRow();

                    dr[0] = p.visitor_id;
                    dr[1] = p.visitor_first;
                    dr[2] = p.visitor_middle;
                    dr[3] = p.visitor_last;
                    dr[4] = p.visitor_license_plate + " " + p.visitor_license_plate_arabic;

                    dt.Rows.Add(dr);
                }

                BeginInvoke((MethodInvoker)delegate
                {
                    dgView.DataSource = dt;
                });
            }
        }

        private void FillGridSearch()
        {
            using (var db = new IVISSEntities())
            {
                var visitors = from v in db.Visitors
                               where v.isDeleted!=1 
                               orderby v.visitor_id descending
                               select new { v.visitor_id, v.visitor_first, v.visitor_middle, v.visitor_last, v.visitor_license_plate, v.visitor_license_plate_arabic };



                if (txtLPSearch.Text != "")
                {
                    visitors = visitors.Where(p => p.visitor_license_plate.Contains(txtLPSearch.Text));
                }



                if (txtFirstnameVistortSearch.Text!="")
                {
                    visitors = visitors.Where(p => p.visitor_first.Contains(txtFirstnameVistortSearch.Text));
                }


                if (txtLastnameVisitorSearch.Text != "")
                {
                    visitors = visitors.Where(p => p.visitor_last.Contains(txtLastnameVisitorSearch.Text));
                }


                //if (m_Date)
                //{
                //    prod = prod.Where(p => p.visitor_entry_date >= dtpFromDate.Value && p.visitor_entry_date <= dtpToDate.Value);
                //}

                //if (m_Time)
                //{
                //    prod = prod.Where(p => p.visitor_entry_time == p2);
                //}

                // Execute the query

                var visResult = visitors.ToList();

                //
                DataTable dt = new DataTable();

                dt.Columns.Add("VisitorID", typeof(String));
                dt.Columns.Add("FirstName", typeof(String));
                dt.Columns.Add("MiddleName", typeof(String));
                dt.Columns.Add("LastName", typeof(String));
                dt.Columns.Add("LicensePlate", typeof(String));

                DataRow dr;

                foreach (var p in visResult)
                {
                    dr = dt.NewRow();

                    dr[0] = p.visitor_id;
                    dr[1] = p.visitor_first;
                    dr[2] = p.visitor_middle;
                    dr[3] = p.visitor_last;
                    dr[4] = p.visitor_license_plate + " " + p.visitor_license_plate_arabic;

                    dt.Rows.Add(dr);
                }

                BeginInvoke((MethodInvoker)delegate
                {
                    dgView.DataSource = dt;
                });
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Visitor = dgView.Rows[e.RowIndex].Cells[0].Value.ToString();
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var db = new IVISSEntities())
            {
                db.Database.ExecuteSqlCommand("delete from Visitor");
            }
        }

        private void btnSearchVistorSearch_Click(object sender, EventArgs e)
        {
            FillGridSearch();
        }
    }
}
