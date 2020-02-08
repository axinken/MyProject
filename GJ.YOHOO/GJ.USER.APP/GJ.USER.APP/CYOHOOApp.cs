using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GJ.APP;
namespace GJ.USER.APP
{
    public class CYOHOOApp:IAPP
    {
        #region 软件版本
        /// <summary>
        /// 软件升级版本
        /// </summary>
        public static string Version = "V1.0.0";
        /// <summary>
        /// 软件升级作者
        /// </summary>
        public static string Author = "kp..lin";
        /// <summary>
        /// 软件升级日期
        /// </summary>
        public static string ReleaseDate = "2019/5/29";
        /// <summary>
        /// 客户信息
        /// </summary>
        public static string Custom = "YOHOO";
        #endregion

        #region 加密狗
        /// <summary>
        /// 加密狗密钥
        /// </summary>
        public static string DogRelease = "091cimT7bBbtvsIS";
        /// <summary>
        /// 加密狗期限提示
        /// </summary>
        public static int DogDayLimit = 7;
        /// <summary>
        /// 不检查加密狗密钥
        /// </summary>
        public static string DogLock = string.Empty;
        /// <summary>
        /// 加密狗密码
        /// </summary>
        public static string DogPwr = string.Empty;
        /// <summary>
        /// 加密狗特征
        /// </summary>
        public static int DogID = 6;
        /// <summary>
        /// 加密狗联系人
        /// </summary>
        public static string DogLiaisons = "【13823217175,熊】";
        #endregion

        #region 项目信息
        /// <summary>
        /// 项目编号
        /// </summary>
        public static string ProID = "AUTO-CSX-YH190901-1";
        /// <summary>
        /// 项目名称
        /// </summary>
        public static string ProName = "优琥电子自动烧机测试线";
        /// <summary>
        /// 项目描述
        /// </summary>
        public static string ProDesc = string.Empty;
        #endregion

        #region 测试流程定义
        /// <summary>
        /// 人工绑定位
        /// </summary>
        public static int LOADUP_FlowId = 0;
        /// <summary>
        /// 人工绑定位
        /// </summary>
        public static string LOADUP_FlowName = "LOADUP";
        /// <summary>
        /// 通电测试流程编号
        /// </summary>
        public static int PRETEST_FlowId = 1;
        /// <summary>
        /// 通电测试流程名称
        /// </summary>
        public static string PRETEST_FlowName = "PRETEST";
        /// <summary>
        /// 老化流程编号
        /// </summary>
        public static int BI_FlowId = 2;
        /// <summary>
        /// 老化流程名称
        /// </summary>
        public static string BI_FlowName = "BURNIN";
        /// <summary>
        /// 高压流程编号
        /// </summary>
        public static int HIPOT_FlowId = 3;
        /// <summary>
        /// 高压流程名称
        /// </summary>
        public static string HIPOT_FlowName = "HIPOT";
        /// <summary>
        /// ATE流程编号
        /// </summary>
        public static int ATE_FlowId = 4;
        /// <summary>
        /// ATE流程名称
        /// </summary>
        public static string ATE_FlowName = "ATE";
        /// <summary>
        /// 下机流程编号
        /// </summary>
        public static int UNLOAD_FlowId = 5;
        /// <summary>
        /// 下机流程名称
        /// </summary>
        public static string UNLOAD_FlowName = "UNLOAD";
        #endregion

        #region 通用参数
        /// <summary>
        /// 线体编号
        /// </summary>
        public static int LineNo = 0;
        /// <summary>
        /// 线体名称
        /// </summary>
        public static string LineName = string.Empty;
        /// <summary>
        /// 客户MES工单号
        /// </summary>
        public static string OrderName = "";
        /// <summary>
        /// 治具槽位数
        /// </summary>
        public static int SlotMax = 16;
        /// <summary>
        /// 治具方向 0:(1->16);1:(16->1)
        /// </summary>
        public static int FixPos = 0;
        /// <summary>
        /// 冠佳Web地址
        /// </summary>
        public static string UlrWeb = "http://192.168.3.130/Service.asmx";
        #endregion

        #region 通电测试位
        /// <summary>
        /// 人工扫描条码枪1对应产品序号
        /// </summary>
        public static string LOADUP_Manual_SnNo1 = "1,2,3,4,5,6,7,8";
        /// <summary>
        /// 人工扫描条码枪2对应产品序号
        /// </summary>
        public static string LOADUP_Manual_SnNo2 = "9,10,11,12,13,14,15,16";
        /// <summary>
        /// 自动扫描条码枪1对应产品序号
        /// </summary>
        public static string LOADUP_Auto_SnNo1 = "1,2,3,4,5,6,7,8";
        /// <summary>
        /// 自动扫描条码枪2对应产品序号
        /// </summary>
        public static string LOADUP_Auto_SnNo2 = "16,15,14,13,12,11,10,9";
        #endregion

        #region 老化测试位
        /// <summary>
        /// 快充模式
        /// </summary>
        public static string QCM_Type = "Normal,QC2_0,QC3_0,FCP,SCP,PD3_0,PE3_0,PE1_0,PE2_0,MTK1_0,MTK2_0";
        /// <summary>
        /// 老化房列方向 0:从左到右 1:从右到左
        /// </summary>
        public static int ColPos = 0;
        #endregion

        #region 互联网
        /// <summary>
        /// 互联网使能
        /// </summary>
        public static int IoT_Enable = 0;
        /// <summary>
        /// 互联网IP
        /// </summary>
        public static string IoT_Server = "192.168.3.130:61613";
        /// <summary>
        /// 工厂名称
        /// </summary>
        public static string Iot_Factory = "KLK";
        /// <summary>
        /// 互联网登录用户
        /// </summary>
        public static string IoT_Admin = "admin";
        /// <summary>
        /// 互联网用户密码
        /// </summary>
        public static string IoT_Pwr= "password";
        #endregion

        #region 方法
        /// <summary>
        /// 初始化设置
        /// </summary>
        public void loadAppSetting()
        {
          
        }
        #endregion
       
    }
}
