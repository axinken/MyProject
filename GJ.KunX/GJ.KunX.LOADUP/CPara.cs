using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using GJ.DEV.PLC;

namespace GJ.KunX.LOADUP
{
    #region UI消息定义
    /// <summary>
    /// 机种UI信息消息
    /// </summary>
    public class CUIModelArgs : EventArgs
    {
        public int idNo = 0;
        public string name = string.Empty;
        public readonly CModelPara model;
        public readonly int lPara;
        public readonly int wPara;
        public CUIModelArgs(int idNo, string name, CModelPara model, int lPara = 0, int wPara = 0)
        {
            this.idNo = idNo;
            this.name = name;
            this.model = model;
            this.lPara = lPara;
            this.wPara = wPara;
        }
    }
    #endregion

    #region 参数枚举
    /// <summary>
    /// 运行状态
    /// </summary>
    public enum ERUN
    {
        空闲,
        到位,
        就绪,
        绑定,
        测试,
        正常治具过站,
        点检治具过站,
        空治具过站,
        等待,
        完成,
        读卡报警,
        异常报警
    }
    /// <summary>
    /// 报警状态
    /// </summary>
    public enum EAlarm
    {
        正常,
        读卡错误,
        状态错误,
        治具禁用,
        超过使用寿命,
        超过连续不良次数,
        低于治具良率设置,
        绑定错误,
        获取治具信息错误
    }
    /// <summary>
    /// 扫描条码模式
    /// </summary>
    public enum ESnMode
    { 
      无条码模式,
      自动扫描模式,
      人工扫描模式,
      人工串口模式
    }
    #endregion

    #region PLC类
    public class CUSER_PLCREG
    {
        /// <summary>
        /// 扫描寄存器
        /// </summary>
        public List<CPLCThread.CREG> scanREG = new List<CPLCThread.CREG>();
        /// <summary>
        /// 写寄存器
        /// </summary>
        public List<CPLCThread.CREG> wREG = new List<CPLCThread.CREG>();
        /// <summary>
        /// 读寄存器
        /// </summary>
        public List<CPLCThread.CREG> rREG = new List<CPLCThread.CREG>();
    }
    #endregion

    #region 参数类
    /// <summary>
    /// 站别信息
    /// </summary>
    public class CStatInfo
    {
        public CStatInfo(int uutMax)
        {
            CheckSn = new List<bool>();

            for (int i = 0; i < uutMax; i++)
            {
                CheckSn.Add(true); 
            }
        }

