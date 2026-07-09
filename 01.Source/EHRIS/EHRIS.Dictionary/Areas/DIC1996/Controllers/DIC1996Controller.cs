using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Dictionary;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Areas.Dictionary.Controllers;

[Area("DIC1996")]
[Route("/[controller]/[action]")]
public class DIC1996Controller : BaseController
{
    protected const int SFUNO = 1996;
    private readonly IUserContextService _userContext;
    private readonly IDIC1996Service _service;

    public DIC1996Controller(IDIC1996Service service, ICommonService commonService, IUserContextService userContext) : base(commonService)
    {
        _service = service;
        _userContext = userContext;
    }

    private WebDataLogger BuildDataLogger(En_DataEventMode eventType, string procName)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = $"公告管理-{procName}",
            ExecFromIP = _userContext.SourceIP,
            ToPeoUID = _userContext.PeoUID,
            EventType = eventType
        };
    }

    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1996()
    {
        await SetBreadcrumbAsync(SFUNO, "DIC1996", "公告管理");
        ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
        return View();
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetData([FromBody] DIC1996Request request)
    {
        var res = await _service.GetDataTableAsync(request);

        foreach (var item in res.data)
        {
            item.EditAction = HasPermission(FunctionAction.Update)
                ? $"<button type=\"button\" class=\"btn btn-primary btn-sm\" onclick=\"window.DIC1996.edit({item.Id})\">修改</button>"
                : "";
            item.DelAction = HasPermission(FunctionAction.Delete)
                ? $"<button type=\"button\" class=\"btn btn-danger btn-sm\" onclick=\"window.DIC1996.del({item.Id})\">刪除</button>"
                : "";
        }

        res.data = HtmlHelper.EncodeStrings(res.data);
        return Json(new { draw = res.draw, recordsTotal = res.recordsTotal, recordsFiltered = res.recordsFiltered, data = res.data });
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);
        return Json(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
    public async Task<IActionResult> Create([FromBody] DIC1996ViewModel model)
    {
        var dataLogger = BuildDataLogger(En_DataEventMode.AddEvent, "新增公告");
        var result = await _service.SaveAsync(model, dataLogger);
        return Json(new { success = result.success, message = result.message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> Update([FromBody] DIC1996ViewModel model)
    {
        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent, "修改公告");
        var result = await _service.SaveAsync(model, dataLogger);
        return Json(new { success = result.success, message = result.message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var dataLogger = BuildDataLogger(En_DataEventMode.DelEvent, "刪除公告");
        var result = await _service.DeleteAsync(id, dataLogger);
        return Json(new { success = result.success, message = result.message });
    }
}