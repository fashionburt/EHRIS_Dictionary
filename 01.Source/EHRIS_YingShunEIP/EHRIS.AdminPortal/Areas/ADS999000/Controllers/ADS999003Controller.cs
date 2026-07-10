using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EHRIS.AdminPortal.Controllers;

[Area("ADS999000")]
[Route("/[controller]/[action]")]
public class ADS999003Controller : BaseController
{
    protected const int SFUNO = 999003;
    private readonly IUserContextService _userContext;
    private readonly IADS999003Service _service;
    private readonly ICommonService _commonService;

    public ADS999003Controller(IUserContextService userContext, IADS999003Service service, ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _service = service;
        _commonService = commonService;
    }

    protected bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ADS999003()
    {
        await SetBreadcrumbAsync(SFUNO, "ADS999003", "使用者管理");
        ViewBag.AddStatus = HasPermission(FunctionAction.Insert);

        if (IsAjaxRequest())
        {
            return PartialView("ADS999003");
        }
        return View();
    }

    private WebDataLogger CreateLogger(En_DataEventMode eventMode)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "使用者管理",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = eventMode
        };
    }

    #region 取得資料
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetAllUserList([FromBody] AdminUserRequestViewModel request)
    {
        var users = await _service.GetUserListAsync();

        var total = users.Count;
        var start = request?.start ?? 0;
        var length = request?.length ?? 10;
        var searchValue = request?.extraSearch?.searchValue?.ToLower() ?? "";
        var searchColumns = request?.extraSearch?.columnIndexes ?? new List<int>();

        if (request?.orderby != null && request.orderby.Count > 0)
        {
            var orderCol = request.orderby[0];
            var colName = request.columns[orderCol.column].data;
            bool isAsc = orderCol.dir == "asc";

            users = colName switch
            {
                "adlName" => isAsc ? users.OrderBy(u => u.AdlName).ToList() : users.OrderByDescending(u => u.AdlName).ToList(),
                "aduDisplayName" => isAsc ? users.OrderBy(u => u.AduDisplayName).ToList() : users.OrderByDescending(u => u.AduDisplayName).ToList(),
                "aduLogin" => isAsc ? users.OrderBy(u => u.AduLogin).ToList() : users.OrderByDescending(u => u.AduLogin).ToList(),
                _ => users
            };
        }

        var filtered = string.IsNullOrEmpty(searchValue)
            ? users
            : users.Where(u =>
                searchColumns.Any(idx =>
                    (idx == 0 && u.AdlName.ToLower().Contains(searchValue)) ||
                    (idx == 1 && u.AduDisplayName.ToLower().Contains(searchValue)) ||
                    (idx == 2 && u.AduLogin.ToLower().Contains(searchValue))
                )).ToList();

        var paged = filtered.Skip(start).Take(length).ToList();
        paged = HtmlHelper.EncodeStrings(paged);

        return Json(new
        {
            draw = request?.draw ?? 0,
            recordsTotal = total,
            recordsFiltered = filtered.Count,
            data = paged.Select(u => new
            {
                aduNo = u.AduNo,
                adlName = WebUtility.HtmlEncode(u.AdlName),
                aduLogin = WebUtility.HtmlEncode(u.AduLogin),
                aduDisplayName = WebUtility.HtmlEncode(u.AduDisplayName),
                aduEmail = WebUtility.HtmlEncode(u.AduEmail),
                roleNames = WebUtility.HtmlEncode(u.RoleNames),
                aduStatus = u.AduStatus,
                aduModifyTime = u.AduModifyTime?.ToString("yyyy/MM/dd"),
                editAction = GetEditButtons(u.AduNo),
                delAction = GetDelButtons(u.AduNo)
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserData(int aduNo)
    {
        var userData = aduNo > 0 ? await _service.GetUserByNoAsync(aduNo) : new AdminUserSaveViewModel();
        var levels = await _service.GetAvailableLevelsAsync();
        var roles = await _service.GetAvailableRolesAsync();

        return Json(new
        {
            success = true,
            data = HtmlHelper.EncodeStrings(userData),
            levels = levels.Select(l => new { adlNo = l.AdlNo, adlName = l.AdlName }),
            roles = roles.Select(r => new { adrNo = r.AdrNo, adrRoleName = r.AdrRoleName })
        });
    }
    #endregion

    #region 操作行為
    [HttpPost]
    public async Task<IActionResult> SaveUser([FromBody] AdminUserSaveViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "輸入格式不正確：\n" + string.Join("\n", errors) });
        }

        En_DataEventMode mode = model.AduNo == 0 ? En_DataEventMode.AddEvent : En_DataEventMode.ModEvent;
        WebDataLogger dataLogger = CreateLogger(mode);

        var result = await _service.SaveUserAsync(model, dataLogger);

        if (!result.success) return Json(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser([FromBody] AdminUserSaveViewModel model)
    {
        if (model == null || model.AduNo == 0)
            return Json(new { success = false, message = "無效的編號" });

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.DelEvent);
        var result = await _service.DeleteUserAsync(model.AduNo, dataLogger);

        if (!result.success) return Json(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }
    #endregion

    #region 操作按鈕
    private string GetEditButtons(int id) =>
        HasPermission(FunctionAction.Update) ? $"<button class='icon-btn text-primary editUser' data-id='{id}' title='編輯'><i class='fa-regular fa-pen-to-square'></i></button> " : "";

    private string GetDelButtons(int id) =>
        HasPermission(FunctionAction.Delete) ? $"<button class='icon-btn text-danger deleteUser' data-id='{id}' title='刪除'><i class='fa fa-trash'></i></button>" : "";
#endregion
}