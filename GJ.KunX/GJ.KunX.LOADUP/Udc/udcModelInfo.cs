using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.KunX.LOADUP.Udc
{
    public partial class udcModelInfo : UserControl
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
            次数归零,
            解除不良,
            设置空治具
        }
        /// <summary>
        /// 按钮事件
        /// </summary>
        public class COnBtnClickArgs : EventArgs
        {
            public readonly EBtnNo btnNo;
            public int value;
            public COnBtnClickArgs(EBtnNo btnNo, int value)
            {
                this.btnNo = btnNo;
                this.value = value;
            }
        }
        /// <summary>
        /// 按钮事件
        /// </summary>
        public COnEvent<COnBtnClickArgs> OnBtnClick = new COnEvent<COnBtnClickArgs>();
        #endregion

        #region 构造函数
        public udcModelInfo()
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
        }
        #endregion

        #region 面板回调函数
        private void btnSelect_Click(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(EBtnNo.选机种,0));
        }
        private void btnClr_Click(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(EBtnNo.解除不良,0));    
        }
       
        private void btnClrNum_Click(object sender, EventArgs e)
        {
            OnBtnClick.OnEvented(new COnBtnClickArgs(EBtnNo.次数归零,0));  
        }
        private void chkFixNull_CheckedChanged(object sender, EventArgs e)
        {
            int value = 0;

            if (chkFixNull.Checked)
                value = 1;

            OnBtnClick.OnEvented(new COnBtnClickArgs(EBtnNo.设置空治具, value)); 
        }
        #endregion

        #region 字段
        
        #endregion

        #region 属性
        #endregion

        #region 方法
        /// <summary>
        /// 显示机种信息
        /// </summary>
        /// <param name="modelPara"></param>
        public void ShowModel(CModelPara modelPara)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<CModelPara>(ShowModel), modelPara);
            else
            {
                labModel.Text = modelPara.Base.Model;
                labCustom.Text = modelPara.Base.Custom;
                labVersion.Text = modelPara.Base.Version;
                labACSpec.Text = modelPara.OutPut.ACVolt +  "V";
                labVSpec.Text = "[" + modelPara.OutPut.Vname + "]:" + modelPara.OutPut.Vmin.ToString() +
                                "V~" + modelPara.OutPut.Vmax.ToString() + "V";
                if (modelPara.OutPut.LoadMode == 0)
                    labISpec.Text = "[CC"+ CLanguage.Lan("模式") + "-" + modelPara.OutPut.LoadSet.ToString() + "A]:" + 
                                                 modelPara.OutPut.LoadMin.ToString() +
                                                "A~" + modelPara.OutPut.LoadMax.ToString() + "A";
                else if (modelPara.OutPut.LoadMode == 1)
                    labISpec.Text = "[CV"+CLanguage.Lan("模式")+"-" + modelPara.OutPut.LoadSet.ToString() + "V]:" + 
                                   modelPara.OutPut.LoadMin.ToString() +
                                  "A~" + modelPara.OutPut.LoadMax.ToString() + "A";  
                else
                    labISpec.Text = "[LED"+CLanguage.Lan("模式")+"-" + modelPara.OutPut.LoadSet.ToString() + "V]:" +
                                    modelPara.OutPut.LoadMin.ToString() +
                                  "A~" + modelPara.OutPut.LoadMax.ToString() + "A";  
            }
        }
        /// <summary>
        /// 设置治具使用次数和不良次数
        /// </summary>
        /// <param name="ttNum"></param>
        /// <param name="failNum"></param>
        public void SetFixUseNumAndFailNum(int ttNum, int failNum,string status="")
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, int, string>(SetFixUseNumAndFailNum), ttNum, failNum, status);
            else
            {
                labTTNum.Text = ttNum.ToString();
                labFailNum.Text = failNum.ToString();
                labStatus.ForeColor = Color.Blue;
                if (status == "")
                    labStatus.Text = CLanguage.Lan("检查治具状态OK.");
                else
                {
                    labStatus.Text = status;
                    labStatus.ForeColor = Color.Red;
                }
            }
        }
        /// <summary>
        /// 按钮启用
        /// </summary>
        /// <param name="enabled"></param>
        public void SetModelEnable(bool enabled)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetModelEnable), enabled);
            else
            {
                btnSelect.Enabled = enabled;
                chkFixNull.Enabled = enabled;
            }
        }
        /// <summary>
        /// 按钮启用
        /// </summary>
        /// <param name="enabled"></param>
        public void SetUIEnable(bool enabled)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetUIEnable), enabled);
            else
            {
                btnClrFail.Enabled = enabled;
            }
        }
        /// <summary>
        /// 设置清除治具次数
        /// </summary>
        /// <param name="enabled"></param>
        public void SetUseNumUI(bool enabled)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetUseNumUI), enabled);
            else
            {
                btnClrNum.Visible = enabled;
                this.Refresh(); 
            }
        }
        /// <summary>
        /// 设置清除不良次数
        /// </summary>
        /// <param name="enabled"></param>
        public void SetFailNumUI(bool enabled)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<bool>(SetFailNumUI), enabled);
            else
            {
                btnClrFail.Visible = enabled;
                labFaiiTitle.Visible = enabled;  
                labFailNum.Visible = enabled;
                this.Refresh(); 
            }
        }
        #endregion


    }
}
