using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GJ;
using System.Windows.Forms;
using System.Diagnostics;
namespace GJ.YOHOO.ATE
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
        TCP状态,
        ATE状态,
        ATE信息,
        治具到位,
        测试状态,
        测试结束,
        产能计数,
        调试模式
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

        public bool bAlarm = false;

        public string AlarmInfo = string.Empty;

        public string IdCard = string.Empty;

        public List<string> SerialNo = null;

        public List<int> Result = null;

        public long WaitTime = 0;

        public int TTNum = 0;

        public int FailNum = 0;

        public List<int> SlotTTNum = null;

        public List<int> SlotFailNum = null;

        public bool DebugMode = false;

        public string ProName = string.Empty;

        public string ModeName = string.Empty;

        public string ElapsedTime = string.Empty;
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
        等待,
        测试,
        结束,
        过站,
        报警,
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
    /// 测试结果
    /// </summary>
    public enum EResult
    {
        报警 = -2,
        空闲 = -1,
        良品 = 0,
        不良 = 1
    }
    #endregion

    #region 类定义
    /// <summary>
    /// 工位状态
    /// </summary>
    public class CStat
    {
        public CStat(int idNo, string name, int flowId, string flowName, int slotMax)
        {
            this._idNo = idNo;
            this._name = name;
            this._slotMax = slotMax;

            this.FlowId = flowId;

            this.FlowName = flowName;

            for (int i = 0; i < slotMax; i++)
            {
                SerialNo.Add("");
                Result.Add(0);
                TestData.Add("");
            }
        }
        public override string ToString()
        {
            return _name;
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CStat Clone()
        {
            CStat hub = new CStat(this._idNo, this._name, this.FlowId, this.FlowName, this._slotMax);

            hub._idNo = this._idNo;

            hub._name = this._name;

            hub._slotMax = this._slotMax;

            hub.FlowId = this.FlowId;

            hub.FlowName = this.FlowName;

            hub.DoRun = this.DoRun;

            hub.Alarm = this.Alarm;

            hub.IdCard = this.IdCard;

            hub.ModelName = this.ModelName;

            hub.MesFlag = this.MesFlag;

            hub.Ready = this.Ready;

            hub.StartTime = this.StartTime;

            hub.EndTime = this.EndTime;

            for (int i = 0; i < _slotMax; i++)
            {
                hub.SerialNo[i] = this.SerialNo[i];
                hub.Result[i] = this.Result[i];
                hub.TestData[i] = this.TestData[i];
            }


            return hub;
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
        /// 运行状态
        /// </summary>
        public ERUN DoRun = ERUN.空闲;
        /// <summary>
        /// 报警状态
        /// </summary>
        public EAlarm Alarm = EAlarm.正常;
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
        /// MES连线模式
        /// </summary>
        public int MesFlag = 0;
        /// <summary>
        /// 治具就绪
        /// </summary>
        public int Ready = 0;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 条码
        /// </summary>
        public List<string> SerialNo = new List<string>();
        /// <summary>
        /// 当前站别结果
        /// </summary>
        public List<int> Result = new List<int>();
        /// <summary>
        /// 测试数据
        /// </summary>
        public List<string> TestData = new List<string>();
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
    }
    /// <summary>
    /// 测试设备
    /// </summary>
    public class CDev
    {
        public CDev(int idNo, string name = "", int chanNum = 8)
        {
            this._idNo = idNo;

            this._name = name;

            this.ChanMax = chanNum;

            for (int i = 0; i < chanNum; i++)
            {
                SerialNo.Add("");
                Result.Add(0);
                TestData.Add("");
            }
        }
        public override string ToString()
        {
            return _name;
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        private int _idNo = 0;
        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name = "";
        /// <summary>
        /// 测试通道数
        /// </summary>
        public int ChanMax = 8;
        /// <summary>
        /// 运行状态
        /// </summary>
        public ERUN DoRun = ERUN.空闲;
        /// <summary>
        /// 报警状态
        /// </summary>
        public EAlarm Alarm = EAlarm.正常;
        /// <summary>
        /// 测试工位编号
        /// </summary>
        public int SubNo = 0;
        /// <summary>
        /// 触发测试
        /// </summary>
        public bool RunTriger = false;
        /// <summary>
        /// 触发报警
        /// </summary>
        public bool RunTrigerAlarm = false;
        /// <summary>
        /// 机种名称
        /// </summary>
        public string ModelName = string.Empty;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 产品条码
        /// </summary>
        public List<string> SerialNo = new List<string>();
        /// <summary>
        /// 测试结果
        /// </summary>
        public List<int> Result = new List<int>();
        /// <summary>
        /// 不良信息
        /// </summary>
        public List<string> TestData = new List<string>();
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
        /// <summary>
        /// IO时间监控
        /// </summary>
        public Stopwatch IoWatcher = new Stopwatch();
        /// <summary>
        /// 测试步骤
        /// </summary>
        public int StepNo = 0;
        /// <summary>
        /// 调试模式
        /// </summary>
        public bool DebugMode = false;
        /// <summary>
        /// 运行中
        /// </summary>
        public bool Running = false;
    }
    /// <summary>
    /// 测试主机应答状态
    /// </summary>
    public class CSerReponse
    {
        public CSerReponse(int slotMax)
        {
            for (int i = 0; i < slotMax; i++)
                SerialNos.Add("");
        }
        /// <summary>
        /// 到位信号
        /// </summary>
        public int Ready = 0;
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
        /// 产品条码
        /// </summary>
        public List<string> SerialNos = new List<string>();
    }
    /// <summary>
    /// 测试产能
    /// </summary>
    public class CYield
    {
        public CYield(int uutMax)
        {
            UUTMax = uutMax;

            for (int i = 0; i < uutMax; i++)
            {
                SlotTTNum.Add(0);
                SlotFailNum.Add(0);
            }
        }
        public int UUTMax = 16;
        /// <summary>
        /// 总数
        /// </summary>
        public int TTNum = 0;
        /// <summary>
        /// 不良数
        /// </summary>
        public int FailNum = 0;
        /// <summary>
        /// 槽位总数
        /// </summary>
        public List<int> SlotTTNum = new List<int>();
        /// <summary>
        /// 槽位不良数
        /// </summary>
        public List<int> SlotFailNum = new List<int>();
    }
    #endregion

}
