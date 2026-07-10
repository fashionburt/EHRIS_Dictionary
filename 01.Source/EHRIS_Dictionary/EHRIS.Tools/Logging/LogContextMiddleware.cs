using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Logging
{
    public class LogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public LogContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var requestId = context.TraceIdentifier;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var logEvent = new LogEvent
            {
                Module = "Web",
                Action = context.Request.Path,
                UserId = userId,
                RequestId = requestId
            };

            context.Items["LogEvent"] = logEvent;

            await _next(context);
        }
    }
}
