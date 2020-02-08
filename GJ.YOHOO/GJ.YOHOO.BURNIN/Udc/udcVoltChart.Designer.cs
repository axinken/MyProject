namespace GJ.YOHOO.BURNIN.Udc
{
    partial class udcVoltChart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcVoltChart));
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labBITime = new System.Windows.Forms.Label();
            this.labRunTime = new System.Windows.Forms.Label();
            this.labStartTime = new System.Windows.Forms.Label();
            this.labEndTime = new System.Windows.Forms.Label();
            this.labCurTemp = new System.Windows.Forms.Label();
            this.labCurACV = new System.Windows.Forms.Label();
            this.labLocalName = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label1, 0, 2);
            this.panel1.Controls.Add(this.label2, 0, 4);
            this.panel1.Controls.Add(this.panel2, 0, 1);
            this.panel1.Controls.Add(this.labLocalName, 0, 0);
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
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.label3, 0, 0);
            this.panel2.Controls.Add(this.label4, 0, 1);
            this.panel2.Controls.Add(this.label5, 2, 0);
            this.panel2.Controls.Add(this.label6, 2, 1);
            this.panel2.Controls.Add(this.label7, 4, 0);
            this.panel2.Controls.Add(this.label8, 4, 1);
            this.panel2.Controls.Add(this.labBITime, 1, 0);
            this.panel2.Controls.Add(this.labRunTime, 1, 1);
            this.panel2.Controls.Add(this.labStartTime, 3, 0);
            this.panel2.Controls.Add(this.labEndTime, 3, 1);
            this.panel2.Controls.Add(this.labCurTemp, 5, 0);
            this.panel2.Controls.Add(this.labCurACV, 5, 1);
            this.panel2.Name = "panel2";
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
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // labBITime
            // 
            resources.ApplyResources(this.labBITime, "labBITime");
            this.labBITime.BackColor = System.Drawing.Color.White;
            this.labBITime.Name = "labBITime";
            // 
            // labRunTime
            // 
            resources.ApplyResources(this.labRunTime, "labRunTime");
            this.labRunTime.BackColor = System.Drawing.Color.White;
            this.labRunTime.Name = "labRunTime";
            // 
            // labStartTime
            // 
            resources.ApplyResources(this.labStartTime, "labStartTime");
            this.labStartTime.BackColor = System.Drawing.Color.White;
            this.labStartTime.Name = "labStartTime";
            // 
            // labEndTime
            // 
            resources.ApplyResources(this.labEndTime, "labEndTime");
            this.labEndTime.BackColor = System.Drawing.Color.White;
            this.labEndTime.Name = "labEndTime";
            // 
            // labCurTemp
            // 
            resources.ApplyResources(this.labCurTemp, "labCurTemp");
            this.labCurTemp.BackColor = System.Drawing.Color.Black;
            this.labCurTemp.ForeColor = System.Drawing.Color.Chartreuse;
            this.labCurTemp.Name = "labCurTemp";
            // 
            // labCurACV
            // 
            resources.ApplyResources(this.labCurACV, "labCurACV");
            this.labCurACV.BackColor = System.Drawing.Color.Black;
            this.labCurACV.ForeColor = System.Drawing.Color.Chartreuse;
            this.labCurACV.Name = "labCurACV";
            // 
            // labLocalName
            // 
            resources.ApplyResources(this.labLocalName, "labLocalName");
            this.labLocalName.Name = "labLocalName";
            // 
            // udcVoltChart
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "udcVoltChart";
            this.Load += new System.EventHandler(this.udcVoltChart_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labBITime;
        private System.Windows.Forms.Label labRunTime;
        private System.Windows.Forms.Label labStartTime;
        private System.Windows.Forms.Label labEndTime;
        private System.Windows.Forms.Label labCurTemp;
        private System.Windows.Forms.Label labCurACV;
        private System.Windows.Forms.Label labLocalName;
    }
}