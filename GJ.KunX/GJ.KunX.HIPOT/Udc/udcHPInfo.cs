using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GJ.COM;
namespace GJ.KunX.HIPOT.Udc
{
    public partial class udcHPInfo : UserControl
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

        #region 枚举
        public enum ERun
        {
            Idle,
            Initialize,
            Ready,
            Testing,
            Debug,
            Pass,
            Fail,
            Error
        }
        #endregion

        #region 构造函数
        public udcHPInfo()
        {
            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();
        }
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

        #region 面板回调函数
        private void udcHPInfo_Load(object sender, EventArgs e)
        {
            
        }
        private void btnSelectModel_Click(object sender, EventArgs e)
        {
            OnBtnArgs.OnEvented(new COnBtnClickArgs(2,0)); 
        }
        private void btnClrNum_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确定要清除测试数量统计?"), "Tip", MessageBoxButtons.YesNo,                              
                              MessageBoxIcon.Question) == DialogResult.Yes)
           {
               OnBtnArgs.OnEvented(new COnBtnClickArgs(3, 0)); 
           }
        }
        private void btnLft_Click(object sender, EventArgs e)
        {
            if (btnLft.Text == CLanguage.Lan("调试") + "(&L)")
            {
                OnBtnArgs.OnEvented(new COnBtnClickArgs(0, 1));
            }
            else
            {
                OnBtnArgs.OnEvented(new COnBtnClickArgs(0, 0));
            }
             
        }
        private void btnRgt_Click(object sender, EventArgs e)
        {
            if (btnRgt.Text == CLanguage.Lan("调试") + "(&R)")
            {
                OnBtnArgs.OnEvented(new COnBtnClickArgs(1, 1));
            }
            else
            {
                OnBtnArgs.OnEvented(new COnBtnClickArgs(1, 0));
            }
        }
        #endregion

        #region 事件
        public class COnBtnClickArgs : EventArgs
        {
            public readonly int idNo;
            public readonly int run;
            public COnBtnClickArgs(int idNo,int run)
            {
                this.idNo = idNo;
                this.run = run;
            }
        }
        public COnEvent<COnBtnClickArgs> OnBtnArgs = new COnEvent<COnBtnClickArgs>(); 
        #endregion

        #region 共享方法
        /// <summary>
        /// 加载机种信息
        /// </summary>
        /// <param name="model"></param>
        public void LoadModel(CModelPara runModel)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CModelPara>(LoadModel), runModel);
            else
            {

                labModel.Text = runModel.Base.Model;

                labCustom.Text = runModel.Base.Custom;

                labVersion.Text = runModel.Base.Version;

                labItemNum.Text = runModel.Para.Step.Count.ToString();

            }
        }
        /// <summary>
        /// 设置数量统计
        /// </summary>
        /// <param name="ttNum"></param>
        /// <param name="failNum"></param>
        public void SetYield(int ttNum, int failNum)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int,int>(SetYield), ttNum, failNum);
            else
            {
                labTTNum.Text = ttNum.ToString();
                labFailNum.Text = failNum.ToString();
                labPassNum.Text = (ttNum - failNum).ToString();
                if (ttNum == 0)
                    labPassRate.Text = "100.00%";
                else
                    labPassRate.Text = ((double)(ttNum - failNum) / (double)ttNum).ToString("P2");
            }
        }
        /// <summary>
        /// 设置运行状态
        /// </summary>
        /// <param name="runStatus"></param>
        public void SetStatus(ERun runStatus)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<ERun>(SetStatus), runStatus);
            else
            {
                labStatus.Text = runStatus.ToString();
                btnSelectModel.Enabled = false;
                btnLft.Enabled = false;
                btnRgt.Enabled = false;
                switch (runStatus)
                {
                    case ERun.Idle:
                        labStatus.ForeColor = Color.Red;
                        labStatus.Font = new Font("宋体", 80f, FontStyle.Bold);
                        btnSelectModel.Enabled = true;
                        btnLft.Enabled = true;
                        btnRgt.Enabled = true;
                        break;
                    case ERun.Initialize:
                        labStatus.ForeColor = Color.Red;
                        labStatus.Font = new Font("宋体", 40f, FontStyle.Bold);
                        break;
                    case ERun.Ready:
                        labStatus.ForeColor = Color.Blue;
                        labStatus.Font = new Font("宋体", 72f, FontStyle.Bold);
                        break;
                    case ERun.Testing:
                        labStatus.ForeColor = Color.Blue;
                        labStatus.Font = new Font("宋体", 60f, FontStyle.Bold);
                        break;
                    case ERun.Debug:
                        labStatus.ForeColor = Color.Red;
                        labStatus.Font = new Font("宋体", 60f, FontStyle.Bold);
                        btnLft.Enabled = true;
                        btnRgt.Enabled = true;
                        break;
                    case ERun.Pass:
                        labStatus.ForeColor = Color.Green;
                        labStatus.Font = new Font("宋体", 90f, FontStyle.Bold);
                        break;
                    case ERun.Fail:
                        labStatus.ForeColor = Color.Red;
                        labStatus.Font = new Font("宋体", 90f, FontStyle.Bold);
                        break;
                    case ERun.Error:
                        labStatus.ForeColor = Color.Red;
                        labStatus.Font = new Font("宋体", 72f, FontStyle.Bold);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 设置测试时间
        /// </summary>
        /// <param name="timeMs"></param>
        public void SetTimes(long timeMs)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<long>(SetTimes), timeMs);
            else
            {
                double testTimes = (double)timeMs / 1000;
                labTestTimes.Text = testTimes.ToString("0.0") + "s";    
            }
        }
        /// <summary>
        /// 设置调式按钮状态
        /// </summary>
        /// <param name="idNo"></param>
        /// <param name="run"></param>
        public void SetDebugBtn(int idNo, int run)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int,int>(SetDebugBtn), idNo, run);
            else
            {
               if(idNo==0)
               {
                   if (run==1)
                       btnLft.Text = CLanguage.Lan("停止") + "(&L)";                   
                   else
                       btnLft.Text = CLanguage.Lan("调试") + "(&L)";
               }
               else
               {
                   if (run == 1)
                       btnRgt.Text = CLanguage.Lan("停止") + "(&R)";
                   else
                       btnRgt.Text = CLanguage.Lan("调试") + "(&R)"; 
               }
            }
        }
        /// <summary>
        /// 设置是否连线
        /// </summary>
        /// <param name="runFlag"></param>
        public void SetMesStatus(bool runFlag)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetMesStatus), runFlag);
            else
            {
                if (runFlag)
                {
                    labMesStatus.BackColor = Color.Lime;
                    labMesStatus.Text = CLanguage.Lan("连接MES"); 
                }
                else
                {
                    labMesStatus.BackColor = Control.DefaultBackColor;
                    labMesStatus.Text = CLanguage.Lan("未连接MES"); 
                }
            }
        }
        /// <summary>
        /// 设置选机种
        /// </summary>
        /// <param name="enable"></param>
        public void SetModelEnable(bool enable)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetModelEnable), enable);
            else
            {
                btnSelectModel.Enabled = enable;
            }
        }
        #endregion

    }
}
