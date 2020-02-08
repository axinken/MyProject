using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.PLUGINS;

namespace GJ.YOHOO.HIPOT
{
    public partial class FrmVersion : Form,IChildMsg 
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
        public FrmVersion()
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

        #region 字段
        private enum ELevel
        {
            主题,
            信息,
            帮助,
            警示,
            错误,
            加密,
            禁用,
            人工
        }
        private class CVersion
        {
            public int level;
            public string upDate;
            public string upContext;
            public string upVer;
            public string upAthour;
        }
        private List<CVersion> updateVer = new List<CVersion>(); 
        #endregion

        #region 面板回调函数
        private void FrmVersion_Load(object sender, EventArgs e)
        {
            writeVersion();

            refreshView();

            panel1.Dock = DockStyle.Fill;   
        }
        #endregion

        #region 方法
        private void writeVersion()
        {
            updateVer.Clear();

            CVersion ver = null;

            ver = new CVersion();
            ver.level = (int)ELevel.主题;
            ver.upDate = "2019/11/12";
            ver.upContext = "深圳坤兴自动老化测试线" + "[AUTO-CSX-KX191101-2]";
            ver.upVer = "V1.0.0";
            ver.upAthour = "kp.lin";
            updateVer.Add(ver);

        }
        private void refreshView()
        {
            VerView.Rows.Clear();
 
            for (int i = 0; i < updateVer.Count; i++)
            {
                VerView.Rows.Add(
                                 (i + 1),
                                 imageList1.Images[updateVer[i].level.ToString()],
                                 updateVer[i].upDate,
                                 updateVer[i].upContext,
                                 updateVer[i].upVer,
                                 updateVer[i].upAthour
                                 );
                VerView.Rows[i].Cells[3].Style.Alignment = DataGridViewContentAlignment.MiddleLeft; 
            }        
        }
        #endregion  

    }
}
