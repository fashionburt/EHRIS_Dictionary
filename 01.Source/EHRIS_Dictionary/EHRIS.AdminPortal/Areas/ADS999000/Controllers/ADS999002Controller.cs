using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Tools.Extensions;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Controllers;
using EHRIS.Security.Permission.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EHRIS.AdminPortal.Controllers;

[Area("ADS999000")]
[Route("/[controller]/[action]")]
public class ADS999002Controller : BaseController
{
    protected const int SFUNO = 999002;
    private readonly IUserContextService _userContext;
    private readonly IADS999002Service _ads999002Service;
    private readonly ICommonService _commonService;

    public ADS999002Controller(IUserContextService userContext, IADS999002Service ads999002Service, ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _ads999002Service = ads999002Service;
        _commonService = commonService;
    }

    protected bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ADS999002()
    {
        await SetBreadcrumbAsync(SFUNO, "ADS999002", "角色管理");
        ViewBag.AddStatus = HasPermission(FunctionAction.Insert);

        if (IsAjaxRequest())
        {
            return PartialView("ADS999002");
        }
        return View();
    }

    private WebDataLogger CreateLogger(En_DataEventMode eventMode)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "角色管理",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = eventMode
        };
    }

    #region 取得資料
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetAllRoleList([FromBody] AdminRoleRequestViewModel request)
    {
        var roles = await _ads999002Service.GetRoleList();
        var total = roles.Count;
        var start = request?.start ?? 0;
        var length = request?.length ?? 10;
        var searchValue = request?.extraSearch?.searchValue?.ToLower() ?? "";
        var searchColumns = request?.extraSearch?.columnIndexes ?? new List<int>();

        if (request?.orderby != null && request.orderby.Count > 0)
        {
            var orderCol = request.orderby[0];
            var colName = request.columns[orderCol.column].data;
            bool isAsc = orderCol.dir == "asc";

            roles = colName switch
            {
                "adrRoleName" => isAsc ? roles.OrderBy(r => r.AdrRoleName).ToList() : roles.OrderByDescending(r => r.AdrRoleName).ToList(),
                "adrRoleMemo" => isAsc ? roles.OrderBy(r => r.AdrRoleMemo).ToList() : roles.OrderByDescending(r => r.AdrRoleMemo).ToList(),
                "statusText" => isAsc ? roles.OrderBy(r => r.AdrStatus).ToList() : roles.OrderByDescending(r => r.AdrStatus).ToList(),
                _ => roles
            };
        }

        var filtered = string.IsNullOrEmpty(searchValue)
            ? roles
            : roles.Where(r =>
                searchColumns.Any(idx =>
                    (idx == 0 && r.AdrRoleName.ToLower().Contains(searchValue)) ||
                    (idx == 1 && r.AdrRoleMemo.ToLower().Contains(searchValue))
                )).ToList();

        var paged = filtered.Skip(start).Take(length).ToList();
        paged = HtmlHelper.EncodeStrings(paged);

        return Json(new
        {
            draw = request?.draw ?? 0,
            recordsTotal = total,
            recordsFiltered = filtered.Count,
            data = paged.Select(r => new
            {
                adrNo = r.AdrNo,
                adrRoleName = WebUtility.HtmlEncode(r.AdrRoleName),
                adrRoleMemo = WebUtility.HtmlEncode(r.AdrRoleMemo),
                statusText = r.AdrStatus == 1 ? "是" : "否",
                modifyTimeText = r.ModifyTime.ToRocDateTime(),
                peopleAction = GetPeopleButtons(r.AdrNo, r.AdrRoleName),
                authorityAction = GetAuthorityButtons(r.AdrNo, r.AdrRoleName),
                editAction = GetEditButtons(r.AdrNo),
                delAction = GetDelButtons(r.AdrNo)
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetRoleByNo(int adrNo)
    {
        var role = await _ads999002Service.GetRoleByNoAsync(adrNo);
        if (role == null) return Json(new { success = false, message = "找不到資料" });
        return Json(new { success = true, data = HtmlHelper.EncodeStrings(role) });
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetPeople([FromBody] AdminRoleRequestViewModel request)
    {
        var peopleList = await _ads999002Service.GetUsersInRoleAsync(request.AdrNo);
        var total = peopleList.Count;
        var start = request?.start ?? 0;
        var length = request?.length ?? 10;
        var paged = peopleList.Skip(start).Take(length).ToList();

        return Json(new
        {
            draw = request?.draw ?? 0,
            recordsTotal = total,
            recordsFiltered = total,
            data = paged.Select(r => new
            {
                aduLogin = WebUtility.HtmlEncode(r.AduLogin),
                aduDisplayName = WebUtility.HtmlEncode(r.AduDisplayName),
                aduStatusText = r.AduStatusText
            })
        });
    }
    #endregion

    #region 操作行為
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetPermission([FromBody] AdminRoles role)
    {
        var menuTree = await _ads999002Service.GetTreeAsync(role.AdrNo);
        return Json(menuTree);
    }

    [HttpPost]
    public async Task<IActionResult> AddRole([FromBody] AdminRoles role)
    {
        role.AdrCreateName = _userContext.UserName;
        role.AdrCreateTime = DateTime.Now;
        role.AdrModifyName = _userContext.UserName;
        role.AdrModifyTime = DateTime.Now;

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "驗證失敗：" + string.Join("；", errors) });
        }

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.AddEvent);
        var result = await _ads999002Service.AddRoleAsync(role, dataLogger);

        if (!result.success) return BadRequest(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRole([FromBody] AdminRoles role)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "資料驗證失敗" });

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.ModEvent);
        var result = await _ads999002Service.UpdateRoleAsync(role, dataLogger);

        if (!result.success) return Json(new { success = false, message = result.message });
        return Json(new { success = true, message = result.message });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRole([FromBody] AdminRoles role)
    {
        if (role == null) return BadRequest();

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.DelEvent);
        var result = await _ads999002Service.DeleteRoleAsync(role.AdrNo, dataLogger);

        if (!result.success) return BadRequest(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePermission([FromBody] AdminAuthorityViewModel request)
    {
        if (request == null) return BadRequest(new { success = false, message = "系統發生異常" });

        foreach (var item in request.RolePermissions)
        {
            item.ModifyName = _userContext.UserName;
            item.ModifyTime = DateTime.Now;
        }

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.ModEvent);
        var isSuccess = await _ads999002Service.SetRolePermissionAsync(request, dataLogger);

        if (isSuccess) return Ok(new { success = true, message = "權限更新成功" });
        return BadRequest(new { success = false, message = "權限更新失敗" });
    }
    #endregion
    
    #region 操作按鈕
    private string GetPeopleButtons(int id, string name) =>
        HasPermission(FunctionAction.Update) ? $"<button class='icon-btn text-primary showPeople' data-id='{id}' data-rolname='{WebUtility.HtmlEncode(name)}'><i class='fa-solid fa-user-group'></i></button> " : "";

    private string GetAuthorityButtons(int id, string name) =>
        HasPermission(FunctionAction.Update) ? $"<button class='icon-btn text-primary setRole' data-rolname='{WebUtility.HtmlEncode(name)}' data-id='{id}'><i class='fa fa-cog'></i></button> " : "";

    private string GetEditButtons(int id) =>
        HasPermission(FunctionAction.Update) ? $"<button class='icon-btn text-primary editRole' data-id='{id}'><i class='fa-regular fa-pen-to-square'></i></button> " : "";

    private string GetDelButtons(int id) =>
        HasPermission(FunctionAction.Delete) ? $"<button class='icon-btn text-danger deleteRole' data-id='{id}'><i class='fa fa-trash'></i></button>" : "";
    #endregion
}