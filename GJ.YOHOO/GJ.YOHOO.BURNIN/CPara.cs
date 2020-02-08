using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using GJ.USER.APP;
using GJ.DEV.PLC;
using System.Runtime.Serialization;
namespace GJ.YOHOO.BURNIN
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
        状态空闲,
        刷新机种,
        刷新数量,
        读取温度,
        读取电压
    }
    /// <summary>
    /// 系统UI
    /// </summary>
    public class CUISystemArgs
    {
        public EUISystem DoRun = EUISystem.空闲;
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
    /// <summary>
    /// 信号UI消息
    /// </summary>
    public class CUISignalArgs : EventArgs
    {
        public readonly string name;

        public readonly string Info;

        public readonly Color lPara;

        public readonly int wPara;

        public CUISignalArgs(string name, string Info, Color lPara, int wPara = 0)
        {
            this.name = name;
            this.Info = Info;
            this.lPara = lPara;
            this.wPara = wPara;
        }
    }
    /// <summary>
    /// 温度消息
    /// </summary>
    public class CUITempArgs : EventArgs
    {
        public readonly double THP = 0;

        public readonly double TLP = 0;

        public readonly double[] TPoint;

        public CUITempArgs(double TLP, double THP, double[] TPoint)
        {
            this.THP = THP;
            this.TLP = TLP;
            this.TPoint = TPoint;
        }
    }
    /// <summary>
    /// 主面板消息
    /// </summary>
    public class CUIMainArgs
    {
        public EUIStatus DoRun = EUIStatus.状态空闲;

        public double rTemp = 0;

        public int ACV = 0;

        public Color rColor = Color.Black;

        public List<string> CurModelList = null;

        public string CurOutModel = string.Empty;

        public int CurModelNum = 0;

        public int CurModelOutNum = 0;

        public bool CurModelChange = false;
    }
    /// <summary>
    /// 提示信息
    /// </summary>
    public class CUIActionArgs:EventArgs
    {
        public CUIActionArgs(string info, Color color)
        {
            this.info = info;
            this.color = color;
        }

        public string info = string.Empty;

        public Color color = Color.Black;
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

    #region 测试枚举
    /// <summary>
    /// 治具状态
    /// </summary>
    public enum EDoRun
    {
        异常报警 = -2,
        位置禁用 = -1,
        位置空闲 = 0,
        正在进机=2,
        进机完毕=3,
        启动老化=4,
        老化自检=5,
        正在老化=6,
        老化完成=7,
        老化结束=8,
        正在出机=9,
        空治具到位=10
    }
    /// <summary>
    /// 治具错误代码
    /// </summary>
    public enum EAlarmCode
    {
        正常,
        治具到位信号异常,
        AC同步信号异常,
        治具AC不通,
        控制板通信异常,
        ERS通信异常,       
        无治具有到位信号,
        有治具无到位信号,
        进机结束检测不到到位,
        出机结束到位信号存在,
        母治具使用寿命已到,
        母治具不良次数超过设置值,
        控制ACON异常,
        设置快充模式异常,
        位置禁用,
        进机异常,
        出机异常,
        负载拉载异常,
        继电器粘连警告,
        人工手动模式,
        S1状态ON
    }
    /// <summary>
    /// 快充切换过程
    /// </summary>
    public enum EQCMChage
    { 
      空闲,
      控制ACON,
      自检电压,
      设置快充,
      设置负载
    }
    /// <summary>
    /// 控制输入AC模式
    /// </summary>
    public enum ECtrlACMode
    {
        PLC控制时序,
        上位机控制时序
    }
    /// <summary>
    /// 库体老化计时模式
    /// </summary>
    public enum ECtrlTimeMode
    {
        上位机计时模式1,
        上位机计时模式2,
        PLC计时模式
    }
    #endregion

    #region 类定义
    /// <summary>
    /// 规格参数
    /// </summary>
    public class CTIME
    {
        /// <summary>
        /// 老化时间(S)
        /// </summary>
        public int BITime = 0;
        /// <summary>
        /// 输出组数
        /// </summary>
        public int OutPutNum = 0;
        /// <summary>
        /// 输出通道数
        /// </summary>
        public int OutChanNum = 0;
        /// <summary>
        /// ONOFF组数
        /// </summary>
        public int OnOffNum = 0;
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CTIME Clone()
        {
            CTIME timeSpec = new CTIME();

            timeSpec.BITime = this.BITime;

            timeSpec.OutPutNum = this.OutPutNum;

            timeSpec.OutChanNum = this.OutChanNum;

            timeSpec.OnOffNum = this.OnOffNum;

            return timeSpec;
        }
    }
    /// <summary>
    /// 通道规格
    /// </summary>
    public class CCHAN
    {
        /// <summary>
        /// 通道名称
        /// </summary>
        public string Vname;
        /// <summary>
        /// 输出下限
        /// </summary>
        public double Vmin;
        /// <summary>
        /// 输出上限
        /// </summary>
        public double Vmax;
        /// <summary>
        /// 负载模式
        /// </summary>
        public int Imode;
        /// <summary>
        /// 负载电流
        /// </summary>
        public double ISet;
        /// <summary>
        /// 电流下限
        /// </summary>
        public double Imin;
        /// <summary>
        /// 电流上限
        /// </summary>
        public double Imax;
        /// <summary>
        /// 快充模式
        /// </summary>
        public int QCType = 0;
        /// <summary>
        /// 快充电压
        /// </summary>
        public double QCV;
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CCHAN Clone()
        {
            CCHAN _chan = new CCHAN();
            _chan.Vname = this.Vname;
            _chan.Imin = this.Imin;
            _chan.Imax = this.Imax;
            _chan.Imode = this.Imode;
            _chan.ISet = this.ISet;
            _chan.Imin = this.Imin;
            _chan.Imax = this.Imax;
            _chan.QCType = this.QCType;
            _chan.QCV = this.QCV;
            return _chan;
        }
    }
    /// <summary>
    /// 输出规格
    /// </summary>
    public class COUTPUT
    {
        /// <summary>
        /// 输出通道规格
        /// </summary>
        public List<CCHAN> Chan = new List<CCHAN>();
        /// <summary>
        /// 复制
        /// </summary>
        public COUTPUT Clone()
        {
            COUTPUT _outPut = new COUTPUT();
            for (int i = 0; i < Chan.Count; i++)
            {
                CCHAN _chan = new CCHAN();
                _chan.Vname = this.Chan[i].Vname;
                _chan.Vmin = this.Chan[i].Vmin;
                _chan.Vmax = this.Chan[i].Vmax;
                _chan.Imode = this.Chan[i].Imode;
                _chan.ISet = this.Chan[i].ISet;
                _chan.Imin = this.Chan[i].Imin;
                _chan.Imax = this.Chan[i].Imax;
                _chan.QCType = this.Chan[i].QCType;
                _chan.QCV = this.Chan[i].QCV;
                _outPut.Chan.Add(_chan);
            }
            return _outPut;
        }
    }
    /// <summary>
    /// ON/OFF段规格
    /// </summary>
    public class CONOFF
    {
        /// <summary>
        /// ONOFF时间(S)
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
        public CONOFF Clone()
        {
            CONOFF item = new CONOFF();
            item.OnOffTime = this.OnOffTime;
            item.OnTime = this.OnTime;
            item.OffTime = this.OffTime;
            item.ACV = this.ACV;
            item.OutPutType = this.OutPutType;
            return item;
        }
    }
    /// <summary>
    /// 当前运行时间状态
    /// </summary>
    public class CRUNTIME
    {
        /// <summary>
        /// 当前时序段
        /// </summary>
        public int CurStepNo = 0;
        /// <summary>
        /// 当前运行时间(S)
        /// </summary>
        public int CurRunTime = 0;
        /// <summary>
        /// 当前输出电压(V)
        /// </summary>
        public int CurRunVolt = 0;
        /// <summary>
        /// 当前输出规则编号
        /// </summary>
        public int CurRunOutPut = 0;
        /// <summary>
        /// 当前快充模式
        /// </summary>
        public int CurQCType = 0;
        /// <summary>
        /// 当前快充电压
        /// </summary>
        public double CurQCV = 0;
        /// <summary>
        /// 快充切换
        /// </summary>
        public EQCMChage CurQCM = EQCMChage.空闲;
        /// <summary>
        /// 快充超时监控
        /// </summary>
        public Stopwatch WatchQCM = new Stopwatch();
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CRUNTIME Clone()
        {
            CRUNTIME runTime = new CRUNTIME();
            runTime.CurStepNo = this.CurStepNo;
            runTime.CurRunTime = this.CurRunTime;
            runTime.CurRunVolt = this.CurRunVolt;
            runTime.CurRunOutPut = this.CurRunOutPut;
            runTime.CurQCType = this.CurQCType;
            runTime.CurQCV = this.CurQCV;
            runTime.CurQCM = this.CurQCM;
            return runTime;
        }
    }
    #endregion

    #region 治具参数
    /// <summary>
    /// 子治具基本信息
    /// </summary>
    public class CUUT_Base
    {
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CUUT_Base Clone()
        {
            CUUT_Base Base = new CUUT_Base();
            Base.uutNo = this.uutNo;
            Base.roomNo = this.roomNo;
            Base.fixNo = this.fixNo;
            Base.subNo = this.subNo;
            Base.iRow = this.iRow;
            Base.iCol = this.iCol;
            Base.localName = this.localName;
            Base.ctrlCom = this.ctrlCom;
            Base.ctrlAddr = this.ctrlAddr;
            Base.ersCom = this.ersCom;
            Base.ersAddr = this.ersAddr;
            Base.ersCH = this.ersCH;
            Base.handRow = this.handRow;
            Base.handCol = this.handCol;
            Base.handPosName = this.handPosName;
            return Base;
        }
        /// <summary>
        /// 子治具编号
        /// </summary>
        public int uutNo;
        /// <summary>
        /// 台车编号
        /// </summary>
        public int roomNo;
        /// <summary>
        /// 母治具编号
        /// </summary>
        public int fixNo;
        /// <summary>
        /// 母治具
        /// </summary>
        public int subNo;
        /// <summary>
        /// 行数
        /// </summary>
        public int iRow;
        /// <summary>
        /// 列数
        /// </summary>
        public int iCol;
        /// <summary>
        /// 位置名称
        /// </summary>
        public string localName;
        /// <summary>
        /// 控制板串口
        /// </summary>
        public int ctrlCom;
        /// <summary>
        /// 控制板地址
        /// </summary>
        public int ctrlAddr;
        /// <summary>
        /// ERS串口
        /// </summary>
        public int ersCom;
        /// <summary>
        /// ERS地址
        /// </summary>
        public int ersAddr;
        /// <summary>
        /// ERS通道
        /// </summary>
        public int ersCH;
        /// <summary>
        /// 机械手行数
        /// </summary>
        public int handRow;
        /// <summary>
        /// 机械手列数
        /// </summary>
        public int handCol;
        /// <summary>
        /// 机械手位置
        /// </summary>
        public string handPosName;
    }
    /// <summary>
    /// 子治具输出及时序
    /// </summary>
    public class CUUT_ONOFF
    {
        /// <summary>
        /// 复制
        /// </summary>
        public CUUT_ONOFF Clone()
        {
            CUUT_ONOFF uut = new CUUT_ONOFF();

            uut.TimeSpec = TimeSpec.Clone();

            uut.TimeRun = TimeRun.Clone();

            for (int i = 0; i < OutPut.Count; i++)
                uut.OutPut.Add(this.OutPut[i].Clone());

            for (int i = 0; i < OnOff.Count; i++)
                uut.OnOff.Add(this.OnOff[i].Clone());

            return uut;
        }
        /// <summary>
        /// 复制
        /// </summary>
        public CUUT_ONOFF Clone1()
        {
            CUUT_ONOFF uut = new CUUT_ONOFF();

            uut.TimeSpec = TimeSpec.Clone();

            uut.TimeRun = TimeRun.Clone();

            for (int i = 0; i < OutPut.Count; i++)
                uut.OutPut.Add(this.OutPut[i].Clone());

            for (int i = 0; i < OnOff.Count; i++)
                uut.OnOff.Add(this.OnOff[i].Clone());

            return uut;
        }
        /// <summary>
        /// 添加项目
        /// </summary>
        /// <param name="OutPutChan"></param>
        /// <param name="OutPutNum"></param>
        /// <param name="OnOffNum"></param>
        public void AddItem(int OutPutNum, int OutPutChan, int OnOffNum)
        {
            OutPut.Clear();

            OnOff.Clear();

            for (int i = 0; i < OutPutNum; i++)
            {
                OutPut.Add(new COUTPUT());

                for (int z = 0; z < OutPutChan; z++)
                    OutPut[i].Chan.Add(new CCHAN());
            }

            for (int i = 0; i < OnOffNum; i++)
			   OnOff.Add(new CONOFF()); 

        }
        /// <summary>
        /// 时间规格
        /// </summary>
        public CTIME TimeSpec = new CTIME();
        /// <summary>
        /// 输出规格
        /// </summary>
        public List<COUTPUT> OutPut = new List<COUTPUT>();
        /// <summary>
        /// ON/OFF参数
        /// </summary>
        public List<CONOFF> OnOff = new List<CONOFF>();
        /// <summary>
        /// 运行时间状态
        /// </summary>
        public CRUNTIME TimeRun = new CRUNTIME();

    }
    /// <summary>
    /// 子治具测试参数
    /// </summary>
    public class CUUT_Para
    {
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CUUT_Para Clone()
        {
            CUUT_Para para = new CUUT_Para();
            para.DoRun = this.DoRun;
            para.IdCard = this.IdCard;
            para.IsNull = this.IsNull;
            para.MesFlag = this.MesFlag;            
            para.ModelName = this.ModelName;
            para.BurnTime = this.BurnTime;
            para.OutPutChan = this.OutPutChan; 
            para.OutPutNum = this.OutPutNum;
            para.OnOffNum = this.OnOffNum; 
            para.StartTime = this.StartTime;
            para.EndTime = this.EndTime;         
            para.SavePathName = this.SavePathName;
            para.SaveFileName = this.SaveFileName;
            para.SaveDataTime = this.SaveDataTime;
            para.UsedNum = this.UsedNum;
            para.FailNum = this.FailNum;
            para.CtrlUUTONLine = this.CtrlUUTONLine;
            para.CtrlACON = this.CtrlACON;
            para.CtrlOnOff = this.CtrlOnOff;
            para.CtrlACSignal = this.CtrlACSignal; 
            para.CtrlACVolt = this.CtrlACVolt;
            para.CtrlVBus = this.CtrlVBus; 
            para.CtrlRunError = this.CtrlRunError;

            para.IniRunTime = this.IniRunTime;
            para.RunTime = this.RunTime;
            para.RunACVolt = this.RunACVolt; 
            para.AlarmCode = this.AlarmCode;
            para.AlarmInfo = this.AlarmInfo;
            para.AlarmTime = this.AlarmTime;
            para.OutLevel = this.OutLevel;   

            return para;
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CUUT_Para Clone1()
        {
            CUUT_Para para = new CUUT_Para();
            para.IdCard = this.IdCard;
            para.IsNull = this.IsNull;
            para.MesFlag = this.MesFlag;
            para.ModelName = this.ModelName;
            para.BurnTime = this.BurnTime;
            para.OutPutChan = this.OutPutChan;
            para.OutPutNum = this.OutPutNum;
            para.OnOffNum = this.OnOffNum;
            para.OutLevel = this.OutLevel;

            return para;
        }
        /// <summary>
        /// 治具状态
        /// </summary>
        public EDoRun DoRun = EDoRun.位置空闲;
        /// <summary>
        /// 治具ID
        /// </summary>
        public string IdCard = string.Empty;
        /// <summary>
        /// 空治具
        /// </summary>
        public int IsNull = 0;
        /// <summary>
        /// MES连线
        /// </summary>
        public int MesFlag = 0;
        /// <summary>
        /// 工单号
        /// </summary>
        public string OrderName = string.Empty;  
        /// <summary>
        /// 老化机种名
        /// </summary>
        public string ModelName = string.Empty;
        /// <summary>
        /// 老化时间(S)
        /// </summary>
        public int BurnTime = 0;
        /// <summary>
        /// 输出组数
        /// </summary>
        public int OutPutChan = 0;
        /// <summary>
        /// 输出项目数
        /// </summary>
        public int OutPutNum = 0;
        /// <summary>
        /// ONOFF项目数
        /// </summary>
        public int OnOffNum = 0;
        /// <summary>
        /// 老化开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 老化结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 保存报表路径
        /// </summary>
        public string SavePathName = string.Empty;
        /// <summary>
        /// 报表名称
        /// </summary>
        public string SaveFileName = string.Empty;
        /// <summary>
        /// 保存报表时间
        /// </summary>
        public string SaveDataTime = string.Empty;
        /// <summary>
        /// 使用次数
        /// </summary>
        public int UsedNum = 0;
        /// <summary>
        /// 连续不良次数
        /// </summary>
        public int FailNum = 0;
        /// <summary>
        /// 治具到位光电
        /// </summary>
        public int CtrlUUTONLine = 0;
        /// <summary>
        /// ON/OFF同步信号
        /// </summary>
        public int CtrlOnOff = 0;
        /// <summary>
        /// 输入AC接触器状态
        /// </summary>
        public int CtrlACON = 0;
        /// <summary>
        /// 输入AC电压检测信号
        /// </summary>
        public int CtrlACSignal = 0;
        /// <summary>
        /// 输入AC电压值
        /// </summary>
        public double CtrlACVolt = 0;
        /// <summary>
        /// 串联负载电压
        /// </summary>
        public double CtrlVBus = 0;
        /// <summary>
        /// 控制板异常报警
        /// </summary>
        public EAlarmCode CtrlRunError = EAlarmCode.正常;
        /// <summary>
        /// 初始运行时间点(基于模板时序)
        /// </summary>
        public int IniRunTime = 0;
        /// <summary>
        /// 老化运行时间
        /// </summary>
        public int RunTime = 0;
        /// <summary>
        /// 当前输入电压
        /// </summary>
        public double RunACVolt = 0;
        /// <summary>
        /// 错误代码
        /// </summary>
        public EAlarmCode AlarmCode = EAlarmCode.正常;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string AlarmInfo = string.Empty;
        /// <summary>
        /// 错误次数
        /// </summary>
        public int AlarmTime = 0;
        /// <summary>
        /// 优先出机
        /// </summary>
        public int OutLevel = 0;
        /// <summary>
        /// 控制板报警指示
        /// </summary>
        public bool bErr = false;
        /// <summary>
        /// 检测AC异常
        /// </summary>
        public int bACErrNum = 0;
        /// <summary>
        /// 检测输入电压值
        /// </summary>
        public int bACVoltNum = 0;
        /// <summary>
        /// 等待时间监控
        /// </summary>
        public string WaitInfo = string.Empty;
        /// <summary>
        /// 动作异常
        /// </summary>
        public bool WaitAlarm = false;
        /// <summary>
        /// 动作时钟
        /// </summary>
        public Stopwatch WaitTimer = new Stopwatch();
        /// <summary>
        /// MTK1.0时间监控
        /// </summary>
        public Stopwatch MTKWatcher = new Stopwatch();
        /// <summary>
        /// 数据字符串
        /// </summary>
        public string strJson = string.Empty;
        /// <summary>
        /// 测试项目数据
        /// </summary>
        public CLED_UUT valJson = new CLED_UUT();
    }
    /// <summary>
    /// 子治具信号参数
    /// </summary>
    public class CUUT_LED
    {
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CUUT_LED Clone()
        {
            CUUT_LED led = new CUUT_LED();
            led.serialNo = this.serialNo;
            led.monV = this.monV;
            led.monA = this.monA;
            led.monVBus = this.monVBus;
            led.unitV = this.unitV;
            led.unitA = this.unitA;
            led.passResult = this.passResult;
            led.failResult = this.failResult;
            led.vFailNum = this.vFailNum;
            led.vBack = this.vBack;
            led.iFailNum = this.iFailNum;
            led.iBack = this.iBack;
            led.failEnd = this.failEnd;
            led.failTime = this.failTime;
            led.failInfo = this.failInfo;
            return led;
        }
        /// <summary>
        /// 条码
        /// </summary>
        public string serialNo = string.Empty;
        /// <summary>
        /// 输出名称
        /// </summary>
        public string vName = string.Empty;
        /// <summary>
        /// 输出下限
        /// </summary>
        public double vMin = 0;
        /// <summary>
        /// 输出上限
        /// </summary>
        public double vMax = 0;
        /// <summary>
        /// 负载模式
        /// </summary>
        public int IMode = 0;
        /// <summary>
        /// 当前电流
        /// </summary>
        public double ISet = 0;
        /// <summary>
        /// 电流下限
        /// </summary>
        public double Imin = 0;
        /// <summary>
        /// 电流上限
        /// </summary>
        public double Imax = 0;
        /// <summary>
        /// 快充电压
        /// </summary>
        public double qcv = 0;
         /// <summary>
        /// 读取电压
        /// </summary>
        public double monV = 0;
        /// <summary>
        /// 读取电流
        /// </summary>
        public double monA = 0;
        /// <summary>
        /// 读取串联电压
        /// </summary>
        public double monVBus = 0;
        /// <summary>
        /// 老化电压
        /// </summary>
        public double unitV = 0;
        /// <summary>
        /// 老化电流 
        /// </summary>
        public double unitA = 0;
        /// <summary>
        /// 良品结果
        /// </summary>
        public int passResult = 0;
        /// <summary>
        /// 不良结果
        /// </summary>
        public int failResult = 0;
        /// <summary>
        /// 电压判断
        /// </summary>
        public int vFailNum = 0;
        /// <summary>
        /// 老化电压备份
        /// </summary>
        public double vBack = 0;
        /// <summary>
        /// 电流判断
        /// </summary>
        public int iFailNum = 0;
        /// <summary>
        /// 老化电流备份
        /// </summary>
        public double iBack = 0;
        /// <summary>
        /// 不良标志
        /// </summary>
        public int failEnd = 0;
        /// <summary>
        /// 不良时间
        /// </summary>
        public string failTime = string.Empty;
        /// <summary>
        /// 不良信息
        /// </summary>
        public string failInfo = string.Empty;
        /// <summary>
        /// 数据字符串
        /// </summary>
        public string strJson = string.Empty;
        /// <summary>
        /// 测试项目数据
        /// </summary>
        public CLED_SLOT valJson = new CLED_SLOT();
    }
    /// <summary>
    /// 子治具信息
    /// </summary>
    public class CUUT
    {
        public CUUT()
        {
            for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                Led.Add(new CUUT_LED());
        }
        public override string ToString()
        {
            return "<" + Base.localName + ">";
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public CUUT Clone()
        {
            CUUT uut = new CUUT();

            uut.Base = this.Base.Clone();

            uut.Para = this.Para.Clone();

            uut.OnOff = this.OnOff.Clone();

            for (int i = 0; i < CYOHOOApp.SlotMax; i++)
                uut.Led[i] = this.Led[i].Clone();

            return uut;
        }
        /// <summary>
        /// 基本信息
        /// </summary>
        public CUUT_Base Base = new CUUT_Base();
        /// <summary>
        /// 测试参数
        /// </summary>
        public CUUT_Para Para = new CUUT_Para();
        /// <summary>
        /// 输出与时序关系
        /// </summary>
        public CUUT_ONOFF OnOff = new CUUT_ONOFF(); 
        /// <summary>
        /// 电压电流输出
        /// </summary>
        public List<CUUT_LED> Led = new List<CUUT_LED>();
    }
    #endregion

    #region 老化库体信息
    /// <summary>
    /// 动作状态 
    /// </summary>
    public enum ERun
    {
        空闲,
        就绪,
        进站,
        跳站,
        报警,
        过站
    }
    /// <summary>
    /// 机械手状态
    /// </summary>
    public enum EHandStat
    {
        空闲,
        忙碌,
        进机,
        出机,
        异常,
        进机中,
        出机中,
        进机超时,
        出机超时,
        忙碌超时,
        指定进机,
        指定超时,
        跳站,
        跳站超时,
        返板,
        返板超时
    }
    /// <summary>
    /// 分配类型
    /// </summary>
    public enum EAssMode
    { 
      进机,
      出机,
      返板
    }
    /// <summary>
    /// 子治具信息
    /// </summary>
    public class CFixture
    {
        public string idCard = string.Empty;
        public int IsFixNull = 0;
        public int MesFlag = 0;
        public string orderName = string.Empty; 
        public string modelName = string.Empty;
        public int curACV = 0;
        public List<string> serialNo = new List<string>();
        public List<int> result = new List<int>();
        public List<int> resultId = new List<int>();
        public CFixture()
        {
            serialNo.Clear();
            result.Clear();
            resultId.Clear();
            for (int i = 0; i <CYOHOOApp.SlotMax; i++)
            {
                serialNo.Add("");
                result.Add(0);
                resultId.Add(0);
            }
        }
    }
    /// <summary>
    /// 老化入口治具状态
    /// </summary>
    public class CEntrance
    {
        public CEntrance(string name)
        {
            this.name = name;

            for (int i = 0; i < 2; i++)
                Fixture.Add(new CFixture());
        }
        public override string ToString()
        {
            return this.name;
        }
        private string name = string.Empty;
        /// <summary>
        /// 状态标识
        /// </summary>
        public ERun DoRun = ERun.空闲;
        /// <summary>
        /// 报警
        /// </summary>
        public string AlarmInfo = string.Empty;
        /// <summary>
        /// 跳过老化测试
        /// </summary>
        public int GoWayBI = 0;
        /// <summary>
        /// 子治具
        /// </summary>
        public List<CFixture> Fixture = new List<CFixture>();
        /// <summary>
        /// 入口时间监控
        /// </summary>
        public Stopwatch InPlatwatcher = new Stopwatch();
        /// <summary>
        /// 时间监控
        /// </summary>
        public Stopwatch Wacther = new Stopwatch();
    }
    /// <summary>
    /// 读PLC信号
    /// </summary>
    public class CPLCSignal
    {
        /// <summary>
        /// PLC运行状态
        /// </summary>
        public string sysStat = string.Empty;
        /// <summary>
        /// 平均温度值
        /// </summary>
        public double rTemp = 0;
        /// <summary>
        /// 读取当前AC电压值
        /// </summary>
        public int rACVolt = 0;
        /// <summary>
        /// 60个温度读值
        /// </summary>
        public double[] rTempPoint = new double[15];
        /// <summary>
        /// 机械手状态
        /// </summary>
        public EHandStat handStat = EHandStat.空闲;
        /// <summary>
        /// 入口平台读卡就绪
        /// </summary>
        public int rIdReady = 0;
        /// <summary>
        /// 入口平台顶升进机就绪
        /// </summary>
        public int rInReady = 0;
        /// <summary>
        /// 出机就绪
        /// </summary>
        public int rOutReady = 0;
        /// <summary>
        /// 返板入口
        /// </summary>
        public int rReturnEntrance = 0;
        /// <summary>
        /// 返板出口
        /// </summary>
        public int rReturnExit = 0;
    }
    /// <summary>
    /// 机械运行操作
    /// </summary>
    public class CRunOP
    {
        /// <summary>
        /// 进空治具
        /// </summary>
        public int ChkInFixEmpty = 0;
        /// <summary>
        /// 禁止进机
        /// </summary>
        public int ChkForbitIn = 0;
        /// <summary>
        /// 出空治具
        /// </summary>
        public int ChkOutFixEmpty = 0;
        /// <summary>
        /// 禁止出机
        /// </summary>
        public int ChkForbitOut = 0;
        /// <summary>
        /// 进出机操作
        /// </summary>
        public EHandStat DoRun = EHandStat.空闲;
        /// <summary>
        /// 进出机动作状态
        /// </summary>
        public bool IsBusy = false;
        /// <summary>
        /// 进机位置
        /// </summary>
        public int InPos = 0;
        /// <summary>
        /// 出机位置
        /// </summary>
        public int OutPos = 0;
        /// <summary>
        /// 重发命令次数
        /// </summary>
        public int RepeatCmd = 0;
        /// <summary>
        /// 交替进出机
        /// </summary>
        public EAssMode Alternant = EAssMode.出机;
        /// <summary>
        /// 动作时间
        /// </summary>
        public Stopwatch watcher = new Stopwatch();
    }
    /// <summary>
    /// 治具产品统计
    /// </summary>
    public class CFixYield
    {
        /// <summary>
        /// 默认机种路径
        /// </summary>
        public string defaultModelPath = string.Empty;
        /// <summary>
        /// 进BI治具数量
        /// </summary>
        public int InBIFixNo = 0;
        /// <summary>
        /// 出BI治具数量
        /// </summary>
        public int OutBIFixNo = 0;
        /// <summary>
        /// 当前老化产品数
        /// </summary>
        public int BITTNum = 0;
        /// <summary>
        /// 当前老化良品数
        /// </summary>
        public int BIPASSNum = 0;

    }
    /// <summary>
    /// 当日产能
    /// </summary>
    public class CDailyYield
    {
        public int ttNum;
        public int failNum;
        public string dayNow;
        public int yieldTTNum;
        public int yieldFailNum;
    }
    /// <summary>
    /// 库体运行状态
    /// </summary>
    public class CCHmrStatus
    {
        /// <summary>
        /// PLC信号解析
        /// </summary>
        public CPLCSignal PLC = new CPLCSignal();
        /// <summary>
        /// 入口进机读卡状态
        /// </summary>
        public CEntrance Entrance = new CEntrance("<入口读卡平台位>");
        /// <summary>
        /// 入口顶升平台
        /// </summary>
        public CEntrance InPlatForm = new CEntrance("<入口顶升平台位>");
        /// <summary>
        /// 进出机动作
        /// </summary>
        public CRunOP OP = new CRunOP();
        /// <summary>
        /// 治具产品统计
        /// </summary>
        public CFixYield FixYield = new CFixYield();
        /// <summary>
        /// 日统计
        /// </summary>
        public CDailyYield DayYield = new CDailyYield();
        /// <summary>
        /// 确定当前机种
        /// </summary>
        public bool CurSelOut = false;
        /// <summary>
        /// 变更机种
        /// </summary>
        public bool CurModelChange = false;
        /// <summary>
        /// 当前出老化机种
        /// </summary>
        public string CurOutModel = string.Empty;
        /// <summary>
        /// 当前机种列表
        /// </summary>
        public List<string> CurModelList = new List<string>();
        /// <summary>
        /// 当前机种数量
        /// </summary>
        public int CurModelNum = 0;
        /// <summary>
        /// 当前待出机种数量
        /// </summary>
        public int CurModelOutNum = 0;
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

    #region 参数类
    /// <summary>
    /// 总输入AC时序
    /// </summary>
    public class CCtrlAC
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 总运行时间(H)
        /// </summary>
        public double TotalTime = 0;
        /// <summary>
        /// 运行时间
        /// </summary>
        public int RunTime = 0;
        /// <summary>
        /// 当前AC电压
        /// </summary>
        public int CurVolt = 0;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 前运行时间
        /// </summary>
        public int BeforeRunTime = 0;
        /// <summary>
        /// 运行时间差
        /// </summary>
        public int DiffTime = 0;
        /// <summary>
        /// 计算时间差
        /// </summary>
        public Stopwatch DiffWatcher = new Stopwatch();
        /// <summary>
        /// 运行时间定时
        /// </summary>
        public Stopwatch Watcher = new Stopwatch();
        /// <summary>
        /// 当前机种时序
        /// </summary>
        public CUUT_ONOFF Time = new CUUT_ONOFF();
        /// <summary>
        /// 运行标志
        /// </summary>
        public bool bRun = false;
    }
    /// <summary>
    /// 参数类
    /// </summary>
    public class CPara
    {
        /// <summary>
        /// 由机种参数获取输出规格及ONOFF规格
        /// </summary>
        /// <param name="OnOff"></param>
        /// <param name="model"></param>
        public static bool GetOutPutAndOnOffFromModel(CModelPara runModel, ref CUUT_ONOFF OnOff, out string er)
        {
            er = string.Empty;

            try
            {
                if (runModel == null)
                {
                    er = "机种参数不存在";
                    return false;
                }

                OnOff.TimeSpec.BITime = (int)(runModel.Para.BITime * 3600);

                OnOff.TimeSpec.OutChanNum = runModel.Para.OutPut_Chan;

                OnOff.TimeSpec.OutPutNum = runModel.Para.OutPut_Num;

                OnOff.TimeSpec.OnOffNum = runModel.Para.OnOff_Num;

                //获取输出规格

                OnOff.OutPut.Clear();

                for (int i = 0; i < OnOff.TimeSpec.OutPutNum; i++)
                {
                    COUTPUT outPut = new COUTPUT();

                    for (int j = 0; j < OnOff.TimeSpec.OutChanNum; j++)
                    {
                        outPut.Chan.Add(new CCHAN());

                        outPut.Chan[j].Vname = runModel.OutPut[i].Chan[j].Vname;

                        outPut.Chan[j].Vmin = runModel.OutPut[i].Chan[j].Vmin;

                        outPut.Chan[j].Vmax = runModel.OutPut[i].Chan[j].Vmax;

                        outPut.Chan[j].Imode = runModel.OutPut[i].Chan[j].Imode;

                        outPut.Chan[j].ISet = runModel.OutPut[i].Chan[j].ISet;

                        outPut.Chan[j].Imin = runModel.OutPut[i].Chan[j].Imin;

                        outPut.Chan[j].Imax = runModel.OutPut[i].Chan[j].Imax;

                        outPut.Chan[j].QCType = runModel.OutPut[i].Chan[j].QCType;

                        outPut.Chan[j].QCV = runModel.OutPut[i].Chan[j].QCV;
                    }

                    OnOff.OutPut.Add(outPut);
                }

                //获取ONOFF规格

                List<CONOFF> onOffList = new List<CONOFF>();

                for (int i = 0; i < OnOff.TimeSpec.OnOffNum; i++)
                {
                    int leftTotalTime = runModel.OnOff[i].TotalTime;

                    while (leftTotalTime > 0)
                    {
                        for (int j = 0; j < runModel.OnOff[i].Item.Length; j++)
                        {
                            int itemTotalTime = runModel.OnOff[i].Item[j].OnOffTime *
                                               (runModel.OnOff[i].Item[j].OnTime + runModel.OnOff[i].Item[j].OffTime);

                            if (itemTotalTime == 0)
                                continue;

                            for (int z = 0; z < runModel.OnOff[i].Item[j].OnOffTime; z++)
                            {
                                int ItemTime = runModel.OnOff[i].Item[j].OnTime + runModel.OnOff[i].Item[j].OffTime;

                                if (leftTotalTime < ItemTime)  //剩余时间<ON/OFF组时间
                                {
                                    ItemTime = leftTotalTime;

                                    leftTotalTime = 0;
                                }
                                else
                                {
                                    leftTotalTime -= ItemTime;
                                }

                                CONOFF onff = new CONOFF();

                                onff.ACV = runModel.OnOff[i].Item[j].ACV;

                                onff.OutPutType = runModel.OnOff[i].Item[j].OutPutType;

                                onff.OnOffTime = ItemTime;

                                if (itemTotalTime >= runModel.OnOff[i].Item[j].OnTime)
                                {
                                    onff.OnTime = runModel.OnOff[i].Item[j].OnTime;
                                    onff.OffTime = ItemTime - runModel.OnOff[i].Item[j].OnTime;
                                }
                                else
                                {
                                    onff.OnTime = ItemTime;
                                    onff.OffTime = 0;
                                }

                                onOffList.Add(onff);

                                if (leftTotalTime <= 0)
                                    break;
                            }
                        }

                    }

                }

                //计算实际ON/OFF

                OnOff.OnOff.Clear();

                int leftTime = OnOff.TimeSpec.BITime;

                while (leftTime > 0)
                {
                    for (int i = 0; i < onOffList.Count; i++)
                    {
                        int itemTime = onOffList[i].OnOffTime;

                        if (leftTime < itemTime)  //剩余时间<ON/OFF组时间
                        {
                            itemTime = leftTime;

                            leftTime = 0;
                        }
                        else
                        {
                            leftTime -= itemTime;
                        }

                        CONOFF onoff = new CONOFF();

                        onoff.ACV = onOffList[i].ACV;

                        onoff.OutPutType = onOffList[i].OutPutType;

                        onoff.OnOffTime = itemTime;

                        if (itemTime >= onOffList[i].OnTime)
                        {
                            onoff.OnTime = onOffList[i].OnTime;
                            onoff.OffTime = itemTime - onOffList[i].OnTime;
                        }
                        else
                        {
                            onoff.OnTime = itemTime;
                            onoff.OffTime = 0;
                        }

                        OnOff.OnOff.Add(onoff);

                        if (leftTime <= 0)
                            break;
                    }
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
        /// 获取当前时序步骤
        /// </summary>
        /// <param name="runTime"></param>
        /// <param name="onOffList"></param>
        /// <param name="runStep"></param>
        /// <returns></returns>
        public static bool GetCurStepFromOnOff(int runTime, List<CONOFF> onOffList, out CRUNTIME runStep, out string er)
        {
            runStep = null;

            er = string.Empty;

            try
            {
                int countTime = 0;

                for (int i = 0; i < onOffList.Count; i++)
                {
                    countTime += onOffList[i].OnOffTime;
                }

                if (countTime <= 0)
                    return true;

                int leftTime = runTime;

                while (leftTime > 0)
                {
                    for (int i = 0; i < onOffList.Count; i++)
                    {
                        int itemTime = onOffList[i].OnOffTime;

                        if (leftTime <= itemTime)  //剩余时间<ON/OFF组时间
                        {
                            itemTime = leftTime;

                            leftTime = 0;
                        }
                        else
                        {
                            leftTime -= itemTime;
                        }

                        //当前ON/OFF段
                        if (leftTime <= 0)
                        {
                            runStep = new CRUNTIME();
                            runStep.CurRunTime = runTime;
                            runStep.CurStepNo = i;
                            runStep.CurRunOutPut = onOffList[i].OutPutType;
                            if (itemTime <= onOffList[i].OnTime)
                                runStep.CurRunVolt = onOffList[i].ACV;
                            else
                                runStep.CurRunVolt = 0;
                            break;
                        }
                    }
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
        /// 由机种参数获取ONOFF规格
        /// </summary>
        /// <param name="OnOff"></param>
        /// <param name="model"></param>
        public static bool GetOnOffFromModel(CModelPara runModel, ref CUUT_ONOFF OnOff, out string er)
        {
            er = string.Empty;

            try
            {
                if (runModel == null)
                {
                    er = "机种参数不存在";
                    return false;
                }

                OnOff.TimeSpec.BITime = (int)(runModel.Para.BITime * 3600);

                OnOff.TimeSpec.OnOffNum = runModel.Para.OnOff_Num;

                //获取ONOFF规格

                List<CONOFF> onOffList = new List<CONOFF>();

                for (int i = 0; i < OnOff.TimeSpec.OnOffNum; i++)
                {
                    int leftTotalTime = runModel.OnOff[i].TotalTime;

                    while (leftTotalTime > 0)
                    {
                        if (runModel.OnOff[i].Item.Length == 0)
                            break;

                        for (int j = 0; j < runModel.OnOff[i].Item.Length; j++)
                        {
                            int itemTotalTime = runModel.OnOff[i].Item[j].OnOffTime *
                                               (runModel.OnOff[i].Item[j].OnTime + runModel.OnOff[i].Item[j].OffTime);

                            if (itemTotalTime == 0)
                                continue;

                            for (int z = 0; z < runModel.OnOff[i].Item[j].OnOffTime; z++)
                            {
                                int ItemTime = runModel.OnOff[i].Item[j].OnTime + runModel.OnOff[i].Item[j].OffTime;

                                if (leftTotalTime < ItemTime)  //剩余时间<ON/OFF组时间
                                {
                                    ItemTime = leftTotalTime;

                                    leftTotalTime = 0;
                                }
                                else
                                {
                                    leftTotalTime -= ItemTime;
                                }

                                CONOFF onff = new CONOFF();

                                onff.ACV = runModel.OnOff[i].Item[j].ACV;

                                onff.OutPutType = runModel.OnOff[i].Item[j].OutPutType;

                                onff.OnOffTime = ItemTime;

                                if (itemTotalTime >= runModel.OnOff[i].Item[j].OnTime)
                                {
                                    onff.OnTime = runModel.OnOff[i].Item[j].OnTime;
                                    onff.OffTime = ItemTime - runModel.OnOff[i].Item[j].OnTime;
                                }
                                else
                                {
                                    onff.OnTime = ItemTime;
                                    onff.OffTime = 0;
                                }

                                onOffList.Add(onff);

                                if (leftTotalTime <= 0)
                                    break;
                            }
                        }

                    }

                }

                //计算实际ON/OFF

                OnOff.OnOff.Clear();

                int leftTime = OnOff.TimeSpec.BITime;

                while (leftTime > 0)
                {
                    if (onOffList.Count == 0)
                        break;

                    for (int i = 0; i < onOffList.Count; i++)
                    {
                        int itemTime = onOffList[i].OnOffTime;

                        if (leftTime < itemTime)  //剩余时间<ON/OFF组时间
                        {
                            itemTime = leftTime;

                            leftTime = 0;
                        }
                        else
                        {
                            leftTime -= itemTime;
                        }

                        CONOFF onoff = new CONOFF();

                        onoff.ACV = onOffList[i].ACV;

                        onoff.OutPutType = onOffList[i].OutPutType;

                        onoff.OnOffTime = itemTime;

                        if (itemTime >= onOffList[i].OnTime)
                        {
                            onoff.OnTime = onOffList[i].OnTime;
                            onoff.OffTime = itemTime - onOffList[i].OnTime;
                        }
                        else
                        {
                            onoff.OnTime = itemTime;
                            onoff.OffTime = 0;
                        }

                        OnOff.OnOff.Add(onoff);

                        if (leftTime <= 0)
                            break;
                    }
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
        /// 获取时序电压
        /// </summary>
        /// <param name="runTime"></param>
        /// <param name="onOffList"></param>
        /// <param name="runStep"></param>
        /// <returns></returns>
        public static bool GetCurACVoltFromOnOff(int runTime, List<CONOFF> onOffList, out CRUNTIME runStep, out string er)
        {
            runStep = null;

            er = string.Empty;

            try
            {
                int leftTime = runTime;

                while (leftTime > 0)
                {
                    for (int i = 0; i < onOffList.Count; i++)
                    {
                        int itemTime = onOffList[i].OnOffTime;

                        if (leftTime <= itemTime)  //剩余时间<ON/OFF组时间
                        {
                            itemTime = leftTime;

                            leftTime = 0;
                        }
                        else
                        {
                            leftTime -= itemTime;
                        }

                        //当前ON/OFF段
                        if (leftTime <= 0)
                        {
                            runStep = new CRUNTIME();
                            runStep.CurRunTime = runTime;
                            runStep.CurStepNo = i;
                            runStep.CurRunOutPut = onOffList[i].OutPutType;
                            runStep.CurRunVolt = onOffList[i].ACV;
                            break;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
    }
    #endregion

    #region 曲线监控
    /// <summary>
    /// 位置单元
    /// </summary>
    public class CUnit
    {
        public CUnit(int FixNo, string Name)
        {
            this.FixNo = FixNo;

            this.Name = Name;
        }
        public override string ToString()
        {
            return this.Name;
        }
        /// <summary>
        /// 编号
        /// </summary>
        public int FixNo = 0;
        /// <summary>
        /// 位置名称
        /// </summary>
        public string Name = string.Empty;
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime = string.Empty;
        /// <summary>
        /// 老化总时间
        /// </summary>
        public int BurnTime = 0;
        /// <summary>
        /// 运行时间
        /// </summary>
        public int CurRunTime = 0;
        /// <summary>
        /// 当前温度
        /// </summary>
        public double CurTemp = 0;
        /// <summary>
        /// 当前输入AC
        /// </summary>
        public double CurACV = 0;
        /// <summary>
        /// 输入AC电压
        /// </summary>
        public CChart ACV = new CChart();
        /// <summary>
        /// 温度值
        /// </summary>
        public CChart Temp = new CChart();
        /// <summary>
        ///计时
        /// </summary>
        public String CountTime = string.Empty;
    }
    /// <summary>
    /// 曲线参数
    /// </summary>
    public class CChart
    {
        /// <summary>
        /// X轴
        /// </summary>
        public List<double> X = new List<double>();
        /// <summary>
        /// Y轴
        /// </summary>
        public List<double> Y = new List<double>();
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
        /// 当前温度
        /// </summary>
        public double Temp
        { get; set; }
        /// <summary>
        /// 当前输入
        /// </summary>
        public int ACV
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

    #region 产品测试项目数据
    /// <summary>
    /// 测试项目
    /// </summary>
    [DataContract]
    public class CLED_ITEM
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string VName { get; set; }
        [DataMember]
        public double Vmax { get; set; }
        [DataMember]
        public double Vmin { get; set; }
        [DataMember]
        public int QCM { get; set; }
        [DataMember]
        public double QCV { get; set; }
        [DataMember]
        public double ISet { get; set; }
        [DataMember]
        public double Imin { get; set; }
        [DataMember]
        public double Imax { get; set; }
    }
    /// <summary>
    /// 测试数据
    /// </summary>
    [DataContract]
    public class CLED_VALUE
    {
        [DataMember]
        public int Result { get; set; }
        [DataMember]
        public double UnitV { get; set; }
        [DataMember]
        public double UnitA { get; set; }
        [DataMember]
        public int FailEnd { get; set; }
        [DataMember]
        public string FailTime { get; set; }
    }
    [DataContract]
    public class CLED_UUT
    {
        [DataMember]
        public List<CLED_ITEM> Value
        { get; set; }
    }
    [DataContract]
    public class CLED_SLOT
    {
        [DataMember]
        public List<CLED_VALUE> Value
        { get; set; }
    }
    #endregion

    #region 测试参数JSON
    /// <summary>
    /// 库位信息
    /// </summary>
    [DataContract]
    public class CUUT_JSON
    {
        [DataMember]
        public int UUTNO { get; set; }
        [DataMember]
        public int DoRun { get; set; }
        [DataMember]
        public int AlarmCode { get; set; }
        [DataMember]
        public int AlarmTime { get; set; }
        [DataMember]
        public string AlarmInfo { get; set; }
        [DataMember]
        public int RunTime { get; set; }
        [DataMember]
        public double RunACVolt { get; set; }
        [DataMember]
        public int CtrlACON { get; set; }
        [DataMember]
        public int CtrlOnOff { get; set; }
        [DataMember]
        public double CtrlACVolt { get; set; }
        [DataMember]
        public double CtrlVBus { get; set; }
        [DataMember]
        public int CtrlUUTONLine { get; set; }
        [DataMember]
        public int CtrlRunError { get; set; }
        [DataMember]
        public int CurStepNo { get; set; }
        [DataMember]
        public int CurOutPut { get; set; }
        [DataMember]
        public int CurACV { get; set; }
        [DataMember]
        public int CurQType { get; set; }
        [DataMember]
        public double CurQCV { get; set; }
        [DataMember]
        public int CurQCM { get; set; }
        [DataMember]
        public string SaveDataTime { get; set; }
    }
    /// <summary>
    /// 测试点信息
    /// </summary>
    [DataContract]
    public class CLED_JSON
    {
        [DataMember]
        public int LEDNO { get; set; }
        [DataMember]
        public double UnitV { get; set; }
        [DataMember]
        public double UnitA { get; set; }
        [DataMember]
        public int PassResult { get; set; }
        [DataMember]
        public int FailResult { get; set; }
        [DataMember]
        public int FailEnd { get; set; }
        [DataMember]
        public string FailTime { get; set; }
        [DataMember]
        public string FailInfo { get; set; }
        [DataMember]
        public string StrJson { get; set; }
    }
    /// <summary>
    /// 库位列表
    /// </summary>
    [DataContract]
    public class CUUTList
    {
        [DataMember]
        public List<CUUT_JSON> UUT { get; set; }
    }
    /// <summary>
    /// 测试列表
    /// </summary>
    [DataContract]
    public class CLEDList
    {
        [DataMember]
        public List<CLED_JSON> LED { get; set; }
    }
    #endregion
}
