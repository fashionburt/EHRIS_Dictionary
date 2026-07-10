using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public interface ILoggerAdapter
    {
        void Info(string message, LogEvent? evt = null);
        void Warn(string message, LogEvent? evt = null);
        void Error(Exception ex, string message, LogEvent? evt = null);
        void Trace(string message, LogEvent? evt = null);
    }

}
