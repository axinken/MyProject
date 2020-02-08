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
using GJ.DEV.SafeDog;

namespace GJ.KunX.LOADUP
{
    public partial class FrmDogLock : Form
    {
        private FrmDogLock(string name)
        {
            InitializeComponent();

            this._name = name;
        }

        #region 字段
        private static FrmDogLock dlg = null;
        private static object syncRoot = new object();
        public static int LogOK = 0;
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
        public static FrmDogLock CreateInstance(string name)
        {            
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new FrmDogLock(name);
                }
            }
            return dlg;
        }
        #endregion

        #region 面板回调函数
        public string _name = string.Empty;
        private void FrmDogLock_Load(object sender, EventArgs e)
        {
            labName.Text = CLanguage.Lan("联系方式:") + CKunXApp.DogLiaisons;

            labDogId.Text = CLanguage.Lan("加密狗序号:") + _name;

            LogOK = 0;

            txtSerialNo.SelectAll(); 

            txtSerialNo.Focus();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtSerialNo.Text == string.Empty)
                {
                    MessageBox.Show(CLanguage.Lan("请输入解锁序列号"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                string er =string.Empty;

                int leftDays =0;

                CSentinel dog = new CSentinel();

                if(!dog.check_safe_dog(CKunXApp.DogID,txtSerialNo.Text,out leftDays,out er))
                {
                  MessageBox.Show(CLanguage.Lan("输入解锁序列号错误:") + er, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                  return;
                }

                LogOK = 1;

                this.Close();
            }
            catch (Exception)
            {                
                throw;
            }           
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

        private void txtSerialNo_TextChanged(object sender, EventArgs e)
        {

        }

        
      
    }
}
