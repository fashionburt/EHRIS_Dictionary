using EHRIS.Core.Models;
using EHRIS.Security.User;
using EHRIS.Tools.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EHRIS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserContextService _userContext;

        public HomeController(ILogger<HomeController> logger, IUserContextService userContext )
        {
            _logger = logger;

            _userContext = userContext;
        }

        public IActionResult Index()
        {
            var peoName = _userContext.UserName;  //HttpContext.Session.GetString("UserName");
            ViewBag.PeoName = peoName;

            string clientIp = IPHelper.GetIpAddress(HttpContext);
            ViewBag.ClientIP = clientIp;

            // 檢查 Session
            var user = HttpContext.Session.GetString("UserAccount");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!_userContext.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            ViewBag.Message = "您沒有權限存取此功能。";
            return View();
        }

        public IActionResult GetMenu(int sys_no)
        {
            // 回傳 ViewComponent 的結果，這會產出完整的 _Sidebar.cshtml 內容
            return ViewComponent("Menu", new { sys_no = sys_no });
        }
    }
}
