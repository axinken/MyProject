using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.USER.APP;
using GJ.COM;
using GJ.UI;
using GJ.DEV.FCMB;

namespace GJ.KunX.BURNIN.Udc
{
    public partial class udcUUTInfo : Form
    {
        #region 构造函数
        public udcUUTInfo(CUUT runUUT)
        {
            this.runUUT = runUUT;

            this.idNo = runUUT.Base.uutNo - 1;

            InitializeComponent();

            IntialControl();

            SetDoubleBuffered();

        }
        #endregion

        #region 字段
        private int idNo = 0;
        private CUUT runUUT = null;
        private int runNowTime = 0;
        #endregion

        #region 初始化
        /// <summary>
        /// 绑定控件
        /// </summary>
        private void IntialControl()
        {
            for (int i = 0; i < CKunXApp.SlotMax; i++)
            {
                Label labId = new Label();
                labId.Name = "labId" + i.ToString();
                labId.Dock = DockStyle.Fill;
                labId.Margin = new Padding(3);
                labId.Font = new Font("宋体", 10);
                labId.TextAlign = ContentAlignment.MiddleCenter;
                labId.Text = CLanguage.Lan("产品") + (i + 1).ToString("D2") + ":";
                labIdList.Add(labId);

                Label labSn = new Label();
                labSn.Name = "labSn" + i.ToString();
                labSn.Dock = DockStyle.Fill;
                labSn.Margin = new Padding(3);
                labSn.Font = new Font("宋体", 10);
                labSn.TextAlign = ContentAlignment.MiddleCenter;
                labSn.Text = "";
                labSnList.Add(labSn);

                Label labResult = new Label();
                labResult.Name = "labResult" + i.ToString();
                labResult.Dock = DockStyle.Fill;
                labResult.Margin = new Padding(3);
                labResult.Font = new Font("宋体", 10);
                labResult.TextAlign = ContentAlignment.MiddleCenter;
                labResult.Text = "";
                labResultList.Add(labResult);

                Label labVolt = new Label();
                labVolt.Name = "labVolt" + i.ToString();
                labVolt.Dock = DockStyle.Fill;
                labVolt.Margin = new Padding(3);
                labVolt.Font = new Font("宋体", 10);
                labVolt.TextAlign = ContentAlignment.MiddleCenter;
                labVolt.Text = "";
                labVoltList.Add(labVolt);

                Label labCur = new Label();
                labCur.Name = "labCur" + i.ToString();
                labCur.Dock = DockStyle.Fill;
                labCur.Margin = new Padding(3);
                labCur.Font = new Font("宋体", 10);
                labCur.TextAlign = ContentAlignment.MiddleCenter;
                labCur.Text = "";
                labCurList.Add(labCur);

                panel2.Controls.Add(labIdList[i], 0, i + 1);
                panel2.Controls.Add(labSnList[i], 1, i + 1);
                panel2.Controls.Add(labResultList[i], 2, i + 1);
                panel2.Controls.Add(labVoltList[i], 3, i + 1);
                panel2.Controls.Add(labCurList[i], 4, i + 1);
            }

            _udcChart.Dock = DockStyle.Fill;
            panel4.Controls.Add(_udcChart, 0, 0);   
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
        private List<Label> labIdList = new List<Label>();
        private List<Label> labSnList = new List<Label>();
        private List<Label> labResultList = new List<Label>();
        private List<Label> labVoltList = new List<Label>();
        private List<Label> labCurList = new List<Label>();
        private udcChartOnOff _udcChart = new udcChartOnOff(); 
        #endregion

        #region 面板回调函数
        private void udcUUTInfo_Load(object sender, EventArgs e)
        {
            lablocalName.Text = runUUT.Base.localName;
            labModel.Text = runUUT.Para.ModelName;
            labStartTime.Text = runUUT.Para.StartTime;
            labIdCard.Text = runUUT.Para.IdCard;
            labBITime.Text = (((double)runUUT.Para.BurnTime) / 3600).ToString("0.0") + "H";            
            DateTime endTime;
            if (runUUT.Para.StartTime == "")
                labEndTime.Text = "";
            else
            {
                endTime = (System.Convert.ToDateTime(runUUT.Para.StartTime)).AddSeconds(runUUT.Para.BurnTime);
                labEndTime.Text = endTime.ToString("yyyy/MM/dd HH:mm:ss");
            }
            runNowTime = runUUT.Para.RunTime;
            TimeSpan ts = new TimeSpan(0, 0, runNowTime);
            TimeSpan tl = new TimeSpan(0, 0, runUUT.Para.BurnTime - runNowTime);
            string runTime = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
            string leftTime = tl.Days.ToString("D2") + ":" + tl.Hours.ToString("D2") + ":" + tl.Minutes.ToString("D2") + ":" + tl.Seconds.ToString("D2");
            labRunTime.Text = runTime;
            labLeftTime.Text = leftTime;

            string status = string.Empty;

            if (runUUT.Para.CtrlACON == 1)
                status = runUUT.OnOff.TimeRun.CurRunVolt  +":AC ON;";
            else
                status = runUUT.OnOff.TimeRun.CurRunVolt + ":AC OFF;";

            if (runUUT.Para.CtrlOnOff == 1)
            {
                labOnOff.Text = status + ":AC ON";
                labOnOff.ForeColor = Color.Blue;
            }
            else
            {
                labOnOff.Text = status + ":AC OFF";
                labOnOff.ForeColor = Color.Black;
            }
            if (runUUT.Para.AlarmCode == EAlarmCode.正常)
            {
                if (runUUT.Para.DoRun == EDoRun.正在老化)
                {
                    labDoRun.Text = "【" + runUUT.Para.DoRun.ToString() + "】：" +
                                    "【" + ((EQCM)runUUT.OnOff.TimeRun.CurQCType).ToString() + "-" +
                                           runUUT.OnOff.TimeRun.CurQCV.ToString() +  "V】";
                }
                else
                {
                    labDoRun.Text = runUUT.Para.DoRun.ToString();
                }
                labDoRun.ForeColor = Color.Blue;
            }
            else
            {
                labDoRun.Text = runUUT.Para.AlarmCode.ToString();
                labDoRun.ForeColor = Color.Red;
            }
            int iCom=runUUT.Base.ctrlCom;
            int iAddr=runUUT.Base.ctrlAddr;
            labDevCtrl.Text = "【" + CGlobalPara.SysPara.Dev.MonCom[iCom] + "-" + iAddr.ToString("D2") + "】";

            int ersCom = runUUT.Base.ersCom;
            int ersAddr = runUUT.Base.ersAddr;
            int ersCH = runUUT.Base.ersCH;
            labDevERS.Text = "【" + CGlobalPara.SysPara.Dev.ErsCom[ersCom] + "-" + ersAddr.ToString("D2") + 
                                                                             "-" + ersCH.ToString("D2") + "】"; 
            for (int i = 0; i < CKunXApp.SlotMax; i++)
            {
                if (runUUT.Led[i].serialNo == "")
                {
                    labSnList[i].Text = "----";
                    labSnList[i].ForeColor = Color.Black;
                    labResultList[i].Text = "----";
                    labResultList[i].ForeColor = Color.Black;
                    labVoltList[i].Text = "----";
                    labVoltList[i].ForeColor = Color.Black;
                    labCurList[i].Text = "----";
                    labCurList[i].ForeColor = Color.Black;
                }
                else
                {
                    labSnList[i].Text = runUUT.Led[i].serialNo;
                    labSnList[i].ForeColor = Color.Black;
                    labVoltList[i].Text = runUUT.Led[i].unitV.ToString("0.000");
                    labCurList[i].Text = runUUT.Led[i].unitA.ToString("0.00");
                    double volt = System.Convert.ToDouble(labVoltList[i].Text);
                    double current = System.Convert.ToDouble(labCurList[i].Text);
                    if (runUUT.Led[i].passResult == 0)
                    {
                        labResultList[i].Text = "PASS";
                        labResultList[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labResultList[i].Text = "FAIL";
                        labResultList[i].ForeColor = Color.Red;
                    }
                    if (runUUT.Led[i].unitV >= runUUT.Led[i].vMin && runUUT.Led[i].unitV <= runUUT.Led[i].vMax)
                    {
                        labVoltList[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labVoltList[i].ForeColor = Color.Red;
                    }
                    if (runUUT.Led[i].unitA >= runUUT.Led[i].Imin && runUUT.Led[i].unitA <= runUUT.Led[i].Imax)
                    {
                        labCurList[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labCurList[i].ForeColor = Color.Red;
                    }
                }
            }
            refreshOnOff();

            if (CGlobalPara.C_RUNNING)
            {
               if(runUUT.Para.DoRun == EDoRun.正在老化 || runUUT.Para.DoRun == EDoRun.老化结束)
               {
                   timer1.Enabled = true;
               }
            }               
        }
        private void udcUUTInfo_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            refreshRunTimeUI();
            refreshData();
            UIRefresh.OnEvented(new CUIRefreshArgs(idNo, ref runUUT));
        }
        private void refreshRunTimeUI()
        {
            runNowTime = _udcChart.runTimes;

            TimeSpan ts = new TimeSpan(0, 0, runNowTime);
            TimeSpan tl = new TimeSpan(0, 0, runUUT.Para.BurnTime - runNowTime);
            string runTime = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
            string leftTime = tl.Days.ToString("D2") + ":" + tl.Hours.ToString("D2") + ":" + tl.Minutes.ToString("D2") + ":" + tl.Seconds.ToString("D2");
            labRunTime.Text = runTime;
            labLeftTime.Text = leftTime;
        }
        private void refreshData()
        {
            for (int i = 0; i < CKunXApp.SlotMax; i++)
            {
                if (runUUT.Led[i].serialNo == "")
                {
                    labSnList[i].Text = "----";
                    labSnList[i].ForeColor = Color.Black;
                    labResultList[i].Text = "----";
                    labResultList[i].ForeColor = Color.Black;
                    labVoltList[i].Text = "----";
                    labVoltList[i].ForeColor = Color.Black;
                    labCurList[i].Text = "----";
                    labCurList[i].ForeColor = Color.Black;
                }
                else
                {
                    labSnList[i].Text = runUUT.Led[i].serialNo;
                    labSnList[i].ForeColor = Color.Black;
                    labVoltList[i].Text = runUUT.Led[i].unitV.ToString("0.000");
                    labVoltList[i].ForeColor = Color.Black;
                    labCurList[i].Text = runUUT.Led[i].unitA.ToString("0.00");
                    labCurList[i].ForeColor = Color.Black;
                    double volt = System.Convert.ToDouble(labVoltList[i].Text);
                    double current = System.Convert.ToDouble(labCurList[i].Text);
                    if (runUUT.Led[i].passResult == 0)
                    {
                        labResultList[i].Text = "PASS";
                        labResultList[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labResultList[i].Text = "FAIL";
                        labResultList[i].ForeColor = Color.Red;
                    }
                    if (runUUT.Led[i].unitV >= runUUT.Led[i].vMin && runUUT.Led[i].unitV <= runUUT.Led[i].vMax)
                    {
                        labVoltList[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labVoltList[i].ForeColor = Color.Red;
                    }
                    if (runUUT.Led[i].unitA >= runUUT.Led[i].Imin && runUUT.Led[i].unitA <= runUUT.Led[i].Imax)
                    {
                        labCurList[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        labCurList[i].ForeColor = Color.Red;
                    }
                }
            }
        }
        private void refreshOnOff()
        {
            int acv = 0;

            _udcChart.biTime = ((double)runUUT.Para.BurnTime) / 3600;

            List<udcChartOnOff.COnOff> itemList = new List<udcChartOnOff.COnOff>();

            for (int i = 0; i < runUUT.OnOff.OnOff.Count; i++)
            {

                udcChartOnOff.COnOff onoff = new udcChartOnOff.COnOff();

                onoff.curVolt= runUUT.OnOff.OnOff[i].ACV;

                onoff.onoffTimes = runUUT.OnOff.OnOff[i].OnOffTime;

                onoff.onTimes = runUUT.OnOff.OnOff[i].OnTime;

                onoff.offTimes = runUUT.OnOff.OnOff[i].OffTime;

                itemList.Add(onoff);

                if (acv < onoff.curVolt)
                {
                    acv = onoff.curVolt;
                }
            }

            _udcChart.maxVolt = 220;

            _udcChart.onoff = itemList;

            _udcChart.Refresh();

            if (CGlobalPara.C_RUNNING)
            {
                if (runUUT.Para.DoRun == EDoRun.正在老化 || runUUT.Para.DoRun == EDoRun.老化结束)
                {
                    _udcChart.startRun(runUUT.Para.RunTime, runUUT.Para.IniRunTime);
                }
            }           
        }
        #endregion

        #region 事件定义
        public class CUIRefreshArgs : EventArgs
        {
            public readonly int idNo;
            public CUUT runUUT;
            public CUIRefreshArgs(int idNo, ref CUUT runUUT)
            {
                this.idNo = idNo;
                this.runUUT = runUUT;
            }
        }
        public COnEvent<CUIRefreshArgs> UIRefresh = new COnEvent<CUIRefreshArgs>();
        #endregion
    }
}
