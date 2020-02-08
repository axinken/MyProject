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
using System.IO;
using System.Diagnostics;
using GJ.APP;
using GJ.USER.APP; 
using GJ.COM;
using GJ.PLUGINS;
using GJ.UI;
using GJ.PDB;
using GJ.DEV;
using GJ.KunX.BURNIN.Udc;
using GJ.DEV.CARD;
using GJ.DEV.FCMB;
using GJ.DEV.ERS;
using GJ.MES;
using GJ.DEV.PLC;
using GJ.DEV.LED;
using GJ.USER.APP.MainWork;
using GJ.USER.APP.Iot;

namespace GJ.KunX.BURNIN
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

                TmrStatus.Enabled = true;
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

                ShowAction(new CUIActionArgs(CLanguage.Lan("软件已启动,等待系统检测状态"),Color.Black));
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
            
            ShowAction(new CUIActionArgs(CLanguage.Lan("软件未启动运行,请启动软件运行."),Color.Red));

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
                    splitContainer1.Panel2Collapsed = true;
                else
                    splitContainer1.Panel2Collapsed = false;
                _maxSize = !_maxSize;
            }
            else if (name == "F7")
            {
                if (MainWork != null)
                {
                    MainWork.OnFrmMainShowONOFFChart();
                }
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

            uiModel.LAN();

            uiSignal.LAN();

            uiSignal.LAN();

            uiOutPut.LAN();

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

            CreatLogControl();

            CreateMainFrame();

            CreateMontherControl();

            CreateChildControl();

            CreateInfoControl();

            InitialControl();

            SetDoubleBuffered();
        }
        #endregion

        #region 面板初始化
        private void CreatLogControl()
        {
            runLog = new udcRunLog();
            runLog.Dock = DockStyle.Fill;
            runLog.mTitleEnable = false;

            plcLog = new udcRunLog();
            plcLog.Dock = DockStyle.Fill;
            plcLog.mTitle = "PLCLog";
            plcLog.mTitleEnable = false;

            uiPassRate = new udcPassRate(0, CLanguage.Lan("老化测试良率预警"));
            uiPassRate.Dock = DockStyle.Fill;
            uiPassRate.OnBtnClick.OnEvent += new COnEvent<udcPassRate.COnBtnClickArgs>.OnEventHandler(OnUIPassRateClick);

            tabPage2.Controls.Add(runLog);
            tabPage3.Controls.Add(plcLog);
            tabPage4.Controls.Add(uiPassRate); 
        }
        /// <summary>
        /// 创建UI主框架
        /// </summary>
        private void CreateMainFrame()
        {
            try
            {
                //加载行数标题栏
                panelTilte = new TableLayoutPanel();
                panelTilte.Dock = DockStyle.Fill;
                panelTilte.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                panelTilte.Margin = new Padding(0);
                panelTilte.GetType().GetProperty("DoubleBuffered",
                                        System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.NonPublic)
                                        .SetValue(panelTilte, true, null);
                panelTilte.RowCount = CGlobalPara.C_ROW_MAX;
                for (int i = 0; i < CGlobalPara.C_ROW_MAX; i++)
                    panelTilte.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelTilte.ColumnCount = 1;
                panelTilte.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                for (int i = 0; i < CGlobalPara.C_ROW_MAX; i++)
                {
                    Label labRow = new Label();
                    labRow.Name = "labRowTilte" + i.ToString();
                    labRow.Text = "L" + (i + 1).ToString();
                    labRow.Dock = DockStyle.Fill;
                    labRow.TextAlign = ContentAlignment.MiddleCenter;
                    if (labRow.Text.Length == 2)
                        labRow.Font = new Font("宋体", 15);
                    else
                        labRow.Font = new Font("宋体", 12);
                    labRow.Margin = new Padding(0);
                    panelTilte.Controls.Add(labRow, 0, i);
                }

                panel1.Controls.Add(panelTilte, 0, 0);

                //加载主界面分隔线
                panelMain = new TableLayoutPanel[CGlobalPara.C_ROOM_MAX];

                for (int rom = 0; rom < panelMain.Length; rom++)
                {
                    panelMain[rom] = new TableLayoutPanel();
                    panelMain[rom].Dock = DockStyle.Fill;
                    panelMain[rom].CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                    panelMain[rom].Margin = new Padding(0);
                    panelMain[rom].GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panelMain[rom], true, null);

                    panelMain[rom].RowCount = CGlobalPara.C_ROW_MAX * 2;

                    for (int i = 0; i < CGlobalPara.C_ROW_MAX; i++)
                    {
                        panelMain[rom].RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
                        panelMain[rom].RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    }

                    panelMain[rom].ColumnCount = CGlobalPara.C_COL_MAX;

                    for (int i = 0; i < CGlobalPara.C_COL_MAX; i++)
                        panelMain[rom].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                    panel1.Controls.Add(panelMain[rom], 1 + rom, 0);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 创建UI库位母治具控件
        /// </summary>
        private void CreateMontherControl()
        {
            try
            {
                //初始化母治具

                panelFix = new List<TableLayoutPanel>();

                int mSLot = 0;

                for (int rom = 0; rom < CGlobalPara.C_ROOM_MAX; rom++)
                {
                    for (int i = 0; i < CGlobalPara.C_ROW_MAX; i++)
                    {
                        for (int j = 0; j < CGlobalPara.C_COL_MAX; j++)
                        {
                            Label labSlot = new Label();
                            labSlot.Name = "labSlot" + mSLot.ToString();
                            if (CKunXApp.ColPos == 0)
                            {
                                labSlot.Text = (j + 1).ToString("D2");
                            }
                            else
                            {
                                labSlot.Text = (CGlobalPara.C_COL_MAX - j).ToString("D2");
                            }
                            labSlot.Dock = DockStyle.Fill;
                            labSlot.TextAlign = ContentAlignment.MiddleCenter;
                            labSlot.Font = new Font("宋体", 9);
                            labSlot.Margin = new Padding(0);
                            labFixSlot.Add(labSlot);

                            TableLayoutPanel panel = new TableLayoutPanel();
                            panel.Dock = DockStyle.Fill;
                            panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                            panel.Margin = new Padding(1);
                            panel.GetType().GetProperty("DoubleBuffered",
                                                    System.Reflection.BindingFlags.Instance |
                                                    System.Reflection.BindingFlags.NonPublic)
                                                    .SetValue(panel, true, null);
                            panel.RowCount = 1;
                            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                            panel.ColumnCount = 2;
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                            panelFix.Add(panel);
                            mSLot += 1;
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 创建UI库位子治具控件
        /// </summary>
        private void CreateChildControl()
        {
            //加载子治具
            for (int i = 0; i < CGlobalPara.C_UUT_MAX; i++)
            {
                udcFixture udcSlot = new udcFixture(i,CKunXApp.FixPos);
                udcSlot.Dock = DockStyle.Fill;
                udcSlot.Margin = new Padding(1);
                udcSlot.menuClick.OnEvent += new COnEvent<udcFixture.CSetMenuArgs>.OnEventHandler(OnUIMenuClick);
                uiUUT.Add(udcSlot);
            }
        }
        /// <summary>
        /// 创建UI库位信息控件
        /// </summary>
        private void CreateInfoControl()
        {
            try
            {
                uiModel = new udcModelBase();
                uiModel.OnBtnClick.OnEvent += new COnEvent<udcModelBase.COnBtnClickArgs>.OnEventHandler(OnUIModelBtnClick);
                uiModel.Dock = DockStyle.Fill;
                panel2.Controls.Add(uiModel, 0, 1);

                uiOutPut = new udcOutput();
                uiOutPut.Dock = DockStyle.Fill;
                panel2.Controls.Add(uiOutPut, 0, 3);

                uiSignal = new udcSignal();
                uiSignal.OnUIArgs.OnEvent += new COnEvent<udcSignal.CUIArgs>.OnEventHandler(OnUISignalArgs);
                uiSignal.Dock = DockStyle.Fill;
                panel2.Controls.Add(uiSignal, 0, 5);

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(InitialControl));
            else
            {
                //加载标题及母治具控件

                int mSLot = 0;

                for (int rom = 0; rom < CGlobalPara.C_ROOM_MAX; rom++)
                {
                    for (int i = 0; i < CGlobalPara.C_ROW_MAX; i++)
                    {
                        for (int j = 0; j < CGlobalPara.C_COL_MAX; j++)
                        {
                            if (CKunXApp.ColPos == 0)
                            {
                                panelMain[rom].Controls.Add(labFixSlot[mSLot], j, i * 2);
                                panelMain[rom].Controls.Add(panelFix[mSLot], j, i * 2 + 1);
                            }
                            else
                            {
                                panelMain[rom].Controls.Add(labFixSlot[mSLot], j, i * 2);
                                panelMain[rom].Controls.Add(panelFix[mSLot], CGlobalPara.C_COL_MAX - j - 1, i * 2 + 1);
                            }
                            mSLot += 1;
                        }
                    }
                }

                ////加载子治具控件

                for (int i = 0; i < CGlobalPara.C_UUT_MAX / 2; i++)
                {
                    int idNo = i * 2;

                    if (CKunXApp.ColPos == 0)
                    {
                        panelFix[i].Controls.Add(uiUUT[idNo], 0, 0);
                        panelFix[i].Controls.Add(uiUUT[idNo + 1], 1, 0);
                    }
                    else
                    {
                        panelFix[i].Controls.Add(uiUUT[idNo], 1, 0);
                        panelFix[i].Controls.Add(uiUUT[idNo + 1], 0, 0);
                    }
                }


                int bSel = System.Convert.ToInt32(CIniFile.ReadFromIni("OutModel", "CurSelOut", CGlobalPara.IniFile, "0"));

                chkSelOut.Checked = (bSel == 1 ? true : false);

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

                MainWork = null;
            }
        }
        private void chkSelOut_CheckedChanged(object sender, EventArgs e)
        {
            if (MainWork == null)
               return;
            MainWork.ChkSelOut_CheckedChanged(chkSelOut.Checked);            
        }
        private void cmbModelList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainWork == null)
                return;
            MainWork.RefreshCurOutModel(cmbModelList.Text);
        }
        private void TmrStatus_Tick(object sender, EventArgs e)
        {
            labAction.Left += 10;

            if (labAction.Left >= panel4.Width - labAction.Width)
            {
                labAction.Left = 0;
            }
        }
        #endregion

        #region 面板消息
        /// <summary>
        /// 基本信息消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUIModelBtnClick(object sender, udcModelBase.COnBtnClickArgs e)
        {
            if (MainWork == null)
                return;

            switch (e.btnNo)
            {
                case udcModelBase.EBtnNo.选机种:
                    string fileDirectry = string.Empty;
                    fileDirectry = CGlobalPara.SysPara.Report.ModelPath;
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.InitialDirectory = fileDirectry;
                    dlg.Filter = "BI files (*.bi)|*.bi";
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                    MainWork.OnFrmMainSelectModel(dlg.FileName); 
                    break;
                case udcModelBase.EBtnNo.温度显示:
                    MainWork.OnFrmMainTempShow();
                    break;
                default:
                    break;
            }
        }
        private void OnUISignalArgs(object sender, udcSignal.CUIArgs e)
        {
            if (MainWork == null)
                return;

            MainWork.OnFrmMainArgs(e.idNo, e.name, e.lPara, e.wPara);            
        }
        private void OnUIMenuClick(object sender, udcFixture.CSetMenuArgs e)
        {
            if (MainWork == null)
                return;

            MainWork.OnFrmMainMenuArgs(e);
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
        /// 标题栏L1-L10
        /// </summary>
        private TableLayoutPanel panelTilte = null;
        /// <summary>
        ///主框架
        /// </summary>
        private TableLayoutPanel[] panelMain = null;
        /// <summary>
        /// 母治具
        /// </summary>
        private List<TableLayoutPanel> panelFix = null;
        /// <summary>
        /// 母治具标题
        /// </summary>
        private List<Label> labFixSlot = new List<Label>();
        /// <summary>
        /// 子治具
        /// </summary>
        private List<udcFixture> uiUUT = new List<udcFixture>();
        /// <summary>
        /// 基本信息
        /// </summary>
        private udcModelBase uiModel = null;
        /// <summary>
        /// 输入规格
        /// </summary>
        private udcOutput uiOutPut = null;
        /// <summary>
        /// 信号监控
        /// </summary>
        private udcSignal uiSignal = null;
        /// <summary>
        /// 良率预警
        /// </summary>
        private udcPassRate uiPassRate = null;
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
                if (CGlobalPara.SysPara.Para.ChkAutoSelf)
                {
                    labBaseInfo.Text = CLanguage.Lan("基本信息") + "【" + CLanguage.Lan("库位自检模式") + "】";
                    labBaseInfo.ForeColor = Color.Red;
                }
                else
                {
                    if (!CGlobalPara.SysPara.Mes.Connect)
                    {
                        labBaseInfo.Text =  CLanguage.Lan("基本信息");
                        labBaseInfo.ForeColor = Color.Black;
                    }
                    else
                    {
                        labBaseInfo.Text =  CLanguage.Lan("基本信息") + "【MES】";
                        labBaseInfo.ForeColor = Color.MidnightBlue;
                    }
                }
            }
        }
        private void RefreshTemp(CUITempArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUITempArgs>(RefreshTemp), e);
            else
            {
                udcTemp.ShowTemp(e.TLP, e.THP, e.TPoint);
            }
        }
        private void RefreshModelList(bool changeModel, string curModel, List<string> modelList)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool, string, List<string>>(RefreshModelList), changeModel, curModel, modelList);
            else
            {
                cmbModelList.Items.Clear();

                for (int i = 0; i < modelList.Count; i++)
                    cmbModelList.Items.Add(modelList[i]);

                cmbModelList.Text = curModel;

                if (chkSelOut.Checked && changeModel)
                {
                    labOutModel.Text = CLanguage.Lan("当前机型已出机完毕,请选择其他机型");
                    labOutModel.ForeColor = Color.Red;
                    tabControl1.SelectTab(3);
                }
                else
                {
                    labOutModel.Text = "";
                }
            }
        }
        private void RefreshModelNum(string cutOutModel, int ttNum, int outNum)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, int, int>(RefreshModelNum), cutOutModel, ttNum, outNum);
            else
            {
                cmbModelList.Text = cutOutModel;

                labModelTTNum.Text = ttNum.ToString();

                labModelOutNum.Text = outNum.ToString();
            }
        }
        #endregion

        #region 主线程类
        private CMainWork MainWork = null;
        /// <summary>
        /// 初始化主线程类
        /// </summary>
        private void InitalMainWork()
        {
            MainWork = new CMainWork(0, "<"+ CLanguage.Lan("自动老化测试工位")+ ">", _fatherGuid);
            MainWork.OnUILogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnLog);
            MainWork.OnPLCLogArgs.OnEvent += new COnEvent<CUILogArgs>.OnEventHandler(OnPLCLog);
            MainWork.OnUIGlobalArgs.OnEvent += new COnEvent<CUIGlobalArgs>.OnEventHandler(OnGlobal);
            MainWork.OnUIInidcatorArgs.OnEvent += new COnEvent<CUIInicatorArgs>.OnEventHandler(OnIndicator); 
            MainWork.OnUISystemArgs.OnEvent += new COnEvent<CUIUserArgs<CUISystemArgs>>.OnEventHandler(OnSystem);
            MainWork.OnUIModelArgs.OnEvent += new COnEvent<CUIUserArgs<CModelPara>>.OnEventHandler(OnModel);
            MainWork.OnUIUUTArgs.OnEvent += new COnEvent<CUIUserArgs<CUUT>>.OnEventHandler(OnUUT);
            MainWork.OnUITemp.OnEvent += new COnEvent<CUITempArgs>.OnEventHandler(OnTemp);
            MainWork.OnUISignalArgs.OnEvent += new COnEvent<CUISignalArgs>.OnEventHandler(OnSignal);
            MainWork.OnUIMainlArgs.OnEvent += new COnEvent<CUIUserArgs<CUIMainArgs>>.OnEventHandler(OnMain);
            MainWork.OnActionArgs.OnEvent += new COnEvent<CUIActionArgs>.OnEventHandler(OnAction);
            MainWork.OnUIPassRateArgs.OnEvent += new COnEvent<CUIUserArgs<CWarnRate>>.OnEventHandler(OnPassRate);
            MainWork.OnUIActionArgs.OnEvent += new COnEvent<CUIUserArgs<CUIActionAgrs>>.OnEventHandler(OnActionArgs);
            MainWork.InitialUI();
        }
        #endregion

        #region 线程类消息
        /// <summary>
        /// 全局UI用户消息
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
        /// 机种信息消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModel(object sender, CUIUserArgs<CModelPara> e)
        {
            double TLow = e.model.Temp.TSet - e.model.Temp.TLP;

            double THigh = e.model.Temp.TSet + e.model.Temp.THP;

            string temp = e.model.Temp.TSet.ToString() + "℃[" + TLow.ToString() + "℃~" + THigh.ToString() + "℃]";

            uiModel.SetModelName(e.model.Base.Model);

            uiModel.SetTemp(temp);

            uiModel.SetBITime(e.model.Para.BITime);

            uiOutPut.SetValue(e.model);
        }
        /// <summary>
        /// 温度信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTemp(object sender, CUITempArgs e)
        {
            RefreshTemp(e);
        }
        /// <summary>
        /// 库位信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUUT(object sender, CUIUserArgs<CUUT> e)
        {
            uiUUT[e.idNo].SetUUT(e.model);
        }
        /// <summary>
        /// 信号信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSignal(object sender, CUISignalArgs e)
        {          
           uiSignal.SetUI(e.name, e.Info, e.lPara, e.wPara);            
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
        /// 状态信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMain(object sender, CUIUserArgs<CUIMainArgs> e)
        {
            switch (e.model.DoRun)
            {
                case EUIStatus.刷新机种:
                    RefreshModelList(e.model.CurModelChange,e.model.CurOutModel, e.model.CurModelList);
                    break;
                case EUIStatus.刷新数量:
                    RefreshModelNum(e.model.CurOutModel,e.model.CurModelNum,e.model.CurModelOutNum);
                    break;
                case EUIStatus.读取温度:
                    uiModel.ReadTemp(e.model.rTemp,e.model.rColor); 
                    break;
                case EUIStatus.读取电压:
                    uiModel.ReadACVolt(e.model.ACV,e.model.rColor);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 提示信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAction(object sender, CUIActionArgs e)
        {
            ShowAction(e);   
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
                    tabControl1.SelectTab(3);
                }
                uiPassRate.SetYield(e.model);
            }
        }
        private void ShowAction(CUIActionArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CUIActionArgs>(ShowAction),e);
            else
            {
                labAction.Text = e.info;
                labAction.ForeColor = e.color;
            }
        }
        private void ShowLog(CUILogArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(new Action<CUILogArgs>(ShowLog), e);
                else
                {
                    runLog.Log(e.info, e.log, e.save);
                }
            }
            catch (Exception)
            {
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
                        uiModel.SetBtnEnable(true);                        
                        break;
                    case EUISystem.启动:
                        uiModel.SetBtnEnable(false); 
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 信息提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActionArgs(object sender, CUIUserArgs<CUIActionAgrs> e)
        {
            ShowActionArgs(e);
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
                SFCS.FrmAction.CreateInstance().Show();

                Application.DoEvents();
            }
        }
        #endregion


    }
}
