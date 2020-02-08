using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using GJ.DEV.PLC;
using GJ.USER.APP;
namespace GJ.YOHOO.UNLOAD
{
    #region UI消息定义
    /// <summary>
    /// 系统UI
    /// </summary>
    public enum EUISystem
    {
        空闲,
        启动
    }
    /// <summary>
    /// UI刷新状态
    /// </summary>
    public enum EUIStatus
    {
        空闲,
        治具到位,
        空治具过站,
        状态信息,
        读卡报警,
        异常报警,
        测试中,
        测试结束,
        不良确认,
        产能计数,
        工位计数,
        显示计数,
        变更机种
    }
    /// <summary>
    /// 系统UI
    /// </summary>
    public class CUISystemArgs
    {
        public EUISystem DoRun = EUISystem.空闲;
    }
    /// <summary>
    /// 主面板消息
    /// </summary>
    public class CUIMainArgs
    {
        public EUIStatus DoRun = EUIStatus.空闲;

        public string Info = string.Empty;

        public CStatTest[] StatHP = new CStatTest[2];

        public CStatTest[] StatATE = new CStatTest[2];

        public CStatHub StatUnLoad = null;

        public int TTNum = 0;

        public int FailNum = 0;

        public string YieldKey = string.Empty;

        public CYield Yield = null;

        public List<CYield> Yields = null;

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

    #region 参数枚举
    /// <summary>
    /// 测试状态
    /// </summary>
    public enum ERUN
    {
        空闲,
        就绪,
        启动,
        等待,
        测试,
        结束,
        过站,
        出站,
        报警
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
    /// PLC结果
    /// </summary>
    public enum EPLCRESULT
    { 
       空闲,
       结果OK,
       结果NG,
       过站
    }
    /// <summary>
    /// PLC下机模式
    /// </summary>
    public enum EPLCMode
    { 
      空闲,
      人工下机,
      自动下机
    }
    #endregion

