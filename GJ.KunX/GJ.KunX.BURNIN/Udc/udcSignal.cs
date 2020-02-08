using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using GJ.COM;

namespace GJ.KunX.BURNIN.Udc
{
    public partial class udcSignal : UserControl
    {
        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);
        }
        #endregion

        #region 构造函数
        public udcSignal()
        {
            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {

        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
        }
        #endregion

        #region 面板消息
        public COnEvent<CUIArgs> OnUIArgs = new COnEvent<CUIArgs>();
        /// <summary>
        /// 全局UI用户消息
        /// </summary>
        public class CUIArgs : EventArgs
        {
            public readonly int idNo;
            public string name;
            public readonly int lPara;
            public readonly int wPara;
            public CUIArgs(int idNo, string name, int lPara, int wPara)
            {
                this.idNo = idNo;
                this.name = name;
                this.lPara = lPara;
                this.wPara = wPara;
            }
        }
        #endregion

        #region 面板回调函数
        private void btnClrNum_Click(object sender, EventArgs e)
        {
            OnUIArgs.OnEvented(new CUIArgs(0, btnClrNum.Text, 0, 0)); 
        }
        private void chkInNull_CheckedChanged(object sender, EventArgs e)
        {
            OnUIArgs.OnEvented(new CUIArgs(1, chkInNull.Text, chkInNull.Checked?1:0, 0)); 
        }
        private void chkForbitIn_CheckedChanged(object sender, EventArgs e)
        {
            OnUIArgs.OnEvented(new CUIArgs(2, chkForbitIn.Text, chkForbitIn.Checked?1:0, 0)); 
        }
        private void chkOutNull_CheckedChanged(object sender, EventArgs e)
        {
            OnUIArgs.OnEvented(new CUIArgs(3, chkOutNull.Text, chkOutNull.Checked?1:0, 0)); 
        }
        private void chkForBitOut_CheckedChanged(object sender, EventArgs e)
        {
            OnUIArgs.OnEvented(new CUIArgs(4, chkForBitOut.Text, chkForBitOut.Checked?1:0, 0)); 
        }
        private void labOPBusy_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要复位进出机状态?"), "Tip", 
                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            OnUIArgs.OnEvented(new CUIArgs(5, "labOPBusy",0, 0)); 
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置UI信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="lPara"></param>
        public void SetUI(string name, string info, Color lPara, int wPara = 0)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, string, Color, int>(SetUI), name, info, lPara, wPara);
            else
            {
                Stopwatch wather = new Stopwatch();

                Control ctrl = null;

                FindCtrlFromName(this, name, ref ctrl);

                if (ctrl == null)
                    return;

                Label lab = (Label)ctrl;
                lab.Text = CLanguage.Lan(info);
                lab.ForeColor = lPara;
            }
        }
        private void FindCtrlFromName(Control fatherControl, string controlName, ref Control ctrl)
        {
            try
            {
                if (ctrl != null)
                    return;

                foreach (Control c in fatherControl.Controls)
                {
                    if (c.Name == controlName)
                    {
                        ctrl = c;

                        return;
                    }

                    FindCtrlFromName(c, controlName, ref ctrl);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

    }
}
