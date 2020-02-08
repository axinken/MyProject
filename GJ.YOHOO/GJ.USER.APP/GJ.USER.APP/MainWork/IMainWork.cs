using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GJ.UI;
using GJ.APP;
using GJ.COM;
using GJ.Iot;
using GJ.USER.APP.Iot;
using System.Drawing;

namespace GJ.USER.APP.MainWork
{
    public abstract class IMainWork
    {
        #region 构造函数
        /// <summary>
        /// 加载参数
        /// </summary>
        public bool C_DownLoad = false;
        /// <summary>
        /// 运行状态
        /// </summary>
        public bool C_RUNNING = false;
        /// <summary>
        /// 初始化类信息
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="name"></param>
        public void CMainWork(int idNo, string name,string guid)
        {
            this.idNo = idNo;
            this.name = name;
            this.guid = guid;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void InitialUI()
        {
            InitUIWorker = new BackgroundWorker();
            InitUIWorker.WorkerSupportsCancellation = true;
            InitUIWorker.DoWork += new DoWorkEventHandler(InitUIWorker_DoWork);
            InitUIWorker.RunWorkerAsync();
        }
        /// <summary>
        /// 启动监控
        /// </summary>
        public virtual bool OnRun()
        {
            try
            {
                if (!InitialRunPara())
                    return false;

                if (!OpenDevice())
                {
                    CloseDevice();
                    return false;
                }

                if (!StartThread())
                {
                    StopThread();
                    CloseDevice();
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
        /// 停止监控
        /// </summary>
        public virtual void OnStop()
        {
            try
            {
                StopThread();

                CloseDevice();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 窗体关闭
        /// </summary>
        public virtual void CloseDlg()
        {
            try
            {
                StopThread();

                CloseDevice();

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        #endregion

        #region 初始化线程
        /// <summary>
        /// 初始化线程
        /// </summary>
        private BackgroundWorker InitUIWorker = null;
        /// <summary>
        /// 初始化线程运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitUIWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                Log(CLanguage.Lan("加载系统配置XML文件."), udcRunLog.ELog.Action);

                LoadSysFile();

                Log(CLanguage.Lan("加载系统运行数据库配置."), udcRunLog.ELog.Action);

                LoadRunPara();

                Log(CLanguage.Lan("加载系统配置INI文件."), udcRunLog.ELog.Action);

                LoadIniFile();

                Log(CLanguage.Lan("加载机种参数配置XML文件."), udcRunLog.ELog.Action);

                LoadModelPara();

                Log(CLanguage.Lan("加载主界面UI状态信息."), udcRunLog.ELog.Action);

                LoadMainFormUI();

                Log(CLanguage.Lan("系统参数和主界面UI初始化完成."), udcRunLog.ELog.Action);

                LoadUIComplete();

                C_DownLoad = true;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
            finally
            {
                if (!C_DownLoad)
                {
                    Log(CLanguage.Lan("系统初始化失败,请检查."), udcRunLog.ELog.NG);
                }
            }
        }
        #endregion

        #region  初始化虚方法
        /// <summary>
        /// 加载系统配置
        /// </summary>
        public virtual void LoadSysFile()
        {
            try
            {
                
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 加载测试参数
        /// </summary>
        public virtual void LoadRunPara()
        {
            try
            {
               

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 加载Ini文件
        /// </summary>
        public virtual void LoadIniFile()
        {
            try
            {
               

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 加载机种参数
        /// </summary>
        public virtual void LoadModelPara()
        {
            try
            {


            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 加载界面UI信息
        /// </summary>
        public virtual void LoadMainFormUI()
        {
            try
            {

            }
            catch (Exception)
            {
                
                throw;
            }
        }
        /// <summary>
        /// 初始化UI结束
        /// </summary>
        public virtual void LoadUIComplete()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 抽象方法
        /// <summary>
        /// 初始测试参数
        /// </summary>
        public abstract bool InitialRunPara();
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns></returns>
        public abstract bool OpenDevice();
        /// <summary>
        /// 关闭设备
        /// </summary>
        public abstract void CloseDevice();
        /// <summary>
        /// 启动线程
        /// </summary>
        /// <returns></returns>
        public abstract bool StartThread();
        /// <summary>
        /// 停止线程
        /// </summary>
        public abstract void StopThread();
        #endregion

        #region 定义UI消息
        /// <summary>
        /// 日志消息
        /// </summary>
        public COnEvent<CUILogArgs> OnUILogArgs = new COnEvent<CUILogArgs>();
        /// <summary>
        /// 报警消息
        /// </summary>
        public COnEvent<CUILogArgs> OnPLCLogArgs = new COnEvent<CUILogArgs>();
        /// <summary>
        /// 全局消息
        /// </summary>
        public COnEvent<CUIGlobalArgs> OnUIGlobalArgs = new COnEvent<CUIGlobalArgs>();
        /// <summary>
        /// 显示消息
        /// </summary>
        public COnEvent<CUIInicatorArgs> OnUIInidcatorArgs = new COnEvent<CUIInicatorArgs>();
        #endregion

        #region 字段
        /// <summary>
        /// 编号
        /// </summary>
        private int idNo = 0;
        /// <summary>
        /// 名称
        /// </summary>
        private string name = "MainWork";
        /// <summary>
        /// 窗口消息
        /// </summary>
        private string guid = string.Empty;
        #endregion

        #region UI刷新方法
        /// <summary>
        /// 显示日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="log"></param>
        /// <param name="save"></param>
        public void Log(string info, udcRunLog.ELog log, bool save = true)
        {
            OnUILogArgs.OnEvented(new CUILogArgs(idNo, name, info, log, save));
        }
        /// <summary>
        /// 显示PLC日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="log"></param>
        /// <param name="save"></param>
        public void PLCLog(string info, udcRunLog.ELog log, bool save = true)
        {
            OnPLCLogArgs.OnEvented(new CUILogArgs(idNo, name, info, log, save));
        }
        /// <summary>
        /// 显示启动状态
        /// </summary>
        /// <param name="status"></param>
        public void Indicator(EIndicator status)
        {
            OnUIInidcatorArgs.OnEvented(new CUIInicatorArgs(idNo, name, status));
        }
        #endregion

        #region 物联网Iot
        /// <summary>
        /// 任务标识
        /// </summary>
        private CancellationTokenSource Iot_cts = null;
        /// <summary>
        /// 上报周期(S)
        /// </summary>
        private double Iot_CycleTime = 0;
        /// <summary>
        /// 任务状态
        /// </summary>
        private bool Iot_Task_Cycle = false;
        /// <summary>
        /// 监控时间
        /// </summary>
        private Stopwatch Iot_Watcher_Task = new Stopwatch();
        /// <summary>
        /// IP地址
        /// </summary>
        private string Iot_IP = string.Empty;
        /// <summary>
        /// 端口
        /// </summary>
        private int Iot_Port = 0;
        /// <summary>
        /// 设备工厂
        /// </summary>
        private string Iot_Factory = string.Empty;
        /// <summary>
        /// 设备列表
        /// </summary>
        private List<CDevList> Iot_Devices = new List<CDevList>();
        /// <summary>
        /// 消息计数
        /// </summary>
        private int Iot_Count = 0;
        /// <summary>
        /// 设备客户端
        /// </summary>
        private CClient Iot_Client = null;
        /// <summary>
        /// 设备运行状态消息类
        /// </summary>
        public Dictionary<string, CData_Status> Iot_DevRunStatus = new Dictionary<string, CData_Status>();
        /// <summary>
        /// 初始化设备信息
        /// </summary>
        public void InitialIotDevice(string ipAndPort, string factory, List<CDevList> deviceList)
        {
            try
            {
                Iot_Factory = factory;

                string[] ip_port = ipAndPort.Split(':');

                if (ip_port.Length != 2)
                {
                    Log("Iot服务器【" + ipAndPort + "】配置错误", udcRunLog.ELog.NG);
                    return;
                }

                Iot_IP = ip_port[0];

                Iot_Port = System.Convert.ToInt32(ip_port[1]);

                Iot_Devices = deviceList;

                Iot_DevRunStatus.Clear();

                for (int i = 0; i < Iot_Devices.Count; i++)
                {
                    Iot_DevRunStatus.Add(Iot_Devices[i].idNo,new CData_Status());

                    Iot_DevRunStatus[Iot_Devices[i].idNo].ID = Iot_Devices[i].idNo;

                    Iot_DevRunStatus[Iot_Devices[i].idNo].Name = Iot_Devices[i].Name;

                    Iot_DevRunStatus[Iot_Devices[i].idNo].RunStatus = 0;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), udcRunLog.ELog.Err);
            }
        }
        /// <summary>
        /// 开启Iot监控
        /// </summary>
        public virtual void StartIot()
        {
            try
            {                

                Iot_cts = new CancellationTokenSource();

                string er = string.Empty;

                if (!ConncetToIot(Iot_IP, Iot_Port, out er))
                {
                    Log(er, udcRunLog.ELog.NG);
                    return;
                }

                Log(er, udcRunLog.ELog.Action);

                int count = 0;

                Stopwatch watcher = new Stopwatch();

                watcher.Start();

                while (true)
                {
                    Thread.Sleep(5);

                    if (Iot_cts.IsCancellationRequested)
                        return;

                    if (watcher.ElapsedMilliseconds < 3000)
                        continue;

                    watcher.Restart();

                    if (Iot_Client != null && Iot_Client.IsConnected)
                        continue;

                    count++;

                    CloseToIot();

                    if (!ConncetToIot(Iot_IP, Iot_Port, out er))
                    {
                        Log("重连接物联网Iot功能正常:" + count.ToString(), udcRunLog.ELog.Action);
                    }
                }
            }
            catch (Exception ex)
            {
                Iot_Client = null;
                Log(ex.ToString(), udcRunLog.ELog.NG);
            }
            finally
            {
                Iot_cts = null;
            }
        }
        /// <summary>
        /// 停止Iot监控
        /// </summary>
        public virtual void StopIot()
        {
            try
            {
                if (Iot_cts != null)
                    Iot_cts.Cancel();

                if (Iot_Task_Cycle)
                {
                    Iot_CycleTime = -1;
                }
                while (Iot_Task_Cycle || Iot_cts != null)
                {
                    Application.DoEvents();
                }

                CloseToIot();
            }
            catch (Exception)
            {

            }           
        }
        /// <summary>
        /// 接收应答指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnIotREQCmd(object sender, CClient.CCmdArgs e)
        {
            try
            {
                FrmClient.Subscribe("【REQ COMMAND】:" + DateTime.Now.ToString("HH:mm:ss.") + DateTime.Now.Millisecond.ToString("D3") + "\r\n", Color.Black);

                FrmClient.Subscribe("【Topic】:", Color.Black);

                FrmClient.Subscribe(e.topic + "\r\n", Color.Blue);

                FrmClient.Subscribe("【Message】:", Color.Black);

                FrmClient.Subscribe(e.message + "\r\n", Color.Green);

                ECmdType cmdType = (ECmdType)e.data.Data[0].CmdType;
            }
            catch (Exception ex)
            {
                FrmClient.Subscribe(ex.ToString() + "\r\n", Color.Red);
            }
        }
        /// <summary>
        /// 应答主控消息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool ReponseIotCommand(string cmdName,string info, out string er)
        {
            er = string.Empty;

            try
            {
                if (Iot_Client == null || !Iot_Client.IsConnected)
                {
                    FrmClient.Subscribe("服务端断开连接" + "\r\n", Color.Red);
                    return false;
                }

                if (!Iot_Client.Reponse_Command(info, out er))
                {
                    FrmClient.Subscribe(er + "\r\n", Color.Red);
                }
                else
                {
                    FrmClient.Publish("执行主控命令指令【" + cmdName + "】OK" + "\r\n", Color.Blue);
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
        /// 接收广播指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIotRPTCmd(object sender, CClient.CCmdArgs e)
        {
            FrmClient.Subscribe("【RPT COMMAND】:" + DateTime.Now.ToString("HH:mm:ss.") + DateTime.Now.Millisecond.ToString("D3") + "\r\n", Color.Black);

            FrmClient.Subscribe("【Topic】:", Color.Black);

            FrmClient.Subscribe(e.topic + "\r\n", Color.Blue);

            FrmClient.Subscribe("【Message】:", Color.Black);

            FrmClient.Subscribe(e.message + "\r\n", Color.Green);

            try
            {
                ECmdType cmdType = (ECmdType)e.data.Data[0].CmdType;

                if (cmdType == ECmdType.上报状态)
                {
                    for (int i = 0; i < e.data.Data.Count; i++)
                    {
                        if (!ContainDeviceId(e.data.Data[i].ID))
                            continue;

                        Iot_CycleTime = System.Convert.ToDouble(e.data.Data[i].CmdInfo);

                        if (Iot_CycleTime != -1)
                        {
                            if (!Iot_Task_Cycle)
                            {
                                Iot_Task_Cycle = true;
                                Task.Factory.StartNew(() => OnTask_ReportStatus());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FrmClient.Subscribe(ex.ToString() + "\r\n", Color.Red);
            }
        }
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIotRecieve(object sender, CClient.CCMessageArgs e)
        {
            Iot_Count++;

            FrmClient.Subscribe("【Time】:" + DateTime.Now.ToString("HH:mm:ss.") + DateTime.Now.Millisecond.ToString("D3") + "\r\n", Color.Black);

            FrmClient.Subscribe("【Topic】:", Color.Black);

            FrmClient.Subscribe(e.topic + "\r\n", Color.Blue);

            FrmClient.Subscribe("【Message】:", Color.Black);

            FrmClient.Subscribe(e.message + "\r\n", Color.Green);

            FrmClient.Subscribe("---------------------------------->" + Iot_Count.ToString() + "\r\n", Color.DarkOrange);

        }
        /// <summary>
        /// 连接Iot服务器
        /// </summary>
        private bool ConncetToIot(string ip, int port, out string er)
        {
            er = string.Empty;

            try
            {
                if (Iot_Client != null)
                {
                    Iot_Client.Close();
                    Iot_Client = null;
                }

                Iot_Count = 0;

                Iot_Client = new CClient(0, "Iot", CNet.HostIP(), CNet.HostName(), Iot_Factory, Iot_Devices);

                if (!Iot_Client.Connect(ip, port, out er))
                {
                    Iot_Client = null;
                    return false;
                }

                Iot_Client.OnMessageArgs.OnEvent += new COnEvent<CClient.CCMessageArgs>.OnEventHandler(OnIotRecieve);
                Iot_Client.OnCmdRPTArgs.OnEvent += new COnEvent<CClient.CCmdArgs>.OnEventHandler(OnIotRPTCmd);
                Iot_Client.OnCmdREQArgs.OnEvent += new COnEvent<CClient.CCmdArgs>.OnEventHandler(OnIotREQCmd);

                if (!Iot_Client.Request_Command(out er))
                    return false;

                er = "启动物联网Iot功能正常";

                return true;
            }
            catch (Exception ex)
            {
                Iot_Client = null;
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 断开Iot服务连接
        /// </summary>
        private void CloseToIot()
        {
            if (Iot_Client != null)
            {
                Iot_Client.OnMessageArgs.OnEvent -= new COnEvent<CClient.CCMessageArgs>.OnEventHandler(OnIotRecieve);
                Iot_Client.OnCmdRPTArgs.OnEvent -= new COnEvent<CClient.CCmdArgs>.OnEventHandler(OnIotRPTCmd);
                Iot_Client.OnCmdREQArgs.OnEvent -= new COnEvent<CClient.CCmdArgs>.OnEventHandler(OnIotREQCmd);
                Iot_Client.Close();
                Iot_Client = null;
            }
        }
        /// <summary>
        /// 上传状态消息
        /// </summary>
        private void OnTask_ReportStatus()
        {
            try
            {
                int count = 0;

                while (true)
                {
                    Thread.Sleep(5);

                    if (Iot_CycleTime == -1)
                        return;

                    if (Iot_CycleTime == 0)
                    {
                        Iot_CycleTime = -1;
                    }
                    else
                    {
                        if (Iot_Watcher_Task.IsRunning && Iot_Watcher_Task.ElapsedMilliseconds < Iot_CycleTime * 1000)
                            continue;

                        Iot_Watcher_Task.Restart();
                    }

                    string er = string.Empty;

                    List<CData_Status> message = new List<CData_Status>();

                    for (int i = 0; i < Iot_Devices.Count; i++)
                    {
                        string devIdNo = Iot_Devices[i].idNo;

                        message.Add(Iot_DevRunStatus[devIdNo]);
                        
                    }

                    count++;

                    FrmClient.Publish("【Time】:" + DateTime.Now.ToString("HH:mm:ss.") + DateTime.Now.Millisecond.ToString("D3") + "\r\n", Color.Black);

                    if (Iot_Client == null || !Iot_Client.IsConnected)
                        return;

                    if (!Iot_Client.Report_Status(message, out er))
                    {
                        FrmClient.Publish("【Message】:" + er + "\r\n", Color.Red);
                    }
                    else
                    {
                        FrmClient.Publish("【Message】:" + "上传设备状态信息OK----" + count.ToString() + "\r\n", Color.Blue);
                    }
                }
            }
            catch (Exception ex)
            {
                FrmClient.Publish(ex.ToString() + "\r\n", Color.Red);
            }
            finally
            {
                Iot_Task_Cycle = false;
            }
        }
        /// <summary>
        /// 设备ID是否存在
        /// </summary>
        /// <param name="deviceIdNo"></param>
        /// <returns></returns>
        private bool ContainDeviceId(string deviceIdNo)
        {
            for (int i = 0; i < Iot_Devices.Count; i++)
            {
                if (deviceIdNo == "-1")
                    return true;

                if (Iot_Devices[i].idNo.ToUpper() == deviceIdNo.ToUpper())
                    return true;
            }
            return false;
        }
        #endregion

    }
}
