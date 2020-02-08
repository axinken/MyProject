namespace GJ.USER.APP
{
    partial class FrmSerTCP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSerTCP));
            this.TcpLog = new GJ.UI.udcRunLog();
            this.SuspendLayout();
            // 
            // TcpLog
            // 
            this.TcpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TcpLog.Location = new System.Drawing.Point(0, 0);
            this.TcpLog.mFont = new System.Drawing.Font("宋体", 9F);
            this.TcpLog.mMaxLine = 1000;
            this.TcpLog.mMaxMB = 1D;
            this.TcpLog.mSaveEnable = true;
            this.TcpLog.mSaveFolder = "";
            this.TcpLog.mSaveName = "TCPLog";
            this.TcpLog.mTitle = "TCP/IP监控";
            this.TcpLog.mTitleEnable = true;
            this.TcpLog.Name = "TcpLog";
            this.TcpLog.Size = new System.Drawing.Size(401, 405);
            this.TcpLog.TabIndex = 0;
            // 
            // FrmSerTCP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 405);
            this.Controls.Add(this.TcpLog);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmSerTCP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "服务器TCP/IP监控";
            this.ResumeLayout(false);

        }

        #endregion

        private UI.udcRunLog TcpLog;
    }
}