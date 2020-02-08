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
using GJ.DEV.FCMB;
using GJ.PLUGINS;
using GJ.USER.APP;
using GJ.DEV.LED;
namespace GJ.YOHOO.LOADUP
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
        /// 初始化控件
        /// </summary>
        private void IntialControl()
        {
            cmbQCM.Items.Clear();

            List<string> qcvList = new List<string>();

            string[] strQcv = CYOHOOApp.QCM_Type.Split(',');

            for (int i = 0; i < strQcv.Length; i++)
            {
                if (!qcvList.Contains(strQcv[i]) && Enum.IsDefined(typeof(EQCM), strQcv[i]))
                {
                    qcvList.Add(strQcv[i]);

                    cmbQCM.Items.Add(strQcv[i]);
                }
            }

            if(cmbQCM.Items.Count>2)
              cmbQCM.SelectedIndex = 1;
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

        #region 字段
        private CModelPara modelPara = new CModelPara();
        private OpenFileDialog openFiledlg = new OpenFileDialog();
        private SaveFileDialog saveFiledlg = new SaveFileDialog(); 
        #endregion

        #region 面板回调函数
        private void FrmModel_Load(object sender, EventArgs e)
        {
            txtACVL.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtACVH.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtVmin.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtVmax.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtVon.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtLoadSet.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtLoadmin.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtLoadmax.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            cmbMode.Items.Clear();
            string[] modes = Enum.GetNames(typeof(EMODE));
            for (int i = 0; i < modes.Length; i++)
            {
                cmbMode.Items.Add(modes[i]);
            }
            cmbMode.SelectedIndex = 0;
        }
        private void OnTextKeyPressIsNumber(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)'.')
                e.Handled = true;
        }
        private void btnNew_Click(object sender, EventArgs e)
        {
            clr();
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            open();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            save();
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            string er = string.Empty;

            CReflect.SendWndMethod(_father, EMessType.OnMessage, out er, new object[] { "btnExit", (int)ElPara.退出, 0 });
        }
        private void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMode.SelectedIndex == 0)
            {
                labLoadSet.Text = CLanguage.Lan("负载设置") + "(A):";
            }
            else
            {
                labLoadSet.Text = CLanguage.Lan("负载设置") + "(V):";
            }
        }
        #endregion

        #region 文件方法
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

            cmbACV.SelectedIndex = 0;
            txtACVL.Text = "0";
            txtACVH.Text = "0";

            txtVname.Text = "+5V";
            txtVmin.Text = "0";
            txtVmax.Text = "0";
            cmbMode.SelectedIndex = 0;
            txtVon.Text = "0";
            labLoadSet.Text = CLanguage.Lan("负载设置") + "(A):";
            txtLoadSet.Text = "0";
            txtLoadmin.Text = "0";
            txtLoadmax.Text = "0";
            txtVOffset.Text = "0"; 

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
                openFiledlg.Filter = "spec files (*.pwr)|*.pwr";
                if (openFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                modelPath = openFiledlg.FileName;
                CSerializable<CModelPara>.ReadXml(modelPath, ref modelPara);

                txtModel.Text = modelPara.Base.Model;
                txtCustom.Text = modelPara.Base.Custom;
                txtVersion.Text = modelPara.Base.Version;
                txtReleaseName.Text = modelPara.Base.ReleaseName;
                //pickerDate.Value = System.Convert.ToDateTime(modelPara.Base.ReleaseDate);

                cmbACV.Text = modelPara.OutPut.ACVolt.ToString();
                txtACVL.Text = modelPara.OutPut.ACvMin.ToString();
                txtACVH.Text = modelPara.OutPut.ACvMax.ToString();    

                txtVname.Text = modelPara.OutPut.Vname;
                txtVmin.Text = modelPara.OutPut.Vmin.ToString();
                txtVmax.Text = modelPara.OutPut.Vmax.ToString();
                cmbMode.SelectedIndex = modelPara.OutPut.LoadMode;
                txtVon.Text = modelPara.OutPut.LoadVon.ToString();
                txtLoadSet.Text = modelPara.OutPut.LoadSet.ToString();
                txtLoadmin.Text = modelPara.OutPut.LoadMin.ToString();
                txtLoadmax.Text = modelPara.OutPut.LoadMax.ToString();
                txtVOffset.Text = modelPara.OutPut.VOffSet.ToString();

                chkQCM.Checked = modelPara.OutPut.ChkQCM;
                cmbQCM.Text = ((EQCM)modelPara.OutPut.QCM).ToString();
                txtQCV.Text = modelPara.OutPut.QCV.ToString();
                chkDD.Checked = modelPara.OutPut.ChkDD;
                chkDV1.Checked = modelPara.OutPut.ChkDG[0];
                chkDV2.Checked = modelPara.OutPut.ChkDG[1];
                chkDV3.Checked = modelPara.OutPut.ChkDG[2];
                chkDV4.Checked = modelPara.OutPut.ChkDG[3];

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
                    fileDirectry = CGlobalPara.SysPara.Report.ModelPath;
                saveFiledlg.InitialDirectory = fileDirectry;
                saveFiledlg.Filter = "Spec files (*.pwr)|*.pwr";
                saveFiledlg.FileName = txtModel.Text;
                if (saveFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                modelPath = saveFiledlg.FileName;

                modelPara.Base.Model = txtModel.Text;
                modelPara.Base.Custom = txtCustom.Text;
                modelPara.Base.Version = txtVersion.Text;
                modelPara.Base.ReleaseName = txtReleaseName.Text;
                modelPara.Base.ReleaseDate = pickerDate.Value.Date.ToString();

                modelPara.OutPut.ACVolt = System.Convert.ToInt16(cmbACV.Text);
                modelPara.OutPut.ACvMin = System.Convert.ToDouble(txtACVL.Text);
                modelPara.OutPut.ACvMax=System.Convert.ToDouble(txtACVH.Text);

                modelPara.OutPut.Vname=txtVname.Text;
                modelPara.OutPut.Vmin=System.Convert.ToDouble(txtVmin.Text);
                modelPara.OutPut.Vmax=System.Convert.ToDouble(txtVmax.Text);
                modelPara.OutPut.LoadMode=cmbMode.SelectedIndex;
                modelPara.OutPut.LoadVon=System.Convert.ToDouble(txtVon.Text);
                modelPara.OutPut.LoadSet=System.Convert.ToDouble(txtLoadSet.Text);
                modelPara.OutPut.LoadMin=System.Convert.ToDouble(txtLoadmin.Text);
                modelPara.OutPut.LoadMax=System.Convert.ToDouble(txtLoadmax.Text);
                modelPara.OutPut.VOffSet = System.Convert.ToDouble(txtVOffset.Text);

                modelPara.OutPut.ChkQCM = chkQCM.Checked;
                modelPara.OutPut.QCM = (int)((EQCM)Enum.Parse(typeof(EQCM), cmbQCM.Text));
                modelPara.OutPut.QCV = System.Convert.ToDouble(txtQCV.Text);
                modelPara.OutPut.ChkDD = chkDD.Checked;
                modelPara.OutPut.ChkDG[0] = chkDV1.Checked;
                modelPara.OutPut.ChkDG[1] = chkDV2.Checked;
                modelPara.OutPut.ChkDG[2] = chkDV3.Checked;
                modelPara.OutPut.ChkDG[3] = chkDV4.Checked;

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
