using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Web
{
    public static class IPHelper
    {
        public static string GetIpAddress(HttpContext context)
        {
            var headersToCheck = new[]
            {
            "X-Forwarded-For",
            "X-Real-IP",
            "HTTP_X_FORWARDED_FOR",
            "HTTP_X_FORWARDED",
            "HTTP_X_CLUSTER_CLIENT_IP",
            "HTTP_FORWARDED_FOR",
            "HTTP_FORWARDED",
            "HTTP_CLIENT_IP"
        };

            foreach (var header in headersToCheck)
            {
                var ip = context.Request.Headers[header].FirstOrDefault();
                if (!string.IsNullOrEmpty(ip) && !ip.Contains("unknown", StringComparison.OrdinalIgnoreCase))
                {
                    // 若有多個 IP，用逗號分隔，取第一個
                    var ipList = ip.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ipList.Length > 0)
                        return ipList[0].Trim();
                }
            }

            // 最後 fallback 到 RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

    }
}
