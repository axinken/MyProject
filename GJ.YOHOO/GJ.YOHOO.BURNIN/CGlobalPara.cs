using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.YOHOO.BURNIN
{
    /// <summary>
    /// 全局类
    /// </summary>
    public class CGlobalPara
    {
        #region 常量定义
        /// <summary>
        /// 工位标识名称
        /// </summary>
        public static string StationName = "BURNIN";
        /// <summary>
        /// 设备标识
        /// </summary>
        public static string DeviceIDNo = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DeviceName = "老化测试位";
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
        /// 库位文件
        /// </summary>
        public static string UUTFile = Application.StartupPath + "\\" + StationName + "\\" + StationName + ".uut";
        /// <summary>
        /// 测试点文件
        /// </summary>
        public static string LedFile = Application.StartupPath + "\\" + StationName + "\\" + StationName + ".led";
        /// <summary>
        /// 入口治具表单
        /// </summary>
        public static string InPlatTable = "InPlatFixture";
        /// <summary>
        /// 初始化状态
        /// </summary>
        public static bool DownLoad = false;
        /// <summary>
        /// 系统运行中
        /// </summary>
        public static bool C_RUNNING = false;
        /// <summary>
        /// 设备启动扫描
        /// </summary>
        public static bool C_SCAN_START = false;
        /// <summary>
        /// 初始化扫描监控
        /// </summary>
        public static bool C_INI_SCAN = false;
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
        /// 软件启动时间
        /// </summary>
        public static int C_SOFTWRE_TIME = 2000;
        /// <summary>
        /// 老化台车数
        /// </summary>
        public static int C_ROOM_MAX = 1;
        /// <summary>
        /// 层数
        /// </summary>
        public static int C_ROW_MAX = 10;
        /// <summary>
        /// 列数
        /// </summary>
        public static int C_COL_MAX = 10;
        /// <summary>
        /// 子治具数量
        /// </summary>
        public static int C_UUT_MAX = 200;
        /// <summary>
        /// 读卡器数量
        /// </summary>
        public static int C_ID_MAX = 2;
        /// <summary>
        /// 输出段数
        /// </summary>
        public static int C_OUTPUT_MAX = 4;
        /// <summary>
        /// 控制板数量
        /// </summary>
        public static int C_MON_NUM = 5;
        /// <summary>
        /// 控制板数量
        /// </summary>
        public static int[] C_MON_MAX = new int[] { 40, 40, 40, 40, 40 };
        /// <summary>
        /// 控制板对应层次
        /// </summary>
        public static string[] C_MON_NAME = new string[] { "【L1-L2控制板】", "【L3-L4控制板】", "【L5-L6控制板】", "【L7-L8控制板】", "【L9-L10控制板】" };
        /// <summary>
        /// ERS负载数量
        /// </summary>
        public static int C_ERS_MAX = 50;
        /// <summary>
        /// ERS负载名称
        /// </summary>
        public static string C_ERS_NAME = "【L1-L10 ERS】";
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

    #region 系统参数类
    /// <summary>
    /// 通信设备
    /// </summary>
    [Serializable]
    public class CSYS_Dev
    {
        /// <summary>
        /// 老化PLC地址
        /// </summary>
        public string Bi_plc = "192.168.3.200";
        /// <summary>
        /// 读卡器串口
        /// </summary>
        public string IdCom = "COM1";
        /// <summary>
        /// 控制板串口
        /// </summary>
        public string[] MonCom = new string[] { "COM2", "COM3", "COM4", "COM5", "COM6" };
        /// <summary>
        /// ERS串口
        /// </summary>
        public string[] ErsCom = new string[] { "COM4" };
        /// <summary>
        /// ERS波特率
        /// </summary>
        public string ErsBaud = "9600,N,8,1";
        /// <summary>
        /// 扫描时间(ms)
        /// </summary>
        public int MonInterval = 100;
        /// <summary>
        /// 控制板电压通道对应关系->0:从左到右 1:从右到左
        /// </summary>
        public int VoltPos = 0;
        /// <summary>
        /// 控制输入AC时序模式
        /// </summary>
        public ECtrlACMode CtrlACMode = ECtrlACMode.PLC控制时序;
        /// <summary>
        /// 老化库体计时模式
        /// </summary>
        public ECtrlTimeMode CtrlTimeMode = ECtrlTimeMode.上位机计时模式1;
        /// <summary>
        /// 检测快充板版本
        /// </summary>
        public bool ChkFCMBVer = false;
        /// <summary>
        /// 快充板版本号
        /// </summary>
        public string FCMBVer = "6.1";
        /// <summary>
        /// 快充电流偏差值
        /// </summary>
        public double FCMBQCI = 0;
    }
    /// <summary>
    /// 测试参数
    /// </summary>
    [Serializable]
    public class CSYS_Para
    {
        /// <summary>
        /// 不判断电流
        /// </summary>
        public bool ChkNoJugdeCur = false;
        /// <summary>
        /// 不锁住当机
        /// </summary>
        public bool ChkNoLockFail = false;
        /// <summary>
        /// 禁止老化出机
        /// </summary>
        public bool ChkForbitOut = false;
        /// <summary>
        /// 手动进机位置
        /// </summary>
        public bool ChkHandIn = false;
        /// <summary>
        /// 电压校正下限
        /// </summary>
        public double VLP = 1;
        /// <summary>
        /// 电压校正上限
        /// </summary>
        public double VHP = 1;
        /// <summary>
        /// 电流校正下限
        /// </summary>
        public double ILP = 1;
        /// <summary>
        /// 电流校正上限
        /// </summary>
        public double IHP = 1;
        /// <summary>
        /// 电流补偿值
        /// </summary>
        public double IOFFSET = 0;
        /// <summary>
        /// 串联负载Von
        /// </summary>
        public double ERSVon = 60;
        /// <summary>
        /// 空载电流
        /// </summary>
        public double IdleLoad = 0.5;
        /// <summary>
        /// 负载延时拉载时间(S)
        /// </summary>
        public double LoadDelayS = 5;
        /// <summary>
        /// 读卡重读次数
        /// </summary>
        public int IdTimes = 0;
        /// <summary>
        /// 自动调用老化机种
        /// </summary>
        public bool ChkAutoModel = false;
        /// <summary>
        /// 老化出同一机种
        /// </summary>
        public bool ChkSameModel = false;
        /// <summary>
        /// 同一机种出机间隔
        /// </summary>
        public int ModelTimes = 10;
        /// <summary>
        /// 空治具自动出机
        /// </summary>
        public bool ChkNullAutoOut = false;
        /// <summary>
        /// 不显示报警提示
        /// </summary>
        public bool NoShowAlarm = false;
        /// <summary>
        /// 检测输入AC电压切换
        /// </summary>
        public bool ChkACVolt = false;
        /// <summary>
        /// 老化计时赔率
        /// </summary>
        public double BITimeRate = 1;
        /// <summary>
        ///备份数据库时间(Min)
        /// </summary>
        public int BackUpDBTime = 3;
        /// <summary>
        /// 快速ON/OFF模式
        /// </summary>
        public bool ChkFastOnOff = false;
        /// <summary>
        /// 库位自检功能
        /// </summary>
        public bool ChkAutoSelf = false;
    }
    /// <summary>
    /// 报警参数
    /// </summary>
    [Serializable]
    public class CSYS_Alarm
    {
        /// <summary>
        /// 通信异常报警
        /// </summary>
        public int ComFailTimes = 0;
        /// <summary>
        /// 电压不良次数
        /// </summary>
        public int FailTimes = 0;
        /// <summary>
        /// 电流不良次数
        /// </summary>
        public int FailCurTimes = 0;
        /// <summary>
        /// 针盘使用寿命
        /// </summary>
        public int FixUserTimes = 0;
        /// <summary>
        /// 针盘不良锁住
        /// </summary>
        public int FixFailLockNum = 0;
        /// <summary>
        /// 检测快充电压
        /// </summary>
        public int Chk_qcv_times = 30;
        /// <summary>
        /// 动作超时时间(S)
        /// </summary>
        public int OP_AlarmDelayS = 30;
        /// <summary>
        /// 启用良率预警功能
        /// </summary>
        public bool ChkPassRate = false;
        /// <summary>
        /// 良率预警下限(%)
        /// </summary>
        public double PassRateLimit = 90;
        /// <summary>
        /// 预警开始数量
        /// </summary>
        public int PassRateStartNum = 100;
        /// <summary>
        /// 预警报警间隔时间(Min)
        /// </summary>
        public int PassRateAlarmTime = 10;
        /// <summary>
        /// 白班清零使能
        /// </summary>
        public bool ChkClrDay = true;
        /// <summary>
        /// 夜班清零使能
        /// </summary>
        public bool ChkClrNight = true;
        /// <summary>
        /// 白班清零时间
        /// </summary>
        public string ClrDayTime = "08:00:00";
        /// <summary>
        /// 夜班清零时间
        /// </summary>
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
        public string DayRecordPath = "DailyReport";
        /// <summary>
        /// 保存温度间隔(秒)
        /// </summary>
        public int TempScanTime = 30;
        /// <summary>
        /// 温度记录
        /// </summary>
        public string TempPath = "D:\\Temperature";
    }
    /// <summary>
    /// MES设置
    /// </summary>
    [Serializable]
    public class CSYS_MES
    {
        public int ChkWebSn = 1;
        public bool Connect = false;
        public bool ChkSn = false;
        public bool ChkGoPass = false;
        /// <summary>
        /// 当前测试工位
        /// </summary>
        public string CurStation = string.Empty;
        /// <summary>
        /// 测试设备名称
        /// </summary>
        public string PCName = string.Empty;
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
        public string Model;
        public string Custom;
        public string Version;

        public string ReleaseName;
        public string ReleaseDate;

    }
    /// <summary>
    /// 测试参数
    /// </summary>
    [Serializable]
    public class CM_Para
    {
        /// <summary>
        /// 老化时间(H)
        /// </summary>
        public double BITime = 0;
        /// <summary>
        /// 输入电压
        /// </summary>
        public int ACV = 0;
        /// <summary>
        /// 快充规格
        /// </summary>
        public int QCV_Type = 0;
        /// <summary>
        /// 产品输出组数
        /// </summary>
        public int OutPut_Chan = 1;
        /// <summary>
        /// 输出定义组数
        /// </summary>
        public int OutPut_Num = 0;
        /// <summary>
        /// ON/OFF组数
        /// </summary>
        public int OnOff_Num = 0;
        /// <summary>
        /// 90V电压时间(Min)
        /// </summary>
        public int AC_90V = 0;
        /// <summary>
        /// 110V电压时间(Min)
        /// </summary>
        public int AC_110V = 0;
        /// <summary>
        /// 264V电压时间(Min)
        /// </summary>
        public int AC_220V = 0;
        /// <summary>
        /// 264V电压时间(Min)
        /// </summary>
        public int AC_264V = 0;
        /// <summary>
        /// 300V电压时间(Min)
        /// </summary>
        public int AC_300V = 0;
        /// <summary>
        /// 330V电压时间(Min)
        /// </summary>
        public int AC_330V = 0;
        /// <summary>
        /// 380V电压时间(Min)
        /// </summary>
        public int AC_380V = 0;
        /// <summary>
        /// 电压补偿值
        /// </summary>
        public double VOffset = 0;
    }
    /// <summary>
    /// 温度参数
    /// </summary>
    [Serializable]
    public class CM_Temp
    {
        public double TSet = 0;
        public double TLP = 0;
        public double THP = 0;
        public double THAlarm = 0;
        public double TOPEN = 0;
        public double TCLOSE = 0;
    }

    /// <summary>
    /// 单通道输出规格
    /// </summary>
    public class COutPut_CH
    {
        public string Vname = string.Empty;
        public double Vmin = 0;
        public double Vmax = 0;
        public int Imode = 0;
        public double ISet = 0;
        public double Imin = 0 ;
        public double Imax =0;
        public int QCType = 0;
        public double QCV =0;
    }
    /// <summary>
    /// 输出规格列表
    /// </summary>
    [Serializable]
    public class COutPut_List
    {
        public COutPut_List()
        {

            for (int i = 0; i < CHAN_MAX; i++)
            {
                Chan[i] = new COutPut_CH(); 

                Chan[i].Vname = _Vname[i];
                Chan[i].Vmin = _Vmin[i];
                Chan[i].Vmax = _Vmax[i];
                Chan[i].Imode = _Imode[i];
                Chan[i].ISet = _ISet[i];
                Chan[i].Imin = _Imin[i];
                Chan[i].Imax = _Imax[i];
                Chan[i].QCType = 0;
                Chan[i].QCV = 5.0;
            }
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public COutPut_List Clone()
        {
            COutPut_List outPut = new COutPut_List();

            outPut.Describe = this.Describe;

            for (int i = 0; i < CHAN_MAX; i++)
            {
                outPut.Chan[i] = new COutPut_CH();
                outPut.Chan[i].Vname = this.Chan[i].Vname;
                outPut.Chan[i].Vmin = this.Chan[i].Vmin;
                outPut.Chan[i].Vmax = this.Chan[i].Vmax;
                outPut.Chan[i].Imode = this.Chan[i].Imode; 
                outPut.Chan[i].ISet = this.Chan[i].ISet;
                outPut.Chan[i].Imin = this.Chan[i].Imin;
                outPut.Chan[i].Imax = this.Chan[i].Imax;
                outPut.Chan[i].QCType = this.Chan[i].QCType;
                outPut.Chan[i].QCV = this.Chan[i].QCV;
            }

            return outPut;
        }
        /// <summary>
        /// 最多通道数
        /// </summary>
        private const int CHAN_MAX = 8;
        /// <summary>
        /// 功能描述
        /// </summary>
        public string Describe;
        /// <summary>
        /// 输出规格
        /// </summary>
        public COutPut_CH[] Chan = new COutPut_CH[CHAN_MAX];
        private string[] _Vname = new string[] { "+5V", "+12V1", "+12V2", "+12V3", "+12V4", "+12V5", "+3.3V", "+5Vsb" };
        private double[] _Vmin = new double[] { 4.75, 11.75, 11.75, 11.75, 11.75, 11.75, 2.75,4.75};
        private double[] _Vmax = new double[] { 5.25, 12.25, 12.25, 12.25, 12.25, 12.25, 3.75, 5.25 };
        private int[] _Imode = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 }; 
        private double[] _ISet = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
        private double[] _Imin = new double[] { 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8 };
        private double[] _Imax = new double[] { 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2 };
    }
    /// <summary>
    /// ON/OFF子类
    /// </summary>
    [Serializable]
    public class COnOff_Item
    {
        /// <summary>
        /// 0：分钟 1：秒单位
        /// </summary>
        public int ChkSec=0;
        /// <summary>
        /// 循环次数
        /// </summary>
        public int OnOffTime = 0;
        /// <summary>
        /// ON时间(S)
        /// </summary>
        public int OnTime = 0;
        /// <summary>
        /// OFF时间(S)
        /// </summary>
        public int OffTime = 0;
        /// <summary>
        /// 输入电压
        /// </summary>
        public int ACV = 0;
        /// <summary>
        /// 输出类型
        /// </summary>
        public int OutPutType = 0;
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public COnOff_Item Clone()
        {
            COnOff_Item item = new COnOff_Item();

            item.ChkSec = this.ChkSec;

            item.OnOffTime = this.OnOffTime;

            item.OnTime = this.OnTime;

            item.OffTime = this.OffTime;

            item.ACV = this.ACV; 

            item.OutPutType = this.OutPutType;

            return item;
        }
    }
    /// <summary>
    /// ON/OFF步骤
    /// </summary>
    [Serializable]
    public class COnOff_List
    {
        public COnOff_List()
        {
            Item = new COnOff_Item[ITEM_MAX]; 

            for (int i = 0; i < ITEM_MAX; i++)
            {
                Item[i] = new COnOff_Item();

                Item[i].ChkSec = 0;

                Item[i].OnOffTime = 0;

                Item[i].OnTime = 0;

                Item[i].OffTime = 0;

                Item[i].ACV = 220;

                Item[i].OutPutType = 0;
            }
        
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public COnOff_List Clone()
        {
            try
            {
                COnOff_List list = new COnOff_List();

                list.Describe = this.Describe;

                list.TotalTime = this.TotalTime;

                for (int i = 0; i < ITEM_MAX; i++)
                {
                    list.Item[i].ChkSec = this.Item[i].ChkSec;
                    list.Item[i].OnOffTime = this.Item[i].OnOffTime;
                    list.Item[i].OnTime = this.Item[i].OnTime;
                    list.Item[i].OffTime = this.Item[i].OffTime;
                    list.Item[i].ACV = this.Item[i].ACV; 
                    list.Item[i].OutPutType = this.Item[i].OutPutType; 
                }

                return list;
            }
            catch (Exception)
            {                
                throw;
            }
        }
        /// <summary>
        /// 功能描述
        /// </summary>
        public string Describe;
        /// <summary>
        /// 总时间(S)
        /// </summary>
        public int TotalTime = 0;
        /// <summary>
        /// ON/OFF项目
        /// </summary>
        public COnOff_Item[] Item;
        /// <summary>
        /// 4段ON/OFF
        /// </summary>
        private int ITEM_MAX = 4;
    }

    [Serializable]
    public class CModelPara
    {
        public CModelPara()
        {
            for (int i = 0; i < C_ITEM_MAX; i++)
            {
                OutPut[i] = new COutPut_List();
                OnOff[i] = new COnOff_List();
            }
        }
        private const int C_ITEM_MAX = 8;
        public CM_Base Base = new CM_Base();
        public CM_Para Para = new CM_Para();
        public CM_Temp Temp = new CM_Temp();
        public COutPut_List[] OutPut = new COutPut_List[C_ITEM_MAX];
        public COnOff_List[] OnOff = new COnOff_List[C_ITEM_MAX]; 
    }
    #endregion

}
