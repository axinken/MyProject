using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.KunX.BURNIN.Udc
{
    public partial class udcClrDealTime : Form
    {
        public udcClrDealTime(string _TitleName = "请输入解除密码:")
        {
            InitializeComponent();

            this._TitleName = CLanguage.Lan(_TitleName);
        }
        #region 面板回调函数
        private string _TitleName = string.Empty;
        private void udcClrDealTime_Load(object sender, EventArgs e)
        {
            labTitle.Text = _TitleName;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtPassWord.Text != "BI")
            {
                MessageBox.Show(CLanguage.Lan("输入解除密码错误,请重新输入"), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        #endregion
    }
}
