namespace GJ.YOHOO.BURNIN.Udc
{
    partial class udcFixture
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(udcFixture));
            this.ImageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tlTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuOp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tlDisplay = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tlChart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tlFree = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tlForbit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.tlStartBI = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tlEndBI = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tlClrAlarm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tlClrFail = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tlResetBI = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.tlInPos = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.tlInPosFirstEnd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.tlSetIsNull = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tlResetQCM = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.tlFirstOut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOp.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImageList1
            // 
            this.ImageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList1.ImageStream")));
            this.ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList1.Images.SetKeyName(0, "FREE");
            this.ImageList1.Images.SetKeyName(1, "PASS1");
            this.ImageList1.Images.SetKeyName(2, "FAIL1");
            this.ImageList1.Images.SetKeyName(3, "PASS2");
            this.ImageList1.Images.SetKeyName(4, "FAIL2");
            // 
            // tlTip
            // 
            this.tlTip.BackColor = System.Drawing.Color.LightYellow;
            this.tlTip.IsBalloon = true;
            // 
            // menuOp
            // 
            resources.ApplyResources(this.menuOp, "menuOp");
            this.menuOp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlDisplay,
            this.toolStripSeparator4,
            this.tlChart,
            this.toolStripSeparator7,
            this.tlFree,
            this.toolStripMenuItem1,
            this.tlForbit,
            this.toolStripMenuItem2,
            this.tlStartBI,
            this.toolStripSeparator2,
            this.tlEndBI,
            this.toolStripSeparator3,
            this.tlClrAlarm,
            this.toolStripSeparator1,
            this.tlClrFail,
            this.toolStripSeparator5,
            this.tlResetBI,
            this.toolStripMenuItem3,
            this.tlInPos,
            this.toolStripMenuItem4,
            this.tlInPosFirstEnd,
            this.toolStripMenuItem5,
            this.tlSetIsNull,
            this.toolStripSeparator6,
            this.tlResetQCM,
            this.toolStripMenuItem7,
            this.tlFirstOut});
            this.menuOp.Name = "contextMenuStrip1";
            this.tlTip.SetToolTip(this.menuOp, resources.GetString("menuOp.ToolTip"));
            // 
            // tlDisplay
            // 
            resources.ApplyResources(this.tlDisplay, "tlDisplay");
            this.tlDisplay.Name = "tlDisplay";
            this.tlDisplay.Click += new System.EventHandler(this.tlDisplay_Click);
            // 
            // toolStripSeparator4
            // 
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // tlChart
            // 
            resources.ApplyResources(this.tlChart, "tlChart");
            this.tlChart.Name = "tlChart";
            this.tlChart.Click += new System.EventHandler(this.tlChart_Click);
            // 
            // toolStripSeparator7
            // 
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            // 
            // tlFree
            // 
            resources.ApplyResources(this.tlFree, "tlFree");
            this.tlFree.Name = "tlFree";
            this.tlFree.Click += new System.EventHandler(this.tlFree_Click);
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // tlForbit
            // 
            resources.ApplyResources(this.tlForbit, "tlForbit");
            this.tlForbit.Name = "tlForbit";
            this.tlForbit.Click += new System.EventHandler(this.tlForbit_Click);
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // tlStartBI
            // 
            resources.ApplyResources(this.tlStartBI, "tlStartBI");
            this.tlStartBI.Name = "tlStartBI";
            this.tlStartBI.Click += new System.EventHandler(this.tlStartBI_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // tlEndBI
            // 
            resources.ApplyResources(this.tlEndBI, "tlEndBI");
            this.tlEndBI.Name = "tlEndBI";
            this.tlEndBI.Click += new System.EventHandler(this.tlEndBI_Click);
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // tlClrAlarm
            // 
            resources.ApplyResources(this.tlClrAlarm, "tlClrAlarm");
            this.tlClrAlarm.Name = "tlClrAlarm";
            this.tlClrAlarm.Click += new System.EventHandler(this.tlClrAlarm_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // tlClrFail
            // 
            resources.ApplyResources(this.tlClrFail, "tlClrFail");
            this.tlClrFail.Name = "tlClrFail";
            this.tlClrFail.Click += new System.EventHandler(this.tlClrFail_Click);
            // 
            // toolStripSeparator5
            // 
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // tlResetBI
            // 
            resources.ApplyResources(this.tlResetBI, "tlResetBI");
            this.tlResetBI.Name = "tlResetBI";
            this.tlResetBI.Click += new System.EventHandler(this.tlResetBI_Click);
            // 
            // toolStripMenuItem3
            // 
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            // 
            // tlInPos
            // 
            resources.ApplyResources(this.tlInPos, "tlInPos");
            this.tlInPos.Name = "tlInPos";
            this.tlInPos.Click += new System.EventHandler(this.tlInPos_Click);
            // 
            // toolStripMenuItem4
            // 
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            // 
            // tlInPosFirstEnd
            // 
            resources.ApplyResources(this.tlInPosFirstEnd, "tlInPosFirstEnd");
            this.tlInPosFirstEnd.Name = "tlInPosFirstEnd";
            this.tlInPosFirstEnd.Click += new System.EventHandler(this.tlInPosFirstEnd_Click);
            // 
            // toolStripMenuItem5
            // 
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            // 
            // tlSetIsNull
            // 
            resources.ApplyResources(this.tlSetIsNull, "tlSetIsNull");
            this.tlSetIsNull.Name = "tlSetIsNull";
            this.tlSetIsNull.Click += new System.EventHandler(this.tlSetIsNull_Click);
            // 
            // toolStripSeparator6
            // 
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            // 
            // tlResetQCM
            // 
            resources.ApplyResources(this.tlResetQCM, "tlResetQCM");
            this.tlResetQCM.Name = "tlResetQCM";
            this.tlResetQCM.Click += new System.EventHandler(this.tlResetQCM_Click);
            // 
            // toolStripMenuItem7
            // 
            resources.ApplyResources(this.toolStripMenuItem7, "toolStripMenuItem7");
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            // 
            // tlFirstOut
            // 
            resources.ApplyResources(this.tlFirstOut, "tlFirstOut");
            this.tlFirstOut.Name = "tlFirstOut";
            this.tlFirstOut.Click += new System.EventHandler(this.tlFirstOut_Click);
            // 
            // udcFixture
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.menuOp;
            this.Name = "udcFixture";
            this.tlTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.udcFixture_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.udcFixture_Paint);
            this.Resize += new System.EventHandler(this.udcFixture_Resize);
            this.menuOp.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ImageList ImageList1;
        private System.Windows.Forms.ToolTip tlTip;
        private System.Windows.Forms.ContextMenuStrip menuOp;
        private System.Windows.Forms.ToolStripMenuItem tlDisplay;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem tlFree;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tlForbit;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem tlStartBI;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tlEndBI;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem tlClrAlarm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tlClrFail;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem tlResetBI;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem tlInPos;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem tlInPosFirstEnd;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem tlSetIsNull;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem tlResetQCM;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem tlFirstOut;
        private System.Windows.Forms.ToolStripMenuItem tlChart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    }
}
