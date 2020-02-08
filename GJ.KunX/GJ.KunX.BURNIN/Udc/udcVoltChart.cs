using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.UI;
using GJ.COM;
namespace GJ.KunX.BURNIN.Udc
{
    public partial class udcVoltChart : Form
    {
        #region 构造函数
        private udcVoltChart(CUnit Fixture)
        {
            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();

            FixNo = Fixture.FixNo;  

            this.runFixture = Fixture; 
        }
        #endregion

        #region 窗口唯一性

            #region 字段
            private static udcVoltChart dlg = null;
            private static object syncRoot = new object();
            #endregion

            #region 属性
        /// <summary>
        /// 窗口状态
        /// </summary>
        public static bool IsAvalible
        {
            get
            {
                lock (syncRoot)
                {
                    if (dlg != null && !dlg.IsDisposed)
                        return true;
                    else
                        return false;
                }
            }
        }
        #endregion

            #region 方法
            /// <summary>
            /// 创建唯一实例
            /// </summary>
            public static udcVoltChart CreateInstance(CUnit Fixture)
            {
                lock (syncRoot)
                {
                    if (dlg == null || dlg.IsDisposed)
                    {
                        dlg = new udcVoltChart(Fixture);
                    }
                }
                return dlg;
            }
            /// <summary>
            /// 刷新曲线
            /// </summary>
            public static void RefreshChart(double CurRunTime,double CurTemp,double CurACV)
            {
                if (dlg != null && !dlg.IsDisposed)
                {
                    dlg.labCurTemp.Text = CurTemp.ToString("0.0");

                    dlg.labCurACV.Text = CurACV.ToString("0.0");

                    dlg.uiTemp.AddXY(CurRunTime, CurTemp);

                    dlg.uiACV.AddXY(CurRunTime, CurACV); 
                }
            }
            #endregion

        #endregion

        #region 初始化
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {
            uiTemp = new udcChartLine();
            uiTemp.Dock = DockStyle.Fill; 

            uiACV = new udcChartLine();
            uiACV.Dock = DockStyle.Fill; 

            panel1.Controls.Add(uiTemp, 0, 3);

            panel1.Controls.Add(uiACV, 0, 5);
  
        }
        /// <summary>
        /// 设置双缓冲
        /// </summary>
        private void SetDoubleBuffered()
        {
            panel1.GetType().GetProperty("DoubleBuffered",
                                      System.Reflection.BindingFlags.Instance |
                                      System.Reflection.BindingFlags.NonPublic)
                                      .SetValue(panel1, true, null);
            panel2.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel2, true, null);
        }
        #endregion

        #region 字段
        public static int FixNo = 0;
        public CUnit runFixture = null;
        #endregion

        #region 面板控件
        private udcChartLine uiTemp = null;
        private udcChartLine uiACV = null;
        #endregion

        #region 面板回调函数
        private void udcVoltChart_Load(object sender, EventArgs e)
        {
            TimeSpan ts = new TimeSpan(0, 0, runFixture.CurRunTime);
            string runTime = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" +
                             ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");

            labLocalName.Text = runFixture.Name;

            labBITime.Text = ((double)runFixture.BurnTime/3600).ToString("0.0") + "H";

            labRunTime.Text = runTime;

            labStartTime.Text = runFixture.StartTime;

            labEndTime.Text = runFixture.EndTime;

            labCurTemp.Text = runFixture.CurTemp.ToString("0.0");

            labCurACV.Text = runFixture.CurACV.ToString("0.0");

            uiTemp.title = CLanguage.Lan("温度") + "(℃)";

            uiTemp.axisX_InterVal = runFixture.BurnTime / 60;

            uiTemp.axisX_MaxNum = runFixture.BurnTime;

            uiTemp.axisX_VisNum = runFixture.BurnTime;

            uiACV.axisY_InterVal = 10;

            uiTemp.axisY_VisNum = 60; 

            uiTemp.axisY_MaxNum = 60;

            uiTemp.Initial();

            uiTemp.BindXY(runFixture.Temp.X, runFixture.Temp.Y);

            uiACV.title = CLanguage.Lan("输入电压") + "(V)";

            uiACV.axisX_InterVal = runFixture.BurnTime / 60;

            uiACV.axisX_MaxNum = runFixture.BurnTime;

            uiACV.axisX_VisNum = runFixture.BurnTime;

            uiACV.axisY_InterVal = 50;

            uiACV.axisY_VisNum = 400;

            uiACV.axisY_MaxNum = 400;

            uiACV.Initial();

            uiACV.BindXY(runFixture.ACV.X, runFixture.ACV.Y);   
        }
        #endregion

    }
}
