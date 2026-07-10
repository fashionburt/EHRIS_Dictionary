using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Security.Middleware
{
    public class RequestSanitizerMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestSanitizerMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Request.Headers;

            //嚴重等級 9.9 的 ASP.NET Core 資安漏洞 CVE-2025-55315 防護程式
            //https://blog.darkthread.net/blog/cve-2025-55315/
            if (headers.ContainsKey("Content-Length") && headers.ContainsKey("Transfer-Encoding"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid request headers.");
                return;
            }

            await _next(context);
        }
    }
}
