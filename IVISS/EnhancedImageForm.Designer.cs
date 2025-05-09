using KaiwaProjects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace IVISS
{
    partial class EnhancedImageForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

      

        private KpImageViewer kpImageViewer1;

        private System.Windows.Forms.Button btnScrollbars;

        private System.Windows.Forms.Button btnAnimations;

        private System.Windows.Forms.Label lblGifFPS;

        private System.Windows.Forms.Button btnApply;

        private System.Windows.Forms.TextBox tbFps;

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
            this.kpImageViewer1 = new KaiwaProjects.KpImageViewer();
            this.btnScrollbars = new System.Windows.Forms.Button();
            this.btnAnimations = new System.Windows.Forms.Button();
            this.lblGifFPS = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.tbFps = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // kpImageViewer1
            // 
            this.kpImageViewer1.AllowDrop = true;
            this.kpImageViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.kpImageViewer1.BackgroundColor = System.Drawing.Color.White;
            this.kpImageViewer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.kpImageViewer1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.kpImageViewer1.GifAnimation = true;
            this.kpImageViewer1.GifFPS = 15D;
            this.kpImageViewer1.Image = null;
            this.kpImageViewer1.Location = new System.Drawing.Point(5, 44);
            this.kpImageViewer1.MenuColor = System.Drawing.Color.White;
            this.kpImageViewer1.MenuPanelColor = System.Drawing.Color.White;
            this.kpImageViewer1.MinimumSize = new System.Drawing.Size(454, 157);
            this.kpImageViewer1.Name = "kpImageViewer1";
            this.kpImageViewer1.NavigationPanelColor = System.Drawing.Color.White;
            this.kpImageViewer1.NavigationTextColor = System.Drawing.SystemColors.ButtonHighlight;
            this.kpImageViewer1.OpenButton = false;
            this.kpImageViewer1.PreviewButton = true;
            this.kpImageViewer1.PreviewPanelColor = System.Drawing.Color.White;
            this.kpImageViewer1.PreviewText = "Preview";
            this.kpImageViewer1.PreviewTextColor = System.Drawing.SystemColors.ButtonHighlight;
            this.kpImageViewer1.Rotation = 0;
            this.kpImageViewer1.Scrollbars = true;
            this.kpImageViewer1.ShowPreview = true;
            this.kpImageViewer1.Size = new System.Drawing.Size(1281, 755);
            this.kpImageViewer1.TabIndex = 0;
            this.kpImageViewer1.TextColor = System.Drawing.SystemColors.ButtonHighlight;
            this.kpImageViewer1.Zoom = 100D;
            // 
            // btnScrollbars
            // 
            this.btnScrollbars.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScrollbars.Location = new System.Drawing.Point(1141, 804);
            this.btnScrollbars.Name = "btnScrollbars";
            this.btnScrollbars.Size = new System.Drawing.Size(0, 0);
            this.btnScrollbars.TabIndex = 1;
            this.btnScrollbars.Text = "Scrollbars off";
            this.btnScrollbars.UseVisualStyleBackColor = true;
            this.btnScrollbars.Visible = false;
            // 
            // btnAnimations
            // 
            this.btnAnimations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnimations.Location = new System.Drawing.Point(1141, 772);
            this.btnAnimations.Name = "btnAnimations";
            this.btnAnimations.Size = new System.Drawing.Size(0, 0);
            this.btnAnimations.TabIndex = 2;
            this.btnAnimations.Text = "Animations off";
            this.btnAnimations.UseVisualStyleBackColor = true;
            this.btnAnimations.Visible = false;
            // 
            // lblGifFPS
            // 
            this.lblGifFPS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGifFPS.AutoSize = true;
            this.lblGifFPS.Location = new System.Drawing.Point(1142, 753);
            this.lblGifFPS.Name = "lblGifFPS";
            this.lblGifFPS.Size = new System.Drawing.Size(0, 13);
            this.lblGifFPS.TabIndex = 3;
            this.lblGifFPS.Visible = false;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(1243, 747);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(0, 0);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Visible = false;
            // 
            // tbFps
            // 
            this.tbFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFps.Location = new System.Drawing.Point(1192, 749);
            this.tbFps.Name = "tbFps";
            this.tbFps.Size = new System.Drawing.Size(0, 20);
            this.tbFps.TabIndex = 5;
            this.tbFps.Text = "15";
            this.tbFps.Visible = false;
            // 
            // EnhancedImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1295, 837);
            this.Controls.Add(this.tbFps);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lblGifFPS);
            this.Controls.Add(this.btnAnimations);
            this.Controls.Add(this.btnScrollbars);
            this.Controls.Add(this.kpImageViewer1);
            this.Name = "EnhancedImageForm";
            this.Load += new System.EventHandler(this.EnhancedImageForm_Load);
            this.Shown += new System.EventHandler(this.EnhancedImageForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}