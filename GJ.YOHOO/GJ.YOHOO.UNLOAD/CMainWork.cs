using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using GJ.USER.APP.MainWork;
using GJ.UI;
using GJ.COM;
using GJ.PDB;
using GJ.DEV.PLC;
using GJ.DEV.CARD;
using GJ.DEV.COM;
using GJ.USER.APP;
using GJ.MES;
using GJ.USER.APP.Iot;
using GJ.Iot;
using GJ.SFCS;

namespace GJ.YOHOO.UNLOAD
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

                if (!load_user_plc_reg(CLanguage.Lan("自动下机位"),out er))
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
                CGlobalPara.DeviceIDNo = CIniFile.ReadFromIni(CGlobalPara.StationName, "GuidSn", CGlobalPara.IniFile);

                _defaultModel = CIniFile.ReadFromIni("UnLoad", "defaulModel", CGlobalPara.IniFile);

                _idCardBak = CIniFile.ReadFromIni("UnLoad", "idCard", CGlobalPara.IniFile);

                LoadYield();

                LoadIniForDaily();

                for (int idNo = 0; idNo < _WarnNum; idNo++)
                {
                    LoadIniForPassRate(idNo);
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
                
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void LoadMainFormUI()
        {
            OnFrmMainSystemUpdate();
        }
        public override void LoadUIComplete()
        {
            MainWorker = new BackgroundWorker();
            MainWorker.WorkerSupportsCancellation = true;
            MainWorker.WorkerReportsProgress = true;
            MainWorker.DoWork += new DoWorkEventHandler(MainWorker_DoWork);

            OnUIGlobalArgs.OnEvented(new CUIGlobalArgs());

            for (int idNo = 0; idNo < _WarnNum; idNo++)
            {
                OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo[idNo], _WarnName[idNo], _WarnRate[idNo]));
            }

            InitialIot();
        }
        #endregion

        #region 实现抽象方法
        public override bool InitialRunPara()
        {
            for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
            {
                _statHipot[idNo].hub.DoRun = ERUN.空闲;

                _statHipot[idNo].test.DoRun = ERUN.空闲;
            }

            for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
            {
                _statATE[idNo].hub.DoRun = ERUN.空闲;

                _statATE[idNo].test.DoRun = ERUN.空闲;
            }

            _statFinal.DoRun = ERUN.空闲;

            _changeModel = false;

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

                IniTask.Add(Task.Factory.StartNew(() => OpenTCPServer()));

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

            CloseTCPServer();
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

                //启动主线程
                if (!MainWorker.IsBusy)
                    MainWorker.RunWorkerAsync();

                UISystemArgs.DoRun = EUISystem.启动;

                OnUISystemArgs.OnEvented(new CUIUserArgs<CUISystemArgs>(idNo, name, UISystemArgs));

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

                while (MainWorker.IsBusy)
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

                UISystemArgs.DoRun = EUISystem.空闲;

                OnUISystemArgs.OnEvented(new CUIUserArgs<CUISystemArgs>(idNo, name, UISystemArgs));

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

        #region 定义UI消息
        /// <summary>
        /// 系统UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUISystemArgs>> OnUISystemArgs = new COnEvent<CUIUserArgs<CUISystemArgs>>();
        /// <summary>
        /// 信息消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUIMainArgs>> OnUIMainArgs = new COnEvent<CUIUserArgs<CUIMainArgs>>();
        /// <summary>
        /// 良品预警
        /// </summary>
        public COnEvent<CUIUserArgs<CWarnRate>> OnUIPassRateArgs = new COnEvent<CUIUserArgs<CWarnRate>>();
        #endregion

        #region UI消息类定义
        /// <summary>
        /// 系统UI
        /// </summary>
        private CUISystemArgs UISystemArgs = new CUISystemArgs();
        /// <summary>
        /// 界面UI
        /// </summary>
        private CUIMainArgs UIMainArgs = new CUIMainArgs();
        #endregion

        #region 加载用户参数
        /// <summary>
        /// 加载PLC读写寄存器
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool load_user_plc_reg(string name, out string er)
        {
            er = string.Empty;

            try
            {
                CUSER_PLCREG plcList = new CUSER_PLCREG();

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.PLCDB);

                DataSet ds = null;

                List<string> rRegList = new List<string>();

                List<string> wRegList = new List<string>();

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
                    rRegList.Add(reg.regDes);
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
                    wRegList.Add(reg.regDes);
                }
                _userPLCReg = plcList;

                //读取PLC报警列表
                _PLCAlarmList = new CPLCAlarmList(idNo, name);
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
                    int regLevel = System.Convert.ToInt32(ds.Tables[0].Rows[i]["regLevel"].ToString());
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

                //检测寄存器数据正常

                foreach (string regName in Enum.GetNames(typeof(EPLCINP)))
                {
                    if (!rRegList.Contains(regName))
                    {
                        er = CLanguage.Lan("功能寄存器") + "[" + regName + "]"+ CLanguage.Lan("未在PLC列表定义");
                        return false;
                    }
                }

                foreach (string regName in Enum.GetNames(typeof(EPLCOUT)))
                {
                    if (!wRegList.Contains(regName))
                    {
                        er = CLanguage.Lan("功能寄存器") + "[" + regName + "]" + CLanguage.Lan("未在PLC列表定义");
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
        /// 加载用户测试参数
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool load_user_info(out string er)
        {
            er = string.Empty;

            try
            {                
                local_db_CreateTable();

                //测试工位定义

                _statHipot = new CStat[CGlobalPara.C_HP_STAT_MAX];

                for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
                {
                    _statHipot[idNo] = new CStat(idNo, "<"+ CLanguage.Lan("高压检查工位")  + (idNo + 1).ToString() + ">",
                                                       "<"+ CLanguage.Lan("高压测试位") + (idNo + 1).ToString() + ">",
                                                       CYOHOOApp.HIPOT_FlowId, CYOHOOApp.HIPOT_FlowName, CYOHOOApp.SlotMax);                                                    
                                                     
                    _statHipot[idNo].hub.IdCardAddr = 1 + idNo;

                    UIMainArgs.StatHP[idNo] = _statHipot[idNo].test;
                }

                _statATE = new CStat[CGlobalPara.C_ATE_STAT_MAX];

                for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
                {
                    _statATE[idNo] = new CStat(CGlobalPara.C_HP_STAT_MAX + idNo, "<"+ CLanguage.Lan("ATE检查工位") + (idNo + 1).ToString() + ">",
                                                                                 "<" + CLanguage.Lan("ATE测试位") + (idNo + 1).ToString() + ">",
                                                                              CYOHOOApp.ATE_FlowId, CYOHOOApp.ATE_FlowName,CYOHOOApp.SlotMax);                                                                             
                    _statATE[idNo].hub.IdCardAddr = 1 + idNo + CGlobalPara.C_HP_STAT_MAX;

                    UIMainArgs.StatATE[idNo] = _statATE[idNo].test;
                }

                _statFinal = new CStatHub(CGlobalPara.C_HP_STAT_MAX + CGlobalPara.C_ATE_STAT_MAX, 
                                             "<"+ CLanguage.Lan("人工/自动下机位") + ">",
                                             CYOHOOApp.UNLOAD_FlowId, CYOHOOApp.UNLOAD_FlowName, CYOHOOApp.SlotMax);                                      
                                             
                _statFinal.IdCardAddr = 1 + CGlobalPara.C_HP_STAT_MAX + CGlobalPara.C_ATE_STAT_MAX;

                UIMainArgs.StatUnLoad = _statFinal;

                //TCP服务器定义

                _serReponse = new CSerSocket(CYOHOOApp.SlotMax);

                List<string> serName = new List<string>();

                List<int> serUUT = new List<int>();

                serName.Add(CGlobalPara.SysPara.Dev.HP_TCP);

                serUUT.Add(CYOHOOApp.SlotMax);

                if (CGlobalPara.SysPara.Dev.AteDevMax == 0)
                {
                    serName.Add(CGlobalPara.SysPara.Dev.ATE_TCP);
                    serUUT.Add(CYOHOOApp.SlotMax);
                }
                else
                {
                    serName.Add(CGlobalPara.SysPara.Dev.ATE_TCP + "1");
                    serUUT.Add(CYOHOOApp.SlotMax / 2);

                    serName.Add(CGlobalPara.SysPara.Dev.ATE_TCP + "2");
                    serUUT.Add(CYOHOOApp.SlotMax / 2);
                }

                _serReponse.LoadStat(serName, serUUT);

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        #endregion

        #region PLC状态定义
        /// <summary>
        /// PLC输入
        /// </summary>
        private enum EPLCINP
        {
            PLC自动运行,
            PLC设备报警,
            PLC设备警告,
            Hipot1轨道治具到位,
            Hipot2轨道治具到位,
            Hipot1测试准备OK,
            Hipot2测试准备OK,
            Hipot1测试治具有无,
            Hipot2测试治具有无,
            ATE1轨道治具到位,
            ATE2轨道治具到位,
            ATE1测试准备OK,
            ATE2测试准备OK,
            ATE1测试治具有无,
            ATE2测试治具有无,
            自动下机位治具到位,
            自动下机位治具到位光电,
            手动下机位治具到位信号,
            手动下机位治具到位光电,
            人工下机模式, //1代表人工下机;2代表自动下机

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
        /// PLC输出
        /// </summary>
        private enum EPLCOUT
        {
            上位机软件启动,
            上位机软件报警,
            Hipot1轨道治具结果,
            Hipot2轨道治具结果,
            ATE1轨道治具结果,
            ATE2轨道治具结果,
            Hipot1测试结果,
            Hipot2测试结果,
            ATE1测试结果,
            ATE2测试结果,

            手动下机位治具结果,
            自动下机位结果,

            下机位产品1结果,
            下机位产品2结果,
            下机位产品3结果,
            下机位产品4结果,
            下机位产品5结果,
            下机位产品6结果,
            下机位产品7结果,
            下机位产品8结果,
            下机位产品9结果,
            下机位产品10结果,
            下机位产品11结果,
            下机位产品12结果,
            下机位产品13结果,
            下机位产品14结果,
            下机位产品15结果,
            下机位产品16结果
        }
        /// <summary>
        /// 返回PLC输入名称
        /// </summary>
        /// <param name="inpIo"></param>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private string InpPLC(EPLCINP inpIo, int idNo)
        {
            return ((EPLCINP)((int)inpIo + idNo)).ToString();
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
        /// 用户PLC寄存器
        /// </summary>
        private CUSER_PLCREG _userPLCReg = null;
        #endregion

        #region 字段
        /// <summary>
        /// 软件启动标志
        /// </summary>
        private Stopwatch Softwatcher = new Stopwatch();
        /// <summary>
        /// 默认机种名
        /// </summary>
        private string _defaultModel = string.Empty;
        /// <summary>
        /// 当前机种名
        /// </summary>
        private string _curModel = string.Empty;
        /// <summary>
        /// 更换机种
        /// </summary>
        private bool _changeModel = false;
        /// <summary>
        /// PLC设备
        /// </summary>
        private CPLCCOM _devPLC = null;
        /// <summary>
        /// 读卡器设备
        /// </summary>
        private CCARDCom _devIDCard = null;
        /// <summary>
        /// TCP服务器
        /// </summary>
        private CServerTCP _devSerTCP = null;
        /// <summary>
        /// PLC线程
        /// </summary>
        private CPLCThread _threadPLC = null;
        /// <summary>
        /// PLC重连接次数
        /// </summary>
        private int _conToPLCAgain = 0;
        /// <summary>
        /// TCP应答类
        /// </summary>
        private CSerSocket _serReponse = null;
        /// <summary>
        /// 高压测试工位
        /// </summary>
        private CStat[] _statHipot = null;
        /// <summary>
        /// ATE测试工位
        /// </summary>
        private CStat[] _statATE = null;
        /// <summary>
        /// 人工自动下机位
        /// </summary>
        private CStatHub _statFinal = null;
        /// <summary>
        /// 下机治具ID备份
        /// </summary>
        private string _idCardBak = string.Empty;
        /// <summary>
        /// 工位列表
        /// </summary>
        private Dictionary<string, CYield> _statYield = null;
        #endregion

        #region 面板方法
        public void OnFrmMainChangeModel()
        {
            _defaultModel = _curModel;

            CIniFile.WriteToIni("UnLoad", "defaulModel", _defaultModel, CGlobalPara.IniFile);

            _changeModel = false;
        }
        public void OnFrmMainSystemUpdate()
        {
            for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
            {
                _statHipot[idNo].hub.Disable = CGlobalPara.SysPara.Para.ChkNoHP;
                _statHipot[idNo].hub.ForceIn = CGlobalPara.SysPara.Para.ChkInHP;
                _statHipot[idNo].test.ChkFail = CGlobalPara.SysPara.Para.ChkHPFail;
                _statHipot[idNo].test.Disable = CGlobalPara.SysPara.Para.ChkNoHP;
            }

            for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
            {
                _statATE[idNo].hub.Disable = CGlobalPara.SysPara.Para.ChkNoATE;
                _statATE[idNo].hub.ForceIn = CGlobalPara.SysPara.Para.ChkInATE;
                _statATE[idNo].test.ChkFail = CGlobalPara.SysPara.Para.ChkATEFail;
                _statATE[idNo].test.Disable = CGlobalPara.SysPara.Para.ChkNoATE;
            }
        }
        public void OnFrmMainFailConfig(string name)
        {
            if (name == "HIPOT")
            {
                for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
                {
                    _statHipot[idNo].test.DisFail = false;
                }
            }
            else
            {
                for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
                {
                    _statATE[idNo].test.DisFail = false;
                }
            }          
        }
        public void OnFrmMainClrStationYield()
        {
            for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
            {
                _statHipot[idNo].test.TTNum = 0;

                _statHipot[idNo].test.FailNum = 0;

                CIniFile.WriteToIni("Parameter", _statHipot[idNo].test.FlowName + (idNo + 1).ToString() + "_TTNum",
                                                 _statHipot[idNo].test.TTNum.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("Parameter", _statHipot[idNo].test.FlowName + (idNo + 1).ToString() + "_FailNum",
                                                 _statHipot[idNo].test.FailNum.ToString(), CGlobalPara.IniFile);


                UIMainArgs.TTNum = _statHipot[idNo].test.TTNum;

                UIMainArgs.FailNum = _statHipot[idNo].test.FailNum;

                UIMainArgs.DoRun = EUIStatus.产能计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_statHipot[idNo].test.idNo, "", UIMainArgs));
            }

            for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
            {
                _statATE[idNo].test.TTNum = 0;

                _statATE[idNo].test.FailNum = 0;

                CIniFile.WriteToIni("Parameter", _statATE[idNo].test.FlowName + (idNo + 1).ToString() + "_TTNum",
                                                 _statATE[idNo].test.TTNum.ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("Parameter", _statATE[idNo].test.FlowName + (idNo + 1).ToString() + "_FailNum",
                                                 _statATE[idNo].test.FailNum.ToString(), CGlobalPara.IniFile);

                UIMainArgs.TTNum = _statATE[idNo].test.TTNum;

                UIMainArgs.FailNum = _statATE[idNo].test.FailNum;

                UIMainArgs.DoRun = EUIStatus.产能计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_statATE[idNo].test.idNo, "", UIMainArgs));
            }       
        }
        public void OnFrmMianClrFlowYield()
        {
            List<CYield> yields = new List<CYield>();

            foreach (string keyName in _statYield.Keys)
            {
                _statYield[keyName].TTNum = 0;
                _statYield[keyName].FailNum = 0;
                yields.Add(new CYield()
                {
                    TTNum = 0,
                    FailNum = 0
                });

                UIMainArgs.YieldKey = keyName;

                UIMainArgs.Yield = _statYield[keyName];

                UIMainArgs.DoRun = EUIStatus.显示计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));
            }

            UIMainArgs.Yields = yields;

            UIMainArgs.DoRun = EUIStatus.工位计数;

            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));

            SaveStationFlowYield();
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
                    _devPLC = new CPLCCOM(EPlcType.Inovance_TCP, 0, "<"+ CLanguage.Lan("下机位PLC") + ">");

                    if (!_devPLC.Open(CGlobalPara.SysPara.Dev.PlcIp, out er, "502"))
                    {
                        Log(_devPLC.ToString() + "[" + CGlobalPara.SysPara.Dev.PlcIp + "]" + CLanguage.Lan("连接通信错误:") + er, udcRunLog.ELog.NG);

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
                    Log(_devPLC.ToString() + "[" + CGlobalPara.SysPara.Dev.PlcIp + "]" + CLanguage.Lan("断开连接"), udcRunLog.ELog.Action);

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
                    _devIDCard = new CCARDCom(ECardType.MFID, 0, CLanguage.Lan("读卡器"));

                    if (!_devIDCard.Open(CGlobalPara.SysPara.Dev.IdCom, out er))
                    {
                        Log(_devIDCard.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.IdCom + "]"+
                                                    CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

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
                                Log(_devIDCard.ToString() + CLanguage.Lan("读取地址") +"[" + (1 + i).ToString("D2") + "]"+
                                                            CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

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
        /// 打开TCP服务端
        /// </summary>
        /// <returns></returns>
        private bool OpenTCPServer()
        {
            try
            {
                if (_devSerTCP == null)
                {
                    _devSerTCP = new CServerTCP(0, CLanguage.Lan("TCP服务端"));
                    _devSerTCP.OnConed += new CServerTCP.EventOnConHander(OnTcpStatus);
                    _devSerTCP.OnRecved += new CServerTCP.EventOnRecvHandler(OnTcpRecv);
                    _devSerTCP.Listen(CGlobalPara.SysPara.Dev.TcpPort);
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
        /// 断开服务端监听
        /// </summary>
        private void CloseTCPServer()
        {
            try
            {
                if (_devSerTCP != null)
                {
                    _devSerTCP.OnConed -= new CServerTCP.EventOnConHander(OnTcpStatus);
                    _devSerTCP.OnRecved -= new CServerTCP.EventOnRecvHandler(OnTcpRecv);
                    _devSerTCP.close();
                    _devSerTCP = null;
                    Log(CLanguage.Lan("停止测试TCP服务器监听:端口") + "[" + CGlobalPara.SysPara.Dev.TcpPort.ToString() + "]", udcRunLog.ELog.Action);
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
                    Log(CLanguage.Lan("连接冠佳WEB") + "【" + CYOHOOApp.UlrWeb + "】"+ CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);
                    return false;
                }

                Log(CLanguage.Lan("连接冠佳WEB") + "【" + CYOHOOApp.UlrWeb + "】"+ CLanguage.Lan("正常"), udcRunLog.ELog.Action);

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
                    Log(CLanguage.Lan("PLC监控线程重新启动"), udcRunLog.ELog.Action);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 主线程
        /// <summary>
        /// 主线程
        /// </summary>
        private BackgroundWorker MainWorker = null;
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

                        if (!CheckSystem(delayMs))
                            continue;

                        if (!CheckPLCAlarm())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckHipotHubReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckHipotTestReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckATEHubReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckATETestReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckFinalReady())
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
        #endregion

        #region 线程方法
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

                if (!_threadPLC.complete)
                {
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

                    return false;
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
        /// 高压治具到位
        /// </summary>
        /// <returns></returns>
        private bool CheckHipotHubReady()
        {
            try
            {
                for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
                {
                    if (_threadPLC.rREG_Val(InpPLC(EPLCINP.Hipot1轨道治具到位, idNo)) != CPLCPara.ON)
                    {
                        _statHipot[idNo].hub.DoRun = ERUN.空闲;
                        continue;
                    }

                    if (_statHipot[idNo].hub.DoRun != ERUN.空闲)
                        continue;

                    _statHipot[idNo].hub.DoRun = ERUN.就绪;

                    Task.Factory.StartNew(() => OnHipotHubTask(idNo));

                    break;
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
        /// ATE治具到位
        /// </summary>
        /// <returns></returns>
        private bool CheckATEHubReady()
        {
            try
            {
                for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
                {
                    if (_threadPLC.rREG_Val(InpPLC(EPLCINP.ATE1轨道治具到位, idNo)) != CPLCPara.ON)
                    {
                        _statATE[idNo].hub.DoRun = ERUN.空闲;
                        continue;
                    }

                    if (_statATE[idNo].hub.DoRun != ERUN.空闲)
                        continue;

                    _statATE[idNo].hub.DoRun = ERUN.就绪;

                    Task.Factory.StartNew(() => OnATEHubTask(idNo));

                    break;
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
        /// 高压测试到位
        /// </summary>
        /// <returns></returns>
        private bool CheckHipotTestReady()
        {
            try
            {
                if (_changeModel)
                    return true;

                int index = -1;

                int readyNum = 0;

                for (int idNo = 0; idNo < CGlobalPara.C_HP_STAT_MAX; idNo++)
                {
                    if (_threadPLC.rREG_Val(InpPLC(EPLCINP.Hipot1测试准备OK, idNo)) == CPLCPara.ON)
                    {
                        index = idNo;

                        readyNum++;
                    }
                    else
                    {
                        _statHipot[idNo].test.DoRun = ERUN.空闲;
                    }
                }

                if (CGlobalPara.C_HP_STAT_MAX>1 && readyNum == CGlobalPara.C_HP_STAT_MAX)
                {
                    _statHipot[idNo].test.bAlarm = true;

                    _statHipot[idNo].test.Info = CLanguage.Lan("测试准备信号同时存在");

                    _statHipot[idNo].test.UIDoRun = EUIStatus.异常报警;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.HIPOT_FlowName, UIMainArgs));

                    Log("<"+ CLanguage.Lan("高压测试工位") + ">" + CLanguage.Lan("治具测试准备信号同时存在,请检查"), udcRunLog.ELog.NG);

                    return false;
                }

                if (index == -1)
                {
                    return true;
                }

                if (_statHipot[index].test.DoRun != ERUN.空闲)
                    return true;

                _statHipot[index].test.DoRun = ERUN.就绪;

                Task.Factory.StartNew(() => OnHipotTestTask(index));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// ATE测试到位
        /// </summary>
        /// <returns></returns>
        private bool CheckATETestReady()
        {
            try
            {
                int index = -1;

                int readyNum = 0;

                for (int idNo = 0; idNo < CGlobalPara.C_ATE_STAT_MAX; idNo++)
                {
                    if (_threadPLC.rREG_Val(InpPLC(EPLCINP.ATE1测试准备OK, idNo)) == CPLCPara.ON)
                    {
                        index = idNo;

                        readyNum++;
                    }
                    else
                    {
                        _statATE[idNo].test.DoRun = ERUN.空闲;
                    }
                }

                if (CGlobalPara.C_ATE_STAT_MAX >1 && readyNum == CGlobalPara.C_ATE_STAT_MAX)
                {
                    _statATE[idNo].test.bAlarm = true;

                    _statATE[idNo].test.Info = CLanguage.Lan("测试准备信号同时存在");

                    _statATE[idNo].test.UIDoRun = EUIStatus.异常报警;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.ATE_FlowName, UIMainArgs));
    
                    Log("<"+ CLanguage.Lan("ATE测试工位") + ">" + CLanguage.Lan("治具测试准备信号同时存在,请检查"), udcRunLog.ELog.NG);

                    return false;
                }

                if (index == -1)
                    return true;

                if (_statATE[index].test.DoRun != ERUN.空闲)
                    return true;

                _statATE[index].test.DoRun = ERUN.就绪;

                Task.Factory.StartNew(() => OnATETestTask(index));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 人工自动下机
        /// </summary>
        /// <returns></returns>
        private bool CheckFinalReady()
        {
            try
            {
                EPLCMode handMode = (EPLCMode)_threadPLC.rREG_Val(EPLCINP.人工下机模式.ToString());

                if (handMode == EPLCMode.自动下机) //自动下机模式
                {
                    if (_threadPLC.rREG_Val(EPLCINP.自动下机位治具到位.ToString()) != 1)
                    {
                        _statFinal.DoRun = ERUN.空闲;
                        
                        if (_threadPLC.rREG_Val(EPLCINP.自动下机位治具到位光电.ToString()) == 0)
                        {
                            _statFinal.bAlarm = false;

                            _statFinal.Info = string.Empty;

                            _statFinal.UIDoRun = EUIStatus.空闲;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));
                        }                           
                        return true;
                    }
                }
                else               //人工下机模式
                {
                    if (_threadPLC.rREG_Val(EPLCINP.手动下机位治具到位信号.ToString()) != 1)
                    {
                        _statFinal.DoRun = ERUN.空闲;
                        
                        if (_threadPLC.rREG_Val(EPLCINP.手动下机位治具到位光电.ToString()) == 0)
                        {
                            _statFinal.bAlarm = false;

                            _statFinal.Info = string.Empty;

                            _statFinal.UIDoRun = EUIStatus.空闲;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));
                        }
                        
                        return true;
                    }
                }

                if (_statFinal.DoRun != ERUN.空闲)
                    return true;

                _statFinal.bAlarm = false;

                _statFinal.Info = string.Empty;

                _statFinal.UIDoRun = EUIStatus.空闲;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));

                _statFinal.DoRun = ERUN.就绪;

                Task.Factory.StartNew(() => OnAutoFinalTask(handMode));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 高压位任务
        /// <summary>
        /// 高压治具检查到位
        /// </summary>
        /// <param name="idNo"></param>
        private void OnHipotHubTask(int idNo)
        {
            try
            {
                _statHipot[idNo].hub._cts = new CancellationTokenSource();

                _statHipot[idNo].hub.Watcher.Restart();

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statHipot[idNo].hub._cts.IsCancellationRequested)
                        return;

                    switch (_statHipot[idNo].hub.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.就绪:
                            OnHubFixtureReady(idNo, _statHipot[idNo].hub);
                            break;
                        case ERUN.报警:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.Hipot1轨道治具结果, idNo), (int)EPLCRESULT.结果NG);
                            _statHipot[idNo].hub.DoRun = ERUN.出站;
                            break;
                        case ERUN.结束:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.Hipot1轨道治具结果, idNo), (int)EPLCRESULT.结果OK);
                            _statHipot[idNo].hub.DoRun = ERUN.出站;
                            break;
                        case ERUN.过站:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.Hipot1轨道治具结果, idNo), (int)EPLCRESULT.过站);
                            _statHipot[idNo].hub.DoRun = ERUN.出站;
                            break;
                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                _statHipot[idNo].hub.Watcher.Stop();

                Log(_statHipot[idNo].hub.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statHipot[idNo].hub.Watcher.ElapsedMilliseconds.ToString() + "ms", 
                    udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 高压测试位到位任务
        /// </summary>
        /// <param name="idNo"></param>
        private void OnHipotTestTask(int idNo)
        {
            try
            {
                _statHipot[idNo].test._cts = new CancellationTokenSource();

                _statHipot[idNo].test.Watcher.Restart();

                _statHipot[idNo].test.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statHipot[idNo].test._cts.IsCancellationRequested)
                        return;

                    switch (_statHipot[idNo].test.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.就绪:
                            if (CGlobalPara.SysPara.Para.ChkGoPass)
                            {
                                Log(_statHipot[idNo].test.ToString() + CLanguage.Lan("设置为【调试过站模式】,等待3秒过站"), udcRunLog.ELog.NG);
                                Thread.Sleep(3000); 
                                _statHipot[idNo].test.DoRun = ERUN.过站;
                                break;
                            }
                            OnStatTestReady(idNo, _statHipot[idNo].test);
                            break;
                        case ERUN.启动:
                            OnStatTestStart(idNo, _statHipot[idNo].test);
                            break;
                        case ERUN.等待:
                            OnStatTestWait(idNo, _statHipot[idNo].test);
                            break;
                        case ERUN.测试:
                            OnStatTestRun(idNo, _statHipot[idNo].test);
                            break;
                        case ERUN.结束:
                            OnStatTestEnd(idNo, _statHipot[idNo].test);
                            break;
                        case ERUN.过站:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.Hipot1测试结果, idNo), (int)EPLCRESULT.结果OK);
                            _statHipot[idNo].test.DoRun = ERUN.出站;
                            return;
                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                _statHipot[idNo].test.Watcher.Stop();

                _statHipot[idNo].test.Io_Watcher.Stop();

                Log(_statHipot[idNo].test.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statHipot[idNo].test.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
            }
        }
        #endregion

        #region ATE位任务
        /// <summary>
        /// ATE治具检查到位
        /// </summary>
        /// <param name="idNo"></param>
        private void OnATEHubTask(int idNo)
        {
            try
            {
                _statATE[idNo].hub._cts = new CancellationTokenSource();

                _statATE[idNo].hub.Watcher.Restart();

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statATE[idNo].hub._cts.IsCancellationRequested)
                        return;

                    switch (_statATE[idNo].hub.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.就绪:
                            OnHubFixtureReady(idNo, _statATE[idNo].hub);
                            break;
                        case ERUN.报警:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.ATE1轨道治具结果, idNo), (int)EPLCRESULT.结果NG);
                            _statATE[idNo].hub.DoRun = ERUN.出站;
                            break;
                        case ERUN.结束:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.ATE1轨道治具结果, idNo), (int)EPLCRESULT.结果OK);
                            _statATE[idNo].hub.DoRun = ERUN.出站;
                            break;
                        case ERUN.过站:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.ATE1轨道治具结果, idNo), (int)EPLCRESULT.过站);
                            _statATE[idNo].hub.DoRun = ERUN.出站;
                            break;
                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                _statATE[idNo].hub.Watcher.Stop();
                Log(_statATE[idNo].hub.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statATE[idNo].hub.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// ATE测试位到位
        /// </summary>
        /// <param name="idNo"></param>
        private void OnATETestTask(int idNo)
        {
            try
            {
                _statATE[idNo].test._cts = new CancellationTokenSource();

                _statATE[idNo].test.Watcher.Restart();

                _statATE[idNo].test.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statATE[idNo].test._cts.IsCancellationRequested)
                        return;

                    switch (_statATE[idNo].test.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.就绪:
                            if (CGlobalPara.SysPara.Para.ChkGoPass)
                            {
                                Log(_statATE[idNo].test.ToString() + CLanguage.Lan("设置为【调试过站模式】,等待3秒过站"), udcRunLog.ELog.NG);
                                Thread.Sleep(3000);
                                _statATE[idNo].test.DoRun = ERUN.过站;
                                break;
                            }
                            OnStatTestReady(idNo, _statATE[idNo].test);
                            break;
                        case ERUN.启动:
                            OnStatTestStart(idNo, _statATE[idNo].test);
                            break;
                        case ERUN.等待:
                            OnStatTestWait(idNo, _statATE[idNo].test);
                            break;
                        case ERUN.测试:
                            OnStatTestRun(idNo, _statATE[idNo].test);
                            break;
                        case ERUN.结束:
                            OnStatTestEnd(idNo, _statATE[idNo].test);
                            break;
                        case ERUN.过站:
                            _threadPLC.addREGWrite(OutPLC(EPLCOUT.ATE1测试结果, idNo), (int)EPLCRESULT.结果OK);
                            _statATE[idNo].test.DoRun = ERUN.出站;
                            return;
                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                _statATE[idNo].test.Watcher.Stop();

                _statATE[idNo].test.Io_Watcher.Stop();

                Log(_statATE[idNo].test.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statATE[idNo].test.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
            }
        }
        #endregion

        #region 人工/自动下机任务
        /// <summary>
        /// 人工自动下机任务
        /// </summary>
        private void OnAutoFinalTask(EPLCMode handMode)
        {
            try
            {
                _statFinal._cts = new CancellationTokenSource();

                _statFinal.Watcher.Restart();

                _statFinal.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statFinal._cts.IsCancellationRequested)
                        return;

                    switch (_statFinal.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.就绪:
                            OnFinalFixtureReady(handMode);
                            break;
                        case ERUN.结束:
                            writeResultToPLC(handMode);
                            _statFinal.DoRun = ERUN.过站;
                            break;
                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                _statFinal.Watcher.Stop();

                Log(_statFinal.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statFinal.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
            }
        }
        #endregion

        #region 检查工位方法
        /// <summary>
        /// 检查工位治具到位
        /// </summary>
        /// <param name="hub"></param>
        private void OnHubFixtureReady(int idNo, CStatHub hub)
        {
            try
            {
                string er = string.Empty;

                string rIdCard = string.Empty;

                if (!_devIDCard.ReadIdCard(hub.IdCardAddr, out rIdCard, out er, CGlobalPara.SysPara.Para.IdReTimes))
                {
                    Log(hub.ToString() + CLanguage.Lan("读取治具ID失败") + ":" + er, udcRunLog.ELog.NG);

                    hub.DoRun = ERUN.报警;
                    
                    return;
                }

                hub.IdCard = rIdCard;

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("到位,检查治具流程."), udcRunLog.ELog.Action);

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = null;

                if (!CWeb2.GetFixtureInfoByIdCard(hub.IdCard, out fixture, out er))
                {
                    hub.DoRun = ERUN.过站;
                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("获取信息错误:") + er, udcRunLog.ELog.NG);
                    return;
                }

                hub.ModelName = fixture.Base.Model;
                hub.OrderName = fixture.Base.OrderName;
                hub.IsNull = (int)fixture.Base.FixtureType;
                hub.MesFlag = fixture.Base.MesFlag;
                for (int slotNo = 0; slotNo < fixture.Para.Count; slotNo++)
                {
                    hub.SerialNo[slotNo] = fixture.Para[slotNo].SerialNo;
                    hub.ResultName[slotNo] = fixture.Para[slotNo].FlowName;
                    hub.ResultId[slotNo] = fixture.Para[slotNo].FlowId;
                    hub.Result[slotNo] = fixture.Para[slotNo].Result;
                }

                if (hub.IsNull == 1)
                {
                    hub.DoRun = ERUN.过站;
                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置为空治具,准备过站") + ":" + 
                                          watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);
                    return;
                }

                bool HaveUUT = false;

                //禁用测试工位->强制写入测试结果
                if (hub.Disable)
                {
                    for (int i = 0; i < hub.SerialNo.Count; i++)
                    {
                        if (hub.Result[i] != 0 || hub.ResultId[i]!=hub.FlowId-1)
                            hub.SerialNo[i] = string.Empty;

                        if (hub.SerialNo[i] != string.Empty)
                            HaveUUT = true;

                        hub.Result[i] = 0;
                    }

                    if (HaveUUT)
                    {
                        hub.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        hub.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        if (!UpdateFixtureResult(hub))
                        {
                            hub.DoRun = ERUN.报警;
                            Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】" + CLanguage.Lan("强制写入测试结果错误:") + er, udcRunLog.ELog.NG);
                            return;
                        }
                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置不测试,强制写入结果,准备过站:") +
                                             watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
                    }
                    else
                    {
                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置不测试,强制准备过站:") +
                                             watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
                    }

                    hub.DoRun = ERUN.过站;
                    
                    return;
                }

                if (hub.ForceIn)  //强制进入测试工位
                {
                    for (int i = 0; i < hub.SerialNo.Count; i++)
                    {
                        if (hub.SerialNo[i] != string.Empty)
                            HaveUUT = true;
                    }
                }
                else
                {
                    for (int i = 0; i < hub.SerialNo.Count; i++)
                    {
                        if (hub.SerialNo[i] == string.Empty)
                            continue;

                        if (hub.Result[i] != 0 || hub.ResultId[i] != (hub.FlowId - 1))
                        {
                            hub.SerialNo[i] = string.Empty;
                            continue;
                        }

                        //检查条码测试工位
                        if (CGlobalPara.SysPara.Mes.Connect && CGlobalPara.SysPara.Mes.ChkStat)
                        {
                            if (CheckSn(hub.FlowName, hub.SerialNo[i], out er))
                            {
                                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "-" + (i + 1).ToString("D2") + "】" + CLanguage.Lan("检查条码") +
                                                    "【" + hub.SerialNo[i] + "】" + CLanguage.Lan("当前站别为【" + hub.FlowName + "】检查OK"), udcRunLog.ELog.OK);

                            }
                            else
                            {
                                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "-" + (i + 1).ToString("D2") + "】" + CLanguage.Lan("检查条码") +
                                                  "【" + hub.SerialNo[i] + "】当前站别为【" + hub.FlowName + "】检查FAIL", udcRunLog.ELog.NG);
                                hub.SerialNo[i] = string.Empty;
                            }
                        }

                        if (hub.SerialNo[i] != string.Empty)
                            HaveUUT = true;
                    }
                }

                if (!HaveUUT)
                {
                    hub.DoRun = ERUN.过站;
                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("无可测产品,准备过站:") +
                                          watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
                    return;
                }

                //提示是否变更机种?
                if (CGlobalPara.SysPara.Para.ChkModel && hub.FlowName == CYOHOOApp.HIPOT_FlowName)
                {
                    _curModel = hub.ModelName;
                    if (_defaultModel == string.Empty)
                    {
                        _defaultModel = _curModel;
                        CIniFile.WriteToIni("UnLoad", "defaulModel", _defaultModel, CGlobalPara.IniFile);
                    }

                    if (_defaultModel != _curModel)
                    {
                        string info = CLanguage.Lan("提示:请确认机种") + "[" + _defaultModel + "]"+ CLanguage.Lan("变更为机种") + "[" + _curModel + "]";
                        UIMainArgs.Info = info;
                        UIMainArgs.DoRun = EUIStatus.变更机种;
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));
                        _changeModel = true;
                    }
                }

                ////记录治具信息到本地数据库HIPOT站
                if (CGlobalPara.SysPara.Dev.IdRecordMode == 0)
                {
                    if (!SaveStatFixToDB(idNo, hub, out er))
                    {
                        hub.DoRun = ERUN.报警;
                        Log(hub.ToString() + CLanguage.Lan("保存治具信息失败:") + er, udcRunLog.ELog.NG);
                        return;
                    }
                }
                else
                {
                    if (!SaveStatFixToIni(idNo, hub, out er))
                    {
                        hub.DoRun = ERUN.报警;
                        Log(hub.ToString() + CLanguage.Lan("保存治具信息失败:") + er, udcRunLog.ELog.NG);
                        return;
                    }
                }

                hub.DoRun = ERUN.结束;

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("检查OK,准备顶升测试:") +
                                    watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 测试工位方法
        /// <summary>
        /// 测试治具到位
        /// </summary>
        /// <param name="hub"></param>
        private void OnStatTestReady(int idNo, CStatTest hub)
        {
            try
            {
                string er = string.Empty;

                //获取治具信息
                if (CGlobalPara.SysPara.Dev.IdRecordMode == 0)
                {
                    if (!GetStatFixFromDB(idNo, hub, out er))
                    {
                        hub.DoRun = ERUN.报警;

                        hub.bAlarm = true;

                        hub.Info = CLanguage.Lan("获取治具ID信息失败");

                        hub.UIDoRun = EUIStatus.异常报警; 

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        Log(hub.ToString() + CLanguage.Lan("获取治具ID信息失败") + ":" + er, udcRunLog.ELog.NG);

                        return;
                    }
                }
                else
                {
                    if (!GetStatFixFromIni(idNo, hub, out er))
                    {
                        hub.DoRun = ERUN.报警;

                        hub.bAlarm = true;

                        hub.Info = CLanguage.Lan("获取治具ID信息失败");

                        hub.UIDoRun = EUIStatus.异常报警;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        Log(hub.ToString() +  CLanguage.Lan("获取治具ID信息失败") + ":" + er, udcRunLog.ELog.NG);

                        return;
                    }
                }

                hub.bAlarm = false;

                hub.Info = CLanguage.Lan("治具到位就绪,等待测试");

                hub.UIDoRun = EUIStatus.治具到位;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                if (hub.Disable)
                {
                    for (int i = 0; i < hub.SerialNo.Count; i++)
                    {
                        hub.Result[i] = 0;
                    }

                    hub.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    hub.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");                   
                    if (!UpdateFixtureResult(hub))
                    {
                        hub.DoRun = ERUN.报警;
                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】" + CLanguage.Lan("强制写入测试结果错误:") + er, udcRunLog.ELog.NG);                                 
                        return;
                    }
                    hub.DoRun = ERUN.过站;
                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置不测试,强制写入结果,准备过站."), udcRunLog.ELog.Action);
                    return;
                }

                bool HaveUUT = false;

                for (int i = 0; i < hub.SerialNo.Count; i++)
                {
                    if (hub.SerialNo[i] != string.Empty)
                        HaveUUT = true;
                }

                if (!HaveUUT)
                {
                    hub.DoRun = ERUN.报警;

                    hub.bAlarm = true;

                    hub.Info = CLanguage.Lan("无可测试产品");

                    hub.UIDoRun = EUIStatus.异常报警; 

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("无可测试产品,准备过站"), udcRunLog.ELog.NG);

                    return;
                }

                hub.Watcher.Restart();

                hub.DoRun = ERUN.启动;

                hub.bAlarm = false;

                hub.Info = CLanguage.Lan("治具就绪,准备开始测试");

                hub.UIDoRun = EUIStatus.状态信息; 

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("治具就绪,准备开始测试"), udcRunLog.ELog.OK);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设置启动状态
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="hub"></param>
        /// <param name="uiStat"></param>
        private void OnStatTestStart(int idNo, CStatTest hub)
        {
            try
            {
                string er = string.Empty;

                if (hub.FlowName == CYOHOOApp.HIPOT_FlowName) //高压位启动测试                
                {
                    CSerSocket.CSAT serTCP = new CSerSocket.CSAT(CYOHOOApp.SlotMax);
                    serTCP.StatName = CGlobalPara.SysPara.Dev.HP_TCP;
                    serTCP.SubNo = idNo + 1;
                    serTCP.ModelName = hub.ModelName;
                    serTCP.MesFlag = hub.MesFlag;
                    serTCP.IdCard = hub.IdCard;
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                    {
                        serTCP.SerialNos[i] = hub.SerialNo[i];
                    }
                    if (!_serReponse.Ready(serTCP, out er))
                    {
                        hub.DoRun = ERUN.报警;

                        hub.bAlarm = true;

                        hub.Info = CLanguage.Lan("设置启动状态错误");

                        hub.UIDoRun = EUIStatus.异常报警;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置启动状态错误") + ":" + er, udcRunLog.ELog.NG);

                        return;
                    }
                }
                else if (hub.FlowName == CYOHOOApp.ATE_FlowName) //ATE启动测试
                {
                    if (CGlobalPara.SysPara.Dev.AteDevMax == 0)
                    {
                        CSerSocket.CSAT serTCP = new CSerSocket.CSAT(CYOHOOApp.SlotMax);
                        serTCP.StatName = CGlobalPara.SysPara.Dev.ATE_TCP;
                        serTCP.SubNo = idNo + 1;
                        serTCP.ModelName = hub.ModelName;
                        serTCP.MesFlag = hub.MesFlag;
                        serTCP.IdCard = hub.IdCard;
                        for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                            serTCP.SerialNos[i] = hub.SerialNo[i];
                        if (!_serReponse.Ready(serTCP, out er))
                        {
                            hub.DoRun = ERUN.报警;

                            hub.bAlarm = true;

                            hub.Info = CLanguage.Lan("设置启动状态错误");

                            hub.UIDoRun = EUIStatus.异常报警;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));                            
                            
                            Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置启动状态错误") + ":" + er, udcRunLog.ELog.NG);                            
                            
                            return;
                        }
                    }
                    else
                    {
                        for (int index = 0; index < 2; index++)
                        {
                            int slotMax = CYOHOOApp.SlotMax / 2;
                            CSerSocket.CSAT serTCP = new CSerSocket.CSAT(slotMax);
                            serTCP.StatName = CGlobalPara.SysPara.Dev.ATE_TCP + (index + 1).ToString();
                            serTCP.SubNo = idNo + 1;
                            serTCP.ModelName = hub.ModelName;
                            serTCP.MesFlag = hub.MesFlag;
                            serTCP.IdCard = hub.IdCard;
                            for (int i = 0; i < slotMax; i++)
                                serTCP.SerialNos[i] = hub.SerialNo[i + index * slotMax];
                            if (!_serReponse.Ready(serTCP, out er))
                            {
                                hub.DoRun = ERUN.报警;

                                hub.bAlarm = true;

                                hub.Info = CLanguage.Lan("设置启动状态错误");

                                hub.UIDoRun = EUIStatus.异常报警;

                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));  
                              
                                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("设置启动状态错误") + ":" + er, udcRunLog.ELog.NG);    
                            
                                return;
                            }
                        }
                    }
                }

                hub.Io_Watcher.Restart();

                hub.DoRun = ERUN.等待;

                hub.bAlarm = false;

                hub.Info = CLanguage.Lan("启动测试状态,等待测试");

                hub.UIDoRun = EUIStatus.状态信息;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("启动测试状态,等待测试"), udcRunLog.ELog.OK);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 等待启动测试
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="hub"></param>
        /// <param name="uiStat"></param>
        private void OnStatTestWait(int idNo, CStatTest hub)
        {
            try
            {
                string er = string.Empty;

                if (hub.FlowName == CYOHOOApp.HIPOT_FlowName) //高压位启动测试                
                {
                    CSerSocket.ERUN status = _serReponse.ReadRunStatus(CGlobalPara.SysPara.Dev.HP_TCP);

                    if (status == CSerSocket.ERUN.测试中 || status == CSerSocket.ERUN.测试结束)
                    {
                        hub.Io_Watcher.Restart();

                        hub.DoRun = ERUN.测试;

                        string info = CLanguage.Lan("仪器设备开始测试") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                        hub.bAlarm = false;

                        hub.Info = info;

                        hub.UIDoRun = EUIStatus.状态信息;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        hub.SideIndex = -1;

                        hub.UIDoRun = EUIStatus.测试中;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("仪器设备开始测试"), udcRunLog.ELog.OK);

                    }
                    else
                    {
                        string info = CLanguage.Lan("启动测试状态,等待测试") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                        hub.bAlarm = false;

                        hub.Info = info;

                        hub.UIDoRun = EUIStatus.状态信息;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                    }
                }
                else if (hub.FlowName == CYOHOOApp.ATE_FlowName) //ATE启动测试
                {
                    if (CGlobalPara.SysPara.Dev.AteDevMax == 0)
                    {
                        CSerSocket.ERUN status = _serReponse.ReadRunStatus(CGlobalPara.SysPara.Dev.ATE_TCP);

                        if (status == CSerSocket.ERUN.测试中 || status == CSerSocket.ERUN.测试结束)
                        {
                            hub.Io_Watcher.Restart();

                            hub.DoRun = ERUN.测试;

                            string info = CLanguage.Lan("仪器设备开始测试") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                            hub.bAlarm = false;

                            hub.Info = info;

                            hub.UIDoRun = EUIStatus.状态信息;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                            hub.SideIndex = -1;

                            hub.UIDoRun = EUIStatus.测试中;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                            Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("仪器设备开始测试") + ":" +
                                                         hub.Io_Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);
                        }
                        else
                        {
                            string info = CLanguage.Lan("启动测试状态,等待测试") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                            hub.bAlarm = false;

                            hub.Info = info;

                            hub.UIDoRun = EUIStatus.状态信息;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));
                        }
                    }
                    else
                    {
                        bool startEnd = false;

                        for (int index = 0; index < 2; index++)
                        {
                            CSerSocket.ERUN status = _serReponse.ReadRunStatus(hub.FlowName + (index + 1).ToString());

                            if (status == CSerSocket.ERUN.测试中 || status == CSerSocket.ERUN.测试结束)
                            {
                                hub.Io_Watcher.Restart();

                                hub.DoRun = ERUN.测试;

                                string info = CLanguage.Lan("仪器设备开始测试") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                                hub.bAlarm = false;

                                hub.Info = info;

                                hub.UIDoRun = EUIStatus.状态信息;

                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                                hub.SideIndex = index;

                                hub.UIDoRun = EUIStatus.测试中;

                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("仪器设备开始测试") + ":" +
                                                          ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s", udcRunLog.ELog.OK);

                                startEnd = true;
                            }
                        }

                        if (!startEnd)
                        {
                            string info = CLanguage.Lan("启动测试状态,等待测试") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                            hub.bAlarm = false;

                            hub.Info = info;

                            hub.UIDoRun = EUIStatus.状态信息;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设备测试中
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="hub"></param>
        /// <param name="uiStat"></param>
        private void OnStatTestRun(int idNo, CStatTest hub)
        {
            try
            {
                string er = string.Empty;

                if (hub.FlowName == CYOHOOApp.HIPOT_FlowName) //高压位启动测试                
                {
                    CSerSocket.ERUN status = _serReponse.ReadRunStatus(CGlobalPara.SysPara.Dev.HP_TCP);

                    if (status == CSerSocket.ERUN.测试结束)
                    {
                        if (!_serReponse.ReadResult(CGlobalPara.SysPara.Dev.HP_TCP, ref hub.Result, out er))
                        {
                            hub.DoRun = ERUN.报警;

                            hub.bAlarm = true;

                            hub.Info = CLanguage.Lan("获取设备结果错误");

                            hub.UIDoRun = EUIStatus.异常报警;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo,hub.FlowName,UIMainArgs));

                            Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("获取设备结果错误") + ":" + er, udcRunLog.ELog.NG);

                            return;
                        }

                        hub.DoRun = ERUN.结束;

                        string info = CLanguage.Lan("仪器设备测试结束") + ":" + ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                        hub.bAlarm = false;

                        hub.Info = info;

                        hub.UIDoRun = EUIStatus.状态信息;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        hub.SideIndex = -1;

                        hub.UIDoRun = EUIStatus.测试结束;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("仪器设备测试结束") + ":" +
                                                    ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s", udcRunLog.ELog.OK);

                    }
                    else
                    {
                        string info = CLanguage.Lan("仪器设备测试中") + ":" + ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                        hub.bAlarm = false;

                        hub.Info = info;

                        hub.UIDoRun = EUIStatus.状态信息;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));
                    }
                }
                else if (hub.FlowName == CYOHOOApp.ATE_FlowName) //ATE启动测试
                {
                    //单仪器

                    if (CGlobalPara.SysPara.Dev.AteDevMax == 0)
                    {
                        CSerSocket.ERUN status = _serReponse.ReadRunStatus(CGlobalPara.SysPara.Dev.ATE_TCP);

                        if (status == CSerSocket.ERUN.测试结束)
                        {
                            if (!_serReponse.ReadResult(CGlobalPara.SysPara.Dev.ATE_TCP, ref hub.Result, out er))
                            {
                                hub.DoRun = ERUN.报警;

                                hub.bAlarm = true;

                                hub.Info = CLanguage.Lan("获取设备结果错误");

                                hub.UIDoRun = EUIStatus.异常报警;

                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("获取设备结果错误") + ":" + er, udcRunLog.ELog.NG);

                                return;
                            }

                            hub.DoRun = ERUN.结束;

                            string info = CLanguage.Lan("仪器设备测试结束") + ":" + ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                            hub.bAlarm = false;

                            hub.Info = info;

                            hub.UIDoRun = EUIStatus.状态信息;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                            hub.SideIndex = -1;

                            hub.UIDoRun = EUIStatus.测试结束;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                            Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("仪器设备测试结束") + ":" +
                                                        ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s", udcRunLog.ELog.OK);

                        }
                        else
                        {
                            string info = CLanguage.Lan("仪器设备测试中") + ":" + ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                            hub.bAlarm = false;

                            hub.Info = info;

                            hub.UIDoRun = EUIStatus.状态信息;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));
                        }

                        return;
                    }

                    //双仪器测试

                    int runEndNum = 0;

                    int slotMax = CYOHOOApp.SlotMax / 2;

                    for (int index = 0; index < 2; index++)
                    {
                        CSerSocket.ERUN status = _serReponse.ReadRunStatus(CGlobalPara.SysPara.Dev.ATE_TCP + (index + 1).ToString());

                        if (status == CSerSocket.ERUN.测试中)
                        {
                            hub.SideIndex = index;

                            hub.UIDoRun = EUIStatus.测试中;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));
                        }
                        else if (status == CSerSocket.ERUN.测试结束)
                        {
                            runEndNum++;

                            List<int> results = new List<int>();

                            for (int i = 0; i < slotMax; i++)
                            {
                                results.Add(0);
                            }

                            if (!_serReponse.ReadResult(CGlobalPara.SysPara.Dev.ATE_TCP + (index + 1).ToString(), ref results, out er))
                            {
                                hub.DoRun = ERUN.报警;

                                hub.bAlarm = true;

                                hub.Info = CLanguage.Lan("获取设备结果错误");

                                hub.UIDoRun = EUIStatus.异常报警;

                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("获取设备结果错误") + ":" + er, udcRunLog.ELog.NG);

                                return;
                            }

                            for (int i = 0; i < slotMax; i++)
                            {
                                hub.Result[i + index * slotMax] = results[i];
                            }

                            hub.SideIndex = index;
                       
                            hub.UIDoRun = EUIStatus.测试结束;

                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));
                        }
                    }

                    if (runEndNum == 2)
                    {
                        hub.DoRun = ERUN.结束;

                        string info = CLanguage.Lan("仪器设备测试结束") + ":" + ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                        hub.bAlarm = false;

                        hub.Info = info;

                        hub.UIDoRun = EUIStatus.状态信息;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));


                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("仪器设备测试结束") + ":" +
                                                    ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s", udcRunLog.ELog.OK);
                    }
                    else
                    {
                        string info = CLanguage.Lan("仪器设备测试中") + ":" + ((double)hub.Io_Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                        hub.bAlarm = false;

                        hub.Info = info;

                        hub.UIDoRun = EUIStatus.状态信息;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设备测试结束
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="hub"></param>
        /// <param name="uiStat"></param>
        private void OnStatTestEnd(int idNo, CStatTest hub)
        {
            try
            {
                string er = string.Empty;

                bool uutPass = true;

                int ttNum = 0;

                int passNum = 0;

                for (int i = 0; i < hub.SerialNo.Count; i++)
                {
                    if (hub.SerialNo[i] != string.Empty)
                    {
                        ttNum++;
                        hub.TTNum++;
                        if (hub.Result[i] != 0)
                        {
                            hub.FailNum++;
                            uutPass = false;
                        }
                        else
                        {
                            passNum++;
                        }
                    }
                }

                //高压不良弹框提示解除
                if (hub.FlowName == "HIPOT" && CGlobalPara.SysPara.Para.ChkLockFail)
                {
                    _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 1);  

                    FrmHipotFail.CreateInstance(CGlobalPara.SysPara.Para.LockHPPwr).Show();

                    while (true)
                    {
                        if (hub._cts.IsCancellationRequested)
                            return;

                        if (!FrmHipotFail.IsAvalible)
                            break;

                        Application.DoEvents();
                    }
                }
                else //确定测试不良
                {
                    if (!uutPass && hub.ChkFail)
                    {
                        hub.DisFail = true;

                        _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 1);

                        hub.bAlarm = true;

                        hub.Info = CLanguage.Lan("等待人工确认不良");

                        hub.UIDoRun = EUIStatus.异常报警;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                        Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("等待人工确认不良"), udcRunLog.ELog.NG);

                        while (hub.DisFail)
                        {
                            if (hub._cts.IsCancellationRequested)
                                return;
                            Application.DoEvents();
                        }
                    }
                }

                if (hub.FlowName == CYOHOOApp.HIPOT_FlowName)
                {
                    _WarnRate[0].TTNum += ttNum;
                    _WarnRate[0].PassNum += passNum;
                }
                else if (hub.FlowName == CYOHOOApp.ATE_FlowName)
                {
                    _WarnRate[1].TTNum += ttNum;
                    _WarnRate[1].PassNum += passNum;

                    //上传测试结果到GJWeb
                    if (!CGlobalPara.SysPara.Para.ChkATEToGJWeb)
                    {
                        UpdateFixtureResult(hub);
                    }
                }

                UIMainArgs.TTNum = hub.TTNum;

                UIMainArgs.TTNum = hub.FailNum;

                UIMainArgs.DoRun = EUIStatus.产能计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(hub.idNo, "", UIMainArgs));

                SaveStatYield(idNo, hub);

                hub.Io_Watcher.Stop();

                hub.Watcher.Stop();

                hub.DoRun = ERUN.过站;

                if (uutPass)
                {
                    string info = CLanguage.Lan("测试结束:PASS,准备过站") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                    hub.bAlarm = false;

                    hub.Info = info;

                    hub.UIDoRun = EUIStatus.状态信息;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                    Log(hub.ToString() + CLanguage.Lan("测试结束:PASS,准备过站") + ":" +
                                          ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s", udcRunLog.ELog.OK);
                }
                else
                {
                    string info = CLanguage.Lan("测试结束:FAIL,准备过站") + ":" + ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                    hub.bAlarm = true;

                    hub.Info = info;

                    hub.UIDoRun = EUIStatus.状态信息;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, hub.FlowName, UIMainArgs));

                    Log(hub.ToString() + CLanguage.Lan("测试结束:FAIL,准备过站") + ":" +
                                        ((double)hub.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s", udcRunLog.ELog.NG);
                }

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 人工/自动下机方法
        /// <summary>
        /// 下机位治具到位
        /// </summary>
        private void OnFinalFixtureReady(EPLCMode handMode)
        {
            try
            {
                string er = string.Empty;

                int idCardAddr = 0;

                string rIdCard = string.Empty;

                if (handMode == EPLCMode.人工下机)
                {
                    idCardAddr = _statFinal.IdCardAddr;

                    if (!_devIDCard.ReadIdCard(idCardAddr, out rIdCard, out er, CGlobalPara.SysPara.Para.IdReTimes))
                    {
                        _threadPLC.addREGWrite(EPLCOUT.手动下机位治具结果.ToString(), (int)EPLCRESULT.结果NG);

                        _statFinal.DoRun = ERUN.报警;

                        _statFinal.bAlarm = true;

                        _statFinal.Info = CLanguage.Lan("读取治具ID卡错误");

                        _statFinal.UIDoRun = EUIStatus.读卡报警;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));

                        Log(_statFinal.ToString() + CLanguage.Lan("读取治具ID卡错误") + ":" + er, udcRunLog.ELog.NG);

                        return;
                    }
                }
                else
                {
                    idCardAddr = _statFinal.IdCardAddr + 1;

                    if (!_devIDCard.ReadIdCard(idCardAddr, out rIdCard, out er, CGlobalPara.SysPara.Para.IdReTimes))
                    {
                        _threadPLC.addREGWrite(EPLCOUT.自动下机位结果.ToString(), (int)EPLCRESULT.结果NG);

                        _statFinal.DoRun = ERUN.报警;

                        _statFinal.bAlarm = true;

                        _statFinal.Info = CLanguage.Lan("读取治具ID卡错误");

                        _statFinal.UIDoRun = EUIStatus.读卡报警;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));

                        Log(_statFinal.ToString() + CLanguage.Lan("读取治具ID卡错误") + ":" + er, udcRunLog.ELog.NG);

                        return;
                    }
                }

                _statFinal.IdCard = rIdCard;

                Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("到位,检查治具流程."), udcRunLog.ELog.Action);

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = null;

                if (!CWeb2.GetFixtureInfoByIdCard(_statFinal.IdCard, out fixture, out er))
                {
                    if (handMode == EPLCMode.自动下机)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.自动下机位结果.ToString(), (int)EPLCRESULT.结果NG);
                    }
                    else
                    {
                        _threadPLC.addREGWrite(EPLCOUT.手动下机位治具结果.ToString(), (int)EPLCRESULT.结果NG);
                    }

                    _statFinal.DoRun = ERUN.报警;

                    _statFinal.bAlarm = true;

                    _statFinal.Info = CLanguage.Lan("获取信息错误");

                    _statFinal.UIDoRun = EUIStatus.异常报警;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _statFinal.FlowName, UIMainArgs));

                    Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("获取信息错误") + ":" + er, udcRunLog.ELog.NG);

                    return;
                }

                watcher.Stop();
               
                bool uutPass = true;

                _statFinal.ModelName = fixture.Base.Model;

                _statFinal.IsNull = (int)fixture.Base.FixtureType;

                _statFinal.MesFlag = fixture.Base.MesFlag;

                for (int i = 0; i < fixture.Para.Count; i++)
                {
                    _statFinal.SerialNo[i] = fixture.Para[i].SerialNo;

                    _statFinal.ResultName[i] = fixture.Para[i].FlowName;

                    _statFinal.ResultId[i] = fixture.Para[i].FlowId;

                    _statFinal.Result[i] = fixture.Para[i].Result;

                    _statFinal.TranOK[i] = false;

                    if (_statFinal.SerialNo[i] != string.Empty)
                    {
                        if (_statFinal.Result[i] != 0 || _statFinal.ResultId[i] != CYOHOOApp.UNLOAD_FlowId - 1)
                        {
                            uutPass = false;
                        }
                    }
                }

                if (_statFinal.IsNull == 1)
                {
                    if (handMode == EPLCMode.自动下机)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.自动下机位结果.ToString(), (int)EPLCRESULT.过站);
                    }
                    else
                    {
                        _threadPLC.addREGWrite(EPLCOUT.手动下机位治具结果.ToString(), (int)EPLCRESULT.结果OK);
                    }

                    _statFinal.DoRun = ERUN.过站;

                    _statFinal.bAlarm = false;

                    _statFinal.Info = "";

                    _statFinal.UIDoRun = EUIStatus.空治具过站;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));

                    Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("设置为空治具,准备过站."), udcRunLog.ELog.OK);

                    return;
                }

                bool UUTPass = false;

                if (!CGlobalPara.SysPara.Mes.Connect || _statFinal.MesFlag==0)
                {
                    for (int i = 0; i < _statFinal.SerialNo.Count; i++)
                    {
                        if (_statFinal.SerialNo[i] != string.Empty)
                        {
                            if (_statFinal.Result[i] == 0 && _statFinal.ResultId[i] == CYOHOOApp.UNLOAD_FlowId - 1)
                                UUTPass = true;
                        }
                    }
                }
                else
                {
                    List<int> resultList = null;

                    if (!GetSnResult(out resultList, out er))
                    {
                        if (handMode == EPLCMode.自动下机)
                            _threadPLC.addREGWrite(EPLCOUT.自动下机位结果.ToString(), (int)EPLCRESULT.结果NG);
                        else
                            _threadPLC.addREGWrite(EPLCOUT.手动下机位治具结果.ToString(), (int)EPLCRESULT.结果NG);
                        _statFinal.DoRun = ERUN.报警;
                        _statFinal.bAlarm = true;
                        _statFinal.Info = CLanguage.Lan("获取信息错误");
                        _statFinal.UIDoRun = EUIStatus.异常报警;
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _statFinal.FlowName, UIMainArgs));
                        Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("获取信息错误") + ":" + er, udcRunLog.ELog.NG);
                        return;
                    }
                    for (int i = 0; i < _statFinal.SerialNo.Count; i++)
                    {
                        if (_statFinal.SerialNo[i] != string.Empty)
                        {
                            if (_statFinal.Result[i] == 0 && _statFinal.ResultId[i] == CYOHOOApp.UNLOAD_FlowId - 1)
                            {
                                if (resultList[i] == _statFinal.Result[i])
                                {
                                    UUTPass = true;
                                }
                                else
                                {
                                    _statFinal.Result[i] = 1;

                                    Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "-" + (i + 1).ToString("D2") + "】"+
                                                                CLanguage.Lan("条码") + "【" + _statFinal.SerialNo[i] + "】"+ CLanguage.Lan("MES检查FAIL")+
                                                                "【" + resultList[i].ToString() + "】" +                                                        
                                                                 CLanguage.Lan("与测试信息PASS不一致,以MES结果为准."), udcRunLog.ELog.NG);
                                }
                            }
                            else
                            {
                                if (resultList[i] == 0)
                                {
                                    UUTPass = true;

                                    _statFinal.Result[i] = 0;

                                    _statFinal.ResultId[i] = CYOHOOApp.UNLOAD_FlowId - 1;

                                    Log(_statFinal.ToString() +  CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "-" + (i + 1).ToString("D2") + "】"+ 
                                                                 CLanguage.Lan("条码") +"【" +  _statFinal.SerialNo[i] + "】"+
                                                                 CLanguage.Lan("MES检查PASS与测试信息") + "【" + _statFinal.ResultName[i] +                                                            
                                                                 "】FAIL不一致,以MES结果为准.", udcRunLog.ELog.NG);
                                }                                
                            }
                        }
                    }
                }

                List<int> ttNum = null;

                List<int> failNum = null;

                CalStationFlowYield(out ttNum,out failNum);

                if (!SaveDailyRecord(ttNum, failNum, out er))
                {
                    Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("保存日报表错误:") + er, udcRunLog.ELog.NG);
                }

                _statFinal.bAlarm = false;

                _statFinal.Info = "";

                _statFinal.UIDoRun = EUIStatus.测试结束;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, CYOHOOApp.UNLOAD_FlowName, UIMainArgs));

                //正常治具
                if (uutPass)
                {
                    Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("为PASS,请取出."), udcRunLog.ELog.OK);

                }
                else
                {
                    if (UUTPass)
                    {
                        Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("为FAIL,请取出不良品."), udcRunLog.ELog.NG);
                    }
                    else
                    {
                        Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("所有产品为FAIL,请检查."), udcRunLog.ELog.NG);

                        MessageBox.Show(_statFinal.ToString() + CLanguage.Lan("治具ID") + "【" + _statFinal.IdCard + "】"+ CLanguage.Lan("所有产品为FAIL,请检查."));
                    }
                }

                _statFinal.DoRun = ERUN.结束;

                _statFinal.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                _idCardBak = _statFinal.IdCard;

                CIniFile.WriteToIni("Parameter", "Unload_IDCard", _idCardBak, CGlobalPara.IniFile);

                SaveDailyReport(_statFinal);

                local_db_recordFailSn(_statFinal);  

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 夹抓对应机械手顺序
        /// </summary>
        private int[] plc_result = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        /// <summary>
        /// 写测试结果到PLC:D5100-D5115
        /// </summary>
        private bool writeResultToPLC(EPLCMode handMode)
        {
            try
            {

                int failNum = 0;

                string er = string.Empty;

                int[] regVal = new int[_statFinal.SerialNo.Count];

                for (int i = 0; i < _statFinal.SerialNo.Count; i++)
                {
                    int plcNo = plc_result[i];

                    if (_statFinal.SerialNo[plcNo] == string.Empty)
                    {
                        regVal[i] = 0;
                    }
                    else
                    {
                        if (_statFinal.Result[plcNo] == 0)      //PASS
                        {
                            if (_statFinal.ResultId[plcNo] == CYOHOOApp.ATE_FlowId) //PASS
                            {
                                regVal[i] = 1;
                            }
                            else if (_statFinal.ResultId[plcNo] == CYOHOOApp.HIPOT_FlowId)  //ATE未测试
                            {
                                regVal[i] = 4;
                                failNum++;
                            }
                            else if (_statFinal.ResultId[plcNo] == CYOHOOApp.BI_FlowId)  //HIPOT未测试
                            {
                                regVal[i] = 3;
                                failNum++;
                            }
                            else if (_statFinal.ResultId[plcNo] == CYOHOOApp.PRETEST_FlowId)  //BI未测试
                            {
                                regVal[i] = 2;
                                failNum++;
                            }
                            else                                                  //FT未测试
                            {
                                regVal[i] = 5;
                                failNum++;
                            }

                        }
                        else if (_statFinal.ResultId[plcNo] == CYOHOOApp.PRETEST_FlowId) //PT FAIL
                        {
                            regVal[i] = 5;
                            failNum++;
                        }
                        else if (_statFinal.ResultId[plcNo] == CYOHOOApp.BI_FlowId)  //BI FAIL
                        {
                            regVal[i] = 2;
                            failNum++;
                        }
                        else if (_statFinal.ResultId[plcNo] == CYOHOOApp.HIPOT_FlowId)  //HP FAIL
                        {
                            regVal[i] = 3;
                            failNum++;
                        }
                        else if (_statFinal.ResultId[plcNo] == CYOHOOApp.ATE_FlowId)  //ATE FAIL
                        {
                            regVal[i] = 4;
                            failNum++;
                        }
                        else                               //无测试结果
                        {
                            regVal[i] = 0;
                        }
                    }
                }

                if (CGlobalPara.SysPara.Para.FailGoNum > 0 && failNum >= CGlobalPara.SysPara.Para.FailGoNum)
                {
                    for (int i = 0; i < _statFinal.SerialNo.Count; i++)
                    {
                        if (regVal[i] != 1)
                            regVal[i] = 0;
                    }

                    Log(_statFinal.ToString() + CLanguage.Lan("治具ID") + "[" + _statFinal.IdCard + "]" +
                              CLanguage.Lan("不良数") + "[" + failNum.ToString() + "]"+ CLanguage.Lan("超过") + ":" +
                              CGlobalPara.SysPara.Para.FailGoNum.ToString() + ","+ CLanguage.Lan("不取不良产品."), udcRunLog.ELog.NG);
                }

                if (handMode == EPLCMode.自动下机)
                {
                    _threadPLC.addMutiREGWrite(ERegType.D, 5271, regVal);

                    _threadPLC.addREGWrite(EPLCOUT.自动下机位结果.ToString(), (int)EPLCRESULT.结果OK);

                }
                else
                {
                    _threadPLC.addREGWrite(EPLCOUT.手动下机位治具结果.ToString(), (int)EPLCRESULT.结果OK);
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

        #region 测试工位统计
        /// <summary>
        /// 加载站别产能
        /// </summary>
        private void LoadYield()
        {
            _statYield = new Dictionary<string, CYield>();

            _statYield.Add(CYOHOOApp.PRETEST_FlowName, new CYield());

            _statYield.Add(CYOHOOApp.BI_FlowName, new CYield());

            _statYield.Add(CYOHOOApp.HIPOT_FlowName, new CYield());

            _statYield.Add(CYOHOOApp.ATE_FlowName, new CYield());

            List<CYield> yields = new List<CYield>();

            foreach (string keyName in _statYield.Keys)
            {
                _statYield[keyName].TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", keyName + "_TTNum", CGlobalPara.IniFile, "0"));
                _statYield[keyName].FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", keyName + "_FailNum", CGlobalPara.IniFile, "0"));

                yields.Add(new CYield()
                {
                    TTNum = _statYield[keyName].TTNum,
                    FailNum = _statYield[keyName].FailNum
                });

                UIMainArgs.YieldKey = keyName;

                UIMainArgs.Yield = _statYield[keyName];

                UIMainArgs.DoRun = EUIStatus.显示计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));
            }

            UIMainArgs.Yields = yields;

            UIMainArgs.DoRun = EUIStatus.工位计数;

            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));

            for (int index = 0; index < CGlobalPara.C_HP_STAT_MAX; index++)
            {
                _statHipot[index].test.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "HIPOT" + (index + 1).ToString() + "_TTNum", CGlobalPara.IniFile, "0"));
              
                _statHipot[index].test.FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "HIPOT" + (index + 1).ToString() + "_FailNum", CGlobalPara.IniFile, "0"));

                UIMainArgs.TTNum = _statHipot[index].test.TTNum;

                UIMainArgs.FailNum = _statHipot[index].test.FailNum;

                UIMainArgs.DoRun = EUIStatus.产能计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_statHipot[index].test.idNo, "", UIMainArgs));

            }

            for (int index = 0; index < CGlobalPara.C_ATE_STAT_MAX; index++)
            {              
                _statATE[index].test.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "ATE" + (index + 1).ToString() + "_TTNum", CGlobalPara.IniFile, "0"));
                
                _statATE[index].test.FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "ATE" + (index + 1).ToString() + "_FailNum", CGlobalPara.IniFile, "0"));

                UIMainArgs.TTNum = _statATE[index].test.TTNum;

                UIMainArgs.FailNum = _statATE[index].test.FailNum;

                UIMainArgs.DoRun = EUIStatus.产能计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_statATE[index].test.idNo, "", UIMainArgs));
            }
        }
        /// <summary>
        /// 保存工位产能
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="stat"></param>
        private void SaveStatYield(int idNo, CStatTest stat)
        {
            CIniFile.WriteToIni("Parameter", stat.FlowName + (idNo + 1).ToString() + "_TTNum", stat.TTNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("Parameter", stat.FlowName + (idNo + 1).ToString() + "_FailNum", stat.FailNum.ToString(), CGlobalPara.IniFile); 
        }
        /// <summary>
        /// 计算流程产能
        /// </summary>
        private void CalStationFlowYield(out List<int> ttNum, out List<int> failNum)
        {
            ttNum = new List<int>();
            
            failNum = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                ttNum.Add(0);
                failNum.Add(0);
            }

            try
            {
                if (_statFinal.IdCard == _idCardBak)
                    return;

                Dictionary<string, int> yieldList = new Dictionary<string, int>();

                yieldList.Add(CYOHOOApp.ATE_FlowName, 3);

                yieldList.Add(CYOHOOApp.HIPOT_FlowName, 2);

                yieldList.Add(CYOHOOApp.BI_FlowName, 1);

                yieldList.Add(CYOHOOApp.PRETEST_FlowName, 0);

                Dictionary<string, int> flowList = new Dictionary<string, int>();

                flowList.Add(CYOHOOApp.ATE_FlowName, CYOHOOApp.ATE_FlowId);

                flowList.Add(CYOHOOApp.HIPOT_FlowName, CYOHOOApp.HIPOT_FlowId);

                flowList.Add(CYOHOOApp.BI_FlowName, CYOHOOApp.BI_FlowId);

                flowList.Add(CYOHOOApp.PRETEST_FlowName, CYOHOOApp.PRETEST_FlowId);

                for (int i = 0; i < _statFinal.SerialNo.Count; i++)
                {
                    if (_statFinal.SerialNo[i] == string.Empty)
                        continue;

                    //产品为良品
                    if (_statFinal.Result[i] == 0 && _statFinal.ResultId[i] == CYOHOOApp.UNLOAD_FlowId - 1)
                    {
                        foreach (string keyName in _statYield.Keys)
                        {
                            _statYield[keyName].TTNum++;

                            ttNum[yieldList[keyName]]++;
                        }
                        continue;
                    }
                    //产品未过站
                    if (_statFinal.Result[i] == 0)
                    {
                        foreach (string keyName in _statYield.Keys)
                        {
                            if (flowList[keyName] <= _statFinal.ResultId[i])
                            {
                                _statYield[keyName].TTNum++;

                                ttNum[yieldList[keyName]]++;
                            }
                        }
                    }
                    //产品不良
                    foreach (string keyName in _statYield.Keys)
                    {
                        if (flowList[keyName] == _statFinal.ResultId[i])
                        {
                            _statYield[keyName].TTNum++;
                            _statYield[keyName].FailNum++;

                            ttNum[yieldList[keyName]]++;
                            failNum[yieldList[keyName]]++;
                        }
                        else if (flowList[keyName] < _statFinal.ResultId[i])
                        {
                            _statYield[keyName].TTNum++;

                            ttNum[yieldList[keyName]]++;
                        }
                    }
                }

                List<CYield> yields = new List<CYield>();

                foreach (string keyName in _statYield.Keys)
                {
                    yields.Add(new CYield()
                    {
                        TTNum = _statYield[keyName].TTNum,
                        FailNum = _statYield[keyName].FailNum
                    });


                    CIniFile.WriteToIni("Parameter", keyName + "_TTNum", _statYield[keyName].TTNum.ToString(), CGlobalPara.IniFile);

                    CIniFile.WriteToIni("Parameter", keyName + "_FailNum", _statYield[keyName].FailNum.ToString(), CGlobalPara.IniFile);

                    UIMainArgs.YieldKey = keyName;

                    UIMainArgs.Yield = _statYield[keyName];

                    UIMainArgs.DoRun = EUIStatus.显示计数;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));
                }

                UIMainArgs.Yields = yields;

                UIMainArgs.DoRun = EUIStatus.工位计数;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, "", UIMainArgs));

            }
            catch (Exception ex)
            {
               Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 保存流程产能
        /// </summary>
        private void SaveStationFlowYield()
        {
            foreach (string keyName in _statYield.Keys)
            {
                CIniFile.WriteToIni("Parameter", keyName + "_TTNum", _statYield[keyName].TTNum.ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("Parameter", keyName + "_FailNum", _statYield[keyName].FailNum.ToString(), CGlobalPara.IniFile);
            }
        }
        #endregion

        #region 本地数据库
        private ReaderWriterLock dbLock = new ReaderWriterLock();
        /// <summary>
        /// 存治具信息到数据库中
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="er"></param>
        private bool SaveStatFixToDB(int idNo, CStatHub stat, out string er)
        {

            er = string.Empty;

            string statName = stat.FlowName + (idNo + 1).ToString();

            try
            {
                dbLock.AcquireWriterLock(-1);

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                bool IsExist = false;

                string sqlCmd = string.Empty;

                sqlCmd = "select * from statFixRecord where statName='" + statName + "' order by statName";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                if (ds.Tables[0].Rows.Count != 0)
                {
                    if (ds.Tables[0].Rows.Count == stat.SerialNo.Count)
                    {
                        IsExist = true;
                    }
                    else
                    {
                        sqlCmd = "delete * from statFixRecord where statName='" + statName + "'";
                        if (!db.excuteSQL(sqlCmd, out er))
                            return false;
                    }
                }

                List<string> sqlCmdList = new List<string>();

                if (IsExist)
                {
                    for (int i = 0; i < stat.SerialNo.Count; i++)
                    {
                        sqlCmd = "update statFixRecord set idCard='" + stat.IdCard + "',serialNo='" + stat.SerialNo[i] +
                                 "',curModel='" + stat.ModelName + "',mesFlag=" + stat.MesFlag.ToString() +  
                                 ",orderName='" + stat.OrderName + "'" +
                                 " where statName='" + statName + "' and slotNo=" + i;
                        sqlCmdList.Add(sqlCmd);
                    }
                }
                else
                {
                    for (int i = 0; i < stat.SerialNo.Count; i++)
                    {
                        sqlCmd = "insert into statFixRecord(statName,idCard,slotNo,serialNo,curModel,mesFlag,orderName) values ('" +
                                 statName + "','" + stat.IdCard + "'," + i.ToString() + ",'" + stat.SerialNo[i] + "','" + 
                                 stat.ModelName + "'," + stat.MesFlag.ToString() + ",'" + stat.OrderName + "')";
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
            finally
            {
                dbLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 从数据库中获取治具信息
        /// </summary>
        /// <param name="uutFix"></param>
        /// <param name="er"></param>
        /// <returns></returns> 
        private bool GetStatFixFromDB(int idNo, CStatTest stat, out string er)
        {
            er = string.Empty;

            string statName = stat.FlowName + (idNo + 1).ToString();

            try
            {
                dbLock.AcquireWriterLock(-1);

                string sqlCmd = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                sqlCmd = "select * from statFixRecord where statName='" + statName + "' order by slotNo";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                if (ds.Tables[0].Rows.Count != stat.SerialNo.Count)
                {
                    er = CLanguage.Lan("获取不到该治具信息:") + ds.Tables[0].Rows.Count.ToString();
                    return false;
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    stat.IdCard = ds.Tables[0].Rows[i]["idCard"].ToString();
                    stat.ModelName = ds.Tables[0].Rows[i]["curModel"].ToString();
                    stat.OrderName = ds.Tables[0].Rows[i]["orderName"].ToString();
                    stat.MesFlag = System.Convert.ToInt16(ds.Tables[0].Rows[i]["mesFlag"].ToString());
                    stat.SerialNo[i] = ds.Tables[0].Rows[i]["serialNo"].ToString();                    
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
                dbLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 存治具信息到INI中
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="er"></param>
        private bool SaveStatFixToIni(int idNo, CStatHub stat, out string er)
        {
            er = string.Empty;

            string statName = stat.FlowName + (idNo + 1).ToString();

            try
            {
                dbLock.AcquireWriterLock(-1);

                CIniFile.WriteToIni(statName, "idCard", stat.IdCard, CGlobalPara.IniFile);

                CIniFile.WriteToIni(statName, "curModel", stat.ModelName, CGlobalPara.IniFile);

                CIniFile.WriteToIni(statName, "orderName", stat.OrderName, CGlobalPara.IniFile);

                CIniFile.WriteToIni(statName, "mesFlag", stat.MesFlag.ToString(), CGlobalPara.IniFile);

                for (int i = 0; i < stat.SerialNo.Count; i++)
                    CIniFile.WriteToIni(statName, "Sn" + i.ToString(), stat.SerialNo[i], CGlobalPara.IniFile);

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {
                dbLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 从INI获取治具信息
        /// </summary>
        /// <param name="uutFix"></param>
        /// <param name="er"></param>
        /// <returns></returns> 
        private bool GetStatFixFromIni(int idNo, CStatTest stat, out string er)
        {
            er = string.Empty;

            string statName = stat.FlowName + (idNo + 1).ToString();

            try
            {
                dbLock.AcquireWriterLock(-1);

                stat.IdCard = CIniFile.ReadFromIni(statName, "idCard", CGlobalPara.IniFile);

                stat.ModelName = CIniFile.ReadFromIni(statName, "curModel", CGlobalPara.IniFile);

                stat.OrderName = CIniFile.ReadFromIni(statName, "orderName", CGlobalPara.IniFile);

                stat.MesFlag = System.Convert.ToInt16(CIniFile.ReadFromIni(statName, "mesFlag", CGlobalPara.IniFile, "0"));

                for (int i = 0; i < stat.SerialNo.Count; i++)
                {
                    stat.SerialNo[i] = CIniFile.ReadFromIni(statName, "Sn" + i.ToString(), CGlobalPara.IniFile);
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
                dbLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 插入不良条码记录
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_recordFailSn(CStatHub hub)
        {
            try
            {
                string sNowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                List<string> sqlCmdList = new List<string>();

                string sqlCmd = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                sqlCmd = "delete * from FailRecord where datediff('d', startTime,Now()) > 30";

                sqlCmdList.Add(sqlCmd);

                for (int i = 0; i < hub.SerialNo.Count; i++)
                {
                    if (hub.SerialNo[i] == string.Empty)
                        continue;

                    if (hub.Result[i] != 0)
                    {
                        sqlCmd = "delete * from FailRecord where serialNo='" + hub.SerialNo[i] + "'";

                        sqlCmdList.Add(sqlCmd);

                        string fileName = createDailyReportFile();

                        string failInfo = "【" + hub.ResultName[i] + "】FAIL";

                        string localName = hub.ToString();

                        double TestTime = ((double)hub.Watcher.ElapsedMilliseconds) / 1000;

                        sqlCmd = string.Format("insert into FailRecord(serialNo,idCard,slotNo,localName,startTime,endTime," +
                                                "TestTime,failInfo,failTime,remark1) values ('{0}','{1}',{2},'{3}','{4}','{5}',{6},'{7}','{8}','{9}')",
                                                 hub.SerialNo[i], hub.IdCard, i + 1, localName, hub.StartTime, hub.EndTime, TestTime, failInfo, sNowTime, fileName);
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
        }
        /// <summary>
        /// 创建表单
        /// </summary>
        private void local_db_CreateTable()
        {
            try
            {
                string er = string.Empty;

                List<string> sqlCmdList = new List<string>();

                string sqlCmd = string.Empty;

                string tableName = string.Empty;

                string[] TableNames = null;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                if (!db.GetTableNames(ref TableNames, out er))
                {
                    Log(er, udcRunLog.ELog.NG);

                    return;
                }

                List<string> creatNames = new List<string>();

                //创建不良条码记录表
                if (!TableNames.Contains("FailRecord"))
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
                        Log(er, udcRunLog.ELog.NG);
                }

                for (int i = 0; i < creatNames.Count; i++)
                {
                    Log(CLanguage.Lan("初始化创建本地表单") + "【" + creatNames[i] + "】", udcRunLog.ELog.Action);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 测试数据
        /// <summary>
        /// 保存高压日报表
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void SaveDailyReport(CStatHub hub)
        {
            try
            {
                if (!CGlobalPara.SysPara.Report.SaveReport)
                    return;

                string fileName = createDailyReportFile();

                if (!File.Exists(fileName))
                {
                    saveReportTitle(fileName);
                }

                saveReportVal(fileName, hub);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err); 
            }
        }
        /// <summary>
        /// 创建日报表文件
        /// </summary>
        /// <returns></returns>
        private string createDailyReportFile()
        {
            try
            {
                if (CGlobalPara.SysPara.Report.ReportPath == string.Empty)
                {
                    CGlobalPara.SysPara.Report.ReportPath = Application.StartupPath + "\\Report";
                }

                string reportPath = CGlobalPara.SysPara.Report.ReportPath + "\\";

                if (!Directory.Exists(reportPath))
                    Directory.CreateDirectory(reportPath);

                string fileName = DateTime.Now.ToString("ddMMyyyy") + ".csv";

                fileName = reportPath + "\\" + fileName;

                return fileName;

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 保存报表标题栏
        /// </summary>
        /// <returns></returns>
        private void saveReportTitle(string fileName)
        {
            try
            {
                string strWrite = string.Empty;

                StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                strWrite = "治具RFID,槽位,产品条码,当前工位,测试结果,测试时间";

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
        /// 保存测试值
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="idNo"></param>
        private void saveReportVal(string fileName, CStatHub hub)
        {
            try
            {
                string strWrite = string.Empty;

                string sNowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                for (int i = 0; i < hub.SerialNo.Count; i++)
                {
                    if (hub.SerialNo[i] == string.Empty)
                        continue;

                    strWrite = "FIX-" + hub.IdCard + ",";

                    strWrite = (i+1).ToString("D2") + ",";

                    strWrite += hub.SerialNo[i] + ",";

                    strWrite += hub.ResultName[i] + ",";

                    strWrite += (hub.Result[i] == 0 ? "PASS" : "FAIL") + ",";

                    strWrite += sNowTime;

                    sw.WriteLine(strWrite);

                }
                sw.Flush();
                sw.Close();
                sw = null;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region PLC线程消息
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
        #endregion

        #region TCP/IP事件
        /// <summary>
        /// TCP同步锁
        /// </summary>
        private ReaderWriterLock _TCPLock = new ReaderWriterLock();
        /// <summary>
        /// TCP状态消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTcpStatus(object sender, CTcpConArgs e)
        {
            try
            {
                _TCPLock.AcquireWriterLock(-1);

                if (!e.bErr)
                    FrmSerTCP.log(e.conStatus, udcRunLog.ELog.Action);
                else
                    FrmSerTCP.log(e.conStatus, udcRunLog.ELog.NG);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _TCPLock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// TCP数据消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTcpRecv(object sender, CTcpRecvArgs e)
        {
            try
            {
                _TCPLock.AcquireWriterLock(-1);

                string er = string.Empty;

                if (CGlobalPara.SysPara.Dev.TcpMode == 0)       //结构体
                {
                    byte[] sendBytes = _serReponse.ReponseStruct(e.recvBytes);

                    byte[] recvBytes = null;

                    CSerSocket.SOCKET_REQUEST request = CStuct<CSerSocket.SOCKET_REQUEST>.BytesToStruct(e.recvBytes);

                    FrmSerTCP.log(CSerSocket.CSOCKET_INFO.show(request), udcRunLog.ELog.Action);

                    if (_devSerTCP.send(e.remoteEndPoint, sendBytes, 0, out recvBytes, out er))
                    {
                        CSerSocket.SOCKET_REQUEST quest = CStuct<CSerSocket.SOCKET_REQUEST>.BytesToStruct(sendBytes);

                        FrmSerTCP.log(CSerSocket.CSOCKET_INFO.show(quest), udcRunLog.ELog.OK);
                    }
                    else
                    {
                        FrmSerTCP.log(er, udcRunLog.ELog.NG);
                    }
                }
                else if (CGlobalPara.SysPara.Dev.TcpMode == 1)  //字符串
                {
                    string rData = string.Empty;

                    string sendCmd = _serReponse.ReponseString(e.recvData);

                    FrmSerTCP.log(e.recvData, udcRunLog.ELog.Action);

                    if (_devSerTCP.send(e.remoteEndPoint, sendCmd, 0, out rData, out er))
                        FrmSerTCP.log(sendCmd, udcRunLog.ELog.OK);
                    else
                        FrmSerTCP.log(er, udcRunLog.ELog.NG);
                }
                else if (CGlobalPara.SysPara.Dev.TcpMode == 2)  //JSON
                {
                    string rData = string.Empty;

                    string sendCmd = _serReponse.ReponseJSON(e.recvData);

                    FrmSerTCP.log(e.recvData, udcRunLog.ELog.Action);

                    if (_devSerTCP.send(e.remoteEndPoint, sendCmd, 0, out rData, out er))
                        FrmSerTCP.log(sendCmd, udcRunLog.ELog.OK);
                    else
                        FrmSerTCP.log(er, udcRunLog.ELog.NG);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _TCPLock.ReleaseWriterLock();
            }
        }
        #endregion

        #region 冠佳Web
        /// <summary>
        /// 写入Web治具结果
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool UpdateFixtureResult(CStatHub hub)
        {
            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = new CWeb2.CFixture();
                fixture.Base.FlowIndex = 0;
                fixture.Base.FlowName = hub.FlowName;
                fixture.Base.FlowGuid = CNet.HostName();
                fixture.Base.SnType = CWeb2.ESnType.外部条码;
                fixture.Base.IdCard = hub.IdCard;
                fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                for (int i = 0; i < hub.SerialNo.Count; i++)
                {
                    CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                    para.SlotNo = i;
                    para.SerialNo = hub.SerialNo[i];
                    para.InnerSn = string.Empty;
                    para.Remark1 = string.Empty;
                    para.Remark2 = string.Empty;
                    para.StartTime = hub.StartTime;
                    para.EndTime = hub.EndTime;
                    para.Result =  hub.Result[i];
                    para.TestData = string.Empty;                    
                    fixture.Para.Add(para);
                }

                if (!CWeb2.UpdateFixtureResult(fixture, out er))
                {
                    MessageBox.Show(hub.ToString() + CLanguage.Lan("写入治具ID") +"【" + hub.IdCard + "】"+ CLanguage.Lan("工位") + "【" + hub.FlowName + "】"+
                                                     CLanguage.Lan("测试结果错误:") + er);

                    if (!CWeb2.UpdateFixtureResult(fixture, out er))
                    {
                        Log(hub.ToString() + CLanguage.Lan("写入治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("工位") + "【" + hub.FlowName + "】"+
                                                      CLanguage.Lan("测试结果错误:") + er, udcRunLog.ELog.NG);

                        return false;
                    }
                }
                else
                {
                    Log(hub.ToString() + CLanguage.Lan("写入治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("工位") + "【" + hub.FlowName + "】"+
                                        CLanguage.Lan("测试结果OK:") + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);
                                                         
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
        /// 写入Web治具结果
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool UpdateFixtureResult(CStatTest hub)
        {
            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = new CWeb2.CFixture();
                fixture.Base.FlowIndex = 0;
                fixture.Base.FlowName = hub.FlowName;
                fixture.Base.FlowGuid = CNet.HostName();
                fixture.Base.SnType = CWeb2.ESnType.外部条码;
                fixture.Base.IdCard = hub.IdCard;
                fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                for (int i = 0; i < hub.SerialNo.Count; i++)
                {
                    CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                    para.SlotNo = i;
                    para.SerialNo = hub.SerialNo[i];
                    para.InnerSn = string.Empty;
                    para.Remark1 = string.Empty;
                    para.Remark2 = string.Empty;
                    para.StartTime = hub.StartTime;
                    para.EndTime = hub.EndTime;
                    para.Result = hub.Result[i];
                    para.TestData = string.Empty;
                    fixture.Para.Add(para);
                }

                if (!CWeb2.UpdateFixtureResult(fixture, out er))
                {
                    MessageBox.Show(hub.ToString() + CLanguage.Lan("写入治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("工位") +
                                                    "【" + hub.FlowName + "】"+ CLanguage.Lan("测试结果错误:") + er);

                    if (!CWeb2.UpdateFixtureResult(fixture, out er))
                    {
                        Log(hub.ToString() + CLanguage.Lan("写入治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("工位") + 
                                             "【" + hub.FlowName + "】"+ CLanguage.Lan("测试结果错误:") + er, udcRunLog.ELog.NG);

                        return false;
                    }
                }
                else
                {
                    Log(hub.ToString() + CLanguage.Lan("写入治具ID") + "【" + hub.IdCard + "】"+ CLanguage.Lan("工位") + "【" + hub.FlowName + "】"+
                                        CLanguage.Lan("测试结果OK:") + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);                                                         
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

        #region 客户MES
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
        private bool CheckSn(string statName, string Sn, out string er)
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
                    StatName = statName,
                    SerialNo = Sn,
                    OrderName = string.Empty,
                    Remark1 = string.Empty,
                    Remark2 = string.Empty
                };

                if (mes.CheckSn(Info, out er))
                {
                    Log(CLanguage.Lan("条码") + "【" + Sn + "】" + CLanguage.Lan("当前站别为") + "【" + statName + "】," + 
                                                                   CLanguage.Lan("MES检查条码OK"), udcRunLog.ELog.Action);
                    return true;
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
        /// 获取治具条码测试结果
        /// </summary>
        /// <returns></returns>
        private bool GetSnResult(out List<int> resultList, out string er)
        {
            er = string.Empty;

            resultList = new List<int>();

            try
            {
                for (int i = 0; i < _statFinal.SerialNo.Count; i++)
                {
                    resultList.Add(0);

                    if (_statFinal.SerialNo[i] != string.Empty)
                    {
                        if (!CheckSn("UNLOAD", _statFinal.SerialNo[i], out er))
                        {
                             resultList[i] = 1;
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
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].TTNum = _statFinal.TTNum;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].FailNum = _statFinal.FailNum;
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

        #region 日产能报表
        /// <summary>
        /// 日产能统计
        /// </summary>
        private CDailyYield _DailyYield = new CDailyYield();
        /// <summary>
        /// 保存日产能
        /// </summary>
        private bool SaveDailyRecord(List<int> ttNum, List<int> failNum, out string er)
        {
            er = string.Empty;

            try
            {
                if (_DailyYield.DayNow == string.Empty)
                {
                    _DailyYield.DayNow = DateTime.Today.ToString("yyyy-MM-dd");

                    for (int i = 0; i < _DailyYield.TTNum.Count; i++)
                    {
                        _DailyYield.TTNum[i] = 0;
                        _DailyYield.FailNum[i] = 0;
                    }                   
                }

                for (int i = 0; i < ttNum.Count; i++)
                {
                    _DailyYield.TTNum[i] += ttNum[i];
                    _DailyYield.FailNum[i] += failNum[i];
                    if (_DailyYield.TTNum[i] != 0)
                    {
                        _DailyYield.PassRate[i] = (double)(_DailyYield.TTNum[i] - _DailyYield.FailNum[i]) / (double)_DailyYield.TTNum[i];
                    }
                }

                if (System.Convert.ToDateTime(_DailyYield.DayNow) >= DateTime.Today)
                    return true;

                if (CGlobalPara.SysPara.Report.DailyFolder == string.Empty)
                    return true;

                if (!Directory.Exists(CGlobalPara.SysPara.Report.DailyFolder))
                    Directory.CreateDirectory(CGlobalPara.SysPara.Report.DailyFolder);


                string fileName = _DailyYield.DayNow + ".xml";
                string filePath = CGlobalPara.SysPara.Report.DailyFolder + "\\" + fileName;
                string strXml = "<?xml version=" + "\"" + "1.0" + "\"" + " encoding=" + "\"" + "GB2312" + "\"" + "?> " + "\r\n";
                strXml += "<ConfigSet>" + "\r\n";
                for (int i = 0; i < _DailyYield.TTNum.Count; i++)
                {
                    strXml += "<!--日产能统计-->" + "\r\n";
                    strXml += "<General name=" + "\"" + _DailyYield.Name[i] + "\"" + ">" + "\r\n";
                    strXml += "<!--总数-->" + "\r\n";
                    strXml += "<TolNum>" + _DailyYield.TTNum[i].ToString() + "</TolNum>" + "\r\n";
                    strXml += "<!--良品数-->" + "\r\n";
                    strXml += "<PassNum>" + (_DailyYield.TTNum[i] - _DailyYield.FailNum[i]).ToString() + "</PassNum>" + "\r\n";
                    strXml += "<!--不良品数-->" + "\r\n";
                    strXml += "<FailNum>" + _DailyYield.FailNum[i].ToString() + "</FailNum>" + "\r\n";
                    strXml += "<!--直通率(%)-->" + "\r\n";
                    strXml += "<PassRate>" + _DailyYield.PassRate[i].ToString("P2") + "</PassRate>" + "\r\n";
                    strXml += "</General>" + "\r\n";
                }               
                strXml += "</ConfigSet>" + "\r\n";
                StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
                sw.Write(strXml);
                sw.Flush();
                sw.Close();

                for (int i = 0; i < _DailyYield.TTNum.Count; i++)
                {
                    _DailyYield.TTNum[i] = 0;
                    _DailyYield.FailNum[i] = 0;
                }               
                _DailyYield.DayNow = DateTime.Today.ToString("yyyy-MM-dd");

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {
                SaveIniForDaily();
            }
        }
        /// <summary>
        /// 保存INI
        /// </summary>
        private void SaveIniForDaily()
        {
            CIniFile.WriteToIni("DailyYield", "DayNow", _DailyYield.DayNow, CGlobalPara.IniFile);

            for (int idNo = 0; idNo < _DailyYield.TTNum.Count; idNo++)
            {
                CIniFile.WriteToIni("DailyYield", "TTNum" + idNo.ToString(), _DailyYield.TTNum[idNo].ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("DailyYield", "FailNum" + idNo.ToString(), _DailyYield.FailNum[idNo].ToString(), CGlobalPara.IniFile);
            }           
        }
        /// <summary>
        /// 加载INI
        /// </summary>
        private void LoadIniForDaily()
        {
            _DailyYield.DayNow = CIniFile.ReadFromIni("DailyYield", "DayNow", CGlobalPara.IniFile);

            for (int idNo = 0; idNo <  _DailyYield.TTNum.Count; idNo++)
            {
                _DailyYield.TTNum[idNo] = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "TTNum" + idNo.ToString(), CGlobalPara.IniFile, "0"));
                _DailyYield.FailNum[idNo] = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "FailNum" + idNo.ToString(), CGlobalPara.IniFile, "0"));
            }
        }
        #endregion

        #region 良品预警功能
        private const int _WarnNum = 2;
        private int[] _WarnIdNo = new int[]{ 0, 1};
        private string[] _WarnName = new string[]{"HIPOT","ATE"};
        /// <summary>
        /// 预警类
        /// </summary>
        private CWarnRate[] _WarnRate = new CWarnRate[] { new CWarnRate(), new CWarnRate() };
        /// <summary>
        /// 刷新日常统计
        /// </summary>
        /// <returns></returns>
        private bool RefreshDailyPassRate()
        {
            try
            {
                for (int idNo = 0; idNo < _WarnNum; idNo++)
                {
                    //是否启用预警功能
                    if (!CGlobalPara.SysPara.Alarm.ChkPassRate[idNo])
                        continue;

                    if (_WarnRate[idNo].CurrentDate == string.Empty)
                    {
                        _WarnRate[idNo].CurrentDate = DateTime.Now.ToString("yyyy/MM/dd");
                        _WarnRate[idNo].CurrentDayClr = 0;
                        _WarnRate[idNo].CurrentNightClr = 0;
                    }

                    if (System.Convert.ToDateTime(_WarnRate[idNo].CurrentDate).Date < DateTime.Now.Date)
                    {
                        _WarnRate[idNo].CurrentDate = DateTime.Now.ToString("yyyy/MM/dd");
                        _WarnRate[idNo].CurrentDayClr = 0;
                        _WarnRate[idNo].CurrentNightClr = 0;
                    }

                    if (CGlobalPara.SysPara.Alarm.ChkClrDay && _WarnRate[idNo].CurrentDayClr == 0)
                    {
                        if (CGlobalPara.SysPara.Alarm.ClrDayTime == string.Empty)
                        {
                            CGlobalPara.SysPara.Alarm.ClrDayTime = "08:00:00";
                        }
                        string dayTime = DateTime.Now.ToString("yyyy/MM/dd") + " " + CGlobalPara.SysPara.Alarm.ClrDayTime;
                        if (System.Convert.ToDateTime(dayTime) <= DateTime.Now)
                        {
                            _WarnRate[idNo].TTNum = 0;
                            _WarnRate[idNo].PassNum = 0;
                            _WarnRate[idNo].CurrentDayClr = 1;
                        }
                    }

                    if (CGlobalPara.SysPara.Alarm.ChkClrNight && _WarnRate[idNo].CurrentNightClr == 0)
                    {
                        if (CGlobalPara.SysPara.Alarm.ClrNightTime == string.Empty)
                        {
                            CGlobalPara.SysPara.Alarm.ClrNightTime = "20:00:00";
                        }
                        string dayTime = DateTime.Now.ToString("yyyy/MM/dd") + " " + CGlobalPara.SysPara.Alarm.ClrNightTime;
                        if (System.Convert.ToDateTime(dayTime) <= DateTime.Now)
                        {
                            _WarnRate[idNo].TTNum = 0;
                            _WarnRate[idNo].PassNum = 0;
                            _WarnRate[idNo].CurrentNightClr = 1;
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
            finally
            {
                for (int idNo = 0; idNo < _WarnNum; idNo++)
                {
                    SaveIniForPassRate(idNo);
                }
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
                bool bAlarm = false;

                bool bRelease = false;

                for (int idNo = 0; idNo < _WarnNum; idNo++)
                {
                    if (!CGlobalPara.SysPara.Alarm.ChkPassRate[idNo])
                    {
                        _WarnRate[idNo].bAlarm = 0;
                        _WarnRate[idNo].DoRun = EWarnResult.未启动;
                        OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo[idNo], _WarnName[idNo], _WarnRate[idNo]));
                        continue;
                    }

                    bool CheckOK = true;

                    if (_WarnRate[idNo].TTNum == 0)
                        _WarnRate[idNo].PassRate = 1;
                    else
                        _WarnRate[idNo].PassRate = (double)_WarnRate[idNo].PassNum / (double)_WarnRate[idNo].TTNum;

                    if (_WarnRate[idNo].TTNum >= CGlobalPara.SysPara.Alarm.PassRateStartNum[idNo] &&
                        _WarnRate[idNo].PassRate < CGlobalPara.SysPara.Alarm.PassRateLimit[idNo])
                    {
                        CheckOK = false;
                    }

                    switch (_WarnRate[idNo].DoRun)
                    {
                        case EWarnResult.空闲:
                            if (CheckOK)
                            {
                                bRelease = true;
                                _WarnRate[idNo].DoRun = EWarnResult.正常;
                                _WarnRate[idNo].bAlarm = 0;
                                _WarnRate[idNo].Watcher.Stop();
                            }
                            else
                            {
                                bAlarm = true;
                                _WarnRate[idNo].DoRun = EWarnResult.报警;
                                _WarnRate[idNo].Watcher.Restart();
                                _WarnRate[idNo].bAlarm = 1;
                                Log(_WarnRate.ToString() + CLanguage.Lan("当前良率") + "[" + _WarnRate[idNo].PassRate.ToString("P2") +
                                                           "]"+ CLanguage.Lan("低于设定值") + "[" + CGlobalPara.SysPara.Alarm.PassRateLimit[idNo].ToString("P2") + 
                                                           "],"+ CLanguage.Lan("请检查确认."), udcRunLog.ELog.NG);
                            }
                            break;
                        case EWarnResult.正常:
                            if (!CheckOK)
                            {
                                _WarnRate[idNo].bAlarm = 0;
                                _WarnRate[idNo].DoRun = EWarnResult.报警;
                                _WarnRate[idNo].Watcher.Restart();
                            }
                            break;
                        case EWarnResult.报警:
                            if (CheckOK)
                            {
                                bRelease = true;
                                _WarnRate[idNo].bAlarm = 0;
                                _WarnRate[idNo].DoRun = EWarnResult.正常;
                                _WarnRate[idNo].Watcher.Stop();
                            }
                            else
                            {
                                if (_WarnRate[idNo].Watcher.ElapsedMilliseconds >= CGlobalPara.SysPara.Alarm.PassRateAlarmTime * 60000)
                                {
                                    bAlarm = true;
                                    _WarnRate[idNo].DoRun = EWarnResult.报警;
                                    _WarnRate[idNo].Watcher.Restart();
                                    _WarnRate[idNo].bAlarm = 1;
                                    Log(_WarnRate[idNo].ToString() + CLanguage.Lan("当前良率") + "[" + _WarnRate[idNo].PassRate.ToString("P2") +
                                                               "]"+ CLanguage.Lan("低于设定值") + "[" + CGlobalPara.SysPara.Alarm.PassRateLimit[idNo].ToString("P2") +
                                                               "],"+ CLanguage.Lan("请检查确认."), udcRunLog.ELog.NG);
                                }
                            }
                            break;
                        case EWarnResult.确认报警:
                            bRelease = true;
                            _WarnRate[idNo].bAlarm = 0;
                            _WarnRate[idNo].DoRun = EWarnResult.正常;
                            _WarnRate[idNo].Watcher.Stop();
                            Log(_WarnRate[idNo].ToString() + CLanguage.Lan("人工确认解除良率报警") + "[" + _WarnRate[idNo].PassRate.ToString("P2") + "]", udcRunLog.ELog.Action);
                            break;
                        default:
                            break;
                    }

                    OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo[idNo], _WarnName[idNo], _WarnRate[idNo]));
                }

                if (bAlarm)
                {
                    _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 1);
                }
                else if (bRelease)
                {
                    _threadPLC.addREGWrite(EPLCOUT.上位机软件报警.ToString(), 0);
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
        /// 保存INI
        /// </summary>
        private void SaveIniForPassRate(int idNo)
        {
            CIniFile.WriteToIni("WarnRate", "TTNum" + idNo.ToString(), _WarnRate[idNo].TTNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "PassNum" + idNo.ToString(), _WarnRate[idNo].PassNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "CurrentDate" + idNo.ToString(), _WarnRate[idNo].CurrentDate, CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "CurrentDayClr" + idNo.ToString(), _WarnRate[idNo].CurrentDayClr.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("WarnRate", "CurrentNightClr" + idNo.ToString(), _WarnRate[idNo].CurrentNightClr.ToString(), CGlobalPara.IniFile);
        }
        /// <summary>
        /// 获取INI
        /// </summary>
        private void LoadIniForPassRate(int idNo)
        {
            _WarnRate[idNo].TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "TTNum" + idNo.ToString(), CGlobalPara.IniFile, "0"));
            _WarnRate[idNo].PassNum = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "PassNum" + idNo.ToString(), CGlobalPara.IniFile, "0"));
            _WarnRate[idNo].CurrentDate = CIniFile.ReadFromIni("WarnRate", "CurrentDate" + idNo.ToString(), CGlobalPara.IniFile);
            _WarnRate[idNo].CurrentDayClr = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "CurrentDayClr" + idNo.ToString(), CGlobalPara.IniFile, "0"));
            _WarnRate[idNo].CurrentNightClr = System.Convert.ToInt32(CIniFile.ReadFromIni("WarnRate", "CurrentNightClr" + idNo.ToString(), CGlobalPara.IniFile, "0"));
        }
        /// <summary>
        /// 设置预警线
        /// </summary>
        /// <param name="passRateLimit"></param>
        public void SetPassRateLimit(int idNo,bool chkPassRate, double passRateLimit)
        {
            if (!chkPassRate)
                _WarnRate[idNo].DoRun = EWarnResult.未启动;
            else
                _WarnRate[idNo].DoRun = EWarnResult.空闲;
            _WarnRate[idNo].PassRateLimit = passRateLimit;
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo[idNo], _WarnName[idNo], _WarnRate[idNo]));
        }
        /// <summary>
        /// 清除统计数量
        /// </summary>
        public void ClearPassRate(int idNo)
        {
            _WarnRate[idNo].TTNum = 0;
            _WarnRate[idNo].PassNum = 0;
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo[idNo], _WarnName[idNo], _WarnRate[idNo]));
            SaveIniForPassRate(idNo);
        }
        /// <summary>
        /// 清除统计报警
        /// </summary>
        public void ClearPassRateAlarm(int idNo)
        {
            _WarnRate[idNo].bAlarm = -1;
            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo[idNo], _WarnName[idNo], _WarnRate[idNo]));
        }
        #endregion

    }
}
