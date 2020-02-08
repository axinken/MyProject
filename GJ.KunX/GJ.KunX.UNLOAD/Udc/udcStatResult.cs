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
    public partial class udcStatResult : UserControl
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
        /// <summary>
        /// 流程4,3,2,1
        /// </summary>
        /// <param name="uutMax"></param>
        /// <param name="FlowId"></param>
        /// <param name="FlowName"></param>
        public udcStatResult(int uutMax, int fixPos, List<int> statFlowId)
        {
            this._uutMax = uutMax;

            this._fixPos = fixPos;

            _statFlowId = statFlowId;

            InitializeComponent();

            SetDoubleBuffered();

            InitialControl();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
        }
        /// <summary>
        /// 加载控件
        /// </summary>
        private void InitialControl()
        {
            labUUTTTNum = new Label[] { labLinePTTT, labLineBITT, labLineHPTT, labLineAteTT };
            labUUTFailNum = new Label[] { labLinePTFail, labLineBIFail, labLineHPFail, labLineAteFail };

            labStatTTNum = new Label[] { labPTTT, labBITT, labHPTT, labAteTT };
            labStatFailNum = new Label[] { labPTFail, labBIFail, labHPFail, labAteFail };
        }
        #endregion

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

        #region 字段
        /// <summary>
        /// 产品数量
        /// </summary>
        private int _uutMax = 0;
        /// <summary>
        /// 治具显示方向-0:1-16 1:16-1
        /// </summary>
        private int _fixPos = 0;
        /// <summary>
        /// 流程站别4,3,2,1
        /// </summary>
        private List<int> _statFlowId = new List<int>();
        /// <summary>
        /// 需要不良确认
        /// </summary>
        private bool _disFail = false;
        #endregion

        #region 属性
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
        private Label[][] labUIResult = null;
        private Label[] labUUTTTNum = null;
        private Label[] labUUTFailNum = null;
        private Label[] labStatTTNum = null;
        private Label[] labStatFailNum = null;
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
                this._uutMax = uutMax;

                disposePanel();

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

                load_Result();

                load_Sn_UUT();

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
                labUIResult = null;
                panelSn.Dispose();
                panelSn = null;
            }

        }
        /// <summary>
        /// 加载8通道测试界面
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
                    lab2.ImageList = imageList1;
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
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 加载16通道测试界面
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
                    lab2.ImageList = imageList1;
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

            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 加载测试结果控件
        /// </summary>
        private void load_Result()
        {
            labUIResult = new Label[_statFlowId.Count][];
            for (int i = 0; i < _statFlowId.Count; i++)
            {
                labUIResult[i] = new Label[_uutMax];
            }
            for (int z = 0; z < _statFlowId.Count; z++)
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    labUIResult[z][i] = new Label();
                    labUIResult[z][i].Name = "labResult" + z.ToString() + "_" + i.ToString();
                    labUIResult[z][i].Dock = DockStyle.Fill;
                    labUIResult[z][i].TextAlign = ContentAlignment.MiddleCenter;
                    labUIResult[z][i].Margin = new Padding(1);
                    labUIResult[z][i].Text = "";
                }
            }
        }
        /// <summary>
        /// 加载产品条码界面
        /// </summary>
        private void load_Sn_UUT()
        {

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
            labT3.Text = CLanguage.Lan("初测");
            labT3.Tag = "初测";

            Label labT4 = new Label();
            labT4.Name = "labTitle4";
            labT4.Dock = DockStyle.Fill;
            labT4.Margin = new Padding(0);
            labT4.TextAlign = ContentAlignment.MiddleCenter;
            labT4.Text = CLanguage.Lan("老化");
            labT4.Tag = "老化";

            Label labT5 = new Label();
            labT5.Name = "labTitle5";
            labT5.Dock = DockStyle.Fill;
            labT5.Margin = new Padding(0);
            labT5.TextAlign = ContentAlignment.MiddleCenter;
            labT5.Text = CLanguage.Lan("高压");
            labT5.Tag = "高压";

            Label labT6 = new Label();
            labT6.Name = "labTitle6";
            labT6.Dock = DockStyle.Fill;
            labT6.Margin = new Padding(0);
            labT6.TextAlign = ContentAlignment.MiddleCenter;
            labT6.Text = "ATE";

            //初始化panelSn
            panelSn = new TableLayoutPanel();
            panelSn.Dock = DockStyle.Fill;
            panelSn.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            panelSn.Margin = new Padding(1);
            panelSn.RowCount = _uutMax + 1;
            panelSn.ColumnCount = 6;
            panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            panelSn.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            for (int i = 0; i < _uutMax; i++)
                panelSn.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panelSn.Controls.Add(labT1, 0, 0);
            panelSn.Controls.Add(labT2, 1, 0);
            panelSn.Controls.Add(labT3, 2, 0);
            panelSn.Controls.Add(labT4, 3, 0);
            panelSn.Controls.Add(labT5, 4, 0);
            panelSn.Controls.Add(labT6, 5, 0);
            for (int i = 0; i < _uutMax; i++)
            {
                panelSn.Controls.Add(labNo[i], 0, i + 1);
                panelSn.Controls.Add(labSn[i], 1, i + 1);
                for (int z = 0; z < _statFlowId.Count; z++)
                    panelSn.Controls.Add(labUIResult[z][i], 2 + z, i + 1);
            }
            panelSn.GetType().GetProperty("DoubleBuffered",
                                      System.Reflection.BindingFlags.Instance |
                                      System.Reflection.BindingFlags.NonPublic)
                                      .SetValue(panelSn, true, null);
            panel1.Controls.Add(panelSn, 1, 0);
        }
        #endregion

        #region 面板回调函数
        private void udcStatResult_Load(object sender, EventArgs e)
        {
            LoadUUT(_uutMax);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("是否已确定不良,需继续下一步测试?"),
                                CLanguage.Lan("不良确定"), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
               == DialogResult.Yes)
            {
                _disFail = false;

            }
        }
        private void labClrUUT_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要测试站别统计?"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            == DialogResult.Yes)
            {
                OnBtnClickArgs.OnEvented(new CBtnClickArgs(0, "测试站别统计"));
            }
        }
        private void labClrFix_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要清除线体产能统计?"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                  == DialogResult.Yes)
            {
                OnBtnClickArgs.OnEvented(new CBtnClickArgs(1, "线体产能统计"));
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置空闲状态
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
                    for (int z = 0; z < _statFlowId.Count; z++)
                    {
                        labUIResult[z][i].Text = "";
                        labUIResult[z][i].BackColor = Button.DefaultBackColor;
                    }
                }
                panelUUT.Visible = false;
                tlTip.RemoveAll();
            }
        }
        /// <summary>
        /// 设置为空治具
        /// </summary>
        public void SetIsNull(string idCard)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetIsNull), idCard);
            else
            {
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                labStatus.Text = CLanguage.Lan("空治具准备过站.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labUUT[i].BackColor = Color.White;
                    for (int z = 0; z < _statFlowId.Count; z++)
                    {
                        labUIResult[z][i].Text = "";
                        labUIResult[z][i].BackColor = Button.DefaultBackColor;
                    }
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 读卡错误
        /// </summary>
        public void SetIdAlarm()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetIdAlarm));
            else
            {
                labIdCard.Text = CLanguage.Lan("读治具ID错误");
                labIdCard.ForeColor = Color.Red;
                labStatus.Text = CLanguage.Lan("读治具ID错误,请检查.");
                labStatus.ForeColor = Color.Red;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labUUT[i].BackColor = Color.White;
                    for (int z = 0; z < _statFlowId.Count; z++)
                    {
                        labUIResult[z][i].Text = "";
                        labUIResult[z][i].BackColor = Button.DefaultBackColor;
                    }
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 设置报警
        /// </summary>
        /// <param name="idCard"></param>
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
        /// 设置信息
        /// </summary>
        /// <param name="idCard"></param>
        /// <param name="alarmInfo"></param>
        public void SetInfo(string info)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetInfo), info);
            else
            {
                labStatus.Text = info;
                labStatus.ForeColor = Color.Blue;
            }
        }
        /// <summary>
        /// 设置测试结果
        /// </summary>
        /// <param name="idCard"></param>
        /// <param name="serialNos"></param>
        /// <param name="results"></param>
        /// <param name="resultIds"></param>
        /// <param name="alarmInfo"></param>
        public void SetResult(string idCard, List<string> serialNos, List<int> results, List<int> resultIds)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, List<string>, List<int>, List<int>>(SetResult),
                                      idCard, serialNos, results, resultIds);
            else
            {
                labIdCard.Text = idCard;

                labIdCard.ForeColor = Color.Blue;

                bool uutPass = true;

                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = serialNos[i];

                    labSn[i].ForeColor = Color.Blue;

                    for (int z = 0; z < _statFlowId.Count; z++)
                    {
                        labUIResult[z][i].Text = "";
                        labUIResult[z][i].BackColor = Button.DefaultBackColor;
                    }
                    if (serialNos[i] == "")
                    {
                        labUUT[i].BackColor = Color.White;
                        continue;
                    }

                    #region 测试结果为PASS
                    if (results[i] == 0)
                    {
                        if (resultIds[i] == _statFlowId[0]) //ATE
                        {
                            labUUT[i].BackColor = Color.LimeGreen;
                            for (int z = 0; z < _statFlowId.Count; z++)
                            {
                                labUIResult[z][i].Text = "PASS";
                                labUIResult[z][i].ForeColor = Color.Blue;
                            }
                        }
                        else
                        {
                            for (int statNo = 1; statNo < _statFlowId.Count; statNo++)
                            {
                                uutPass = false;
                                labUUT[i].BackColor = Color.Red;
                                if (resultIds[i] == _statFlowId[statNo]) //HIPOT
                                {
                                    for (int z = 0; z < _statFlowId.Count - statNo; z++)
                                    {
                                        labUIResult[z][i].Text = "PASS";
                                        labUIResult[z][i].ForeColor = Color.Blue;
                                    }
                                    break;
                                }
                            }
                        }
                        continue;
                    }
                    #endregion

                    #region 测试结果为FAIL

                    uutPass = false;

                    labUUT[i].BackColor = Color.Red;

                    labSn[i].ForeColor = Color.Red;

                    for (int statNo = 0; statNo < _statFlowId.Count; statNo++)
                    {
                        if (resultIds[i] == _statFlowId[statNo])
                        {
                            //FAIL          
                            int iDN = _statFlowId[statNo] - 1;
                            labUIResult[iDN][i].Text = "FAIL";
                            labUIResult[iDN][i].ForeColor = Color.Red;
                            //PASS
                            for (int z = 0; z < _statFlowId.Count - 1 - statNo; z++)
                            {
                                labUIResult[z][i].Text = "PASS";
                                labUIResult[z][i].ForeColor = Color.Blue;
                            }
                            break;
                        }
                        continue;
                    }
                    #endregion
                }

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

                panelUUT.Visible = true;

            }
        }
        /// <summary>
        /// 设置提示信息
        /// </summary>
        /// <param name="uutInfo"></param>
        public void SetToolTip(List<string> uutInfo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<string>>(SetToolTip), uutInfo);
            else
            {
                tlTip.RemoveAll();
                for (int i = 0; i < uutInfo.Count; i++)
                {
                    if (uutInfo[i] != "")
                        tlTip.SetToolTip(labUUT[i], uutInfo[i]);
                }
            }
        }
        /// <summary>
        /// 设置当前测试工位统计
        /// </summary>
        /// <param name="ttNum"></param>
        /// <param name="failNum"></param>
        public void SetTestNum(int idNo, int ttNum, int failNum)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, int, int>(SetTestNum), idNo, ttNum, failNum);
            else
            {
                labUUTTTNum[idNo].Text = ttNum.ToString();
                labUUTFailNum[idNo].Text = failNum.ToString();
            }
        }
        /// <summary>
        /// 设置流程工位统计
        /// </summary>
        public void SetFlowNum(List<CYield> yields)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<CYield>>(SetFlowNum), yields);
            else
            {
                for (int i = 0; i < yields.Count; i++)
                {
                    labStatTTNum[i].Text = yields[i].TTNum.ToString();
                    labStatFailNum[i].Text = yields[i].FailNum.ToString();
                }
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
