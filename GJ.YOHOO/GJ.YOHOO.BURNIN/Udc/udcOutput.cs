using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.DEV.FCMB;

namespace GJ.YOHOO.BURNIN.Udc
{
    public partial class udcOutput : UserControl
    {
        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);
        }
        #endregion

        #region 构造函数
        public udcOutput()
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
            //机种规格
            labVName = new Label[CGlobalPara.C_OUTPUT_MAX];
            labISet = new Label[CGlobalPara.C_OUTPUT_MAX];
            labVSpec = new Label[CGlobalPara.C_OUTPUT_MAX];
            labISpec = new Label[CGlobalPara.C_OUTPUT_MAX];
            for (int i = 0; i < CGlobalPara.C_OUTPUT_MAX; i++)
            {
                labVName[i] = new Label();
                labVName[i].Dock = DockStyle.Fill;
                labVName[i].TextAlign = ContentAlignment.MiddleCenter;
                labVName[i].Margin = new Padding(0);
                labVName[i].Visible = false;
                labISet[i] = new Label();
                labISet[i].Dock = DockStyle.Fill;
                labISet[i].TextAlign = ContentAlignment.MiddleCenter;
                labISet[i].Margin = new Padding(0);
                labISet[i].Visible = false;
                labVSpec[i] = new Label();
                labVSpec[i].Dock = DockStyle.Fill;
                labVSpec[i].TextAlign = ContentAlignment.MiddleCenter;
                labVSpec[i].Margin = new Padding(0);
                labVSpec[i].Visible = false;
                labISpec[i] = new Label();
                labISpec[i].Dock = DockStyle.Fill;
                labISpec[i].TextAlign = ContentAlignment.MiddleCenter;
                labISpec[i].Margin = new Padding(0);
                labISpec[i].Visible = false;
                panel4.Controls.Add(labVName[i], 0, 1 + i);
                panel4.Controls.Add(labISet[i], 1, 1 + i);
                panel4.Controls.Add(labVSpec[i], 2, 1 + i);
                panel4.Controls.Add(labISpec[i], 3, 1 + i);
            }
        }
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            CUISetting.SetUIDoubleBuffered(this);
        }
        #endregion

        #region 面板控件
        /// <summary>
        /// 输出规格
        /// </summary>
        private Label[] labVName = null;
        /// <summary>
        /// 输出电流
        /// </summary>
        private Label[] labISet = null;
        /// <summary>
        /// 电压规格
        /// </summary>
        private Label[] labVSpec = null;
        /// <summary>
        /// 电流规格
        /// </summary>
        private Label[] labISpec = null;
        #endregion

        #region 面板回调函数
        private void udcOutput_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region 方法
        public void SetValue(CModelPara runModel)
        { 
           if(this.InvokeRequired)
               this.Invoke(new Action<CModelPara>(SetValue),runModel);
           else
           {
               for (int i = 0; i < CGlobalPara.C_OUTPUT_MAX; i++)
               {
                   if (i < labVName.Length)
                   {
                       labVName[i].Visible = false;
                       labISet[i].Visible = false;
                       labVSpec[i].Visible = false;
                       labISpec[i].Visible = false;
                   }
               }
               for (int i = 0; i < runModel.Para.OutPut_Num; i++)
               {
                   if (i < labVName.Length)
                   {
                       labVName[i].Text = ((EQCM)runModel.OutPut[i].Chan[0].QCType).ToString() + ":" +
                                          runModel.OutPut[i].Chan[0].QCV.ToString() + "V";
                       labVName[i].Visible = true;

                       labISet[i].Text = runModel.OutPut[i].Chan[0].ISet.ToString() + "A";
                       labISet[i].Visible = true;

                       labVSpec[i].Text = runModel.OutPut[i].Chan[0].Vmin.ToString() + "--" + runModel.OutPut[i].Chan[0].Vmax.ToString();
                       labVSpec[i].Visible = true;

                       labISpec[i].Text = runModel.OutPut[i].Chan[0].Imin.ToString() + "--" + runModel.OutPut[i].Chan[0].Imax.ToString();
                       labISpec[i].Visible = true;
                   }
               } 
           }
        }
        #endregion

    }
}
