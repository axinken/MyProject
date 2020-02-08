using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.YOHOO.BURNIN.Udc
{
    public partial class udcTemp : Form
    {        
        #region 构造函数
        public udcTemp()
        {
            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {
            int temp_Max = 15;

            int col_num = 5;

            int row_num = 3;

            panelTemp = new TableLayoutPanel();

            panelTemp.Dock = DockStyle.Fill;

            panelTemp.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            panelTemp.GetType().GetProperty("DoubleBuffered",
                                 System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)
                                 .SetValue(panelTemp, true, null);

            panelTemp.RowCount = row_num;

            panelTemp.ColumnCount = col_num * 2;

            for (int i = 0; i < row_num; i++)
                panelTemp.RowStyles.Add(new RowStyle(SizeType.Percent,100));

            for (int i = 0; i < col_num * 2; i++)
               panelTemp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));

            this.Controls.Add(panelTemp);   

            labNo.Clear();

            labT.Clear();

            for (int col = 0; col < col_num; col++)
            {
                for (int row = 0; row < row_num; row++)
                {
                    int idNo = col * row_num + row;

                    if (idNo < temp_Max)
                    {
                        Label lab1 = new Label();
                        lab1.Name = "labNo" + idNo.ToString();
                        lab1.Dock = DockStyle.Fill;
                        lab1.Margin = new Padding(0);
                        lab1.Font = new Font("宋体", 10);
                        lab1.TextAlign = ContentAlignment.MiddleCenter;
                        lab1.Text = CLanguage.Lan("温度") + (idNo + 1).ToString("D2") + ":";

                        Label lab2 = new Label();
                        lab2.Name = "labT" + idNo.ToString();
                        lab2.Dock = DockStyle.Fill;
                        lab2.Margin = new Padding(0);
                        lab2.TextAlign = ContentAlignment.MiddleCenter;
                        lab2.BorderStyle = BorderStyle.Fixed3D;
                        lab2.Font = new Font("宋体", 12, FontStyle.Bold);
                        lab2.BackColor = Color.Black;
                        lab2.ForeColor = Color.Cyan;

                        labNo.Add(lab1);
                        labT.Add(lab2);

                        panelTemp.Controls.Add(labNo[idNo], 0 + col * 2, row);

                        panelTemp.Controls.Add(labT[idNo], 1 + col * 2, row);
                    }

                }
            }

        }
        /// <summary>
        /// 设置双缓冲
        /// </summary>
        private void SetDoubleBuffered()
        {
            
        }
        #endregion

        #region 字段
        private static udcTemp dlg = null;
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
        public static udcTemp CreateInstance(double T_Low, double T_High, double[] rTPoint)
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new udcTemp();
                }
                for (int i = 0; i < rTPoint.Length; i++)
                {
                    dlg.labT[i].Text = rTPoint[i].ToString("0.0");
                    if (rTPoint[i] < T_Low)
                        dlg.labT[i].ForeColor = Color.Yellow;
                    else if (rTPoint[i] > T_High)
                        dlg.labT[i].ForeColor = Color.Red;
                    else
                        dlg.labT[i].ForeColor = Color.Lime;
                }
            }
            return dlg;
        }
        /// <summary>
        /// 即时显示温度
        /// </summary>
        /// <param name="T_Low"></param>
        /// <param name="T_High"></param>
        /// <param name="rTPoint"></param>
        public static void ShowTemp(double T_Low, double T_High, double[] rTPoint)
        {
            if (dlg == null || dlg.IsDisposed)
                return;
            if(dlg.InvokeRequired)
                dlg.Invoke(new Action<double,double,double[]>(ShowTemp),T_Low,T_High,rTPoint);
            else
            {
                for (int i = 0; i < rTPoint.Length; i++)
                {
                    dlg.labT[i].Text = rTPoint[i].ToString("0.0");
                    if (rTPoint[i] < T_Low)
                        dlg.labT[i].ForeColor = Color.Yellow;
                    else if (rTPoint[i] > T_High)
                        dlg.labT[i].ForeColor = Color.Red;
                    else
                        dlg.labT[i].ForeColor = Color.Lime;
                }
            }
        }
        #endregion

        #region 面板控件
        private TableLayoutPanel panelTemp = null; 
        private List<Label> labNo = new List<Label>();
        private List<Label> labT = new List<Label>();
        #endregion
    }
}
