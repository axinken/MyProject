using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports; 
using GJ.PLUGINS;
using GJ.COM;
using GJ.APP;
namespace GJ.YOHOO.UNLOAD
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
            CLanguage.SetLanguage(this);
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

        #region 初始化
        /// <summary>
        /// 设置双缓冲,防止界面闪烁
        /// </summary>
        private void SetDoubleBuffered()
        {
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
            txtTestPort.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtMonDelay.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtReportSaveTimes.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            load();
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
        private void OnTextKeyPressIsNumber(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)'.')
                e.Handled = true;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 加载设置
        /// </summary>
        private void load()
        {
            try
            {
                string[] comArray = SerialPort.GetPortNames();

                cmbIdCom.Items.Clear();

                for (int i = 0; i < comArray.Length; i++)
                {
                    cmbIdCom.Items.Add(comArray[i]);
                }

                txtPlcIP.Text = CGlobalPara.SysPara.Dev.PlcIp;
                cmbIdCom.Text = CGlobalPara.SysPara.Dev.IdCom;
                txtTestPort.Text = CGlobalPara.SysPara.Dev.TcpPort.ToString();

                cmbTcpMode.Items.Clear();
                cmbTcpMode.Items.Add(CLanguage.Lan("自定义结构体"));
                cmbTcpMode.Items.Add(CLanguage.Lan("自定义字符串"));
                cmbTcpMode.Items.Add(CLanguage.Lan("自定义JSON"));
                cmbTcpMode.SelectedIndex = CGlobalPara.SysPara.Dev.TcpMode;    

                txtMonDelay.Text = CGlobalPara.SysPara.Dev.MonInterval.ToString();

                cmbDbMode.Items.Clear();
                cmbDbMode.Items.Add(CLanguage.Lan("数据库"));
                cmbDbMode.Items.Add(CLanguage.Lan("Ini文件"));
                cmbDbMode.SelectedIndex = CGlobalPara.SysPara.Dev.IdRecordMode;

                cmbATEMax.SelectedIndex = CGlobalPara.SysPara.Dev.AteDevMax; 
               
                chkHPFail.Checked = CGlobalPara.SysPara.Para.ChkHPFail;
                chkATEFail.Checked = CGlobalPara.SysPara.Para.ChkATEFail;
                chkNoHipot.Checked = CGlobalPara.SysPara.Para.ChkNoHP;
                chkNoATE.Checked = CGlobalPara.SysPara.Para.ChkNoATE;
                chkInHipot.Checked = CGlobalPara.SysPara.Para.ChkInHP;
                chkInATE.Checked = CGlobalPara.SysPara.Para.ChkInATE;
                chkLockFail.Checked = CGlobalPara.SysPara.Para.ChkLockFail; 
                txtLockPwr.Text = CGlobalPara.SysPara.Para.LockHPPwr;
                txtIdTimes.Text = CGlobalPara.SysPara.Para.IdReTimes.ToString(); 
                chkChangeModel.Checked = CGlobalPara.SysPara.Para.ChkModel;
                txtFailGoNum.Text = CGlobalPara.SysPara.Para.FailGoNum.ToString();
                chkDebugMode.Checked = CGlobalPara.SysPara.Para.ChkGoPass;

                chkPassRate1.Checked = CGlobalPara.SysPara.Alarm.ChkPassRate[0];
                txtPassRateLimit1.Text = (CGlobalPara.SysPara.Alarm.PassRateLimit[0] * 100).ToString();
                txtStartNumber1.Text = CGlobalPara.SysPara.Alarm.PassRateStartNum[0].ToString();
                chkPassRate2.Checked = CGlobalPara.SysPara.Alarm.ChkPassRate[1];
                txtPassRateLimit2.Text = (CGlobalPara.SysPara.Alarm.PassRateLimit[1] * 100).ToString();
                txtStartNumber2.Text = CGlobalPara.SysPara.Alarm.PassRateStartNum[1].ToString();
                txtPassRateInterval.Text = CGlobalPara.SysPara.Alarm.PassRateAlarmTime.ToString();
                chkClrDay.Checked = CGlobalPara.SysPara.Alarm.ChkClrDay;
                chkClrNight.Checked = CGlobalPara.SysPara.Alarm.ChkClrNight;
                txtClrDay.Text = CGlobalPara.SysPara.Alarm.ClrDayTime;
                txtClrNight.Text = CGlobalPara.SysPara.Alarm.ClrNightTime;
                chkATEToWeb.Checked = CGlobalPara.SysPara.Para.ChkATEToGJWeb;

                txtModelPath.Text = CGlobalPara.SysPara.Report.ModelPath;
                chkSaveReport.Checked = CGlobalPara.SysPara.Report.SaveReport;
                txtReportPath.Text = CGlobalPara.SysPara.Report.ReportPath;
                txtReportSaveTimes.Text = CGlobalPara.SysPara.Report.SaveReportTimes.ToString();
                txtDailyFolder.Text = CGlobalPara.SysPara.Report.DailyFolder;

                chkGJWeb.Checked = CGlobalPara.SysPara.Mes.ChkWebSn == 1 ? true : false;
                chkMesCon.Checked = CGlobalPara.SysPara.Mes.Connect;
                chkStatMes.Checked = CGlobalPara.SysPara.Mes.ChkStat;

            }
            catch (Exception)
            {                
                throw;
            }
        }
        /// <summary>
        /// 保存设置
        /// </summary>
        private void save()
        {
            try
            {
                CGlobalPara.SysPara.Dev.PlcIp = txtPlcIP.Text;
                CGlobalPara.SysPara.Dev.IdCom=cmbIdCom.Text;
                CGlobalPara.SysPara.Dev.TcpPort=System.Convert.ToInt16(txtTestPort.Text);
                CGlobalPara.SysPara.Dev.TcpMode = cmbTcpMode.SelectedIndex;     
                CGlobalPara.SysPara.Dev.MonInterval=System.Convert.ToInt16(txtMonDelay.Text);
                CGlobalPara.SysPara.Dev.IdRecordMode=cmbDbMode.SelectedIndex;
                CGlobalPara.SysPara.Dev.AteDevMax = cmbATEMax.SelectedIndex;    

                CGlobalPara.SysPara.Para.ChkHPFail=chkHPFail.Checked;
                CGlobalPara.SysPara.Para.ChkATEFail=chkATEFail.Checked;
                CGlobalPara.SysPara.Para.ChkNoHP=chkNoHipot.Checked;
                CGlobalPara.SysPara.Para.ChkNoATE=chkNoATE.Checked;
                CGlobalPara.SysPara.Para.ChkInHP = chkInHipot.Checked;
                CGlobalPara.SysPara.Para.ChkInATE = chkInATE.Checked;
                CGlobalPara.SysPara.Para.ChkLockFail = chkLockFail.Checked; 
                CGlobalPara.SysPara.Para.LockHPPwr=txtLockPwr.Text;
                CGlobalPara.SysPara.Para.IdReTimes = System.Convert.ToInt32(txtIdTimes.Text); 
                CGlobalPara.SysPara.Para.ChkModel = chkChangeModel.Checked;
                CGlobalPara.SysPara.Para.FailGoNum = System.Convert.ToInt32(txtFailGoNum.Text);
                CGlobalPara.SysPara.Para.ChkGoPass = chkDebugMode.Checked;
                CGlobalPara.SysPara.Para.ChkATEToGJWeb = chkATEToWeb.Checked;

                CGlobalPara.SysPara.Alarm.ChkPassRate[0] = chkPassRate1.Checked;
                CGlobalPara.SysPara.Alarm.PassRateLimit[0] = System.Convert.ToDouble(txtPassRateLimit1.Text) / 100;
                CGlobalPara.SysPara.Alarm.PassRateStartNum[0] = System.Convert.ToInt32(txtStartNumber1.Text);
                CGlobalPara.SysPara.Alarm.ChkPassRate[1] = chkPassRate2.Checked;
                CGlobalPara.SysPara.Alarm.PassRateLimit[1] = System.Convert.ToDouble(txtPassRateLimit2.Text) / 100;
                CGlobalPara.SysPara.Alarm.PassRateStartNum[1] = System.Convert.ToInt32(txtStartNumber2.Text);
                CGlobalPara.SysPara.Alarm.PassRateAlarmTime = System.Convert.ToInt32(txtPassRateInterval.Text);
                CGlobalPara.SysPara.Alarm.ChkClrDay = chkClrDay.Checked;
                CGlobalPara.SysPara.Alarm.ChkClrNight = chkClrNight.Checked;
                CGlobalPara.SysPara.Alarm.ClrDayTime = txtClrDay.Text;
                CGlobalPara.SysPara.Alarm.ClrNightTime = txtClrNight.Text;


                CGlobalPara.SysPara.Report.ModelPath = txtModelPath.Text;
                CGlobalPara.SysPara.Report.SaveReport = chkSaveReport.Checked;
                CGlobalPara.SysPara.Report.ReportPath = txtReportPath.Text;
                CGlobalPara.SysPara.Report.SaveReportTimes = System.Convert.ToDouble(txtReportSaveTimes.Text);
                CGlobalPara.SysPara.Report.DailyFolder = txtDailyFolder.Text;

                CGlobalPara.SysPara.Mes.ChkWebSn = chkGJWeb.Checked ? 1 : 0; 
                CGlobalPara.SysPara.Mes.Connect = chkMesCon.Checked;
                CGlobalPara.SysPara.Mes.ChkStat = chkStatMes.Checked;
                
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
