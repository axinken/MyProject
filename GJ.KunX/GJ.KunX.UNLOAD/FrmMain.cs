using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Threading;
using System.Configuration;
using System.Diagnostics; 
using System.IO;
using System.Reflection;
using GJ.KunX.UNLOAD.Udc;
using GJ.COM;
using GJ.PLUGINS;
using GJ.MES;
using GJ.UI;
using GJ.PDB;
using GJ.APP;
using GJ.USER.APP;
using GJ.DEV.COM; 
using GJ.DEV.PLC;
using GJ.DEV.CARD;
using GJ.USER.APP.MainWork;
using GJ.USER.APP.Iot;

namespace GJ.KunX.UNLOAD
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
            if (MessageBox.Show(CLanguage.Lan("确定要退出系统?"),CLanguage.Lan("退出系统"),
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
                    panel2.ColumnStyles[0].Width =100;
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

            uiHipot.LAN();

            uiATE.LAN();

            uiFinal.LAN();

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

        #region 面板控件
        /// <summary>
        /// 运行日志
        /// </summary>
        private udcRunLog runLog = null;
        /// <summary>
        /// PLC日志
        /// </summary>
        private udcRunLog plcLog = null;
        /// <summary>
        /// 高压测试控件
        /// </summary>
        private udcStatFixture uiHipot = null;
        /// <summary>
        /// ATE测试控件
        /// </summary>
        private udcStatFixture uiATE = null;
        /// <summary>
        /// 下机位控件
        /// </summary>
        private udcStatResult uiFinal = null;
        /// <summary>
        /// 工位饼图列表
        /// </summary>
        private Dictionary<string, udcStatYieldChart> uiYield = null;
        /// <summary>
        /// 良率预警
        /// </summary>
        private udcPassRate[] uiPassRate = null;
        #endregion

        #region 面板初始化
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {
            runLog = new udcRunLog();
            runLog.Dock = DockStyle.Fill;
            runLog.mTitleEnable = false;

            plcLog = new udcRunLog();
            plcLog.Dock = DockStyle.Fill;
            plcLog.mTitle = "PLCLog";
            plcLog.mTitleEnable = false;

            uiPassRate = new udcPassRate[2];

            uiPassRate[0] = new udcPassRate(0, CLanguage.Lan("高压测试良率预警"));
            uiPassRate[0].Dock = DockStyle.Fill;
            uiPassRate[0].OnBtnClick.OnEvent += new COnEvent<udcPassRate.COnBtnClickArgs>.OnEventHandler(OnUIPassRateClick);

            uiPassRate[1] = new udcPassRate(1, CLanguage.Lan("ATE测试良率预警"));
            uiPassRate[1].Dock = DockStyle.Fill;
            uiPassRate[1].OnBtnClick.OnEvent += new COnEvent<udcPassRate.COnBtnClickArgs>.OnEventHandler(OnUIPassRateClick); 

            tabPage1.Controls.Add(runLog);
            tabPage2.Controls.Add(plcLog);
            tabPage3.Controls.Add(uiPassRate[0]);
            tabPage4.Controls.Add(uiPassRate[1]);

            uiHipot = new udcStatFixture(0,"HIPOT",CKunXApp.SlotMax, CKunXApp.FixPos);
            uiHipot.OnBtnClickArgs.OnEvent += new COnEvent<udcStatFixture.CBtnClickArgs>.OnEventHandler(OnFailConfigArgs); 
            uiHipot.Dock = DockStyle.Fill;
            panel4.Controls.Add(uiHipot, 0, 1);

            uiATE = new udcStatFixture(1,"ATE", CKunXApp.SlotMax, CKunXApp.FixPos);
            uiATE.OnBtnClickArgs.OnEvent += new COnEvent<udcStatFixture.CBtnClickArgs>.OnEventHandler(OnFailConfigArgs); 
            uiATE.Dock = DockStyle.Fill;
            panel5.Controls.Add(uiATE, 0, 1);

            uiFinal = new udcStatResult(CKunXApp.SlotMax, CKunXApp.FixPos, 
                                                            new List<int> { CKunXApp.ATE_FlowId, CKunXApp.HIPOT_FlowId,
                                                                            CKunXApp.BI_FlowId,CKunXApp.PRETEST_FlowId,
                                                                           });
            uiFinal.OnBtnClickArgs.OnEvent += new COnEvent<udcStatResult.CBtnClickArgs>.OnEventHandler(OnYieldBtnClick);

            uiFinal.Dock = DockStyle.Fill;
            panel7.Controls.Add(uiFinal, 0, 1);

            uiYield = new Dictionary<string, udcStatYieldChart>();

            uiYield.Add(CKunXApp.PRETEST_FlowName, new udcStatYieldChart(CKunXApp.PRETEST_FlowId, CKunXApp.PRETEST_FlowName));

            uiYield.Add(CKunXApp.BI_FlowName, new udcStatYieldChart(CKunXApp.BI_FlowId, CKunXApp.BI_FlowName));

            uiYield.Add(CKunXApp.HIPOT_FlowName, new udcStatYieldChart(CKunXApp.HIPOT_FlowId, CKunXApp.HIPOT_FlowName));

            uiYield.Add(CKunXApp.ATE_FlowName, new udcStatYieldChart(CKunXApp.ATE_FlowId, CKunXApp.ATE_FlowName));

            foreach (var keyName in uiYield.Keys)
                uiYield[keyName].Dock = DockStyle.Fill;

            panel9.Controls.Add(uiYield[CKunXApp.PRETEST_FlowName], 0, 0);

            panel9.Controls.Add(uiYield[CKunXApp.BI_FlowName], 1, 0);

            panel9.Controls.Add(uiYield[CKunXApp.HIPOT_FlowName], 0, 1);

            panel9.Controls.Add(uiYield[CKunXApp.ATE_FlowName], 1, 1);

        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
        }       
        #endregion

        #region 面板消息
        /// <summary>
        /// 产能清除消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnYieldBtnClick(object sender, udcStatResult.CBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            if (e.idNo == 0) //清除测试工位统计
            {
                MainWork.OnFrmMainClrStationYield();
            }
            else if (e.idNo == 1) //清除流程工位统计
            {
                MainWork.OnFrmMianClrFlowYield(); 
            }
        }
        /// <summary>
        /// 不良确认消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFailConfigArgs(object sender, udcStatFixture.CBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            MainWork.OnFrmMainFailConfig(e.name);
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
                if (!CGlobalPara.SysPara.Mes.Connect)
                    labUnload.Text = CLanguage.Lan("人工/自动下机位");
                else
                    labUnload.Text = CLanguage.Lan("人工/自动下机位") + "【MES】";

                if (MainWork != null)
                {
                    MainWork.OnFrmMainSystemUpdate();

                    MainWork.SetPassRateLimit(0,CGlobalPara.SysPara.Alarm.ChkPassRate[0], CGlobalPara.SysPara.Alarm.PassRateLimit[0]);

                    MainWork.SetPassRateLimit(1, CGlobalPara.SysPara.Alarm.ChkPassRate[1], CGlobalPara.SysPara.Alarm.PassRateLimit[1]); 
                }
            }
        }
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
        private void labUnload_DoubleClick(object sender, EventArgs e)
        {
            FrmSerTCP.CreateInstance().Show();
        }
        private void btnChangeModel_Click(object sender, EventArgs e)
        {
            if (MainWork == null)
                return;

            MainWork.OnFrmMainChangeModel();

            panel10.Visible = false; 
        }
        /// <summary>
        /// 良率预警按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUIPassRateClick(object sender, udcPassRate.COnBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            string[] names = new string[] { "【"+ CLanguage.Lan("高压工位") + "】", "【"+ CLanguage.Lan("ATE工位") + "】" };

            if (e.lPara == 0)
            {
                if (MessageBox.Show(CLanguage.Lan("确定要归零良率统计数据") + names[e.idNo] + "?", "Tip",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                MainWork.ClearPassRate(e.idNo);

            }
            else if (e.lPara == 1)
            {
                if (MessageBox.Show(CLanguage.Lan("确定要解除良率报警") + names[e.idNo] + "?", "Tip",
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                MainWork.ClearPassRateAlarm(e.idNo);
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
            MainWork = new CMainWork(0, "<" + CLanguage.Lan("人工/自动下机位") + ">", _fatherGuid);
            MainWork.OnUILogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnLog);
            MainWork.OnPLCLogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnPLCLog);
            MainWork.OnUIGlobalArgs.OnEvent += new COnEvent<CUIGlobalArgs>.OnEventHandler(OnGlobal);
            MainWork.OnUIInidcatorArgs.OnEvent += new COnEvent<CUIInicatorArgs>.OnEventHandler(OnIndicator); 
            MainWork.OnUISystemArgs.OnEvent += new COnEvent<CUIUserArgs<CUISystemArgs>>.OnEventHandler(OnSystem);
            MainWork.OnUIMainArgs.OnEvent += new COnEvent<CUIUserArgs<CUIMainArgs>>.OnEventHandler(OnMain);
            MainWork.OnUIPassRateArgs.OnEvent += new COnEvent<CUIUserArgs<CWarnRate>>.OnEventHandler(OnPassRate);
            MainWork.InitialUI();
        }
        /// <summary>
        /// 同步锁
        /// </summary>
        private object UILock = new object();
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
        /// 报警日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPLCLog(object sender, CUILogArgs e)
        {
            ShowPLCLog(e);
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
            lock (UILock)
            {
                if (e.name == CKunXApp.HIPOT_FlowName)
                {
                    ShowHPUI(e);
                }
                else if (e.name == CKunXApp.ATE_FlowName)
                {
                    ShowATEUI(e);
                }
                else if (e.name == CKunXApp.UNLOAD_FlowName)
                {
                    ShowUnLoadUI(e);
                }
                else
                {
                    switch (e.model.DoRun)
                    {
                        case EUIStatus.产能计数:
                            uiFinal.SetTestNum(e.idNo, e.model.TTNum, e.model.FailNum);
                            break;
                        case EUIStatus.工位计数:
                            uiFinal.SetFlowNum(e.model.Yields);
                            break;
                        case EUIStatus.显示计数:
                            uiYield[e.model.YieldKey].SetYield(e.model.Yield);
                            break;
                        case EUIStatus.变更机种:
                            ShowModelChange(e.model.Info);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 良率预警
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPassRate(object sender, CUIUserArgs<CWarnRate> e)
        {
            ShowPassRate(e);
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
        /// 显示报警日志UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowPLCLog(CUILogArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUILogArgs>(ShowPLCLog), e);
            else
            {
                plcLog.Log(e.info, e.log, e.save);
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
        /// <summary>
        /// 测试工位UI显示
        /// </summary>
        /// <param name="e"></param>
        private void ShowHPUI(CUIUserArgs<CUIMainArgs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIMainArgs>>(ShowHPUI), e);
            else
            {
                switch (e.model.StatHP[e.idNo].UIDoRun)
                {
                    case EUIStatus.空闲:
                        //uiHipot.SetFree();
                        break;
                    case EUIStatus.状态信息:
                        uiHipot.SetInfo(e.model.StatHP[e.idNo].Info);
                        break;
                    case EUIStatus.读卡报警:
                        break;
                    case EUIStatus.异常报警:
                        uiHipot.SetAlarm(e.model.StatHP[e.idNo].Info);
                        break;
                    case EUIStatus.治具到位:
                        uiHipot.SetReady(e.model.StatHP[e.idNo].IdCard, e.model.StatHP[e.idNo].SerialNo);
                        break;
                    case EUIStatus.空治具过站:
                        uiHipot.SetNull(e.model.StatHP[e.idNo].IdCard);
                        break;
                    case EUIStatus.测试中:
                        if (e.model.StatHP[e.idNo].SideIndex == -1)
                            uiHipot.SetTesting();
                        else
                            uiHipot.SetTesting(e.idNo);
                        break;
                    case EUIStatus.测试结束:
                        if (e.model.StatHP[e.idNo].SideIndex == -1)
                            uiHipot.SetEnd(e.model.StatHP[e.idNo].Result);
                        else
                            uiHipot.SetEnd(e.model.StatHP[e.idNo].Result, e.model.StatHP[e.idNo].SideIndex);
                        break;
                    case EUIStatus.不良确认:
                        uiHipot.SetFailCfg();
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 测试工位UI显示
        /// </summary>
        /// <param name="e"></param>
        private void ShowATEUI(CUIUserArgs<CUIMainArgs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIMainArgs>>(ShowATEUI), e);
            else
            {
                switch (e.model.StatATE[e.idNo].UIDoRun)
                {
                    case EUIStatus.空闲:
                        //uiATE.SetFree();
                        break;
                    case EUIStatus.状态信息:
                        uiATE.SetInfo(e.model.StatATE[e.idNo].Info);
                        break;
                    case EUIStatus.读卡报警:
                        break;
                    case EUIStatus.异常报警:
                        uiATE.SetAlarm(e.model.StatATE[e.idNo].Info);
                        break;
                    case EUIStatus.治具到位:
                        uiATE.SetReady(e.model.StatATE[e.idNo].IdCard, e.model.StatATE[e.idNo].SerialNo);
                        break;
                    case EUIStatus.空治具过站:
                        uiATE.SetNull(e.model.StatATE[e.idNo].IdCard);
                        break;
                    case EUIStatus.测试中:
                        if (e.model.StatATE[e.idNo].SideIndex == -1)
                            uiATE.SetTesting();
                        else
                            uiATE.SetTesting(e.model.StatATE[e.idNo].SideIndex);
                        break;
                    case EUIStatus.测试结束:
                        if (e.model.StatATE[e.idNo].SideIndex == -1)
                            uiATE.SetEnd(e.model.StatATE[e.idNo].Result);
                        else
                            uiATE.SetEnd(e.model.StatATE[e.idNo].Result, e.model.StatATE[e.idNo].SideIndex);
                        break;
                    case EUIStatus.不良确认:
                        uiATE.SetFailCfg();
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 下机位UI显示
        /// </summary>
        /// <param name="e"></param>
        private void ShowUnLoadUI(CUIUserArgs<CUIMainArgs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIMainArgs>>(ShowUnLoadUI), e);
            else
            {
                switch (e.model.StatUnLoad.UIDoRun)
                {
                    case EUIStatus.空闲:
                        uiFinal.SetFree(); 
                        break;
                    case EUIStatus.治具到位:
                        break;
                    case EUIStatus.空治具过站:
                        uiFinal.SetIsNull(e.model.StatUnLoad.IdCard); 
                        break;
                    case EUIStatus.状态信息:
                        uiFinal.SetInfo(e.model.StatUnLoad.Info);  
                        break;
                    case EUIStatus.读卡报警:
                        uiFinal.SetIdAlarm();
                        break;
                    case EUIStatus.异常报警:
                        uiFinal.SetAlarm(e.model.StatUnLoad.Info); 
                        break;
                    case EUIStatus.测试结束:
                        uiFinal.SetResult(e.model.StatUnLoad.IdCard, e.model.StatUnLoad.SerialNo, e.model.StatUnLoad.Result, e.model.StatUnLoad.ResultId);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 显示换机种信息
        /// </summary>
        /// <param name="info"></param>
        private void ShowModelChange(string info)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(ShowModelChange), info);
            else
            {
                labModelChange.Text = info;
                panel10.Visible = true;
                panel10.BringToFront();
            }
        }
        /// <summary>
        /// 显示良率
        /// </summary>
        /// <param name="e"></param>
        private void ShowPassRate(CUIUserArgs<CWarnRate> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CWarnRate>>(ShowPassRate), e);
            else
            {
                 if (e.model.bAlarm == 1)
                 {
                    tabControl1.SelectTab(2 + e.idNo);
                 }
                 uiPassRate[e.idNo].SetYield(e.model);
            }
        }
        #endregion

    }
}
