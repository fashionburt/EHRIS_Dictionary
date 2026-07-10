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
            var allMenuHierarchy = await _menuRepository.GetMenusByAccount(_userContext.AccNO);

            return View("~/Views/Shared/_Sidebar.cshtml", allMenuHierarchy);
        }
    }

}