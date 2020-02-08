using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.YOHOO.UNLOAD
{
    #region 全局类
    public class CGlobalPara
    {
        #region 常量定义
        /// <summary>
        /// 工位标识名称
        /// </summary>
        public static string StationName = "UNLOAD";
        /// <summary>
        /// 设备标识
        /// </summary>
        public static string DeviceIDNo = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DeviceName = "自动下机位";
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
        /// PLC运行数据库
        /// </summary>
        public static string PLCDB = Application.StartupPath + "\\" + StationName + "\\PLC_" + StationName + ".accdb";
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
        /// 软件启动时间
        /// </summary>
        public static int C_SOFTWRE_TIME = 2000;
        /// <summary>
        /// 报警次数
        /// </summary>
        public static int C_ALARM_TIME = 3;
        /// <summary>
        /// 任务延时
        /// </summary>
        public const int C_TASK_DELAY = 10;
        /// <summary>
        /// 读卡器数量
        /// </summary>
        public const int C_ID_MAX = 5;
        /// <summary>
        /// 高压工位数量:单工位;双工位
        /// </summary>
        public const int C_HP_STAT_MAX = 2;
        /// <summary>
        /// ATE工位数量:单工位;双工位
        /// </summary>
        public const int C_ATE_STAT_MAX = 2;
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
        /// PLC地址
        /// </summary>
        public string PlcIp = "192.168.3.231";
        /// <summary>
        /// 读卡器串口
        /// </summary>
        public string IdCom = "COM1";
        /// <summary>
        /// TCP端口
        /// </summary>
        public int TcpPort = 8000;
        /// <summary>
        /// 协议->0:结构体 1:字符串 2:JSON格式
        /// </summary>
        public int TcpMode = 1;
        /// <summary>
        /// 高压TCP名称
        /// </summary>
        public string HP_TCP="HIPOT";
        /// <summary>
        /// ATE TCP名称
        /// </summary>
        public string ATE_TCP = "ATE";
        /// <summary>
        /// 扫描间隔
        /// </summary>
        public int MonInterval = 100;
        /// <summary>
        /// 记录ID模式
        /// </summary>
        public int IdRecordMode = 0;
        /// <summary>
        /// ATE设备测试数量:0,1
        /// </summary>
        public int AteDevMax = 0;
    }
    [Serializable]
    public class _CPara
    {
        /// <summary>
        /// 高压不良人工确认
        /// </summary>
        public bool ChkHPFail = false;
        /// <summary>
        /// ATE不良人工确认
        /// </summary>
        public bool ChkATEFail = false;
        /// <summary>
        /// 高压不测试
        /// </summary>
        public bool ChkNoHP = false;
        /// <summary>
        /// ATE不测试
        /// </summary>
        public bool ChkNoATE = false;
        /// <summary>
        /// 强制高压测试
        /// </summary>
        public bool ChkInHP = false;
        /// <summary>
        /// 强制ATE测试
        /// </summary>
        public bool ChkInATE = false;
        /// <summary>
        /// 高压不良锁住
        /// </summary>
        public bool ChkLockFail = false;
        /// <summary>
        /// 高压解锁密码
        /// </summary>
        public string LockHPPwr = "GuanJia";
        /// <summary>
        /// 重读治具ID次数
        /// </summary>
        public int IdReTimes;
        /// <summary>
        /// 提示机种变更
        /// </summary>
        public bool ChkModel = false;
        /// <summary>
        /// 超过不良直接不取产品
        /// </summary>
        public int FailGoNum = 0;
        /// <summary>
        /// 调试过站
        /// </summary>
        public bool ChkGoPass = false;
        /// <summary>
        /// ATE上传GJWeb
        /// </summary>
        public bool ChkATEToGJWeb = false;
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
        /// <summary>
        /// 日产能报表
        /// </summary>
        public string DailyFolder = "D:\\DailyRecord";
    }
    /// <summary>
    /// 报警参数
    /// </summary>
    [Serializable]
    public class _CAlarm
    {
        public bool[] ChkPassRate = new bool[2];

        public double[] PassRateLimit = new double[] { 1,1};

        public int[] PassRateStartNum = new int[] { 100,100 };

        public int PassRateAlarmTime = 10;

        public bool ChkClrDay = false;

        public bool ChkClrNight = false;

        public string ClrDayTime = "08:00:00";

        public string ClrNightTime = "20:00:00";

    }
    [Serializable]
    public class _CMES
    {
        /// <summary>
        /// 检查冠佳Web
        /// </summary>
        public int ChkWebSn = 1;
        /// <summary>
        /// MES连线模式
        /// </summary>
        public bool Connect = false;
        /// <summary>
        /// 检查工位流程
        /// </summary>
        public bool ChkStat = false; 
    }
    [Serializable]
    public class CSysPara
    {
        /// <summary>
        /// 设备参数
        /// </summary>
        public _CDev Dev = new _CDev();
        /// <summary>
        /// 测试参数
        /// </summary>
        public _CPara Para = new _CPara();
        /// <summary>
        /// 报表参数
        /// </summary>
        public _CReport Report = new _CReport();
        /// <summary>
        /// 报警提示
        /// </summary>
        public _CAlarm Alarm = new _CAlarm();
        /// <summary>
        /// MES参数
        /// </summary>
        public _CMES Mes = new _CMES();
    }
    #endregion

}
