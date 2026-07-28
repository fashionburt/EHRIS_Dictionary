using EHRIS.Core.Entities;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Dictionary;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EHRIS.Dictionary.Areas.Dictionary.Controllers;

[Area("DIC1999")]
[Route("/[controller]/[action]")]
public class DIC1999R01Controller : BaseController
{
    protected const int SFUNO = 1999;
    private readonly IUserContextService _userContext;
    private readonly IDIC1999R01Service _service;

    public DIC1999R01Controller(IUserContextService userContext, IDIC1999R01Service service, ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _service = service;
    }

    private string GetRealIp(string? sid)
    {
        return sid switch
        {
            "111" => "192.168.11.111",
            "112" => "192.168.11.112",
            _ => "192.168.11.111"
        };
    }

    private WebDataLogger BuildDataLogger(En_DataEventMode eventType, string procName, string serverIp)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = $"{procName} [{serverIp}]",
            ExecFromIP = _userContext.SourceIP,
            ToPeoUID = _userContext.PeoUID,
            EventType = eventType
        };
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1999R01(string dbKey, string sid)
    {
        if (string.IsNullOrEmpty(dbKey)) return RedirectToAction("DIC1999", "DIC1999");

        await SetBreadcrumbAsync(SFUNO, "DIC1999R01", "資料表列表管理");

        string serverIp = GetRealIp(sid);

        ViewBag.DbKey = dbKey;
        ViewBag.Sid = sid;
        ViewBag.ServerIP = serverIp;

        ViewBag.QueryStatus = HasPermission(FunctionAction.Query);
        ViewBag.InsertStatus = HasPermission(FunctionAction.Insert);
        ViewBag.UpdateStatus = HasPermission(FunctionAction.Update);
        ViewBag.DeleteStatus = HasPermission(FunctionAction.Delete);

        return PartialView("~/Areas/DIC1999/Views/DIC1999/DIC1999R01.cshtml");
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetData([FromBody] DIC1999R01Request request, [FromQuery] string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.AddEvent, "讀取資料表(同步實體表)", serverIp);

        var serviceResponse = await _service.GetDataTableAsync(request, serverIp, dataLogger);

        serviceResponse.data = HtmlHelper.EncodeStrings(serviceResponse.data);

        var responseData = serviceResponse.data.Select(a => new
        {
            tableName = WebUtility.HtmlEncode(a.TableName),
            sheetId = a.SheetId,
            sheetDesc = WebUtility.HtmlEncode(a.SheetDesc),
            editAction = GetEditButtons(a.SheetId ?? 0, a.TableName),
            logAction = $"<button type='button' class='icon-btn text-info showLogs' data-db='{WebUtility.HtmlEncode(request.DbKey)}' data-table='{WebUtility.HtmlEncode(a.TableName)}' data-sid='{sid}' title='操作紀錄'><i class='fa-solid fa-clock-rotate-left'></i></button>",
            deleteAction = GetDelButtons(a.SheetId ?? 0, a.TableName)
        });

        return Json(new { draw = serviceResponse.draw, recordsTotal = serviceResponse.recordsTotal, recordsFiltered = serviceResponse.recordsFiltered, data = responseData });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateDescriptions([FromBody] List<DIC1999R01ViewModel> updates, [FromQuery] string dbKey, [FromQuery] string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent, "批次更新表描述", serverIp);
        var result = await _service.UpdateDescriptionsAsync(dbKey, serverIp, updates, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> DeleteTable(string dbKey, string tableName, int sheetId, string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.DelEvent, $"刪除實體表：{tableName}", serverIp);
        var result = await _service.DeleteTableAsync(dbKey, serverIp, tableName, sheetId, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> RenameTable(string dbKey, int sheetId, string oldName, string newName, string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent, $"資料表更名：{oldName} -> {newName}", serverIp);

        var result = await _service.RenameTableAsync(dbKey, serverIp, sheetId, oldName, newName, dataLogger);

        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
    public async Task<IActionResult> CreateTable([FromBody] DIC1999R01CreateViewModel model, [FromQuery] string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.AddEvent, $"新增資料表：{model.TableName}", serverIp);

        var result = await _service.CreateTableAsync(model, serverIp, dataLogger);

        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> ImportDictionary(IFormFile file, [FromForm] string dbKey, [FromForm] string sid)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "請選擇要匯入的 JSON 檔案" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".json")
        {
            return Json(new { success = false, message = "僅允許上傳 .json 格式的檔案" });
        }

        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return Json(new { success = false, message = "檔案過大，請確認內容是否正確（上限 10MB）" });
        }

        string serverIp = GetRealIp(sid);
        string jsonContent;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            jsonContent = await reader.ReadToEndAsync();
        }

        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent, "匯入資料字典", serverIp);
        var result = await _service.ImportDictionaryAsync(dbKey, serverIp, jsonContent, dataLogger);

        return Json(new { success = result.Success, message = WebUtility.HtmlEncode(result.Message) });
    }


    private string GetEditButtons(int id, string name)
    {
        if (!HasPermission(FunctionAction.Update)) return "";
        return $"<button type='button' class='icon-btn text-primary renameBtn' data-id='{id}' data-name='{WebUtility.HtmlEncode(name)}' title='重新命名'><i class='fa-solid fa-pen-to-square'></i></button>";
    }

    private string GetDelButtons(int id, string name)
    {
        if (!HasPermission(FunctionAction.Delete)) return "";
        return $"<button type='button' class='icon-btn text-danger deleteBtn' data-id='{id}' data-name='{WebUtility.HtmlEncode(name)}' title='刪除實體表'><i class='fa-solid fa-trash'></i></button>";
    }
}