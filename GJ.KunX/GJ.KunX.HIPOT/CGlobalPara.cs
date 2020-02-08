using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.DEV.HIPOT;

namespace GJ.KunX.HIPOT
{
    #region 全局类
    public class CGlobalPara
    {
        #region 常量定义
        /// <summary>
        /// 工位标识名称
        /// </summary>
        public static string StationName = "HIPOT";
        /// <summary>
        /// 设备标识
        /// </summary>
        public static string DeviceIDNo = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DeviceName = "高压测试位";
        /// <summary>
        /// 系统参数路径
        /// </summary>
        public static string SysFile = Application.StartupPath + "\\" + StationName + "\\" + StationName + ".xml";
        /// <summary>
        /// 工位配置文件路径
        /// </summary>
        public static string IniFile = Application.StartupPath + "\\" + StationName + "\\" + StationName + ".ini";
        /// <summary>
        /// 运行数据库
        /// </summary>
        public static string SysDB = Application.StartupPath + "\\" + StationName + "\\" + StationName + ".accdb";
        /// <summary>
        /// 初始化状态
        /// </summary>
        public static bool DownLoad = false;
        /// <summary>
        /// 系统运行中
        /// </summary>
        public static bool C_RUNNING = false;
        /// <summary>
        /// 线程扫描间隔
        /// </summary>
        public static int C_THREAD_DELAY = 20;
        /// <summary>
        /// 报警延时
        /// </summary>
        public static int C_ALARM_DELAY = 1000;
        /// <summary>
        /// 报警次数
        /// </summary>
        public static int C_ALARM_TIME = 3;
        /// <summary>
        /// 任务延时
        /// </summary>
        public static int C_TASK_DELAY = 10;
        #endregion

        #region 用户信息
        /// <summary>
        /// 用户权限等级
        /// </summary>
        private const int C_PWR_LEVEL_MAX = 8;
        /// <summary>
        /// 登录用户
        /// </summary>
        public static string LogName = "op";
        /// <summary>
        /// 登录权限
        /// </summary>
        public static int[] LogLevel = new int[C_PWR_LEVEL_MAX];
        #endregion

        #region 系统参数
        public static CSysPara SysPara = new CSysPara();
        /// <summary>
        /// 加载系统参数
        /// </summary>
        public static void LoadSysXml()
        {
            CSerializable<CSysPara>.ReadXml(SysFile, ref SysPara);
        }
        #endregion
    }
    #endregion

    #region 系统参数
    [Serializable]
    public class _CDev
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string SerIP = "127.0.0.1";
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int SerPort = 8000;
        /// <summary>
        /// 客户端工位名
        /// </summary>
        public string SerStat = "HIPOT";
        /// <summary>
        /// 协议->0:结构体 1:字符串 2:自定义JSON
        /// </summary>
        public int TcpMode = 1;
        /// <summary>
        /// 高压串口
        /// </summary>
        public string[] HPCom =new string[]{ "COM1","COM2"};
        /// <summary>
        /// 高压IO使能
        /// </summary>
        public bool ChkIoEnable = true;
        /// <summary>
        /// 高压IO板
        /// </summary>
        public string IoCom = "COM3";
        /// <summary>
        /// 监控时间(ms)
        /// </summary>
        public int MonInterval = 500;
        /// <summary>
        /// IO切换时间
        /// </summary>
        public int IoDelayMs = 50;
        /// <summary>
        /// 高压型号
        /// </summary>
        public EHPType HPType = EHPType.Chroma19020;
        /// <summary>
        /// 高压机波特率(默认:9600,N,8,1)
        /// </summary>
        public string HPBaud = "9600,N,8,1";
        /// <summary>
        /// 高压机通道
        /// </summary>
        public int HPChanMax = 8;
        /// <summary>
        /// 高压设备数量
        /// </summary>
        public int HPDevMax = 1;
    }
    [Serializable]
    public class _CPara
    {
        /// <summary>
        /// 自动调高压程序
        /// </summary>
        public bool ChkAutoModel = false;
        /// <summary>
        /// 调用高压机内部程序
        /// </summary>
        public bool ChkImpPrg = false;
        /// <summary>
        /// 高压不良重测试功能
        /// </summary>
        public bool ChkReTest = false;
    }
    [Serializable]
    public class _CReport
    {
        /// <summary>
        /// 获取机种路径
        /// </summary>
        public string ModelPath = "D:\\Model";
        /// <summary>
        /// 保存测试报表
        /// </summary>
        public bool SaveReport = false;
        /// <summary>
        /// 保存报表间隔时间
        /// </summary>
        public double SaveReportTimes = 1;
        /// <summary>
        /// 测试报表路径
        /// </summary>
        public string ReportPath = "D:\\Report";
    }
    [Serializable]
    public class _CMES
    {
        public int ChkWebSn = 1;
        public bool Connect = false;
        public int FailNoTranNum = 0;
    }
    [Serializable]
    public class CSysPara
    {
        public _CDev Dev = new _CDev();
        public _CPara Para = new _CPara();
        public _CReport Report = new _CReport();
        public _CMES Mes = new _CMES();
    }
    #endregion

    #region 机种参数
    [Serializable]
    public class _CModelBase
    {
        public string Model;
        public string Custom;
        public string Version;
        public string ReleaseName;
        public string ReleaseDate;
        /// <summary>
        /// 高压机程序名
        /// </summary>
        public string HPModel;
    }
    [Serializable]
    public class _CModelPara
    {
        /// <summary>
        /// 高压机测试通道
        /// </summary>
        public List<int> HpCH = new List<int>();
        /// <summary>
        /// IO测试通道
        /// </summary>
        public List<int> IoCH = new List<int>();
        public List<CHPPara.CStep> Step = new List<CHPPara.CStep>();
    }
    [Serializable]
    public class CModelPara
    {
        public _CModelBase Base = new _CModelBase();
        public _CModelPara Para = new _CModelPara();
    }
    #endregion

}
