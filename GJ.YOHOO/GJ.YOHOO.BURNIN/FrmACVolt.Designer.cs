namespace GJ.YOHOO.BURNIN
{
    partial class FrmACVolt
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmACVolt));
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labWaveVolt = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labStartTime = new System.Windows.Forms.Label();
            this.labRunTime = new System.Windows.Forms.Label();
            this.labCurVolt = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label1, 0, 0);
            this.panel1.Controls.Add(this.panel2, 0, 2);
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.labWaveVolt, 7, 0);
            this.panel2.Controls.Add(this.label2, 0, 0);
            this.panel2.Controls.Add(this.label3, 2, 0);
            this.panel2.Controls.Add(this.label4, 4, 0);
            this.panel2.Controls.Add(this.labStartTime, 1, 0);
            this.panel2.Controls.Add(this.labRunTime, 3, 0);
            this.panel2.Controls.Add(this.labCurVolt, 5, 0);
            this.panel2.Controls.Add(this.label5, 6, 0);
            this.panel2.Name = "panel2";
            // 
            // labWaveVolt
            // 
            resources.ApplyResources(this.labWaveVolt, "labWaveVolt");
            this.labWaveVolt.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labWaveVolt.ForeColor = System.Drawing.Color.Cyan;
            this.labWaveVolt.Name = "labWaveVolt";
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
            // labStartTime
            // 
            resources.ApplyResources(this.labStartTime, "labStartTime");
            this.labStartTime.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labStartTime.ForeColor = System.Drawing.Color.Cyan;
            this.labStartTime.Name = "labStartTime";
            // 
            // labRunTime
            // 
            resources.ApplyResources(this.labRunTime, "labRunTime");
            this.labRunTime.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labRunTime.ForeColor = System.Drawing.Color.Cyan;
            this.labRunTime.Name = "labRunTime";
            // 
            // labCurVolt
            // 
            resources.ApplyResources(this.labCurVolt, "labCurVolt");
            this.labCurVolt.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labCurVolt.ForeColor = System.Drawing.Color.Cyan;
            this.labCurVolt.Name = "labCurVolt";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FrmACVolt
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmACVolt";
            this.Load += new System.EventHandler(this.FrmACVolt_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labStartTime;
        private System.Windows.Forms.Label labRunTime;
        private System.Windows.Forms.Label labCurVolt;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labWaveVolt;
        private System.Windows.Forms.Label label5;
    }
}