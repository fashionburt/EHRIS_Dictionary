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
public class DIC1999R02Controller : BaseController
{
    protected const int SFUNO = 1999;
    private readonly IUserContextService _userContext;
    private readonly IDIC1999R02Service _service;

    public DIC1999R02Controller(IUserContextService userContext, IDIC1999R02Service service, ICommonService commonService) : base(commonService)
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
            ExecProName = $"{procName} [Server:{serverIp}]",
            ExecFromIP = _userContext.SourceIP,
            ToPeoUID = _userContext.PeoUID,
            EventType = eventType
        };
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1999R02(string dbKey, string tableName, string sid)
    {
        if (string.IsNullOrEmpty(dbKey) || string.IsNullOrEmpty(tableName))
            return RedirectToAction("DIC1999", "DIC1999");

        await SetBreadcrumbAsync(SFUNO, "DIC1999R02", "欄位列表");

        ViewBag.DbKey = dbKey;
        ViewBag.TableName = tableName;
        ViewBag.Sid = sid;
        ViewBag.ServerIp = GetRealIp(sid);
        ViewBag.UpdateStatus = HasPermission(FunctionAction.Update);
        ViewBag.DeleteStatus = HasPermission(FunctionAction.Delete);

        return PartialView("~/Areas/DIC1999/Views/DIC1999/DIC1999R02.cshtml");
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetData([FromBody] DIC1999R02Request request, [FromQuery] string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.AddEvent, "讀取欄位列表(同步實體欄位)", serverIp);
        var serviceResponse = await _service.GetDataTableAsync(request, serverIp, dataLogger);

        serviceResponse.data = HtmlHelper.EncodeStrings(serviceResponse.data);

        var responseData = serviceResponse.data.Select(a => new
        {
            rowName = GetNameWithBadges(a),
            dataType = WebUtility.HtmlEncode(a.DataType),
            length = a.Length?.ToString() ?? "-",
            isNull = a.IsNullable ? "允許" : "不允許",
            rowDesc = a.RowDesc,
            rowRemark = a.RowRemark,
            rowId = a.RowId,
            logAction = $"<button type='button' class='icon-btn text-info showLogs' data-db='{WebUtility.HtmlEncode(request.DbKey)}' data-table='{WebUtility.HtmlEncode(request.TableName)}' data-column='{WebUtility.HtmlEncode(a.RowName)}' data-sid='{sid}' title='操作紀錄'><i class='fa-solid fa-clock-rotate-left'></i></button>",
            deleteAction = GetDelButtons(a.RowId, a.RowName, sid)
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
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateFields([FromBody] List<DIC1999R02ViewModel> updates, [FromQuery] string dbKey, [FromQuery] string tableName, [FromQuery] string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent, $"更新資料表【{tableName}】欄位描述/備註", serverIp);
        var result = await _service.UpdateFieldsAsync(dbKey, serverIp, tableName, updates, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
    public async Task<IActionResult> CreateColumn([FromBody] DIC1999R02CreateViewModel model, [FromQuery] string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.AddEvent, $"新增資料欄：{model.ColumnName}", serverIp);
        var result = await _service.CreateColumnAsync(model, serverIp, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
    public async Task<IActionResult> DeleteColumn(string dbKey, string tableName, string columnName, int rowId, string sid)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.DelEvent, $"刪除資料欄：{columnName}", serverIp);
        var result = await _service.DeleteColumnAsync(dbKey, serverIp, tableName, columnName, rowId, dataLogger);
        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1999R03(string dbKey, string tableName, string keyword, string sid)
    {
        if (string.IsNullOrEmpty(dbKey) || string.IsNullOrEmpty(tableName))
            return RedirectToAction("DIC1999", "DIC1999");

        string serverIp = GetRealIp(sid);
        await SetBreadcrumbAsync(SFUNO, "DIC1999R03", "資料內容");

        ViewBag.DbKey = dbKey;
        ViewBag.TableName = tableName;
        ViewBag.Keyword = keyword;
        ViewBag.Sid = sid;

        var (dataTable, pkList, fkList, colDescDict) = await _service.GetTableDetailAsync(dbKey, serverIp, tableName, keyword);

        ViewBag.PrimaryKeys = pkList;
        ViewBag.ForeignKeys = fkList;
        ViewBag.ColumnDescriptions = colDescDict;

        return PartialView("~/Areas/DIC1999/Views/DIC1999/DIC1999R03.cshtml", dataTable);
    }

    private string GetNameWithBadges(DIC1999R02ViewModel a)
    {
        var name = WebUtility.HtmlEncode(a.RowName);
        if (a.IsPrimaryKey) name += " <span class='badge bg-success ms-1'>PK</span>";
        if (a.IsForeignKey) name += " <span class='badge bg-info ms-1'>FK</span>";
        return name;
    }

    private string GetDelButtons(int rowId, string colName, string sid)
    {
        if (!HasPermission(FunctionAction.Delete)) return "";
        return $"<button type='button' class='icon-btn text-danger deleteColBtn' data-id='{rowId}' data-name='{WebUtility.HtmlEncode(colName)}' data-sid='{sid}' title='刪除實體欄位'><i class='fa-solid fa-trash'></i></button>";
    }
}