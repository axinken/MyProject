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
    public partial class udcHPData : UserControl
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
        public udcHPData(int slotMax=8)
        {

            this.slotMax = slotMax;

            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();

        }

        #region 字段
        private int slotMax = 8;
        #endregion

        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {          
            for (int i = 0; i < labValList.Length; i++)
                labValList[i] = new List<Label>();              
        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {

            splitContainer1.Panel1.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(splitContainer1.Panel1, true, null);
            splitContainer1.Panel2.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(splitContainer1.Panel2, true, null);
        }
 
        #endregion

        #region 面板控件
        private TableLayoutPanel[] panelUUT = new TableLayoutPanel[2];
        private List<Label>[] labValList = new List<Label>[16]; 
        private List<Label> labResultList = new List<Label>();        
        #endregion

        #region 面板回调函数
        private void udcHPData_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region 私有方法
        
        #endregion

        #region 共享方法
        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="step"></param>
        public void RefreshUI(List<CHPPara.CStep> step)
        {

            int chanNum = 8;

            for (int i = 0; i < labValList.Length; i++)            
                labValList[i].Clear();
            labResultList.Clear();

            for (int i = 0; i < panelUUT.Length; i++)
            {
                if(panelUUT[i]!=null)
                {
                    panelUUT[i].Dispose();
                    panelUUT[i] = null;
                }
                panelUUT[i] = new TableLayoutPanel();
                panelUUT[i].Dock = DockStyle.Fill;
                panelUUT[i].CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
                panelUUT[i].GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panelUUT[i], true, null);
            }

            int stepNo = step.Count;

            for (int index = 0; index < 2; index++)           
            {
                panelUUT[index].RowCount = stepNo + 3;
                for (int i = 0; i < stepNo+2; i++)
                     panelUUT[index].RowStyles.Add(new RowStyle(SizeType.Absolute,24));
                panelUUT[index].RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                panelUUT[index].ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,80));
                for (int i = 0; i < chanNum; i++)
                    panelUUT[index].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                Label labid = new Label();
                labid.Dock = DockStyle.Fill;
                labid.Margin = new Padding(0); 
                labid.BackColor = Color.Turquoise;
                labid.TextAlign = ContentAlignment.MiddleCenter;
                labid.Font = new Font("宋体", 12);
                labid.Text = "StepNo";
                panelUUT[index].Controls.Add(labid, 0, 0);

                for (int i = 0; i < chanNum; i++)
                {
                    Label labCH = new Label();
                    labCH.Dock = DockStyle.Fill;
                    labCH.Margin = new Padding(0); 
                    labCH.BackColor = Color.Turquoise;
                    labCH.TextAlign = ContentAlignment.MiddleCenter;
                    labCH.Font = new Font("宋体", 12);
                    if(slotMax==8)
                       labCH.Text = "CH" + (i + 1).ToString("D2");
                    else
                        labCH.Text = "CH" + (i + index * chanNum + 1).ToString("D2");
                    panelUUT[index].Controls.Add(labCH, i+1, 0); 
                }
            }

            for (int index = 0; index < 2; index++)
            {
                for (int i = 0; i < stepNo; i++)
                {
                    Label labStep = new Label();
                    labStep.Dock = DockStyle.Fill;
                    labStep.TextAlign = ContentAlignment.MiddleCenter;
                    labStep.Margin = new Padding(0);
                    labStep.Font = new Font("宋体", 12);
                    labStep.Text = step[i].name.ToString();
                    panelUUT[index].Controls.Add(labStep, 0, i + 1);

                    for (int CH = 0; CH < chanNum; CH++)
                    {
                        Label labVal = new Label();
                        labVal.Dock = DockStyle.Fill;
                        labVal.TextAlign = ContentAlignment.MiddleCenter;
                        labVal.Margin = new Padding(0);
                        labVal.Font = new Font("宋体", 12);
                        labVal.BackColor = Color.White;
                        labVal.Text ="";
                        labValList[CH + index * chanNum].Add(labVal);
                        panelUUT[index].Controls.Add(labValList[CH + index * chanNum][i], CH + 1, i + 1);
                    }
                }

                if (stepNo > 0)
                {
                    Label labResultId = new Label();
                    labResultId.Dock = DockStyle.Fill;
                    labResultId.TextAlign = ContentAlignment.MiddleCenter;
                    labResultId.Margin = new Padding(0);
                    labResultId.Font = new Font("宋体", 12);
                    labResultId.Text = "Result";
                    panelUUT[index].Controls.Add(labResultId, 0, stepNo + 1);

                    for (int CH = 0; CH < chanNum; CH++)
                    {
                        Label labResult = new Label();
                        labResult.Dock = DockStyle.Fill;
                        labResult.TextAlign = ContentAlignment.MiddleCenter;
                        labResult.Margin = new Padding(0);
                        labResult.Font = new Font("宋体", 12);
                        labResult.BackColor = Color.White;
                        labResult.Text = "";
                        labResultList.Add(labResult);
                        panelUUT[index].Controls.Add(labResultList[CH + index * chanNum], CH + 1, stepNo + 1);
                    }
                }              
            }

            splitContainer1.Panel1.Controls.Add(panelUUT[0]);
            splitContainer1.Panel2.Controls.Add(panelUUT[1]);  
        }
        /// <summary>
        /// 初始化UI
        /// </summary>
        public void SetFree(int idNo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int>(SetFree),idNo);
            else
            {
                for (int j = 0; j < labValList[idNo].Count; j++)
                    labValList[idNo][j].Text = "";
                labResultList[idNo].Text = "";
            }
        }
        /// <summary>
        /// 设置测试数据
        /// </summary>
        /// <param name="stepValList"></param>
        public void SetTestVal(List<string> serialNo,List<CHPPara.CStepVal> stepValList)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<List<string>,List<CHPPara.CStepVal>>(SetTestVal), serialNo,stepValList);
            else
            {
                for (int uutNo = 0; uutNo < labValList.Length; uutNo++)
                {
                    if (stepValList[uutNo].mVal.Count != labValList[uutNo].Count)
                        continue;
                    string failCode = string.Empty; 
                    for (int stepNo = 0; stepNo < labValList[uutNo].Count; stepNo++)
                    {                        
                        EStepName stepName = stepValList[uutNo].mVal[stepNo].name; 
                        double stepVal = stepValList[uutNo].mVal[stepNo].value;
                        string stepUnit = stepValList[uutNo].mVal[stepNo].unit;
                        string stepShow = stepVal.ToString("0.00") + stepUnit;                       
                        if (stepName == EStepName.IR)
                        {
                            if (stepVal >= 1000)
                                stepShow = "UUUUUU";
                        }
                        if (serialNo[uutNo] != string.Empty)
                        {
                            labValList[uutNo][stepNo].Text = stepShow;
                            if (stepValList[uutNo].mVal[stepNo].result == 0)
                                labValList[uutNo][stepNo].ForeColor = Color.Blue;
                            else
                            {
                                labValList[uutNo][stepNo].ForeColor = Color.Red;
                                failCode = stepValList[uutNo].mVal[stepNo].code;
                            }
                        }
                        else
                        {
                            labValList[uutNo][stepNo].Text = "";
                            labValList[uutNo][stepNo].ForeColor = Color.Black;
                        }
                    }
                    if (serialNo[uutNo] != string.Empty)
                    {
                        if (stepValList[uutNo].result == 0)
                        {
                            labResultList[uutNo].Text = "PASS";
                            labResultList[uutNo].ForeColor = Color.Blue;
                        }
                        else
                        {
                            labResultList[uutNo].Text = failCode;
                            labResultList[uutNo].ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        labResultList[uutNo].Text = "";
                        labResultList[uutNo].ForeColor = Color.Black;
                    }
                }
            }
        }
        /// <summary>
        /// 设置单个产品数据
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="stepVal"></param>
        public void SetTestVal(int uutNo, CHPPara.CStepVal ItemVal)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, CHPPara.CStepVal>(SetTestVal),uutNo, ItemVal);
            else
            {
                if (ItemVal.mVal.Count != labValList[uutNo].Count)
                     return;
                string failCode = string.Empty;
                for (int stepNo = 0; stepNo < labValList[uutNo].Count; stepNo++)
                {
                    EStepName stepName = ItemVal.mVal[stepNo].name;
                    double stepVal = ItemVal.mVal[stepNo].value;
                    string stepUnit = ItemVal.mVal[stepNo].unit;
                    string stepShow = stepVal.ToString("0.00") + stepUnit;
                    if (stepName == EStepName.IR)
                    {
                        if (stepVal >= 1000)
                            stepShow = "UUUUUU";
                    }
                    labValList[uutNo][stepNo].Text = stepShow;
                    if (ItemVal.mVal[stepNo].result == 0)
                        labValList[uutNo][stepNo].ForeColor = Color.Blue;
                    else
                    {
                        labValList[uutNo][stepNo].ForeColor = Color.Red;
                        failCode = ItemVal.mVal[stepNo].code;
                    }                                     
                }
                if (ItemVal.result == 0)
                {
                    labResultList[uutNo].Text = "PASS";
                    labResultList[uutNo].ForeColor = Color.Blue;
                }
                else
                {
                    labResultList[uutNo].Text = failCode;
                    labResultList[uutNo].ForeColor = Color.Red;
                }
            }        
        }
        #endregion
    }
}
