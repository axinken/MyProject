namespace GJ.YOHOO.BURNIN.Udc
{
    partial class udcOnOff
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcOnOff));
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labOnOff = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOnOff = new System.Windows.Forms.TextBox();
            this.labOn = new System.Windows.Forms.Label();
            this.labOff = new System.Windows.Forms.Label();
            this.txtOn = new System.Windows.Forms.TextBox();
            this.txtOff = new System.Windows.Forms.TextBox();
            this.labOnUint = new System.Windows.Forms.Label();
            this.labOffUint = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbVType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbACV = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkUnit = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel2, 0, 1);
            this.panel1.Controls.Add(this.chkUnit, 0, 0);
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.labOnOff, 0, 0);
            this.panel2.Controls.Add(this.label2, 2, 0);
            this.panel2.Controls.Add(this.txtOnOff, 1, 0);
            this.panel2.Controls.Add(this.labOn, 0, 1);
            this.panel2.Controls.Add(this.labOff, 0, 2);
            this.panel2.Controls.Add(this.txtOn, 1, 1);
            this.panel2.Controls.Add(this.txtOff, 1, 2);
            this.panel2.Controls.Add(this.labOnUint, 2, 1);
            this.panel2.Controls.Add(this.labOffUint, 2, 2);
            this.panel2.Controls.Add(this.label1, 0, 4);
            this.panel2.Controls.Add(this.cmbVType, 1, 4);
            this.panel2.Controls.Add(this.label3, 0, 3);
            this.panel2.Controls.Add(this.cmbACV, 1, 3);
            this.panel2.Controls.Add(this.label4, 2, 3);
            this.panel2.Name = "panel2";
            // 
            // labOnOff
            // 
            resources.ApplyResources(this.labOnOff, "labOnOff");
            this.labOnOff.Name = "labOnOff";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtOnOff
            // 
            resources.ApplyResources(this.txtOnOff, "txtOnOff");
            this.txtOnOff.Name = "txtOnOff";
            // 
            // labOn
            // 
            resources.ApplyResources(this.labOn, "labOn");
            this.labOn.Name = "labOn";
            // 
            // labOff
            // 
            resources.ApplyResources(this.labOff, "labOff");
            this.labOff.Name = "labOff";
            // 
            // txtOn
            // 
            resources.ApplyResources(this.txtOn, "txtOn");
            this.txtOn.Name = "txtOn";
            // 
            // txtOff
            // 
            resources.ApplyResources(this.txtOff, "txtOff");
            this.txtOff.Name = "txtOff";
            // 
            // labOnUint
            // 
            resources.ApplyResources(this.labOnUint, "labOnUint");
            this.labOnUint.Name = "labOnUint";
            // 
            // labOffUint
            // 
            resources.ApplyResources(this.labOffUint, "labOffUint");
            this.labOffUint.Name = "labOffUint";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbVType
            // 
            resources.ApplyResources(this.cmbVType, "cmbVType");
            this.cmbVType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVType.FormattingEnabled = true;
            this.cmbVType.Name = "cmbVType";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbACV
            // 
            resources.ApplyResources(this.cmbACV, "cmbACV");
            this.cmbACV.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbACV.FormattingEnabled = true;
            this.cmbACV.Items.AddRange(new object[] {
            resources.GetString("cmbACV.Items"),
            resources.GetString("cmbACV.Items1"),
            resources.GetString("cmbACV.Items2"),
            resources.GetString("cmbACV.Items3")});
            this.cmbACV.Name = "cmbACV";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // chkUnit
            // 
            resources.ApplyResources(this.chkUnit, "chkUnit");
            this.chkUnit.Name = "chkUnit";
            this.chkUnit.UseVisualStyleBackColor = true;
            this.chkUnit.Click += new System.EventHandler(this.chkUnit_Click);
            // 
            // udcOnOff
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "udcOnOff";
            this.Load += new System.EventHandler(this.udcOnOff_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label labOnOff;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOnOff;
        private System.Windows.Forms.Label labOn;
        private System.Windows.Forms.Label labOff;
        private System.Windows.Forms.TextBox txtOn;
        private System.Windows.Forms.TextBox txtOff;
        private System.Windows.Forms.Label labOnUint;
        private System.Windows.Forms.Label labOffUint;
        private System.Windows.Forms.CheckBox chkUnit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbVType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbACV;
        private System.Windows.Forms.Label label4;

    }
}
