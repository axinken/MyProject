using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GJ.YOHOO.BURNIN.Udc
{
    public partial class udcSelLocal : Form
    {
        public udcSelLocal()
        {
            InitializeComponent();
        }
        public static int C_Room = 0;
        public static int C_Row = 0;
        public static int C_Col = 0;
        private void btnOK_Click(object sender, EventArgs e)
        {
            C_Row = System.Convert.ToInt16(txtRow.Text);
            C_Col = System.Convert.ToInt16(txtCol.Text);
            this.DialogResult = DialogResult.OK;
            this.Close(); 
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
