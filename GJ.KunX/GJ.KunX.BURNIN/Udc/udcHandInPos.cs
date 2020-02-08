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
    public partial class udcHandInPos : Form
    {
        #region 构造函数
        public udcHandInPos()
        {
            InitializeComponent();
        }
        #endregion        

        #region 字段
        /// <summary>
        /// 区号
        /// </summary>
        public static int C_Room = 0;
        /// <summary>
        /// 行数
        /// </summary>
        public static int C_Row = 0;
        /// <summary>
        /// 列数
        /// </summary>
        public static int C_Col = 0;
        #endregion

        #region 面板回调函数
        private void udcHandInPos_Load(object sender, EventArgs e)
        {
            labRow.Text = CLanguage.Lan("层数") + "(1-" + CGlobalPara.C_ROW_MAX.ToString() + "):";
            labCol.Text = CLanguage.Lan("列数") + "(1-" + CGlobalPara.C_COL_MAX.ToString() + "):";
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            C_Row = System.Convert.ToInt32(txtRow.Text);

            C_Col = System.Convert.ToInt32(txtCol.Text);

            if (C_Row < 1 || C_Row > CGlobalPara.C_ROW_MAX)
            {
                MessageBox.Show(CLanguage.Lan("输入层数范围为") + "1-" + CGlobalPara.C_ROW_MAX.ToString());
                return;
            }
            if (C_Col < 1 || C_Col > CGlobalPara.C_COL_MAX)
            {
                MessageBox.Show(CLanguage.Lan("输入列数范围为") + "1-"+ CGlobalPara.C_COL_MAX.ToString());
                return;
            }

            this.DialogResult = DialogResult.OK;
            
            this.Close();
        }
        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void txtRow_KeyPress(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8)
                e.Handled = true;
        }
        private void txtCol_KeyPress(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8)
                e.Handled = true;
        }
        #endregion
    }
}
