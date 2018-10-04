namespace IVISS
{
    partial class frmTest
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
            this.btnClickSnapshot = new System.Windows.Forms.Button();
            this.tbDriver = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.tbDriver)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClickSnapshot
            // 
            this.btnClickSnapshot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClickSnapshot.FlatAppearance.BorderSize = 0;
            this.btnClickSnapshot.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnClickSnapshot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnClickSnapshot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClickSnapshot.Font = new System.Drawing.Font("Open Sans Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClickSnapshot.Location = new System.Drawing.Point(8, 8);
            this.btnClickSnapshot.Name = "btnClickSnapshot";
            this.btnClickSnapshot.Size = new System.Drawing.Size(239, 31);
            this.btnClickSnapshot.TabIndex = 53;
            this.btnClickSnapshot.Text = "Click to take snapshot";
            this.btnClickSnapshot.UseVisualStyleBackColor = true;
            // 
            // tbDriver
            // 
            this.tbDriver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(46)))), ((int)(((byte)(60)))));
            this.tbDriver.Location = new System.Drawing.Point(142, 113);
            this.tbDriver.Name = "tbDriver";
            this.tbDriver.Size = new System.Drawing.Size(239, 50);
            this.tbDriver.TabIndex = 54;
            this.tbDriver.Visible = false;
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tbDriver);
            this.Controls.Add(this.btnClickSnapshot);
            this.Name = "frmTest";
            this.Text = "frmTest";
            ((System.ComponentModel.ISupportInitialize)(this.tbDriver)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClickSnapshot;
        private System.Windows.Forms.TrackBar tbDriver;
    }
}