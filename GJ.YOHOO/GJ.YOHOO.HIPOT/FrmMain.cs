using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using GJ.COM;
using GJ.PLUGINS;
using GJ.UI;
using GJ.APP;
using GJ.YOHOO.HIPOT.Udc;
using GJ.USER.APP;
using GJ.DEV.RemoteIO;
using GJ.DEV.HIPOT;
using GJ.DEV.COM;
using GJ.USER.APP.MainWork;
using GJ.USER.APP.Iot;
using GJ.PDB;

namespace GJ.YOHOO.HIPOT
{
    public partial class FrmMain : Form, IChildMsg
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
            if (MessageBox.Show(CLanguage.Lan("确定要退出系统?"), CLanguage.Lan("退出系统"),
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
            if (name == "F6")
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

        #region 初始化方法
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {
            uiHpInfo = new udcHPInfo();
            uiHpInfo.Dock = DockStyle.Fill;
            uiHpInfo.OnBtnArgs.OnEvent += new COnEvent<udcHPInfo.COnBtnClickArgs>.OnEventHandler(OnHpInfoBtn);
            panel2.Controls.Add(uiHpInfo, 0, 0);

            uiHpResult = new udcHPResult(CYOHOOApp.SlotMax);
            uiHpResult.Dock = DockStyle.Fill;
            uiHpResult.Margin = new Padding(0);
            tabPage1.Controls.Add(uiHpResult);

            uiHpData = new udcHPData(CYOHOOApp.SlotMax);
            uiHpData.Dock = DockStyle.Fill;
            uiHpData.Margin = new Padding(0);
            tabPage2.Controls.Add(uiHpData);

            runLog = new udcRunLog();
            runLog.mSaveName = "RunLog";
            runLog.Dock = DockStyle.Fill;
            panel2.Controls.Add(runLog, 1, 0);

            tcpLog = new udcRunLog();
            tcpLog.mTitle = "HIPOT TCP/IP";
            tcpLog.mSaveName = "TcpLog";
            tcpLog.mSaveEnable = false;
            tcpLog.Dock = DockStyle.Fill;
            tcpLog.Margin = new Padding(0);
            panel3.Controls.Add(tcpLog, 0, 0);

            tcpShow = new udcTcpRecv();
            tcpShow.Dock = DockStyle.Fill;
            tcpShow.Margin = new Padding(0);
            panel3.Controls.Add(tcpShow, 1, 0);
        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
        }
        #endregion

        #region 面板控件
        private udcHPInfo uiHpInfo = null;
        private udcHPResult uiHpResult = null;
        private udcHPData uiHpData = null;
        private udcRunLog runLog = null;
        private udcRunLog tcpLog = null;
        private udcTcpRecv tcpShow = null;
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
        #endregion

        #region 面板消息
        /// <summary>
        /// HP选机种和调式按钮触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHpInfoBtn(object sender, udcHPInfo.COnBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            string er = string.Empty;

            switch (e.idNo)
            {
                case 0:   //调式高压机1
                    if (CGlobalPara.SysPara.Dev.HPDevMax > 0)
                        MainWork.OnFrmMainDebugMode(e.idNo, e.run);  
                    break;
                case 1:   //调式高压机2
                    if (CGlobalPara.SysPara.Dev.HPDevMax > 1)
                        MainWork.OnFrmMainDebugMode(e.idNo, e.run);  
                    break;
                case 2:   //选机种
                    string fileDirectry = string.Empty;
                    fileDirectry = CGlobalPara.SysPara.Report.ModelPath;
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.InitialDirectory = fileDirectry;
                    dlg.Filter = "hp files (*.hp)|*.hp";
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                    MainWork.OnFrmMainChangeModel(dlg.FileName);
                    break;
                case 3:
                    MainWork.OnFrmMainClearYield();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 面板方法
        /// <summary>
        /// 刷新UI设置状态
        /// </summary>
        private void RefreshUISetting()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(RefreshUISetting));
            else
            {
                uiHpInfo.SetMesStatus(CGlobalPara.SysPara.Mes.Connect);
            }
        }
        #endregion

        #region 线程类
        private CMainWork MainWork = null;
        /// <summary>
        /// 初始化主线程类
        /// </summary>
        private void InitalMainWork()
        {
            MainWork = new CMainWork(0, "<"+ CLanguage.Lan("高压测试工位") + ">", _fatherGuid);
            MainWork.OnUILogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnLog);
            MainWork.OnUIGlobalArgs.OnEvent += new COnEvent<CUIGlobalArgs>.OnEventHandler(OnGlobal);
            MainWork.OnUIInidcatorArgs.OnEvent += new COnEvent<CUIInicatorArgs>.OnEventHandler(OnIndicator); 
            MainWork.OnUISystemArgs.OnEvent += new COnEvent<CUIUserArgs<CUISystemArgs>>.OnEventHandler(OnSystem); 
            MainWork.OnUIModelArgs.OnEvent += new COnEvent<CUIModelArgs>.OnEventHandler(OnModel);
            MainWork.OnUIMainArgs.OnEvent += new COnEvent<CUIUserArgs<CUIMainArgs>>.OnEventHandler(OnMain);  
            MainWork.OnTCPLogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnTcpLog);
            MainWork.OnUIActionArgs.OnEvent += new COnEvent<CUIUserArgs<CUIActionAgrs>>.OnEventHandler(OnActionArgs);
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
        /// 机种UI消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModel(object sender, CUIModelArgs e)
        {
            ShowModel(e);
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
                case EUIStatus.设置状态:
                    uiHpInfo.SetStatus((udcHPInfo.ERun)e.model.RunStatus);
                    break;
                case EUIStatus.设置时间:
                    uiHpInfo.SetTimes(e.model.WatcherTime);
                    break;
                case EUIStatus.设置产能:
                    uiHpInfo.SetYield(e.model.TTNum, e.model.FailNum);
                    break;
                case EUIStatus.设置按钮:
                    uiHpInfo.SetDebugBtn(e.model.BtnNo, e.model.BtnValue);
                    break;
                case EUIStatus.治具就绪:
                    uiHpResult.SetFixtureId(e.model.IdCard);
                    break;
                case EUIStatus.设置条码:
                    uiHpResult.SetSnId(e.model.SlotNo,e.model.CurSn);
                    uiHpResult.SetFree(e.model.SlotNo);
                    uiHpData.SetFree(e.model.SlotNo);
                    break;
                case EUIStatus.设置结果:
                    uiHpResult.SetResult(e.model.SlotNo,e.model.Result);
                    uiHpData.SetTestVal(e.model.SlotNo, e.model.HPResult);
                    break;
                case EUIStatus.设置参数:
                    uiHpData.RefreshUI(e.model.Step);
                    break;          
                case EUIStatus.TCP界面:
                    ShowTcpUI(e.model.TcpReponse);
                    break;
                default:
                    break;
            }           
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
                        uiHpInfo.SetModelEnable(true);
                        break;
                    case EUISystem.启动:
                        uiHpInfo.SetModelEnable(false);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 显示机种
        /// </summary>
        /// <param name="e"></param>
        private void ShowModel(CUIModelArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIModelArgs>(ShowModel), e);
            else
            {
                if (e.model != null)
                {
                    uiHpInfo.LoadModel(e.model);

                    uiHpData.RefreshUI(e.model.Para.Step);
                }
            }
        }
        /// <summary>
        /// 显示TCP UI状态
        /// </summary>
        /// <param name="e"></param>
        private void ShowTcpUI(CSerReponse e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CSerReponse>(ShowTcpUI), e);
            else
            {
                tcpShow.SetStatus(e.Ready, 
                                  e.IdCard,
                                  e.ModelName, 
                                  e.SerialNos);
            }
        }
        /// <summary>
        /// 信息提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActionArgs(object sender, CUIUserArgs<CUIActionAgrs> e)
        {
            if (e.name == "Action")
            {
                ShowActionArgs(e);
            }
            else
            {
                ShowSetActionArgs(e);
            }
        }
        /// <summary>
        /// 提示信息
        /// </summary>
        /// <param name="e"></param>
        private void ShowActionArgs(CUIUserArgs<CUIActionAgrs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIActionAgrs>>(ShowActionArgs), e);
            else
            {
                if (!SFCS.FrmAction.IsAvalible && SFCS.FrmAction.AlarmFlag != 0)
                {
                    SFCS.FrmAction.CreateInstance().Show();

                    Application.DoEvents();
                }
            }
        }
        /// <summary>
        /// 提示报警信息
        /// </summary>
        /// <param name="e"></param>
        private void ShowSetActionArgs(CUIUserArgs<CUIActionAgrs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIActionAgrs>>(ShowSetActionArgs), e);
            else
            {
                if (SFCS.FrmAction.AlarmFlag == 0)
                {
                    SFCS.FrmAction.AlarmFlag = e.model.AlarmFlag;
                    SFCS.FrmAction.AlarmInfo = e.model.AlarmInfo;
                }
            }
        }
        #endregion

    }
}
