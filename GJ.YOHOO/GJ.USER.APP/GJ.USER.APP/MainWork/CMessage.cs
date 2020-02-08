using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GJ.UI;
using GJ.COM;

namespace GJ.USER.APP.MainWork
{
    /// <summary>
    /// 日志UI消息
    /// </summary>
    public class CUILogArgs : EventArgs
    {
        public readonly int idNo;
        public readonly string name;
        public readonly string info;
        public readonly udcRunLog.ELog log;
        public readonly bool save;
        public CUILogArgs(int idNo, string name, string info, udcRunLog.ELog log, bool save)
        {
            this.idNo = idNo;
            this.name = name;
            this.info = info;
            this.log = log;
            this.save = save;
        }
    }
    /// <summary>
    /// 全局UI消息
    /// </summary>
    public class CUIGlobalArgs : EventArgs
    {
        public readonly int idNo;
        public readonly string name;
        public readonly int lPara;
        public readonly int wPara;
        public CUIGlobalArgs(int idNo = 0, string name = "全局消息", int lPara = 0, int wPara = 0)
        {
            this.idNo = idNo;
            this.name = name;
            this.lPara = lPara;
            this.wPara = wPara;
        }
    }
    public class CUIInicatorArgs:EventArgs
    {
        public readonly int idNo;
        public readonly string name;
        public readonly EIndicator status = EIndicator.Idel;

        public CUIInicatorArgs(int idNo, string name, EIndicator status)
        {
            this.idNo = idNo;
            this.name = name;
            this.status = status;
        }
    }
    /// <summary>
    /// 用户UI消息
    /// </summary>
    public class CUIUserArgs<T> : EventArgs where T : class
    {
        public readonly int idNo=0;

        public readonly string name=string.Empty;

        public readonly T model = null;

        public readonly int lPara = 0;

        public readonly int wPara = 0;

        public CUIUserArgs(int idNo,string name, T model,int lPara=0,int wPara=0)
        {
            this.idNo = idNo;
            this.name = name;
            this.model = model;
            this.lPara = lPara;
            this.wPara = wPara;
        }
    }
}
