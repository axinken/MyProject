﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO.Ports;

namespace GJ.DEV.BARCODE
{
    /// <summary>
    /// KS100条码枪
    /// </summary>
    public class CKS100:IBarCode
    {
        #region 构造函数
        public CKS100(int idNo = 0, string name = "KS100")
        {
            this._idNo = idNo;
            this._name = name;            
        }
        public override string ToString()
        {
            return _name;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event OnRecvHandler OnRecved;
        void OnRecv(CRecvArgs e)
        {
            if (OnRecved != null)
            {
                OnRecved(this, e);
            }
        }
        #endregion

        #region 字段
        private int _idNo = 0;
        private string _name = "KS100";
        private EComMode _comMode = EComMode.SerialPort;
        private SerialPort _rs232;
        private string _recvData = string.Empty;
        private bool _recvThreshold = false;
        private bool _enableThreshold = false;
        #endregion

        #region 属性
        /// <summary>
        /// 设备ID
        /// </summary>
        public int idNo
        {
            get{return _idNo;}
            set{_idNo = value;}
        }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string name
        {
            get{return _name;}
            set{_name =value;}
        }
        /// <summary>
        /// 通信接口
        /// </summary>
        public EComMode comMode
        {
            get { return _comMode; }
        }
        #endregion

        #region 共享方法
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="comName"></param>
        /// <param name="er"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool Open(string comName, out string er, string setting = "115200,n,8,1", bool recvThreshold = false)
        {
            er = string.Empty;

            try
            {
                this._recvThreshold = recvThreshold;

                string[] arrayPara = setting.Split(',');
                
                if (arrayPara.Length != 4)
                {
                    er = "串口设置参数错误";
                    return false;
                }
                int bandRate = System.Convert.ToInt32(arrayPara[0]);
                Parity parity = Parity.None;
                switch (arrayPara[1].ToUpper())
                {
                    case "O":
                        parity = Parity.Odd;
                        break;
                    case "E":
                        parity = Parity.Even;
                        break;
                    case "M":
                        parity = Parity.Mark;
                        break;
                    case "S":
                        parity = Parity.Space;
                        break;
                    default:
                        break;
                }
                int dataBit = System.Convert.ToInt32(arrayPara[2]);
                StopBits stopBits = StopBits.One;
                switch (arrayPara[3])
                {
                    case "0":
                        stopBits = StopBits.None;
                        break;
                    case "1.5":
                        stopBits = StopBits.OnePointFive;
                        break;
                    case "2":
                        stopBits = StopBits.Two;
                        break;
                    default:
                        break;
                }
                if (_rs232 != null)
                {
                    if (_rs232.IsOpen)
                        _rs232.Close();
                    _rs232 = null;
                }
                _rs232 = new SerialPort(comName, bandRate, parity, dataBit, stopBits);
                _rs232.RtsEnable = false;
                _rs232.DtrEnable = true;
                _rs232.DataReceived += new SerialDataReceivedEventHandler(OnRS232_Recv);
                _rs232.Open();

                return true;
            }
            catch (Exception e)
            {
                er = e.ToString();
                return false;
            }
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            if (_rs232 != null)
            {
                if (_rs232.IsOpen)
                    _rs232.Close();
                _rs232 = null;
            }
        }
        /// <summary>
        /// 读取条码
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="er"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool Read(out string serialNo, out string er, int rLen = 0, int timeOut = 1000)
        {
            serialNo = string.Empty;

            er = string.Empty;

            try
            {
                if (_rs232 == null)
                {
                    er = "串口未打开";
                    return false;
                }

                _enableThreshold = false;

                Stopwatch watcher = new Stopwatch();

                string wCmd = "E" + "\r\n";

                string rData = string.Empty;

                _rs232.DiscardInBuffer();

                _rs232.DiscardOutBuffer();

                _rs232.Write(wCmd);

                watcher.Start();

                while (true)
                {

                    System.Threading.Thread.Sleep(10);

                    if (_rs232.BytesToRead > 0)
                    {
                        System.Threading.Thread.Sleep(10);

                        rData += _rs232.ReadExisting();
                    }

                    if (rLen > 0)
                    {
                        if (rData.Length >= rLen)
                        {
                            System.Threading.Thread.Sleep(10);
                            if (_rs232.BytesToRead == 0)
                                break;
                        }
                    }
                    else
                    {
                        if (rData.Length > 0)
                        {
                            System.Threading.Thread.Sleep(10);
                            if (_rs232.BytesToRead == 0)
                                break;
                        }
                    }

                    if (watcher.ElapsedMilliseconds > timeOut)
                        break;
                }

                watcher.Stop();

                if (rData == string.Empty || rData.ToUpper() == "NOREAD")
                {
                    er = "扫描条码超时";
                    return false;
                }

                serialNo = rData;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
            finally
            {
                if (_rs232 != null)
                {
                    _rs232.Write("D\r\n");
                }
            }
        }
        public bool Read(out string serialNo, out string er, string SOI, int rLen, int timeOut = 2000)
        {
            serialNo = string.Empty;

            er = string.Empty;

            try
            {
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 触发条码枪接收数据
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public bool Triger_Start(out string er)
        {
            er = string.Empty;

            try
            {
                if (_rs232 == null)
                {
                    er = "串口未打开";
                    return false;
                }

                _enableThreshold = true;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 触发接收串口数据
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public bool Triger_End(out string er)
        {
            er = string.Empty;

            try
            {
                if (_rs232 == null)
                {
                    er = "串口未打开";
                    return false;
                }

                _enableThreshold = false;

                return true;
            }
            catch (Exception ex)
            {
                er = ex.ToString();
                return false;
            }
        }
        #endregion

        #region 私有方法
        /// 格式化条码有效字符
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        private string formatSn(string serialNo)
        {
            string Sn = string.Empty;

            for (int i = 0; i < serialNo.Length; i++)
            {
                char s = System.Convert.ToChar(serialNo.Substring(i, 1));

                if (s > (char)32 && s < (char)126)
                {
                    Sn += serialNo.Substring(i, 1);
                }
            }

            return Sn;
        }
        private void OnRS232_Recv(object sender, SerialDataReceivedEventArgs e)
        {
            if (_recvThreshold || _enableThreshold)
            {
                System.Threading.Thread.Sleep(5);

                string recv = _rs232.ReadExisting();

                OnRecv(new CRecvArgs(_idNo, recv));
            }
        }
        #endregion
       
    }
}
