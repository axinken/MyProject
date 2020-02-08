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
using GJ.PDB;
namespace GJ.KunX.BURNIN
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

            InitialControl();

            SetDoubleBuffered();
          
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
            panel3.GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panel3, true, null);
            panel4.GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panel4, true, null);
            panel5.GetType().GetProperty("DoubleBuffered",
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.NonPublic)
                                            .SetValue(panel5, true, null);
        }
        #endregion

        #region 面板控件

        #endregion

        #region 面板回调函数
        private void FrmSysPara_Load(object sender, EventArgs e)
        {
            load();

            if (CGlobalPara.LogName == "GUANJIA")
                chkNoLockFail.Visible = true;
            else
                chkNoLockFail.Visible = false;


            
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
        private void btnDayBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtDayRecord.Text = dlg.SelectedPath;  
        }
        private void btnTempFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
               txtTempFolder.Text = dlg.SelectedPath;  
        }
        private void btnClrUse_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CLanguage.Lan("确认要归零库位母治具使用次数?"), "Tip",
                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string er = string.Empty;

            CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

            string sqlCmd = "update RUN_PARA set UsedNum=0";

            if (!db.excuteSQL(sqlCmd, out er))
                MessageBox.Show(CLanguage.Lan("归零库位母治具使用次数错误") + ":" + er);
            else
                MessageBox.Show(CLanguage.Lan("归零库位母治具使用次数OK"));
        }
        #endregion

        #region 方法
        private void load()
        {
            try
            {
                if (CGlobalPara.LogName == "GUANJIA")
                {
                    chkNoLockFail.Visible = true;
                    chkNoJugdeCur.Visible = true;
                }
                else
                {
                    chkNoLockFail.Visible = false;
                    chkNoJugdeCur.Visible = false;
                }
                
                cmbIdCom.Items.Clear();
                cmbMonCom1.Items.Clear();
                cmbMonCom2.Items.Clear();
                cmbMonCom3.Items.Clear();
                cmbMonCom4.Items.Clear();
                cmbMonCom5.Items.Clear();
                cmbERSCom1.Items.Clear();
                string[] comArray = SerialPort.GetPortNames();
                for (int i = 0; i < comArray.Length; i++)
                {
                    cmbIdCom.Items.Add(comArray[i]); 
                    cmbMonCom1.Items.Add(comArray[i]);
                    cmbMonCom2.Items.Add(comArray[i]);
                    cmbMonCom3.Items.Add(comArray[i]);
                    cmbMonCom4.Items.Add(comArray[i]);
                    cmbMonCom5.Items.Add(comArray[i]);
                    cmbERSCom1.Items.Add(comArray[i]);
                }
                cmbIdCom.Text = CGlobalPara.SysPara.Dev.IdCom; 
                txtBIPLC.Text = CGlobalPara.SysPara.Dev.Bi_plc;   
                cmbMonCom1.Text = CGlobalPara.SysPara.Dev.MonCom[0];
                cmbMonCom2.Text = CGlobalPara.SysPara.Dev.MonCom[1];
                cmbMonCom3.Text = CGlobalPara.SysPara.Dev.MonCom[2];
                cmbMonCom4.Text = CGlobalPara.SysPara.Dev.MonCom[3];
                cmbMonCom5.Text = CGlobalPara.SysPara.Dev.MonCom[4];
                cmbERSCom1.Text = CGlobalPara.SysPara.Dev.ErsCom[0];
                txtERSBaud.Text = CGlobalPara.SysPara.Dev.ErsBaud;
                txtMonInterval.Text = CGlobalPara.SysPara.Dev.MonInterval.ToString();

                cmbVoltPos.Items.Clear();
                cmbVoltPos.Items.Add(CLanguage.Lan("面对产品从左到右"));
                cmbVoltPos.Items.Add(CLanguage.Lan("面对产品从右到左"));
                cmbVoltPos.SelectedIndex = CGlobalPara.SysPara.Dev.VoltPos;

                cmbCtrlACMode.Items.Clear();
                cmbCtrlACMode.Items.Add(CLanguage.Lan("PLC控制时序"));
                cmbCtrlACMode.Items.Add(CLanguage.Lan("上位机控制时序"));
                cmbCtrlACMode.SelectedIndex = (int)CGlobalPara.SysPara.Dev.CtrlACMode;

                cmbRunTimeMode.Items.Clear();
                cmbRunTimeMode.Items.Add(CLanguage.Lan("上位机计时模式1"));
                cmbRunTimeMode.Items.Add(CLanguage.Lan("上位机计时模式2"));
                cmbRunTimeMode.Items.Add(CLanguage.Lan("PLC计时模式"));
                cmbRunTimeMode.SelectedIndex = (int)CGlobalPara.SysPara.Dev.CtrlTimeMode;

                chkFMCBVer.Checked = CGlobalPara.SysPara.Dev.ChkFCMBVer;
                txtFCMBVer.Text = CGlobalPara.SysPara.Dev.FCMBVer;
                txtQCI.Text = CGlobalPara.SysPara.Dev.FCMBQCI.ToString();

                chkHandIn.Checked = CGlobalPara.SysPara.Para.ChkHandIn;
                chkForbitOut.Checked = CGlobalPara.SysPara.Para.ChkForbitOut;
                chkNoLockFail.Checked = CGlobalPara.SysPara.Para.ChkNoLockFail;
                chkNoJugdeCur.Checked = CGlobalPara.SysPara.Para.ChkNoJugdeCur;
                txtVLP.Text = CGlobalPara.SysPara.Para.VLP.ToString();
                txtVHP.Text = CGlobalPara.SysPara.Para.VHP.ToString();
                txtILP.Text = CGlobalPara.SysPara.Para.ILP.ToString();
                txtIHP.Text = CGlobalPara.SysPara.Para.IHP.ToString();
                txtIOFFSET.Text = CGlobalPara.SysPara.Para.IOFFSET.ToString();
                txtIdTimes.Text = CGlobalPara.SysPara.Para.IdTimes.ToString();
                txtFirstLoad.Text = CGlobalPara.SysPara.Para.IdleLoad.ToString();
                txtLoadDelayS.Text = CGlobalPara.SysPara.Para.LoadDelayS.ToString();
                chkAutoModel.Checked = CGlobalPara.SysPara.Para.ChkAutoModel;
                chkSameMode.Checked = CGlobalPara.SysPara.Para.ChkSameModel;
                txtSameTimes.Text = CGlobalPara.SysPara.Para.ModelTimes.ToString();
                chkAutoNull.Checked = CGlobalPara.SysPara.Para.ChkNullAutoOut;
                chkNoShow.Checked = CGlobalPara.SysPara.Para.NoShowAlarm;
                chkACVolt.Checked = CGlobalPara.SysPara.Para.ChkACVolt;
                txtBITimeRate.Text = CGlobalPara.SysPara.Para.BITimeRate.ToString();
                txtBackTime.Text = CGlobalPara.SysPara.Para.BackUpDBTime.ToString();
                chkFastOnOff.Checked  = CGlobalPara.SysPara.Para.ChkFastOnOff;
                chkAutoSelf.Checked = CGlobalPara.SysPara.Para.ChkAutoSelf;
                txtERSVon.Text = CGlobalPara.SysPara.Para.ERSVon.ToString();

                txtFailTimes.Text = CGlobalPara.SysPara.Alarm.FailTimes.ToString();
                txtComFails.Text = CGlobalPara.SysPara.Alarm.ComFailTimes.ToString();
                txtFixUseNum.Text = CGlobalPara.SysPara.Alarm.FixUserTimes.ToString();
                txtFixFailLockNum.Text = CGlobalPara.SysPara.Alarm.FixFailLockNum.ToString();
                txtChkQCVTimes.Text = CGlobalPara.SysPara.Alarm.Chk_qcv_times.ToString();
                txtCurFailAlarm.Text = CGlobalPara.SysPara.Alarm.FailCurTimes.ToString();
                txtOpDelayS.Text = CGlobalPara.SysPara.Alarm.OP_AlarmDelayS.ToString();
                txtPassRateLimit.Text = CGlobalPara.SysPara.Alarm.PassRateLimit.ToString();

                chkPassRate.Checked = CGlobalPara.SysPara.Alarm.ChkPassRate;
                txtPassRateLimit.Text = CGlobalPara.SysPara.Alarm.PassRateLimit.ToString();
                txtStartNum.Text = CGlobalPara.SysPara.Alarm.PassRateStartNum.ToString();
                txtAlarmInterval.Text = CGlobalPara.SysPara.Alarm.PassRateAlarmTime.ToString();
                chkClrDay.Checked = CGlobalPara.SysPara.Alarm.ChkClrDay;
                chkClrNight.Checked = CGlobalPara.SysPara.Alarm.ChkClrNight;
                txtClrDayTime.Text = CGlobalPara.SysPara.Alarm.ClrDayTime;
                txtClrNightTime.Text = CGlobalPara.SysPara.Alarm.ClrNightTime;

                txtModelPath.Text = CGlobalPara.SysPara.Report.ModelPath;
                chkSaveReport.Checked = CGlobalPara.SysPara.Report.SaveReport;
                txtReportPath.Text = CGlobalPara.SysPara.Report.ReportPath;
                txtReportSaveTimes.Text = CGlobalPara.SysPara.Report.SaveReportTimes.ToString();
                txtDayRecord.Text = CGlobalPara.SysPara.Report.DayRecordPath;
                txtTemTimes.Text = CGlobalPara.SysPara.Report.TempScanTime.ToString();
                txtTempFolder.Text = CGlobalPara.SysPara.Report.TempPath;  

                chkGJWeb.Checked = CGlobalPara.SysPara.Mes.ChkWebSn == 1 ? true : false;
                chkMesCon.Checked = CGlobalPara.SysPara.Mes.Connect;
                chkChkSn.Checked = CGlobalPara.SysPara.Mes.ChkSn;
                chkGoPass.Checked = CGlobalPara.SysPara.Mes.ChkGoPass;
                txtCurStation.Text = CGlobalPara.SysPara.Mes.CurStation;
                txtPCName.Text = CGlobalPara.SysPara.Mes.PCName;
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
                CGlobalPara.SysPara.Dev.Bi_plc = txtBIPLC.Text; 
                CGlobalPara.SysPara.Dev.MonCom[0] = cmbMonCom1.Text;
                CGlobalPara.SysPara.Dev.MonCom[1] = cmbMonCom2.Text;
                CGlobalPara.SysPara.Dev.MonCom[2] = cmbMonCom3.Text;
                CGlobalPara.SysPara.Dev.MonCom[3] = cmbMonCom4.Text;
                CGlobalPara.SysPara.Dev.MonCom[4] = cmbMonCom5.Text;
                CGlobalPara.SysPara.Dev.ErsCom[0] = cmbERSCom1.Text;
                CGlobalPara.SysPara.Dev.ErsBaud = txtERSBaud.Text;
                CGlobalPara.SysPara.Dev.MonInterval = System.Convert.ToInt16(txtMonInterval.Text);
                CGlobalPara.SysPara.Dev.VoltPos = cmbVoltPos.SelectedIndex;
                CGlobalPara.SysPara.Dev.CtrlACMode = (ECtrlACMode)cmbCtrlACMode.SelectedIndex;
                CGlobalPara.SysPara.Dev.CtrlTimeMode = (ECtrlTimeMode)cmbRunTimeMode.SelectedIndex;
                CGlobalPara.SysPara.Dev.ChkFCMBVer = chkFMCBVer.Checked;
                CGlobalPara.SysPara.Dev.FCMBVer = txtFCMBVer.Text;
                CGlobalPara.SysPara.Dev.FCMBQCI = System.Convert.ToDouble(txtQCI.Text);

                CGlobalPara.SysPara.Para.ChkNoJugdeCur = chkNoJugdeCur.Checked;
                CGlobalPara.SysPara.Para.ChkHandIn = chkHandIn.Checked;
                CGlobalPara.SysPara.Para.ChkNoLockFail = chkNoLockFail.Checked;
                CGlobalPara.SysPara.Para.ChkForbitOut = chkForbitOut.Checked;
                CGlobalPara.SysPara.Para.VHP = System.Convert.ToDouble(txtVHP.Text);
                CGlobalPara.SysPara.Para.VLP = System.Convert.ToDouble(txtVLP.Text);
                CGlobalPara.SysPara.Para.ILP = System.Convert.ToDouble(txtILP.Text);
                CGlobalPara.SysPara.Para.IHP = System.Convert.ToDouble(txtIHP.Text);
                CGlobalPara.SysPara.Para.ERSVon = System.Convert.ToDouble(txtERSVon.Text);
                CGlobalPara.SysPara.Para.IOFFSET = System.Convert.ToDouble(txtIOFFSET.Text);
                CGlobalPara.SysPara.Para.IdleLoad = System.Convert.ToDouble(txtFirstLoad.Text);
                CGlobalPara.SysPara.Para.LoadDelayS = System.Convert.ToInt16(txtLoadDelayS.Text);
                CGlobalPara.SysPara.Para.IdTimes = System.Convert.ToInt16(txtIdTimes.Text);
                CGlobalPara.SysPara.Para.ChkAutoModel = chkAutoModel.Checked;
                CGlobalPara.SysPara.Para.ChkSameModel = chkSameMode.Checked;
                CGlobalPara.SysPara.Para.ModelTimes = System.Convert.ToInt16(txtSameTimes.Text);
                CGlobalPara.SysPara.Para.ChkNullAutoOut = chkAutoNull.Checked;
                CGlobalPara.SysPara.Para.NoShowAlarm = chkNoShow.Checked;
                CGlobalPara.SysPara.Para.ChkACVolt = chkACVolt.Checked;
                CGlobalPara.SysPara.Para.BITimeRate = System.Convert.ToDouble(txtBITimeRate.Text);
                CGlobalPara.SysPara.Para.BackUpDBTime = System.Convert.ToInt32(txtBackTime.Text);
                CGlobalPara.SysPara.Para.ChkFastOnOff = chkFastOnOff.Checked;
                CGlobalPara.SysPara.Para.ChkAutoSelf = chkAutoSelf.Checked;

                CGlobalPara.SysPara.Alarm.FailTimes = System.Convert.ToInt16(txtFailTimes.Text);
                CGlobalPara.SysPara.Alarm.FailCurTimes = System.Convert.ToInt16(txtCurFailAlarm.Text);
                CGlobalPara.SysPara.Alarm.ComFailTimes = System.Convert.ToInt16(txtComFails.Text);
                CGlobalPara.SysPara.Alarm.FixUserTimes = System.Convert.ToInt16(txtFixUseNum.Text);
                CGlobalPara.SysPara.Alarm.FixFailLockNum = System.Convert.ToInt16(txtFixFailLockNum.Text);
                CGlobalPara.SysPara.Alarm.Chk_qcv_times = System.Convert.ToInt32(txtChkQCVTimes.Text);
                CGlobalPara.SysPara.Alarm.OP_AlarmDelayS = System.Convert.ToInt16(txtOpDelayS.Text);
                CGlobalPara.SysPara.Alarm.PassRateLimit = System.Convert.ToDouble(txtPassRateLimit.Text);

                CGlobalPara.SysPara.Alarm.ChkPassRate = chkPassRate.Checked;
                CGlobalPara.SysPara.Alarm.PassRateLimit = System.Convert.ToDouble(txtPassRateLimit.Text);
                CGlobalPara.SysPara.Alarm.PassRateStartNum = System.Convert.ToInt32(txtStartNum.Text);
                CGlobalPara.SysPara.Alarm.PassRateAlarmTime = System.Convert.ToInt32(txtAlarmInterval.Text);
                CGlobalPara.SysPara.Alarm.ChkClrDay = chkClrDay.Checked;
                CGlobalPara.SysPara.Alarm.ChkClrNight = chkClrNight.Checked;
                CGlobalPara.SysPara.Alarm.ClrDayTime = txtClrDayTime.Text;
                CGlobalPara.SysPara.Alarm.ClrNightTime = txtClrNightTime.Text;

                CGlobalPara.SysPara.Report.ModelPath=txtModelPath.Text;
                CGlobalPara.SysPara.Report.SaveReport=chkSaveReport.Checked;
                CGlobalPara.SysPara.Report.ReportPath=txtReportPath.Text;
                CGlobalPara.SysPara.Report.SaveReportTimes = System.Convert.ToDouble(txtReportSaveTimes.Text);
                CGlobalPara.SysPara.Report.DayRecordPath = txtDayRecord.Text;
                CGlobalPara.SysPara.Report.TempScanTime = System.Convert.ToInt16(txtTemTimes.Text);
                CGlobalPara.SysPara.Report.TempPath = txtTempFolder.Text;

                CGlobalPara.SysPara.Mes.ChkWebSn = chkGJWeb.Checked ? 1 : 0; 
                CGlobalPara.SysPara.Mes.Connect = chkMesCon.Checked;
                CGlobalPara.SysPara.Mes.ChkSn = chkChkSn.Checked;
                CGlobalPara.SysPara.Mes.ChkGoPass = chkGoPass.Checked;
                CGlobalPara.SysPara.Mes.CurStation = txtCurStation.Text;
                CGlobalPara.SysPara.Mes.PCName = txtPCName.Text;

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
