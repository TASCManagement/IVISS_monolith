namespace IVISS
{
    partial class frmVisitorSearch
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgView = new System.Windows.Forms.DataGridView();
            this.VisitorID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.License = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Accuracy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label13 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label68 = new System.Windows.Forms.Label();
            this.txtFirstnameVistortSearch = new MaterialSkin.Controls.MaterialSingleLineTextField();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLastnameVisitorSearch = new MaterialSkin.Controls.MaterialSingleLineTextField();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSearchVistorSearch = new MaterialSkin.Controls.MaterialRaisedButton();
            this.txtLPSearch = new MaterialSkin.Controls.MaterialSingleLineTextField();
            ((System.ComponentModel.ISupportInitialize)(this.dgView)).BeginInit();
            this.SuspendLayout();
            // 
            // dgView
            // 
            this.dgView.AllowUserToAddRows = false;
            this.dgView.AllowUserToDeleteRows = false;
            this.dgView.AllowUserToResizeColumns = false;
            this.dgView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
            this.dgView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgView.BackgroundColor = System.Drawing.Color.White;
            this.dgView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial Black", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(20);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.VisitorID,
            this.License,
            this.Path,
            this.DateTime,
            this.Accuracy});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgView.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgView.EnableHeadersVisualStyles = false;
            this.dgView.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(103)))), ((int)(((byte)(123)))));
            this.dgView.Location = new System.Drawing.Point(12, 191);
            this.dgView.Name = "dgView";
            this.dgView.ReadOnly = true;
            this.dgView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgView.RowHeadersVisible = false;
            this.dgView.RowTemplate.Height = 30;
            this.dgView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgView.Size = new System.Drawing.Size(1274, 711);
            this.dgView.TabIndex = 43;
            this.dgView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgView_CellDoubleClick);
            // 
            // VisitorID
            // 
            this.VisitorID.DataPropertyName = "VisitorID";
            this.VisitorID.HeaderText = "VisitorID";
            this.VisitorID.Name = "VisitorID";
            this.VisitorID.ReadOnly = true;
            this.VisitorID.Visible = false;
            // 
            // License
            // 
            this.License.DataPropertyName = "FirstName";
            this.License.FillWeight = 98.47716F;
            this.License.HeaderText = "First Name";
            this.License.Name = "License";
            this.License.ReadOnly = true;
            // 
            // Path
            // 
            this.Path.DataPropertyName = "MiddleName";
            this.Path.HeaderText = "Middle Name";
            this.Path.Name = "Path";
            this.Path.ReadOnly = true;
            // 
            // DateTime
            // 
            this.DateTime.DataPropertyName = "LastName";
            this.DateTime.FillWeight = 101.5228F;
            this.DateTime.HeaderText = "Last Name";
            this.DateTime.Name = "DateTime";
            this.DateTime.ReadOnly = true;
            // 
            // Accuracy
            // 
            this.Accuracy.DataPropertyName = "LicensePlate";
            this.Accuracy.HeaderText = "License Plate";
            this.Accuracy.Name = "Accuracy";
            this.Accuracy.ReadOnly = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.Transparent;
            this.label13.Font = new System.Drawing.Font("Arial Black", 12F);
            this.label13.ForeColor = System.Drawing.Color.DimGray;
            this.label13.Location = new System.Drawing.Point(12, 152);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(247, 23);
            this.label13.TabIndex = 45;
            this.label13.Text = "DOUBLE CLICK TO SELECT";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.BackColor = System.Drawing.Color.Transparent;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.ForeColor = System.Drawing.Color.White;
            this.label21.Location = new System.Drawing.Point(8, 10);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(130, 20);
            this.label21.TabIndex = 44;
            this.label21.Text = "ALL VISITORS";
            // 
            // label68
            // 
            this.label68.AutoSize = true;
            this.label68.BackColor = System.Drawing.Color.White;
            this.label68.Font = new System.Drawing.Font("Arial Black", 11F);
            this.label68.ForeColor = System.Drawing.Color.DarkGray;
            this.label68.Location = new System.Drawing.Point(25, 66);
            this.label68.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(158, 22);
            this.label68.TabIndex = 161;
            this.label68.Text = "LICENSE PLATE #";
            // 
            // txtFirstnameVistortSearch
            // 
            this.txtFirstnameVistortSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFirstnameVistortSearch.Depth = 0;
            this.txtFirstnameVistortSearch.Font = new System.Drawing.Font("Arial Black", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFirstnameVistortSearch.Hint = "";
            this.txtFirstnameVistortSearch.Location = new System.Drawing.Point(534, 47);
            this.txtFirstnameVistortSearch.MaxLength = 25;
            this.txtFirstnameVistortSearch.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtFirstnameVistortSearch.Name = "txtFirstnameVistortSearch";
            this.txtFirstnameVistortSearch.PasswordChar = '\0';
            this.txtFirstnameVistortSearch.SelectedText = "";
            this.txtFirstnameVistortSearch.SelectionLength = 0;
            this.txtFirstnameVistortSearch.SelectionStart = 0;
            this.txtFirstnameVistortSearch.Size = new System.Drawing.Size(299, 71);
            this.txtFirstnameVistortSearch.TabIndex = 165;
            this.txtFirstnameVistortSearch.TabStop = false;
            this.txtFirstnameVistortSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtFirstnameVistortSearch.UseSystemPasswordChar = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font = new System.Drawing.Font("Arial Black", 11F);
            this.label1.ForeColor = System.Drawing.Color.DarkGray;
            this.label1.Location = new System.Drawing.Point(425, 66);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 22);
            this.label1.TabIndex = 164;
            this.label1.Text = "First Name";
            // 
            // txtLastnameVisitorSearch
            // 
            this.txtLastnameVisitorSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLastnameVisitorSearch.Depth = 0;
            this.txtLastnameVisitorSearch.Font = new System.Drawing.Font("Arial Black", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLastnameVisitorSearch.Hint = "";
            this.txtLastnameVisitorSearch.Location = new System.Drawing.Point(959, 47);
            this.txtLastnameVisitorSearch.MaxLength = 25;
            this.txtLastnameVisitorSearch.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtLastnameVisitorSearch.Name = "txtLastnameVisitorSearch";
            this.txtLastnameVisitorSearch.PasswordChar = '\0';
            this.txtLastnameVisitorSearch.SelectedText = "";
            this.txtLastnameVisitorSearch.SelectionLength = 0;
            this.txtLastnameVisitorSearch.SelectionStart = 0;
            this.txtLastnameVisitorSearch.Size = new System.Drawing.Size(332, 71);
            this.txtLastnameVisitorSearch.TabIndex = 167;
            this.txtLastnameVisitorSearch.TabStop = false;
            this.txtLastnameVisitorSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtLastnameVisitorSearch.UseSystemPasswordChar = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Font = new System.Drawing.Font("Arial Black", 11F);
            this.label2.ForeColor = System.Drawing.Color.DarkGray;
            this.label2.Location = new System.Drawing.Point(850, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 22);
            this.label2.TabIndex = 166;
            this.label2.Text = "Last Name";
            // 
            // btnSearchVistorSearch
            // 
            this.btnSearchVistorSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSearchVistorSearch.Depth = 0;
            this.btnSearchVistorSearch.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearchVistorSearch.Icon = null;
            this.btnSearchVistorSearch.Location = new System.Drawing.Point(1051, 116);
            this.btnSearchVistorSearch.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSearchVistorSearch.Name = "btnSearchVistorSearch";
            this.btnSearchVistorSearch.Primary = true;
            this.btnSearchVistorSearch.Size = new System.Drawing.Size(235, 68);
            this.btnSearchVistorSearch.TabIndex = 168;
            this.btnSearchVistorSearch.Text = "Search";
            this.btnSearchVistorSearch.UseVisualStyleBackColor = true;
            this.btnSearchVistorSearch.Click += new System.EventHandler(this.btnSearchVistorSearch_Click);
            // 
            // txtLPSearch
            // 
            this.txtLPSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLPSearch.Depth = 0;
            this.txtLPSearch.Font = new System.Drawing.Font("Arial Black", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLPSearch.Hint = "";
            this.txtLPSearch.Location = new System.Drawing.Point(195, 44);
            this.txtLPSearch.MaxLength = 25;
            this.txtLPSearch.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtLPSearch.Name = "txtLPSearch";
            this.txtLPSearch.PasswordChar = '\0';
            this.txtLPSearch.SelectedText = "";
            this.txtLPSearch.SelectionLength = 0;
            this.txtLPSearch.SelectionStart = 0;
            this.txtLPSearch.Size = new System.Drawing.Size(208, 71);
            this.txtLPSearch.TabIndex = 169;
            this.txtLPSearch.TabStop = false;
            this.txtLPSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtLPSearch.UseSystemPasswordChar = false;
            // 
            // frmVisitorSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1298, 917);
            this.Controls.Add(this.txtLPSearch);
            this.Controls.Add(this.btnSearchVistorSearch);
            this.Controls.Add(this.txtLastnameVisitorSearch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFirstnameVistortSearch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label68);
            this.Controls.Add(this.dgView);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label21);
            this.Name = "frmVisitorSearch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.frmVisitorSearch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgView;
        private System.Windows.Forms.DataGridViewTextBoxColumn VisitorID;
        private System.Windows.Forms.DataGridViewTextBoxColumn License;
        private System.Windows.Forms.DataGridViewTextBoxColumn Path;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Accuracy;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label68;
        private MaterialSkin.Controls.MaterialSingleLineTextField txtFirstnameVistortSearch;
        private System.Windows.Forms.Label label1;
        private MaterialSkin.Controls.MaterialSingleLineTextField txtLastnameVisitorSearch;
        private System.Windows.Forms.Label label2;
        private MaterialSkin.Controls.MaterialRaisedButton btnSearchVistorSearch;
        private MaterialSkin.Controls.MaterialSingleLineTextField txtLPSearch;
    }
}