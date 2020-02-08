using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.KunX.BURNIN.Udc
{
    public partial class udcModelBase : UserControl
    {
        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);
        }
        #endregion

        #region 事件定义
        /// <summary>
        /// 按钮类型
        /// </summary>
        public enum EBtnNo
        {
            选机种,
            温度显示
        }
        /// <summary>
        /// 按钮事件
        /// </summary>
        public class COnBtnClickArgs : EventArgs
        {
            public readonly EBtnNo btnNo;
            public readonly int lPara = 0;
            public readonly int wPara = 0;
            public COnBtnClickArgs(EBtnNo btnNo, int lPara,int wPara)
            {
                this.btnNo = btnNo;
                this.lPara = lPara;
                this.wPara = wPara;
            }
        }
        /// <summary>
        /// 按钮事件
        /// </summary>
        public COnEvent<COnBtnClickArgs> OnBtnClick = new COnEvent<COnBtnClickArgs>();
        #endregion

        #region 构造函数
        public udcModelBase()
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

        #region 方法
        /// <summary>
        /// 设置按钮可视
        /// </summary>
        /// <param name="enable"></param>
        public void SetBtnEnable(bool enable)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetBtnEnable), enable);
            else
            {
                btnModel.Enabled = enable;
            }
        }
        /// <summary>
        /// 设置机种名
        /// </summary>
        /// <param name="modelName"></param>
        public void SetModelName(string modelName)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetModelName), modelName);
            else
            {
                labModel.Text = modelName;
            }
        }
        /// <summary>
        /// 设置老化时间(H)
        /// </summary>
        /// <param name="modelName"></param>
        public void SetBITime(double burnTime)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<double>(SetBITime), burnTime);
            else
            {
                labBITime.Text = burnTime.ToString("0.0") + "H";
            }
        }
        /// <summary>
        /// 设置温度
        /// </summary>
        /// <param name="modelName"></param>
        public void SetTemp(string temp)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetTemp), temp);
            else
            {
                labTSet.Text = temp;
            }
        }
        /// <summary>
        /// 读取温度
        /// </summary>
        /// <param name="modelName"></param>
        public void ReadTemp(double temp,Color color)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<double, Color>(ReadTemp), temp, color);
            else
            {
                labTRead.Text = temp.ToString("0.0");
                labTRead.ForeColor = color;
            }
        }
        /// <summary>
        /// 设置当前输入电压
        /// </summary>
        /// <param name="volt"></param>
        /// <param name="color"></param>
        public void ReadACVolt(int volt, Color color)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, Color>(ReadACVolt), volt, color);
            else
            {
                labACV.Text = volt.ToString();
                labACV.ForeColor = color;
            }
        }
        #endregion

        #region 面板回调函数
        private void udcModelBase_Load(object sender, EventArgs e)
        {

        }
        private void btnModel_Click(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(EBtnNo.选机种,0, 0));
        }
        private void labTRead_DoubleClick(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(EBtnNo.温度显示, 0, 0));
        }
        #endregion

    }
}
