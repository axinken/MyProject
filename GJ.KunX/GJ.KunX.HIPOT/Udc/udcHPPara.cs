using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.DEV.HIPOT;
using GJ.COM;

namespace GJ.KunX.HIPOT.Udc
{
    public partial class udcHPPara : UserControl
    {
        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);

            SetLabelControlLangange(this);
        }
        private void SetLabelControlLangange(Control ctrl)
        {
            foreach (Control item in ctrl.Controls)
            {
                if (item.GetType().ToString() == "System.Windows.Forms.Label")
                {
                    Label lab = (Label)item;

                    if (lab.Tag != null)
                    {
                        lab.Text = CLanguage.Lan(lab.Tag.ToString());
                    }
                }
                else
                {
                    SetLabelControlLangange(item);
                }
            }
        }
        #endregion

        #region 构造函数
        public udcHPPara()
        {
            InitializeComponent();
        }
        #endregion

        #region 面板控件
        private TableLayoutPanel panelStep = null;
        private Label[] labStepName = null;
        private TextBox[] txtStepVal = null;
        private Label[] labStepDes = null;
        #endregion

        #region 面板回调函数
        private void OnStepValKeyPress(object sender, KeyPressEventArgs e)
        { 
            //char-8为退格键
          if(!char.IsNumber(e.KeyChar) && e.KeyChar!=(char)8 && e.KeyChar!=(char)'.')
             e.Handled=true; 
        }
        private void OnStepValChange(object sender, EventArgs e)
        {
            TextBox txtBox = (TextBox)sender;

            int idNo = System.Convert.ToInt16(txtBox.Name.Substring(7, txtBox.Name.Length - 7));

            string strBox = txtBox.Text;

            if (strBox!="" && step != null)
            {
                step.para[idNo].setVal = System.Convert.ToDouble(strBox);
                OnStepChange.OnEvented(new CStepChangeArgs(step.stepNo,idNo, step.para[idNo].setVal));  
            }                
        }
        #endregion

        #region 字段
        private CHPPara.CStep step = new CHPPara.CStep(); 
        #endregion

        #region 属性
        public CHPPara.CStep mStep
        {
            get { return step; }
        }
        #endregion

        #region 定义事件类
        public class CStepChangeArgs:EventArgs 
        {
            public CStepChangeArgs(int stepNo, int itemNo,double itemVal)
            {
                this.stepNo = stepNo; 
                this.itemNo = itemNo;
                this.itemVal = itemVal;  
            }
            public readonly int stepNo;
            public readonly int itemNo;
            public readonly double itemVal;
        }
        public COnEvent<CStepChangeArgs> OnStepChange = new COnEvent<CStepChangeArgs>();
        #endregion

        #region 方法
        public void SetItem(CHPPara.CStep stepItem)
        {
            step = stepItem;
            if (panelStep != null)
            {
                foreach (Control item in panelStep.Controls)
                {
                    item.Dispose(); 
                }
                labStepName = null;
                txtStepVal = null;
                labStepDes = null;
                panelStep.Dispose(); 
            }
            int itemNum = step.para.Count;

            labItemName.Text = CLanguage.Lan(step.des);  

            panelStep = new TableLayoutPanel();
            panelStep.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelStep, true, null);
            panelStep.Dock = DockStyle.Fill;
            panelStep.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;   
            panelStep.RowCount = itemNum + 1;
            panelStep.ColumnCount = 3;
            for (int i = 0; i <  itemNum; i++)            
                panelStep.RowStyles.Add(new RowStyle(SizeType.Absolute,28));    
            panelStep.RowStyles.Add(new RowStyle(SizeType.Percent,100));  
            panelStep.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,120));
            panelStep.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,120));
            panelStep.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,120));

            labStepName = new Label[itemNum];
            txtStepVal = new TextBox[itemNum];
            labStepDes = new Label[itemNum];

            for (int i = 0; i < itemNum; i++)
            {
                labStepName[i] = new Label();
                labStepName[i].Name = "stepName" + i.ToString(); 
                labStepName[i].Dock = DockStyle.Fill;
                labStepName[i].TextAlign = ContentAlignment.MiddleLeft;
                labStepName[i].Text = CLanguage.Lan(step.para[i].name) +":";
                panelStep.Controls.Add(labStepName[i], 0, i); 

                txtStepVal[i] = new TextBox();
                txtStepVal[i].Name = "stepVal" + i.ToString(); 
                txtStepVal[i].Dock = DockStyle.Fill;
                txtStepVal[i].TextAlign=HorizontalAlignment.Center;
                txtStepVal[i].Text = step.para[i].setVal.ToString();
                txtStepVal[i].KeyPress += new KeyPressEventHandler(OnStepValKeyPress); 
                txtStepVal[i].TextChanged += new EventHandler(OnStepValChange);  
                panelStep.Controls.Add(txtStepVal[i], 1, i); 

                labStepDes[i] = new Label();
                labStepDes[i].Name = "stepDes" + i.ToString(); 
                labStepDes[i].Dock = DockStyle.Fill;
                labStepDes[i].TextAlign = ContentAlignment.MiddleLeft;
                labStepDes[i].Text = CLanguage.Lan(step.para[i].unitDes);
                panelStep.Controls.Add(labStepDes[i], 2, i); 
            }
            splitContainer1.Panel2.Controls.Add(panelStep);    
            
        }
        #endregion
    }
}
