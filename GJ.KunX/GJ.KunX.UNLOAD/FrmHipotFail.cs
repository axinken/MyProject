using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GJ.KunX.UNLOAD
{
    public partial class FrmHipotFail : Form
    {

        #region 构造函数
        public FrmHipotFail()
        {
            InitializeComponent();
        }
        #endregion
       
        #region 字段
        private static object syncRoot = new object();
        private static FrmHipotFail dlg = null;
        private static string _passWord = string.Empty;
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
        #endregion

        #region 面板回调函数
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (dlg != null && !dlg.IsDisposed)
            {
                if (txtPassWord.Text != _passWord)
                {
                    MessageBox.Show("The password is error,please input again.","HIPOT FAIL",
                                    MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                    return;
                }
                dlg.Close();
            }
        }
        #endregion

        #region 方法
        public static FrmHipotFail CreateInstance(string passWord)
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                    dlg = new FrmHipotFail();
                _passWord = passWord;
            }
            return dlg;
        }
        /// <summary>
        /// 关闭界面
        /// </summary>
        public static void close()
        {
            if (dlg != null)
                dlg.Close();
        }
        #endregion

        private void FrmFailConfirm_Load(object sender, EventArgs e)
        {
            this.Top = Screen.AllScreens[0].WorkingArea.Height/2+50; 
            this.Left = (Screen.AllScreens[0].WorkingArea.Width-this.Width)/2; 
        }
    }
}
