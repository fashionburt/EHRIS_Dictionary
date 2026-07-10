using EHRIS.Tools.Net;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EHRIS.Core.Models.Event
{
    public class ScheduleDataLogger : IDataLogger
    {  
        public int ExecUID { get; set; } = 0;         // 或排程指定的 ID 
        public int ExecSfuNo { get; set; } = 0;

        public string ExecProName { get; set; } = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        public string ExecType => "20"; // 排程類型

        public string ExecFromIP { get; set; } = NetworkHelper.GetLocalIpAddress(); 


        public int ToPeoUID { get; set; } = 0;
        public En_DataEventMode EventType { get; set; } = En_DataEventMode.ModEvent; // 預設修改事件

        

    }
}
