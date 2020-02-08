using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GJ.PLUGINS;
using GJ.COM;
namespace GJ.YOHOO.UNLOAD
{
    public partial class FrmReport : Form,IChildMsg
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
        }
        #endregion

        #region 构造函数
        public FrmReport()
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
         splitContainer1.Panel1.GetType().GetProperty("DoubleBuffered",
                                        System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.NonPublic)
                                        .SetValue(splitContainer1.Panel1, true, null);
         splitContainer1.Panel2.GetType().GetProperty("DoubleBuffered",
                                        System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.NonPublic)
                                        .SetValue(splitContainer1.Panel1, true, null);
      }
        #endregion

        #region 字段
        private string dataFolder = CGlobalPara.SysPara.Report.ReportPath;
        private string runlogFile = string.Empty;
        #endregion

        #region 面板回调函数
        private void FrmReport_Load(object sender, EventArgs e)
        {
            RefreshView();
        }
        private void treeFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
         if (e.Node.ToolTipText == "")
            return;
         runlogFile = e.Node.ToolTipText;
         this.Text = CLanguage.Lan("测试数据查询");
         labStatus.Text = CLanguage.Lan("正在加载.");
         labStatus.BackColor = Color.Red;        
         progressBar1.Value =0;
         rtbRunLog.Clear(); 
         timer1.Interval = 10;
         timer1.Start(); 
       }
        private void timer1_Tick(object sender, EventArgs e)
        {
         timer1.Stop();
         rtbRunLog.LoadFile(runlogFile, RichTextBoxStreamType.PlainText);
         rtbRunLog.Text = rtbRunLog.Text.Replace(",", "\t"); 
         labStatus.Text = CLanguage.Lan("加载完毕.");
         this.Text = CLanguage.Lan("测试数据查询") + "--" + runlogFile;
         labStatus.BackColor = Color.Green;
         progressBar1.Value = progressBar1.Maximum;  
        }
        #endregion

        #region 方法
        private void RefreshView()
        {
            try
            {
                treeFiles.Nodes.Clear();
                treeFiles.ImageList = imageList1;
                treeFiles.Nodes.Add(CLanguage.Lan("测试数据查询"));
                treeFiles.Nodes[0].ImageIndex = 5;
                treeFiles.Nodes[0].SelectedImageIndex = 5;
                TreeNode foldNode = treeFiles.Nodes[0];
                if (!Directory.Exists(dataFolder))
                    return;
                //文件夹       
                foldNode.Nodes.Add(dataFolder);
                foldNode.Nodes[0].ImageIndex = 1;
                foldNode.Nodes[0].SelectedImageIndex = 1;
                //机种文件夹
                string[] modelName = Directory.GetDirectories(dataFolder);
                if (modelName.Length == 0)
                    return;
                TreeNode modelNode = foldNode.Nodes[0];
                for (int i = 0; i < modelName.Length; i++)
                {
                    modelNode.Nodes.Add(Path.GetFileNameWithoutExtension(modelName[i]));
                    modelNode.Nodes[i].ImageIndex = 3;
                    modelNode.Nodes[i].SelectedImageIndex = 2;
                    //测试文件
                    string[] logFiles = Directory.GetFiles(modelName[i]);
                    if (logFiles.Length == 0)
                        continue;
                    TreeNode fileNode = modelNode.Nodes[i];
                    for (int k = 0; k < logFiles.Length; k++)
                    {
                        fileNode.Nodes.Add(Path.GetFileNameWithoutExtension(logFiles[k]));
                        fileNode.Nodes[k].ToolTipText = logFiles[k];
                        fileNode.Nodes[k].ImageIndex = 4;
                        fileNode.Nodes[k].SelectedImageIndex = 4;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

    }
}
