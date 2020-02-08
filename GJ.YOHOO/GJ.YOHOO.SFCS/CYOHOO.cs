using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using GJ.SFCS;
using GJ.COM;
namespace GJ.YOHOO.SFCS
{
    /// <summary>
    /// 深圳市优泰克MES
    /// </summary>
    [ExportMetadata("PlugName", "YOHOO")]  //PlugName需要和接口名称同步IPlugClass中PlugName一致
    [Export(typeof(ISFCS))]
    public class CYOHOO:ISFCS
    {
        #region 实体类
        [DataContract]
        public class CTestConnectReponse
        {
            [DataMember]
            public string msg { get; set; }
            [DataMember]
            public string result { get; set; }
            [DataMember]
            public string status { get; set; }
        }
        [DataContract]
        public class CSnValidationRequest
        {
            /// <summary>
            /// 工序代码
            /// </summary>
            [DataMember]
            public string ProcessCode { get; set; }
            /// <summary>
            /// 产品条码
            /// </summary>
            [DataMember]
            public string BarCode { get; set; }
            /// <summary>
            /// 创建人ID
            /// </summary>
            [DataMember]
            public string CreateBy { get; set; }
            /// <summary>
            /// 客户端代码
            /// </summary>
            [DataMember]
            public string ClientCode { get; set; }
        }
        [DataContract]
        public class CSnValidationReponse
        {
            /// <summary>
            /// 状态:200或400
            /// </summary>
            [DataMember]
            public string status { get; set; }
            /// <summary>
            /// 结果
            /// </summary>
            [DataMember]
            public string result { get; set; }
            /// <summary>
            ///  信息:SUCCESS
            /// </summary>
            [DataMember]
            public string msg { get; set; }
            [DataMember]
            public List<CSnValidationPara> data { get; set; }
        }
        [DataContract]
        public class CSnValidationPara
        {
            [DataMember]
            public string TrackInBarCode { get; set; }
            [DataMember]
            public string ProcessCode { get; set; }
        }

        [DataContract]
        public class CSnResultRequest
        {
            /// <summary>
            /// 工序代码(必填）
            /// </summary>
            [DataMember]
            public string ProcessCode { get; set; }
            /// <summary>
            /// 产品条码(必填）
            /// </summary>
            [DataMember]
            public string BarCode { get; set; }
            /// <summary>
            /// 硬件版本（非必填）
            /// </summary>
            [DataMember]
            public string HardwareVer { get; set; }
            /// <summary>
            /// 软件版本   （非必填）
            /// </summary>
            [DataMember]
            public string SoftwareVer { get; set; }
            /// <summary>
            ///终端ID     （非必填）
            /// </summary>
            [DataMember]
            public string DeviceId { get; set; }
            /// <summary>
            /// SIM号            （非必填）
            /// </summary>
            [DataMember]
            public string SIM { get; set; }
            /// <summary>
            /// 预留字段1
            /// </summary>
            [DataMember]
            public string Ext1 { get; set; }
            /// <summary>
            /// 预留字段2
            /// </summary>
            [DataMember]
            public string Ext2 { get; set; }
            /// <summary>
            /// 预留字段3
            /// </summary>
            [DataMember]
            public string Ext3 { get; set; }
            /// <summary>
            /// 测试结果（成功1 失败 0）（必填）
            /// </summary>
            [DataMember]
            public int Flag { get; set; }
            /// <summary>
            /// 项目测试结果参数
            /// </summary>
            [DataMember]
            public string ResultDetail { get; set; }
            /// <summary>
            /// 创建人ID
            /// </summary>
            [DataMember]
            public string CreateBy { get; set; }
            /// <summary>
            /// 客户端代码
            /// </summary>
            [DataMember]
            public string ClientCode { get; set; }
        }

        [DataContract]
        public class CSnResultReponse
        {
            /// <summary>
            /// 状态:200或400
            /// </summary>
            [DataMember]
            public string status { get; set; }
            /// <summary>
            /// 结果
            /// </summary>
            [DataMember]
            public string result { get; set; }
            /// <summary>
            ///  信息:SUCCESS
            /// </summary>
            [DataMember]
            public string msg { get; set; }
        }

        #endregion

        #region 构造函数
        private CYOHOO()
        { 
        
        }
        #endregion

        #region 字段
        private string iniFile = Environment.CurrentDirectory + "\\SFCS\\YOHOO.ini";
        #endregion

