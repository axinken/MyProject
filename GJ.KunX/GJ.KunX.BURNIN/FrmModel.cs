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
using GJ.UI;
using GJ.USER.APP;
using GJ.DEV.FCMB;
namespace GJ.KunX.BURNIN
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

            cmbQCVType.Items.Clear();

            List<string> qcvList=new List<string>();

            string[] strQcv = CKunXApp.QCM_Type.Split(',');

            for (int i = 0; i < strQcv.Length; i++)
            {
                if (!qcvList.Contains(strQcv[i]) && Enum.IsDefined(typeof(EQCM), strQcv[i]))
                {
                    qcvList.Add(strQcv[i]);

                    cmbQCVType.Items.Add(strQcv[i]);
                }
            }

            _outPut = new COutPut_List[OUTPUT_MAX];

            for (int i = 0; i < OUTPUT_MAX; i++)
                _outPut[i] = new COutPut_List();

            FrmOutPut.OnSaveArgs.OnEvent += new COnEvent<FrmOutPut.COutPutArgs>.OnEventHandler(OnOutPutSaveArgs);


            _onOff = new COnOff_List[ONOFF_MAX];

            for (int i = 0; i < ONOFF_MAX; i++)
                _onOff[i] = new COnOff_List();

            FrmOnOff.OnSaveArgs.OnEvent += new COnEvent<FrmOnOff.COnOffArgs>.OnEventHandler(OnOnOffSaveArgs); 

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

        #region 字段
        private CModelPara modelPara = new CModelPara();
        private OpenFileDialog openFiledlg = new OpenFileDialog();
        private SaveFileDialog saveFiledlg = new SaveFileDialog();
        #endregion

        #region 面板回调函数
        private void FrmModel_Load(object sender, EventArgs e)
        {
            txtBITime.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtTSet.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtTLP.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtTHP.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtHAlarm.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtTOpen.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtTClose.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);

            clr();
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
        private void cmbDCVNum_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }        
        #endregion

        #region 面板控件
        private udcChartOnOff _udcChart = null;
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

            txtBITime.Text = "2.0";
            cmbACV.SelectedIndex = 1;
            cmbDCVNum.SelectedIndex = 0;
            cmbQCVType.SelectedIndex = 0;

            OutputView.Rows.Clear();

            OnOffView.Rows.Clear();

            _outPutNum = 0;

            _onOffNum = 0; 
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
                openFiledlg.Filter = "BI files (*.bi)|*.bi";
                if (openFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                modelPath = openFiledlg.FileName;
                CSerializable<CModelPara>.ReadXml(modelPath, ref modelPara);

                txtModel.Text = modelPara.Base.Model;
                txtCustom.Text = modelPara.Base.Custom;
                txtVersion.Text = modelPara.Base.Version;
                txtReleaseName.Text = modelPara.Base.ReleaseName;
                //pickerDate.Value = System.Convert.ToDateTime(modelPara.Base.ReleaseDate);

                txtTSet.Text = modelPara.Temp.TSet.ToString();
                txtTLP.Text = modelPara.Temp.TLP.ToString();
                txtTHP.Text = modelPara.Temp.THP.ToString();
                txtHAlarm.Text = modelPara.Temp.THAlarm.ToString();
                txtTOpen.Text = modelPara.Temp.TOPEN.ToString();
                txtTClose.Text = modelPara.Temp.TCLOSE.ToString();

                txtBITime.Text = modelPara.Para.BITime.ToString();
                cmbACV.Text = modelPara.Para.ACV.ToString();
                cmbDCVNum.Text = modelPara.Para.OutPut_Chan.ToString();
                cmbQCVType.Text = ((EQCM)modelPara.Para.QCV_Type).ToString();
                txt110V.Text = modelPara.Para.AC_110V.ToString();
                txt220V.Text = modelPara.Para.AC_220V.ToString();
                txt250V.Text = modelPara.Para.AC_264V.ToString();
                txt360V.Text = modelPara.Para.AC_380V.ToString();

                txtVOffSet.Text = modelPara.Para.VOffset.ToString();  

                _outPutNum = modelPara.Para.OutPut_Num;

                _outPutChan = modelPara.Para.OutPut_Chan;

                _onOffNum = modelPara.Para.OnOff_Num; 

                for (int i = 0; i < modelPara.Para.OutPut_Num; i++)
                    _outPut[i] = modelPara.OutPut[i].Clone();

                for (int i = 0; i < modelPara.Para.OnOff_Num; i++)
                    _onOff[i] = modelPara.OnOff[i].Clone();

                refreshOutPutView();

                refreshOnOffView();

                refreshChart();
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
                saveFiledlg.Filter = "BI files (*.bi)|*.bi";
                saveFiledlg.FileName = txtModel.Text;
                if (saveFiledlg.ShowDialog() != DialogResult.OK)
                    return;
                modelPath = saveFiledlg.FileName;

                modelPara.Base.Model = txtModel.Text;
                modelPara.Base.Custom = txtCustom.Text;
                modelPara.Base.Version = txtVersion.Text;
                modelPara.Base.ReleaseName = txtReleaseName.Text;
                modelPara.Base.ReleaseDate = pickerDate.Value.Date.ToString();

                modelPara.Temp.TSet = System.Convert.ToDouble(txtTSet.Text);
                modelPara.Temp.TLP = System.Convert.ToDouble(txtTLP.Text);
                modelPara.Temp.THP = System.Convert.ToDouble(txtTHP.Text);
                modelPara.Temp.THAlarm = System.Convert.ToDouble(txtHAlarm.Text);
                modelPara.Temp.TOPEN = System.Convert.ToDouble(txtTOpen.Text);
                modelPara.Temp.TCLOSE = System.Convert.ToDouble(txtTClose.Text);

                modelPara.Para.BITime = System.Convert.ToDouble(txtBITime.Text);
                modelPara.Para.ACV = System.Convert.ToInt32(cmbACV.Text);
                modelPara.Para.QCV_Type = (int)((EQCM)Enum.Parse(typeof(EQCM), cmbQCVType.Text));
                modelPara.Para.OutPut_Chan = System.Convert.ToInt32(cmbDCVNum.Text);
                modelPara.Para.OutPut_Num= _outPutNum;
                modelPara.Para.OnOff_Num = _onOffNum;     
                modelPara.Para.AC_110V = System.Convert.ToInt16(txt110V.Text);
                modelPara.Para.AC_220V = System.Convert.ToInt16(txt220V.Text);                
                modelPara.Para.AC_264V = System.Convert.ToInt16(txt250V.Text);
                modelPara.Para.AC_380V = System.Convert.ToInt16(txt360V.Text);

                modelPara.Para.VOffset = System.Convert.ToDouble(txtVOffSet.Text);
  
                for (int i = 0; i <modelPara.Para.OutPut_Num; i++)
                    modelPara.OutPut[i] = _outPut[i].Clone();

                for (int i = 0; i < modelPara.Para.OnOff_Num; i++)
                    modelPara.OnOff[i] = _onOff[i].Clone(); 

                CSerializable<CModelPara>.WriteXml(modelPath, modelPara);  
                
                MessageBox.Show(CLanguage.Lan("机种参数保存成功."), CLanguage.Lan("机种保存"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region 输出规格
        /// <summary>
        /// 最多输出规格
        /// </summary>
        private const int OUTPUT_MAX=8;
        /// <summary>
        /// 输出列表数
        /// </summary>
        private int _outPutNum = 0;
        /// <summary>
        /// 输出通道数
        /// </summary>
        private int _outPutChan = 0;
        /// <summary>
        /// 输出规格列表
        /// </summary>
        private COutPut_List[] _outPut = null;
        /// <summary>
        /// 刷新输出规格列表
        /// </summary>
        private void refreshOutPutView()
        {
            try
            {
                OutputView.Rows.Clear();

                for (int i = 0; i < modelPara.Para.OutPut_Num; i++)
                    OutputView.Rows.Add(i + 1, _outPut[i].Describe);  
                
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        private void SpecView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                DataGridViewTextBoxCell txtCell = (DataGridViewTextBoxCell)OutputView.Rows[e.RowIndex].DataGridView[e.ColumnIndex, e.RowIndex];

                txtCell.Style.BackColor = Color.Cyan;
            }
        }
        private void SpecView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                DataGridViewTextBoxCell txtCell = (DataGridViewTextBoxCell)OutputView.Rows[e.RowIndex].DataGridView[e.ColumnIndex, e.RowIndex];

                txtCell.Style.BackColor = Color.White;

                _outPut[e.RowIndex].Describe = txtCell.Value.ToString(); 
            }
        }
        private void btnVAdd_Click(object sender, EventArgs e)
        {
            if (_outPutNum >= OUTPUT_MAX)
            {
                MessageBox.Show(CLanguage.Lan("设置输出规格组数超过最多") + "【" + OUTPUT_MAX.ToString() + "】", CLanguage.Lan("输出规格"),
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                return;
            }

            OutputView.Rows.Add(OutputView.Rows.Count+1, "");

            _outPutNum++;

            _outPutChan = cmbDCVNum.SelectedIndex + 1;

        }
        private void btnVEdit_Click(object sender, EventArgs e)
        {
            if (OutputView.SelectedCells.Count == 0)
                return;
            
            if (OutputView.SelectedCells[0].RowIndex < 0)
                return;

             int row = OutputView.SelectedCells[0].RowIndex;

             _outPut[row].Describe = OutputView.Rows[row].Cells[1].Value.ToString();

             FrmOutPut.CreateInstance(row,_outPutChan, _outPut[row]).Show();

        }
        private void btnVDel_Click(object sender, EventArgs e)
        {
            if (OutputView.SelectedCells.Count == 0)
                return;

            if (OutputView.SelectedCells[0].RowIndex < 0)
                return;

            int row = OutputView.SelectedCells[0].RowIndex;

            OutputView.Rows.RemoveAt(row);

            _outPutNum--;

            for (int i = row; i < OUTPUT_MAX-1; i++)
                _outPut[i] = _outPut[i + 1].Clone();
            
            for (int i = 0; i < OutputView.Rows.Count; i++)
                OutputView.Rows[i].Cells[0].Value = (i + 1).ToString();                 
            
        }
        private void OnOutPutSaveArgs(object sender, FrmOutPut.COutPutArgs e)
        {
            _outPut[e.idNo] = e.outPut.Clone();            
        }
        #endregion

        #region ON/OFF规格
        /// <summary>
        /// 最多ON/OFF段数
        /// </summary>
        private const int ONOFF_MAX = 8;
        /// <summary>
        /// 输出ONOFF段数
        /// </summary>
        private int _onOffNum = 0;
        /// <summary>
        /// ONOFF列表
        /// </summary>
        private COnOff_List[] _onOff = null;
        /// <summary>
        /// 刷新输出规格列表
        /// </summary>
        private void refreshOnOffView()
        {
            try
            {
                OnOffView.Rows.Clear();

                for (int i = 0; i < modelPara.Para.OnOff_Num; i++)
                    OnOffView.Rows.Add(i + 1, _onOff[i].Describe);

            }
            catch (Exception)
            {

                throw;
            }
        }
        private void OnOffView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                DataGridViewTextBoxCell txtCell = (DataGridViewTextBoxCell)OnOffView.Rows[e.RowIndex].DataGridView[e.ColumnIndex, e.RowIndex];

                txtCell.Style.BackColor = Color.Cyan;

                _onOff[e.RowIndex].Describe = txtCell.Value.ToString(); 
            }
        }
        private void OnOffView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                DataGridViewTextBoxCell txtCell = (DataGridViewTextBoxCell)OnOffView.Rows[e.RowIndex].DataGridView[e.ColumnIndex, e.RowIndex];

                txtCell.Style.BackColor = Color.White;
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_outPutNum == 0)
            {
                MessageBox.Show(CLanguage.Lan("请先设置输出规格,再设置ON/OFF参数"), "ON/OFF",
                                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            if (_onOffNum >= ONOFF_MAX)
            {
                MessageBox.Show(CLanguage.Lan("设置ON/OFF组数超过最多") + "【" + ONOFF_MAX.ToString() + "】", "ON/OFF",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            OnOffView.Rows.Add(OnOffView.Rows.Count + 1, "");

            _onOffNum++;

        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (OnOffView.SelectedCells.Count == 0)
                return;

            if (OnOffView.SelectedCells[0].RowIndex < 0)
                return;

            int row = OnOffView.SelectedCells[0].RowIndex;

            _onOff[row].Describe = OnOffView.Rows[row].Cells[1].Value.ToString();

            FrmOnOff.CreateInstance(row,_outPutNum, _onOff[row]).Show();
        }
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (OnOffView.SelectedCells.Count == 0)
                return;

            if (OnOffView.SelectedCells[0].RowIndex < 0)
                return;

            int row = OnOffView.SelectedCells[0].RowIndex;

            OnOffView.Rows.RemoveAt(row);

            _onOffNum--;

            for (int i = row; i < OUTPUT_MAX - 1; i++)
                _onOff[i] = _onOff[i + 1].Clone();

            for (int i = 0; i < OnOffView.Rows.Count; i++)
                OnOffView.Rows[i].Cells[0].Value = (i + 1).ToString();    
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            refreshChart();
        }
        private void OnOnOffSaveArgs(object sender, FrmOnOff.COnOffArgs e)
        {
            _onOff[e.idNo] = e.outPut.Clone();
        }
        private void refreshChart()
        {
            try
            {
                if (_udcChart == null)
                {
                    _udcChart = new udcChartOnOff();

                    _udcChart.Dock = DockStyle.Fill; 

                    panel3.Controls.Add(_udcChart, 0, 4);
                }

                int maxAC = 0;

                for (int i = 0; i < _onOffNum; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (maxAC < _onOff[i].Item[j].ACV)
                            maxAC = _onOff[i].Item[j].ACV;
                    }
                }

                _udcChart.maxVolt = maxAC;

                _udcChart.biTime = System.Convert.ToDouble(txtBITime.Text);

                List<udcChartOnOff.COnOff> itemList = calOnOffItem();

                _udcChart.onoff = itemList;

                _udcChart.Refresh();

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 计算总时序时间段参数
        /// </summary>
        private List<udcChartOnOff.COnOff> calOnOffItem()
        {
            try
            {
                List<udcChartOnOff.COnOff> itemList = new List<udcChartOnOff.COnOff>();

                int burnTime = (int)(_udcChart.biTime * 3600);

                int leftTime = burnTime;

                int sumTime = 0;

                for (int i = 0; i < _onOffNum; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        sumTime += _onOff[i].Item[j].OnOffTime * (_onOff[i].Item[j].OnTime + _onOff[i].Item[j].OffTime);
                    }
                }

                if (sumTime == 0)
                {
                    return itemList;
                }

                while (leftTime > 0)
                {
                    for (int i = 0; i < _onOffNum; i++)
                    {
                        int itemTime = (int)(_onOff[i].TotalTime);

                        if (leftTime < itemTime)  //剩余时间<ON/OFF组时间
                        {
                            itemTime = leftTime;

                            leftTime = 0;
                        }
                        else
                        {
                            leftTime -= itemTime;
                        }
                        //4小段ON/OFF时间

                        int itemLeftTime = itemTime;

                        while (itemLeftTime>0)
                        {
                            for (int j = 0; j < 4; j++) 
                            {
                                if (itemLeftTime == 0)
                                    break;

                                int onoffTime = _onOff[i].Item[j].OnOffTime * (_onOff[i].Item[j].OnTime + _onOff[i].Item[j].OffTime);

                                if (onoffTime == 0)
                                    continue;

                                //单个ON/OFF时序
                                for (int z = 0; z < _onOff[i].Item[j].OnOffTime; z++)
                                {
                                    udcChartOnOff.COnOff onoffItem = new udcChartOnOff.COnOff();

                                     onoffItem.curVolt = _onOff[i].Item[j].ACV;

                                     onoffItem.outPutType = _onOff[i].Item[j].OutPutType;  

                                    //ON段
                                    if (itemLeftTime >= _onOff[i].Item[j].OnTime)
                                    {
                                        onoffItem.onTimes = _onOff[i].Item[j].OnTime;

                                        onoffItem.onoffTimes += onoffItem.onTimes;

                                        itemLeftTime -= _onOff[i].Item[j].OnTime;
                                    }
                                    else
                                    {
                                        onoffItem.onTimes = itemLeftTime;

                                        onoffItem.onoffTimes += onoffItem.onTimes;

                                        itemLeftTime = 0; ;
                                    }

                                    //OFF段
                                    if (itemLeftTime >= _onOff[i].Item[j].OnOffTime)
                                    {
                                        onoffItem.offTimes = _onOff[i].Item[j].OffTime;

                                        onoffItem.onoffTimes += onoffItem.offTimes;

                                        itemLeftTime -= _onOff[i].Item[j].OffTime;
                                    }
                                    else
                                    {
                                        onoffItem.offTimes = itemLeftTime;

                                        onoffItem.onoffTimes += onoffItem.offTimes;

                                        itemLeftTime = 0; ;
                                    }

                                    if (onoffItem.onoffTimes > 0)
                                        itemList.Add(onoffItem); 
                                }
                            }
                        }
                    }
                }

                return itemList;

            }
            catch (Exception)
            {                
                throw;
            }        
        }
        #endregion

    }
}
