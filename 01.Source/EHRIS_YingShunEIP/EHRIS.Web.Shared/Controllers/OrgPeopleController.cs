using EHRIS.Core.Models.Common;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.Controllers;

/// <summary>
/// 人員選擇器共用元件 API（部門樹 + 人員 DataTable server-side + 正面表列）。
/// </summary>
[Authorize]
[Route("[controller]/[action]")]
public class OrgPeopleController : Controller
{
    private readonly IOrgPeopleService _service;
    private readonly IUserContextService _userContext;

    public OrgPeopleController(IOrgPeopleService service, IUserContextService userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    private OrgPeopleFilter BuildFilter(int? ptyNo, int peopleStatus, bool showSelf, int nodeType)
        => OrgPeopleFilter.From(ptyNo, peopleStatus, showSelf,
            _userContext.PeoUID, nodeType, _userContext.UserDepartmentNO);

    /// <summary>部門樹（含直屬人數），前端建樹。</summary>
    [HttpGet]
    public async Task<IActionResult> Tree(int? ptyNo = null, int peopleStatus = 1, bool showSelf = true, int nodeType = (int)NodeType.Automa)
    {
        var filter = BuildFilter(ptyNo, peopleStatus, showSelf, nodeType);
        var nodes = await _service.GetDeptTreeAsync(_userContext.AccNO, filter);
        return Json(nodes);
    }

    /// <summary>人員清單（DataTables server-side）。</summary>
    [HttpPost]
    public async Task<IActionResult> People([FromBody] OrgPeopleGridRequest request)
    {
        request ??= new OrgPeopleGridRequest();
        var filter = BuildFilter(request.PtyNo, request.PeopleStatus, request.ShowSelf, request.NodeType);
        var (rows, total) = await _service.GetPeoplePageAsync(_userContext.AccNO, request, filter);

        return Json(new
        {
            draw = request.Draw,
            recordsTotal = total,
            recordsFiltered = total,
            data = rows
        });
    }

    /// <summary>「正面表列」：取某部門排除指定人員後仍保留的人員（前端把全選轉個別選取用）。</summary>
    [HttpPost]
    public async Task<IActionResult> DeptKept([FromBody] OrgDeptKeptRequest request)
    {
        request ??= new OrgDeptKeptRequest();
        var filter = BuildFilter(request.PtyNo, request.PeopleStatus, request.ShowSelf, request.NodeType);
        var rows = await _service.GetDeptKeptAsync(
            _userContext.AccNO, request.DepNo, filter, request.Exclude ?? new List<int>());
        return Json(rows);
    }
}
