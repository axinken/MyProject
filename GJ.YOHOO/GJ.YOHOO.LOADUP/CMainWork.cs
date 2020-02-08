using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using GJ.USER.APP.MainWork;
using GJ.USER.APP;
using GJ.USER.APP.Iot;
using GJ.DEV.PLC;
using GJ.PDB;
using GJ.DEV.CARD;
using GJ.DEV.FCMB;
using GJ.DEV.BARCODE;
using GJ.DEV.Meter;
using GJ.DEV.SafeDog;
using GJ.UI;
using GJ.MES;
using GJ.COM;
using GJ.Iot;
using GJ.SFCS;
using GJ.DEV.ELOAD;

namespace GJ.YOHOO.LOADUP
{
    public class CMainWork : IMainWork
    {
        #region 构造函数
        private int _idNo = 0;
        private string _name = string.Empty;
        private string _guid = string.Empty;
        public CMainWork(int idNo, string name, string guid)
        {
            this._idNo = idNo;
            this._name = name;
            this._guid = guid;
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

                if (!load_user_plc_reg(CLanguage.Lan("通电测试位"), out er))
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
                CGlobalPara.DeviceIDNo = CIniFile.ReadFromIni(CGlobalPara.StationName, "GuidSn", CGlobalPara.IniFile, CGlobalPara.StationName);

                _defaultModelPath = CIniFile.ReadFromIni("Parameter", "ModelPath", CGlobalPara.IniFile);

                _statInfo.StatUseNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "StatUseNum", CGlobalPara.IniFile, "0"));

