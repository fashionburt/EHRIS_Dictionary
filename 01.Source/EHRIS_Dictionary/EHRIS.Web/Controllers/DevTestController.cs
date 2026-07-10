using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Controllers
{
    /// <summary>
    /// [TEST] 元件測試用 Controller — 日後正式上線請移除
    /// </summary>
    [Route("/[controller]/[action]")]
    public class DevTestController : Controller
    {
        [HttpGet]
        public IActionResult DevTest()
        {
            return PartialView();
        }
    }
}
