using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVISS
{
    public partial class frmRptViewer : Form
    {
        public frmRptViewer(ReportDocument rptDoc)
        {
            InitializeComponent();

            this.crViewer.ReportSource = rptDoc;
        }

        private void crystalReportViewer2_Load(object sender, EventArgs e)
        {
            int exportFormatFlags = (int)(CrystalDecisions.Shared.ViewerExportFormats.PdfFormat); //| CrystalDecisions.Shared.ViewerExportFormats.ExcelFormat
            crViewer.AllowedExportFormats = exportFormatFlags;
        }
    }
}
