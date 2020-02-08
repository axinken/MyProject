namespace GJ.KunX.BURNIN
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkSelOut = new System.Windows.Forms.CheckBox();
            this.cmbModelList = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.labModelTTNum = new System.Windows.Forms.Label();
            this.labModelOutNum = new System.Windows.Forms.Label();
            this.labOutModel = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.labVInfo = new System.Windows.Forms.Label();
            this.labBaseInfo = new System.Windows.Forms.Label();
            this.labStatus = new System.Windows.Forms.Label();
            this.TmrStatus = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.labAction = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.tabControl1, 0, 6);
            this.panel2.Controls.Add(this.labVInfo, 0, 2);
            this.panel2.Controls.Add(this.labBaseInfo, 0, 0);
            this.panel2.Controls.Add(this.labStatus, 0, 4);
            this.panel2.Name = "panel2";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage4);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            // 
            // tabPage3
            // 
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabPage1.Controls.Add(this.panel3);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.chkSelOut, 0, 0);
            this.panel3.Controls.Add(this.cmbModelList, 1, 0);
            this.panel3.Controls.Add(this.label12, 0, 1);
            this.panel3.Controls.Add(this.label17, 0, 2);
            this.panel3.Controls.Add(this.labModelTTNum, 1, 1);
            this.panel3.Controls.Add(this.labModelOutNum, 1, 2);
            this.panel3.Controls.Add(this.labOutModel, 1, 3);
            this.panel3.Name = "panel3";
            // 
            // chkSelOut
            // 
            resources.ApplyResources(this.chkSelOut, "chkSelOut");
            this.chkSelOut.Name = "chkSelOut";
            this.chkSelOut.UseVisualStyleBackColor = true;
            this.chkSelOut.CheckedChanged += new System.EventHandler(this.chkSelOut_CheckedChanged);
            // 
            // cmbModelList
            // 
            resources.ApplyResources(this.cmbModelList, "cmbModelList");
            this.cmbModelList.FormattingEnabled = true;
            this.cmbModelList.Name = "cmbModelList";
            this.cmbModelList.SelectedIndexChanged += new System.EventHandler(this.cmbModelList_SelectedIndexChanged);
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            // 
            // labModelTTNum
            // 
            resources.ApplyResources(this.labModelTTNum, "labModelTTNum");
            this.labModelTTNum.Name = "labModelTTNum";
            // 
            // labModelOutNum
            // 
            resources.ApplyResources(this.labModelOutNum, "labModelOutNum");
            this.labModelOutNum.Name = "labModelOutNum";
            // 
            // labOutModel
            // 
            resources.ApplyResources(this.labOutModel, "labOutModel");
            this.labOutModel.Name = "labOutModel";
            // 
            // tabPage4
            // 
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // labVInfo
            // 
            resources.ApplyResources(this.labVInfo, "labVInfo");
            this.labVInfo.Name = "labVInfo";
            // 
            // labBaseInfo
            // 
            resources.ApplyResources(this.labBaseInfo, "labBaseInfo");
            this.labBaseInfo.Name = "labBaseInfo";
            // 
            // labStatus
            // 
            resources.ApplyResources(this.labStatus, "labStatus");
            this.labStatus.Name = "labStatus";
            // 
            // TmrStatus
            // 
            this.TmrStatus.Interval = 500;
            this.TmrStatus.Tick += new System.EventHandler(this.TmrStatus_Tick);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.labAction);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // labAction
            // 
            resources.ApplyResources(this.labAction, "labAction");
            this.labAction.BackColor = System.Drawing.Color.White;
            this.labAction.ForeColor = System.Drawing.Color.Red;
            this.labAction.Name = "labAction";
            // 
            // FrmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FrmMain";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMain_FormClosed);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labBaseInfo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label labVInfo;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.CheckBox chkSelOut;
        private System.Windows.Forms.ComboBox cmbModelList;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label labModelTTNum;
        private System.Windows.Forms.Label labModelOutNum;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Timer TmrStatus;
        private System.Windows.Forms.Label labOutModel;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label labAction;
        private System.Windows.Forms.TableLayoutPanel panel1;


    }
}