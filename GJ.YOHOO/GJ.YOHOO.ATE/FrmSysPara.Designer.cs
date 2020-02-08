namespace GJ.YOHOO.ATE
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSysPara));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSerTcpIP = new System.Windows.Forms.TextBox();
            this.txtSerTcpPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbChanMax = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbATELanguage = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtSerStatName = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMonDelay = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbDevMax = new System.Windows.Forms.ComboBox();
            this.cmbIO = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtIoDelayMs = new System.Windows.Forms.TextBox();
            this.cmbTcpMode = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.chkIoEnable = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtATEPrg = new System.Windows.Forms.TextBox();
            this.txtATEResultPath = new System.Windows.Forms.TextBox();
            this.panel6 = new System.Windows.Forms.TableLayoutPanel();
            this.txtATEDelay = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.TableLayoutPanel();
            this.txtATERepeats = new System.Windows.Forms.TextBox();
            this.chkBarForm = new System.Windows.Forms.CheckBox();
            this.txtBarFormName = new System.Windows.Forms.TextBox();
            this.chkImg = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.TableLayoutPanel();
            this.txtATEMon = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.panel9 = new System.Windows.Forms.TableLayoutPanel();
            this.txtFailLock = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.chkGetTestData = new System.Windows.Forms.CheckBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label20 = new System.Windows.Forms.Label();
            this.chkSaveReport = new System.Windows.Forms.CheckBox();
            this.label21 = new System.Windows.Forms.Label();
            this.txtModelPath = new System.Windows.Forms.TextBox();
            this.txtReportPath = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label22 = new System.Windows.Forms.Label();
            this.txtReportSaveTimes = new System.Windows.Forms.TextBox();
            this.btnModel = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnReport = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkMesCon = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
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
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel9.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tabPage3.SuspendLayout();
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
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.btnExit);
            this.splitContainer1.Panel2.Controls.Add(this.btnOK);
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            // 
            // tabPage1
            // 
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label1, 0, 0);
            this.panel1.Controls.Add(this.label2, 0, 1);
            this.panel1.Controls.Add(this.txtSerTcpIP, 1, 0);
            this.panel1.Controls.Add(this.txtSerTcpPort, 1, 1);
            this.panel1.Controls.Add(this.label3, 0, 5);
            this.panel1.Controls.Add(this.cmbChanMax, 1, 5);
            this.panel1.Controls.Add(this.label10, 0, 8);
            this.panel1.Controls.Add(this.cmbATELanguage, 1, 8);
            this.panel1.Controls.Add(this.label14, 0, 3);
            this.panel1.Controls.Add(this.txtSerStatName, 1, 3);
            this.panel1.Controls.Add(this.label9, 0, 9);
            this.panel1.Controls.Add(this.txtMonDelay, 1, 9);
            this.panel1.Controls.Add(this.label11, 0, 4);
            this.panel1.Controls.Add(this.cmbDevMax, 1, 4);
            this.panel1.Controls.Add(this.cmbIO, 1, 6);
            this.panel1.Controls.Add(this.label13, 0, 7);
            this.panel1.Controls.Add(this.txtIoDelayMs, 1, 7);
            this.panel1.Controls.Add(this.cmbTcpMode, 1, 2);
            this.panel1.Controls.Add(this.label15, 0, 2);
            this.panel1.Controls.Add(this.chkIoEnable, 0, 6);
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtSerTcpIP
            // 
            resources.ApplyResources(this.txtSerTcpIP, "txtSerTcpIP");
            this.txtSerTcpIP.Name = "txtSerTcpIP";
            // 
            // txtSerTcpPort
            // 
            resources.ApplyResources(this.txtSerTcpPort, "txtSerTcpPort");
            this.txtSerTcpPort.Name = "txtSerTcpPort";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbChanMax
            // 
            resources.ApplyResources(this.cmbChanMax, "cmbChanMax");
            this.cmbChanMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChanMax.FormattingEnabled = true;
            this.cmbChanMax.Items.AddRange(new object[] {
            resources.GetString("cmbChanMax.Items"),
            resources.GetString("cmbChanMax.Items1"),
            resources.GetString("cmbChanMax.Items2")});
            this.cmbChanMax.Name = "cmbChanMax";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // cmbATELanguage
            // 
            resources.ApplyResources(this.cmbATELanguage, "cmbATELanguage");
            this.cmbATELanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbATELanguage.FormattingEnabled = true;
            this.cmbATELanguage.Items.AddRange(new object[] {
            resources.GetString("cmbATELanguage.Items"),
            resources.GetString("cmbATELanguage.Items1")});
            this.cmbATELanguage.Name = "cmbATELanguage";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // txtSerStatName
            // 
            resources.ApplyResources(this.txtSerStatName, "txtSerStatName");
            this.txtSerStatName.Name = "txtSerStatName";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // txtMonDelay
            // 
            resources.ApplyResources(this.txtMonDelay, "txtMonDelay");
            this.txtMonDelay.Name = "txtMonDelay";
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // cmbDevMax
            // 
            resources.ApplyResources(this.cmbDevMax, "cmbDevMax");
            this.cmbDevMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDevMax.FormattingEnabled = true;
            this.cmbDevMax.Items.AddRange(new object[] {
            resources.GetString("cmbDevMax.Items"),
            resources.GetString("cmbDevMax.Items1")});
            this.cmbDevMax.Name = "cmbDevMax";
            // 
            // cmbIO
            // 
            resources.ApplyResources(this.cmbIO, "cmbIO");
            this.cmbIO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIO.FormattingEnabled = true;
            this.cmbIO.Name = "cmbIO";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // txtIoDelayMs
            // 
            resources.ApplyResources(this.txtIoDelayMs, "txtIoDelayMs");
            this.txtIoDelayMs.Name = "txtIoDelayMs";
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
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // chkIoEnable
            // 
            resources.ApplyResources(this.chkIoEnable, "chkIoEnable");
            this.chkIoEnable.Name = "chkIoEnable";
            this.chkIoEnable.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.label4, 0, 0);
            this.panel2.Controls.Add(this.label5, 0, 1);
            this.panel2.Controls.Add(this.label6, 0, 4);
            this.panel2.Controls.Add(this.label7, 0, 5);
            this.panel2.Controls.Add(this.txtATEPrg, 1, 0);
            this.panel2.Controls.Add(this.txtATEResultPath, 1, 1);
            this.panel2.Controls.Add(this.panel6, 1, 4);
            this.panel2.Controls.Add(this.panel7, 1, 5);
            this.panel2.Controls.Add(this.chkBarForm, 0, 6);
            this.panel2.Controls.Add(this.txtBarFormName, 1, 6);
            this.panel2.Controls.Add(this.chkImg, 0, 7);
            this.panel2.Controls.Add(this.label16, 0, 3);
            this.panel2.Controls.Add(this.panel8, 1, 3);
            this.panel2.Controls.Add(this.label18, 0, 8);
            this.panel2.Controls.Add(this.panel9, 1, 8);
            this.panel2.Controls.Add(this.chkGetTestData, 0, 2);
            this.panel2.Name = "panel2";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // txtATEPrg
            // 
            resources.ApplyResources(this.txtATEPrg, "txtATEPrg");
            this.txtATEPrg.Name = "txtATEPrg";
            // 
            // txtATEResultPath
            // 
            resources.ApplyResources(this.txtATEResultPath, "txtATEResultPath");
            this.txtATEResultPath.Name = "txtATEResultPath";
            // 
            // panel6
            // 
            resources.ApplyResources(this.panel6, "panel6");
            this.panel6.Controls.Add(this.txtATEDelay, 0, 0);
            this.panel6.Controls.Add(this.label8, 1, 0);
            this.panel6.Name = "panel6";
            // 
            // txtATEDelay
            // 
            resources.ApplyResources(this.txtATEDelay, "txtATEDelay");
            this.txtATEDelay.Name = "txtATEDelay";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // panel7
            // 
            resources.ApplyResources(this.panel7, "panel7");
            this.panel7.Controls.Add(this.txtATERepeats, 0, 0);
            this.panel7.Name = "panel7";
            // 
            // txtATERepeats
            // 
            resources.ApplyResources(this.txtATERepeats, "txtATERepeats");
            this.txtATERepeats.Name = "txtATERepeats";
            // 
            // chkBarForm
            // 
            resources.ApplyResources(this.chkBarForm, "chkBarForm");
            this.chkBarForm.Name = "chkBarForm";
            this.chkBarForm.UseVisualStyleBackColor = true;
            // 
            // txtBarFormName
            // 
            resources.ApplyResources(this.txtBarFormName, "txtBarFormName");
            this.txtBarFormName.Name = "txtBarFormName";
            // 
            // chkImg
            // 
            resources.ApplyResources(this.chkImg, "chkImg");
            this.chkImg.Name = "chkImg";
            this.chkImg.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            // 
            // panel8
            // 
            resources.ApplyResources(this.panel8, "panel8");
            this.panel8.Controls.Add(this.txtATEMon, 0, 0);
            this.panel8.Controls.Add(this.label17, 1, 0);
            this.panel8.Name = "panel8";
            // 
            // txtATEMon
            // 
            resources.ApplyResources(this.txtATEMon, "txtATEMon");
            this.txtATEMon.Name = "txtATEMon";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            // 
            // label18
            // 
            resources.ApplyResources(this.label18, "label18");
            this.label18.Name = "label18";
            // 
            // panel9
            // 
            resources.ApplyResources(this.panel9, "panel9");
            this.panel9.Controls.Add(this.txtFailLock, 0, 0);
            this.panel9.Controls.Add(this.label19, 1, 0);
            this.panel9.Name = "panel9";
            // 
            // txtFailLock
            // 
            resources.ApplyResources(this.txtFailLock, "txtFailLock");
            this.txtFailLock.Name = "txtFailLock";
            // 
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.Name = "label19";
            // 
            // chkGetTestData
            // 
            resources.ApplyResources(this.chkGetTestData, "chkGetTestData");
            this.chkGetTestData.Name = "chkGetTestData";
            this.chkGetTestData.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage4.Controls.Add(this.panel4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Controls.Add(this.label20, 0, 0);
            this.panel4.Controls.Add(this.chkSaveReport, 0, 1);
            this.panel4.Controls.Add(this.label21, 0, 2);
            this.panel4.Controls.Add(this.txtModelPath, 1, 0);
            this.panel4.Controls.Add(this.txtReportPath, 1, 2);
            this.panel4.Controls.Add(this.panel5, 1, 1);
            this.panel4.Controls.Add(this.btnModel, 2, 0);
            this.panel4.Controls.Add(this.btnReport, 2, 2);
            this.panel4.Name = "panel4";
            // 
            // label20
            // 
            resources.ApplyResources(this.label20, "label20");
            this.label20.Name = "label20";
            // 
            // chkSaveReport
            // 
            resources.ApplyResources(this.chkSaveReport, "chkSaveReport");
            this.chkSaveReport.Name = "chkSaveReport";
            this.chkSaveReport.UseVisualStyleBackColor = true;
            // 
            // label21
            // 
            resources.ApplyResources(this.label21, "label21");
            this.label21.Name = "label21";
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
            this.panel5.Controls.Add(this.label22, 0, 0);
            this.panel5.Controls.Add(this.txtReportSaveTimes, 1, 0);
            this.panel5.Name = "panel5";
            // 
            // label22
            // 
            resources.ApplyResources(this.label22, "label22");
            this.label22.Name = "label22";
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
            // tabPage3
            // 
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage3.Controls.Add(this.panel3);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.chkMesCon, 0, 1);
            this.panel3.Controls.Add(this.label12, 0, 2);
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
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
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
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSerTcpIP;
        private System.Windows.Forms.TextBox txtSerTcpPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbChanMax;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbATELanguage;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtSerStatName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMonDelay;
        public System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtATEPrg;
        private System.Windows.Forms.TextBox txtATEResultPath;
        private System.Windows.Forms.TableLayoutPanel panel6;
        private System.Windows.Forms.TextBox txtATEDelay;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TableLayoutPanel panel7;
        private System.Windows.Forms.TextBox txtATERepeats;
        private System.Windows.Forms.CheckBox chkBarForm;
        private System.Windows.Forms.TextBox txtBarFormName;
        public System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbDevMax;
        private System.Windows.Forms.ComboBox cmbIO;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtIoDelayMs;
        private System.Windows.Forms.ComboBox cmbTcpMode;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox chkImg;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TableLayoutPanel panel8;
        private System.Windows.Forms.TextBox txtATEMon;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TableLayoutPanel panel9;
        private System.Windows.Forms.TextBox txtFailLock;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.CheckBox chkIoEnable;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.CheckBox chkMesCon;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtFailNoTran;
        private System.Windows.Forms.CheckBox chkGJWeb;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TableLayoutPanel panel4;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox chkSaveReport;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox txtModelPath;
        private System.Windows.Forms.TextBox txtReportPath;
        private System.Windows.Forms.TableLayoutPanel panel5;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox txtReportSaveTimes;
        private System.Windows.Forms.Button btnModel;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.CheckBox chkGetTestData;

    }
}