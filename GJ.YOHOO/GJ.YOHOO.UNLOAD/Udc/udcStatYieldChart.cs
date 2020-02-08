using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.UI;
using GJ.COM;

namespace GJ.YOHOO.UNLOAD.Udc
{
    public partial class udcStatYieldChart : UserControl
    {
        #region 构造函数
        public udcStatYieldChart(int idNo,string name)
        {
            this._idNo = idNo;

            this._name = name;

            InitializeComponent();

            InitialControl();

            SetDoubleBuffered();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {
            uiChart = new udcChartPie();

            uiChart.Dock = DockStyle.Fill;

            panel1.Controls.Add(uiChart, 0, 1); 
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
        }
        #endregion

        #region 面板控件
        /// <summary>
        /// 饼图
        /// </summary>
        private udcChartPie uiChart = null;
        #endregion

        #region 面板回调函数
        private void udcStatYieldChart_Load(object sender, EventArgs e)
        {
            labTitle.Text = "【" + _name + "】";
        }
        #endregion

        #region 字段
        /// <summary>
        /// 编号
        /// </summary>
        private int _idNo = 0;
        /// <summary>
        /// 名称
        /// </summary>
        private string _name = string.Empty;
        #endregion

        #region 方法
        /// <summary>
        /// 设置产能
        /// </summary>
        /// <param name="yield"></param>
        public void SetYield(CYield yield)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CYield>(SetYield), yield);
            else
            {
                uiChart.SetValue(yield.TTNum, yield.FailNum);
            }
        }
        #endregion

       

    }
}
