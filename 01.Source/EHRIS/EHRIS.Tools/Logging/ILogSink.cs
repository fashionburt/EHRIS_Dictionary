using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public interface ILogSink
    {
        Task WriteAsync(LogEvent logEvent, LogLevel level, string message, Exception? ex = null);
    }

}
