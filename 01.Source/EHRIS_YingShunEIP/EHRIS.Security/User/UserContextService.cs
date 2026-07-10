using EHRIS.Tools.Formatter;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Security.User
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int AccNO => _httpContextAccessor.HttpContext?.User.FindFirst("AccNo")?.Value.ToInt() ?? 0;

        public string UserAccount => _httpContextAccessor.HttpContext?.User.FindFirst("UserAccount")?.Value;
        
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public int PeoUID => _httpContextAccessor.HttpContext?.User.FindFirst("PeoUid")?.Value.ToInt() ??0;
        public string UserName => _httpContextAccessor.HttpContext?.User.FindFirst("UserName")?.Value;
         
        public int? TopChangeLoginUID => _httpContextAccessor.HttpContext?.User.FindFirst("TopChangeLoginUID")?.Value.ToInt() ?? 0;

        public int UserDepartmentLv1NO => _httpContextAccessor.HttpContext?.User.FindFirst("UserDepartmentLv1NO")?.Value.ToInt() ?? 0;

        public int UserDepartmentNO => _httpContextAccessor.HttpContext?.User.FindFirst("UserDepartmentNO")?.Value.ToInt() ?? 0;

        public string UserDepartmentName  => _httpContextAccessor.HttpContext?.User.FindFirst("UserDepartmentName")?.Value;

        public string UserUnitName => _httpContextAccessor.HttpContext?.User.FindFirst("UserUnitName")?.Value;

        public int UserPtyNO => _httpContextAccessor.HttpContext?.User.FindFirst("UserPtyNO")?.Value.ToInt() ?? 0;

        public string UserPtyName => _httpContextAccessor.HttpContext?.User.FindFirst("UserPtyName")?.Value;

        public int UserProfessNO => _httpContextAccessor.HttpContext?.User.FindFirst("UserProfessNO")?.Value.ToInt() ?? 0;

        public string UserProfessName => _httpContextAccessor.HttpContext?.User.FindFirst("UserProfessName")?.Value;

        public string SourceIP => _httpContextAccessor.HttpContext?.User.FindFirst("SourceIP")?.Value;

        #region 後台管理
        public int AduNo => _httpContextAccessor.HttpContext?.User.FindFirst("AduNo")?.Value.ToInt() ?? 0;
        public int AdlNo => _httpContextAccessor.HttpContext?.User.FindFirst("AdlNo")?.Value.ToInt() ?? 0;
        public int AdlRank => _httpContextAccessor.HttpContext?.User.FindFirst("AdlRank")?.Value.ToInt() ?? 0; 
        #endregion
    }
}
