using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.DEV.FCMB;

namespace GJ.KunX.BURNIN.Udc
{
    public partial class udcFixture : UserControl
    {
        #region 枚举
        /// <summary>
        /// UI状态
        /// </summary>
        public enum EUI
        {
            空闲,
            状态,
            老化
        }
        /// <summary>
        /// 菜单状态
        /// </summary>
        public enum ESetMenu
        {
            显示信息,
            显示曲线,
            位置空闲,
            禁用位置,
            启动老化,
            停止老化,
            解除报警,
            清除不良,
            复位老化,
            指定位置老化,
            优先老化结束,
            设置为空治具,
            复位快充模式,
            指定优先出机
        }
        #endregion

        #region 构造函数
        public udcFixture(int idNo, int fixPos)
        {
            this._fixPos = fixPos;

            this._idNo = idNo;

            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();

            load_Max_UUT(_uutMax);

        }
        public override string ToString()
        {
            return _name;
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {
            labStatus = new Label();
            labStatus.Dock = DockStyle.Fill;
            labStatus.Font = new Font("宋体", 9.5f);
            labStatus.TextAlign = ContentAlignment.MiddleCenter;
            labStatus.Text = "";
            labStatus.BackColor = Color.White;
            //是否显示提示信息
            tlTip.Active = true;
            //是否显示提示信息，当窗体没有获得焦点时
            tlTip.ShowAlways = true;
            //工具提示”窗口显示之前，鼠标指针必须在控件内保持静止的时间（以毫秒计）
            tlTip.InitialDelay = 200;
            // 提示信息刷新时间 
            tlTip.ReshowDelay = 300;
            //提示信息延迟时间
            tlTip.AutomaticDelay = 200;
            // 提示信息弹出时间
            tlTip.AutoPopDelay = 10000;
            // 提示信息
            tlTip.ToolTipTitle = CLanguage.Lan("产品信息");
        }
        /// <summary>
        /// 设置双缓冲
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
        }
        /// <summary>
        /// 加载20通道界面
        /// </summary>
        private void load_Max_UUT(int uutMax)
        {
            try
            {
                //治具界面
                if (panelUUT != null)
                {
                    foreach (Control item in panelUUT.Controls)
                    {
                        panelUUT.Controls.Remove(item);
                        item.Dispose();
                    }
                    labUUT.Clear();
                    panelUUT.Dispose();
                    panelUUT = null;
                }

                for (int i = 0; i < uutMax; i++)
                {
                    Label lab = new Label();
                    lab.Name = "labUUT" + i.ToString();
                    lab.Dock = DockStyle.Fill;
                    lab.Margin = new Padding(0);
                    lab.TextAlign = ContentAlignment.MiddleCenter;
                    lab.BackColor = Color.White;
                    lab.Text = "";
                    labUUT.Add(lab);
                }
                //初始化panelUUT
                int N = _uutMax / 2;
                panelUUT = new TableLayoutPanel();
                panelUUT.Dock = DockStyle.Fill;
                panelUUT.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                panelUUT.Margin = new Padding(0);
                panelUUT.RowCount = N;
                panelUUT.ColumnCount = 2;
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelUUT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                for (int i = 0; i < N; i++)
                    panelUUT.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                for (int i = 0; i < N; i++)
                {
                    if (_fixPos == 0)
                    {
                        panelUUT.Controls.Add(labUUT[i], 0, i);
                        panelUUT.Controls.Add(labUUT[i + N], 1, N - i - 1);
                    }
                    else
                    {
                        ;
                        panelUUT.Controls.Add(labUUT[i + N], 0, i);
                        panelUUT.Controls.Add(labUUT[N - i - 1], 1, i);
                    }
                }
                panelUUT.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT, true, null);
                this.Controls.Add(labStatus);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 面板控件
        private TableLayoutPanel panelUUT = null;
        private Label labStatus = null;
        private List<Label> labUUT = new List<Label>();
        #endregion

        #region 字段
        /// <summary>
        /// 治具编号
        /// </summary>
        private int _idNo = 0;
        /// <summary>
        /// 槽位名称
        /// </summary>
        private string _name = string.Empty;
        /// <summary>
        /// 产品数量
        /// </summary>
        private int _uutMax = 16;
        /// <summary>
        /// 治具显示方向-0:1-16 1:16-1
        /// </summary>
        private int _fixPos = 0;
        /// <summary>
        /// 治具信息
        /// </summary>
        private CUUT _runUUT = null;
        /// <summary>
        /// 当前状态
        /// </summary>
        private EUI IsUI = EUI.空闲;
        /// <summary>
        /// 治具信息
        /// </summary>
        private string uutBaseInfo = string.Empty;
        /// <summary>
        /// 设备信息
        /// </summary>
        private string uutDevInfo = string.Empty;
        /// <summary>
        /// 显示备份
        /// </summary>
        private string strBackup = string.Empty;
        #endregion

        #region 属性
        /// <summary>
        /// 编号
        /// </summary>
        public int idNo
        {
            set { _idNo = idNo; }
            get { return _idNo; }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string name
        {
            set { _name = name; }
            get { return _name; }
        }
        #endregion

        #region 面板回调函数
        private void udcFixture_Load(object sender, EventArgs e)
        {

        }
        private void udcFixture_Paint(object sender, PaintEventArgs e)
        {
            ResizePanel();
        }
        private void udcFixture_Resize(object sender, EventArgs e)
        {
            
        }
        private void tlDisplay_Click(object sender, EventArgs e)
        {
            menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.显示信息));
        }
        private void tlChart_Click(object sender, EventArgs e)
        {
            menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.显示曲线));
        }
        private void tlFree_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要设置该位置为空闲状态?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.位置空闲));
            }
        }
        private void tlForbit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要设置该位置为禁用状态?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.禁用位置));
            }
        }
        private void tlStartBI_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要强制该位置启动老化?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.启动老化));
            }
        }
        private void tlEndBI_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要强制该位置结束老化?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.停止老化));
            }
        }
        private void tlClrAlarm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要解除该位置异常报警?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.解除报警));
            }
        }
        private void tlClrFail_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要清除该位置产品不良?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.清除不良));
            }
        }
        private void tlResetBI_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要复位该位置老化?"), "Tip", MessageBoxButtons.YesNo,
              MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.复位老化));
            }
        }
        private void tlInPos_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要指定位置老化?"), "Tip", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.指定位置老化));
            }
        }
        private void tlInPosFirstEnd_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要该位置优先结束老化?"), "Tip", MessageBoxButtons.YesNo,
              MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.优先老化结束));
            }
        }
        private void tlSetIsNull_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要设置为空治具?"), "Tip", MessageBoxButtons.YesNo,
             MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.设置为空治具));
            }
        }
        private void tlResetQCM_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要复位快充模式?"), "Tip", MessageBoxButtons.YesNo,
             MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.复位快充模式));
            }
        }
        private void tlFirstOut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要指定优先出机?"), "Tip", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                menuClick.OnEvented(new CSetMenuArgs(_idNo, ESetMenu.指定优先出机));
            }
        }
        #endregion

        #region 面板消息
        public class CSetMenuArgs : EventArgs
        {
            public readonly int idNo;
            public readonly ESetMenu menuInfo;
            public CSetMenuArgs(int idNo, ESetMenu menuInfo)
            {
                this.idNo = idNo;
                this.menuInfo = menuInfo;
            }
        }
        public COnEvent<CSetMenuArgs> menuClick = new COnEvent<CSetMenuArgs>();
        #endregion

        #region 方法
        /// <summary>
        /// 设置测试状态
        /// </summary>
        public void SetUUT(CUUT runUUT)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUUT>(SetUUT), runUUT);
            else
            {
                if (panelUUT == null)
                    return;

                this._runUUT = runUUT.Clone();

                EUI NowIsUI = EUI.空闲;

                //非空治具
                if (_runUUT.Para.AlarmCode == EAlarmCode.正常 && _runUUT.Para.IsNull == 0)
                {
                    if (_runUUT.Para.DoRun == EDoRun.正在老化 || _runUUT.Para.DoRun == EDoRun.老化完成 ||
                        _runUUT.Para.DoRun == EDoRun.老化结束)
                        NowIsUI = EUI.老化;
                    else
                        NowIsUI = EUI.状态;
                }
                else
                {
                    NowIsUI = EUI.状态;
                }

                if (IsUI != NowIsUI)
                {
                    foreach (Control item in this.Controls)
                    {
                        this.Controls.Remove(item);
                    }

                    if (NowIsUI == EUI.老化)
                        this.Controls.Add(panelUUT);
                    else
                        this.Controls.Add(labStatus);

                    IsUI = NowIsUI;
                }

                uutBaseInfo = "【"+ CLanguage.Lan("位置编号") + "】:" + _runUUT.Base.localName + "\r\n";

                uutDevInfo = string.Empty;

                if (_runUUT.Para.DoRun == EDoRun.空治具到位)
                {
                    uutDevInfo += "【"+ CLanguage.Lan("治具ID") + "】:" + _runUUT.Para.IdCard + "\r\n";
                }

                uutDevInfo += "【" + CLanguage.Lan("控制板地址") + "】:" + CGlobalPara.SysPara.Dev.MonCom[_runUUT.Base.ctrlCom] + "_" +
                              _runUUT.Base.ctrlAddr.ToString("D2") +
                              ";【"+ CLanguage.Lan("ERS地址") + "】:" + CGlobalPara.SysPara.Dev.ErsCom[_runUUT.Base.ersCom] +
                              "_" + _runUUT.Base.ersAddr.ToString("D2") + "_" + _runUUT.Base.ersCH.ToString() + ";\r\n";                                 

                uutDevInfo += "【"+  CLanguage.Lan("母治具使用次数") + "】:" + _runUUT.Para.UsedNum.ToString() +
                              ";【"+ CLanguage.Lan("连续不良次数") + "】:" + _runUUT.Para.FailNum.ToString() + ";\r\n";

                if (IsUI == EUI.状态)
                    SetStatus();
                else
                    SetBI();
            }
        }
        /// <summary>
        /// 设置治具状态
        /// </summary>
        /// <param name="runUUT"></param>
        private void SetStatus()
        {
            string runInfo = string.Empty;

            if (_runUUT.Para.DoRun != EDoRun.位置禁用 && _runUUT.Para.AlarmCode != EAlarmCode.正常)
            {
                if (_runUUT.Para.AlarmCode == EAlarmCode.母治具使用寿命已到)
                {
                    labStatus.Text = CLanguage.Lan("针盘维修");
                    labStatus.ForeColor = Color.Red;
                    labStatus.BackColor = Color.Yellow;
                    runInfo = CLanguage.Lan("异常报警") + ":" + _runUUT.Para.AlarmCode.ToString();
                }
                else
                {
                    labStatus.Text = CLanguage.Lan("异常报警");
                    labStatus.ForeColor = Color.Red;
                    labStatus.BackColor = Color.Yellow;
                    runInfo = CLanguage.Lan("异常报警") + ":" + _runUUT.Para.AlarmCode.ToString();
                }
            }
            else if (_runUUT.Para.AlarmInfo != "")
            {
                labStatus.Text = _runUUT.Para.AlarmInfo;
                labStatus.ForeColor = Color.Red;
                labStatus.BackColor = Color.Yellow;
                runInfo = CLanguage.Lan("异常报警") + ":" + _runUUT.Para.AlarmInfo;
            }
            else
            {
                if (_runUUT.Para.DoRun >= EDoRun.启动老化 && _runUUT.Para.DoRun <= EDoRun.老化结束)
                {
                    if (_runUUT.Para.IsNull == 1) //空治具到位
                    {
                        labStatus.Text = "";
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.PaleTurquoise;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:"+ CLanguage.Lan("空治具");
                        runInfo += "\r\n";
                        runInfo = uutBaseInfo + runInfo + uutDevInfo;
                        if (strBackup != runInfo)
                        {
                            strBackup = runInfo;
                            tlTip.SetToolTip(labStatus, runInfo);
                        }
                        return;
                    }
                }
                switch (_runUUT.Para.DoRun)
                {
                    case EDoRun.异常报警:
                        labStatus.Text = _runUUT.Para.AlarmInfo;
                        labStatus.ForeColor = Color.Red;
                        labStatus.BackColor = Color.Yellow;
                        runInfo = CLanguage.Lan("异常报警") + ":" + _runUUT.Para.AlarmInfo;
                        break;
                    case EDoRun.位置禁用:
                        labStatus.Text = "";
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Control.DefaultBackColor;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:"+ CLanguage.Lan("该位置已禁用");
                        break;
                    case EDoRun.位置空闲:
                        labStatus.Text = "";
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.White;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:" + CLanguage.Lan("空闲,等待进机.");
                        break;
                    case EDoRun.正在进机:
                        labStatus.Text = _runUUT.Para.DoRun.ToString();
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.Turquoise;
                        runInfo = "【" + CLanguage.Lan("位置状态")+ "】:"+ CLanguage.Lan("治具正在进机中.");
                        break;
                    case EDoRun.进机完毕:
                        labStatus.Text = _runUUT.Para.DoRun.ToString();
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.Turquoise;
                        runInfo = "【"+  CLanguage.Lan("位置状态") + "】:"+ CLanguage.Lan("治具进机完毕.");
                        break;
                    case EDoRun.启动老化:
                        labStatus.Text = _runUUT.Para.DoRun.ToString();
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.Turquoise;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:" + CLanguage.Lan("治具启动老化.");
                        break;
                    case EDoRun.老化自检:
                        labStatus.Text = _runUUT.Para.DoRun.ToString();
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.Turquoise;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:"+ CLanguage.Lan("老化自检.");
                        break;
                    case EDoRun.正在老化:
                        runInfo = "【"+ CLanguage.Lan("位置状态")+ "】:"+ CLanguage.Lan("正在老化.");
                        break;
                    case EDoRun.老化完成:
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:" + CLanguage.Lan("老化完成.");
                        break;
                    case EDoRun.老化结束:
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:"+  CLanguage.Lan("老化结束.");
                        break;
                    case EDoRun.正在出机:
                        labStatus.Text = _runUUT.Para.DoRun.ToString();
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.Turquoise;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:"+ CLanguage.Lan("正在出机.");
                        break;
                    case EDoRun.空治具到位:
                        labStatus.Text = "";
                        labStatus.ForeColor = Color.Black;
                        labStatus.BackColor = Color.PaleTurquoise;
                        runInfo = "【"+ CLanguage.Lan("位置状态") + "】:"+ CLanguage.Lan("空治具.");
                        break;
                    default:
                        break;
                }
            }
            runInfo += "\r\n";
            string info = uutBaseInfo + runInfo + uutDevInfo;
            if (strBackup != info)
            {
                strBackup = info;
                //tlTip.RemoveAll();
                tlTip.SetToolTip(labStatus, info);
            }
        }
        /// <summary>
        /// 设置治具老化中
        /// </summary>
        /// <param name="runUUT"></param>
        private void SetBI()
        {
            string testInfo = string.Empty;
            bool uutPass = true;
            string ac_ctrl = (_runUUT.Para.CtrlACON == 1 ? "AC ON;" : "AC OFF;");
            string ac_status = (_runUUT.Para.CtrlOnOff == 1 ? "AC ON;" : "AC OFF;");
            TimeSpan ts = new TimeSpan(0, 0, _runUUT.Para.RunTime);
            string runTime = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" +
                             ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
            testInfo += "【"+ CLanguage.Lan("机种名称") + "】:" + _runUUT.Para.ModelName +
                        ";【"+ CLanguage.Lan("老化时间") + "】:" +
                        (((double)_runUUT.Para.BurnTime) / 3600).ToString("0.0") + CLanguage.Lan("小时") + ";\r\n";
            testInfo += "【" + CLanguage.Lan("开始时间") + "】:" + _runUUT.Para.StartTime +
                       ";【" + CLanguage.Lan("运行时间") + "】:" + runTime + ";\r\n";
            testInfo += "【"+ CLanguage.Lan("当前控制电压") + "】:" + ac_ctrl + 
                        "【"+ CLanguage.Lan("电压同步状态") + "】:" + ac_status +
                        "【"+ CLanguage.Lan("输入电压") + "】:" + _runUUT.Para.CtrlACVolt.ToString("0.0") + "V" + "\r\n" +
                        "【"+ CLanguage.Lan("负载电压") + "】:" + _runUUT.Para.CtrlVBus.ToString("0.0") + "V" + "\r\n" +
                        "【"+ CLanguage.Lan("快充模式") + "】:" + ((EQCM)_runUUT.OnOff.TimeRun.CurQCType).ToString() + ";" +
                        "【"+ CLanguage.Lan("快充电压") + "】:" + _runUUT.OnOff.TimeRun.CurQCV.ToString() + "V;" +
                        "【"+ CLanguage.Lan("快充状态") + "】:" + _runUUT.OnOff.TimeRun.CurQCM.ToString() + ";" + "\r\n";                         
            for (int i = 0; i < _uutMax; i++)
            {
                if (_runUUT.Led[i].serialNo != "")
                {
                    if (_runUUT.Led[i].failResult == 0)
                    {
                        testInfo += "【" + CLanguage.Lan("产品") + (i + 1).ToString("D2") + "】:" + _runUUT.Led[i].serialNo + ";";
                        testInfo += CLanguage.Lan("电压") + "=" + _runUUT.Led[i].unitV.ToString("0.000") + "V;"+
                                    CLanguage.Lan("电流") + "=" + _runUUT.Led[i].unitA.ToString("0.00") + "A;";
                        testInfo += "->PASS;\r\n";
                    }
                    else
                    {
                        uutPass = false;
                        testInfo += "【" + CLanguage.Lan("产品") + (i + 1).ToString("D2") + "】:" + _runUUT.Led[i].serialNo + ";";
                        testInfo += CLanguage.Lan("电压") + "=" + _runUUT.Led[i].unitV.ToString("0.000") + "V;"+
                                    CLanguage.Lan("电流") + "=" + _runUUT.Led[i].unitA.ToString("0.00") + "A;";
                        testInfo += "->FAIL;";
                        testInfo += CLanguage.Lan("不良时间:") + _runUUT.Led[i].failTime + ";"+ 
                                    CLanguage.Lan("不良信息:") + _runUUT.Led[i].failInfo + ";\r\n";
                    }
                }
                else
                {
                    testInfo += "【" + CLanguage.Lan("产品") + (i + 1).ToString("D2") + "】:"+ CLanguage.Lan("无产品槽位") + ";\r\n";
                }
            }
            if (uutPass)
                testInfo = "【"+ CLanguage.Lan("治具ID") + "】:[" + _runUUT.Para.IdCard + "];\r\n" +
                           "【"+ CLanguage.Lan("老化结果") + "】:PASS;\r\n" + testInfo;
            else
                testInfo = "【"+ CLanguage.Lan("治具ID") + "】:[" + _runUUT.Para.IdCard + "];\r\n" + 
                           "【"+ CLanguage.Lan("老化结果") + "】:FAIL;\r\n" + testInfo;

            string info = uutBaseInfo + testInfo + uutDevInfo;

            if (strBackup != info)
            {
                strBackup = info;
                //tlTip.RemoveAll();
                tlTip.SetToolTip(panelUUT, info);
            }

            if (_runUUT.Para.DoRun == EDoRun.正在老化)
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    if (_runUUT.Led[i].serialNo == "")
                    {
                        labUUT[i].BackColor = Color.White;
                    }
                    else
                    {
                        if (_runUUT.Led[i].failResult == 0)
                        {
                            labUUT[i].BackColor = Color.LimeGreen;
                        }
                        else if (_runUUT.Led[i].failResult == 2)
                        {
                            labUUT[i].BackColor = Color.Yellow;
                        }
                        else
                        {
                            labUUT[i].BackColor = Color.Red;
                        }
                    }
                    tlTip.SetToolTip(labUUT[i], info);
                }
            }
            else
            {
                for (int i = 0; i < _uutMax; i++)
                {
                    if (_runUUT.Led[i].serialNo == "")
                    {
                        labUUT[i].BackColor = Color.White;
                    }
                    else
                    {
                        if (_runUUT.Led[i].failResult == 0)
                            labUUT[i].BackColor = Color.Lime;
                        else
                            labUUT[i].BackColor = Color.Fuchsia;
                    }
                    tlTip.SetToolTip(labUUT[i], info);
                }
            }
        }
        #endregion

        #region Panel行距调整
        public void ResizePanel()
        {
            try
            {
                if (panelUUT == null)
                    return;

                int row = panelUUT.RowCount;

                int h1 = panelUUT.Height - (row + 1) * 2;

                float h2 = (float)h1 / row;

                int h3 = h1 % row;

                float f = (float)h3 / row;

                if (h2 > 1)
                {
                    for (int i = 0; i < row; i++)
                    {
                        panelUUT.RowStyles[i].SizeType = SizeType.Absolute;

                        if (h3 > row - i)
                        {
                            panelUUT.RowStyles[i].Height = (int)h2 + f;
                        }
                        else
                        {
                            panelUUT.RowStyles[i].Height = (int)h2;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < row; i++)
                    {
                        panelUUT.RowStyles[i].SizeType = SizeType.Percent;
                        panelUUT.RowStyles[i].Height = 100;
                    }
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
