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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTest));
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTbDriver = new MetroFramework.Controls.MetroPanel();
            this.btnClickSnapshot = new MaterialSkin.Controls.MaterialFlatButton();
            this.tbDriver = new MetroFramework.Controls.MetroTrackBar();
            this.btnSnapShotScene = new MaterialSkin.Controls.MaterialFlatButton();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pnlLicensePlateImage = new System.Windows.Forms.Panel();
            this.label16 = new System.Windows.Forms.Label();
            this.picLicensePlate = new System.Windows.Forms.PictureBox();
            this.vspLP = new AForge.Controls.VideoSourcePlayer();
            this.pnlLicensePlateImageMain = new System.Windows.Forms.Panel();
            this.pnlSceneImage = new System.Windows.Forms.Panel();
            this.label17 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.vspScene = new AForge.Controls.VideoSourcePlayer();
            this.pnlSceneImageMain = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2.SuspendLayout();
            this.pnlTbDriver.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.pnlLicensePlateImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLicensePlate)).BeginInit();
            this.pnlLicensePlateImageMain.SuspendLayout();
            this.pnlSceneImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.pnlSceneImageMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel2.Controls.Add(this.pnlTbDriver, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.pnlSceneImageMain, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.pnlLicensePlateImageMain, 4, 1);
            this.tableLayoutPanel2.Controls.Add(this.pictureBox4, 4, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnSnapShotScene, 2, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.75F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1076, 397);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // pnlTbDriver
            // 
            this.pnlTbDriver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTbDriver.AutoScroll = true;
            this.pnlTbDriver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.pnlTbDriver.Controls.Add(this.btnClickSnapshot);
            this.pnlTbDriver.Controls.Add(this.tbDriver);
            this.pnlTbDriver.HorizontalScrollbar = true;
            this.pnlTbDriver.HorizontalScrollbarBarColor = true;
            this.pnlTbDriver.HorizontalScrollbarHighlightOnWheel = false;
            this.pnlTbDriver.HorizontalScrollbarSize = 10;
            this.pnlTbDriver.Location = new System.Drawing.Point(3, 346);
            this.pnlTbDriver.Name = "pnlTbDriver";
            this.pnlTbDriver.Size = new System.Drawing.Size(342, 37);
            this.pnlTbDriver.TabIndex = 274;
            this.pnlTbDriver.UseCustomBackColor = true;
            this.pnlTbDriver.VerticalScrollbar = true;
            this.pnlTbDriver.VerticalScrollbarBarColor = true;
            this.pnlTbDriver.VerticalScrollbarHighlightOnWheel = false;
            this.pnlTbDriver.VerticalScrollbarSize = 10;
            // 
            // btnClickSnapshot
            // 
            this.btnClickSnapshot.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClickSnapshot.Depth = 0;
            this.btnClickSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClickSnapshot.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClickSnapshot.Icon = null;
            this.btnClickSnapshot.Location = new System.Drawing.Point(0, 0);
            this.btnClickSnapshot.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnClickSnapshot.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnClickSnapshot.Name = "btnClickSnapshot";
            this.btnClickSnapshot.Primary = false;
            this.btnClickSnapshot.Size = new System.Drawing.Size(342, 37);
            this.btnClickSnapshot.TabIndex = 233;
            this.btnClickSnapshot.Text = "Click to take snapshot";
            this.btnClickSnapshot.UseVisualStyleBackColor = true;
            // 
            // tbDriver
            // 
            this.tbDriver.BackColor = System.Drawing.Color.Transparent;
            this.tbDriver.Location = new System.Drawing.Point(18, 7);
            this.tbDriver.Name = "tbDriver";
            this.tbDriver.Size = new System.Drawing.Size(242, 23);
            this.tbDriver.TabIndex = 232;
            this.tbDriver.Text = "metroTrackBar1";
            // 
            // btnSnapShotScene
            // 
            this.btnSnapShotScene.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSnapShotScene.Depth = 0;
            this.btnSnapShotScene.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSnapShotScene.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSnapShotScene.Icon = null;
            this.btnSnapShotScene.Location = new System.Drawing.Point(362, 349);
            this.btnSnapShotScene.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSnapShotScene.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSnapShotScene.Name = "btnSnapShotScene";
            this.btnSnapShotScene.Primary = false;
            this.btnSnapShotScene.Size = new System.Drawing.Size(340, 42);
            this.btnSnapShotScene.TabIndex = 230;
            this.btnSnapShotScene.Text = "Click to take snapshot";
            this.btnSnapShotScene.UseVisualStyleBackColor = true;
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox4.BackgroundImage")));
            this.pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox4.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox4.Image = global::IVISS.Properties.Resources.TaskManagementLogo;
            this.pictureBox4.Location = new System.Drawing.Point(858, 346);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(215, 48);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 273;
            this.pictureBox4.TabStop = false;
            // 
            // pnlLicensePlateImage
            // 
            this.pnlLicensePlateImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlLicensePlateImage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLicensePlateImage.Controls.Add(this.picLicensePlate);
            this.pnlLicensePlateImage.Controls.Add(this.label16);
            this.pnlLicensePlateImage.Location = new System.Drawing.Point(1, 2);
            this.pnlLicensePlateImage.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLicensePlateImage.Name = "pnlLicensePlateImage";
            this.pnlLicensePlateImage.Size = new System.Drawing.Size(355, 38);
            this.pnlLicensePlateImage.TabIndex = 204;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.label16.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Gray;
            this.label16.Location = new System.Drawing.Point(4, 7);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(136, 19);
            this.label16.TabIndex = 201;
            this.label16.Text = "LICENSE PLATE";
            // 
            // picLicensePlate
            // 
            this.picLicensePlate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picLicensePlate.Image = global::IVISS.Properties.Resources.Zoom;
            this.picLicensePlate.Location = new System.Drawing.Point(312, 4);
            this.picLicensePlate.Margin = new System.Windows.Forms.Padding(2);
            this.picLicensePlate.Name = "picLicensePlate";
            this.picLicensePlate.Size = new System.Drawing.Size(40, 30);
            this.picLicensePlate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLicensePlate.TabIndex = 203;
            this.picLicensePlate.TabStop = false;
            // 
            // vspLP
            // 
            this.vspLP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vspLP.BackColor = System.Drawing.Color.White;
            this.vspLP.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.vspLP.BorderColor = System.Drawing.Color.Silver;
            this.vspLP.ForeColor = System.Drawing.Color.DimGray;
            this.vspLP.Location = new System.Drawing.Point(1, 42);
            this.vspLP.Name = "vspLP";
            this.vspLP.Size = new System.Drawing.Size(353, 285);
            this.vspLP.TabIndex = 205;
            this.vspLP.VideoSource = null;
            // 
            // pnlLicensePlateImageMain
            // 
            this.pnlLicensePlateImageMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlLicensePlateImageMain.Controls.Add(this.vspLP);
            this.pnlLicensePlateImageMain.Controls.Add(this.pnlLicensePlateImage);
            this.pnlLicensePlateImageMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLicensePlateImageMain.Location = new System.Drawing.Point(718, 12);
            this.pnlLicensePlateImageMain.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLicensePlateImageMain.Name = "pnlLicensePlateImageMain";
            this.pnlLicensePlateImageMain.Size = new System.Drawing.Size(356, 329);
            this.pnlLicensePlateImageMain.TabIndex = 233;
            // 
            // pnlSceneImage
            // 
            this.pnlSceneImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSceneImage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlSceneImage.Controls.Add(this.pictureBox3);
            this.pnlSceneImage.Controls.Add(this.label17);
            this.pnlSceneImage.Location = new System.Drawing.Point(0, 2);
            this.pnlSceneImage.Margin = new System.Windows.Forms.Padding(2);
            this.pnlSceneImage.Name = "pnlSceneImage";
            this.pnlSceneImage.Size = new System.Drawing.Size(342, 38);
            this.pnlSceneImage.TabIndex = 204;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.label17.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label17.Location = new System.Drawing.Point(4, 7);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(66, 19);
            this.label17.TabIndex = 201;
            this.label17.Text = "SCENE";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox3.Location = new System.Drawing.Point(299, 4);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(40, 30);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 203;
            this.pictureBox3.TabStop = false;
            // 
            // vspScene
            // 
            this.vspScene.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vspScene.BackColor = System.Drawing.Color.White;
            this.vspScene.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.vspScene.BorderColor = System.Drawing.Color.Silver;
            this.vspScene.ForeColor = System.Drawing.Color.DimGray;
            this.vspScene.Location = new System.Drawing.Point(2, 42);
            this.vspScene.Name = "vspScene";
            this.vspScene.Size = new System.Drawing.Size(340, 284);
            this.vspScene.TabIndex = 205;
            this.vspScene.VideoSource = null;
            // 
            // pnlSceneImageMain
            // 
            this.pnlSceneImageMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlSceneImageMain.Controls.Add(this.vspScene);
            this.pnlSceneImageMain.Controls.Add(this.pnlSceneImage);
            this.pnlSceneImageMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSceneImageMain.Location = new System.Drawing.Point(360, 12);
            this.pnlSceneImageMain.Margin = new System.Windows.Forms.Padding(2);
            this.pnlSceneImageMain.Name = "pnlSceneImageMain";
            this.pnlSceneImageMain.Size = new System.Drawing.Size(344, 329);
            this.pnlSceneImageMain.TabIndex = 234;
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 397);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "frmTest";
            this.Text = "frmTest";
            this.tableLayoutPanel2.ResumeLayout(false);
            this.pnlTbDriver.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.pnlLicensePlateImage.ResumeLayout(false);
            this.pnlLicensePlateImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLicensePlate)).EndInit();
            this.pnlLicensePlateImageMain.ResumeLayout(false);
            this.pnlSceneImage.ResumeLayout(false);
            this.pnlSceneImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.pnlSceneImageMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroPanel pnlTbDriver;
        private MaterialSkin.Controls.MaterialFlatButton btnClickSnapshot;
        private MetroFramework.Controls.MetroTrackBar tbDriver;
        private MaterialSkin.Controls.MaterialFlatButton btnSnapShotScene;
        private System.Windows.Forms.Panel pnlSceneImageMain;
        private AForge.Controls.VideoSourcePlayer vspScene;
        private System.Windows.Forms.Panel pnlSceneImage;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Panel pnlLicensePlateImageMain;
        private AForge.Controls.VideoSourcePlayer vspLP;
        private System.Windows.Forms.Panel pnlLicensePlateImage;
        private System.Windows.Forms.PictureBox picLicensePlate;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.PictureBox pictureBox4;
    }
}