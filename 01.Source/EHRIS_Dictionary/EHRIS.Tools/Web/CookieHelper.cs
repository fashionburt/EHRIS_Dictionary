using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHRIS.Tools.Web
{
    public class DomainSettings
    {
        public string Domain { get; set; }               //客戶的網域，如 "yshun.com.tw"
        public bool RequiresCrossSubdomain { get; set; } // 是否需要跨子域共享 Cookie
    }


    public class CookieHelper
    {
        private readonly DomainSettings _tenant;

        public CookieHelper(DomainSettings tenant)
        {
            _tenant = tenant;
        }

        public CookieOptions BuildSecureCookieOptions()
        {
            var options = new CookieOptions
            {
                Path = "/",                // 標準路徑
                HttpOnly = true,           // 防止 JS 存取
                Secure = true,             // 僅允許 HTTPS
                SameSite = _tenant.RequiresCrossSubdomain
                            ? SameSiteMode.Lax
                            : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)  
            };

            // 如果需要跨子域共享，才設定 Domain
            if (_tenant.RequiresCrossSubdomain && !string.IsNullOrEmpty(_tenant.Domain))
            {
                options.Domain = _tenant.Domain; // 例如 "customerA.com"
            }
            // 否則不設定 Domain → 預設 host-only，最安全

            return options;
        }

    }
}
