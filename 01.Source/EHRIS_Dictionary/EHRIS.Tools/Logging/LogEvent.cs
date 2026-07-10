using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public class LogEvent
    {
        public string Module { get; set; } = "General";
        public string Action { get; set; } = "Unknown";
        public string UserId { get; set; }
        public string RequestId { get; set; }
    }

}
