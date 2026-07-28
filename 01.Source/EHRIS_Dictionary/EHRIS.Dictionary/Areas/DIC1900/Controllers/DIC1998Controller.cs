using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Dictionary;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EHRIS.Dictionary.Controllers;

[Area("DIC1900")]
[Route("/[controller]/[action]")]
public class DIC1998Controller : BaseController
{
    protected const int SFUNO = 1998;
    private readonly IUserContextService _userContext;
    private readonly IDIC1998Service _service;
    private readonly ApplicationDbContext _context;

    public DIC1998Controller(IUserContextService userContext, IDIC1998Service service, ICommonService commonService, ApplicationDbContext context) : base(commonService)
    {
        _userContext = userContext;
        _service = service;
        _context = context;
    }

    private WebDataLogger BuildDataLogger(En_DataEventMode eventType)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "IP授權管理",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = eventType
        };
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1998()
    {
        await SetBreadcrumbAsync(SFUNO, "DIC1998", "IP 授權管理");

        ViewBag.QueryStatus = HasPermission(FunctionAction.Query);
        ViewBag.InsertStatus = HasPermission(FunctionAction.Insert);
        ViewBag.UpdateStatus = HasPermission(FunctionAction.Update);
        ViewBag.DeleteStatus = HasPermission(FunctionAction.Delete);

        var allAccess = _context.Menu_Access.AsNoTracking().Where(a => a.IsEnabled != 2);
        var allMenus = _context.Menus.AsNoTracking().Where(m => m.IsEnabled == 1);

        ViewBag.ClientIps = await allAccess.Select(a => a.ClientIp).Distinct().OrderBy(a => a).ToListAsync();
        ViewBag.ServerIps = await allMenus.Select(m => m.ServerIP).Distinct().OrderBy(m => m).ToListAsync();
        ViewBag.Menus = await allMenus.Select(m => new { m.MenuId, m.MenuName }).OrderBy(m => m.MenuName).ToListAsync();

        return PartialView("DIC1998");
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetData([FromBody] DIC1998SearchModel request)
    {
        var serviceResponse = await _service.GetGroupedDataTableAsync(request);
        return Json(new
        {
            draw = serviceResponse.draw,
            recordsTotal = serviceResponse.recordsTotal,
            recordsFiltered = serviceResponse.recordsFiltered,
            data = serviceResponse.data
        });
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetAvailableMenus([FromBody] Dictionary<string, string> request)
    {
        if (!request.TryGetValue("serverIp", out var serverIp) || string.IsNullOrEmpty(serverIp))
        {
            return Json(new { success = false, message = "伺服器 IP 未指定" });
        }

        var menus = await _service.GetAvailableMenusAsync(serverIp);
        var data = menus.Select(m => new { value = m.MenuId, text = m.MenuName }).ToList();

        return Json(new { success = true, data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert | FunctionAction.Update)]
    public async Task<IActionResult> Save([FromBody] DIC1998SaveViewModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.ClientIp) || model.MenuId <= 0)
        {
            return Json(new { success = false, message = "參數錯誤或欄位未填寫完整" });
        }

        var eventType = model.AccessId > 0 ? En_DataEventMode.ModEvent : En_DataEventMode.AddEvent;
        var dataLogger = BuildDataLogger(eventType);

        var result = await _service.SaveAsync(model, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> Delete(int accessId)
    {
        if (accessId <= 0) return Json(new { success = false, message = "參數錯誤" });

        var dataLogger = BuildDataLogger(En_DataEventMode.DelEvent);

        var result = await _service.DeleteAsync(accessId, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }
}