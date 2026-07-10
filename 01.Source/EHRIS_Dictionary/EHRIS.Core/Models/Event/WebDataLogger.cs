using EHRIS.Tools.Formatter;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Text;

namespace EHRIS.Core.Models.Event
{
    public class WebDataLogger : IDataLogger
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        public WebDataLogger()
        { 
        }
        
        public WebDataLogger(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private int? _execUidOverride;
         
        public int ExecUID
        {
            get
            {
                if (_execUidOverride.HasValue)
                    return _execUidOverride.Value;

                return _httpContextAccessor?.HttpContext?.User.FindFirst("PeoUid")?.Value.ToInt() ?? 0;
            }
            set
            {
                _execUidOverride = value;
            }
        }


        public int ExecSfuNo { get; set; }
        public string ExecProName { get; set; }
        public string ExecType => "10"; // 操作類型

        private string _execFromIPOverride;
        public string ExecFromIP
        {
            get
            {
                if (!string.IsNullOrEmpty(_execFromIPOverride))
                    return _execFromIPOverride ;

                return _httpContextAccessor?.HttpContext != null
                    ? EHRIS.Tools.Web.IPHelper.GetIpAddress(_httpContextAccessor.HttpContext)
                    : "Unknown";
            }
            set
            {
                _execFromIPOverride = value;
            }
        }

        public int ToPeoUID { get; set; } = 0;
        public En_DataEventMode EventType { get; set; }

    }
}