    #region 类参数
    /// <summary>
    /// 工位节点
    /// </summary>
    public class CStatHub
    {
        #region 构造函数
        public CStatHub(int idNo, string name, int flowId, string flowName, int slotMax)
        {
            this._idNo = idNo;
            this._name = name;
            this._slotMax = slotMax;
            this.FlowId = flowId;
            this.FlowName = flowName;
            for (int i = 0; i < slotMax; i++)
            {
                SnEnable.Add(true);
                SerialNo.Add("");
                ResultName.Add("");
                ResultId.Add(0);
                Result.Add(0);
                TranOK.Add(false); 
            }
        }
        public override string ToString()
        {
            return this._name;
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CStatHub Clone()
        {
            CStatHub hub = new CStatHub(this._idNo, this._name, this.FlowId, this.FlowName, this._slotMax);

            hub._idNo = this._idNo;

            hub._name = this._name;

            hub._slotMax = this._slotMax;

            hub.FlowId = this.FlowId;

            hub.FlowName = this.FlowName;

            hub.IdCardAddr = this.IdCardAddr;

            hub.DoRun = this.DoRun;

            hub.Alarm = this.Alarm;

            hub.IdCard = this.IdCard;

            hub.ModelName = this.ModelName;

            hub.MesFlag = this.MesFlag;

            hub.IsNull = this.IsNull;

            hub.StartTime = this.StartTime;

            hub.EndTime = this.EndTime;

            hub.TTNum = this.TTNum;

            hub.FailNum = this.FailNum;

            for (int i = 0; i < _slotMax; i++)
            {
                hub.SnEnable[i] = this.SnEnable[i];
                hub.SerialNo[i] = this.SerialNo[i];
                hub.ResultName[i] = this.ResultName[i];
                hub.ResultId[i] = this.ResultId[i];
                hub.Result[i] = this.Result[i];
                hub.TranOK[i] = this.TranOK[i]; 
            }

            return hub;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 编号
        /// </summary>
        public int idNo
        {
            get { return _idNo; }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string name
        {
            get { return _name; }
        }
        #endregion

        #region 字段
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
        /// 运行状态
        /// </summary>
        public ERUN DoRun = ERUN.空闲;
        /// <summary>
        /// 报警状态
        /// </summary>
        public EAlarm Alarm = EAlarm.正常;
        /// <summary>
        /// 报警提示
        /// </summary>
        public bool bAlarm = false;
        /// <summary>
        /// 信息显示
        /// </summary>
        public string Info = string.Empty;
        /// <summary>
        /// UI信息状态
        /// </summary>
        public EUIStatus UIDoRun = EUIStatus.空闲;
        /// <summary>
        /// 治具ID
        /// </summary>
        public string IdCard = string.Empty;
        /// <summary>
        /// 机种名称
        /// </summary>
        public string ModelName = string.Empty;
        /// <summary>
        /// 工单号
        /// </summary>
        public string OrderName = string.Empty;
        /// <summary>
        /// MES连线
        /// </summary>
        public int MesFlag = 0;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 空治具
        /// </summary>
        public int IsNull = 0;
        /// <summary>
        /// 禁用工位
        /// </summary>
        public bool Disable = false;
        /// <summary>
        /// 强制进站
        /// </summary>
        public bool ForceIn = false;
        /// <summary>
        /// 总数
        /// </summary>
        public int TTNum = 0;
        /// <summary>
        /// 不良数
        /// </summary>
        public int FailNum = 0;
        /// <summary>
        /// 条码位置启用
        /// </summary>
        public List<bool> SnEnable = new List<bool>();
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
        /// 当前上传MES结果OK
        /// </summary>
        public List<bool> TranOK = new List<bool>(); 
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
        /// <summary>
        /// 取消任务
        /// </summary>
        public CancellationTokenSource _cts = null;
        #endregion
       
    }
    /// <summary>
    /// 测试节点
    /// </summary>
    public class CStatTest
    {
        #region 构造函数
        public CStatTest(int idNo, string name, int flowId, string flowName, int slotMax)
        {
            this._idNo = idNo;

            this._name = name;

            this._slotMax = slotMax;

            this.FlowId = flowId;

            this.FlowName = flowName;

            for (int i = 0; i < slotMax; i++)
            {
                SerialNo.Add("");
                ResultName.Add("");
                ResultId.Add(0);
                Result.Add(0);                
                Value.Add("");
                TranOK.Add(false); 
            }
        }
        public override string ToString()
        {
            return this._name;
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CStatTest Clone()
        {
            CStatTest hub = new CStatTest(this._idNo, this._name, this.FlowId, this.FlowName, this._slotMax);

            hub._idNo = this._idNo;

            hub._name = this._name;

            hub._slotMax = this._slotMax;

            hub.FlowId = this.FlowId;

            hub.FlowName = this.FlowName;

            hub.IdCard = this.IdCard;

            hub.ModelName = this.ModelName;

            hub.MesFlag = this.MesFlag;

            hub.IsNull = this.IsNull;

            hub.StartTime = this.StartTime;

            hub.EndTime = this.EndTime;

            hub.TTNum = this.TTNum;

            hub.FailNum = this.FailNum;

            hub.ChkFail = this.ChkFail;

            for (int i = 0; i < _slotMax; i++)
            {
                hub.SerialNo[i] = this.SerialNo[i];
                hub.ResultName[i] = this.ResultName[i];
                hub.ResultId[i] = this.ResultId[i];
                hub.Result[i] = this.Result[i];
                hub.Value[i] = this.Value[i];
                hub.TranOK[i] = this.TranOK[i];
            }

            return hub;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 编号
        /// </summary>
        public int idNo
        {
            get { return _idNo; }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string name
        {
            get { return _name; }
        }
        #endregion

        #region 字段
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
        /// 测试状态
        /// </summary>
        public ERUN DoRun = ERUN.空闲;
        /// <summary>
        /// 报警状态
        /// </summary>
        public EAlarm Alarm = EAlarm.正常;
        /// <summary>
        /// 报警提示
        /// </summary>
        public bool bAlarm = false;
        /// <summary>
        /// 信息显示
        /// </summary>
        public string Info = string.Empty;
        /// <summary>
        /// UI信息状态
        /// </summary>
        public EUIStatus UIDoRun = EUIStatus.空闲;
        /// <summary>
        /// 测试工位
        /// </summary>
        public int SideIndex = -1;
        /// <summary>
        /// 治具ID
        /// </summary>
        public string IdCard = string.Empty;
        /// <summary>
        /// 空治具
        /// </summary>
        public int IsNull = 0;
        /// <summary>
        /// 机种名称
        /// </summary>
        public string ModelName = string.Empty;
        /// <summary>
        /// 工单信息
        /// </summary>
        public string OrderName = string.Empty;
        /// <summary>
        /// MES连线
        /// </summary>
        public int MesFlag = 0;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 不良确认
        /// </summary>
        public bool ChkFail = false;
        /// <summary>
        /// 确定不良
        /// </summary>
        public bool DisFail = false;
        /// <summary>
        /// 禁用工位
        /// </summary>
        public bool Disable = false;
        /// <summary>
        /// 总数
        /// </summary>
        public int TTNum = 0;
        /// <summary>
        /// 不良数
        /// </summary>
        public int FailNum = 0;
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
        /// 当前上传MES结果OK
        /// </summary>
        public List<bool> TranOK = new List<bool>(); 
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
        /// <summary>
        /// IO时间监控
        /// </summary>
        public Stopwatch Io_Watcher = new Stopwatch();
        /// <summary>
        /// 取消任务
        /// </summary>
        public CancellationTokenSource _cts = null;
        #endregion
       
    }
    /// <summary>
    /// 工位测试
    /// </summary>
    public class CStat
    {
        public CStat(int idNo, string hubName, string testName, int flowId, string flowName, int slotMax)
        {
            hub = new CStatHub(idNo, hubName,flowId, flowName, slotMax);

            test = new CStatTest(idNo, testName,flowId, flowName, slotMax);

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
    /// <summary>
    /// 产能类
    /// </summary>
    public class CYield
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int TTNum = 0;
        /// <summary>
        /// 不良数
        /// </summary>
        public int FailNum = 0;
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

    #region 日产能
    /// <summary>
    /// 当日产能
    /// </summary>
    public class CDailyYield
    {
        public CDailyYield()
        {
            for (int i = 0; i < 4; i++)
            {
                TTNum.Add(0);
                FailNum.Add(0);
                PassRate.Add(1); 
            }
        }
        public string DayNow = string.Empty;
        public string[] Name = new string[] { "通电测试", "老化测试", "高压测试", "ATE测试" };       
        public List<int> TTNum = new List<int>();
        public List<int> FailNum = new List<int>();
        public List<double> PassRate = new List<double>();
    }
    #endregion

}
