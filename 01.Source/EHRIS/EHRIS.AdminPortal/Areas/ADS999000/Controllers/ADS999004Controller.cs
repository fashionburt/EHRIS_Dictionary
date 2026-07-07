using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Tools.Extensions;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.AdminPortal.Controllers;

[Area("ADS999000")]
[Route("/[controller]/[action]")]
public class ADS999004Controller : BaseController
{
    protected const int SFUNO = 999004;
    private readonly IUserContextService _userContext;
    private readonly IADS999004Service _ads999004Service;

    public ADS999004Controller(IUserContextService userContext, IADS999004Service ads999004Service, ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _ads999004Service = ads999004Service;
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ADS999004()
    {
        await SetBreadcrumbAsync(SFUNO, "ADS999004", "ADS999004");
        ViewBag.AddStatus = HasPermission(FunctionAction.Insert);

        ViewBag.GroupList = await _ads999004Service.GetGroupListAsync();

        return PartialView("ADS999004");
    }

    #region 取得資料
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetList([FromBody] Ads999004DataTableRequest request)
    {
        var (data, recordsFiltered, recordsTotal) = await _ads999004Service.GetListAsync(request);
        return Json(new
        {
            draw = request.draw,
            recordsTotal = recordsTotal,
            recordsFiltered = recordsFiltered,
            data = data.Select(r => new
            {
                argVariable = r.ArgVariable,
                argDescribe = r.ArgDescribe,
                argValue = r.ArgValue,
                argDefaultValue = r.ArgDefaultValue,
                argOpenManagerDisplay = r.ArgOpenManagerDisplay,
                modifyName = r.ArgModifyName,
                modifyTimeDisplay = r.ArgModifyTime.ToRocDateTime(),
                editAction = GetEditButtons(r.ArgVariable),
                delAction = GetDelButtons(r.ArgVariable)
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetByVariable(string variable)
    {
        var data = await _ads999004Service.GetByVariableAsync(variable);
        if (data == null) return Json(new { success = false, message = "找不到資料" });
        return Json(new { success = true, data });
    }
    #endregion

    #region 操作紀錄
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
    public async Task<IActionResult> Add([FromBody] Ads999004EditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "驗證失敗：" + string.Join("；", errors) });
        }

        var result = await _ads999004Service.AddAsync(model, _userContext.UserName, CreateLogger(En_DataEventMode.AddEvent));
        return Json(new { success = result.success, message = result.message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> Update([FromBody] Ads999004EditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "驗證失敗：" + string.Join("；", errors) });
        }

        var result = await _ads999004Service.UpdateAsync(model, _userContext.UserName, CreateLogger(En_DataEventMode.ModEvent));
        return Json(new { success = result.success, message = result.message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> Delete([FromBody] Ads999004EditViewModel model)
    {
        if (string.IsNullOrEmpty(model.ArgVariable)) return Json(new { success = false, message = "參數變數不可為空" });

        var result = await _ads999004Service.DeleteAsync(model.ArgVariable, _userContext.UserName, CreateLogger(En_DataEventMode.DelEvent));
        return Json(new { success = result.success, message = result.message });
    }

    private WebDataLogger CreateLogger(En_DataEventMode mode) => new()
    {
        ExecUID = _userContext.PeoUID,
        ExecSfuNo = SFUNO,
        ExecProName = "系統參數設定",
        ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
        ToPeoUID = _userContext.PeoUID,
        EventType = mode
    };
    #endregion

    #region 操作按鈕
    private string GetEditButtons(string id) => HasPermission(FunctionAction.Update)
        ? $"<button class='icon-btn text-primary editItem' data-id='{id}'><i class='fa-regular fa-pen-to-square'></i></button> " : "";

    private string GetDelButtons(string id) => HasPermission(FunctionAction.Delete)
        ? $"<button class='icon-btn text-danger deleteItem' data-id='{id}'><i class='fa fa-trash'></i></button>" : "";
    #endregion
}
