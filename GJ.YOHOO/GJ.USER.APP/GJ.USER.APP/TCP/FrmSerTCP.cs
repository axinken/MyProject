using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.UI;

namespace GJ.USER.APP
{
    public partial class FrmSerTCP : Form
    {
        #region 构造函数
        private FrmSerTCP()
        {
            InitializeComponent();
        }
        #endregion

        #region 字段
        private static FrmSerTCP dlg = null;
        private static object syncRoot = new object();
        #endregion

        #region 属性
        /// <summary>
        /// 窗口状态
        /// </summary>
        public static bool IsAvalible
        {
            get
            {
                lock (syncRoot)
                {
                    if (dlg != null && !dlg.IsDisposed)
                        return true;
                    else
                        return false;
                }
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 创建唯一实例
        /// </summary>
        public static FrmSerTCP CreateInstance()
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new FrmSerTCP();
                    dlg.TcpLog.Log("服务器开始启动监听数据...", udcRunLog.ELog.Content);
                }                
            }
            return dlg;
        }
        public static void log(string info, udcRunLog.ELog Elog)
        {
            if (dlg != null && !dlg.IsDisposed)
            {
                dlg.TcpLog.Log(info, Elog); 
            }            
        }
        #endregion
    }
}
