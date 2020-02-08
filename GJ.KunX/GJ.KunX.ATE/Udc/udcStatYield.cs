using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.KunX.ATE.Udc
{
    public partial class udcStatYield : UserControl
    {
        #region 构造函数
        public udcStatYield(int uutMax)
        {
            this._uutMax = uutMax;
 
            InitializeComponent();
        }
        #endregion

        #region 字段
        /// <summary>
        /// 产品数量
        /// </summary>
        private int _uutMax = 0;
        #endregion

        #region 面板控件
        private TableLayoutPanel panelYield = null;
        private List<Label> labTitle = new List<Label>();
        private List<Label> labName = new List<Label>();
        private List<Label> labTTNum = new List<Label>();
        private List<Label> labFailNum = new List<Label>();
        private List<Label> labPassRate = new List<Label>();
        private List<Label> labTotal = new List<Label>();
        #endregion

        #region 面板回调函数 
        private void udcStatYield_Load(object sender, EventArgs e)
        {
            loadUUT(_uutMax);
        }
        #endregion

        #region 面板方法
        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="uutMax"></param>
        private void loadUUT(int uutMax)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(loadUUT), uutMax);
            else
            {
                disposePanel();

                this._uutMax = uutMax;

                load_Max_UUT(uutMax);
            }
        }
        /// <summary>
        /// 销毁控件
        /// </summary>
        private void disposePanel()
        {
            if (panelYield != null)
            {
                foreach (Control item in panelYield.Controls)
                {
                    panelYield.Controls.Remove(item);
                    item.Dispose();
                }
                labTitle.Clear();
                labName.Clear();
                labTTNum.Clear();
                labFailNum.Clear();
                labPassRate.Clear(); 
                panelYield.Dispose();
                panelYield = null;                 
            }
        }
        /// <summary>
        /// 加载通道界面
        /// </summary>
        private void load_Max_UUT(int uutMax)
        {
            try
            {
                Label lab1 = new Label();
                lab1.Dock = DockStyle.Fill;
                lab1.Margin = new Padding(3);
                lab1.Font = new Font("宋体", 10);
                lab1.ForeColor = Color.Black;
                lab1.TextAlign = ContentAlignment.MiddleCenter;
                lab1.Text = CLanguage.Lan("仪器槽位编号");
                labTitle.Add(lab1);

                Label lab2 = new Label();
                lab2.Dock = DockStyle.Fill;
                lab2.Margin = new Padding(3);
                lab2.Font = new Font("宋体", 10);
                lab2.ForeColor = Color.Black;
                lab2.TextAlign = ContentAlignment.MiddleCenter;
                lab2.Text = CLanguage.Lan("测试总数");
                labTitle.Add(lab2);

                Label lab3 = new Label();
                lab3.Dock = DockStyle.Fill;
                lab3.Margin = new Padding(3);
                lab3.Font = new Font("宋体", 10);
                lab3.ForeColor = Color.Black;
                lab3.TextAlign = ContentAlignment.MiddleCenter;
                lab3.Text = CLanguage.Lan("测试不良");
                labTitle.Add(lab3);

                Label lab4 = new Label();
                lab4.Dock = DockStyle.Fill;
                lab4.Margin = new Padding(3);
                lab4.Font = new Font("宋体", 10);
                lab4.ForeColor = Color.Black;
                lab4.TextAlign = ContentAlignment.MiddleCenter;
                lab4.Text = CLanguage.Lan("良率") + "(%)";
                labTitle.Add(lab4);

                for (int i = 0; i < uutMax; i++)
                {
                    Label lab5 = new Label();
                    lab5.Dock = DockStyle.Fill;
                    lab5.Margin = new Padding(3);
                    lab5.TextAlign = ContentAlignment.MiddleCenter;
                    lab5.ForeColor = Color.Black;
                    lab5.Font = new Font("宋体", 10);
                    lab5.Name = "labSlotName" + i.ToString();
                    lab5.Text = "【"+ CLanguage.Lan("槽位")  + (i + 1).ToString("D2") + "】:";
                    labName.Add(lab5);

                    Label labAteTT = new Label();
                    labAteTT.Dock = DockStyle.Fill;
                    labAteTT.Margin = new Padding(3);
                    labAteTT.TextAlign = ContentAlignment.MiddleCenter;
                    labAteTT.ForeColor = Color.Black;
                    labAteTT.Font = new Font("宋体", 10);
                    labAteTT.Name = "labAteTT" + i.ToString();
                    labAteTT.Text = "0";
                    labTTNum.Add(labAteTT);

                    Label labAteFAIL = new Label();
                    labAteFAIL.Dock = DockStyle.Fill;
                    labAteFAIL.Margin = new Padding(3);
                    labAteFAIL.TextAlign = ContentAlignment.MiddleCenter;
                    labAteFAIL.ForeColor = Color.Red;
                    labAteFAIL.Font = new Font("宋体", 10);
                    labAteFAIL.Name = "labAteFAIL" + i.ToString();
                    labAteFAIL.Text = "0";
                    labFailNum.Add(labAteFAIL);

                    Label labATEPass = new Label();
                    labATEPass.Dock = DockStyle.Fill;
                    labATEPass.Margin = new Padding(3);
                    labATEPass.TextAlign = ContentAlignment.MiddleCenter;
                    labATEPass.ForeColor = Color.Green;
                    labATEPass.Font = new Font("宋体", 10);
                    labATEPass.Name = "labATEPass" + i.ToString();
                    labATEPass.Text = "100%";
                    labPassRate.Add(labATEPass);

                }

                Label labYielName = new Label();
                labYielName.Dock = DockStyle.Fill;
                labYielName.Margin = new Padding(3);
                labYielName.Font = new Font("宋体", 10);
                labYielName.ForeColor = Color.Black;
                labYielName.TextAlign = ContentAlignment.MiddleCenter;
                labYielName.Text =CLanguage.Lan("总数统计");
                labTotal.Add(labYielName);  

                Label labSlotTTNum = new Label();
                labSlotTTNum.Dock = DockStyle.Fill;
                labSlotTTNum.Margin = new Padding(3);
                labSlotTTNum.Font = new Font("宋体", 10);
                labSlotTTNum.ForeColor = Color.Blue;
                labSlotTTNum.TextAlign = ContentAlignment.MiddleCenter;
                labSlotTTNum.Text = "0";
                labTotal.Add(labSlotTTNum);

                Label labSlotFailNum = new Label();
                labSlotFailNum.Dock = DockStyle.Fill;
                labSlotFailNum.Margin = new Padding(3);
                labSlotFailNum.Font = new Font("宋体", 10);
                labSlotFailNum.ForeColor = Color.Red;
                labSlotFailNum.TextAlign = ContentAlignment.MiddleCenter;
                labSlotFailNum.Text = "0";
                labTotal.Add(labSlotFailNum);

                Label labSlotPassRate = new Label();
                labSlotPassRate.Dock = DockStyle.Fill;
                labSlotPassRate.Margin = new Padding(3);
                labSlotPassRate.Font = new Font("宋体", 10);
                labSlotPassRate.ForeColor = Color.Green;
                labSlotPassRate.TextAlign = ContentAlignment.MiddleCenter;
                labSlotPassRate.Text = "0";
                labTotal.Add(labSlotPassRate);

                panelYield = new TableLayoutPanel();
                panelYield.Dock = DockStyle.Fill;
                panelYield.Margin = new Padding(1, 3, 1, 3);
                panelYield.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                panelYield.GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panelYield, true, null);
                panelYield.RowCount = uutMax + 2;
                panelYield.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                for (int i = 0; i < uutMax; i++)
                    panelYield.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelYield.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelYield.ColumnCount = 4;
                panelYield.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
                for (int i = 0; i < 3; i++)
                    panelYield.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                for (int i = 0; i < labTitle.Count; i++)
                    panelYield.Controls.Add(labTitle[i], i, 0);

                for (int i = 0; i < uutMax; i++)
                {
                    panelYield.Controls.Add(labName[i], 0, i + 1);
                    panelYield.Controls.Add(labTTNum[i], 1, i + 1);
                    panelYield.Controls.Add(labFailNum[i], 2, i + 1);
                    panelYield.Controls.Add(labPassRate[i], 3, i + 1);
                }

                for (int i = 0; i < labTotal.Count; i++)
                    panelYield.Controls.Add(labTotal[i], i, uutMax + 1);

                this.Controls.Add(panelYield);  

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 是否显示
        /// </summary>
        /// <param name="flag"></param>
        public void SetEnable(bool bShow)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetEnable), bShow);
            else
            {
                panelYield.Visible = bShow;
            }
        }
        /// <summary>
        /// 设置产能
        /// </summary>
        /// <param name="ttNum"></param>
        /// <param name="failNum"></param>
        /// <param name="slotTTNum"></param>
        /// <param name="slotFailNum"></param>
        public void SetYield(int ttNum, int failNum, List<int> slotTTNum, List<int> slotFailNum)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, int, List<int>, List<int>>(SetYield), ttNum, failNum, slotTTNum, slotFailNum);
            else
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    labTTNum[i].Text = slotTTNum[i].ToString();
                    labFailNum[i].Text = slotFailNum[i].ToString();
                    if (slotTTNum[i] == 0)
                        labPassRate[i].Text = "100%";
                    else
                        labPassRate[i].Text = ((double)((slotTTNum[i] - slotFailNum[i]) * 100) /
                                                  (double)slotTTNum[i]).ToString("0.0");
                }
                labTotal[1].Text = ttNum.ToString();
                labTotal[2].Text = failNum.ToString();
                if (ttNum == 0)
                    labTotal[3].Text = "100.0%";
                else
                    labTotal[3].Text = ((double)(ttNum - failNum) / (double)ttNum).ToString("P1");
                                                
            }
        }
        #endregion

    }
}
