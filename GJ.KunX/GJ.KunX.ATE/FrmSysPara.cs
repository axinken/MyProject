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
namespace GJ.KunX.ATE
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

            switch (CLanguage.languageType)
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

        #region 初始化
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
            panel1.GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panel1, true, null);
            panel2.GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panel2, true, null);
        }
        #endregion

        #region 面板回调函数
        private void FrmSysPara_Load(object sender, EventArgs e)
        {

            txtSerTcpPort.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            txtMonDelay.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            txtATEDelay.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            txtATERepeats.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            load();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("确定要保存系统参数设置?", "系统参数设置", MessageBoxButtons.YesNo,
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
        private void OnTextKeyPressIsNumber(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)'.')
                e.Handled = true;
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
        #endregion

        #region 方法
        private void load()
        {
            try
            {
                  string[] comArray = SerialPort.GetPortNames();
                  cmbIO.Items.Clear();
                  for (int i = 0; i < comArray.Length; i++)
                     cmbIO.Items.Add(comArray[i]);

                 cmbATELanguage.Items.Clear();
                 cmbATELanguage.Items.Add(CLanguage.Lan("英文界面"));
                 cmbATELanguage.Items.Add(CLanguage.Lan("中文界面"));                 

                txtSerTcpIP.Text = CGlobalPara.SysPara.Dev.SerIP;
                txtSerTcpPort.Text = CGlobalPara.SysPara.Dev.SerPort.ToString();
                txtSerStatName.Text = CGlobalPara.SysPara.Dev.SerStat;

                cmbTcpMode.Items.Clear();
                cmbTcpMode.Items.Add(CLanguage.Lan("自定义结构体"));
                cmbTcpMode.Items.Add(CLanguage.Lan("自定义字符串"));
                cmbTcpMode.Items.Add(CLanguage.Lan("自定义JSON"));
                cmbTcpMode.SelectedIndex = CGlobalPara.SysPara.Dev.TcpMode;

                cmbDevMax.Text = CGlobalPara.SysPara.Dev.DevMax.ToString();     
                cmbChanMax.Text = CGlobalPara.SysPara.Dev.ChanMax.ToString();
                chkIoEnable.Checked = CGlobalPara.SysPara.Dev.ChkIoEnable;
                cmbIO.Text = CGlobalPara.SysPara.Dev.IoCom;
                txtIoDelayMs.Text = CGlobalPara.SysPara.Dev.IoDelayMs.ToString();  
                cmbATELanguage.SelectedIndex = CGlobalPara.SysPara.Dev.Ate_Languge;
                txtMonDelay.Text = CGlobalPara.SysPara.Dev.InternalTime.ToString();
                txtATEMon.Text=CGlobalPara.SysPara.Para.Ate_Mon.ToString();
  
                txtATEPrg.Text = CGlobalPara.SysPara.Para.Ate_Title_Name;
                txtATEResultPath.Text = CGlobalPara.SysPara.Para.Ate_Result_Folder;
                chkBarForm.Checked = CGlobalPara.SysPara.Para.Ate_scanSn_Enable;
                txtBarFormName.Text = CGlobalPara.SysPara.Para.Ate_ScanSn_Name;
                txtATEDelay.Text = CGlobalPara.SysPara.Para.Ate_Result_Delay.ToString();
                txtATERepeats.Text = CGlobalPara.SysPara.Para.Ate_Result_Repeats.ToString();
                chkImg.Checked = CGlobalPara.SysPara.Para.Ate_getImg_handler;
                txtFailLock.Text = CGlobalPara.SysPara.Para.FailNumLock.ToString();
                chkGetTestData.Checked = CGlobalPara.SysPara.Para.ATE_TestData;

                txtModelPath.Text = CGlobalPara.SysPara.Report.ModelPath;
                chkSaveReport.Checked = CGlobalPara.SysPara.Report.SaveReport;
                txtReportPath.Text = CGlobalPara.SysPara.Report.ReportPath;
                txtReportSaveTimes.Text = CGlobalPara.SysPara.Report.SaveReportTimes.ToString();

                chkGJWeb.Checked = CGlobalPara.SysPara.Mes.ChkWebSn == 1 ? true : false;
                chkMesCon.Checked = CGlobalPara.SysPara.Mes.Connect;
                txtFailNoTran.Text = CGlobalPara.SysPara.Mes.FailNoTranNum.ToString(); 
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

                CGlobalPara.SysPara.Dev.SerIP = txtSerTcpIP.Text;
                CGlobalPara.SysPara.Dev.SerPort = System.Convert.ToInt32(txtSerTcpPort.Text);
                CGlobalPara.SysPara.Dev.SerStat = txtSerStatName.Text;
                CGlobalPara.SysPara.Dev.TcpMode=cmbTcpMode.SelectedIndex;
                CGlobalPara.SysPara.Dev.Ate_Languge = cmbATELanguage.SelectedIndex;
                CGlobalPara.SysPara.Dev.InternalTime = System.Convert.ToInt32(txtMonDelay.Text);
                CGlobalPara.SysPara.Dev.DevMax = System.Convert.ToInt16(cmbDevMax.Text);
                CGlobalPara.SysPara.Dev.ChanMax = System.Convert.ToInt16(cmbChanMax.Text);
                CGlobalPara.SysPara.Dev.ChkIoEnable = chkIoEnable.Checked;
                CGlobalPara.SysPara.Dev.IoCom=cmbIO.Text;
                CGlobalPara.SysPara.Dev.IoDelayMs=System.Convert.ToInt32(txtIoDelayMs.Text);  

                CGlobalPara.SysPara.Para.Ate_Title_Name = txtATEPrg.Text;
                CGlobalPara.SysPara.Para.Ate_Result_Folder = txtATEResultPath.Text;
                CGlobalPara.SysPara.Para.Ate_scanSn_Enable = chkBarForm.Checked;
                CGlobalPara.SysPara.Para.Ate_ScanSn_Name = txtBarFormName.Text;
                CGlobalPara.SysPara.Para.Ate_Result_Delay = System.Convert.ToInt32(txtATEDelay.Text);
                CGlobalPara.SysPara.Para.Ate_Result_Repeats = System.Convert.ToInt32(txtATERepeats.Text);
                CGlobalPara.SysPara.Para.Ate_getImg_handler = chkImg.Checked;
                CGlobalPara.SysPara.Para.Ate_Mon = System.Convert.ToInt32(txtATEMon.Text);
                CGlobalPara.SysPara.Para.FailNumLock = System.Convert.ToInt16(txtFailLock.Text);
                CGlobalPara.SysPara.Para.ATE_TestData = chkGetTestData.Checked;

                CGlobalPara.SysPara.Report.ModelPath = txtModelPath.Text;
                CGlobalPara.SysPara.Report.SaveReport = chkSaveReport.Checked;
                CGlobalPara.SysPara.Report.ReportPath = txtReportPath.Text;
                CGlobalPara.SysPara.Report.SaveReportTimes = System.Convert.ToDouble(txtReportSaveTimes.Text);

                CGlobalPara.SysPara.Mes.ChkWebSn = chkGJWeb.Checked ? 1 : 0;
                CGlobalPara.SysPara.Mes.Connect = chkMesCon.Checked;
                CGlobalPara.SysPara.Mes.FailNoTranNum = System.Convert.ToInt16(txtFailNoTran.Text);

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
