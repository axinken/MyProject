namespace GJ.KunX.BURNIN
{
    partial class FrmReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmReport));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.treeFiles = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labStatus = new System.Windows.Forms.Label();
            this.rtbRunLog = new System.Windows.Forms.RichTextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.rtbRunLog);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.treeFiles, 0, 0);
            this.panel1.Controls.Add(this.panel2, 0, 1);
            this.panel1.Name = "panel1";
            // 
            // treeFiles
            // 
            resources.ApplyResources(this.treeFiles, "treeFiles");
            this.treeFiles.Name = "treeFiles";
            this.treeFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFiles_NodeMouseClick);
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.progressBar1, 0, 0);
            this.panel2.Controls.Add(this.labStatus, 1, 0);
            this.panel2.Name = "panel2";
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // labStatus
            // 
            resources.ApplyResources(this.labStatus, "labStatus");
            this.labStatus.BackColor = System.Drawing.Color.Lime;
            this.labStatus.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labStatus.Name = "labStatus";
            // 
            // rtbRunLog
            // 
            resources.ApplyResources(this.rtbRunLog, "rtbRunLog");
            this.rtbRunLog.Name = "rtbRunLog";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Tree View.ico");
            this.imageList1.Images.SetKeyName(1, "Folder List.ico");
            this.imageList1.Images.SetKeyName(2, "folder.ico");
            this.imageList1.Images.SetKeyName(3, "Folder-Closed.ico");
            this.imageList1.Images.SetKeyName(4, "View-One Page.ico");
            this.imageList1.Images.SetKeyName(5, "4577.ico");
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FrmReport
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FrmReport";
            this.Load += new System.EventHandler(this.FrmReport_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeFiles;
        private System.Windows.Forms.RichTextBox rtbRunLog;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.Timer timer1;

    }
}