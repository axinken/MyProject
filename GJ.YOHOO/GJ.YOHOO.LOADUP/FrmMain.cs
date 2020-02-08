using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using GJ.PLUGINS;
using GJ.COM;
using GJ.APP;
using GJ.UI;
using GJ.USER.APP.MainWork;
using GJ.USER.APP.Iot;
using GJ.YOHOO.LOADUP.Udc;
using GJ.USER.APP;
using GJ.DEV.SafeDog;
using GJ.PDB;

namespace GJ.YOHOO.LOADUP
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
                this.WindowState = FormWindowState.Maximized;
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
            if (MainWork!=null)
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
            else if (name == "F7")
            {
                if (CYOHOOApp.DogLock == CYOHOOApp.DogRelease) //不检查加密狗
                    return;

                CSentinel dog = new CSentinel();

                string er = string.Empty;

                int leftDay = 0;

                string serialNo = string.Empty;

                if (!dog.check_safe_dog(CYOHOOApp.DogID, out leftDay, out serialNo, out er))
                {
                    MessageBox.Show(CLanguage.Lan("未找不到软件加密狗,请确认已插上加密狗?") + "\r\n" +
                                    CLanguage.Lan("错误信息") + ":" + er, CLanguage.Lan("软件加密狗"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                FrmDogLock.CreateInstance(serialNo).Show();            
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
            
            uiSnBand.LAN();

            uiModel.LAN();

            uiPreTest.LAN();

            uiRunTest.LAN();

            uiPassRate.LAN();

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

        #region 面板初始化
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(InitialControl));
            else
            {
                uiModel = new udcModelInfo();
                uiModel.OnBtnClick.OnEvent += new COnEvent<udcModelInfo.COnBtnClickArgs>.OnEventHandler(OnUIModelBtnArgsClick);
                uiModel.Dock = DockStyle.Fill;

                uiSnBand = new udcBandFixture(CYOHOOApp.FixPos);
                uiSnBand.Dock = DockStyle.Fill;
                uiSnBand.OnCheckArgs.OnEvent += new COnEvent<udcBandFixture.CCheckArgs>.OnEventHandler(OnUICheckSnArgs);
                uiSnBand.OnSnKeyPress.OnEvent += new COnEvent<GJ.YOHOO.LOADUP.Udc.udcBandFixture.CSnKeyArgs>.OnEventHandler(OnUISnKeyRecvArgs);
                uiSnBand.LoadUUT(CYOHOOApp.SlotMax);

                uiRunTest = new udcPreTest();
                uiRunTest.Dock = DockStyle.Fill;
                uiRunTest.OnBtnClickArgs.OnEvent += new COnEvent<udcPreTest.CBtnClickArgs>.OnEventHandler(OnUIYieldBtnArgsClick);
                uiRunTest.LoadUUT(CYOHOOApp.SlotMax);

                uiPreTest = new udcStatFixture(CYOHOOApp.FixPos);
                uiPreTest.Dock = DockStyle.Fill;
                uiPreTest.OnBtnClickArgs.OnEvent += new COnEvent<udcStatFixture.CBtnClickArgs>.OnEventHandler(OnUIFailConfirmArgsClick);
                uiPreTest.LoadUUT(CYOHOOApp.SlotMax);

                uiInBI = new udcStatFixture(CYOHOOApp.FixPos);
                uiInBI.Dock = DockStyle.Fill;
                uiInBI.LoadUUT(CYOHOOApp.SlotMax);

                uiPassRate = new udcPassRate(0, CLanguage.Lan("通电测试良率预警"));
                uiPassRate.Dock = DockStyle.Fill;
                uiPassRate.OnBtnClick.OnEvent += new COnEvent<udcPassRate.COnBtnClickArgs>.OnEventHandler(OnUIPassRateClick);  

                runLog = new udcRunLog();
                runLog.Dock = DockStyle.Fill;
                runLog.mTitleEnable = false;

                plcLog = new udcRunLog();
                plcLog.Dock = DockStyle.Fill;
                plcLog.mTitleEnable = false;
                plcLog.mTitle = "PLCLog";

                panel2.Controls.Add(uiSnBand, 0, 1);
                panel2.Controls.Add(uiModel, 0, 2);
                panel3.Controls.Add(uiPreTest, 0, 1);
                panel3.Controls.Add(uiRunTest, 0, 2);
                panel4.Controls.Add(uiInBI, 0, 1);
                tabPage1.Controls.Add(runLog);
                tabPage2.Controls.Add(plcLog);
                tabPage3.Controls.Add(uiPassRate); 
            }
        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
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
            }
        }
        #endregion

        #region 面板控件
        /// <summary>
        /// 运行日志
        /// </summary>
        private udcRunLog runLog = null;
        /// <summary>
        /// PLC报警列表
        /// </summary>
        private udcRunLog plcLog = null;
        /// <summary>
        /// 机种控件
        /// </summary>
        private udcModelInfo uiModel = null;
        /// <summary>
        /// 绑定控件
        /// </summary>
        private udcBandFixture uiSnBand = null;
        /// <summary>
        /// 测试控件
        /// </summary>
        private udcPreTest uiRunTest = null;
        /// <summary>
        /// 通电测试控件
        /// </summary>
        private udcStatFixture uiPreTest = null;
        /// <summary>
        /// 进老化控件
        /// </summary>
        private udcStatFixture uiInBI = null;
        /// <summary>
        /// 良率预警
        /// </summary>
        private udcPassRate uiPassRate = null;
        #endregion

        #region 面板消息
        /// <summary>
        /// 绑定控件消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUICheckSnArgs(object sender, udcBandFixture.CCheckArgs e)
        {
             if (MainWork == null)
                return;

             MainWork.OnFrmMainCheckSn(e.idNo, e.value);            
        }
        /// <summary>
        /// 绑定控件消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUISnKeyRecvArgs(object sender, udcBandFixture.CSnKeyArgs e)
        {
             if (MainWork == null)
                return;

            string serialNo = e.keyString;

            serialNo = serialNo.Replace("\r", "");

            serialNo = serialNo.Replace("\n", "");

            MainWork.OnSnKeyRecv(serialNo);
            
        }
        /// <summary>
        /// 机种控件消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUIModelBtnArgsClick(object sender, udcModelInfo.COnBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            string idCard = uiSnBand.IdCard;

            switch (e.btnNo)
            { 
                case udcModelInfo.EBtnNo.选机种:
                    string fileDirectry = string.Empty;
                    if (CGlobalPara.SysPara.Report.ModelPath != "")
                        fileDirectry = CGlobalPara.SysPara.Report.ModelPath;
                    else
                    {
                        fileDirectry = Application.StartupPath + "\\Model";
                        if (!Directory.Exists(fileDirectry))
                            Directory.CreateDirectory(fileDirectry);
                    }
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.InitialDirectory = fileDirectry;
                    dlg.Filter = "spec files (*.pwr)|*.pwr";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        MainWork.OnFrmMainSelectModel(dlg.FileName);
                    }
                    break;
                case udcModelInfo.EBtnNo.设置空治具:
                    MainWork.OnFrmMainSetEmptyFixture(e.value);  
                    break;
                case udcModelInfo.EBtnNo.次数归零:                    
                    if (idCard == "")
                    {
                        MessageBox.Show(CLanguage.Lan("请放置要清除数量治具就绪"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }
                    if (MessageBox.Show(CLanguage.Lan("确定要归零治具使用次数")+ "[" + idCard + "]?", "Tip", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes)
                        break;
                    MainWork.OnFrmMainClrFixtureUseNum(idCard); 
                    break;
                case udcModelInfo.EBtnNo.解除不良:
                    if (idCard == "")
                    {
                        MessageBox.Show(CLanguage.Lan("请放置要清除数量治具就绪"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }
                    if (MessageBox.Show(CLanguage.Lan("确定要解除治具不良次数") + "[" + idCard + "]?", "Tip", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes)
                        break;
                    MainWork.OnFrmMainClrFixtureUseNum(idCard);
                    break;
            }
        }
        /// <summary>
        /// 测试数据控件消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUIYieldBtnArgsClick(object sender, udcPreTest.CBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            if (e.idNo == 0)  //工位使用次数
            {
                MainWork.OnFrmMainClrStatUseNum();
            }
            else if (e.idNo == 1) //工位连续不良
            {
                MainWork.ClrStatFailNum();
            }
            else if (e.idNo == 2) //工位测试产能
            {
                MainWork.OnFrmMainClrYieldNum();
            }
        }
        /// <summary>
        /// 测试状态控件消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUIFailConfirmArgsClick(object sender, udcStatFixture.CBtnClickArgs e)
        {
            if (MainWork == null)
               return;

            if (e.idNo == 0)
                MainWork.OnFrmMainManualConfirmFail();
            else
                MainWork.OnFrmMainManualReset();
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

            if (e.lPara == 0)
            {
                if (MessageBox.Show(CLanguage.Lan("确定要归零良率统计数据?"), "Tip", 
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                
                MainWork.ClearPassRate(); 

            }
            else if (e.lPara == 1)
            {
                if (MessageBox.Show(CLanguage.Lan("确定要解除良率报警?"), "Tip",
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                MainWork.ClearPassRateAlarm(); 
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
                if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.无条码模式)
                {
                    labTitle1.Text = CLanguage.Lan("治具条码绑定位") + "【"+ CLanguage.Lan("无条码模式") +"】";
                }
                else if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工扫描模式)
                {
                    labTitle1.Text = CLanguage.Lan("治具条码绑定位") + "【" + CLanguage.Lan("人工扫描模式") + "】";
                }
                else if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工串口模式)
                {
                    labTitle1.Text = CLanguage.Lan("治具条码绑定位") + "【" + CLanguage.Lan("人工串口模式") + "】";
                }
                else
                {
                    labTitle1.Text =  CLanguage.Lan("治具条码绑定位") + "【"+ CLanguage.Lan("自动扫描模式") + "】";
                }

                if (CGlobalPara.SysPara.Mes.Connect)
                {
                    labTitle1.Text += "-【MES】";
                }

                if (MainWork != null)
                {
                    MainWork.SetPassRateLimit(CGlobalPara.SysPara.Alarm.ChkPassRate, CGlobalPara.SysPara.Alarm.PassRateLimit);  
                }
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
            MainWork = new CMainWork(0, "<"+ CLanguage.Lan("通电测试工位") + ">",_fatherGuid);
            MainWork.OnUILogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnLog);
            MainWork.OnPLCLogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnPLCLog);
            MainWork.OnUIGlobalArgs.OnEvent += new COnEvent<CUIGlobalArgs>.OnEventHandler(OnGlobal);
            MainWork.OnUIInidcatorArgs.OnEvent += new COnEvent<CUIInicatorArgs>.OnEventHandler(OnIndicator); 
            MainWork.OnUISystemArgs.OnEvent += new COnEvent<CUIUserArgs<CUISystemArgs>>.OnEventHandler(OnSystem);
            MainWork.OnUIActionArgs.OnEvent += new COnEvent<CUIUserArgs<CUIActionAgrs>>.OnEventHandler(OnActionArgs);
            MainWork.OnUIModelArgs.OnEvent += new COnEvent<CUIUserArgs<CModelPara>>.OnEventHandler(OnModel);
            MainWork.OnUIBandSnArgs.OnEvent += new COnEvent<CUIUserArgs<CStatHub>>.OnEventHandler(OnBandSn);
            MainWork.OnUIScanSnArgs.OnEvent += new COnEvent<CUIUserArgs<CUIScanSnArgs>>.OnEventHandler(OnScanSn);
            MainWork.OnUIStatHubArgs.OnEvent += new COnEvent<CUIUserArgs<CStat>>.OnEventHandler(OnStatHub);
            MainWork.OnUIStatTestArgs.OnEvent += new COnEvent<CUIUserArgs<CStat>>.OnEventHandler(OnStatTest);
            MainWork.OnUIInBIArgs.OnEvent += new COnEvent<CUIUserArgs<CStatHub>>.OnEventHandler(OnInBI);
            MainWork.OnUIStatDataArgs.OnEvent += new COnEvent<CUIUserArgs<CUIStatDataArgs>>.OnEventHandler(OnStatData);
            MainWork.OnUIPassRateArgs.OnEvent += new COnEvent<CUIUserArgs<CWarnRate>>.OnEventHandler(OnPassRate);

            MainWork.InitialUI();
        }
        #endregion

        #region 线程类消息
        /// <summary>
        /// 全局消息
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
        /// 机种UI消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModel(object sender, CUIUserArgs<CModelPara> e)
        {
            ShowModel(e);
        }
        /// <summary>
        /// 治具绑定位UI消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBandSn(object sender, CUIUserArgs<CStatHub> e)
        {
            ShowBandSn(e);
        }
        /// <summary>
        /// 扫描条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScanSn(object sender, CUIUserArgs<CUIScanSnArgs> e)
        {
            ShowScanSn(e);
        }
        /// <summary>
        /// 测试工站
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStatHub(object sender, CUIUserArgs<CStat> e)
        {
            ShowStatHub(e);
        }
        /// <summary>
        /// 测试工站
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStatTest(object sender, CUIUserArgs<CStat> e)
        {
            ShowStatTest(e);
        }
        /// <summary>
        /// 工位数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStatData(object sender, CUIUserArgs<CUIStatDataArgs> e)
        {
            ShowStatData(e);
        }
        /// <summary>
        /// 测试工站
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInBI(object sender, CUIUserArgs<CStatHub> e)
        {
            ShowInBI(e);
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
        /// 显示日志UI
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
                         uiModel.SetModelEnable(true);
                         uiModel.SetUIEnable(true);
                         uiModel.SetUseNumUI(true);
                         break;
                    case EUISystem.启动:
                         uiModel.SetModelEnable(false);
                         uiModel.SetUIEnable(false);
                         uiModel.SetUseNumUI(false);
                         break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 显示机种UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowModel(CUIUserArgs<CModelPara> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CModelPara>>(ShowModel), e);
            else
            {
                if (e.model != null)
                {
                    uiRunTest.Vmin = e.model.OutPut.Vmin;
                    uiRunTest.Vmax = e.model.OutPut.Vmax;
                    uiRunTest.Imin = e.model.OutPut.LoadMin;
                    uiRunTest.Imax = e.model.OutPut.LoadMax;
                    uiModel.ShowModel(e.model);
                }
            }
        }
        /// <summary>
        /// 显示治具绑定位UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowBandSn(CUIUserArgs<CStatHub> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CStatHub>>(ShowBandSn), e);
            else
            {
                switch (e.model.Para.DoRun)
                {
                    case ERUN.空闲:
                        uiSnBand.SetFree();
                        break;
                    case ERUN.读卡报警:
                        uiSnBand.SetIdAlarm();
                        break;
                    case ERUN.就绪:
                        uiSnBand.SetReady(e.model.Fixture.IdCard);
                        break;
                    case ERUN.绑定:
                        uiSnBand.SetSnTextEnable(CGlobalPara.SysPara.Mes.SnMode);
                        break;
                    case ERUN.等待:
                        uiSnBand.SetSnTextEnable(CGlobalPara.SysPara.Mes.SnMode);
                        break;
                    case ERUN.正常治具过站:
                        uiSnBand.SetEnd(e.model.Fixture.IdCard, e.model.Fixture.SerialNo);
                        break;
                    case ERUN.空治具过站:
                        uiSnBand.SetNull(e.model.Fixture.IdCard);
                        break;
                    case ERUN.点检治具过站:
                        uiSnBand.SetStatus(e.model.Para.AlarmInfo);
                        break;
                    case ERUN.异常报警:
                        uiSnBand.SetStatus(e.model.Para.AlarmInfo, true);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 显示扫描条码UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowScanSn(CUIUserArgs<CUIScanSnArgs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIScanSnArgs>>(ShowScanSn), e);
            else
            {
                switch (e.model.DoRun)
                {
                    case EUIScanSn.空闲:
                        break;
                    case EUIScanSn.文本聚焦:
                        uiSnBand.SetSnTextFouse(e.model.AlarmInfo,e.model.bAlarm);
                        break;
                    case EUIScanSn.设置条码:
                        uiSnBand.SetSn(e.model.SlotNo,e.model.SerialNo,e.model.AlarmInfo,e.model.bAlarm);
                        break;
                    case EUIScanSn.绑定完毕:
                        uiSnBand.SetStatus(e.model.AlarmInfo);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 测试工位UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowStatHub(CUIUserArgs<CStat> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CStat>>(ShowStatHub), e);
            else
            {
                switch (e.model.hub.Para.DoRun)
                {
                    case ERUN.空闲:
                        break;
                    case ERUN.读卡报警:
                        uiPreTest.SetIdAlarm();
                        break;
                    case ERUN.异常报警:
                        uiPreTest.SetStatus(e.model.hub.Para.AlarmInfo,true);
                        break;
                    case ERUN.空治具过站:
                        uiPreTest.SetNull(e.model.hub.Fixture.IdCard);
                        break;
                    case ERUN.正常治具过站:
                        uiPreTest.SetStatus(e.model.hub.Para.AlarmInfo, false);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 测试工位UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowStatTest(CUIUserArgs<CStat> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CStat>>(ShowStatTest), e);
            else
            {
                if (e.lPara == 0)
                {
                    switch (e.model.test.Para.DoRun)
                    {
                        case ERUN.空闲:
                            uiPreTest.SetFree();
                            break;
                        case ERUN.读卡报警:
                            uiPreTest.SetIdAlarm();
                            break;
                        case ERUN.异常报警:
                            uiPreTest.SetStatus(e.model.test.Para.AlarmInfo, true);
                            break;
                        case ERUN.正常治具过站:
                            uiPreTest.SetEnd(e.model.test.Fixture.Result, e.model.test.Para.TestTime);
                            break;
                        case ERUN.到位:
                            uiPreTest.SetReady(e.model.test.Fixture.IdCard, e.model.test.Fixture.SerialNo);
                            break;
                        case ERUN.等待:
                            uiPreTest.SetStatus(e.model.test.Para.AlarmInfo, false);
                            break;
                        case ERUN.就绪:
                            uiPreTest.SetStatus(e.model.test.Para.AlarmInfo, false);
                            break;
                        case ERUN.测试:
                            break;
                        default:
                            break;
                    }
                }
                else if (e.lPara == 1)
                {
                    uiPreTest.SetEnd(e.model.test.Fixture.Result, e.model.test.Para.TestTime);
                }
            }
        }
        /// <summary>
        /// 测试工位数据
        /// </summary>
        /// <param name="e"></param>
        private void ShowStatData(CUIUserArgs<CUIStatDataArgs> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CUIStatDataArgs>>(ShowStatData), e);
            else
            {
                switch (e.model.DoRun)
                {
                    case EUIStatData.空闲:
                        uiRunTest.SetFree();
                        break;
                    case EUIStatData.使用次数:
                        uiRunTest.SetUseTimes(e.model.UseNum);
                        break;
                    case EUIStatData.产能统计:
                        uiRunTest.SetTestNum(e.model.TTNum, e.model.FailNum);
                        uiRunTest.SetFailTimes(e.model.ConFailNum);
                        break;
                    case EUIStatData.测试信息:
                        uiRunTest.ShowData(e.model.SerialNo,e.model.V,e.model.I,
                                           e.model.DD,e.model.TestTime,e.model.TestEnd);
                        break;
                    case EUIStatData.设定电压:
                        uiRunTest.SetACV(e.model.ACV.ToString(),e.model.ACFlag);
                        break;
                    case EUIStatData.监控时间:
                        uiRunTest.SetMonTime(e.model.MonTime);
                        break;
                    case EUIStatData.确定不良:
                        uiPreTest.SetFailCfg();
                        break;
                    case EUIStatData.取消确定:
                        uiPreTest.DisFailCfg();
                        break;
                    case EUIStatData.状态提示:
                        uiPreTest.SetStatus(e.model.AlarmInfo,e.model.bAlarm);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 测试工位UI
        /// </summary>
        /// <param name="e"></param>
        private void ShowInBI(CUIUserArgs<CStatHub> e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIUserArgs<CStatHub>>(ShowInBI), e);
            else
            {
                switch (e.model.Para.DoRun)
                {
                    case ERUN.空闲:
                        uiInBI.SetFree();
                        break;
                    case ERUN.读卡报警:
                        uiInBI.SetIdAlarm();
                        break;
                    case ERUN.异常报警:
                        uiInBI.SetStatus(e.model.Para.AlarmInfo, true);
                        break;
                    case ERUN.空治具过站:
                        uiInBI.SetNull(e.model.Fixture.IdCard);
                        break;
                    case ERUN.正常治具过站:
                        uiInBI.SetEnd(e.model.Fixture.IdCard,e.model.Fixture.SerialNo,e.model.Fixture.Result);
                        break;
                    default:
                        break;
                }
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
                if (e.model.bAlarm==1)
                {
                    tabControl1.SelectTab(2);  
                }
                uiPassRate.SetYield(e.model);
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
