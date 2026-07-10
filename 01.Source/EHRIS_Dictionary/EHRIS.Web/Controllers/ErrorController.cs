using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet("/Error/DbError")]
        public IActionResult DbError(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}
