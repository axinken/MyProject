using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GJ.COM;
using GJ.PLUGINS;
using GJ.DEV.HIPOT;
using GJ.KunX.HIPOT.Udc; 

namespace GJ.KunX.HIPOT
{
    public partial class FrmModel : Form,IChildMsg
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
        public FrmModel()
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
            hpChan = new udcHPChan();
            hpChan.Dock = DockStyle.Fill;
            panel1.Controls.Add(hpChan, 0, 3);

            hpPara = new udcHPPara();
            hpPara.Dock = DockStyle.Fill;
            panel3.Controls.Add(hpPara, 0, 1);
            hpPara.OnStepChange.OnEvent += new COnEvent<udcHPPara.CStepChangeArgs>.OnEventHandler(OnStepValChange); 
            hpPara.SetItem(CHPPara.IniStep(EStepName.AC,0));

            c_HPStepName = new string[] {
                                         CLanguage.Lan("交流电压耐压(AC)测试"),
                                         CLanguage.Lan("直流电压耐压(DC)测试"), 
                                         CLanguage.Lan("绝缘阻抗(IR)测试"), 
                                         CLanguage.Lan("开短路侦测(OS)测试")
                                         };

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
        }
        #endregion

        #region 面板拖拉
        private bool sourceDrag = false;
        /// <summary>
        /// 当鼠标移动到接收容器的上方会触发DragEnter消息,DragEventArg包含Effect和KeyStatus属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listSource_DragEnter(object sender, DragEventArgs e)
        {
            if (e.KeyState == 9) //Ctrl键
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.Move;         
        }
        /// <summary>
        /// 当鼠标松开时触发DrapDrop消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listSource_DragDrop(object sender, DragEventArgs e)
        {
            if (!sourceDrag)
            {
                if (listTarget.SelectedIndex < 0)
                    return;
                step.RemoveAt(listTarget.SelectedIndex);
                listTarget.Items.RemoveAt(listTarget.SelectedIndex);
                for (int i = 0; i < step.Count; i++)
                    step[i].stepNo = i;
            }               
        }
        /// <summary>
        /// 调用拖和放使用DoDragDrop方法-->在MouseDown事件中实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listSource_MouseDown(object sender, MouseEventArgs e)
        {
            if (listSource.SelectedIndex < 0)
                return;
            sourceDrag = true;
            if (e.Clicks == 2) //双击鼠标添加测试项目
            {
                step.Add(CHPPara.IniStep((EStepName)(listSource.SelectedIndex), listTarget.Items.Count));
                listTarget.Items.Add(listSource.Items[listSource.SelectedIndex]);
            }
            else
            {
                if (e.Button == MouseButtons.Left) //鼠标按下左键
                {
                    DragDropEffects dragDropResult = listSource.DoDragDrop(listSource.Items[listSource.SelectedIndex],
                                                     DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }
        /// <summary>
        /// 当鼠标移动到接收容器的上方会触发DragEnter消息,DragEventArg包含Effect和KeyStatus属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">>以表示鼠标移动或复制;KeyStatus:监测Ctrl,Alt,Shift,按Ctrl表示复制</param>
        private void listTarget_DragEnter(object sender, DragEventArgs e)
        {
            if (e.KeyState == 9) //Ctrl键
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.Move; 
        }
        /// <summary>
        /// 当鼠标松开时触发DrapDrop消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listTarget_DragDrop(object sender, DragEventArgs e)
        {
            if (sourceDrag)
            {
                if (listTarget.SelectedIndex < 0)
                {
                    step.Add(CHPPara.IniStep((EStepName)(listSource.SelectedIndex), listTarget.Items.Count));
                    listTarget.Items.Add(e.Data.GetData(DataFormats.Text));
                }
                else
                {
                    step.Insert(listTarget.SelectedIndex, CHPPara.IniStep((EStepName)(listSource.SelectedIndex),
                                listTarget.SelectedIndex));
                    listTarget.Items.Insert(listTarget.SelectedIndex, e.Data.GetData(DataFormats.Text));
                }
            }            
        }
        /// <summary>
        /// 调用拖和放使用DoDragDrop方法-->在MouseDown事件中实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listTarget_MouseDown(object sender, MouseEventArgs e)
        {
            if (listTarget.SelectedIndex < 0)
                return;
            sourceDrag = false;
            if (e.Button == MouseButtons.Left) //鼠标按下左键
            {
                if (listTarget.SelectedIndex >= 0 && listTarget.SelectedIndex < step.Count)
                {
                    step[listTarget.SelectedIndex].stepNo = listTarget.SelectedIndex;
                    hpPara.SetItem(step[listTarget.SelectedIndex]);
                }
                DragDropEffects dragDropResult = listTarget.DoDragDrop(listTarget.Items[listTarget.SelectedIndex],
                                                    DragDropEffects.Move | DragDropEffects.Copy);
            }          
        }
        private void listTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listTarget.SelectedIndex >= 0 && listTarget.SelectedIndex < step.Count)
            {
                step[listTarget.SelectedIndex].stepNo = listTarget.SelectedIndex;
                hpPara.SetItem(step[listTarget.SelectedIndex]);
            }
        }

        #endregion

        #region  面板回调函数
        private void FrmModel_Load(object sender, EventArgs e)
        {
            listSource.Items.Clear();

            for (int i = 0; i < c_HPStepName.Length; i++)
                listSource.Items.Add(c_HPStepName[i]);

            clr();
        }
        private void btnNew_Click(object sender, EventArgs e)
        {
            clr();
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            open();

            InitListTarget();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtHPModel.Text == string.Empty)
            {
                MessageBox.Show(CLanguage.Lan("高压机机种名不能为空"),"Tip",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);

                return;
            }

            save();
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            string er = string.Empty;

            CReflect.SendWndMethod(_father, EMessType.OnMessage, out er, new object[] { "btnExit", (int)ElPara.退出, 0 });
        }
        #endregion

        #region 面板控件
        private udcHPChan hpChan = null;
        private udcHPPara hpPara = null;
        #endregion

        #region 面板事件响应
        private void OnStepValChange(object sender, udcHPPara.CStepChangeArgs e)
        {
            if (e.stepNo < step.Count)
                step[e.stepNo].para[e.itemNo].setVal = e.itemVal;
        }
        #endregion

        #region 字段
        private string[] c_HPStepName = null;
        private List<CHPPara.CStep> step = new List<CHPPara.CStep>();
        #endregion

        #region 方法
        private void InitListTarget()
        {
            listTarget.Items.Clear();
            for (int i = 0; i < step.Count; i++)
                listTarget.Items.Add(CLanguage.Lan(step[i].des));
        }
        #endregion

        #region 文件方法
        private OpenFileDialog openFiledlg = new OpenFileDialog();
        private SaveFileDialog saveFiledlg = new SaveFileDialog();
        /// <summary>
        /// 新建
        /// </summary>
        private void clr()
        {
            txtModel.Text = "";
            txtCustom.Text = "";
            txtVersion.Text = "";
            txtReleaseName.Text = "";
            pickerDate.Value = DateTime.Now;

        }
        /// <summary>
        /// 打开
        /// </summary>
        private void open()
        {
            try
            {
                string modelPath = string.Empty;
                string fileDirectry = string.Empty;
                fileDirectry = CGlobalPara.SysPara.Report.ModelPath;
                openFiledlg.InitialDirectory = fileDirectry;
                openFiledlg.Filter = "HP files (*.hp)|*.hp";
                if (openFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                modelPath = openFiledlg.FileName;

                CModelPara modelPara = new CModelPara();

                CSerializable<CModelPara>.ReadXml(modelPath, ref modelPara);

                txtModel.Text = modelPara.Base.Model;
                txtCustom.Text = modelPara.Base.Custom;
                txtVersion.Text = modelPara.Base.Version;

                txtReleaseName.Text = modelPara.Base.ReleaseName;

                if (modelPara.Base.ReleaseDate != null && modelPara.Base.ReleaseDate!=string.Empty)
                {
                    pickerDate.Value = System.Convert.ToDateTime(modelPara.Base.ReleaseDate);
                }
                txtHPModel.Text = modelPara.Base.HPModel;  

                hpChan.mHpChan = modelPara.Para.HpCH;
                hpChan.mIoChan = modelPara.Para.IoCH;

                step = modelPara.Para.Step;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }      
        /// <summary>
        ///保存
        /// </summary>
        private void save()
        {
            try
            {
                string modelPath = string.Empty;

                string fileDirectry = string.Empty;
                
                if (CGlobalPara.SysPara.Report.ModelPath == "")
                {
                    fileDirectry = Application.StartupPath + "\\Model";
                    if (!Directory.Exists(fileDirectry))
                        Directory.CreateDirectory(fileDirectry);
                }
                else
                {
                    fileDirectry = CGlobalPara.SysPara.Report.ModelPath;
                }
                
                saveFiledlg.InitialDirectory = fileDirectry;
                saveFiledlg.Filter = "HP files (*.hp)|*.hp";
                saveFiledlg.FileName = txtModel.Text;
                
                if (saveFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                
                modelPath = saveFiledlg.FileName;

                CModelPara modelPara = new CModelPara();

                modelPara.Base.Model = txtModel.Text;
                modelPara.Base.Custom = txtCustom.Text;
                modelPara.Base.Version = txtVersion.Text;

                modelPara.Base.ReleaseName = txtReleaseName.Text;
                modelPara.Base.ReleaseDate = pickerDate.Value.Date.ToString();
                modelPara.Base.HPModel = txtHPModel.Text;  

                modelPara.Para.HpCH = hpChan.mHpChan;
                modelPara.Para.IoCH = hpChan.mIoChan;

                modelPara.Para.Step = step;

                CSerializable<CModelPara>.WriteXml(modelPath, modelPara);

                MessageBox.Show(CLanguage.Lan("机种参数保存成功."), "Tip", MessageBoxButtons.OK, MessageBoxIcon.Information);                              
                               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        #endregion
        
    }
}
