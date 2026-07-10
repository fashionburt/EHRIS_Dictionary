using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public class LogContextEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogContextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public LogEvent CreateLogEvent(string module, string action)
        {
            var context = _httpContextAccessor.HttpContext;
            var userId = context?.User?.Identity?.Name ?? "Anonymous";
            var requestId = context?.TraceIdentifier ?? Guid.NewGuid().ToString();

            return new LogEvent
            {
                Module = module,
                Action = action,
                UserId = userId,
                RequestId = requestId
            };
        }

    }
}
