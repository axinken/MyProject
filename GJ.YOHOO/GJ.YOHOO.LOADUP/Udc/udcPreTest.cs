using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.YOHOO.LOADUP.Udc
{
    public partial class udcPreTest : UserControl
    {
        #region 事件定义
        /// <summary>
        /// 按钮定义
        /// </summary>
        public class CBtnClickArgs : EventArgs
        {
            public readonly int idNo;
            public readonly string status;
            public CBtnClickArgs(int idNo, string status)
            {
                this.idNo = idNo;
                this.status = status;
            }
        }
        /// <summary>
        /// 按钮消息
        /// </summary>
        public COnEvent<CBtnClickArgs> OnBtnClickArgs = new COnEvent<CBtnClickArgs>();
        #endregion

        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);

            SetLabelControlLangange(this);
        }
        private void SetLabelControlLangange(Control ctrl)
        {
            foreach (Control item in ctrl.Controls)
            {
                if (item.GetType().ToString() == "System.Windows.Forms.Label")
                {
                    Label lab = (Label)item;

                    if (lab.Tag != null)
                    {
                        lab.Text = CLanguage.Lan(lab.Tag.ToString());
                    }
                }
                else
                {
                    SetLabelControlLangange(item);
                }
            }
        }
        #endregion

        #region 构造函数
        public udcPreTest()
        {
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
        }
        #endregion

        #region 面板控件
        private TableLayoutPanel panelUUT = null;
        private List<Label> labNo = new List<Label>();
        private List<Label> labV = new List<Label>();
        private List<Label> labI = new List<Label>();
        private List<Label> labResult = new List<Label>();
        #endregion

        #region 面板回调函数
        private void udcPreTest_Load(object sender, EventArgs e)
        {
            
        }
        private void labStatTimes_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要归零工位测试次数?"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
              DialogResult.Yes)
            {
                OnBtnClickArgs.OnEvented(new CBtnClickArgs(0, "工位测试次数"));
            }
        }
        private void labStatFailNum_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要归零工位连续不良次数?"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
             DialogResult.Yes)
            {
                OnBtnClickArgs.OnEvented(new CBtnClickArgs(1, "工位连续不良"));
            }
        }
        private void labTTNum_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要归零工位测试总数?"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
             DialogResult.Yes)
            {
                OnBtnClickArgs.OnEvented(new CBtnClickArgs(2, "工位测试总数"));
            }
        }
        #endregion

        #region 面板方法 
        /// <summary>
        /// 加载8通道界面
        /// </summary>
        private void load_Max_UUT8()
        {
            try
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    Label lab1 = new Label();
                    lab1.Name = "labNo" + i.ToString();
                    lab1.Dock = DockStyle.Fill;
                    lab1.Margin = new Padding(0);
                    lab1.TextAlign = ContentAlignment.MiddleCenter;
                    lab1.Text = (i + 1).ToString("D2");
                    labNo.Add(lab1);

                    Label lab2 = new Label();
                    lab2.Name = "labV" + i.ToString();
                    lab2.Dock = DockStyle.Fill;
                    lab2.Margin = new Padding(0);
                    lab2.TextAlign = ContentAlignment.MiddleCenter;
                    lab2.Text = "";
                    labV.Add(lab2);

                    Label lab3 = new Label();
                    lab3.Name = "labI" + i.ToString();
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(0);
                    lab3.TextAlign = ContentAlignment.MiddleCenter;
                    lab3.Text = "";
                    labI.Add(lab3);

                    Label lab4 = new Label();
                    lab4.Name = "labResult" + i.ToString();
                    lab4.Dock = DockStyle.Fill;
                    lab4.Margin = new Padding(0);
                    lab4.TextAlign = ContentAlignment.MiddleCenter;
                    lab4.Text = "";
                    labResult.Add(lab4);
                }

                //初始化panelUUT                
                int N = _uutMax;                
                panelUUT = new TableLayoutPanel();
                panelUUT.Dock = DockStyle.Fill;
                panelUUT.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelUUT.Margin = new Padding(2);
                panelUUT.RowCount = N+1;
                panelUUT.ColumnCount = 4;
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                for (int i = 0; i < N; i++)
                    panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
               
                Label labT1 = new Label();
                labT1.Name = "labT1";
                labT1.Dock = DockStyle.Fill;
                labT1.Margin = new Padding(0);
                labT1.TextAlign = ContentAlignment.MiddleCenter;
                labT1.Text = CLanguage.Lan("编号");
                labT1.Tag = "编号";

                Label labT2 = new Label();
                labT2.Name = "labT2";
                labT2.Dock = DockStyle.Fill;
                labT2.Margin = new Padding(0);
                labT2.TextAlign = ContentAlignment.MiddleCenter;
                labT2.Text = CLanguage.Lan("电压(V)");
                labT2.Tag = "电压(V)";

                Label labT3 = new Label();
                labT3.Name = "labT3";
                labT3.Dock = DockStyle.Fill;
                labT3.Margin = new Padding(0);
                labT3.TextAlign = ContentAlignment.MiddleCenter;
                labT3.Text = CLanguage.Lan("电流(A)");
                labT3.Tag = "电流(A)";

                Label labT4 = new Label();
                labT4.Name = "labT4";
                labT4.Dock = DockStyle.Fill;
                labT4.Margin = new Padding(0);
                labT4.TextAlign = ContentAlignment.MiddleCenter;
                labT4.Text = CLanguage.Lan("结果");
                labT4.Tag = "结果";

                panelUUT.Controls.Add(labT1, 0, 0);
                panelUUT.Controls.Add(labT2, 1, 0);
                panelUUT.Controls.Add(labT3, 2, 0);
                panelUUT.Controls.Add(labT4, 3, 0);

                for (int i = 0; i < N; i++)
                {
                    panelUUT.Controls.Add(labNo[i], 0, i + 1);
                    panelUUT.Controls.Add(labV[i], 1, i + 1);
                    panelUUT.Controls.Add(labI[i], 2, i + 1);
                    panelUUT.Controls.Add(labResult[i], 3, i + 1);
                }
                panelUUT.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT, true, null);
                panel1.Controls.Add(panelUUT,0,0);
               
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 加载16通道界面
        /// </summary>
        private void load_Max_UUT16()
        {
            try
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    Label lab1 = new Label();
                    lab1.Name = "labNo" + i.ToString();
                    lab1.Dock = DockStyle.Fill;
                    lab1.Margin = new Padding(0);
                    lab1.TextAlign = ContentAlignment.MiddleCenter;
                    lab1.Text = (i + 1).ToString("D2");
                    labNo.Add(lab1);

                    Label lab2 = new Label();
                    lab2.Name = "labV" + i.ToString();
                    lab2.Dock = DockStyle.Fill;
                    lab2.Margin = new Padding(0);
                    lab2.TextAlign = ContentAlignment.MiddleCenter;
                    lab2.Text = "";
                    labV.Add(lab2);

                    Label lab3 = new Label();
                    lab3.Name = "labI" + i.ToString();
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(0);
                    lab3.TextAlign = ContentAlignment.MiddleCenter;
                    lab3.Text = "";
                    labI.Add(lab3);

                    Label lab4 = new Label();
                    lab4.Name = "labResult" + i.ToString();
                    lab4.Dock = DockStyle.Fill;
                    lab4.Margin = new Padding(0);
                    lab4.TextAlign = ContentAlignment.MiddleCenter;
                    lab4.Text = "";
                    labResult.Add(lab4);
                }

                //初始化panelUUT                
                int N = _uutMax/2;
                panelUUT = new TableLayoutPanel();
                panelUUT.Dock = DockStyle.Fill;
                panelUUT.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelUUT.Margin = new Padding(3);
                panelUUT.RowCount = N + 1;
                panelUUT.ColumnCount = 8;
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                for (int i = 0; i < N; i++)
                    panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                for (int i = 0; i < 2; i++)
                {
                    Label labT1 = new Label();
                    labT1.Name = "labT1";
                    labT1.Dock = DockStyle.Fill;
                    labT1.Margin = new Padding(0);
                    labT1.TextAlign = ContentAlignment.MiddleCenter;
                    labT1.Text = CLanguage.Lan("编号");
                    labT1.Tag = "编号";

                    Label labT2 = new Label();
                    labT2.Name = "labT2";
                    labT2.Dock = DockStyle.Fill;
                    labT2.Margin = new Padding(0);
                    labT2.TextAlign = ContentAlignment.MiddleCenter;
                    labT2.Text = CLanguage.Lan("电压(V)");
                    labT2.Tag = "电压(V)";

                    Label labT3 = new Label();
                    labT3.Name = "labT3";
                    labT3.Dock = DockStyle.Fill;
                    labT3.Margin = new Padding(0);
                    labT3.TextAlign = ContentAlignment.MiddleCenter;
                    labT3.Text = CLanguage.Lan("电流(A)");
                    labT3.Tag = "电流(A)";

                    Label labT4 = new Label();
                    labT4.Name = "labT4";
                    labT4.Dock = DockStyle.Fill;
                    labT4.Margin = new Padding(0);
                    labT4.TextAlign = ContentAlignment.MiddleCenter;
                    labT4.Text = CLanguage.Lan("结果");
                    labT4.Tag = "结果";

                    panelUUT.Controls.Add(labT1, 0 + 4 * i, 0);
                    panelUUT.Controls.Add(labT2, 1 + 4 * i, 0);
                    panelUUT.Controls.Add(labT3, 2 + 4 * i, 0);
                    panelUUT.Controls.Add(labT4, 3 + 4 * i, 0);
                }               

                for (int i = 0; i < N; i++)
                {
                    panelUUT.Controls.Add(labNo[i], 0, i + 1);
                    panelUUT.Controls.Add(labV[i], 1, i + 1);
                    panelUUT.Controls.Add(labI[i], 2, i + 1);
                    panelUUT.Controls.Add(labResult[i], 3, i + 1);

                    panelUUT.Controls.Add(labNo[i + N], 4, i + 1);
                    panelUUT.Controls.Add(labV[i + N], 5, i + 1);
                    panelUUT.Controls.Add(labI[i + N], 6, i + 1);
                    panelUUT.Controls.Add(labResult[i + N], 7, i + 1);
                }
                panelUUT.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT, true, null);
                panel1.Controls.Add(panelUUT, 0, 0);

            }
            catch (Exception)
            {

                throw;
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
                labV.Clear();
                labI.Clear();
                labResult.Clear();
                panelUUT.Dispose();
                panelUUT = null;
            }
        }
        #endregion

        #region 字段
        private int _uutMax = 0;
        private double _Vmin = 0;
        private double _Vmax = 0;
        private double _Imin = 0;
        private double _Imax = 0;
        #endregion

        #region 属性
        public double Vmin
        {
            set { _Vmin = value; }
        }        
        public double Vmax
        {
            set { _Vmax = value; }
        }
        public double Imin
        {
            set { _Imin = value; }
        }
        public double Imax
        {
            set { _Imax = value; }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="uutMax"></param>
        public void LoadUUT(int uutMax)
        {
            try
            {
                if (this._uutMax == uutMax)
                    return;

                this._uutMax = uutMax;

                disposePanel();

                if (_uutMax == 8)
                {
                    load_Max_UUT8();
                }
                else if (_uutMax == 16)
                {
                    load_Max_UUT16();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// 工位使用次数
        /// </summary>
        public void SetUseTimes(int useNum)
        {
            if(this.InvokeRequired)
                this.Invoke(new Action<int>(SetUseTimes),useNum);
            else
            {
                labStatTimes.Text = useNum.ToString();
            }            
        }
        /// <summary>
        /// 工位连续不良
        /// </summary>
        public void SetFailTimes(int failNum)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(SetFailTimes), failNum);
            else
            {
                labStatFailNum.Text = failNum.ToString();
            }
        }
        /// <summary>
        /// 测试产品数量
        /// </summary>
        /// <param name="bFail"></param>
        public void SetTestNum(int ttNum,int failNum)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, int>(SetTestNum), ttNum, failNum);
            else
            {
                labTTNum.Text = ttNum.ToString();
                labFailNum.Text = failNum.ToString();
                if (ttNum == 0)
                    labPassRate.Text = "100.0%";
                else
                    labPassRate.Text = ((double)(ttNum - failNum) * 100 / (double)ttNum).ToString("0.0") + "%";
            }            
        }
        /// <summary>
        /// 清除状态
        /// </summary>
        public void SetFree()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetFree));
            else
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    labV[i].Text = "";
                    labI[i].Text = ""; 
                    labResult[i].Text = ""; 
                }
                labTestTimes.Text = "0";
            }            
        }
        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="V"></param>
        /// <param name="I"></param>
        /// <param name="result"></param>
        /// <param name="testTimes"></param>
        public void ShowData(List<string> serialNo, List<double> V,List<double> I,List<string> DD,long testTimes, bool testEnd)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<string>, List<double>, List<double>, List<string>, long, bool>(ShowData), serialNo, V, I, DD, testTimes, testEnd);
            else
            {
                bool uutPass = true;
                int[] result = new int[_uutMax];
                for (int i = 0; i < _uutMax; i++)
                {
                    result[i] = 0;
                    if (serialNo[i] == "")
                    {
                        labV[i].Text = "---";
                        labV[i].ForeColor = Color.Black;
                        labI[i].Text = "---";
                        labI[i].ForeColor = Color.Black;
                        labResult[i].Text = "---";
                        labResult[i].ForeColor = Color.Black;
                    }
                    else
                    {
                        labV[i].Text = V[i].ToString("0.000");
                        labV[i].ForeColor = Color.Blue;
                        if (V[i] < _Vmin || V[i] > _Vmax)
                        {
                            labV[i].ForeColor = Color.Red;
                            result[i] = 1;
                        }
                        labI[i].Text = I[i].ToString("0.000");
                        labI[i].ForeColor = Color.Blue;
                        if (I[i] < _Imin || I[i] > _Imax)
                        {
                            labI[i].ForeColor = Color.Red;
                            result[i] = 1;
                        }
                        if (result[i] != 0)
                            uutPass = false;

                        if (DD[i] != "正常")
                        {
                            result[i] = 1;
                            uutPass = false;
                        }
                    }
                }
                if (uutPass)
                {
                    for (int i = 0; i < _uutMax; i++)
                    {
                        if (serialNo[i] != "")
                        {
                            labResult[i].Text = "PASS";
                            labResult[i].ForeColor = Color.Blue;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _uutMax; i++)
                    {
                        if (serialNo[i] != "")
                        {
                            if (result[i] == 0)
                            {
                                labResult[i].Text = "PASS";
                                labResult[i].ForeColor = Color.Blue;
                            }
                            else
                            {
                                if (DD[i] == "正常")
                                {
                                    labResult[i].Text = "FAIL";
                                    labResult[i].ForeColor = Color.Red;
                                }
                                else
                                {
                                    labResult[i].Text = DD[i];
                                    labResult[i].ForeColor = Color.Red;
                                }
                            }
                        }
                    }
                }
                labTestTimes.Text = (((double)testTimes) / 1000).ToString("0.0");
                this.Refresh();
            }
        }
        /// <summary>
        /// 显示测试时间
        /// </summary>
        /// <param name="testTimes"></param>
        public void ShowTestTimes(int testTimes)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(ShowTestTimes), testTimes);
            else
            {
                labTestTimes.Text = (((double)testTimes) / 1000).ToString("0.0");
            }
        }
        /// <summary>
        /// 输入AC电压
        /// </summary>
        /// <param name="acv"></param>
        /// <param name="flag"></param>
        public void SetACV(string acv, int flag = 0)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, int>(SetACV), acv, flag);
            else
            {
                labACV.Text = acv + "V";
                if (flag==1)
                    labACV.ForeColor = Color.Blue;
                else if (flag==2)
                    labACV.ForeColor = Color.Red; 
                else
                    labACV.ForeColor = Color.Black; 
            }
        }
        /// <summary>
        /// 设置监控时间
        /// </summary>
        /// <param name="mTime"></param>
        public void SetMonTime(long mTime)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<long>(SetMonTime), mTime);
            else
            {
                labMonTimes.Text = mTime.ToString();
            }
        }
        /// <summary>
        /// 设置UI信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="lPara"></param>
        public void SetUI(string name, string info, Color lPara, int wPara = 0)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, string, Color, int>(SetUI), name, info, lPara, wPara);
            else
            {
                Control ctrl = null;

                FindCtrlFromName(this, name, ref ctrl);

                if (ctrl == null)
                    return;

                Label lab = (Label)ctrl;

                lab.Text = info;

                lab.ForeColor = lPara;
            }
        }
        private void FindCtrlFromName(Control fatherControl, string controlName, ref Control ctrl)
        {
            try
            {
                if (ctrl != null)
                    return;

                foreach (Control c in fatherControl.Controls)
                {
                    if (c.Name == controlName)
                    {
                        ctrl = c;

                        return;
                    }

                    FindCtrlFromName(c, controlName, ref ctrl);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

    }
}
