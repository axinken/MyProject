using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using GJ.COM;
using GJ.PLUGINS;
using GJ.UI;
using GJ.APP;
using GJ.USER.APP;
using GJ.DEV.CATEXY;
using GJ.DEV.COM;
using GJ.KunX.ATE.Udc;
using GJ.USER.APP.MainWork;
using GJ.USER.APP.Iot;
using GJ.PDB;

namespace GJ.KunX.ATE
{
    public partial class FrmMain : Form,IChildMsg
    {
        #region 当前软件版本及日期
        private const string PROGRAM_VERSION = "V1.0.0";
        private const string PROGRAM_DATE = "2019/11/12";
        #endregion

        #region 插件方法
        /// <summary>
        /// 父窗口
        /// </summary>
        private Form _father = null;
        /// <summary>
        /// 父窗口唯一标识
        /// </summary>
        private string _fatherGuid = string.Empty;
        /// <summary>
        /// 窗体最大化
        /// </summary>
        private bool _maxSize = false;
        /// <summary>
        /// 加载当前窗口及软件版本日期
        /// </summary>
        /// <param name="fatherForm"></param>
        /// <param name="control"></param>
        /// <param name="guid"></param>
        public void OnShowDlg(Form fatherForm, Control control, string guid)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<Form, Control, string>(OnShowDlg), fatherForm, control, guid);
            else
            {
                this._father = fatherForm;
                this._fatherGuid = guid;

                this.Dock = DockStyle.Fill;
                this.TopLevel = false;
                this.FormBorderStyle = FormBorderStyle.None;
                control.Controls.Add(this);
                this.Show();

                string er = string.Empty;

                CReflect.SendWndMethod(_father, EMessType.OnShowVersion, out er,
                                                 new object[] { PROGRAM_VERSION, PROGRAM_DATE });

                loadAppSetting();
            }
        }
        /// <summary>
        /// 关闭当前窗口 
        /// </summary>
        public void OnCloseDlg()
        {
            if (MessageBox.Show(GJ.COM.CLanguage.Lan("确定要退出系统?"), GJ.COM.CLanguage.Lan("退出系统"),
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (MainWork != null)
                {
                    MainWork.CloseDlg();

                    MainWork = null;
                }

                System.Environment.Exit(0);
            }
        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="mPwrLevel"></param>
        public void OnLogIn(string user, int[] mPwrLevel)
        {
            CGlobalPara.LogName = user;
            for (int i = 0; i < mPwrLevel.Length; i++)
            {
                if (CGlobalPara.LogLevel.Length > i)
                    CGlobalPara.LogLevel[i] = mPwrLevel[i];
            }
        }
        /// <summary>
        /// 启动监控
        /// </summary>
        public void OnStartRun()
        {
            if (!MainWork.C_RUNNING && MainWork.C_DownLoad)
            {
                if (MainWork.OnRun())
                {
                    MainWork.C_RUNNING = true;
                }
            }

            if (MainWork.C_RUNNING)
            {
                string er = string.Empty;

                CReflect.SendWndMethod(_father, EMessType.OnShowStatus, out er, EIndicator.Auto);
            }
        }
        /// <summary>
        /// 停止监控
        /// </summary>
        public void OnStopRun()
        {
            if (MainWork.C_RUNNING)
            {
                MainWork.OnStop();

                MainWork.C_RUNNING = false;
            }

            string er = string.Empty;

            CReflect.SendWndMethod(_father, EMessType.OnShowStatus, out er, EIndicator.Idel);

        }
        /// <summary>
        /// 中英文切换
        /// </summary>
        public void OnChangeLAN()
        {
            SetUILanguage();
        }
        /// <summary>
        /// 消息响应
        /// </summary>
        /// <param name="para"></param>
        public void OnMessage(string name, int lPara, int wPara)
        {
            if (name == "F8")
            {
                if (!_maxSize)
                {
                    panel2.ColumnStyles[0].SizeType = SizeType.Percent;
                    panel2.ColumnStyles[1].SizeType = SizeType.Percent;
                    panel2.ColumnStyles[2].SizeType = SizeType.Absolute;
                    panel2.ColumnStyles[0].Width = 100;
                    panel2.ColumnStyles[1].Width = 100;
                    panel2.ColumnStyles[2].Width = 0;

                }
                else
                {
                    panel2.ColumnStyles[0].SizeType = SizeType.Percent;
                    panel2.ColumnStyles[1].SizeType = SizeType.Percent;
                    panel2.ColumnStyles[2].SizeType = SizeType.Percent;
                    panel2.ColumnStyles[0].Width = 100;
                    panel2.ColumnStyles[1].Width = 100;
                    panel2.ColumnStyles[2].Width = 100;
                }
                _maxSize = !_maxSize;
            }
            else if (name == "F6")
            {
                FrmClient.CreateInstance().Show();
            }
        }
        #endregion

        #region 语言设置
        /// <summary>
        /// 加载中英文字典
        /// </summary>
        private void LoadLanguge()
        {
            try
            {
                CLanguage.LoadLanType();

                string lanDB = Application.StartupPath + "\\LAN.accdb";

                if (!File.Exists(lanDB))
                    return;

                CAccess db = new CAccess(".", lanDB);

                string er = string.Empty;

                DataSet ds = null;

                string sqlCmd = "select * from LanList order by idNo";

                if (!db.QueryCmd(sqlCmd, out ds, out er))
                    return;

                Dictionary<string, string> lan = new Dictionary<string, string>();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string LAN_CH = ds.Tables[0].Rows[i]["LAN_CH"].ToString();

                    string LAN_EN = ds.Tables[0].Rows[i]["LAN_EN"].ToString();

                    if (!lan.ContainsKey(LAN_CH))
                        lan.Add(LAN_CH, LAN_EN);
                }

                CLanguage.Load(lan, out er);

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 设置中英文界面
        /// </summary>
        private void SetUILanguage()
        {
            CLanguage.LoadLanType();

            CLanguage.SetLanguage(this);
        }
        #endregion

        #region 全局消息
        /// <summary>
        /// 加载应用程序配置
        /// </summary>
        private void loadAppSetting()
        {
            //定义全局消息
            CUserApp.OnUserArgs.OnEvent += new COnAppEvent<CUserArgs>.OnEventHandler(OnUserArgs);
        }
        /// <summary>
        /// 全局消息触发
        /// object[0]:表示功能名称
        /// object[1]:表示功能状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserArgs(object sender, CUserArgs e)
        {
            try
            {
                if (e.lPara == (int)ElPara.保存)
                {
                    RefreshUISetting();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 刷新UI设置状态
        /// </summary>
        private void RefreshUISetting()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(RefreshUISetting));
            else
            {
                uiATE.ShowSetting(CGlobalPara.SysPara.Dev.SerIP + ":" +
                                  CGlobalPara.SysPara.Dev.SerPort.ToString(),
                                  CGlobalPara.SysPara.Para.Ate_Title_Name); 
            }
        }
        #endregion

        #region 构造函数
        public FrmMain()
        {
            InitializeComponent();

            LoadLanguge();

            InitialControl();

            SetDoubleBuffered();

        }
        #endregion

        #region 面板控件
        private udcStatFixture uiStat = null;
        private udcStatYield uiYield = null;
        private udcStatInfo uiATE = null;
        private udcRunLog tcpLog = null;
        private udcRunLog runLog = null;
        #endregion

        #region 面板回调函数
        private void FrmMain_Load(object sender, EventArgs e)
        {
            InitalMainWork();
        }
        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MainWork != null)
            {
                MainWork.CloseDlg();

                MainWork = null;
            }
        }
        private void OnDebugBtnClick(object sender, udcStatFixture.COnBtnClickArgs e)
        {
            try
            {
                if (MainWork == null)
                    return;

                if (e.idNo == 0)  //调试
                {
                    MainWork.OnFrmMainDebug(e.idNo, e.status);
                }
                else             //清除产能
                {
                    if (MessageBox.Show(CLanguage.Lan("确定要归零测试数量统计?"), "Tip", 
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    MainWork.OnFrmMainClearYield();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region  初始化方法
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {
            int uutMax = CKunXApp.SlotMax / CGlobalPara.SysPara.Dev.DevMax;

            uiATE = new udcStatInfo();
            uiATE.Dock = DockStyle.Fill;

            uiStat = new udcStatFixture(uutMax);
            uiStat.Dock = DockStyle.Fill;
            uiStat.OnBtnClick.OnEvent += new COnEvent<udcStatFixture.COnBtnClickArgs>.OnEventHandler(OnDebugBtnClick);

            uiYield = new udcStatYield(uutMax);
            uiYield.Dock = DockStyle.Fill;

            runLog = new udcRunLog();
            runLog.Dock = DockStyle.Fill;
            panel3.Controls.Add(runLog, 1, 0);

            tcpLog = new udcRunLog();
            tcpLog.Dock = DockStyle.Fill;
            tcpLog.mTitleEnable = false;
            tcpLog.mSaveEnable = false;

            panel2.Controls.Add(uiStat, 0, 0);

            panel2.Controls.Add(uiYield, 1, 0);

            tabPage1.Controls.Add(uiATE);

            tabPage2.Controls.Add(tcpLog);

        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this); 
        }
        #endregion

        #region 线程类
        private CMainWork MainWork = null;
        /// <summary>
        /// 初始化主线程类
        /// </summary>
        private void InitalMainWork()
        {
            MainWork = new CMainWork(0, "<ATE>", _fatherGuid);
            MainWork.OnUILogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnLog);
            MainWork.OnUIGlobalArgs.OnEvent += new COnEvent<CUIGlobalArgs>.OnEventHandler(OnGlobal);
            MainWork.OnUIInidcatorArgs.OnEvent += new COnEvent<CUIInicatorArgs>.OnEventHandler(OnIndicator); 
            MainWork.OnUISystemArgs.OnEvent += new COnEvent<CUIUserArgs<CUISystemArgs>>.OnEventHandler(OnSystem);
            MainWork.OnUIMainArgs.OnEvent += new COnEvent<CUIUserArgs<CUIMainArgs>>.OnEventHandler(OnMain);
            MainWork.OnTCPLogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnTcpLog);
            MainWork.InitialUI();
        }
        #endregion

        #region 线程类消息
        /// <summary>
        /// 用户UI消息
        /// </summary>
        private void OnGlobal(object sender, CUIGlobalArgs e)
        {
            RefreshUISetting();
        }
        /// <summary>
        /// 日志消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLog(object sender, CUILogArgs e)
        {
            ShowLog(e);
        }
        /// <summary>
        /// TCP日志
        /// </summary>
        /// <param name="sendrt"></param>
        /// <param name="e"></param>
        private void OnTcpLog(object sendrt, CUILogArgs e)
        {
            ShowTcpLog(e);
        }
        /// <summary>
        /// 显示运行状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIndicator(object sender, CUIInicatorArgs e)
        {
            string er = string.Empty;

            CReflect.SendWndMethod(_father, EMessType.OnShowStatus, out er, e.status);
        }
        /// <summary>
        /// 系统消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSystem(object sender, CUIUserArgs<CUISystemArgs> e)
        {
            ShowSystem(e);
        }
        /// <summary>
        /// 界面消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMain(object sender, CUIUserArgs<CUIMainArgs> e)
        {
            switch (e.model.DoRun)
            {
                case EUIStatus.空闲:
                    uiATE.SetFree();
                    break;
                case EUIStatus.TCP状态:
                    uiATE.SetTCPStatus(e.model.AlarmInfo, e.model.bAlarm);
                    break;
                case EUIStatus.治具到位:
                    uiATE.SetReady(e.model.AlarmInfo);
                    uiStat.SetReady(e.model.IdCard, e.model.SerialNo);
                    break;
                case EUIStatus.测试状态:
                    uiATE.SetTimes(e.model.WaitTime);
                    uiATE.SetStatus(e.model.AlarmInfo,e.model.bAlarm);
                    break;
                case EUIStatus.测试结束:
                    uiStat.SetEnd(e.model.Result, e.model.WaitTime);
                    uiATE.SetStatus(e.model.AlarmInfo, e.model.bAlarm);
                    break;
                case EUIStatus.产能计数:
                    uiYield.SetYield(e.model.TTNum, e.model.FailNum, e.model.SlotTTNum, e.model.SlotFailNum);
                    break;
                case EUIStatus.ATE状态:
                    uiATE.SetATEStatus(e.model.AlarmInfo, e.model.bAlarm);
                    break;
                case EUIStatus.ATE信息:
                    uiATE.SetATEInfo(e.model.ProName,e.model.ModeName,e.model.ElapsedTime);
                    break;
                case EUIStatus.调试模式:
                    uiStat.SetDebugMode(e.model.DebugMode);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 显示日志
        /// </summary>
        /// <param name="e"></param>
        private void ShowLog(CUILogArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUILogArgs>(ShowLog), e);
            else
            {
                runLog.Log(e.info, e.log, e.save);
            }
        }
        /// <summary>
        /// 显示日志
        /// </summary>
        /// <param name="e"></param>
        private void ShowTcpLog(CUILogArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUILogArgs>(ShowTcpLog), e);
            else
            {
                tcpLog.Log(e.info, e.log, e.save);
            }
        }
        /// <summary>
        /// 系统UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowSystem(CUIUserArgs<CUISystemArgs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUISystemArgs>>(ShowSystem), e);
            else
            {
                switch (e.model.DoRun)
                {
                    case EUISystem.空闲:
                        break;
                    case EUISystem.启动:
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

    }
}
