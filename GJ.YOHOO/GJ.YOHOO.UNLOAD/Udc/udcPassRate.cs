using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.YOHOO.UNLOAD.Udc
{
    public partial class udcPassRate : UserControl
    {

        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);
        }
        #endregion

        #region 构造函数
        public udcPassRate(int idNo,string name)
        {
            InitializeComponent();

            InitialControl();

            SetDoubleBuffered();

            this.idNo = idNo;

            this.name = name;
        }
        public override string ToString()
        {
            return "<" + name + ">";
        }
        #endregion

        #region 字段
        private int idNo = 0;
        private string name = string.Empty;
        #endregion

        #region 事件定义
        /// <summary>
        /// 按钮事件
        /// </summary>
        public class COnBtnClickArgs : EventArgs
        {
            public readonly int idNo = 0;
            public readonly int lPara = 0;
            public readonly int wPara = 0;
            public COnBtnClickArgs(int idNo, int lPara, int wPara)
            {
                this.idNo = idNo;
                this.lPara = lPara;
                this.wPara = wPara;
            }
        }
        /// <summary>
        /// 按钮事件
        /// </summary>
        public COnEvent<COnBtnClickArgs> OnBtnClick = new COnEvent<COnBtnClickArgs>();
        #endregion

        #region 面板初始化
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
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
        private void udcYieldLock_Load(object sender, EventArgs e)
        {
            labTitle.Text = name;
        }
        private void BtnZero_Click(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(idNo, 0, 0)); 
        }
        private void btnCfgAlarm_Click(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(idNo, 1, 0)); 
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置良率
        /// </summary>
        /// <param name="ttNum"></param>
        /// <param name="passNum"></param>
        public void SetYield(CWarnRate e)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CWarnRate>(SetYield), e);
            else
            {
                double passRate = 1;
                if (e.TTNum != 0)
                {
                    passRate = ((double)e.PassNum) / ((double)e.TTNum); 
                }                
                labPassRateLimit.Text = e.PassRateLimit.ToString("P2");
                labTTNum.Text = e.TTNum.ToString();
                labPassNum.Text = e.PassNum.ToString();
                labPassRate.Text = passRate.ToString("P2");
                if (e.bAlarm==1)
                {
                    btnCfgAlarm.Visible = true;
                }
                else if (e.bAlarm == -1)
                {
                    btnCfgAlarm.Visible = false;
                }
                switch (e.DoRun)
                {
                    case EWarnResult.空闲:
                        labStatus.Text = CLanguage.Lan("空闲");
                        labStatus.BackColor = Color.White;
                        break;
                    case EWarnResult.未启动:
                        labStatus.Text = CLanguage.Lan("未启用");
                        labStatus.BackColor = Color.White;
                        break;
                    case EWarnResult.正常:
                        labStatus.Text = CLanguage.Lan("良率正常");
                        labStatus.BackColor = Color.Lime;
                        break;
                    case EWarnResult.报警:
                        labStatus.Text = CLanguage.Lan("良率偏低");
                        labStatus.BackColor = Color.Red;
                        break;
                    case EWarnResult.确认报警:
                        break;
                    default:
                        break;
                }         

            }
        }
        #endregion

    }
}
