using IVISS.Utility;
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
    public partial class frmAddAdditionALPRSettings : MaterialForm
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

        public frmAddAdditionALPRSettings()
        {
            InitializeComponent();
        }

        private void frmVisitorSearch_Load(object sender, EventArgs e)
        {
            this.FillALPR();
        }

        private void FillALPR()
        {

            using (var db = new IVISSEntities())
            {
                var alpr = (from a in db.AdditionalALPRs select a).FirstOrDefault();

                if (alpr != null) //new ALPR
                {


                    txtALPRGate2IP.Text = alpr.ALPRIP;
                    chkAIAdditionalALPRSettings.Checked = alpr.AIEnabled ??false;
                    chkLoopAdditionalALPRSettings.Checked=alpr.LoopEnabled??false  ;
                    txtGateNameAdditionalALPRSettings.Text=alpr.GateName ;

                    
                    

                    
                }


            }

        }

        private void btnSaveAdditionalALPRSettings_Click(object sender, EventArgs e)
        {
            try
            {

                if(txtALPRGate2IP.Text=="" || txtGateNameAdditionalALPRSettings.Text=="")
                {

                    BeginInvoke((MethodInvoker)delegate
                    {
                        lblMessageAdditionalALPR.Text = "ALPR IP and Gate name must be entered";
                    });

                    return;
                }



                using (var db = new IVISSEntities())
                {
                    var found = (from a in db.AdditionalALPRs select a).FirstOrDefault();

                    if (found == null) //new ALPR
                    {
                        AdditionalALPR alpr = new AdditionalALPR();

                        alpr.ALPRIP = txtALPRGate2IP.Text;
                        alpr.AIEnabled = chkAIAdditionalALPRSettings.Checked;
                        alpr.LoopEnabled = chkLoopAdditionalALPRSettings.Checked;
                        alpr.GateName = txtGateNameAdditionalALPRSettings.Text;

                        db.AdditionalALPRs.Add(alpr);
                        db.SaveChanges();

                        BeginInvoke((MethodInvoker)delegate
                        {
                            lblMessageAdditionalALPR.Text = "ALPR added successfully,Please restart application to see effect";
                        });

                      
                    }
                    else
                    {
                        found.ALPRIP = txtALPRGate2IP.Text;
                        found.AIEnabled = chkAIAdditionalALPRSettings.Checked;
                        found.LoopEnabled = chkLoopAdditionalALPRSettings.Checked;
                        found.GateName = txtGateNameAdditionalALPRSettings.Text;


                        db.SaveChanges();

                        BeginInvoke((MethodInvoker)delegate
                        {
                            lblMessageAdditionalALPR.Text = "ALPR updated successfully,Please restart application to see effect";
                        });
                        
                    }

                    
                }
            }
            catch (Exception)
            {


                BeginInvoke((MethodInvoker)delegate
                {
                    lblMessageAdditionalALPR.Text = "Unable to save ALPR";
                });

                
            }


        }
    }
}
