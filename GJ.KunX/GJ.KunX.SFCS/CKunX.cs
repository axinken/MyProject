using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Threading;
using GJ.SFCS;
using GJ.COM;
using mes;
namespace GJ.KunX.SFCS
{
    /// <summary>
    /// 插件类
    /// </summary>
    [ExportMetadata("PlugName", "KunX")]  //PlugName需要和接口名称同步IPlugClass中PlugName一致
    [Export(typeof(ISFCS))]
    public class CKunX:ISFCS
    {
        #region 实体类
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
        private CKunX()
        { 
        
        }
        #endregion

        #region 字段
        /// <summary>
        /// 配置文件
        /// </summary>
        private string iniFile = Environment.CurrentDirectory + "\\SFCS\\KunX.ini";
        /// <summary>
        /// 类库
        /// </summary>
        private mes.MesService mesService = new MesService();
        /// <summary>
        /// 条码
        /// </summary>
        private string serialNo = string.Empty;
        /// <summary>
        /// 结果
        /// </summary>
        private string result = string.Empty;
        /// <summary>
        /// 工单号
        /// </summary>
        private string orderName = string.Empty;
        /// <summary>
        /// 资料
        /// </summary>
        private string details = string.Empty;
        /// <summary>
        /// 错误信息
        /// </summary>
        private string errCode = string.Empty;
        /// <summary>
        /// 返回状态
        /// </summary>
        private bool bFlag = false; 
        #endregion

        #region 接口实现
        /// <summary>
        /// 测试与服务端连接
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool Start(out EMesState status, out string er)
        {
            er = string.Empty;

            status = EMesState.正常;

            try
            {

                Thread NetServer = new Thread(new ThreadStart(GetOrderAndProgram));

                NetServer.SetApartmentState(ApartmentState.STA);
            
                NetServer.IsBackground = true;
            
                NetServer.Start();

                NetServer.Join();

                er = errCode;

                return bFlag;
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
        public bool CheckSn(CSFCS.CSnInfo sn, out EMesState status, out string er)
        {
            er = string.Empty;

            status = EMesState.正常;

            try
            {
                serialNo = sn.SerialNo;

                Thread NetServer = new Thread(new ThreadStart(CheckBarcode));

                NetServer.SetApartmentState(ApartmentState.STA);

                NetServer.IsBackground = true;

                NetServer.Start();

                NetServer.Join();

                er = errCode;

                return bFlag;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        public bool TranSn(CSFCS.CSnData data,out EMesState status, out string er)
        {
            er = string.Empty;

            status = EMesState.正常;

            try
            {
                string strXML = string.Empty;

                strXML += "<inspections>" + "\r\n";

                for (int i = 0; i < data.Item.Count; i++)
                {
                    strXML += "<item line" + "=" + "\"" + (i+1).ToString()  +  "\"" + 
                                             " name=" +  "\"" + data.Item[i].Name + "\"" +
                                             " rmk=" + "\""  + data.Item[i].Desc + "\""  +
                                             " cp=" + "\"" + "" + "\"" +
                                             " std=" + "\"" + data.Item[i].Name + "\"" +
                                             " max=" + "\"" + data.Item[i].UpLimit.ToString() + "\""+
                                             " min=" + "\"" + data.Item[i].LowLimit.ToString() + "\"" +
                                             " act=" + "\"" + data.Item[i].Value.ToString() + "\"" +
                                             " um=" + "\"" + data.Item[i].Unit + "\"" +
                                             " cd=" + "\"" +"" + "\"" + 
                                             " rslt=" + "\""  + (data.Item[i].Result==0?"P":"F") + "\"" +
                               "/>";
                }

               strXML += "</inspections>" + "\r\n";

               orderName = data.OrderName;

               serialNo = data.SerialNo;
                
               result = (data.Result==0?"PASS":"FAIL");

               details = strXML;

               Thread NetServer = new Thread(new ThreadStart(SaveTestData));

               NetServer.SetApartmentState(ApartmentState.STA);

               NetServer.IsBackground = true;

               NetServer.Start();

               NetServer.Join();

               er = errCode;

               return bFlag;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        private string EscapeAttributeValue(string value)
        {
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("\r", "&#xD;").Replace("\n", "&#xA;");
        }
        #endregion

        #region 委托方法
        private void GetOrderAndProgram()
        {
            try
            {
                //设置测试平台
                //string program = mesService.GetProgram("prg");

                //if (program == null || program == string.Empty)
                //{
                //    errCode = "调用测试平台初始化参数失败";

                //    bFlag = false;

                //    return;
                //}

                errCode = mesService.GetWorkOrder();

                if (errCode == null || errCode == string.Empty)
                {
                    errCode = "调用测试平台初始化参数失败";

                    bFlag = false;

                    return;
                }

                bFlag = true;
            }
            catch (Exception ex)
            {
                errCode = ex.ToString();
                bFlag = false;
            }
        }
        private void CheckBarcode()
        {
            try
            {

                string iRet = mesService.CheckBarcode(serialNo);

                if (iRet!="OK")
                {
                    errCode = iRet;
                    bFlag = false;
                    return;
                }
                errCode = string.Empty;
                bFlag = true;
            }
            catch (Exception ex)
            {
                errCode = ex.ToString();
                bFlag = false;
            }
        }
        private void SaveTestData()
        {
            try
            {
                 string fileFolder = CIniFile.ReadFromIni("Parameter", "fileFolder", iniFile);

                 string fileName = string.Empty;

                 string iRet = string.Empty;

                 if (orderName != string.Empty)
                 {
                     iRet = mesService.SaveTestData2(orderName, serialNo, result, details, fileName);
                 }
                 else
                 {
                     iRet = mesService.SaveTestData(serialNo, result, details, fileName);
                 }

                 if (iRet!="OK")
                 {
                     errCode = "调用上传接口错误";
                     bFlag = false;
                     return;
                 }
                 errCode = string.Empty;
                 bFlag = true;
            }
            catch (Exception ex)
            {
                errCode = ex.ToString();
                bFlag = false;
            }
        }
        #endregion

    }
}