                _statInfo.StatFailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "StatFailNum", CGlobalPara.IniFile, "0"));

                _statInfo.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "TTNum", CGlobalPara.IniFile, "0"));

                _statInfo.FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "FailNum", CGlobalPara.IniFile, "0"));

                LoadIniForDaily();

                LoadIniForPassRate();

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

                OnUIModelArgs.OnEvented(new CUIUserArgs<CModelPara>(_idNo, _name, _runModel));
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void LoadMainFormUI()
        {
            base.LoadMainFormUI();
        }
        public override void LoadUIComplete()
        {
            MainWorker = new BackgroundWorker();
            MainWorker.WorkerSupportsCancellation = true;
            MainWorker.WorkerReportsProgress = true;
            MainWorker.DoWork += new DoWorkEventHandler(MainWorker_DoWork);
            OnUIGlobalArgs.OnEvented(new CUIGlobalArgs());

            UIStatDataArgs.UseNum = _statInfo.StatUseNum;
            UIStatDataArgs.DoRun = EUIStatData.使用次数;
            OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

            UIStatDataArgs.TTNum = _statInfo.TTNum;
            UIStatDataArgs.FailNum = _statInfo.FailNum;
            UIStatDataArgs.ConFailNum = _statInfo.StatFailNum;
            UIStatDataArgs.DoRun = EUIStatData.产能统计;
            OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

            OnUIPassRateArgs.OnEvented(new CUIUserArgs<CWarnRate>(_WarnIdNo, _WarnName, _WarnRate));

            InitialIot();
        }
        #endregion

        #region 实现抽象方法
        public override bool InitialRunPara()
        {
            if (!CheckSafeDog())
                return false;

            if (_runModel == null || _runModel.Base.Model == null)
            {
                MessageBox.Show(CLanguage.Lan("请选择要测试机种名称,再启动监控."), "Tip",
                           MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            _statInfo.SetNewLoad = true;

            _statManualScan.Para.DoRun = ERUN.空闲;

            _statAutoScan.Para.DoRun = ERUN.空闲;

            _statPreTest.hub.Para.DoRun = ERUN.空闲;

            _statPreTest.test.Para.DoRun = ERUN.空闲;

            _statInBI.Para.DoRun = ERUN.空闲;

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

                IniTask.Add(Task.Factory.StartNew(() => OpenELoadMon()));

                IniTask.Add(Task.Factory.StartNew(() => OpenACMuti()));

                IniTask.Add(Task.Factory.StartNew(() => OpenQCVMon()));

                IniTask.Add(Task.Factory.StartNew(() => OpenSnCom()));

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
                    Log(CLanguage.Lan("系统硬件设备自检结束") + "【PASS】:" + waitTime, udcRunLog.ELog.OK);
                }
                else
                {
                    Log(CLanguage.Lan("系统硬件设备自检结束") + "【FAIL】:" + waitTime, udcRunLog.ELog.NG);
                }
            }
        }
        public override void CloseDevice()
        {
            ClosePLC();

            CloseIdCard();

            CloseELoadMon();

            CloseSnCom();

            CloseQCVMon();

            CloseACMuti();
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
                    Log("PLC" + CLanguage.Lan("监控线程启动"), udcRunLog.ELog.Action);
                }

                //启动主线程
                if (!MainWorker.IsBusy)
                    MainWorker.RunWorkerAsync();

                UISystemArgs.DoRun = EUISystem.启动;

                OnUISystemArgs.OnEvented(new CUIUserArgs<CUISystemArgs>(_idNo, _name, UISystemArgs));

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
                //销毁子线程

                if (_statManualScan.Para._cts != null)
                    _statManualScan.Para._cts.Cancel();

                if (_statAutoScan.Para._cts != null)
                    _statAutoScan.Para._cts.Cancel();

                if (_statPreTest.hub.Para._cts != null)
                    _statPreTest.hub.Para._cts.Cancel();

                if (_statPreTest.test.Para._cts != null)
                    _statPreTest.test.Para._cts.Cancel();

                if (_statInBI.Para._cts != null)
                    _statInBI.Para._cts.Cancel();

                //销毁主线程                
                if (MainWorker.IsBusy)
                    MainWorker.CancelAsync();

                while (MainWorker.IsBusy)
                {
                    Application.DoEvents();
                }

                while (_statManualScan.Para._cts != null ||
                      _statAutoScan.Para._cts != null ||
                      _statPreTest.hub.Para._cts != null ||
                      _statInBI.Para._cts != null)
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
                    Log("PLC" + CLanguage.Lan("监控线程销毁"), udcRunLog.ELog.NG);
                }

                UISystemArgs.DoRun = EUISystem.空闲;

                OnUISystemArgs.OnEvented(new CUIUserArgs<CUISystemArgs>(_idNo, _name, UISystemArgs));

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
        /// 机种信息UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CModelPara>> OnUIModelArgs = new COnEvent<CUIUserArgs<CModelPara>>();
        /// <summary>
        /// 治具位UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CStatHub>> OnUIBandSnArgs = new COnEvent<CUIUserArgs<CStatHub>>();
        /// <summary>
        /// 系统UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUISystemArgs>> OnUISystemArgs = new COnEvent<CUIUserArgs<CUISystemArgs>>();
        /// <summary>
        /// 信息提示
        /// </summary>
        public COnEvent<CUIUserArgs<CUIActionAgrs>> OnUIActionArgs = new COnEvent<CUIUserArgs<CUIActionAgrs>>();
        /// <summary>
        /// 扫描条码UI
        /// </summary>
        public COnEvent<CUIUserArgs<CUIScanSnArgs>> OnUIScanSnArgs = new COnEvent<CUIUserArgs<CUIScanSnArgs>>();
        /// <summary>
        /// 治具位UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CStat>> OnUIStatHubArgs = new COnEvent<CUIUserArgs<CStat>>();
        /// <summary>
        /// 治具位UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CStat>> OnUIStatTestArgs = new COnEvent<CUIUserArgs<CStat>>();
        /// <summary>
        /// 治具位UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUIStatDataArgs>> OnUIStatDataArgs = new COnEvent<CUIUserArgs<CUIStatDataArgs>>();
        /// <summary>
        /// 进老化位UI消息
        /// </summary>
        public COnEvent<CUIUserArgs<CStatHub>> OnUIInBIArgs = new COnEvent<CUIUserArgs<CStatHub>>();
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
        /// 扫描条码UI
        /// </summary>
        private CUIScanSnArgs UIScanSnArgs = new CUIScanSnArgs();
        /// <summary>
        /// 测试工位UI
        /// </summary>
        private CUIStatDataArgs UIStatDataArgs = new CUIStatDataArgs();
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
                _PLCAlarmList = new CPLCAlarmList(_idNo, name);
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
                        er = CLanguage.Lan("功能寄存器") + "[" + regName + "]" + CLanguage.Lan("未在PLC列表定义");
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

                _statInfo = new CStatInfo(CYOHOOApp.SlotMax);

                _statManualScan = new CStatHub(0, "<" + CLanguage.Lan("人工扫描绑定位") + ">",
                                                  CYOHOOApp.LOADUP_FlowId, CYOHOOApp.LOADUP_FlowName, CYOHOOApp.SlotMax);
                _statManualScan.Base.IdCardAddr = 1;
                _statManualScan.Base.SnBarNo[0] = new int[CYOHOOApp.SlotMax / 2];
                _statManualScan.Base.SnBarNo[1] = new int[CYOHOOApp.SlotMax / 2];
                string[] Str1 = CYOHOOApp.LOADUP_Manual_SnNo1.Split(',');
                string[] Str2 = CYOHOOApp.LOADUP_Manual_SnNo2.Split(',');
                for (int i = 0; i < CYOHOOApp.SlotMax / 2; i++)
                {
                    _statManualScan.Base.SnBarNo[0][i] = System.Convert.ToInt16(Str1[i]) - 1;
                    _statManualScan.Base.SnBarNo[1][i] = System.Convert.ToInt16(Str2[i]) - 1;
                }

                _statAutoScan = new CStatHub(0, "<" + CLanguage.Lan("自动扫描绑定位") + ">",
                                                CYOHOOApp.LOADUP_FlowId, CYOHOOApp.LOADUP_FlowName, CYOHOOApp.SlotMax);
                _statAutoScan.Base.IdCardAddr = 2;
                _statAutoScan.Base.SnBarNo[0] = new int[CYOHOOApp.SlotMax / 2];
                _statAutoScan.Base.SnBarNo[1] = new int[CYOHOOApp.SlotMax / 2];
                string[] Str3 = CYOHOOApp.LOADUP_Auto_SnNo1.Split(',');
                string[] Str4 = CYOHOOApp.LOADUP_Auto_SnNo2.Split(',');
                for (int i = 0; i < CYOHOOApp.SlotMax / 2; i++)
                {
                    _statAutoScan.Base.SnBarNo[0][i] = System.Convert.ToInt16(Str3[i]) - 1;
                    _statAutoScan.Base.SnBarNo[1][i] = System.Convert.ToInt16(Str4[i]) - 1;
                }

                _statPreTest = new CStat(1, "<" + CLanguage.Lan("初测测试检查位") + ">", "<" + CLanguage.Lan("初测通电测试位") + ">",
                                                  CYOHOOApp.PRETEST_FlowId, CYOHOOApp.PRETEST_FlowName, CYOHOOApp.SlotMax);
                _statPreTest.hub.Base.IdCardAddr = 2;

                _statInBI = new CStatHub(2, "<" + CLanguage.Lan("进老化检查位") + ">", CYOHOOApp.BI_FlowId, CYOHOOApp.BI_FlowName, CYOHOOApp.SlotMax);
                _statInBI.Base.IdCardAddr = 0;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        #endregion

        #region PLC定义
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
        /// PLC输入状态
        /// </summary>
        private enum EPLCINP
        {
            PLC自动运行,
            PLC设备报警,
            PLC设备警告,
            人工扫条码位治具到位,
            人工扫条码位治具光电,
            自动扫条码位治具到位,
            自动扫条码位治具光电,
            初测位轨道治具到位,
            初测位测试准备OK,
            初测位轨道治具光电,
            进老化前检查位治具到位,
            进老化前检查位治具光电,
            自动扫条码位置号,

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
        /// PLC输出状态
        /// </summary>
        private enum EPLCOUT
        {
            上位机软件启动,
            上位机软件报警,
            人工扫条码位治具结果,
            自动扫条码位治具结果,
            自动扫描条码报警,
            初测位测试AC通电,
            初测位轨道治具结果,
            初测位测试结果,
            进老化前检查位结果,
            发送自动扫条码位置号,
            扫描条码1异常报警,
            扫描条码2异常报警
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
        /// 默认机种路径
        /// </summary>
        private string _defaultModelPath = Application.StartupPath + "\\Model\\Demo.pwr";
        /// <summary>
        /// 机种参数
        /// </summary>
        private CModelPara _runModel = new CModelPara();
        /// <summary>
        /// PLC设备
        /// </summary>
        private CPLCCOM _devPLC = null;
        /// <summary>
        /// 读卡器设备
        /// </summary>
        private CCARDCom _devIDCard = null;
        /// <summary>
        /// 电子负载
        /// </summary>
        private CELCom _devELoad = null;
        /// <summary>
        /// 快充主控板
        /// </summary>
        private CFMBCom _devFCMB = null;
        /// <summary>
        /// 条码枪
        /// </summary>
        private CBarCOM[] _devSn = new CBarCOM[2];
        /// <summary>
        /// AC电表
        /// </summary>
        private CMeterCom _devMeter = null;
        /// <summary>
        /// PLC线程
        /// </summary>
        private CPLCThread _threadPLC = null;
        /// <summary>
        /// PLC重连接次数
        /// </summary>
        private int _conToPLCAgain = 0;
        /// <summary>
        /// 工位信息
        /// </summary>
        private CStatInfo _statInfo = null;
        /// <summary>
        /// 人工扫描位
        /// </summary>
        private CStatHub _statManualScan = null;
        /// <summary>
        /// 自动扫描条码工位
        /// </summary>
        private CStatHub _statAutoScan = null;
        /// <summary>
        /// 通电测试工位
        /// </summary>
        private CStat _statPreTest = null;
        /// <summary>
        /// 进老化工位
        /// </summary>
        private CStatHub _statInBI = null;
        /// <summary>
        /// 启动触发条码枪
        /// </summary>
        private bool _trigerScanner = false;
        /// <summary>
        /// 监控时间(ms)
        /// </summary>
        private Stopwatch _monWatcher = new Stopwatch();
        #endregion

        #region 面板方法
        /// <summary>
        /// 选择机种
        /// </summary>
        /// <param name="modelName"></param>
        public void OnFrmMainSelectModel(string modelName)
        {
            try
            {
                _defaultModelPath = modelName;

                LoadModelPara();

                _statInfo.SetNewLoad = true;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 设置空治具
        /// </summary>
        /// <param name="emptyFixture"></param>
        public void OnFrmMainSetEmptyFixture(int emptyFixture)
        {
            try
            {
                _statInfo.EmptyFixture = emptyFixture;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 清除治具使用次数
        /// </summary>
        /// <param name="idCard"></param>
        public void OnFrmMainClrFixtureUseNum(string idCard)
        {

        }
        /// <summary>
        /// 清除治具不良数
        /// </summary>
        /// <param name="idCard"></param>
        public void OnFrmMainClrFixtureFailNum(string idCard)
        {

        }
        /// <summary>
        /// 勾选扫描条码
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="value"></param>
        public void OnFrmMainCheckSn(int idNo, bool value)
        {
            try
            {
                _statInfo.CheckSn[idNo] = value;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 不良确认
        /// </summary>
        public void OnFrmMainManualConfirmFail()
        {
            _statInfo.DisFail = false;
        }
        /// <summary>
        /// 复位AC
        /// </summary>
        public void OnFrmMainManualReset()
        {
            _statInfo.Reset = 1;
        }
        /// <summary>
        /// 清除测试工位计数
        /// </summary>
        public void OnFrmMainClrStatUseNum()
        {
            _statInfo.StatUseNum = 0;

            CIniFile.WriteToIni("Parameter", "StatUseNum", _statInfo.StatUseNum.ToString(), CGlobalPara.IniFile);

            UIStatDataArgs.UseNum = _statInfo.StatUseNum;

            UIStatDataArgs.DoRun = EUIStatData.使用次数;

            OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));
        }
        /// <summary>
        /// 清除产能
        /// </summary>
        public void OnFrmMainClrYieldNum()
        {
            _statInfo.TTNum = 0;

            _statInfo.FailNum = 0;

            CIniFile.WriteToIni("Parameter", "TTNum", _statInfo.TTNum.ToString(), CGlobalPara.IniFile);

            CIniFile.WriteToIni("Parameter", "FailNum", _statInfo.FailNum.ToString(), CGlobalPara.IniFile);

            UIStatDataArgs.TTNum = _statInfo.TTNum;

            UIStatDataArgs.FailNum = _statInfo.FailNum;

            UIStatDataArgs.DoRun = EUIStatData.产能统计;

            OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));
        }
        /// <summary>
        /// 清除工位产能
        /// </summary>
        public void ClrStatFailNum()
        {
            _statInfo.StatFailNum = 0;

            CIniFile.WriteToIni("Parameter", "StatFailNum", _statInfo.StatFailNum.ToString(), CGlobalPara.IniFile);

            UIStatDataArgs.ConFailNum = _statInfo.StatFailNum;

            UIStatDataArgs.DoRun = EUIStatData.产能统计;

            OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));
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
                    _devPLC = new CPLCCOM(EPlcType.Inovance_TCP, 0, "<" + CLanguage.Lan("通电测试位") + "PLC>");

                    if (!_devPLC.Open(CGlobalPara.SysPara.Dev.PlcIP, out er, "502"))
                    {
                        Log(_devPLC.ToString() + "[" + CGlobalPara.SysPara.Dev.PlcIP + "]" + CLanguage.Lan("连接通信错误:") + er, udcRunLog.ELog.NG);

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
                    Log(_devPLC.ToString() + "[" + CGlobalPara.SysPara.Dev.PlcIP + "]" + CLanguage.Lan("断开连接"), udcRunLog.ELog.Action);

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
                        Log(_devIDCard.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.IdCom + "]" +
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
                                Log(_devIDCard.ToString() + CLanguage.Lan("读取地址") + "[" + (1 + i).ToString("D2") + "]" +
                                                            CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                                _devIDCard.Close();

                                _devIDCard = null;

                                return false;
                            }
                        }
                    }

                    Log(_devIDCard.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.IdCom + "]" +
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
        /// 打开电子负载
        /// </summary>
        /// <returns></returns>
        private bool OpenELoadMon()
        {
            try
            {
                string er = string.Empty;

                if (_devELoad == null)
                {
                    _devELoad = new CELCom(DEV.ELOAD.EType.EL_100_04, 0, CLanguage.Lan("电子负载"));

                    if (!_devELoad.Open(CGlobalPara.SysPara.Dev.EloadCom, out er))
                    {
                        Log(_devELoad.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.EloadCom + "]" +
                                                   CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                        _devELoad = null;

                        return false;
                    }

                    int AddrMax = CYOHOOApp.SlotMax / _devELoad.maxCH;

                    CEL_ReadData rData = new CEL_ReadData();

                    for (int i = 0; i < AddrMax; i++)
                    {
                        Thread.Sleep(20);

                        if (!_devELoad.ReadELData(1 + i, rData, out er))
                        {
                            Thread.Sleep(20);

                            if (!_devELoad.ReadELData(1 + i, rData, out er))
                            {
                                Log(_devELoad.ToString() + CLanguage.Lan("读取地址") + "[" + (i + 1).ToString("D2") + "]" +
                                                           CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                                _devELoad.Close();

                                _devELoad = null;

                                return false;
                            }
                        }
                    }

                    Log(_devELoad.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.EloadCom + "]" +
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
        /// 关闭控制板
        /// </summary>
        private void CloseELoadMon()
        {
            try
            {
                if (_devELoad != null)
                {
                    Log(_devELoad.ToString() + CLanguage.Lan("关闭串口") + "[" + CGlobalPara.SysPara.Dev.EloadCom + "]", udcRunLog.ELog.Action);

                    _devELoad.Close();

                    _devELoad = null;
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
        private bool OpenQCVMon()
        {
            try
            {
                string er = string.Empty;

                if (_devFCMB != null)
                {
                    _devFCMB.Close();
                    _devFCMB = null;
                }

                _devFCMB = new CFMBCom(DEV.FCMB.EType.FMB_V1, 0, "<" + CLanguage.Lan("快充主控板") + ">");

                if (!_devFCMB.Open(CGlobalPara.SysPara.Dev.FCMBCom, out er))
                {
                    Log(_devFCMB.ToString() + CLanguage.Lan("打开串口") + CGlobalPara.SysPara.Dev.FCMBCom + CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                    _devFCMB = null;

                    return false;
                }

                string rVal = string.Empty;

                if (!_devFCMB.ReadVersion(1, out rVal, out er))
                {
                    Thread.Sleep(50);

                    if (!_devFCMB.ReadVersion(1, out rVal, out er))
                    {
                        Log(_devFCMB.ToString() + CLanguage.Lan("读取地址") + "【01】" + CLanguage.Lan("通信异常:") + er, udcRunLog.ELog.NG);

                        return false;
                    }
                }

                Log(_devFCMB.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.FCMBCom + "]" +
                                          CLanguage.Lan("通信正常"), udcRunLog.ELog.Action);

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
                if (_devFCMB != null)
                {
                    Log(_devFCMB.ToString() + CLanguage.Lan("关闭串口") + "[" + CGlobalPara.SysPara.Dev.FCMBCom + "]", udcRunLog.ELog.Action);

                    _devFCMB.Close();

                    _devFCMB = null;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开条码枪
        /// </summary>
        /// <returns></returns>
        private bool OpenSnCom()
        {
            try
            {
                string er = string.Empty;

                bool recvThreshold = false;

                if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工串口模式)
                {
                    recvThreshold = true;
                }

                bool chkFlag = true;

                for (int idNo = 0; idNo < _devSn.Length; idNo++)
                {
                    if (_devSn[idNo] == null)
                    {
                        EBarType barType = EBarType.SR700;

                        if (Enum.IsDefined(typeof(EBarType), CGlobalPara.SysPara.Dev.SnMode))
                        {
                            barType = (EBarType)Enum.Parse(typeof(EBarType), CGlobalPara.SysPara.Dev.SnMode);
                        }
                        _devSn[idNo] = new CBarCOM(barType, idNo, CLanguage.Lan("条码枪") + (idNo + 1).ToString() + "【" + CGlobalPara.SysPara.Dev.SnMode + "】");

                        if (!_devSn[idNo].Open(CGlobalPara.SysPara.Dev.SnCom[idNo], out er, CGlobalPara.SysPara.Dev.SnBaud, recvThreshold))
                        {
                            Log(_devSn[idNo].ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.SnCom[idNo] + "]" +
                                                           CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                            _devSn[idNo] = null;

                            chkFlag = false;

                            continue;
                        }

                        _devSn[idNo].Triger_End(out er);

                        _devSn[idNo].OnRecved += new OnRecvHandler(OnSnComRecved);

                        Log(_devSn[idNo].ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.SnCom[idNo] + "]" +
                                                       CLanguage.Lan("正常"), udcRunLog.ELog.Action);

                        if (recvThreshold)
                        {
                            _devSn[idNo].Triger_End(out er);
                        }
                    }
                }

                return chkFlag;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 关闭条码枪
        /// </summary>
        private void CloseSnCom()
        {
            try
            {
                for (int idNo = 0; idNo < _devSn.Length; idNo++)
                {
                    if (_devSn[idNo] != null)
                    {
                        Log(_devSn[idNo].ToString() + CLanguage.Lan("关闭串口") + "[" + CGlobalPara.SysPara.Dev.SnCom[idNo] + "]", udcRunLog.ELog.Action);

                        _devSn[idNo].OnRecved -= new OnRecvHandler(OnSnComRecved);

                        _devSn[idNo].Close();

                        _devSn[idNo] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开AC电表
        /// </summary>
        private bool OpenACMuti()
        {
            try
            {
                string er = string.Empty;

                if (_devMeter == null)
                {
                    _devMeter = new CMeterCom(DEV.Meter.EType.PRU80_R1_2A_AC, 0, "<" + CLanguage.Lan("AC电压表") + ">");

                    if (!_devMeter.Open(CGlobalPara.SysPara.Dev.MeterCom, out er))
                    {
                        Log(_devMeter.ToString() + CLanguage.Lan("打开串口") + CGlobalPara.SysPara.Dev.MeterCom +
                                                   CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                        _devMeter = null;

                        return false;
                    }

                    double acv = 0;

                    if (!_devMeter.ReadACV(0, out acv, out er))
                    {
                        Thread.Sleep(100);
                        if (!_devMeter.ReadACV(0, out acv, out er))
                        {
                            Log(_devMeter.ToString() + CLanguage.Lan("读取地址") + "[00]" + CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);

                            _devMeter.Close();

                            _devMeter = null;

                            return false;
                        }
                    }

                    Log(_devMeter.ToString() + CLanguage.Lan("打开串口") + "[" + CGlobalPara.SysPara.Dev.MeterCom + "]" +
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
        /// 断开AC电表
        /// </summary>
        private void CloseACMuti()
        {
            try
            {
                if (_devMeter != null)
                {
                    Log(_devMeter.ToString() + CLanguage.Lan("关闭串口") + "[" + CGlobalPara.SysPara.Dev.MeterCom + "]", udcRunLog.ELog.Action);

                    _devMeter.Close();

                    _devMeter = null;
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
                    Log(CLanguage.Lan("连接冠佳Web") + "【" + CYOHOOApp.UlrWeb + "】" +
                                                             CLanguage.Lan("错误:") + er, udcRunLog.ELog.NG);
                    return false;
                }

                Log(CLanguage.Lan("连接冠佳Web") + "【" + CYOHOOApp.UlrWeb + "】" + CLanguage.Lan("正常"), udcRunLog.ELog.Action);

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
                    Log(GJ.COM.CLanguage.Lan("PLC监控线程重新启动."), udcRunLog.ELog.Action);
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

                        if (!CheckMESStatus())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckACVolt())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckACONOFF())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckOverDueTimes())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckSetELoad())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckManualScanReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckAutoScanReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckTestHubReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckTestStatReady())
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

                        //if (!CheckInBIStatReady())
                        //{
                        //    Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                        //    continue;
                        //}
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

                Log(GJ.COM.CLanguage.Lan("主监控线程销毁"), udcRunLog.ELog.NG);
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

                if (!_monWatcher.IsRunning)
                {
                    _monWatcher.Start();
                }
                else
                {
                    delayTimes = (int)_monWatcher.ElapsedMilliseconds;
                    _monWatcher.Stop();
                }

                UIStatDataArgs.MonTime = _monWatcher.ElapsedMilliseconds;
                UIStatDataArgs.DoRun = EUIStatData.监控时间;
                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));
                _monWatcher.Restart();

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

                if (_threadPLC.rREG_Val(EPLCINP.PLC自动运行.ToString()) != CPLCPara.ON)
                {
                    if (_conToPLCAgain < counT2)
                        _conToPLCAgain++;
                    else
                    {
                        _conToPLCAgain = 0;
                        Log(CLanguage.Lan("线体PLC未启动运行,请启动PLC."), udcRunLog.ELog.NG);
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
                        Log(CLanguage.Lan("线体PLC设备报警,请检查PLC触摸屏报警信息."), udcRunLog.ELog.NG);
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
        /// 检查状态
        /// </summary>
        /// <returns></returns>
        private bool CheckACVolt()
        {
            double acv = _runModel.OutPut.ACVolt;

            int result = 0;

            UIStatDataArgs.ACV = acv;

            UIStatDataArgs.ACFlag = result;

            try
            {
                string er = string.Empty;

                if (_devMeter == null)
                {
                    if (!OpenACMuti())
                        return false;
                }

                if (!_devMeter.ReadACV(0, out acv, out er))
                {
                    Thread.Sleep(200);

                    if (!_devMeter.ReadACV(0, out acv, out er))
                    {
                        Log(_devMeter.ToString() + CLanguage.Lan("通信异常:") + er, udcRunLog.ELog.NG);

                        return false;
                    }
                }

                if (acv < _runModel.OutPut.ACvMin || acv > _runModel.OutPut.ACvMax)
                {
                    result = 2;

                    UIStatDataArgs.ACV = acv;

                    UIStatDataArgs.ACFlag = result;

                    Log(CLanguage.Lan("当前输入电压") + "=" + acv.ToString() + "V," + CLanguage.Lan("超过设置范围,请检查."), udcRunLog.ELog.NG);

                    return false;
                }

                result = 1;

                UIStatDataArgs.ACV = acv;

                UIStatDataArgs.ACFlag = result;

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
            finally
            {
                UIStatDataArgs.DoRun = EUIStatData.设定电压;

                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));
            }
        }
        /// <summary>
        /// 检测AC ON/OFF
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckACONOFF()
        {
            try
            {
                string er = string.Empty;

                if (!CGlobalPara.SysPara.Para.ChkACON)
                    return true;

                List<int> X = null;

                if (!_devFCMB.ReadIO(1, out X, out er))
                {
                    Thread.Sleep(50);

                    if (!_devFCMB.ReadIO(1, out X, out er))
                    {
                        UIStatDataArgs.bAlarm = true;

                        UIStatDataArgs.AlarmInfo = CLanguage.Lan("快充板通信异常");

                        UIStatDataArgs.DoRun = EUIStatData.状态提示;

                        OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                        Log(_devFCMB.ToString() + CLanguage.Lan("通信异常:") + er, udcRunLog.ELog.NG);

                        return false;
                    }

                }

                if (X[(int)EFMB_rIO.AC同步信号] == 1)
                {
                    UIStatDataArgs.bAlarm = true;

                    UIStatDataArgs.AlarmInfo = CLanguage.Lan("请检查AC是否跳闸?");

                    UIStatDataArgs.DoRun = EUIStatData.状态提示;

                    OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                    Log(CLanguage.Lan("检查AC ON异常,请检查AC是否跳闸?"), udcRunLog.ELog.NG);

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
        /// 检查设备寿命是否次数
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckOverDueTimes()
        {
            try
            {
                string er = string.Empty;

                if (CGlobalPara.SysPara.Alarm.StatTimes > 0 && _statInfo.StatUseNum > CGlobalPara.SysPara.Alarm.StatTimes)
                {
                    er = _statPreTest.ToString() + CLanguage.Lan("工位使用寿命超过设置值") + "[" + CGlobalPara.SysPara.Alarm.StatTimes.ToString() + "]";
                    Log(er, udcRunLog.ELog.NG);
                    return false;
                }
                if (CGlobalPara.SysPara.Alarm.StatFailTimes > 0 && _statInfo.StatFailNum > CGlobalPara.SysPara.Alarm.StatFailTimes)
                {
                    er = _statPreTest.ToString() + CLanguage.Lan("工位连续不良") + "[" + _statInfo.StatFailNum.ToString() +
                                                   "]" + CLanguage.Lan("超过设置值") + "[" + CGlobalPara.SysPara.Alarm.StatFailTimes.ToString() + "]";
                    Log(er, udcRunLog.ELog.NG);
                    return false;
                }
                if (CGlobalPara.SysPara.Alarm.FixPassRate > 0)
                {
                    if (_statInfo.TTNum > 0)
                    {
                        double passRate = (double)(_statInfo.TTNum - _statInfo.FailNum) * 100 / (double)_statInfo.TTNum;

                        if (passRate < CGlobalPara.SysPara.Alarm.FixPassRate)
                        {
                            er = _statPreTest.ToString() + CLanguage.Lan("工位良率") + "[" + passRate.ToString() + "%]" +
                                                           CLanguage.Lan("低于设置值") + "[" +
                                                           CGlobalPara.SysPara.Alarm.FixPassRate.ToString() + "%]";
                            Log(er, udcRunLog.ELog.NG);

                            return false;
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
        /// 设置电子负载拉载电流
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckSetELoad()
        {
            try
            {
                if (!_statInfo.SetNewLoad)
                    return true;

                string er = string.Empty;

                //设置负载电流
                if (_threadPLC.wREG_Val(EPLCOUT.初测位测试AC通电.ToString()) == CPLCPara.ON)
                {
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试AC通电.ToString(), 0);
                    Thread.Sleep(2000);
                }
                if (CGlobalPara.SysPara.Para.ChkIdleLoad)
                {
                    SetLoadValue(CGlobalPara.SysPara.Para.IdleLoad);
                }
                else
                {
                    SetLoadValue(_runModel.OutPut.LoadSet);
                }

                //设置快充电压
                SetQCM();

                _statInfo.SetNewLoad = false;

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 人工扫描条码位
        /// </summary>
        /// <returns></returns>
        private bool CheckManualScanReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.人工扫条码位治具到位.ToString()) != CPLCPara.ON)
                {
                    if (_statManualScan.Para._cts != null)
                        _statManualScan.Para._cts.Cancel();

                    _statManualScan.Para.DoRun = ERUN.空闲;

                    _statManualScan.Para.Alarm = EAlarm.正常;

                    _statManualScan.Fixture.IdCard = string.Empty;

                    for (int i = 0; i < _statManualScan.Fixture.SerialNo.Count; i++)
                    {
                        _statManualScan.Fixture.SerialNo[i] = string.Empty;
                    }

                    if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工扫描模式 || CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工串口模式)
                    {
                        if (_threadPLC.rREG_Val(EPLCINP.人工扫条码位治具光电.ToString()) == CPLCPara.OFF)
                        {
                            //触发条码枪
                            if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工串口模式 && _trigerScanner)
                            {
                                string er = string.Empty;

                                for (int idNo = 0; idNo < _devSn.Length; idNo++)
                                {
                                    _devSn[idNo].Triger_End(out er);
                                }

                                _trigerScanner = false;
                            }

                            OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statManualScan));
                        }
                    }

                    if (_threadPLC.rREG_Val(EPLCINP.人工扫条码位治具光电.ToString()) == CPLCPara.OFF)
                    {
                        for (int idNo = 0; idNo < 2; idNo++)
                        {
                            if (_threadPLC.wREG_Val(OutPLC(EPLCOUT.扫描条码1异常报警, idNo)) == CPLCPara.ON)
                            {
                                _threadPLC.addREGWrite(OutPLC(EPLCOUT.扫描条码1异常报警, idNo), 0);
                            }
                        }
                    }

                    return true;
                }

                if (_statManualScan.Para.DoRun != ERUN.空闲)
                    return true;

                _statManualScan.Para.DoRun = ERUN.到位;

                _statManualScan.Para.Alarm = EAlarm.正常;

                _statManualScan.Para.AlarmInfo = string.Empty;

                Task.Factory.StartNew(() => OnManualScanTask());

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 自动扫描条码位
        /// </summary>
        /// <returns></returns>
        private bool CheckAutoScanReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.自动扫条码位治具到位.ToString()) != CPLCPara.ON)
                {
                    if (_statAutoScan.Para._cts != null)
                        _statAutoScan.Para._cts.Cancel();

                    _statAutoScan.Para.DoRun = ERUN.空闲;

                    _statAutoScan.Para.Alarm = EAlarm.正常;

                    _statAutoScan.Fixture.IdCard = string.Empty;

                    for (int i = 0; i < _statAutoScan.Fixture.SerialNo.Count; i++)
                    {
                        _statAutoScan.Fixture.SerialNo[i] = string.Empty;
                    }

                    if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.自动扫描模式)
                    {
                        if (_threadPLC.rREG_Val(EPLCINP.自动扫条码位治具光电.ToString()) == CPLCPara.OFF)
                        {
                            OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statAutoScan));
                        }

                        for (int idNo = 0; idNo < 2; idNo++)
                        {
                            if (_threadPLC.wREG_Val(OutPLC(EPLCOUT.扫描条码1异常报警, idNo)) == CPLCPara.ON)
                            {
                                _threadPLC.addREGWrite(OutPLC(EPLCOUT.扫描条码1异常报警, idNo), 0);
                            }
                        }
                    }

                    return true;
                }

                if (_statAutoScan.Para.DoRun != ERUN.空闲)
                    return true;

                _statAutoScan.Para.DoRun = ERUN.到位;

                _statAutoScan.Para.Alarm = EAlarm.正常;

                _statAutoScan.Para.AlarmInfo = string.Empty;

                Task.Factory.StartNew(() => OnAutoScanTask());

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 通电测试位
        /// </summary>
        /// <returns></returns>
        private bool CheckTestHubReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.初测位轨道治具到位.ToString()) != CPLCPara.ON)
                {
                    _statPreTest.hub.Para.DoRun = ERUN.空闲;

                    _statPreTest.hub.Para.Alarm = EAlarm.正常;

                    return true;
                }

                if (_statPreTest.hub.Para.DoRun != ERUN.空闲)
                    return true;

                _statPreTest.hub.Para.DoRun = ERUN.到位;

                _statPreTest.hub.Para.Alarm = EAlarm.正常;

                Task.Factory.StartNew(() => OnTestHubTask());

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 测试工位就绪
        /// </summary>
        /// <returns></returns>
        private bool CheckTestStatReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.初测位测试准备OK.ToString()) != CPLCPara.ON)
                {
                    _statPreTest.test.Para.DoRun = ERUN.空闲;

                    _statPreTest.test.Para.Alarm = EAlarm.正常;

                    _statPreTest.test.Para.AlarmInfo = string.Empty;


                    if (_threadPLC.rREG_Val(EPLCINP.初测位轨道治具光电.ToString()) == CPLCPara.OFF &&
                        _threadPLC.rREG_Val(EPLCINP.初测位测试准备OK.ToString()) != CPLCPara.ON)
                    {
                        if (_statPreTest.test.Para.ClrUI && _statPreTest.test.Para.Io_Watcher.ElapsedMilliseconds > CGlobalPara.C_CLEAR_UI_DELAY)
                        {
                            UIStatDataArgs.DoRun = EUIStatData.空闲;

                            UIStatDataArgs.bAlarm = false;

                            UIStatDataArgs.AlarmInfo = string.Empty;

                            OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                            OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                        }

                        if (_threadPLC.wREG_Val(EPLCOUT.初测位测试AC通电.ToString()) == CPLCPara.ON)
                        {
                            _threadPLC.addREGWrite(EPLCOUT.初测位测试AC通电.ToString(), 0);
                        }
                    }

                    if (!_statPreTest.test.Para.ClrUI)
                    {
                        _statPreTest.test.Para.ClrUI = true;

                        _statPreTest.test.Para.Io_Watcher.Restart();
                    }

                    return true;
                }

                if (_statPreTest.test.Para.DoRun != ERUN.空闲)
                    return true;

                _statPreTest.test.Para.ClrUI = false;

                _statPreTest.test.Para.DoRun = ERUN.到位;

                _statPreTest.test.Para.Alarm = EAlarm.正常;

                Task.Factory.StartNew(() => OnTestStatTask());

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 进老化检查位
        /// </summary>
        /// <returns></returns>
        private bool CheckInBIStatReady()
        {
            try
            {
                if (_threadPLC.rREG_Val(EPLCINP.进老化前检查位治具到位.ToString()) != CPLCPara.ON)
                {
                    _statInBI.Para.DoRun = ERUN.空闲;

                    _statInBI.Para.Alarm = EAlarm.正常;

                    if (_threadPLC.rREG_Val(EPLCINP.进老化前检查位治具光电.ToString()) == CPLCPara.OFF)
                    {
                        OnUIInBIArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statInBI));
                    }

                    return true;
                }

                if (_statInBI.Para.DoRun != ERUN.空闲)
                    return true;

                _statInBI.Para.DoRun = ERUN.到位;

                _statInBI.Para.Alarm = EAlarm.正常;

                Task.Factory.StartNew(() => OnInBIStatTask());

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        #endregion

        #region 人工扫描条码绑定任务
        /// <summary>
        /// 人工扫描条码绑定任务
        /// </summary>
        private void OnManualScanTask()
        {
            try
            {
                _statManualScan.Para._cts = new CancellationTokenSource();

                _statManualScan.Para.Watcher.Restart();

                while (true)
                {
                    if (_statManualScan.Para._cts.IsCancellationRequested)
                        return;

                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    switch (_statManualScan.Para.DoRun)
                    {
                        case ERUN.到位:
                            OnManualScanReady();
                            break;
                        case ERUN.绑定:
                            OnMainScanBanding();
                            break;
                        case ERUN.等待:
                            break;
                        case ERUN.完成:
                            OnManualScanBandOver();
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

                _statManualScan.Para._cts = null;

                _statManualScan.Para.Watcher.Stop();

                Log(_statManualScan.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statManualScan.Para.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);

            }
        }
        /// <summary>
        /// 人工扫描条码位治具到位
        /// </summary>
        private void OnManualScanReady()
        {
            try
            {
                string er = string.Empty;

                if (!OnScan_ReadIdCard(_statManualScan))
                {
                    _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);

                    return;
                }

                _statManualScan.Para.Alarm = EAlarm.正常;

                _statManualScan.Para.AlarmInfo = CLanguage.Lan("读取治具OK,等待检查.");

                _statManualScan.Para.DoRun = ERUN.就绪;

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statManualScan));

                Log(_statManualScan.ToString() + CLanguage.Lan("治具到位,读取治具ID") + "【" + _statManualScan.Fixture.IdCard +
                                                            "】" + "OK," + CLanguage.Lan("等待治具检查"), udcRunLog.ELog.Action);

                //检查治具状态

                bool bSample = false;

                if (!OnScan_CheckFixture(_statManualScan, out bSample))
                {
                    _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }
                if (bSample)
                {
                    _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                    Log(_statManualScan.ToString() + CLanguage.Lan("治具ID") + "【" + _statManualScan.Fixture.IdCard + "】" +
                                                   CLanguage.Lan("为点检治具,直接过站."), udcRunLog.ELog.Action);
                    return;
                }

                _statManualScan.Para.Alarm = EAlarm.正常;

                _statManualScan.Para.AlarmInfo = string.Empty;

                _statManualScan.Para.DoRun = ERUN.绑定;

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statManualScan));

                Log(_statManualScan.ToString() + CLanguage.Lan("治具ID") + "【" + _statManualScan.Fixture.IdCard + "】" +
                                               CLanguage.Lan("检查就绪,等待绑定条码."), udcRunLog.ELog.Action);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 人工绑定条码
        /// </summary>
        private void OnMainScanBanding()
        {
            try
            {
                //设置为空治具直接过站
                if (_statInfo.EmptyFixture == 1)
                {
                    if (!OnScan_EmptyFixtureGoBy(_statManualScan))
                        _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    else
                        _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                    return;
                }

                if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.无条码模式) //自动生成条码
                {
                    if (!OnScan_AutoBandingSn(_statManualScan))
                        _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    else
                        _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                    return;
                }

                if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.自动扫描模式)
                {
                    _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                    _statManualScan.Para.DoRun = ERUN.正常治具过站;
                    Log(_statManualScan.ToString() + CLanguage.Lan("设置为") + "【" + CGlobalPara.SysPara.Mes.SnMode.ToString() + "】," +
                                                     CLanguage.Lan("直接流入下一工位."), udcRunLog.ELog.Action);
                    return;
                }

                for (int i = 0; i < _statAutoScan.Fixture.SerialNo.Count; i++)
                {
                    _statAutoScan.Fixture.SerialNo[i] = string.Empty;
                }

                //等待人工扫描条码
                for (int i = 0; i < _statManualScan.Fixture.SerialNo.Count; i++)
                {
                    _statManualScan.Fixture.SerialNo[i] = string.Empty;
                }

                _statManualScan.Para.CurSnNo[0] = -1;

                _statManualScan.Para.CurSnNo[1] = -1;

                _statManualScan.Para.DoRun = ERUN.等待;

                _statManualScan.Para.Alarm = EAlarm.正常;

                _statManualScan.Para.AlarmInfo = CLanguage.Lan("治具到位,请扫描产品条码.");

                //触发条码枪
                if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工串口模式)
                {
                    string er = string.Empty;

                    for (int idNo = 0; idNo < _devSn.Length; idNo++)
                    {
                        _statManualScan.Para.CurSeriaoNo[idNo] = string.Empty;

                        _devSn[idNo].Triger_Start(out er);

                    }

                    _trigerScanner = true;
                }

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statManualScan));

                Log(_statManualScan.ToString() + CLanguage.Lan("治具ID") + "【" + _statManualScan.Fixture.IdCard + "】" +
                                                 CLanguage.Lan("就绪,等待扫描产品条码."), udcRunLog.ELog.Action);
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 人工扫描条码绑定条码
        /// </summary>
        private void OnManualScanBandOver()
        {
            try
            {
                if (!OnScan_BandSnToFxiture(_statManualScan))
                {
                    _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                }
                else
                {
                    _threadPLC.addREGWrite(EPLCOUT.人工扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                }

                //触发条码枪
                if (CGlobalPara.SysPara.Mes.SnMode == ESnMode.人工串口模式)
                {
                    string er = string.Empty;

                    for (int idNo = 0; idNo < _devSn.Length; idNo++)
                    {
                        _devSn[idNo].Triger_End(out er);
                    }

                    _trigerScanner = false;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 人工扫描文本框接收条码
        /// </summary>
        /// <param name="serialNo"></param>
        public void OnSnKeyRecv(string serialNo)
        {
            try
            {
                if (CGlobalPara.SysPara.Mes.SnMode != ESnMode.人工扫描模式)
                    return;

                if (_statManualScan.Para.DoRun != ERUN.等待)
                    return;

                string er = string.Empty;

                if (serialNo.Length < 3)
                {
                    UIScanSnArgs.DoRun = EUIScanSn.文本聚焦;

                    UIScanSnArgs.bAlarm = true;

                    UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码长度") + "<3";

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    _threadPLC.addREGWrite(EPLCOUT.自动扫描条码报警.ToString(), CPLCPara.ON);

                    return;
                }

                if (_statManualScan.Para.CurSnNo[0] == -1)
                    _statManualScan.Para.CurSnNo[0] = 0;

                int curUUTNo = OnSnTextIsComplete();

                if (curUUTNo == -1)
                {
                    _statManualScan.Para.DoRun = ERUN.完成;

                    UIScanSnArgs.DoRun = EUIScanSn.绑定完毕;

                    UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码结束,等待过站.");

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    return;
                }

                if (!OnScan_CheckSn(0, serialNo, _statManualScan.Fixture.SerialNo, out er))
                {
                    UIScanSnArgs.SlotNo = curUUTNo;

                    UIScanSnArgs.SerialNo = serialNo;

                    UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                    UIScanSnArgs.bAlarm = true;

                    UIScanSnArgs.AlarmInfo = er;

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    UIScanSnArgs.DoRun = EUIScanSn.文本聚焦;

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    Log(_statManualScan.ToString() + CLanguage.Lan("位置") + "【" + (curUUTNo + 1).ToString() + "】" +
                                                     CLanguage.Lan("条码") + "[" + serialNo + "]:" + er, udcRunLog.ELog.NG);

                    _threadPLC.addREGWrite(EPLCOUT.自动扫描条码报警.ToString(), CPLCPara.ON);

                    return;
                }

                _statManualScan.Fixture.SerialNo[curUUTNo] = serialNo;

                UIScanSnArgs.SlotNo = curUUTNo;

                UIScanSnArgs.SerialNo = serialNo;

                UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                UIScanSnArgs.bAlarm = false;

                UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码OK,等待下一个扫描.");

                OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                UIScanSnArgs.DoRun = EUIScanSn.文本聚焦;

                OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                Log(_statManualScan.ToString() + CLanguage.Lan("位置") + "【" + (curUUTNo + 1).ToString() + "】" +
                                                 CLanguage.Lan("条码") + "[" + serialNo + "]:" + CLanguage.Lan("扫描") + "OK", udcRunLog.ELog.Action);

                _statManualScan.Para.CurSnNo[0]++;

                curUUTNo = OnSnTextIsComplete();

                if (curUUTNo == -1)
                {
                    _statManualScan.Para.DoRun = ERUN.完成;

                    UIScanSnArgs.DoRun = EUIScanSn.绑定完毕;

                    UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码结束,等待过站.");

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 检查文本输入扫描是否结束?
        /// </summary>
        /// <returns></returns>
        private int OnSnTextIsComplete()
        {
            int curUUTNo = -1;

            for (int i = _statManualScan.Para.CurSnNo[0]; i < _statManualScan.Fixture.SerialNo.Count; i++)
            {
                if (_statInfo.CheckSn[i] && _statManualScan.Fixture.SerialNo[i] == string.Empty)
                {
                    curUUTNo = i;
                    break;
                }
            }

            return curUUTNo;
        }
        /// <summary>
        /// 人工扫描条码枪中断接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSnComRecved(object sender, CRecvArgs e)
        {
            try
            {
                if (CGlobalPara.SysPara.Mes.SnMode != ESnMode.人工串口模式)
                    return;

                if (_statManualScan.Para.DoRun != ERUN.等待)
                    return;

                string Sn = e.recvData.Replace("\r", "");

                Sn = Sn.Replace("\n", "");

                //防止重复扫描条码触发
                if (_statManualScan.Para.CurSeriaoNo[e.idNo] == Sn)
                    return;

                _statManualScan.Para.CurSeriaoNo[e.idNo] = Sn;

                string er = string.Empty;

                int maxScan = CYOHOOApp.SlotMax / 2;

                if (_statManualScan.Para.CurSnNo[e.idNo] == -1)
                    _statManualScan.Para.CurSnNo[e.idNo] = 0;

                int curUUTNo = -1;

                if (OnSnComIsComplete(e.idNo, out curUUTNo))
                {
                    _statManualScan.Para.DoRun = ERUN.完成;

                    UIScanSnArgs.DoRun = EUIScanSn.绑定完毕;

                    UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码结束,等待过站.");

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    return;
                }

                if (curUUTNo == -1)
                    return;

                int uutNo = _statManualScan.Base.SnBarNo[e.idNo][curUUTNo];

                if (!OnScan_CheckSn(e.idNo, Sn, _statManualScan.Fixture.SerialNo, out er))
                {
                    UIScanSnArgs.SlotNo = uutNo;

                    UIScanSnArgs.SerialNo = Sn;

                    UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                    UIScanSnArgs.bAlarm = true;

                    UIScanSnArgs.AlarmInfo = er;

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    Log(_statManualScan.ToString() + CLanguage.Lan("位置") + "【" + (uutNo + 1).ToString() +
                              "】" + CLanguage.Lan("条码") + "[" + Sn + "]:" + er, udcRunLog.ELog.NG);

                    _threadPLC.addREGWrite(OutPLC(EPLCOUT.扫描条码1异常报警, e.idNo), 1);

                    return;
                }

                _statManualScan.Fixture.SerialNo[uutNo] = Sn;

                Log(_statManualScan.ToString() + CLanguage.Lan("位置") + "【" + (uutNo + 1).ToString() +
                    "】" + CLanguage.Lan("条码") + "[" + Sn + "]:" + CLanguage.Lan("扫描") + "OK", udcRunLog.ELog.Action);

                UIScanSnArgs.SlotNo = uutNo;

                UIScanSnArgs.SerialNo = Sn;

                UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                UIScanSnArgs.bAlarm = false;

                UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码OK,等待下一个扫描.");

                OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                _statManualScan.Para.CurSnNo[e.idNo]++;

                _threadPLC.addREGWrite(OutPLC(EPLCOUT.扫描条码1异常报警, e.idNo), 0);

                //再次判断
                if (OnSnComIsComplete(e.idNo, out curUUTNo))
                {
                    _statManualScan.Para.DoRun = ERUN.完成;

                    UIScanSnArgs.DoRun = EUIScanSn.绑定完毕;

                    UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描条码结束,等待过站.");

                    OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(_idNo, _name, UIScanSnArgs));

                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 检查人工串口扫描条码是否结束?
        /// </summary>
        /// <returns></returns>
        private bool OnSnComIsComplete(int idNo, out int curUUTNo)
        {

            curUUTNo = -1;

            int maxScan = CYOHOOApp.SlotMax / 2;

            for (int i = _statManualScan.Para.CurSnNo[idNo]; i < maxScan; i++)
            {
                int uutNo = idNo * maxScan + i;

                if (_statInfo.CheckSn[uutNo])
                {
                    if (curUUTNo == -1)
                        curUUTNo = i;
                }
            }

            if (curUUTNo == -1)
            {
                _statManualScan.Para.CurSnNo[idNo] = maxScan;
            }

            bool scanEnd = true;

            for (int z = 0; z < 2; z++)
            {
                for (int i = 0; i < maxScan; i++)
                {
                    int uutNo = z * maxScan + i;

                    if (_statInfo.CheckSn[uutNo] && _statManualScan.Fixture.SerialNo[uutNo] == string.Empty)
                    {
                        scanEnd = false;
                    }
                }
            }

            if (scanEnd)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 自动扫描条码绑定位任务
        /// <summary>
        /// 自动扫描条码绑定任务
        /// </summary>
        private void OnAutoScanTask()
        {
            try
            {
                _statAutoScan.Para._cts = new CancellationTokenSource();

                _statAutoScan.Para.Watcher.Restart();

                while (true)
                {
                    if (_statAutoScan.Para._cts.IsCancellationRequested)
                        return;

                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    switch (_statAutoScan.Para.DoRun)
                    {
                        case ERUN.到位:
                            OnAutoScanReady();
                            break;
                        case ERUN.绑定:
                            OnAutoScanBanding();
                            break;
                        case ERUN.等待:
                            break;
                        case ERUN.完成:
                            OnAutoSnBandOver();
                            break;
                        case ERUN.异常报警:
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
                _statAutoScan.Para._cts = null;

                _statAutoScan.Para.Watcher.Stop();

                Log(_statAutoScan.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statAutoScan.Para.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 人工位-扫描条码位治具到位
        /// </summary>
        private void OnAutoScanReady()
        {
            try
            {
                string er = string.Empty;

                //读取治具ID
                if (!OnScan_ReadIdCard(_statAutoScan))
                {
                    _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                Log(_statAutoScan.ToString() + CLanguage.Lan("治具到位,读取治具ID") + "【" + _statAutoScan.Fixture.IdCard + "】OK," +
                                               CLanguage.Lan("等待治具检查."), udcRunLog.ELog.Action);

                _statAutoScan.Para.DoRun = ERUN.就绪;

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statAutoScan));

                //检查治具状态
                bool bSample = false;

                if (!OnScan_CheckFixture(_statAutoScan, out bSample))
                {
                    _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                if (bSample)
                {
                    _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                    Log(_statAutoScan.ToString() + CLanguage.Lan("治具ID") + "【" + _statAutoScan.Fixture.IdCard + "】" +
                                                   CLanguage.Lan("为点检治具,直接过站."), udcRunLog.ELog.Action);
                    return;
                }

                _statAutoScan.Para.DoRun = ERUN.绑定;

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statAutoScan));

                Log(_statAutoScan.ToString() + CLanguage.Lan("治具ID") + "【" + _statAutoScan.Fixture.IdCard + "】" +
                                               CLanguage.Lan("检查就绪,等待绑定条码."), udcRunLog.ELog.Action);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 人工位绑定
        /// </summary>
        private void OnAutoScanBanding()
        {
            try
            {
                if (CGlobalPara.SysPara.Mes.SnMode != ESnMode.自动扫描模式)
                {
                    _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);

                    _statAutoScan.Para.DoRun = ERUN.正常治具过站;

                    Log(_statAutoScan.ToString() + CLanguage.Lan("设置为") + "【" + CGlobalPara.SysPara.Mes.SnMode.ToString() + "】," +
                                                   CLanguage.Lan("直接流入下一工位."), udcRunLog.ELog.Action);

                    return;
                }

                for (int i = 0; i < _statAutoScan.Fixture.SerialNo.Count; i++)
                {
                    _statAutoScan.Fixture.SerialNo[i] = string.Empty;
                }

                //设置为空治具直接过站
                if (_statInfo.EmptyFixture == 1)
                {
                    if (!OnScan_EmptyFixtureGoBy(_statAutoScan))
                        _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    else
                        _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                    return;
                }

                _statAutoScan.Para.CurSnNo[0] = -1;

                _statAutoScan.Para.Alarm = EAlarm.正常;

                _statAutoScan.Para.AlarmInfo = CLanguage.Lan("治具就绪,等待扫描产品条码");

                _statAutoScan.Para.DoRun = ERUN.等待;

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statAutoScan));

                Task.Factory.StartNew(() => OnScanTask());

                Log(_statAutoScan.ToString() + CLanguage.Lan("治具ID") + "【" + _statAutoScan.Fixture.IdCard + "】" +
                                               CLanguage.Lan("就绪,等待扫描产品条码."), udcRunLog.ELog.OK);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 无条码绑定条码
        /// </summary>
        private void OnAutoSnBandOver()
        {
            try
            {
                if (!OnScan_BandSnToFxiture(_statAutoScan))
                {
                    _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                }
                else
                {
                    _threadPLC.addREGWrite(EPLCOUT.自动扫条码位治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 自动扫描条码任务
        /// </summary>
        private void OnScanTask()
        {
            Stopwatch watcher = new Stopwatch();

            watcher.Start();

            try
            {
                Log(_statAutoScan.ToString() + CLanguage.Lan("自动扫描条码任务开始."), udcRunLog.ELog.Action);

                while (true)
                {

                    if (_statAutoScan.Para._cts.IsCancellationRequested)
                        return;

                    if (_statAutoScan.Para.DoRun == ERUN.空闲)
                        return;

                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statAutoScan.Para.DoRun == ERUN.异常报警)
                    {
                        if (_threadPLC.wREG_Val(EPLCOUT.自动扫描条码报警.ToString()) == 0)
                        {
                            _statAutoScan.Para.DoRun = ERUN.等待;
                        }
                        continue;
                    }

                    //启动扫描位置01
                    if (_statAutoScan.Para.CurSnNo[0] == -1)
                    {
                        _statAutoScan.Para.CurSnNo[0] = 0;
                        _threadPLC.addREGWrite(EPLCOUT.发送自动扫条码位置号.ToString(), _statAutoScan.Para.CurSnNo[0] + 1);
                        continue;
                    }

                    //等待扫描位置就绪
                    if (_threadPLC.rREG_Val(EPLCINP.自动扫条码位置号.ToString()) != _statAutoScan.Para.CurSnNo[0] + 1)
                        continue;

                    //启动2个条码枪读取条码       
                    bool bAlarm = false;

                    string er = string.Empty;

                    List<Task<string>> barCode = new List<Task<string>>();

                    //启动条码枪扫描
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        int taskNo = idNo;
                        int slot = _statAutoScan.Base.SnBarNo[idNo][_statAutoScan.Para.CurSnNo[0]];
                        _statAutoScan.Fixture.SerialNo[slot] = string.Empty;
                        barCode.Add(Task.Factory.StartNew(() => ReadBarCode(taskNo)));
                    }

                    //等待扫描结束?
                    Task.WaitAll(barCode.ToArray());

                    //检查条码是否OK?
                    for (int idNo = 0; idNo < 2; idNo++)
                    {
                        int slot = _statAutoScan.Base.SnBarNo[idNo][_statAutoScan.Para.CurSnNo[0]];

                        if (_statInfo.CheckSn[slot])
                        {
                            string serialNo = barCode[idNo].Result;

                            if (serialNo == string.Empty)
                            {
                                UIScanSnArgs.SlotNo = slot;

                                UIScanSnArgs.SerialNo = serialNo;

                                UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                                UIScanSnArgs.bAlarm = true;

                                UIScanSnArgs.AlarmInfo = CLanguage.Lan("扫描不到条码,请检查.");

                                OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(idNo, _name, UIScanSnArgs));

                                Log(_statAutoScan.ToString() + CLanguage.Lan("位置") + "【" + (slot + 1).ToString() + "】" +
                                                               CLanguage.Lan("扫描不到条码,请检查."), udcRunLog.ELog.NG);

                                bAlarm = true;

                                continue;
                            }

                            if (!OnScan_CheckSn(idNo, serialNo, _statAutoScan.Fixture.SerialNo, out er))
                            {
                                UIScanSnArgs.SlotNo = slot;

                                UIScanSnArgs.SerialNo = serialNo;

                                UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                                UIScanSnArgs.bAlarm = true;

                                UIScanSnArgs.AlarmInfo = er;

                                OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(idNo, _name, UIScanSnArgs));

                                Log(_statAutoScan.ToString() + CLanguage.Lan("位置") + "【" + (slot + 1).ToString() + "】" + CLanguage.Lan("条码") +
                                                              "[" + serialNo + "]:" + er, udcRunLog.ELog.NG);

                                bAlarm = true;

                                continue;
                            }

                            _statAutoScan.Fixture.SerialNo[slot] = serialNo;

                            UIScanSnArgs.SlotNo = slot;

                            UIScanSnArgs.SerialNo = serialNo;

                            UIScanSnArgs.DoRun = EUIScanSn.设置条码;

                            UIScanSnArgs.bAlarm = false;

                            UIScanSnArgs.AlarmInfo = CLanguage.Lan("位置") + "【" + (slot + 1).ToString() + "】" + CLanguage.Lan("条码扫描") + "OK";

                            OnUIScanSnArgs.OnEvented(new CUIUserArgs<CUIScanSnArgs>(idNo, _name, UIScanSnArgs));

                            Log(_statAutoScan.ToString() + CLanguage.Lan("位置") + "【" + (slot + 1).ToString() + "】" +
                                                            CLanguage.Lan("条码") + "[" + serialNo + "]:" + CLanguage.Lan("扫描") + "OK", udcRunLog.ELog.Action);
                        }
                    }

                    //扫描条码错误报警
                    if (bAlarm)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.自动扫描条码报警.ToString(), 1);

                        _statAutoScan.Para.DoRun = ERUN.异常报警;

                        Thread.Sleep(CGlobalPara.C_ALARM_DELAY);

                        continue;
                    }

                    _statAutoScan.Para.CurSnNo[0]++;

                    if (_statAutoScan.Para.CurSnNo[0] < CYOHOOApp.SlotMax / 2)
                    {
                        _threadPLC.addREGWrite(EPLCOUT.发送自动扫条码位置号.ToString(), _statAutoScan.Para.CurSnNo[0] + 1);
                        continue;
                    }

                    _statAutoScan.Para.DoRun = ERUN.完成;

                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                string waitTime = ((double)watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                Log(_statAutoScan.ToString() + CLanguage.Lan("自动扫描条码任务结束") + ":" + waitTime, udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 读条码枪任务
        /// </summary>
        /// <param name="idNo"></param>
        private string ReadBarCode(int idNo)
        {
            string serialNo = string.Empty;

            try
            {
                if (!_statInfo.CheckSn[idNo])
                {
                    return serialNo;
                }

                string er = string.Empty;

                for (int i = 0; i < CGlobalPara.SysPara.Para.ReadSnTimes + 1; i++)
                {
                    if (_devSn[idNo].Read(out serialNo, out er, CGlobalPara.SysPara.Mes.SnLen))
                        break;

                    Thread.Sleep(300);
                }

                return serialNo;
            }
            catch (Exception)
            {
                return serialNo;
            }
        }
        #endregion

        #region 人工扫描位与自动扫描位
        /// <summary>
        /// 读取治具ID卡
        /// </summary>
        /// <param name="hub"></param>
        /// <returns></returns>
        private bool OnScan_ReadIdCard(CStatHub hub)
        {
            try
            {
                string er = string.Empty;

                string rIdCard = string.Empty;

                if (!_devIDCard.ReadIdCard(hub.Base.IdCardAddr, out rIdCard, out er, CGlobalPara.SysPara.Para.IdReTimes))
                {
                    hub.Para.DoRun = ERUN.读卡报警;

                    hub.Para.Alarm = EAlarm.读卡错误;

                    hub.Para.AlarmInfo = CLanguage.Lan("读取治具ID错误");

                    OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                    Log(hub.ToString() + CLanguage.Lan("治具到位,读取治具ID错误:") + er, udcRunLog.ELog.NG);

                    return false;
                }

                hub.Fixture.IdCard = rIdCard;

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检查治具状态
        /// </summary>
        /// <param name="hub"></param>
        /// <returns></returns>
        private bool OnScan_CheckFixture(CStatHub hub, out bool SampleFixture)
        {
            SampleFixture = false;

            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFix_Base fixture = null;

                if (!CWeb2.GetFixtureInfo(hub.Fixture.IdCard, out fixture, out er))
                {
                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.状态错误;

                    hub.Para.AlarmInfo = er;

                    OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                    Log(hub.ToString() + CLanguage.Lan("获取治具ID") + "【" + hub.Fixture.IdCard + "】" +
                                         CLanguage.Lan("基本信息") + CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);

                    return false;
                }

                if (fixture.FixtureType == CWeb2.EFixtureType.禁用)
                {
                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.治具禁用;

                    hub.Para.AlarmInfo = CLanguage.Lan("该治具已被禁用.");

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" + CLanguage.Lan("已被禁用,请解除."), udcRunLog.ELog.NG);

                    OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                    return false;
                }
                else if (fixture.FixtureType == CWeb2.EFixtureType.点检治具)
                {
                    SampleFixture = true;

                    return true;
                }

                bool fix_alarm = false;

                if (CGlobalPara.SysPara.Alarm.FixtureTimes > 0 && fixture.TTNum > CGlobalPara.SysPara.Alarm.FixtureTimes)
                {
                    er = CLanguage.Lan("使用次数") + "[" + fixture.TTNum.ToString() + "]" +
                                      CLanguage.Lan("超过设置") + "[" + CGlobalPara.SysPara.Alarm.FixtureTimes.ToString() + "]";

                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.超过使用寿命;

                    hub.Para.AlarmInfo = er;

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" + er, udcRunLog.ELog.NG);

                    fix_alarm = true;
                }

                if (CGlobalPara.SysPara.Alarm.FixFailTimes > 0 && fixture.ConFailNum > CGlobalPara.SysPara.Alarm.FixFailTimes)
                {
                    er = CLanguage.Lan("连续不良次数") + "[" + fixture.ConFailNum.ToString() + "]" +
                         CLanguage.Lan("超过设置") + "[" + CGlobalPara.SysPara.Alarm.FixFailTimes.ToString() + "]";

                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.超过连续不良次数;

                    hub.Para.AlarmInfo = er;

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" + er, udcRunLog.ELog.NG);

                    fix_alarm = true;
                }

                double passRate = 1;

                if (fixture.TTNum > 0)
                {
                    passRate = (double)(fixture.TTNum - fixture.FailNum) / (double)fixture.TTNum;
                }

                if (CGlobalPara.SysPara.Alarm.FixPassRate > 0 && (passRate * 100) < CGlobalPara.SysPara.Alarm.FixPassRate)
                {
                    er = CLanguage.Lan("治具良率") + "[" + passRate.ToString("P2") + "]" + CLanguage.Lan("低于设置") +
                         "[" + CGlobalPara.SysPara.Alarm.FixPassRate.ToString() + "%]";

                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.低于治具良率设置;

                    hub.Para.AlarmInfo = er;

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" + er, udcRunLog.ELog.NG);

                    fix_alarm = true;
                }

                string fix_status = fixture.TTNum.ToString() + "|" + fixture.ConFailNum.ToString() + "|" +
                                    passRate.ToString("P2");

                if (fix_alarm)
                {
                    OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                    return false;
                }

                watcher.Stop();

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" + CLanguage.Lan("状态") + ":" +
                                     fix_status + ":" + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 空治具过站
        /// </summary>
        /// <returns></returns>
        private bool OnScan_EmptyFixtureGoBy(CStatHub hub)
        {
            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                if (!BandSnToFixture(CWeb2.EFixtureType.空治具, hub, out er))
                {
                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.绑定错误;

                    hub.Para.AlarmInfo = er;

                    OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" + CLanguage.Lan("绑定为空治具错误:") + er, udcRunLog.ELog.NG);

                    return false;
                }

                hub.Para.DoRun = ERUN.空治具过站;

                hub.Para.Alarm = EAlarm.正常;

                hub.Para.AlarmInfo = CLanguage.Lan("绑定为空治具过站.");

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                watcher.Stop();

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard +
                                      "】" + CLanguage.Lan("绑定为空治具OK,准备过站:") +
                                      watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 自动绑定治具条码
        /// </summary>
        /// <param name="hub"></param>
        /// <returns></returns>
        private bool OnScan_AutoBandingSn(CStatHub hub)
        {
            try
            {
                string er = string.Empty;

                string[] guid = Guid.NewGuid().ToString().Split('-');

                for (int i = 0; i < hub.Fixture.SerialNo.Count; i++)
                {
                    string serialNo = guid[0].ToUpper() + (i + 1).ToString("D2");

                    if (_statInfo.CheckSn[i])
                        hub.Fixture.SerialNo[i] = serialNo;
                    else
                        hub.Fixture.SerialNo[i] = string.Empty;
                }

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                if (!BandSnToFixture(CWeb2.EFixtureType.正常, hub, out er))
                {
                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.绑定错误;

                    hub.Para.AlarmInfo = CLanguage.Lan("治具条码绑定错误");

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" +
                                         CLanguage.Lan("绑定错误") + ":" + er, udcRunLog.ELog.NG);

                    return false;
                }

                hub.Para.DoRun = ERUN.正常治具过站;

                hub.Para.Alarm = EAlarm.正常;

                hub.Para.AlarmInfo = CLanguage.Lan("自动绑定条码OK,准备过站");

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                watcher.Stop();

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" +
                                     CLanguage.Lan("自动绑定条码OK,准备过站") + ":" + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 绑定治具与条码
        /// </summary>
        /// <param name="hub"></param>
        /// <returns></returns>
        private bool OnScan_BandSnToFxiture(CStatHub hub)
        {
            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                if (!BandSnToFixture(CWeb2.EFixtureType.正常, hub, out er))
                {
                    hub.Para.DoRun = ERUN.异常报警;

                    hub.Para.Alarm = EAlarm.绑定错误;

                    hub.Para.AlarmInfo = CLanguage.Lan("治具条码绑定错误");

                    OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                    Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" +
                                         CLanguage.Lan("绑定治具错误") + ":" + er, udcRunLog.ELog.NG);

                    return false;
                }

                hub.Para.DoRun = ERUN.正常治具过站;

                hub.Para.Alarm = EAlarm.正常;

                hub.Para.AlarmInfo = CLanguage.Lan("扫描条码绑定OK,准备过站");

                OnUIBandSnArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, hub));

                watcher.Stop();

                Log(hub.ToString() + CLanguage.Lan("治具ID") + "【" + hub.Fixture.IdCard + "】" +
                                    CLanguage.Lan("扫描条码绑定OK,准备过站") + ":" +
                                    watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检查扫描条码
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool OnScan_CheckSn(int idNo, string serialNo, List<string> SnList, out string er)
        {
            er = string.Empty;

            try
            {
                if (CGlobalPara.SysPara.Mes.SnLen != 0 && CGlobalPara.SysPara.Mes.SnLen != serialNo.Length)
                {
                    er = CLanguage.Lan("条码长度错误") + ":" + serialNo.Length.ToString();
                    return false;
                }
                if (CGlobalPara.SysPara.Mes.SnSpec != string.Empty &&
                    CGlobalPara.SysPara.Mes.SnSpec != serialNo.Substring(0, CGlobalPara.SysPara.Mes.SnSpec.Length))
                {
                    er = CLanguage.Lan("条码规则错误") + ":" + "[" + CGlobalPara.SysPara.Mes.SnSpec + "]";
                    return false;
                }
                if (SnList.Contains(serialNo))
                {
                    er = CLanguage.Lan("条码重复扫描,请重新扫描.");
                    return false;
                }

                if (CGlobalPara.SysPara.Mes.Connect) //客户MES
                {
                    if (!CheckSn(serialNo, out er))
                        return false;
                }

                //冠佳Web
                if (CGlobalPara.SysPara.Mes.ChkWebSn == 1)
                {
                    if (!CheckSnFormWeb(serialNo, out er))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        #endregion

        #region 通电治具检查任务
        /// <summary>
        /// 测试位治具到位
        /// </summary>
        private void OnTestHubTask()
        {

            _statPreTest.hub.Para._cts = new CancellationTokenSource();

            _statPreTest.hub.Para.Watcher.Restart();

            try
            {
                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statPreTest.hub.Para._cts.IsCancellationRequested)
                        return;

                    switch (_statPreTest.hub.Para.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.到位:
                            OnTestHubReady();
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
                _statPreTest.hub.Para._cts = null;

                _statPreTest.hub.Para.Watcher.Stop();

                Log(_statPreTest.hub.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statPreTest.hub.Para.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 测试位治具到位
        /// </summary>
        private void OnTestHubReady()
        {
            try
            {
                string er = string.Empty;

                string rIdCard = string.Empty;

                if (!_devIDCard.ReadIdCard(_statPreTest.hub.Base.IdCardAddr, out rIdCard, out er, CGlobalPara.SysPara.Para.IdReTimes))
                {
                    _statPreTest.hub.Para.DoRun = ERUN.读卡报警;
                    _statPreTest.hub.Para.Alarm = EAlarm.读卡错误;
                    _statPreTest.hub.Para.AlarmInfo = CLanguage.Lan("读取治具ID错误");
                    OnUIStatHubArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具到位,读取治具ID错误:") + er, udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位轨道治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                _statPreTest.hub.Fixture.IdCard = rIdCard;

                Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具到位,读取治具ID") + "【" + _statPreTest.hub.Fixture.IdCard + "】OK," +
                                                  CLanguage.Lan("等待治具检查"), udcRunLog.ELog.Action);

                CWeb2.CFixture fix = null;

                if (!CWeb2.GetFixtureInfoByIdCard(_statPreTest.hub.Fixture.IdCard, out fix, out er))
                {
                    _statPreTest.hub.Para.DoRun = ERUN.异常报警;
                    _statPreTest.hub.Para.Alarm = EAlarm.获取治具信息错误;
                    _statPreTest.hub.Para.AlarmInfo = CLanguage.Lan("获取治具条码错误");
                    OnUIStatHubArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.hub.Fixture.IdCard + "】" +
                                                      CLanguage.Lan("获取WEB错误:") + er, udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位轨道治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                if (fix.Base.FixtureType == CWeb2.EFixtureType.禁用)
                {
                    _statPreTest.hub.Para.DoRun = ERUN.异常报警;
                    _statPreTest.hub.Para.Alarm = EAlarm.治具禁用;
                    _statPreTest.hub.Para.AlarmInfo = CLanguage.Lan("该治具已被禁用");
                    OnUIStatHubArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.hub.Fixture.IdCard + "】" +
                                                      CLanguage.Lan("该治具已被禁用,请取出."), udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位轨道治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }
                else if (fix.Base.FixtureType == CWeb2.EFixtureType.空治具)
                {
                    _statPreTest.hub.Para.DoRun = ERUN.空治具过站;
                    _statPreTest.hub.Para.Alarm = EAlarm.正常;
                    _statPreTest.hub.Para.AlarmInfo = CLanguage.Lan("设置为空治具,准备过站.");
                    OnUIStatHubArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.hub.Fixture.IdCard + "】" +
                                                      CLanguage.Lan("设置为空治具,准备过站."), udcRunLog.ELog.OK);
                    _threadPLC.addREGWrite(EPLCOUT.初测位轨道治具结果.ToString(), (int)EPLC_RESULT.直接过站);
                    return;
                }
                else
                {
                    for (int i = 0; i < fix.Para.Count; i++)
                    {
                        _statPreTest.hub.Fixture.SerialNo[i] = fix.Para[i].SerialNo;
                    }

                    if (!local_db_SaveFixtureInfo(_statPreTest.hub, out er))
                    {
                        _statPreTest.hub.Para.DoRun = ERUN.异常报警;
                        _statPreTest.hub.Para.Alarm = EAlarm.状态错误;
                        _statPreTest.hub.Para.AlarmInfo = CLanguage.Lan("治具ID") + CLanguage.Lan("写入本地数据错误");
                        OnUIStatHubArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                        Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.hub.Fixture.IdCard +
                                                          "】" + CLanguage.Lan("写入本地数据错误") + ":" + er, udcRunLog.ELog.NG);
                        _threadPLC.addREGWrite(EPLCOUT.初测位轨道治具结果.ToString(), (int)EPLC_RESULT.结果NG);
                        return;
                    }

                    _statPreTest.hub.Para.DoRun = ERUN.正常治具过站;
                    _statPreTest.hub.Para.Alarm = EAlarm.正常;
                    _statPreTest.hub.Para.AlarmInfo = CLanguage.Lan("治具到位就绪,准备顶升测试.");

                    OnUIStatHubArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.hub.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.hub.Fixture.IdCard +
                                                      "】" + CLanguage.Lan("治具到位就绪,准备顶升测试."), udcRunLog.ELog.OK);
                    _threadPLC.addREGWrite(EPLCOUT.初测位轨道治具结果.ToString(), (int)EPLC_RESULT.结果OK);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 通电测试任务
        /// <summary>
        /// 测试位到位
        /// </summary>
        private void OnTestStatTask()
        {
            try
            {
                _statPreTest.test.Para._cts = new CancellationTokenSource();

                _statPreTest.test.Para.Watcher.Restart();

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statPreTest.test.Para._cts.IsCancellationRequested)
                        return;

                    switch (_statPreTest.test.Para.DoRun)
                    {
                        case ERUN.到位:
                            OnTestStatReady();
                            break;
                        case ERUN.等待:
                            if (_statPreTest.test.Para.Io_Watcher.ElapsedMilliseconds < CGlobalPara.SysPara.Para.AcDelayTimes)
                                break;
                            if (CGlobalPara.SysPara.Para.ChkIdleLoad)
                            {
                                if (!CheckVoltReady())
                                {
                                    if (_statPreTest.test.Para.Io_Watcher.ElapsedMilliseconds < CGlobalPara.SysPara.Para.AcDelayTimes * 2)
                                        break;
                                }
                            }
                            _statPreTest.test.Para.Io_Watcher.Stop();
                            _statPreTest.test.Para.DoRun = ERUN.就绪;
                            break;
                        case ERUN.就绪:
                            if (CGlobalPara.SysPara.Para.ChkIdleLoad)
                            {
                                SetLoadValue(_runModel.OutPut.LoadSet);
                            }
                            _statInfo.Reset = 0;
                            _statPreTest.test.Para.DoRun = ERUN.测试;
                            _statPreTest.test.Para.AlarmInfo = CLanguage.Lan("正在检测电压和电流");
                            OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                            Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                               CLanguage.Lan("正在检测电压和电流"), udcRunLog.ELog.Action);
                            break;
                        case ERUN.测试:
                            OnTesting();
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
                _statPreTest.test.Para._cts = null;

                _statPreTest.test.Para.Watcher.Stop();

                Log(_statPreTest.test.ToString() + CLanguage.Lan("任务结束") + ":" +
                    _statPreTest.test.Para.Watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);

            }
        }
        /// <summary>
        /// 测试准备OK
        /// </summary>
        private void OnTestStatReady()
        {
            try
            {
                string er = string.Empty;

                string rIdCard = string.Empty;

                List<string> serialNos = null;

                if (!local_db_GetFixtureInfo(out rIdCard, out serialNos, out er))
                {
                    _statPreTest.test.Para.DoRun = ERUN.读卡报警;
                    _statPreTest.test.Para.Alarm = EAlarm.读卡错误;
                    _statPreTest.test.Para.AlarmInfo = CLanguage.Lan("治具到位,获取治具ID错误");
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具到位,获取治具ID错误") + ":" + er, udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                bool haveUUT = false;

                _statPreTest.test.Fixture.IdCard = rIdCard;

                _statPreTest.test.Para.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                for (int i = 0; i < serialNos.Count; i++)
                {
                    _statPreTest.test.Fixture.SerialNo[i] = serialNos[i];

                    if (serialNos[i] != string.Empty)
                        haveUUT = true;
                }

                OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));

                if (!haveUUT)
                {
                    _statPreTest.test.Para.DoRun = ERUN.正常治具过站;
                    _statPreTest.test.Para.Alarm = EAlarm.正常;
                    _statPreTest.test.Para.AlarmInfo = CLanguage.Lan("无可测试产品,直接过站");
                    _statPreTest.test.Para.TestTime = _statPreTest.test.Para.Watcher.ElapsedMilliseconds;
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                       CLanguage.Lan("到位") + "," + CLanguage.Lan("无可测试产品,直接过站"), udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                if (CGlobalPara.SysPara.Para.ChkIdleLoad)
                {
                    SetLoadValue(CGlobalPara.SysPara.Para.IdleLoad);
                }

                //设置快充电压
                SetQCM();

                _threadPLC.addREGWrite(EPLCOUT.初测位测试AC通电.ToString(), 1);

                _statPreTest.test.Para.DoRun = ERUN.等待;

                _statPreTest.test.Para.Alarm = EAlarm.正常;

                _statPreTest.test.Para.AlarmInfo = CLanguage.Lan("启动AC ON,延时:") + CGlobalPara.SysPara.Para.AcDelayTimes + "ms";

                _statPreTest.test.Para.Io_Watcher.Restart();

                _statPreTest.test.Para.Watcher.Restart();

                OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));

                Log(_statPreTest.test.ToString() + CLanguage.Lan("治具到位,获取治具ID") + "【" + _statPreTest.test.Fixture.IdCard +
                                                   "】OK," + CLanguage.Lan("启动AC ON,延时:") + CGlobalPara.SysPara.Para.AcDelayTimes + "ms",
                                                   udcRunLog.ELog.Action);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 测试中
        /// </summary>
        private void OnTesting()
        {
            try
            {
                string er = string.Empty;

                bool haveUUT = false;

                bool uutPass = true;

                if (!OnReadVolt(out haveUUT, out uutPass, out er))
                {
                    _statPreTest.test.Para.DoRun = ERUN.异常报警;
                    _statPreTest.test.Para.Alarm = EAlarm.状态错误;
                    _statPreTest.test.Para.AlarmInfo = er;
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】:" + er, udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                if (CGlobalPara.SysPara.Para.ChkFailWait && !uutPass)
                {
                    if (!OnTest_ConfigFailResult(out haveUUT, out uutPass, out er))
                    {
                        _statPreTest.test.Para.DoRun = ERUN.异常报警;
                        _statPreTest.test.Para.Alarm = EAlarm.状态错误;
                        _statPreTest.test.Para.AlarmInfo = er;
                        OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                        Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】:" + er, udcRunLog.ELog.NG);
                        _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);
                        return;
                    }
                }
                _statPreTest.test.Para.Io_Watcher.Stop();
                _statPreTest.test.Para.Watcher.Stop();
                _statPreTest.test.Para.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                if (!haveUUT)
                {
                    _statPreTest.test.Para.DoRun = ERUN.异常报警;
                    _statPreTest.test.Para.Alarm = EAlarm.状态错误;
                    _statPreTest.test.Para.AlarmInfo = CLanguage.Lan("无可测试产品,请取下治具");
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                       CLanguage.Lan("无可测试产品,请取下治具"), udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);
                    return;
                }

                if (CGlobalPara.SysPara.Mes.Connect && CGlobalPara.SysPara.Mes.TranDataToMes)
                {
                    Task.Factory.StartNew(() => TranSn());
                }

                if (!UpdateFixtureResult(out er))
                {
                    _statPreTest.test.Para.DoRun = ERUN.异常报警;
                    _statPreTest.test.Para.Alarm = EAlarm.状态错误;
                    _statPreTest.test.Para.AlarmInfo = CLanguage.Lan("写入WEB测试结果错误");
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                       CLanguage.Lan("写入WEB测试结果错误") + ":" + er, udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);

                    return;
                }

                if (!OnTest_UpdateYield(out er))
                {
                    _statPreTest.test.Para.DoRun = ERUN.异常报警;
                    _statPreTest.test.Para.Alarm = EAlarm.状态错误;
                    _statPreTest.test.Para.AlarmInfo = er;
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                       CLanguage.Lan("写入WEB测试结果错误") + ":" + er, udcRunLog.ELog.NG);
                    _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果NG);

                    return;
                }

                //统计测试数据
                int ttNum = 0;
                int passNum = 0;
                int failNum = 0;
                for (int i = 0; i < _statPreTest.test.Fixture.SerialNo.Count; i++)
                {
                    if (_statPreTest.test.Fixture.SerialNo[i] == string.Empty)
                        continue;
                    ttNum++;
                    if (_statPreTest.test.Fixture.Result[i] == 0)
                        passNum++;
                    else
                        failNum++;
                }

                //预报警统计
                _WarnRate.TTNum += ttNum;

                _WarnRate.PassNum += passNum;

                if (!SaveDailyRecord(ttNum, failNum, out er))
                {
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("保存日产能错误:") + er, udcRunLog.ELog.NG);
                }

                if (!SaveDailyReport(out er))
                {
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("保存测试数据错误:") + er, udcRunLog.ELog.NG);
                }

                local_db_recordFailSn(_statPreTest.test);

                _statPreTest.test.Para.DoRun = ERUN.正常治具过站;

                _threadPLC.addREGWrite(EPLCOUT.初测位测试AC通电.ToString(), 0);

                _threadPLC.addREGWrite(EPLCOUT.初测位测试结果.ToString(), (int)EPLC_RESULT.结果OK);

                string waitTime = ((double)_statPreTest.test.Para.Watcher.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                if (uutPass)
                {
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                       CLanguage.Lan("测试结果:PASS,准备过站:") + waitTime, udcRunLog.ELog.OK);
                }
                else
                {
                    Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                        CLanguage.Lan("测试结果:FAIL,准备过站:") + waitTime, udcRunLog.ELog.NG);
                }

                OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest));

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 读取结果
        /// </summary>
        /// <returns></returns>
        private bool OnReadVolt(out bool haveUUT, out bool uutPass, out string er, bool OnlyTime = false, bool idleLoad = false)
        {
            haveUUT = false;

            uutPass = true;

            er = string.Empty;

            try
            {
                int ReTestTimes = 1;

                if (!OnlyTime)
                {
                    ReTestTimes = CGlobalPara.SysPara.Para.ReTestTimes + 1;
                }

                for (int i = 0; i < ReTestTimes; i++)
                {
                    uutPass = true;

                    Thread.Sleep(CGlobalPara.SysPara.Para.ReTestDelay);

                    if (_statInfo.Reset == 1)  //重启AC
                    {
                        _threadPLC.addREGWrite(EPLCOUT.初测位测试AC通电.ToString(), 0);

                        Thread.Sleep(CGlobalPara.SysPara.Para.ReTestDelay);

                        if (idleLoad)
                        {
                            SetLoadValue(CGlobalPara.SysPara.Para.IdleLoad);
                        }

                        //设置快充电压
                        SetQCM();

                        _statInfo.ResetWatcher.Restart();

                        _statInfo.Reset = 2;

                        continue;
                    }
                    else if (_statInfo.Reset == 2) //启动AC
                    {
                        if (!CheckVoltOFF())
                            continue;

                        _threadPLC.addREGWrite(EPLCOUT.初测位测试AC通电.ToString(), 1);

                        if (idleLoad)
                        {
                            _statInfo.Reset = 3;
                            _statInfo.ResetWatcher.Restart();
                            continue;
                        }

                        _statInfo.Reset = 0;

                        _statInfo.ResetWatcher.Stop();
                    }
                    else if (_statInfo.Reset == 3)
                    {
                        if (!CheckVoltReady())
                        {
                            if (_statInfo.ResetWatcher.ElapsedMilliseconds < CGlobalPara.SysPara.Para.AcDelayTimes)
                                continue;
                        }

                        SetLoadValue(_runModel.OutPut.LoadSet);

                        _statInfo.Reset = 0;

                        _statInfo.ResetWatcher.Stop();
                    }

                    if (!OnTest_ReadELoad(out er))
                        return false;

                    if (CGlobalPara.SysPara.Para.ChkVSenor)
                    {
                        if (!OnTest_ReadFCMBVolt(out er))
                            return false;
                    }

                    if (!OnTest_JudgeVoltAndCurrent(out er))
                        return false;

                    if (_runModel.OutPut.ChkDD)
                    {
                        if (!OnTest_JudgeShort(out er))
                            return false;
                    }

                    bool TestEnd = false;

                    if (i == CGlobalPara.SysPara.Para.ReTestTimes)
                        TestEnd = true;

                    UIStatDataArgs.DoRun = EUIStatData.测试信息;
                    UIStatDataArgs.SerialNo = _statPreTest.test.Fixture.SerialNo;
                    UIStatDataArgs.V = _statPreTest.test.Fixture.Volt;
                    UIStatDataArgs.I = _statPreTest.test.Fixture.Cur;
                    UIStatDataArgs.DD = _statPreTest.test.Fixture.DD;
                    UIStatDataArgs.TestTime = _statPreTest.test.Para.Watcher.ElapsedMilliseconds;
                    UIStatDataArgs.TestEnd = TestEnd;
                    OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                    if (!OnTest_JudgeResult(out haveUUT, out uutPass, out er))
                        return false;

                    if (uutPass)
                        break;
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
        /// 读取电子负载数据
        /// </summary>
        /// <returns></returns>
        private bool OnTest_ReadELoad(out string er)
        {
            er = string.Empty;

            try
            {

                //读取电压和电流

                CEL_ReadData eloadVal = new CEL_ReadData();

                int AddrMax = CYOHOOApp.SlotMax / _devELoad.maxCH;

                for (int addr = 0; addr < AddrMax; addr++)
                {
                    Thread.Sleep(100);

                    if (!_devELoad.ReadELData(addr + 1, eloadVal, out er))
                    {
                        Thread.Sleep(100);

                        if (!_devELoad.ReadELData(addr + 1, eloadVal, out er))
                        {
                            er = "读取电子负载" + (addr + 1).ToString() + "通信异常";

                            er = CLanguage.Lan("读取电子负载") + "[" + (addr + 1).ToString("D2") + "]" + CLanguage.Lan("通信异常");
                            return false;
                        }
                    }

                    for (int ch = 0; ch < _devELoad.maxCH; ch++)
                    {
                        _statPreTest.test.Fixture.Volt[ch + addr * _devELoad.maxCH] = eloadVal.Volt[ch];
                        _statPreTest.test.Fixture.Volt[ch + addr * _devELoad.maxCH] += _runModel.OutPut.VOffSet;
                        _statPreTest.test.Fixture.Cur[ch + addr * _devELoad.maxCH] = eloadVal.Load[ch];
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
        /// 读取快充板电压
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool OnTest_ReadFCMBVolt(out string er)
        {
            er = string.Empty;

            try
            {
                List<double> volts = null;

                Thread.Sleep(50);

                if (!_devFCMB.ReadVolt(1, out volts, out er, false, EVMODE.VOLT_32))
                {
                    Thread.Sleep(50);

                    if (!_devFCMB.ReadVolt(1, out volts, out er, false, EVMODE.VOLT_32))
                    {
                        er = CLanguage.Lan("读取快充板电压通信异常");
                        return false;
                    }
                }

                for (int i = 0; i < _statPreTest.test.Fixture.Volt.Count; i++)
                {
                    _statPreTest.test.Fixture.Volt[i] = volts[i] + _runModel.OutPut.VOffSet;
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
        /// 判定电压与电流规格
        /// </summary>
        /// <returns></returns>
        private bool OnTest_JudgeVoltAndCurrent(out string er)
        {
            er = string.Empty;

            try
            {
                for (int slot = 0; slot < _statPreTest.test.Fixture.SerialNo.Count; slot++)
                {
                    _statPreTest.test.Fixture.Result[slot] = 0;

                    _statPreTest.test.Fixture.DD[slot] = "正常";

                    if (_statPreTest.test.Fixture.SerialNo[slot] != string.Empty)
                    {
                        if (_statPreTest.test.Fixture.Volt[slot] < _runModel.OutPut.Vmin || _statPreTest.test.Fixture.Volt[slot] > _runModel.OutPut.Vmax)
                        {
                            _statPreTest.test.Fixture.Result[slot] = CYOHOOApp.PRETEST_FlowId;
                        }
                        if (_statPreTest.test.Fixture.Cur[slot] < _runModel.OutPut.LoadMin || _statPreTest.test.Fixture.Cur[slot] > _runModel.OutPut.LoadMax)
                        {
                            _statPreTest.test.Fixture.Result[slot] = CYOHOOApp.PRETEST_FlowId;

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
        /// 短路检测
        /// </summary>
        /// <returns></returns>
        private bool OnTest_JudgeShort(out string er)
        {
            er = string.Empty;

            try
            {

                string[] C_D_GND = new string[] { "D+V-", "D-V-", "D+D-", "D-V+" };

                for (int slot = 0; slot < _statPreTest.test.Fixture.SerialNo.Count; slot++)
                {
                    if (_statPreTest.test.Fixture.Result[slot] != 0)
                        continue;

                    List<int> status = null;

                    Thread.Sleep(50);

                    if (!_devFCMB.ReadDGND(1, slot + 1, out status, out er))
                    {
                        Thread.Sleep(50);

                        if (!_devFCMB.ReadDGND(1, slot + 1, out status, out er))
                        {
                            _statPreTest.test.Fixture.DD[slot] = "异常";
                            _statPreTest.test.Fixture.Result[slot] = CYOHOOApp.PRETEST_FlowId;
                            continue;
                        }
                    }

                    for (int i = 0; i < _runModel.OutPut.ChkDG.Length; i++)
                    {
                        if (_runModel.OutPut.ChkDG[i] && status[i] != 0)
                        {
                            if (_statPreTest.test.Fixture.DD[slot] == "正常")  //短路
                                _statPreTest.test.Fixture.DD[slot] = C_D_GND[i] + ";";
                            else
                                _statPreTest.test.Fixture.DD[slot] += C_D_GND[i] + ";";

                            _statPreTest.test.Fixture.Result[slot] = CYOHOOApp.PRETEST_FlowId;
                        }
                        else if (!_runModel.OutPut.ChkDG[i] && status[i] == 0) //开路
                        {
                            if (_statPreTest.test.Fixture.DD[slot] == "正常")
                                _statPreTest.test.Fixture.DD[slot] = C_D_GND[i] + ";";
                            else
                                _statPreTest.test.Fixture.DD[slot] += C_D_GND[i] + ";";
                            _statPreTest.test.Fixture.Result[slot] = CYOHOOApp.PRETEST_FlowId;
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
        /// 判定结果
        /// </summary>
        /// <returns></returns>
        private bool OnTest_JudgeResult(out bool haveUUT, out bool uutPass, out string er)
        {
            er = string.Empty;

            haveUUT = false;

            uutPass = true;

            try
            {
                for (int slot = 0; slot < _statPreTest.test.Fixture.SerialNo.Count; slot++)
                {
                    if (_statPreTest.test.Fixture.SerialNo[slot] != string.Empty)
                    {
                        if (_statPreTest.test.Fixture.Result[slot] == 0)
                            haveUUT = true;
                        else
                            uutPass = false;
                    }


                    CTestData UUT = new CTestData();

                    UUT.UUT = new List<CTestVal>();

                    UUT.UUT.Add(new CTestVal()
                    {
                        Vname = _runModel.OutPut.Vname,
                        Volt = _statPreTest.test.Fixture.Volt[slot],
                        Current = _statPreTest.test.Fixture.Cur[slot],
                        DDStatus = _statPreTest.test.Fixture.DD[slot],
                        Result = _statPreTest.test.Fixture.Result[slot]
                    });

                    _statPreTest.test.Fixture.Value[slot] = GJ.COM.CJSon.Serializer<CTestData>(UUT);
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
        /// 人工确认不良
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool OnTest_ConfigFailResult(out bool haveUUT, out bool uutPass, out string er)
        {
            haveUUT = false;

            uutPass = true;

            er = string.Empty;

            try
            {
                _statInfo.DisFail = true;

                UIStatDataArgs.DoRun = EUIStatData.确定不良;

                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                while (_statInfo.DisFail)
                {
                    Thread.Sleep(CGlobalPara.SysPara.Para.ReTestDelay);

                    if (_statPreTest.test.Para._cts.IsCancellationRequested)
                        return true;

                    if (_statPreTest.test.Para.DoRun == ERUN.空闲)
                        return true;

                    uutPass = true;

                    if (!OnReadVolt(out haveUUT, out uutPass, out er, true, CGlobalPara.SysPara.Para.ChkIdleLoad))
                    {
                        UIStatDataArgs.bAlarm = true;

                        UIStatDataArgs.AlarmInfo = er;

                        UIStatDataArgs.DoRun = EUIStatData.状态提示;

                        OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                        return false;
                    }
                    UIStatDataArgs.DoRun = EUIStatData.测试信息;
                    UIStatDataArgs.SerialNo = _statPreTest.test.Fixture.SerialNo;
                    UIStatDataArgs.V = _statPreTest.test.Fixture.Volt;
                    UIStatDataArgs.I = _statPreTest.test.Fixture.Cur;
                    UIStatDataArgs.DD = _statPreTest.test.Fixture.DD;
                    UIStatDataArgs.TestTime = _statPreTest.test.Para.Watcher.ElapsedMilliseconds;
                    UIStatDataArgs.TestEnd = false;
                    OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));
                    OnUIStatTestArgs.OnEvented(new CUIUserArgs<CStat>(_idNo, _name, _statPreTest, 1));
                }

                UIStatDataArgs.DoRun = EUIStatData.取消确定;

                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 更新产能计数
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool OnTest_UpdateYield(out string er)
        {
            er = string.Empty;

            try
            {
                bool FixPass = true;

                for (int i = 0; i < _statPreTest.test.Fixture.SerialNo.Count; i++)
                {
                    if (_statPreTest.test.Fixture.SerialNo[i] == string.Empty)
                        continue;

                    if (_statPreTest.test.Fixture.Result[i] != 0)
                    {
                        FixPass = false;
                        _statInfo.FailNum += 1;
                    }

                    _statInfo.TTNum++;
                }

                _statInfo.StatUseNum++;

                if (FixPass)
                {
                    _statInfo.StatFailNum = 0;
                }
                else
                {
                    _statInfo.StatFailNum++;
                }

                CIniFile.WriteToIni("Parameter", "StatUseNum", _statInfo.StatUseNum.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("Parameter", "StatFailNum", _statInfo.StatFailNum.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("Parameter", "TTNum", _statInfo.TTNum.ToString(), CGlobalPara.IniFile);

                CIniFile.WriteToIni("Parameter", "FailNum", _statInfo.FailNum.ToString(), CGlobalPara.IniFile);

                UIStatDataArgs.UseNum = _statInfo.StatUseNum;

                UIStatDataArgs.TTNum = _statInfo.TTNum;

                UIStatDataArgs.FailNum = _statInfo.FailNum;

                UIStatDataArgs.ConFailNum = _statInfo.StatFailNum;

                UIStatDataArgs.DoRun = EUIStatData.取消确定;

                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                UIStatDataArgs.DoRun = EUIStatData.使用次数;

                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                UIStatDataArgs.DoRun = EUIStatData.产能统计;

                OnUIStatDataArgs.OnEvented(new CUIUserArgs<CUIStatDataArgs>(_idNo, _name, UIStatDataArgs));

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 检查电压是否正常？
        /// </summary>
        /// <returns></returns>
        private bool CheckVoltReady()
        {
            try
            {
                string er = string.Empty;

                if (!OnTest_ReadELoad(out er))
                    return false;

                bool uutPass = true;

                for (int slot = 0; slot < _statPreTest.test.Fixture.SerialNo.Count; slot++)
                {
                    if (_statPreTest.test.Fixture.SerialNo[slot] == string.Empty)
                        continue;

                    if (_statPreTest.test.Fixture.Volt[slot] < _runModel.OutPut.Vmin || _statPreTest.test.Fixture.Volt[slot] > _runModel.OutPut.Vmax)
                    {
                        uutPass = false;
                    }
                }

                return uutPass;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.NG);
                return false;
            }
        }
        /// <summary>
        /// 检查电压是否正常？
        /// </summary>
        /// <returns></returns>
        private bool CheckVoltOFF()
        {
            try
            {
                string er = string.Empty;

                if (!OnTest_ReadELoad(out er))
                    return false;

                bool uutHave = true;

                for (int slot = 0; slot < _statPreTest.test.Fixture.SerialNo.Count; slot++)
                {
                    if (_statPreTest.test.Fixture.SerialNo[slot] == string.Empty)
                        continue;

                    if (_statPreTest.test.Fixture.Volt[slot] > 2)
                    {
                        uutHave = false;
                    }
                }

                return uutHave;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.NG);
                return false;
            }
        }
        #endregion

        #region 负载设置
        /// <summary>
        /// 设置负载值
        /// </summary>
        /// <param name="loadValue"></param>
        /// <returns></returns>
        private bool SetLoadValue(double loadValue)
        {
            try
            {
                string er = string.Empty;

                //设置负载电流

                CEL_SetPara para = new CEL_SetPara();

                for (int i = 0; i < para.Run_Val.Length; i++)
                {
                    para.Run_Power[i] = 0;
                    para.Run_Von[i] = _runModel.OutPut.LoadVon;
                    para.Run_Mode[i] = (DEV.ELOAD.EMode)_runModel.OutPut.LoadMode;
                    para.Run_Val[i] = loadValue;
                }

                if (!_devELoad.SetELData(0, para, out er))
                {
                    System.Threading.Thread.Sleep(100);

                    if (!_devELoad.SetELData(0, para, out er))
                    {
                        Log(CLanguage.Lan("设置负载电流") + ":" + loadValue.ToString() + "A" +
                                                              CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);
                    }
                }

                Log(CLanguage.Lan("设置负载电流") + ":" + loadValue.ToString() + "A", udcRunLog.ELog.Action);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        /// <summary>
        /// 设置快充
        /// </summary>
        /// <returns></returns>
        private bool SetQCM()
        {
            try
            {
                string er = string.Empty;

                EQCM qcmSetting = (EQCM)_runModel.OutPut.QCM;

                double qcvSetting = _runModel.OutPut.QCV;

                if (!_runModel.OutPut.ChkQCM)
                {
                    qcmSetting = EQCM.Reserve;
                }

                //回读快充模式

                EQCM qcm = EQCM.Normal;

                double qcv = 0;

                double qci = 0;

                Thread.Sleep(50);

                if (!_devFCMB.ReadQCM(1, out qcm, out qcv, out qci, out er))
                {
                    Thread.Sleep(100);

                    if (!_devFCMB.ReadQCM(1, out qcm, out qcv, out qci, out er))
                    {
                        Log(CLanguage.Lan("设置快充主控板模式") + "->[" + ((EQCM)_runModel.OutPut.QCM).ToString() + "]" +
                                          CLanguage.Lan("快充电压") + "[" + _runModel.OutPut.QCV.ToString() + "]" +
                                          CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);
                        return true;
                    }
                }

                if (qcm == qcmSetting && qcv == qcvSetting)
                    return true;

                Thread.Sleep(50);

                if (!_devFCMB.SetQCM(1, qcmSetting, qcvSetting, 0, out er))
                {
                    Thread.Sleep(100);

                    if (!_devFCMB.SetQCM(1, qcmSetting, qcvSetting, 0, out er))
                    {
                        Log(CLanguage.Lan("设置快充主控板模式") + "[" + qcmSetting.ToString() + "]" +
                                            CLanguage.Lan("快充电压") + "[" + qcvSetting.ToString() + "]" +
                                            CLanguage.Lan("错误") + ":" + er, udcRunLog.ELog.NG);
                    }
                    else
                    {
                        Log(CLanguage.Lan("设置快充主控板模式") + "[" + qcmSetting.ToString() + "]" +
                            CLanguage.Lan("快充电压") + "[" + qcvSetting.ToString() + "V]", udcRunLog.ELog.Action);
                    }
                }
                else
                {
                    Log(CLanguage.Lan("设置快充主控板模式") + "[" + qcmSetting.ToString() + "]" +
                        CLanguage.Lan("快充电压") + "[" + qcvSetting.ToString() + "V]", udcRunLog.ELog.Action);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.NG);
                return false;
            }
        }
        #endregion

        #region 进老化检查任务
        /// <summary>
        /// 测试位到位
        /// </summary>
        private void OnInBIStatTask()
        {
            try
            {
                _statInBI.Para._cts = new CancellationTokenSource();

                _statInBI.Para.Watcher.Restart();

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_statInBI.Para._cts.IsCancellationRequested)
                        return;

                    switch (_statInBI.Para.DoRun)
                    {
                        case ERUN.空闲:
                            return;
                        case ERUN.到位:
                            OnInBIStatReady();
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
                _statInBI.Para._cts = null;

                _statInBI.Para.Watcher.Stop();

                Log(_statInBI.ToString() + CLanguage.Lan("任务结束") + ":" +
                           _statInBI.Para.Watcher.ElapsedMilliseconds.ToString() +
                           "ms", udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 进老化检查到位
        /// </summary>
        private void OnInBIStatReady()
        {
            try
            {
                string er = string.Empty;

                //读取治具Id
                string rIdCard = string.Empty;

                if (!_devIDCard.ReadIdCard(_statInBI.Base.IdCardAddr, out rIdCard, out er, CGlobalPara.SysPara.Para.IdReTimes))
                {
                    _statInBI.Para.DoRun = ERUN.读卡报警;

                    _statInBI.Para.Alarm = EAlarm.读卡错误;

                    _statInBI.Para.AlarmInfo = CLanguage.Lan("读取治具ID失败");

                    _threadPLC.addREGWrite(EPLCOUT.进老化前检查位结果.ToString(), (int)EPLC_RESULT.结果NG);

                    OnUIInBIArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statInBI));

                    Log(_statInBI.ToString() + CLanguage.Lan("读取治具ID失败") + ":" + er, udcRunLog.ELog.NG);

                    return;
                }

                _statInBI.Fixture.IdCard = rIdCard;

                Log(_statInBI.ToString() + CLanguage.Lan("治具ID") + "【" + _statInBI.Fixture.IdCard + "】" +
                                           CLanguage.Lan("到位,检查治具流程."), udcRunLog.ELog.Action);

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = null;

                if (!CWeb2.GetFixtureInfoByIdCard(_statInBI.Fixture.IdCard, out fixture, out er))
                {
                    _statInBI.Para.DoRun = ERUN.异常报警;

                    _statInBI.Para.Alarm = EAlarm.读卡错误;

                    _statInBI.Para.AlarmInfo = er;

                    _threadPLC.addREGWrite(EPLCOUT.进老化前检查位结果.ToString(), (int)EPLC_RESULT.结果NG);

                    OnUIInBIArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statInBI));

                    Log(_statInBI.ToString() + CLanguage.Lan("治具ID") + "【" + _statInBI.Fixture.IdCard + "】" +
                                               CLanguage.Lan("获取产品信息错误:") + er, udcRunLog.ELog.NG);
                }

                watcher.Stop();

                if (fixture.Base.FixtureType == CWeb2.EFixtureType.空治具)
                {
                    _statInBI.Para.DoRun = ERUN.空治具过站;

                    _statInBI.Para.Alarm = EAlarm.正常;

                    _statInBI.Para.AlarmInfo = CLanguage.Lan("为空治具,准备过站");

                    _threadPLC.addREGWrite(EPLCOUT.进老化前检查位结果.ToString(), (int)EPLC_RESULT.结果OK);

                    OnUIInBIArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statInBI));

                    Log(_statInBI.ToString() + CLanguage.Lan("治具ID") + "【" + _statInBI.Fixture.IdCard + "】" +
                                               CLanguage.Lan("为空治具,准备过站") + ":" +
                                               watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                    return;
                }

                for (int i = 0; i < fixture.Para.Count; i++)
                {
                    _statInBI.Fixture.SerialNo[i] = fixture.Para[i].SerialNo;

                    _statInBI.Fixture.ResultId[i] = fixture.Para[i].FlowId;

                    _statInBI.Fixture.ResultName[i] = fixture.Para[i].FlowName;

                    _statInBI.Fixture.Result[i] = fixture.Para[i].Result;
                }

                //获取产品测试结果

                bool HaveUUT = false;

                bool uutPass = true;

                for (int i = 0; i < _statInBI.Fixture.SerialNo.Count; i++)
                {
                    if (_statInBI.Fixture.SerialNo[i] != string.Empty)
                    {
                        if (_statInBI.Fixture.Result[i] == 0 && _statInBI.Fixture.ResultId[i] == CYOHOOApp.PRETEST_FlowId)
                        {
                            HaveUUT = true;
                        }
                        else
                        {
                            _statInBI.Fixture.Result[i] = CYOHOOApp.PRETEST_FlowId;

                            uutPass = false;
                        }
                    }
                }


                if (!HaveUUT)
                {
                    _statInBI.Para.DoRun = ERUN.正常治具过站;

                    OnUIInBIArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statInBI));

                    _statInBI.Para.DoRun = ERUN.异常报警;

                    _statInBI.Para.Alarm = EAlarm.获取治具信息错误;

                    _statInBI.Para.AlarmInfo = CLanguage.Lan("治具无可测试产品");

                    _threadPLC.addREGWrite(EPLCOUT.进老化前检查位结果.ToString(), (int)EPLC_RESULT.结果NG);

                    Log(_statInBI.ToString() + CLanguage.Lan("治具ID") + "【" + _statInBI.Fixture.IdCard + "】" +
                                             CLanguage.Lan("无可测试产品:") + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.NG);

                    return;
                }

                _statInBI.Para.DoRun = ERUN.正常治具过站;

                _statInBI.Para.Alarm = EAlarm.正常;

                _threadPLC.addREGWrite(EPLCOUT.进老化前检查位结果.ToString(), (int)EPLC_RESULT.结果OK);

                OnUIInBIArgs.OnEvented(new CUIUserArgs<CStatHub>(_idNo, _name, _statInBI));

                if (uutPass)
                    Log(_statInBI.ToString() + CLanguage.Lan("治具ID") + "【" + _statInBI.Fixture.IdCard + "】" +
                                                CLanguage.Lan("为PASS,准备过站:") + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);
                else
                    Log(_statInBI.ToString() + CLanguage.Lan("治具ID") + "【" + _statInBI.Fixture.IdCard + "】:FAIL," +
                                               CLanguage.Lan("下机位处理不良品."), udcRunLog.ELog.NG);

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
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
        private bool local_db_SaveFixtureInfo(CStatHub statHub, out string er)
        {
            er = string.Empty;

            try
            {
                dbLock.AcquireWriterLock(-1);

                string statName = CYOHOOApp.PRETEST_FlowName;

                er = string.Empty;

                bool IsExist = false;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = string.Empty;

                DataSet ds = null;

                sqlCmd = "select * from FixtureRecord where statName='" + statName + "' order by statName";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                if (ds.Tables[0].Rows.Count != 0)
                {
                    if (ds.Tables[0].Rows.Count == statHub.Fixture.SerialNo.Count)
                        IsExist = true;
                    else
                    {
                        sqlCmd = "delete * from FixtureRecord where statName='" + statName + "'";
                        if (!db.excuteSQL(sqlCmd, out er))
                            return false;
                    }
                }

                List<string> sqlCmdList = new List<string>();

                if (IsExist)
                {
                    for (int i = 0; i < statHub.Fixture.SerialNo.Count; i++)
                    {
                        sqlCmd = "update FixtureRecord set idCard='" + statHub.Fixture.IdCard + "',serialNo='" +
                                 statHub.Fixture.SerialNo[i] + "'" + " where statName='" + statName + "' and slotNo=" + i;
                        sqlCmdList.Add(sqlCmd);
                    }
                }
                else
                {
                    for (int i = 0; i < statHub.Fixture.SerialNo.Count; i++)
                    {
                        sqlCmd = "insert into FixtureRecord(statName,idCard,slotNo,serialNo) values ('" +
                                 statName + "','" + statHub.Fixture.IdCard + "'," + i.ToString() + ",'" + statHub.Fixture.SerialNo[i] + "')";
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
        private bool local_db_GetFixtureInfo(out string idCard, out List<string> serialNos, out string er)
        {
            idCard = string.Empty;

            serialNos = new List<string>();

            er = string.Empty;

            try
            {
                dbLock.AcquireWriterLock(-1);

                string statName = CYOHOOApp.PRETEST_FlowName;

                er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                string sqlCmd = string.Empty;

                DataSet ds = null;

                sqlCmd = "select * from FixtureRecord where statName='" + statName + "' order by slotNo";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                    return false;

                if (ds.Tables[0].Rows.Count == 0)
                {
                    er = CLanguage.Lan("获取不到该测试位治具信息");
                    return false;
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    idCard = ds.Tables[0].Rows[i]["idCard"].ToString();

                    serialNos.Add(ds.Tables[0].Rows[i]["serialNo"].ToString());
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
        private void local_db_recordFailSn(CStatTest hub)
        {
            try
            {
                dbLock.AcquireWriterLock(-1);

                string sNowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                List<string> sqlCmdList = new List<string>();

                string sqlCmd = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                sqlCmd = "delete * from FailRecord where datediff('d', startTime,Now()) > 30";

                sqlCmdList.Add(sqlCmd);

                for (int i = 0; i < hub.Fixture.SerialNo.Count; i++)
                {
                    if (hub.Fixture.SerialNo[i] == string.Empty)
                        continue;

                    if (hub.Fixture.Result[i] != 0)
                    {
                        sqlCmd = "delete * from FailRecord where serialNo='" + hub.Fixture.SerialNo[i] + "'";

                        sqlCmdList.Add(sqlCmd);

                        string fileName = createDailyReportFile();

                        string localName = hub.Fixture.IdCard + "-" + (i + 1).ToString("D2");

                        double TestTime = ((double)hub.Para.TestTime) / 1000;

                        string failInfo = CLanguage.Lan("电压") + "=" + hub.Fixture.Volt[i].ToString("0.00") + "V;";

                        failInfo += CLanguage.Lan("电流") + "=" + hub.Fixture.Cur[i].ToString("0.00") + "A;";

                        failInfo += CLanguage.Lan("状态") + "=" + hub.Fixture.DD[i];

                        sqlCmd = string.Format("insert into FailRecord(SerialNo,IdCard,SlotNo,localName,StartTime,EndTime," +
                                               "TestTime,FailInfo,FailTime,ReportPath) values ('{0}','{1}',{2},'{3}','{4}','{5}',{6},'{7}','{8}','{9}')",
                                                hub.Fixture.SerialNo[i], hub.Fixture.IdCard, i + 1,
                                                localName, hub.Para.StartTime, hub.Para.EndTime,
                                                TestTime, failInfo, sNowTime, fileName);
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
                dbLock.ReleaseWriterLock();
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

        #region 冠佳Web
        /// <summary>
        /// 绑定治具条码
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool BandSnToFixture(CWeb2.EFixtureType fixType, CStatHub statHub, out string er)
        {
            er = string.Empty;

            try
            {
                CWeb2.CFixture fixture = new CWeb2.CFixture();

                fixture.Base.FlowIndex = 0;
                fixture.Base.FlowName = CYOHOOApp.LOADUP_FlowName;
                fixture.Base.FlowGuid = CNet.HostName();
                fixture.Base.LineNo = CYOHOOApp.LineNo;
                fixture.Base.LineName = CYOHOOApp.LineName;
                fixture.Base.Model = _runModel.Base.Model;
                fixture.Base.OrderName = CYOHOOApp.OrderName;
                fixture.Base.MesFlag = (CGlobalPara.SysPara.Mes.Connect ? 1 : 0);
                fixture.Base.SnType = CWeb2.ESnType.外部条码;
                fixture.Base.MaxSlot = CYOHOOApp.SlotMax;
                fixture.Base.IdCard = statHub.Fixture.IdCard;
                fixture.Base.FixtureType = fixType;
                fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                for (int i = 0; i < fixture.Base.MaxSlot; i++)
                {
                    CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                    para.SlotNo = i;
                    para.SerialNo = statHub.Fixture.SerialNo[i];
                    para.InnerSn = string.Empty;
                    para.Remark1 = string.Empty;
                    para.Remark2 = string.Empty;
                    fixture.Para.Add(para);
                }

                if (!CWeb2.BandSnToFixture(fixture, out er))
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
        /// 更新测试结果
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool UpdateFixtureResult(out string er)
        {
            try
            {
                Stopwatch wathcer = new Stopwatch();

                wathcer.Start();
                CWeb2.CFixture fixture = new CWeb2.CFixture();
                fixture.Base.FlowIndex = 0;
                fixture.Base.FlowName = CYOHOOApp.PRETEST_FlowName;
                fixture.Base.FlowGuid = CNet.HostName();
                fixture.Base.SnType = CWeb2.ESnType.外部条码;
                fixture.Base.IdCard = _statPreTest.test.Fixture.IdCard;
                fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                for (int i = 0; i < _statPreTest.test.Fixture.SerialNo.Count; i++)
                {
                    CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                    para.SlotNo = i;
                    para.SerialNo = _statPreTest.test.Fixture.SerialNo[i];
                    para.InnerSn = string.Empty;
                    para.Result = _statPreTest.test.Fixture.Result[i];
                    para.TestData = _statPreTest.test.Fixture.Value[i];
                    para.Remark1 = _runModel.Base.Model;
                    para.Remark2 = string.Empty;
                    para.StartTime = _statPreTest.test.Para.StartTime;
                    para.EndTime = _statPreTest.test.Para.EndTime;
                    fixture.Para.Add(para);
                }

                if (!CWeb2.UpdateFixtureResult(fixture, out er))
                    return false;

                wathcer.Stop();

                Log(_statPreTest.test.ToString() + CLanguage.Lan("治具ID") + "【" + _statPreTest.test.Fixture.IdCard + "】" +
                                                  CLanguage.Lan("写入WEB正常:") + wathcer.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 检查条码
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckSnFormWeb(string serialNo, out string er)
        {
            try
            {
                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                if (!CWeb2.CheckSn(CYOHOOApp.LOADUP_FlowName, serialNo, out er))
                    return false;

                Log(CLanguage.Lan("条码") + "【" + serialNo + "】" +
                    CLanguage.Lan("检查冠佳Web正常:") + watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.OK);

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
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
                    StatName = CYOHOOApp.PRETEST_FlowName,
                    SerialNo = Sn,
                    OrderName = string.Empty,
                    Remark1 = string.Empty,
                    Remark2 = string.Empty
                };

                if (mes.CheckSn(Info, out er))
                {
                    Log(CLanguage.Lan("条码") + "【" + Sn + "】" + CLanguage.Lan("当前站别为【初测位】,MES检查条码OK"), udcRunLog.ELog.Action);
                    return true;
                }

                if (mes.State == EMesState.网络异常)
                {
                    SetMesStatus(1, "MES系统网络异常,请检查网络是否正常?");
                    return false;
                }

                if (CGlobalPara.SysPara.Mes.ChkSnBI)
                {
                    Info.StatName = CYOHOOApp.BI_FlowName;

                    if (Info.StatName != string.Empty && mes.CheckSn(Info, out er))
                    {
                        Log(CLanguage.Lan("条码") + "【" + Sn + "】" + CLanguage.Lan("当前站别为【老化位】,MES检查条码OK"), udcRunLog.ELog.Action);

                        return true;
                    }
                }

                if (CGlobalPara.SysPara.Mes.ChkSnHP)
                {
                    Info.StatName = CYOHOOApp.HIPOT_FlowName;

                    if (Info.StatName != string.Empty && mes.CheckSn(Info, out er))
                    {
                        Log(CLanguage.Lan("条码") + "【" + Sn + "】" + CLanguage.Lan("当前站别为【高压位】,MES检查条码OK"), udcRunLog.ELog.Action);
                        return true;
                    }
                }

                if (CGlobalPara.SysPara.Mes.ChkSnATE)
                {
                    Info.StatName = CYOHOOApp.ATE_FlowName;

                    if (Info.StatName != string.Empty && mes.CheckSn(Info, out er))
                    {
                        Log(CLanguage.Lan("条码") + "【" + Sn + "】" + CLanguage.Lan("当前站别为【ATE】,MES检查条码OK"), udcRunLog.ELog.Action);
                        return true;
                    }
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
        /// 上传条码
        /// </summary>
        /// <returns></returns>
        private bool TranSn()
        {
            try
            {
                string er = string.Empty;

                bool uutPass = false;

                CSFCS mes = new CSFCS(0, "WebAPI", CYOHOOApp.Custom);

                if (mes.State != EMesState.正常)
                {
                    er = mes.Message;
                    return false;
                }

                for (int i = 0; i < _statPreTest.test.Fixture.SerialNo.Count; i++)
                {
                    if (_statPreTest.test.Fixture.SerialNo[i] == string.Empty)
                        continue;

                    CSFCS.CSnData SnData = new CSFCS.CSnData()
                    {
                        DeviceId = "0",
                        DeviceName = CNet.HostIP(),
                        StatName = CYOHOOApp.PRETEST_FlowName,
                        StatDesc = CLanguage.Lan("通电测试"),
                        Fixture = _statPreTest.test.Fixture.IdCard + "_" + (i + 1).ToString("D2"),
                        LocalName = _statPreTest.test.ToString(),
                        StartTime = _statPreTest.test.Para.StartTime,
                        EndTime = _statPreTest.test.Para.EndTime,
                        Model = _runModel.Base.Model,
                        OrderName = CYOHOOApp.OrderName,
                        SerialNo = _statPreTest.test.Fixture.SerialNo[i],
                        Result = _statPreTest.test.Fixture.Result[i],
                        Remark1 = string.Empty,
                        Remark2 = string.Empty,
                        Item = new List<CSFCS.CSnItem>()
                    };

                    string errCode = string.Empty;
                    string errInfo = string.Empty;
                    if (_statPreTest.test.Fixture.Volt[i] < 2)
                    {
                        errCode = "001";
                        errInfo = CLanguage.Lan("无输出");
                    }
                    else if (_statPreTest.test.Fixture.Volt[i] > _runModel.OutPut.Vmax)
                    {
                        errCode = "002";
                        errInfo = CLanguage.Lan("电压偏高");
                    }
                    else if (_statPreTest.test.Fixture.Volt[i] < _runModel.OutPut.Vmin)
                    {
                        errCode = "003";
                        errInfo = CLanguage.Lan("电压偏低");
                    }

                    CSFCS.CSnItem SnItem1 = new CSFCS.CSnItem()
                    {
                        IdNo = 0,
                        Name = CLanguage.Lan("电压测试"),
                        Desc = _runModel.OutPut.Vname,
                        LowLimit = _runModel.OutPut.Vmin.ToString(),
                        UpLimit = _runModel.OutPut.Vmax.ToString(),
                        Value = _statPreTest.test.Fixture.Volt[i].ToString("0.000"),
                        Unit = "V",
                        Result = (_statPreTest.test.Fixture.Volt[i] >= _runModel.OutPut.Vmin && _statPreTest.test.Fixture.Volt[i] <= _runModel.OutPut.Vmax) ? 0 : 1,
                        ErroCode = errCode,
                        ErrInfo = errInfo,
                        Remark1 = string.Empty,
                        Remark2 = string.Empty
                    };
                    SnData.Item.Add(SnItem1);

                    errCode = string.Empty;
                    errInfo = string.Empty;
                    if (_statPreTest.test.Fixture.Volt[i] < 2)
                    {
                        errCode = "001";
                        errInfo = CLanguage.Lan("无输出");
                    }
                    else if (_statPreTest.test.Fixture.Cur[i] > _runModel.OutPut.LoadMax)
                    {
                        errCode = "004";
                        errInfo = CLanguage.Lan("电流偏高");
                    }
                    else if (_statPreTest.test.Fixture.Cur[i] < _runModel.OutPut.LoadMin)
                    {
                        errCode = "005";
                        errInfo = CLanguage.Lan("电流偏低");
                    }
                    CSFCS.CSnItem SnItem2 = new CSFCS.CSnItem()
                    {
                        IdNo = 1,
                        Name = CLanguage.Lan("电流测试"),
                        Desc = _runModel.OutPut.LoadSet.ToString() + "A",
                        LowLimit = _runModel.OutPut.LoadMin.ToString(),
                        UpLimit = _runModel.OutPut.LoadMax.ToString(),
                        Value = _statPreTest.test.Fixture.Cur[i].ToString("0.000"),
                        Unit = "A",
                        Result = (_statPreTest.test.Fixture.Cur[i] >= _runModel.OutPut.Vmin &&
                                  _statPreTest.test.Fixture.Cur[i] <= _runModel.OutPut.Vmax) ? 0 : 1,
                        ErroCode = errCode,
                        ErrInfo = errInfo,
                        Remark1 = string.Empty,
                        Remark2 = string.Empty
                    };
                    SnData.Item.Add(SnItem2);

                    if (_runModel.OutPut.ChkDD)
                    {
                        string[] C_D_GND = new string[] { "D+V-", "D-V-", "D+D-", "D-V+" };
                        string openValue = string.Empty;
                        string shortValue = string.Empty;
                        for (int z = 0; z < _runModel.OutPut.ChkDG.Length; z++)
                        {
                            if (_runModel.OutPut.ChkDG[i])
                                shortValue += C_D_GND[i] + ";";
                            else
                                openValue += C_D_GND[i] + ";";
                        }
                        CSFCS.CSnItem SnItem3 = new CSFCS.CSnItem()
                        {
                            IdNo = 1,
                            Name = "D+D-",
                            Desc = "D+D-",
                            LowLimit = shortValue,
                            UpLimit = openValue,
                            Value = _statPreTest.test.Fixture.DD[i],
                            Unit = "-",
                            Result = (_statPreTest.test.Fixture.DD[i] == "正常") ? 0 : 1,
                            ErroCode = errCode,
                            ErrInfo = errInfo,
                            Remark1 = string.Empty,
                            Remark2 = string.Empty
                        };
                        SnData.Item.Add(SnItem3);
                    }

                    string strResult = _statPreTest.test.Fixture.Result[i] == 0 ? "PASS" : "FAIL";

                    if (!mes.TranSnData(SnData, out er))
                    {
                        Log(CLanguage.Lan("治具ID") + "[" + _statPreTest.test.Fixture.IdCard + "-" + (i + 1).ToString("D2") + "]" +
                            CLanguage.Lan("条码") + "【" + _statPreTest.test.Fixture.SerialNo[i] + "】" +
                            CLanguage.Lan("上传MES结果") + "【" + strResult + "】" + "错误:" + er, udcRunLog.ELog.NG);

                        if (mes.State == EMesState.网络异常)
                        {
                            SetMesStatus(1, "MES系统网络异常,请检查网络是否正常?");
                            break;
                        }
                    }
                    else
                    {
                        uutPass = true;

                        Log(CLanguage.Lan("治具ID") + "[" + _statPreTest.test.Fixture.IdCard + "-" + (i + 1).ToString("D2") + "]" +
                            CLanguage.Lan("条码") + "【" + _statPreTest.test.Fixture.SerialNo[i] + "】" +
                            CLanguage.Lan("上传MES结果") + "【" + strResult + "】OK", udcRunLog.ELog.OK);
                    }
                }

                if (!uutPass)
                {
                    SetMesStatus(2, "治具ID[" + _statPreTest.test.Fixture.IdCard + "]上传MES结果所有FAIL,请检查.");
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

        #region 测试报表
        /// <summary>
        /// 保存日报表
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool SaveDailyReport(out string er)
        {
            er = string.Empty;

            try
            {
                if (!CGlobalPara.SysPara.Report.SaveReport)
                    return true;

                string fileName = createDailyReportFile();

                if (!File.Exists(fileName))
                {
                    saveReportTitle(fileName);
                }

                saveReportVal(fileName);

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
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
                    CGlobalPara.SysPara.Report.ReportPath = Application.StartupPath + "\\Report";

                string reportPath = CGlobalPara.SysPara.Report.ReportPath + "\\";

                reportPath += _runModel.Base.Model;

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

                strWrite = "ModelName:," + _runModel.Base.Model;
                sw.WriteLine(strWrite);

                strWrite = "Input AC:," + _runModel.OutPut.ACVolt.ToString() + "V";
                sw.WriteLine(strWrite);

                strWrite = "Input Spec.:," + _runModel.OutPut.ACvMin.ToString() + "V-" + _runModel.OutPut.ACvMax.ToString() + "V";
                sw.WriteLine(strWrite);

                strWrite = "Output:," + _runModel.OutPut.Vname + "V";
                sw.WriteLine(strWrite);

                strWrite = "OutPut Spec:," + _runModel.OutPut.Vmin.ToString() + "V-" + _runModel.OutPut.Vmax.ToString() + "V";
                sw.WriteLine(strWrite);

                strWrite = "Load Setting:," + _runModel.OutPut.LoadSet.ToString() + "A";
                sw.WriteLine(strWrite);

                strWrite = "Load Spec:," + _runModel.OutPut.LoadMin.ToString() + "A-" + _runModel.OutPut.LoadMax.ToString() + "A";
                sw.WriteLine(strWrite);

                sw.WriteLine("");

                strWrite = "ID Card,Slot,SerialNo,StartTime,EndTime,Result,Voltage(V),Current(A),Short Status";
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
        private void saveReportVal(string fileName)
        {
            try
            {
                string strWrite = string.Empty;

                StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                for (int i = 0; i < _statPreTest.test.Fixture.SerialNo.Count; i++)
                {

                    if (_statPreTest.test.Fixture.SerialNo[i] != string.Empty)
                    {
                        strWrite = "FIX-" + _statPreTest.test.Fixture.IdCard + ",";

                        strWrite += (i + 1).ToString("D2") + ",";

                        strWrite += _statPreTest.test.Fixture.SerialNo[i] + ",";

                        strWrite += _statPreTest.test.Para.StartTime + ",";

                        strWrite += _statPreTest.test.Para.EndTime + ",";

                        if (_statPreTest.test.Fixture.Result[i] == 0)
                            strWrite += "PASS,";
                        else
                            strWrite += "FAIL,";

                        strWrite += _statPreTest.test.Fixture.Volt[i].ToString("0.000") + ",";

                        strWrite += _statPreTest.test.Fixture.Cur[i].ToString("0.000") + ",";

                        if (_runModel.OutPut.ChkDD)
                        {
                            strWrite += _statPreTest.test.Fixture.DD[i];
                        }
                        else
                        {
                            strWrite += "-";
                        }

                        sw.WriteLine(strWrite);
                    }
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
                    PLCLog("<" + _PLCAlarmList.ToString() + ">" + "解除[" + releaseReg[i].RegFun + "]", udcRunLog.ELog.Action);
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
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].TTNum = _statInfo.TTNum;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].FailNum = _statInfo.FailNum;
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

        #region 加密狗
        /// <summary>
        /// 检测加密狗状态
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckSafeDog(out string er)
        {
            er = string.Empty;

            try
            {
                string serialNo = string.Empty;

                int leftDay = -1;

                if (CYOHOOApp.DogLock == CYOHOOApp.DogRelease) //不检查加密狗
                    return true;

                CSentinel dog = new CSentinel();

                if (!dog.check_safe_dog(CYOHOOApp.DogID, out leftDay, out serialNo, out er))
                {
                    er = CLanguage.Lan("未找不到软件加密狗,请确认已插上加密狗?") + "\r\n" + CLanguage.Lan("错误信息:") + er;
                    return false;
                }

                if (leftDay <= 0)
                {
                    er = CLanguage.Lan("软件使用期限到期,请与我司业务人员联系") + CYOHOOApp.DogLiaisons;
                    return false;

                }
                else if (leftDay < CYOHOOApp.DogDayLimit)
                {
                    er = CLanguage.Lan("软件使用期限还剩") + leftDay.ToString() + CLanguage.Lan("天") + "," +
                          CLanguage.Lan("请及时与我司业务人员联系") + CYOHOOApp.DogLiaisons;
                    return false;
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
        /// 加密狗检查
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckSafeDog()
        {
            string er = string.Empty;

            try
            {
                string serialNo = string.Empty;

                int leftDay = -1;

                if (CYOHOOApp.DogLock == CYOHOOApp.DogRelease) //不检查加密狗
                {
                    return true;
                }

                CSentinel dog = new CSentinel();

                if (!dog.check_safe_dog(CYOHOOApp.DogID, out leftDay, out serialNo, out er))
                {
                    MessageBox.Show(CLanguage.Lan("未找不到软件加密狗,请确认已插上加密狗?") + "\r\n" +
                                     CLanguage.Lan("错误信息:") + er, "Tip",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                if (leftDay <= 0)
                {
                    FrmDogLock.CreateInstance(serialNo).ShowDialog();

                    while (FrmDogLock.LogOK == 0)
                    {
                        Application.DoEvents();
                    }

                    if (FrmDogLock.LogOK == -1)
                    {
                        return false;
                    }
                }
                else if (leftDay < CYOHOOApp.DogDayLimit)
                {
                    MessageBox.Show(CLanguage.Lan("软件使用期限还剩") + leftDay.ToString() + CLanguage.Lan("天") + "," +
                                    CLanguage.Lan("请及时与我司业务人员联系") + CYOHOOApp.DogLiaisons, "Tip",
                                     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
        #endregion

        #region 日产能报表
        /// <summary>
        /// 日产能统计
        /// </summary>
        private CDailyYield _DailyYield = new CDailyYield();
        /// <summary>
        /// 保存日产能
        /// </summary>
        private bool SaveDailyRecord(int ttNum, int failNum, out string er)
        {
            er = string.Empty;

            try
            {
                if (_DailyYield.DayNow == string.Empty)
                {
                    _DailyYield.DayNow = DateTime.Today.ToString("yyyy-MM-dd");
                    _DailyYield.TTNum = 0;
                    _DailyYield.FailNum = 0;
                }

                _DailyYield.TTNum += ttNum;
                _DailyYield.FailNum += failNum;
                if (_DailyYield.TTNum != 0)
                {
                    _DailyYield.PassRate = (double)(_DailyYield.TTNum - _DailyYield.FailNum) / (double)_DailyYield.TTNum;
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
                strXml += "<!--日产能统计-->" + "\r\n";
                strXml += "<General>" + "\r\n";
                strXml += "<!--总数-->" + "\r\n";
                strXml += "<TolNum>" + _DailyYield.TTNum.ToString() + "</TolNum>" + "\r\n";
                strXml += "<!--良品数-->" + "\r\n";
                strXml += "<PassNum>" + (_DailyYield.TTNum - _DailyYield.FailNum).ToString() + "</PassNum>" + "\r\n";
                strXml += "<!--不良品数-->" + "\r\n";
                strXml += "<FailNum>" + _DailyYield.FailNum.ToString() + "</FailNum>" + "\r\n";
                strXml += "<!--直通率(%)-->" + "\r\n";
                strXml += "<PassRate>" + _DailyYield.PassRate.ToString("P2") + "</PassRate>" + "\r\n";
                strXml += "</General>" + "\r\n";
                strXml += "</ConfigSet>" + "\r\n";
                StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
                sw.Write(strXml);
                sw.Flush();
                sw.Close();

                _DailyYield.TTNum = 0;
                _DailyYield.FailNum = 0;
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
            CIniFile.WriteToIni("DailyYield", "TTNum", _DailyYield.TTNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("DailyYield", "FailNum", _DailyYield.FailNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("DailyYield", "DayNow", _DailyYield.DayNow, CGlobalPara.IniFile);
        }
        /// <summary>
        /// 加载INI
        /// </summary>
        private void LoadIniForDaily()
        {
            _DailyYield.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "TTNum", CGlobalPara.IniFile, "0"));
            _DailyYield.FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "FailNum", CGlobalPara.IniFile, "0"));
            _DailyYield.DayNow = CIniFile.ReadFromIni("DailyYield", "DayNow", CGlobalPara.IniFile);

        }
        #endregion

        #region 良品预警功能
        private int _WarnIdNo = 0;
        private string _WarnName = "PRETEST";
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

    }
}
