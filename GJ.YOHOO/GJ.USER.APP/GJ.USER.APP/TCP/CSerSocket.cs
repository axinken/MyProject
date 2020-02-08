using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Collections.Concurrent;  
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using GJ.COM;

namespace GJ.USER.APP
{
    public class CSerSocket
    {
        #region 枚举
        /// <summary>
        /// 运行状态
        /// </summary>
        public enum ERUN
        {
            空闲,
            测试就绪,
            测试中,
            测试结束,
            异常报警
        }
        #endregion

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
            /// 机种名
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

        #region 类定义
        /// <summary>
        /// 测试站信息
        /// </summary>
        public class CSAT
        {
            public CSAT(int slotNum)
            {
                this.SlotNum = slotNum;

                for (int i = 0; i < slotNum; i++)
                {
                    SerialNos.Add("");
                    Result.Add(0);
                }
            }
            /// <summary>
            /// 槽位数量
            /// </summary>
            public int SlotNum = 16;
            /// <summary>
            /// 工位ID编号
            /// </summary>
            public int StatId;
            /// <summary>
            /// 工位名称
            /// </summary>
            public string StatName;
            /// <summary>
            /// 运行状态
            /// </summary>
            public ERUN DoRun = ERUN.空闲;
            /// <summary>
            /// 子工位编号
            /// </summary>
            public int SubNo = 0;
            /// <summary>
            /// 治具ID
            /// </summary>
            public string IdCard = string.Empty;
            /// <summary>
            /// 机种名称
            /// </summary>
            public string ModelName = string.Empty;
            /// <summary>
            /// MES连线模式
            /// </summary>
            public int MesFlag = 0;
            /// <summary>
            /// 测试条码
            /// </summary>
            public List<string> SerialNos = new List<string>();
            /// <summary>
            /// 测试结果
            /// </summary>
            public List<int> Result = new List<int>();
        }
        #endregion

        #region 字段
        /// <summary>
        /// 站别名->对应站别编号
        /// </summary>
        public ConcurrentDictionary<string, int> StatIdList = new ConcurrentDictionary<string, int>();
        /// <summary>
        /// 站别号->对应测试站信息
        /// </summary>
        public ConcurrentDictionary<int, CSAT> StatRunList = new ConcurrentDictionary<int, CSAT>();
        /// <summary>
        /// 最大槽位数
        /// </summary>
        private int SLOT_MAX = 16;
        /// <summary>
        /// 接收字节数
        /// </summary>
        private int REQUEST_LEN = 0;
        /// <summary>
        /// 结构体应答信息
        /// </summary>
        private SOCKET_REQUEST socket_reponse;
        /// <summary>
        /// JSON应答信息
        /// </summary>
        private CJSON_REQUEST json_reponse;
        #endregion

