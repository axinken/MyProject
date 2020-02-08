using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.PLUGINS;
using GJ.PDB;
using GJ.COM;
namespace GJ.YOHOO.BURNIN
{
    public partial class FrmSample : Form, IChildMsg
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
        public FrmSample()
        {
            InitializeComponent();
        }
        #endregion

        #region 面板回调函数
        private void FrmYeild_Load(object sender, EventArgs e)
        {

            refreshTotalYield();

            refreshModelList();

            refreshYieldRecord();
            
            refreshChmrStatus();

            timer1.Start();

            string curOutModel = CIniFile.ReadFromIni("Parameter", "curOutModel", CGlobalPara.IniFile);

            labOutModel.Text = curOutModel;
        
        }
        #endregion

        #region 方法
        private void refreshTotalYield()
        {
            int ttNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "yieldTTNum", CGlobalPara.IniFile, "0"));
            int failNum = System.Convert.ToInt32(CIniFile.ReadFromIni("DailyYield", "yieldFailNum", CGlobalPara.IniFile, "0"));
            labTTNum.Text = ttNum.ToString();
            labFailNum.Text = failNum.ToString();   
        }
        private void refreshModelList()
        {
            try
            {
                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                string sqlCmd = "select distinct ModelName from RUN_PARA where doRun="+ (int)EDoRun.正在老化 + " or doRun=" + (int)EDoRun.老化结束;

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                {
                    MessageBox.Show(er);
                    return;
                }

                labModelNum.Text = ds.Tables[0].Rows.Count.ToString();

                cmbModel.Items.Clear();

                cmbModel.Items.Add(CLanguage.Lan("所有老化机种"));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    cmbModel.Items.Add(ds.Tables[0].Rows[i]["ModelName"].ToString());

                cmbModel.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }
        /// <summary>
        /// 产能统计
        /// </summary>
        private void refreshYieldRecord()
        {
            try
            {
                string er=string.Empty; 

                YieldView.Rows.Clear();

                CDBCOM db = new CDBCOM(EDBType.Access,"", CGlobalPara.SysDB);

                string sqlCmd = "select * from YieldRecord order by idNo";

                DataSet ds = null;
 
                if(!db.QuerySQL(sqlCmd,out ds,out er))
                {
                      MessageBox.Show(er);
                      return; 
                }
                int dayTTNum = 0;
                int dayPassNum = 0;
                double dayFailRate = 0;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string idNo = (i + 1).ToString(); 
                    string yieldTimes=ds.Tables[0].Rows[i]["YieldTimes"].ToString();
                    int ttNum = System.Convert.ToInt32(ds.Tables[0].Rows[i]["ttNum"].ToString());
                    int passNum = System.Convert.ToInt32(ds.Tables[0].Rows[i]["passNum"].ToString());
                    double failRate = 0;
                    if (ttNum != 0)
                        failRate = (double)(ttNum - passNum) / (double)ttNum;
                    YieldView.Rows.Add(idNo, yieldTimes, ttNum, passNum, failRate.ToString("P2"));
                    dayTTNum += ttNum;
                    dayPassNum += passNum;
                }
                if (dayTTNum != 0)
                    dayFailRate = (double)(dayTTNum - dayPassNum) / (double)dayTTNum;
                YieldView.Rows.Add("*", "00:00--24:00", dayTTNum, dayPassNum, dayFailRate.ToString("P2"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 老化中数量
        /// </summary>
        private void refreshBIStatus()
        {
            try
            {
                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                string sqlCmd = string.Empty;

                if (cmbModel.Text == CLanguage.Lan("所有老化机种"))
                    sqlCmd = "select * from RUN_PARA where doRun=" + (int)EDoRun.正在老化 + " order by UUTNO";
                else
                    sqlCmd = "select * from RUN_PARA where doRun=" + (int)EDoRun.正在老化 + " and ModelName='" + cmbModel.Text + "' order by UUTNO";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                {
                    MessageBox.Show(er);
                    return;
                }

                labBINum.Text = ds.Tables[0].Rows.Count.ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }
        /// <summary>
        /// 老化结束数量
        /// </summary>
        private void refreshEndBIStatus()
        {
            try
            {
                string er = string.Empty;

                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                string sqlCmd = string.Empty;

                if (cmbModel.Text == CLanguage.Lan("所有老化机种"))
                    sqlCmd = "select * from RUN_PARA where doRun=" + (int)EDoRun.老化结束 + " order by UUTNO";
                else
                    sqlCmd = "select * from RUN_PARA where doRun=" + (int)EDoRun.老化结束 + " and ModelName='" + cmbModel.Text + "' order by UUTNO";

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                {
                    MessageBox.Show(er);
                    return;
                }

                labOutNum.Text = ds.Tables[0].Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }           
        }
        /// <summary>
        /// 即将老化结束
        /// </summary>
        private void refreshChmrStatus()
        {
            try
            {
                string er = string.Empty;
 
                CDBCOM db = new CDBCOM(EDBType.Access, "", CGlobalPara.SysDB);

                DataSet ds = null;

                int leftTimes = System.Convert.ToInt32(txtEndTimes.Text)*60;

                string sqlCmd = string.Empty;

                if (cmbModel.Text == CLanguage.Lan("所有老化机种"))
                {
                    sqlCmd = "select RUN_BASE.LocalName," +
                                    "RUN_PARA.ModelName," +
                                    "RUN_PARA.IDCard," +
                                    "RUN_PARA.StartTime," +
                                    "RUN_PARA.EndTime," +
                                    "UUT_PARA.RunTime," +
                                    "RUN_PARA.doRun," +
                                    "RUN_PARA.BurnTime," +
                                    "RUN_PARA.UUTNO" +
                                    " from RUN_PARA,RUN_BASE,UUT_PARA" +
                                    " where RUN_PARA.UUTNO = RUN_BASE.UUTNO" +
                                      " and RUN_PARA.UUTNO = UUT_PARA.UUTNO" +
                                      " and RUN_PARA.IDCard<>''" +
                                      " and (RUN_PARA.doRun=" + (int)EDoRun.老化结束 +
                                      " or (RUN_PARA.doRun=" + (int)EDoRun.正在老化 +
                                      " and (RUN_PARA.BurnTime - UUT_PARA.RunTime)<" +
                                      leftTimes.ToString() + "))" +
                                      "  order by RUN_PARA.ModelName,RUN_PARA.StartTime";
                }
                else
                {
                    sqlCmd = "select RUN_BASE.LocalName," +
                                    "RUN_PARA.ModelName," +
                                    "RUN_PARA.IDCard," +
                                    "RUN_PARA.StartTime," +
                                    "RUN_PARA.EndTime," +
                                    "UUT_PARA.RunTime," +
                                    "RUN_PARA.doRun," +
                                    "RUN_PARA.BurnTime," +
                                    "RUN_PARA.UUTNO" +
                                    " from RUN_PARA,RUN_BASE,UUT_PARA" +
                                    " where RUN_PARA.UUTNO=RUN_BASE.UUTNO" +
                                    " and RUN_PARA.UUTNO = UUT_PARA.UUTNO" +
                                    " and RUN_PARA.IDCard<>''" +
                                    " and (RUN_PARA.doRun=" + (int)EDoRun.老化结束 +
                                    " or (RUN_PARA.doRun=" + (int)EDoRun.正在老化 +
                                    " and (RUN_PARA.BurnTime-UUT_PARA.RunTime)<" +
                                     leftTimes.ToString() + "))" +
                                     " and ModelName='" + cmbModel.Text + "'" +
                                     " order by RUN_PARA.ModelName,RUN_PARA.StartTime";
                }

                if (!db.QuerySQL(sqlCmd, out ds, out er))
                {
                    MessageBox.Show(er);
                    return;
                }

                int preNum = ds.Tables[0].Rows.Count;

                uutView.Rows.Clear();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    int idNo = i + 1;

                    string modelName = ds.Tables[0].Rows[i]["ModelName"].ToString();  

                    string localName = ds.Tables[0].Rows[i]["LocalName"].ToString();

                    string idCard = ds.Tables[0].Rows[i]["IDCard"].ToString();

                    string startTime = ds.Tables[0].Rows[i]["StartTime"].ToString();

                    int runTime = System.Convert.ToInt32(ds.Tables[0].Rows[i]["RunTime"].ToString());

                    TimeSpan ts = new TimeSpan(0, 0, runTime);
                    
                    string runTimes = ts.Days.ToString("D2") + ":" + ts.Hours.ToString("D2") + ":" +
                                      ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");    

                    int doRun = System.Convert.ToInt32(ds.Tables[0].Rows[i]["doRun"].ToString());

                    string status = string.Empty;

                    if (doRun == (int)EDoRun.正在老化)
                        status = "老化中";
                    else
                        status = "老化结束";

                    uutView.Rows.Add(idNo, modelName,localName, idCard, startTime, runTimes, status);

                }

                labPreNum.Text = preNum.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        private void btnQuery_Click(object sender, EventArgs e)
        {
            refreshTotalYield();

            refreshBIStatus();
            
            refreshEndBIStatus();

            refreshChmrStatus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
 
            refreshBIStatus();

            refreshEndBIStatus();
        }

    }
}
