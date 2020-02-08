using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.UI;
using GJ.YOHOO.BURNIN.Udc;

namespace GJ.YOHOO.BURNIN
{
    public partial class FrmOnOff : Form
    {
        #region 显示窗口

        #region 字段
        private static FrmOnOff dlg = null;
        private static object syncRoot = new object();
        #endregion

        #region 属性
        public static bool IsAvalible
        {
            get
            {
                lock (syncRoot)
                {
                    if (dlg != null && !dlg.IsDisposed)
                        return true;
                    else
                        return false;
                }
            }
        }
        public static Form mdlg
        {
            get
            {
                return dlg;
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 创建唯一实例
        /// </summary>
        public static FrmOnOff CreateInstance(int idNo,int outPutNum, COnOff_List OnOff)
        {
            lock (syncRoot)
            {
                if (dlg == null || dlg.IsDisposed)
                {
                    dlg = new FrmOnOff(idNo,outPutNum, OnOff);
                }
            }
            return dlg;
        }
        #endregion

        #endregion

        #region 构造函数
        public FrmOnOff(int idNo,int outPutNum, COnOff_List Output)
        {
            this._idNo = idNo;

            this._outPutNum = outPutNum;

            this._OnOff = Output.Clone();

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
            _udcOnOff = new udcOnOff[ONOFF_MAX];

            for (int i = 0; i < _udcOnOff.Length; i++)
            {
                _udcOnOff[i] = new udcOnOff(i,_outPutNum, _OnOff.Item[i].ChkSec,_OnOff.Item[i].OnOffTime,
                                                          _OnOff.Item[i].OnTime, _OnOff.Item[i].OffTime,
                                                          _OnOff.Item[i].ACV, _OnOff.Item[i].OutPutType);

                _udcOnOff[i].Dock = DockStyle.Fill;

                _udcOnOff[i].Margin = new Padding(0);

                panel2.Controls.Add(_udcOnOff[i], i, 0); 
 
            }

            labDescribe.Text = _OnOff.Describe;

            txtTotalTime.Text = (((double)_OnOff.TotalTime) / 60).ToString("0.0");

            txtTotalTime.KeyPress += new KeyPressEventHandler(OnTextKeyPressIsNumber);
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
                                            .SetValue(splitContainer1.Panel2, true, null);

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
        private int _idNo = 0;
        private int _outPutNum = 0;
        private COnOff_List _OnOff = null;
        private const int ONOFF_MAX = 4;
        #endregion

        #region 面板控件
        /// <summary>
        /// ONOFF控件
        /// </summary>
        private udcOnOff[] _udcOnOff;
        /// <summary>
        /// ONOFF曲线
        /// </summary>
        private udcChartOnOff _udcChart;
        #endregion

        #region 面板回调函数
        private void FrmOnOff_Load(object sender, EventArgs e)
        {
            refreshChart(); 
        }
        private void OnTextKeyPressIsNumber(object sender, KeyPressEventArgs e)
        {
            //char-8为退格键
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)'.')
                e.Handled = true;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            dlg.Close();

        }
        private void BtnExit_Click(object sender, EventArgs e)
        {
            dlg.Close();
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            refreshChart();
        }
        #endregion

        #region 方法
        /// <summary>
        /// 保存设置
        /// </summary>
        private bool save()
        {
            try
            {
                int countTime = 0;

                _OnOff.Describe = labDescribe.Text;

                _OnOff.TotalTime = (int)(System.Convert.ToDouble(txtTotalTime.Text) * 60);

                for (int i = 0; i < _OnOff.Item.Length; i++)
                {
                    _OnOff.Item[i].ChkSec=_udcOnOff[i].chkSec;

                    _OnOff.Item[i].OnOffTime = _udcOnOff[i].onoffTime;

                    _OnOff.Item[i].OnTime = _udcOnOff[i].onTime;

                    _OnOff.Item[i].OffTime = _udcOnOff[i].offTime;

                    _OnOff.Item[i].ACV = _udcOnOff[i].acv;

                    _OnOff.Item[i].OutPutType = _udcOnOff[i].outPutType;

                    if (_OnOff.Item[i].OnOffTime > 0)
                    {
                        if (_OnOff.Item[i].OnTime == 0 && _OnOff.Item[i].OffTime == 0)
                        {
                            MessageBox.Show("ONOFF" + (i + 1).ToString() + CLanguage.Lan("参数设置错误"));
                            return false;
                        }

                        countTime = _OnOff.Item[i].OnOffTime * (_OnOff.Item[i].OnTime + _OnOff.Item[i].OffTime);
                    }
                }

                if (countTime == 0)
                {
                    MessageBox.Show("ONOFF1-4"+ CLanguage.Lan("参数设置错误"));
                    return false;
                }

                OnSaveArgs.OnEvented(new COnOffArgs(_idNo, _OnOff));

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// ON/OFF曲线
        /// </summary>
        private void refreshChart()
        {
            try
            {
                if (_udcChart == null)
                {
                    _udcChart = new udcChartOnOff();

                    _udcChart.Dock = DockStyle.Fill; 

                    panel1.Controls.Add(_udcChart, 0, 2);  
                }

                int maxAC = 0;

                for (int i = 0; i < ONOFF_MAX; i++)
                {
                    if (maxAC < _udcOnOff[i].acv)
                        maxAC = _udcOnOff[i].acv; 
                }

                _udcChart.maxVolt = maxAC;

                _udcChart.biTime =System.Convert.ToDouble(txtTotalTime.Text) / 60;

                List<udcChartOnOff.COnOff> itemList = new List<udcChartOnOff.COnOff>();

                for (int i = 0; i < ONOFF_MAX; i++)
                {
                    udcChartOnOff.COnOff item = new udcChartOnOff.COnOff();

                    item.curVolt = _udcOnOff[i].acv;

                    item.onTimes = System.Convert.ToInt32(_udcOnOff[i].onTime);

                    item.offTimes = System.Convert.ToInt32(_udcOnOff[i].offTime);

                    item.onoffTimes = System.Convert.ToInt32(_udcOnOff[i].onoffTime) * (item.onTimes + item.offTimes);

                    item.outPutType = _udcOnOff[i].outPutType; 

                    itemList.Add(item); 
                }

                _udcChart.onoff = itemList;

                _udcChart.Refresh();

            }
            catch (Exception)
            {                
                throw;
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 输出规格事件
        /// </summary>
        public class COnOffArgs : EventArgs
        {
            public COnOffArgs(int idNo, COnOff_List outPut)
            {
                this.idNo = idNo;
                this.outPut = outPut;
            }
            public readonly int idNo;
            public readonly COnOff_List outPut;
        }
        /// <summary>
        /// 保存参数消息
        /// </summary>
        public static COnEvent<COnOffArgs> OnSaveArgs = new COnEvent<COnOffArgs>();
        #endregion

        #region 关闭按钮失效
        /// <summary>
        /// 重写窗体消息
        /// </summary>
        /// <param name="m">屏蔽关闭按钮</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            const int SC_MINIMIZE = 0xF020;
            const int SC_MAXIMIZE = 0xF030;
            if (m.Msg == WM_SYSCOMMAND)
            {
                switch ((int)m.WParam)
                {
                    case SC_CLOSE:
                        return;
                    case SC_MINIMIZE:
                        break;
                    case SC_MAXIMIZE:
                        break;
                    default:
                        break;
                }
            }
            base.WndProc(ref m);
        }
        #endregion

    }
}
