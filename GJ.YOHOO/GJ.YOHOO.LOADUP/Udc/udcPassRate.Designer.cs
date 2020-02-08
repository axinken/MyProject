namespace GJ.YOHOO.LOADUP.Udc
{
    partial class udcPassRate
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcPassRate));
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labPassRateLimit = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labTTNum = new System.Windows.Forms.Label();
            this.labPassNum = new System.Windows.Forms.Label();
            this.labPassRate = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.BtnZero = new System.Windows.Forms.Button();
            this.btnCfgAlarm = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.labStatus = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.labTitle, 0, 0);
            this.panel1.Controls.Add(this.panel2, 0, 1);
            this.panel1.Controls.Add(this.panel3, 0, 2);
            this.panel1.Name = "panel1";
            // 
            // labTitle
            // 
            resources.ApplyResources(this.labTitle, "labTitle");
            this.labTitle.ForeColor = System.Drawing.Color.Navy;
            this.labTitle.Name = "labTitle";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.labPassRateLimit, 1, 3);
            this.panel2.Controls.Add(this.label6, 0, 3);
            this.panel2.Controls.Add(this.label2, 0, 0);
            this.panel2.Controls.Add(this.label3, 0, 1);
            this.panel2.Controls.Add(this.label4, 0, 2);
            this.panel2.Controls.Add(this.labTTNum, 1, 0);
            this.panel2.Controls.Add(this.labPassNum, 1, 1);
            this.panel2.Controls.Add(this.labPassRate, 1, 2);
            this.panel2.Controls.Add(this.tableLayoutPanel1, 1, 4);
            this.panel2.Name = "panel2";
            // 
            // labPassRateLimit
            // 
            resources.ApplyResources(this.labPassRateLimit, "labPassRateLimit");
            this.labPassRateLimit.BackColor = System.Drawing.Color.White;
            this.labPassRateLimit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labPassRateLimit.Name = "labPassRateLimit";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // labTTNum
            // 
            resources.ApplyResources(this.labTTNum, "labTTNum");
            this.labTTNum.BackColor = System.Drawing.Color.White;
            this.labTTNum.Name = "labTTNum";
            // 
            // labPassNum
            // 
            resources.ApplyResources(this.labPassNum, "labPassNum");
            this.labPassNum.BackColor = System.Drawing.Color.White;
            this.labPassNum.Name = "labPassNum";
            // 
            // labPassRate
            // 
            resources.ApplyResources(this.labPassRate, "labPassRate");
            this.labPassRate.BackColor = System.Drawing.Color.White;
            this.labPassRate.ForeColor = System.Drawing.Color.Green;
            this.labPassRate.Name = "labPassRate";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.BtnZero, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCfgAlarm, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // BtnZero
            // 
            resources.ApplyResources(this.BtnZero, "BtnZero");
            this.BtnZero.Name = "BtnZero";
            this.BtnZero.UseVisualStyleBackColor = true;
            this.BtnZero.Click += new System.EventHandler(this.BtnZero_Click);
            // 
            // btnCfgAlarm
            // 
            resources.ApplyResources(this.btnCfgAlarm, "btnCfgAlarm");
            this.btnCfgAlarm.BackColor = System.Drawing.Color.Red;
            this.btnCfgAlarm.Name = "btnCfgAlarm";
            this.btnCfgAlarm.UseVisualStyleBackColor = false;
            this.btnCfgAlarm.Click += new System.EventHandler(this.btnCfgAlarm_Click);
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.label5, 0, 0);
            this.panel3.Controls.Add(this.label7, 1, 0);
            this.panel3.Controls.Add(this.labStatus, 1, 1);
            this.panel3.Name = "panel3";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            this.panel3.SetRowSpan(this.label5, 2);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label7.Name = "label7";
            // 
            // labStatus
            // 
            resources.ApplyResources(this.labStatus, "labStatus");
            this.labStatus.BackColor = System.Drawing.Color.White;
            this.labStatus.Name = "labStatus";
            // 
            // udcPassRate
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "udcPassRate";
            this.Load += new System.EventHandler(this.udcYieldLock_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label labTitle;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label labPassRateLimit;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labTTNum;
        private System.Windows.Forms.Label labPassNum;
        private System.Windows.Forms.Label labPassRate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button BtnZero;
        private System.Windows.Forms.Button btnCfgAlarm;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labStatus;
    }
}
