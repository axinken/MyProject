using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
namespace GJ.YOHOO.ATE.Udc
{
    public partial class udcStatInfo : UserControl
    {
        #region 构造函数
        public udcStatInfo()
        {
            InitializeComponent();

            SetDoubleBuffered();
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
            panel1.GetType().GetProperty("DoubleBuffered",
                                          System.Reflection.BindingFlags.Instance |
                                          System.Reflection.BindingFlags.NonPublic)
                                          .SetValue(panel1, true, null);
        }
        #endregion

        #region 方法
        /// <summary>
        /// 显示设定值
        /// </summary>
        /// <param name="serName"></param>
        /// <param name="ateTilte"></param>
        public void ShowSetting(string serName,string ateTilte)
        { 
          if(this.InvokeRequired)
              this.Invoke(new Action<string, string>(ShowSetting), serName, ateTilte);
          else
          {
              labTcpIP.Text = serName;
              labATETitle.Text = ateTilte;  
          }
        }
        /// <summary>
        /// 显示ATE捕捉信息
        /// </summary>
        public void SetATEInfo(string proName,string modelName,string testTimes)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, string, string>(SetATEInfo), proName, modelName, testTimes);
            else
            {
                labATEPrgName.Text = proName;
                labATEModel.Text = modelName;
                labATETestTimes.Text = testTimes;    
            }
        }
        /// <summary>
        /// TCP状态
        /// </summary>
        /// <param name="status"></param>
        /// <param name="bFail"></param>
        public void SetTCPStatus(string status, bool bFail=false)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetTCPStatus), status, bFail);
            else
            {
                labTcpStatus.Text = status;
                if (bFail)
                    labTcpStatus.ForeColor = Color.Red;
                else
                    labTcpStatus.ForeColor = Color.Blue;
            }
        }
        /// <summary>
        /// 设置ATE状态
        /// </summary>
        /// <param name="status"></param>
        /// <param name="bFail"></param>
        public void SetATEStatus(string status,bool bFail=true)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetATEStatus), status, bFail);
            else
            {
                if (bFail)
                {
                    labATEStatus.Text = status;
                    labATEStatus.ForeColor = Color.Red;
                }
                else
                {
                    labATEStatus.Text = status;
                    labATEStatus.ForeColor = Color.Blue;
                }
            }
        }
        /// <summary>
        /// 空闲
        /// </summary>
        public void SetFree()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetFree));
            else
            {
                labStat.Text = CLanguage.Lan("等待治具就绪.");
                labStat.ForeColor = Color.Black;
            }
        }
        /// <summary>
        /// 就绪
        /// </summary>
        /// <param name="idNo"></param>
        public void SetReady(string  statName)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string>(SetReady), statName);
            else
            {
                labStat.Text = statName;
                labStat.ForeColor = Color.Blue;
            }
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(string status,bool bFail=false)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, bool>(SetStatus), status, bFail);
            else
            {
                labStat.Text = status;
                if (bFail)
                    labStat.ForeColor = Color.Red;
                else
                    labStat.ForeColor = Color.Blue;
            }
        }
        /// <summary>
        /// 测试时间
        /// </summary>
        /// <param name="timeMs"></param>
        public void SetTimes(long timeMs)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<long>(SetTimes), timeMs);
            else
            {
                double testTimes = (double)timeMs / 1000;
                labRunTimes.Text = testTimes.ToString("0.0") + "s";
            }
        }
        #endregion

    }
}
