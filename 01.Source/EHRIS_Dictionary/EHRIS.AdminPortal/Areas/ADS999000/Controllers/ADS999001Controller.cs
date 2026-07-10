using EHRIS.Core.Models;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.AdminPortal.Controllers;

[Area("ADS999000")]
[Route("/[controller]/[action]")]
public class ADS999001Controller : BaseController
{
    protected const int SFUNO = 999001;
    private readonly IUserContextService _userContext;
    private readonly IADS999001Service _service;
    private readonly ICommonService _commonService;
    private readonly IADS999001Service _noticeService;


    public ADS999001Controller(IUserContextService userContext, IADS999001Service service, ICommonService commonService, IADS999001Service noticeService) : base(commonService)
    {
        _userContext = userContext;
        _service = service;
        _commonService = commonService;
        _noticeService = noticeService;
    }

    protected bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ADS999001()
    {
        await SetBreadcrumbAsync(SFUNO, "ADS999001", "系統公告管理");
        ViewBag.AddStatus = HasPermission(FunctionAction.Insert);

        if (IsAjaxRequest())
        {
            return PartialView("ADS999001");
        }
        return View();
    }

    private WebDataLogger CreateLogger(En_DataEventMode eventMode)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "系統公告管理",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = eventMode
        };
    }

    #region 取得資料
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetSysNoticeData([FromBody] DataTablesRequest request)
    {
        var serviceResponse = await _service.GetSysNoticePagedListAsync(request);

        return Json(new
        {
            draw = serviceResponse.draw,
            recordsTotal = serviceResponse.recordsTotal,
            recordsFiltered = serviceResponse.recordsFiltered,
            data = serviceResponse.data.Select(p => new
            {
                sysnNo = p.SysnNo,
                sysnType = p.SysnType,
                sysnTypeName = System.Net.WebUtility.HtmlEncode(p.SysnTypeName),
                sysnContent = System.Net.WebUtility.HtmlEncode(p.SysnContent),
                sysnPublicDt = p.SysnPublicDt.ToString("yyyy/MM/dd"),
                sysnStartTime = p.SysnStartTime.ToString("yyyy/MM/dd HH:mm"),
                sysnEndTime = p.SysnEndTime.ToString("yyyy/MM/dd HH:mm"),
                sysnTop = p.SysnTop,
                editAction = GetEditButtons(p.SysnNo),
                delAction = GetDelButtons(p.SysnNo)
            })
        });
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetSysNoticeForEdit(int id)
    {
        var viewModel = await _service.GetSysNoticeForEditAsync(id);
        if (viewModel == null)
        {
            return Json(new { success = false, message = "找不到指定的資料" });
        }
        return Json(new { success = true, data = viewModel });
    }
    #endregion

    #region 操作行為
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
    public async Task<IActionResult> CreateSysNotice([FromBody] SysNoticeUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "輸入格式不正確：\n" + string.Join("\n", errors) });
        }

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.AddEvent);
        var result = await _service.CreateSysNoticeAsync(model, dataLogger, _userContext.UserName);

        if (!result.success) return Json(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateSysNotice([FromBody] SysNoticeUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "輸入格式不正確：\n" + string.Join("\n", errors) });
        }

        if (model.SysnNo == 0)
            return Json(new { success = false, message = "無效的編號" });

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.ModEvent);
        var result = await _service.UpdateSysNoticeAsync(model, dataLogger, _userContext.UserName);

        if (!result.success) return Json(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> DeleteSysNotice([FromBody] SysNoticeUpdateViewModel model)
    {
        if (model == null || model.SysnNo == 0)
            return Json(new { success = false, message = "無效的編號" });

        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.DelEvent);
        var result = await _service.SoftDeleteSysNoticeAsync(model.SysnNo, dataLogger, _userContext.UserName);

        if (!result.success) return Json(new { success = false, message = result.message });
        return Ok(new { success = true, message = result.message });
    }
    #endregion

    #region 操作按鈕
    private string GetEditButtons(int id) =>
        HasPermission(FunctionAction.Update) ? $"<button class='icon-btn text-primary editSysNotice' data-id='{id}' title='編輯'><i class='fa-regular fa-pen-to-square'></i></button> " : "";

    private string GetDelButtons(int id) =>
        HasPermission(FunctionAction.Delete) ? $"<button class='icon-btn text-danger deleteSysNotice' data-id='{id}' title='刪除'><i class='fa fa-trash'></i></button>" : "";
    #endregion

    #region 首頁公告
    [HttpGet]
    public async Task<IActionResult> Login()
    {
        var model = new LoginViewModel
        {
            Notices = await _noticeService.GetLoginNoticesAsync(10)
        };
        return View(model);
    }
    #endregion
}