        #region 构造函数
        public CSerSocket(int slotMax)
        {
            try
            {
                SLOT_MAX = slotMax;

                //初始化结构体应答信息

                socket_reponse = new SOCKET_REQUEST();

                socket_reponse.UUT = new SOCKET_UUT[SLOT_MAX];

                for (int i = 0; i < SLOT_MAX; i++)
                    socket_reponse.UUT[i] = new SOCKET_UUT();

                REQUEST_LEN = CStuct<SOCKET_REQUEST>.GetStuctLen(socket_reponse);

                //初始化JSON应答信息

                json_reponse = new CJSON_REQUEST();

                json_reponse.Name = string.Empty;

                json_reponse.IdCard = string.Empty;

                json_reponse.Model = string.Empty;

                json_reponse.OrderName = string.Empty;

                json_reponse.MesFlag = 0;

                json_reponse.Ready = 0;

                json_reponse.UUT = new List<CJSON_UUT>();

                for (int i = 0; i < SLOT_MAX; i++)
                {
                    json_reponse.UUT.Add(new CJSON_UUT
                    {
                        Result = 0,
                        SerialNo = string.Empty
                    });
                }
            }
            catch (Exception)
            {
                
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 加载工位信息
        /// </summary>
        /// <param name="serName"></param>
        /// <param name="uutMax"></param>
        public void LoadStat(List<string> serName, List<int> uutMax)
        {
            try
            {
                for (int i = 0; i < serName.Count; i++)
                {
                    if (!StatIdList.ContainsKey(serName[i]))
                        StatIdList.TryAdd(serName[i], i + 1);
                    if (!StatRunList.ContainsKey(StatIdList[serName[i]]))
                        StatRunList.TryAdd(StatIdList[serName[i]], new CSAT(uutMax[i]));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 自定义字符串应答客户端数据
        /// </summary>
        /// <param name="recvData"></param>
        /// <returns></returns>
        public string ReponseString(string recvData)
        {
            try
            {
                if (StatIdList.Count == 0)
                    return ESOCKET_ERROR.STATION_IS_NOT_EXIST.ToString();

                recvData = recvData.Replace("\r\n", "");

                recvData = recvData.Replace("\r", "");

                recvData = recvData.Replace("\n", "");

                string er = string.Empty;

                //获取命令类型

                ESOCKET_CMD cmdType = ESOCKET_CMD.QUERY_STATE;

                int statId = 0;

                string statName = string.Empty;

                if (recvData.Length > (ESOCKET_CMD.QUERY_STATE.ToString().Length + 1) &&
                    recvData.Substring(0, ESOCKET_CMD.QUERY_STATE.ToString().Length) == ESOCKET_CMD.QUERY_STATE.ToString())
                {
                    cmdType = ESOCKET_CMD.QUERY_STATE;
                    string[] strData = recvData.Split('_');
                    statName = strData[2];
                }
                else if (recvData.Length > (ESOCKET_CMD.START_TEST.ToString().Length + 1) &&
                         recvData.Substring(0, ESOCKET_CMD.START_TEST.ToString().Length) == ESOCKET_CMD.START_TEST.ToString())
                {
                    cmdType = ESOCKET_CMD.START_TEST;
                    string[] strData = recvData.Split('_');
                    statName = strData[2];
                }
                else if (recvData.Length > (ESOCKET_CMD.END_TEST.ToString().Length + 1) &&
                         recvData.Substring(0, ESOCKET_CMD.END_TEST.ToString().Length) == ESOCKET_CMD.END_TEST.ToString())
                {
                    cmdType = ESOCKET_CMD.END_TEST;
                    string[] strData = recvData.Split('_');
                    statName = strData[2];
                }
                else
                {
                    return ESOCKET_ERROR.COMMAND_IS_NOT_DEFINE.ToString();
                }

                //获取站别编号
                if (!StatIdList.ContainsKey(statName))
                    return ESOCKET_ERROR.STATION_IS_NOT_EXIST.ToString() + ":" + statName;

                statId = StatIdList[statName];

                string reposeData = string.Empty;

                switch (cmdType)
                {
                    //接收:QUERY_STATE_XXXX
                    //应答:STATE_XXX_OK_#;IdCard,Model;MesFlag;@;Sn1,Sn2,... 
                    //(#:为治具到位信号->0:未就绪;1:就绪;@:接收条码数量;Sn为条码)
                    case ESOCKET_CMD.QUERY_STATE:
                        recvData = recvData.Replace("QUERY_STATE", "STATE");
                        if (StatRunList[statId].DoRun == ERUN.空闲 || StatRunList[statId].DoRun == ERUN.测试结束)
                        {
                            reposeData = "STATE_" + statName + "_OK_0;";
                        }
                        else
                        {
                            int ready = StatRunList[statId].SubNo;
                            reposeData = "STATE_" + statName + "_OK_" + ready.ToString() + ";";
                            reposeData += StatRunList[statId].IdCard + ";";
                            reposeData += StatRunList[statId].ModelName + ";";
                            reposeData += StatRunList[statId].MesFlag.ToString() + ";";
                            reposeData += StatRunList[statId].SerialNos.Count.ToString() + ";";
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                            {
                                if (i != StatRunList[statId].SerialNos.Count - 1)
                                    reposeData += StatRunList[statId].SerialNos[i] + ",";
                                else
                                    reposeData += StatRunList[statId].SerialNos[i];
                            }
                        }
                        break;
                    //接收:START_TEST_XXX
                    //应答:START_XXX_OK
                    case ESOCKET_CMD.START_TEST:
                        if (StatRunList[statId].DoRun == ERUN.空闲)
                            reposeData = "START_" + statName + "_NG:" + ESOCKET_ERROR.STATION_IS_NOT_READY.ToString();
                        else
                        {
                            reposeData = "START_" + statName + "_OK";
                            StatRunList[statId].DoRun = ERUN.测试中;
                        }
                        break;
                    //接收:END_TEST_XXX_@;IdCard;Result1,Result2,...  @-->长度
                    //应答:END_###_OK
                    case ESOCKET_CMD.END_TEST:
                        if (StatRunList[statId].DoRun == ERUN.空闲)
                            reposeData = "END_" + statName + "_NG:" + ESOCKET_ERROR.STATION_IS_NOT_READY.ToString();
                        else
                        {
                            string[] recvList = recvData.Split(';');
                            if (recvList.Length < 2)
                            {
                                reposeData = "END_" + statName + "_NG:" + ESOCKET_ERROR.COMMAND_LENTH_ERROR.ToString();
                                return reposeData;
                            }
                            string[] soi = recvList[0].Split('_');
                            int uutMax = System.Convert.ToInt16(soi[3]);
                            string idCard = recvList[1];
                            string[] resultList = recvList[2].Split(',');
                            //if (idCard != statRunList[statId].idCard)
                            //{
                            //    reposeData = "END_" + statName + "_NG:ID ERR_" + idCard + "_" + statRunList[statId].idCard;
                            //    return reposeData;
                            //}
                            if (resultList.Length != StatRunList[statId].SerialNos.Count ||
                                uutMax != StatRunList[statId].SerialNos.Count)
                            {
                                reposeData = "END_" + statName + "_NG:" +
                                             ESOCKET_ERROR.COMMAND_LENTH_ERROR.ToString() +
                                             "_" + resultList.Length.ToString();
                                return reposeData;
                            }
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                                StatRunList[statId].Result[i] = System.Convert.ToInt32(resultList[i]);
                            reposeData = "END_" + statName + "_OK";
                            StatRunList[statId].DoRun = ERUN.测试结束;

                        }
                        break;
                    default:
                        break;
                }
                return reposeData;
            }
            catch (Exception)
            {
                return ESOCKET_ERROR.SYSTEM_ERROR.ToString();
            }
            finally
            {

            }
        }
        /// <summary>
        /// 自定义结构体应答客户端数据
        /// </summary>
        /// <param name="recvData"></param>
        /// <returns></returns>
        public byte[] ReponseStruct(byte[] recvBytes)
        {
            try
            {
                //接收字节长度
                if (recvBytes.Length != REQUEST_LEN)
                {
                    socket_reponse.CmdNo = ESOCKET_CMD.NG;
                    socket_reponse.Name = "";
                    socket_reponse.ErrCode = ESOCKET_ERROR.COMMAND_LENTH_ERROR;
                    socket_reponse.Ready = 0;
                    socket_reponse.UUT_NUM = 0;
                    return CStuct<SOCKET_REQUEST>.StructToBytes(socket_reponse);
                }

                SOCKET_REQUEST socketQuest = CStuct<SOCKET_REQUEST>.BytesToStruct(recvBytes);

                //站别名称
                string statName = socketQuest.Name;

                if (!StatIdList.ContainsKey(statName))
                {
                    socket_reponse.CmdNo = ESOCKET_CMD.NG;
                    socket_reponse.Name = socketQuest.Name;
                    socket_reponse.ErrCode = ESOCKET_ERROR.STATION_IS_NOT_EXIST;
                    socket_reponse.Ready = 0;
                    socket_reponse.UUT_NUM = 0;
                    return CStuct<SOCKET_REQUEST>.StructToBytes(socket_reponse);
                }

                int statId = StatIdList[statName];

                //命令类型
                switch (socketQuest.CmdNo)
                {
                    case ESOCKET_CMD.QUERY_STATE:
                        socket_reponse.CmdNo = ESOCKET_CMD.STATE_OK;
                        socket_reponse.Name = socketQuest.Name;
                        socket_reponse.IdCard = StatRunList[statId].IdCard;
                        socket_reponse.Model = StatRunList[statId].ModelName;
                        socket_reponse.MesFlag = StatRunList[statId].MesFlag;
                        if (StatRunList[statId].DoRun == ERUN.空闲 || StatRunList[statId].DoRun == ERUN.测试结束)
                        {
                            socket_reponse.ErrCode = ESOCKET_ERROR.OK;
                            socket_reponse.Ready = 0;
                            socket_reponse.UUT_NUM = StatRunList[statId].SlotNum;
                            for (int i = 0; i < socket_reponse.UUT_NUM; i++)
                            {
                                socket_reponse.UUT[i].SerialNo = string.Empty;
                                socket_reponse.UUT[i].Result = 0;
                                StatRunList[statId].Result[i] = 0;
                            }
                        }
                        else
                        {
                            int ready = StatRunList[statId].SubNo;
                            socket_reponse.ErrCode = ESOCKET_ERROR.OK;
                            socket_reponse.Ready = ready;
                            socket_reponse.UUT_NUM = StatRunList[statId].SlotNum;
                            for (int i = 0; i < socket_reponse.UUT_NUM; i++)
                            {
                                socket_reponse.UUT[i].SerialNo = StatRunList[statId].SerialNos[i];
                                socket_reponse.UUT[i].Result = 0;
                            }
                        }
                        break;
                    case ESOCKET_CMD.START_TEST:
                        if (StatRunList[statId].DoRun == ERUN.空闲)
                        {
                            socket_reponse.CmdNo = ESOCKET_CMD.NG;
                            socket_reponse.Name = socketQuest.Name;
                            socket_reponse.ErrCode = ESOCKET_ERROR.STATION_IS_NOT_READY;
                            socket_reponse.Ready = 0;
                            socket_reponse.UUT_NUM = 0;
                            return CStuct<SOCKET_REQUEST>.StructToBytes(socket_reponse);
                        }
                        else
                        {
                            socket_reponse.ErrCode = ESOCKET_ERROR.OK;
                            socket_reponse.CmdNo = ESOCKET_CMD.START_OK;
                            socket_reponse.Name = socketQuest.Name;
                            socket_reponse.IdCard = StatRunList[statId].IdCard;
                            socket_reponse.Model = StatRunList[statId].ModelName;
                            socket_reponse.MesFlag = StatRunList[statId].MesFlag;
                            socket_reponse.Ready = StatRunList[statId].SubNo;
                            socket_reponse.UUT_NUM = StatRunList[statId].SlotNum;
                            for (int i = 0; i < socket_reponse.UUT_NUM; i++)
                            {
                                socket_reponse.UUT[i].SerialNo = StatRunList[statId].SerialNos[i];
                                socket_reponse.UUT[i].Result = 0;
                                StatRunList[statId].Result[i] = 0;
                            }
                            StatRunList[statId].DoRun = ERUN.测试中;
                        }
                        break;
                    case ESOCKET_CMD.END_TEST:
                        if (StatRunList[statId].DoRun == ERUN.空闲)
                        {
                            socket_reponse.CmdNo = ESOCKET_CMD.NG;
                            socket_reponse.Name = socketQuest.Name;
                            socket_reponse.ErrCode = ESOCKET_ERROR.STATION_IS_NOT_READY;
                            socket_reponse.Ready = 0;
                            socket_reponse.UUT_NUM = 0;
                            return CStuct<SOCKET_REQUEST>.StructToBytes(socket_reponse);
                        }
                        else
                        {
                            socket_reponse.ErrCode = ESOCKET_ERROR.OK;
                            socket_reponse.CmdNo = ESOCKET_CMD.END_OK;
                            socket_reponse.Name = socketQuest.Name;
                            socket_reponse.IdCard = StatRunList[statId].IdCard;
                            socket_reponse.Model = StatRunList[statId].ModelName;
                            socket_reponse.MesFlag = StatRunList[statId].MesFlag;
                            socket_reponse.Ready = StatRunList[statId].SubNo;
                            socket_reponse.UUT_NUM = StatRunList[statId].SlotNum;
                            for (int i = 0; i < StatRunList[statId].SlotNum; i++)
                            {
                                socket_reponse.UUT[i].SerialNo = StatRunList[statId].SerialNos[i];
                                StatRunList[statId].Result[i] = socketQuest.UUT[i].Result;
                            }
                            StatRunList[statId].DoRun = ERUN.测试结束;
                        }
                        break;
                    default:
                        socket_reponse.CmdNo = ESOCKET_CMD.NG;
                        socket_reponse.ErrCode = ESOCKET_ERROR.COMMAND_IS_NOT_DEFINE;
                        socket_reponse.Ready = 0;
                        socket_reponse.UUT_NUM = 0;
                        break;
                }
                return CStuct<SOCKET_REQUEST>.StructToBytes(socket_reponse);
            }
            catch (Exception)
            {
                socket_reponse.CmdNo = ESOCKET_CMD.NG;
                socket_reponse.ErrCode = ESOCKET_ERROR.SYSTEM_ERROR;
                socket_reponse.Ready = 0;
                socket_reponse.UUT_NUM = 0;
                return CStuct<SOCKET_REQUEST>.StructToBytes(socket_reponse);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 自定义JSON应答客户端数据
        /// </summary>
        /// <param name="recvData"></param>
        /// <returns></returns>
        public string ReponseJSON(string recvData)
        {
            try
            {
                string reposeData = string.Empty;

                CJSON_REQUEST obj = CJSon.Deserialize<CJSON_REQUEST>(recvData);

                if (obj == null)
                {
                    json_reponse.CmdNo = ESOCKET_CMD.NG;
                    json_reponse.ErrCode = ESOCKET_ERROR.COMMAND_IS_NOT_DEFINE;
                    json_reponse.Ready = 0;
                    return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
                }

                //站别名称
                string statName = obj.Name;
                if (!StatIdList.ContainsKey(statName))
                {
                    json_reponse.CmdNo = ESOCKET_CMD.NG;
                    json_reponse.Name = obj.Name;
                    json_reponse.ErrCode = ESOCKET_ERROR.STATION_IS_NOT_EXIST;
                    json_reponse.Ready = 0;
                    return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
                }
                int statId = StatIdList[statName];

                if (obj.UUT.Count != StatRunList[statId].SerialNos.Count)
                {
                    json_reponse.CmdNo = ESOCKET_CMD.NG;
                    json_reponse.Name = obj.Name;
                    json_reponse.ErrCode = ESOCKET_ERROR.COMMAND_LENTH_ERROR;
                    json_reponse.Ready = 0;
                    return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
                }

                //命令类型
                switch (obj.CmdNo)
                {
                    case ESOCKET_CMD.QUERY_STATE:
                        json_reponse.CmdNo = ESOCKET_CMD.STATE_OK;
                        json_reponse.Name = obj.Name;
                        json_reponse.IdCard = StatRunList[statId].IdCard;
                        json_reponse.Model = StatRunList[statId].ModelName;
                        json_reponse.MesFlag = StatRunList[statId].MesFlag;
                        json_reponse.UUT.Clear();
                        if (StatRunList[statId].DoRun == ERUN.空闲 || StatRunList[statId].DoRun == ERUN.测试结束)
                        {
                            json_reponse.ErrCode = ESOCKET_ERROR.OK;
                            json_reponse.Ready = 0;
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                            {
                                json_reponse.UUT.Add(new CJSON_UUT());
                                json_reponse.UUT[i].SerialNo = string.Empty;
                                json_reponse.UUT[i].Result = 0;
                            }
                        }
                        else
                        {
                            int ready = StatRunList[statId].SubNo;
                            json_reponse.ErrCode = ESOCKET_ERROR.OK;
                            json_reponse.Ready = ready;
                            json_reponse.UUT.Clear();
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                            {
                                json_reponse.UUT.Add(new CJSON_UUT());
                                json_reponse.UUT[i].SerialNo = StatRunList[statId].SerialNos[i];
                                json_reponse.UUT[i].Result = 0;
                            }
                        }
                        break;
                    case ESOCKET_CMD.START_TEST:
                        if (StatRunList[statId].DoRun == ERUN.空闲)
                        {
                            json_reponse.CmdNo = ESOCKET_CMD.NG;
                            json_reponse.Name = obj.Name;
                            json_reponse.ErrCode = ESOCKET_ERROR.STATION_IS_NOT_READY;
                            json_reponse.Ready = 0;
                            json_reponse.UUT.Clear();
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                            {
                                json_reponse.UUT.Add(new CJSON_UUT());
                                json_reponse.UUT[i].SerialNo = string.Empty;
                                json_reponse.UUT[i].Result = 0;
                            }
                            return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
                        }
                        else
                        {
                            json_reponse.ErrCode = ESOCKET_ERROR.OK;
                            json_reponse.CmdNo = ESOCKET_CMD.START_OK;
                            json_reponse.Name = obj.Name;
                            json_reponse.IdCard = StatRunList[statId].IdCard;
                            json_reponse.Model = StatRunList[statId].ModelName;
                            json_reponse.MesFlag = StatRunList[statId].MesFlag;
                            json_reponse.Ready = StatRunList[statId].SubNo;
                            json_reponse.UUT.Clear();
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                            {
                                json_reponse.UUT.Add(new CJSON_UUT());
                                json_reponse.UUT[i].SerialNo = StatRunList[statId].SerialNos[i];
                                json_reponse.UUT[i].Result = 0;
                            }
                            StatRunList[statId].DoRun = ERUN.测试中;
                        }
                        break;
                    case ESOCKET_CMD.END_TEST:
                        if (StatRunList[statId].DoRun == ERUN.空闲)
                        {
                            json_reponse.CmdNo = ESOCKET_CMD.NG;
                            json_reponse.Name = obj.Name;
                            json_reponse.ErrCode = ESOCKET_ERROR.STATION_IS_NOT_READY;
                            json_reponse.Ready = 0;
                            return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
                        }
                        else
                        {
                            json_reponse.ErrCode = ESOCKET_ERROR.OK;
                            json_reponse.CmdNo = ESOCKET_CMD.END_OK;
                            json_reponse.Name = obj.Name;
                            json_reponse.IdCard = StatRunList[statId].IdCard;
                            json_reponse.Model = StatRunList[statId].ModelName;
                            json_reponse.MesFlag = StatRunList[statId].MesFlag;
                            json_reponse.Ready = StatRunList[statId].SubNo;
                            json_reponse.UUT.Clear();
                            for (int i = 0; i < StatRunList[statId].SerialNos.Count; i++)
                            {
                                json_reponse.UUT.Add(new CJSON_UUT());
                                socket_reponse.UUT[i].SerialNo = StatRunList[statId].SerialNos[i];
                                StatRunList[statId].Result[i] = obj.UUT[i].Result;
                            }
                            StatRunList[statId].DoRun = ERUN.测试结束;
                        }
                        break;
                    default:
                        json_reponse.CmdNo = ESOCKET_CMD.NG;
                        json_reponse.ErrCode = ESOCKET_ERROR.COMMAND_IS_NOT_DEFINE;
                        json_reponse.Ready = 0;
                        break;
                }

                return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
            }
            catch (Exception)
            {
                json_reponse.CmdNo = ESOCKET_CMD.NG;
                json_reponse.ErrCode = ESOCKET_ERROR.SYSTEM_ERROR;
                json_reponse.Ready = 0;
                return CJSon.Serializer<CJSON_REQUEST>(json_reponse);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 置状态空闲
        /// </summary>
        /// <param name="statName"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool Free(string statName, out string er)
        {
            er = string.Empty;

            try
            {

                int statId = -1;

                if (StatIdList.ContainsKey(statName))
                    statId = StatIdList[statName];

                if (statId == -1)
                {
                    er = "STATION IS NOT EXIST";
                    return false;
                }

                StatRunList[statId].DoRun = ERUN.空闲;

                return true;

            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {

            }
        }
        /// <summary>
        /// 设置就绪
        /// </summary>
        public bool Ready(CSAT stat, out string er)
        {
            er = string.Empty;

            try
            {

                int statId = -1;

                if (StatIdList.ContainsKey(stat.StatName))
                    statId = StatIdList[stat.StatName];

                if (statId == -1)
                {
                    er = "STATION IS NOT EXIST";
                    return false;
                }
                StatRunList[statId].IdCard = stat.IdCard;
                StatRunList[statId].ModelName = stat.ModelName;
                StatRunList[statId].SubNo = stat.SubNo;
                StatRunList[statId].MesFlag = stat.MesFlag;
                for (int i = 0; i < StatRunList[statId].SlotNum; i++)
                {
                    StatRunList[statId].SerialNos[i] = stat.SerialNos[i];
                    StatRunList[statId].Result[i] = 0;
                }
                StatRunList[statId].DoRun = ERUN.测试就绪;
                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {

            }
        }
        /// <summary>
        /// 读状态 
        /// </summary>
        /// <param name="statName"></param>
        /// <returns></returns>
        public ERUN ReadRunStatus(string statName)
        {
            try
            {
                int statId = -1;
                if (StatIdList.ContainsKey(statName))
                    statId = StatIdList[statName];
                if (statId == -1)
                    return ERUN.异常报警;
                return StatRunList[statId].DoRun;
            }
            catch (Exception)
            {
                return ERUN.异常报警;
            }
            finally
            {

            }
        }
        /// <summary>
        /// 读取测试结果
        /// </summary>
        /// <param name="statName"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public bool ReadResult(string statName, ref List<int> results, out string er)
        {
            er = string.Empty;

            try
            {

                int statId = -1;
                if (StatIdList.ContainsKey(statName))
                    statId = StatIdList[statName];
                if (statId == -1)
                {
                    er = "STATION IS NOT EXIST";
                    return false;
                }
                for (int i = 0; i < results.Count; i++)
                    results[i] = StatRunList[statId].Result[i];
                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {

            }
        }
        #endregion

    }
}
