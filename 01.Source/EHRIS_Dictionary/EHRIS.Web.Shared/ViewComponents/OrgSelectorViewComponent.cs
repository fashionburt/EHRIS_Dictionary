using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.ViewComponents;

public class OrgSelectorViewComponent : ViewComponent
{
    private readonly IOrgSelectorService _orgService;
    private readonly IUserContextService _userContext;

    public OrgSelectorViewComponent(IOrgSelectorService orgService, IUserContextService userContext)
    {
        _orgService = orgService;
        _userContext = userContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(OrgSelectorModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.SelectedIds))
        {
            var ids = model.SelectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
            model.PreloadedNodes = await _orgService.GetNodesByIdsAsync(model.Scope, _userContext.AccNO, ids);
        }
        return View(model);
    }
}
