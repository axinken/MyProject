using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.KunX.ATE
{
    #region 全局类
    public class CGlobalPara
    {
        #region 常量定义
        /// <summary>
        /// 工位标识名称
        /// </summary>
        public static string StationName = "ATE";
        /// <summary>
        /// 设备标识
        /// </summary>
        public static string DeviceIDNo = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DeviceName = "ATE测试位";
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
        /// 暂停初始化设备
        /// </summary>
        public static bool C_CANCEL_INIT = false;
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
        /// <summary>
        /// 测试工位
        /// </summary>
        public static int C_STAT_MAX = 2;
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
        /// <summary>
        /// 系统参数
        /// </summary>
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
        /// 服务端IP
        /// </summary>
        public string SerIP = "127.0.0.1";
        /// <summary>
        /// 服务端端口
        /// </summary>
        public int SerPort = 8000;
        /// <summary>
        /// 工位名称
        /// </summary>
        public string SerStat = "ATE";
        /// <summary>
        /// 协议->0:结构体 1:字符串 2:JSON
        /// </summary>
        public int TcpMode = 0;
        /// <summary>
        /// IO板使能
        /// </summary>
        public bool ChkIoEnable = false;
        /// <summary>
        /// IO串口
        /// </summary>
        public string IoCom = "COM1";
        /// <summary>
        /// IO切换延时ms
        /// </summary>
        public int IoDelayMs = 100;
        /// <summary>
        /// 扫描间隔ms
        /// </summary>
        public int InternalTime = 500;
        /// <summary>
        /// 中英语界面
        /// </summary>
        public int Ate_Languge = 0;
        /// <summary>
        /// ATE设备测试数量:1/2
        /// </summary>
        public int DevMax = 1;
        /// <summary>
        /// ATE设备测试通道:4/8/16
        /// </summary>
        public int ChanMax = 16;
    }
    [Serializable]
    public class _CPara
    {
        public string Ate_Title_Name = "Chroma 8020 IDE - [Execution Control]";
        public string Ate_Result_Folder = @"C:\Program Files\Chroma\Chroma 8020\Log";
        public string Ate_ScanSn_Name = "BarCode Reader";

        public bool Ate_scanSn_Enable = false;
        public int Ate_Mon = 200;
        public int Ate_Result_Delay = 100;
        public int Ate_Result_Repeats = 3;
        /// <summary>
        /// 通过图片获取测试结果
        /// </summary>
        public bool Ate_getImg_handler = false;
        /// <summary>
        /// 不良数超过设置人工确认
        /// </summary>
        public int FailNumLock = 0;
        /// <summary>
        /// 获取ATE测试数据
        /// </summary>
        public bool ATE_TestData = false;
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
        public bool SaveReport=false;
        /// <summary>
        /// 保存报表间隔时间
        /// </summary>
        public double SaveReportTimes=1;
        /// <summary>
        /// 测试报表路径
        /// </summary>
        public string ReportPath= "D:\\Report";
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

}
