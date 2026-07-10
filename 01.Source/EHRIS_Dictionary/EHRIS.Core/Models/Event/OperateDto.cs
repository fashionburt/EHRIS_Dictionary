using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Core.Models.Event
{
    /// <summary>
    /// 操作行為
    /// </summary>
    

    public class OperateDto
    {
        public int PeoUid { get; set; }
        public string DepName { get; set; }
        public string ProName { get; set; }
        public string PeoName { get; set; }
        public int SfuNo { get; set; }
        public string SfuName { get; set; }
        public int Function { get; set; }
        public string ActionMethod { get; set; }
        public string Memo { get; set; }
        public string IpAddress { get; set; }
    }
}
