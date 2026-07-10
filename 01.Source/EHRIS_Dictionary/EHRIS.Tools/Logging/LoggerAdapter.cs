using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public class LoggerAdapter : ILoggerAdapter
    {
        private readonly ILogger<LoggerAdapter> _logger;

        public LoggerAdapter(ILogger<LoggerAdapter> logger)
        {
            _logger = logger;
        }

        public void Info(string message, LogEvent? evt = null)
        {
            _logger.LogInformation("{Message} {@Event}", message, evt);
        }

        public void Warn(string message, LogEvent? evt = null)
        {
            _logger.LogWarning("{Message} {@Event}", message, evt);
        }

        public void Error(Exception ex, string message, LogEvent? evt = null)
        {
            _logger.LogError(ex, "{Message} {@Event}", message, evt);
        }

        public void Trace(string message, LogEvent? evt = null)
        {
            _logger.LogTrace("{Message} {@Event}", message, evt);
        }
    }

}
