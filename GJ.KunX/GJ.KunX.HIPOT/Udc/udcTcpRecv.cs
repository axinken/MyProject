using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GJ.COM;
using GJ.USER.APP;

namespace GJ.KunX.HIPOT.Udc
{
    public partial class udcTcpRecv : UserControl
    {
        #region 语言设置
        public void LAN()
        {
            CLanguage.SetLanguage(this);

            SetLabelControlLangange(this);
        }
        private void SetLabelControlLangange(Control ctrl)
        {
            foreach (Control item in ctrl.Controls)
            {
                if (item.GetType().ToString() == "System.Windows.Forms.Label")
                {
                    Label lab = (Label)item;

                    if (lab.Tag != null)
                    {
                        lab.Text = CLanguage.Lan(lab.Tag.ToString());
                    }
                }
                else
                {
                    SetLabelControlLangange(item);
                }
            }
        }
        #endregion

        #region 构造函数
        public udcTcpRecv()
        {
            InitializeComponent();
        }
        #endregion

        #region 字段
        private byte[] recvBytes =null; 
        #endregion

        #region 方法
        public void SetStatus(byte[] rbytes)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<byte[]>(SetStatus), rbytes);
            else
            {
                try
                {
                    if (recvBytes!=null && recvBytes.Equals(rbytes))
                        return;

                    recvBytes = (byte[])rbytes.Clone();

                    SOCKET_REQUEST request = CStuct<SOCKET_REQUEST>.BytesToStruct(recvBytes);

                    labCmd.Text = request.CmdNo.ToString();

                    labName.Text = request.Name;

                    labStatus.Text = request.ErrCode.ToString();

                    if (request.ErrCode == ESOCKET_ERROR.OK)
                        labStatus.ForeColor = Color.Blue;
                    else
                        labStatus.ForeColor = Color.Red;

                    rtbSnList.Clear();

                    if (request.Ready == 0)
                    {
                        labReady.Text = CLanguage.Lan("治具未就绪");
                        labReady.ForeColor = Color.Red;
                        labModel.Text = "----";
                        labIdCard.Text = "----";
                    }
                    else
                    {
                        if (request.Ready == 1)
                            labReady.Text = CLanguage.Lan("前工位");
                        else
                            labReady.Text = CLanguage.Lan("后工位");

                        labReady.ForeColor = Color.Blue;

                        labIdCard.Text = request.IdCard;

                        labModel.Text = request.Model;

                        for (int i = 0; i < request.UUT_NUM; i++)
                        {
                            string sn = CLanguage.Lan("条码") + "【" + (i + 1).ToString() + "】:" + request.UUT[i].SerialNo +
                                     ";" + CLanguage.Lan("结果") + "=" + request.UUT[i].Result.ToString() + "\r\n";                               
                                      
                            rtbSnList.AppendText(sn);
                        }                        
                    }
                }
                catch (Exception)                
                {
                    throw;
                }
            }
        }
        public void SetStatus(int ready,string idCard,string model,List<string>serialNos)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<int, string, string, List<string>>(SetStatus), ready, idCard, model, serialNos);
            else
            {
                if (ready == 0)
                {
                    labReady.Text = CLanguage.Lan("治具未就绪");
                    labReady.ForeColor = Color.Red;
                    labModel.Text = "----";
                    labIdCard.Text = "----";
                }
                else
                {
                    if (ready == 1)
                        labReady.Text = CLanguage.Lan("前工位");
                    else
                        labReady.Text = CLanguage.Lan("后工位");

                    labReady.ForeColor = Color.Blue;

                    labIdCard.Text =idCard;

                    labModel.Text = model;

                    for (int i = 0; i < serialNos.Count; i++)
                    {
                        string sn = CLanguage.Lan("条码") + "【" + (i + 1).ToString() + "】:" + serialNos[i] + ";\r\n";                                   
                        rtbSnList.AppendText(sn);
                    }
                } 
            }
        }
        #endregion

    }
}
