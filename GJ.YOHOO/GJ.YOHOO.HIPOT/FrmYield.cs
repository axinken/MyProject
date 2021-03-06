﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.MES;
using GJ.PLUGINS;
using GJ.COM;
using GJ.USER.APP;
using System.IO;

namespace GJ.YOHOO.HIPOT
{

    public partial class FrmYield : Form,IChildMsg
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
        public FrmYield()
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

        #region 字段
        /// <summary>
        /// web检查状态
        /// </summary>
        private bool web_check_flag = false;
        #endregion

        #region 面板回调函数
        private void FrmYield_Load(object sender, EventArgs e)
        {
            List<string> keyName = new List<string>();

            List<string> keyValue = new List<string>();

            CIniFile.GetIniKeySection("PLCAlarm", out  keyName, out keyValue, Application.StartupPath + "\\iniFile.ini");

            cmbStatName.Items.Clear();

            cmbStatName.Items.Add(CLanguage.Lan("所有工位"));

            for (int i = 0; i < keyValue.Count; i++)
            {
                cmbStatName.Items.Add(keyValue[i]);
            }
            cmbStatName.SelectedIndex = 0;

            cmbbAlarm.Items.Clear();
            cmbbAlarm.Items.Add(CLanguage.Lan("全部"));
            cmbbAlarm.Items.Add(CLanguage.Lan("解除"));
            cmbbAlarm.Items.Add(CLanguage.Lan("报警"));
            cmbbAlarm.SelectedIndex = 0;

            cmbFixNumSlotNo.SelectedIndex = 0;

            check_web_status_handler check_web = new check_web_status_handler(check_web_status);

            check_web.BeginInvoke(null, null);
        }
        private void btnQueryStat_Click(object sender, EventArgs e)
        {
            try
            {
                btnQueryStat.Enabled = false;

                if (!web_check_flag)
                {
                    labT1.Text = CLanguage.Lan("无法连接") + "[" + CYOHOOApp.UlrWeb + "]";
                    labT1.ForeColor = Color.Red;
                    return;
                }

                string er = string.Empty;

                CWeb2.CYield_Base input = new CWeb2.CYield_Base();

                input.StartTime = dpYieldStartTime.Value.Date.ToString("yyyy/MM/dd");
                input.EndTime = dpYieldEndTime.Value.Date.ToString("yyyy/MM/dd");
                input.FlowIndex = 0;
                input.FlowName = string.Empty;
                input.FlowGuid = string.Empty;
                input.LineNo = -1;
                input.LineName = string.Empty;
                input.OrderName = string.Empty;
                input.Model = string.Empty;

                List<CWeb2.CYield_Para> output = null;

                if (!CWeb2.QueryProductivity(input, out output, out er))
                {
                    labT1.Text = er;
                    labT1.ForeColor = Color.Red;
                    return;
                }

                YieldView.Rows.Clear();

                for (int i = 0; i < output.Count; i++)
                {
                    YieldView.Rows.Add(output[i].IdNo, output[i].Name, output[i].TTNum, output[i].FailNum);
                }

                labT1.Text = CLanguage.Lan("查询数量") + "【" + output.Count.ToString() + "】";
                labT1.ForeColor = Color.Blue;
            }
            catch (Exception ex)
            {
                labT1.Text = ex.ToString();
                labT1.ForeColor = Color.Red;
            }
            finally
            {
                btnQueryStat.Enabled = true;
            }
        }
        private void btnQueryAlarmList_Click(object sender, EventArgs e)
        {
            try
            {
                btnQueryAlarmList.Enabled = false;

                string er = string.Empty;

                if (!web_check_flag)
                {
                    labT1.Text = CLanguage.Lan("无法连接") + "[" + CYOHOOApp.UlrWeb + "]";
                    labT1.ForeColor = Color.Red;
                    return;
                }

                List<CWeb2.CAlarmRecord> alarmList = null;

                CWeb2.CAlarm_Base condition = new CWeb2.CAlarm_Base();

                condition.StartTime = dpAlarmStartDate.Value.Date.ToString("yyyy/MM/dd");

                condition.EndTime = dpAlarmEndDate.Value.Date.ToString("yyyy/MM/dd");

                if (cmbStatName.Text == CLanguage.Lan("所有工位"))
                    condition.StatName = "";
                else
                    condition.StatName = cmbStatName.Text;

                condition.StatGuid = txtAlarmStatGuid.Text;

                condition.ErrNo = System.Convert.ToInt32(txtAlarmCode.Text);

                condition.bAlarm = cmbbAlarm.SelectedIndex - 1;

                if (!CWeb2.GetAlarmRecord(condition, out alarmList, out er))
                {
                    labT1.Text = er;
                    labT1.ForeColor = Color.Red;
                    return;
                }

                DataTable dt = new DataTable();

                dt.Columns.Add(CLanguage.Lan("编号"));
                dt.Columns.Add(CLanguage.Lan("工位名称"));
                dt.Columns.Add(CLanguage.Lan("工位标识"));
                dt.Columns.Add(CLanguage.Lan("报警状态"));
                dt.Columns.Add(CLanguage.Lan("报警代号"));
                dt.Columns.Add(CLanguage.Lan("报警时间"));
                dt.Columns.Add(CLanguage.Lan("报警信息"));
                dt.Columns.Add(CLanguage.Lan("备注") + "1");
                dt.Columns.Add(CLanguage.Lan("备注") + "2");

                for (int i = 0; i < alarmList.Count; i++)
                {
                    dt.Rows.Add(i + 1,
                                alarmList[i].StatName, alarmList[i].StatGuid,
                                (alarmList[i].bAlarm == 1 ? CLanguage.Lan("报警") : CLanguage.Lan("解除")), alarmList[i].ErrNo,
                                 alarmList[i].HappenTime, alarmList[i].AlarmInfo,
                                alarmList[i].Remark1, alarmList[i].Remark2
                                );
                }

                AlarmView.DataSource = dt;

                labT2.Text = CLanguage.Lan("查询数量") + "【" + alarmList.Count.ToString() + "】";
                labT2.ForeColor = Color.Blue;
            }
            catch (Exception ex)
            {
                labT2.Text = ex.ToString();
                labT2.ForeColor = Color.Red;
            }
            finally
            {
                btnQueryAlarmList.Enabled = true;
            }
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                btnExport.Enabled = false;

                if (AlarmView.RowCount == 0)
                    return;

                SaveFileDialog saveFiledlg = new SaveFileDialog();
                saveFiledlg.InitialDirectory = Application.StartupPath;
                saveFiledlg.Filter = "csv files (*.csv)|*.csv";
                if (saveFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                string filePath = saveFiledlg.FileName;
                StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
                string strWrite = string.Empty;

                strWrite += "编号,工位名称,工位标识,报警状态,报警代号,报警时间,报警信息,备注1,备注2";
                sw.WriteLine(strWrite);
                for (int i = 0; i < AlarmView.Rows.Count; i++)
                {
                    strWrite = string.Empty;
                    strWrite += AlarmView.Rows[i].Cells[0].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[1].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[2].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[3].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[4].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[5].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[6].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[7].Value.ToString() + ",";
                    strWrite += AlarmView.Rows[i].Cells[8].Value.ToString() + ",";
                    sw.WriteLine(strWrite);
                }

                sw.Flush();
                sw.Close();
                sw = null;
            }
            catch (Exception ex)
            {
                labT2.Text = ex.ToString();
                labT2.ForeColor = Color.Red;
            }
            finally
            {
                btnExport.Enabled = true;
            }
        }
        private void btnFixNumQuery_Click(object sender, EventArgs e)
        {
            try
            {
                btnFixNumQuery.Enabled = false;

                string er = string.Empty;

                if (!web_check_flag)
                {
                    labT2.Text = CLanguage.Lan("无法连接") + "[" + CYOHOOApp.UlrWeb + "]";
                    labT2.ForeColor = Color.Red;
                    return;
                }

                FixNumView.Rows.Clear();

                List<CWeb2.CFixUseNum> idCardList = null;

                CWeb2.CFixCondition condition = new CWeb2.CFixCondition()
                {
                    FlowName = txtStatName.Text,
                    IdCard = txtFixNumIdCard.Text,
                    SlotNo = cmbFixNumSlotNo.SelectedIndex - 1
                };

                if (!CWeb2.QueryIdCardUseNum(condition, out idCardList, out er))
                {
                    labT2.Text = er;
                    labT2.ForeColor = Color.Red;
                    return;
                }

                for (int i = 0; i < idCardList.Count; i++)
                {
                    FixNumView.Rows.Add(i + 1, idCardList[i].IdCard, idCardList[i].SlotNo,
                                       idCardList[i].TTNum, idCardList[i].FailNum, idCardList[i].ConFailNum);
                }

            }
            catch (Exception ex)
            {
                labT2.Text = ex.ToString();
                labT2.ForeColor = Color.Red;
            }
            finally
            {
                btnFixNumQuery.Enabled = true;
            }
        }
        #endregion

        #region 委托
        private delegate void check_web_status_handler();
        /// <summary>
        /// 检查web状态
        /// </summary>
        private void check_web_status()
        {
            try
            {
                string er = string.Empty;

                string ver = string.Empty;

                if (!CWeb2.CheckSystem(CYOHOOApp.UlrWeb, out ver, out er))
                    return;

                web_check_flag = true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion     
    }
}
