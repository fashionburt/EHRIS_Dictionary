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

[Area("DIC1997")]
[Route("/[controller]/[action]")]
public class DIC1997Controller : BaseController
{
    protected const int SFUNO = 1997;
    private readonly IUserContextService _userContext;
    private readonly IDIC1997Service _service;

    public DIC1997Controller(IDIC1997Service service, ICommonService commonService, IUserContextService userContext) : base(commonService)
    {
        _service = service;
        _userContext = userContext;
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

    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> DIC1997(string? dbKey, string? tableName, string? pkName, string? sid)
    {
        string serverIp = GetRealIp(sid);
        await SetBreadcrumbAsync(SFUNO, "DIC1997", $"操作紀錄查詢 ({sid})");

        ViewBag.DbKeys = await _service.GetDbKeysAsync(serverIp);
        ViewBag.InitialDbKey = dbKey;
        ViewBag.InitialTableName = tableName;
        ViewBag.InitialPkName = pkName;
        ViewBag.Sid = sid;

        return View("~/Areas/DIC1997/Views/DIC1997/DIC1997.cshtml");
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetData([FromBody] DIC1997Request request)
    {
        string currentSid = (request.Sid ?? Request.Query["sid"].ToString() ?? "111").Trim();

        if (string.IsNullOrEmpty(currentSid)) currentSid = "111";

        string serverIp = GetRealIp(currentSid);

        var res = await _service.GetDataTableAsync(request, serverIp);

        res.data = HtmlHelper.EncodeStrings(res.data);
        var responseData = res.data.Select(x => new
        {
            dbKey = x.DbKey,
            tableName = x.TableName,
            pkName = x.PkName,
            stateText = x.StateText,
            detail = x.Detail,
            dateText = x.DateText,
            serverIp = x.ServerIP
        });

        return Json(new { draw = res.draw, recordsTotal = res.recordsTotal, recordsFiltered = res.recordsFiltered, data = responseData });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateFields([FromQuery] string sid, string dbKey, string tableName, [FromBody] List<DIC1997UpdateModel> updates)
    {
        string serverIp = GetRealIp(sid);
        var dataLogger = BuildDataLogger(En_DataEventMode.ModEvent, $"更新描述：{tableName}", serverIp);
        var result = await _service.UpdateFieldsAsync(serverIp, dbKey, tableName, updates, dataLogger);

        return Json(new { success = result.success, message = WebUtility.HtmlEncode(result.message) });
    }

    [HttpGet]
    public async Task<IActionResult> GetTableNames(string dbKey, string? pkName, string? sid)
        => Json(await _service.GetTableNamesAsync(GetRealIp(sid), dbKey, pkName));

    [HttpGet]
    public async Task<IActionResult> GetPkNames(string dbKey, string? tableName, string? sid)
        => Json(await _service.GetPkNamesAsync(GetRealIp(sid), dbKey, tableName));
}