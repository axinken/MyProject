using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace GJ.USER.APP
{
    #region 自定义结构体协议
    /// <summary>
    /// 命令定义
    /// </summary>
    public enum ESOCKET_CMD
    {
        QUERY_STATE,
        START_TEST,
        END_TEST,
        STATE_OK,
        START_OK,
        END_OK,
        NG
    }
    /// <summary>
    /// 错误代码
    /// </summary>
    public enum ESOCKET_ERROR
    {
        OK,
        SYSTEM_ERROR,
        COMMAND_LENTH_ERROR,
        COMMAND_IS_NOT_DEFINE,
        STATION_IS_NOT_EXIST,
        STATION_IS_NOT_READY
    }
    /// <summary>
    /// 结构体通信协议
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SOCKET_REQUEST
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        public ESOCKET_CMD CmdNo;
        /// <summary>
        /// 站别GUID
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string Name;
        /// <summary>
        /// 到位信号 0:空闲 1:上层到位 2:下层到位
        /// </summary>
        public int Ready;
        /// <summary>
        /// 错误代码
        /// </summary>
        public ESOCKET_ERROR ErrCode;
        /// <summary>
        /// MES连线状态 
        /// </summary>
        public int MesFlag;
        /// <summary>
        /// 机种名称
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string Model;
        /// <summary>
        /// 工单号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string OrderName;
        /// <summary>
        /// 治具ID
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string IdCard;
        /// <summary>
        /// 治具产品数:最大为16
        /// </summary>
        public int UUT_NUM;
        /// <summary>
        /// 治具产品信息
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public SOCKET_UUT[] UUT;
    }
    /// <summary>
    /// 治具产品信息
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SOCKET_UUT
    {
        /// <summary>
        /// 条码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string SerialNo;
        /// <summary>
        /// 测试结果
        /// </summary>
        public int Result;
    }
    /// <summary>
    /// 解析SOCKET结构体数据
    /// </summary>
    public class CSOCKET_INFO
    {
        public static string show(SOCKET_REQUEST request)
        {
            string strInfo = string.Empty;

            strInfo += request.CmdNo.ToString() + "_";
            strInfo += request.Name.ToString() + "_";
            if (request.Ready == 0)
                strInfo += request.Ready.ToString();
            else
            {
                strInfo += request.ErrCode.ToString() + "_";
                strInfo += request.Ready.ToString() + "_";
                strInfo += request.Model.ToString() + "_";
                strInfo += request.OrderName.ToString() + "_";
                strInfo += request.IdCard.ToString() + "_";
                strInfo += request.MesFlag.ToString() + "_";
                for (int i = 0; i < request.UUT_NUM; i++)
                {
                    strInfo += request.UUT[i].SerialNo + ":" + request.UUT[i].Result.ToString();
                    if (i != request.UUT_NUM - 1)
                        strInfo += "|";
                }
            }
            return strInfo;
        }
    }
    #endregion

    #region 自定义JSON协议
    /// <summary>
    /// 产品信息
    /// </summary>
    [DataContract]
    public class CJSON_UUT
    {
        /// <summary>
        /// 输出名称
        /// </summary>
        [DataMember]
        public string SerialNo
        { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        [DataMember]
        public int Result
        { get; set; }
    }
    /// <summary>
    /// JSON通信协议
    /// </summary>
    [DataContract]
    public class CJSON_REQUEST
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        [DataMember]
        public ESOCKET_CMD CmdNo
        { get; set; }
        /// <summary>
        /// 站别GUID
        /// </summary>
        [DataMember]
        public string Name
        { get; set; }
        /// <summary>
        /// 到位信号 0:空闲 1:上层到位 2:下层到位
        /// </summary>
        [DataMember]
        public int Ready
        { get; set; }
        /// <summary>
        /// 错误代码
        /// </summary>
        [DataMember]
        public ESOCKET_ERROR ErrCode
        { get; set; }
        /// <summary>
        /// MES连线状态 
        /// </summary>
        [DataMember]
        public int MesFlag
        { get; set; }
        /// <summary>
        /// 机种名
        /// </summary>
        [DataMember]
        public string Model
        { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        [DataMember]
        public string OrderName
        { get; set; }
        /// <summary>
        /// 治具ID
        /// </summary>
        [DataMember]
        public string IdCard
        { get; set; }
        /// <summary>
        /// 产品信息
        /// </summary>
        [DataMember]
        public List<CJSON_UUT> UUT
        { get; set; }
    }
    #endregion
}
