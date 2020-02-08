namespace GJ.YOHOO.ATE.Udc
{
    partial class udcStatFixture
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcStatFixture));
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labIdCard = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.labStatus = new System.Windows.Forms.Label();
            this.btnDebug1 = new System.Windows.Forms.Button();
            this.btnDebug2 = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.panel3, 0, 0);
            this.panel2.Controls.Add(this.panel1, 0, 2);
            this.panel2.Name = "panel2";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.label1, 0, 0);
            this.panel3.Controls.Add(this.label2, 2, 0);
            this.panel3.Controls.Add(this.labIdCard, 1, 0);
            this.panel3.Name = "panel3";
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
            // labIdCard
            // 
            resources.ApplyResources(this.labIdCard, "labIdCard");
            this.labIdCard.ForeColor = System.Drawing.Color.Blue;
            this.labIdCard.Name = "labIdCard";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label4, 0, 0);
            this.panel1.Controls.Add(this.labStatus, 1, 0);
            this.panel1.Controls.Add(this.btnDebug1, 2, 0);
            this.panel1.Controls.Add(this.btnDebug2, 3, 0);
            this.panel1.Name = "panel1";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // labStatus
            // 
            resources.ApplyResources(this.labStatus, "labStatus");
            this.labStatus.ForeColor = System.Drawing.Color.Blue;
            this.labStatus.Name = "labStatus";
            // 
            // btnDebug1
            // 
            resources.ApplyResources(this.btnDebug1, "btnDebug1");
            this.btnDebug1.Name = "btnDebug1";
            this.btnDebug1.UseVisualStyleBackColor = true;
            this.btnDebug1.Click += new System.EventHandler(this.btnDebug1_Click);
            // 
            // btnDebug2
            // 
            resources.ApplyResources(this.btnDebug2, "btnDebug2");
            this.btnDebug2.Name = "btnDebug2";
            this.btnDebug2.UseVisualStyleBackColor = true;
            this.btnDebug2.Click += new System.EventHandler(this.btnDebug2_Click);
            // 
            // udcStatFixture
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Name = "udcStatFixture";
            this.Load += new System.EventHandler(this.udcStatFixture_Load);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labIdCard;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.Button btnDebug1;
        private System.Windows.Forms.Button btnDebug2;
    }
}
