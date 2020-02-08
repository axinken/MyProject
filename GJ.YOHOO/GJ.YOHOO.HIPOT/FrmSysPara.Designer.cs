namespace GJ.YOHOO.HIPOT
{
    partial class FrmSysPara
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSysPara));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cmbIoCom = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSerPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbHPCom1 = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMonDelay = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtSerIP = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtSerStat = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.cmbTcpMode = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtIoDelayMs = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.cmbHPCom2 = new System.Windows.Forms.ComboBox();
            this.chkIoEnable = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbHPType = new System.Windows.Forms.ComboBox();
            this.txtHpBaud = new System.Windows.Forms.TextBox();
            this.cmbHPChanMax = new System.Windows.Forms.ComboBox();
            this.cmbHPMax = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chkImpPrg = new System.Windows.Forms.CheckBox();
            this.chkAutoModel = new System.Windows.Forms.CheckBox();
            this.chkReTest = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label11 = new System.Windows.Forms.Label();
            this.chkSaveReport = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtModelPath = new System.Windows.Forms.TextBox();
            this.txtReportPath = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.txtReportSaveTimes = new System.Windows.Forms.TextBox();
            this.btnModel = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList();
            this.btnReport = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkMesCon = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtFailNoTran = new System.Windows.Forms.TextBox();
            this.chkGJWeb = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnExit);
            this.splitContainer1.Panel2.Controls.Add(this.btnOK);
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            // 
            // tabPage1
            // 
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage1.Controls.Add(this.panel1);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.cmbIoCom, 1, 2);
            this.panel1.Controls.Add(this.label4, 0, 4);
            this.panel1.Controls.Add(this.txtSerPort, 1, 4);
            this.panel1.Controls.Add(this.label1, 0, 0);
            this.panel1.Controls.Add(this.cmbHPCom1, 1, 0);
            this.panel1.Controls.Add(this.label5, 0, 7);
            this.panel1.Controls.Add(this.txtMonDelay, 1, 7);
            this.panel1.Controls.Add(this.label6, 0, 3);
            this.panel1.Controls.Add(this.txtSerIP, 1, 3);
            this.panel1.Controls.Add(this.label9, 0, 6);
            this.panel1.Controls.Add(this.txtSerStat, 1, 6);
            this.panel1.Controls.Add(this.label15, 0, 5);
            this.panel1.Controls.Add(this.cmbTcpMode, 1, 5);
            this.panel1.Controls.Add(this.label10, 0, 8);
            this.panel1.Controls.Add(this.txtIoDelayMs, 1, 8);
            this.panel1.Controls.Add(this.label17, 0, 1);
            this.panel1.Controls.Add(this.cmbHPCom2, 1, 1);
            this.panel1.Controls.Add(this.chkIoEnable, 0, 2);
            this.panel1.Controls.Add(this.label16, 2, 0);
            this.panel1.Controls.Add(this.label18, 2, 1);
            this.panel1.Controls.Add(this.label2, 2, 2);
            this.panel1.Controls.Add(this.label7, 2, 3);
            this.panel1.Controls.Add(this.cmbHPType, 3, 0);
            this.panel1.Controls.Add(this.txtHpBaud, 3, 1);
            this.panel1.Controls.Add(this.cmbHPChanMax, 3, 2);
            this.panel1.Controls.Add(this.cmbHPMax, 3, 3);
            this.panel1.Name = "panel1";
            // 
            // cmbIoCom
            // 
            resources.ApplyResources(this.cmbIoCom, "cmbIoCom");
            this.cmbIoCom.FormattingEnabled = true;
            this.cmbIoCom.Name = "cmbIoCom";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // txtSerPort
            // 
            resources.ApplyResources(this.txtSerPort, "txtSerPort");
            this.txtSerPort.Name = "txtSerPort";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbHPCom1
            // 
            resources.ApplyResources(this.cmbHPCom1, "cmbHPCom1");
            this.cmbHPCom1.FormattingEnabled = true;
            this.cmbHPCom1.Name = "cmbHPCom1";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // txtMonDelay
            // 
            resources.ApplyResources(this.txtMonDelay, "txtMonDelay");
            this.txtMonDelay.Name = "txtMonDelay";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // txtSerIP
            // 
            resources.ApplyResources(this.txtSerIP, "txtSerIP");
            this.txtSerIP.Name = "txtSerIP";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // txtSerStat
            // 
            resources.ApplyResources(this.txtSerStat, "txtSerStat");
            this.txtSerStat.Name = "txtSerStat";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // cmbTcpMode
            // 
            resources.ApplyResources(this.cmbTcpMode, "cmbTcpMode");
            this.cmbTcpMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTcpMode.FormattingEnabled = true;
            this.cmbTcpMode.Items.AddRange(new object[] {
            resources.GetString("cmbTcpMode.Items"),
            resources.GetString("cmbTcpMode.Items1"),
            resources.GetString("cmbTcpMode.Items2")});
            this.cmbTcpMode.Name = "cmbTcpMode";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // txtIoDelayMs
            // 
            resources.ApplyResources(this.txtIoDelayMs, "txtIoDelayMs");
            this.txtIoDelayMs.Name = "txtIoDelayMs";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            // 
            // cmbHPCom2
            // 
            resources.ApplyResources(this.cmbHPCom2, "cmbHPCom2");
            this.cmbHPCom2.FormattingEnabled = true;
            this.cmbHPCom2.Name = "cmbHPCom2";
            // 
            // chkIoEnable
            // 
            resources.ApplyResources(this.chkIoEnable, "chkIoEnable");
            this.chkIoEnable.Name = "chkIoEnable";
            this.chkIoEnable.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            // 
            // label18
            // 
            resources.ApplyResources(this.label18, "label18");
            this.label18.Name = "label18";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // cmbHPType
            // 
            this.cmbHPType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHPType.FormattingEnabled = true;
            this.cmbHPType.Items.AddRange(new object[] {
            resources.GetString("cmbHPType.Items"),
            resources.GetString("cmbHPType.Items1")});
            resources.ApplyResources(this.cmbHPType, "cmbHPType");
            this.cmbHPType.Name = "cmbHPType";
            // 
            // txtHpBaud
            // 
            resources.ApplyResources(this.txtHpBaud, "txtHpBaud");
            this.txtHpBaud.Name = "txtHpBaud";
            // 
            // cmbHPChanMax
            // 
            this.cmbHPChanMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHPChanMax.FormattingEnabled = true;
            this.cmbHPChanMax.Items.AddRange(new object[] {
            resources.GetString("cmbHPChanMax.Items"),
            resources.GetString("cmbHPChanMax.Items1"),
            resources.GetString("cmbHPChanMax.Items2")});
            resources.ApplyResources(this.cmbHPChanMax, "cmbHPChanMax");
            this.cmbHPChanMax.Name = "cmbHPChanMax";
            // 
            // cmbHPMax
            // 
            this.cmbHPMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHPMax.FormattingEnabled = true;
            this.cmbHPMax.Items.AddRange(new object[] {
            resources.GetString("cmbHPMax.Items"),
            resources.GetString("cmbHPMax.Items1")});
            resources.ApplyResources(this.cmbHPMax, "cmbHPMax");
            this.cmbHPMax.Name = "cmbHPMax";
            // 
            // tabPage2
            // 
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage2.Controls.Add(this.panel2);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.chkImpPrg, 0, 1);
            this.panel2.Controls.Add(this.chkAutoModel, 0, 0);
            this.panel2.Controls.Add(this.chkReTest, 0, 2);
            this.panel2.Name = "panel2";
            // 
            // chkImpPrg
            // 
            resources.ApplyResources(this.chkImpPrg, "chkImpPrg");
            this.chkImpPrg.Name = "chkImpPrg";
            this.chkImpPrg.UseVisualStyleBackColor = true;
            // 
            // chkAutoModel
            // 
            resources.ApplyResources(this.chkAutoModel, "chkAutoModel");
            this.chkAutoModel.Name = "chkAutoModel";
            this.chkAutoModel.UseVisualStyleBackColor = true;
            // 
            // chkReTest
            // 
            resources.ApplyResources(this.chkReTest, "chkReTest");
            this.chkReTest.Name = "chkReTest";
            this.chkReTest.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage3.Controls.Add(this.panel4);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Controls.Add(this.label11, 0, 0);
            this.panel4.Controls.Add(this.chkSaveReport, 0, 1);
            this.panel4.Controls.Add(this.label12, 0, 2);
            this.panel4.Controls.Add(this.txtModelPath, 1, 0);
            this.panel4.Controls.Add(this.txtReportPath, 1, 2);
            this.panel4.Controls.Add(this.panel5, 1, 1);
            this.panel4.Controls.Add(this.btnModel, 2, 0);
            this.panel4.Controls.Add(this.btnReport, 2, 2);
            this.panel4.Name = "panel4";
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // chkSaveReport
            // 
            resources.ApplyResources(this.chkSaveReport, "chkSaveReport");
            this.chkSaveReport.Name = "chkSaveReport";
            this.chkSaveReport.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // txtModelPath
            // 
            resources.ApplyResources(this.txtModelPath, "txtModelPath");
            this.txtModelPath.Name = "txtModelPath";
            // 
            // txtReportPath
            // 
            resources.ApplyResources(this.txtReportPath, "txtReportPath");
            this.txtReportPath.Name = "txtReportPath";
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Controls.Add(this.label13, 0, 0);
            this.panel5.Controls.Add(this.txtReportSaveTimes, 1, 0);
            this.panel5.Name = "panel5";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // txtReportSaveTimes
            // 
            resources.ApplyResources(this.txtReportSaveTimes, "txtReportSaveTimes");
            this.txtReportSaveTimes.Name = "txtReportSaveTimes";
            // 
            // btnModel
            // 
            resources.ApplyResources(this.btnModel, "btnModel");
            this.btnModel.ImageList = this.imageList1;
            this.btnModel.Name = "btnModel";
            this.btnModel.UseVisualStyleBackColor = true;
            this.btnModel.Click += new System.EventHandler(this.btnModel_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Edit.ICO");
            this.imageList1.Images.SetKeyName(1, "contents.ico");
            this.imageList1.Images.SetKeyName(2, "4519.ico");
            // 
            // btnReport
            // 
            resources.ApplyResources(this.btnReport, "btnReport");
            this.btnReport.ImageList = this.imageList1;
            this.btnReport.Name = "btnReport";
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage4.Controls.Add(this.panel3);
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.chkMesCon, 0, 1);
            this.panel3.Controls.Add(this.label8, 0, 2);
            this.panel3.Controls.Add(this.txtFailNoTran, 1, 2);
            this.panel3.Controls.Add(this.chkGJWeb, 0, 0);
            this.panel3.Name = "panel3";
            // 
            // chkMesCon
            // 
            resources.ApplyResources(this.chkMesCon, "chkMesCon");
            this.chkMesCon.Name = "chkMesCon";
            this.chkMesCon.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // txtFailNoTran
            // 
            resources.ApplyResources(this.txtFailNoTran, "txtFailNoTran");
            this.txtFailNoTran.Name = "txtFailNoTran";
            // 
            // chkGJWeb
            // 
            resources.ApplyResources(this.chkGJWeb, "chkGJWeb");
            this.chkGJWeb.Name = "chkGJWeb";
            this.chkGJWeb.UseVisualStyleBackColor = true;
            // 
            // btnExit
            // 
            resources.ApplyResources(this.btnExit, "btnExit");
            this.btnExit.ImageList = this.imageList1;
            this.btnExit.Name = "btnExit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.ImageList = this.imageList1;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // FrmSysPara
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FrmSysPara";
            this.Load += new System.EventHandler(this.FrmSysPara_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.ComboBox cmbIoCom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSerPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbHPCom1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMonDelay;
        public System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel panel2;
        public System.Windows.Forms.Button btnExit;
        public System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtSerIP;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TableLayoutPanel panel4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chkSaveReport;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtModelPath;
        private System.Windows.Forms.TextBox txtReportPath;
        private System.Windows.Forms.TableLayoutPanel panel5;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtReportSaveTimes;
        private System.Windows.Forms.Button btnModel;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.CheckBox chkImpPrg;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtSerStat;
        private System.Windows.Forms.CheckBox chkAutoModel;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox cmbTcpMode;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.CheckBox chkMesCon;
        private System.Windows.Forms.ComboBox cmbHPType;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbHPMax;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtIoDelayMs;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtFailNoTran;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox cmbHPCom2;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtHpBaud;
        private System.Windows.Forms.CheckBox chkIoEnable;
        private System.Windows.Forms.CheckBox chkGJWeb;
        private System.Windows.Forms.CheckBox chkReTest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbHPChanMax;

    }
}