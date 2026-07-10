using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Web.Shared.ViewComponents
{
    public class TitleBarViewComponent : ViewComponent 
    {
        public IViewComponentResult Invoke(string title)
        {
            ViewData["TitleBarText"] = title;
            ViewData["QueryTime"] = $"{DateTime.Now.Year - 1911}-{DateTime.Now:MM-dd HH:mm:ss}";
            return View();
        } 
    }
}
