using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using GJ.YOHOO.BURNIN.Udc;
using GJ.USER.APP.MainWork;
using GJ.UI;
using GJ.DEV.PLC;
using GJ.DEV.CARD;
using GJ.DEV.FCMB;
using GJ.DEV.LED;
using GJ.PDB;
using GJ.MES;
using GJ.USER.APP;
using GJ.COM;
using GJ.USER.APP.Iot;
using GJ.Iot;
using GJ.SFCS;


namespace GJ.YOHOO.BURNIN
{
    public class CMainWork : IMainWork
    {
        #region 构造函数
        private int idNo = 0;
        private string name = string.Empty;
        private string guid = string.Empty;
        public CMainWork(int idNo, string name, string guid)
        {
            this.idNo = idNo;
            this.name = name;
            this.guid = guid;
            base.CMainWork(idNo, name, guid); 
        }
        #endregion   

        #region 实现初始化虚方法
        public override void LoadSysFile()
        {
            try
            {
                CGlobalPara.LoadSysXml();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void LoadRunPara()
        {
            try
            {
                string er = string.Empty;

                if (!load_user_plc_reg(CLanguage.Lan("老化测试位"),out er))
                    Log(er, udcRunLog.ELog.NG);

                if (!load_user_info(out er))
                    Log(er, udcRunLog.ELog.NG);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        public override void LoadIniFile()
        {
            try
            {
                try
                {
                    CGlobalPara.DeviceIDNo = CIniFile.ReadFromIni(CGlobalPara.StationName, "GuidSn", CGlobalPara.IniFile);

                    _defaultModelPath = CIniFile.ReadFromIni("Parameter", "ModelPath", CGlobalPara.IniFile);

                    _chmr.CurOutModel = CIniFile.ReadFromIni("Parameter", "curOutModel", CGlobalPara.IniFile);

                    _chmr.FixYield.InBIFixNo = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "InBIFixNo", CGlobalPara.IniFile, "0"));
                    _chmr.FixYield.OutBIFixNo = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "OutBIFixNo", CGlobalPara.IniFile, "0"));
                    _chmr.FixYield.BITTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "BITTNum", CGlobalPara.IniFile, "0"));
                    _chmr.FixYield.BIPASSNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "BIPASSNum", CGlobalPara.IniFile, "0"));

                    _chmr.DayYield.dayNow = CIniFile.ReadFromIni("DailyYield", "dayNow", CGlobalPara.IniFile);
                    _chmr.DayYield.ttNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "ttNum", CGlobalPara.IniFile, "0"));
                    _chmr.DayYield.failNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "failNum", CGlobalPara.IniFile, "0"));
                    _chmr.DayYield.yieldTTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "yieldTTNum", CGlobalPara.IniFile, "0"));
                    _chmr.DayYield.yieldFailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "yieldFailNum", CGlobalPara.IniFile, "0"));

                    LoadIniForPassRate();

                    LoadCtrlACIniFile();

                }
                catch (Exception)
                {
                    throw;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void LoadModelPara()
        {
            try
            {
                if (!File.Exists(_defaultModelPath))
                    return;

                COM.CSerializable<CModelPara>.ReadXml(_defaultModelPath, ref _runModel);

                CIniFile.WriteToIni("Parameter", "ModelPath", _defaultModelPath, CGlobalPara.IniFile);

                OnUIModelArgs.OnEvented(new CUIUserArgs<CModelPara>(idNo, name, _runModel));

                for (int uutNo = 0; uutNo < _runUUT.Count; uutNo++)
                {
                    if (_runUUT[uutNo].Para.strJson == string.Empty)
                        LoadLedItem(uutNo);

                    for (int i = 0; i < _runUUT[uutNo].Led.Count; i++)
                    {
                        if (_runUUT[uutNo].Led[i].strJson == string.Empty)
                            LoadLedValue(uutNo);
                    }
                }

                LoadCtrlACPara();

                _intPara = true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void LoadMainFormUI()
        {
            try
            {
                RefreshSlotUI();
                
                UpdateInBIFixNumUI();

                UpdateOutBIFixNumUI();

                UpdateUUTYieldUI();
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        public override void LoadUIComplete()
        {
            MainWorker = new BackgroundWorker();
            MainWorker.WorkerSupportsCancellation = true;
            MainWorker.WorkerReportsProgress = true;
            MainWorker.DoWork += new DoWorkEventHandler(MainWorker_DoWork);

            DataWorker = new BackgroundWorker();
            DataWorker.WorkerSupportsCancellation = true;
            DataWorker.WorkerReportsProgress = true;
            DataWorker.DoWork += new DoWorkEventHandler(DataWorker_DoWork);

            OnUIGlobalArgs.OnEvented(new CUIGlobalArgs());
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));

            RefreshCurModelList();

            InitialIot();

        }
        #endregion

        #region 实现抽象方法
        public override bool InitialRunPara()
        {
            if (_runModel == null || _runModel.Base.Model == null)
            {
                MessageBox.Show(CLanguage.Lan("请选择要测试机种名称,再启动监控."), "Tip",
                                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            _intPara = true;

            _chmr.Entrance.DoRun = ERun.空闲;

            _chmr.InPlatForm.DoRun = ERun.空闲;

            return true;
        }
        public override bool OpenDevice()
        {
            Log(CLanguage.Lan("系统硬件设备开始自检."), udcRunLog.ELog.OK);

            bool CheckSys = true;

            Stopwatch wather = new Stopwatch();

            wather.Start();

            try
            {
                List<Task<bool>> IniTask = new List<Task<bool>>();

                IniTask.Add(Task.Factory.StartNew(() => OpenPLC()));

                IniTask.Add(Task.Factory.StartNew(() => OpenIdCard()));

                for (int i = 0; i < CGlobalPara.C_MON_NUM; i++)
                {
                    int idNo = i;

                    IniTask.Add(Task.Factory.StartNew(() => OpenQCVMon(idNo)));
                }

                IniTask.Add(Task.Factory.StartNew(() => OpenERS()));

                IniTask.Add(Task.Factory.StartNew(() => OpenGJWebServer()));

                if (CGlobalPara.SysPara.Mes.Connect)
                {
                    IniTask.Add(Task.Factory.StartNew(() => CheckTestConnect()));
                }

                while (true)
                {
                    Application.DoEvents();

                    bool ExitFlag = true;

                    for (int idNo = 0; idNo < IniTask.Count; idNo++)
                    {
                        if (!IniTask[idNo].IsCompleted)
                            ExitFlag = false;
                    }
                    if (ExitFlag)
                        break;
                }

                for (int idNo = 0; idNo < IniTask.Count; idNo++)
                {
                    if (!IniTask[idNo].Result)
                    {
                        CheckSys = false;
                    }
                }

                return CheckSys;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
            finally
            {
                string waitTime = ((double)wather.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                if (CheckSys)
                {
                    Log(CLanguage.Lan("系统硬件设备自检结束【PASS】:") + waitTime, udcRunLog.ELog.OK);
                }
                else
                {
                    Log(CLanguage.Lan("系统硬件设备自检结束【FAIL】:") + waitTime, udcRunLog.ELog.NG);
                }
            }
        }
        public override void CloseDevice()
        {
            ClosePLC();

            CloseIdCard();

            CloseQCVMon();

            CloseERS();

        }
        public override bool StartThread()
        {
            try
            {
                //启动PLC背景线程
                if (_threadPLC == null)
                {
                    _threadPLC = new CPLCThread(_userPLCReg.scanREG, _userPLCReg.rREG, _userPLCReg.wREG);
                    _threadPLC.OnConArgs.OnEvent += new COnEvent<CPLCConArgs>.OnEventHandler(OnPLCConArgs);
                    _threadPLC.OnDataArgs.OnEvent += new COnEvent<CPLCDataArgs>.OnEventHandler(OnPLCDataArgs);
                    _threadPLC.SpinUp(_devPLC);
                    Log(CLanguage.Lan("PLC监控线程启动"), udcRunLog.ELog.Action);
                }

                //启动控制板线程
                for (int idNo = 0; idNo < CGlobalPara.C_MON_MAX.Length; idNo++)
                {
                    if (_threadMon[idNo] == null)
                    {
                        _threadMon[idNo] = new CFMBThread(idNo, CGlobalPara.C_MON_NAME[idNo], 1, CGlobalPara.C_MON_MAX[idNo]);
                        _threadMon[idNo].OnStatusArgs.OnEvent += new COnEvent<DEV.FCMB.CConArgs>.OnEventHandler(OnMonConArgs);
                        _threadMon[idNo].OnDataArgs.OnEvent += new COnEvent<DEV.FCMB.CDataArgs>.OnEventHandler(OnMonDataArgs);
                        _threadMon[idNo].SpinUp(_devMon[idNo]);
                        Log(CGlobalPara.C_MON_NAME[idNo] + CLanguage.Lan("监控线程启动"), udcRunLog.ELog.Action);
                    }
                }

                //启动ERS线程
                if (_threadERS == null)
                {
                    _threadERS = new CLEDThread(0, CGlobalPara.C_ERS_NAME, 1, CGlobalPara.C_ERS_MAX);
                    _threadERS.OnStatusArgs.OnEvent += new COnEvent<DEV.LED.CConArgs>.OnEventHandler(OnERSConArgs);
                    _threadERS.OnDataArgs.OnEvent += new COnEvent<DEV.LED.CDataArgs>.OnEventHandler(OnERSDataArgs);
                    _threadERS.SpinUp(_devERS);
                    Log(CGlobalPara.C_ERS_NAME + CLanguage.Lan("监控线程启动"), udcRunLog.ELog.Action);
                }

                //启动主线程
                if (!MainWorker.IsBusy)
                    MainWorker.RunWorkerAsync();

                if (!DataWorker.IsBusy)
                    DataWorker.RunWorkerAsync();

                UISystemArgs.DoRun = EUISystem.启动;

                OnUISystemArgs.OnEvented(new CUIUserArgs<CUISystemArgs>(this.idNo, name, UISystemArgs));

                UpdateIotDeviceStatus();

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }

        }
        public override void StopThread()
        {
            try
            {
                //销毁主线程                
                if (MainWorker.IsBusy)
                    MainWorker.CancelAsync();

                if (DataWorker.IsBusy)
                    DataWorker.CancelAsync();

                while (MainWorker.IsBusy || DataWorker.IsBusy)
                {
                    Application.DoEvents();
                }

                //销毁PLC背景线程
                if (_threadPLC != null)
                {
                    _threadPLC.OnConArgs.OnEvent -= new COnEvent<CPLCConArgs>.OnEventHandler(OnPLCConArgs);
                    _threadPLC.OnDataArgs.OnEvent -= new COnEvent<CPLCDataArgs>.OnEventHandler(OnPLCDataArgs);
                    _threadPLC.SpinDown();
                    _threadPLC = null;
                    Log(CLanguage.Lan("PLC监控线程销毁"), udcRunLog.ELog.NG);
                }

                //销毁控制板线程
                for (int idNo = 0; idNo < CGlobalPara.C_MON_MAX.Length; idNo++)
                {
                    if (_threadMon[idNo] != null)
                    {
                        _threadMon[idNo].OnStatusArgs.OnEvent -= new COnEvent<DEV.FCMB.CConArgs>.OnEventHandler(OnMonConArgs);
                        _threadMon[idNo].OnDataArgs.OnEvent -= new COnEvent<DEV.FCMB.CDataArgs>.OnEventHandler(OnMonDataArgs);
                        _threadMon[idNo].SpinDown();
                        _threadMon[idNo] = null;
                        Log(CGlobalPara.C_MON_NAME[idNo] + CLanguage.Lan("监控线程销毁"), udcRunLog.ELog.NG);
                    }
                }

                //销毁ERS线程
                if (_threadERS != null)
                {
                    _threadERS.OnStatusArgs.OnEvent -= new COnEvent<DEV.LED.CConArgs>.OnEventHandler(OnERSConArgs);
                    _threadERS.OnDataArgs.OnEvent -= new COnEvent<DEV.LED.CDataArgs>.OnEventHandler(OnERSDataArgs);
                    _threadERS.SpinDown();
                    _threadERS = null;
                    Log(CGlobalPara.C_ERS_NAME + CLanguage.Lan("监控线程销毁"), udcRunLog.ELog.NG);
                }

                UISystemArgs.DoRun = EUISystem.空闲;

                OnUISystemArgs.OnEvented(new CUIUserArgs<CUISystemArgs>(this.idNo, name, UISystemArgs));

                UpdateIotDeviceStatus();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        public override void CloseDlg()
        {
            StopThread();

            CloseDevice();

            StopIot();
        }
        #endregion

        #region UI消息类定义
        /// <summary>
        /// 系统UI
        /// </summary>
        private CUISystemArgs UISystemArgs = new CUISystemArgs();
        /// <summary>
        /// 状态类
        /// </summary>
        private CUIMainArgs UIMainArgs = new CUIMainArgs();
        /// <summary>
        /// 信息提示
        /// </summary>
        public COnEvent<CUIUserArgs<CUIActionAgrs>> OnUIActionArgs = new COnEvent<CUIUserArgs<CUIActionAgrs>>();
        #endregion

        #region 定义UI消息
        /// <summary>
        /// 系统UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUISystemArgs>> OnUISystemArgs = new COnEvent<CUIUserArgs<CUISystemArgs>>();
        /// <summary>
        /// 机种信息消息
        /// </summary>
        public COnEvent<CUIUserArgs<CModelPara>> OnUIModelArgs = new COnEvent<CUIUserArgs<CModelPara>>();
        /// <summary>
        /// 槽位刷新消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUUT>> OnUIUUTArgs = new COnEvent<CUIUserArgs<CUUT>>();
        /// <summary>
        /// 信号刷新消息
        /// </summary>
        public COnEvent<CUISignalArgs> OnUISignalArgs = new COnEvent<CUISignalArgs>();
        /// <summary>
        /// 定义温度消息
        /// </summary>
        public COnEvent<CUITempArgs> OnUITemp = new COnEvent<CUITempArgs>(); 
        /// <summary>
        /// 主界面消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUIMainArgs>> OnUIMainlArgs = new COnEvent<CUIUserArgs<CUIMainArgs>>();
        /// <summary>
        /// 信息指示
        /// </summary>
        public COnEvent<CUIActionArgs> OnActionArgs = new COnEvent<CUIActionArgs>();
        /// <summary>
        /// 良品预警
        /// </summary>
        public COnEvent<CUIUserArgs<CWarnRate>> OnUIPassRateArgs = new COnEvent<CUIUserArgs<CWarnRate>>();
        #endregion

        #region 加载用户参数
        /// <summary>
        /// 加载PLC读写寄存器
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool load_user_plc_reg(string name,out string er)
        {
            er = string.Empty;

            try
            {
                CUSER_PLCREG plcList = new CUSER_PLCREG();

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.PLCDB);

                DataSet ds = null;

                //扫描寄存器
                string sqlCmd = "select * from scanREG order by idNo";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CPLCThread.CREG reg = new CPLCThread.CREG();
                    reg.regName = ds.Tables[0].Rows[i]["regName"].ToString();
                    reg.regType = (GJ.DEV.PLC.ERegType)Enum.Parse(typeof(GJ.DEV.PLC.ERegType), reg.regName.Substring(0, 1));
                    reg.regLen = System.Convert.ToInt16(ds.Tables[0].Rows[i]["regLen"].ToString());
                    string starAddr = reg.regName.Substring(1, reg.regName.Length - 1);
                    reg.startAddr = System.Convert.ToInt16(starAddr);
                    plcList.scanREG.Add(reg);
                }

                //读寄存器 
                sqlCmd = "select * from rREG where regUsed=1 order by idNo";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CPLCThread.CREG reg = new CPLCThread.CREG();
                    reg.regName = ds.Tables[0].Rows[i]["regName"].ToString();
                    reg.regType = (GJ.DEV.PLC.ERegType)Enum.Parse(typeof(GJ.DEV.PLC.ERegType), reg.regName.Substring(0, 1));
                    reg.regDes = ds.Tables[0].Rows[i]["regDes"].ToString();
                    reg.regLen = System.Convert.ToInt16(ds.Tables[0].Rows[i]["regLen"].ToString());
                    string starAddr = reg.regName.Substring(1, reg.regName.Length - 1);
                    reg.startAddr = System.Convert.ToInt16(starAddr);
                    plcList.rREG.Add(reg);
                }

                //写寄存器
                sqlCmd = "select * from wREG where regUsed=1 order by idNo";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    CPLCThread.CREG reg = new CPLCThread.CREG();
                    reg.regName = ds.Tables[0].Rows[i]["regName"].ToString();
                    reg.regType = (GJ.DEV.PLC.ERegType)Enum.Parse(typeof(GJ.DEV.PLC.ERegType), reg.regName.Substring(0, 1));
                    reg.regDes = ds.Tables[0].Rows[i]["regDes"].ToString();
                    reg.regLen = System.Convert.ToInt16(ds.Tables[0].Rows[i]["regLen"].ToString());
                    string starAddr = reg.regName.Substring(1, reg.regName.Length - 1);
                    reg.startAddr = System.Convert.ToInt16(starAddr);
                    plcList.wREG.Add(reg);
                }
                _userPLCReg = plcList;

                //读取PLC报警列表
                _PLCAlarmList = new CPLCAlarmList(idNo, CLanguage.Lan("老化测试位"));
                sqlCmd = "select * from AlamList where RegDisable=0 order by idNo";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    int index = System.Convert.ToInt32(ds.Tables[0].Rows[i]["idNo"].ToString());
                    int regNo = System.Convert.ToInt32(ds.Tables[0].Rows[i]["RegNo"].ToString());
                    int regBit = System.Convert.ToInt32(ds.Tables[0].Rows[i]["RegBit"].ToString());
                    string regFun = ds.Tables[0].Rows[i]["RegFun"].ToString();
                    string regDecs = ds.Tables[0].Rows[i]["RegDesc"].ToString();
                    int regLevel = System.Convert.ToInt32(ds.Tables[0].Rows[i]["RegLevel"].ToString());
                    CPLCAlarmReg alarmReg = new CPLCAlarmReg
                    {
                        idNo = index,
                        RegNo = regNo,
                        RegBit = regBit,
                        RegFun = regDecs,
                        RegLevel = regLevel,
                        RegVal = 0,
                        CurVal = 0
                    };
                    _PLCAlarmList.AddReg(alarmReg);
                }

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 加载用户测试参数
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool load_user_info(out string er)
        {
            er = string.Empty;

            try
            {
                for (int i = 0; i < CGlobalPara.C_UUT_MAX; i++)
                    _runUUT.Add(new CUUT());

                if (!local_db_compact(out er))
                    return false;

                if (!local_db_createTable(out er))
                    return false;

                if (!local_db_createColum(out er))
                    return false;

                if (!local_db_loadTable(out er))
                    return false;

                if (!local_db_dataFromJson(out er))
                    return false;

                if (!local_db_get_fixture(out er))
                    return false;

                InitialChartPara();

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        #endregion

        #region PLC读写定义
        /// <summary>
        /// PLC结果
        /// </summary>
        private enum EPLC_RESULT
        {
            空闲,
            结果OK,
            结果NG,
            直接过站
        }
        /// <summary>
        /// PLC IO输入
        /// </summary>
        private enum EPLCINP
        {
            PLC自动运行,
            PLC设备报警,
            PLC设备警告,
            当前总输入时间,
            当前输入AC电压值,
            老化入口平台等待进机,
            老化出口平台空闲,
            老化入口平台1需要读取ID卡,
            老化入口平台2需要读取ID卡,
            老化入口平台1治具光电,
            老化入口平台2治具光电,
            机械手状态,
            老化返板入口就绪,
            老化返板出口就绪,

            温度点1,
            温度点2,
            温度点3,
            温度点4,
            温度点5,
            温度点6,
            温度点7,
            温度点8,
            温度点9,
            温度点10,
            温度点11,
            温度点12,
            温度点13,
            温度点14,
            温度点15,

            PLC报警列表01,
            PLC报警列表02,
            PLC报警列表03,
            PLC报警列表04,
            PLC报警列表05,
            PLC报警列表06,
            PLC报警列表07,
            PLC报警列表08,
            PLC报警列表09,
            PLC报警列表10,
            PLC报警列表11,
            PLC报警列表12,
            PLC报警列表13,
            PLC报警列表14,
            PLC报警列表15,
            PLC报警列表16,
            PLC报警列表17,
            PLC报警列表18,
            PLC报警列表19,
            PLC报警列表20
        }
        /// <summary>
        /// PLC IO输出
        /// </summary>
        private enum EPLCOUT
        {
            上位机软件启动,
            上位机软件报警,
            当前总输入时间,
            老化入口平台1结果,
            老化入口平台2结果,
            机械手平台取哪层,
            机械手平台取哪列,
            机械手平台放哪层,
            机械手平台放哪列,
            机械手取放坐标完成,
            设定温度,
            温度上限,
            温度下限,
            超温上限,
            启动排风,
            停止排风,
            通电90V时间,
            通电110V时间,
            通电220V时间,
            通电264V时间,
            通电300V时间,
            通电330V时间,
            通电380V时间
        }
        /// <summary>
        /// 返回PLC输入名称
        /// </summary>
        /// <param name="inpIo"></param>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private string InpPLC(EPLCINP inpIo, int idNo)
        {
            int index = (int)inpIo + idNo;

            EPLCINP reg = (EPLCINP)index;

            return reg.ToString();
        }
        /// <summary>
        /// 返回PLC输出名称
        /// </summary>
        /// <param name="outIo"></param>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private string OutPLC(EPLCOUT outIo, int idNo)
        {
            return ((EPLCOUT)((int)outIo + idNo)).ToString();
        }
        /// <summary>
        /// PLC重连接次数
        /// </summary>
        private int _conToPLCAgain = 0;
        /// <summary>
        /// 用户PLC寄存器
        /// </summary>
        private CUSER_PLCREG _userPLCReg = null;
        #endregion

        #region 字段
        /// <summary>
        /// 默认机种路径
        /// </summary>
        private string _defaultModelPath = Application.StartupPath + "\\Model\\Demo.bi";
        /// <summary>
        /// 机种参数
        /// </summary>
        private CModelPara _runModel = new CModelPara();
        /// <summary>
        /// 治具参数
        /// </summary>
        private List<CUUT> _runUUT = new List<CUUT>();
        /// <summary>
        /// 母治具参数
        /// </summary>
        private List<CUnit> _RunFixture = new List<CUnit>();
        /// <summary>
        /// 老化库体信息
        /// </summary>
        private CCHmrStatus _chmr = new CCHmrStatus();
        /// <summary>
        /// PLC设备
        /// </summary>
        private CPLCCOM _devPLC = null;
        /// <summary>
        /// 读卡器设备
        /// </summary>
        private CCARDCom _devIDCard = null;
        /// <summary>
        /// 控制板
        /// </summary>
        private CFMBCom[] _devMon = new CFMBCom[CGlobalPara.C_MON_NUM];
        /// <summary>
        /// ERS模块750-4
        /// </summary>
        private CLEDCom _devERS = null;
        /// <summary>
        /// PLC线程
        /// </summary>
        private CPLCThread _threadPLC = null;
        /// <summary>
        /// 控制板线程
        /// </summary>
        private CFMBThread[] _threadMon = new CFMBThread[CGlobalPara.C_MON_NUM];
        /// <summary>
        /// ERS-750-4线程
        /// </summary>
        private CLEDThread _threadERS = null;
        /// <summary>
        /// 初始化参数设置
        /// </summary>
        private bool _intPara = true;
        /// <summary>
        /// 主线程周期监控
        /// </summary>
        private Stopwatch MainMonWatcher = new Stopwatch();
        /// <summary>
        /// 刷新库体UI时钟
        /// </summary>
        private Stopwatch UUTUIWatcher = new Stopwatch();
        /// <summary>
        /// 软件启动标志
        /// </summary>
        private Stopwatch Softwatcher = new Stopwatch();
        #endregion

        #region 面板消息
        /// <summary>
        /// 选机种消息
        /// </summary>
        /// <param name="filePath"></param>
        public void OnFrmMainSelectModel(string filePath)
        {
            _defaultModelPath = filePath;

            LoadModelPara();
        }
        /// <summary>
        /// 显示温度
        /// </summary>
        public void OnFrmMainTempShow()
        {
            udcTemp.CreateInstance(_runModel.Temp.TSet - _runModel.Temp.TLP,
                                   _runModel.Temp.TSet + _runModel.Temp.THP,
                                   _chmr.PLC.rTempPoint).Show();
        }
        /// <summary>
        /// 面板消息
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="name"></param>
        /// <param name="lPara"></param>
        /// <param name="wPara"></param>
        public void OnFrmMainArgs(int idNo, string name, int lPara, int wPara)
        {
            if (name == "进空治具")
            {
                _chmr.OP.ChkInFixEmpty = lPara;
            }
            else if (name == "禁进机")
            {
                _chmr.OP.ChkForbitIn = lPara;
            }
            else if (name == "出空治具")
            {
                _chmr.OP.ChkOutFixEmpty = lPara;
            }
            else if (name == "禁出机")
            {
                _chmr.OP.ChkForbitOut = lPara;
            }
            else if (name == "labOPBusy")
            {
                _chmr.OP.IsBusy = false;
                _chmr.OP.DoRun = EHandStat.空闲;
            }
            else if (name == "归零")
            {
                if (MessageBox.Show(CLanguage.Lan("确定要清除统计数量?"), "Tip", MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _chmr.FixYield.InBIFixNo = 0;

                    _chmr.FixYield.OutBIFixNo = 0;

                    UpdateInBIFixNumUI();

                    UpdateOutBIFixNumUI();

                    _chmr.DayYield.ttNum = 0;

                    _chmr.DayYield.failNum = 0;

                    CIniFile.WriteToIni("DailyYield", "yieldTTNum", _chmr.DayYield.ttNum.ToString(), CGlobalPara.IniFile);

                    CIniFile.WriteToIni("DailyYield", "yieldFailNum", _chmr.DayYield.failNum.ToString(), CGlobalPara.IniFile);
                }
            }
        }
        /// <summary>
        /// 手动功能
        /// </summary>
        /// <param name="e"></param>
        public void OnFrmMainMenuArgs(udcFixture.CSetMenuArgs e)
        {
            int uutNo = 0;

            if (e.idNo % 2 == 0) //治具1
                uutNo = e.idNo;
            else               //治具2
                uutNo = e.idNo - 1;

            switch (e.menuInfo)
            {
                case udcFixture.ESetMenu.显示信息:
                    hand_show_uut(e.idNo);
                    break;
                case udcFixture.ESetMenu.显示曲线:
                    hand_show_chart(e.idNo);
                    break;
                case udcFixture.ESetMenu.位置空闲:
                    hand_set_free(uutNo);
                    break;
                case udcFixture.ESetMenu.禁用位置:
                    hand_set_forbit(uutNo);
                    break;
                case udcFixture.ESetMenu.启动老化:
                    hand_set_startBI(uutNo);
                    break;
                case udcFixture.ESetMenu.停止老化:
                    hand_set_EndBI(uutNo);
                    break;
                case udcFixture.ESetMenu.解除报警:
                    hand_set_release_alrm(uutNo);
                    break;
                case udcFixture.ESetMenu.清除不良:
                    hand_clear_fail_info(uutNo);
                    break;
                case udcFixture.ESetMenu.指定位置老化:
                    hand_set_pos_in(uutNo);
                    break;
                case udcFixture.ESetMenu.优先老化结束:
                    hand_set_end_preform(uutNo);
                    break;
                case udcFixture.ESetMenu.设置为空治具:
                    hand_set_is_null(e.idNo);
                    break;
                case udcFixture.ESetMenu.复位快充模式:
                    hand_reset_dcm_mode(uutNo);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 显示整体AC ON/OFF时序
        /// </summary>
        public void OnFrmMainShowONOFFChart()
        {
            FrmACVolt.CreateInstance(_CtrlAC).Show();
        }
        #endregion

        #region 手动功能
        /// <summary>
        /// 显示治具信息
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_show_uut(int uutNo)
        {
            try
            {
                int index = uutNo;

                if (uutNo % 2 != 0)
                    index = uutNo - 1;

                udcUUTInfo uutInfo = new udcUUTInfo(_runUUT[uutNo]);

                uutInfo.UIRefresh.OnEvent += new COnEvent<udcUUTInfo.CUIRefreshArgs>.OnEventHandler(RefreshVoltUI);

                uutInfo.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 显示曲线信息
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_show_chart(int uutNo)
        {
            try
            {
                int index = uutNo;

                if (uutNo % 2 != 0)
                    index = uutNo - 1;

                int fixNo = _runUUT[uutNo].Base.fixNo - 1;

                udcVoltChart.CreateInstance(_RunFixture[fixNo]).Show();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 空闲
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_free(int uutNo)
        {
            try
            {
                string er = string.Empty;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.位置空闲;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[uutNo + idNo].Para.IsNull = 0;
                    _runUUT[uutNo + idNo].Para.OutLevel = 0;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;

                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" + 
                                           _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 禁用
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_forbit(int uutNo)
        {
            try
            {
                string er = string.Empty;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.位置禁用;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[uutNo + idNo].Para.IsNull = 0;
                    _runUUT[uutNo + idNo].Para.OutLevel = 0;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;

                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                        _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 启动老化
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_startBI(int uutNo)
        {
            try
            {
                string er = string.Empty;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[uutNo + idNo].Para.CtrlUUTONLine == 0)
                    {
                        MessageBox.Show(_runUUT[uutNo + idNo].Base.localName + ":" + CLanguage.Lan("检测不到到位信号,不能启动老化"), "Tip",
                                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                for (int idNo = 0; idNo < 2; idNo++)
                {
                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.正在进机;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                     
                     SetTimer(uutNo + idNo, CLanguage.Lan("正在进机"));

                     RefreshSlotUI(uutNo + idNo);

                     local_db_update_fix_status(uutNo + idNo);

                     Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                            _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 停止老化
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_EndBI(int uutNo)
        {
            try
            {
                string er = string.Empty;

                int iCom = _runUUT[uutNo].Base.ctrlCom;

                int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                if (_threadMon == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (_threadMon[iCom] == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                //强制老化结束
                for (int idNo = 0; idNo < 2; idNo++)
                {
                     InitialQCM(uutNo + idNo, 0);
                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.老化完成;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;

                    SetTimer(uutNo + idNo, CLanguage.Lan("老化完成"));

                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                           _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 解除报警
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_release_alrm(int uutNo)
        {
            try
            {
                string er = string.Empty;

                if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.母治具使用寿命已到 ||
                     _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.母治具使用寿命已到)
                {
                    udcClrDealTime dlg = new udcClrDealTime();
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].Para.UsedNum = 0;
                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                        _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    }
                }
                else if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.母治具不良次数超过设置值 ||
                    _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.母治具不良次数超过设置值)
                {
                    udcClrDealTime dlg = new udcClrDealTime();
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].Para.FailNum = 0;
                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                        _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    }
                }
                else
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].Para.FailNum = 0;
                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                        _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    }
                }
                for (int idNo = 0; idNo < 2; idNo++)
                {
                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                           _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 解除不良
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_clear_fail_info(int uutNo)
        {
            try
            {
                string er = string.Empty;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[uutNo + idNo].Para.DoRun == EDoRun.正在老化 ||
                        _runUUT[uutNo + idNo].Para.DoRun == EDoRun.老化完成||
                        _runUUT[uutNo + idNo].Para.DoRun == EDoRun.老化结束)
                    {
                        for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                        {
                            _runUUT[uutNo + idNo].Led[slot].unitV = _runUUT[idNo].Led[slot].vMin + 0.2;
                            _runUUT[uutNo + idNo].Led[slot].unitA = _runUUT[idNo].Led[slot].ISet;
                            _runUUT[uutNo + idNo].Led[slot].passResult = 0;
                            _runUUT[uutNo + idNo].Led[slot].failResult = 0;
                            _runUUT[uutNo + idNo].Led[slot].failEnd = 0;
                            _runUUT[uutNo + idNo].Led[slot].failTime = string.Empty;
                            _runUUT[uutNo + idNo].Led[slot].failInfo = string.Empty;
                        }
                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                        _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    }
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                           _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 指定位置进机老化
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_pos_in(int uutNo)
        {
            try
            {
                string er = string.Empty;

                int icom = _runUUT[uutNo].Base.ctrlCom;

                int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                if (_threadMon == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (_threadMon[icom] == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (_chmr.PLC.handStat != EHandStat.空闲)
                {
                    MessageBox.Show(CLanguage.Lan("机械手忙碌中,不能执行进出机"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                udcSelLocal seldlg = new udcSelLocal();

                if (seldlg.ShowDialog() != DialogResult.OK)
                    return;

                if (udcSelLocal.C_Row < 1 || udcSelLocal.C_Row > CGlobalPara.C_ROW_MAX)
                    return;

                if (udcSelLocal.C_Col < 1 || udcSelLocal.C_Col > CGlobalPara.C_COL_MAX)
                    return;

                int InUUTNo = -1;

                for (int i = 0; i < _runUUT.Count; i++)
                {
                    if (_runUUT[i].Base.roomNo == udcSelLocal.C_Room &&
                       _runUUT[i].Base.iRow == udcSelLocal.C_Row &&
                       _runUUT[i].Base.iCol == udcSelLocal.C_Col)
                    {
                        InUUTNo = i;
                        break;
                    }
                }

                if (InUUTNo == -1)
                    return;

                SetACON(uutNo, 0); 

                if (_runUUT[InUUTNo].Para.CtrlUUTONLine == 1)
                { 
                    MessageBox.Show(_runUUT[InUUTNo].ToString() + CLanguage.Lan("库位检测到治具到位信号,不能进机"),"Tip",
                                                                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }

                if (_runUUT[InUUTNo + 1].Para.CtrlUUTONLine == 1)
                {
                    MessageBox.Show(_runUUT[InUUTNo + 1].ToString() + CLanguage.Lan("库位检测到治具到位信号,不能进机"), "Tip",
                                                                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    InitialQCM(InUUTNo + idNo, 0);

                    //测试参数
                    _runUUT[InUUTNo + idNo].Para.IdCard = _runUUT[uutNo + idNo].Para.IdCard;
                    _runUUT[InUUTNo + idNo].Para.IsNull = _runUUT[uutNo + idNo].Para.IsNull;
                    _runUUT[InUUTNo + idNo].Para.MesFlag = _runUUT[uutNo + idNo].Para.MesFlag;
                    _runUUT[InUUTNo + idNo].Para.OrderName = _runUUT[uutNo + idNo].Para.OrderName;
                    _runUUT[InUUTNo + idNo].Para.ModelName = _runModel.Base.Model;
                    _runUUT[InUUTNo + idNo].Para.BurnTime = (int)(_runModel.Para.BITime * 3600);
                    _runUUT[InUUTNo + idNo].Para.RunTime = 0;
                    _runUUT[InUUTNo + idNo].Para.OutLevel = 0;
                    _runUUT[InUUTNo + idNo].Para.OutPutNum = _runModel.Para.OutPut_Num;
                    _runUUT[InUUTNo + idNo].Para.OnOffNum = _runModel.Para.OnOff_Num;

                    if (CPara.GetOutPutAndOnOffFromModel(_runModel, ref _runUUT[InUUTNo + idNo].OnOff, out er))
                    {
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurStepNo = 0;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurRunVolt = _runModel.OnOff[0].Item[0].ACV;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurRunOutPut = _runModel.OnOff[0].Item[0].OutPutType;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurQCType = _runModel.OutPut[_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut].Chan[0].QCType;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurQCV = _runModel.OutPut[_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut].Chan[0].QCV;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                    }

                    int outPutNo = _runUUT[InUUTNo + idNo].OnOff.OnOff[0].OutPutType;

                    //输出
                    for (int slot = 0; slot < _runUUT[InUUTNo + idNo].Led.Count; slot++)
                    {
                        _runUUT[InUUTNo + idNo].Led[slot].serialNo = _runUUT[uutNo + idNo].Led[slot].serialNo;
                        _runUUT[InUUTNo + idNo].Led[slot].vName = _runModel.OutPut[outPutNo].Chan[0].Vname;
                        _runUUT[InUUTNo + idNo].Led[slot].vMin = _runModel.OutPut[outPutNo].Chan[0].Vmin;
                        _runUUT[InUUTNo + idNo].Led[slot].vMax = _runModel.OutPut[outPutNo].Chan[0].Vmax;
                        _runUUT[InUUTNo + idNo].Led[slot].IMode = _runModel.OutPut[outPutNo].Chan[0].Imode;
                        _runUUT[InUUTNo + idNo].Led[slot].ISet = _runModel.OutPut[outPutNo].Chan[0].ISet;
                        _runUUT[InUUTNo + idNo].Led[slot].Imin = _runModel.OutPut[outPutNo].Chan[0].Imin;
                        _runUUT[InUUTNo + idNo].Led[slot].Imax = _runModel.OutPut[outPutNo].Chan[0].Imax;
                        _runUUT[InUUTNo + idNo].Led[slot].qcv = _runModel.OutPut[outPutNo].Chan[0].QCV;
                        _runUUT[InUUTNo + idNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].qcv;
                        _runUUT[InUUTNo + idNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].ISet;
                        _runUUT[InUUTNo + idNo].Led[slot].passResult = 0;
                        _runUUT[InUUTNo + idNo].Led[slot].failResult = 0;
                        _runUUT[InUUTNo + idNo].Led[slot].failEnd = 0;
                        _runUUT[InUUTNo + idNo].Led[slot].failTime = "";
                        _runUUT[InUUTNo + idNo].Led[slot].failInfo = "";
                    }

                    _runUUT[InUUTNo + idNo].Para.DoRun = EDoRun.正在进机;
                    _runUUT[InUUTNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[InUUTNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    SetTimer(InUUTNo + idNo, CLanguage.Lan("正在进机"));

                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.正在出机;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    SetTimer(uutNo + idNo, CLanguage.Lan("正在出机"));
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    RefreshSlotUI(InUUTNo + idNo);

                    local_db_update_in_bi(InUUTNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("指定进机") + "【" +
                                                          _runUUT[InUUTNo + idNo].ToString() + "】", udcRunLog.ELog.NG);
                }

                ControlPLC_PosToPos(uutNo, InUUTNo, out er);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 优先老化结束
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_end_preform(int uutNo)
        {
            try
            {
                string er = string.Empty;

                int icom = _runUUT[uutNo].Base.ctrlCom;

                int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                if (_threadMon == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (_threadMon[icom] == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                //强制老化结束
                for (int idNo = 0; idNo < 2; idNo++)
                {
                    InitialQCM(uutNo + idNo, 0);
                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.老化完成;
                    _runUUT[uutNo + idNo].Para.OutLevel = 1;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                    SetTimer(uutNo + idNo, CLanguage.Lan("老化完成"));
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                           _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 设置为空治具
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_set_is_null(int uutNo)
        {
            try
            {
                string er = string.Empty;

                int icom = _runUUT[uutNo].Base.ctrlCom;

                int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                if (_threadMon == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (_threadMon[icom] == null)
                {
                    MessageBox.Show(CLanguage.Lan("强制老化结束需在监控模式下"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (_runUUT[uutNo].Para.CtrlUUTONLine == 0)
                {
                    MessageBox.Show(_runUUT[uutNo].Base.localName + ":"+ CLanguage.Lan("检测不到到位信号,不能停止老化"), "Tip",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                _runUUT[uutNo].Para.DoRun = EDoRun.空治具到位;
                _runUUT[uutNo].Para.IsNull = 1;
                _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                _runUUT[uutNo].Para.AlarmTime = 0;
                _runUUT[uutNo].Para.AlarmInfo = string.Empty;

                RefreshSlotUI(uutNo);

                local_db_update_fix_status(uutNo);

                Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                       _runUUT[uutNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 复位快充模式
        /// </summary>
        /// <param name="uutNo"></param>
        private void hand_reset_dcm_mode(int uutNo)
        {
            try
            {
                string er = string.Empty;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[idNo + uutNo].Para.CtrlUUTONLine == 0)
                    {
                        MessageBox.Show(_runUUT[idNo + uutNo].Base.localName + ":"+ CLanguage.Lan("检测到位信号,不能复位快充模式"), "Tip",
                                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (_runUUT[idNo + uutNo].Para.DoRun != EDoRun.正在老化)
                    {
                        MessageBox.Show(_runUUT[idNo + uutNo].Base.localName + ":" + CLanguage.Lan("位置不处于老化中,不能复位快充模式"), "Tip",
                                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                for (int idNo = 0; idNo < 2; idNo++)
                {
                    InitialQCM(uutNo + idNo, 0);

                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunVolt = 0;

                    for (int slot = 0; slot < CYOHOOApp.SlotMax; slot++)
                    {
                        _runUUT[uutNo + idNo].Led[slot].passResult = 0;
                        _runUUT[uutNo + idNo].Led[slot].failResult = 0;
                        _runUUT[uutNo + idNo].Led[slot].vFailNum = 0;
                        _runUUT[uutNo + idNo].Led[slot].iFailNum = 0;
                        _runUUT[uutNo + idNo].Led[slot].failEnd = 0;
                    }

                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    Log(CLanguage.Lan("<手动强制功能>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置为") + "【" +
                                           _runUUT[uutNo + idNo].Para.DoRun.ToString() + "】", udcRunLog.ELog.NG);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 面板方法
        /// <summary>
        /// 刷新槽位状态
        /// </summary>
        private void RefreshSlotUI()
        {
            for (int idNo = 0; idNo < _runUUT.Count; idNo++)
            {
                OnUIUUTArgs.OnEvented(new CUIUserArgs<CUUT>(idNo,_runUUT[idNo].ToString(),_runUUT[idNo]));
            }
        }
        /// <summary>
        /// 刷新槽位状态
        /// </summary>
        private void RefreshSlotUI(int idNo)
        {
            OnUIUUTArgs.OnEvented(new CUIUserArgs<CUUT>(idNo, _runUUT[idNo].ToString(), _runUUT[idNo]));
        }
        /// <summary>
        /// 同步刷新子治具数据界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshVoltUI(object sender, udcUUTInfo.CUIRefreshArgs e)
        {
            e.runUUT = _runUUT[e.idNo];
        }
        /// <summary>
        /// 更新监控信息
        /// </summary>
        private void RefreshMainWatcherUI()
        {
            try
            {
                if (!MainMonWatcher.IsRunning)
                {
                    MainMonWatcher.Restart();
                    return;
                }

                string waitTime = MainMonWatcher.ElapsedMilliseconds.ToString() + "ms";

                OnUISignalArgs.OnEvented(new CUISignalArgs("labScanTime", waitTime, Color.Black));

                MainMonWatcher.Restart();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 刷新扫描监控时间
        /// </summary>
        /// <param name="spanTime"></param>
        private void RefreshMonTimeUI(long spanTime)
        {
            string waitTime = ((double)spanTime / 1000).ToString("0.0") + "s";

            OnUISignalArgs.OnEvented(new CUISignalArgs("labMonTime", waitTime, Color.Blue));
        }
        /// <summary>
        /// 更新进机位置
        /// </summary>
        private void UpdateInPosUI(string localName)
        {
            OnUISignalArgs.OnEvented(new CUISignalArgs("labInPos", localName, Color.Blue));
        }
        /// <summary>
        /// 更新机位置
        /// </summary>
        private void UpdateOutPosUI(string localName)
        {
            OnUISignalArgs.OnEvented(new CUISignalArgs("labOutPos", localName, Color.Blue));
        }
        /// <summary>
        /// 修改进BI治具数量
        /// </summary>
        /// <param name="uutNo"></param>
        private void UpdateInBIFixNumUI()
        {

            CIniFile.WriteToIni("Parameter", "InBIFixNo", _chmr.FixYield.InBIFixNo.ToString(), CGlobalPara.IniFile);

            OnUISignalArgs.OnEvented(new CUISignalArgs("labInNum", _chmr.FixYield.InBIFixNo.ToString(), Color.Blue));

        }
        /// <summary>
        /// 修改进BI治具数量
        /// </summary>
        /// <param name="uutNo"></param>
        private void UpdateOutBIFixNumUI()
        {

            CIniFile.WriteToIni("Parameter", "OutBIFixNo", _chmr.FixYield.OutBIFixNo.ToString(), CGlobalPara.IniFile);

            OnUISignalArgs.OnEvented(new CUISignalArgs("labOutNum", _chmr.FixYield.OutBIFixNo.ToString(), Color.Blue));

        }
        /// <summary>
        /// 更新老化计数
        /// </summary>
        private void UpdateUUTYieldUI()
        {
            CIniFile.WriteToIni("Parameter", "BITTNum", _chmr.FixYield.BITTNum.ToString(), CGlobalPara.IniFile);

            CIniFile.WriteToIni("Parameter", "BIPASSNum", _chmr.FixYield.BIPASSNum.ToString(), CGlobalPara.IniFile);

            double passRate = 1;

            if (_chmr.FixYield.BITTNum > 0)
                passRate = ((double)_chmr.FixYield.BIPASSNum) / ((double)_chmr.FixYield.BITTNum);

            OnUISignalArgs.OnEvented(new CUISignalArgs("labTTNum", _chmr.FixYield.BITTNum.ToString(), Color.Blue));

            OnUISignalArgs.OnEvented(new CUISignalArgs("labPassNum", _chmr.FixYield.BIPASSNum.ToString(), Color.Blue));

            OnUISignalArgs.OnEvented(new CUISignalArgs("labPassRate", passRate.ToString("P2"), Color.Blue));

        }
        #endregion

        #region 打开与关闭设备
        /// <summary>
        /// 打开PLC
        /// </summary>
        /// <returns></returns>
        private bool OpenPLC()
        {
            try
            {
                string er = string.Empty;

                if (_devPLC == null)
                {
                    _devPLC = new CPLCCOM(EPlcType.Inovance_TCP, 0, "【"+ CLanguage.Lan("老化位PLC") + "】");

                    if (!_devPLC.Open(CGlobalPara.SysPara.Dev.Bi_plc, out er, "502"))
                    {
                        Log(_devPLC.ToString() + "[" + CGlobalPara.SysPara.Dev.Bi_plc + "]" + CLanguage.Lan("连接通信错误:") + er, udcRunLog.ELog.NG);

                        _devPLC = null;

                        return false;
                    }

                    Log(_devPLC.ToString() + CLanguage.Lan("连接正常"), udcRunLog.ELog.Action);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 关闭PLC
        /// </summary>
        private void ClosePLC()
        {
            try
            {
                if (_devPLC != null)
                {
                    Log(_devPLC.ToString() + "[" + CGlobalPara.SysPara.Dev.Bi_plc + "]" + CLanguage.Lan("断开连接"), udcRunLog.ELog.Action);

                    _devPLC.Close();

                    _devPLC = null;

                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开读卡器
        /// </summary>
        /// <returns></returns>
        private bool OpenIdCard()
        {
            try
            {
                string er = string.Empty;

                if (_devIDCard == null)
                {
                    _devIDCard = new CCARDCom(ECardType.MFID, 0, "【"+ CLanguage.Lan("读卡器") + "】");

                    if (!_devIDCard.Open(CGlobalPara.SysPara.Dev.IdCom, out er))
                    {
                        Log(_devIDCard.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.IdCom + "]"+ 
                                                    CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);

                        _devIDCard = null;

                        return false;
                    }

                    string rSn = string.Empty;

                    for (int i = 0; i < CGlobalPara.C_ID_MAX; i++)
                    {
                        Thread.Sleep(100);

                        if (!_devIDCard.GetRecorderSn(1 + i, out rSn, out er))
                        {
                            Thread.Sleep(200);

                            if (!_devIDCard.GetRecorderSn(1 + i, out rSn, out er))
                            {
                                Log(_devIDCard.ToString() + CLanguage.Lan("读取地址")+ "[" + (1 + i).ToString("D2") + "]"+ 
                                                            CLanguage.Lan("错误")+ ":" + er, udcRunLog.ELog.NG);

                                _devIDCard.Close();

                                _devIDCard = null;

                                return false;
                            }
                        }
                    }

                    Log(_devIDCard.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.IdCom + "]"+ 
                                                CLanguage.Lan("通信正常"), udcRunLog.ELog.Action);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 关闭读卡器
        /// </summary>
        private void CloseIdCard()
        {
            try
            {
                if (_devIDCard != null)
                {
                    Log(_devIDCard.ToString() + CLanguage.Lan("关闭串口") + "[" + CGlobalPara.SysPara.Dev.IdCom + "]", udcRunLog.ELog.Action);

                    _devIDCard.Close();

                    _devIDCard = null;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开控制板
        /// </summary>
        /// <returns></returns>
        private bool OpenQCVMon(int idNo)
        {
            try
            {
                string er = string.Empty;

                if (_devMon[idNo] != null)
                    return true;

                _devMon[idNo] = new CFMBCom(DEV.FCMB.EType.FMB_V1, idNo, CGlobalPara.C_MON_NAME[idNo]);

                if (!_devMon[idNo].Open(CGlobalPara.SysPara.Dev.MonCom[idNo], out er))
                {
                    Log(_devMon[idNo].ToString() + CLanguage.Lan("快充控制板打开") + CGlobalPara.C_MON_NAME[idNo] + "[" + CLanguage.Lan("串口") +
                                                   CGlobalPara.SysPara.Dev.MonCom[idNo] + "]" + CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);                                                           
                    _devMon[idNo] = null;
                    return false;
                }

                Log(_devMon[idNo].ToString() + CLanguage.Lan("快充控制板成功打开") + CGlobalPara.C_MON_NAME[idNo] + "[" + CLanguage.Lan("串口") +
                                                CGlobalPara.SysPara.Dev.MonCom[idNo] + "]", udcRunLog.ELog.Action);

                for (int iAddr = 0; iAddr < CGlobalPara.C_MON_MAX[idNo]; iAddr++)
                {
                    Thread.Sleep(10);

                    string rVal = string.Empty;

                    if (!_devMon[idNo].ReadVersion(iAddr + 1, out rVal, out er))
                    {
                        Thread.Sleep(10);

                        if (!_devMon[idNo].ReadVersion(iAddr + 1, out rVal, out er))
                        {
                            Log(_devMon[idNo].ToString() + CLanguage.Lan("地址") + "【" + (iAddr + 1).ToString() + "】"+ 
                                                           CLanguage.Lan("通信异常:") + er, udcRunLog.ELog.NG);

                            continue;
                        }
                    }

                    if (CGlobalPara.SysPara.Dev.ChkFCMBVer && CGlobalPara.SysPara.Dev.FCMBVer != string.Empty)
                    {
                        if (CGlobalPara.SysPara.Dev.FCMBVer != rVal)
                        {
                            Log(_devMon[idNo].ToString() + CLanguage.Lan("地址") + "【" + (iAddr + 1).ToString() + "】"+
                                                          CLanguage.Lan("当前版本") + "【"+ rVal +"】" + CLanguage.Lan("与设置版本") + "【"+
                                                          CGlobalPara.SysPara.Dev.FCMBVer + "】"+ CLanguage.Lan("不符合,请检查."), udcRunLog.ELog.NG);
                        }                    
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 关闭控制板
        /// </summary>
        private void CloseQCVMon()
        {
            try
            {
                for (int idNo = 0; idNo < CGlobalPara.C_MON_MAX.Length; idNo++)
                {
                    if (_devMon[idNo] != null)
                    {
                        Log(_devMon[idNo].ToString() + CLanguage.Lan("快充控制板关闭") + CGlobalPara.C_MON_NAME[idNo] + CLanguage.Lan("串口") +
                                                      CGlobalPara.SysPara.Dev.MonCom[idNo], udcRunLog.ELog.Action);                                                                    

                        _devMon[idNo].Close();
                        _devMon[idNo] = null;

                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开ERS模块
        /// </summary>
        /// <returns></returns>
        private bool OpenERS()
        {
            try
            {
                string er = string.Empty;

                if (_devERS != null)
                    return true;

                _devERS = new CLEDCom(DEV.LED.EType.DA_750_4, 0, CGlobalPara.C_ERS_NAME);

                if (!_devERS.Open(CGlobalPara.SysPara.Dev.ErsCom[0], out er, CGlobalPara.SysPara.Dev.ErsBaud))
                {
                     Log(_devERS.ToString() +  CLanguage.Lan("打开串口") + CGlobalPara.SysPara.Dev.ErsCom[0] + 
                                               CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);

                    _devERS = null;

                    return false;
                }

                Log(_devERS.ToString() + CLanguage.Lan("成功打开串口") + CGlobalPara.SysPara.Dev.ErsCom[0], udcRunLog.ELog.Action);

                for (int iAddr = 0; iAddr < CGlobalPara.C_ERS_MAX; iAddr++)
                {
                    string ver = string.Empty;

                    Thread.Sleep(10);

                    if (!_devERS.ReadVersion(iAddr + 1, out ver, out er))
                    {
                        if (!_devERS.ReadVersion(iAddr + 1, out ver, out er))
                        {
                            Log(_devERS.ToString() + CLanguage.Lan("地址") + "【" + (iAddr + 1).ToString() + "】"+ 
                                                     CLanguage.Lan("通信异常:") + er, udcRunLog.ELog.NG);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 关闭ERS模块
        /// </summary>
        private void CloseERS()
        {
            try
            {
                if (_devERS != null)
                {
                    Log(_devERS.ToString() +  CLanguage.Lan("关闭串口") + CGlobalPara.SysPara.Dev.ErsCom[0], udcRunLog.ELog.Action);
                    
                    _devERS.Close();

                    _devERS = null;
                   
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 连接冠佳MES
        /// </summary>
        /// <returns></returns>
        private bool OpenGJWebServer()
        {
            try
            {
                string version = string.Empty;

                string er = string.Empty;

                if (!CWeb2.CheckSystem(CYOHOOApp.UlrWeb, out version, out er))
                {
                    Log(CLanguage.Lan("连接冠佳WEB") + "【" + CYOHOOApp.UlrWeb + "】"+ CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);
                    return false;
                }

                Log(CLanguage.Lan("连接冠佳WEB") + "【" + CYOHOOApp.UlrWeb + "】OK", udcRunLog.ELog.Action);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 重启PLC
        /// </summary>
        private void RestartPLC()
        {
            try
            {
                Log(CLanguage.Lan("PLC监控线程断开连接,尝试重新连接."), udcRunLog.ELog.NG);

                if (_threadPLC != null)
                {
                    _threadPLC.OnConArgs.OnEvent -= new COnEvent<CPLCConArgs>.OnEventHandler(OnPLCConArgs);
                    _threadPLC.OnDataArgs.OnEvent -= new COnEvent<CPLCDataArgs>.OnEventHandler(OnPLCDataArgs);
                    _threadPLC.SpinDown();
                    _threadPLC = null;
                }

                ClosePLC();

                if (!OpenPLC())
                {
                    Log(CLanguage.Lan("PLC监控线程重新连接失败."), udcRunLog.ELog.NG);
                    return;
                }

                if (_threadPLC == null)
                {
                    _threadPLC = new CPLCThread(_userPLCReg.scanREG, _userPLCReg.rREG, _userPLCReg.wREG);
                    _threadPLC.OnConArgs.OnEvent += new COnEvent<CPLCConArgs>.OnEventHandler(OnPLCConArgs);
                    _threadPLC.OnDataArgs.OnEvent += new COnEvent<CPLCDataArgs>.OnEventHandler(OnPLCDataArgs);
                    _threadPLC.SpinUp(_devPLC);
                    Log(GJ.COM.CLanguage.Lan("PLC监控线程重新启动"), udcRunLog.ELog.Action);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 主线程/数据线程
        /// <summary>
        /// 主线程
        /// </summary>
        private BackgroundWorker MainWorker = null;
        /// <summary>
        /// 数据线程
        /// </summary>
        private BackgroundWorker DataWorker = null;
        private void MainWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                CGlobalPara.C_RUNNING = true;

                Indicator(EIndicator.Auto);

                Log(CLanguage.Lan("主监控线程开始"), udcRunLog.ELog.Action);

                while (true)
                {
                    try
                    {
                        if (MainWorker.CancellationPending)
                            return;

                        int delayMs = CGlobalPara.SysPara.Dev.MonInterval;

                        Thread.Sleep(delayMs);

                        UpdateIotDeviceStatus();

                        RefreshMainWatcherUI();

                        if (!CheckSystem(delayMs))
                            continue;

                        if (!CheckPLCAlarm())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckMESStatus())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                        }
 
                        if (!SetIniBIPara())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!RefreshSignal())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckInPlatIdReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckInPlatInReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!ControlTotalAC())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CtrlDiffACTime())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!Control_AC_ONOFF())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (CGlobalPara.C_INI_SCAN)  //需初始化扫描监控后启动->加速进出机动作
                        {
                            if (!AssignPositon())
                            {
                                Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                                continue;
                            }

                            if (!UpdateUUTStatus())
                            {
                                Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                                continue;
                            }
                        }

                        if (!WaitMonitorComplete())
                            continue;

                        if (!UpdateMonitorStatus())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckMonitorStatus())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!UpdateMonitorSignal())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!SetMonitorErr())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!UpdateAllUTTUI())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!Control_QCM())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!UpdateBIYied())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!RefreshDailyPassRate())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckPassRateAlarm())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString(), udcRunLog.ELog.Err);
                        System.Threading.Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                CGlobalPara.C_RUNNING = false;

                Indicator(EIndicator.Idel);

                Log(CLanguage.Lan("主监控线程销毁"), udcRunLog.ELog.NG);
            }
        }
        private void DataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                CGlobalPara.C_RUNNING = true;

                Indicator(EIndicator.Auto);

                Log(CLanguage.Lan("数据监控线程开始"), udcRunLog.ELog.Action);

                while (true)
                {
                    try
                    {
                        if (DataWorker.CancellationPending)
                            return;

                        int delayMs = CGlobalPara.SysPara.Dev.MonInterval;

                        Thread.Sleep(delayMs);

                        SaveBIReport();

                        SaveToChartFile();

                        SaveTempReport();

                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString(), udcRunLog.ELog.Err);
                        System.Threading.Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                Log(CLanguage.Lan("数据监控线程销毁"), udcRunLog.ELog.NG);
            }
        }
        #endregion

        #region 线程方法
        /// <summary>
        /// 初始化老化房参数:老化输入电压和温度
        /// </summary>
        /// <param name="er"></param>
        private bool SetIniBIPara()
        {
            try
            {
                if (_intPara)
                {
                    //设置温度
                    _threadPLC.addREGWrite(EPLCOUT.设定温度.ToString(), (int)(_runModel.Temp.TSet * 10));
                    _threadPLC.addREGWrite(EPLCOUT.温度上限.ToString(), (int)(_runModel.Temp.THP * 10));
                    _threadPLC.addREGWrite(EPLCOUT.温度下限.ToString(), (int)(_runModel.Temp.TLP * 10));
                    _threadPLC.addREGWrite(EPLCOUT.超温上限.ToString(), (int)(_runModel.Temp.THAlarm * 10));
                    _threadPLC.addREGWrite(EPLCOUT.启动排风.ToString(), (int)(_runModel.Temp.TOPEN * 10));
                    _threadPLC.addREGWrite(EPLCOUT.停止排风.ToString(), (int)(_runModel.Temp.TCLOSE * 10));

                    //设置输入AC电压
                    if (CGlobalPara.SysPara.Dev.CtrlACMode == ECtrlACMode.PLC控制时序)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), (int)(_runModel.Para.AC_90V * 60));
                        _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), (int)(_runModel.Para.AC_110V * 60));
                        _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), (int)(_runModel.Para.AC_220V * 60));
                        _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), (int)(_runModel.Para.AC_264V * 60));
                        _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), (int)(_runModel.Para.AC_300V * 60));
                        _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), (int)(_runModel.Para.AC_330V * 60));
                        _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), (int)(_runModel.Para.AC_380V * 60));
                    }

                    _intPara = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 检查系统运行状态
        /// </summary>
        /// <param name="delayTimes"></param>
        /// <returns></returns>
        private bool CheckSystem(int delayTimes)
        {
            try
            {
                string er = string.Empty;

                //10S重连接1次
                int counT1 = 10000 / delayTimes;

                //检测PLC通信是否断开?
                if (_threadPLC == null || !_threadPLC.conStatus)
                {
                    if (_conToPLCAgain < counT1)
                        _conToPLCAgain++;
                    else
                    {
                        RestartPLC();
                        _conToPLCAgain = 0;
                    }
                    return false;
                }

                //3S提示1次异常
                int counT2 = 3000 / delayTimes;

                if (_threadPLC.threadStatus != CPLCThread.EThreadStatus.运行)
                {
                    if (_conToPLCAgain < counT2)
                        _conToPLCAgain++;
                    else
                    {
                        RestartPLC();
                        _conToPLCAgain = 0;
                        Log(CLanguage.Lan("PLC监控线程未启动运行,请检查PLC是否已连接？"), udcRunLog.ELog.NG);
                    }
                    return false;
                }

                if (!_threadPLC.complete)
                {
                    if (_conToPLCAgain < counT2)
                        _conToPLCAgain++;
                    else
                    {
                        _conToPLCAgain = 0;
                        Log(CLanguage.Lan("线体PLC自检状态超时,请检查PLC线路是否正常?"), udcRunLog.ELog.NG);
                    }
                    return false;
                }

                if (!Softwatcher.IsRunning)
                {
                    _threadPLC.addREGWrite(EPLCOUT.上位机软件启动.ToString(), 1);

                    Softwatcher.Restart();
                }
                else
                {
                    if (Softwatcher.ElapsedMilliseconds > CGlobalPara.C_SOFTWRE_TIME)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.上位机软件启动.ToString(), 1);

                        Softwatcher.Restart();
                    }
                }

                if (_threadPLC.threadStatus != CPLCThread.EThreadStatus.运行)
                {
                    if (_conToPLCAgain < counT2)
                        _conToPLCAgain++;
                    else
                    {
                        RestartPLC();
                        _conToPLCAgain = 0;
                        Log(CLanguage.Lan("PLC监控线程未启动运行,请检查PLC是否已连接?"), udcRunLog.ELog.NG);
                    }
                    return false;
                }

                if (_threadPLC.rREG_Val(EPLCINP.PLC自动运行.ToString()) != CPLCPara.ON)
                {
                    if (_conToPLCAgain < counT2)
                        _conToPLCAgain++;
                    else
                    {
                        _conToPLCAgain = 0;
                        Log(CLanguage.Lan("线体PLC未启动运行,请启动PLC"), udcRunLog.ELog.NG);
                    }
                    return true;
                }
                if (_threadPLC.rREG_Val(EPLCINP.PLC设备报警.ToString()) == CPLCPara.ON)
                {
                    if (_conToPLCAgain < counT2)
                        _conToPLCAgain++;
                    else
                    {
                        _conToPLCAgain = 0;
                        Log(CLanguage.Lan("线体PLC设备报警,请检查PLC触摸屏报警信息"), udcRunLog.ELog.NG);
                    }
                    return true;
                }

                //if (_threadPLC.rREG_Val(EPLCINP.PLC设备警告.ToString()) == CPLCPara.ON)
                //{
                //    if (_conToPLCAgain < counT2)
                //        _conToPLCAgain++;
                //    else
                //    {
                //        _conToPLCAgain = 0;
                //        Log("线体PLC设备警告,请检查PLC触摸屏报警信息", udcRunLog.ELog.NG);
                //    }
                //    return true;
                //}

                _conToPLCAgain = 0;

                if (FrmAction.AlarmFlag != 0)
                {
                    if (!FrmAction.IsAvalible)
                    {
                        FrmAction.CreateInstance().Show();

                        Application.DoEvents();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 刷新测试信号
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool RefreshSignal()
        {
            try
            {
                //运行状态
                if (_threadPLC.rREG_Val(EPLCINP.PLC自动运行.ToString()) != CPLCPara.ON)
                {
                    _chmr.PLC.sysStat = CLanguage.Lan("未运行");
                }
                else
                {
                    if (_threadPLC.rREG_Val(EPLCINP.PLC设备报警.ToString()) == CPLCPara.ON)
                        _chmr.PLC.sysStat = CLanguage.Lan("报警中");
                    else
                        _chmr.PLC.sysStat = CLanguage.Lan("运行中");
                }

                if (_chmr.PLC.sysStat == CLanguage.Lan("运行中"))
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labSysStatus", _chmr.PLC.sysStat, Color.Blue));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labSysStatus", _chmr.PLC.sysStat, Color.Red));

                //获取平均温度值

                double rTemp = 0;

                for (int i = 0; i < _chmr.PLC.rTempPoint.Length; i++)
                {
                    _chmr.PLC.rTempPoint[i] = ((double)_threadPLC.rREG_Val(InpPLC(EPLCINP.温度点1, i))) / 10;
                    rTemp += _chmr.PLC.rTempPoint[i];
                }                
                _chmr.PLC.rTemp = rTemp / _chmr.PLC.rTempPoint.Length;

                if (_chmr.PLC.rTemp < _runModel.Temp.TSet - _runModel.Temp.TLP)
                    UIMainArgs.rColor = Color.Yellow;
                else if (_chmr.PLC.rTemp > _runModel.Temp.TSet + _runModel.Temp.THP)
                    UIMainArgs.rColor = Color.Red;
                else
                    UIMainArgs.rColor = Color.Lime;

                UIMainArgs.rTemp = _chmr.PLC.rTemp;

                UIMainArgs.DoRun = EUIStatus.读取温度;

                OnUIMainlArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                if (udcTemp.IsAvalible)
                {
                    OnUITemp.OnEvented(new CUITempArgs(_runModel.Temp.TSet - _runModel.Temp.TLP,
                                                     _runModel.Temp.TSet + _runModel.Temp.THP,
                                                     _chmr.PLC.rTempPoint)
                                                     );
                }

                //获取当前输入电压值

                int index = _threadPLC.rREG_Val(EPLCINP.当前输入AC电压值.ToString());

                if (index >= 0 && index < C_ACVolt.Length)
                {
                    _chmr.PLC.rACVolt = C_ACVolt[index];

                    UIMainArgs.rColor = Color.Blue;
                }
                else
                {
                    _chmr.PLC.rACVolt = 0;

                    UIMainArgs.rColor = Color.Red;
                }

                UIMainArgs.ACV = _chmr.PLC.rACVolt;

                UIMainArgs.DoRun = EUIStatus.读取电压;

                OnUIMainlArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                //获取机械手状态
                int handStat = _threadPLC.rREG_Val(EPLCINP.机械手状态.ToString());

                if (Enum.IsDefined(typeof(EHandStat), handStat))
                    _chmr.PLC.handStat = (EHandStat)handStat;
                else
                    _chmr.PLC.handStat = EHandStat.忙碌;

                if (_chmr.PLC.handStat == EHandStat.空闲)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labHandStatus", _chmr.PLC.handStat.ToString(), Color.Blue));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labHandStatus", _chmr.PLC.handStat.ToString(), Color.Red));

                //入口读卡到位

                if(_threadPLC.rREG_Val(EPLCINP.老化入口平台1需要读取ID卡.ToString())==CPLCPara.ON &&
                   _threadPLC.rREG_Val(EPLCINP.老化入口平台2需要读取ID卡.ToString())==CPLCPara.ON)
                    _chmr.PLC.rIdReady = CPLCPara.ON;
                else
                   _chmr.PLC.rIdReady = CPLCPara.OFF;

                if (_chmr.PLC.rIdReady == CPLCPara.ON)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labPlatIdReay", CLanguage.Lan("就绪"), Color.Blue));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labPlatIdReay", CLanguage.Lan("空闲"), Color.Black));

                //入口顶升到位
                _chmr.PLC.rInReady = _threadPLC.rREG_Val(EPLCINP.老化入口平台等待进机.ToString());

                if (_chmr.PLC.rInReady == CPLCPara.ON)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labInPlatReady", CLanguage.Lan("就绪") + "|" + CLanguage.Lan(_chmr.OP.Alternant.ToString()), Color.Blue));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labInPlatReady", CLanguage.Lan("空闲") + "|" + CLanguage.Lan(_chmr.OP.Alternant.ToString()), Color.Black));

                //出口平台就绪                
                _chmr.PLC.rOutReady = _threadPLC.rREG_Val(EPLCINP.老化出口平台空闲.ToString());
                if (_chmr.PLC.rOutReady == CPLCPara.ON)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labOutPlat", CLanguage.Lan("就绪"), Color.Blue));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labOutPlat", CLanguage.Lan("空闲"), Color.Black));

                //返板状态
                _chmr.PLC.rReturnEntrance = _threadPLC.rREG_Val(EPLCINP.老化返板入口就绪.ToString());
                _chmr.PLC.rReturnExit = _threadPLC.rREG_Val(EPLCINP.老化返板出口就绪.ToString());
                string state = _chmr.PLC.rReturnEntrance.ToString() + "|" + _chmr.PLC.rReturnExit.ToString();
                if (_chmr.PLC.rReturnEntrance == 0 && _chmr.PLC.rReturnExit == 0)
                {
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labReturnStatus", state, Color.Black));
                }
                else
                {
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labReturnStatus", state, Color.Blue));
                }

                //入口执行状态
                string status = CLanguage.Lan(_chmr.Entrance.DoRun.ToString()) + "|" +
                                CLanguage.Lan(_chmr.InPlatForm.DoRun.ToString());

                if (_chmr.Entrance.DoRun == ERun.空闲 && _chmr.InPlatForm.DoRun == ERun.空闲)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labInPlatStatus", status,Color.Black));
                else if (_chmr.Entrance.DoRun == ERun.报警 && _chmr.InPlatForm.DoRun == ERun.报警)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labInPlatStatus", status, Color.Red));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labInPlatStatus", status, Color.Blue));

                //进出机状态
                if(_chmr.OP.DoRun==EHandStat.空闲)
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labOPBusy", _chmr.OP.DoRun.ToString(), Color.Black));
                else
                    OnUISignalArgs.OnEvented(new CUISignalArgs("labOPBusy", _chmr.OP.DoRun.ToString(), Color.Black));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 老化入口读卡平台治具到位
        /// </summary>
        /// <returns></returns>
        private bool CheckInPlatIdReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.老化入口平台1需要读取ID卡.ToString()) != CPLCPara.ON ||
                    _threadPLC.rREG_Val(EPLCINP.老化入口平台2需要读取ID卡.ToString()) != CPLCPara.ON)
                {
                    _chmr.Entrance.DoRun = ERun.空闲;
                    _chmr.Entrance.AlarmInfo = string.Empty;
                    return true;
                }

                if (_chmr.Entrance.DoRun != ERun.空闲)
                    return true;

                if (FrmAction.AlarmFlag != 0)
                    return true;

                _chmr.Entrance.DoRun = ERun.就绪;

                _chmr.Entrance.AlarmInfo = string.Empty;

                _chmr.Entrance.GoWayBI = 0;

                Task.Factory.StartNew(() => InPlatIdReadyTask());

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 入口顶升平台就绪
        /// </summary>
        /// <returns></returns>
        private bool CheckInPlatInReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.老化入口平台等待进机.ToString()) != CPLCPara.ON)
                {
                    _chmr.InPlatForm.DoRun = ERun.空闲;
                    _chmr.InPlatForm.AlarmInfo = string.Empty;
                    _chmr.InPlatForm.GoWayBI = 0;
                    return true;
                }

                if (_chmr.InPlatForm.DoRun != ERun.空闲)
                    return true;

                string er = string.Empty;

                if (!local_db_get_fixture(out er))
                {
                    _chmr.InPlatForm.DoRun = ERun.报警;

                    _chmr.InPlatForm.AlarmInfo = er;

                    Log(_chmr.InPlatForm.ToString() + er, udcRunLog.ELog.NG);

                    return false;
                }

                if (_chmr.InPlatForm.GoWayBI == 1)
                {
                    _chmr.InPlatForm.DoRun = ERun.跳站;

                    Log(_chmr.InPlatForm.ToString() + CLanguage.Lan("准备进站就绪,直接跳过老化测试"), udcRunLog.ELog.NG);
                }
                else
                {
                    _chmr.InPlatForm.DoRun = ERun.进站;

                    Log(_chmr.InPlatForm.ToString() + CLanguage.Lan("准备进站就绪,等待分配进机位置"), udcRunLog.ELog.Action);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 老化库位状态
        /// <summary>
        /// 更新治具状态
        /// </summary>
        /// <returns></returns>
        private bool UpdateUUTStatus()
        {
            try
            {
                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;

                    switch (_runUUT[uutNo].Para.DoRun)
                    {
                        case EDoRun.位置禁用:
                            break;
                        case EDoRun.位置空闲:
                            uut_wait_for_enter(uutNo);
                            break;
                        case EDoRun.正在进机:
                            uut_enter_into_bi(uutNo);
                            break;
                        case EDoRun.进机完毕:
                            uut_start_bi_test(uutNo);
                            break;
                        case EDoRun.启动老化:
                            if (CGlobalPara.C_SCAN_START) //扫描周期结束有效
                                break;
                            uut_self_testing(uutNo);
                            break;
                        case EDoRun.老化自检:
                            if (CGlobalPara.C_SCAN_START) //扫描周期结束有效
                                break;
                            uut_bi_testing(uutNo);
                            break;
                        case EDoRun.正在老化:
                            if (CGlobalPara.C_SCAN_START) //扫描周期结束有效
                                break;
                            uut_bi_burning(uutNo);
                            break;
                        case EDoRun.老化完成:
                            if (FrmAction.AlarmFlag != 0)
                                break;
                            uut_bi_test_over(uutNo);
                            break;
                        case EDoRun.老化结束:
                            uut_bi_out_from_bi(uutNo);
                            break;
                        case EDoRun.空治具到位:
                            uut_bi_out_from_bi(uutNo);
                            break;
                        case EDoRun.正在出机:
                            uut_out_bi_ready(uutNo);
                            break;
                        default:
                            break;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        ///  定时刷新库位UI
        /// </summary>
        /// <returns></returns>
        private bool UpdateAllUTTUI()
        {
            try
            {
                if (!UUTUIWatcher.IsRunning)
                {
                    UUTUIWatcher.Restart();

                    RefreshSlotUI();
                }
                else if (UUTUIWatcher.ElapsedMilliseconds > 1000)
                {
                    UUTUIWatcher.Restart();

                    UpdateUUTYieldUI();

                    RefreshSlotUI();
                }

                bool alarm = false;

                if (CGlobalPara.SysPara.Alarm.PassRateLimit > 0)
                {
                    if (_chmr.FixYield.BITTNum > 0)
                    {
                        double passRate = (double)_chmr.FixYield.BIPASSNum / (double)_chmr.FixYield.BITTNum * 100;

                        if (passRate < CGlobalPara.SysPara.Alarm.PassRateLimit)
                            alarm = true;
                    }
                }

                if (alarm)
                {
                    if (_threadPLC.wREG_Val(EPLCOUT.上位机软件报警.ToString()) != CPLCPara.ON)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), CPLCPara.ON);
                    }
                }
                else
                {
                    if (_threadPLC.wREG_Val(EPLCOUT.上位机软件报警.ToString()) != CPLCPara.OFF)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), CPLCPara.OFF);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 老化产能统计
        /// </summary>
        /// <returns></returns>
        private bool UpdateBIYied()
        {
            try
            {
                _chmr.FixYield.BITTNum = 0;

                _chmr.FixYield.BIPASSNum = 0;

                _WarnRate.TTNum = 0;

                _WarnRate.PassNum = 0;

                for (int uutNo = 0; uutNo < _runUUT.Count; uutNo++)
                {
                     if (_runUUT[uutNo].Para.IsNull == 1)
                        continue;

                     if (_runUUT[uutNo].Para.DoRun == EDoRun.正在老化 ||
                         _runUUT[uutNo].Para.DoRun == EDoRun.老化完成 ||
                         _runUUT[uutNo].Para.DoRun == EDoRun.老化结束)
                     {

                         for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                         {
                             if (_runUUT[uutNo].Led[slot].serialNo == string.Empty)
                                 continue;

                             if (_runUUT[uutNo].Led[slot].failResult == 0)
                             {
                                 _chmr.FixYield.BIPASSNum++;

                                 _WarnRate.PassNum++;
                             }                             

                             _chmr.FixYield.BITTNum++;

                             _WarnRate.TTNum++;
                         }
                     }
                }

                CIniFile.WriteToIni("Parameter", "BITTNum", _chmr.FixYield.BITTNum.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("Parameter", "BIPASSNum", _chmr.FixYield.BIPASSNum.ToString(), CGlobalPara.IniFile);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 保存测试报表
        /// </summary>
        /// <returns></returns>
        private bool SaveBIReport()
        {
            try
            {
                if (!CGlobalPara.SysPara.Report.SaveReport)
                    return true;

                for (int uutNo = 0; uutNo < _runUUT.Count; uutNo++)
                {
                    if (_runUUT[uutNo].Para.DoRun != EDoRun.正在老化)
                        continue;

                    if (_runUUT[uutNo].Para.IsNull == 1)
                        continue;

                    if (_runUUT[uutNo].Para.SaveDataTime == "")
                    {
                        _runUUT[uutNo].Para.SaveDataTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        continue;
                    }

                    int spanTime = CTimer.DateDiff(_runUUT[uutNo].Para.SaveDataTime);

                    int saveTime = (int)((double)CGlobalPara.SysPara.Report.SaveReportTimes * 60 / CGlobalPara.SysPara.Para.BITimeRate);

                    if (spanTime < saveTime)
                        continue;

                    uut_save_report(uutNo);

                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 更新监控状态
        /// </summary>
        /// <returns></returns>
        private bool UpdateMonitorStatus()
        {
            try
            {
                string er = string.Empty;

                //快充控制板

                for (int uutNo = 0; uutNo < _runUUT.Count / 2; uutNo++)
                {
                    int idNo = uutNo * 2;

                    int ctrlCom = _runUUT[idNo].Base.ctrlCom;

                    int ctrlAddr = _runUUT[idNo].Base.ctrlAddr;

                    if (_runUUT[idNo].Para.DoRun == EDoRun.位置禁用)
                    {
                        _threadMon[ctrlCom].SetStatus(ctrlAddr, DEV.FCMB.ESTATUS.禁用, out er);
                    }
                    else if (_runUUT[idNo].Para.DoRun == EDoRun.位置空闲)
                    {
                        _threadMon[ctrlCom].SetStatus(ctrlAddr, DEV.FCMB.ESTATUS.空闲, out er);
                    }
                    else
                    {
                        _threadMon[ctrlCom].SetStatus(ctrlAddr, DEV.FCMB.ESTATUS.运行, out er);
                    }
                }

                //ERS负载板---注意地址排列关系

                bool[] ers_using = new bool[CGlobalPara.C_ERS_MAX]; 

                for (int uutNo = 0; uutNo < _runUUT.Count; uutNo++)
                {
                    int ersCom = _runUUT[uutNo].Base.ersCom;

                    int ersAddr = _runUUT[uutNo].Base.ersAddr;

                    if (_runUUT[uutNo].Para.DoRun != EDoRun.位置禁用)
                    {
                        ers_using[ersAddr - 1] = true;
                    }
                }

                for (int i = 0; i < ers_using.Length; i++)
                {
                    if (ers_using[i])
                    {
                        _threadERS.SetStatus(i + 1, DEV.LED.ESTATUS.空闲, out er);
                    }
                    else
                    {
                        _threadERS.SetStatus(i + 1, DEV.LED.ESTATUS.禁用, out er); 
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检查设备状态
        /// </summary>
        /// <returns></returns>
        private bool CheckMonitorStatus()
        {
            try
            {
                for (int uutNo = 0; uutNo < _runUUT.Count; uutNo++)
                {
                    //位置禁用
                    if (_runUUT[uutNo].Para.DoRun == EDoRun.位置禁用)
                        continue;

                    //母治具使用寿命
                    if (CGlobalPara.SysPara.Alarm.FixUserTimes > 0 &&
                        _runUUT[uutNo].Para.UsedNum > CGlobalPara.SysPara.Alarm.FixUserTimes)
                    {
                        if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.母治具使用寿命已到)
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.母治具使用寿命已到;
                        continue;
                    }
                    else
                    {
                        if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.母治具使用寿命已到)
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                    }

                    //母治具连续不良锁住
                    if (CGlobalPara.SysPara.Alarm.FixFailLockNum > 0 &&
                        _runUUT[uutNo].Para.FailNum > CGlobalPara.SysPara.Alarm.FixFailLockNum)
                    {
                        if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.母治具不良次数超过设置值)
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.母治具不良次数超过设置值;
                        continue;
                    }
                    else
                    {
                        if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.母治具不良次数超过设置值)
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                    }

                    //控制板通信检查
                    int icom = _runUUT[uutNo].Base.ctrlCom;

                    int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                    if (!_threadMon[icom]._Mon[iAddr].Base.conStatus)
                    {
                        if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.控制板通信异常)
                        {
                            if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                                _runUUT[uutNo].Para.AlarmTime++;
                            else
                            {
                                _runUUT[uutNo].Para.AlarmCode = EAlarmCode.控制板通信异常;
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.控制板通信异常)
                        {
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                            _runUUT[uutNo].Para.AlarmTime = 0;
                        }
                    }

                    //ERS模块通信检查

                    int ersCom = _runUUT[uutNo].Base.ersCom;

                    int ersAddr = _runUUT[uutNo].Base.ersAddr;

                    int ersCH = _runUUT[uutNo].Base.ersCH - 1;

                    if (!_threadERS._Mon[ersAddr].Base.conStatus)
                    {
                        if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.ERS通信异常)
                        {
                            if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                                _runUUT[uutNo].Para.AlarmTime++;
                            else
                            {
                                _runUUT[uutNo].Para.AlarmCode = EAlarmCode.ERS通信异常;
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.ERS通信异常)
                        {
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                            _runUUT[uutNo].Para.AlarmTime = 0;
                        }
                    }


                    //检测控制板是否报警?
                    if (_runUUT[uutNo].Para.CtrlRunError != EAlarmCode.正常)
                    {
                        _runUUT[uutNo].Para.AlarmCode = _runUUT[uutNo].Para.CtrlRunError;
                        continue;
                    }
                    else if (_runUUT[uutNo].Para.AlarmCode >= EAlarmCode.继电器粘连警告)
                    {
                        _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                        _runUUT[uutNo].Para.AlarmTime = 0;
                    }

                    //检测到位信号-->无子治具但有到位信号
                    if (_runUUT[uutNo].Para.DoRun == EDoRun.位置空闲)
                    {
                        if (_runUUT[uutNo].Para.CtrlUUTONLine == 1)
                        {
                            if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.无治具有到位信号)
                            {
                                if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                                    _runUUT[uutNo].Para.AlarmTime++;
                                else
                                    _runUUT[uutNo].Para.AlarmCode = EAlarmCode.无治具有到位信号;
                            }
                            continue;
                        }
                        else
                        {
                            if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.无治具有到位信号)
                            {
                                _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                                _runUUT[uutNo].Para.AlarmTime = 0;
                            }
                        }
                    }

                    //有子治具但没有到位信号
                    if ((_runUUT[uutNo].Para.DoRun > EDoRun.正在进机 && _runUUT[uutNo].Para.DoRun < EDoRun.正在出机) ||
                         _runUUT[uutNo].Para.DoRun == EDoRun.空治具到位)
                    {
                        if (_runUUT[uutNo].Para.CtrlUUTONLine == 0)
                        {
                            if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.有治具无到位信号)
                            {
                                if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                                    _runUUT[uutNo].Para.AlarmTime++;
                                else
                                    _runUUT[uutNo].Para.AlarmCode = EAlarmCode.有治具无到位信号;
                            }
                            continue;
                        }
                        else
                        {
                            if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.有治具无到位信号)
                            {
                                _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                                _runUUT[uutNo].Para.AlarmTime = 0;
                            }
                        }
                    }

                    //检测治具AC电压异常
                    if (_runUUT[uutNo].Para.DoRun == EDoRun.正在老化 || _runUUT[uutNo].Para.DoRun == EDoRun.老化结束)
                    {
                        if (_runUUT[uutNo].Para.CtrlACON == 1 && _runUUT[uutNo].Para.CtrlOnOff == 1)
                        {
                            if (_runUUT[uutNo].Para.CtrlACSignal == 0)
                            {
                                if (_runUUT[uutNo].Para.AlarmCode != EAlarmCode.治具AC不通)
                                {
                                    if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                                        _runUUT[uutNo].Para.AlarmTime++;
                                    else
                                        _runUUT[uutNo].Para.AlarmCode = EAlarmCode.治具AC不通;
                                }
                                continue;
                            }
                            else
                            {
                                if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.治具AC不通)
                                {
                                    _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                                    _runUUT[uutNo].Para.AlarmTime = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.治具AC不通)
                        {
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                            _runUUT[uutNo].Para.AlarmTime = 0;
                        }
                    }

                    //检测异常断电->重启AC ON
                    if (_runUUT[uutNo].Para.DoRun == EDoRun.正在老化)
                    {
                        if (_runUUT[uutNo].OnOff.TimeRun.CurQCM == EQCMChage.空闲 && _runUUT[uutNo].OnOff.TimeRun.CurRunVolt > 0 &&
                            _runUUT[uutNo].Para.CtrlOnOff == 1 && _runUUT[uutNo].Para.CtrlACON == 0)
                        {
                            if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                            {
                                _runUUT[uutNo].Para.AlarmTime++;
                            }
                            else
                            {
                                InitialQCM(uutNo, 1);
                                InitialQCM(uutNo + 1, 1);
                                _runUUT[uutNo].Para.CtrlACON = 1;
                                _runUUT[uutNo].OnOff.TimeRun.CurQCM = EQCMChage.控制ACON;
                                _runUUT[uutNo].OnOff.TimeRun.WatchQCM.Restart();
                                _runUUT[uutNo].Para.AlarmTime = 0;
                                Log(_runUUT[uutNo].ToString() + CLanguage.Lan("检测异常断电,重启AC ON"), udcRunLog.ELog.NG);

                            }
                            continue;
                        }
                        else if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.控制ACON异常)
                        {
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                            _runUUT[uutNo].Para.AlarmTime = 0;
                        }
                    }
                    else
                    {
                        if (_runUUT[uutNo].Para.AlarmCode == EAlarmCode.控制ACON异常)
                        {
                            _runUUT[uutNo].Para.AlarmCode = EAlarmCode.正常;
                            _runUUT[uutNo].Para.AlarmTime = 0;
                        }
                    }

                    //治具空闲有输入AC
                    if (_runUUT[uutNo].Para.DoRun == EDoRun.位置空闲 || _runUUT[uutNo].Para.DoRun == EDoRun.位置禁用)
                    {
                        if (_runUUT[uutNo].Para.CtrlACON > 0)
                        {
                            if (_runUUT[uutNo].Para.AlarmTime < CGlobalPara.C_ALARM_TIME)
                            {
                                _runUUT[uutNo].Para.AlarmTime++;
                            }
                            else
                            {
                                SetACON(uutNo, 0);
                            }
                            continue;
                        }
                    }

                    _runUUT[uutNo].Para.AlarmTime = 0;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 更新监控数据
        /// </summary>
        /// <returns></returns>
        private bool UpdateMonitorSignal()
        {
            try
            {
                for (int uutNo = 0; uutNo < _runUUT.Count; uutNo++)
                {
                    //控制板数据更新

                    int iCom = _runUUT[uutNo].Base.ctrlCom;

                    int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                    if (_threadMon[iCom]._Mon[iAddr].Base.conStatus)
                    {
                        if (_threadMon[iCom]._Mon[iAddr].Para.rIO[EFMB_rIO.继电器粘连警告] == 1)
                        {
                            _runUUT[uutNo].Para.CtrlRunError = EAlarmCode.继电器粘连警告;
                        }
                        else if (_threadMon[iCom]._Mon[iAddr].Para.rIO[EFMB_rIO.S1状态] == 1)
                        {
                            _runUUT[uutNo].Para.CtrlRunError = EAlarmCode.S1状态ON;
                        }
                        else
                        {
                            _runUUT[uutNo].Para.CtrlRunError = EAlarmCode.正常;
                        }

                        _runUUT[uutNo].Para.CtrlACON = _threadMon[iCom]._Mon[iAddr].Para.rIO[EFMB_rIO.AC电压信号];

                        _runUUT[uutNo].Para.CtrlOnOff = _threadMon[iCom]._Mon[iAddr].Para.rIO[EFMB_rIO.AC同步信号];

                        _runUUT[uutNo].Para.CtrlACSignal = _threadMon[iCom]._Mon[iAddr].Para.rIO[EFMB_rIO.检测AC电压];

                        _runUUT[uutNo].Para.CtrlACVolt = _threadMon[iCom]._Mon[iAddr].Para.CurACVolt;

                        for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                        {
                            _runUUT[uutNo].Led[slot].monV = _threadMon[iCom]._Mon[iAddr].Para.Volt[slot];

                            _runUUT[uutNo].Para.CtrlUUTONLine = _threadMon[iCom]._Mon[iAddr].Para.rIO[EFMB_rIO.治具到位信号1];
                        }
                    }

                    //ERS模块数据更新

                    int ersCom = _runUUT[uutNo].Base.ersCom;

                    int ersAddr = _runUUT[uutNo].Base.ersAddr;

                    int ersCH = _runUUT[uutNo].Base.ersCH - 1;

                    if (_threadERS._Mon[ersAddr].Base.conStatus)
                    {
                        for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                        {
                            _runUUT[uutNo].Led[slot].monVBus = _threadERS._Mon[ersAddr].Data.chan[ersCH].volt;

                            _runUUT[uutNo].Led[slot].monA = _threadERS._Mon[ersAddr].Data.chan[ersCH].current;
                        }

                        _runUUT[uutNo].Para.CtrlVBus = _runUUT[uutNo].Led[0].monVBus;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 等待进机过程
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_wait_for_enter(int uutNo)
        {
            try
            {
                string er = string.Empty;

                if (_chmr.OP.DoRun != EHandStat.进机 || _chmr.OP.InPos != uutNo)
                    return;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    //测试参数
                    _runUUT[uutNo + idNo].Para.IdCard = _chmr.InPlatForm.Fixture[idNo].idCard;
                    _runUUT[uutNo + idNo].Para.IsNull = _chmr.InPlatForm.Fixture[idNo].IsFixNull;
                    _runUUT[uutNo + idNo].Para.MesFlag = _chmr.InPlatForm.Fixture[idNo].MesFlag;
                    _runUUT[uutNo + idNo].Para.OrderName = _chmr.InPlatForm.Fixture[idNo].orderName;
                    _runUUT[uutNo + idNo].Para.ModelName = _runModel.Base.Model;
                    _runUUT[uutNo + idNo].Para.BurnTime = (int)(_runModel.Para.BITime * 3600);
                    _runUUT[uutNo + idNo].Para.RunTime = 0;
                    _runUUT[uutNo + idNo].Para.OutLevel = 0;

                    if (CPara.GetOutPutAndOnOffFromModel(_runModel, ref _runUUT[uutNo + idNo].OnOff, out er))
                    {
                        _runUUT[uutNo + idNo].Para.OutPutChan = _runUUT[uutNo + idNo].OnOff.TimeSpec.OutChanNum;
                        _runUUT[uutNo + idNo].Para.OutPutNum = _runUUT[uutNo + idNo].OnOff.TimeSpec.OutPutNum;
                        _runUUT[uutNo + idNo].Para.OnOffNum = _runUUT[uutNo + idNo].OnOff.OnOff.Count;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurStepNo = 0;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunVolt = _runModel.OnOff[0].Item[0].ACV;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut = _runModel.OnOff[0].Item[0].OutPutType;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCType = _runModel.OutPut[_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut].Chan[0].QCType;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCV = _runModel.OutPut[_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut].Chan[0].QCV;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                    }

                    int outPutNo = _runUUT[uutNo + idNo].OnOff.OnOff[0].OutPutType;

                    //输出
                    for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                    {
                        _runUUT[uutNo + idNo].Led[slot].serialNo = _chmr.InPlatForm.Fixture[idNo].serialNo[slot];
                        _runUUT[uutNo + idNo].Led[slot].vName = _runModel.OutPut[outPutNo].Chan[0].Vname;
                        _runUUT[uutNo + idNo].Led[slot].vMin = _runModel.OutPut[outPutNo].Chan[0].Vmin;
                        _runUUT[uutNo + idNo].Led[slot].vMax = _runModel.OutPut[outPutNo].Chan[0].Vmax;
                        _runUUT[uutNo + idNo].Led[slot].IMode = _runModel.OutPut[outPutNo].Chan[0].Imode;
                        _runUUT[uutNo + idNo].Led[slot].ISet = _runModel.OutPut[outPutNo].Chan[0].ISet;
                        _runUUT[uutNo + idNo].Led[slot].Imin = _runModel.OutPut[outPutNo].Chan[0].Imin;
                        _runUUT[uutNo + idNo].Led[slot].Imax = _runModel.OutPut[outPutNo].Chan[0].Imax;
                        _runUUT[uutNo + idNo].Led[slot].qcv = _runModel.OutPut[outPutNo].Chan[0].QCV;
                        _runUUT[uutNo + idNo].Led[slot].unitV = _runUUT[idNo].Led[slot].qcv;
                        _runUUT[uutNo + idNo].Led[slot].unitA = _runUUT[idNo].Led[slot].ISet;
                        _runUUT[uutNo + idNo].Led[slot].passResult = 0;
                        _runUUT[uutNo + idNo].Led[slot].failResult = 0;
                        _runUUT[uutNo + idNo].Led[slot].failEnd = 0;
                        _runUUT[uutNo + idNo].Led[slot].failTime = "";
                        _runUUT[uutNo + idNo].Led[slot].failInfo = "";
                    }

                    clearLoadVal(uutNo + idNo);
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo].Para.IdCard + "】"+ CLanguage.Lan("正在进机"));

                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.正在进机;

                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;

                    UpdateInPosUI(_runUUT[uutNo + idNo].Base.localName);

                    RefreshSlotUI(uutNo + idNo);
                 
                    local_db_update_in_bi(uutNo + idNo);
                }

                if (!ControlToPLC_InPos(uutNo, out er))
                {
                    _runUUT[uutNo].Para.AlarmInfo = CLanguage.Lan("进机异常");
                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("进机异常") + ":" + er, udcRunLog.ELog.NG);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 进机过程中
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_enter_into_bi(int uutNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Para.ChkAutoSelf)
                {
                    if (_runUUT[uutNo].Para.CtrlUUTONLine == 0 || _runUUT[uutNo + 1].Para.CtrlUUTONLine == 0)
                    {
                        if (_chmr.PLC.handStat == EHandStat.空闲 && ReadTimerOut(uutNo, CLanguage.Lan("进机超时")))
                        {
                            ControlSelfPos(uutNo, CLanguage.Lan("检测不到治具到位信号"));
                        }
                        return;
                    }
                }

                if (_runUUT[uutNo].Para.CtrlUUTONLine == 1 && _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1)
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        LoadLedItem(uutNo + idNo);

                        LoadLedValue(uutNo + idNo);

                        _runUUT[uutNo + idNo].Para.DoRun = EDoRun.进机完毕;

                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                        RefreshSlotUI(uutNo + idNo);

                        SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+ CLanguage.Lan("进机完毕"));

                        local_db_update_fix_status(uutNo + idNo);

                    }

                    RefreshCurModelList();
                }
                else
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        if (ReadTimerOut(uutNo + idNo, CLanguage.Lan("进机超时")))
                            _runUUT[uutNo + idNo].Para.AlarmInfo = CLanguage.Lan("进机超时");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 启动老化测试
        /// </summary>
        private bool uut_start_bi_test(int uutNo)
        {
            try
            {
                string er = string.Empty;

                //2个治具都为空治具
                if (_runUUT[uutNo].Para.IsNull == 1 && _runUUT[uutNo + 1].Para.IsNull == 1)
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].Para.DoRun = EDoRun.空治具到位;

                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                        RefreshSlotUI(uutNo + idNo);

                        SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") +"【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+ CLanguage.Lan("空治具到位"));

                        local_db_update_fix_status(uutNo + idNo);

                        _chmr.FixYield.InBIFixNo++;

                        UpdateInBIFixNumUI();
                    }
                    return true;
                }

                //治具需要老化测试
                for (int idNo = 0; idNo < 2; idNo++)
                {
                    _runUUT[uutNo + idNo].Para.RunTime = 0;
                    _runUUT[uutNo + idNo].Para.OutLevel = 0;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurStepNo = 0;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut = _runUUT[uutNo + idNo].OnOff.OnOff[0].OutPutType;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunVolt = _runModel.OnOff[0].Item[0].ACV;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                    int outPutNo = _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCType = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].QCType;
                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCV = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].QCV;
                    for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                    {
                        _runUUT[uutNo + idNo].Led[slot].vName = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].Vname;
                        _runUUT[uutNo + idNo].Led[slot].vMin = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].Vmin;
                        _runUUT[uutNo + idNo].Led[slot].vMax = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].Vmax;
                        _runUUT[uutNo + idNo].Led[slot].IMode = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].Imode;
                        _runUUT[uutNo + idNo].Led[slot].ISet = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].ISet;
                        _runUUT[uutNo + idNo].Led[slot].Imin = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].Imin;
                        _runUUT[uutNo + idNo].Led[slot].Imax = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].Imax;
                        _runUUT[uutNo + idNo].Led[slot].qcv = _runUUT[uutNo + idNo].OnOff.OutPut[outPutNo].Chan[0].QCV;
                        _runUUT[uutNo + idNo].Led[slot].unitV = _runUUT[uutNo + idNo].Led[slot].qcv;
                        _runUUT[uutNo + idNo].Led[slot].unitA = _runUUT[uutNo + idNo].Led[slot].ISet;
                        _runUUT[uutNo + idNo].Led[slot].passResult = 0;
                        _runUUT[uutNo + idNo].Led[slot].failResult = 0;
                        _runUUT[uutNo + idNo].Led[slot].failEnd = 0;
                        _runUUT[uutNo + idNo].Led[slot].failTime = "";
                        _runUUT[uutNo + idNo].Led[slot].failInfo = "";
                    }

                    InitialQCM(uutNo + idNo, 1);

                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.启动老化;

                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                    _runUUT[uutNo + idNo].Para.AlarmTime = 0;

                    SetTimer(uutNo + idNo, "治具【" + _runUUT[uutNo + idNo].Para.IdCard + "】启动老化");
                       
                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_in_bi(uutNo + idNo);

                    local_db_add_usedNum(uutNo + idNo);

                    _chmr.FixYield.InBIFixNo++;

                    UpdateInBIFixNumUI();

                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 治具自检过程
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_self_testing(int uutNo)
        {
            try
            {

                //检测AC ON信号是否存在

                if (_runUUT[uutNo].Para.CtrlOnOff != 1)
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        if (_runUUT[uutNo + idNo].Para.IsNull == 1)
                            continue;

                        if (ReadTimerOut(uutNo + idNo, CLanguage.Lan("启动老化超时:AC同步信号异常")))
                        {
                            _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.AC同步信号异常;
                            _runUUT[uutNo + idNo].Para.AlarmInfo =  CLanguage.Lan("AC同步信号异常");
                        }
                    }
                    return;
                }

                bool ChkFlag = true;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[uutNo + idNo].Para.CtrlOnOff != 1)
                    {
                        if (_runUUT[uutNo + idNo].Para.IsNull == 1)
                            continue;
                        if (ReadTimerOut(uutNo + idNo, CLanguage.Lan("启动老化超时")))
                        {
                            _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.AC同步信号异常;
                            _runUUT[uutNo + idNo].Para.AlarmInfo = CLanguage.Lan("AC同步信号异常");
                        }
                        ChkFlag = false;
                        continue;
                    }

                    int uutNum = 0;

                    int monNum = 0;

                    for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                    {
                        if (_runUUT[uutNo + idNo].Led[slot].serialNo != string.Empty)
                        {
                            if (_runUUT[uutNo + idNo].Led[slot].monV > 2)
                                monNum++;
                            uutNum++;
                        }
                    }

                    //检测产品数量是否与输出数量一致?
                    if (uutNum != monNum)
                    {
                        if (!ReadTimerOut(uutNo, CLanguage.Lan("启动老化超时:自检电压异常"),CGlobalPara.SysPara.Alarm.Chk_qcv_times))
                        {
                            ChkFlag = false;
                            continue;
                        }
                    }
                }

                if (ChkFlag)
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        SetQCM(uutNo + idNo, (EQCM)_runUUT[uutNo + idNo].OnOff.TimeRun.CurQCType,
                                             _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCV,
                                             _runUUT[uutNo + idNo].Led[0].ISet);

                        _runUUT[uutNo + idNo].Para.DoRun = EDoRun.老化自检;

                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                        RefreshSlotUI(uutNo + idNo);

                        SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+ CLanguage.Lan("开始老化自检"));

                        local_db_update_fix_status(uutNo + idNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 治具老化中
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_bi_testing(int uutNo)
        {
            try
            {
                string er = string.Empty;

                bool CheckQCM = true;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[uutNo + idNo].Para.IsNull == 1)
                        continue;

                    int uut_Num = 0;

                    int qcm_Num = 0;

                    for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                    {
                        if (_runUUT[uutNo + idNo].Led[slot].serialNo != string.Empty && _runUUT[uutNo + idNo].Led[slot].monV > 2)
                        {
                            if (_runUUT[uutNo + idNo].Led[slot].monV >= _runUUT[uutNo + idNo].Led[slot].vMin &&
                                _runUUT[uutNo + idNo].Led[slot].monV <= _runUUT[uutNo + idNo].Led[slot].vMax)
                                qcm_Num++;
                            uut_Num++;
                        }
                    }

                    if (qcm_Num != uut_Num)
                    {
                        if (!ReadTimerOut(uutNo + idNo, CLanguage.Lan("老化自检超时")))
                            CheckQCM = false;
                    }
                }

                //快充模式等待快充电压
                if (!CheckQCM && _runUUT[uutNo].OnOff.TimeRun.CurQCType != (int)EQCM.Normal &&
                                 _runUUT[uutNo].OnOff.TimeRun.CurQCType!=(int)EQCM.Reserve) 
                {
                    if ((EQCM)_runUUT[uutNo].OnOff.TimeRun.CurQCType == EQCM.MTK1_0) //上升电压
                    {
                        for (int idNo = 0; idNo < 2; idNo++)
                        {
                            if (_runUUT[uutNo + idNo].Para.IsNull == 1)
                                continue;

                            setMTK(uutNo + idNo);
                        }
                    }
                    return;
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    _runUUT[uutNo + idNo].Para.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    _runUUT[uutNo + idNo].Para.EndTime = DateTime.Now.AddSeconds(_runUUT[uutNo + idNo].Para.BurnTime).ToString("yyyy/MM/dd HH:mm:ss");
                    _runUUT[uutNo + idNo].Para.SaveDataTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    _runUUT[uutNo + idNo].Para.SaveFileName = _runUUT[uutNo + idNo].Para.ModelName + "_" + _runUUT[uutNo + idNo].Para.IdCard +
                                                              "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                    
                    if (CGlobalPara.SysPara.Report.ReportPath == string.Empty)
                        CGlobalPara.SysPara.Report.ReportPath = Application.StartupPath + "\\Report";
                    
                    if (!System.IO.Directory.Exists(CGlobalPara.SysPara.Report.ReportPath))
                        System.IO.Directory.CreateDirectory(CGlobalPara.SysPara.Report.ReportPath);

                    _runUUT[uutNo + idNo].Para.SavePathName = CGlobalPara.SysPara.Report.ReportPath + "\\" + DateTime.Now.ToString("yyyyMMdd") +
                                                                                                      "\\" + _runUUT[uutNo + idNo].Para.ModelName;

                    if (CGlobalPara.SysPara.Dev.CtrlACMode == ECtrlACMode.上位机控制时序)
                        _runUUT[uutNo + idNo].Para.IniRunTime = _CtrlAC.RunTime;
                    else
                        _runUUT[uutNo + idNo].Para.IniRunTime = 0;

                    _runUUT[uutNo + idNo].Para.RunTime = 0;

                    _runUUT[uutNo + idNo].Para.RunACVolt = _runUUT[uutNo + idNo].Para.CtrlACVolt;

                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.正在老化;

                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                    RefreshSlotUI(uutNo + idNo);

                    SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+ CLanguage.Lan("开始老化"));

                    setLoadVal(uutNo + idNo);

                    local_db_update_bi_start(uutNo + idNo);
                }

                ReNewChartFile(_runUUT[uutNo].Base.fixNo - 1);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 老化运行中
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_bi_burning(int uutNo)
        {
            try
            {
                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[uutNo + idNo].Para.IsNull == 1)
                        continue;

                    if (_runUUT[uutNo + idNo].Para.CtrlOnOff == 0)
                        continue;

                    if (_runUUT[uutNo + idNo].Para.CtrlACON == 0)
                        continue;

                    if (_runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM != EQCMChage.空闲)
                        continue;

                    judge_bi_volt(uutNo + idNo);

                    judge_bi_current(uutNo + idNo);

                    judge_bi_result(uutNo + idNo);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 老化结束
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_bi_test_over(int uutNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Para.ChkAutoSelf)
                {
                    if (_chmr.PLC.handStat == EHandStat.空闲)
                    {
                        ControlSelfPos(uutNo, "OK");
                    }

                    return;
                }

                int dailyTTNum = 0;

                int dailyFailNum = 0;

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    if (_runUUT[uutNo + idNo].Para.IsNull == 0)
                    {
                        string er = string.Empty;

                        Stopwatch watcher = new Stopwatch();

                        watcher.Start();

                        CWeb2.CFixture fixture = new CWeb2.CFixture();
                        fixture.Base.FlowIndex = 0;
                        fixture.Base.FlowName = CYOHOOApp.BI_FlowName;
                        fixture.Base.FlowGuid = CNet.HostName();
                        fixture.Base.SnType = CWeb2.ESnType.外部条码;
                        fixture.Base.IdCard = _runUUT[uutNo + idNo].Para.IdCard;
                        fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                        string endTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        if (CGlobalPara.SysPara.Para.BITimeRate != 1 && _runUUT[uutNo + idNo].Para.StartTime != string.Empty)
                        {
                            DateTime d = System.Convert.ToDateTime(_runUUT[uutNo + idNo].Para.StartTime);

                            endTime = d.AddSeconds(_runUUT[uutNo + idNo].Para.BurnTime).ToString("yyyy/MM/dd HH:mm:ss");
                        }

                        _runUUT[uutNo + idNo].Para.EndTime = endTime;

                        _runUUT[uutNo + idNo].Para.RunTime = _runUUT[uutNo + idNo].Para.BurnTime;

                        for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                        {
                            CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                            para.SlotNo = slot;
                            para.SerialNo = _runUUT[uutNo + idNo].Led[slot].serialNo;
                            para.InnerSn = string.Empty;
                            para.Remark1 = _runUUT[uutNo + idNo].Para.ModelName;
                            para.Remark2 = _runUUT[uutNo + idNo].Base.localName;
                            para.StartTime = _runUUT[uutNo + idNo].Para.StartTime;
                            para.EndTime = _runUUT[uutNo + idNo].Para.EndTime;
                            para.Result = 0;
                            para.TestData = string.Empty;
                            if (_runUUT[uutNo + idNo].Led[slot].serialNo != string.Empty)
                            {
                                if (_runUUT[uutNo + idNo].Led[slot].failResult == 0)
                                {
                                    para.Result = 0;
                                }
                                else
                                {
                                    para.Result = CYOHOOApp.BI_FlowId;
                                    dailyFailNum++;
                                }

                                CTestData UUT = new CTestData();

                                UUT.UUT = new List<CTestVal>();

                                UUT.UUT.Add(new CTestVal()
                                {
                                    Vname = _runUUT[uutNo + idNo].Led[slot].vName,
                                    Volt = _runUUT[uutNo + idNo].Led[slot].unitV,
                                    Current = _runUUT[uutNo + idNo].Led[slot].unitA,
                                    Result = para.Result,
                                    ACV = _runModel.OnOff[0].Item[0].ACV,
                                    Temp = _chmr.PLC.rTemp
                                });

                                para.TestData = GJ.COM.CJSon.Serializer<CTestData>(UUT);

                                dailyTTNum++;
                            }

                            fixture.Para.Add(para);
                        }

                        if (!CWeb2.UpdateFixtureResult(fixture, out er))
                        {
                            MessageBox.Show(_runUUT[uutNo + idNo].ToString() + CLanguage.Lan("写入治具ID")+ "[" + _runUUT[uutNo + idNo].Para.IdCard + "]"+ 
                                                                               CLanguage.Lan("测试结果错误:") + er);

                            if (!CWeb2.UpdateFixtureResult(fixture, out er))
                            {
                                Log(_runUUT[uutNo + idNo].ToString() + CLanguage.Lan("写入治具ID") + "[" + _runUUT[uutNo + idNo].Para.IdCard + "]"+ 
                                                                       CLanguage.Lan("测试结果错误:") + er, udcRunLog.ELog.NG);
                            }
                        }
                        else
                        {
                            Log(_runUUT[uutNo + idNo].ToString() + CLanguage.Lan("写入治具ID") + "[" + _runUUT[uutNo + idNo].Para.IdCard + "]"+
                                                                   CLanguage.Lan("测试结果OK") + ":" + watcher.ElapsedMilliseconds.ToString() + "ms",
                                                                   udcRunLog.ELog.OK);                                                                  
                        }

                        local_db_recordFailSn(uutNo + idNo);
                    }

                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.老化结束;

                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                    local_db_set_bi_end(uutNo + idNo);

                    RefreshSlotUI(uutNo + idNo);

                    Log(_runUUT[uutNo + idNo].ToString() + CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+
                                                           CLanguage.Lan("老化结束,等待出机."), udcRunLog.ELog.Action);
                }

                if (CGlobalPara.SysPara.Mes.Connect)
                {
                    Task t1 = new Task(() => TranSnToMES(uutNo));

                    Task t2 = t1.ContinueWith(new Action<Task>(t => TranSnToMES(uutNo + 1)), TaskContinuationOptions.None);

                    t1.Start();
                }

                //记录产能统计

                dailyRecord(dailyTTNum, dailyFailNum);

                yieldRecord(dailyTTNum, dailyFailNum);

                RefreshCurModelList();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 老化出机
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_bi_out_from_bi(int uutNo)
        {
            try
            {
                if (_chmr.OP.DoRun == EHandStat.出机 && _chmr.OP.OutPos == uutNo)
                {
                    string er = string.Empty;

                    if (!ControlPLC_OutPos(uutNo, out er))
                    {
                        for (int idNo = 0; idNo < 2; idNo++)
                        {
                            _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.出机异常;
                            _runUUT[uutNo + idNo].Para.AlarmInfo = CLanguage.Lan("出机异常");
                        }
                        return;
                    }

                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].Para.DoRun = EDoRun.正在出机;

                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                        RefreshSlotUI(uutNo + idNo);

                        SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+ CLanguage.Lan("正在出机中"));

                        local_db_update_fix_status(uutNo + idNo);

                        _chmr.FixYield.OutBIFixNo++;

                        UpdateOutBIFixNumUI();
                    }

                    UpdateOutPosUI(_runUUT[uutNo].Base.localName);

                    //记录当前出机机种

                    _chmr.CurOutModel = _runUUT[uutNo].Para.ModelName;

                    CIniFile.WriteToIni("Parameter", "CurOutModel", _chmr.CurOutModel, CGlobalPara.IniFile);

                    RefreshCurModelList();
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 出机过程
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_out_bi_ready(int uutNo)
        {
            try
            {
                if (_runUUT[uutNo].Para.CtrlUUTONLine == 0 && _runUUT[uutNo + 1].Para.CtrlUUTONLine == 0)
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].Para.DoRun = EDoRun.位置空闲;

                        _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;

                        _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;

                        RefreshSlotUI(uutNo + idNo);

                        local_db_update_fix_status(uutNo + idNo);

                        SetTimer(uutNo + idNo, CLanguage.Lan("治具ID") + "【" + _runUUT[uutNo + idNo].Para.IdCard + "】"+ CLanguage.Lan("出机完毕,等待下一治具"));
                    }
                }
                else
                {
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        if (ReadTimerOut(uutNo + idNo, CLanguage.Lan("出机超时")))
                            _runUUT[uutNo + idNo].Para.AlarmInfo = CLanguage.Lan("出机超时");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 测试数据处理
        /// <summary>
        /// 判断电压规格
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private void judge_bi_volt(int uutNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Para.VLP == 0)
                    CGlobalPara.SysPara.Para.VLP = 1;

                if (CGlobalPara.SysPara.Para.VHP == 0)
                    CGlobalPara.SysPara.Para.VHP = 1;

                for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                {
                    if (_runUUT[uutNo].Led[slot].serialNo == string.Empty)
                        continue;

                    _runUUT[uutNo].Led[slot].passResult = 0;

                    if (_runUUT[uutNo].Led[slot].monV < _runUUT[uutNo].Led[slot].vMin ||
                        _runUUT[uutNo].Led[slot].monV > _runUUT[uutNo].Led[slot].vMax)
                    {
                        //产品原先PASS,处理当前当机状态
                        if (_runUUT[uutNo].Led[slot].failResult == 0)
                        {
                            if (_runUUT[uutNo].Led[slot].monV > _runUUT[uutNo].Led[slot].vMin * CGlobalPara.SysPara.Para.VLP &&
                                _runUUT[uutNo].Led[slot].monV < _runUUT[uutNo].Led[slot].vMin)  //下限补偿  
                            {
                                _runUUT[uutNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].vBack;
                            }
                            else if (_runUUT[uutNo].Led[slot].monV > _runUUT[uutNo].Led[slot].vMax &&   //上限补偿 
                                     _runUUT[uutNo].Led[slot].monV < _runUUT[uutNo].Led[slot].vMax * CGlobalPara.SysPara.Para.VHP)
                            {
                                _runUUT[uutNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].vBack;
                            }
                            else
                            {
                                if (_runUUT[uutNo].Led[slot].vFailNum > CGlobalPara.SysPara.Alarm.FailTimes)
                                {
                                    _runUUT[uutNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].monV;
                                    _runUUT[uutNo].Led[slot].passResult = 1;
                                }
                                else
                                {
                                    _runUUT[uutNo].Led[slot].vFailNum++;
                                }
                            }
                        }
                        else
                        {
                            _runUUT[uutNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].monV;
                            _runUUT[uutNo].Led[slot].passResult = 1;
                        }
                    }
                    else
                    {
                        _runUUT[uutNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].monV;
                        _runUUT[uutNo].Led[slot].vFailNum = 0;
                    }

                    _runUUT[uutNo].Led[slot].vBack = _runUUT[uutNo].Led[slot].unitV;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 判断电流规格
        /// </summary>
        /// <param name="uutNo"></param>
        /// <returns></returns>
        private void judge_bi_current(int uutNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Para.ILP == 0)
                    CGlobalPara.SysPara.Para.ILP = 1;

                if (CGlobalPara.SysPara.Para.IHP == 0)
                    CGlobalPara.SysPara.Para.IHP = 1;

                for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                {
                    if (_runUUT[uutNo].Led[slot].serialNo == string.Empty)
                        continue;

                    if (_runUUT[uutNo].Led[slot].monA < _runUUT[uutNo].Led[slot].Imin ||
                        _runUUT[uutNo].Led[slot].monA > _runUUT[uutNo].Led[slot].Imax)
                    {
                        //产品原先PASS,处理当前当机状态
                        if (_runUUT[uutNo].Led[slot].failResult == 0)
                        {
                            if (_runUUT[uutNo].Led[slot].monA > _runUUT[uutNo].Led[slot].Imin * CGlobalPara.SysPara.Para.ILP &&
                                _runUUT[uutNo].Led[slot].monA < _runUUT[uutNo].Led[slot].Imin)  //下限补偿 
                            {
                                _runUUT[uutNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].iBack;
                            }
                            else if (_runUUT[uutNo].Led[slot].monA > _runUUT[uutNo].Led[slot].Imax &&   //上限补偿 
                                     _runUUT[uutNo].Led[slot].monA < _runUUT[uutNo].Led[slot].Imax * CGlobalPara.SysPara.Para.IHP)
                            {
                                _runUUT[uutNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].iBack;
                            }
                            else
                            {
                                if (_runUUT[uutNo].Led[slot].iFailNum > CGlobalPara.SysPara.Alarm.FailTimes)
                                {
                                    _runUUT[uutNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].monA;
                                    _runUUT[uutNo].Led[slot].passResult = 1;
                                }
                                else
                                {
                                    _runUUT[uutNo].Led[slot].iFailNum++;
                                }
                            }
                        }
                        else
                        {
                            _runUUT[uutNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].monA;
                            _runUUT[uutNo].Led[slot].passResult = 1;
                        }
                    }
                    else
                    {
                        _runUUT[uutNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].monA;
                        _runUUT[uutNo].Led[slot].iFailNum = 0;
                    }

                    if (_runUUT[uutNo].Led[slot].unitV < 2)
                        _runUUT[uutNo].Led[slot].unitA = 0;

                    _runUUT[uutNo].Led[slot].iBack = _runUUT[uutNo].Led[slot].unitA;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 判断老化结果
        /// </summary>
        /// <param name="uutNo"></param>
        /// <returns></returns>
        private void judge_bi_result(int uutNo)
        {
            try
            {
                for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                {
                    if (_runUUT[uutNo].Led[slot].serialNo == string.Empty)
                        continue;

                    if (_runUUT[uutNo].Led[slot].passResult == 0)
                    {
                        if (CGlobalPara.SysPara.Para.ChkNoLockFail) //不锁住当机->可恢复为良品
                        {
                            Random t = new Random();

                            for (int i = 0; i < _runUUT[uutNo].Led[slot].valJson.Value.Count; i++)
                            {
                                if (_runUUT[uutNo].Led[slot].valJson.Value[i].FailEnd == 1)
                                {
                                    double middle = (_runUUT[uutNo].Para.valJson.Value[i].Vmin + _runUUT[uutNo].Para.valJson.Value[i].Vmax) / 2;
                                    middle = (middle - 0.1) + 0.2 * t.NextDouble();
                                    _runUUT[uutNo].Led[slot].valJson.Value[i].UnitV = middle;
                                    _runUUT[uutNo].Led[slot].valJson.Value[i].UnitA = _runUUT[uutNo].Para.valJson.Value[i].ISet;
                                    _runUUT[uutNo].Led[slot].valJson.Value[i].Result = 0;
                                    _runUUT[uutNo].Led[slot].valJson.Value[i].FailEnd = 0;
                                    _runUUT[uutNo].Led[slot].valJson.Value[i].FailTime = string.Empty;
                                }
                            }                        
                            _runUUT[uutNo].Led[slot].failResult = 0;
                            _runUUT[uutNo].Led[slot].failEnd = 0;
                            _runUUT[uutNo].Led[slot].failTime = "";
                            _runUUT[uutNo].Led[slot].failInfo = "";
                        }
                        else
                        {
                            int stepNo = (_runUUT[uutNo].OnOff.TimeRun.CurRunOutPut >= 0 ? _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut : 0);

                            if (_runUUT[uutNo].Led[slot].valJson.Value.Count > stepNo)
                            {
                                if (_runUUT[uutNo].Led[slot].valJson.Value[stepNo].FailEnd == 0)
                                {
                                    _runUUT[uutNo].Led[slot].valJson.Value[stepNo].UnitV = System.Convert.ToDouble(_runUUT[uutNo].Led[slot].unitV.ToString("0.000"));
                                    _runUUT[uutNo].Led[slot].valJson.Value[stepNo].UnitA = System.Convert.ToDouble(_runUUT[uutNo].Led[slot].unitA.ToString("0.00"));
                                }
                            }

                            if (_runUUT[uutNo].Led[slot].failResult == 1)
                            {
                                _runUUT[uutNo].Led[slot].failResult = 2;  //锁住当机
                            }
                        }
                    }
                    else
                    {
                        int stepNo = (_runUUT[uutNo].OnOff.TimeRun.CurRunOutPut>=0?_runUUT[uutNo].OnOff.TimeRun.CurRunOutPut:0);

                        if (_runUUT[uutNo].Led[slot].valJson.Value.Count > stepNo)
                        {
                            if (_runUUT[uutNo].Led[slot].valJson.Value[stepNo].FailEnd == 0)
                            {
                                _runUUT[uutNo].Led[slot].valJson.Value[stepNo].Result = 1;
                                _runUUT[uutNo].Led[slot].valJson.Value[stepNo].UnitV = System.Convert.ToDouble(_runUUT[uutNo].Led[slot].unitV.ToString("0.000"));
                                _runUUT[uutNo].Led[slot].valJson.Value[stepNo].UnitA = System.Convert.ToDouble(_runUUT[uutNo].Led[slot].unitA.ToString("0.00"));
                                _runUUT[uutNo].Led[slot].valJson.Value[stepNo].FailEnd = 1;
                                _runUUT[uutNo].Led[slot].valJson.Value[stepNo].FailTime = DateTime.Now.ToString("HH:mm:ss");
                            }
                        }

                        if (_runUUT[uutNo].Led[slot].failResult == 0)
                        {
                            _runUUT[uutNo].Led[slot].failResult = 1;
                        }

                        if (_runUUT[uutNo].Led[slot].failEnd == 0)
                        {
                            _runUUT[uutNo].Led[slot].failEnd = 1;
                            _runUUT[uutNo].Led[slot].failTime = DateTime.Now.ToString("HH:mm:ss");
                            _runUUT[uutNo].Led[slot].failInfo = _runUUT[uutNo].Led[slot].vName + ":" +
                                                               CLanguage.Lan("电压") + "=" + _runUUT[uutNo].Led[slot].unitV.ToString("0.000") + "V;" +
                                                               CLanguage.Lan("电流") + "=" + _runUUT[uutNo].Led[slot].unitA.ToString("0.00") + "A";
                                                               

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 扫描监控
        /// <summary>
        /// 采集时间监控
        /// </summary>
        private Stopwatch monWatcher = new Stopwatch();
        /// <summary>
        /// 等待同步扫描结束
        /// </summary>
        /// <returns></returns>
        private bool WaitMonitorComplete()
        {
            try
            {
                if (!CGlobalPara.C_SCAN_START)   //未启动扫描->启动扫描
                {
                    monWatcher.Restart();
                    monWatcher.Restart();

                    //启动监控同步扫描线程
                    for (int i = 0; i < _threadMon.Length; i++)
                        _threadMon[i].Continued();

                   _threadERS.Continued();

                    //异步委托更新数据

                   if (!db_asyn)
                   {
                       update_local_db_handler update_db = new update_local_db_handler(local_db_update_para_intoJson);

                       update_db.BeginInvoke(null, null);
                   }

                    CGlobalPara.C_SCAN_START = true;

                    return false;
                }
                else
                {
                    //检测扫描线程是否结束?

                    bool EndFlag = true;

                    for (int i = 0; i < _threadMon.Length; i++)
                    {
                        if (_threadMon[i].threadStatus != DEV.FCMB.EThreadStatus.暂停)
                            EndFlag = false;
                    }

                    if (_threadERS.threadStatus != DEV.LED.EThreadStatus.暂停)
                        EndFlag = false;
                    

                    //检测更新数据是否结束?
                    if (db_asyn)
                        return false;

                    if (!EndFlag)
                        return false;

                    monWatcher.Stop();

                    RefreshMonTimeUI(monWatcher.ElapsedMilliseconds);

                    Log(CLanguage.Lan("扫描监控周期") + "=" + ((double)monWatcher.ElapsedMilliseconds / 1000).ToString("0.0") + "秒",
                                                              udcRunLog.ELog.Action, false);

                    CGlobalPara.C_SCAN_START = false;

                    CGlobalPara.C_INI_SCAN = true;

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 动作时间
        /// <summary>
        /// 设置时间
        /// </summary>
        /// <param name="uutNo"></param>
        private void SetTimer(int uutNo, string info)
        {
            string localName = "<" + _runUUT[uutNo].Base.localName + ">";
            _runUUT[uutNo].Para.WaitAlarm = false;
            _runUUT[uutNo].Para.WaitTimer.Restart();
            _runUUT[uutNo].Para.WaitInfo = localName + info;
            Log(_runUUT[uutNo].Para.WaitInfo, udcRunLog.ELog.Action);
        }
        /// <summary>
        /// 读设置时间超时
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="info"></param>
        /// <param name="wTimeOut"></param>
        /// <returns></returns>
        private bool ReadTimerOut(int uutNo, string info = "",int waitTime=0)
        {

            if (!_runUUT[uutNo].Para.WaitTimer.IsRunning)
                _runUUT[uutNo].Para.WaitTimer.Restart();

            if (waitTime == 0)
            {
                waitTime = CGlobalPara.SysPara.Alarm.OP_AlarmDelayS * 1000;
            }

            if (_runUUT[uutNo].Para.WaitTimer.ElapsedMilliseconds > waitTime)
            {
                if (!_runUUT[uutNo].Para.WaitAlarm)
                {
                    _runUUT[uutNo].Para.WaitAlarm = true;
                    if (info != "")
                    {
                        string localName = "<" + _runUUT[uutNo].Base.localName + ">";
                        Log(localName + info, udcRunLog.ELog.NG);
                    }
                    else
                        Log(_runUUT[uutNo].Para.WaitInfo, udcRunLog.ELog.NG);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region 老化入口取机平台任务
        /// <summary>
        /// 入口取机平台任务
        /// </summary>
        private void InPlatIdReadyTask()
        {
            try
            {
                Log(_chmr.Entrance.ToString() +  CLanguage.Lan("任务开始"), udcRunLog.ELog.Action);

                _chmr.Entrance.Wacther.Restart();

                string er = string.Empty;

                if (!readPlatIdCard())
                    return;

                if (!checkInBIModel())
                    return;

                if (!checkInBIACV())
                    return;

                if (!local_db_save_fixture(out er))
                {
                    _chmr.Entrance.DoRun = ERun.报警;
                    _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 0), (int)EPLC_RESULT.结果NG);
                    _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 1), (int)EPLC_RESULT.结果NG);
                    Log(_chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "【" + _chmr.Entrance.Fixture[0].idCard + "】"+
                                                   CLanguage.Lan("与治具ID") + "【" + _chmr.Entrance.Fixture[1].idCard + "】:" + er, udcRunLog.ELog.NG);                                                                                  
                    return;
                }

                //设置进站标志
                _threadPLC.addREGWrite(EPLCOUT.老化入口平台1结果.ToString(), (int)EPLC_RESULT.结果OK);

                _threadPLC.addREGWrite(EPLCOUT.老化入口平台2结果.ToString(), (int)EPLC_RESULT.结果OK);

                _chmr.Entrance.DoRun = ERun.进站;

                Log(_chmr.Entrance.ToString() + CLanguage.Lan("写入治具ID状态OK"), udcRunLog.ELog.Action);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                _chmr.Entrance.Wacther.Stop();

                string waitTime = ((double)_chmr.Entrance.Wacther.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                Log(_chmr.Entrance.ToString() + CLanguage.Lan("任务结束") + ":" + waitTime, udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 获取入口治具ID信息
        /// </summary>
        /// <returns></returns>
        private bool readPlatIdCard()
        {
            try
            {
                string er = string.Empty;

                string alarmInfo = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                Log(_chmr.Entrance.ToString() + CLanguage.Lan("老化入口平台治具就绪,获取治具信息."), udcRunLog.ELog.Action);

                for (int idNo = 0; idNo < _chmr.Entrance.Fixture.Count; idNo++)
                {
                    string rIdCard = string.Empty;

                    if (!_devIDCard.ReadIdCard(idNo + 1, out rIdCard, out er, CGlobalPara.SysPara.Para.IdTimes, true, 500))
                    {
                        _chmr.Entrance.DoRun = ERun.报警;
                        _chmr.Entrance.AlarmInfo = CLanguage.Lan("读卡") + (idNo + 1).ToString() + CLanguage.Lan("错误");
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, idNo), (int)EPLC_RESULT.结果NG);
                        alarmInfo = _chmr.Entrance.ToString() + CLanguage.Lan("读取治具ID") + (idNo + 1).ToString() + CLanguage.Lan("失败");
                        Log(alarmInfo, udcRunLog.ELog.NG);
                        MessageBox.Show(alarmInfo,"Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);   
                        return false;
                    }

                    _chmr.Entrance.Fixture[idNo].curACV = _runModel.OnOff[0].Item[0].ACV;

                    _chmr.Entrance.Fixture[idNo].idCard = rIdCard;

                    _chmr.Entrance.Fixture[idNo].IsFixNull = 0;

                    CWeb2.CFixture fixture = null;

                    if (!CWeb2.GetFixtureInfoByIdCard(_chmr.Entrance.Fixture[idNo].idCard, out fixture, out er))
                    {
                        _chmr.Entrance.DoRun = ERun.报警;
                        _chmr.Entrance.AlarmInfo = CLanguage.Lan("Web错误");
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, idNo), (int)EPLC_RESULT.结果NG);
                        alarmInfo = _chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "【" + _chmr.Entrance.Fixture[idNo].idCard + "】"+
                                    CLanguage.Lan("获取产品信息错误:") + er;                                                                  
                        Log(alarmInfo, udcRunLog.ELog.NG);
                        MessageBox.Show(alarmInfo, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);  
                        return false;
                    }

                    _chmr.Entrance.Fixture[idNo].IsFixNull = (int)fixture.Base.FixtureType;
                    _chmr.Entrance.Fixture[idNo].modelName = fixture.Base.Model;
                    _chmr.Entrance.Fixture[idNo].orderName = fixture.Base.OrderName;
                    _chmr.Entrance.Fixture[idNo].MesFlag = fixture.Base.MesFlag;
                    for (int i = 0; i < _chmr.Entrance.Fixture[idNo].serialNo.Count; i++)
                    {
                        _chmr.Entrance.Fixture[idNo].result[i] = fixture.Para[i].Result;
                        _chmr.Entrance.Fixture[idNo].resultId[i] = fixture.Para[i].FlowId;
                        _chmr.Entrance.Fixture[idNo].serialNo[i] = fixture.Para[i].SerialNo;
                    }

                    //空治具
                    if (_chmr.Entrance.Fixture[idNo].IsFixNull == 1)
                    {
                        Log(_chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "【" + _chmr.Entrance.Fixture[idNo].idCard +
                                   "】"+ CLanguage.Lan("为空治具,准备进站"), udcRunLog.ELog.OK);
                        continue;
                    }

                    for (int i = 0; i < fixture.Base.MaxSlot; i++)
                    {
                        if (_chmr.Entrance.Fixture[idNo].result[i] != 0 || _chmr.Entrance.Fixture[idNo].resultId[i] != CYOHOOApp.BI_FlowId - 1)
                        {
                            _chmr.Entrance.Fixture[idNo].serialNo[i] = string.Empty;
                        }
                    }
                }

                bool HaveUUT = false;

                for (int idNo = 0; idNo < _chmr.Entrance.Fixture.Count; idNo++)
                {
                    if (_chmr.Entrance.Fixture[idNo].IsFixNull == 1)
                    {
                        HaveUUT = true;
                    }
                    else
                    {
                        if (CGlobalPara.SysPara.Mes.Connect && CGlobalPara.SysPara.Mes.ChkSn)
                        {
                            for (int i = 0; i < _chmr.Entrance.Fixture[idNo].serialNo.Count; i++)
                            {
                                if (_chmr.Entrance.Fixture[idNo].serialNo[i] == string.Empty)
                                    continue;

                                if (!CheckSn(_chmr.Entrance.Fixture[idNo].serialNo[i], out er))
                                {
                                    Log(_chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + (idNo + 1).ToString() + "【" + _chmr.Entrance.Fixture[idNo].idCard + "】" +
                                                                    CLanguage.Lan("条码") + "【" + _chmr.Entrance.Fixture[idNo].serialNo[i] + "】MES:" + er,
                                                                    udcRunLog.ELog.NG);
                                    _chmr.Entrance.Fixture[idNo].serialNo[i] = string.Empty;
                                    continue;
                                }
                                HaveUUT = true;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < _chmr.Entrance.Fixture[idNo].serialNo.Count; i++)
                            {
                                if (_chmr.Entrance.Fixture[idNo].serialNo[i] != string.Empty)
                                    HaveUUT = true;
                            }
                        }
                    }
                }

                if (!HaveUUT)
                {
                    if (!CGlobalPara.SysPara.Mes.ChkGoPass)
                    {                      
                        _chmr.Entrance.DoRun = ERun.报警;
                        _chmr.Entrance.AlarmInfo = CLanguage.Lan("无可测产品");
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 0), (int)EPLC_RESULT.结果NG);
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 1), (int)EPLC_RESULT.结果NG);
                        alarmInfo = _chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "1【" + _chmr.Entrance.Fixture[0].idCard + "】" +
                                                             CLanguage.Lan("与") + CLanguage.Lan("治具ID")+ "2【" + _chmr.Entrance.Fixture[1].idCard + "】"+
                                                             CLanguage.Lan("无可测产品");
                        Log(alarmInfo, udcRunLog.ELog.NG);
                        MessageBox.Show(alarmInfo, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);  
                        return false;
                    }
                    else
                    {
                        _chmr.Entrance.GoWayBI = 1;

                        Log(_chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "1【" + _chmr.Entrance.Fixture[0].idCard + "】" +
                                                        CLanguage.Lan("与") + CLanguage.Lan("治具ID") + "2【" + _chmr.Entrance.Fixture[1].idCard + "】" +
                                                        CLanguage.Lan("无可测产品,直接跳站处理"), udcRunLog.ELog.NG);
                    }
                }

                watcher.Stop();

                Log(_chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "1【" + _chmr.Entrance.Fixture[0].idCard + "】"+
                                                CLanguage.Lan("与") + CLanguage.Lan("治具ID") + "2【" + _chmr.Entrance.Fixture[1].idCard + "】" +
                                                CLanguage.Lan("检查正常,准备顶升进机") + ":" + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 检查入口2个治具机种参数
        /// </summary>
        /// <returns></returns>
        private bool checkInBIModel()
        {
            try
            {
                string curModel = string.Empty;

                string alarmInfo = string.Empty;

                //判断机种是否一致?
                if (!CGlobalPara.SysPara.Para.ChkAutoModel)
                    return true;

                for (int idNo = 0; idNo < _chmr.Entrance.Fixture.Count; idNo++)
                {
                    if (_chmr.Entrance.Fixture[idNo].IsFixNull == 0)
                    {
                        if (curModel == string.Empty)
                        {
                            curModel = _chmr.Entrance.Fixture[idNo].modelName;
                        }
                        else
                        {
                            if (curModel != _chmr.Entrance.Fixture[idNo].modelName)
                            {
                                alarmInfo = _chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "【" + _chmr.Entrance.Fixture[0].idCard + "】:" +
                                                                       _chmr.Entrance.Fixture[0].modelName + CLanguage.Lan("与治具ID") + "【" +
                                                                       _chmr.Entrance.Fixture[1].idCard + "】:" + _chmr.Entrance.Fixture[1].modelName +
                                                                       CLanguage.Lan("不一致,请检查");                                                                                     
                                _chmr.Entrance.DoRun = ERun.报警;
                                _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 0), (int)EPLC_RESULT.结果NG);
                                _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 1), (int)EPLC_RESULT.结果NG);
                                Log(alarmInfo, udcRunLog.ELog.NG);
                                MessageBox.Show(alarmInfo, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                                                              
                                return false;
                            }
                        }
                    }
                }

                //空治具进站
                if (curModel == string.Empty)
                    return true;

                //自动调用机种参数
                if (curModel != _runModel.Base.Model)
                {
                    string modelPath = CGlobalPara.SysPara.Report.ModelPath + "\\" + curModel + ".bi";

                    if (!File.Exists(modelPath))
                    {
                        _chmr.Entrance.DoRun = ERun.报警;
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 0), (int)EPLC_RESULT.结果NG);
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 1), (int)EPLC_RESULT.结果NG);
                        alarmInfo = _chmr.Entrance.ToString() + CLanguage.Lan("无法调用机种参数") + "[" + modelPath + "]";
                        Log(alarmInfo, udcRunLog.ELog.NG);
                        MessageBox.Show(alarmInfo, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                                                                     
                        return false;
                    }
                    else
                    {
                        _defaultModelPath = modelPath;

                        LoadModelPara();

                        Log(_chmr.Entrance.ToString() + CLanguage.Lan("重新调机种参数") + "[" + _runModel.Base.Model + "]", udcRunLog.ELog.Action);

                        SetIniBIPara();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检查入口2个治具输入AC电压
        /// </summary>
        /// <returns></returns>
        private bool checkInBIACV()
        {
            try
            {
                int curACVolt = 0;

                string alarmInfo = string.Empty;

                for (int idNo = 0; idNo < _chmr.Entrance.Fixture.Count; idNo++)
                {
                    if (_chmr.Entrance.Fixture[idNo].IsFixNull == 0)
                    {
                        if (curACVolt == 0)
                        {
                            curACVolt = _chmr.Entrance.Fixture[idNo].curACV;
                        }
                        else
                        {
                            if (curACVolt != _chmr.Entrance.Fixture[idNo].curACV)
                            {
                                _chmr.Entrance.DoRun = ERun.报警;
                                _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 0), (int)EPLC_RESULT.结果NG);
                                _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 1), (int)EPLC_RESULT.结果NG);
                                alarmInfo = _chmr.Entrance.ToString() + CLanguage.Lan("治具ID") + "【" + _chmr.Entrance.Fixture[0].idCard + "】:" +
                                            _chmr.Entrance.Fixture[0].curACV.ToString() + "V"+ CLanguage.Lan("与治具ID") + 
                                            "【" + _chmr.Entrance.Fixture[1].idCard + "】:" +
                                            _chmr.Entrance.Fixture[1].curACV.ToString() + "V"+ CLanguage.Lan("不一致,请检查");                                                             
                                Log(alarmInfo, udcRunLog.ELog.NG);
                                MessageBox.Show(alarmInfo, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);  
                                return false;
                            }
                        }
                    }
                }

                //空治具进站
                if (curACVolt == 0)
                    return true;

                //提示输入电压不一致?
                if (curACVolt != _runModel.OnOff[0].Item[0].ACV)
                {
                    if (MessageBox.Show(CLanguage.Lan("老化测试线当前电压") + ":" + curACVolt.ToString() + CLanguage.Lan("与当前治具机种电压") + 
                                       ":" + _runModel.OnOff[0].Item[0].ACV.ToString() + CLanguage.Lan("不符合,请确认是否要进机?"), "Tip",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                       != DialogResult.Yes)
                    {
                        _chmr.Entrance.DoRun = ERun.报警;
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 0), (int)EPLC_RESULT.结果NG);
                        _threadPLC.addREGWrite(OutPLC(EPLCOUT.老化入口平台1结果, 1), (int)EPLC_RESULT.结果NG);
                        Log(CLanguage.Lan("老化测试线当前电压")+ ":" + curACVolt.ToString() + CLanguage.Lan("与当前治具机种电压") + ":" +
                             _runModel.OnOff[0].Item[0].ACV.ToString() + CLanguage.Lan("不符合"), udcRunLog.ELog.NG);                       
                        return false;
                    }
                    else
                    {
                        Log(CLanguage.Lan("老化测试线当前电压") + ":" + curACVolt.ToString() +
                            CLanguage.Lan("与当前治具机种电压") + ":" + _runModel.OnOff[0].Item[0].ACV.ToString() +
                                          CLanguage.Lan("不符合,人工确认后进机"), udcRunLog.ELog.OK);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 老化进出机机制
        /// <summary>
        /// 分配进出机位置
        /// </summary>
        /// <returns></returns>
        private bool AssignPositon()
        {
            try
            {

                int inPos = -1;

                int outPos = -1;

                bool returnPos = false;

                if (!CheckHandFree())
                    return true;

                if (Set_Fixture_GoPass())
                    return true;

                //返板条件
                if (!Get_Fixture_ReturnReady(out returnPos))
                    return false;

                //进机条件
                if (_chmr.InPlatForm.DoRun == ERun.进站 && _chmr.PLC.rInReady == CPLCPara.ON)
                {
                    if (_chmr.OP.ChkForbitIn == 0 && !CGlobalPara.SysPara.Para.ChkAutoSelf)
                    {
                        if (!Get_Fixture_InBIPos(out inPos))
                            return false;
                    }
                }

                //出机条件
                if (_chmr.PLC.rOutReady == CPLCPara.ON && !CGlobalPara.SysPara.Para.ChkAutoSelf)
                {
                    if (!Get_Fixture_outBIPos(out outPos))
                        return false;
                }

                if (!SetAssignPosTask(inPos, outPos, returnPos))
                    return false;
              
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检测机械手是否空闲可用?
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckHandFree()
        {
            try
            {
                int WaitTime = 6000;

                if (_chmr.OP.IsBusy)
                {
                    if (_chmr.PLC.handStat != EHandStat.空闲)
                    {
                        _chmr.OP.IsBusy = false;
                    }
                    else
                    {
                        if (_chmr.OP.watcher.ElapsedMilliseconds > WaitTime)
                        {
                            if (_chmr.OP.DoRun == EHandStat.进机)
                                _chmr.OP.DoRun = EHandStat.进机超时;
                            if (_chmr.OP.DoRun == EHandStat.出机)
                                _chmr.OP.DoRun = EHandStat.出机超时;
                            if (_chmr.OP.DoRun == EHandStat.指定进机)
                                _chmr.OP.DoRun = EHandStat.指定超时;
                            if (_chmr.OP.DoRun == EHandStat.跳站)
                                _chmr.OP.DoRun = EHandStat.跳站超时;
                            if (_chmr.OP.DoRun == EHandStat.返板)
                                _chmr.OP.DoRun = EHandStat.返板超时; 
                        }
                    }

                    OnActionArgs.OnEvented(new CUIActionArgs(CLanguage.Lan("进出机状态锁住为") + "【" + _chmr.OP.DoRun.ToString() + "】,"+ 
                                                             CLanguage.Lan("不能实行进出机."), Color.Red));

                    return false;
                }

                if (_chmr.PLC.handStat != EHandStat.空闲)
                {
                    OnActionArgs.OnEvented(new CUIActionArgs(CLanguage.Lan("当前机械手状态为")+ "【"+ CLanguage.Lan("忙碌") + 
                                            "=" + ((int)_chmr.PLC.handStat).ToString() + "】,"+ CLanguage.Lan("不能实行进出机."), Color.Red));
                    return false;
                }

                if (_threadPLC.rREG_Val(EPLCINP.PLC自动运行.ToString()) != CPLCPara.ON)
                {
                    OnActionArgs.OnEvented(new CUIActionArgs(CLanguage.Lan("PLC未处于【自动运行状态】,不能实行进出机"), Color.Red));
                    return false;
                }

                //if (_threadPLC.rREG_Val(EPLCINP.PLC设备报警.ToString()) == CPLCPara.ON)
                //{
                //    OnActionArgs.OnEvented(new CUIActionArgs("PLC运行处于【设备异常报警】,不能实行进出机", Color.Red));
                //    return false;
                //}

                _chmr.OP.DoRun = EHandStat.空闲;

                return true;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 跳过老化房直接出机
        /// </summary>
        /// <returns></returns>
        private bool Set_Fixture_GoPass()
        {
            try
            {
                string er = string.Empty;

                if (_chmr.PLC.rOutReady == CPLCPara.ON && _chmr.PLC.rInReady == CPLCPara.ON && _chmr.InPlatForm.DoRun == ERun.跳站)
                {
                    ControlToPLC_GoPass();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 获取返板就绪
        /// </summary>
        /// <returns></returns>
        private bool Get_Fixture_ReturnReady(out bool returnPos)
        {
            returnPos = false;

            try
            {
                if (_chmr.PLC.rReturnEntrance == CPLCPara.ON &&
                    _chmr.PLC.rReturnExit == CPLCPara.ON)
                {
                    returnPos = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 获取进机位置
        /// </summary>
        /// <param name="inPos"></param>
        /// <returns></returns>
        private bool Get_Fixture_InBIPos(out int inPos)
        {
            inPos = -1;

            try
            {
                if (!Get_InPos(out inPos))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 获取出机位置
        /// </summary>
        /// <param name="outPos"></param>
        /// <returns></returns>
        private bool Get_Fixture_outBIPos(out int outPos)
        {
            outPos = -1;

            try
            {

                bool outEnd = false;

                if (_chmr.OP.ChkOutFixEmpty == 1 && outPos == -1)
                {
                    Get_OutEmptyPos(out outPos);
                }
                if (_chmr.OP.ChkForbitOut == 0 && outPos == -1)
                {
                    if (outPos == -1)
                    {
                        Get_PreforOutPos(out outPos);
                    }
                    if (outPos == -1)
                    {
                        if (_chmr.CurSelOut)
                        {
                            Get_OutSelectModelPos(out outPos);

                            outEnd = Get_SameModelBurnnig();

                            if (CGlobalPara.SysPara.Para.ChkSameModel)  //勾选选择出同一机种
                            {
                                outEnd = true;
                            }
                        }
                    }
                    if (outPos == -1 && !outEnd)
                    {
                        Get_SingleOutPos(out outPos);
                    }
                    if (outPos == -1 && !outEnd)
                    {
                        Get_OutNormalPos(out outPos);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 分配进出机及返板
        /// </summary>
        /// <returns></returns>
        private bool SetAssignPosTask(int inPos, int outPos, bool returnPos)
        {
            string action = string.Empty;

            Color color = Color.Red;

            try
            {
                string localName = string.Empty;

                int inPos1 = -1;

                int outPos1 = -1;

                Get_Fixture_InBIPos(out inPos1);

                Get_Fixture_outBIPos(out outPos1);

                if (_chmr.PLC.rInReady != CPLCPara.ON)
                    action += CLanguage.Lan("【进机】:") + CLanguage.Lan("入口无治具") + ";";
                else if (_chmr.OP.ChkForbitIn == 1)
                    action += CLanguage.Lan("【进机】:") + CLanguage.Lan("禁止进机") + ";";
                else if (_chmr.InPlatForm.DoRun != ERun.进站)
                    action += CLanguage.Lan("【进机】:") + _chmr.InPlatForm.DoRun.ToString();
                else if (inPos1 != -1)
                {
                    localName = "L" + _runUUT[inPos1].Base.handRow + "-" + _runUUT[inPos1].Base.handCol.ToString("D2");
                    action += CLanguage.Lan("【进机】:") + localName + ";";
                }
                else
                {
                    action += CLanguage.Lan("【进机】:") + CLanguage.Lan("无进机库位") + ";";
                }

                if (_chmr.PLC.rOutReady != CPLCPara.ON)
                {
                    action += CLanguage.Lan("【出机】:") + CLanguage.Lan("出口忙碌") + ";";
                }
                else if (_chmr.OP.ChkOutFixEmpty == 1)
                {
                    action +=  CLanguage.Lan("【出机】:") + CLanguage.Lan("禁止出机") + ";";
                }
                else if (outPos1 != -1)
                {
                    localName = "L" + _runUUT[outPos1].Base.handRow + "-" + _runUUT[outPos1].Base.handCol.ToString("D2");
                    action += CLanguage.Lan("【出机】:") + localName + ";";
                }
                else
                {
                    action += CLanguage.Lan("【出机】:") + CLanguage.Lan("无出机库位") + ";";
                }

                if (returnPos)
                {
                    action += CLanguage.Lan("【返板】:") + CLanguage.Lan("就绪") + ";";
                }
                else
                {
                    action += CLanguage.Lan("【返板】:") + CLanguage.Lan("等待") + ";";
                }

                //获取进出机位置 
                if (inPos == -1 && outPos == -1)
                {
                    if (returnPos)
                    {
                        ControlPLC_ReturnPos();
                        _chmr.OP.Alternant = EAssMode.返板;
                        action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("返板") + ";";
                        color = Color.Blue;
                    }
                    return true;
                }

                //有可进机位置和可出机位置-->交替进出机--> 0:进机 1：出机 2: 返板

                if (inPos != -1 && outPos != -1) //有进机和出机位置
                {
                    if (!returnPos) //不需要返板
                    {
                        if (_chmr.OP.Alternant != EAssMode.进机)
                        {
                            _chmr.OP.InPos = inPos;
                            _chmr.OP.DoRun = EHandStat.进机;
                            _chmr.OP.Alternant = EAssMode.进机;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("进机") + ";";
                            color = Color.Blue;
                        }
                        else
                        {
                            _chmr.OP.OutPos = outPos;
                            _chmr.OP.DoRun = EHandStat.出机;
                            _chmr.OP.Alternant = EAssMode.出机;
                            action += "-" +  CLanguage.Lan("【执行】:") + CLanguage.Lan("出机") + ";";
                            color = Color.Blue;
                        }
                    }
                    else        //需要返板
                    {
                        if (_chmr.OP.Alternant == EAssMode.出机)
                        {
                            ControlPLC_ReturnPos();
                            _chmr.OP.Alternant = EAssMode.返板;
                            action += "-" + CLanguage.Lan("【执行】:") + CLanguage.Lan("返板") + ";";
                            color = Color.Blue;
                            return true;
                        }
                        else if (_chmr.OP.Alternant == EAssMode.返板)
                        {
                            _chmr.OP.InPos = inPos;
                            _chmr.OP.DoRun = EHandStat.进机;
                            _chmr.OP.Alternant = EAssMode.进机;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("进机") + ";";
                            color = Color.Blue;
                        }
                        else
                        {
                            _chmr.OP.OutPos = outPos;
                            _chmr.OP.DoRun = EHandStat.出机;
                            _chmr.OP.Alternant = EAssMode.出机;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("出机") + ";";
                            color = Color.Blue;
                        }
                    }
                }
                else if (inPos != -1)      //只有进机位置
                {
                    if (!returnPos) //不需要返板
                    {
                        _chmr.OP.InPos = inPos;
                        _chmr.OP.DoRun = EHandStat.进机;
                        _chmr.OP.Alternant = EAssMode.进机;
                        action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("进机") + ";";
                        color = Color.Blue;
                    }
                    else
                    {
                        if (_chmr.OP.Alternant == EAssMode.进机)
                        {
                            ControlPLC_ReturnPos();
                            _chmr.OP.Alternant = EAssMode.返板;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("返板") + ";";
                            color = Color.Blue;
                            return true;
                        }
                        else
                        {
                            _chmr.OP.InPos = inPos;
                            _chmr.OP.DoRun = EHandStat.进机;
                            _chmr.OP.Alternant = EAssMode.进机;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("进机") + ";";
                            color = Color.Blue;
                        }
                    }
                }
                else if (outPos != -1)    //只有出机位置
                {
                    if (!returnPos) //不需要返板
                    {
                        _chmr.OP.OutPos = outPos;
                        _chmr.OP.DoRun = EHandStat.出机;
                        _chmr.OP.Alternant = EAssMode.出机;
                        action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("出机") + ";";
                        color = Color.Blue;
                    }
                    else
                    {
                        if (_chmr.OP.Alternant == EAssMode.出机)
                        {
                            ControlPLC_ReturnPos();
                            _chmr.OP.Alternant = EAssMode.返板;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("返板") + ";";
                            color = Color.Blue;
                            return true;
                        }
                        else
                        {
                            _chmr.OP.OutPos = outPos;
                            _chmr.OP.DoRun = EHandStat.出机;
                            _chmr.OP.Alternant = EAssMode.出机;
                            action += "-"+ CLanguage.Lan("【执行】:") + CLanguage.Lan("出机") + ";";
                            color = Color.Blue;
                        }
                    }
                }

                //手动进机
                if (_chmr.OP.DoRun == EHandStat.进机 && CGlobalPara.SysPara.Para.ChkHandIn)
                {
                    udcHandInPos handInDlg = new udcHandInPos();

                    if (handInDlg.ShowDialog() != DialogResult.OK)
                    {
                        _chmr.OP.DoRun = EHandStat.空闲;
                        return true;
                    }

                    int uutNo = -1;

                    for (int i = 0; i < _runUUT.Count; i++)
                    {
                        if (_runUUT[i].Base.roomNo == udcHandInPos.C_Room &&
                            _runUUT[i].Base.iRow == udcHandInPos.C_Row &&
                            _runUUT[i].Base.iCol == udcHandInPos.C_Col)
                        {
                            uutNo = i;
                            break;
                        }
                    }

                    if (uutNo == -1)
                    {
                        _chmr.OP.DoRun = EHandStat.空闲;
                        return true;
                    }

                    if (_runUUT[uutNo].Para.CtrlUUTONLine == 1 ||
                        _runUUT[uutNo].Para.AlarmCode != EAlarmCode.正常 ||
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1 ||
                        _runUUT[uutNo + 1].Para.AlarmCode != EAlarmCode.正常)
                    {
                        _chmr.OP.DoRun = EHandStat.空闲;
                    }
                    else
                    {
                        _chmr.OP.InPos = uutNo;
                        localName = "L" + _runUUT[inPos].Base.handRow + "-" + _runUUT[inPos].Base.handCol.ToString("D2");
                        action = CLanguage.Lan("【执行-手动进机】: ") + localName + ";";
                        color = Color.Blue;
                    }

                    Thread.Sleep(50);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
            finally
            {
                OnActionArgs.OnEvented(new CUIActionArgs(action, Color.Blue));
            }
        }
        /// <summary>
        /// 进机方法
        /// </summary>
        /// <returns></returns>
        private bool Get_InPos(out int pos)
        {
            pos = -1;

            try
            {
                int useNum = -1;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;

                    if (
                        _runUUT[uutNo + 0].Para.DoRun == EDoRun.位置空闲 &&
                        _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 0].Para.CtrlUUTONLine == 0 &
                        _runUUT[uutNo + 1].Para.DoRun == EDoRun.位置空闲 &&
                        _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 0
                        )
                    {
                        if (useNum == -1 || _runUUT[uutNo].Para.UsedNum < useNum)
                        {
                            useNum = _runUUT[uutNo].Para.UsedNum;
                            pos = uutNo;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 用于强制出空治具模式
        /// </summary>
        /// <returns></returns>
        private bool Get_OutEmptyPos(out int pos)
        {
            pos = -1;

            try
            {
                for (int i = 0; i < _runUUT.Count / 2; i++)
                {

                    int uutNo = i * 2;

                    if (
                       _runUUT[uutNo + 0].Para.DoRun == EDoRun.空治具到位 &&
                       _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                       _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &
                       _runUUT[uutNo + 1].Para.DoRun == EDoRun.空治具到位 &&
                       _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                       _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1
                       )
                    {
                        pos = uutNo;
                        break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 获取指定同一机种出机
        /// </summary>
        /// <returns></returns>
        private bool Get_OutSelectModelPos(out int pos)
        {
            pos = -1;

            try
            {
                string startTime = string.Empty;

                //获取单边治具老化
                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;
                    //行1,2老化结束
                    if (
                        (_runUUT[uutNo + 0].Para.DoRun == EDoRun.老化结束 &&
                         _runUUT[uutNo + 0].Para.OutLevel == 0 &&
                         _runUUT[uutNo + 0].Para.ModelName == _chmr.CurOutModel &&
                         _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                         _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                         _runUUT[uutNo + 1].Para.DoRun == EDoRun.空治具到位 &&
                         _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                         _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1)
                         ||
                        (_runUUT[uutNo + 0].Para.DoRun == EDoRun.空治具到位 &&
                         _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                         _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                         _runUUT[uutNo + 1].Para.DoRun == EDoRun.老化结束 &&
                         _runUUT[uutNo + 1].Para.OutLevel == 0 &&
                         _runUUT[uutNo + 1].Para.ModelName == _chmr.CurOutModel &&
                         _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                         _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1)
                       )
                    {
                        if (startTime == string.Empty)
                        {
                            startTime = _runUUT[uutNo].Para.StartTime;
                            pos = uutNo;
                        }
                        else
                        {
                            if (System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime) < System.Convert.ToDateTime(startTime))
                            {
                                startTime = _runUUT[uutNo].Para.StartTime;
                                pos = uutNo;
                            }
                        }
                    }
                }

                if (pos != -1)
                {
                    return true;
                }

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;

                    if (
                        _runUUT[uutNo + 0].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                        _runUUT[uutNo + 0].Para.OutLevel == 0 &&
                        _runUUT[uutNo + 0].Para.ModelName == _chmr.CurOutModel &&
                        _runUUT[uutNo + 1].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1 &
                        _runUUT[uutNo + 1].Para.OutLevel == 0
                        )
                    {
                        if (startTime == string.Empty)
                        {
                            startTime = _runUUT[uutNo].Para.StartTime;
                            pos = uutNo;
                        }
                        else
                        {
                            if (System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime) < System.Convert.ToDateTime(startTime))
                            {
                                startTime = _runUUT[uutNo].Para.StartTime;
                                pos = uutNo;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 获取是否有同一机种在老化中
        /// </summary>
        /// <returns></returns>
        private bool Get_SameModelBurnnig()
        {
            try
            {
                bool getBIModel = false;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    if (_runUUT[i].Para.ModelName != _chmr.CurOutModel)
                        continue;

                    if (_runUUT[i].Para.CtrlUUTONLine == 1)
                    {
                        if (_runUUT[i].Para.DoRun == EDoRun.老化结束)
                        {
                            getBIModel = true;
                            break;
                        }
                        else if ((_runUUT[i].Para.DoRun == EDoRun.正在老化) && (_runUUT[i].Para.AlarmCode == EAlarmCode.正常) &&
                                 (_runUUT[i].Para.RunTime > _runUUT[i].Para.BurnTime - CGlobalPara.SysPara.Para.ModelTimes * 60))
                        {
                            getBIModel = true;
                            break;
                        }
                    }
                }
                return getBIModel;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 出机方法1
        /// 手动优先级
        /// 按老化结束时间
        /// </summary>
        /// <returns></returns>
        private bool Get_PreforOutPos(out int pos)
        {
            pos = -1;

            try
            {
                string startTime = string.Empty;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;
                    //行1,2老化结束
                    if (
                        _runUUT[uutNo + 0].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                        _runUUT[uutNo + 0].Para.OutLevel == 1 &&
                        _runUUT[uutNo + 1].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1 &
                        _runUUT[uutNo + 1].Para.OutLevel == 1
                       )
                    {
                        if (startTime == string.Empty)
                        {
                            startTime = _runUUT[uutNo].Para.StartTime;
                            pos = uutNo;
                        }
                        else
                        {
                            if (System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime) < System.Convert.ToDateTime(startTime))
                            {
                                startTime = _runUUT[uutNo].Para.StartTime;
                                pos = uutNo;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 出机方法0
        /// 单边老化结束
        /// 按老化结束时间
        /// </summary>
        /// <returns></returns>
        private bool Get_SingleOutPos(out int pos)
        {
            pos = -1;

            try
            {
                string startTime = string.Empty;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;
                    //行1,2老化结束
                    if (
                        (_runUUT[uutNo + 0].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                        _runUUT[uutNo + 1].Para.DoRun == EDoRun.空治具到位 &&
                        _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1) ||
                        (_runUUT[uutNo + 0].Para.DoRun == EDoRun.空治具到位 &&
                        _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                        _runUUT[uutNo + 1].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1)
                       )
                    {
                        if (startTime == string.Empty)
                        {
                            startTime = _runUUT[uutNo].Para.StartTime;
                            pos = uutNo;
                        }
                        else
                        {
                            if (System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime) < System.Convert.ToDateTime(startTime))
                            {
                                startTime = _runUUT[uutNo].Para.StartTime;
                                pos = uutNo;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 获取2个母治具老化结束出机位置
        /// </summary>
        /// <returns></returns>
        private bool Get_OutNormalPos(out int pos)
        {
            pos = -1;

            try
            {
                string startTime = string.Empty;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;
                    if (
                        _runUUT[uutNo + 0].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 0].Para.CtrlUUTONLine == 1 &&
                        _runUUT[uutNo + 0].Para.OutLevel == 0 &&
                        _runUUT[uutNo + 1].Para.DoRun == EDoRun.老化结束 &&
                        _runUUT[uutNo + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[uutNo + 1].Para.CtrlUUTONLine == 1 &
                        _runUUT[uutNo + 1].Para.OutLevel == 0
                        )
                    {
                        if (startTime == string.Empty)
                        {
                            startTime = _runUUT[uutNo].Para.StartTime;
                            pos = uutNo;
                        }
                        else
                        {
                            if (System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime) < System.Convert.ToDateTime(startTime))
                            {
                                startTime = _runUUT[uutNo].Para.StartTime;
                                pos = uutNo;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 控制PLC进出机
        /// <summary>
        /// 治具直接过老化房
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private void ControlToPLC_GoPass()
        {
            try
            {
                //取位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪层.ToString(), 29);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪列.ToString(), 29); ;

                ////放位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪层.ToString(), 30);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪列.ToString(), 30);

                Thread.Sleep(100);

                _threadPLC.addREGWrite(EPLCOUT.机械手取放坐标完成.ToString(), 1);

                _chmr.OP.DoRun = EHandStat.跳站;

                _chmr.OP.IsBusy = true;

                _chmr.OP.watcher.Restart();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设置老化进机
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool ControlToPLC_InPos(int uutNo, out string er)
        {
            er = string.Empty;

            try
            {
                er = string.Empty;

                //发进机命令
                int iRow = _runUUT[uutNo].Base.handRow;
                int iCol = _runUUT[uutNo].Base.handCol;

                //取位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪层.ToString(), 29);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪列.ToString(), 29);

                ////放位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪层.ToString(), iRow);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪列.ToString(), iCol);

                _threadPLC.addREGWrite(EPLCOUT.机械手取放坐标完成.ToString(), 1);

                _chmr.OP.DoRun = EHandStat.进机中;

                _chmr.OP.IsBusy = true;

                _chmr.OP.watcher.Restart();

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 设置治具出机
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool ControlPLC_OutPos(int uutNo, out string er)
        {
            er = string.Empty;

            try
            {
                int iRow = _runUUT[uutNo].Base.handRow;
                int iCol = _runUUT[uutNo].Base.handCol;

                //治具出机位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪层.ToString(), iRow);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪列.ToString(), iCol);

                //治具放机位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪层.ToString(), 30);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪列.ToString(), 30);

                //PC发送机械手取放坐标完成
                _threadPLC.addREGWrite(EPLCOUT.机械手取放坐标完成.ToString(), 1);

                _chmr.OP.DoRun = EHandStat.出机中;

                _chmr.OP.IsBusy = true;

                _chmr.OP.watcher.Restart();

                //记录当前出机机种
                _chmr.CurOutModel = _runUUT[uutNo].Para.ModelName;

                CIniFile.WriteToIni("Parameter", "CurOutModel", _chmr.CurOutModel, CGlobalPara.IniFile);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 进指定位置
        /// </summary>
        /// <param name="outUUTNo"></param>
        /// <param name="inUUTNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool ControlPLC_PosToPos(int outUUTNo, int inUUTNo, out string er)
        {
            er = string.Empty;

            try
            {
                int iRow = _runUUT[outUUTNo].Base.handRow;
                int iCol = _runUUT[outUUTNo].Base.handCol;

                //治具出机位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪层.ToString(), iRow);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪列.ToString(), iCol);

                iRow = _runUUT[inUUTNo].Base.handRow;
                iCol = _runUUT[inUUTNo].Base.handCol;

                //治具放机位置
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪层.ToString(), iRow);
                _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪列.ToString(), iCol);

                //PC发送机械手取放坐标完成
                _threadPLC.addREGWrite(EPLCOUT.机械手取放坐标完成.ToString(), 1);

                _chmr.OP.DoRun = EHandStat.指定进机;

                _chmr.OP.IsBusy = true;

                _chmr.OP.watcher.Restart();

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 控制PLC返板
        /// </summary>
        /// <returns></returns>
        private void ControlPLC_ReturnPos()
        {
            //取位置
            _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪层.ToString(), 31);
            _threadPLC.addREGWrite(EPLCOUT.机械手平台取哪列.ToString(), 31); ;

            ////放位置
            _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪层.ToString(), 32);
            _threadPLC.addREGWrite(EPLCOUT.机械手平台放哪列.ToString(), 32);

            Thread.Sleep(100);

            _threadPLC.addREGWrite(EPLCOUT.机械手取放坐标完成.ToString(), 1);

            _chmr.OP.DoRun = EHandStat.返板;

            _chmr.OP.IsBusy = true;

            _chmr.OP.watcher.Restart();
        }
        #endregion

        #region ON/OFF时序控制---上位机控制AC时序
        /// <summary>
        /// 输入电压定义
        /// </summary>
        private int[] C_ACVolt = new int[] { 0, 90, 110, 220, 264, 300, 330, 380 };
        /// <summary>
        /// 老化时序负载切换
        /// </summary>
        /// <returns></returns>
        private bool Control_AC_ONOFF()
        {
            if (CGlobalPara.SysPara.Para.BITimeRate == 0)
                CGlobalPara.SysPara.Para.BITimeRate = 1;

            string er = string.Empty;

            for (int i = 0; i < _runUUT.Count / 2; i++)
            {
                try
                {
                    int uutNo = i * 2;

                    int index = -1;

                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        if (_runUUT[uutNo + idNo].Para.DoRun == EDoRun.正在老化)
                        {
                            if (CGlobalPara.SysPara.Dev.CtrlTimeMode != ECtrlTimeMode.上位机计时模式1)
                            {
                                _runUUT[uutNo + idNo].Para.RunTime += (int)((double)_CtrlAC.DiffTime * CGlobalPara.SysPara.Para.BITimeRate);
                            }
                            else
                            {
                                _runUUT[uutNo + idNo].Para.RunTime = CTimer.DateDiff(_runUUT[uutNo + idNo].Para.StartTime);
                                _runUUT[uutNo + idNo].Para.RunTime = (int)((double)_runUUT[uutNo + idNo].Para.RunTime * CGlobalPara.SysPara.Para.BITimeRate);
                            }
                            if (_runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM == EQCMChage.空闲 &&
                                _runUUT[uutNo + idNo].Para.RunTime > CGlobalPara.SysPara.Alarm.Chk_qcv_times)                             
                                index = uutNo;
                        }
                    }

                    if (index == -1)
                        continue;

                    if (_runUUT[uutNo].Para.RunTime >= _runUUT[uutNo].Para.BurnTime) //老化完成
                    {
                        for (int idNo = 0; idNo < 2; idNo++)
                        {
                             InitialQCM(uutNo + idNo, 0);
                            _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                            _runUUT[uutNo + idNo].Para.DoRun = EDoRun.老化完成;
                        }
                        continue;
                    }

                    List<CONOFF> OnOffList = new List<CONOFF>();

                    for (int z = 0; z < _runUUT[uutNo].OnOff.OnOff.Count; z++)
                    {
                        CONOFF OnOff = new CONOFF()
                        {
                            ACV = _runUUT[uutNo].OnOff.OnOff[z].ACV,
                            OnOffTime = _runUUT[uutNo].OnOff.OnOff[z].OnOffTime,
                            OnTime = _runUUT[uutNo].OnOff.OnOff[z].OnTime,
                            OffTime = _runUUT[uutNo].OnOff.OnOff[z].OffTime,
                            OutPutType = _runUUT[uutNo].OnOff.OnOff[z].OutPutType
                        };
                        OnOffList.Add(OnOff);
                    }

                    int runTime = _runUUT[uutNo].Para.RunTime;

                    if (CGlobalPara.SysPara.Dev.CtrlACMode == ECtrlACMode.上位机控制时序)
                    {
                        if (_runUUT[uutNo].Para.RunTime + _runUUT[uutNo].Para.IniRunTime <= _runUUT[uutNo].Para.BurnTime)
                        {
                            runTime = _runUUT[uutNo].Para.RunTime + _runUUT[uutNo].Para.IniRunTime;
                        }
                        else
                        {
                            runTime = _runUUT[uutNo].Para.RunTime + _runUUT[uutNo].Para.IniRunTime - _runUUT[uutNo].Para.BurnTime;
                        }
                    }

                    bool QCM_Changed = false;

                    CRUNTIME CurOnOff = null;

                    if (CPara.GetCurStepFromOnOff(runTime, OnOffList, out CurOnOff, out er))
                    {
                        if (CurOnOff.CurRunVolt != _runUUT[uutNo].OnOff.TimeRun.CurRunVolt) //输入电压切换
                        {
                            QCM_Changed = true;
                        }
                        else if (CurOnOff.CurRunOutPut != _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut) //快充切换
                        {
                            QCM_Changed = true;
                        }
                    }

                    if (!QCM_Changed)
                    {
                        if (_runUUT[uutNo].OnOff.TimeRun.CurRunVolt > 0 && _runUUT[uutNo].Para.CtrlACON == 0)
                        {
                            //当前输入电压与设置电压不一致-->重启AC
                            if (_runUUT[uutNo].Para.bACErrNum > CGlobalPara.C_ALARM_TIME)
                            {
                                Log(_runUUT[uutNo].ToString() + CLanguage.Lan("当前输入电压") + "=" +
                                           _runUUT[uutNo].OnOff.TimeRun.CurRunVolt.ToString() + "V;"+
                                           CLanguage.Lan("检测不到AC ON信号,重启AC ON."),
                                           udcRunLog.ELog.NG);
                            }
                            else
                            {
                                _runUUT[uutNo].Para.bACErrNum++;
                                continue;
                            }
                        }
                        else if (_runUUT[uutNo].OnOff.TimeRun.CurRunVolt == 0 && _runUUT[uutNo].Para.CtrlACON == 1)
                        {
                            InitialQCM(uutNo, 0);
                            InitialQCM(uutNo + 1, 0);
                            continue;
                        }
                        else if (CGlobalPara.SysPara.Para.ChkACVolt && _runUUT[uutNo].Para.CtrlACVolt > 40)  //电压是否存在切换?
                        {
                            double acv = Math.Abs(_runUUT[uutNo].Para.CtrlACVolt - _runUUT[uutNo].Para.RunACVolt);
                            if (acv < 40) //AC电压偏差40-->存在电压切换
                                continue;
                            if (_runUUT[uutNo].Para.bACVoltNum > 1)
                            {
                                Log(_runUUT[uutNo].ToString() + CLanguage.Lan("原先输入电压") + "=" + _runUUT[uutNo].Para.RunACVolt.ToString("0.0") +
                                            "V;"+ CLanguage.Lan("当前输入电压") + "=" + _runUUT[uutNo].Para.CtrlACVolt.ToString("0.0") + "V,"+
                                            CLanguage.Lan("老化切换输入AC电压"), udcRunLog.ELog.Action);                                            
                            }
                            else
                            {
                                _runUUT[uutNo].Para.bACVoltNum++;
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    _runUUT[uutNo].Para.bACVoltNum = 0;

                    _runUUT[uutNo].Para.bACErrNum = 0;
                    
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurStepNo = CurOnOff.CurStepNo;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunVolt = CurOnOff.CurRunVolt;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut = CurOnOff.CurRunOutPut;
                        int stepNo = (_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut >= 0 ? _runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut : 0);
                        if (stepNo >= _runUUT[uutNo + idNo].OnOff.OutPut.Count)
                        {
                            stepNo = _runUUT[uutNo + idNo].OnOff.OutPut.Count - 1;
                        }
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCType = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].QCType;
                        _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCV = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].QCV;
                        for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                        {
                            _runUUT[uutNo + idNo].Led[slot].qcv = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].QCV;
                            _runUUT[uutNo + idNo].Led[slot].vName = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].Vname;
                            _runUUT[uutNo + idNo].Led[slot].vMin = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].Vmin;
                            _runUUT[uutNo + idNo].Led[slot].vMax = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].Vmax;
                            _runUUT[uutNo + idNo].Led[slot].IMode = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].Imode;
                            _runUUT[uutNo + idNo].Led[slot].ISet = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].ISet;
                            _runUUT[uutNo + idNo].Led[slot].Imin = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].Imin;
                            _runUUT[uutNo + idNo].Led[slot].Imax = _runUUT[uutNo + idNo].OnOff.OutPut[stepNo].Chan[0].Imax;
                            _runUUT[uutNo + idNo].Led[slot].vFailNum = 0;
                            _runUUT[uutNo + idNo].Led[slot].iFailNum = 0;
                        }

                        InitialQCM(uutNo + idNo, (CurOnOff.CurRunVolt > 0 ? 1 : 0));

                        if (!CGlobalPara.SysPara.Para.ChkFastOnOff)   //检测快充及负载过程
                        {
                            if (CurOnOff.CurRunVolt > 0)
                            {
                                _runUUT[uutNo + idNo].Para.CtrlACON = 1;
                                _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.控制ACON;
                                _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();
                            }
                        }
                        else
                        {
                            if (CurOnOff.CurRunVolt > 0)
                            {
                                //设置快充模式
                                SetQCM(uutNo + idNo, (EQCM)_runUUT[uutNo + idNo].OnOff.TimeRun.CurQCType,
                                                              _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCV,
                                                              _runUUT[uutNo + idNo].Led[0].ISet);
                                //设置负载值
                                setLoadVal(uutNo + idNo);                                
                                _runUUT[uutNo + idNo].Para.CtrlACON = 1;
                                _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;                       
                            }
                        }

                        local_db_qcm_change(uutNo + idNo);
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.ToString(), udcRunLog.ELog.Err);
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// 控制输入AC时序参数
        /// </summary>
        private CCtrlAC _CtrlAC = new CCtrlAC();
        /// <summary>
        /// 加载AC时序INI文件
        /// </summary>
        private void LoadCtrlACIniFile()
        {
            try
            {
                _CtrlAC.StartTime = CIniFile.ReadFromIni("CtrlAC", "StartTime", CGlobalPara.IniFile);

                _CtrlAC.EndTime = CIniFile.ReadFromIni("CtrlAC", "EndTime", CGlobalPara.IniFile);

                _CtrlAC.RunTime = System.Convert.ToInt32(CIniFile.ReadFromIni("CtrlAC", "RunTime", CGlobalPara.IniFile, "0"));

                _CtrlAC.BeforeRunTime = System.Convert.ToInt32(CIniFile.ReadFromIni("CtrlAC", "BeforeRunTime", CGlobalPara.IniFile, "0"));
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 加载总输入AC时序参数
        /// </summary>
        private void LoadCtrlACPara()
        {
            try
            {
                string er = string.Empty;

                if (_runModel == null)
                    return;

                _CtrlAC.TotalTime = _runModel.Para.BITime;

                _CtrlAC.bRun = false;

                if (!CPara.GetOnOffFromModel(_runModel, ref _CtrlAC.Time, out er))
                {
                    Log(er, udcRunLog.ELog.NG);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 控制总AC时序
        /// </summary>
        private bool ControlTotalAC()
        {
            try
            {
                string er = string.Empty;

                //获取运行时间

                if (CGlobalPara.SysPara.Dev.CtrlTimeMode == ECtrlTimeMode.PLC计时模式)      //读取PLC计时寄存器
                {
                    if (_threadPLC.rREG_Val(EPLCINP.当前总输入时间.ToString()) >= 0)
                    {
                        _CtrlAC.RunTime = _threadPLC.rREG_Val(EPLCINP.当前总输入时间.ToString());

                        if (_CtrlAC.RunTime == 0 || _CtrlAC.RunTime >= _runModel.Para.BITime * 3600)
                        {
                            _CtrlAC.RunTime = 1;
                            _CtrlAC.BeforeRunTime = 0;
                            _CtrlAC.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            _CtrlAC.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            _threadPLC.addREGWrite(EPLCOUT.当前总输入时间.ToString(), _CtrlAC.RunTime);
                        }
                    }
                }
                else if (CGlobalPara.SysPara.Dev.CtrlTimeMode == ECtrlTimeMode.上位机计时模式2)  //采用时间差计时
                {
                    if (!_CtrlAC.Watcher.IsRunning || _CtrlAC.EndTime == String.Empty)
                    {
                        _CtrlAC.Watcher.Restart();
                        _CtrlAC.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    if (_CtrlAC.Watcher.ElapsedMilliseconds > 1000)
                    {
                        _CtrlAC.RunTime += CTimer.DateDiff(_CtrlAC.EndTime);
                        _CtrlAC.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _CtrlAC.Watcher.Restart();
                    }
                    if (_CtrlAC.RunTime >= _runModel.Para.BITime * 3600)
                    {
                        _CtrlAC.RunTime = 1;
                        _CtrlAC.BeforeRunTime = 0;
                        _CtrlAC.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _CtrlAC.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
                else if (CGlobalPara.SysPara.Dev.CtrlTimeMode == ECtrlTimeMode.上位机计时模式1)     //采用开始时间计时
                {
                    if (_CtrlAC.StartTime == string.Empty)
                    {
                        _CtrlAC.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _CtrlAC.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    _CtrlAC.RunTime = CTimer.DateDiff(_CtrlAC.StartTime);
                    if (_CtrlAC.RunTime >= _runModel.Para.BITime * 3600)
                    {
                        _CtrlAC.RunTime = 1;
                        _CtrlAC.BeforeRunTime = 0;
                        _CtrlAC.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _CtrlAC.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }

                //控制AC切换输入电压

                if (_CtrlAC.RunTime > 0)
                {
                    CRUNTIME CurOnOff = null;
                    if (CPara.GetCurACVoltFromOnOff(_CtrlAC.RunTime, _CtrlAC.Time.OnOff, out CurOnOff, out er))
                    {
                        if (_CtrlAC.CurVolt != CurOnOff.CurRunVolt)
                        {
                            _CtrlAC.CurVolt = CurOnOff.CurRunVolt;
                        }
                    }
                    if (CGlobalPara.SysPara.Dev.CtrlACMode == ECtrlACMode.上位机控制时序)
                    {
                        ctrlPLCToAC(_CtrlAC.CurVolt);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 计算时间差
        /// </summary>
        /// <returns></returns>
        private bool CtrlDiffACTime()
        {
            try
            {
                //计算时间差

                if (!_CtrlAC.DiffWatcher.IsRunning)
                    _CtrlAC.DiffWatcher.Restart();

                if (_CtrlAC.DiffWatcher.ElapsedMilliseconds > 1000)
                {
                    _CtrlAC.DiffTime = _CtrlAC.RunTime - _CtrlAC.BeforeRunTime;
                    _CtrlAC.BeforeRunTime = _CtrlAC.RunTime;
                    _CtrlAC.DiffWatcher.Restart();
                }
                else
                {
                    _CtrlAC.DiffTime = 0;
                }

                saveCtrlIniFile();

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 控制PLC切换AC
        /// </summary>
        /// <param name="acv"></param>
        /// <returns></returns>
        private bool ctrlPLCToAC(int acv)
        {
            try
            {

                int curAC = _threadPLC.rREG_Val(EPLCINP.当前输入AC电压值.ToString());

                if (curAC <= 0)
                    return true;

                if (curAC > C_ACVolt.Length || C_ACVolt[curAC] == acv)
                    return true;

                if (acv == 90)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 3600);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 0);
                }
                else if (acv == 110)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 3600);                   
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 0);
                }
                else if (acv == 220)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 3600);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 0);
                }
                else if (acv == 264)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 3600);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 0);
                }
                else if (acv == 300)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 3600);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 0);
                }
                else if (acv == 330)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 3600);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 0);
                }
                else if (acv == 380)
                {
                    _threadPLC.addREGWrite(EPLCOUT.通电90V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电110V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电220V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电264V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电300V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电330V时间.ToString(), 0);
                    _threadPLC.addREGWrite(EPLCOUT.通电380V时间.ToString(), 3600);
                }

                return true;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 保存AC时序INI文件
        /// </summary>
        private void saveCtrlIniFile()
        {
            try
            {
                CIniFile.WriteToIni("CtrlAC", "StartTime", _CtrlAC.StartTime, CGlobalPara.IniFile);

                CIniFile.WriteToIni("CtrlAC", "EndTime", _CtrlAC.EndTime, CGlobalPara.IniFile);

                CIniFile.WriteToIni("CtrlAC", "RunTime", _CtrlAC.RunTime.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("CtrlAC", "BeforeRunTime", _CtrlAC.BeforeRunTime.ToString(), CGlobalPara.IniFile);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 设置快充控制板
        /// <summary>
        /// 初始为负载设置为空闲状态
        /// </summary>
        private void InitialQCM(int uutNo, int wOnOff)
        {
            try
            {
                clearLoadVal(uutNo);

                SetACON(uutNo, wOnOff);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 控制快充检测
        /// </summary>
        /// <returns></returns>
        private bool Control_QCM()
        {
            try
            {
                //等待30S

                int waitTime = CGlobalPara.SysPara.Alarm.Chk_qcv_times * 1000;

                int uutNum = 0;

                int qcmNum = 0;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int uutNo = i * 2;

                    switch (_runUUT[uutNo].OnOff.TimeRun.CurQCM)
                    {
                        case EQCMChage.空闲:
                            break;
                        case EQCMChage.控制ACON:
                            if (_runUUT[uutNo].Para.CtrlOnOff == 1)
                            {
                                for (int idNo = 0; idNo < 2; idNo++)
                                {
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.自检电压;
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();
                                }
                            }
                            else
                            {
                                for (int idNo = 0; idNo < 2; idNo++)
                                {
                                    if (!_runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.IsRunning)
                                        _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();

                                    if (_runUUT[uutNo + idNo].Para.DoRun == EDoRun.正在老化)
                                    {
                                        if (_runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.ElapsedMilliseconds > waitTime)
                                        {
                                            _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.控制ACON异常;
                                            _runUUT[uutNo + idNo].Para.AlarmInfo = CLanguage.Lan("控制ACON异常");
                                        }
                                    }
                                }
                            }
                            break;
                        case EQCMChage.自检电压:
                            bool checkVolt = true;
                            uutNum = 0;
                            qcmNum = 0;
                            for (int idNo = 0; idNo < 2; idNo++)
                            {
                                if (!_runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.IsRunning)
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();

                                if (_runUUT[uutNo + idNo].Para.DoRun == EDoRun.正在老化)
                                {
                                    for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                                    {
                                        if (_runUUT[uutNo + idNo].Led[slot].serialNo != string.Empty)
                                        {
                                            if (_runUUT[uutNo + idNo].Led[slot].monV > 2)
                                                qcmNum++;
                                            uutNum++;
                                        }
                                    }
                                }
                            }
                            if (uutNum != qcmNum || _runUUT[uutNo].Led[0].monA > CGlobalPara.SysPara.Para.IdleLoad + 0.3)
                            {
                                if (_runUUT[uutNo].OnOff.TimeRun.WatchQCM.ElapsedMilliseconds < waitTime)
                                {
                                    checkVolt = false;
                                }
                                else
                                {
                                    if (_runUUT[uutNo].Led[0].monA > CGlobalPara.SysPara.Para.IdleLoad + 0.3)
                                        Log(_runUUT[uutNo].ToString() + CLanguage.Lan("快充自检电压超时异常"), udcRunLog.ELog.NG);
                                }
                            }

                            if (checkVolt)
                            {
                                for (int idNo = 0; idNo < 2; idNo++)
                                {
                                    SetQCM(uutNo + idNo, (EQCM)_runUUT[uutNo + idNo].OnOff.TimeRun.CurQCType, 
                                                               _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCV,
                                                               _runUUT[uutNo + idNo].Led[0].ISet);
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.设置快充;
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();
                                }
                            }
                            break;
                        case EQCMChage.设置快充:
                            bool checkQCV = true;
                            for (int idNo = 0; idNo < 2; idNo++)
                            {
                                uutNum = 0;

                                qcmNum = 0;

                                if (!_runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.IsRunning)
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();

                                if (_runUUT[uutNo + idNo].Para.DoRun == EDoRun.正在老化)
                                {
                                    for (int slot = 0; slot < _runUUT[uutNo + idNo].Led.Count; slot++)
                                    {
                                        if (_runUUT[uutNo + idNo].Led[slot].serialNo != string.Empty &&
                                            _runUUT[uutNo + idNo].Led[slot].monV > 2)
                                        {
                                            uutNum++;
                                            if (_runUUT[uutNo + idNo].Led[slot].monV >= _runUUT[uutNo + idNo].Led[slot].vMin &&
                                               _runUUT[uutNo + idNo].Led[slot].monV <= _runUUT[uutNo + idNo].Led[slot].vMax)
                                                qcmNum++;
                                        }
                                    }
                                }
                                if (uutNum != qcmNum)
                                {
                                    if (_runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.ElapsedMilliseconds < waitTime)
                                    {
                                        checkQCV = false;
                                    }
                                    else
                                    {
                                        Log(_runUUT[uutNo + idNo].ToString() + CLanguage.Lan("设置快充模式异常"), udcRunLog.ELog.NG);
                                    }
                                }
                            }
                            if (checkQCV || _runUUT[uutNo].OnOff.TimeRun.CurQCType == 0 || _runUUT[uutNo].OnOff.TimeRun.CurQCType == 8) //普通老化不检测快充电压
                            {
                                for (int idNo = 0; idNo < 2; idNo++)
                                {
                                    setLoadVal(uutNo + idNo);
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.设置负载;
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();
                                }
                            }
                            else if (_runUUT[uutNo].OnOff.TimeRun.CurQCType == (int)EQCM.MTK1_0)
                            {
                                for (int idNo = 0; idNo < 2; idNo++)
                                {
                                    setMTK(uutNo + idNo);
                                }
                            }
                            break;
                        case EQCMChage.设置负载:

                            bool checkI = false;

                            for (int idNo = 0; idNo < 2; idNo++)
                            {
                                if (!_runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.IsRunning)
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Restart();

                                if (_runUUT[uutNo + idNo].Para.DoRun == EDoRun.正在老化)
                                {
                                    if (_runUUT[uutNo + idNo].Led[0].monA >= _runUUT[uutNo + idNo].Led[0].Imin &&
                                       _runUUT[uutNo + idNo].Led[0].monA <= _runUUT[uutNo + idNo].Led[0].Imax)
                                        checkI = true;
                                }
                            }
                            if (checkI || _runUUT[uutNo].OnOff.TimeRun.WatchQCM.ElapsedMilliseconds > waitTime)
                            {
                                for (int idNo = 0; idNo < 2; idNo++)
                                {
                                    _runUUT[uutNo + idNo].Para.RunACVolt = _runUUT[uutNo + idNo].Para.CtrlACVolt;
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                                    _runUUT[uutNo + idNo].OnOff.TimeRun.WatchQCM.Stop();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 设置控制板报警指示状态
        /// </summary>
        /// <returns></returns>
        private bool SetMonitorErr()
        {
            try
            {
                string er = string.Empty;

                for (int uutNo = 0; uutNo < _runUUT.Count / 2; uutNo++)
                {
                    int idNo = uutNo * 2;

                    //位置禁用
                    if (_runUUT[idNo].Para.DoRun == EDoRun.位置禁用)
                        continue;

                    if (_runUUT[idNo].Para.AlarmCode != EAlarmCode.正常 || _runUUT[idNo].Para.AlarmInfo != string.Empty)
                    {
                        if (!_runUUT[idNo].Para.bErr)
                        {
                            int iCom = _runUUT[idNo].Base.ctrlCom;
                            int iAddr = _runUUT[idNo].Base.ctrlAddr;
                            _threadMon[iCom].SetIO(iAddr, EFMB_wIO.错误信号灯, 1, out er);
                            _runUUT[idNo].Para.bErr = true;
                        }
                    }
                    else
                    {
                        if (_runUUT[idNo].Para.bErr)
                        {
                            int iCom = _runUUT[idNo].Base.ctrlCom;
                            int iAddr = _runUUT[idNo].Base.ctrlAddr;
                            _threadMon[iCom].SetIO(iAddr, EFMB_wIO.错误信号灯, 0, out er);
                            _runUUT[idNo].Para.bErr = false;
                        }

                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 设置快充模式
        /// </summary>
        /// <param name="uutNo"></param>
        private void SetQCM(int uutNo, EQCM qcm, double qcv,double qci)
        {
            try
            {
                string er = string.Empty;

                if (qcm == EQCM.MTK1_0 || qcm == EQCM.MTK2_0)
                {
                    setMTK(uutNo);
                }
                else
                {
                    int iCom = _runUUT[uutNo].Base.ctrlCom;

                    int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                    CFMB_QCM fmb = new CFMB_QCM();

                    fmb.qcm = qcm;

                    fmb.qcv = qcv;

                    fmb.qci = 0;

                    if (CGlobalPara.SysPara.Dev.FCMBQCI != 0)
                    {
                        fmb.qci = qci + CGlobalPara.SysPara.Dev.FCMBQCI;
                    }

                    fmb.op = GJ.DEV.FCMB.EOP.写入;

                    _threadMon[iCom].SetFMBQCM(iAddr, fmb, out er);


                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("快充模式") + "【" + ((EQCM)_runUUT[uutNo].OnOff.TimeRun.CurQCType).ToString() + "】;" +
                                                    CLanguage.Lan("快充电压") + "【" + _runUUT[uutNo].OnOff.TimeRun.CurQCV.ToString() + "V】;" +
                                                    CLanguage.Lan("设置负载") + "【" + _runUUT[uutNo].Led[0].ISet.ToString("0.00") + "A】;" +
                                                    CLanguage.Lan("当前电流") + "【" + _runUUT[uutNo].Led[0].monA.ToString("0.00") + "A】", 
                                                    udcRunLog.ELog.Content);
                }

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设置AC ON
        /// </summary>
        /// <param name="uutNo"></param>
        private void SetACON(int uutNo, int wOnOff)
        {
            try
            {
                string er = string.Empty;

                int iCom = _runUUT[uutNo].Base.ctrlCom;

                int iAddr = _runUUT[uutNo].Base.ctrlAddr;

                CFMB_ACON fmb = new CFMB_ACON();

                fmb.acVolt = _runUUT[uutNo].OnOff.TimeRun.CurRunVolt;

                fmb.wOnOff = wOnOff;

                fmb.synC = true;

                fmb.op = GJ.DEV.FCMB.EOP.写入;

                _threadMon[iCom].SetACONOFF(iAddr, fmb, out er);

                _runUUT[uutNo].Para.CtrlACON = wOnOff;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 设置ERS
        /// <summary>
        /// 设置治具默认负载值为1A
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void clearLoadVal(int uutNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Para.ERSVon == 0)
                    CGlobalPara.SysPara.Para.ERSVon = 60;

                string er = string.Empty;

                int ersCom = _runUUT[uutNo].Base.ersCom;

                int ersAddr = _runUUT[uutNo].Base.ersAddr;

                int ersCH = _runUUT[uutNo].Base.ersCH;

                CLOAD load = new CLOAD();

                load.Mode = (EMODE)_runUUT[uutNo].Led[0].IMode;

                load.load = CGlobalPara.SysPara.Para.IdleLoad;

                load.Von = CGlobalPara.SysPara.Para.ERSVon;

                load.mark = CGlobalPara.SysPara.Para.LoadDelayS; //负载等待2S拉载

                if (!_threadERS.SetCHLoad(ersAddr, ersCH, load, out er))
                {
                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置负载值") + "【" + ersAddr.ToString("D2") + "_" + ersCH.ToString() + "】" +
                                                    CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);
                }
                else
                {
                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置负载值") + "【" + ersAddr.ToString("D2") + "_" + ersCH.ToString() + "】:" +
                                                    CGlobalPara.SysPara.Para.IdleLoad.ToString() + "A", udcRunLog.ELog.Content);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设置治具负载值
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void setLoadVal(int uutNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Para.ERSVon == 0)
                    CGlobalPara.SysPara.Para.ERSVon = 60;

                string er = string.Empty;

                int ersCom = _runUUT[uutNo].Base.ersCom;

                int ersAddr = _runUUT[uutNo].Base.ersAddr;

                int ersCH = _runUUT[uutNo].Base.ersCH;

                CLOAD load = new CLOAD();

                load.Mode = (EMODE)_runUUT[uutNo].Led[0].IMode;

                load.load = _runUUT[uutNo].Led[0].ISet;

                load.Von = CGlobalPara.SysPara.Para.ERSVon;

                load.mark = CGlobalPara.SysPara.Para.LoadDelayS; //负载等待2S拉载

                if (!_threadERS.SetCHLoad(ersAddr, ersCH, load, out er))
                {
                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置负载值") + "【" + ersAddr.ToString("D2") + "_" + ersCH.ToString() +
                                                   "】" + CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);
                }
                else
                {
                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置负载值") + "【" + ersAddr.ToString("D2") + "_" + ersCH.ToString() +
                                                   "】:" + _runUUT[uutNo].Led[0].ISet.ToString() + "A", udcRunLog.ELog.Content);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设置 MTK模式
        /// </summary>
        /// <param name="uutNo"></param>
        private void setMTK(int uutNo)
        {
            try
            {
                string er = string.Empty;

                int ersCom = _runUUT[uutNo].Base.ersCom;

                int ersAddr = _runUUT[uutNo].Base.ersAddr;

                int ersCH = _runUUT[uutNo].Base.ersCH;

                CLOAD load = new CLOAD();

                load.Mode = EMODE.MTK;

                load.load = 1;

                load.Von = CGlobalPara.SysPara.Para.ERSVon;

                EQCM qcm = (EQCM)_runUUT[uutNo].OnOff.TimeRun.CurQCType;

                double qcv = _runUUT[uutNo].OnOff.TimeRun.CurQCV;

                if (qcm == EQCM.MTK1_0)
                {
                    bool ReStart = false;

                    if (!_runUUT[uutNo].Para.MTKWatcher.IsRunning)
                    {
                        _runUUT[uutNo].Para.MTKWatcher.Restart();

                        ReStart = true;
                    }

                    if (ReStart || _runUUT[uutNo].Para.MTKWatcher.ElapsedMilliseconds > 2000)
                    {
                        load.mark = 1; //1:电压上升命令 2:电压下降命令

                        if (!_threadERS.SetCHLoad(ersAddr, ersCH, load, out er))
                        {
                            Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置MTK1.0值错误") + "【" + ersAddr.ToString("D2") + "_" + 
                                                                           ersCH.ToString() + "】:" + er, udcRunLog.ELog.NG);
                        }

                        Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置MTK1.0电压上升"), udcRunLog.ELog.Content);

                        _runUUT[uutNo].Para.MTKWatcher.Restart();
                    }
                }
                else
                {
                    load.mark = 3;  //7V

                    if (qcv == 8)
                        load.mark = 4;
                    else if (qcv == 9)
                        load.mark = 5;
                    else if (qcv == 12)
                        load.mark = 6;

                    if (!_threadERS.SetCHLoad(ersAddr, ersCH, load, out er))
                    {
                        Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置MTK2.0值错误") + "【" + ersAddr.ToString("D2") + "_" + 
                                                                     ersCH.ToString() + "】:" + er, udcRunLog.ELog.NG);
                    }

                    Log(_runUUT[uutNo].ToString() + CLanguage.Lan("设置MTK2.0:") + qcv.ToString() + "V;", udcRunLog.ELog.Content);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 线程消息
        private void OnPLCConArgs(object sender, CPLCConArgs e)
        {
            try
            {
                if (!_threadPLC.conStatus)
                    return;

                if (e.e == EMessage.异常)
                    Log(e.status, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        private void OnPLCDataArgs(object sender, CPLCDataArgs e)
        {
            try
            {
                //检测PLC通信是否断开?

                if (!_threadPLC.conStatus)
                    return;

                if (e.e == EMessage.异常)
                    Log(e.rData, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
               Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        private void OnMonConArgs(object sender, GJ.DEV.FCMB.CConArgs e)
        {
            if (CGlobalPara.SysPara.Para.NoShowAlarm)
                return;
            if (e.bErr)
                Log(e.conStatus, udcRunLog.ELog.NG);
        }
        private void OnMonDataArgs(object sender, GJ.DEV.FCMB.CDataArgs e)
        {
            if (CGlobalPara.SysPara.Para.NoShowAlarm)
                return;
            if (e.bErr)
                Log(e.rData, udcRunLog.ELog.NG);
        }
        private void OnERSConArgs(object sender, GJ.DEV.LED.CConArgs e)
        {
            if (CGlobalPara.SysPara.Para.NoShowAlarm)
                return;
            if (e.bErr)
               Log(e.conStatus, udcRunLog.ELog.NG);
        }
        private void OnERSDataArgs(object sender, GJ.DEV.LED.CDataArgs e)
        {
            if (CGlobalPara.SysPara.Para.NoShowAlarm)
                return;
            if (e.bErr)
                Log(e.rData, udcRunLog.ELog.NG);
        }
        #endregion

        #region 测试报表
        /// <summary>
        /// 子治具保存测试报表
        /// </summary>
        /// <param name="uutNo"></param>
        private void uut_save_report(int uutNo)
        {
            try
            {
                //获取保存数据时间

                DateTime saveTime = (System.Convert.ToDateTime(_runUUT[uutNo].Para.SaveDataTime)).
                                     AddMinutes(CGlobalPara.SysPara.Report.SaveReportTimes);

                int spanTime = (int)((double)CGlobalPara.SysPara.Report.SaveReportTimes * 60 / CGlobalPara.SysPara.Para.BITimeRate);

                _runUUT[uutNo].Para.SaveDataTime = (System.Convert.ToDateTime(_runUUT[uutNo].Para.SaveDataTime)).
                                                    AddSeconds(spanTime).ToString("yyyy/MM/dd HH:mm:ss");

                if (!Directory.Exists(_runUUT[uutNo].Para.SavePathName))
                    Directory.CreateDirectory(_runUUT[uutNo].Para.SavePathName);

                string filePath = _runUUT[uutNo].Para.SavePathName + "\\" + _runUUT[uutNo].Para.SaveFileName;

                if (_runUUT[uutNo].OnOff.TimeRun.CurRunVolt == 0)
                    return;

                if (_runUUT[uutNo].Para.CtrlACON == 0)
                    return;

                //if (_runUUT[uutNo].Para.CtrlACSignal == 0)
                //    return;

                bool IsExist = true;

                if (!File.Exists(filePath))
                    IsExist = false;

                StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8);

                string strWrite = string.Empty;

                string strTemp = string.Empty;

                //写入标题栏
                if (!IsExist)
                {
                    strWrite = "Model Name:," + _runUUT[uutNo].Para.ModelName;
                    sw.WriteLine(strWrite);
                    strWrite = "Location:," + _runUUT[uutNo].Base.localName;
                    sw.WriteLine(strWrite);
                    strWrite = "Id Card:," + _runUUT[uutNo].Para.IdCard;
                    sw.WriteLine(strWrite);
                    for (int i = 0; i < _runUUT[uutNo].Led.Count; i++)
                    {
                        if (_runUUT[uutNo].Led[i].serialNo != string.Empty)
                        {
                            strWrite = "Sn" + (i + 1).ToString() + ":," + _runUUT[uutNo].Led[i].serialNo;
                            sw.WriteLine(strWrite);
                        }
                    }
                    strWrite = "StartTime:," + _runUUT[uutNo].Para.StartTime;
                    sw.WriteLine(strWrite);
                    DateTime endTime = (System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime)).AddSeconds(_runUUT[uutNo].Para.BurnTime);
                    strWrite = "EndTime:," + endTime.ToString("yyyy/MM/dd HH:mm:ss");
                    sw.WriteLine(strWrite);
                    strWrite = "BurnTime(H):," + (_runUUT[uutNo].Para.BurnTime / 3600).ToString("0.0");
                    sw.WriteLine(strWrite);

                    for (int i = 0; i < _runUUT[uutNo].OnOff.OutPut.Count; i++)
                    {
                        strWrite = "NO" + (i + 1).ToString() + ":," +
                                  "VOLT:," + _runUUT[uutNo].OnOff.OutPut[i].Chan[0].Vname + "," +
                                             _runUUT[uutNo].OnOff.OutPut[i].Chan[0].Vmin.ToString() + "V~" +
                                             _runUUT[uutNo].OnOff.OutPut[i].Chan[0].Vmax.ToString() + "V,";
                        strWrite += "Current:," + _runUUT[uutNo].OnOff.OutPut[i].Chan[0].ISet.ToString() + "A," +
                                                  _runUUT[uutNo].OnOff.OutPut[i].Chan[0].Imin.ToString() + "A~" +
                                                  _runUUT[uutNo].OnOff.OutPut[i].Chan[0].Imax.ToString() + "A";
                        sw.WriteLine(strWrite);
                    }

                    strWrite = string.Empty;

                    strTemp = "Scan Time,Temp.(℃),OutPutNo,";

                    strWrite += strTemp;

                    for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                    {
                        if (_runUUT[uutNo].Led[slot].serialNo != string.Empty)
                        {
                            strTemp = (slot + 1).ToString("D2") + "(V)" + ",";
                            strWrite += strTemp;
                            strTemp = (slot + 1).ToString("D2") + "(A)" + ",";
                            strWrite += strTemp;
                        }
                    }
                    sw.WriteLine(strWrite);
                }



                strWrite = string.Empty;
                strTemp = saveTime + ",";
                strWrite += strTemp;

                strTemp = _chmr.PLC.rTemp.ToString("0.0") + ",";
                strWrite += strTemp;

                strTemp = (_runUUT[uutNo].OnOff.TimeRun.CurRunOutPut + 1).ToString() + ",";
                strWrite += strTemp;

                for (int slot = 0; slot < _runUUT[uutNo].Led.Count; slot++)
                {
                    if (_runUUT[uutNo].Led[slot].serialNo != string.Empty)
                    {
                        strTemp = _runUUT[uutNo].Led[slot].unitV.ToString() + ",";
                        strWrite += strTemp;
                        strTemp = _runUUT[uutNo].Led[slot].unitA.ToString() + ",";
                        strWrite += strTemp;
                    }
                }
                sw.WriteLine(strWrite);                

                sw.Flush();

                sw.Close();

                sw = null;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 修改测试报表保存时间
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool uut_save_report_timer(int uutNo)
        {
            try
            {
                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = "update RUN_PARA Set SaveDataTime='" + _runUUT[uutNo].Para.SaveDataTime + "'" +
                         " where UUTNO=" + (uutNo + 1).ToString();

                if (!db.excuteSQL(sqlCmd, out er))
                    Log(er, udcRunLog.ELog.NG);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
            finally
            {
            }
        }
        #endregion

        #region 产能记录
        /// <summary>
        /// 日常产能记录
        /// </summary>
        private void dailyRecord(int ttNum, int failNum)
        {
            try
            {
                if (_chmr.DayYield.dayNow == string.Empty)
                {
                    _chmr.DayYield.dayNow = DateTime.Today.ToString("yyyy-MM-dd");
                    _chmr.DayYield.ttNum = 0;
                    _chmr.DayYield.failNum = 0;
                }
                if (System.Convert.ToDateTime(_chmr.DayYield.dayNow) < DateTime.Today)
                {
                    saveDailyRecord();
                    _chmr.DayYield.dayNow = DateTime.Today.ToString("yyyy-MM-dd");
                    _chmr.DayYield.ttNum = ttNum;
                    _chmr.DayYield.failNum = failNum;
                }
                else
                {
                    _chmr.DayYield.ttNum += ttNum;
                    _chmr.DayYield.failNum += failNum;
                }
                _chmr.DayYield.yieldTTNum += ttNum;
                _chmr.DayYield.yieldFailNum += failNum;

                CIniFile.WriteToIni("DailyYield", "dayNow", _chmr.DayYield.dayNow, CGlobalPara.IniFile);
                CIniFile.WriteToIni("DailyYield", "ttNum", _chmr.DayYield.ttNum.ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("DailyYield", "failNum", _chmr.DayYield.failNum.ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("DailyYield", "yieldTTNum", _chmr.DayYield.yieldTTNum.ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("DailyYield", "yieldFailNum", _chmr.DayYield.yieldFailNum.ToString(), CGlobalPara.IniFile);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 产能记录2个小时
        /// </summary>
        /// <param name="ttNum"></param>
        /// <param name="failNum"></param>
        private void yieldRecord(int ttNum, int failNum)
        {
            try
            {
                string recordDate = CIniFile.ReadFromIni("YieldDate", "curDate", CGlobalPara.IniFile);

                string sqlCmd = string.Empty;

                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                if (recordDate == string.Empty)
                {
                    recordDate = DateTime.Now.Date.ToString();

                    CIniFile.WriteToIni("YieldDate", "curDate", recordDate, CGlobalPara.IniFile);

                    sqlCmd = "update YieldRecord set ttNum=0,passNum=0,failNum=0";

                    if (!db.excuteSQL(sqlCmd, out er))
                    {
                        Log(er, udcRunLog.ELog.Err);
                        return;
                    }
                }
                else
                {
                    if (System.Convert.ToDateTime(recordDate).Date < DateTime.Now.Date)
                    {
                        recordDate = DateTime.Now.Date.ToString();

                        CIniFile.WriteToIni("YieldDate", "curDate", recordDate, CGlobalPara.IniFile);

                        sqlCmd = "update YieldRecord set ttNum=0,passNum=0,failNum=0";

                        if (!db.excuteSQL(sqlCmd, out er))
                        {
                            Log(er, udcRunLog.ELog.Err);
                            return;
                        }
                    }
                }

                int curHour = DateTime.Now.Hour;

                int idNo = 0;

                if (curHour >= 8 && curHour < 10)
                    idNo = 0;
                else if (curHour >= 10 && curHour < 12)
                    idNo = 1;
                else if (curHour >= 12 && curHour < 14)
                    idNo = 2;
                else if (curHour >= 14 && curHour < 16)
                    idNo = 3;
                else if (curHour >= 16 && curHour < 18)
                    idNo = 4;
                else if (curHour >= 18 && curHour < 20)
                    idNo = 5;
                else if (curHour >= 20 && curHour < 22)
                    idNo = 6;
                else if (curHour >= 22 && curHour <= 23)
                    idNo = 7;
                else if (curHour >= 0 && curHour < 2)
                    idNo = 8;
                else if (curHour >= 2 && curHour < 4)
                    idNo = 9;
                else if (curHour >= 4 && curHour < 6)
                    idNo = 10;
                else if (curHour >= 6 && curHour < 8)
                    idNo = 11;

                sqlCmd = "update YieldRecord set ttNum=ttNum+" + ttNum.ToString() + "," +
                                 "passNum=passNum+" + (ttNum - failNum).ToString() +
                                 ",failNum=failNum+" + failNum.ToString() +
                                 " where idNo=" + idNo.ToString();

                if (!db.excuteSQL(sqlCmd, out er))
                {
                    Log(er, udcRunLog.ELog.Err);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 保存日产能
        /// </summary>
        private void saveDailyRecord()
        {
            try
            {
                if (CGlobalPara.SysPara.Report.DayRecordPath == string.Empty)
                    return;
                if (!Directory.Exists(CGlobalPara.SysPara.Report.DayRecordPath))
                    return;
                double passRate = 0;
                if (_chmr.DayYield.ttNum == 0)
                    passRate = 1;
                else
                    passRate = (double)(_chmr.DayYield.ttNum - _chmr.DayYield.failNum) / (double)_chmr.DayYield.ttNum;
                string fileName = _chmr.DayYield.dayNow + ".xml";
                string filePath = CGlobalPara.SysPara.Report.DayRecordPath + "\\" + fileName;
                string strXml = string.Empty;
                strXml += "<ConfigSet>" + "\r\n";
                strXml += "<!--老化日报表-->" + "\r\n";
                strXml += "<General>" + "\r\n";
                strXml += "<!--总数-->" + "\r\n";
                strXml += "<TolNum>" + _chmr.DayYield.ttNum.ToString() + "</TolNum>" + "\r\n";
                strXml += "<!--良品数-->" + "\r\n";
                strXml += "<PassNum>" + (_chmr.DayYield.ttNum - _chmr.DayYield.failNum).ToString() + "</PassNum>" + "\r\n";
                strXml += "<!--不良品数-->" + "\r\n";
                strXml += "<FailNum>" + _chmr.DayYield.failNum.ToString() + "</FailNum>" + "\r\n";
                strXml += "<!--直通率(%)-->" + "\r\n";
                strXml += "<PassRate>" + passRate.ToString("P2") + "</PassRate>" + "\r\n";
                strXml += "</General>" + "\r\n";
                strXml += "</ConfigSet>" + "\r\n";
                StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
                sw.Write(strXml);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 选择性出机种
        /// <summary>
        /// 加载当前选择性出机参数
        /// </summary>
        private void LoadSelectModelIni()
        {
            int bSel = System.Convert.ToInt32(CIniFile.ReadFromIni("OutModel", "CurSelOut", CGlobalPara.IniFile, "0"));
            if (bSel == 0)
               _chmr.CurSelOut = false;
            else
                _chmr.CurSelOut = true;

            _chmr.CurOutModel = CIniFile.ReadFromIni("OutModel", "CurOutModel", CGlobalPara.IniFile);

            _chmr.CurModelNum = System.Convert.ToInt32(CIniFile.ReadFromIni("OutModel", "CurModelNum", CGlobalPara.IniFile, "0"));

            _chmr.CurModelOutNum = System.Convert.ToInt32(CIniFile.ReadFromIni("OutModel", "CurModelOutNum", CGlobalPara.IniFile, "0"));

            RefreshCurModelList();

            RefreshCurOutModel(_chmr.CurOutModel);
        }
        /// <summary>
        /// 刷新当前机种信息
        /// </summary>
        private void RefreshCurModelList()
        {
            try
            {
                _chmr.CurModelList.Clear();

                for (int i = 0; i < _runUUT.Count; i++)
                {
                    int uutNo = i;

                    if (_runUUT[uutNo].Para.IsNull == 1)
                        continue;

                    if (_runUUT[uutNo].Para.DoRun >= EDoRun.正在进机 && _runUUT[uutNo].Para.DoRun <= EDoRun.正在出机)
                    {
                        if (!_chmr.CurModelList.Contains(_runUUT[uutNo].Para.ModelName))
                            _chmr.CurModelList.Add(_runUUT[uutNo].Para.ModelName);
                    }
                }

                UIMainArgs.CurModelList = _chmr.CurModelList;

                UIMainArgs.DoRun = EUIStatus.刷新机种;

                OnUIMainlArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }

        }
        /// <summary>
        /// 刷新当前机种信息
        /// </summary>
        /// <param name="modelName"></param>
        public void RefreshCurOutModel(string modelName)
        {
            try
            {
                _chmr.CurOutModel = modelName;

                _chmr.CurModelNum = 0;

                _chmr.CurModelOutNum = 0;

                for (int i = 0; i < _runUUT.Count; i++)
                {
                    int uutNo = i;

                    if (_runUUT[uutNo].Para.ModelName != _chmr.CurOutModel)
                        continue;

                    if (_runUUT[uutNo].Para.DoRun > EDoRun.正在进机 && _runUUT[uutNo].Para.DoRun < EDoRun.正在出机)
                    {
                        _chmr.CurModelNum++;
                    }

                    if (_runUUT[uutNo].Para.DoRun == EDoRun.老化结束)
                    {
                        _chmr.CurModelOutNum++;
                    }
                }

                if (_chmr.CurModelList.Count == 0)
                {
                    _chmr.CurOutModel = string.Empty;

                    _chmr.CurModelChange = true;
                }
                else if (!_chmr.CurModelList.Contains(_chmr.CurOutModel))
                {
                    if (CGlobalPara.SysPara.Para.ChkSameModel)
                    {
                        _chmr.CurOutModel = _chmr.CurModelList[0];
                    }

                    _chmr.CurModelChange = true;
                }
                else
                {
                    _chmr.CurModelChange = false;
                }

                CIniFile.WriteToIni("OutModel", "CurOutModel", _chmr.CurOutModel, CGlobalPara.IniFile);

                CIniFile.WriteToIni("OutModel", "CurModelNum", _chmr.CurModelNum.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("OutModel", "CurModelOutNum", _chmr.CurModelOutNum.ToString(), CGlobalPara.IniFile);

                UIMainArgs.CurModelList = _chmr.CurModelList;

                UIMainArgs.CurOutModel = _chmr.CurOutModel;

                UIMainArgs.CurModelNum = _chmr.CurModelNum;

                UIMainArgs.CurModelOutNum = _chmr.CurModelOutNum;

                UIMainArgs.CurModelChange = _chmr.CurModelChange;

                UIMainArgs.DoRun = EUIStatus.刷新数量;

                OnUIMainlArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 启用选择性出机
        /// </summary>
        /// <param name="chkFlag"></param>
        public void ChkSelOut_CheckedChanged(bool chkFlag)
        {
            _chmr.CurSelOut = chkFlag;

            if (_chmr.CurSelOut)
            {
                CIniFile.WriteToIni("OutModel", "CurSelOut", "1", CGlobalPara.IniFile);
            }
            else
            {
                CIniFile.WriteToIni("OutModel", "CurSelOut", "0", CGlobalPara.IniFile);
            }
        }
        #endregion

        #region 温度电压曲线
        /// <summary>
        /// 初始化曲线参数
        /// </summary>
        private void InitialChartPara()
        {
            try
            {
                for (int i = 0; i < CGlobalPara.C_UUT_MAX / 2; i++)
                {
                    int uutNo = i * 2;

                    string[] strLocal = _runUUT[uutNo].Base.localName.Split('-');

                    string localName = strLocal[0] + "-" + strLocal[1];

                    _RunFixture.Add(new CUnit(i, localName));

                    if (_runUUT[uutNo].Para.DoRun >= EDoRun.正在老化 && _runUUT[uutNo].Para.DoRun < EDoRun.正在出机)
                    {
                        _RunFixture[i].StartTime = _runUUT[uutNo].Para.StartTime;
                        _RunFixture[i].EndTime = _runUUT[uutNo].Para.EndTime;
                        _RunFixture[i].BurnTime = _runUUT[uutNo].Para.BurnTime;
                        LoadChartFile(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }

        }
        /// <summary>
        /// 加载曲线文件数据
        /// </summary>
        /// <param name="FixNo"></param>
        private void LoadChartFile(int FixNo)
        {
            try
            {
                string fileName = Application.StartupPath + "\\Chart\\" + _RunFixture[FixNo].ToString() + ".csv";

                if (!File.Exists(fileName))
                    return;

                StreamReader sr = new StreamReader(fileName);

                while (sr.Peek() != -1)
                {
                    string Str = sr.ReadLine();

                    string[] ArrayStr = Str.Split(',');

                    if (ArrayStr.Length < 3)
                        continue;

                    double x = System.Convert.ToDouble(ArrayStr[0]);

                    double temp = System.Convert.ToDouble(ArrayStr[1]);

                    double acv = System.Convert.ToDouble(ArrayStr[2]);

                    _RunFixture[FixNo].CurRunTime = (int)x;

                    _RunFixture[FixNo].CurTemp = temp;

                    _RunFixture[FixNo].CurACV = acv;

                    _RunFixture[FixNo].Temp.X.Add(_RunFixture[FixNo].CurRunTime);

                    _RunFixture[FixNo].Temp.Y.Add(_RunFixture[FixNo].CurTemp);

                    _RunFixture[FixNo].ACV.X.Add(_RunFixture[FixNo].CurRunTime);

                    _RunFixture[FixNo].ACV.Y.Add(_RunFixture[FixNo].CurACV);

                }

                sr.Close();

                sr = null;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 重建曲线文件数据
        /// </summary>
        /// <param name="FixNo"></param>
        private void ReNewChartFile(int FixNo)
        {
            try
            {
                int uutNo = FixNo * 2;

                string fileName = Application.StartupPath + "\\Chart";

                if (!Directory.Exists(fileName))
                    Directory.CreateDirectory(fileName);

                fileName += "\\" + _RunFixture[FixNo].ToString() + ".csv";

                if (File.Exists(fileName))
                    File.Delete(fileName);

                _RunFixture[FixNo].StartTime = _runUUT[uutNo].Para.StartTime;
                _RunFixture[FixNo].EndTime = _runUUT[uutNo].Para.EndTime;
                _RunFixture[FixNo].BurnTime = _runUUT[uutNo].Para.BurnTime;
                _RunFixture[FixNo].CurRunTime = 0;
                _RunFixture[FixNo].CountTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _RunFixture[FixNo].CurTemp = _chmr.PLC.rTemp;
                _RunFixture[FixNo].CurACV = _runUUT[uutNo].Para.CtrlACVolt;
                _RunFixture[FixNo].Temp.X.Clear();
                _RunFixture[FixNo].Temp.Y.Clear();
                _RunFixture[FixNo].ACV.X.Clear();
                _RunFixture[FixNo].ACV.Y.Clear();
                _RunFixture[FixNo].Temp.X.Add(_RunFixture[FixNo].CurRunTime);
                _RunFixture[FixNo].Temp.Y.Add(_RunFixture[FixNo].CurTemp);
                _RunFixture[FixNo].ACV.X.Add(_RunFixture[FixNo].CurRunTime);
                _RunFixture[FixNo].ACV.Y.Add(_RunFixture[FixNo].CurACV);

                StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                string Str = string.Empty;

                Str += _RunFixture[FixNo].CurRunTime.ToString() + ",";

                Str += _RunFixture[FixNo].CurTemp.ToString() + ",";

                Str += _RunFixture[FixNo].CurACV.ToString();

                sw.WriteLine(Str);

                sw.Flush();

                sw.Close();

                sw = null;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 保存曲线文件
        /// </summary>
        /// <param name="FixNo"></param>
        private bool SaveToChartFile()
        {
            try
            {
                if (CGlobalPara.SysPara.Report.TempScanTime == 0)
                    return true;

                for (int i = 0; i < CGlobalPara.C_UUT_MAX / 2; i++)
                {
                    int uutNo = i * 2;

                    if (_runUUT[uutNo].Para.DoRun == EDoRun.正在老化)
                    {
                        string fileName = Application.StartupPath + "\\Chart";

                        if (!Directory.Exists(fileName))
                            Directory.CreateDirectory(fileName);

                        fileName += "\\" + _RunFixture[i].ToString() + ".csv";

                        if (_RunFixture[i].CountTime != String.Empty)
                        {
                            int count = CTimer.DateDiff(_RunFixture[i].CountTime);

                            if (count < CGlobalPara.SysPara.Report.TempScanTime)
                                continue;
                        }
                        _RunFixture[i].CurRunTime = _runUUT[uutNo].Para.RunTime;
                        _RunFixture[i].CountTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _RunFixture[i].CurTemp = _chmr.PLC.rTemp;
                        _RunFixture[i].CurACV = _runUUT[uutNo].Para.CtrlACVolt;
                        _RunFixture[i].Temp.X.Add(_RunFixture[i].CurRunTime);
                        _RunFixture[i].Temp.Y.Add(_RunFixture[i].CurTemp);
                        _RunFixture[i].ACV.X.Add(_RunFixture[i].CurRunTime);
                        _RunFixture[i].ACV.Y.Add(_RunFixture[i].CurACV);

                        StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                        string Str = string.Empty;

                        Str += _RunFixture[i].CurRunTime.ToString() + ",";

                        Str += _RunFixture[i].CurTemp.ToString("0.0") + ",";

                        Str += _RunFixture[i].CurACV.ToString("0.0");

                        sw.WriteLine(Str);

                        sw.Flush();

                        sw.Close();

                        sw = null;

                        if (i == udcVoltChart.FixNo)
                        {
                            udcVoltChart.RefreshChart(_RunFixture[i].CurRunTime,
                                                      _RunFixture[i].CurTemp,
                                                      _RunFixture[i].CurACV);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        #endregion

        #region 本地数据库
        /// <summary>
        /// 数据库锁
        /// </summary>
        private ReaderWriterLock db_lock = new ReaderWriterLock();
        /// <summary>
        /// 数据更新时间监控
        /// </summary>
        private Stopwatch wather_db = new Stopwatch();
        /// <summary>
        /// 数据库运行时间
        /// </summary>
        private string g_DB_BAK_STARTTIMES = string.Empty;
        /// <summary>
        /// 异步委托状态
        /// </summary>
        private volatile bool db_asyn = false;
        /// <summary>
        /// 异步委托更新数据
        /// </summary>
        private delegate void update_local_db_handler();
        /// <summary>
        /// 更新数据到文件
        /// </summary>
        private void local_db_update_para_intoJson()
        {
            try
            {
                //更新测试状态

                db_asyn = true;

                wather_db.Restart();

                CUUTList uutList = new CUUTList();

                uutList.UUT = new List<CUUT_JSON>();

                for (int uutNo = 0; uutNo < CGlobalPara.C_UUT_MAX; uutNo++)
                {
                    uutList.UUT.Add(new CUUT_JSON()
                                        {
                                            UUTNO = uutNo + 1,
                                            DoRun = (int)_runUUT[uutNo].Para.DoRun,
                                            AlarmCode = (int)_runUUT[uutNo].Para.AlarmCode,
                                            AlarmTime = _runUUT[uutNo].Para.AlarmTime,
                                            AlarmInfo = _runUUT[uutNo].Para.AlarmInfo,
                                            RunTime = _runUUT[uutNo].Para.RunTime,
                                            RunACVolt = _runUUT[uutNo].Para.RunACVolt,
                                            CtrlACON = _runUUT[uutNo].Para.CtrlACON,
                                            CtrlOnOff = _runUUT[uutNo].Para.CtrlOnOff,
                                            CtrlACVolt = _runUUT[uutNo].Para.CtrlACVolt,
                                            CtrlVBus = _runUUT[uutNo].Para.CtrlVBus, 
                                            CtrlUUTONLine = _runUUT[uutNo].Para.CtrlUUTONLine,
                                            CtrlRunError = (int)_runUUT[uutNo].Para.CtrlRunError,
                                            CurStepNo = _runUUT[uutNo].OnOff.TimeRun.CurStepNo,
                                            CurOutPut =  _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut,
                                            CurACV = _runUUT[uutNo].OnOff.TimeRun.CurRunVolt,
                                            CurQType =_runUUT[uutNo].OnOff.TimeRun.CurQCType,
                                            CurQCV = _runUUT[uutNo].OnOff.TimeRun.CurQCV,
                                            CurQCM =(int)_runUUT[uutNo].OnOff.TimeRun.CurQCM, 
                                            SaveDataTime = _runUUT[uutNo].Para.SaveDataTime                        
                                        }
                                    );
                }

                StringBuilder strJson1 = new StringBuilder();

                strJson1.Append(CJSon.Serializer<CUUTList>(uutList));

                StreamWriter sw1 = new StreamWriter(CGlobalPara.UUTFile);

                sw1.Write(strJson1.ToString());

                sw1.Flush();

                sw1.Close();

                sw1 = null;

                //更新数据

                CLEDList ledList = new CLEDList();

                ledList.LED = new List<CLED_JSON>();

                for (int uutNo = 0; uutNo < CGlobalPara.C_UUT_MAX; uutNo++)
                {
                    for (int slotNo = 0; slotNo < CYOHOOApp.SlotMax; slotNo++)
                    {
                        int ledNo = uutNo * CYOHOOApp.SlotMax + slotNo;

                        ledList.LED.Add(
                                            new CLED_JSON()
                                            {
                                                LEDNO = ledNo + 1,
                                                UnitV = _runUUT[uutNo].Led[slotNo].unitV,
                                                UnitA = _runUUT[uutNo].Led[slotNo].unitA,
                                                PassResult = _runUUT[uutNo].Led[slotNo].passResult,
                                                FailResult = _runUUT[uutNo].Led[slotNo].failResult,
                                                FailEnd = _runUUT[uutNo].Led[slotNo].failEnd,
                                                FailTime = _runUUT[uutNo].Led[slotNo].failTime,
                                                FailInfo = _runUUT[uutNo].Led[slotNo].failInfo,
                                                StrJson = _runUUT[uutNo].Led[slotNo].strJson                                                 
                                            }
                                       );
                    }
                }

                StringBuilder strJson2 = new StringBuilder();

                strJson2.Append(CJSon.Serializer<CLEDList>(ledList));

                StreamWriter sw2 = new StreamWriter(CGlobalPara.LedFile);

                sw2.Write(strJson2.ToString());

                sw2.Flush();

                sw2.Close();

                sw2 = null;

                local_db_backup();

                Log(CLanguage.Lan("更新数据文本状态时间:") + wather_db.ElapsedMilliseconds.ToString("") + "ms", udcRunLog.ELog.Action, false);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_asyn = false;

                wather_db.Stop();
            }
        }
        /// <summary>
        /// 压缩数据库
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool local_db_compact(out string er)
        {
            er = string.Empty;

            try
            {
                CAccess db = new CAccess();

                if (!db.CompactDatabase(CGlobalPara.SysDB, false))
                {
                    Log(CLanguage.Lan("压缩数据库") + "【" + Path.GetFileName(CGlobalPara.SysDB) + "】"+ CLanguage.Lan("错误"), udcRunLog.ELog.NG);
                }
                else
                {
                    Log(CLanguage.Lan("压缩数据库") + "【" + Path.GetFileName(CGlobalPara.SysDB) + "】", udcRunLog.ELog.Action);
                }
                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 备份ACCESS数据库
        /// </summary>
        private void local_db_backup()
        {
            try
            {
                if (g_DB_BAK_STARTTIMES == string.Empty)
                {
                    g_DB_BAK_STARTTIMES = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                }

                if (CGlobalPara.SysPara.Para.BackUpDBTime == 0 || CTimer.DateDiff(g_DB_BAK_STARTTIMES) > CGlobalPara.SysPara.Para.BackUpDBTime * 60)
                {
                    g_DB_BAK_STARTTIMES = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    CAccess db = new CAccess();
                    db.CompactDatabase(CGlobalPara.SysDB, true);
                    Log(CLanguage.Lan("备份测试数据库") + "【" + Path.GetFileName(CGlobalPara.SysDB) + "】", udcRunLog.ELog.Action);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 创建数据表单
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool local_db_createTable(out string er)
        {
            er = string.Empty;

            try
            {
                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string[] TableNames = null;

                if (!db.GetTableNames(ref TableNames, out er))
                    return false;

                List<string> tableList = new List<string>();

                tableList = TableNames.ToList();

                if (!local_db_createTableForIdCard(tableList, out er))
                    return false;

                if (!local_db_createTableForUUT(tableList, out er))
                    return false;

                if (!local_db_createTableForLed(tableList, out er))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 创建表单栏位
        /// </summary>
        private bool local_db_createColum(out string er)
        {
            er = string.Empty;

            try
            {
                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                string columName = string.Empty;

                bool columIsExist = false;

                //创建RUN_PARA表单栏位

                columName = "strJson";

                columIsExist = false;

                string sqlCmd = "select * from RUN_PARA order by UUTNO";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    if (ds.Tables[0].Columns[i].ToString() == columName)
                        columIsExist = true;
                }

                if (!columIsExist)
                {
                    string sqlCmdAdd = "alter table " + "RUN_PARA" + " add column " + columName + " memo default null";

                    if (!db.excuteSQL(sqlCmdAdd, out er))
                    {
                        er = CLanguage.Lan("添加数据表单") + "[RUN_PARA]"+ CLanguage.Lan("字段") + "[" + columName + "]"+ CLanguage.Lan("错误:") + er;
                        return false;
                    }
                }

                //创建RUN_DATA表单栏位

                columName = "strJson";

                columIsExist = false;

                sqlCmd = "select * from RUN_DATA order by UUTNO";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    if (ds.Tables[0].Columns[i].ToString() == columName)
                        columIsExist = true;
                }
                if (!columIsExist)
                {
                    string sqlCmdAdd = "alter table " + "RUN_DATA" + " add column " + columName + " memo default null";

                    if (!db.excuteSQL(sqlCmdAdd, out er))
                    {
                        er = CLanguage.Lan("添加数据表单") + "[RUN_DATA]" + CLanguage.Lan("字段") + "[" + columName + "]" + CLanguage.Lan("错误:") + er;
                        return false;
                    }
                }

                //创建LED_DATA表单栏位

                columName = "strJson";

                columIsExist = false;

                sqlCmd = "select * from LED_DATA order by UUTNO";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    if (ds.Tables[0].Columns[i].ToString() == columName)
                        columIsExist = true;
                }
                if (!columIsExist)
                {
                    string sqlCmdAdd = "alter table " + "LED_DATA" + " add column " + columName + " memo default null";

                    if (!db.excuteSQL(sqlCmdAdd, out er))
                    {
                        er = CLanguage.Lan("添加数据表单") + "[LED_DATA]" + CLanguage.Lan("字段") + "[" + columName + "]" + CLanguage.Lan("错误:") + er;
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 获取数据库表单数据
        /// </summary>
        private bool local_db_loadTable(out string er)
        {
            er = string.Empty;

            try
            {
                string sqlCmd = string.Empty;

                DataSet ds = null;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                //基本信息

                sqlCmd = "select * from RUN_BASE order by UUTNO";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                if (ds.Tables[0].Rows.Count != CGlobalPara.C_UUT_MAX)
                {
                    er = CLanguage.Lan("数据库表单数据异常") + "【RUN_BASE】";
                    return false;
                }
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    _runUUT[i].Base.uutNo = System.Convert.ToInt32(ds.Tables[0].Rows[i]["UUTNO"].ToString());
                    _runUUT[i].Base.localName = ds.Tables[0].Rows[i]["LocalName"].ToString();
                    _runUUT[i].Base.roomNo = System.Convert.ToInt32(ds.Tables[0].Rows[i]["RoomNo"].ToString());
                    _runUUT[i].Base.fixNo = System.Convert.ToInt32(ds.Tables[0].Rows[i]["FixNo"].ToString());
                    _runUUT[i].Base.subNo = System.Convert.ToInt32(ds.Tables[0].Rows[i]["subNo"].ToString());
                    _runUUT[i].Base.iRow = System.Convert.ToInt32(ds.Tables[0].Rows[i]["iRow"].ToString());
                    _runUUT[i].Base.iCol = System.Convert.ToInt32(ds.Tables[0].Rows[i]["iCol"].ToString());
                    _runUUT[i].Base.ctrlCom = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CtrliCOM"].ToString());
                    _runUUT[i].Base.ctrlAddr = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CtrlAddr"].ToString());
                    _runUUT[i].Base.ersCom = System.Convert.ToInt32(ds.Tables[0].Rows[i]["ERSCom"].ToString());
                    _runUUT[i].Base.ersAddr = System.Convert.ToInt32(ds.Tables[0].Rows[i]["ERSAddr"].ToString());
                    _runUUT[i].Base.ersCH = System.Convert.ToInt32(ds.Tables[0].Rows[i]["ERSCH"].ToString());
                    _runUUT[i].Base.handRow = System.Convert.ToInt32(ds.Tables[0].Rows[i]["HandRow"].ToString());
                    _runUUT[i].Base.handCol = System.Convert.ToInt32(ds.Tables[0].Rows[i]["HandCol"].ToString());
                    _runUUT[i].Base.handPosName = "L" + _runUUT[i].Base.handRow.ToString() + "-" +
                                                       _runUUT[i].Base.handCol.ToString("D2");

                }

                //测试参数

                sqlCmd = "select * from RUN_PARA order by UUTNO";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                if (ds.Tables[0].Rows.Count != CGlobalPara.C_UUT_MAX)
                {
                    er = CLanguage.Lan("数据库表单数据异常") + "【RUN_PARA】";
                    return false;
                }
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    _runUUT[i].Para.IdCard = ds.Tables[0].Rows[i]["IDCard"].ToString();
                    _runUUT[i].Para.OrderName = ds.Tables[0].Rows[i]["OrderName"].ToString();
                    _runUUT[i].Para.ModelName = ds.Tables[0].Rows[i]["ModelName"].ToString();
                    _runUUT[i].Para.BurnTime = System.Convert.ToInt32(ds.Tables[0].Rows[i]["BurnTime"].ToString());
                    _runUUT[i].Para.StartTime = ds.Tables[0].Rows[i]["StartTime"].ToString();
                    _runUUT[i].Para.EndTime = ds.Tables[0].Rows[i]["EndTime"].ToString();
                    _runUUT[i].Para.IsNull = System.Convert.ToInt32(ds.Tables[0].Rows[i]["IsNull"].ToString());
                    _runUUT[i].Para.MesFlag = System.Convert.ToInt32(ds.Tables[0].Rows[i]["MesFlag"].ToString());
                    _runUUT[i].Para.DoRun = (EDoRun)System.Convert.ToInt32(ds.Tables[0].Rows[i]["doRun"].ToString());
                    _runUUT[i].Para.AlarmCode = (EAlarmCode)System.Convert.ToInt32(ds.Tables[0].Rows[i]["AlarmCode"].ToString());
                    _runUUT[i].Para.AlarmTime = System.Convert.ToInt32(ds.Tables[0].Rows[i]["AlarmTime"].ToString());
                    _runUUT[i].Para.AlarmInfo = ds.Tables[0].Rows[i]["AlarmInfo"].ToString();
                    _runUUT[i].Para.SaveDataTime = ds.Tables[0].Rows[i]["SaveDataTime"].ToString();
                    _runUUT[i].Para.SaveFileName = ds.Tables[0].Rows[i]["SaveFileName"].ToString();
                    _runUUT[i].Para.SavePathName = ds.Tables[0].Rows[i]["SavePathName"].ToString();
                    _runUUT[i].Para.UsedNum = System.Convert.ToInt32(ds.Tables[0].Rows[i]["UsedNum"].ToString());
                    _runUUT[i].Para.FailNum = System.Convert.ToInt32(ds.Tables[0].Rows[i]["FailNum"].ToString());
                    _runUUT[i].Para.IniRunTime = System.Convert.ToInt32(ds.Tables[0].Rows[i]["IniRunTime"].ToString());
                    _runUUT[i].Para.RunTime = System.Convert.ToInt32(ds.Tables[0].Rows[i]["RunTime"].ToString());
                    _runUUT[i].Para.RunACVolt = System.Convert.ToDouble(ds.Tables[0].Rows[i]["RunACVolt"].ToString());
                    _runUUT[i].Para.CtrlVBus = System.Convert.ToDouble(ds.Tables[0].Rows[i]["CtrlVBus"].ToString());
                    _runUUT[i].Para.CtrlOnOff = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CtrlOnOff"].ToString());
                    _runUUT[i].Para.CtrlACON = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CtrlACON"].ToString());
                    _runUUT[i].Para.CtrlACVolt = System.Convert.ToDouble(ds.Tables[0].Rows[i]["CtrlACVolt"].ToString());
                    _runUUT[i].Para.CtrlUUTONLine = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CtrlUUTONLine"].ToString());
                    _runUUT[i].Para.CtrlRunError = (EAlarmCode)System.Convert.ToInt32(ds.Tables[0].Rows[i]["CtrlRunError"].ToString());
                    _runUUT[i].Para.OutLevel = System.Convert.ToInt32(ds.Tables[0].Rows[i]["OutLevel"].ToString());
                    _runUUT[i].Para.OutPutChan = System.Convert.ToInt32(ds.Tables[0].Rows[i]["OutPutChan"].ToString());
                    _runUUT[i].Para.OutPutNum = System.Convert.ToInt32(ds.Tables[0].Rows[i]["OutPutNum"].ToString());
                    _runUUT[i].Para.OnOffNum = System.Convert.ToInt32(ds.Tables[0].Rows[i]["OnOffNum"].ToString());
                    _runUUT[i].Para.strJson = ds.Tables[0].Rows[i]["strJson"].ToString();
                    if (_runUUT[i].Para.strJson != string.Empty)
                        _runUUT[i].Para.valJson = CJSon.Deserialize<CLED_UUT>(_runUUT[i].Para.strJson);
                    
                    _runUUT[i].OnOff = new CUUT_ONOFF();
                    _runUUT[i].OnOff.AddItem(_runUUT[i].Para.OutPutNum, _runUUT[i].Para.OutPutChan, _runUUT[i].Para.OnOffNum);
                    _runUUT[i].OnOff.TimeSpec.BITime = _runUUT[i].Para.BurnTime;
                    _runUUT[i].OnOff.TimeSpec.OutChanNum = _runUUT[i].Para.OutPutChan;
                    _runUUT[i].OnOff.TimeSpec.OutPutNum = _runUUT[i].Para.OutPutNum;
                    _runUUT[i].OnOff.TimeSpec.OnOffNum = _runUUT[i].Para.OnOffNum;
                    _runUUT[i].OnOff.TimeRun.CurStepNo = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CurStepNo"].ToString());
                    _runUUT[i].OnOff.TimeRun.CurRunVolt = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CurACV"].ToString());
                    _runUUT[i].OnOff.TimeRun.CurRunOutPut = System.Convert.ToInt32(ds.Tables[0].Rows[i]["CurOutPut"].ToString());
                    _runUUT[i].OnOff.TimeRun.CurQCType = System.Convert.ToInt32(ds.Tables[0].Rows[i]["QcvType"].ToString());
                    _runUUT[i].OnOff.TimeRun.CurQCV = System.Convert.ToDouble(ds.Tables[0].Rows[i]["CurQCV"].ToString());
                    _runUUT[i].OnOff.TimeRun.CurQCM = (EQCMChage)System.Convert.ToInt32(ds.Tables[0].Rows[i]["CurQCM"].ToString());

                    string StrOutPut = ds.Tables[0].Rows[i]["OutPutList"].ToString();

                    if (StrOutPut != string.Empty)
                    {
                        string[] Str1 = StrOutPut.Split(';');

                        if (Str1.Length >= _runUUT[i].Para.OutPutNum)
                        {
                            for (int z = 0; z < _runUUT[i].OnOff.OutPut.Count; z++)
                            {
                                string[] Str2 = Str1[z].Split('*');

                                for (int k = 0; k < _runUUT[i].OnOff.OutPut[z].Chan.Count; k++)
                                {
                                    string[] Str3 = Str2[k].Split(',');
                                    if (Str3.Length > 7)
                                    {
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].Vname = Str3[0];
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].Vmin = System.Convert.ToDouble(Str3[1]);
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].Vmax = System.Convert.ToDouble(Str3[2]);
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].Imode = System.Convert.ToInt16(Str3[3]);
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].ISet = System.Convert.ToDouble(Str3[4]);
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].Imin = System.Convert.ToDouble(Str3[5]);
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].Imax = System.Convert.ToDouble(Str3[6]);
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].QCV = System.Convert.ToDouble(Str3[7]);
                                    }
                                    if (Str3.Length > 8)
                                    {
                                        _runUUT[i].OnOff.OutPut[z].Chan[k].QCType = System.Convert.ToInt16(Str3[8]);
                                    }
                                }
                            }
                        }
                    }

                    string StrOnOff = ds.Tables[0].Rows[i]["OnOffList"].ToString();

                    if (StrOnOff != string.Empty)
                    {
                        string[] Str1 = StrOnOff.Split(';');

                        if (Str1.Length >= _runUUT[i].OnOff.OnOff.Count)
                        {
                            for (int z = 0; z < _runUUT[i].OnOff.OnOff.Count; z++)
                            {
                                string[] Str2 = Str1[z].Split(',');
                                if (Str2.Length >= 5)
                                {
                                    _runUUT[i].OnOff.OnOff[z].ACV = System.Convert.ToInt32(Str2[0]);
                                    _runUUT[i].OnOff.OnOff[z].OnOffTime = System.Convert.ToInt32(Str2[1]);
                                    _runUUT[i].OnOff.OnOff[z].OnTime = System.Convert.ToInt32(Str2[2]);
                                    _runUUT[i].OnOff.OnOff[z].OffTime = System.Convert.ToInt32(Str2[3]);
                                    _runUUT[i].OnOff.OnOff[z].OutPutType = System.Convert.ToInt32(Str2[4]);
                                }
                            }
                        }
                    }
                }

                //测试点信息
                sqlCmd = "select * from RUN_DATA order by LEDNO";
                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;
                if (ds.Tables[0].Rows.Count != CGlobalPara.C_UUT_MAX * CYOHOOApp.SlotMax)
                {
                    er = CLanguage.Lan("数据库表单数据异常") + "【RUN_DATA】";
                    return false;
                }
                for (int i = 0; i < CGlobalPara.C_UUT_MAX; i++)
                {
                    for (int z = 0; z < CYOHOOApp.SlotMax; z++)
                    {
                        int uutNo = i * CYOHOOApp.SlotMax + z;
                        _runUUT[i].Led[z].serialNo = ds.Tables[0].Rows[uutNo]["SerialNo"].ToString();
                        _runUUT[i].Led[z].vName = ds.Tables[0].Rows[uutNo]["VName"].ToString();
                        _runUUT[i].Led[z].vMin = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["Vmin"].ToString());
                        _runUUT[i].Led[z].vMax = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["Vmax"].ToString());
                        _runUUT[i].Led[z].IMode = System.Convert.ToInt16(ds.Tables[0].Rows[uutNo]["IMode"].ToString());
                        _runUUT[i].Led[z].ISet = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["ISET"].ToString());
                        _runUUT[i].Led[z].Imin = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["IMin"].ToString());
                        _runUUT[i].Led[z].Imax = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["IMax"].ToString());
                        _runUUT[i].Led[z].qcv = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["QCV"].ToString());
                        _runUUT[i].Led[z].unitV = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["UnitV"].ToString());
                        _runUUT[i].Led[z].unitA = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["UnitA"].ToString());
                        _runUUT[i].Led[z].passResult = System.Convert.ToInt32(ds.Tables[0].Rows[uutNo]["passResult"].ToString());
                        _runUUT[i].Led[z].failResult = System.Convert.ToInt32(ds.Tables[0].Rows[uutNo]["failResult"].ToString());
                        _runUUT[i].Led[z].failEnd = System.Convert.ToInt32(ds.Tables[0].Rows[uutNo]["FailEnd"].ToString());
                        _runUUT[i].Led[z].failTime = ds.Tables[0].Rows[uutNo]["FailTime"].ToString();
                        _runUUT[i].Led[z].failInfo = ds.Tables[0].Rows[uutNo]["FailStr"].ToString();
                        _runUUT[i].Led[z].vBack = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["UnitV"].ToString());
                        _runUUT[i].Led[z].iBack = System.Convert.ToDouble(ds.Tables[0].Rows[uutNo]["UnitA"].ToString());
                        _runUUT[i].Led[z].strJson = ds.Tables[0].Rows[uutNo]["StrJson"].ToString();
                        if (_runUUT[i].Led[z].strJson != string.Empty)
                            _runUUT[i].Led[z].valJson = CJSon.Deserialize<CLED_SLOT>(_runUUT[i].Led[z].strJson);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 从文本获取测试点数据
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool local_db_dataFromJson(out string er)
        {
            er = string.Empty;

            try
            {
                if (File.Exists(CGlobalPara.UUTFile))
                {
                    StreamReader sr = new StreamReader(CGlobalPara.UUTFile);

                    StringBuilder strJson = new StringBuilder();

                    strJson.Append(sr.ReadToEnd());

                    sr.Close();

                    if (strJson.Length > 0)
                    {
                        CUUTList uutList = CJSon.Deserialize<CUUTList>(strJson.ToString());

                        if (uutList.UUT.Count == CGlobalPara.C_UUT_MAX)
                        {
                            for (int i = 0; i < CGlobalPara.C_UUT_MAX; i++)
                            {
                                _runUUT[i].Para.AlarmCode = (EAlarmCode)uutList.UUT[i].AlarmCode;
                                _runUUT[i].Para.AlarmTime = uutList.UUT[i].AlarmTime;
                                _runUUT[i].Para.AlarmInfo = uutList.UUT[i].AlarmInfo;
                                _runUUT[i].Para.RunTime = uutList.UUT[i].RunTime;
                                _runUUT[i].Para.RunACVolt = uutList.UUT[i].RunACVolt;
                                _runUUT[i].Para.CtrlACON = uutList.UUT[i].CtrlACON;
                                _runUUT[i].Para.CtrlOnOff = uutList.UUT[i].CtrlOnOff;
                                _runUUT[i].Para.CtrlACVolt = uutList.UUT[i].CtrlACVolt;
                                _runUUT[i].Para.CtrlVBus = uutList.UUT[i].CtrlVBus;
                                _runUUT[i].Para.CtrlUUTONLine = uutList.UUT[i].CtrlUUTONLine;
                                _runUUT[i].Para.CtrlRunError = (EAlarmCode)uutList.UUT[i].CtrlRunError;
                                _runUUT[i].OnOff.TimeRun.CurStepNo = uutList.UUT[i].CurStepNo;
                                _runUUT[i].OnOff.TimeRun.CurRunOutPut = uutList.UUT[i].CurOutPut;
                                _runUUT[i].OnOff.TimeRun.CurRunVolt = uutList.UUT[i].CurACV;
                                _runUUT[i].OnOff.TimeRun.CurQCType = uutList.UUT[i].CurQType;
                                _runUUT[i].OnOff.TimeRun.CurQCV = uutList.UUT[i].CurQCV;
                                _runUUT[i].OnOff.TimeRun.CurQCM = (EQCMChage)uutList.UUT[i].CurQCM;
                                _runUUT[i].Para.SaveDataTime = uutList.UUT[i].SaveDataTime;
                            }
                        }
                    }
                }

                if (File.Exists(CGlobalPara.LedFile))
                {
                    StreamReader sr = new StreamReader(CGlobalPara.LedFile);

                    StringBuilder strJson = new StringBuilder();

                    strJson.Append(sr.ReadToEnd());

                    sr.Close();

                    if (strJson.Length > 0)
                    {
                        CLEDList ledList = CJSon.Deserialize<CLEDList>(strJson.ToString());

                        if (ledList.LED.Count == CGlobalPara.C_UUT_MAX * CYOHOOApp.SlotMax)
                        {
                            for (int uutNo = 0; uutNo < CGlobalPara.C_UUT_MAX; uutNo++)
                            {
                                for (int slotNo = 0; slotNo < CYOHOOApp.SlotMax; slotNo++)
                                {
                                    int ledNo = CYOHOOApp.SlotMax * uutNo + slotNo;
                                    _runUUT[uutNo].Led[slotNo].unitV = ledList.LED[ledNo].UnitV;
                                    _runUUT[uutNo].Led[slotNo].unitA = ledList.LED[ledNo].UnitA;
                                    _runUUT[uutNo].Led[slotNo].passResult = ledList.LED[ledNo].PassResult;
                                    _runUUT[uutNo].Led[slotNo].failResult = ledList.LED[ledNo].FailResult;
                                    _runUUT[uutNo].Led[slotNo].failEnd = ledList.LED[ledNo].FailEnd;
                                    _runUUT[uutNo].Led[slotNo].failTime = ledList.LED[ledNo].FailTime;
                                    _runUUT[uutNo].Led[slotNo].failInfo = ledList.LED[ledNo].FailInfo;
                                    _runUUT[uutNo].Led[slotNo].vBack = ledList.LED[ledNo].UnitV;
                                    _runUUT[uutNo].Led[slotNo].iBack = ledList.LED[ledNo].UnitA;
                                    _runUUT[uutNo].Led[slotNo].strJson = ledList.LED[ledNo].StrJson;
                                    if (_runUUT[uutNo].Led[slotNo].strJson != string.Empty)
                                    {
                                        CLED_SLOT valJson = CJSon.Deserialize<CLED_SLOT>(_runUUT[uutNo].Led[slotNo].strJson);
                                        if (valJson != null)
                                        {
                                            _runUUT[uutNo].Led[slotNo].valJson = valJson;
                                        }
                                    }
                                }                               
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 增加入口治具ID卡记录表
        /// </summary>
        /// <param name="tableNameList"></param>
        private bool local_db_createTableForIdCard(List<string> tableNameList, out string er)
        {
            er = string.Empty;

            try
            {
                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                List<string> creatNames = new List<string>();

                string tableName = string.Empty;

                string sqlCmd = string.Empty;

                List<string> sqlCmdList = new List<string>();

                if (!tableNameList.Contains(CGlobalPara.InPlatTable))
                {
                    //创建表单

                    tableName = CGlobalPara.InPlatTable;

                    creatNames.Add(tableName);

                    sqlCmd = "Create table " + tableName +
                                  "(" +
                                  "idNo integer identity(1,1)  primary key," +
                                  "LocalName varchar(50) default null," +
                                  "IdCard varchar(50) default null," +
                                  "Slot integer not null default 0," +
                                  "SerialNo varchar(50) default null," +
                                  "Model  varchar(50) default null," +
                                  "MesFlag integer not null default 0," +
                                  "IsNull integer not null default 0," +
                                  "OrderName varchar(50) default null," +
                                  "goWayBI integer not null default 0," +
                                  "Remark1 varchar(100) default null," +
                                  "Remark2 varchar(100) default null," +
                                  "Remark3 varchar(100) default null" +
                                  ")";
                    sqlCmdList.Add(sqlCmd);

                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        string localName = "Fixture" + (idNo + 1).ToString();

                        for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        {
                            sqlCmd = string.Format("Insert into {0}(LocalName,IdCard,Slot,SerialNo,Model,OrderName) values ('{1}','{2}',{3},'{4}','{5}','{6}')",
                                                        tableName, localName, "", i, "", "", "");

                            sqlCmdList.Add(sqlCmd);

                        }
                    }
                }

                //创建不良条码记录表
                if (!tableNameList.Contains("FailRecord"))
                {
                    //创建表单

                    tableName = "FailRecord";

                    creatNames.Add(tableName);

                    sqlCmd = "Create table " + tableName +
                                "(" +
                                "idNo integer identity(1,1)  primary key," +
                                "SerialNo varchar(50) default null," +
                                "IdCard varchar(50) default null," +
                                "SlotNo integer not null default 0," +
                                "LocalName varchar(50) default null," +
                                "StartTime varchar(50) default null," +
                                "EndTime varchar(50) default null," +
                                "TestTime float not null default 0," +
                                "FailInfo varchar(255) default null," +
                                "FailTime varchar(50) default null," +
                                "ReportPath varchar(255) default null," +
                                "Remark1 varchar(100) default null," +
                                "Remark2 varchar(100) default null," +
                                "Remark3 varchar(100) default null" +
                                ")";

                    sqlCmdList.Add(sqlCmd);

                }

                if (sqlCmdList.Count > 0)
                {
                    if (!db.excuteSQLArray(sqlCmdList, out er))
                        return false;
                }

                for (int i = 0; i < creatNames.Count; i++)
                {
                    Log(CLanguage.Lan("初始化创建本地表单") + "【" + creatNames[i] + "】", udcRunLog.ELog.Action);
                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 创建库位表单
        /// </summary>
        /// <param name="tableNameList"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool local_db_createTableForUUT(List<string> tableNameList, out string er)
        {
            er = string.Empty;

            try
            {
                string tableName = "UUT_PARA";

                if (tableNameList.Contains(tableName))
                    return true;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                List<string> sqlCmdList = new List<string>();

                string sqlCmd = "Create table " + tableName +
                                         "(" +
                                         "UUTNO smallint primary key," +
                                         "DoRun smallint not null default 0," +
                                         "AlarmCode smallint  not null default 0," +
                                         "AlarmTime smallint  not null default 0," +
                                         "AlarmInfo Text default null," +
                                         "RunTime integer not null default 0," +
                                         "RunACVolt Single not null," +
                                         "CtrlACON smallint not null default 0," +
                                         "CtrlOnOff smallint not null default 0," +
                                         "CtrlACVolt Single not null default 0," +
                                         "CtrlVBus Single not null default 0," +
                                         "CtrlUUTONLine smallint not null default 0," +
                                         "CtrlRunError smallint not null default 0," +
                                         "CurStepNo smallint not null default 0," +
                                         "CurOutPut smallint not null default 0," +
                                         "CurACV smallint not null default 0," +
                                         "CurQType smallint not null default 0," +
                                         "CurQCV Single not null default 0," +
                                         "CurQCM smallint not null default 0," +
                                         "SaveDataTime varchar(50) default null" +
                                         ")";
                sqlCmdList.Add(sqlCmd);

                //创建索引
                sqlCmd = "CREATE UNIQUE INDEX idxUUTNO ON " + tableName + " (UUTNO)";

                sqlCmdList.Add(sqlCmd);

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 增加测试点采集数据表
        /// </summary>
        private bool local_db_createTableForLed(List<string> tableNameList, out string er)
        {
            er = string.Empty;

            try
            {
                string tableName = "LED_DATA";

                if (tableNameList.Contains(tableName))
                    return true;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                List<string> sqlCmdList = new List<string>();

                string sqlCmd = "Create table " + tableName +
                                         "(" +
                                         "LEDNO smallint primary key," +
                                         "UUTNO smallint not null default 0," +
                                         "SlotNo smallint  not null default 0," +
                                         "UnitV Single not null default 0," +
                                         "UnitA  Single not null," +
                                         "passResult smallint not null default 0," +
                                         "failResult smallint not null default 0," +
                                         "FailEnd smallint not null default 0," +
                                         "FailTime varchar(50) default null," +
                                         "FailStr Text default null" +
                                         ")";
                sqlCmdList.Add(sqlCmd);

                //创建索引
                sqlCmd = "CREATE UNIQUE INDEX idxLEDNO ON " + tableName + " (LEDNO)";

                sqlCmdList.Add(sqlCmd);

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 插入库位信息数据
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool local_db_insertDataForUUT(out string er, bool repair = false)
        {
            er = string.Empty;

            try
            {
                string tableName = "UUT_PARA";

                string sqlCmd = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                if (!repair)
                {
                    DataSet ds = null;

                    sqlCmd = "select * from UUT_PARA order by UUTNO";

                    if (!db.QuerySQL(sqlCmd, out ds, out er))
                        return false;

                    if (ds.Tables[0].Rows.Count == CGlobalPara.C_UUT_MAX)
                        return true;
                }

                List<string> sqlCmdList = new List<string>();

                sqlCmdList.Add(string.Format("delete * from {0}", tableName));

                for (int uutNo = 0; uutNo < CGlobalPara.C_UUT_MAX; uutNo++)
                {
                    sqlCmd = string.Format("insert into {0}(UUTNO,DoRun,AlarmCode,AlarmTime,AlarmInfo,RunTime,RunACVolt," +
                                                           "CtrlACON,CtrlOnOff,CtrlACVolt,CtrlVBus,CtrlUUTONLine,CtrlRunError,CurStepNo,CurOutPut," +
                                                           "CurACV,CurQType,CurQCV,CurQCM,SaveDataTime) values(" +
                                                            "{1},{2},{3},{4},'{5}',{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},'{20}')",
                                                            tableName, uutNo + 1, (int)_runUUT[uutNo].Para.DoRun,
                                                           (int)_runUUT[uutNo].Para.AlarmCode, _runUUT[uutNo].Para.AlarmTime, _runUUT[uutNo].Para.AlarmInfo,
                                                           _runUUT[uutNo].Para.RunTime, _runUUT[uutNo].Para.RunACVolt, _runUUT[uutNo].Para.CtrlACON,
                                                           _runUUT[uutNo].Para.CtrlOnOff, _runUUT[uutNo].Para.CtrlACVolt, _runUUT[uutNo].Para.CtrlVBus,
                                                           _runUUT[uutNo].Para.CtrlUUTONLine, (int)_runUUT[uutNo].Para.CtrlRunError, _runUUT[uutNo].OnOff.TimeRun.CurStepNo,
                                                           _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut, _runUUT[uutNo].OnOff.TimeRun.CurRunVolt, _runUUT[uutNo].OnOff.TimeRun.CurQCType,
                                                           _runUUT[uutNo].OnOff.TimeRun.CurQCV, (int)_runUUT[uutNo].OnOff.TimeRun.CurQCM, _runUUT[uutNo].Para.SaveDataTime);


                    sqlCmdList.Add(sqlCmd);
                }

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 插入测试点采集数据
        /// </summary>
        private bool local_db_insertDataForLed(out string er, bool repair = false)
        {
            er = string.Empty;

            try
            {
                string tableName = "LED_DATA";

                string sqlCmd = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                if (!repair)
                {
                    DataSet ds = null;

                    sqlCmd = "select * from LED_DATA order by LEDNO";

                    if (!db.QuerySQL(sqlCmd, out ds, out er))
                        return false;

                    if (ds.Tables[0].Rows.Count == CGlobalPara.C_UUT_MAX * CYOHOOApp.SlotMax)
                        return true;
                }

                List<string> sqlCmdList = new List<string>();

                sqlCmdList.Add(string.Format("delete * from {0}", tableName));

                for (int uutNo = 0; uutNo < CGlobalPara.C_UUT_MAX; uutNo++)
                {
                    for (int slot = 0; slot < CYOHOOApp.SlotMax; slot++)
                    {
                        int ledNo = uutNo * CYOHOOApp.SlotMax + slot;

                        sqlCmd = string.Format("insert into {0}(LEDNO,UUTNO,SlotNo,UnitV,UnitA,passResult,failResult," +
                                                               "FailEnd,FailTime,FailStr,StrJson) values(" +
                                                               "{1},{2},{3},{4},{5},{6},{7},{8},'{9}','{10}','{11}')",
                                                               tableName, ledNo + 1, uutNo + 1, slot + 1,
                                                               _runUUT[uutNo].Led[slot].unitV.ToString("0.000"), _runUUT[uutNo].Led[slot].unitA.ToString("0.000"),
                                                               _runUUT[uutNo].Led[slot].passResult, _runUUT[uutNo].Led[slot].failResult,
                                                               _runUUT[uutNo].Led[slot].failEnd, _runUUT[uutNo].Led[slot].failTime,
                                                               _runUUT[uutNo].Led[slot].failInfo,_runUUT[uutNo].Led[slot].strJson
                                                               );

                        sqlCmdList.Add(sqlCmd);

                    }
                }

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 更新进机位置数据
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_update_in_bi(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                List<string> sqlCmdList = new List<string>();

                string er = string.Empty;

                string sqlCmd = string.Empty;

                string OutPutList = string.Empty;

                for (int i = 0; i < _runUUT[uutNo].OnOff.OutPut.Count; i++)
                {
                    for (int z = 0; z < _runUUT[uutNo].Para.OutPutChan; z++)
                    {
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].Vname + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].Vmin.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].Vmax.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].Imode.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].ISet.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].Imin.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].Imax.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].QCV.ToString() + ",";
                        OutPutList += _runUUT[uutNo].OnOff.OutPut[i].Chan[z].QCType.ToString();
                        if (z < _runUUT[uutNo].Para.OutPutChan - 1)
                            OutPutList += "*";
                    }
                    if (i != _runUUT[uutNo].Para.OutPutNum - 1)
                        OutPutList += ";";
                }

                string OnOffList = string.Empty;

                for (int i = 0; i < _runUUT[uutNo].OnOff.OnOff.Count; i++)
                {
                    OnOffList += _runUUT[uutNo].OnOff.OnOff[i].ACV.ToString() + ",";
                    OnOffList += _runUUT[uutNo].OnOff.OnOff[i].OnOffTime + ",";
                    OnOffList += _runUUT[uutNo].OnOff.OnOff[i].OnTime + ",";
                    OnOffList += _runUUT[uutNo].OnOff.OnOff[i].OffTime + ",";
                    OnOffList += _runUUT[uutNo].OnOff.OnOff[i].OutPutType + ",";
                    if (i != _runUUT[uutNo].Para.OnOffNum - 1)
                        OnOffList += ";";
                }

                sqlCmd = "update RUN_PARA Set DoRun=" + (int)_runUUT[uutNo].Para.DoRun +
                                            ",IDCard='" + _runUUT[uutNo].Para.IdCard +
                                            "',IsNull=" + _runUUT[uutNo].Para.IsNull +
                                            ",MesFlag=" + _runUUT[uutNo].Para.MesFlag +
                                            ",ModelName='" + _runUUT[uutNo].Para.ModelName + 
                                            "',OrderName='" + _runUUT[uutNo].Para.OrderName +
                                            "',BurnTime=" + _runUUT[uutNo].Para.BurnTime +
                                            ",OutPutChan=" + _runUUT[uutNo].Para.OutPutChan +
                                            ",OutPutNum=" + _runUUT[uutNo].Para.OutPutNum + 
                                            ",OnOffNum=" + _runUUT[uutNo].Para.OnOffNum +
                                            ",OutPutList='" + OutPutList + 
                                            "',OnOffList='" + OnOffList + 
                                            "',AlarmCode=" + (int)_runUUT[uutNo].Para.AlarmCode +
                                            ",AlarmTime=" + _runUUT[uutNo].Para.AlarmTime + 
                                            ",AlarmInfo='" + _runUUT[uutNo].Para.AlarmInfo +
                                            "',RunTime=0,OutLevel=0,CurStepNo=" + _runUUT[uutNo].OnOff.TimeRun.CurStepNo +
                                            ",CurOutPut=" + _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut +
                                            ",CurACV=" + _runUUT[uutNo].OnOff.TimeRun.CurRunVolt +
                                            ",QcvType=" + _runUUT[uutNo].OnOff.TimeRun.CurQCType +
                                            ",CurQCV=" + _runUUT[uutNo].OnOff.TimeRun.CurQCV +
                                            ",CurQCM=" + (int)_runUUT[uutNo].OnOff.TimeRun.CurQCM +
                                            ",StrJson='" + _runUUT[uutNo].Para.strJson + "'" +
                                            " where UUTNO=" + (uutNo + 1);
                sqlCmdList.Add(sqlCmd);

                for (int i = 0; i < _runUUT[uutNo].Led.Count; i++)
                {
                    sqlCmd = "update RUN_DATA Set SerialNo='" + _runUUT[uutNo].Led[i].serialNo +
                                                 "',Vname='" + _runUUT[uutNo].Led[i].vName + 
                                                 "',Vmin=" + _runUUT[uutNo].Led[i].vMin.ToString() +
                                                 ",Vmax=" + _runUUT[uutNo].Led[i].vMax.ToString() +
                                                 ",IMode=" + _runUUT[uutNo].Led[i].IMode +
                                                 ",ISet=" + _runUUT[uutNo].Led[i].ISet + 
                                                 ",IMin=" + _runUUT[uutNo].Led[i].Imin +
                                                 ",IMax=" + _runUUT[uutNo].Led[i].Imax +
                                                 ",UnitV=" + _runUUT[uutNo].Led[i].unitV +
                                                 ",UnitA=" + _runUUT[uutNo].Led[i].unitA + 
                                                 ",passResult=0,failResult=0,FailEnd=0,FailTime='',FailStr=''" +
                                                 ",StrJson='" + _runUUT[uutNo].Led[i].strJson + "'" +
                                                 " where UUTNO=" + (uutNo + 1).ToString() + " and SlotNo=" + (i + 1).ToString();
                    sqlCmdList.Add(sqlCmd);
                }

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    Log(er, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 设置老化开始
        /// </summary>
        /// <param name="uuutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_update_bi_start(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = "Update RUN_PARA set DoRun=" + (int)_runUUT[uutNo].Para.DoRun + ",StartTime='" + _runUUT[uutNo].Para.StartTime + "',EndTime='" +
                              _runUUT[uutNo].Para.EndTime + "',SaveDataTime='" + _runUUT[uutNo].Para.SaveDataTime +
                              "',SaveFileName='" + _runUUT[uutNo].Para.SaveFileName + "'," + "SavePathName='" +
                              _runUUT[uutNo].Para.SavePathName + "',IniRunTime=" + _runUUT[uutNo].Para.IniRunTime.ToString() +
                              " where UUTNO=" + (uutNo + 1);

                if (!db.excuteSQL(sqlCmd, out er))
                   Log(er, udcRunLog.ELog.NG);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 设置老化结束
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_set_bi_end(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = "update RUN_PARA set DoRun=" + (int)_runUUT[uutNo].Para.DoRun +
                               ",RunTime=BurnTime,EndTime='" + _runUUT[uutNo].Para.EndTime + "'" +
                               " where UUTNO=" + (uutNo + 1);

                if (!db.excuteSQL(sqlCmd, out er))
                    Log(er, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 设置母治具状态
        /// </summary>
        /// <param name="uutNo"></param>
        private void local_db_update_fix_status(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = "update RUN_PARA Set doRun=" + (int)_runUUT[uutNo].Para.DoRun + ",IdCard='" + _runUUT[uutNo].Para.IdCard + "',AlarmCode=" +
                                (int)_runUUT[uutNo].Para.AlarmCode + ",AlarmTime=" + _runUUT[uutNo].Para.AlarmTime + ",AlarmInfo='" +
                                _runUUT[uutNo].Para.AlarmInfo + "',IsNull=" + _runUUT[uutNo].Para.IsNull + ",OutLevel=" +
                               _runUUT[uutNo].Para.OutLevel + ",UsedNum=" + _runUUT[uutNo].Para.UsedNum + ",FailNum=" + _runUUT[uutNo].Para.FailNum +
                               ",CurStepNo=" + _runUUT[uutNo].OnOff.TimeRun.CurStepNo + ",CurOutPut=" + _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut +
                               ",CurACV=" + _runUUT[uutNo].OnOff.TimeRun.CurRunVolt +
                               ",QcvType=" + _runUUT[uutNo].OnOff.TimeRun.CurQCType +
                               ",CurQCV=" + _runUUT[uutNo].OnOff.TimeRun.CurQCV +
                               ",CurQCM=" + (int)_runUUT[uutNo].OnOff.TimeRun.CurQCM +
                               " where UUTNO=" + (uutNo + 1).ToString();
                if (!db.excuteSQL(sqlCmd, out er))
                    Log(er, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 设置治具使用次数
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_add_usedNum(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                _runUUT[uutNo].Para.UsedNum++;

                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = "Update RUN_PARA set UsedNum=UsedNum+1 where UUTNO=" + (uutNo + 1);

                if (!db.excuteSQL(sqlCmd, out er))
                    Log(er, udcRunLog.ELog.NG);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 快充模式切换
        /// </summary>
        /// <param name="uutNo"></param>
        private void local_db_qcm_change(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                string er = string.Empty;

                List<string> sqlCmdList = new List<string>();

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = "update RUN_PARA Set CurStepNo=" + _runUUT[uutNo].OnOff.TimeRun.CurStepNo + ",CurACV=" +
                                _runUUT[uutNo].OnOff.TimeRun.CurRunVolt + ",CurOutPut=" + _runUUT[uutNo].OnOff.TimeRun.CurRunOutPut +
                                ",QcvType=" + _runUUT[uutNo].OnOff.TimeRun.CurQCType + 
                                ",CurQCV=" + _runUUT[uutNo].OnOff.TimeRun.CurQCV + 
                                ",CurQCM=" + (int)_runUUT[uutNo].OnOff.TimeRun.CurQCM +
                                ",RunTime=" + _runUUT[uutNo].Para.RunTime + " where UUTNO=" + (uutNo + 1).ToString();
                sqlCmdList.Add(sqlCmd);

                sqlCmd = "update RUN_DATA Set VName='" + _runUUT[uutNo].Led[0].vName + "',Vmin=" + _runUUT[uutNo].Led[0].vMin +
                       ",Vmax=" + _runUUT[uutNo].Led[0].vMax + ",IMode=" + _runUUT[uutNo].Led[0].IMode + ",ISET=" +
                       _runUUT[uutNo].Led[0].ISet + ",IMin=" + _runUUT[uutNo].Led[0].Imin + ",IMax=" +
                       _runUUT[uutNo].Led[0].Imax + ",QCV=" + _runUUT[uutNo].Led[0].qcv + ",UnitV=" +
                       _runUUT[uutNo].Led[0].unitV + ",UnitA=" + _runUUT[uutNo].Led[0].unitA +
                       " where UUTNO=" + (uutNo + 1).ToString();
                sqlCmdList.Add(sqlCmd);

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    Log(er, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 插入不良条码记录
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_recordFailSn(int uutNo)
        {
            try
            {
                db_lock.AcquireWriterLock(-1);

                List<string> sqlCmdList = new List<string>();

                string sqlCmd = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                sqlCmd = "delete * from FailRecord where datediff('d', startTime,Now()) > 30";

                sqlCmdList.Add(sqlCmd);

                for (int i = 0; i < _runUUT[uutNo].Led.Count; i++)
                {
                    if (_runUUT[uutNo].Led[i].serialNo == string.Empty)
                        continue;

                    if (_runUUT[uutNo].Led[i].failResult != 0)
                    {
                        sqlCmd = "delete * from FailRecord where serialNo='" + _runUUT[uutNo].Led[i].serialNo + "'";

                        sqlCmdList.Add(sqlCmd);

                        string fileName = _runUUT[uutNo].Para.SavePathName + "\\" + _runUUT[uutNo].Para.SaveFileName;

                        sqlCmd = string.Format("insert into FailRecord(SerialNo,IdCard,SlotNo,localName,StartTime,EndTime," +
                                                "TestTime,FailInfo,FailTime,ReportPath) values ('{0}','{1}',{2},'{3}','{4}','{5}',{6},'{7}','{8}','{9}')",
                                                _runUUT[uutNo].Led[i].serialNo, _runUUT[uutNo].Para.IdCard, i + 1,
                                                _runUUT[uutNo].Base.localName, _runUUT[uutNo].Para.StartTime, _runUUT[uutNo].Para.EndTime,
                                                _runUUT[uutNo].Para.BurnTime, _runUUT[uutNo].Led[i].failInfo, _runUUT[uutNo].Led[i].failTime,
                                                fileName);

                        sqlCmdList.Add(sqlCmd);
                    }
                }

                string er = string.Empty;

                if (!db.excuteSQLArray(sqlCmdList, out er))
                    Log(er, udcRunLog.ELog.NG);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 保存入口治具ID卡信息
        /// </summary>
        private bool local_db_save_fixture(out string er)
        {
            er = string.Empty;

            try
            {
                db_lock.AcquireWriterLock(-1);

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = string.Empty;

                DataSet ds = null;

                for (int idNo = 0; idNo < _chmr.Entrance.Fixture.Count; idNo++)
                {
                    string localName = "Fixture" + (idNo + 1).ToString();

                    List<string> sqlCmdList = new List<string>();

                    sqlCmd = "select * from " + CGlobalPara.InPlatTable + " where LocalName='" + localName + "' order by idNo";

                    if (!db.QuerySQL(sqlCmd, out ds, out er))
                        return false;

                    if (ds.Tables[0].Rows.Count != CYOHOOApp.SlotMax)
                    {
                        sqlCmd = "delete * from " + CGlobalPara.InPlatTable + " where LocalName='" + localName + "'";

                        if (!db.excuteSQL(sqlCmd, out er))
                            return false;

                        for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        {
                            sqlCmd = "Insert into " + CGlobalPara.InPlatTable + "(LocalName,IdCard,Slot,SerialNo,Model,MesFlag," +
                                     "IsNull,goWayBI,OrderName,Remark1,Remark2,Remark3) values('" + localName + "','" + _chmr.Entrance.Fixture[idNo].idCard + "'," +
                                      i.ToString() + ",'" + _chmr.Entrance.Fixture[idNo].serialNo[i] + "','" + _chmr.Entrance.Fixture[idNo].modelName + "'," +
                                      _chmr.Entrance.Fixture[idNo].MesFlag.ToString() + "," + _chmr.Entrance.Fixture[idNo].IsFixNull.ToString() + "," +
                                      _chmr.Entrance.GoWayBI.ToString() + ",'" + _chmr.Entrance.Fixture[idNo].orderName + "','','','')";
                            sqlCmdList.Add(sqlCmd);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        {
                            sqlCmd = "update " + CGlobalPara.InPlatTable + " set IdCard='" + _chmr.Entrance.Fixture[idNo].idCard + "'," +
                                   "SerialNo='" + _chmr.Entrance.Fixture[idNo].serialNo[i] + "',Model='" + _chmr.Entrance.Fixture[idNo].modelName + "'," +
                                   "MesFlag=" + _chmr.Entrance.Fixture[idNo].MesFlag.ToString() + ",IsNull=" + _chmr.Entrance.Fixture[idNo].IsFixNull.ToString() + "," +
                                   "goWayBI=" + _chmr.Entrance.GoWayBI.ToString() + ",OrderName='" + _chmr.Entrance.Fixture[idNo].orderName + "'" +
                                  " where LocalName='" + localName + "' and Slot=" + i.ToString();
                            sqlCmdList.Add(sqlCmd);
                        }
                    }

                    if (!db.excuteSQLArray(sqlCmdList, out er))
                        return false;

                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 获取入口治具ID卡信息
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool local_db_get_fixture(out string er)
        {
            er = string.Empty;

            try
            {
                db_lock.AcquireWriterLock(-1);

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = string.Empty;

                DataSet ds = null;

                for (int idNo = 0; idNo < _chmr.Entrance.Fixture.Count; idNo++)
                {
                    string localName = "Fixture" + (idNo + 1).ToString();

                    sqlCmd = "select * from " + CGlobalPara.InPlatTable + " where LocalName='" + localName + "' order by Slot";

                    if (!db.QuerySQL(sqlCmd, out ds, out er))
                        return false;

                    if (ds.Tables[0].Rows.Count != CYOHOOApp.SlotMax)
                    {
                        er = CLanguage.Lan("获取入口治具ID信息错误:") + ds.Tables[0].Rows.Count.ToString();

                        return false;
                    }

                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                    {
                        _chmr.InPlatForm.Fixture[idNo].idCard = ds.Tables[0].Rows[i]["IdCard"].ToString();
                        _chmr.InPlatForm.Fixture[idNo].modelName = ds.Tables[0].Rows[i]["Model"].ToString();
                        _chmr.InPlatForm.Fixture[idNo].orderName = ds.Tables[0].Rows[i]["OrderName"].ToString();
                        _chmr.InPlatForm.Fixture[idNo].serialNo[i] = ds.Tables[0].Rows[i]["SerialNo"].ToString();
                        _chmr.InPlatForm.Fixture[idNo].MesFlag = System.Convert.ToInt16(ds.Tables[0].Rows[i]["MesFlag"].ToString());
                        _chmr.InPlatForm.Fixture[idNo].IsFixNull = System.Convert.ToInt16(ds.Tables[0].Rows[i]["IsNull"].ToString());
                        _chmr.InPlatForm.GoWayBI = System.Convert.ToInt16(ds.Tables[0].Rows[i]["goWayBI"].ToString());
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {
                db_lock.ReleaseWriterLock();
            }
        }
        #endregion

        #region 客户MES
        /// <summary>
        /// 检测MES状态
        /// </summary>
        /// <returns></returns>
        private bool CheckMESStatus()
        {
            try
            {
                OnUIActionArgs.OnEvented(new CUIUserArgs<CUIActionAgrs>(0, "Action",
                                                                        new CUIActionAgrs(0, string.Empty)
                                                                        ));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 设置报警提示
        /// </summary>
        /// <param name="alarmFlag"></param>
        /// <param name="alarmInfo"></param>
        private void SetMesStatus(int alarmFlag, string alarmInfo)
        {
            try
            {
                OnUIActionArgs.OnEvented(new CUIUserArgs<CUIActionAgrs>(0, "AlarmAction",
                                                                        new CUIActionAgrs(alarmFlag, alarmInfo)
                                                                        ));
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 检查客户MES连线是否正常
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckTestConnect()
        {
            try
            {
                string er = string.Empty;

                CSFCS mes = new CSFCS(0, "WebAPI", CYOHOOApp.Custom);

                if (!mes.Start(out er))
                {
                    Log(er, udcRunLog.ELog.NG);
                    return false;
                }

                Log(CLanguage.Lan("连接客户MES服务器正常"), udcRunLog.ELog.Action);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检查条码
        /// </summary>
        /// <param name="Sn"></param>
        /// <returns></returns>
        private bool CheckSn(string Sn, out string er)
        {
            er = string.Empty;

            try
            {
                CSFCS mes = new CSFCS(0, "WebAPI", CYOHOOApp.Custom);

                if (mes.State != EMesState.正常)
                {
                    er = mes.Message;
                    return false;
                }

                CSFCS.CSnInfo Info = new CSFCS.CSnInfo()
                {
                    UserId = CGlobalPara.LogName,
                    StatName = CYOHOOApp.BI_FlowName,
                    SerialNo = Sn,
                    OrderName = string.Empty,
                    Remark1 = string.Empty,
                    Remark2 = string.Empty
                };

                if (mes.CheckSn(Info, out er))
                {
                    Log(CLanguage.Lan("条码") + "【" + Sn + "】" + CLanguage.Lan("当前站别为【老化位】,MES检查条码OK"), udcRunLog.ELog.Action);

                    return true;
                }

                if (mes.State == EMesState.网络异常)
                {
                    SetMesStatus(1, "MES系统网络异常,请检查网络是否正常?");
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 上传客户MES
        /// </summary>
        /// <param name="uutNo"></param>
        /// <returns></returns>
        private void TranSnToMES(int uutNo)
        {
            string er = string.Empty;

            try
            {
                if (_runUUT[uutNo].Para.IsNull == 1 || _runUUT[uutNo].Para.MesFlag==0)
                    return;

                DateTime StartTime = DateTime.Now;

                DateTime EndTime = DateTime.Now;

                string BITime = ((double)_runUUT[uutNo].Para.BurnTime / 3600).ToString() + "H";

                bool uutPass = false;

                CSFCS mes = new CSFCS(0, "WebAPI", CYOHOOApp.Custom);

                if (mes.State != EMesState.正常)
                {
                    er = mes.Message;
                    return;
                }

                for (int slotNo = 0; slotNo < _runUUT[uutNo].Led.Count; slotNo++)
                {
                    if (_runUUT[uutNo].Led[slotNo].serialNo == string.Empty)
                        continue;

                    if (_runUUT[uutNo].Para.StartTime != string.Empty)
                    {
                        StartTime = System.Convert.ToDateTime(_runUUT[uutNo].Para.StartTime);
                    }
                    if (_runUUT[uutNo].Para.EndTime != string.Empty)
                    {
                        EndTime = System.Convert.ToDateTime(_runUUT[uutNo].Para.EndTime);
                    }
                    string UUTResult = (_runUUT[uutNo].Led[slotNo].failResult == 0 ? "PASS" : "FAIL");
                   
                    string FailCode = string.Empty;
                   
                    string FailTime = string.Empty;
                   
                    if (!CalTestValue(uutNo, slotNo, out FailCode, out FailTime))
                        continue;

                    CSFCS.CSnData SnData = new CSFCS.CSnData()
                    {
                        DeviceId = "0",
                        DeviceName = CNet.HostIP(),
                        StatName = CYOHOOApp.BI_FlowName,
                        StatDesc = CLanguage.Lan("老化测试"),
                        Fixture = _runUUT[uutNo].Para.IdCard + (slotNo + 1).ToString("D2"),
                        LocalName = _runUUT[uutNo].Base.localName,
                        StartTime = StartTime.ToString("yyyy/MM/dd HH:mm:ss"),
                        EndTime = EndTime.ToString("yyyy/MM/dd HH:mm:ss"),
                        Model = _runUUT[uutNo].Para.ModelName,
                        OrderName = CYOHOOApp.OrderName,
                        SerialNo = _runUUT[uutNo].Led[slotNo].serialNo,
                        Result = _runUUT[uutNo].Led[slotNo].failResult,
                        Remark1 = string.Empty,
                        Remark2 = string.Empty,
                        Item = new List<CSFCS.CSnItem>()
                    };

                    for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                    {
                        double Vmax = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax;

                        double Vmin = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin;

                        string IName = _runUUT[uutNo].Para.valJson.Value[stepNo].ISet.ToString() + "A";

                        double Imax = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax;

                        double Imin = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin;

                        int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;

                        double unitV = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV;

                        double unitA = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA;

                        int VoltResult = 0;

                        string VoltDes = string.Empty;

                        int CurResult = 0;
                        
                        string CurDes = string.Empty;

                        if (unitV < Vmin)
                        {
                            VoltDes = "[电压偏下限]";
                            VoltResult = 1;
                        }
                        if (unitV > Vmax)
                        {
                            VoltDes = "[电压偏上限]";
                            VoltResult = 1;
                        }
                        if (unitA < Imin)
                        {
                            CurDes = "[电流偏下限]";
                            CurResult = 1;
                        }
                        if (unitA > Imax)
                        {
                            CurDes = "[电流偏上限]";
                            CurResult = 1;
                        }

                        CSFCS.CSnItem SnItem1 = new CSFCS.CSnItem()
                        {
                            IdNo = stepNo,
                            Name = _runUUT[uutNo].Para.valJson.Value[stepNo].Name,
                            Desc = ((EQCM)_runUUT[uutNo].Para.valJson.Value[stepNo].QCM).ToString(),
                            LowLimit = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin.ToString(),
                            UpLimit = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax.ToString(),
                            Value = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV.ToString("0.000"),
                            Unit = "V",
                            Result = VoltResult,
                            ErroCode = VoltDes,
                            ErrInfo = FailTime,
                            Remark1 = string.Empty,
                            Remark2 = string.Empty
                        };
                        SnData.Item.Add(SnItem1);

                        CSFCS.CSnItem SnItem2 = new CSFCS.CSnItem()
                        {
                            IdNo = 1,
                            Name = CLanguage.Lan("电流测试"),
                            Desc = _runUUT[uutNo].Para.valJson.Value[stepNo].ISet.ToString() + "A",
                            LowLimit = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin.ToString(),
                            UpLimit = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax.ToString(),
                            Value = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA.ToString("0.000"),
                            Unit = "A",
                            Result = CurResult,
                            ErroCode = CurDes,
                            ErrInfo = FailTime,
                            Remark1 = string.Empty,
                            Remark2 = string.Empty
                        };
                        SnData.Item.Add(SnItem2);

                    }

                    if (!mes.TranSnData(SnData, out er))
                    {
                        Log(CLanguage.Lan("治具ID") + "[" + _runUUT[uutNo].Para.IdCard + "-" + (slotNo + 1).ToString("D2") + "]" +
                            CLanguage.Lan("条码") + "【" + _runUUT[uutNo].Led[slotNo].serialNo + "】" +
                            CLanguage.Lan("上传MES结果") + "【" + UUTResult + "】" + "错误:" + er, udcRunLog.ELog.NG);

                        if (mes.State == EMesState.网络异常)
                        {
                            SetMesStatus(1, "MES系统网络异常,请检查网络是否正常?");
                            break;
                        }
                    }
                    else
                    {
                        uutPass = true;

                        Log(CLanguage.Lan("治具ID") + "[" + _runUUT[uutNo].Para.IdCard + "-" + (slotNo + 1).ToString("D2") + "]" +
                            CLanguage.Lan("条码") + "【" +  _runUUT[uutNo].Led[slotNo].serialNo + "】" +
                            CLanguage.Lan("上传MES结果") + "【" + UUTResult + "】OK", udcRunLog.ELog.OK);
                    }
                }

                if (!uutPass)
                {
                    SetMesStatus(2, "治具ID[" + _runUUT[uutNo].Para.IdCard + "]上传MES结果所有FAIL,请检查.");
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region PLC报警日志
        /// <summary>
        /// PLC报警列表
        /// </summary>
        private CPLCAlarmList _PLCAlarmList = null;
        /// <summary>
        /// 检测PLC报警信息
        /// </summary>
        /// <param name="devName"></param>
        /// <param name="threadPLC"></param>
        /// <returns></returns>
        private bool CheckPLCAlarm()
        {
            try
            {
                string er = string.Empty;

                List<int> regVal = new List<int>();

                for (int i = 0; i < CPLCAlarmList.C_BIT_MAX; i++)
                {
                    int val = _threadPLC.rREG_Val(InpPLC(EPLCINP.PLC报警列表01, i));

                    if (val < 0)
                        val = 0;

                    regVal.Add(val);
                }

                List<CPLCAlarmReg> alarmReg = null;

                List<CPLCAlarmReg> releaseReg = null;

                if (!_PLCAlarmList.GetAlarmCode(regVal, out alarmReg, out releaseReg, out er))
                {
                    Log(_PLCAlarmList.ToString() + er, udcRunLog.ELog.NG);
                    return false;
                }

                //报警记录
                CWeb2.CAlarm Alarm = new CWeb2.CAlarm();
                Alarm.Base.LineNo = CYOHOOApp.LineNo;
                Alarm.Base.LineName = CYOHOOApp.LineName;
                Alarm.Base.StatName = _PLCAlarmList.ToString();
                Alarm.Base.StatGuid = CNet.HostName();
                for (int i = 0; i < alarmReg.Count; i++)
                {
                    CWeb2.CAlarm_Para para = new CWeb2.CAlarm_Para();
                    para.ErrNo = alarmReg[i].idNo;
                    para.ErrCode = alarmReg[i].RegNo.ToString() + "_" + alarmReg[i].RegBit.ToString("D2");
                    para.bAlarm = alarmReg[i].CurVal;
                    para.AlarmInfo = alarmReg[i].RegFun;
                    Alarm.Para.Add(para);
                    PLCLog("<" + _PLCAlarmList.ToString() + ">" + alarmReg[i].RegFun, udcRunLog.ELog.NG);
                }
                for (int i = 0; i < releaseReg.Count; i++)
                {
                    CWeb2.CAlarm_Para para = new CWeb2.CAlarm_Para();
                    para.ErrNo = releaseReg[i].idNo;
                    para.ErrCode = releaseReg[i].RegNo.ToString() + "_" + releaseReg[i].RegBit.ToString("D2");
                    para.bAlarm = releaseReg[i].CurVal;
                    para.AlarmInfo = releaseReg[i].RegFun;
                    Alarm.Para.Add(para);
                    PLCLog("<" + _PLCAlarmList.ToString() + ">" + CLanguage.Lan("解除") + "[" + releaseReg[i].RegFun + "]", udcRunLog.ELog.Action);
                }

                if (Alarm.Para.Count > 0)
                {
                    Stopwatch watcher = new Stopwatch();

                    watcher.Start();

                    if (!CWeb2.InsertAlarmRecord(Alarm, out er))
                    {
                        Log(CLanguage.Lan("上传PLC报警信息错误:") + er, udcRunLog.ELog.NG);
                        return false;
                    }

                    watcher.Stop();

                    Log(CLanguage.Lan("上传PLC报警信息到Web服务器:") + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);
                }

                return true;
            }
            catch (Exception ex)
            {
                PLCLog(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 良品预警功能
        private int _WarnIdNo = 0;
        private string _WarnName = "BURNIN";
        /// <summary>
        /// 预警类
        /// </summary>
        private CWarnRate _WarnRate = new CWarnRate();
        /// <summary>
        /// 刷新日常统计
        /// </summary>
        /// <returns></returns>
        private bool RefreshDailyPassRate()
        {
            try
            {
                //是否启用预警功能
                if (!CGlobalPara.SysPara.Alarm.ChkPassRate)
                    return true;

                if (_WarnRate.CurrentDate == string.Empty)
                {
                    _WarnRate.CurrentDate = DateTime.Now.ToString("yyyy/MM/dd");
                    _WarnRate.CurrentDayClr = 0;
                    _WarnRate.CurrentNightClr = 0;
                }

                if (System.Convert.ToDateTime(_WarnRate.CurrentDate).Date < DateTime.Now.Date)
                {
                    _WarnRate.CurrentDate = DateTime.Now.ToString("yyyy/MM/dd");
                    _WarnRate.CurrentDayClr = 0;
                    _WarnRate.CurrentNightClr = 0;
                }

                if (CGlobalPara.SysPara.Alarm.ChkClrDay && _WarnRate.CurrentDayClr == 0)
                {
                    if (CGlobalPara.SysPara.Alarm.ClrDayTime == string.Empty)
                    {
                        CGlobalPara.SysPara.Alarm.ClrDayTime = "08:00:00";
                    }
                    string dayTime = DateTime.Now.ToString("yyyy/MM/dd") + " " + CGlobalPara.SysPara.Alarm.ClrDayTime;
                    if (System.Convert.ToDateTime(dayTime) <= DateTime.Now)
                    {
                        _WarnRate.TTNum = 0;
                        _WarnRate.PassNum = 0;
                        _WarnRate.CurrentDayClr = 1;
                    }
                }

                if (CGlobalPara.SysPara.Alarm.ChkClrNight && _WarnRate.CurrentNightClr == 0)
                {
                    if (CGlobalPara.SysPara.Alarm.ClrNightTime == string.Empty)
                    {
                        CGlobalPara.SysPara.Alarm.ClrNightTime = "20:00:00";
                    }
                    string dayTime = DateTime.Now.ToString("yyyy/MM/dd") + " " + CGlobalPara.SysPara.Alarm.ClrNightTime;
                    if (System.Convert.ToDateTime(dayTime) <= DateTime.Now)
                    {
                        _WarnRate.TTNum = 0;
                        _WarnRate.PassNum = 0;
                        _WarnRate.CurrentNightClr = 1;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
            finally
            {
                SaveIniForPassRate();
            }
        }
        /// <summary>
        /// 良率预警功能
        /// </summary>
        /// <returns></returns>
        private bool CheckPassRateAlarm()
        {
            try
            {
                if (!CGlobalPara.SysPara.Alarm.ChkPassRate)
                {
                    _WarnRate.bAlarm = 0;
                    _WarnRate.DoRun = EWarnResult.未启动;
                    OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));
                    return true;
                }

                bool CheckOK = true;

                if (_WarnRate.TTNum == 0)
                    _WarnRate.PassRate = 1;
                else
                    _WarnRate.PassRate = (double)_WarnRate.PassNum / (double)_WarnRate.TTNum;

                if (_WarnRate.TTNum >= CGlobalPara.SysPara.Alarm.PassRateStartNum &&
                    _WarnRate.PassRate < CGlobalPara.SysPara.Alarm.PassRateLimit)
                {
                    CheckOK = false;
                }

                switch (_WarnRate.DoRun)
                {
                    case EWarnResult.空闲:
                        if (CheckOK)
                        {
                            _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 0);
                            _WarnRate.DoRun = EWarnResult.正常;
                            _WarnRate.bAlarm = 0;
                            _WarnRate.Watcher.Stop();
                        }
                        else
                        {
                            _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 1);
                            _WarnRate.DoRun = EWarnResult.报警;
                            _WarnRate.Watcher.Restart();
                            _WarnRate.bAlarm = 1;
                            Log(_WarnRate.ToString() + CLanguage.Lan("当前良率") + "[" + _WarnRate.PassRate.ToString("P2") +
                                                       "]" + CLanguage.Lan("低于设定值") + "[" + CGlobalPara.SysPara.Alarm.PassRateLimit.ToString("P2") +
                                                       "]," + CLanguage.Lan("请检查确认."), udcRunLog.ELog.NG);
                        }
                        break;
                    case EWarnResult.正常:
                        if (!CheckOK)
                        {
                            _WarnRate.bAlarm = 0;
                            _WarnRate.DoRun = EWarnResult.报警;
                            _WarnRate.Watcher.Restart();
                        }
                        break;
                    case EWarnResult.报警:
                        if (CheckOK)
                        {
                            _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 0);
                            _WarnRate.bAlarm = 0;
                            _WarnRate.DoRun = EWarnResult.正常;
                            _WarnRate.Watcher.Stop();
                        }
                        else
                        {
                            if (_WarnRate.Watcher.ElapsedMilliseconds >= CGlobalPara.SysPara.Alarm.PassRateAlarmTime * 60000)
                            {
                                _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 1);
                                _WarnRate.DoRun = EWarnResult.报警;
                                _WarnRate.Watcher.Restart();
                                _WarnRate.bAlarm = 1;
                                Log(_WarnRate.ToString() + CLanguage.Lan("当前良率") + "[" + _WarnRate.PassRate.ToString("P2") +
                                                           "]" + CLanguage.Lan("低于设定值") + "[" +
                                                           CGlobalPara.SysPara.Alarm.PassRateLimit.ToString("P2") + "]," +
                                                           CLanguage.Lan("请检查确认."), udcRunLog.ELog.NG);
                            }
                        }
                        break;
                    case EWarnResult.确认报警:
                        _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 0);
                        _WarnRate.bAlarm = 0;
                        _WarnRate.DoRun = EWarnResult.正常;
                        _WarnRate.Watcher.Stop();
                        Log(_WarnRate.ToString() + CLanguage.Lan("人工确认解除良率报警") + "[" + _WarnRate.PassRate.ToString("P2") + "]", udcRunLog.ELog.Action);
                        break;
                    default:
                        break;
                }

                OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 保存INI
        /// </summary>
        private void SaveIniForPassRate()
        {
            CIniFile.WriteToIni("WarnRate", "TTNum", _WarnRate.TTNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "PassNum", _WarnRate.PassNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "CurrentDate", _WarnRate.CurrentDate, CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "CurrentDayClr", _WarnRate.CurrentDayClr.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "CurrentNightClr", _WarnRate.CurrentNightClr.ToString(), CGlobalPara.IniFile);
        }
        /// <summary>
        /// 获取INI
        /// </summary>
        private void LoadIniForPassRate()
        {
            _WarnRate.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "TTNum", CGlobalPara.IniFile, "0"));
            _WarnRate.PassNum = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "PassNum", CGlobalPara.IniFile, "0"));
            _WarnRate.CurrentDate = CIniFile.ReadFromIni("WarnRate", "CurrentDate", CGlobalPara.IniFile);
            _WarnRate.CurrentDayClr = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "CurrentDayClr", CGlobalPara.IniFile, "0"));
            _WarnRate.CurrentNightClr = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "CurrentNightClr", CGlobalPara.IniFile, "0"));
        }
        /// <summary>
        /// 设置预警线
        /// </summary>
        /// <param name="passRateLimit"></param>
        public void SetPassRateLimit(bool chkPassRate, double passRateLimit)
        {
            if (!chkPassRate)
                _WarnRate.DoRun = EWarnResult.未启动;
            else
                _WarnRate.DoRun = EWarnResult.空闲;
            _WarnRate.PassRateLimit = passRateLimit;
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));
        }
        /// <summary>
        /// 清除统计数量
        /// </summary>
        public void ClearPassRate()
        {
            _WarnRate.TTNum = 0;
            _WarnRate.PassNum = 0;
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));
            SaveIniForPassRate();
        }
        /// <summary>
        /// 清除统计报警
        /// </summary>
        public void ClearPassRateAlarm()
        {
            _WarnRate.bAlarm = -1;
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));
        }
        #endregion

        #region 保存温度数据
        /// <summary>
        /// 温度时间
        /// </summary>
        private Stopwatch Watcher_Temp = new Stopwatch();
        /// <summary>
        /// 保存老化库位温度
        /// </summary>
        private bool SaveTempReport()
        {
            try
            {

                if (CGlobalPara.SysPara.Report.TempPath == string.Empty)
                    return true;

                if (CGlobalPara.SysPara.Report.TempScanTime == 0)
                    return true;

                if (!Directory.Exists(CGlobalPara.SysPara.Report.TempPath))
                {
                    Directory.CreateDirectory(CGlobalPara.SysPara.Report.TempPath);
                }

                if (Watcher_Temp.IsRunning)
                {
                    if (Watcher_Temp.ElapsedMilliseconds < CGlobalPara.SysPara.Report.TempScanTime * 1000)
                        return true;
                }

                Watcher_Temp.Restart();

                string dayTempFile = CGlobalPara.SysPara.Report.TempPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "_T.csv";

                bool existFile = File.Exists(dayTempFile);

                StreamWriter sw = new StreamWriter(dayTempFile, true, Encoding.UTF8);

                string strWrite = string.Empty;

                //创建序号
                if (!existFile)
                {
                    strWrite = "MCGS_Time,Total_T,";

                    for (int i = 0; i < _chmr.PLC.rTempPoint.Length; i++)
                    {
                        strWrite += "TS" + (i + 1).ToString() + ",";
                    }

                    sw.WriteLine(strWrite);
                }

                strWrite = DateTime.Now.ToString("HH:mm:ss") + ",";

                strWrite += _chmr.PLC.rTemp.ToString("0.0") + ",";

                for (int i = 0; i < _chmr.PLC.rTempPoint.Length; i++)
                {
                    strWrite += _chmr.PLC.rTempPoint[i].ToString("0.0") + ",";
                }
              
                sw.WriteLine(strWrite);

                sw.Flush();

                sw.Close();

                sw = null;

                return true;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        #endregion

        #region 物联网Iot
        /// <summary>
        /// 初始化
        /// </summary>
        private void InitialIot()
        {
            try
            {
                if (CYOHOOApp.IoT_Enable != 1)
                    return;

                List<CDevList> devList = new List<CDevList>();

                devList.Add(new CDevList()
                {
                    idNo = CGlobalPara.DeviceIDNo,
                    Name = CGlobalPara.DeviceName
                });

                InitialIotDevice(CYOHOOApp.IoT_Server, CYOHOOApp.Iot_Factory, devList);

                Task.Factory.StartNew(() => StartIot());

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 更新设备状态
        /// </summary>
        private void UpdateIotDeviceStatus()
        {
            try
            {
                if (CYOHOOApp.IoT_Enable != 1)
                    return;

                string er = string.Empty;

                EDevRunStatus runStatus = EDevRunStatus.未运行;

                EAlarmLevel alarmLevel = EAlarmLevel.解除;

                string alarmCode = string.Empty;

                string alarmInfo = string.Empty;

                if (CGlobalPara.C_RUNNING)
                {
                    if (_threadPLC.rREG_Val(EPLCINP.PLC自动运行.ToString()) != CPLCPara.ON)
                    {
                        runStatus = EDevRunStatus.停止;
                    }
                    else
                    {
                        if (_threadPLC.rREG_Val(EPLCINP.PLC设备报警.ToString()) == CPLCPara.ON)
                        {
                            runStatus = EDevRunStatus.报警;
                            alarmLevel = EAlarmLevel.报警;
                        }
                        else
                        {
                            runStatus = EDevRunStatus.运行;
                        }
                        //获取报警信息
                        List<int> regVal = new List<int>();
                        for (int i = 0; i < CPLCAlarmList.C_BIT_MAX; i++)
                        {
                            int val = _threadPLC.rREG_Val(InpPLC(EPLCINP.PLC报警列表01, i));
                            if (val < 0)
                                val = 0;
                            regVal.Add(val);
                        }
                        List<CPLCAlarmReg> alarmReg = null;
                        if (_PLCAlarmList.GetCurAlarmInfo(regVal, out alarmReg, out er))
                        {
                            if (alarmReg.Count > 0)
                            {
                                alarmCode = alarmReg[0].idNo.ToString();
                                alarmInfo = alarmReg[0].RegFun;
                            }
                        }
                    }
                }

                //更新设备状态
                if (Iot_DevRunStatus.ContainsKey(CGlobalPara.DeviceIDNo))
                {
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].RunStatus = (int)runStatus;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].TTNum = _chmr.DayYield.ttNum;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].FailNum = _chmr.DayYield.failNum;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].AlarmLevel = (int)alarmLevel;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].AlarmCode = alarmCode;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].AlarmInfo = alarmInfo;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].Remark1 = string.Empty;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].Remark2 = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }       
        }
        /// <summary>
        /// 接收应答指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnIotREQCmd(object sender, CClient.CCmdArgs e)
        {
            FrmClient.Subscribe("【REQ COMMAND】:" + DateTime.Now.ToString("HH:mm:ss.") + DateTime.Now.Millisecond.ToString("D3") + "\r\n", Color.Black);

            FrmClient.Subscribe("【Topic】:", Color.Black);

            FrmClient.Subscribe(e.topic + "\r\n", Color.Blue);

            FrmClient.Subscribe("【Message】:", Color.Black);

            FrmClient.Subscribe(e.message + "\r\n", Color.Green);

            ECmdType cmdType = (ECmdType)e.data.Data[0].CmdType;

            try
            {
                if (cmdType != ECmdType.控制指令)
                    return;

                string er = string.Empty;

                for (int i = 0; i < e.data.Data.Count; i++)
                {
                    if (!Iot_DevRunStatus.ContainsKey(e.data.Data[i].ID))
                        continue;

                    EDevRunStatus runStatus = (EDevRunStatus)System.Convert.ToInt16(e.data.Data[0].CmdInfo);

                    bool ExcuteOK = true;

                    switch (runStatus)
                    {
                        case EDevRunStatus.运行:
                            if (!CGlobalPara.C_RUNNING)
                            {
                                if (!OnRun())
                                {
                                    ExcuteOK = false;
                                }
                            }
                            break;
                        case EDevRunStatus.停止:
                            if (CGlobalPara.C_RUNNING)
                            {
                                OnStop();
                            }
                            break;
                        case EDevRunStatus.停线:
                            break;
                        default:
                            return;
                    }

                    string info = (ExcuteOK ? "OK" : "NG");

                    if (!ReponseIotCommand(e.data.Data[0].CmdName, info, out er))
                    {
                        Log(CLanguage.Lan("<Iot命令指令>执行错误:") + er, udcRunLog.ELog.NG);
                    }
                    else
                    {
                        Log(CLanguage.Lan("<Iot命令指令>已执行完毕:") + info, udcRunLog.ELog.Action);
                    }
                }
            }
            catch (Exception ex)
            {
                FrmClient.Subscribe(ex.ToString() + "\r\n", Color.Red);
            }
        }
        #endregion

        #region 加载测试项目数据
        /// <summary>
        /// 加载测试项目
        /// </summary>
        /// <param name="uutNo"></param>
        private void LoadLedItem(int uutNo)
        {
            try
            {
                //初始化测试项目
                _runUUT[uutNo].Para.valJson = new CLED_UUT();

                _runUUT[uutNo].Para.valJson.Value = new List<CLED_ITEM>();

                for (int step = 0; step < _runModel.Para.OutPut_Num; step++)
                {
                    CLED_ITEM item = new CLED_ITEM()
                    {
                        Name = _runModel.OutPut[step].Describe,
                        VName = _runModel.OutPut[step].Chan[0].Vname,
                        Vmin = _runModel.OutPut[step].Chan[0].Vmin,
                        Vmax = _runModel.OutPut[step].Chan[0].Vmax,
                        QCM = _runModel.OutPut[step].Chan[0].QCType,
                        QCV = _runModel.OutPut[step].Chan[0].QCV,
                        ISet = _runModel.OutPut[step].Chan[0].ISet,
                        Imin = _runModel.OutPut[step].Chan[0].Imin,
                        Imax = _runModel.OutPut[step].Chan[0].Imax
                    };

                    _runUUT[uutNo].Para.valJson.Value.Add(item);
                }

                _runUUT[uutNo].Para.strJson = CJSon.Serializer<CLED_UUT>(_runUUT[uutNo].Para.valJson);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 初始测试数据
        /// </summary>
        /// <param name="uutNo"></param>
        private void LoadLedValue(int uutNo)
        {
            try
            {
                Random t = new Random();

                double middle = 0;

                for (int slotNo = 0; slotNo < _runUUT[uutNo].Led.Count; slotNo++)
                {
                    _runUUT[uutNo].Led[slotNo].valJson = new CLED_SLOT();

                    _runUUT[uutNo].Led[slotNo].valJson.Value = new List<CLED_VALUE>();

                    for (int step = 0; step < _runModel.Para.OutPut_Num; step++)
                    {
                        middle = (_runModel.OutPut[step].Chan[0].Vmin + _runModel.OutPut[step].Chan[0].Vmax) / 2 + 0.2 * t.NextDouble();

                        middle = System.Convert.ToDouble(middle.ToString("0.000"));

                        CLED_VALUE value = new CLED_VALUE()
                        {
                            UnitV = middle,
                            UnitA = _runModel.OutPut[step].Chan[0].ISet,
                            Result = 0,
                            FailEnd = 0,
                            FailTime = string.Empty
                        };

                        _runUUT[uutNo].Led[slotNo].valJson.Value.Add(value);
                    }

                    _runUUT[uutNo].Led[slotNo].strJson = CJSon.Serializer<CLED_SLOT>(_runUUT[uutNo].Led[slotNo].valJson);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 处理测试数据
        /// </summary>
        private bool CalTestValue(int uutNo, int slotNo, out string FailCode, out string FailTime)
        {
            FailCode = string.Empty;

            FailTime = string.Empty;

            try
            {
                //老化快充不良
                //老化ON/OFF不良
                //老化无输出
                //老化当机
                //老化输出异常

                //补偿检查数据
                if (_runUUT[uutNo].Para.strJson == string.Empty)
                {
                    LoadLedItem(uutNo);
                }
                if (_runUUT[uutNo].Led[slotNo].strJson == string.Empty)
                {
                    LoadLedValue(uutNo);

                    if (_runUUT[uutNo].Led[slotNo].failResult != 0)
                    {
                        int stepMax = _runUUT[uutNo].Led[slotNo].valJson.Value.Count - 1;
                        _runUUT[uutNo].Led[slotNo].valJson.Value[stepMax].Result = 1;
                        _runUUT[uutNo].Led[slotNo].valJson.Value[stepMax].FailEnd = 1;
                        _runUUT[uutNo].Led[slotNo].valJson.Value[stepMax].FailTime = _runUUT[uutNo].Led[slotNo].failTime;
                        _runUUT[uutNo].Led[slotNo].valJson.Value[stepMax].UnitV = _runUUT[uutNo].Led[slotNo].unitV;
                        _runUUT[uutNo].Led[slotNo].valJson.Value[stepMax].UnitA = _runUUT[uutNo].Led[slotNo].unitA;
                    }
                }

                int ItemMax = _runUUT[uutNo].Para.valJson.Value.Count;

                int ValueMax = _runUUT[uutNo].Led[slotNo].valJson.Value.Count;

                if (ValueMax == 0)
                {
                    Log(_runUUT[uutNo].ToString() + "[" + _runUUT[uutNo].Para.IdCard + "-" + slotNo.ToString() + "]" +
                                                        CLanguage.Lan("产品条码") + "[" + _runUUT[uutNo].Led[slotNo].serialNo + "]"+
                                                        CLanguage.Lan("数据异常")+ "1", udcRunLog.ELog.NG);
                    return false;
                }
                if (ValueMax != ItemMax)
                {
                    Log(_runUUT[uutNo].ToString() + "[" + _runUUT[uutNo].Para.IdCard + "-" + slotNo.ToString() + "]" +
                                                           CLanguage.Lan("产品条码") + "[" + _runUUT[uutNo].Led[slotNo].serialNo + "]" +
                                                           CLanguage.Lan("数据异常") + "2", udcRunLog.ELog.NG);
                    return false;
                }

                Random t = new Random();

                //1.PASS数据处理

                if (_runUUT[uutNo].Led[slotNo].failResult == 0)
                {
                    for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                    {
                        double Vmax = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax;
                        double Vmin = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin;
                        double Imax = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax;
                        double Imin = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin;
                        int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;
                        double unitV = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV;
                        double unitA = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA;
                        _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result = 0;
                        if (unitV < Vmin || unitV > Vmax)
                        {
                            double middle = ((Vmin + Vmax) / 2 - 0.1) + t.NextDouble() * 0.2;
                            _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV = middle;
                        }
                        if (unitA < Imin || unitA > Imax)
                        {
                            _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA = _runUUT[uutNo].Para.valJson.Value[stepNo].ISet;
                        }
                    }

                    return true;
                }

                //2.不良数据处理

                for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                {
                    double Vmax = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax;
                    double Vmin = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin;
                    double Imax = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax;
                    double Imin = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin;
                    int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;
                    double unitV = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV;
                    double unitA = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA;
                    if (result == 0)
                    {
                        if (unitV < Vmin || unitV > Vmax)
                        {
                            double middle = ((Vmin + Vmax) / 2 - 0.1) + t.NextDouble() * 0.2;

                            _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV = middle;
                        }
                        if (unitA < Imin || unitA > Imax)
                        {
                            _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA = _runUUT[uutNo].Para.valJson.Value[stepNo].ISet;
                        }
                    }
                    else
                    {
                        if ((unitV >= Vmin && unitV <= Vmax) && (unitA >= Imin && unitA <= Imax))
                        {
                            _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV = 0.1 * t.NextDouble();
                            _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA = 0.1 * t.NextDouble();
                        }

                        if (FailTime == string.Empty)
                        {
                            FailTime = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].FailTime;
                        }
                    }
                }

                //2-1.老化无输出

                int NoOutput = 1;

                for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                {
                    double Vmax = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax;
                    double Vmin = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin;
                    double Imax = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax;
                    double Imin = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin;
                    int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;
                    double unitV = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV;
                    double unitA = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA;
                    if (unitV > 3)
                    {
                        NoOutput = 0;
                    }
                    else
                    {
                        if (result != 0 && FailTime == string.Empty)
                        {
                            FailTime = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].FailTime;
                        }
                    }
                }

                if (NoOutput == 1)
                {
                    FailCode = "老化无输出";
                    return true;
                }

                //2-2.老化输出异常

                string OUT_Error = string.Empty;

                int OUT_Fail = 1;

                for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                {
                    double Vmax = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax;
                    double Vmin = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin;
                    double Imax = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax;
                    double Imin = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin;
                    int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;
                    double unitV = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV;
                    double unitA = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA;
                    if (result == 0 || unitV > 3)
                    {
                        OUT_Fail = 0;
                    }
                    else
                    {
                        if (result != 0 && FailTime == string.Empty)
                        {
                            if (unitV < Vmin)
                            {
                                OUT_Error = "电压偏下限";
                            }
                            if (unitV > Vmax)
                            {
                                OUT_Error = "电压偏上限";
                            }
                            if (unitA < Imin)
                            {
                                OUT_Error = "电流偏下限";
                            }
                            if (unitA > Imax)
                            {
                                OUT_Error = "电流偏上限";
                            }
                            FailTime = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].FailTime;
                        }
                    }
                }
                if (OUT_Fail == 1)
                {
                    FailCode = OUT_Error;
                    return true;
                }

                //2-3.老化快充不良                
                int QCV_Fail = 0;
                for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                {
                    double Vmax = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmax;
                    double Vmin = _runUUT[uutNo].Para.valJson.Value[stepNo].Vmin;
                    double Imax = _runUUT[uutNo].Para.valJson.Value[stepNo].Imax;
                    double Imin = _runUUT[uutNo].Para.valJson.Value[stepNo].Imin;
                    int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;
                    double unitV = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitV;
                    double unitA = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].UnitA;
                    if (result == 1 && _runUUT[uutNo].Para.valJson.Value[stepNo].QCM != (int)EQCM.Normal)
                    {
                        QCV_Fail = 1;
                    }
                    if (result != 0 && FailTime == string.Empty)
                    {
                        FailTime = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].FailTime;
                    }
                }
                if (QCV_Fail == 1)
                {
                    FailCode = "老化快充不良";
                    return true;
                }

                //2-4.老化当机

                int StepMax = _runUUT[uutNo].Led[slotNo].valJson.Value.Count;

                if (_runUUT[uutNo].Led[slotNo].valJson.Value[StepMax - 1].Result == 0)
                {
                    for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                    {
                        int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;

                        if (result != 0 && FailTime == string.Empty)
                        {
                            FailTime = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].FailTime;
                        }
                    }

                    FailCode = "老化当机";

                    return true;
                }

                //2-5.老化ON/OFF不良

                for (int stepNo = 0; stepNo < _runUUT[uutNo].Led[slotNo].valJson.Value.Count; stepNo++)
                {
                    int result = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].Result;

                    if (result != 0 && FailTime == string.Empty)
                    {
                        FailTime = _runUUT[uutNo].Led[slotNo].valJson.Value[stepNo].FailTime;
                    }
                }

                FailCode = "老化ON/OFF不良";

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 库位自检模式
        private bool bSelfPosEnd = false;
        /// <summary>
        /// 控制下一位置
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="state"></param>
        private void ControlSelfPos(int uutNo, string state)
        {
            try
            {
                //保存测试数据

                string er = string.Empty;

                string folder = Application.StartupPath + "\\Inspection";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = folder + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

                bool isExist = false;

                if (File.Exists(fileName))
                    isExist = true;

                string strWrite = string.Empty;

                StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                //写入标题栏
                if (!isExist)
                {
                    strWrite = "库位,时间,状态,结果,";

                    for (int i = 0; i < _runUUT[uutNo].Led.Count; i++)
                    {
                        strWrite += (idNo + 1).ToString() + "-" + "CH" + (i + 1).ToString("D2") + ",";
                    }

                    sw.WriteLine(strWrite);
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    strWrite = _runUUT[uutNo + idNo].ToString() + "," +
                                                            DateTime.Now.ToString("HH:mm:ss") + "," + state + ",";

                    if (state == "OK")
                    {
                        string strResult = "OK,";

                        string strData = string.Empty;

                        for (int i = 0; i < _runUUT[uutNo + idNo].Led.Count; i++)
                        {
                            if (_runUUT[uutNo + idNo].Led[i].failResult == 0)
                            {
                                strData += "PASS|";
                            }
                            else
                            {
                                strData += "FAIL|";
                                strResult = "NG,";
                            }
                            strData += _runUUT[uutNo + idNo].Led[i].unitV.ToString("0.000") + "V|";
                            strData += _runUUT[uutNo + idNo].Led[i].unitA.ToString("0.00") + "A,";
                        }

                        strWrite += strResult + strData;
                    }

                    sw.WriteLine(strWrite);
                }


                sw.Flush();

                sw.Close();

                sw = null;

                //移入下一个位置
                int InUUTNo = -1;

                for (int i = 0; i < _runUUT.Count / 2; i++)
                {
                    int k = i * 2;

                    if (k <= uutNo)
                        continue;

                    if (
                        _runUUT[k + 0].Para.DoRun == EDoRun.位置空闲 &&
                        _runUUT[k + 0].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[k + 0].Para.CtrlUUTONLine == 0 &
                        _runUUT[k + 1].Para.DoRun == EDoRun.位置空闲 &&
                        _runUUT[k + 1].Para.AlarmCode == EAlarmCode.正常 &&
                        _runUUT[k + 1].Para.CtrlUUTONLine == 0
                        )
                    {
                        InUUTNo = k;
                        break;
                    }
                }

                if (InUUTNo == -1)
                {
                    if (!bSelfPosEnd)
                    {
                        bSelfPosEnd = true;
                        MessageBox.Show(CLanguage.Lan("库位自检结束,请检查库位报表数据"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                //启动移位

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    InitialQCM(InUUTNo + idNo, 0);

                    //测试参数
                    _runUUT[InUUTNo + idNo].Para.IdCard = _runUUT[uutNo + idNo].Para.IdCard;
                    _runUUT[InUUTNo + idNo].Para.IsNull = _runUUT[uutNo + idNo].Para.IsNull;
                    _runUUT[InUUTNo + idNo].Para.MesFlag = _runUUT[uutNo + idNo].Para.MesFlag;
                    _runUUT[InUUTNo + idNo].Para.OrderName = _runUUT[uutNo + idNo].Para.OrderName;
                    _runUUT[InUUTNo + idNo].Para.ModelName = _runModel.Base.Model;
                    _runUUT[InUUTNo + idNo].Para.BurnTime = (int)(_runModel.Para.BITime * 3600);
                    _runUUT[InUUTNo + idNo].Para.RunTime = 0;
                    _runUUT[InUUTNo + idNo].Para.OutLevel = 0;
                    _runUUT[InUUTNo + idNo].Para.OutPutNum = _runModel.Para.OutPut_Num;
                    _runUUT[InUUTNo + idNo].Para.OnOffNum = _runModel.Para.OnOff_Num;

                    if (CPara.GetOutPutAndOnOffFromModel(_runModel, ref _runUUT[InUUTNo + idNo].OnOff, out er))
                    {
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurStepNo = 0;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurRunVolt = _runModel.OnOff[0].Item[0].ACV;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurRunOutPut = _runModel.OnOff[0].Item[0].OutPutType;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurQCType = _runModel.OutPut[_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut].Chan[0].QCType;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurQCV = _runModel.OutPut[_runUUT[uutNo + idNo].OnOff.TimeRun.CurRunOutPut].Chan[0].QCV;
                        _runUUT[InUUTNo + idNo].OnOff.TimeRun.CurQCM = EQCMChage.空闲;
                    }

                    int outPutNo = _runUUT[InUUTNo + idNo].OnOff.OnOff[0].OutPutType;

                    //输出
                    for (int slot = 0; slot < _runUUT[InUUTNo + idNo].Led.Count; slot++)
                    {
                        _runUUT[InUUTNo + idNo].Led[slot].serialNo = _runUUT[uutNo + idNo].Led[slot].serialNo;
                        _runUUT[InUUTNo + idNo].Led[slot].vName = _runModel.OutPut[outPutNo].Chan[0].Vname;
                        _runUUT[InUUTNo + idNo].Led[slot].vMin = _runModel.OutPut[outPutNo].Chan[0].Vmin;
                        _runUUT[InUUTNo + idNo].Led[slot].vMax = _runModel.OutPut[outPutNo].Chan[0].Vmax;
                        _runUUT[InUUTNo + idNo].Led[slot].IMode = _runModel.OutPut[outPutNo].Chan[0].Imode;
                        _runUUT[InUUTNo + idNo].Led[slot].ISet = _runModel.OutPut[outPutNo].Chan[0].ISet;
                        _runUUT[InUUTNo + idNo].Led[slot].Imin = _runModel.OutPut[outPutNo].Chan[0].Imin;
                        _runUUT[InUUTNo + idNo].Led[slot].Imax = _runModel.OutPut[outPutNo].Chan[0].Imax;
                        _runUUT[InUUTNo + idNo].Led[slot].qcv = _runModel.OutPut[outPutNo].Chan[0].QCV;
                        _runUUT[InUUTNo + idNo].Led[slot].unitV = _runUUT[uutNo].Led[slot].qcv;
                        _runUUT[InUUTNo + idNo].Led[slot].unitA = _runUUT[uutNo].Led[slot].ISet;
                        _runUUT[InUUTNo + idNo].Led[slot].passResult = 0;
                        _runUUT[InUUTNo + idNo].Led[slot].failResult = 0;
                        _runUUT[InUUTNo + idNo].Led[slot].failEnd = 0;
                        _runUUT[InUUTNo + idNo].Led[slot].failTime = "";
                        _runUUT[InUUTNo + idNo].Led[slot].failInfo = "";
                    }


                    _runUUT[InUUTNo + idNo].Para.DoRun = EDoRun.正在进机;
                    _runUUT[InUUTNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[InUUTNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    SetTimer(InUUTNo + idNo, CLanguage.Lan("正在进机"));

                    _runUUT[uutNo + idNo].Para.DoRun = EDoRun.正在出机;
                    _runUUT[uutNo + idNo].Para.AlarmInfo = string.Empty;
                    _runUUT[uutNo + idNo].Para.AlarmCode = EAlarmCode.正常;
                    SetTimer(uutNo + idNo, CLanguage.Lan("正在出机"));
                }

                for (int idNo = 0; idNo < 2; idNo++)
                {
                    RefreshSlotUI(uutNo + idNo);

                    local_db_update_fix_status(uutNo + idNo);

                    RefreshSlotUI(InUUTNo + idNo);

                    local_db_update_in_bi(InUUTNo + idNo);

                    Log(CLanguage.Lan("<库位自检模式>") + _runUUT[uutNo + idNo].ToString() + CLanguage.Lan("指定进机") +
                                           _runUUT[InUUTNo + idNo].ToString(), udcRunLog.ELog.NG);
                }

                ControlPLC_PosToPos(uutNo, InUUTNo, out er);

                bSelfPosEnd = false;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

    }
}
