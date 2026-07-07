using EHRIS.Security.User;
using EHRIS.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.Controllers;

/// <summary>
/// 共用 OrgSelector 元件 API。
/// </summary>
[Authorize]
[Route("[controller]/[action]")]
public class OrgSelectorController : Controller
{
    private readonly IOrgSelectorService _orgService;
    private readonly IUserContextService _userContext;

    public OrgSelectorController(IOrgSelectorService orgService, IUserContextService userContext)
    {
        _orgService = orgService;
        _userContext = userContext;
    }

    /// <summary>取得樹節點清單，依 scope 切換資料來源。</summary>
    /// <param name="scope">AuthorizedUnit = 權責機關 / AuthorizedDept = 權責單位</param>
    [HttpGet]
    public async Task<IActionResult> Tree(string scope = "AuthorizedDept")
    {
        if (!Enum.TryParse<OrgSelectorScope>(scope, true, out var scopeEnum))
            scopeEnum = OrgSelectorScope.AuthorizedDept;

        var nodes = await _orgService.GetTreeAsync(scopeEnum, _userContext.AccNO);
        return Json(nodes);
    }
}
