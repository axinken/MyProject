using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization;
using GJ.DEV.HIPOT;
namespace GJ.YOHOO.HIPOT
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
        TCP界面,
        设置状态,
        治具就绪,
        设置条码,
        状态就绪,
        设置时间,
        设置产能,
        设置结果,
        设置参数,
        设置按钮
    }
    /// <summary>
    /// 系统UI
    /// </summary>
    public class CUISystemArgs
    {
        public EUISystem DoRun = EUISystem.空闲;
    }
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
    /// <summary>
    /// 主面板消息
    /// </summary>
    public class CUIMainArgs 
    {
        public EUIStatus DoRun = EUIStatus.空闲;

        public int TTNum = 0;

        public int FailNum = 0;

        public int RunStatus = 0;

        public long WatcherTime = 0;

        public int BtnNo = 0;

        public int BtnValue = 0;

        public string IdCard = string.Empty;

        public int SlotNo = 0;

        public string CurSn = string.Empty;

        public int Result = 0;

        public CHPPara.CStepVal HPResult = null;

        public List<CHPPara.CStep> Step = null;

        public CSerReponse TcpReponse = null;

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

    #region UI参数枚举
    /// <summary>
    /// UI控件
    /// </summary>
    public enum EUICtrl
    {
        系统状态,
        TCP界面,
        机种信息,
        测试界面,
        数据界面
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
                HpResult.Add(new CHPPara.CStepVal());
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
                hub.HpResult[i] = this.HpResult[i];
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
        /// 高压机测试数据
        /// </summary>
        public List<CHPPara.CStepVal> HpResult = new List<CHPPara.CStepVal>();
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
        public CDev(int idNo, string name = "", int chanNum = 16)
        {
            this._idNo = idNo;
            
            this._name = name;
            
            this.ChanMax = chanNum;

            for (int i = 0; i < chanNum; i++)
            {
                SerialNo.Add("");
                Result.Add(0);
                FailInfo.Add("");
                HpResult.Add(new CHPPara.CStepVal());
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
        /// 高压仪器对应测试产品数量:1->16;2->8
        /// </summary>
        public int ChanMax = 16;
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
        public List<string> FailInfo = new List<string>();
        /// <summary>
        /// 高压测试数据
        /// </summary>
        public List<CHPPara.CStepVal> HpResult = new List<CHPPara.CStepVal>();
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
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
        /// <summary>
        /// 不良测试步骤
        /// </summary>
        public int FailStepNo = -1;
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

    #region JSON测试结果
    /// <summary>
    /// 测试项目数据
    /// </summary>
    [DataContract]
    public class CTestVal
        {
            [DataMember]
            public int StepNo
            { set; get; }
            [DataMember]
            public string StepName
            { set; get; }
            [DataMember]
            public double Value
            { set; get; }
            [DataMember]
            public string Unit
            { set; get; }
            [DataMember]
            public int Result
            { set; get; }
        }
    /// <summary>
        /// 测试数据
        /// </summary>
    [DataContract]
    public class CTestData
    {
        [DataMember]
        public List<CTestVal> Item
        { get; set; }
    }
    #endregion

}
