using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using GJ.COM;
using GJ.PLUGINS;
using GJ.APP;
using System.IO;

namespace GJ.KunX.LOADUP
{
    public partial class FrmSysPara : Form,IChildMsg
    {
        #region 插件方法
        /// <summary>
        /// 父窗口
        /// </summary>
        private Form _father = null;
        /// <summary>
        /// 父窗口唯一标识
        /// </summary>
        private string _fatherGuid = string.Empty;
        /// <summary>
        /// 加载当前窗口及软件版本日期
        /// </summary>
        /// <param name="fatherForm"></param>
        /// <param name="control"></param>
        /// <param name="guid"></param>
        public void OnShowDlg(Form fatherForm, Control control, string guid)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<Form, Control, string>(OnShowDlg), fatherForm, control, guid);
            else
            {
                this._father = fatherForm;
                this._fatherGuid = guid;

                this.Dock = DockStyle.Fill;
                this.TopLevel = false;
                this.FormBorderStyle = FormBorderStyle.None;
                control.Controls.Add(this);
                this.Show();
            }
        }
        /// <summary>
        /// 关闭当前窗口 
        /// </summary>
        public void OnCloseDlg()
        {

        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="mPwrLevel"></param>
        public void OnLogIn(string user, int[] mPwrLevel)
        {

        }
        /// <summary>
        /// 启动监控
        /// </summary>
        public void OnStartRun()
        {


        }
        /// <summary>
        /// 停止监控
        /// </summary>
        public void OnStopRun()
        {


        }
        /// <summary>
        /// 中英文切换
        /// </summary>
        public void OnChangeLAN()
        {
            SetUILanguage();
        }
        /// <summary>
        /// 消息响应
        /// </summary>
        /// <param name="para"></param>
        public void OnMessage(string name, int lPara, int wPara)
        {

        }
        #endregion

        #region 语言设置
        /// <summary>
        /// 设置中英文界面
        /// </summary>
        private void SetUILanguage()
        {
            GJ.COM.CLanguage.SetLanguage(this);

            switch (GJ.COM.CLanguage.languageType)
            {
                case CLanguage.EL.中文:
                    break;
                case CLanguage.EL.英语:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 构造函数
        public FrmSysPara()
        {
            InitializeComponent();

            SetDoubleBuffered();

            InitialControl();

        }
        #endregion

        #region  初始化方法
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

            CUISetting.SetUIDoubleBuffered(this);
        }
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitialControl()
        {

        }
        #endregion

        #region 面板控件

        #endregion

        #region 面板回调函数
        private void FrmSysPara_Load(object sender, EventArgs e)
        {
            try
            {
                string[] comArray = SerialPort.GetPortNames();
                cmbIdCom.Items.Clear();
                cmbEloadCom.Items.Clear();
                cmbFCMB.Items.Clear();
                cmbMutiCom.Items.Clear();
                cmbSnCom1.Items.Clear();
                cmbSnCom2.Items.Clear();
                for (int i = 0; i < comArray.Length; i++)
                {
                    cmbIdCom.Items.Add(comArray[i]);
                    cmbEloadCom.Items.Add(comArray[i]);
                    cmbFCMB.Items.Add(comArray[i]);
                    cmbMutiCom.Items.Add(comArray[i]);
                    cmbSnCom1.Items.Add(comArray[i]);
                    cmbSnCom2.Items.Add(comArray[i]);
                }

                cmbSnMode.Items.Clear();
                cmbSnMode.Items.Add(CLanguage.Lan("无条码模式"));
                cmbSnMode.Items.Add(CLanguage.Lan("自动扫描模式"));
                cmbSnMode.Items.Add(CLanguage.Lan("人工扫描模式"));
                cmbSnMode.Items.Add(CLanguage.Lan("人工串口模式"));           

                txtScanTimes.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
                txtACDelay.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
                txtReTestTimes.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
                txtStatTimes.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
                txtFixTimes.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
                txtSaveTimes.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
                txtSnLen.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

                load();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(CLanguage.Lan("确定要保存系统参数设置?"), "Tip", MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    save();

                    string er = string.Empty;

                    CReflect.SendWndMethod(_father, EMessType.OnMessage, out er, new object[] { "btnOK", (int)ElPara.保存, 0 });

                    CUserApp.OnUserArgs.OnEvented(new CUserArgs("FrmSysPara", (int)ElPara.保存, 0));
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            string er = string.Empty;

            CReflect.SendWndMethod(_father, EMessType.OnMessage, out er, new object[] { "btnExit", (int)ElPara.退出, 0 });
        }
        private void btnModel_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtModelPath.Text = dlg.SelectedPath;  
        }
        private void btnReport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtReportPath.Text = dlg.SelectedPath;  
        }
        private void btnDailyFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtDailyFolder.Text = dlg.SelectedPath;
        }
        private void OnTextKeyPressIsNumber(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)'.')
                e.Handled = true;
        }
        #endregion

        #region 方法
        private void load()
        {
            try
            {
                txtPlcIP.Text = CGlobalPara.SysPara.Dev.PlcIP;
                cmbIdCom.Text = CGlobalPara.SysPara.Dev.IdCom;
                cmbEloadCom.Text = CGlobalPara.SysPara.Dev.EloadCom;
                cmbFCMB.Text = CGlobalPara.SysPara.Dev.FCMBCom; 
                cmbMutiCom.Text = CGlobalPara.SysPara.Dev.MeterCom;
                cmbSnCom1.Text = CGlobalPara.SysPara.Dev.SnCom[0];
                cmbSnCom2.Text = CGlobalPara.SysPara.Dev.SnCom[1];
                txtSnBaud.Text = CGlobalPara.SysPara.Dev.SnBaud;
                cmbScanner.Text = CGlobalPara.SysPara.Dev.SnMode.ToString(); 
                txtScanTimes.Text = CGlobalPara.SysPara.Dev.MonInterval.ToString();

                txtACDelay.Text = CGlobalPara.SysPara.Para.AcDelayTimes.ToString();
                txtReTestTimes.Text = CGlobalPara.SysPara.Para.ReTestTimes.ToString();
                txtReDelayMs.Text = CGlobalPara.SysPara.Para.ReTestDelay.ToString();  
                txtIdTimes.Text = CGlobalPara.SysPara.Para.IdReTimes.ToString();
                txtReadSnNum.Text = CGlobalPara.SysPara.Para.ReadSnTimes.ToString();
                chkFailWait.Checked = CGlobalPara.SysPara.Para.ChkFailWait;
                chkVSenor.Checked = CGlobalPara.SysPara.Para.ChkVSenor;
                chkACON.Checked = CGlobalPara.SysPara.Para.ChkACON;
                chkIdleLoad.Checked = CGlobalPara.SysPara.Para.ChkIdleLoad;
                txtIdleLoad.Text = CGlobalPara.SysPara.Para.IdleLoad.ToString();

                txtStatTimes.Text = CGlobalPara.SysPara.Alarm.StatTimes.ToString();
                txtStatFailNum.Text = CGlobalPara.SysPara.Alarm.StatFailTimes.ToString();
                txtStatPassRate.Text = CGlobalPara.SysPara.Alarm.StatPassRate.ToString();  
                txtFixTimes.Text = CGlobalPara.SysPara.Alarm.FixtureTimes.ToString();
                txtFixFailTimes.Text = CGlobalPara.SysPara.Alarm.FixFailTimes.ToString();
                txtFixPassRate.Text = CGlobalPara.SysPara.Alarm.FixPassRate.ToString();
                chkPassRate.Checked = CGlobalPara.SysPara.Alarm.ChkPassRate;
                txtPassRateLimit.Text = (CGlobalPara.SysPara.Alarm.PassRateLimit*100).ToString();
                txtStartNumber.Text = CGlobalPara.SysPara.Alarm.PassRateStartNum.ToString();
                txtPassRateInterval.Text = CGlobalPara.SysPara.Alarm.PassRateAlarmTime.ToString();
                chkClrDay.Checked = CGlobalPara.SysPara.Alarm.ChkClrDay;
                chkClrNight.Checked = CGlobalPara.SysPara.Alarm.ChkClrNight;
                txtClrDay.Text = CGlobalPara.SysPara.Alarm.ClrDayTime;
                txtClrNight.Text = CGlobalPara.SysPara.Alarm.ClrNightTime;

                txtModelPath.Text = CGlobalPara.SysPara.Report.ModelPath;
                chkSaveReport.Checked = CGlobalPara.SysPara.Report.SaveReport;
                txtSaveTimes.Text = CGlobalPara.SysPara.Report.SaveReportTimes.ToString();
                txtReportPath.Text = CGlobalPara.SysPara.Report.ReportPath;
                txtDailyFolder.Text = CGlobalPara.SysPara.Report.DailyFolder;

                chkGJWeb.Checked = CGlobalPara.SysPara.Mes.ChkWebSn == 1 ? true : false;
                chkMesCon.Checked = CGlobalPara.SysPara.Mes.Connect;
                chkTranData.Checked = CGlobalPara.SysPara.Mes.TranDataToMes;
                cmbSnMode.SelectedIndex = (int)CGlobalPara.SysPara.Mes.SnMode; 
                txtSnLen.Text = CGlobalPara.SysPara.Mes.SnLen.ToString();
                txtSnSpec.Text = CGlobalPara.SysPara.Mes.SnSpec;
                chkBISn.Checked = CGlobalPara.SysPara.Mes.ChkSnBI;
                chkHPSn.Checked = CGlobalPara.SysPara.Mes.ChkSnHP;
                chkATESn.Checked = CGlobalPara.SysPara.Mes.ChkSnATE;
          
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void save()
        {
            try
            {

                CGlobalPara.SysPara.Dev.PlcIP=txtPlcIP.Text;
                CGlobalPara.SysPara.Dev.IdCom=cmbIdCom.Text;
                CGlobalPara.SysPara.Dev.EloadCom=cmbEloadCom.Text;
                CGlobalPara.SysPara.Dev.FCMBCom = cmbFCMB.Text; 
                CGlobalPara.SysPara.Dev.MeterCom=cmbMutiCom.Text;   
                CGlobalPara.SysPara.Dev.SnCom[0] = cmbSnCom1.Text;
                CGlobalPara.SysPara.Dev.SnCom[1] = cmbSnCom2.Text;
                CGlobalPara.SysPara.Dev.SnMode = cmbScanner.Text;
                CGlobalPara.SysPara.Dev.SnBaud = txtSnBaud.Text; 
                CGlobalPara.SysPara.Dev.MonInterval=System.Convert.ToInt16(txtScanTimes.Text);

                CGlobalPara.SysPara.Para.AcDelayTimes = System.Convert.ToInt32(txtACDelay.Text);
                CGlobalPara.SysPara.Para.ReTestTimes = System.Convert.ToInt32(txtReTestTimes.Text);
                CGlobalPara.SysPara.Para.ReTestDelay = System.Convert.ToInt32(txtReDelayMs.Text);
                CGlobalPara.SysPara.Para.IdReTimes = System.Convert.ToInt32(txtIdTimes.Text);
                CGlobalPara.SysPara.Para.ReadSnTimes = System.Convert.ToInt32(txtReadSnNum.Text); 
                CGlobalPara.SysPara.Para.ChkFailWait = chkFailWait.Checked;    
                CGlobalPara.SysPara.Para.ChkVSenor = chkVSenor.Checked;
                CGlobalPara.SysPara.Para.ChkACON = chkACON.Checked;
                CGlobalPara.SysPara.Para.ChkIdleLoad = chkIdleLoad.Checked;
                CGlobalPara.SysPara.Para.IdleLoad = System.Convert.ToDouble(txtIdleLoad.Text);

                CGlobalPara.SysPara.Alarm.StatTimes=System.Convert.ToInt32(txtStatTimes.Text);
                CGlobalPara.SysPara.Alarm.StatFailTimes = System.Convert.ToInt32(txtStatFailNum.Text);
                CGlobalPara.SysPara.Alarm.StatPassRate = System.Convert.ToDouble(txtStatPassRate.Text);   
                CGlobalPara.SysPara.Alarm.FixtureTimes=System.Convert.ToInt32(txtFixTimes.Text);
                CGlobalPara.SysPara.Alarm.FixFailTimes=System.Convert.ToInt32(txtFixFailTimes.Text);
                CGlobalPara.SysPara.Alarm.FixPassRate = System.Convert.ToInt32(txtFixPassRate.Text);
                CGlobalPara.SysPara.Alarm.ChkPassRate = chkPassRate.Checked;
                CGlobalPara.SysPara.Alarm.PassRateLimit = System.Convert.ToDouble(txtPassRateLimit.Text) / 100;
                CGlobalPara.SysPara.Alarm.PassRateStartNum = System.Convert.ToInt32(txtStartNumber.Text);
                CGlobalPara.SysPara.Alarm.PassRateAlarmTime = System.Convert.ToInt32(txtPassRateInterval.Text);
                CGlobalPara.SysPara.Alarm.ChkClrDay = chkClrDay.Checked;
                CGlobalPara.SysPara.Alarm.ChkClrNight = chkClrNight.Checked;
                CGlobalPara.SysPara.Alarm.ClrDayTime = txtClrDay.Text;
                CGlobalPara.SysPara.Alarm.ClrNightTime = txtClrNight.Text;

                CGlobalPara.SysPara.Report.ModelPath=txtModelPath.Text;
                CGlobalPara.SysPara.Report.SaveReport=chkSaveReport.Checked;
                CGlobalPara.SysPara.Report.SaveReportTimes=System.Convert.ToInt16(txtSaveTimes.Text);
                CGlobalPara.SysPara.Report.ReportPath=txtReportPath.Text;
                CGlobalPara.SysPara.Report.DailyFolder = txtDailyFolder.Text;

                CGlobalPara.SysPara.Mes.ChkWebSn = chkGJWeb.Checked ? 1 : 0; 
                CGlobalPara.SysPara.Mes.Connect = chkMesCon.Checked;
                CGlobalPara.SysPara.Mes.TranDataToMes = chkTranData.Checked;
                CGlobalPara.SysPara.Mes.SnMode = (ESnMode)cmbSnMode.SelectedIndex; 
                CGlobalPara.SysPara.Mes.SnLen=System.Convert.ToInt16(txtSnLen.Text);
                CGlobalPara.SysPara.Mes.SnSpec=txtSnSpec.Text;
                CGlobalPara.SysPara.Mes.ChkSnBI = chkBISn.Checked;
                CGlobalPara.SysPara.Mes.ChkSnHP = chkHPSn.Checked;
                CGlobalPara.SysPara.Mes.ChkSnATE = chkATESn.Checked;

                CSerializable<CSysPara>.WriteXml(CGlobalPara.SysFile, CGlobalPara.SysPara);          
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
