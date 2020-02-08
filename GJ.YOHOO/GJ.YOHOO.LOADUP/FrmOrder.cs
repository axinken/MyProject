using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.USER.APP;
namespace GJ.YOHOO.LOADUP
{
    public partial class FrmOrder : Form
    {
        public FrmOrder()
        {
            InitializeComponent();
        }

        #region 字段
        private static FrmOrder dlg = null;
        private static object syncRoot = new object();
        public static int LogOK = 0;
        public static string LogOrder = string.Empty;
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
        public static FrmOrder CreateInstance()
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new FrmOrder();
                }
            }
            return dlg;
        }
        #endregion

        #region 面板回调函数
        private void FrmOrder_Load(object sender, EventArgs e)
        {

            txtOrder.Text = CIniFile.ReadFromIni("Parameter", "MES_Order", CGlobalPara.IniFile, "");

            LogOK = 0;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {

            if (txtOrder.Text == "")
            {
                MessageBox.Show(CLanguage.Lan("请输入当前工单号"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            LogOrder = txtOrder.Text;

            CIniFile.WriteToIni("Parameter", "MES_Order", LogOrder, CGlobalPara.IniFile);

            LogOK = 1;

            this.Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            LogOK = -1;

            this.Close();
        }
        #endregion

        #region 关闭按钮失效
        /// <summary>
        /// 重写窗体消息
        /// </summary>
        /// <param name="m">屏蔽关闭按钮</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            const int SC_MINIMIZE = 0xF020;
            const int SC_MAXIMIZE = 0xF030;
            if (m.Msg == WM_SYSCOMMAND)
            {
                switch ((int)m.WParam)
                {
                    case SC_CLOSE:
                        return;
                    case SC_MINIMIZE:
                        break;
                    case SC_MAXIMIZE:
                        break;
                    default:
                        break;
                }
            }
            base.WndProc(ref m);
        }
        #endregion
      
    }
}