        #region 接口实现
        /// <summary>
        /// 测试与服务端连接
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool Start(out EMesState status,out string er)
        {
            er = string.Empty;

            status = EMesState.正常;

            try
            {
                string url = CIniFile.ReadFromIni("Parameter", "url", iniFile, "http://192.168.50.250:8080/biz/api/") + "TestConnect";

                CTestConnectReponse reponse = null;

                string requestData = string.Empty;

                string reponseData = string.Empty;

                if (!CNet.HttpGet(url, requestData, out reponseData, out er))
                {
                    status = EMesState.网络异常;
                    return false;
                }

                reponse = CJSon.Deserialize<CTestConnectReponse>(reponseData);

                if (reponse.status != "200")
                {
                    status = EMesState.网络异常;
                    er = reponseData;
                    return false;
                }

                if (reponse.result.ToUpper() != "SUCCESS")
                {
                    status = EMesState.异常错误;
                    er = reponse.msg;
                    return false;
                }

                er = requestData;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool Close(out string er)
        {
            er = string.Empty;

            try
            {
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 检查条码
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="status"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool CheckSn(CSFCS.CSnInfo sn, out EMesState status, out string er)
        {
            er = string.Empty;

            status = EMesState.正常;

            try
            {
                string url = CIniFile.ReadFromIni("Parameter", "url", iniFile, "http://192.168.50.250:8080/biz/api/") + "tongda/GetATEProcess?";

                string processCode = CIniFile.ReadFromIni(sn.StatName,"ProcessCode", iniFile);

                string clientCode = CIniFile.ReadFromIni(sn.StatName, "ClientCode", iniFile);

                string createBy = CIniFile.ReadFromIni(sn.StatName, "CreateBy", iniFile);

                CSnValidationRequest request = new CSnValidationRequest()
                {
                    ProcessCode = processCode,
                    BarCode = sn.SerialNo,
                    ClientCode = clientCode,
                    CreateBy = createBy
                };

                CSnValidationReponse reponse = null;

                url += string.Format("ProcessCode={0}&BarCode={1}&CreateBy={2}&ClientCode={3}",
                                                    request.ProcessCode,request.BarCode,request.CreateBy,request.ClientCode);

                string requestData = string.Empty;

                string reponseData = string.Empty;

                if (!CNet.HttpPost(url, requestData, out reponseData, out er))
                {
                    status = EMesState.网络异常;  
                    return false;
                }

                reponse = CJSon.Deserialize<CSnValidationReponse>(reponseData);

                if (reponse.status != "200")
                {
                    status = EMesState.异常错误;
                    er = reponse.msg;
                    return false;
                }

                if (reponse.msg != "SUCCESS")
                {
                    status = EMesState.异常错误;
                    er = reponse.msg;
                    return false;
                }

                er = requestData;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool TranSn(CSFCS.CSnData data,out EMesState status, out string er)
        {
            er = string.Empty;

            status = EMesState.正常;

            try
            {
                string url = CIniFile.ReadFromIni("Parameter", "url", iniFile, "http://192.168.50.250:8080/biz/api/") + "tongda/WriteATEProcess02?";

                string processCode = CIniFile.ReadFromIni(data.StatName, "ProcessCode", iniFile);

                string clientCode = CIniFile.ReadFromIni(data.StatName, "ClientCode", iniFile);

                string createBy = CIniFile.ReadFromIni(data.StatName, "CreateBy", iniFile);

                CSnResultRequest request = new CSnResultRequest()
                {
                    ClientCode = clientCode,
                    ProcessCode = processCode,
                    CreateBy = createBy,
                    BarCode  = data.SerialNo,                   
                    DeviceId = data.DeviceId,
                    Flag = (data.Result==0?1:0),
                    SoftwareVer="V1.0.0",
                    HardwareVer ="V1.0.0",
                    Ext1 =string.Empty,
                    Ext2 =string.Empty,
                    Ext3 = string.Empty,
                    SIM = string.Empty,
                    ResultDetail =string.Empty
                };

                //测试内容_测试项_值范围_测试值_测试结果(测试结果为PASS或者NG)
                for (int i = 0; i < data.Item.Count; i++)
                {
                    if (i > 0)
                    {
                        request.ResultDetail += ",";
                    }
                    request.ResultDetail += data.Item[i].Name + "_";
                    request.ResultDetail +=  data.Item[i].Desc + "_" ;
                    request.ResultDetail += data.Item[i].LowLimit + data.Item[i].Unit + "~" + data.Item[i].UpLimit + data.Item[i].Unit + "_";
                    request.ResultDetail += data.Item[i].Value + "_";
                    request.ResultDetail += data.Item[i].Result == 0 ? "PASS" : "NG";
                    
                }

                CSnResultReponse reponse = null;

                url += string.Format("ProcessCode={0}"+
                                    "&BarCode={1}"+                                                 
                                    "&HardwareVer={2}" +
                                    "&SoftwareVer={3}"+
                                    "&DeviceId={4}"+
                                    "&SIM={5}"+                                  
                                    "&Ext1={6}"+
                                    "&Ext2={7}"+
                                    "&Ext3={8}"+
                                    "&Flag={9}"+
                                    "&ResultDetail={10}"+
                                    "&CreateBy={11}" +
                                    "&ClientCode={12}",
                                    request.ProcessCode, 
                                    request.BarCode,
                                    request.HardwareVer,
                                    request.SoftwareVer,
                                    request.DeviceId,
                                    request.SIM,
                                    request.Ext1,
                                    request.Ext2,
                                    request.Ext3,
                                    request.Flag,
                                    request.ResultDetail,
                                    request.CreateBy, 
                                    request.ClientCode);

                string requestData = string.Empty;

                string reponseData = string.Empty;

                if (!CNet.HttpPost(url, requestData, out reponseData, out er))
                {
                    status = EMesState.网络异常;  
                    return false;
                }

                reponse = CJSon.Deserialize<CSnResultReponse>(reponseData);

                if (reponse.status != "200")
                {
                    status = EMesState.异常错误;
                    er = reponse.msg;
                    return false;
                }

                if (reponse.msg != "SUCCESS")
                {
                    status = EMesState.异常错误;  
                    er = reponse.msg;
                    return false;
                }

                er = requestData;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        #endregion
      
    }
}
