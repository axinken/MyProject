namespace GJ.KunX.LOADUP.Udc
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcStatFixture));
            this.panel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tlTip = new System.Windows.Forms.ToolTip(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.labIdCard = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.panel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.labStatus = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            this.tlTip.SetToolTip(this.panel4, resources.GetString("panel4.ToolTip"));
            // 
            // tlTip
            // 
            this.tlTip.BackColor = System.Drawing.Color.Cornsilk;
            this.tlTip.ForeColor = System.Drawing.Color.Black;
            this.tlTip.IsBalloon = true;
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackColor = System.Drawing.Color.Red;
            this.btnOK.Name = "btnOK";
            this.tlTip.SetToolTip(this.btnOK, resources.GetString("btnOK.ToolTip"));
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.panel3, 0, 0);
            this.panel2.Controls.Add(this.panel5, 0, 2);
            this.panel2.Controls.Add(this.panel4, 0, 1);
            this.panel2.Name = "panel2";
            this.tlTip.SetToolTip(this.panel2, resources.GetString("panel2.ToolTip"));
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.label1, 0, 0);
            this.panel3.Controls.Add(this.labIdCard, 1, 0);
            this.panel3.Controls.Add(this.btnOK, 4, 0);
            this.panel3.Controls.Add(this.btnReset, 2, 0);
            this.panel3.Name = "panel3";
            this.tlTip.SetToolTip(this.panel3, resources.GetString("panel3.ToolTip"));
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.tlTip.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
            // 
            // labIdCard
            // 
            resources.ApplyResources(this.labIdCard, "labIdCard");
            this.labIdCard.Name = "labIdCard";
            this.tlTip.SetToolTip(this.labIdCard, resources.GetString("labIdCard.ToolTip"));
            // 
            // btnReset
            // 
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.Name = "btnReset";
            this.tlTip.SetToolTip(this.btnReset, resources.GetString("btnReset.ToolTip"));
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Controls.Add(this.label2, 0, 0);
            this.panel5.Controls.Add(this.labStatus, 1, 0);
            this.panel5.Name = "panel5";
            this.tlTip.SetToolTip(this.panel5, resources.GetString("panel5.ToolTip"));
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.tlTip.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
            // 
            // labStatus
            // 
            resources.ApplyResources(this.labStatus, "labStatus");
            this.labStatus.ForeColor = System.Drawing.Color.Blue;
            this.labStatus.Name = "labStatus";
            this.tlTip.SetToolTip(this.labStatus, resources.GetString("labStatus.ToolTip"));
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel2, 0, 0);
            this.panel1.Name = "panel1";
            this.tlTip.SetToolTip(this.panel1, resources.GetString("panel1.ToolTip"));
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
            // udcStatFixture
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "udcStatFixture";
            this.tlTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.udcStatFixture_Load);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel4;
        private System.Windows.Forms.ToolTip tlTip;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.TableLayoutPanel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labIdCard;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel panel5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.Button btnReset;
    }
}
