using EHRIS.Security.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.AdminPortal.Controllers
{
    [Authorize]
    [Route("AdminPortal/Home/[action]")]

    public class AdminHomeController : Controller
    {
        private readonly IUserContextService _userContext;

        public AdminHomeController(IUserContextService userContext)
        {
            _userContext = userContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // 檢查是否為後台管理員登入（對應 AdminAccountController 設定的 Claim）
            var serviceMode = User.FindFirst("ServiceMode")?.Value;
            if (serviceMode != "ADMIN")
            {
                return Redirect(Url.Content("~/AdminPortal/Account/Login"));
            }

            ViewBag.UserName = _userContext.UserName;
            return View();
        }
    }
}
