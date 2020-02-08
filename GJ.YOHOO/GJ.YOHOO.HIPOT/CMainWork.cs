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
using GJ.DEV.HIPOT;
using GJ.DEV.RemoteIO;
using GJ.DEV.COM;
using GJ.USER.APP;
using GJ.YOHOO.HIPOT.Udc;
using GJ.MES;
using GJ.PDB;
using GJ.Iot;
using GJ.USER.APP.Iot;
using GJ.SFCS;

namespace GJ.YOHOO.HIPOT
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

                if (!load_user_plc_reg(out er))
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

                _defaultModelPath = CIniFile.ReadFromIni("Parameter", "ModelPath", CGlobalPara.IniFile);

                _yield.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "TTNum", CGlobalPara.IniFile, "0"));

                _yield.FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Parameter", "FailNum", CGlobalPara.IniFile, "0"));

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

                OnUIModelArgs.OnEvented(new CUIModelArgs(_idNo, _name, _runModel));
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void LoadMainFormUI()
        {
            UIMainArgs.TTNum = _yield.TTNum;

            UIMainArgs.FailNum = _yield.FailNum;

            UIMainArgs.DoRun = EUIStatus.设置产能;

            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

        }
        public override void LoadUIComplete()
        {
            MainWorker = new BackgroundWorker();
            MainWorker.WorkerSupportsCancellation = true;
            MainWorker.WorkerReportsProgress = true;
            MainWorker.DoWork += new DoWorkEventHandler(MainWorker_DoWork);
            OnUIGlobalArgs.OnEvented(new CUIGlobalArgs());

            InitialIot();
        }
        #endregion

        #region 实现抽象方法
        public override bool InitialRunPara()
        {
            try
            {
                if (_runModel == null || _runModel.Base.Model == null)
                {
                    MessageBox.Show(CLanguage.Lan("请选择要测试机种名称,再启动监控."), "Tip",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                if (_hpIniModelPara != 0)
                {
                    MessageBox.Show(CLanguage.Lan("正在初始化高压设备参数,请稍等."), "Tip",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                {
                    _devRun[i].DoRun = ERUN.空闲;
                    _devRun[i].DebugMode = false;
                    _devRun[i].Running = false;
                }

                _statHP.DoRun = ERUN.空闲;                

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(),udcRunLog.ELog.Err);

                return false;
            }          
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

                for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                {
                    int index = 0;

                    IniTask.Add(Task.Factory.StartNew(() => OpenHP(index)));
                }               

                if (CGlobalPara.SysPara.Dev.ChkIoEnable)
                {
                    IniTask.Add(Task.Factory.StartNew(() => OpenIO()));
                }

                IniTask.Add(Task.Factory.StartNew(() => OpenTCP()));

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
            for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
            {
                CloseHP(0);
            }           

            CloseIO();

            CloseTCP();
        }
        public override bool StartThread()
        {
            try
            {
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

                //停止已测试设备任务
                for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                {
                    if (_devRun[i].Running)
                        _devRun[i].DoRun = ERUN.空闲;
                }

                while (true)
                {
                    Application.DoEvents();

                    bool taskEnd = true;

                    for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                    {
                        if (_devRun[i].Running)
                            taskEnd = false;
                    }
                    if (taskEnd)
                        break;
                }

                //销毁主线程                
                if (MainWorker.IsBusy)
                    MainWorker.CancelAsync();

                while (MainWorker.IsBusy)
                {
                    Application.DoEvents();
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
        public COnEvent<CUIModelArgs> OnUIModelArgs = new COnEvent<CUIModelArgs>();
        /// <summary>
        /// TCP运行日志
        /// </summary>
        public COnEvent<CUILogArgs> OnTCPLogArgs = new COnEvent<CUILogArgs>();
        /// <summary>
        /// 面板消息
        /// </summary>
        public COnEvent<CUIUserArgs<CUIMainArgs>> OnUIMainArgs =new COnEvent<CUIUserArgs<CUIMainArgs>>();
        #endregion

        #region 加载用户参数
        /// <summary>
        /// 加载PLC读写寄存器
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool load_user_plc_reg(out string er)
        {
            er = string.Empty;

            try
            {
                

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

                _statHP = new CStat(_idNo, "<" + CLanguage.Lan("高压测试工位") + ">",
                                                CYOHOOApp.HIPOT_FlowId,
                                                CYOHOOApp.HIPOT_FlowName, 
                                                CYOHOOApp.SlotMax);

                _devHP = new CHPCom[CGlobalPara.SysPara.Dev.HPDevMax]; 

                _devRun = new CDev[CGlobalPara.SysPara.Dev.HPDevMax];

                for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                {
                    int slotNum = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.HPDevMax;

                    _devRun[idNo] = new CDev(idNo, "<"+ CLanguage.Lan("高压仪器设备") + (idNo + 1).ToString() + ">", slotNum);
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

        #region 面板方法
        /// <summary>
        /// 选择机种
        /// </summary>
        /// <param name="modelName"></param>
        public void OnFrmMainChangeModel(string modelName)
        {
            try
            {
                _defaultModelPath = modelName;

                LoadModelPara();

                UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Initialize;

                UIMainArgs.DoRun = EUIStatus.设置状态;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                _hpIniModelPara = 1;

                BackgroundWorker IntHPWorker = new BackgroundWorker();

                IntHPWorker.WorkerSupportsCancellation = true;

                IntHPWorker.WorkerReportsProgress = true;

                IntHPWorker.DoWork += new DoWorkEventHandler(IntHPWorker_DoWork);

                IntHPWorker.RunWorkerAsync();

                while (IntHPWorker.IsBusy)
                {
                    Application.DoEvents();

                    UIMainArgs.WatcherTime = watcher.ElapsedMilliseconds;

                    UIMainArgs.DoRun = EUIStatus.设置时间;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));
                }

                UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Idle;

                UIMainArgs.DoRun = EUIStatus.设置状态;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err); 
            }
        }
        /// <summary>
        /// 调试模式
        /// </summary>
        /// <param name="idNo"></param>
        public void OnFrmMainDebugMode(int idNo,int runStatus)
        {
            if (runStatus == 1)
            {
                SetHPToDebug(idNo);
            }
            else
            {
                _devRun[idNo].DoRun = ERUN.空闲;

            }

            UIMainArgs.BtnValue = runStatus;

            UIMainArgs.BtnNo = idNo;

            UIMainArgs.DoRun = EUIStatus.设置按钮;

            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
        }
        /// <summary>
        /// 清除产能计数
        /// </summary>
        public void OnFrmMainClearYield()
        {
            _yield.TTNum = 0;

            _yield.FailNum = 0;

            CIniFile.WriteToIni("Parameter", "TTNum", _yield.TTNum.ToString(), CGlobalPara.IniFile);

            CIniFile.WriteToIni("Parameter", "FailNum", _yield.FailNum.ToString(), CGlobalPara.IniFile);

            UIMainArgs.TTNum = _yield.TTNum;

            UIMainArgs.FailNum = _yield.FailNum;

            UIMainArgs.DoRun = EUIStatus.设置产能;

            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

        }
        #endregion

        #region 字段
        /// <summary>
        /// 高压设备
        /// </summary>
        private CHPCom[] _devHP = null;
        /// <summary>
        /// 高压IO板
        /// </summary>
        private CIOCom _devIO = null;
        /// <summary>
        /// TCP客户端
        /// </summary>
        private CClientTCP _devTCP = null;
        /// <summary>
        /// 机种参数
        /// </summary>
        private CModelPara _runModel = new CModelPara();
        /// <summary>
        /// 工位信息
        /// </summary>
        private CStat _statHP = null;
        /// <summary>
        /// 高压仪器测试
        /// </summary>
        private CDev[] _devRun = null;
        /// <summary>
        /// TCP重新连接时间
        /// </summary>
        private int _conToTcpAgain = 0;
        /// <summary>
        /// TCP重新连接次数
        /// </summary>
        private int _conToTcpCount = 0;
        /// <summary>
        /// TCP通信失败次数
        /// </summary>
        private int _conRecvTimeOut = 0;
        /// <summary>
        /// 高压设备初始化参数
        /// </summary>
        private int _hpIniModelPara = 0;
        /// <summary>
        /// 机种路径
        /// </summary>
        private string _defaultModelPath = string.Empty;
        /// <summary>
        /// 产能统计
        /// </summary>
        private CYield _yield = new CYield();
        /// <summary>
        /// IO同步锁
        /// </summary>
        private ReaderWriterLock ioLock = new ReaderWriterLock();
        #endregion

        #region 打开与关闭设备
        /// <summary>
        /// 打开高压机串口
        /// </summary>
        private bool OpenHP(int idNo)
        {
            try
            {
                string er = string.Empty;

                if (_devHP[idNo] == null)
                {
                    _devHP[idNo] = new CHPCom(CGlobalPara.SysPara.Dev.HPType, idNo, CGlobalPara.SysPara.Dev.HPType.ToString());

                    string comSetting = CGlobalPara.SysPara.Dev.HPCom[idNo];

                    string comBaud = CGlobalPara.SysPara.Dev.HPBaud;

                    if (!_devHP[idNo].Open(comSetting, out er, comBaud))
                    {
                        _devHP[idNo].Close();
                        _devHP[idNo] = null;
                        Log(CLanguage.Lan("打开高压机") + (idNo + 1).ToString() + CLanguage.Lan("串口") + "[" +
                                                                                  CGlobalPara.SysPara.Dev.HPCom[idNo] + "]" +
                                                                                  CLanguage.Lan("失败") + ":" + er, udcRunLog.ELog.NG);
                        return false;
                    }

                    Thread.Sleep(50);

                    if (!_devHP[idNo].Init(out er))
                    {
                        Thread.Sleep(50);

                        if (!_devHP[idNo].Init(out er))
                        {
                            _devHP[idNo].Close();
                            _devHP[idNo] = null;
                            Log(CLanguage.Lan("初始化高压机") + (idNo + 1).ToString() + CLanguage.Lan("失败") + ":" + er, udcRunLog.ELog.NG);
                            return false;
                        }
                    }

                    Log(CLanguage.Lan("成功初始化高压机") + (idNo + 1).ToString(), udcRunLog.ELog.Action);
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
        /// 关闭高压机串口
        /// </summary>
        private void CloseHP(int idNo)
        {
            try
            {
                if (_devHP[idNo] != null)
                {
                    _devHP[idNo].Close();
                    _devHP[idNo] = null;
                    Log(CLanguage.Lan("关闭高压机") + (idNo + 1).ToString() + CLanguage.Lan("通信端口"), udcRunLog.ELog.Content);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开IO板
        /// </summary>
        /// <returns></returns>
        private bool OpenIO()
        {
            try
            {
                string er = string.Empty;

                if (_devIO == null)
                {
                    _devIO = new CIOCom(EType.IO_24_16, 0, CLanguage.Lan("高压IO板"));

                    if (!_devIO.Open(CGlobalPara.SysPara.Dev.IoCom, out er))
                    {
                        _devIO.Close();
                        _devIO = null;
                        Log(CLanguage.Lan("打开高压IO板串口") + "[" + CGlobalPara.SysPara.Dev.IoCom + "]"+ CLanguage.Lan("失败") + ":" + er, udcRunLog.ELog.NG);
                        return false;
                    }

                    int rVal = 0;

                    Thread.Sleep(50);

                    if (!_devIO.ReadVersion(1, out rVal, out er))
                    {
                        Thread.Sleep(50);

                        if (!_devIO.ReadVersion(1, out rVal, out er))
                        {
                            _devIO.Close();
                            _devIO = null;
                            Log(CLanguage.Lan("读取高压IO板串口") + "[" + CGlobalPara.SysPara.Dev.IoCom + "]" + CLanguage.Lan("失败") + ":" + er, udcRunLog.ELog.NG);
                            return false;
                        }
                    }

                    Log(CLanguage.Lan("成功初始化高压IO板串口") + "[" + CGlobalPara.SysPara.Dev.IoCom + "]", udcRunLog.ELog.Action);
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
        /// 关闭IO板
        /// </summary>
        private void CloseIO()
        {
            try
            {
                if (_devIO != null)
                {
                    _devIO.Close();
                    _devIO = null;
                    Log(CLanguage.Lan("关闭高压IO板串口") + "[" + CGlobalPara.SysPara.Dev.IoCom + "]", udcRunLog.ELog.Content);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 连接TCP服务端
        /// </summary>
        /// <returns></returns>
        private bool OpenTCP()
        {
            try
            {
                string er = string.Empty;

                if (_devTCP == null)
                {
                    _devTCP = new CClientTCP(0, CGlobalPara.SysPara.Dev.SerStat, EDataType.ASCII格式);
                    _devTCP.OnConed += new CClientTCP.EventOnConHander(OnTcpCon);
                    _devTCP.OnRecved += new CClientTCP.EventOnRecvHandler(OnTcpRecv);
                    _devTCP.open(CGlobalPara.SysPara.Dev.SerIP, out er, CGlobalPara.SysPara.Dev.SerPort.ToString());

                    Stopwatch watcher = new Stopwatch();

                    watcher.Start();

                    while (true)
                    {
                        Application.DoEvents();

                        if (_devTCP.conStatus)
                            break;
                        if (watcher.ElapsedMilliseconds > 2000)
                            break;
                    }

                    watcher.Stop();

                    if (!_devTCP.conStatus)
                    {
                        _devTCP.OnConed -= new CClientTCP.EventOnConHander(OnTcpCon);
                        _devTCP.OnRecved -= new CClientTCP.EventOnRecvHandler(OnTcpRecv);
                        _devTCP.close();
                        _devTCP = null;
                        Log(CLanguage.Lan("无法连接测试服务端") + "[" + CGlobalPara.SysPara.Dev.SerIP + ":" +
                                   CGlobalPara.SysPara.Dev.SerPort.ToString() + "]", udcRunLog.ELog.NG);
                        return false;
                    }

                    Log(CLanguage.Lan("正常连接测试服务端")+"[" + CGlobalPara.SysPara.Dev.SerIP + ":" +
                                CGlobalPara.SysPara.Dev.SerPort.ToString() + "]", udcRunLog.ELog.Action);
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
        /// 断开TCP连接
        /// </summary>
        private void CloseTCP()
        {
            try
            {
                if (_devTCP != null)
                {
                    _devTCP.OnConed -= new CClientTCP.EventOnConHander(OnTcpCon);
                    _devTCP.OnRecved -= new CClientTCP.EventOnRecvHandler(OnTcpRecv);
                    _devTCP.close();
                    _devTCP = null;
                    Log(CLanguage.Lan("断开连接测试服务端") + "[" + CGlobalPara.SysPara.Dev.SerIP + ":" +
                                      CGlobalPara.SysPara.Dev.SerPort.ToString() + "]", udcRunLog.ELog.Content);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 重新连接TCP服务端
        /// </summary>
        private void ConnectToTCPAgain()
        {
            try
            {
                if (_devTCP != null)
                {
                    _devTCP.OnConed -= new CClientTCP.EventOnConHander(OnTcpCon);
                    _devTCP.OnRecved -= new CClientTCP.EventOnRecvHandler(OnTcpRecv);
                    _devTCP.close();
                    _devTCP = null;
                }

                _conToTcpCount++;

                string er = string.Empty;

                _devTCP = new CClientTCP(0, CGlobalPara.SysPara.Dev.SerStat, EDataType.ASCII格式);
                _devTCP.OnConed += new CClientTCP.EventOnConHander(OnTcpCon);
                _devTCP.OnRecved += new CClientTCP.EventOnRecvHandler(OnTcpRecv);
                _devTCP.open(CGlobalPara.SysPara.Dev.SerIP, out er, CGlobalPara.SysPara.Dev.SerPort.ToString());

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                while (true)
                {
                    Application.DoEvents();

                    if (_devTCP.conStatus)
                        break;
                    if (watcher.ElapsedMilliseconds > 1000)
                        break;
                }

                watcher.Stop();

                if (!_devTCP.conStatus)
                {
                    _devTCP.OnConed -= new CClientTCP.EventOnConHander(OnTcpCon);
                    _devTCP.OnRecved -= new CClientTCP.EventOnRecvHandler(OnTcpRecv);
                    _devTCP.close();
                    _devTCP = null;
                    Log(CLanguage.Lan("尝试重新连接测试服务端") + "[" + CGlobalPara.SysPara.Dev.SerIP + ":" +
                                      CGlobalPara.SysPara.Dev.SerPort.ToString() + "]"+ CLanguage.Lan("失败") + ":" + 
                                      _conToTcpCount.ToString(), udcRunLog.ELog.NG);
                    return;
                }

                Log(CLanguage.Lan("重新连接测试服务端") + "[" + CGlobalPara.SysPara.Dev.SerIP + ":" +
                                   CGlobalPara.SysPara.Dev.SerPort.ToString() + "]", udcRunLog.ELog.Action);

                _conToTcpCount = 0;
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

                Log(CLanguage.Lan("连接冠佳WEB") + "【" + CYOHOOApp.UlrWeb + "】OK", udcRunLog.ELog.Action);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
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

                        if (!CheckMESStatus())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckStatReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        if (!CheckStatTest())
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
        /// TCP/IP连接检查
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool CheckSystem(int delayTimes)
        {
            try
            {
                string er = string.Empty;

                //5S重连接1次
                int counT1 = 3000 / delayTimes;

                if (_devTCP == null || !_devTCP.conStatus || _conRecvTimeOut > CGlobalPara.C_ALARM_TIME)
                {
                    if (_conToTcpAgain < counT1)
                    {
                        _conToTcpAgain++;
                    }
                    else
                    {
                        ConnectToTCPAgain();
                        _conToTcpAgain = 0;
                        _conRecvTimeOut = 0;
                    }
                    return false;
                }

                _conToTcpAgain = 0;

                _conToTcpCount = 0;

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
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 检测工位到位信号
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool CheckStatReady()
        {
            try
            {
                string er = string.Empty;

                int idNo = 0;

                CSerReponse serReponse = null;

                if (!QUERY_STATE(CGlobalPara.SysPara.Dev.SerStat, out serReponse, out er))
                {
                    _conRecvTimeOut++;
                    Log(er, udcRunLog.ELog.NG);
                    return false;
                }

                _conRecvTimeOut = 0;

                if (serReponse.Ready == 0)
                {
                    _statHP.DoRun = ERUN.空闲;
                    
                    for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                    {
                        _devRun[i].DoRun = ERUN.空闲;
                    }

                    UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Idle;

                    UIMainArgs.DoRun = EUIStatus.设置状态;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                    return true;
                }

                if (_statHP.DoRun != ERUN.空闲)
                    return true;

                //停止已测试设备任务
                for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                {
                    if (_devRun[i].Running)
                        _devRun[i].DoRun = ERUN.空闲;
                }

                while (true)
                {
                    Application.DoEvents();

                    bool taskEnd = true;

                    for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                    {
                        if (_devRun[i].Running)
                            taskEnd = false;
                    }
                    if (taskEnd)
                        break;
                }

                _statHP.Ready = serReponse.Ready;
                _statHP.IdCard = serReponse.IdCard;
                _statHP.ModelName = serReponse.ModelName;
                _statHP.MesFlag = serReponse.MesFlag;
                _statHP.DoRun = ERUN.到位;

                for (int i = 0; i < _statHP.SerialNo.Count; i++)
                {
                    _statHP.SerialNo[i] = serReponse.SerialNos[i];
                    _statHP.Result[i] = 0;
                    UIMainArgs.SlotNo = i;
                    UIMainArgs.CurSn = _statHP.SerialNo[i];
                    UIMainArgs.DoRun = EUIStatus.设置条码;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
                }

                UIMainArgs.IdCard = _statHP.IdCard;

                UIMainArgs.DoRun = EUIStatus.治具就绪;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Ready;

                UIMainArgs.DoRun = EUIStatus.设置状态;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 控制工位测试
        /// </summary>
        /// <returns></returns>
        private bool CheckStatTest()
        {
            try
            {
                string er = string.Empty;

                int slotMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.HPDevMax;

                switch (_statHP.DoRun)
                {
                    case ERUN.空闲:
                        break;
                    case ERUN.到位:                     
                        for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                        {
                            for (int slot = 0; slot < slotMax; slot++)
                            {
                                int uutNo = i * slotMax + slot;
                                int index = _runModel.Para.IoCH[uutNo] - 1;                                
                                _devRun[i].SerialNo[slot] = _statHP.SerialNo[index];                                
                                _devRun[i].Result[slot] = 0;
                            }
                            _devRun[i].ModelName = _statHP.ModelName;
                            _devRun[i].StepNo = 0;
                            _devRun[i].DoRun = ERUN.到位;
                            int TaskNo = i;
                            Task.Factory.StartNew(() => HP_Device_Task(TaskNo));
                        }
                        _statHP.DoRun = ERUN.等待;
                        _statHP.StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _statHP.Watcher.Restart();
                        Log(_statHP.ToString() + CLanguage.Lan("治具ID") + "【" + _statHP.IdCard + "】"+
                                                 CLanguage.Lan("到位就绪,等待启动测试"), udcRunLog.ELog.OK);
                        break;
                    case ERUN.等待:
                        UIMainArgs.WatcherTime = _statHP.Watcher.ElapsedMilliseconds;
                        UIMainArgs.DoRun = EUIStatus.设置时间;
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                        for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                        {
                            if (_devRun[idNo].DoRun == ERUN.测试)
                            {
                                if (!START_TEST(CGlobalPara.SysPara.Dev.SerStat, out er))
                                {
                                    Log(_statHP.ToString() + CLanguage.Lan("发送TCP测试信号错误:") + er, udcRunLog.ELog.NG);
                                    break;
                                }
                                _statHP.DoRun = ERUN.测试;
                                UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Testing;
                                UIMainArgs.DoRun = EUIStatus.设置状态;
                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                                break;
                            }
                        }
                        break;
                    case ERUN.测试:
                        UIMainArgs.WatcherTime = _statHP.Watcher.ElapsedMilliseconds;
                        UIMainArgs.DoRun = EUIStatus.设置时间;
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                        for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                        {
                            if (_devRun[idNo].DoRun == ERUN.结束)
                            {
                                for (int slot = 0; slot < slotMax; slot++)
                                {
                                    int uutNo = idNo * slotMax + slot;
                                    int index = _runModel.Para.IoCH[uutNo] - 1;
                                    _statHP.Result[index] = _devRun[idNo].Result[slot];
                                    _statHP.HpResult[index].mVal.Clear();                                    
                                    for (int z = 0; z < _devRun[idNo].HpResult[slot].mVal.Count; z++)
                                    {
                                        CHPPara.CVal stepVal = new CHPPara.CVal();
                                        stepVal.name = _devRun[idNo].HpResult[slot].mVal[z].name;
                                        stepVal.value = _devRun[idNo].HpResult[slot].mVal[z].value;
                                        stepVal.unit = _devRun[idNo].HpResult[slot].mVal[z].unit;
                                        stepVal.code = _devRun[idNo].HpResult[slot].mVal[z].code;
                                        stepVal.result = _devRun[idNo].HpResult[slot].mVal[z].result;
                                        _statHP.HpResult[index].mVal.Add(stepVal);
                                    }
                                }
                                _devRun[idNo].DoRun = ERUN.过站;
                            }
                        }
                        //检测高压设备是否测试结束?
                        for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                        {
                            if (_devRun[idNo].DoRun != ERUN.过站)
                                return true;
                        }
                        _statHP.EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _statHP.Watcher.Stop();                      

                        //获取高压机产品结果
                        bool uutPass = true;
                        for (int i = 0; i < _statHP.SerialNo.Count; i++)
                        {
                            if (_statHP.SerialNo[i] == string.Empty)
                            {                               
                                _statHP.Result[i] = 0;
                                _statHP.TestData[i] = string.Empty;
                            }
                            else
                            {
                                if (_statHP.Result[i] != 0)
                                {
                                    _statHP.Result[i] = CYOHOOApp.HIPOT_FlowId;
                                    uutPass = false;
                                }
                                //记录测试数据
                                CTestData UUT = new CTestData();
                                UUT.Item = new List<CTestVal>();
                                for (int step = 0; step < _statHP.HpResult[i].mVal.Count; step++)
                                {
                                    CTestVal item = new CTestVal()
                                    {
                                        StepNo = step + 1,
                                        StepName = _statHP.HpResult[i].mVal[step].name.ToString(),
                                        Value = _statHP.HpResult[i].mVal[step].value,
                                        Unit = _statHP.HpResult[i].mVal[step].unit,
                                        Result = _statHP.HpResult[i].mVal[step].result
                                    };
                                    UUT.Item.Add(item);
                                }
                                _statHP.TestData[i] = GJ.COM.CJSon.Serializer<CTestData>(UUT);
                            }
                        }

                        //写入Web结果
                        if (!UpdateFixtureResult())
                        {
                            _statHP.DoRun = ERUN.报警;
                            UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Error;
                            UIMainArgs.DoRun = EUIStatus.设置状态;
                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                            break;
                        }
                        
                        if (CGlobalPara.SysPara.Mes.Connect)
                        {
                            if (_statHP.MesFlag == 0)
                            {
                                Log(_statHP.ToString() + CLanguage.Lan("治具ID") + "[" + _statHP.IdCard + "]"+
                                                         CLanguage.Lan("未连线模式,不上传数据"), udcRunLog.ELog.NG);
                            }
                            else
                            {
                                TranSnToMES();
                                //Task.Factory.StartNew(() => TranSnToMES());
                            }
                        }

                        //发TCP过站信号    
                        if (!END_TEST(CGlobalPara.SysPara.Dev.SerStat, _statHP.IdCard, _statHP.Result, out er))
                        {
                            Log(_statHP.ToString() + CLanguage.Lan("发送TCP过站信号错误:") + er, udcRunLog.ELog.NG);
                            break;
                        }

                        if (!SaveDailyReport(out er))
                            Log(_statHP.ToString() + CLanguage.Lan("保存报表错误:") + er, udcRunLog.ELog.NG);

                        //设置产能统计
                        for (int i = 0; i < _statHP.SerialNo.Count; i++)
                        {
                            if (_statHP.SerialNo[i] != string.Empty)
                            {
                                _yield.TTNum++;
                                if (_statHP.Result[i] != 0)
                                    _yield.FailNum++;
                            }
                        }
                        CIniFile.WriteToIni("Parameter", "TTNum", _yield.TTNum.ToString(), CGlobalPara.IniFile);
                        CIniFile.WriteToIni("Parameter", "FailNum", _yield.FailNum.ToString(), CGlobalPara.IniFile);
                        UIMainArgs.TTNum = _yield.TTNum;
                        UIMainArgs.FailNum = _yield.FailNum;
                        UIMainArgs.DoRun = EUIStatus.设置产能;
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));

                        double runTimes = (double)_statHP.Watcher.ElapsedMilliseconds / 1000;
                        
                        if (uutPass)
                        {
                            UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Pass;
                            UIMainArgs.DoRun = EUIStatus.设置状态;
                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                            Log(_statHP.ToString() + CLanguage.Lan("治具ID") + "【" + _statHP.IdCard + "】" + CLanguage.Lan("高压测试:PASS,准备过站") + 
                                                                                ":" + runTimes.ToString("0.0") + "s", udcRunLog.ELog.OK);
                        }
                        else
                        {
                            UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Fail;
                            UIMainArgs.DoRun = EUIStatus.设置状态;
                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                            Log(_statHP.ToString() + CLanguage.Lan("治具ID") + "【" + _statHP.IdCard +
                                            "】"+ CLanguage.Lan("高压测试:FAIL,准备过站") + ":" + runTimes.ToString("0.0") + "s", udcRunLog.ELog.NG);
                        }
                        _statHP.DoRun = ERUN.过站;
                        break;
                    case ERUN.结束:
                        break;
                    case ERUN.报警:
                        break;
                    default:
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
        #endregion

        #region 高压任务线程
        /// <summary>
        /// 高压机测试任务
        /// </summary>
        /// <param name="idNo"></param>
        private void HP_Device_Task(int idNo)
        {
            try
            {
                string er = string.Empty;

                 Log(_devRun[idNo].ToString() + CLanguage.Lan("开始测试"), udcRunLog.ELog.Action);

                _devRun[idNo].StartTime = DateTime.Now.ToString();
                _devRun[idNo].Watcher.Restart();
                _devRun[idNo].Running = true;
                _devRun[idNo].FailStepNo = -1;

                while (true)
                {
                    try
                    {
                        Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                        switch (_devRun[idNo].DoRun)
                        {
                            case ERUN.到位:
                                on_hp_ready(idNo);
                                break;
                            case ERUN.测试:
                                on_hp_testing(idNo);
                                break;
                            default:
                                return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(_devRun[idNo].ToString() + ex.ToString(), udcRunLog.ELog.Err);
                        Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(_devRun[idNo].ToString() + ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                string er = string.Empty;

                SetIOAndHPEvent(-1, out er);
                
                _devRun[idNo].Watcher.Stop();

                _devRun[idNo].Running = false;

                double runTimes = ((double)_devRun[idNo].Watcher.ElapsedMilliseconds) / 1000;

                Log(_devRun[idNo].ToString() + CLanguage.Lan("测试结束") + ":" + runTimes.ToString("0.0") + "秒", udcRunLog.ELog.Action);

                if (_devRun[idNo].DebugMode)
                {
                    CloseHP(idNo);

                    _devRun[idNo].DebugMode = false;                 

                    bool debugEnd = true;

                    for (int i = 0; i < CGlobalPara.SysPara.Dev.HPDevMax; i++)
                    {
                        if (_devRun[idNo].DebugMode)
                            debugEnd = false;
                    }

                    if (debugEnd)
                    {
                        CloseIO();
                    }

                    UIMainArgs.BtnNo = idNo;
                    UIMainArgs.BtnValue = 0;
                    UIMainArgs.DoRun = EUIStatus.设置按钮;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
                    UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Idle;
                    UIMainArgs.DoRun = EUIStatus.设置状态;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
                }
            }
        }
        /// <summary>
        /// 高压测试到位
        /// </summary>
        /// <param name="idNo"></param>
        private void on_hp_ready(int idNo)
        {
            try
            {
                string er = string.Empty;

                //自动调用机种参数

                if (!_devRun[idNo].DebugMode && CGlobalPara.SysPara.Para.ChkAutoModel)
                {
                    if (_devRun[idNo].ModelName.ToUpper() != _runModel.Base.Model.ToUpper())
                    {
                        if (!AutoSelectHipot(idNo, _devRun[idNo].ModelName, out er))
                        {
                            _devRun[idNo].DoRun = ERUN.报警;
                            UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Error;
                            UIMainArgs.DoRun = EUIStatus.设置状态;
                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
                            Log(CLanguage.Lan("自动调用高压机") + (idNo + 1).ToString() + CLanguage.Lan("机种") + "[" + 
                                              _devRun[idNo].ModelName + "]"+ CLanguage.Lan("失败") + ":" + er, udcRunLog.ELog.NG);                                         
                            return;
                        }
                    }
                }

                if (!GetRunTestStep(idNo))
                {
                    _devRun[idNo].DoRun = ERUN.结束;
                    return;
                }

                if (!SetIOAndHPEvent(idNo, out er))
                {
                    _devRun[idNo].DoRun = ERUN.报警;

                    UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Error;

                    UIMainArgs.DoRun = EUIStatus.设置状态;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                    Log(_devRun[idNo].ToString() + CLanguage.Lan("切换IO通道错误:") + er, udcRunLog.ELog.NG);
                    
                    return;
                }

                //读高压机测试状态
                
                //EHPStatus hpRun = EHPStatus.STOPPED;

                //if (!_devHP[idNo].ReadStatus(out hpRun, out er))
                //{
                //    Log(_devRun[idNo].ToString() + CLanguage.Lan("读高压机运行状态错误:") + er, udcRunLog.ELog.NG);
                //    return;
                //}
                
                ////检查高压机是否测试结束？
                //if (hpRun == EHPStatus.RUNNING)
                //{
                //    _devHP[idNo].Stop(out er);

                //    System.Threading.Thread.Sleep(500);

                //    return;
                //}

                //启动高压机测试
                if (!_devHP[idNo].Start(out er))
                {
                    System.Threading.Thread.Sleep(500);

                    if (!_devHP[idNo].Start(out er))
                    {
                        _devRun[idNo].DoRun = ERUN.报警;

                        UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Error;

                        UIMainArgs.DoRun = EUIStatus.设置状态;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                        Log(_devRun[idNo].ToString() + CLanguage.Lan("启动高压测试错误:") + er, udcRunLog.ELog.NG);
                
                        return;
                    }
                }

                System.Threading.Thread.Sleep(500);

                Log(_devRun[idNo].ToString() + CLanguage.Lan("步骤") + "【" + (_devRun[idNo].StepNo + 1).ToString() + "】"+
                                              CLanguage.Lan("开始测试"), udcRunLog.ELog.OK);

                _devRun[idNo].RunTriger = false;

                _devRun[idNo].DoRun = ERUN.测试;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err); 
            }
        }
        /// <summary>
        /// 高压测试中
        /// </summary>
        /// <param name="idNo"></param>
        private void on_hp_testing(int idNo)
        {
            try
            {
                //读高压机测试状态

                string er = string.Empty;

                string info = string.Empty;

                EHPStatus hpStatus = EHPStatus.STOPPED;

                if (!_devHP[idNo].ReadStatus(out hpStatus, out er))
                {
                    Log(_devRun[idNo].ToString() + CLanguage.Lan("读高压机运行状态错误:") + er, udcRunLog.ELog.NG);
                    return;
                }
                //检查高压机是否测试结束？
                if (hpStatus == EHPStatus.RUNNING)
                    return;

                info = er;

                //获取高压结果
                if (!GetResultFromHP(idNo, out er))
                {
                    System.Threading.Thread.Sleep(1000);

                    if (!GetResultFromHP(idNo, out er))
                    {
                        _devRun[idNo].DoRun = ERUN.报警;

                        UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Error;

                        UIMainArgs.DoRun = EUIStatus.设置状态;

                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                        Log(_devRun[idNo].ToString() + CLanguage.Lan("获取高压测试结果错误:") + er, udcRunLog.ELog.NG);

                        return;
                    }
                }

                ///高压不良重测功能
                if (CGlobalPara.SysPara.Para.ChkReTest)
                {
                    bool uutPass = true;

                    for (int i = 0; i < CGlobalPara.SysPara.Dev.HPChanMax; i++)
                    {
                        int uutNo = CGlobalPara.SysPara.Dev.HPChanMax * _devRun[idNo].StepNo + i;

                        if (_devRun[idNo].SerialNo[uutNo] == string.Empty)
                            continue;

                        if (_devRun[idNo].Result[uutNo] != 0)
                            uutPass = false;
                    }

                    if (!uutPass)
                    {
                        if (_devRun[idNo].FailStepNo != _devRun[idNo].StepNo)
                        {
                            _devRun[idNo].FailStepNo = _devRun[idNo].StepNo;

                            _devRun[idNo].DoRun = ERUN.到位;

                            Log(_devRun[idNo].ToString() + CLanguage.Lan("测试步骤") + "[" + (_devRun[idNo].StepNo + 1).ToString() +
                                                                   "]"+ CLanguage.Lan("测试不良,启动【不良重测试】功能"), udcRunLog.ELog.NG); 

                            return;
                        }
                    }
                }

                int slotMax = CYOHOOApp.SlotMax/CGlobalPara.SysPara.Dev.HPDevMax;

                for (int i = 0; i < CGlobalPara.SysPara.Dev.HPChanMax; i++)
                {
                    int slot = CGlobalPara.SysPara.Dev.HPChanMax * _devRun[idNo].StepNo + i;

                    int uutNo = idNo * slotMax + slot;

                    int index = _runModel.Para.IoCH[uutNo] - 1;

                    UIMainArgs.SlotNo = index;

                    UIMainArgs.Result = _devRun[idNo].Result[slot];

                    UIMainArgs.HPResult = _devRun[idNo].HpResult[slot];

                    UIMainArgs.DoRun = EUIStatus.设置结果;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                }

                _devRun[idNo].StepNo++;

                if (GetRunTestStep(idNo))
                {
                    _devRun[idNo].DoRun = ERUN.到位;
                    return;

                }

                _devRun[idNo].DoRun = ERUN.结束;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 高压机加载参数任务
        private void IntHPWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log(CLanguage.Lan("开始加载高压机参数") + "[" + _runModel.Base.Model + "],"+ CLanguage.Lan("请稍等."), udcRunLog.ELog.Action);

            Stopwatch wathcer = new Stopwatch();

            wathcer.Start();

            try
            {
                string er = string.Empty;

                List<Task<bool>> tasks = new List<Task<bool>>();

                for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                {
                    if (!OpenHP(idNo))
                        continue;

                    int index = idNo;

                    tasks.Add(Task.Factory.StartNew(
                                                   () => SetHipotPara(index, _runModel.Base.HPModel, _runModel.Para.Step)
                                                   ))
                                                   ;
                }

                //等待扫描结束?
                Task.WaitAll(tasks.ToArray());

                for (int idNo = 0; idNo < tasks.Count; idNo++)
                {
                    if (!tasks[idNo].Result)
                        return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                for (int idNo = 0; idNo < CGlobalPara.SysPara.Dev.HPDevMax; idNo++)
                {
                    CloseHP(idNo);
                }

                string waitTime = ((double)wathcer.ElapsedMilliseconds / 1000).ToString("0.0") + "s";

                Log(CLanguage.Lan("加载高压机参数") + "[" + _runModel.Base.Model + "]"+ CLanguage.Lan("结束") + ":" + waitTime, udcRunLog.ELog.Action);
                
                _hpIniModelPara = 0;
            }
        }
        #endregion

        #region 高压机设置参数
        /// <summary>
        /// 自动调用HIPOT机种参数
        /// </summary>
        /// <param name="prgName"></param>
        /// <param name="step"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool AutoSelectHipot(int idNo, string prgName, out string er)
        {
            try
            {
                ioLock.AcquireWriterLock(-1);

                er = string.Empty;

                string modelPath = CGlobalPara.SysFile + "\\" + prgName + ".hp";

                if (!File.Exists(modelPath))
                {
                    er = CLanguage.Lan("找不到HIPOT机种文件") + "[" + modelPath + "],"+ CLanguage.Lan("请确定其路径.");
                    return false;
                }

                CSerializable<CModelPara>.ReadXml(modelPath, ref _runModel);

                _defaultModelPath = modelPath;

                CIniFile.WriteToIni("Parameter", "ModelPath", _defaultModelPath, CGlobalPara.IniFile);

                if (_runModel != null)
                {
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                    OnUIModelArgs.OnEvented(new CUIModelArgs(idNo, _name, _runModel));

                    UIMainArgs.RunStatus = (int)udcHPInfo.ERun.Initialize;

                    UIMainArgs.DoRun = EUIStatus.设置状态;

                    UIMainArgs.Step = _runModel.Para.Step;

                    UIMainArgs.DoRun = EUIStatus.设置参数;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));

                    if (!SetHipotPara(idNo, _runModel.Base.HPModel, _runModel.Para.Step))
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
                ioLock.ReleaseLock();
            }
        }
        /// <summary>
        /// 初始化高压HIPOT程序
        /// </summary>
        /// <param name="step"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool SetHipotPara(int idNo, string prgName, List<CHPPara.CStep> step)
        {
            try
            {
                string er = string.Empty;

                if (!CGlobalPara.SysPara.Para.ChkImpPrg)
                {
                    //重新加载程序
                    if (!_devHP[idNo].SetTestPara(step, out er, prgName, true))
                    {
                        Log(_devHP[idNo].ToString() + CLanguage.Lan("加载高压程序错误:") + er, udcRunLog.ELog.NG);
                        return false;
                    }
                }
                else
                {
                    //导入高压机内部程序
                    if (!_devHP[idNo].ImportProgram(prgName, out er))
                    {
                        Log(_devHP[idNo].ToString() + CLanguage.Lan("导入高压机内部程序错误:") + er, udcRunLog.ELog.NG);
                        return false;
                    }
                    //比对程序是否一致
                    for (int i = 0; i < _runModel.Para.Step.Count; i++)
                    {
                        EStepName stepName = EStepName.AC;

                        List<double> stepVal = new List<double>();

                        if (!_devHP[idNo].ReadStepSetting(i + 1, out stepName, out stepVal, out er))
                        {
                            Log(_devHP[idNo].ToString() + CLanguage.Lan("读取高压机步骤错误:") + er, udcRunLog.ELog.NG);
                            return false;
                        }
                        if (stepName != _runModel.Para.Step[i].name)
                        {
                            er = CLanguage.Lan("测试步骤") + (i + 1).ToString() + CLanguage.Lan("不一致:机种") + "[" +
                                  _runModel.Para.Step[i].name + "]"+ CLanguage.Lan("与高压机") + "[" + stepName + "]";
                            Log(_devHP[idNo].ToString() + er, udcRunLog.ELog.NG);
                            return false;
                        }
                        for (int j = 0; j < _runModel.Para.Step[i].para.Count; j++)
                        {
                            if (_runModel.Para.Step[i].para[j].setVal.ToString("0.00") != stepVal[j].ToString("0.00"))
                            {
                                er = CLanguage.Lan("测试步骤") + (i + 1).ToString() + CLanguage.Lan("参数") + _runModel.Para.Step[i].para[j].name +
                                     CLanguage.Lan("不一致:机种") + "[" + _runModel.Para.Step[i].para[j].setVal + "]"+ CLanguage.Lan("机种与高压机") + "[" + stepVal[j] + "]";
                                Log(_devHP[idNo].ToString() + er, udcRunLog.ELog.NG);
                                return false;
                            }
                        }
                    }
                }

                //设置高压机有效通道

                int chan = 0;

                List<int> hp_Chan_Sel = new List<int>();

                for (int i = 0; i < CGlobalPara.SysPara.Dev.HPChanMax; i++)
                {
                    chan = i;

                    if (_runModel.Para.HpCH[chan] > 0)
                        hp_Chan_Sel.Add(_runModel.Para.HpCH[chan]);
                }

                if (!_devHP[idNo].SetChanEnable(hp_Chan_Sel, out er))
                {
                    Log(_devHP[idNo].ToString() + CLanguage.Lan("设置有效通道错误:") + er, udcRunLog.ELog.NG);
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
        /// 获取高压测试步骤
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool GetRunTestStep(int idNo)
        {
            try
            {
                bool HaveUUT = false;

                int stepNum = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.HPDevMax / CGlobalPara.SysPara.Dev.HPChanMax;

                for (int i = _devRun[idNo].StepNo; i < stepNum; i++)
                {
                    for (int ch = 0; ch < CGlobalPara.SysPara.Dev.HPChanMax; ch++)
                    {
                        int uutNo = CGlobalPara.SysPara.Dev.HPChanMax * i + ch;

                        if (_devRun[idNo].SerialNo[uutNo] != string.Empty)
                            HaveUUT = true;
                    }

                    if (HaveUUT)
                    {
                        _devRun[idNo].StepNo = i;
                        return true;
                    }
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
        /// 获取高压测试结果
        /// </summary>
        /// <param name="curFixStep"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool GetResultFromHP(int idNo, out string er)
        {
            er = string.Empty;

            try
            {

                List<CCHResult> uutResultList = null;

                if (!_devHP[idNo].ReadResult(CGlobalPara.SysPara.Dev.HPChanMax, _runModel.Para.Step.Count, out uutResultList, out er))
                    return false;

                int slotMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.HPDevMax;

                for (int i = 0; i < uutResultList.Count; i++)
                {
                    int slot = CGlobalPara.SysPara.Dev.HPChanMax * _devRun[idNo].StepNo + i;

                    int uutNo = idNo * slotMax + slot;

                    if (_runModel.Para.HpCH[uutNo] == 0)
                        continue;

                    //检测测试项目与测试数据是否一致?

                    if (_runModel.Para.Step.Count != uutResultList[i].Step.Count)
                    {
                        er = CLanguage.Lan("获取测试步骤数") + "【" + uutResultList[i].Step.Count.ToString() + "】"+ "与设置机种步骤数"+ "【" +
                                              _runModel.Para.Step.Count.ToString() + "】"+ CLanguage.Lan("不一致");
                        return false;
                    }

                    if (CGlobalPara.SysPara.Dev.HPType.ToString().IndexOf("Chroma") >= 0)
                    {
                        for (int z = 0; z < uutResultList[i].Step.Count; z++)
                        {
                            if (_runModel.Para.Step[z].name != uutResultList[i].Step[z].Name)
                            {
                                er = CLanguage.Lan("机种步骤") + (z + 1).ToString() + ":【" + _runModel.Para.Step[z].name.ToString() +
                                     "】" + "与获取测试数据步骤" + "【" + uutResultList[i].Step[z].Name + "】" + CLanguage.Lan("不一致");
                                return false;
                            }
                        }
                    }

                    //高压通道对应产品编号0-15

                    int hpChan = _runModel.Para.HpCH[uutNo] - 1;

                    _devRun[idNo].Result[slot] = uutResultList[hpChan].Result;

                    _devRun[idNo].HpResult[slot].chanNo = _runModel.Para.HpCH[uutNo];

                    _devRun[idNo].HpResult[slot].result = uutResultList[hpChan].Result;

                    _devRun[idNo].HpResult[slot].mVal.Clear();

                    for (int z = 0; z < uutResultList[hpChan].Step.Count; z++)
                    {
                        CHPPara.CVal ItemVal = new CHPPara.CVal();

                        ItemVal.name = uutResultList[hpChan].Step[z].Name;

                        ItemVal.result = uutResultList[hpChan].Step[z].Result;

                        ItemVal.code = uutResultList[hpChan].Step[z].Code;

                        ItemVal.value = uutResultList[hpChan].Step[z].Value;

                        ItemVal.unit = uutResultList[hpChan].Step[z].Unit;

                        _devRun[idNo].HpResult[slot].mVal.Add(ItemVal);
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
        #endregion

        #region 设置IO
                /// <summary>
        /// 设置高压启动条件:高压通道及IO通道
        /// </summary>
        /// <param name="curFixStep"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool SetIOAndHPEvent(int idNo, out string er)
        {
            er = string.Empty;

            try
            {
                ioLock.AcquireWriterLock(-1);

                if (!CGlobalPara.SysPara.Dev.ChkIoEnable)
                    return true;

                int CountX = 5;

                //控制IO所有OFF
                if (idNo == -1)
                {
                    int[] wBit = new int[16];

                    for (int i = 0; i < wBit.Length; i++)
                        wBit[i] = 0;

                    for (int i = 0; i < CountX; i++)
                    {
                        System.Threading.Thread.Sleep(50);

                        if (_devIO.Write(1, ERegType.Y, 0, wBit, out er))
                            return true;
                    }

                    return false;
                }

                int slotMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.HPDevMax;

                int[] Y = new int[slotMax];

                for (int i = 0; i < Y.Length; i++)
                    Y[i] = 0;

                for (int i = 0; i < CGlobalPara.SysPara.Dev.HPChanMax; i++)
                {
                    int ch = CGlobalPara.SysPara.Dev.HPChanMax * _devRun[idNo].StepNo + i;

                    int uutNo = idNo * slotMax + CGlobalPara.SysPara.Dev.HPChanMax * _devRun[idNo].StepNo + i;

                    if (_runModel.Para.IoCH[uutNo] > 0 && _devRun[idNo].SerialNo[ch] != string.Empty)
                    {
                        Y[_runModel.Para.IoCH[uutNo] - 1] = 1;
                    }
                }

                int startAddr = idNo * slotMax;

                for (int i = 0; i < CountX; i++)
                {
                    System.Threading.Thread.Sleep(50);

                    if (_devIO.Write(1, ERegType.Y, startAddr, Y, out er))
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {
                System.Threading.Thread.Sleep(CGlobalPara.SysPara.Dev.IoDelayMs);

                ioLock.ReleaseWriterLock();
            }
        }
        #endregion

        #region 调试模式
        /// <summary>
        /// 设置高压机处于调式模式
        /// </summary>
        /// <param name="idNo"></param>
        private void SetHPToDebug(int idNo)
        {
            try
            {
                string er = string.Empty;

                string sNowTime = string.Empty;

                if (_devRun[idNo].DebugMode)
                {
                    MessageBox.Show(CLanguage.Lan("高压机") + (idNo + 1).ToString() + CLanguage.Lan("处于测试,请停止."),
                                     "Tip", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!OpenHP(idNo))
                {
                    UIMainArgs.BtnValue = 0;
                    UIMainArgs.BtnNo = idNo;
                    UIMainArgs.DoRun = EUIStatus.设置按钮;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
                    return;
                }

                if (CGlobalPara.SysPara.Dev.ChkIoEnable)
                {
                    if (!OpenIO())
                    {
                        UIMainArgs.BtnValue = 0;
                        UIMainArgs.BtnNo = idNo;
                        UIMainArgs.DoRun = EUIStatus.设置按钮;
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, _name, UIMainArgs));
                        return;
                    }
                }

                string[] guid = Guid.NewGuid().ToString().ToUpper().Split('-');

                UIMainArgs.IdCard = guid[0];

                UIMainArgs.DoRun = EUIStatus.治具就绪;

                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));

                for (int i = 0; i < _statHP.SerialNo.Count; i++)
                {
                    _statHP.SerialNo[i] = guid[0] + (i + 1).ToString("D2");

                    UIMainArgs.SlotNo = i;

                    UIMainArgs.CurSn = _statHP.SerialNo[i];

                    UIMainArgs.DoRun = EUIStatus.设置条码;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(0, _name, UIMainArgs));
                }

                int slotMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.HPDevMax;

                for (int slot = 0; slot < slotMax; slot++)
                {
                    int uutNo = idNo * slotMax + slot;

                    int index = _runModel.Para.IoCH[uutNo] - 1;

                    _devRun[idNo].SerialNo[slot] = _statHP.SerialNo[index];

                    _devRun[idNo].Result[slot] = 0;
                }

                _devRun[idNo].StepNo = 0;

                _devRun[idNo].DoRun = ERUN.到位;

                _devRun[idNo].DebugMode = true;


                Task.Factory.StartNew(() => { HP_Device_Task(idNo); });

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err); 
            }
        }
        #endregion

        #region TCP客户端通信
        /// <summary>
        /// 查询命令
        /// </summary>
        /// <param name="serName">站别名</param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool QUERY_STATE(string serName, out CSerReponse serReponse, out string er)
        {
            serReponse = new CSerReponse(CYOHOOApp.SlotMax);

            er = string.Empty;

            try
            {
                string cmd = ESOCKET_CMD.QUERY_STATE.ToString() + "_" + serName;

                TcpLog(cmd, udcRunLog.ELog.Action);

                //自定义结构体
                if (CGlobalPara.SysPara.Dev.TcpMode == 0)
                {
                    SOCKET_REQUEST socketReponse = new SOCKET_REQUEST();
                    socketReponse.Name = serName;
                    socketReponse.CmdNo = ESOCKET_CMD.QUERY_STATE;
                    socketReponse.UUT_NUM = CYOHOOApp.SlotMax;
                    socketReponse.UUT = new SOCKET_UUT[CYOHOOApp.SlotMax];
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        socketReponse.UUT[i] = new SOCKET_UUT();
                    byte[] rBytes = null;
                    byte[] sendBytes = CStuct<SOCKET_REQUEST>.StructToBytes(socketReponse); ;
                    int rLen = CStuct<SOCKET_REQUEST>.GetStuctLen(socketReponse);
                    if (!_devTCP.send(sendBytes, rLen, out rBytes, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    socketReponse = GJ.COM.CStuct<SOCKET_REQUEST>.BytesToStruct(rBytes, typeof(SOCKET_REQUEST));
                    if (socketReponse.CmdNo != ESOCKET_CMD.STATE_OK)
                    {
                        er = CLanguage.Lan("返回命令错误:") + socketReponse.CmdNo.ToString();
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (socketReponse.Name != serName)
                    {
                        er = CLanguage.Lan("返回工站错误:") + socketReponse.Name.ToString();
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (socketReponse.ErrCode != ESOCKET_ERROR.OK)
                    {
                        er = CLanguage.Lan("返回命令错误:") + socketReponse.CmdNo.ToString();
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    serReponse.Ready = socketReponse.Ready;
                    serReponse.IdCard = socketReponse.IdCard;
                    serReponse.ModelName = socketReponse.Model;
                    serReponse.MesFlag = socketReponse.MesFlag;
                    for (int i = 0; i < socketReponse.UUT_NUM; i++)
                        serReponse.SerialNos[i] = socketReponse.UUT[i].SerialNo;

                    UIMainArgs.TcpReponse = serReponse;

                    UIMainArgs.DoRun = EUIStatus.TCP界面;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

                    TcpLog(CSOCKET_INFO.show(socketReponse), udcRunLog.ELog.OK);

                    return true;
                }

                //自定义字符串
                if (CGlobalPara.SysPara.Dev.TcpMode == 1)
                {
                    string recv = string.Empty;
                    string SOI = "STATE_" + serName + "_OK_";
                    if (!_devTCP.send(cmd, SOI.Length, out recv, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (recv.Substring(0, SOI.Length) != SOI)
                    {
                        serReponse.Ready = 0;
                        serReponse.IdCard = "";
                        serReponse.ModelName = "";
                        serReponse.MesFlag = 0;
                        er = CLanguage.Lan("返回命令错误:") + recv;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    string rData = recv.Replace(SOI, "");
                    string[] ItemList = rData.Split(';');
                    serReponse.Ready = System.Convert.ToInt16(ItemList[0]);
                    if (serReponse.Ready == 0)
                    {
                        serReponse.IdCard = "";
                        serReponse.ModelName = "";
                        serReponse.MesFlag = 0;
                    }
                    else
                    {
                        serReponse.IdCard = ItemList[1];
                        serReponse.ModelName = ItemList[2];
                        serReponse.MesFlag = System.Convert.ToInt16(ItemList[3]);
                        int uutMax = System.Convert.ToInt16(ItemList[4]);
                        string[] serialNos = ItemList[5].Split(',');
                        for (int i = 0; i < uutMax; i++)
                        {
                            serReponse.SerialNos[i] = serialNos[i];
                        }
                    }

                    UIMainArgs.TcpReponse = serReponse;

                    UIMainArgs.DoRun = EUIStatus.TCP界面;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

                    TcpLog(recv, udcRunLog.ELog.OK);

                    return true;
                }

                //自定义JSON
                if (CGlobalPara.SysPara.Dev.TcpMode == 2)
                {
                    CJSON_REQUEST obj = new CJSON_REQUEST();
                    obj.Name = serName;
                    obj.CmdNo = ESOCKET_CMD.QUERY_STATE;
                    obj.UUT = new List<CJSON_UUT>();
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        obj.UUT.Add(new CJSON_UUT());
                    string strJSON = CJSon.Serializer<CJSON_REQUEST>(obj);

                    string recv = string.Empty;
                    string SOI = "STATE_" + serName + "_OK_";
                    if (!_devTCP.send(strJSON, SOI.Length, out recv, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    CJSON_REQUEST objRecv = CJSon.Deserialize<CJSON_REQUEST>(recv);

                    if (objRecv == null)
                    {
                        serReponse.Ready = 0;
                        serReponse.IdCard = "";
                        serReponse.ModelName = "";
                        serReponse.MesFlag = 0;
                        er = CLanguage.Lan("返回数据错误:") + recv;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (obj.Name != objRecv.Name)
                    {
                        er = CLanguage.Lan("返回工站错误:") + objRecv.Name;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (obj.ErrCode != ESOCKET_ERROR.OK)
                    {
                        er = CLanguage.Lan("返回工站错误:") + objRecv.Name;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    serReponse.Ready = objRecv.Ready;
                    serReponse.IdCard = objRecv.IdCard;
                    serReponse.ModelName = objRecv.Model;
                    serReponse.MesFlag = objRecv.MesFlag;
                    for (int i = 0; i < objRecv.UUT.Count; i++)
                        serReponse.SerialNos[i] = objRecv.UUT[i].SerialNo;

                    UIMainArgs.TcpReponse = serReponse;

                    UIMainArgs.DoRun = EUIStatus.TCP界面;

                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(_idNo, _name, UIMainArgs));

                    TcpLog(recv, udcRunLog.ELog.OK);
                    
                    return true;
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
        /// 启动测试
        /// </summary>
        /// <param name="statId"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool START_TEST(string serName, out string er)
        {
            er = string.Empty;

            try
            {
                string cmd = ESOCKET_CMD.START_TEST.ToString() + "_" + serName;

                TcpLog(cmd, udcRunLog.ELog.Action);

                //自定结构体
                if (CGlobalPara.SysPara.Dev.TcpMode == 0)
                {
                    SOCKET_REQUEST socketReponse = new SOCKET_REQUEST();
                    socketReponse.Name = serName;
                    socketReponse.CmdNo = ESOCKET_CMD.START_TEST;
                    socketReponse.UUT_NUM = CYOHOOApp.SlotMax;
                    socketReponse.UUT = new SOCKET_UUT[CYOHOOApp.SlotMax];
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        socketReponse.UUT[i] = new SOCKET_UUT();
                    byte[] rBytes = null;
                    byte[] sendBytes = GJ.COM.CStuct<SOCKET_REQUEST>.StructToBytes(socketReponse);
                    int rLen = GJ.COM.CStuct<SOCKET_REQUEST>.GetStuctLen(socketReponse);
                    if (!_devTCP.send(sendBytes, rLen, out rBytes, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    socketReponse = GJ.COM.CStuct<SOCKET_REQUEST>.BytesToStruct(rBytes, typeof(SOCKET_REQUEST));

                    if (socketReponse.Name != serName)
                    {
                        er = CLanguage.Lan("返回工位错误:") + socketReponse.Name;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (socketReponse.CmdNo != ESOCKET_CMD.START_OK)
                    {
                        er = CLanguage.Lan("返回命令错误:") + socketReponse.ErrCode.ToString();
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    TcpLog(GJ.COM.CStuct<SOCKET_REQUEST>.ShowInfo(socketReponse), udcRunLog.ELog.OK);
                    return true;
                }

                //自定义字符串
                if (CGlobalPara.SysPara.Dev.TcpMode == 1)
                {
                    string recv = string.Empty;
                    string SOI = "START_" + serName + "_OK";
                    if (!_devTCP.send(cmd, SOI.Length, out recv, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (recv.Substring(0, SOI.Length) != SOI)
                    {
                        er = CLanguage.Lan("返回命令错误:") + recv;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    TcpLog(recv, udcRunLog.ELog.OK);
                    return true;
                }

                //自定义JSON
                if (CGlobalPara.SysPara.Dev.TcpMode == 2)
                {
                    CJSON_REQUEST obj = new CJSON_REQUEST();
                    obj.Name = serName;
                    obj.CmdNo = ESOCKET_CMD.START_TEST;
                    obj.UUT = new List<CJSON_UUT>();
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        obj.UUT.Add(new CJSON_UUT());
                    string strJSON = CJSon.Serializer<CJSON_REQUEST>(obj);

                    string recv = string.Empty;
                    string SOI = "START_" + serName + "_OK";
                    if (!_devTCP.send(strJSON, SOI.Length, out recv, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    CJSON_REQUEST objRecv = CJSon.Deserialize<CJSON_REQUEST>(recv);
                    if (objRecv == null)
                    {
                        er = CLanguage.Lan("返回数据错误:") + recv;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (obj.Name != objRecv.Name)
                    {
                        er = CLanguage.Lan("返回工站错误:") + objRecv.Name;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (obj.CmdNo != ESOCKET_CMD.START_OK)
                    {
                        er = CLanguage.Lan("返回命令错误:") + objRecv.CmdNo.ToString();
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    TcpLog(recv, udcRunLog.ELog.OK);

                    return true;
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
        /// 测试结束
        /// </summary>
        /// <param name="statId"></param>
        /// <param name="idCard"></param>
        /// <param name="result"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool END_TEST(string serName, string idCard, List<int> result, out string er)
        {
            er = string.Empty;

            try
            {
                string cmd = ESOCKET_CMD.END_TEST.ToString() + "_" + serName + "_" + result.Count.ToString();
                cmd += ";" + idCard + ";";
                for (int i = 0; i < result.Count; i++)
                {
                    if (i < result.Count - 1)
                        cmd += result[i] + ",";
                    else
                        cmd += result[i];
                }
                TcpLog(cmd, udcRunLog.ELog.Action);

                //自定义结构体
                if (CGlobalPara.SysPara.Dev.TcpMode == 0)
                {
                    SOCKET_REQUEST socketReponse = new SOCKET_REQUEST();
                    socketReponse.Name = serName;
                    socketReponse.CmdNo = ESOCKET_CMD.END_TEST;
                    socketReponse.UUT_NUM = CYOHOOApp.SlotMax;
                    socketReponse.UUT = new SOCKET_UUT[CYOHOOApp.SlotMax];
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                        socketReponse.UUT[i] = new SOCKET_UUT();
                    socketReponse.IdCard = idCard;
                    for (int i = 0; i < socketReponse.UUT_NUM; i++)
                        socketReponse.UUT[i].Result = result[i];

                    byte[] rBytes = null;
                    byte[] sendBytes = GJ.COM.CStuct<SOCKET_REQUEST>.StructToBytes(socketReponse);
                    int rLen = GJ.COM.CStuct<SOCKET_REQUEST>.GetStuctLen(socketReponse);
                    if (!_devTCP.send(sendBytes, rLen, out rBytes, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.Err);
                        return false;
                    }
                    socketReponse = GJ.COM.CStuct<SOCKET_REQUEST>.BytesToStruct(rBytes, typeof(SOCKET_REQUEST));
                    if (socketReponse.Name != serName)
                    {
                        er = CLanguage.Lan("返回工位错误:") + socketReponse.Name;
                        TcpLog(er, udcRunLog.ELog.Err);
                        return false;
                    }
                    if (socketReponse.CmdNo != ESOCKET_CMD.END_OK)
                    {
                        er = CLanguage.Lan("返回命令错误:") + socketReponse.ErrCode.ToString();
                        TcpLog(er, udcRunLog.ELog.Err);
                        return false;
                    }
                    TcpLog(GJ.COM.CStuct<SOCKET_REQUEST>.ShowInfo(socketReponse), udcRunLog.ELog.OK);
                    return true;
                }

                //自定义字符串
                if (CGlobalPara.SysPara.Dev.TcpMode == 1)
                {
                    string recv = string.Empty;
                    string SOI = "END_" + serName + "_OK";
                    if (!_devTCP.send(cmd, SOI.Length, out recv, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (recv.Substring(0, SOI.Length) != SOI)
                    {
                        er = CLanguage.Lan("返回命令错误:") + recv;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    TcpLog(recv, udcRunLog.ELog.OK);
                    return true;
                }

                //自定义字符串
                if (CGlobalPara.SysPara.Dev.TcpMode == 2)
                {
                    CJSON_REQUEST obj = new CJSON_REQUEST();
                    obj.Name = serName;
                    obj.CmdNo = ESOCKET_CMD.END_TEST;
                    obj.UUT = new List<CJSON_UUT>();
                    for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                    {
                        obj.UUT.Add(new CJSON_UUT());
                        obj.UUT[i].Result = result[i];
                    }
                    string strJSON = CJSon.Serializer<CJSON_REQUEST>(obj);
                    string recv = string.Empty;
                    string SOI = "END_" + serName + "_OK";
                    if (!_devTCP.send(strJSON, SOI.Length, out recv, out er))
                    {
                        er = CLanguage.Lan("发送数据超时:") + cmd;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    CJSON_REQUEST objRecv = CJSon.Deserialize<CJSON_REQUEST>(recv);
                    if (objRecv == null)
                    {
                        er = CLanguage.Lan("返回数据错误:") + recv;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (obj.Name != objRecv.Name)
                    {
                        er = CLanguage.Lan("返回工站错误:") + objRecv.Name;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }
                    if (obj.CmdNo != ESOCKET_CMD.END_OK)
                    {
                        er = CLanguage.Lan("返回命令错误:") + objRecv.CmdNo.ToString();
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    TcpLog(recv, udcRunLog.ELog.OK);

                    return true;
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

        #region 本地数据
        /// <summary>
        /// 插入不良条码记录
        /// </summary>
        /// <param name="uutNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private void local_db_recordFailSn(CStat hub)
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

                        string failInfo = hub.TestData[i];

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
        private bool SaveDailyReport(out string er)
        {
            er = string.Empty;

            try
            {
                if (!CGlobalPara.SysPara.Report.SaveReport)
                    return true;

                string fileName = createDailyReportFile();

                if (!File.Exists(fileName))
                    saveReportTitle(fileName);

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

                strWrite = "TestSoft  Version :" + _runModel.Base.Version;
                sw.WriteLine(strWrite);

                strWrite = "TestSoft  Version Date :" +
                            System.Convert.ToDateTime(_runModel.Base.ReleaseDate).ToString("yyyy/MM/dd");
                sw.WriteLine(strWrite);

                strWrite = "Hipot Device :" + CGlobalPara.SysPara.Dev.HPType.ToString();
                sw.WriteLine(strWrite);

                strWrite = "Product Number :," + _runModel.Base.Model;
                sw.WriteLine(strWrite);

                sw.WriteLine("");

                strWrite = "Item Name :,";
                for (int i = 0; i < _runModel.Para.Step.Count; i++)
                    strWrite += _runModel.Para.Step[i].name.ToString() + ",";
                sw.WriteLine(strWrite);

                strWrite = "SPES MAX :,";
                for (int i = 0; i < _runModel.Para.Step.Count; i++)
                {
                    if (_runModel.Para.Step[i].name == EStepName.IR)
                        strWrite += _runModel.Para.Step[i].para[2].setVal.ToString() + ",";
                    else if (_runModel.Para.Step[i].name == EStepName.OSC)
                        strWrite += _runModel.Para.Step[i].para[0].setVal.ToString() + ",";
                    else
                        strWrite += _runModel.Para.Step[i].para[1].setVal.ToString() + ",";

                }
                sw.WriteLine(strWrite);

                strWrite = "SPES MIN :,";
                for (int i = 0; i < _runModel.Para.Step.Count; i++)
                {
                    if (_runModel.Para.Step[i].name == EStepName.IR)
                        strWrite += _runModel.Para.Step[i].para[1].setVal.ToString() + ",";
                    else if (_runModel.Para.Step[i].name == EStepName.OSC)
                        strWrite += _runModel.Para.Step[i].para[1].setVal.ToString() + ",";
                    else
                        strWrite += _runModel.Para.Step[i].para[2].setVal.ToString() + ",";

                }
                sw.WriteLine(strWrite);

                sw.WriteLine("");

                strWrite = "Fixture Name,Serial Number,Result,Date,Time,Device,Slot,";

                for (int i = 0; i < _runModel.Para.Step.Count; i++)
                {
                    strWrite += "ItemResult,";

                    if (_runModel.Para.Step[i].name == EStepName.AC ||
                        _runModel.Para.Step[i].name == EStepName.DC)
                    {
                        strWrite += _runModel.Para.Step[i].name.ToString() + "(mA),";
                    }
                    else if (_runModel.Para.Step[i].name == EStepName.IR)
                    {
                        strWrite += _runModel.Para.Step[i].name.ToString() + "(M0hm),";
                    }
                    else if (_runModel.Para.Step[i].name == EStepName.OSC)
                    {
                        strWrite += _runModel.Para.Step[i].name.ToString() + "(nF),";
                    }
                    else
                    {
                        strWrite += _runModel.Para.Step[i].name.ToString() + ",";
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
        /// 保存测试值
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="idNo"></param>
        private void saveReportVal(string fileName)
        {
            try
            {
                string strWrite = string.Empty;

                string strDate = DateTime.Now.ToString("yyyy/MM/dd");

                string strTime = DateTime.Now.ToString("HH:mm:ss");

                StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);

                for (int i = 0; i < _statHP.SerialNo.Count; i++)
                {
                    if (_statHP.SerialNo[i] == string.Empty)
                        continue;

                    strWrite = "FIX-" + _statHP.IdCard + ",";

                    strWrite += _statHP.SerialNo[i] + ",";

                    strWrite += (_statHP.Result[i] == 0 ? "PASS" : "FAIL") + ",";

                    strWrite += strDate + ",";

                    strWrite += strTime + ",";

                    strWrite += _statHP.Ready.ToString() + ",";

                    strWrite += (i + 1).ToString() + ",";

                    for (int ItemNo = 0; ItemNo < _statHP.HpResult[i].mVal.Count; ItemNo++)
                    {
                        strWrite += _statHP.HpResult[i].mVal[ItemNo].code + ",";

                        if (_statHP.HpResult[i].mVal[ItemNo].name == EStepName.IR)
                        {
                            if (_statHP.HpResult[i].mVal[ItemNo].unit == "M0hm")
                            {
                                if (_statHP.HpResult[i].mVal[ItemNo].value > 1000000)
                                    strWrite += "UUUU,";
                                else
                                    strWrite += _statHP.HpResult[i].mVal[ItemNo].value.ToString("0.000") + ",";
                            }
                            else
                            {
                                if (_statHP.HpResult[i].mVal[ItemNo].value > 1000)
                                    strWrite += "UUUU,";
                                else
                                    strWrite += (_statHP.HpResult[i].mVal[ItemNo].value * 1000).ToString("0.000") + ",";
                            }                           
                        }
                        else
                        {
                            strWrite += _statHP.HpResult[i].mVal[ItemNo].value.ToString("0.000") + ",";
                        }
                    }

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

        #region TCP/IP消息
        private void TcpLog(string info, udcRunLog.ELog e)
        {
            OnTCPLogArgs.OnEvented(new CUILogArgs(0, "TcpLog", info, e, false));   
        }
        private void OnTcpCon(object sender, CTcpConArgs e)
        {
            if (!e.bErr)
            {
               OnTCPLogArgs.OnEvented(new CUILogArgs(0,"TcpLog",e.conStatus,udcRunLog.ELog.Action,false));   
            }
            else
            {
                OnTCPLogArgs.OnEvented(new CUILogArgs(0, "TcpLog", e.conStatus, udcRunLog.ELog.NG, false));  
            }
        }
        private void OnTcpRecv(object sender, CTcpRecvArgs e)
        {
            //OnTCPLogArgs.OnEvented(new CUILogArgs(0, "TcpLog", e.recvData, udcRunLog.ELog.Action, false));   
        }
        #endregion

        #region 冠佳Web
        /// <summary>
        /// 写入Web治具结果
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool UpdateFixtureResult()
        {
            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = new CWeb2.CFixture();
                fixture.Base.FlowIndex = 0;
                fixture.Base.FlowName = CYOHOOApp.HIPOT_FlowName;
                fixture.Base.FlowGuid = CNet.HostName();
                fixture.Base.SnType = CWeb2.ESnType.外部条码;
                fixture.Base.IdCard = _statHP.IdCard;
                fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                for (int i = 0; i < _statHP.SerialNo.Count; i++)
                {
                    CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                    para.SlotNo = i;
                    para.SerialNo = _statHP.SerialNo[i];
                    para.InnerSn = string.Empty;
                    para.Remark1 = _runModel.Base.Model;
                    para.Remark2 = string.Empty;
                    para.StartTime = _statHP.StartTime;
                    para.EndTime = _statHP.EndTime;
                    para.Result = _statHP.Result[i];
                    para.TestData = _statHP.TestData[i];
                    fixture.Para.Add(para);
                }

                if (!CWeb2.UpdateFixtureResult(fixture, out er))
                {
                    MessageBox.Show(_statHP.ToString() + CLanguage.Lan("写入治具ID") + "[" + _statHP.IdCard + "]"+
                                                               CLanguage.Lan("测试结果错误:") + er);

                    if (!CWeb2.UpdateFixtureResult(fixture, out er))
                    {
                        Log(_statHP.ToString() + CLanguage.Lan("写入治具ID")+ "[" + _statHP.IdCard + "]"+
                                                       CLanguage.Lan("测试结果错误:") + er, udcRunLog.ELog.NG);

                        return false;
                    }
                }
                else
                {
                    Log(_statHP.ToString() + CLanguage.Lan("写入治具ID") + "[" + _statHP.IdCard + "]"+
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
        /// 上传客户MES
        /// </summary>
        /// <param name="uutNo"></param>
        /// <returns></returns>
        private bool TranSnToMES()
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

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                for (int slotNo = 0; slotNo < _statHP.SerialNo.Count; slotNo++)
                {
                    if (_statHP.SerialNo[slotNo] == string.Empty)
                        continue;

                    CSFCS.CSnData SnData = new CSFCS.CSnData()
                    {
                        DeviceId = "0",
                        DeviceName = CNet.HostIP(),
                        StatName = CYOHOOApp.HIPOT_FlowName,
                        StatDesc = CLanguage.Lan("安规测试"),
                        Fixture = _statHP.IdCard + (slotNo + 1).ToString("D2"),
                        LocalName = _statHP.ToString(),
                        StartTime = _statHP.StartTime,
                        EndTime = _statHP.EndTime,
                        Model = _runModel.Base.Model,
                        OrderName = CYOHOOApp.OrderName,
                        SerialNo = _statHP.SerialNo[slotNo],
                        Result = _statHP.Result[slotNo],
                        Remark1 = string.Empty,
                        Remark2 = string.Empty,
                        Item = new List<CSFCS.CSnItem>()
                    };

                    for (int i = 0; i < _statHP.HpResult[slotNo].mVal.Count; i++)
                    {
                        ////只上传AC项目

                        //if (_runModel.Para.Step[i].name != EStepName.AC)
                        //    continue;

                        //SnData.Result = _statHP.HpResult[slotNo].mVal[i].result;

                        string min = string.Empty;

                        string max = string.Empty;

                        string value = string.Empty;

                        string unit = _statHP.HpResult[slotNo].mVal[i].unit;

                        if (_runModel.Para.Step[i].name == EStepName.IR)
                        {
                            if (_statHP.HpResult[slotNo].mVal[i].unit == "M0hm")
                            {
                                if (_statHP.HpResult[slotNo].mVal[i].value > 1000000)
                                    value = "99999";
                                else
                                    value = _statHP.HpResult[slotNo].mVal[i].value.ToString();
                            }
                            else
                            {
                                if (_statHP.HpResult[slotNo].mVal[i].value > 1000)
                                    value = "99999";
                                else
                                    value = _statHP.HpResult[slotNo].mVal[i].value.ToString();
                            }
                        }
                        else
                        {
                            value = _statHP.HpResult[slotNo].mVal[i].value.ToString("0.000");
                        }

                        string itemResult = (_statHP.HpResult[slotNo].mVal[i].result == 0 ? "PASS" : "FAIL");

                        if (_runModel.Para.Step[i].name == EStepName.IR)
                        {
                            min = _runModel.Para.Step[i].para[1].setVal.ToString();
                            max = _runModel.Para.Step[i].para[2].setVal.ToString();
                        }
                        else if (_runModel.Para.Step[i].name == EStepName.OSC)
                        {
                            min = _runModel.Para.Step[i].para[0].setVal.ToString();
                            max = _runModel.Para.Step[i].para[1].setVal.ToString();
                        }
                        else
                        {
                            min = _runModel.Para.Step[i].para[2].setVal.ToString();
                            max = _runModel.Para.Step[i].para[1].setVal.ToString();
                        }

                        CSFCS.CSnItem SnItem = new CSFCS.CSnItem()
                        {
                            IdNo = i,
                            Name = _runModel.Para.Step[i].name.ToString(),
                            Desc = _runModel.Para.Step[i].des,
                            LowLimit = min,
                            UpLimit = max,
                            Value = value,
                            Unit = _statHP.HpResult[slotNo].mVal[i].unit,
                            Result = _statHP.HpResult[slotNo].mVal[i].result,
                            ErroCode = _statHP.HpResult[slotNo].mVal[i].code,
                            ErrInfo = string.Empty,
                            Remark1 = string.Empty,
                            Remark2 = string.Empty
                        };
                        SnData.Item.Add(SnItem);
                    }

                    if (mes.TranSnData(SnData, out er))
                    {
                        if (_statHP.Result[slotNo] == 0)
                        {
                            Log(CLanguage.Lan("治具ID") + "【" + _statHP.IdCard + "-" + (slotNo + 1).ToString("D2") + "】" + CLanguage.Lan("条码") +
                                                           "【" + _statHP.SerialNo[slotNo] + "】" + CLanguage.Lan("上传高压结果【PASS】正常"), udcRunLog.ELog.OK);
                        }
                        else
                        {
                            Log(CLanguage.Lan("治具ID") + "【" + _statHP.IdCard + "-" + (slotNo + 1).ToString("D2") + "】" + CLanguage.Lan("条码") +
                                            "【" + _statHP.SerialNo[slotNo] + "】" + CLanguage.Lan("上传高压结果【FAIL】正常"), udcRunLog.ELog.OK);
                        }

                        uutPass = true;

                    }
                    else
                    {

                        if (mes.State == EMesState.网络异常)
                        {
                            SetMesStatus(1, "MES系统网络异常,请检查网络是否正常?");
                            break;
                        }

                        Log(CLanguage.Lan("治具ID") + "【" + _statHP.IdCard + "-" + (slotNo + 1).ToString("D2") + "】" + CLanguage.Lan("条码") +
                                                      "【" + _statHP.SerialNo[slotNo] + "】" + CLanguage.Lan("上传高压结果错误:") + er, udcRunLog.ELog.NG);
                    }
                }

                watcher.Stop();

                Log(CLanguage.Lan("治具ID") + "【" + _statHP.IdCard + "】" + CLanguage.Lan("上传高压结果结束:") +
                                                     watcher.ElapsedMilliseconds.ToString() + "ms", udcRunLog.ELog.Action);

                if (!uutPass)
                {
                    SetMesStatus(2, "治具ID[" + _statHP.IdCard + "]上传MES结果所有FAIL,请检查.");
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
                    runStatus = EDevRunStatus.运行;
                }

                //更新设备状态
                if (Iot_DevRunStatus.ContainsKey(CGlobalPara.DeviceIDNo))
                {
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].RunStatus = (int)runStatus;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].TTNum = _yield.TTNum;
                    Iot_DevRunStatus[CGlobalPara.DeviceIDNo].FailNum = _yield.FailNum;
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
                                    ExcuteOK = false;                                    

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

    }
}
