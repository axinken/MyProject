using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.KunX.LOADUP
{
    #region 全局类
    public class CGlobalPara
    {
        #region 常量定义
        /// <summary>
        /// 工位标识名称
        /// </summary>
        public static string StationName = "LOADUP";
        /// <summary>
        /// 设备标识
        /// </summary>
        public static string DeviceIDNo = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DeviceName = "通电测试位";
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
        /// 延时清除界面
        /// </summary>
        public static int C_CLEAR_UI_DELAY = 3000;
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
        public const int C_ID_MAX = 2;
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

    #region 系统参数类
    /// <summary>
    /// 通信设备
    /// </summary>
    [Serializable]
    public class CSYS_Dev
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string PlcIP = "192.168.3.101";
        /// <summary>
        /// ID卡串口
        /// </summary>
        public string IdCom = "COM1";
        /// <summary>
        /// 电子负载串口
        /// </summary>
        public string EloadCom = "COM2";
        /// <summary>
        /// 快充主控板
        /// </summary>
        public string FCMBCom = "COM3";
        /// <summary>
        /// 电表串口
        /// </summary>
        public string MeterCom = "COM4";
        /// <summary>
        /// 条码枪串口
        /// </summary>
        public string[] SnCom = new string[] { "COM4", "COM5" };
        /// <summary>
        /// 条码枪波特率
        /// </summary>
        public string SnBaud = "115200,E,8,1";
        /// <summary>
        /// 条码枪类型
        /// </summary>
        public string SnMode = "SR700";
        /// <summary>
        /// 监控周期(ms)
        /// </summary>
        public int MonInterval = 100;
    }
    /// <summary>
    /// 测试参数
    /// </summary>
    [Serializable]
    public class CSYS_Para
    {
        /// <summary>
        /// AC延时启动时间(ms)
        /// </summary>
        public int AcDelayTimes = 3000;
        /// <summary>
        /// 不良重测次数
        /// </summary>
        public int ReTestTimes = 3;
        /// <summary>
        /// 重测延时(ms)
        /// </summary>
        public int ReTestDelay=1000;
        /// <summary>
        /// 重读治具ID次数
        /// </summary>
        public int IdReTimes = 3;
        /// <summary>
        /// 重读条码次数
        /// </summary>
        public int ReadSnTimes = 3;
        /// <summary>
        /// 不良人工取产品
        /// </summary>
        public bool ChkFailWait=false;
        /// <summary>
        /// 读取Vs
        /// </summary>
        public bool ChkVSenor=false;
        /// <summary>
        /// AC ON
        /// </summary>
        public bool ChkACON = false;
        /// <summary>
        /// 启用空载开机
        /// </summary>
        public bool ChkIdleLoad = false;
        /// <summary>
        /// 空载电流
        /// </summary>
        public double IdleLoad = 0.5;

    }
    /// <summary>
    /// 报警参数
    /// </summary>
    [Serializable]
    public class CSYS_Alarm
    {
        /// <summary>
        /// 工位使用寿命
        /// </summary>
        public int StatTimes;
        /// <summary>
        /// 工位连续不良
        /// </summary>
        public int StatFailTimes;
        /// <summary>
        /// 工位良率下限报警
        /// </summary>
        public double StatPassRate = 0;
        /// <summary>
        /// 治具使用寿命
        /// </summary>
        public int FixtureTimes;
        /// <summary>
        /// 治具连续不良报警
        /// </summary>
        public int FixFailTimes;
        /// <summary>
        /// 治具良率下限报警
        /// </summary>
        public double FixPassRate;

        public bool ChkPassRate = false;

        public double PassRateLimit = 1;

        public int PassRateStartNum = 100;

        public int PassRateAlarmTime = 10;

        public bool ChkClrDay = false;

        public bool ChkClrNight = false;

        public string ClrDayTime = "08:00:00";

        public string ClrNightTime = "20:00:00";
       
    }
    /// <summary>
    /// 测试报表
    /// </summary>
    [Serializable]
    public class CSYS_Report
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
    /// MES设置
    /// </summary>
    [Serializable]
    public class CSYS_MES
    {
        /// <summary>
        /// 冠佳Web条码检查
        /// </summary>
        public int ChkWebSn = 0;
        /// <summary>
        /// MES连线
        /// </summary>
        public bool Connect = false;
        /// <summary>
        /// 上传初测MES数据
        /// </summary>
        public bool TranDataToMes = false;
        /// <summary>
        /// 扫描条码模式
        /// </summary>
        public ESnMode SnMode = ESnMode.无条码模式; 
        /// <summary>
        /// 条码长度
        /// </summary>
        public int SnLen = 0;
        /// <summary>
        /// 条码规则
        /// </summary>
        public string SnSpec = string.Empty;
        /// <summary>
        /// 检查BI
        /// </summary>
        public bool ChkSnBI = false;
        /// <summary>
        /// 检查HP
        /// </summary>
        public bool ChkSnHP = false;
        /// <summary>
        /// 检查ATE
        /// </summary>
        public bool ChkSnATE = false;
    }
    /// <summary>
    /// 系统参数
    /// </summary>
    [Serializable]
    public class CSysPara
    {
        /// <summary>
        /// 通信设备
        /// </summary>
        public CSYS_Dev Dev = new CSYS_Dev();
        /// <summary>
        /// 测试参数
        /// </summary>
        public CSYS_Para Para = new CSYS_Para();
        /// <summary>
        /// 报警参数
        /// </summary>
        public CSYS_Alarm Alarm = new CSYS_Alarm();
        /// <summary>
        /// 测试报表
        /// </summary>
        public CSYS_Report Report = new CSYS_Report();
        /// <summary>
        /// MES设置
        /// </summary>
        public CSYS_MES Mes = new CSYS_MES();
    }
    #endregion

    #region 机种参数
    /// <summary>
    /// 基本信息
    /// </summary>
    [Serializable]
    public class CM_Base
    {
        /// <summary>
        /// 机种名称
        /// </summary>
        public string Model;
        /// <summary>
        /// 机种客户
        /// </summary>
        public string Custom;
        /// <summary>
        /// 机种版本
        /// </summary>
        public string Version;
        /// <summary>
        /// 发行人
        /// </summary>
        public string ReleaseName;
        /// <summary>
        /// 发行日期 
        /// </summary>
        public string ReleaseDate;

    }
    [Serializable]
    public class CM_OutPut
    {
        /// <summary>
        /// 输入AC
        /// </summary>
        public int ACVolt = 220;
        /// <summary>
        /// AC下限
        /// </summary>
        public double ACvMin = 0;
        /// <summary>
        /// AC上限
        /// </summary>
        public double ACvMax = 0;
        /// <summary>
        /// 输出名称
        /// </summary>
        public string Vname=string.Empty;
        /// <summary>
        /// 输出下限
        /// </summary>
        public double Vmin=0;
        /// <summary>
        /// 输出上限
        /// </summary>
        public double Vmax=0;
        /// <summary>
        /// 负载模式
        /// </summary>
        public int LoadMode=0;
        /// <summary>
        /// 负载Von
        /// </summary>
        public double LoadVon=0;
        /// <summary>
        /// 负载电流
        /// </summary>
        public double LoadSet=0;
        /// <summary>
        /// 电流下限
        /// </summary>
        public double LoadMin=0;
        /// <summary>
        /// 电流上限
        /// </summary>
        public double LoadMax=0;
        /// <summary>
        /// 电压补偿值
        /// </summary>
        public double VOffSet = 0;
        /// <summary>
        /// 设置快充模式
        /// </summary>
        public bool ChkQCM = false;
        /// <summary>
        /// 快充模式
        /// </summary>
        public int QCM = 1;
        /// <summary>
        /// 快充电压
        /// </summary>
        public double QCV = 5;
        /// <summary>
        /// 启用短路检测
        /// </summary>
        public bool ChkDD = false;
        /// <summary>
        /// D+D-短路检测
        /// </summary>
        public bool[] ChkDG = new bool[4];
    }
    [Serializable]
    public class CModelPara
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        public CM_Base Base = new CM_Base();
        /// <summary>
        /// 输出信息
        /// </summary>
        public CM_OutPut OutPut = new CM_OutPut();
    }
    #endregion
}
