namespace GJ.YOHOO.BURNIN.Udc
{
    partial class udcModelBase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcModelBase));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.labModel = new System.Windows.Forms.Label();
            this.labACV = new System.Windows.Forms.Label();
            this.labBITime = new System.Windows.Forms.Label();
            this.labTSet = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnModel = new System.Windows.Forms.Button();
            this.labTRead = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "OPEN");
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label13, 0, 0);
            this.panel1.Controls.Add(this.label14, 0, 1);
            this.panel1.Controls.Add(this.label15, 0, 2);
            this.panel1.Controls.Add(this.label16, 0, 3);
            this.panel1.Controls.Add(this.labModel, 1, 0);
            this.panel1.Controls.Add(this.labACV, 1, 1);
            this.panel1.Controls.Add(this.labBITime, 1, 2);
            this.panel1.Controls.Add(this.labTSet, 1, 3);
            this.panel1.Controls.Add(this.panel2, 1, 4);
            this.panel1.Controls.Add(this.label27, 0, 4);
            this.panel1.Name = "panel1";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            // 
            // labModel
            // 
            resources.ApplyResources(this.labModel, "labModel");
            this.labModel.BackColor = System.Drawing.SystemColors.Control;
            this.labModel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.labModel.Name = "labModel";
            // 
            // labACV
            // 
            resources.ApplyResources(this.labACV, "labACV");
            this.labACV.BackColor = System.Drawing.SystemColors.Control;
            this.labACV.Name = "labACV";
            // 
            // labBITime
            // 
            resources.ApplyResources(this.labBITime, "labBITime");
            this.labBITime.Name = "labBITime";
            // 
            // labTSet
            // 
            resources.ApplyResources(this.labTSet, "labTSet");
            this.labTSet.Name = "labTSet";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.btnModel, 2, 0);
            this.panel2.Controls.Add(this.labTRead, 0, 0);
            this.panel2.Controls.Add(this.label29, 1, 0);
            this.panel2.Name = "panel2";
            // 
            // btnModel
            // 
            resources.ApplyResources(this.btnModel, "btnModel");
            this.btnModel.ImageList = this.imageList1;
            this.btnModel.Name = "btnModel";
            this.btnModel.UseVisualStyleBackColor = true;
            this.btnModel.Click += new System.EventHandler(this.btnModel_Click);
            // 
            // labTRead
            // 
            resources.ApplyResources(this.labTRead, "labTRead");
            this.labTRead.BackColor = System.Drawing.Color.Black;
            this.labTRead.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labTRead.ForeColor = System.Drawing.Color.Lime;
            this.labTRead.Name = "labTRead";
            this.labTRead.DoubleClick += new System.EventHandler(this.labTRead_DoubleClick);
            // 
            // label29
            // 
            resources.ApplyResources(this.label29, "label29");
            this.label29.Name = "label29";
            // 
            // label27
            // 
            resources.ApplyResources(this.label27, "label27");
            this.label27.Name = "label27";
            // 
            // udcModelBase
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "udcModelBase";
            this.Load += new System.EventHandler(this.udcModelBase_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label labModel;
        private System.Windows.Forms.Label labACV;
        private System.Windows.Forms.Label labBITime;
        private System.Windows.Forms.Label labTSet;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Button btnModel;
        private System.Windows.Forms.Label labTRead;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label27;
    }
}