        public List<bool> CheckSn = null;
        /// <summary>
        /// 设置新负载
        /// </summary>
        public bool SetNewLoad = true;
        /// <summary>
        /// 设置为空治具
        /// </summary>
        public int EmptyFixture = 0;
        /// <summary>
        /// 确定不良
        /// </summary>
        public bool DisFail = false;
        /// <summary>
        /// 复位
        /// </summary>
        public int Reset = 0;
        /// <summary>
        /// 测试总数
        /// </summary>
        public int TTNum = 0;
        /// <summary>
        /// 不良数
        /// </summary>
        public int FailNum = 0;
        /// <summary>
        /// 工位使用次数
        /// </summary>
        public int StatUseNum = 0;
        /// <summary>
        /// 工位连续不良
        /// </summary>
        public int StatFailNum = 0;
        /// <summary>
        /// 复位时钟
        /// </summary>
        public Stopwatch ResetWatcher = new Stopwatch();
    }
    /// <summary>
    /// 基本信息
    /// </summary>
    public class CStatHubBase
    {
        public CStatHubBase(int idNo, string name, int flowId, string flowName, int slotMax)
        {
            this._idNo = idNo;
            this._name = name;
            this._slotMax = slotMax;
            this.FlowId = flowId;
            this.FlowName = flowName;   
        }
        public override string ToString()
        {
            return _name;
        }
        /// <summary>
        /// 编号
        /// </summary>
        private int _idNo = 0;
        /// <summary>
        /// 名称
        /// </summary>
        private string _name = string.Empty;
        /// <summary>
        /// 槽位数量
        /// </summary>
        private int _slotMax = 16;
        /// <summary>
        /// 当前流程编号
        /// </summary>
        public int FlowId = 0;
        /// <summary>
        /// 当前流程名称
        /// </summary>
        public string FlowName = string.Empty;
        /// <summary>
        /// 治具ID地址
        /// </summary>
        public int IdCardAddr = 0;
        /// <summary>
        /// 条码枪对应条码编号
        /// </summary>
        public int[][] SnBarNo = new int[2][]; 
    }
    /// <summary>
    /// 运行参数
    /// </summary>
    public class CStatHubPara
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        public ERUN DoRun = ERUN.空闲;
        /// <summary>
        /// 报警状态
        /// </summary>
        public EAlarm Alarm = EAlarm.正常;
        /// <summary>
        /// 报警信息
        /// </summary>
        public string AlarmInfo = string.Empty;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 测试时间(ms)
        /// </summary>
        public long TestTime = 0;
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
        /// <summary>
        /// 取消任务
        /// </summary>
        public CancellationTokenSource _cts = null;
        /// <summary>
        /// IO时间监控
        /// </summary>
        public Stopwatch Io_Watcher = new Stopwatch();
        /// <summary>
        /// 当前扫描条码编号
        /// </summary>
        public int[] CurSnNo = new int[2];
        /// <summary>
        /// 当前扫描条码
        /// </summary>
        public string[] CurSeriaoNo = new string[2];
        /// <summary>
        /// 清除界面
        /// </summary>
        public bool ClrUI = false;
    }
    /// <summary>
    /// 工位治具信息
    /// </summary>
    public class CStatHubFixture
    {
        public CStatHubFixture(int slotMax)
        {
            for (int i = 0; i < slotMax; i++)
            {
                SerialNo.Add("");
                ResultName.Add("");
                ResultId.Add(0);
                Result.Add(0);
            }
        }
        /// <summary>
        /// 机种名称
        /// </summary>
        public string ModelName = string.Empty;
        /// <summary>
        /// 治具ID
        /// </summary>
        public string IdCard = string.Empty;
        /// <summary>
        /// 为空治具
        /// </summary>
        public int EmptyFixture = 0;
        /// <summary>
        /// 条码
        /// </summary>
        public List<string> SerialNo = new List<string>();
        /// <summary>
        /// 当前站别名称
        /// </summary>
        public List<string> ResultName = new List<string>();
        /// <summary>
        /// 当前站别ID
        /// </summary>
        public List<int> ResultId = new List<int>();
        /// <summary>
        /// 当前站别结果
        /// </summary>
        public List<int> Result = new List<int>();
    }
    /// <summary>
    /// 工位治具信息
    /// </summary>
    public class CStatTestFixture
    {
        public CStatTestFixture(int slotMax)
        {
            for (int i = 0; i < slotMax; i++)
            {
                SerialNo.Add("");
                ResultName.Add("");
                ResultId.Add(0);
                Result.Add(0);
                Value.Add("");
                Volt.Add(0);
                Cur.Add(0);
                DD.Add("");
            }
        }
        /// <summary>
        /// 机种名称
        /// </summary>
        public string ModelName = string.Empty;
        /// <summary>
        /// 治具ID
        /// </summary>
        public string IdCard = string.Empty;
        /// <summary>
        /// 为空治具
        /// </summary>
        public int EmptyFixture = 0;
        /// <summary>
        /// 条码
        /// </summary>
        public List<string> SerialNo = new List<string>();
        /// <summary>
        /// 当前站别名称
        /// </summary>
        public List<string> ResultName = new List<string>();
        /// <summary>
        /// 当前站别ID
        /// </summary>
        public List<int> ResultId = new List<int>();
        /// <summary>
        /// 当前站别结果
        /// </summary>
        public List<int> Result = new List<int>();
        /// <summary>
        /// 数据
        /// </summary>
        public List<string> Value = new List<string>();
        /// <summary>
        /// 电压读值
        /// </summary>
        public List<double> Volt = new List<double>();
        /// <summary>
        /// 电流读取
        /// </summary>
        public List<double> Cur = new List<double>();
        /// <summary>
        /// D+D-短路检测
        /// </summary>
        public List<string> DD = new List<string>();
    }
    /// <summary>
    /// 工位节点
    /// </summary>
    public class CStatHub
    {
        public CStatHub(int idNo, string name, int flowId, string flowName, int slotMax)
        {
            Base = new CStatHubBase(idNo, name, flowId, flowName, slotMax);

            Para = new CStatHubPara();

            Fixture = new CStatHubFixture(slotMax);
        }
        public override string ToString()
        {
            return Base.ToString();
        }
        public CStatHubBase Base = null;
        public CStatHubPara Para = null;
        public CStatHubFixture Fixture = null;
    }
    /// <summary>
    /// 测试节点
    /// </summary>
    public class CStatTest
    {
        public CStatTest(int idNo, string name, int flowId, string flowName, int slotMax)
        {
            Base = new CStatHubBase(idNo, name, flowId, flowName, slotMax);

            Para = new CStatHubPara();

            Fixture = new CStatTestFixture(slotMax);         
        }
        public override string ToString()
        {
            return Base.ToString();
        }
        public CStatHubBase Base = null;
        public CStatHubPara Para = null;
        public CStatTestFixture Fixture = null;
    }
    /// <summary>
    /// 工位测试
    /// </summary>
    public class CStat
    {
        public CStat(int idNo, string hubName, string testName,int flowId, string flowName, int slotMax)
        {
            hub = new CStatHub(idNo, hubName,flowId, flowName, slotMax);

            test = new CStatTest(idNo, testName, flowId,flowName, slotMax);
        }
        public override string ToString()
        {
            return hub.ToString();
        }
        /// <summary>
        /// 状态
        /// </summary>
        public CStatHub hub = null;
        /// <summary>
        /// 测试
        /// </summary>
        public CStatTest test = null;
    }
    #endregion

    #region 日产能
    /// <summary>
    /// 当日产能
    /// </summary>
    public class CDailyYield
    {
        public string DayNow = string.Empty;
        public int TTNum = 0;
        public int FailNum = 0;
        public double PassRate = 1;
    }
    #endregion

    #region 良率预警
    /// <summary>
    /// 预警状态
    /// </summary>
    public enum EWarnResult
    {
        空闲 = 0,
        正常 = 1,
        报警 = 2,
        确认报警 = 3,
        未启动 = 4
    }
    /// <summary>
    /// 良品预警类
    /// </summary>
    public class CWarnRate
    {
        /// <summary>
        /// 预警值
        /// </summary>
        public double PassRateLimit = 0;
        /// <summary>
        /// 总数
        /// </summary>
        public int TTNum = 0;
        /// <summary>
        /// 良品数
        /// </summary>
        public int PassNum = 0;
        /// <summary>
        /// 良品率
        /// </summary>
        public double PassRate = 0;
        /// <summary>
        /// 当前日期
        /// </summary>
        public string CurrentDate = string.Empty;
        /// <summary>
        /// 当前白班清除标志
        /// </summary>
        public int CurrentDayClr = 0;
        /// <summary>
        /// 当前夜班清除标志
        /// </summary>
        public int CurrentNightClr = 0;
        /// <summary>
        /// 当前报警状态
        /// </summary>
        public EWarnResult DoRun = EWarnResult.空闲;
        /// <summary>
        /// 监控时间
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
        /// <summary>
        /// 提示报警
        /// </summary>
        public int bAlarm = 0;
    }
    #endregion

    #region UI消息类定义
    /// <summary>
    /// UI刷新状态
    /// </summary>
    public enum EUIStatus
    {
        状态空闲,
        治具到位,
        空治具过站,
        状态正常,
        读卡报警,
        异常报警,
        扫描状态,
        治具过站,
        文本聚焦,
        设置条码,
        测试状态,
        确定不良,
        取消确定,
        设定电压,
        设置产能,
        使用次数,
        连续不良
    }
    /// <summary>
    /// 扫描条码
    /// </summary>
    public enum EUIScanSn
    {
        空闲,
        文本聚焦,
        设置条码,
        绑定完毕
    }
    /// <summary>
    /// 系统UI
    /// </summary>
    public enum EUISystem
    {
        空闲,
        启动    
    }
    /// <summary>
    /// 工位数据UI
    /// </summary>
    public enum EUIStatData
    {
        空闲,
        使用次数,
        产能统计,
        测试信息,
        设定电压,
        监控时间,
        确定不良,
        取消确定,
        状态提示
    }
    /// <summary>
    /// 扫描条码
    /// </summary>
    public class CUIScanSnArgs
    {
        public EUIScanSn DoRun = EUIScanSn.空闲;

        public bool bAlarm = false;

        public string AlarmInfo = string.Empty;

        public int SlotNo = 0;

        public string SerialNo = string.Empty;
    }
    /// <summary>
    /// 系统UI
    /// </summary>
    public class CUISystemArgs
    {
        public EUISystem DoRun = EUISystem.空闲;
    }
    /// <summary>
    /// 工位数据
    /// </summary>
    public class CUIStatDataArgs
    {
        public EUIStatData DoRun = EUIStatData.空闲;

        public int UseNum = 0;

        public int TTNum = 0;

        public int FailNum = 0;

        public int ConFailNum = 0;

        public bool bAlarm = false;

        public string AlarmInfo = string.Empty;

        public double ACV = 0;

        public int ACFlag = 0;

        public long MonTime = 0;

        public List<string> SerialNo =null;

        public List<double> V = null;

        public List<double> I = null;

        public List<string> DD = null;

        public long TestTime = 0;

        public bool TestEnd = false;
    }
    public class CUIActionAgrs
    {
        public CUIActionAgrs(int alarmFlag, string alarmInfo)
        {
            this.AlarmFlag = alarmFlag;
            this.AlarmInfo = alarmInfo;
        }

        public int AlarmFlag = 0;

        public string AlarmInfo = string.Empty;
    }
    #endregion

    #region 测试数据JSON
    [DataContract]
    public class CTestVal
    {
        /// <summary>
        /// 输出名称
        /// </summary>
        [DataMember]
        public string Vname
        { get; set; }
        /// <summary>
        /// 电压值
        /// </summary>
        [DataMember]
        public double Volt
        { get; set; }
        /// <summary>
        /// 电流值
        /// </summary>
        [DataMember]
        public double Current
        { get; set; }
        /// <summary>
        /// D+D-短路检测
        /// </summary>
        [DataMember]
        public string DDStatus
        { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        [DataMember]
        public int Result
        { get; set; }
    }
    [DataContract]
    public class CTestData
    {
        [DataMember]
        public List<CTestVal> UUT
        { get; set; }
    }
    #endregion

}
