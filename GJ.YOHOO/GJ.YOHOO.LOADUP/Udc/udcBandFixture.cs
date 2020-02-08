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
    public partial class udcBandFixture : UserControl
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

                    if (lab.Tag!=null)
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

        #region 事件定义
        /// <summary>
        /// 选择条码事件
        /// </summary>
        public class CCheckArgs : EventArgs
        {
            public readonly int idNo = 0;
            public readonly bool value = false;
            public CCheckArgs(int idNo, bool value)
            {
                this.idNo = idNo;
                this.value = value;
            }
        }
        /// <summary>
        /// 回传文本内容
        /// </summary>
        public class CSnKeyArgs : EventArgs
        {
            public readonly string keyString;
            public CSnKeyArgs(string keyString)
            {
                this.keyString = keyString;
            }
        }
        public COnEvent<CCheckArgs> OnCheckArgs = new COnEvent<CCheckArgs>(); 
        /// <summary>
        /// 文本输入消息
        /// </summary>
        public COnEvent<CSnKeyArgs> OnSnKeyPress = new COnEvent<CSnKeyArgs>();
        #endregion

        #region 构造函数
        public udcBandFixture(int fixPos)
        {
            this._fixPos = fixPos;

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
            panel4.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel4, true, null);
            panel5.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel5, true, null);
            panel6.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel6, true, null);
        }
        #endregion

        #region 面板控件
        private TableLayoutPanel panelUUT = null;
        private List<Label> labUUTNo = new List<Label>();
        private List<Label> labUUT = new List<Label>();
        private TableLayoutPanel panelSn = null;
        private List<CheckBox> labNo = new List<CheckBox>();
        private List<Label> labSn = new List<Label>();
        #endregion

        #region 属性
        /// <summary>
        /// 治具ID
        /// </summary>
        public string IdCard
        {
            get { return labIdCard.Text; }
        }
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
        #endregion

        #region 面板回调函数
        private void udcFixSnBar_Load(object sender, EventArgs e)
        {

        }
        private void chkNo_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            int len =checkBox.Name.Length;

            int idNo = System.Convert.ToInt16(checkBox.Name.Substring(5, len - 5));

            OnCheckArgs.OnEvented(new CCheckArgs(idNo,checkBox.Checked));   
        }
        private void txtSnPress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                OnSnKeyPress.OnEvented(new CSnKeyArgs(txtSnPress.Text));
            }
        }
        #endregion

        #region 面板方法
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

                    CheckBox lab3 = new CheckBox();
                    lab3.Name = "labNo" + i.ToString();
                    lab3.Font = new Font("宋体", 9);
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(3, 0, 0, 0);
                    lab3.Text = (i + 1).ToString("D2");
                    labNo.Add(lab3); 
                    labNo[i].CheckedChanged += new EventHandler(chkNo_CheckedChanged);
                    labNo[i].Checked = true;

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
                panelUUT.Margin = new Padding(5, 20, 5, 20);
                panelUUT.RowCount = 3;
                panelUUT.ColumnCount = _uutMax;
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelUUT.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
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
                labT1.Dock = DockStyle.Fill;
                labT1.Margin = new Padding(0);
                labT1.TextAlign = ContentAlignment.MiddleCenter;
                labT1.Tag = "编号";
                labT1.Text = CLanguage.Lan("编号");

                Label labT2 = new Label();
                labT2.Dock = DockStyle.Fill;
                labT2.Margin = new Padding(0);
                labT2.TextAlign = ContentAlignment.MiddleCenter;
                labT2.Tag = "产品条码";
                labT2.Text = CLanguage.Lan("产品条码");

                //初始化panelSn
                panelSn = new TableLayoutPanel();
                panelSn.Dock = DockStyle.Fill;
                panelSn.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelSn.Margin = new Padding(1);
                panelSn.RowCount = _uutMax + 1;
                panelSn.ColumnCount = 2;
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelSn.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                for (int i = 0; i < _uutMax; i++)
                    panelSn.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelSn.Controls.Add(labT1, 0, 0);
                panelSn.Controls.Add(labT2, 1, 0);
                for (int i = 0; i < _uutMax; i++)
                {
                    panelSn.Controls.Add(labNo[i], 0, i + 1);
                    panelSn.Controls.Add(labSn[i], 1, i + 1);
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

                    CheckBox lab3 = new CheckBox();
                    lab3.Name = "labNo" + i.ToString();
                    lab3.Font = new Font("宋体", 9);
                    lab3.Dock = DockStyle.Fill;
                    lab3.Margin = new Padding(3, 0, 0, 0);
                    lab3.Text = (i + 1).ToString("D2");
                    labNo.Add(lab3);
                    labNo[i].CheckedChanged += new EventHandler(chkNo_CheckedChanged); 
                    labNo[i].Checked = true;
                   

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
                panelUUT.Margin = new Padding(5, 20, 5, 20);
                panelUUT.RowCount = N;
                panelUUT.ColumnCount = 4;
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
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
                labT1.Dock = DockStyle.Fill;
                labT1.Margin = new Padding(0);
                labT1.TextAlign = ContentAlignment.MiddleCenter;
                labT1.Tag = "编号";
                labT1.Text = CLanguage.Lan("编号");

                Label labT2 = new Label();
                labT2.Dock = DockStyle.Fill;
                labT2.Margin = new Padding(0);
                labT2.TextAlign = ContentAlignment.MiddleCenter;
                labT2.Tag = "产品条码";
                labT2.Text = CLanguage.Lan("产品条码");

                //初始化panelSn
                panelSn = new TableLayoutPanel();
                panelSn.Dock = DockStyle.Fill;
                panelSn.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelSn.Margin = new Padding(1);
                panelSn.RowCount = _uutMax + 1;
                panelSn.ColumnCount = 2;
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
                panelSn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelSn.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                for (int i = 0; i < _uutMax; i++)
                    panelSn.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelSn.Controls.Add(labT1, 0, 0);
                panelSn.Controls.Add(labT2, 1, 0);
                for (int i = 0; i < _uutMax; i++)
                {
                    panelSn.Controls.Add(labNo[i], 0, i + 1);
                    panelSn.Controls.Add(labSn[i], 1, i + 1);
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
        /// 加载界面
        /// </summary>
        /// <param name="uutMax"></param>
        public void LoadUUT(int uutMax)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(LoadUUT), uutMax);
            else
            {
                if (this._uutMax == uutMax)
                    return;

                disposePanel();

                this._uutMax = uutMax;

                if (_uutMax == 8)
                {
                    load_Max_UUT8();
                }
                else if (_uutMax == 16)
                {
                    load_Max_UUT16();
                }
                SetFree();
            }
        }
        /// <summary>
        /// 空闲状态 
        /// </summary>
        public void SetFree()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetFree));
            else
            {
                txtSnPress.ReadOnly = true;
                txtSnPress.Text = "";
                txtSnPress.Enabled = false;
                labIdCard.Text = "";
                labStatus.Text = CLanguage.Lan("等待治具到位.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labUUT[i].BackColor = Color.White;
                    labSn[i].Text = "";
                }
                panelUUT.Visible = false;
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
                txtSnPress.ReadOnly = true;
                txtSnPress.Text = "";
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                labStatus.Text = CLanguage.Lan("空治具准备过站.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labUUT[i].BackColor = Color.White;
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 设置治具就绪
        /// </summary>
        /// <param name="idCard"></param>
        /// <param name="serialNos"></param>
        public void SetReady(string idCard)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetReady), idCard);
            else
            {
                txtSnPress.Text = "";
                txtSnPress.ReadOnly = false;
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                labStatus.Text = CLanguage.Lan("治具就绪,等待绑定条码.");
                labStatus.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labUUT[i].BackColor = Color.White;
                    labSn[i].Text = "";
                    labSn[i].ForeColor = Color.Black;
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 设置过站
        /// </summary>
        /// <param name="idCard"></param>
        /// <param name="serialNos"></param>
        /// <param name="results"></param>
        public void SetEnd(string idCard, List<string> serialNos)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, List<string>>(SetEnd), idCard, serialNos);
            else
            {
                txtSnPress.Text = "";
                txtSnPress.Enabled = false;
                txtSnPress.ReadOnly = true;
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Blue;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = serialNos[i];
                    if (serialNos[i] != "")
                    {
                        labUUT[i].BackColor = Color.Cyan;
                        labSn[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labUUT[i].BackColor = Color.White;
                        labSn[i].ForeColor = Color.Black;
                    }
                }
                labStatus.Text = CLanguage.Lan("条码绑定OK,准备过站.");
                labStatus.ForeColor = Color.Blue;
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 设置条码
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="status"></param>
        /// <param name="bAlarm"></param>
        public void SetSn(int idNo, string serialNo, string status, bool bAlarm = false)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, string, string, bool>(SetSn), idNo, serialNo, status, bAlarm);
            else
            {
                labSn[idNo].Text = serialNo;
                labStatus.Text = status;
                if (!bAlarm)
                {
                    labUUT[idNo].BackColor = Color.Cyan;
                    labSn[idNo].ForeColor = Color.Blue;
                    labStatus.ForeColor = Color.Blue;
                    txtSnPress.Text = "";
                    txtSnPress.Focus();
                }
                else
                {
                    labUUT[idNo].BackColor = Color.Red;
                    labSn[idNo].ForeColor = Color.Red;
                    labStatus.ForeColor = Color.Red;
                    txtSnPress.SelectAll();
                }
            }
        }
        /// <summary>
        /// 启动手动扫描框
        /// </summary>
        /// <param name="enable"></param>
        public void SetSnTextEnable(ESnMode SnMode)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<ESnMode>(SetSnTextEnable), SnMode);
            else
            {
                if (SnMode == ESnMode.人工扫描模式)
                {
                    txtSnPress.Text = "";
                    txtSnPress.Enabled = true;
                    txtSnPress.ForeColor = Color.Blue;
                    txtSnPress.SelectAll();
                    txtSnPress.Focus();
                }
                else
                {
                    txtSnPress.Text = "";
                    txtSnPress.Enabled = false;
                }
            }
        }
        /// <summary>
        /// 设置手动扫描框状态
        /// </summary>
        /// <param name="bAlarm"></param>
        public void SetSnTextFouse(string info, bool bAlarm)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetSnTextFouse), info,bAlarm);
            else
            {
                if (bAlarm)
                {                    
                    txtSnPress.ForeColor = Color.Red;
                    txtSnPress.SelectAll();
                    txtSnPress.Focus();
                    labStatus.Text = info;
                    labStatus.ForeColor = Color.Red;
                }
                else
                {
                    txtSnPress.ForeColor = Color.Blue;
                    txtSnPress.Text = "";
                    txtSnPress.SelectAll();
                    txtSnPress.Focus();
                    labStatus.Text = info;
                    labStatus.ForeColor = Color.Blue;
                }
            }
        }
        /// <summary>
        /// 设置提示状态
        /// </summary>
        /// <param name="alarmInfo"></param>
        public void SetStatus(string status, bool bAlarm = false)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetStatus), status, bAlarm);
            else
            {
                labStatus.Text = status;
                if (!bAlarm)
                    labStatus.ForeColor = Color.Blue;
                else
                    labStatus.ForeColor = Color.Red;
            }
        }
        /// <summary>
        /// 设置读卡失败
        /// </summary>
        public void SetIdAlarm()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetIdAlarm));
            else
            {
                labIdCard.Text = CLanguage.Lan("治具ID错误");
                labIdCard.ForeColor = Color.Red;
                labStatus.Text = CLanguage.Lan("读取治具ID失败");
                labStatus.ForeColor = Color.Red;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "";
                    labUUT[i].BackColor = Color.White;
                }
                panelUUT.Visible = true;
            }
        }
        /// <summary>
        /// 设置治具使用寿命报警
        /// </summary>
        /// <param name="idCard"></param>
        public void SetFixStatus(string status, bool bAlarm = false)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetFixStatus), status, bAlarm);
            else
            {
                labFixStatus.Text = status;
                if (bAlarm)
                {
                    labFixStatus.ForeColor = Color.Red;
                }
                else
                {
                    labFixStatus.ForeColor = Color.Blue;
                }
            }
        }
        /// <summary>
        /// 设置治具不良次数报警
        /// </summary>
        /// <param name="idCard"></param>
        /// <param name="statName"></param>
        /// <param name="ttNum"></param>
        /// <param name="failNum"></param>
        public void SetFailNumAlarm(string idCard, List<string> statName, List<int> ttNumList, List<int> failNumList)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, List<string>, List<int>, List<int>>(SetFailNumAlarm),
                            idCard, statName, ttNumList, failNumList);
            else
            {
                labIdCard.Text = idCard;
                labIdCard.ForeColor = Color.Red;
                labStatus.Text =CLanguage.Lan("治具测试不良超过设置值");
                labStatus.ForeColor = Color.Red;
                for (int i = 0; i < _uutMax; i++)
                {
                    labSn[i].Text = "【" + statName[i] + "】" + CLanguage.Lan("总数") + "=" + ttNumList[i].ToString() +
                                                               ";"+  CLanguage.Lan("不良数") + "=" + failNumList[i].ToString();
                    labSn[i].ForeColor = Color.Red;
                    labUUT[i].BackColor = Color.White;
                }
                panelUUT.Visible = true;
            }
        }
        #endregion

    }
}
