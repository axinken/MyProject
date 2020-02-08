using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Drawing;
using GJ.USER.APP.MainWork;
using GJ.UI;
using GJ.COM;
using GJ.DEV.CATEXY;
using GJ.DEV.COM;
using GJ.USER.APP;
using GJ.MES;
using GJ.PDB;
using GJ.Iot;
using GJ.USER.APP.Iot;

namespace GJ.YOHOO.ATE
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

                local_db_CreateTable();

                _statATE = new CStat[CGlobalPara.C_STAT_MAX];

                int uutMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.DevMax;

                for (int idNo = 0; idNo < CGlobalPara.C_STAT_MAX; idNo++)
                {
                    _statATE[idNo] = new CStat(idNo, "<"+  CLanguage.Lan("ATE工位") + (idNo + 1).ToString() + ">",
                                               CYOHOOApp.ATE_FlowId, CYOHOOApp.ATE_FlowName, uutMax);
                }

                _devATE = new CDev(0, CLanguage.Lan("ATE测试仪器"), CGlobalPara.SysPara.Dev.ChanMax);

                _yield = new CYield(uutMax);

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

                CIniFile.GetIniKeySection("ATE", out  C_ATE_CONFIRM_INFO, out C_ATE_CONFIRM_VALUE, CGlobalPara.IniFile);

                LoadYieldFromIniFile();
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
            for (int idNo = 0; idNo < CGlobalPara.C_STAT_MAX; idNo++)
            {
                _statATE[idNo].DoRun = ERUN.空闲; 
            }

            _devATE.DoRun = ERUN.空闲;

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

                IniTask.Add(Task.Factory.StartNew(() => OpenATE()));

                IniTask.Add(Task.Factory.StartNew(() => OpenTCP()));

                IniTask.Add(Task.Factory.StartNew(() => OpenGJWebServer()));

                if (CGlobalPara.SysPara.Dev.ChkIoEnable)
                {
                    IniTask.Add(Task.Factory.StartNew(() => OpenIO()));
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
                    Log(CLanguage.Lan("系统硬件设备自检结束【PASS】:")+ waitTime, udcRunLog.ELog.OK);
                }
                else
                {
                    Log(CLanguage.Lan("系统硬件设备自检结束【FAIL】:") + waitTime, udcRunLog.ELog.NG);
                }
            }
        }
        public override void CloseDevice()
        {
            CloseTCP();

            CloseIO();

            CloseATE();
        }
        public override bool StartThread()
        {
            try
            {
               

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
        /// TCP运行日志
        /// </summary>
        public COnEvent<CUILogArgs> OnTCPLogArgs = new COnEvent<CUILogArgs>();
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
        /// <summary>
        /// TCP消息日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="e"></param>
        private void TcpLog(string info, udcRunLog.ELog e)
        {
            OnTCPLogArgs.OnEvented(new CUILogArgs(0, "TcpLog", info, e, false));
        }
        #endregion

        #region 字段
        /// <summary>
        /// ATE设备
        /// </summary>
        private CChromaHwnd ChromaATE = null;
        /// <summary>
        /// IO切换板
        /// </summary>
        private CGJX6Y6 _devIO = null;
        /// <summary>
        /// TCP客户端
        /// </summary>
        private CClientTCP _devTCP = null;
        /// <summary>
        /// 仪器设备
        /// </summary>
        private CDev _devATE = null;
        /// <summary>
        /// 工位
        /// </summary>
        private CStat[] _statATE = null;
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
        /// ATE报警提示
        /// </summary>
        private int _conToATECount = 0;
        /// <summary>
        /// 产能统计
        /// </summary>
        private CYield _yield = null;
        /// <summary>
        /// ATE程序调出确认对话框信息标题名称
        /// </summary>
        private List<string> C_ATE_CONFIRM_INFO = new List<string>();
        /// <summary>
        /// ATE程序调出确认对话框信息值
        /// </summary>
        private List<string> C_ATE_CONFIRM_VALUE = new List<string>();
        /// <summary>
        /// 调试状态
        /// </summary>
        private bool C_DEBUG_MODE = false;
        #endregion

        #region 面板方法
        public void OnFrmMainDebug(int idNo, int status)
        {
            try
            {
                //停止调式
                if (status == 0)
                {
                    _devATE.DoRun = ERUN.空闲;
                    return;
                }
                
                bool ChkFlag=true;

                //启动调式

                if (!OpenATE())
                    ChkFlag=false;

                if (CGlobalPara.SysPara.Dev.ChkIoEnable)
                {
                    if (!OpenIO())
                        ChkFlag = false;
                }

                if (!ChkFlag)
                {
                    CloseATE();
                    CloseIO();
                    return;
                }

                string[] guid = Guid.NewGuid().ToString().ToUpper().Split('-');

                _statATE[0].IdCard = guid[0];
                _statATE[1].IdCard = guid[0];

                for (int i = 0; i < _statATE[0].SerialNo.Count; i++)
                {
                    _devATE.SerialNo[i] = guid[0] + (i + 1).ToString("D2");
                    _statATE[0].SerialNo[i] = guid[0] + (i + 1).ToString("D2");
                    _statATE[1].SerialNo[i] = guid[0] + (i + 1).ToString("D2");
                }


                UIMainArgs.bAlarm = false;
                UIMainArgs.AlarmInfo = _statATE[0].ToString() + CLanguage.Lan("到位就绪");
                UIMainArgs.IdCard = _statATE[0].IdCard;
                UIMainArgs.SerialNo = _statATE[0].SerialNo;
                UIMainArgs.Result = _statATE[0].Result; 
                UIMainArgs.DoRun = EUIStatus.治具到位;
                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                UIMainArgs.DebugMode = true;
                UIMainArgs.DoRun = EUIStatus.调试模式;
                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                C_DEBUG_MODE = true;
                _devATE.StepNo = 0;
                _devATE.DoRun = ERUN.到位;
                _statATE[0].DoRun = ERUN.等待;
                _statATE[0].StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _statATE[0].Watcher.Restart();
                Task.Factory.StartNew(() => On_ATE_Task());


            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err); 
            }
        }
        public void OnFrmMainClearYield()
        {
            _yield.TTNum = 0;
            _yield.FailNum = 0;
            for (int i = 0; i < _yield.SlotTTNum.Count; i++)
            {
                _yield.SlotTTNum[i] = 0;
                _yield.SlotFailNum[i] = 0;
            }
            SaveYieldToIniFile();
        }
        #endregion

        #region 打开与关闭设备
        /// <summary>
        /// 打开ATE设备连接
        /// </summary>
        /// <returns></returns>
        private bool OpenATE()
        {
            try
            {
                if (ChromaATE != null)
                    ChromaATE = null;

                string er = string.Empty;

                ChromaATE = new CChromaHwnd(CGlobalPara.SysPara.Para.Ate_Title_Name, CGlobalPara.SysPara.Para.Ate_Result_Folder,
                                            !CGlobalPara.SysPara.Para.Ate_scanSn_Enable, CGlobalPara.SysPara.Para.Ate_ScanSn_Name,
                                            CGlobalPara.SysPara.Dev.ChanMax, CGlobalPara.SysPara.Dev.Ate_Languge);

                if (!ChromaATE.GetATEHanlder(out er))
                {
                    UIMainArgs.bAlarm = true;
                    UIMainArgs.AlarmInfo =er;
                    UIMainArgs.DoRun = EUIStatus.ATE状态;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                    Log(er, udcRunLog.ELog.NG);
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
        /// 关闭ATE连接
        /// </summary>
        private void CloseATE()
        {
            try
            {
                if (ChromaATE != null)
                    ChromaATE = null;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 打开IO切换板
        /// </summary>
        /// <returns></returns>
        private bool OpenIO()
        {
            try
            {
                string er = string.Empty;

                if (_devIO == null)
                {
                    _devIO = new CGJX6Y6(0, CLanguage.Lan("ATE IO切换板"));

                    if (!_devIO.Open(CGlobalPara.SysPara.Dev.IoCom, out er))
                    {
                        _devIO.Close();
                        _devIO = null;
                        Log(CLanguage.Lan("打开ATE IO板串口失败:") + er, udcRunLog.ELog.NG);
                        return false;
                    }

                    List<int> X = new List<int>();

                    List<int> Y = new List<int>();

                    if (!_devIO.ReadXY(1, out X, out Y, out er))
                    {
                        _devIO.Close();
                        _devIO = null;
                        Log(CLanguage.Lan("读取ATE IO板数据失败:") + er, udcRunLog.ELog.NG);
                        return false;
                    }

                    Log(CLanguage.Lan("成功ATE IO板串口,检测ATE IO板通信正常."), udcRunLog.ELog.Action);
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
                    Log(CLanguage.Lan("关闭ATE IO板串口."), udcRunLog.ELog.Content);
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

                    Log(CLanguage.Lan("正常连接测试服务端") + "[" + CGlobalPara.SysPara.Dev.SerIP + ":" +
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
                                       CGlobalPara.SysPara.Dev.SerPort.ToString() + "]"+ CLanguage.Lan("失败") + 
                                       ":" + _conToTcpCount.ToString(), udcRunLog.ELog.NG);
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

                        int delayMs = CGlobalPara.SysPara.Dev.InternalTime;

                        Thread.Sleep(delayMs);

                        string er = string.Empty;

                        UpdateIotDeviceStatus();

                        if (!CheckSystem(delayMs))
                            continue;

                        if (!CheckStatReady())
                        {
                            Thread.Sleep(CGlobalPara.C_ALARM_DELAY);
                            continue;
                        }

                        for (int idNo = 0; idNo < CGlobalPara.C_STAT_MAX; idNo++)
                        {
                            CheckStatTest(idNo);
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
                    UIMainArgs.bAlarm = true;
                    UIMainArgs.AlarmInfo = CLanguage.Lan("连接错误");
                    UIMainArgs.DoRun = EUIStatus.TCP状态;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                    Log(er, udcRunLog.ELog.NG);
                    return false;
                }

                UIMainArgs.bAlarm = false;
                UIMainArgs.AlarmInfo =  CLanguage.Lan("连接正常");
                UIMainArgs.DoRun = EUIStatus.TCP状态;
                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                _conRecvTimeOut = 0;

                if (serReponse.Ready == 0)
                {
                    for (int i = 0; i < _statATE.Length; i++)
                    {
                        _statATE[i].DoRun = ERUN.空闲;
                    }
                    _devATE.DoRun = ERUN.空闲;
                    UIMainArgs.bAlarm = false;
                    UIMainArgs.AlarmInfo = string.Empty;
                    UIMainArgs.DoRun = EUIStatus.空闲;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                    return true;
                }
                else if (serReponse.Ready == 1) //工位1就绪
                {
                    idNo = 0;

                    _statATE[1].DoRun = ERUN.空闲;

                    if (_statATE[idNo].DoRun != ERUN.空闲)
                    {
                        return true;
                    }

                    _statATE[idNo].IdCard = serReponse.IdCard;
                    _statATE[idNo].ModelName = serReponse.ModelName;
                    _statATE[idNo].OrderName = serReponse.OrderName;
                    _statATE[idNo].MesFlag = serReponse.MesFlag;

                    for (int z = 0; z < _statATE[idNo].SerialNo.Count; z++)
                    {
                        _statATE[idNo].SerialNo[z] = serReponse.SerialNos[z];
                        _statATE[idNo].Result[z] = 0;
                    }
                }
                else if (serReponse.Ready == 2)
                {
                    idNo = 1;

                    _statATE[0].DoRun = ERUN.空闲;

                    if (_statATE[idNo].DoRun != ERUN.空闲)
                    {
                        return true;
                    }

                    _statATE[idNo].IdCard = serReponse.IdCard;
                    _statATE[idNo].ModelName = serReponse.ModelName;
                    _statATE[idNo].OrderName = serReponse.OrderName;
                    _statATE[idNo].MesFlag = serReponse.MesFlag;

                    for (int z = 0; z < _statATE[idNo].SerialNo.Count; z++)
                    {
                        _statATE[idNo].SerialNo[z] = serReponse.SerialNos[z];
                        _statATE[idNo].Result[z] = 0;
                    }
                }
                else
                {                    
                    _statATE[0].DoRun = ERUN.空闲;
                    _statATE[1].DoRun = ERUN.空闲;
                    Log(CLanguage.Lan("接收测试工位到位信号异常") + "【" + serReponse.Ready.ToString() + "】", udcRunLog.ELog.NG);
                    return false;
                }

                //停止已测试设备任务
                if (_devATE.Running)
                {
                    _devATE.DoRun = ERUN.空闲;
                }

                while (true)
                {
                    Application.DoEvents();

                    if (!_devATE.Running)
                        break;
                }

                _statATE[idNo].IdCard = serReponse.IdCard;
                _statATE[idNo].ModelName = serReponse.ModelName;
                _statATE[idNo].OrderName = serReponse.OrderName;
                _statATE[idNo].MesFlag = serReponse.MesFlag;
                _statATE[idNo].DoRun = ERUN.到位;
                for (int i = 0; i < _statATE[idNo].SerialNo.Count; i++)
                {
                    _statATE[idNo].SerialNo[i] = serReponse.SerialNos[i];
                    _statATE[idNo].Result[i] = 0;
                    _statATE[idNo].TestData[i] = "";
                }

                UIMainArgs.bAlarm = false;
                UIMainArgs.AlarmInfo = _statATE[idNo].ToString() + CLanguage.Lan("到位就绪");
                UIMainArgs.IdCard = _statATE[idNo].IdCard;
                UIMainArgs.SerialNo = _statATE[idNo].SerialNo;
                UIMainArgs.DoRun = EUIStatus.治具到位;
                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

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
        private void CheckStatTest(int idNo)
        {
            try
            {
                string er = string.Empty;

                switch (_statATE[idNo].DoRun)
                {
                    case ERUN.空闲:
                        break;
                    case ERUN.到位:
                        for (int i = 0; i < _devATE.SerialNo.Count; i++)
                        {
                            _devATE.SerialNo[i] = _statATE[idNo].SerialNo[i];
                            _devATE.Result[i] = 0;
                            _devATE.TestData[i] = "";
                        }
                        _devATE.ModelName = _statATE[idNo].ModelName;                        
                        _devATE.StepNo = 0;
                        _devATE.DoRun = ERUN.到位;

                        _statATE[idNo].DoRun = ERUN.等待;
                        _statATE[idNo].StartTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        _statATE[idNo].Watcher.Restart();

                        Task.Factory.StartNew(() => On_ATE_Task());

                        Log(_statATE[idNo].ToString() + CLanguage.Lan("治具ID") + "【" + _statATE[idNo].IdCard + "】"+
                                                        CLanguage.Lan("到位就绪,等待启动测试"), udcRunLog.ELog.OK);
                        break;
                    case ERUN.等待:
                       UIMainArgs.bAlarm = false;
                       UIMainArgs.AlarmInfo = _statATE[idNo].ToString() + CLanguage.Lan("等待ATE启动测试");
                       UIMainArgs.WaitTime = _statATE[idNo].Watcher.ElapsedMilliseconds;
                       UIMainArgs.DoRun = EUIStatus.测试状态;
                       OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                        if (_devATE.DoRun == ERUN.测试)
                        {
                            if (!START_TEST(CGlobalPara.SysPara.Dev.SerStat, out er))
                            {
                                Log(_statATE[idNo].ToString() + CLanguage.Lan("发送TCP测试信号错误:") + er, udcRunLog.ELog.NG);
                                break;
                            }
                            _statATE[idNo].DoRun = ERUN.测试;
                            break;
                        }
                        break;
                    case ERUN.测试:
                       UIMainArgs.bAlarm = false;
                       UIMainArgs.AlarmInfo = _statATE[idNo].ToString() + CLanguage.Lan("ATE测试中");
                       UIMainArgs.WaitTime = _statATE[idNo].Watcher.ElapsedMilliseconds;
                       UIMainArgs.DoRun = EUIStatus.测试状态;
                       OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                       if (_devATE.DoRun != ERUN.结束)
                           break;

                        for (int uutNo = 0; uutNo < _devATE.SerialNo.Count; uutNo++)
                        {
                            _statATE[idNo].Result[uutNo] = _devATE.Result[uutNo];
                            _statATE[idNo].TestData[uutNo] = _devATE.TestData[uutNo];
                        }
                        _statATE[idNo].EndTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        _statATE[idNo].Watcher.Stop();

                        double runTimes = (double)_statATE[idNo].Watcher.ElapsedMilliseconds / 1000;

                        //获取高压机产品结果
                        bool uutPass = true;
                        for (int i = 0; i < _statATE[idNo].SerialNo.Count; i++)
                        {
                            if (_statATE[idNo].SerialNo[i] == string.Empty)
                            {
                                _statATE[idNo].Result[i] = 0;
                            }
                            else
                            {
                                if (_statATE[idNo].Result[i] != 0)
                                {
                                    _statATE[idNo].Result[i] = CYOHOOApp.ATE_FlowId;
                                    uutPass = false;
                                }
                            }
                        }

                        //写入Web结果
                        if (!UpdateFixtureResult(idNo))
                        {
                            _statATE[idNo].DoRun = ERUN.报警;
                            UIMainArgs.bAlarm = true;
                            UIMainArgs.AlarmInfo = CLanguage.Lan("写入Web错误");
                            UIMainArgs.DoRun = EUIStatus.测试状态;
                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                            break;
                        }

                        //发TCP过站信号    
                        if (!END_TEST(CGlobalPara.SysPara.Dev.SerStat, _statATE[idNo].IdCard, _statATE[idNo].Result, out er))
                        {
                            Log(_statATE[idNo].ToString() + CLanguage.Lan("发送TCP过站信号错误:") + er, udcRunLog.ELog.NG);
                            break;
                        }

                        if (uutPass)
                        {
                            UIMainArgs.bAlarm = false;
                            UIMainArgs.AlarmInfo = _statATE[idNo].ToString() + CLanguage.Lan("测试PASS");
                            Log(_statATE[idNo].ToString() + CLanguage.Lan("治具ID") + "【" + _statATE[idNo].IdCard +
                                        "】"+ CLanguage.Lan("ATE测试:PASS,准备过站:") + runTimes.ToString("0.0") + "s", udcRunLog.ELog.OK);
                        }
                        else
                        {
                            UIMainArgs.bAlarm = true;
                            UIMainArgs.AlarmInfo = _statATE[idNo].ToString() + CLanguage.Lan("测试FAIL");
                            Log(_statATE[idNo].ToString() + CLanguage.Lan("治具ID") + "【" + _statATE[idNo].IdCard +
                                            "】"+ CLanguage.Lan("ATE测试:FAIL,准备过站:") + runTimes.ToString("0.0") + "s", udcRunLog.ELog.NG);
                        }

                        //统计产能
                        for (int i = 0; i < _statATE[idNo].SerialNo.Count; i++)
                        {
                            if (_statATE[idNo].SerialNo[i] != string.Empty)
                            {
                                _yield.TTNum++;
                                _yield.SlotTTNum[i]++;
                                if (_statATE[idNo].Result[i] != 0)
                                {
                                    _yield.FailNum++;
                                    _yield.SlotFailNum[i]++;
                                }
                            }
                        }
                        SaveYieldToIniFile();
                        SaveDailyReport(_statATE[idNo]);
                        //local_db_recordFailSn(_statATE[idNo]);
                        UIMainArgs.Result = _statATE[idNo].Result;
                        UIMainArgs.WaitTime = _statATE[idNo].Watcher.ElapsedMilliseconds;
                        UIMainArgs.DoRun = EUIStatus.测试结束;          
                        OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));                        
                        _statATE[idNo].DoRun = ERUN.过站;
                        break;
                    case ERUN.结束:
                        break;
                    case ERUN.报警:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region ATE测试任务
        /// <summary>
        /// 测试任务
        /// </summary>
        private void On_ATE_Task()
        {
            try
            {
                Log(_devATE.ToString() + CLanguage.Lan("开始测试"), udcRunLog.ELog.Action);

                _devATE.StartTime = DateTime.Now.ToString();

                _devATE.Watcher.Restart();

                _devATE.Running = true;

                while (true)
                {
                    Thread.Sleep(CGlobalPara.C_TASK_DELAY);

                    if (_devATE.DoRun == ERUN.空闲)
                        return;

                    if (!CheckChromaATE(CGlobalPara.C_TASK_DELAY))
                        continue;

                    string er = string.Empty;

                    switch (_devATE.DoRun)
                    {
                        case ERUN.到位:
                            if (!GetRunTestStep())
                            {
                                _devATE.DoRun = ERUN.结束;
                                UIMainArgs.bAlarm = true;
                                UIMainArgs.AlarmInfo = CLanguage.Lan("无可测试产品");
                                UIMainArgs.DoRun = EUIStatus.ATE状态;
                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));                                
                                return;
                            }
                            if (!SetIOAndHPEvent())
                            {
                                _devATE.DoRun = ERUN.报警;
                                UIMainArgs.bAlarm = true;
                                UIMainArgs.AlarmInfo = CLanguage.Lan("切换IO板错误");
                                UIMainArgs.DoRun = EUIStatus.ATE状态;
                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));                               
                                return;
                            }
                            _devATE.DoRun = ERUN.等待;
                            UIMainArgs.bAlarm = false;
                            UIMainArgs.AlarmInfo = _devATE.ToString() + CLanguage.Lan("就绪,准备启动测试");
                            UIMainArgs.DoRun = EUIStatus.ATE状态;
                            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                            Log(_devATE.ToString() + CLanguage.Lan("就绪,准备启动测试"), udcRunLog.ELog.OK);
                            break;
                        case ERUN.等待:
                            if (!Check_ATE_Ready())
                                break;
                            if (!Send_Sn_To_ATE())
                            {
                                _devATE.DoRun = ERUN.报警;
                                UIMainArgs.bAlarm = true;
                                UIMainArgs.AlarmInfo = _devATE.ToString() + CLanguage.Lan("传递产品条码失败");
                                UIMainArgs.DoRun = EUIStatus.ATE状态;
                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                                return;
                            }
                            _devATE.DoRun = ERUN.测试;
                            _devATE.RunTriger = false;
                            _devATE.RunTrigerAlarm = false;
                            _devATE.IoWatcher.Restart();
                             UIMainArgs.bAlarm = false;
                             UIMainArgs.AlarmInfo = _devATE.ToString() + CLanguage.Lan("启动开始测试");
                             UIMainArgs.DoRun = EUIStatus.ATE状态;
                             OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                             Log(_devATE.ToString() + CLanguage.Lan("启动开始测试"), udcRunLog.ELog.OK);
                            break;
                        case ERUN.测试:
                            if (ChromaATE.ConfirmMsgBoxYes(C_ATE_CONFIRM_INFO, C_ATE_CONFIRM_VALUE))
                                break;
                            if (!Check_Ate_Run_Triger())
                                break;
                             UIMainArgs.bAlarm = false;
                             UIMainArgs.AlarmInfo = _devATE.ToString() + CLanguage.Lan("测试中");
                             UIMainArgs.DoRun = EUIStatus.ATE状态;
                             OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                            //检测ATE测试结束信号
                            if (ChromaATE.ateRun.wPara.doFinish == 0)
                                break;
                            if (!Get_Ate_ResultAndData())
                            {
                                _devATE.DoRun = ERUN.报警;
                                UIMainArgs.bAlarm = true;
                                UIMainArgs.AlarmInfo = _devATE.ToString() + CLanguage.Lan("获取结果失败");
                                UIMainArgs.DoRun = EUIStatus.ATE状态;
                                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                                return;
                            }
                            _devATE.StepNo++;
                            if (GetRunTestStep())
                            {
                                _devATE.DoRun = ERUN.到位;
                                break;
                            }
                            _devATE.DoRun = ERUN.结束;
                            for (int i = 0; i < _devATE.Result.Count; i++)
                            {
                                UIMainArgs.Result[i] = _devATE.Result[i];
                            }
                             UIMainArgs.bAlarm = false;
                             UIMainArgs.AlarmInfo = _devATE.ToString() + CLanguage.Lan("测试结束");
                             UIMainArgs.DoRun = EUIStatus.测试结束;
                             OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
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
                if (C_DEBUG_MODE)
                {
                    CloseIO();
                    CloseATE();
                    C_DEBUG_MODE = false;
                    UIMainArgs.bAlarm = false;
                    UIMainArgs.AlarmInfo = string.Empty;
                    UIMainArgs.DebugMode = C_DEBUG_MODE;
                    UIMainArgs.DoRun = EUIStatus.调试模式;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                }
                _devATE.Watcher.Stop();
                _devATE.Running = false;
                double runTimes = ((double)_devATE.Watcher.ElapsedMilliseconds) / 1000;
                Log(_devATE.ToString() + CLanguage.Lan("测试结束") + ":" + runTimes.ToString("0.0") + "秒", udcRunLog.ELog.Action);
            }
        }
        /// <summary>
        /// 获取当前测试步骤
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool GetRunTestStep()
        {
            try
            {
                bool HaveUUT = false;

                int stepNum = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.DevMax / CGlobalPara.SysPara.Dev.ChanMax;

                for (int i = _devATE.StepNo; i < stepNum; i++)
                {
                    for (int slot = 0; slot < CGlobalPara.SysPara.Dev.ChanMax; slot++)
                    {
                        int uutNo = i * CGlobalPara.SysPara.Dev.ChanMax + slot;

                        if (_devATE.SerialNo[uutNo] != string.Empty)
                        {
                            HaveUUT = true;
                        }
                    }
                    if (HaveUUT)
                    {
                        _devATE.StepNo = i;
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
        /// 切换IO
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool SetIOAndHPEvent()
        {
            try
            {
                if (!CGlobalPara.SysPara.Dev.ChkIoEnable)
                    return true;

                string er = string.Empty;

                if (_devATE.StepNo == 0)
                {
                    if (!_devIO.CtrlYRelay(1, 1, 0, out er))
                    {
                        Thread.Sleep(CGlobalPara.SysPara.Dev.IoDelayMs);
                        if (!_devIO.CtrlYRelay(1, 1, 0, out er))
                        {
                            Log(_devATE.ToString() + CLanguage.Lan("切换IO通道失败:") + er, udcRunLog.ELog.NG);
                            return false;
                        }
                    }
                }
                else
                {
                    if (!_devIO.CtrlYRelay(1, 1, 1, out er))
                    {
                        Thread.Sleep(CGlobalPara.SysPara.Dev.IoDelayMs);
                        if (!_devIO.CtrlYRelay(1, 1, 1, out er))
                        {
                            Log(_devATE.ToString() + CLanguage.Lan("切换IO通道失败:") + er, udcRunLog.ELog.NG);
                            return false;
                        }
                    }
                }

                Log(_devATE.ToString() + CLanguage.Lan("切换IO通道:") + CLanguage.Lan("步骤") + "【" + (_devATE.StepNo + 1).ToString() + "】", udcRunLog.ELog.Action);

                Thread.Sleep(CGlobalPara.SysPara.Dev.IoDelayMs);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);

                return false;
            }
        }
        #endregion

        #region Chroma8020
        /// <summary>
        /// 检测Chroma ATE程序是否OK?
        /// </summary>
        /// <returns></returns>
        private bool CheckChromaATE(int delayTimes)
        {
            try
            {
                string er = string.Empty;

                if (ChromaATE == null)
                    return false;

                //2S重连接1次
                int counT1 = 2000 / delayTimes;

                if (!ChromaATE.GetATEHanlder(out er))
                {
                    if (_conToATECount < counT1)
                        _conToATECount++;
                    else
                    {
                        _conToATECount = 0;
                        Log(er, udcRunLog.ELog.NG);
                    }

                    UIMainArgs.bAlarm = true;
                    UIMainArgs.AlarmInfo = er;
                    UIMainArgs.DoRun = EUIStatus.ATE状态;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                    UIMainArgs.ProName = string.Empty;
                    UIMainArgs.ModeName = string.Empty;
                    UIMainArgs.ElapsedTime = string.Empty;
                    UIMainArgs.DoRun = EUIStatus.ATE信息;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));

                    return false;
                }               

                UIMainArgs.ProName = ChromaATE.ateRun.wPara.programName;
                UIMainArgs.ModeName = ChromaATE.ateRun.wPara.modelName;
                UIMainArgs.ElapsedTime = ChromaATE.ateRun.wPara.elapsedTime;
                UIMainArgs.DoRun = EUIStatus.ATE信息;
                OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
                return false;
            }
        }
        /// <summary>
        /// 检测Chroma ATE是否启动就绪?
        /// </summary>
        /// <returns></returns>
        private bool Check_ATE_Ready()
        {
            try
            {
                //检测程序处于运行状态-->停止运行
                if (ChromaATE.ateRun.wPara.doFinish == 0)
                {
                    UIMainArgs.bAlarm = true;
                    UIMainArgs.AlarmInfo = CLanguage.Lan("Chroma ATE未处于运行状态,请检查.");
                    UIMainArgs.DoRun = EUIStatus.测试状态;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
                    return false;
                }
                //检测程序是否处于扫描条码状态?
                if (ChromaATE.ateRun.wPara.doRun == 0)
                {
                    UIMainArgs.bAlarm = true;
                    UIMainArgs.AlarmInfo = CLanguage.Lan("Chroma ATE未处于输入条码框状态,请检查.");
                    UIMainArgs.DoRun = EUIStatus.测试状态;
                    OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
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
        /// 发条码到ATE程序
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool Send_Sn_To_ATE()
        {
            try
            {
                string er = string.Empty;

                List<string> serialNos = new List<string>();

                for (int i = 0; i < CGlobalPara.SysPara.Dev.ChanMax; i++)
                {
                    int uutNo = CGlobalPara.SysPara.Dev.ChanMax * _devATE.StepNo + i;
                    serialNos.Add(_devATE.SerialNo[uutNo]);
                }

                if (!ChromaATE.SendSnToSnText(serialNos, out er, CGlobalPara.SysPara.Para.Ate_getImg_handler))
                {
                    Log(_devATE.ToString() + CLanguage.Lan("传产品条码错误:") + er, udcRunLog.ELog.NG);
                    return false;
                }

                for (int i = 0; i < CGlobalPara.SysPara.Dev.ChanMax; i++)
                {
                    int uutNo = CGlobalPara.SysPara.Dev.ChanMax * _devATE.StepNo + i;

                    Log(_devATE.ToString() + CLanguage.Lan("传产品条码") +
                              (uutNo + 1).ToString("D2") + ":【" + serialNos[i] + "】", udcRunLog.ELog.Action);
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
        /// 检测ATE启动测试中
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool Check_Ate_Run_Triger()
        {
            try
            {
                if (_devATE.RunTriger)
                {
                    return true;
                }

                if (ChromaATE.ateRun.wPara.doFinish == 0)
                {
                    _devATE.RunTriger = true;
                }

                if (!_devATE.RunTriger)
                {
                    if (_devATE.IoWatcher.ElapsedMilliseconds > 5000)
                    {
                        if (!_devATE.RunTrigerAlarm)
                        {
                            Log(_devATE.ToString() + CLanguage.Lan("启动ATE开始测试超时"), udcRunLog.ELog.NG);
                            _devATE.RunTrigerAlarm = true;
                        }
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
        /// 获取测试结果及测试数据
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool Get_Ate_ResultAndData()
        {
            try
            {
                string er = string.Empty;

                //获取测试结果

                int uutNo = CGlobalPara.SysPara.Dev.ChanMax * _devATE.StepNo;

                if (CGlobalPara.SysPara.Para.Ate_getImg_handler)
                {
                    Thread.Sleep(CGlobalPara.SysPara.Para.Ate_Result_Delay);

                    if (!ChromaATE.GetResultFromImage(out er))
                    {
                        Log(_devATE.ToString() + CLanguage.Lan("获取测试结果失败:") + er, udcRunLog.ELog.NG);
                        return false;
                    }

                    for (int CH = 0; CH < CGlobalPara.SysPara.Dev.ChanMax; CH++)
                    {
                        _devATE.Result[CH + uutNo] = ChromaATE.ateRun.wResult.result[CH];
                    }
                    return true;
                }
                else
                {
                    bool IsExist = false;

                    for (int i = 0; i < CGlobalPara.SysPara.Para.Ate_Result_Repeats; i++)
                    {
                        Thread.Sleep(CGlobalPara.SysPara.Para.Ate_Result_Delay);

                        if (ChromaATE.GetSnResultFromDB(out IsExist, out er))
                        {
                            if (IsExist)
                            {
                                for (int CH = 0; CH < CGlobalPara.SysPara.Dev.ChanMax; CH++)
                                {
                                    _devATE.Result[CH + uutNo] = ChromaATE.ateRun.wResult.result[CH];
                                }
                                break;
                            }
                        }
                    }
                    if (!IsExist)
                    {
                        Log(_devATE.ToString() + CLanguage.Lan("获取测试结果超时:") + er, udcRunLog.ELog.NG);
                        return false;
                    }
                }

                //获取测试数据

                if (CGlobalPara.SysPara.Para.ATE_TestData)
                {
                    bool ChkFlag = false;

                    for (int i = 0; i < CGlobalPara.SysPara.Para.Ate_Result_Repeats; i++)
                    {
                        Thread.Sleep(CGlobalPara.SysPara.Para.Ate_Result_Delay);

                        if (ChromaATE.GetSnTestValue(out er))
                        {
                            for (int CH = 0; CH < CGlobalPara.SysPara.Dev.ChanMax; CH++)
                            {
                                _devATE.TestData[CH + uutNo] = ChromaATE.ateRun.wResult.testValue[CH];
                            }

                            ChkFlag = true;

                            break;
                        }

                    }

                    if (!ChkFlag)
                    {
                        Log(_devATE.ToString() + CLanguage.Lan("获取测试数据错误:") + er, udcRunLog.ELog.NG);
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

        #region TCP/IP消息
        private void OnTcpCon(object sender, CTcpConArgs e)
        {
            if (!e.bErr)
                TcpLog(e.conStatus, udcRunLog.ELog.Action);
            else
                TcpLog(e.conStatus, udcRunLog.ELog.NG);
        }
        private void OnTcpRecv(object sender, CTcpRecvArgs e)
        {
            //tcpLog.Log(e.recvData, udcRunLog.ELog.Content);
        }
        #endregion

        #region TCP命令协议
        /// <summary>
        /// 查询命令
        /// </summary>
        /// <param name="serName">站别名</param>
        /// <param name="er"></param>
        /// <returns></returns>
        private bool QUERY_STATE(string serName, out CSerReponse serReponse, out string er)
        {
            int uutMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.DevMax;

            serReponse = new CSerReponse(uutMax);

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
                    socketReponse.UUT_NUM = uutMax;
                    socketReponse.UUT = new SOCKET_UUT[uutMax];
                    for (int i = 0; i < uutMax; i++)
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
                    }
                    serReponse.Ready = socketReponse.Ready;
                    serReponse.IdCard = socketReponse.IdCard;
                    serReponse.ModelName = socketReponse.Model;
                    serReponse.OrderName = socketReponse.OrderName;
                    serReponse.MesFlag = socketReponse.MesFlag;
                    for (int i = 0; i < socketReponse.UUT_NUM; i++)
                        serReponse.SerialNos[i] = socketReponse.UUT[i].SerialNo;
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
                        serReponse.OrderName = "";
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
                        serReponse.OrderName="";
                        serReponse.MesFlag = 0;
                    }
                    else
                    {
                        serReponse.IdCard = ItemList[1];
                        serReponse.ModelName = ItemList[2];
                        serReponse.OrderName = ItemList[3];
                        serReponse.MesFlag = System.Convert.ToInt16(ItemList[4]);
                        int uutNo = System.Convert.ToInt16(ItemList[5]);
                        string[] serialNos = ItemList[6].Split(',');
                        for (int i = 0; i < uutNo; i++)
                            serReponse.SerialNos[i] = serialNos[i];
                    }
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
                    for (int i = 0; i < uutMax; i++)
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
                        serReponse.OrderName = "";
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
                        er = CLanguage.Lan("返回数据错误:") + objRecv.Name;
                        TcpLog(er, udcRunLog.ELog.NG);
                        return false;
                    }

                    serReponse.Ready = objRecv.Ready;
                    serReponse.IdCard = objRecv.IdCard;
                    serReponse.ModelName = objRecv.Model;
                    serReponse.OrderName = objRecv.OrderName;
                    serReponse.MesFlag = objRecv.MesFlag;
                    for (int i = 0; i < objRecv.UUT.Count; i++)
                        serReponse.SerialNos[i] = objRecv.UUT[i].SerialNo;
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
                int uutMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.DevMax;

                string cmd = ESOCKET_CMD.START_TEST.ToString() + "_" + serName;

                TcpLog(cmd, udcRunLog.ELog.Action);

                //自定结构体
                if (CGlobalPara.SysPara.Dev.TcpMode == 0)
                {
                    SOCKET_REQUEST socketReponse = new SOCKET_REQUEST();
                    socketReponse.Name = serName;
                    socketReponse.CmdNo = ESOCKET_CMD.START_TEST;
                    socketReponse.UUT_NUM = uutMax;
                    socketReponse.UUT = new SOCKET_UUT[uutMax];
                    for (int i = 0; i < uutMax; i++)
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
                    for (int i = 0; i < uutMax; i++)
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
                int uutMax = CYOHOOApp.SlotMax / CGlobalPara.SysPara.Dev.DevMax;

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
                    socketReponse.UUT_NUM = uutMax;
                    socketReponse.UUT = new SOCKET_UUT[uutMax];
                    for (int i = 0; i < uutMax; i++)
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
                    for (int i = 0; i < uutMax; i++)
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

        #region 产能统计
        private void SaveYieldToIniFile()
        {
            CIniFile.WriteToIni("Yield", "TTNum", _yield.TTNum.ToString(), CGlobalPara.IniFile);
            CIniFile.WriteToIni("Yield", "FailNum", _yield.FailNum.ToString(), CGlobalPara.IniFile);
            for (int i = 0; i < _yield.SlotTTNum.Count; i++)
            {
                CIniFile.WriteToIni("Yield", "SlotTTNum" + i.ToString(), _yield.SlotTTNum[i].ToString(), CGlobalPara.IniFile);
                CIniFile.WriteToIni("Yield", "SlotFailNum" + i.ToString(), _yield.SlotFailNum[i].ToString(), CGlobalPara.IniFile);
            }

            UIMainArgs.bAlarm = false;
            UIMainArgs.TTNum = _yield.TTNum;
            UIMainArgs.FailNum = _yield.FailNum;
            UIMainArgs.SlotTTNum = _yield.SlotTTNum;
            UIMainArgs.SlotFailNum = _yield.SlotFailNum;
            UIMainArgs.DoRun = EUIStatus.产能计数;
            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
        }
        private void LoadYieldFromIniFile()
        {
            _yield.TTNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Yield", "TTNum", CGlobalPara.IniFile, "0"));
            _yield.FailNum = System.Convert.ToInt32(CIniFile.ReadFromIni("Yield", "FailNum", CGlobalPara.IniFile, "0"));
            for (int i = 0; i < _yield.SlotTTNum.Count; i++)
            {
                _yield.SlotTTNum[i] = System.Convert.ToInt32(CIniFile.ReadFromIni("Yield", "SlotTTNum" + i.ToString(), CGlobalPara.IniFile, "0"));
                _yield.SlotFailNum[i] = System.Convert.ToInt32(CIniFile.ReadFromIni("Yield", "SlotFailNum" + i.ToString(), CGlobalPara.IniFile, "0"));
            }

            UIMainArgs.bAlarm = false;
            UIMainArgs.TTNum = _yield.TTNum;
            UIMainArgs.FailNum = _yield.FailNum;
            UIMainArgs.SlotTTNum = _yield.SlotTTNum;
            UIMainArgs.SlotFailNum = _yield.SlotFailNum;
            UIMainArgs.DoRun = EUIStatus.产能计数;
            OnUIMainArgs.OnEvented(new CUIUserArgs<CUIMainArgs>(idNo, name, UIMainArgs));
        }
        #endregion

        #region 冠佳Web
        /// <summary>
        /// 写入Web治具结果
        /// </summary>
        /// <param name="idNo"></param>
        /// <returns></returns>
        private bool UpdateFixtureResult(int idNo)
        {
            try
            {
                string er = string.Empty;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                CWeb2.CFixture fixture = new CWeb2.CFixture();
                fixture.Base.FlowIndex = 0;
                fixture.Base.FlowName = CYOHOOApp.ATE_FlowName;
                fixture.Base.FlowGuid = CNet.HostName();
                fixture.Base.SnType = CWeb2.ESnType.外部条码;
                fixture.Base.IdCard = _statATE[idNo].IdCard;
                fixture.Base.CheckSn = CGlobalPara.SysPara.Mes.ChkWebSn;

                for (int i = 0; i < _statATE[idNo].SerialNo.Count; i++)
                {
                    CWeb2.CFix_Para para = new CWeb2.CFix_Para();
                    para.SlotNo = i;
                    para.SerialNo = _statATE[idNo].SerialNo[i];
                    para.InnerSn = string.Empty;
                    para.Remark1 = ChromaATE.ateRun.wPara.programName;
                    para.Remark2 = ChromaATE.ateRun.wPara.modelName;
                    para.StartTime = _statATE[idNo].StartTime;
                    para.EndTime = _statATE[idNo].EndTime;
                    para.Result = _statATE[idNo].Result[i];
                    para.TestData = string.Empty; //_statATE[idNo].TestData[i];
                    fixture.Para.Add(para);
                }

                if (!CWeb2.UpdateFixtureResult(fixture, out er))
                {
                    MessageBox.Show(_statATE[idNo].ToString() + CLanguage.Lan("写入治具ID") + "[" + _statATE[idNo].IdCard + "]"+
                                                                CLanguage.Lan("测试结果错误:") + er);

                    if (!CWeb2.UpdateFixtureResult(fixture, out er))
                    {
                        Log(_statATE[idNo].ToString() + CLanguage.Lan("写入治具ID") + "[" + _statATE[idNo].IdCard + "]"+
                                                        CLanguage.Lan("测试结果错误:") + er, udcRunLog.ELog.NG);

                        return false;
                    }
                }
                else
                {
                    Log(_statATE[idNo].ToString() + CLanguage.Lan("写入治具ID") + "[" + _statATE[idNo].IdCard + "]"+
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

                        string fileName = string.Empty;

                        //string failInfo = hub.TestData[i];

                        string failInfo = string.Empty;

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
        private void SaveDailyReport(CStat hub)
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
        private void saveReportVal(string fileName, CStat hub)
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

                    strWrite = (i + 1).ToString("D2") + ",";

                    strWrite += hub.SerialNo[i] + ",";

                    strWrite += hub.ToString() + ",";

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
    }

}
