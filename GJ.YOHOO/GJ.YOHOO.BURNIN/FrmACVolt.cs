using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.UI;
namespace GJ.YOHOO.BURNIN
{
    public partial class FrmACVolt : Form
    {
        #region 构造函数
        public FrmACVolt()
        {
            InitializeComponent();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {
            
        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
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
        private static FrmACVolt dlg = null;
        private static object syncRoot = new object();
        private static CCtrlAC _CtrlAC = null;
        private udcChartOnOff uiOnOff = null;
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
        public static FrmACVolt CreateInstance(CCtrlAC ctrlAC)
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)                
                {
                    _CtrlAC = ctrlAC;

                    dlg = new FrmACVolt();

                }
            }
            return dlg;
        }
        #endregion

        #region 面板回调函数
        private void FrmACVolt_Load(object sender, EventArgs e)
        {
            labStartTime.Text = _CtrlAC.StartTime;

            TimeSpan ts = new TimeSpan(0, 0, _CtrlAC.RunTime);
            
            labRunTime.Text = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");

            labCurVolt.Text = _CtrlAC.CurVolt.ToString();

            labWaveVolt.Text = _CtrlAC.CurVolt.ToString(); 

            uiOnOff = new udcChartOnOff();

            uiOnOff.Dock = DockStyle.Fill;

            panel1.Controls.Add(uiOnOff, 0, 1);

            int maxAC = 0;

            uiOnOff.biTime = _CtrlAC.TotalTime;

            List<udcChartOnOff.COnOff> itemList = new List<udcChartOnOff.COnOff>();

            for (int i = 0; i < _CtrlAC.Time.OnOff.Count; i++)
            {
                udcChartOnOff.COnOff onoff = new udcChartOnOff.COnOff();

                onoff.curVolt = _CtrlAC.Time.OnOff[i].ACV;

                onoff.onoffTimes = _CtrlAC.Time.OnOff[i].OnOffTime;

                onoff.onTimes = _CtrlAC.Time.OnOff[i].OnTime;

                onoff.offTimes = _CtrlAC.Time.OnOff[i].OffTime;

                itemList.Add(onoff);

                if (_CtrlAC.Time.OnOff[i].ACV > maxAC)
                    maxAC = _CtrlAC.Time.OnOff[i].ACV;
            }

            uiOnOff.maxVolt = maxAC;

            uiOnOff.onoff = itemList;

            uiOnOff.Refresh();

            uiOnOff.startRun(_CtrlAC.RunTime);

            timer1.Enabled = true;

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            uiOnOff.runTimes = _CtrlAC.RunTime; 

            TimeSpan ts = new TimeSpan(0, 0, uiOnOff.runTimes);

            labRunTime.Text = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");

            labCurVolt.Text = _CtrlAC.CurVolt.ToString();

            labWaveVolt.Text = uiOnOff.runVolt.ToString(); 
        }
        #endregion
    }
}
