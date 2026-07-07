using DNTCaptcha.Core;
using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Event;
using EHRIS.Security.Permission;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Tools.Crypto;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static EHRIS.Core.Models.LoginViewModel;

namespace EHRIS.Web.Controllers;

public class AccountController : Controller
{
    private readonly ICommonService _commonService;
    private readonly IAccountsService _accountsService;
    //private readonly IEventObjectRepository _eventOperator;
    private readonly IPermissionService _permissionService;

    private readonly IDNTCaptchaValidatorService _captchaValidatorService;
    private readonly IDNTCaptchaApiProvider _captchaApiProvider;

    private readonly IDbHealthCheck _dbHealthCheck;
    private readonly ISystemConfigService _systemConfigService;
    private readonly FieldsMappingService _fieldMappingService;
    private readonly IUserContextService _userContext;
    private readonly IADS999001Service _noticeService;

    /// <summary>
    /// 登入首頁
    /// </summary>
    /// <param name="commonService"></param>
    /// <param name="accountsService"></param>
    /// <param name="captchaValidatorService"></param>
    /// <param name="captchaApiProvider"></param>
    /// <param name="dbHealthCheck"></param>
    /// <param name="systemConfigService"></param>
    /// <param name="fieldMappingService"></param>
    /// <param name="permissionService"></param>
    /// <param name="userContext"></param>
    /// <param name="announcementService"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AccountController(ICommonService commonService, IAccountsService accountsService
        //, IEventObjectRepository eventOperator
        , IDNTCaptchaValidatorService captchaValidatorService
        , IDNTCaptchaApiProvider captchaApiProvider
        , IDbHealthCheck dbHealthCheck
        , ISystemConfigService systemConfigService, FieldsMappingService fieldMappingService
        , IPermissionService permissionService
        , IUserContextService userContext
        , IADS999001Service noticeService
        )
    {
        _commonService = commonService ?? throw new ArgumentNullException(nameof(commonService));
        _accountsService = accountsService ?? throw new ArgumentNullException(nameof(accountsService));
        //_eventOperator = eventOperator ?? throw new ArgumentNullException(nameof(eventOperator));
        _captchaValidatorService = captchaValidatorService;
        _captchaApiProvider = captchaApiProvider;
        _dbHealthCheck = dbHealthCheck;
        _systemConfigService = systemConfigService;
        _fieldMappingService = fieldMappingService;

        _permissionService = permissionService;
        _userContext = userContext;
        _noticeService = noticeService;
    }

    // GET: /Account/Login
    [HttpGet]
    public async Task<IActionResult> Login(int page = 1)
    {
        var model = new LoginViewModel
        {
            RememberMe = false,
        };

        if (HttpContext.Items.ContainsKey("DbErrorMessage"))
        {
            ModelState.AddModelError(string.Empty, HttpContext.Items["DbErrorMessage"]?.ToString());
        }
        //// 如果之前有登入且設置了 cookie，可嘗試回填
        //if (Request.Cookies.TryGetValue("RememberMe", out var rememberMeValue) &&
        //    bool.TryParse(rememberMeValue, out var rememberMe))
        //{
        //    model.RememberMe = rememberMe;
        //}

        // 如果之前有勾 RememberMe，才回填帳號
        var lastAccount = HttpContext.Session.GetString("LastLoginAccount");
        if (string.IsNullOrEmpty(lastAccount))
        {
            lastAccount = Request.Cookies["LastLoginAccount"];
        }

        if (!string.IsNullOrEmpty(lastAccount))
        {
            model.UxID = lastAccount;
            model.RememberMe = true;
        }

        model.Notices = await _noticeService.GetLoginNoticesAsync(10) ?? new List<LoginNoticeViewModel>();
        _systemConfigService.LoadSystemInfo(_commonService.MasterKey);
        if (!string.IsNullOrEmpty(AppConfig.config_message))
        {
            this.AlertError(AppConfig.config_message);
            ViewBag.LoginDisabled = true;
        }

        return View(model);

    }
    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // 系統停用時，後端也擋（防止前端 disabled 被繞過）
        _systemConfigService.LoadSystemInfo(_commonService.MasterKey);
        if (!string.IsNullOrEmpty(AppConfig.config_message))
        {
            this.AlertError(AppConfig.config_message);
            return RedirectToAction("Login");
        }

        var dbMessage = HttpContext.Items["DbErrorMessage"]?.ToString();
        if (!string.IsNullOrEmpty(dbMessage) && !ModelState.Values.SelectMany(v => v.Errors).Any(e => e.ErrorMessage == dbMessage))
        {
            ModelState.AddModelError("", dbMessage);
        }
        if (!_captchaValidatorService.HasRequestValidCaptchaEntry())
        {
            ModelState.AddModelError("DNTCaptchaInputText", "驗證碼錯誤");

        }
        if (!ModelState.IsValid)
        {

            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.UxID) || string.IsNullOrWhiteSpace(model.MbrKey))
        {
            this.AlertError("帳號與密碼不可為空！");
            return RedirectToAction("Login");

        }

        string clientIp = IPHelper.GetIpAddress(HttpContext);
        string hmacKeyStr = _commonService.MasterKey;

        #region 登入成功
        var peoData = await _accountsService.LoginAsync(model.UxID, model.MbrKey, clientIp);
        if (peoData != null)
        {
            // 1. 建立身份驗證 Claims (Cookie) 
            var claims = new List<Claim>
            {
                //要於Cookie的值加在這裡
                new Claim("ServiceMode", "NORMAL"),
                new Claim(ClaimTypes.Name, peoData.acc_login),
                new Claim(ClaimTypes.NameIdentifier, peoData.acc_no.ToString()),
                new Claim("AccNo", peoData.acc_no.ToString()), //Account.AccNo
                new Claim("UserAccount", model.UxID),         //帳號

                new Claim("PeoUID", peoData.peo_uid.ToString()),             //人員編號
                new Claim("TopChangeLoginUID", peoData.peo_uid.ToString()),  //最上層人員編號 

                new Claim("UserName", peoData.peo_name.ToString()),           //人員姓名

                new Claim("UserDepartmentNO", peoData.dep_no.ToString()),    //單位編號
                new Claim("UserDepartmentName", peoData.dep_name.ToString()),//單位  
                new Claim("UserUnitName", peoData.uni_name.ToString()),      //機關名稱
                new Claim("UserPtyNO", peoData.pty_no.ToString()),           //人員類別編號  
                new Claim("UserPtyName", peoData.pty_name.ToString()),       //人員類別  
                new Claim("UserProfessNO", peoData.pro_no.ToString()),       //職稱編號 
                new Claim("UserProfessName", peoData.pro_name.ToString()),   //職稱
                new Claim("SourceIP", clientIp ),   //來源IP
            };

            // 1.1 建立權限列表
            var perList = await _permissionService.GetPermissionFunction(peoData.acc_no);
            foreach (var p in perList)
            {
                if (p.CanQuery)
                    claims.Add(new Claim($"FUNCTION:{p.SfuNo}:Query", "true"));
                if (p.CanCreate)
                    claims.Add(new Claim($"FUNCTION:{p.SfuNo}:Insert", "true"));
                if (p.CanUpdate)
                    claims.Add(new Claim($"FUNCTION:{p.SfuNo}:Update", "true"));
                if (p.CanDelete)
                    claims.Add(new Claim($"FUNCTION:{p.SfuNo}:Delete", "true"));
            }


            // 2. 建立身份認證票據(Cookie)            
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, "Custom"));
            var principal = new ClaimsPrincipal(identity);

            // 2.5 Cookie，並根據 `RememberMe` 設定過期時間
            bool remember = model.RememberMe == true;
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = remember, //  記住我功能
                ExpiresUtc = remember
                             ? DateTime.UtcNow.AddDays(30)    // 記住我 => 30 天
                             : DateTime.UtcNow.AddMinutes(60) // 一般登入 => 60 分鐘
            };

            if (model.RememberMe)
            {
                HttpContext.Session.SetString("LastLoginAccount", model.UxID);

                Response.Cookies.Append("LastLoginAccount", model.UxID, new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,   // 防止 JS 存取
                    Secure = true,     // 僅允許 HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(30) // 與 RememberMe 同步
                });
            }
            else
            {
                HttpContext.Session.Remove("LastLoginAccount");
                Response.Cookies.Delete("LastLoginAccount");
            }


            // 3. 登入 Cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          principal,
                                          authProperties);
            HttpContext.User = principal;

            // 4. 寫入 Session
            HttpContext.Session.SetString("UserAccount", peoData.acc_login);
            HttpContext.Session.SetString("UserId", peoData.acc_no.ToString());
            HttpContext.Session.SetString("PeoUid", peoData.peo_uid.ToString());



            return RedirectToAction("Index", "Home");
        }
        #endregion
        else
        {

            ModelState.AddModelError("", "登入失敗，帳號或密碼錯誤！");
            return View(model);
        }


    }


    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        int peo_uid = _userContext.PeoUID;
        string dep_name = _userContext.UserDepartmentName;
        string pro_name = _userContext.UserProfessName;
        string peo_name = _userContext.UserName;
        string acc_login = _userContext.UserAccount;

        await _accountsService.LogoutAsync(peo_uid, dep_name, pro_name, peo_name, acc_login, HttpContext);
        return RedirectToAction("Login");
    }


    [HttpGet]
    [Route("Captcha/Generate")]
    public IActionResult Generate()
    {
        var captcha = _captchaApiProvider.CreateDNTCaptcha(new DNTCaptchaTagHelperHtmlAttributes
        {
            Language = DNTCaptcha.Core.Language.English,
            Max = 999999,
            Min = 100000,
            DisplayMode = DisplayMode.ShowDigits,
            UseRelativeUrls = true,
            FontName = "arial",
            FontSize = 30
        });

        return Json(new
        {
            imageUrl = captcha.DntCaptchaImgUrl,   // 圖片網址 
            captchaToken = captcha.DntCaptchaTokenValue,
            captchaText = captcha.DntCaptchaTextValue
        });
    }


    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgetPasswordModel request)
    {
        if (string.IsNullOrWhiteSpace(request.IdCard))
        {
            ModelState.AddModelError("", "請輸入身分證號");
            return Json(new { success = false, message = "請輸入身分證號" });
        }
        if (!_captchaValidatorService.HasRequestValidCaptchaEntry())
        {
            return Json(new { success = false, message = "驗證碼錯誤", urlsafe = "" });
        }

        string clientIp = IPHelper.GetIpAddress(HttpContext);
        var result = await _accountsService.ForgotPasswordAsync(request, clientIp);

        return Json(new { success = result.success, message = result.message, urlsafe = result.urlSafe });

    }
    [HttpGet]
    public IActionResult ResetPassword(string Code)
    {
        if (string.IsNullOrEmpty(Code))
            return BadRequest();

        string decrypted = SecureEncryptor.Decrypt(Uri.UnescapeDataString(Code), _commonService.MasterKey);
        var verifyInfo = decrypted.Split('|');
        string email = verifyInfo[0];
        ViewBag.VerifyCode = Code;
        ViewBag.Notice = string.Format(@"{0} 已發送認證信件至您的信箱[{1}]，請於時限內輸入認證碼並按下確認鍵，以利系統重置您的密碼。若無法收到認證信件，請重新執行忘記密碼或向人事管理者確認資料是否正確。", AppConfig.syi_name, email);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromBody] ForgetPasswordModel request)
    {
        if (request == null)
        {
            return Json(new { success = false, message = "連結無效或已過期" });
        }

        string verifydata = request.verifydata;
        string verificationCode = request.verificationCode;
        string newPassword = request.newPassword;
        string confirmPassword = request.confirmPassword;

        string message = "";
        string decrypted = SecureEncryptor.Decrypt(Uri.UnescapeDataString(verifydata), _commonService.MasterKey);

        var verifyInfo = decrypted.Split('|');
        string email = verifyInfo[0];
        int accno = 0;
        int.TryParse(verifyInfo[1], out accno);
        string token = verifyInfo[2];
        var forgetInfo = await _accountsService.GetForgetPassWordInfoByEmail(email);

        if (forgetInfo == null ||
            forgetInfo.FpwToken != token ||
            forgetInfo.FpwCode != verificationCode ||
            forgetInfo.FpwCodeTime < DateTime.Now)
        {
            return Json(new { success = false, message = "連結無效或已過期" });
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "兩次密碼不一致");
            return Json(new { success = false, message = "兩次密碼不一致" });
        }

        // 確保 ValidatePassword 與 SetNewPassWordAsync 使用安全雜湊
        var (isValid, validMessage) = await _accountsService.ValidatePassword(newPassword, accno);

        if (!isValid)
        {
            return Json(new { success = false, message = validMessage });
        }
        var existsPeople = await _accountsService.GetPeopleByAccno(accno);
        var peoUID = 0;
        if (existsPeople != null)
        {
            peoUID = existsPeople.PeoUid;
        }
        string clientIp = IPHelper.GetIpAddress(HttpContext);

        WebDataLogger dataLogger = new WebDataLogger()
        {
            ExecUID = peoUID,

            ExecSfuNo = 1001,
            ExecProName = "重設密碼",

            ExecFromIP = clientIp,

            ToPeoUID = peoUID,
            EventType = En_DataEventMode.ModEvent,
        };

        var setNewPassword = await _accountsService.SetNewPassWordAsync(forgetInfo, newPassword, dataLogger);
        if (setNewPassword.result)
        {
            message = "密碼已重設成功，請重新登入";

        }
        else
        {
            message = "密碼設定失敗!";

        }
        return Json(new { success = setNewPassword.result, message = message });

        //return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (model == null || !ModelState.IsValid)
            return Json(new { success = false, message = "資料格式錯誤" });

        if (string.IsNullOrWhiteSpace(model.OldPassword) ||
            string.IsNullOrWhiteSpace(model.NewPassword) ||
            string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            return Json(new { success = false, message = "密碼欄位不可為空" });
        }

        if (model.NewPassword != model.ConfirmPassword)
            return Json(new { success = false, message = "新密碼與確認密碼不符" });

        if (model.OldPassword == model.NewPassword)
            return Json(new { success = false, message = "新密碼不能與舊密碼相同" });

        // 驗證舊密碼是否正確 + 新密碼強度
        var (isValid, validMessage) = await _accountsService.ValidatePassword(
            model.NewPassword, _userContext.AccNO, true, model.OldPassword);
        if (!isValid)
            return Json(new { success = false, message = validMessage });

        //帳號是否存在
        var existsPeople = await _accountsService.GetPeopleByAccno(_userContext.AccNO);
        var peoUID = existsPeople?.PeoUid ?? 0;

        string clientIp = IPHelper.GetIpAddress(HttpContext);
        WebDataLogger dataLogger = new WebDataLogger()
        {
            ExecUID = peoUID,

            ExecSfuNo = 1001,
            ExecProName = "變更密碼",

            ExecFromIP = clientIp,

            ToPeoUID = peoUID,
            EventType = En_DataEventMode.ModEvent,
        };

        var setNewPassword = await _accountsService.SetPersonalNewPassWordAsync(
        _userContext.AccNO, model.NewPassword, dataLogger);

        string message = setNewPassword.result ? "密碼已變更成功，請重新登入" : "密碼設定失敗";

        return Json(new { success = setNewPassword.result, message });
    }
    [AllowAnonymous]
    [HttpPost]
    public IActionResult KeepAlive()
    {
        // 任何對 Session 的讀寫都會自動刷新 Session Timeout
        HttpContext.Session.SetString("KeepAlive", DateTime.Now.ToString());

        return Json(new { success = true });
    }
}
