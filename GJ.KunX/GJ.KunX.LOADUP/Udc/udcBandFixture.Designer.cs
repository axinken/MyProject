namespace GJ.KunX.LOADUP.Udc
{
    partial class udcBandFixture
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcBandFixture));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.txtSnPress = new System.Windows.Forms.TextBox();
            this.panel6 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.TableLayoutPanel();
            this.labStatus = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.labFixStatus = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.labIdCard = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tlTip = new System.Windows.Forms.ToolTip(this.components);
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "PASS");
            this.imageList1.Images.SetKeyName(1, "FAIL");
            this.imageList1.Images.SetKeyName(2, "READY");
            this.imageList1.Images.SetKeyName(3, "TEST");
            // 
            // txtSnPress
            // 
            resources.ApplyResources(this.txtSnPress, "txtSnPress");
            this.txtSnPress.Name = "txtSnPress";
            this.txtSnPress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSnPress_KeyPress);
            // 
            // panel6
            // 
            resources.ApplyResources(this.panel6, "panel6");
            this.panel6.Controls.Add(this.txtSnPress, 1, 0);
            this.panel6.Controls.Add(this.label5, 0, 0);
            this.panel6.Name = "panel6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // labStatus
            // 
            resources.ApplyResources(this.labStatus, "labStatus");
            this.labStatus.ForeColor = System.Drawing.Color.Blue;
            this.labStatus.Name = "labStatus";
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Controls.Add(this.label2, 0, 1);
            this.panel5.Controls.Add(this.labStatus, 1, 1);
            this.panel5.Controls.Add(this.label3, 0, 0);
            this.panel5.Controls.Add(this.labFixStatus, 1, 0);
            this.panel5.Name = "panel5";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // labFixStatus
            // 
            resources.ApplyResources(this.labFixStatus, "labFixStatus");
            this.labFixStatus.Name = "labFixStatus";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.label1, 0, 0);
            this.panel3.Controls.Add(this.labIdCard, 1, 0);
            this.panel3.Name = "panel3";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // labIdCard
            // 
            resources.ApplyResources(this.labIdCard, "labIdCard");
            this.labIdCard.Name = "labIdCard";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.panel3, 0, 0);
            this.panel2.Controls.Add(this.panel5, 0, 3);
            this.panel2.Controls.Add(this.panel4, 0, 1);
            this.panel2.Controls.Add(this.panel6, 0, 2);
            this.panel2.Name = "panel2";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel2, 0, 0);
            this.panel1.Name = "panel1";
            // 
            // tlTip
            // 
            this.tlTip.BackColor = System.Drawing.Color.Cornsilk;
            this.tlTip.ForeColor = System.Drawing.Color.Black;
            this.tlTip.IsBalloon = true;
            // 
            // udcBandFixture
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "udcBandFixture";
            this.Load += new System.EventHandler(this.udcFixSnBar_Load);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TextBox txtSnPress;
        private System.Windows.Forms.TableLayoutPanel panel6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel panel4;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.TableLayoutPanel panel5;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labIdCard;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.ToolTip tlTip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labFixStatus;
    }
}
