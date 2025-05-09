namespace IVISS
{
    partial class frmOverlay
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
            this.vspDriverViewDetails = new AForge.Controls.VideoSourcePlayer();
            this.lblPathViewDetails = new System.Windows.Forms.Label();
            this.TableLayoutMainViewDetails = new System.Windows.Forms.TableLayoutPanel();
            this.pBoxComparisonViewDetails = new System.Windows.Forms.PictureBox();
            this.pnlLiveLP = new System.Windows.Forms.Panel();
            this.vspLPViewDetails = new AForge.Controls.VideoSourcePlayer();
            this.pnlLicensePlateImageViewDetails = new System.Windows.Forms.Panel();
            this.picLicensePlate = new System.Windows.Forms.PictureBox();
            this.label16 = new System.Windows.Forms.Label();
            this.pnlLiveScene = new System.Windows.Forms.Panel();
            this.vspSceneViewDetails = new AForge.Controls.VideoSourcePlayer();
            this.pnlSceneImageViewDetails = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label17 = new System.Windows.Forms.Label();
            this.pnlLiveDriver = new System.Windows.Forms.Panel();
            this.pnlTbDriver = new MetroFramework.Controls.MetroPanel();
            this.btnClickSnapshot = new MaterialSkin.Controls.MaterialFlatButton();
            this.tbDriver = new MetroFramework.Controls.MetroTrackBar();
            this.pnlDriverImageViewDetails = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label15 = new System.Windows.Forms.Label();
            this.pBoxStitchViewDetails = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.TableLayoutMainViewDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBoxComparisonViewDetails)).BeginInit();
            this.pnlLiveLP.SuspendLayout();
            this.pnlLicensePlateImageViewDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLicensePlate)).BeginInit();
            this.pnlLiveScene.SuspendLayout();
            this.pnlSceneImageViewDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.pnlLiveDriver.SuspendLayout();
            this.pnlTbDriver.SuspendLayout();
            this.pnlDriverImageViewDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBoxStitchViewDetails)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // vspDriverViewDetails
            // 
            this.vspDriverViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vspDriverViewDetails.BackColor = System.Drawing.Color.White;
            this.vspDriverViewDetails.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.vspDriverViewDetails.BorderColor = System.Drawing.Color.Silver;
            this.vspDriverViewDetails.ForeColor = System.Drawing.Color.DimGray;
            this.vspDriverViewDetails.Location = new System.Drawing.Point(1, 42);
            this.vspDriverViewDetails.Name = "vspDriverViewDetails";
            this.vspDriverViewDetails.Size = new System.Drawing.Size(404, 227);
            this.vspDriverViewDetails.TabIndex = 84;
            this.vspDriverViewDetails.VideoSource = null;
            this.vspDriverViewDetails.Click += new System.EventHandler(this.vspDriver_Click);
            // 
            // lblPathViewDetails
            // 
            this.lblPathViewDetails.AutoSize = true;
            this.lblPathViewDetails.BackColor = System.Drawing.Color.Transparent;
            this.lblPathViewDetails.Font = new System.Drawing.Font("Arial Black", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPathViewDetails.ForeColor = System.Drawing.Color.White;
            this.lblPathViewDetails.Location = new System.Drawing.Point(12, 10);
            this.lblPathViewDetails.Name = "lblPathViewDetails";
            this.lblPathViewDetails.Size = new System.Drawing.Size(68, 19);
            this.lblPathViewDetails.TabIndex = 113;
            this.lblPathViewDetails.Text = "Overlay";
            // 
            // TableLayoutMainViewDetails
            // 
            this.TableLayoutMainViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TableLayoutMainViewDetails.BackColor = System.Drawing.Color.White;
            this.TableLayoutMainViewDetails.ColumnCount = 3;
            this.TableLayoutMainViewDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.15997F));
            this.TableLayoutMainViewDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.91386F));
            this.TableLayoutMainViewDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.92617F));
            this.TableLayoutMainViewDetails.Controls.Add(this.pBoxComparisonViewDetails, 1, 1);
            this.TableLayoutMainViewDetails.Controls.Add(this.pnlLiveLP, 2, 3);
            this.TableLayoutMainViewDetails.Controls.Add(this.pnlLiveScene, 2, 2);
            this.TableLayoutMainViewDetails.Controls.Add(this.pnlLiveDriver, 2, 1);
            this.TableLayoutMainViewDetails.Controls.Add(this.pBoxStitchViewDetails, 0, 1);
            this.TableLayoutMainViewDetails.Controls.Add(this.panel1, 1, 0);
            this.TableLayoutMainViewDetails.Location = new System.Drawing.Point(2, 34);
            this.TableLayoutMainViewDetails.Name = "TableLayoutMainViewDetails";
            this.TableLayoutMainViewDetails.RowCount = 4;
            this.TableLayoutMainViewDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.TableLayoutMainViewDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.TableLayoutMainViewDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.TableLayoutMainViewDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.TableLayoutMainViewDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableLayoutMainViewDetails.Size = new System.Drawing.Size(1367, 990);
            this.TableLayoutMainViewDetails.TabIndex = 114;
            // 
            // pBoxComparisonViewDetails
            // 
            this.pBoxComparisonViewDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pBoxComparisonViewDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pBoxComparisonViewDetails.Location = new System.Drawing.Point(483, 41);
            this.pBoxComparisonViewDetails.Name = "pBoxComparisonViewDetails";
            this.TableLayoutMainViewDetails.SetRowSpan(this.pBoxComparisonViewDetails, 3);
            this.pBoxComparisonViewDetails.Size = new System.Drawing.Size(471, 946);
            this.pBoxComparisonViewDetails.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pBoxComparisonViewDetails.TabIndex = 238;
            this.pBoxComparisonViewDetails.TabStop = false;
            // 
            // pnlLiveLP
            // 
            this.pnlLiveLP.BackColor = System.Drawing.Color.Transparent;
            this.pnlLiveLP.Controls.Add(this.vspLPViewDetails);
            this.pnlLiveLP.Controls.Add(this.pnlLicensePlateImageViewDetails);
            this.pnlLiveLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLiveLP.Location = new System.Drawing.Point(959, 677);
            this.pnlLiveLP.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLiveLP.Name = "pnlLiveLP";
            this.pnlLiveLP.Size = new System.Drawing.Size(406, 311);
            this.pnlLiveLP.TabIndex = 237;
            // 
            // vspLPViewDetails
            // 
            this.vspLPViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vspLPViewDetails.BackColor = System.Drawing.Color.White;
            this.vspLPViewDetails.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.vspLPViewDetails.BorderColor = System.Drawing.Color.Silver;
            this.vspLPViewDetails.ForeColor = System.Drawing.Color.DimGray;
            this.vspLPViewDetails.Location = new System.Drawing.Point(1, 42);
            this.vspLPViewDetails.Name = "vspLPViewDetails";
            this.vspLPViewDetails.Size = new System.Drawing.Size(403, 267);
            this.vspLPViewDetails.TabIndex = 205;
            this.vspLPViewDetails.VideoSource = null;
            this.vspLPViewDetails.Click += new System.EventHandler(this.vspLP_Click);
            // 
            // pnlLicensePlateImageViewDetails
            // 
            this.pnlLicensePlateImageViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlLicensePlateImageViewDetails.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLicensePlateImageViewDetails.Controls.Add(this.picLicensePlate);
            this.pnlLicensePlateImageViewDetails.Controls.Add(this.label16);
            this.pnlLicensePlateImageViewDetails.Location = new System.Drawing.Point(1, 2);
            this.pnlLicensePlateImageViewDetails.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLicensePlateImageViewDetails.Name = "pnlLicensePlateImageViewDetails";
            this.pnlLicensePlateImageViewDetails.Size = new System.Drawing.Size(405, 38);
            this.pnlLicensePlateImageViewDetails.TabIndex = 204;
            // 
            // picLicensePlate
            // 
            this.picLicensePlate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picLicensePlate.Image = global::IVISS.Properties.Resources.Zoom;
            this.picLicensePlate.Location = new System.Drawing.Point(362, 4);
            this.picLicensePlate.Margin = new System.Windows.Forms.Padding(2);
            this.picLicensePlate.Name = "picLicensePlate";
            this.picLicensePlate.Size = new System.Drawing.Size(40, 30);
            this.picLicensePlate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLicensePlate.TabIndex = 203;
            this.picLicensePlate.TabStop = false;
            this.picLicensePlate.Click += new System.EventHandler(this.picLicensePlate_Click);
            this.picLicensePlate.MouseLeave += new System.EventHandler(this.pic_MouseLeave);
            this.picLicensePlate.MouseHover += new System.EventHandler(this.pic_MouseHover);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.Color.Transparent;
            this.label16.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Gray;
            this.label16.Location = new System.Drawing.Point(4, 7);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(136, 19);
            this.label16.TabIndex = 201;
            this.label16.Text = "LICENSE PLATE";
            // 
            // pnlLiveScene
            // 
            this.pnlLiveScene.BackColor = System.Drawing.Color.Transparent;
            this.pnlLiveScene.Controls.Add(this.vspSceneViewDetails);
            this.pnlLiveScene.Controls.Add(this.pnlSceneImageViewDetails);
            this.pnlLiveScene.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLiveScene.Location = new System.Drawing.Point(959, 363);
            this.pnlLiveScene.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLiveScene.Name = "pnlLiveScene";
            this.pnlLiveScene.Size = new System.Drawing.Size(406, 310);
            this.pnlLiveScene.TabIndex = 236;
            // 
            // vspSceneViewDetails
            // 
            this.vspSceneViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vspSceneViewDetails.BackColor = System.Drawing.Color.White;
            this.vspSceneViewDetails.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.vspSceneViewDetails.BorderColor = System.Drawing.Color.Silver;
            this.vspSceneViewDetails.ForeColor = System.Drawing.Color.DimGray;
            this.vspSceneViewDetails.Location = new System.Drawing.Point(2, 42);
            this.vspSceneViewDetails.Name = "vspSceneViewDetails";
            this.vspSceneViewDetails.Size = new System.Drawing.Size(402, 265);
            this.vspSceneViewDetails.TabIndex = 205;
            this.vspSceneViewDetails.VideoSource = null;
            // 
            // pnlSceneImageViewDetails
            // 
            this.pnlSceneImageViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSceneImageViewDetails.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlSceneImageViewDetails.Controls.Add(this.pictureBox3);
            this.pnlSceneImageViewDetails.Controls.Add(this.label17);
            this.pnlSceneImageViewDetails.Location = new System.Drawing.Point(0, 2);
            this.pnlSceneImageViewDetails.Margin = new System.Windows.Forms.Padding(2);
            this.pnlSceneImageViewDetails.Name = "pnlSceneImageViewDetails";
            this.pnlSceneImageViewDetails.Size = new System.Drawing.Size(404, 38);
            this.pnlSceneImageViewDetails.TabIndex = 204;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox3.Location = new System.Drawing.Point(361, 4);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(40, 30);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 203;
            this.pictureBox3.TabStop = false;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.Color.Transparent;
            this.label17.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label17.Location = new System.Drawing.Point(4, 7);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(66, 19);
            this.label17.TabIndex = 201;
            this.label17.Text = "SCENE";
            // 
            // pnlLiveDriver
            // 
            this.pnlLiveDriver.BackColor = System.Drawing.Color.Transparent;
            this.pnlLiveDriver.Controls.Add(this.pnlTbDriver);
            this.pnlLiveDriver.Controls.Add(this.pnlDriverImageViewDetails);
            this.pnlLiveDriver.Controls.Add(this.vspDriverViewDetails);
            this.pnlLiveDriver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLiveDriver.Location = new System.Drawing.Point(959, 40);
            this.pnlLiveDriver.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLiveDriver.Name = "pnlLiveDriver";
            this.pnlLiveDriver.Size = new System.Drawing.Size(406, 319);
            this.pnlLiveDriver.TabIndex = 234;
            // 
            // pnlTbDriver
            // 
            this.pnlTbDriver.AutoScroll = true;
            this.pnlTbDriver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.pnlTbDriver.Controls.Add(this.btnClickSnapshot);
            this.pnlTbDriver.Controls.Add(this.tbDriver);
            this.pnlTbDriver.HorizontalScrollbar = true;
            this.pnlTbDriver.HorizontalScrollbarBarColor = true;
            this.pnlTbDriver.HorizontalScrollbarHighlightOnWheel = false;
            this.pnlTbDriver.HorizontalScrollbarSize = 10;
            this.pnlTbDriver.Location = new System.Drawing.Point(0, 287);
            this.pnlTbDriver.Name = "pnlTbDriver";
            this.pnlTbDriver.Size = new System.Drawing.Size(406, 45);
            this.pnlTbDriver.TabIndex = 275;
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
            this.btnClickSnapshot.Size = new System.Drawing.Size(406, 45);
            this.btnClickSnapshot.TabIndex = 234;
            this.btnClickSnapshot.Text = "Click to take snapshot";
            this.btnClickSnapshot.UseVisualStyleBackColor = true;
            this.btnClickSnapshot.Click += new System.EventHandler(this.btnClickSnapshot_Click);
            // 
            // tbDriver
            // 
            this.tbDriver.BackColor = System.Drawing.Color.Transparent;
            this.tbDriver.Location = new System.Drawing.Point(18, 7);
            this.tbDriver.Name = "tbDriver";
            this.tbDriver.Size = new System.Drawing.Size(242, 23);
            this.tbDriver.TabIndex = 232;
            this.tbDriver.Text = "metroTrackBar1";
            this.tbDriver.Scroll += new System.Windows.Forms.ScrollEventHandler(this.tbDriver_Scroll);
            // 
            // pnlDriverImageViewDetails
            // 
            this.pnlDriverImageViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDriverImageViewDetails.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlDriverImageViewDetails.Controls.Add(this.pictureBox1);
            this.pnlDriverImageViewDetails.Controls.Add(this.label15);
            this.pnlDriverImageViewDetails.Location = new System.Drawing.Point(2, 2);
            this.pnlDriverImageViewDetails.Margin = new System.Windows.Forms.Padding(2);
            this.pnlDriverImageViewDetails.Name = "pnlDriverImageViewDetails";
            this.pnlDriverImageViewDetails.Size = new System.Drawing.Size(402, 38);
            this.pnlDriverImageViewDetails.TabIndex = 204;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::IVISS.Properties.Resources.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(359, 4);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(40, 30);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 203;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.picDriver_Click);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pic_MouseLeave);
            this.pictureBox1.MouseHover += new System.EventHandler(this.pic_MouseHover);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.Color.Transparent;
            this.label15.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Gray;
            this.label15.Location = new System.Drawing.Point(4, 7);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(71, 19);
            this.label15.TabIndex = 201;
            this.label15.Text = "DRIVER";
            // 
            // pBoxStitchViewDetails
            // 
            this.pBoxStitchViewDetails.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pBoxStitchViewDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pBoxStitchViewDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pBoxStitchViewDetails.Location = new System.Drawing.Point(3, 41);
            this.pBoxStitchViewDetails.Name = "pBoxStitchViewDetails";
            this.TableLayoutMainViewDetails.SetRowSpan(this.pBoxStitchViewDetails, 3);
            this.pBoxStitchViewDetails.Size = new System.Drawing.Size(474, 946);
            this.pBoxStitchViewDetails.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pBoxStitchViewDetails.TabIndex = 111;
            this.pBoxStitchViewDetails.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(483, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(471, 32);
            this.panel1.TabIndex = 239;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox2.Image = global::IVISS.Properties.Resources.EditImage;
            this.pictureBox2.Location = new System.Drawing.Point(419, -2);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(50, 33);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 240;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // frmOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1374, 1036);
            this.Controls.Add(this.TableLayoutMainViewDetails);
            this.Controls.Add(this.lblPathViewDetails);
            this.Name = "frmOverlay";
            this.Style = MetroFramework.MetroColorStyle.Silver;
            this.Load += new System.EventHandler(this.frmOverlay_Load);
            this.TableLayoutMainViewDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pBoxComparisonViewDetails)).EndInit();
            this.pnlLiveLP.ResumeLayout(false);
            this.pnlLicensePlateImageViewDetails.ResumeLayout(false);
            this.pnlLicensePlateImageViewDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLicensePlate)).EndInit();
            this.pnlLiveScene.ResumeLayout(false);
            this.pnlSceneImageViewDetails.ResumeLayout(false);
            this.pnlSceneImageViewDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.pnlLiveDriver.ResumeLayout(false);
            this.pnlTbDriver.ResumeLayout(false);
            this.pnlDriverImageViewDetails.ResumeLayout(false);
            this.pnlDriverImageViewDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBoxStitchViewDetails)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private AForge.Controls.VideoSourcePlayer vspDriverViewDetails;
        private System.Windows.Forms.PictureBox pBoxStitchViewDetails;
        private System.Windows.Forms.Label lblPathViewDetails;
        private System.Windows.Forms.TableLayoutPanel TableLayoutMainViewDetails;
        private System.Windows.Forms.Panel pnlLiveDriver;
        private System.Windows.Forms.Panel pnlDriverImageViewDetails;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Panel pnlLiveScene;
        private AForge.Controls.VideoSourcePlayer vspSceneViewDetails;
        private System.Windows.Forms.Panel pnlSceneImageViewDetails;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Panel pnlLiveLP;
        private AForge.Controls.VideoSourcePlayer vspLPViewDetails;
        private System.Windows.Forms.Panel pnlLicensePlateImageViewDetails;
        private System.Windows.Forms.PictureBox picLicensePlate;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.PictureBox pBoxComparisonViewDetails;
        private MetroFramework.Controls.MetroPanel pnlTbDriver;
        private MetroFramework.Controls.MetroTrackBar tbDriver;
        private MaterialSkin.Controls.MaterialFlatButton btnClickSnapshot;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}