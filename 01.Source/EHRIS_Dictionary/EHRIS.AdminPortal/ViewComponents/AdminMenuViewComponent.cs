using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;
using EHRIS.Security.User;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.AdminPortal.ViewComponents
{
    /// <summary>
    /// 後台客服系統 Sidebar 選單元件
    /// 複製自前台 MenuViewComponent，Repository 方法可自行改寫
    /// </summary>
    public class AdminMenuViewComponent : ViewComponent
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IUserContextService _userContext;

        public AdminMenuViewComponent(IMenuRepository menuRepository, IUserContextService userContextService)
        {
            _menuRepository = menuRepository;
            _userContext = userContextService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // TODO: 日後可改寫為後台專用的 Repository 方法，例如 GetAdminMenusByAccount
            var allMenuHierarchy = await _menuRepository.GetAdminMenusByAccount(_userContext.AccNO);

            // 後台不分子系統，直接取全部選單
            // 如果要篩選特定模組，可在此調整
            int currentSysNo = 0;
            if (allMenuHierarchy.Any())
                currentSysNo = allMenuHierarchy.First().sys_no;

            ViewBag.CurrentSysNo = currentSysNo;

            return View("~/Views/Shared/_AdminSidebar.cshtml", allMenuHierarchy);
        }
    }
}
