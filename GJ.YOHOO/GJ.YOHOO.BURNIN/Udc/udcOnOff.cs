using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;

namespace GJ.YOHOO.BURNIN.Udc
{
    public partial class udcOnOff : UserControl
    {
        #region 构造函数
        public udcOnOff(int idNo,int outPutNum, int chkSec,int onoffTime,int onTime,int offTime,int acv,int outPutType)
        {
            this._idNo = idNo;

            this._outPutNum = outPutNum;

            this._chkSec = chkSec;

            this._onoffTime = onoffTime;

            this._onTime = onTime;

            this._offTime = offTime;

            this._acv = acv;

            this._outPutType = outPutType;

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
            txtOnOff.Text = _onoffTime.ToString();

            if (_chkSec==1)
            {
                chkUnit.Checked = true;
                txtOn.Text = _onTime.ToString();
                txtOff.Text = _offTime.ToString();
                labOnUint.Text = CLanguage.Lan("秒");
                labOffUint.Text = CLanguage.Lan("秒");
            }
            else
            {
                chkUnit.Checked = false;
                txtOn.Text = (_onTime / 60).ToString();
                txtOff.Text = (_offTime / 60).ToString();
                labOnUint.Text = CLanguage.Lan("分钟");
                labOffUint.Text = CLanguage.Lan("分钟");
            }

            cmbACV.Text = _acv.ToString();

            cmbVType.Items.Clear();
            for (int i = 0; i < _outPutNum; i++)
                cmbVType.Items.Add(i + 1);
            if (cmbVType.Items.Count > _outPutType)
            {
                cmbVType.SelectedIndex = _outPutType;
            }

            chkUnit.Text = "ONOFF" + (_idNo + 1).ToString() + "----" + CLanguage.Lan("单位:") + CLanguage.Lan("秒");
            labOnOff.Text = "ONOFF" + (_idNo + 1).ToString() + CLanguage.Lan("循环:");
            labOn.Text = "ON" + (_idNo + 1).ToString() + CLanguage.Lan("时间:");
            labOff.Text = "OFF" + (_idNo + 1).ToString() + CLanguage.Lan("时间:");

            txtOnOff.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtOn.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
            txtOff.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
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

        #region 字段
        /// <summary>
        /// 编号
        /// </summary>
        private int _idNo = 0;
        /// <summary>
        /// 输出组数
        /// </summary>
        private int _outPutNum = 0;
        /// <summary>
        /// 选择秒
        /// </summary>
        private int _chkSec = 0;
        /// <summary>
        /// 循环次数
        /// </summary>
        private int _onoffTime = 0;
        /// <summary>
        /// ON时间(S)
        /// </summary>
        private int _onTime = 0;
        /// <summary>
        /// OFF时间(S)
        /// </summary>
        private int _offTime = 0;
        /// <summary>
        /// 输入电压
        /// </summary>
        private int _acv = 0;
        /// <summary>
        /// 输出序号
        /// </summary>
        private int _outPutType = 0;
        #endregion

        #region 属性
        /// <summary>
        /// 0：分钟 1：秒
        /// </summary>
        public int chkSec
        {
           set {
                _chkSec = value;
                if (_chkSec == 1)
                {
                    chkUnit.Checked = true;
                    labOnUint.Text = CLanguage.Lan("秒");
                    labOffUint.Text = CLanguage.Lan("秒");
                }
                else
                {
                    chkUnit.Checked = false;
                    labOnUint.Text = CLanguage.Lan("分钟");
                    labOffUint.Text = CLanguage.Lan("分钟");
                }
                }
           get {
               if (chkUnit.Checked)
                   _chkSec = 1;
               else
                   _chkSec = 0;
                return _chkSec; 
              }
        }
        /// <summary>
        /// 循环时间
        /// </summary>
        public int onoffTime
        {
            set {
                  _onoffTime = value;
                  txtOnOff.Text = _onoffTime.ToString(); 
                 }
            get {
                  _onoffTime = System.Convert.ToInt32(txtOnOff.Text);
                  return _onoffTime;
                 }
        }
        /// <summary>
        /// ON时间(S)
        /// </summary>
        public int onTime
        {
            set {
                     _onTime = value;

                    if (chkUnit.Checked)
                        txtOn.Text = _onTime.ToString();
                    else
                        txtOn.Text = (_onTime / 60).ToString();   

                }
            get {

                    _onTime = System.Convert.ToInt32(txtOn.Text);

                    if (chkUnit.Checked)
                        return _onTime;
                    else
                        return _onTime * 60;

                 }
        }
        /// <summary>
        /// Off时间(S)
        /// </summary>
        public int offTime
        {
            set
            {
                _offTime = value;

                if (chkUnit.Checked)
                    txtOff.Text = _offTime.ToString();
                else
                    txtOff.Text = (_offTime / 60).ToString();
            }
            get
            {
                _offTime = System.Convert.ToInt32(txtOff.Text);

                if (chkUnit.Checked)
                    return _offTime;
                else
                    return _offTime * 60;
            }
        }
        /// <summary>
        /// 输入电压
        /// </summary>
        public int acv
        {
            set {
                  _acv = value;
                  cmbACV.Text = _acv.ToString();  
                }
            get {
                _acv = System.Convert.ToInt16(cmbACV.Text);
                return _acv; 
                }
        }
        /// <summary>
        /// 输出序号
        /// </summary>
        public int outPutType
        {
            set {
                  _outPutType = value;
                  cmbVType.SelectedIndex = _outPutType; 
                }
            get { 
                  _outPutType=cmbVType.SelectedIndex;
                  return _outPutType;
                }
        }
        #endregion

        #region 面板回调函数
        private void udcOnOff_Load(object sender, EventArgs e)
        {

           
  
        }
        private void chkUnit_Click(object sender, EventArgs e)
        {
            if (chkUnit.Checked)
            {
                labOnUint.Text = CLanguage.Lan("秒");
                labOffUint.Text = CLanguage.Lan("秒");                
            }
            else
            {
                labOnUint.Text = CLanguage.Lan("分钟");
                labOffUint.Text = CLanguage.Lan("分钟"); 
            }  
        }
        private void OnTextKeyPressIsNumber(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)'.')
                e.Handled = true;
        }
        #endregion

    }
}
