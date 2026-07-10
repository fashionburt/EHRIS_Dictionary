using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.ViewComponents;

public class OrgSelectorDropdownViewComponent : ViewComponent
{
    private readonly IOrgSelectorService _orgService;
    private readonly IUserContextService _userContext;

    public OrgSelectorDropdownViewComponent(IOrgSelectorService orgService, IUserContextService userContext)
    {
        _orgService = orgService;
        _userContext = userContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(OrgSelectorDropdownModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.SelectedId))
        {
            var nodes = await _orgService.GetNodesByIdsAsync(
                model.Scope, _userContext.AccNO, new[] { model.SelectedId.Trim() });
            model.PreloadedNode = nodes.FirstOrDefault();
        }
        return View(model);
    }
}
