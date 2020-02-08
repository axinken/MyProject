using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.KunX.HIPOT.Udc
{
    public partial class udcHPResult : UserControl
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
        public udcHPResult(int slotMax=16)
        {

            this.slotMax = slotMax;

            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();

        }
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {
            labId = new Label[]{
                               labId1, labId2,labId3,labId4,labId5,labId6,labId7,labId8,
                               labId9,labId10,labId11,labId12,labId13,labId14,labId15,labId16
                              };
            labSn = new Label[]{
                               labSn1,labSn2,labSn3,labSn4,labSn5,labSn6,labSn7,labSn8,
                               labSn9,labSn10,labSn11,labSn12,labSn13,labSn14,labSn15,labSn16
                               };
            labResult = new Label[]{
                                  labResult1,labResult2,labResult3,labResult4,labResult5,labResult6,labResult7,labResult8,
                                  labResult9,labResult10,labResult11,labResult12,labResult13,labResult14,labResult15,labResult16
                                  };
            labSlot = new Label[]{
                                labSlot1,labSlot2,labSlot3,labSlot4,labSlot5,labSlot6,labSlot7,labSlot8,
                                labSlot9,labSlot10,labSlot11,labSlot12,labSlot13,labSlot14,labSlot15,labSlot16
                                };
            imgUUT = new PictureBox[]{
                                    imgUUT1,imgUUT2,imgUUT3,imgUUT4,imgUUT5,imgUUT6,imgUUT7,imgUUT8,
                                    imgUUT9,imgUUT10,imgUUT11,imgUUT12,imgUUT13,imgUUT14,imgUUT15,imgUUT16
                                    };
            panel4.RowCount = slotMax;
            for (int i = slotMax; i < labSn.Length; i++)
            {
                labId[i].Visible = false;  
                labSn[i].Visible = false;
                labResult[i].Visible = false;

                labSlot[i].Text = "CH"+(i-slotMax+1).ToString("D2");  
                imgUUT[i].Visible = true;                  
            }
            
        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            panel1.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel1, true, null);
            panel2.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel2, true, null);
            panel3.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel3, true, null);
            panel4.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel4, true, null);
            panel5.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel5, true, null);

        }
        #endregion

        #region 面板控件
        private Label[] labId = null; 
        private Label[] labSn = null;
        private Label[] labResult = null;
        private Label[] labSlot = null;
        private PictureBox[] imgUUT = null;
        #endregion

        #region 字段
        private int slotMax = 16;
        #endregion

        #region 面板回调函数
        private void udcHPResult_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region 私有方法
        
        #endregion

        #region 共享方法
        /// <summary>
        /// 清除状态
        /// </summary>
        public void SetFree(int idNo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(SetFree), idNo);
            else
            {
                labResult[idNo].Text = "";
                imgUUT[idNo].Image = null;
            }
        }
        /// <summary>
        /// 设置治具ID
        /// </summary>
        /// <param name="idCard"></param>
        public void SetFixtureId(string idCard)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetFixtureId), idCard);
            else
            {
                labIdCard.Text = idCard;
            }
        }
        /// <summary>
        /// 设置条码
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="serialNo"></param>
        public void SetSnId(int idNo, string serialNo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, string>(SetSnId), idNo, serialNo);
            else
            {
                labSn[idNo].Text = serialNo;
                labResult[idNo].Text = "";
                imgUUT[idNo].Image = null;
            }
        }
        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="result"></param>
        public void SetResult(int idNo, int result)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, int>(SetResult), idNo, result);
            else
            {
                if (labSn[idNo].Text != "")
                {
                    if (result == -1)    //空闲
                    {
                        labResult[idNo].Text = "";
                        labResult[idNo].ForeColor = Color.Black;
                        imgUUT[idNo].Image = null;
                    }
                    else if (result == -2)   //报警
                    {
                        labResult[idNo].Text = "NA";
                        labResult[idNo].ForeColor = Color.Red;
                        imgUUT[idNo].Image = ImageList1.Images["NA"];
                    }
                    else if (result == 0)
                    {
                        labResult[idNo].Text = "PASS";
                        labResult[idNo].ForeColor = Color.Blue;
                        imgUUT[idNo].Image = ImageList1.Images["PASS"];
                    }
                    else
                    {
                        labResult[idNo].Text = "FAIL";
                        labResult[idNo].ForeColor = Color.Red;
                        imgUUT[idNo].Image = ImageList1.Images["FAIL"];
                    }
                }
            }
        }
        #endregion

    }
}
