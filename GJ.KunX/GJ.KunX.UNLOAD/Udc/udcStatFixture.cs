using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.KunX.UNLOAD.Udc
{
    public partial class udcStatFixture : UserControl
    {
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
        public udcStatFixture(int idNo, string name, int uutMax, int fixPos)
        {
            this.idNo = idNo;

            this.name = name;

            this._uutMax = uutMax;

            this._fixPos = fixPos;

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
            panel3.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel3, true, null);
            panel4.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel4, true, null);
            panel5.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel5, true, null);
        }
        #endregion

        #region 事件定义
        /// <summary>
        /// 按钮定义
        /// </summary>
        public class CBtnClickArgs : EventArgs
        {
            public readonly int idNo;
            public readonly string name;
            public readonly int lPara;
            public readonly int wPara;
            public CBtnClickArgs(int idNo, string name, int lPara=0,int wPara=0)
            {
                this.idNo = idNo;
                this.name = name;
                this.lPara = lPara;
                this.wPara = wPara;
            }
        }
        /// <summary>
        /// 按钮消息
        /// </summary>
        public COnEvent<CBtnClickArgs> OnBtnClickArgs = new COnEvent<CBtnClickArgs>();
        #endregion

        #region 字段
        /// <summary>
        /// 编号
        /// </summary>
        private int idNo = 0;
        /// <summary>
        /// 名称
        /// </summary>
        private string name = string.Empty;
        /// <summary>
        /// 产品数量
        /// </summary>
        private int _uutMax = 0;
        /// <summary>
        /// 治具显示方向-0:1-16 1:16-1
        /// </summary>
        private int _fixPos = 0;
        /// <summary>
        /// 需要不良确认
        /// </summary>
        private bool _disFail = false;
        #endregion

        #region 属性
        /// <summary>
        /// 不良确认
        /// </summary>
        public bool disFail
        {
            get { return _disFail; }
        }
        #endregion

        #region 面板控件
        private TableLayoutPanel panelUUT = null;
        private List<Label> labUUTNo = new List<Label>();
        private List<Label> labUUT = new List<Label>();
        private TableLayoutPanel panelSn = null;
        private List<Label> labNo = new List<Label>();
        private List<Label> labSn = new List<Label>();
        private List<Label> labResult = new List<Label>();
        #endregion

        #region 面板回调函数
        private void udcStatFixture_Load(object sender, EventArgs e)
        {
            LoadUUT(_uutMax);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("是否已确定不良,需继续下一步测试?"),
                                CLanguage.Lan("不良确定"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
               == DialogResult.Yes)
            {
                OnBtnClickArgs.OnEvented(new CBtnClickArgs(idNo, name));
            }
        }
        #endregion

        #region 面板方法
        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="uutMax"></param>
        public void LoadUUT(int uutMax)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(LoadUUT), uutMax);
            else
            {
                disposePanel();

                this._uutMax = uutMax;

                if (_uutMax != 8 && _uutMax != 16)
                    return;

                if (_uutMax == 8)
                {
                    load_Max_UUT8();
                }
                else
                {
                    load_Max_UUT16();
                }

                DisFailCfg();

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
                labUUTNo.Clear();
                labUUT.Clear();
                panelUUT.Dispose();
                panelUUT = null;
            }
            //条码界面
            if (panelSn != null)
            {
                foreach (Control item in panelSn.Controls)
                {
                    panelSn.Controls.Remove(item);
                    item.Dispose();
                }
                labNo.Clear();
                labSn.Clear();
                labResult.Clear();
                panelSn.Dispose();
                panelSn = null;
            }

        }
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
                    lab1.Name = "labUUTNo" + i.ToString();
                    lab1.Dock = DockStyle.Fill;
                    lab1.Margin = new Padding(0);
                    lab1.TextAlign = ContentAlignment.MiddleCenter;
                    lab1.Text = (i + 1).ToString("D2");
                    labUUTNo.Add(lab1);

                    Label lab2 = new Label();
                    lab2.Name = "labUUT" + i.ToString();
                    lab2.Dock = DockStyle.Fill;
                    lab2.Margin = new Padding(0);
                    lab2.TextAlign = ContentAlignment.MiddleCenter;
                    lab2.Text = "";
                    lab2.BackColor = Color.White;
                    labUUT.Add(lab2);

                    Label lab3 = new Label();
                    lab3.Name = "labNo" + i.ToString();
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(0);
                    lab3.TextAlign = ContentAlignment.MiddleCenter;
                    lab3.Text = (i + 1).ToString("D2");
                    labNo.Add(lab3);

                    Label lab4 = new Label();
                    lab4.Name = "labSn" + i.ToString();
                    lab4.Dock = DockStyle.Fill;
                    lab4.Margin = new Padding(0);
                    lab4.TextAlign = ContentAlignment.MiddleCenter;
                    lab4.Text = "";
                    labSn.Add(lab4);

                    Label lab5 = new Label();
                    lab5.Name = "labResult" + i.ToString();
                    lab5.Dock = DockStyle.Fill;
                    lab5.Margin = new Padding(0);
                    lab5.TextAlign = ContentAlignment.MiddleCenter;
                    lab5.Text = "";
                    labResult.Add(lab5);

                }
                //附加标志编号
                for (int i = 0; i < _uutMax; i++)
                {
                    Label lab1 = new Label();
                    lab1.Name = "labUUTNo" + (i + _uutMax).ToString();
                    lab1.Dock = DockStyle.Fill;
                    lab1.Margin = new Padding(0);
                    lab1.TextAlign = ContentAlignment.MiddleCenter;
                    lab1.Text = (i + 1).ToString("D2");
                    labUUTNo.Add(lab1);
                }
                //初始化panelUUT
                panelUUT = new TableLayoutPanel();
                panelUUT.Dock = DockStyle.Fill;
                panelUUT.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                panelUUT.Margin = new Padding(5, 25, 5, 25);
                panelUUT.RowCount = 3;
                panelUUT.ColumnCount = _uutMax;
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
                for (int i = 0; i < _uutMax; i++)
                    panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                for (int i = 0; i < _uutMax; i++)
                {
                    panelUUT.Controls.Add(labUUTNo[i], i, 0);
                    panelUUT.Controls.Add(labUUT[i], i, 1);
                    panelUUT.Controls.Add(labUUTNo[i + _uutMax], i, 2);
                }
                panelUUT.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT, true, null);
                panel4.Controls.Add(panelUUT);

                //初始化标题
                Label labT1 = new Label();
                labT1.Name = "labTitle1";
                labT1.Dock = DockStyle.Fill;
                labT1.Margin = new Padding(0);
                labT1.TextAlign = ContentAlignment.MiddleCenter;
                labT1.Text = CLanguage.Lan("编号");
                labT1.Tag = "编号";

                Label labT2 = new Label();
                labT2.Name = "labTitle2";
                labT2.Dock = DockStyle.Fill;
                labT2.Margin = new Padding(0);
                labT2.TextAlign = ContentAlignment.MiddleCenter;
                labT2.Text = CLanguage.Lan("产品条码");
                labT2.Tag = "产品条码";

                Label labT3 = new Label();
                labT3.Name = "labTitle3";
                labT3.Dock = DockStyle.Fill;
                labT3.Margin = new Padding(0);
                labT3.TextAlign = ContentAlignment.MiddleCenter;
                labT3.Text = CLanguage.Lan("结果");
                labT3.Tag = "结果";

                //初始化panelSn
                panelSn = new TableLayoutPanel();
                panelSn.Dock = DockStyle.Fill;
                panelSn.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelSn.Margin = new Padding(1);
                panelSn.RowCount = _uutMax + 1;
                panelSn.ColumnCount = 3;
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
                panelSn.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                for (int i = 0; i < _uutMax; i++)
                    panelSn.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelSn.Controls.Add(labT1, 0, 0);
                panelSn.Controls.Add(labT2, 1, 0);
                panelSn.Controls.Add(labT3, 2, 0);
                for (int i = 0; i < _uutMax; i++)
                {
                    panelSn.Controls.Add(labNo[i], 0, i + 1);
                    panelSn.Controls.Add(labSn[i], 1, i + 1);
                    panelSn.Controls.Add(labResult[i], 2, i + 1);
                }
                panelSn.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelSn, true, null);
                panel1.Controls.Add(panelSn, 1, 0);
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
                    lab1.Name = "labUUTNo" + i.ToString();
                    lab1.Dock = DockStyle.Fill;
                    lab1.Margin = new Padding(0);
                    lab1.TextAlign = ContentAlignment.MiddleCenter;
                    lab1.Text = (i + 1).ToString("D2");
                    labUUTNo.Add(lab1);

                    Label lab2 = new Label();
                    lab2.Name = "labUUT" + i.ToString();
                    lab2.Dock = DockStyle.Fill;
                    lab2.Margin = new Padding(0);
                    lab2.TextAlign = ContentAlignment.MiddleCenter;
                    lab2.Text = "";
                    lab2.BackColor = Color.White;
                    labUUT.Add(lab2);

                    Label lab3 = new Label();
                    lab3.Name = "labNo" + i.ToString();
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(0);
                    lab3.TextAlign = ContentAlignment.MiddleCenter;
                    lab3.Text = (i + 1).ToString("D2");
                    labNo.Add(lab3);

                    Label lab4 = new Label();
                    lab4.Name = "labSn" + i.ToString();
                    lab4.Dock = DockStyle.Fill;
                    lab4.Margin = new Padding(0);
                    lab4.TextAlign = ContentAlignment.MiddleCenter;
                    lab4.Text = "";
                    labSn.Add(lab4);

                    Label lab5 = new Label();
                    lab5.Name = "labResult" + i.ToString();
                    lab5.Dock = DockStyle.Fill;
                    lab5.Margin = new Padding(0);
                    lab5.TextAlign = ContentAlignment.MiddleCenter;
                    lab5.Text = "";
                    labResult.Add(lab5);

                }
                //初始化panelUUT
                int N = _uutMax / 2;
                panelUUT = new TableLayoutPanel();
                panelUUT.Dock = DockStyle.Fill;
                panelUUT.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                panelUUT.Margin = new Padding(5, 25, 5, 25);
                panelUUT.RowCount = N;
                panelUUT.ColumnCount = 4;
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
                for (int i = 0; i < N; i++)
                    panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                for (int i = 0; i < N; i++)
                {
                    if (_fixPos == 0)
                    {
                        panelUUT.Controls.Add(labUUTNo[i], 0, i);
                        panelUUT.Controls.Add(labUUT[i], 1, i);
                        panelUUT.Controls.Add(labUUT[i + N], 2, N - i - 1);
                        panelUUT.Controls.Add(labUUTNo[i + N], 3, N - i - 1);
                    }
                    else
                    {
                        panelUUT.Controls.Add(labUUTNo[i + N], 0, i);
                        panelUUT.Controls.Add(labUUT[i + N], 1, i);
                        panelUUT.Controls.Add(labUUT[N - i - 1], 2, i);
                        panelUUT.Controls.Add(labUUTNo[N - i - 1], 3, i);
                    }
                }
                panelUUT.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT, true, null);
                panel4.Controls.Add(panelUUT);

                //初始化标题
                Label labT1 = new Label();
                labT1.Name = "labTitle1";
                labT1.Dock = DockStyle.Fill;
                labT1.Margin = new Padding(0);
                labT1.TextAlign = ContentAlignment.MiddleCenter;
                labT1.Text = CLanguage.Lan("编号");
                labT1.Tag = "编号";


                Label labT2 = new Label();
                labT2.Name = "labTitle2";
                labT2.Dock = DockStyle.Fill;
                labT2.Margin = new Padding(0);
                labT2.TextAlign = ContentAlignment.MiddleCenter;
                labT2.Text = CLanguage.Lan("产品条码");
                labT2.Tag = "产品条码";

                Label labT3 = new Label();
                labT3.Name = "labTitle3";
                labT3.Dock = DockStyle.Fill;
                labT3.Margin = new Padding(0);
                labT3.TextAlign = ContentAlignment.MiddleCenter;
                labT3.Text = CLanguage.Lan("结果");
                labT3.Tag = "结果";

                //初始化panelSn
                panelSn = new TableLayoutPanel();
                panelSn.Dock = DockStyle.Fill;
                panelSn.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelSn.Margin = new Padding(1);
                panelSn.RowCount = _uutMax + 1;
                panelSn.ColumnCount = 3;
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
                panelSn.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                for (int i = 0; i < _uutMax; i++)
                    panelSn.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelSn.Controls.Add(labT1, 0, 0);
                panelSn.Controls.Add(labT2, 1, 0);
                panelSn.Controls.Add(labT3, 2, 0);
                for (int i = 0; i < _uutMax; i++)
                {
                    panelSn.Controls.Add(labNo[i], 0, i + 1);
                    panelSn.Controls.Add(labSn[i], 1, i + 1);
                    panelSn.Controls.Add(labResult[i], 2, i + 1);
                }
                panelSn.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelSn, true, null);
                panel1.Controls.Add(panelSn, 1, 0);
            }
            catch (Exception)
            {

                throw;
            }
        }
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
                    labUUT[i].BackColor = Color.White;
                    labSn[i].Text = "";
                    labResult[i].Text = "";
                }
                panelUUT.Visible = false;
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
                labStatus.Text = CLanguage.Lan("治具到位就绪,等待测试");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    if (serialNo[i] == "")
                        labUUT[i].BackColor = Color.White;
                    else
                        labUUT[i].BackColor = Color.DarkTurquoise;
                    labSn[i].Text = serialNo[i];
                    labSn[i].ForeColor = Color.Black;
                    labResult[i].Text = "";
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 测试中
        /// </summary>
        public void SetTesting()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetTesting));
            else
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    if (labSn[i].Text != "")
                        labUUT[i].BackColor = Color.LightSlateGray;
                    else
                        labUUT[i].BackColor = Color.White;
                    labResult[i].Text = "";
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 测试中 
        /// </summary>
        /// <param name="testTimes"></param>
        /// <param name="pcNo"> 
        /// pcNo=-1:16产品
        /// pcNo=0: 1-8产品;
        /// pcNo=1: 9-16产品;
        /// </param>
        public void SetTesting(int pcNo = -1)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(SetTesting), pcNo);
            else
            {
                if (pcNo == -1)
                {
                    for (int i = 0; i < _uutMax; i++)
                    {
                        if (labSn[i].Text != "")
                            labUUT[i].BackColor = Color.LightSlateGray;
                        else
                            labUUT[i].BackColor = Color.White;
                        labResult[i].Text = "";
                    }
                }
                else
                {
                    int SLOT_SIDE = _uutMax / 2;

                    for (int i = pcNo * SLOT_SIDE; i < (pcNo + 1) * SLOT_SIDE; i++)
                    {
                        if (labSn[i].Text != "")
                            labUUT[i].BackColor = Color.LightSlateGray;
                        else
                            labUUT[i].BackColor = Color.White;
                        labResult[i].Text = "";
                    }
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 测试结束
        /// </summary>
        public void SetEnd(List<int> result)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<int>>(SetEnd), result);
            else
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    if (labSn[i].Text != "")
                    {
                        if (result[i] == 0)
                        {
                            labUUT[i].BackColor = Color.LimeGreen;
                            labSn[i].ForeColor = Color.Blue;
                            labResult[i].Text = "PASS";
                            labResult[i].ForeColor = Color.Blue;
                        }
                        else
                        {
                            labUUT[i].BackColor = Color.Red;
                            labSn[i].ForeColor = Color.Red;
                            labResult[i].Text = "FAIL";
                            labResult[i].ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        labUUT[i].BackColor = Color.White;
                        labSn[i].ForeColor = Color.Black;
                        labResult[i].Text = "";
                    }
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 测试结束
        /// </summary>
        /// <param name="result"></param>
        /// <param name="testTimes"></param>
        /// <param name="pcNo">
        /// pcNo=-1:16产品
        /// pcNo=0: 1-8产品;
        /// pcNo=1: 9-16产品;
        /// </param>
        public void SetEnd(List<int> result, int pcNo = -1)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<int>, int>(SetEnd), result, pcNo);
            else
            {
                if (pcNo == -1)
                {
                    for (int i = 0; i < _uutMax; i++)
                    {
                        if (labSn[i].Text != "")
                        {
                            if (result[i] == 0)
                            {
                                labUUT[i].BackColor = Color.LimeGreen;
                                labSn[i].ForeColor = Color.Blue;
                                labResult[i].Text = "PASS";
                                labResult[i].ForeColor = Color.Blue;
                            }
                            else
                            {
                                labUUT[i].BackColor = Color.Red;
                                labSn[i].ForeColor = Color.Red;
                                labResult[i].Text = "FAIL";
                                labResult[i].ForeColor = Color.Red;
                            }
                        }
                        else
                        {
                            labUUT[i].BackColor = Color.White;
                            labSn[i].ForeColor = Color.Black;
                            labResult[i].Text = "";
                        }
                    }
                    panelUUT.Visible = true;
                }
                else  //双工位显示
                {
                    int SLOT_SIDE = _uutMax / 2;

                    for (int i = pcNo * SLOT_SIDE; i < (pcNo + 1) * SLOT_SIDE; i++)
                    {
                        if (labSn[i].Text != "")
                        {
                            if (result[i] == 0)
                            {
                                labUUT[i].BackColor = Color.LimeGreen;
                                labSn[i].ForeColor = Color.Blue;
                                labResult[i].Text = "PASS";
                                labResult[i].ForeColor = Color.Blue;
                            }
                            else
                            {
                                labUUT[i].BackColor = Color.Red;
                                labSn[i].ForeColor = Color.Red;
                                labResult[i].Text = "FAIL";
                                labResult[i].ForeColor = Color.Red;
                            }
                        }
                        else
                        {
                            labUUT[i].BackColor = Color.White;
                            labSn[i].ForeColor = Color.Black;
                            labResult[i].Text = "";
                        }
                    }
                    panelUUT.Visible = true;
                }
            }
        }
        /// <summary>
        /// 测试结束
        /// </summary>
        public void SetEnd(string idCard, List<string> serialNo, List<int> result)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, List<string>, List<int>>(SetEnd), idCard, serialNo, result);
            else
            {
                bool uutPass = true;
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = serialNo[i];
                    labSn[i].ForeColor = Color.Black;
                    if (serialNo[i] == "")
                        labUUT[i].BackColor = Color.White;
                    else
                        labUUT[i].BackColor = Color.DarkTurquoise;
                    if (labSn[i].Text != "")
                    {
                        if (result[i] == 0)
                        {
                            labUUT[i].BackColor = Color.LimeGreen;
                            labSn[i].ForeColor = Color.Blue;
                            labResult[i].Text = "PASS";
                            labResult[i].ForeColor = Color.Blue;
                        }
                        else
                        {
                            labUUT[i].BackColor = Color.Red;
                            labSn[i].ForeColor = Color.Red;
                            labResult[i].Text = "FAIL";
                            labResult[i].ForeColor = Color.Red;
                            uutPass = false;
                        }
                    }
                    else
                    {
                        labUUT[i].BackColor = Color.White;
                        labSn[i].ForeColor = Color.Black;
                        labResult[i].Text = "";
                    }
                }
                panelUUT.Visible = true;
                if (uutPass)
                {
                    labStatus.Text = CLanguage.Lan("测试结果:PASS,准备过站.");
                    labStatus.ForeColor = Color.Blue;
                }
                else
                {
                    labStatus.Text = CLanguage.Lan("测试结果:FAIL,请检查.");
                    labStatus.ForeColor = Color.Red;
                }
            }
        }
        /// <summary>
        /// 绑定治具条码
        /// </summary>
        /// <param name="idCard"></param>
        /// <param name="serialNo"></param>
        public void SetBand(string idCard, List<string> serialNo, List<int> result)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, List<string>, List<int>>(SetBand), idCard, serialNo, result);
            else
            {
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                bool uutBandOK = true;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = serialNo[i];
                    if (serialNo[i] != "")
                    {
                        if (result[i] == 0)
                        {
                            labUUT[i].BackColor = Color.DarkTurquoise;
                            labSn[i].ForeColor = Color.Blue;
                            labResult[i].Text = "READY";
                            labResult[i].ForeColor = Color.Blue;
                        }
                        else
                        {
                            labUUT[i].BackColor = Color.Red;
                            labSn[i].ForeColor = Color.Red;
                            labResult[i].Text = "FAIL";
                            labResult[i].ForeColor = Color.Red;
                            uutBandOK = false;
                        }
                    }
                    else
                    {
                        labUUT[i].BackColor = Color.White;
                        labSn[i].ForeColor = Color.Black;
                        labResult[i].Text = "";
                    }
                }
                if (uutBandOK)
                {
                    labStatus.Text = CLanguage.Lan("绑定条码OK,准备测试.");
                    labStatus.ForeColor = Color.Blue;
                }
                else
                {
                    labStatus.Text = CLanguage.Lan("绑定条码FAIL,请检查.");
                    labStatus.ForeColor = Color.Red;
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 空治具
        /// </summary>
        /// <param name="idCard"></param>
        public void SetNull(string idCard)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetNull), idCard);
            else
            {
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labResult[i].Text = "";
                    labUUT[i].BackColor = Color.White;
                }
                panelUUT.Visible = true;
                labStatus.Text = CLanguage.Lan("空治具准备过站.");
                labStatus.ForeColor = Color.Blue;
            }
        }
        /// <summary>
        /// 读取ID错误
        /// </summary>
        public void SetIdAlarm(string idCard = "")
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetIdAlarm), idCard);
            else
            {
                if (idCard == "")
                {
                    labIdCard.Text = CLanguage.Lan("读取不到治具ID");
                    labStatus.Text = CLanguage.Lan("获取治具ID错误");
                }
                else
                {
                    labIdCard.Text = idCard;
                    labStatus.Text = CLanguage.Lan("获取治具ID信息错误");
                }
                labIdCard.ForeColor = Color.Red;
                labStatus.ForeColor = Color.Red;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labSn[i].ForeColor = Color.Black;
                    labResult[i].Text = "";
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 报警
        /// </summary>
        /// <param name="alarmInfo"></param>
        public void SetAlarm(string alarmInfo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetAlarm), alarmInfo);
            else
            {
                labStatus.Text = alarmInfo;
                labStatus.ForeColor = Color.Red;
            }
        }
        /// <summary>
        /// 设置测试状态
        /// </summary>
        /// <param name="status"></param>
        /// <param name="bNG"></param>
        public void SetInfo(string status, bool bNG = false)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetInfo), status, bNG);
            else
            {
                labStatus.Text = status;
                if (!bNG)
                    labStatus.ForeColor = Color.Blue;
                else
                    labStatus.ForeColor = Color.Red;
            }
        }
        /// <summary>
        /// 不良确定
        /// </summary>
        /// <param name="IsFail"></param>
        public void SetFailCfg()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetFailCfg));
            else
            {
                _disFail = true;
                panel3.ColumnStyles[2].SizeType = SizeType.Absolute;
                panel3.ColumnStyles[2].Width = 60;
                btnOK.Visible = true;
            }
        }
        /// <summary>
        /// 解除报警确认
        /// </summary>
        public void DisFailCfg()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(DisFailCfg));
            else
            {
                _disFail = false;
                panel3.ColumnStyles[2].SizeType = SizeType.Absolute;
                panel3.ColumnStyles[2].Width = 1;
                btnOK.Visible = false;
            }
        }
        #endregion

    }
}
