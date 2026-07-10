using EHRIS.Core.Models.Common;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.ViewComponents;

public class OrgPeoplePickerViewComponent : ViewComponent
{
    private readonly IOrgPeopleService _service;
    private readonly IUserContextService _userContext;

    public OrgPeoplePickerViewComponent(IOrgPeopleService service, IUserContextService userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(OrgPeoplePickerModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.SelectedTokens))
        {
            var filter = OrgPeopleFilter.From(
                model.PtyNo, (int)model.PeopleStatus,
                model.ShowSelf == PeopleShowSelf.True, _userContext.PeoUID,
                (int)model.NodeType, _userContext.UserDepartmentNO);

            model.Preloaded = await _service.ResolveDisplayAsync(
                _userContext.AccNO, model.SelectedTokens, filter);
        }
        return View(model);
    }
}
