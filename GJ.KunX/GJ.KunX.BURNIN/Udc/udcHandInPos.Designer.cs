namespace GJ.KunX.BURNIN.Udc
{
    partial class udcHandInPos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcHandInPos));
            this.panel1 = new System.Windows.Forms.Panel();
            this.BtnExit = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.txtCol = new System.Windows.Forms.TextBox();
            this.txtRow = new System.Windows.Forms.TextBox();
            this.labCol = new System.Windows.Forms.Label();
            this.labRow = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Aqua;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.BtnExit);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.txtCol);
            this.panel1.Controls.Add(this.txtRow);
            this.panel1.Controls.Add(this.labCol);
            this.panel1.Controls.Add(this.labRow);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(338, 220);
            this.panel1.TabIndex = 0;
            // 
            // BtnExit
            // 
            this.BtnExit.ImageKey = "EXIT";
            this.BtnExit.ImageList = this.imageList1;
            this.BtnExit.Location = new System.Drawing.Point(174, 148);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(91, 41);
            this.BtnExit.TabIndex = 6;
            this.BtnExit.Text = "取消(&C)";
            this.BtnExit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "EXIT");
            this.imageList1.Images.SetKeyName(1, "OK");
            // 
            // btnOK
            // 
            this.btnOK.ImageKey = "OK";
            this.btnOK.ImageList = this.imageList1;
            this.btnOK.Location = new System.Drawing.Point(51, 148);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(91, 41);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确认(&O)";
            this.btnOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtCol
            // 
            this.txtCol.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCol.Location = new System.Drawing.Point(126, 98);
            this.txtCol.Name = "txtCol";
            this.txtCol.Size = new System.Drawing.Size(138, 26);
            this.txtCol.TabIndex = 4;
            this.txtCol.Text = "1";
            this.txtCol.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCol.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCol_KeyPress);
            // 
            // txtRow
            // 
            this.txtRow.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtRow.Location = new System.Drawing.Point(126, 51);
            this.txtRow.Name = "txtRow";
            this.txtRow.Size = new System.Drawing.Size(138, 26);
            this.txtRow.TabIndex = 3;
            this.txtRow.Text = "1";
            this.txtRow.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtRow.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRow_KeyPress);
            // 
            // labCol
            // 
            this.labCol.AutoSize = true;
            this.labCol.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labCol.Location = new System.Drawing.Point(24, 101);
            this.labCol.Name = "labCol";
            this.labCol.Size = new System.Drawing.Size(96, 16);
            this.labCol.TabIndex = 2;
            this.labCol.Text = "列数(1-24):";
            // 
            // labRow
            // 
            this.labRow.AutoSize = true;
            this.labRow.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labRow.Location = new System.Drawing.Point(24, 56);
            this.labRow.Name = "labRow";
            this.labRow.Size = new System.Drawing.Size(96, 16);
            this.labRow.TabIndex = 1;
            this.labRow.Text = "层数(1-10):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label1.Location = new System.Drawing.Point(79, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "输入当前进机位置";
            // 
            // udcHandInPos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 220);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "udcHandInPos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "手动输入进机位置";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.udcHandInPos_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labCol;
        private System.Windows.Forms.Label labRow;
        private System.Windows.Forms.TextBox txtCol;
        private System.Windows.Forms.TextBox txtRow;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.Button btnOK;
    }
}