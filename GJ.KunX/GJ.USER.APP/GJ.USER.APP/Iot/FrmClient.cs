using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GJ.USER.APP.Iot
{
    public partial class FrmClient : Form
    {
        #region 构造函数
        private FrmClient()
        {
            InitializeComponent();
        }
        #endregion

        #region 字段
        private static FrmClient dlg = null;
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

        #region 面板回调函数
        private void PulishLog(string msg, Color color)
        {
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(new Action<string, Color>(PulishLog), msg, color);
                else
                {
                    int lines = rbtPublish.TextLength;
                    if (lines > 20000)
                    {
                        rbtPublish.Clear();
                    }
                    rbtPublish.AppendText(msg);
                    int lens = rbtPublish.TextLength;
                    rbtPublish.Select(lines, lens);
                    rbtPublish.SelectionColor = color;
                    rbtPublish.ScrollToCaret();
                }
            }
            catch (Exception)
            {
                
            }           
        }
        private void SubscribeLog(string msg, Color color)
        {
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(new Action<string, Color>(SubscribeLog), msg, color);
                else
                {
                    int lines = rbtSubscribe.TextLength;
                    if (lines > 20000)
                    {
                        rbtSubscribe.Clear();
                    }
                    rbtSubscribe.AppendText(msg);
                    int lens = rbtSubscribe.TextLength;
                    rbtSubscribe.Select(lines, lens);
                    rbtSubscribe.SelectionColor = color;
                    rbtSubscribe.ScrollToCaret();
                }
            }
            catch (Exception)
            {
                
            }           
        }
        #endregion

        #region 方法
        /// <summary>
        /// 创建唯一实例
        /// </summary>
        public static FrmClient CreateInstance()
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new FrmClient();
                }
            }
            return dlg;
        }
        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="info"></param>
        /// <param name="color"></param>
        public static void Publish(string info, Color color)
        {
            if (dlg != null && !dlg.IsDisposed)
            {
                dlg.PulishLog(info, color);
            }
        }
        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="info"></param>
        /// <param name="color"></param>
        public static void Subscribe(string info, Color color)
        {
            if (dlg != null && !dlg.IsDisposed)
            {
                dlg.SubscribeLog(info, color);
            }
        }
        #endregion
        
    }
}
