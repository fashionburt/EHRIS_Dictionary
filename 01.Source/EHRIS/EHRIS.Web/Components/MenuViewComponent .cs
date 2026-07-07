using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;
using EHRIS.Security.User;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EHRIS.Web.Components
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IUserContextService _userContext;
        public MenuViewComponent(IMenuRepository menuRepository, IUserContextService userContextService)
        {
            _menuRepository = menuRepository;
            _userContext = userContextService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //var menus =await _menuRepository.GetMenusByAccount(_userContext.AccNO);
            //return View("~/Views/Shared/_Sidebar.cshtml", menus); //  傳遞 `menus` 給 `_Sidebar.cshtml`
            // 抓取網址上的 sys_no
            int.TryParse(HttpContext.Request.Query["sys_no"], out int currentSysNo);

            // 取得該帳號所有選單
            var allMenuHierarchy = await _menuRepository.GetMenusByAccount(_userContext.AccNO);

            if (currentSysNo == 0) currentSysNo = 10;
            // 如果沒選，預設第一個
            if (currentSysNo == 0 && allMenuHierarchy.Any())
                currentSysNo = allMenuHierarchy.First().sys_no;

            ViewBag.CurrentSysNo = currentSysNo;

            // 直接把「全部選單」傳給 _Sidebar.cshtml
            //return View("_Sidebar", allMenuHierarchy);
            return View("~/Views/Shared/_Sidebar.cshtml", allMenuHierarchy);

        }
    }

}
