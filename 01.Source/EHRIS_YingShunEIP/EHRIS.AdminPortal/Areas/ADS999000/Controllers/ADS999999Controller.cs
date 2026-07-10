using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.AdminPortal.Controllers;

[Area("ADS999000")]
[Route("/[controller]/[action]")]
public class ADS999999Controller : BaseController
{
    protected const int SFUNO = 999999;
    private readonly IUserContextService _userContext;
    private readonly IADS999999Service _ads999999Service;

    public ADS999999Controller(
        IUserContextService userContext,
        IADS999999Service ads999999Service,
        ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _ads999999Service = ads999999Service;
    }

    #region 平台管理
    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> ADS999999()
    {
        await SetBreadcrumbAsync(SFUNO, "ADS999999", "平台管理");

        var model = await _ads999999Service.GetSystemInfoAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("ADS999999", model);
        }
        return View(model);
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateSystemInfo([FromBody] ADS999999ViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "資料驗證失敗" });
        }

        model.ModifyName = _userContext.UserName;
        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.ModEvent);

        var result = await _ads999999Service.UpdateSystemInfoAsync(model, dataLogger);

        if (result.success)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return Json(new
            {
                success = true,
                message = "系統設定已更新，請重新登入以套用變更。",
                redirectUrl = Url.Content("~/AdminPortal/Account/Login")
            });
        }
        return BadRequest(new { success = false, message = result.message });
    }

    private WebDataLogger CreateLogger(En_DataEventMode eventMode)
    {
        return new WebDataLogger
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "平台管理",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = eventMode
        };
    }
    #endregion

    #region 上傳圖片
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UploadBackgroundImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "請選擇檔案" });
        }

        if (Path.GetExtension(file.FileName).ToLower() != ".png")
        {
            return Json(new { success = false, message = "僅允許上傳 PNG 格式" });
        }

        try
        {
            string targetFolder = @"D:\Dictionary\01.Source\EHRIS\EHRIS.Web\wwwroot\image\Login";
            string targetPath = Path.Combine(targetFolder, "Logon_backgroud_Custom.png");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new { success = true, message = "背景圖已成功更新" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"上傳失敗：{ex.Message}" });
        }
    }
    #endregion

    #region 權限管理
    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetLevelData()
    {
        var model = await _ads999999Service.GetLevelDataAsync();
        return PartialView("ADS999999EDT", model);
    }

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Update)]
    public async Task<IActionResult> UpdateLevelData([FromBody] LevelDataViewModel model)
    {
        if (model == null || model.Levels == null)
        {
            return Json(new { success = false, message = "資料格式錯誤" });
        }

        model.ModifyName = _userContext.UserName;
        WebDataLogger dataLogger = CreateLogger(En_DataEventMode.ModEvent);

        var result = await _ads999999Service.UpdateLevelDataAsync(model, dataLogger);

        if (result.success)
        {
            return Json(new { success = true, message = result.message });
        }

        return BadRequest(new { success = false, message = result.message });
    }
    #endregion
}