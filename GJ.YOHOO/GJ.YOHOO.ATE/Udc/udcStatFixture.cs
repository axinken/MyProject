using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.YOHOO.ATE.Udc
{
    public partial class udcStatFixture : UserControl
    {
        #region 构造函数
        public udcStatFixture(int uutMax)
        {
            this._uutMax = uutMax;

            InitializeComponent();

            SetDoubleBuffered();
        }
        #endregion

        #region 初始化
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
            panel3.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel3, true, null);
        }
        #endregion

        #region 字段
        /// <summary>
        /// 产品数量
        /// </summary>
        private int _uutMax = 0;
        #endregion

        #region 面板控件
        private TableLayoutPanel panelUUT = null;
        private List<Label> labNo = new List<Label>();
        private List<Label> labSn = new List<Label>();
        private List<Label> labResult = new List<Label>();
        #endregion

        #region 面板回调函数
        private void udcStatFixture_Load(object sender, EventArgs e)
        {
            load_Max_UUT(_uutMax);
        }
        private void btnDebug1_Click(object sender, EventArgs e)
        {
            if (btnDebug1.Text == CLanguage.Lan("调试") + "(&D)")
                OnBtnClick.OnEvented(new COnBtnClickArgs(0, 1));
            else
                OnBtnClick.OnEvented(new COnBtnClickArgs(0, 0));
        }
        private void btnDebug2_Click(object sender, EventArgs e)
        {
           OnBtnClick.OnEvented(new COnBtnClickArgs(1, 0));
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
                if (this._uutMax == uutMax)
                    return;

                disposePanel();

                this._uutMax = uutMax;

                load_Max_UUT(uutMax);

                SetFree();
            }
        }
        /// <summary>
        /// 销毁控件
        /// </summary>
        private void disposePanel()
        {
            //治具界面
            if (panelUUT != null)
            {
                foreach (Control item in panelUUT.Controls)
                {
                    panelUUT.Controls.Remove(item);
                    item.Dispose();
                }
                labNo.Clear();
                labSn.Clear();
                labResult.Clear();
                panelUUT.Dispose();
                panelUUT = null;
            }
        }
        /// <summary>
        /// 加载通道界面
        /// </summary>
        private void load_Max_UUT(int uutMax)
        {
            try
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    Label lab1 = new Label();
                    lab1.Dock = DockStyle.Fill;
                    lab1.Margin = new Padding(1);
                    lab1.TextAlign = ContentAlignment.MiddleLeft;
                    lab1.Font = new Font("宋体", 10);
                    lab1.Name = "labSlot" + i.ToString();
                    lab1.Text = "【"+ CLanguage.Lan("产品条码") + (i + 1).ToString("D2") + "】:";
                    labNo.Add(lab1);

                    Label lab2 = new Label();
                    lab2.Dock = DockStyle.Fill;
                    lab2.Margin = new Padding(1);
                    lab2.TextAlign = ContentAlignment.MiddleCenter;
                    lab2.Font = new Font("宋体", 10);
                    lab2.Name = "labSn" + i.ToString();
                    lab2.Text = "";
                    labSn.Add(lab2);

                    Label lab3 = new Label();
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(1);
                    lab3.TextAlign = ContentAlignment.MiddleCenter;
                    lab3.Font = new Font("宋体", 10);
                    lab3.Name = "labResult" + i.ToString();
                    lab3.Text = "";
                    labResult.Add(lab3);
                }
               
                //初始化panelUUT
                panelUUT = new TableLayoutPanel();
                panelUUT.Dock = DockStyle.Fill;
                panelUUT.Margin = new Padding(1, 3, 1, 3);
                panelUUT.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                panelUUT.ColumnCount = 3;
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
                panelUUT.RowCount = uutMax;
                for (int i = 0; i < uutMax; i++)
                  panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent,100));    
                panelUUT.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT, true, null);
                for (int i = 0; i < uutMax; i++)
                {
                    panelUUT.Controls.Add(labNo[i],0, i);
                    panelUUT.Controls.Add(labSn[i], 1, i);
                    panelUUT.Controls.Add(labResult[i], 2, i);
                }
                panel2.Controls.Add(panelUUT, 0, 1);    
               
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 面板消息
        public class COnBtnClickArgs:EventArgs 
        {
            public readonly int idNo;
            public readonly int status;
            public COnBtnClickArgs(int idNo,int status)
            {
                this.idNo = idNo;
                this.status = status;  
            }
        }
        public COnEvent<COnBtnClickArgs> OnBtnClick = new COnEvent<COnBtnClickArgs>(); 
        #endregion

        #region 方法
        /// <summary>
        /// 空闲
        /// </summary>
        public void SetFree()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetFree));
            else
            {
                labIdCard.Text = "";
                labStatus.Text = CLanguage.Lan("等待治具到位.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labResult[i].Text = "";
                }
            }
        }
        /// <summary>
        /// 就绪
        /// </summary>
        public void SetReady(string idCard, List<string> serialNo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, List<string>>(SetReady), idCard, serialNo);
            else
            {
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                labStatus.Text = CLanguage.Lan("治具到位就绪,等待测试.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = serialNo[i];
                    labSn[i].ForeColor = Color.Black;
                    labResult[i].Text = "";
                }
            }
        }
        /// <summary>
        /// 就绪
        /// </summary>
        public void SetReady(int idNo,string idCard, List<string> serialNo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int,string, List<string>>(SetReady),idNo, idCard, serialNo);
            else
            {
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                labStatus.Text = CLanguage.Lan("治具到位就绪,等待测试.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < serialNo.Count; i++)
                {
                    int uutNo = idNo * serialNo.Count + i;
                    labSn[uutNo].Text = serialNo[i];
                    labSn[uutNo].ForeColor = Color.Black;
                    labResult[uutNo].Text = "";
                }
            }
        }
        /// <summary>
        /// 测试中
        /// </summary>
        /// <param name="testTimes"></param>
        public void SetTesting(long testTimes)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<long>(SetTesting), testTimes);
            else
            {
                labStatus.Text = CLanguage.Lan("测试中:") + ((double)testTimes/1000).ToString("0.0") + "s"; ;
                labStatus.ForeColor = Color.Blue;
            }
        }
        /// <summary>
        /// 测试结束
        /// </summary>
        public void SetEnd(List<int> result, long testTimes)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<int>, long>(SetEnd), result, testTimes);
            else
            {
                bool uutPass = true;
                for (int i = 0; i < _uutMax; i++)
                {
                    if (labSn[i].Text != "")
                    {
                        if (result[i] == 0)
                        {
                            labSn[i].ForeColor = Color.Blue;
                            labResult[i].Text = "PASS";
                            labResult[i].ForeColor = Color.Blue;
                        }
                        else
                        {
                            labSn[i].ForeColor = Color.Red;
                            labResult[i].Text = "FAIL";
                            labResult[i].ForeColor = Color.Red;
                            uutPass = false;
                        }
                    }
                    else
                    {
                        labSn[i].ForeColor = Color.Black;
                        labResult[i].Text = "";
                    }
                }
                if (uutPass)
                {
                    labStatus.Text = CLanguage.Lan("测试结果:PASS,准备过站:") + testTimes.ToString() + "s"; ;
                    labStatus.ForeColor = Color.Blue;
                }
                else
                {
                    labStatus.Text = CLanguage.Lan("测试结果:FAIL,准备过站:") + testTimes.ToString() + "s";
                    labStatus.ForeColor = Color.Red;
                }
            }
        }
        /// <summary>
        /// 测试结束
        /// </summary>
        public void SetEnd(int idNo,List<int> result)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, List<int>>(SetEnd), idNo, result);
            else
            {
                for (int i = 0; i < result.Count; i++)
                {
                    int uutNo = idNo * result.Count + i;

                    if (labSn[uutNo].Text != "")
                    {
                        if (result[i] == 0)
                        {
                            labSn[uutNo].ForeColor = Color.Blue;
                            labResult[uutNo].Text = "PASS";
                            labResult[uutNo].ForeColor = Color.Blue;
                        }
                        else
                        {
                            labSn[uutNo].ForeColor = Color.Red;
                            labResult[uutNo].Text = "FAIL";
                            labResult[uutNo].ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        labSn[uutNo].ForeColor = Color.Black;
                        labResult[uutNo].Text = "";
                    }
                }
            }
        }
        /// <summary>
        /// 设置调式按钮状态 
        /// </summary>
        /// <param name="status"></param>
        public void SetDebugMode(bool status)
        { 
          if(this.InvokeRequired)
              this.Invoke(new Action<bool>(SetDebugMode), status);
          else
          {
              if (status)
                  btnDebug1.Text = CLanguage.Lan("停止") + "(&D)";
              else
                  btnDebug1.Text = CLanguage.Lan("调试") + "(&D)";
          }
        }
        /// <summary>
        /// 设置按钮使能
        /// </summary>
        /// <param name="enable"></param>
        public void SetDebugBtn(bool enable)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetDebugBtn), enable);
            else
            {
                btnDebug1.Text = CLanguage.Lan("调试") + "(&D)";
                if (enable)
                {
                    btnDebug1.Enabled = true;
                }
                else
                {
                    btnDebug1.Enabled = false;
                }
            }
        }
        #endregion
    }
}
