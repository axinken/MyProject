using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.YOHOO.ATE.Udc
{
    public partial class FrmFailInfo : Form
    {
        #region 构造函数
        public FrmFailInfo()
        {
            InitializeComponent();
        }
        #endregion

        #region 显示窗口

        #region 字段
        private static FrmFailInfo dlg = null;
        private static object syncRoot = new object();
        #endregion

        #region 属性
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
        public static Form mdlg
        {
            get
            {
                return dlg;
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 创建唯一实例
        /// </summary>
        public static FrmFailInfo CreateInstance()
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new FrmFailInfo();
                }
            }
            return dlg;
        }
        #endregion

        #endregion

        #region 字段
        /// <summary>
        /// 不良数
        /// </summary>
        public static int failNum = 0;
        /// <summary>
        /// 设置不良数
        /// </summary>
        public static int failNumLock = 0;
        /// <summary>
        /// 确认结果
        /// </summary>
        public static int result = 0;
        #endregion

        #region 面板回调函数
        private void FrmFailInfo_Load(object sender, EventArgs e)
        {
            labInfo.Text = string.Format("",failNum,failNumLock);
        }
        private void btnFail_Click(object sender, EventArgs e)
        {
            if (CGlobalPara.LogLevel[0] == 0)
            {
                MessageBox.Show(CLanguage.Lan("当前用户权限不够解除报警"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show(CLanguage.Lan("确认为【不良产品】,治具将直接过站？"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                DialogResult.Yes)
                return;

            result = 1;

            if (dlg != null)
            {
                dlg.Dispose();
                dlg = null;
            }
        }
        private void btnReTest_Click(object sender, EventArgs e)
        {
            if (CGlobalPara.LogLevel[0] == 0)
            {
                MessageBox.Show(CLanguage.Lan("当前用户权限不够解除报警"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (MessageBox.Show(CLanguage.Lan("确认为【不良重测】,治具将直接重新测试？"), "Tip", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
               DialogResult.Yes)
                return;
            
            result = 2;

            if (dlg != null)
            {
                dlg.Dispose();
                dlg = null;
            }
        }
        #endregion

      

      
    }
}
