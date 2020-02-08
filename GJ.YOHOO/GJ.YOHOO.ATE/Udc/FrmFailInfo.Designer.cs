namespace GJ.YOHOO.ATE.Udc
{
    partial class FrmFailInfo
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFailInfo));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnReTest = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnFail = new System.Windows.Forms.Button();
            this.labInfo = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.Yellow;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.btnReTest);
            this.panel1.Controls.Add(this.btnFail);
            this.panel1.Controls.Add(this.labInfo);
            this.panel1.Name = "panel1";
            // 
            // btnReTest
            // 
            resources.ApplyResources(this.btnReTest, "btnReTest");
            this.btnReTest.ImageList = this.imageList1;
            this.btnReTest.Name = "btnReTest";
            this.btnReTest.UseVisualStyleBackColor = true;
            this.btnReTest.Click += new System.EventHandler(this.btnReTest_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "arrow last.ico");
            this.imageList1.Images.SetKeyName(1, "arrow first.ico");
            // 
            // btnFail
            // 
            resources.ApplyResources(this.btnFail, "btnFail");
            this.btnFail.ImageList = this.imageList1;
            this.btnFail.Name = "btnFail";
            this.btnFail.UseVisualStyleBackColor = true;
            this.btnFail.Click += new System.EventHandler(this.btnFail_Click);
            // 
            // labInfo
            // 
            resources.ApplyResources(this.labInfo, "labInfo");
            this.labInfo.ForeColor = System.Drawing.Color.Red;
            this.labInfo.Name = "labInfo";
            // 
            // FrmFailInfo
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmFailInfo";
            this.Load += new System.EventHandler(this.FrmFailInfo_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnReTest;
        private System.Windows.Forms.Button btnFail;
        private System.Windows.Forms.Label labInfo;
        private System.Windows.Forms.ImageList imageList1;
    }
}