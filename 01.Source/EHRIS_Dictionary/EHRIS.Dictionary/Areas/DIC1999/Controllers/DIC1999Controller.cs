using EHRIS.Core.Models.Common;
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
using System.Net;

namespace EHRIS.SysBasic.Areas.Dictionary.Controllers;

[Area("DIC1999")]
[Route("/[controller]/[action]")]
public class DIC1999Controller : BaseController
{
    protected const int SFUNO = 1999;
    private readonly IUserContextService _userContext;
    private readonly IDIC1999Service _service;

    public DIC1999Controller(IUserContextService userContext, IDIC1999Service service, ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _service = service;
    }

    private string GetRealIp(string sid)
    {
        return sid switch
        {
            "111" => "192.168.11.111",
            "112" => "192.168.11.112",
            _ => "192.168.11.111"
        };
    }

    private string GetSid(string ip)
    {
        return ip switch
        {
            "192.168.11.111" => "111",
            "192.168.11.112" => "112",
            _ => "111"
        };
    }

    private WebDataLogger BuildDataLogger(En_DataEventMode eventType)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "資料字典管理",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = eventType
        };
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1999()
    {
        await SetBreadcrumbAsync(SFUNO, "DIC1999", "資料庫列表管理");

        string defaultIp = "192.168.11.111";
        ViewBag.AllDatabases = await _service.GetAllDatabaseNamesAsync(defaultIp);

        ViewBag.QueryStatus = HasPermission(FunctionAction.Query);
        ViewBag.InsertStatus = HasPermission(FunctionAction.Insert);
        ViewBag.UpdateStatus = HasPermission(FunctionAction.Update);
        ViewBag.DeleteStatus = HasPermission(FunctionAction.Delete);

        return PartialView("DIC1999");
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetAvailableDatabases(string serverIp)
    {
        var dbs = await _service.GetAllDatabaseNamesAsync(serverIp);
        return Json(new { success = true, data = dbs });
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ExportExcel(int menuId, string serverIp)
    {
        var (content, fileName) = await _service.ExportExcelAsync(menuId, serverIp);
        if (content.Length == 0) return RedirectToAction(nameof(DIC1999));
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ExportJson(int menuId, string serverIp)
    {
        var (content, fileName) = await _service.ExportJsonAsync(menuId, serverIp);
        if (content.Length == 0) return RedirectToAction(nameof(DIC1999));
        return File(content, "application/json", fileName);
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetTableListForExport(int menuId, string serverIp)
    {
        var tables = await _service.GetTableListAsync(menuId, serverIp);
        var data = tables.Select(t => new { tableName = t.TableName, tableDesc = t.TableDesc }).ToList();
        return Json(new { success = true, data });
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ExportWord(int menuId, string serverIp, [FromQuery] List<string>? tables = null)
    {
        var (content, fileName) = await _service.ExportWordAsync(menuId, serverIp, tables);
        if (content.Length == 0) return NotFound("找不到該資料庫的資料，或未選擇任何資料表");
        return File(content, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetData([FromBody] DataTableRequest request)
    {
        string serverIp = request.ServerIp ?? "192.168.11.111";

        string clientIp = _userContext.SourceIP;

        var serviceResponse = await _service.GetDataTableAsync(request, serverIp, clientIp);

        serviceResponse.data = HtmlHelper.EncodeStrings(serviceResponse.data);

        var responseData = serviceResponse.data.Select(a => new
        {
            menuId = a.MenuId,
            serverIp = a.ServerIP,
            menuName = WebUtility.HtmlEncode(a.MenuName),
            menuDesc = WebUtility.HtmlEncode(a.MenuDesc),
            deleteAction = GetDelButtons(a.MenuId, a.MenuName, serverIp),
            excelAction = $"<button type='button' class='icon-btn text-success exportExcel' data-id='{a.MenuId}' data-ip='{serverIp}' title='匯出Excel'><i class='fa-solid fa-file-excel'></i></button>",
            wordAction = $"<button type='button' class='icon-btn text-primary exportWord' data-id='{a.MenuId}' data-ip='{serverIp}' title='匯出Word'><i class='fa-solid fa-file-word'></i></button>",
            jsonAction = $"<button type='button' class='icon-btn text-dark exportJson' data-id='{a.MenuId}' data-ip='{serverIp}' title='匯出Json'><i class='fa-solid fa-code'></i></button>",
            logAction = $"<button type='button' class='icon-btn text-info showLogs' data-name='{WebUtility.HtmlEncode(a.MenuName)}' data-ip='{serverIp}' title='操作紀錄'><i class='fa-solid fa-clock-rotate-left'></i></button>"
        });

        return Json(new
        {
            draw = serviceResponse.draw,
            recordsTotal = serviceResponse.recordsTotal,
            recordsFiltered = serviceResponse.recordsFiltered,
            data = responseData
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
    public async Task<IActionResult> AddDescription([FromBody] DIC1999ViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.ServerIP)) return Json(new { success = false, message = "參數錯誤或未指定伺服器" });

        var dataLogger = BuildDataLogger(En_DataEventMode.AddEvent);

        var result = await _service.AddDescriptionAsync(model, model.ServerIP, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> Delete(int menuId, string serverIp)
    {
        var dataLogger = BuildDataLogger(En_DataEventMode.DelEvent);

        var result = await _service.DeleteAsync(menuId, serverIp, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateDescriptions([FromBody] List<DIC1999ViewModel> updates)
    {
        if (updates == null || !updates.Any()) return Json(new { success = false, message = "無異動資料" });

        string serverIp = updates.First().ServerIP ?? "192.168.11.111";

        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent);

        var result = await _service.UpdateDescriptionsAsync(updates, serverIp, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> ToggleStatus(int menuId, string serverIp)
    {
        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent);

        var result = await _service.ToggleStatusAsync(menuId, serverIp, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    private string GetDelButtons(int id, string? name, string serverIp)
    {
        if (!HasPermission(FunctionAction.Delete)) return "";
        return $"<button type='button' class='icon-btn text-danger deleteBtn' data-id='{id}' data-name='{WebUtility.HtmlEncode(name)}' data-ip='{serverIp}' title='刪除'><i class='fa fa-trash'></i></button>";
    }
}