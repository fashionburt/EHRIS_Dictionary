using DNTCaptcha.Core;
using EHRIS.Core.Models;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Event;
using EHRIS.Security.Permission;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Security.Claims;

namespace EHRIS.AdminPortal.Controllers
{
    
    [Route("AdminPortal/Account/[action]")]
    public class AdminAccountController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IAccountsService _accountsService;
        private readonly IPermissionService _permissionService;

        private readonly IDNTCaptchaValidatorService _captchaValidatorService;
        private readonly IDbHealthCheck _dbHealthCheck;
        private readonly ISystemConfigService _systemConfigService;
        private readonly FieldsMappingService _fieldMappingService;
        private readonly IUserContextService _userContext;
        public AdminAccountController(ICommonService commonService, IAccountsService accountsService
        , IDNTCaptchaValidatorService captchaValidatorService
        , IDNTCaptchaApiProvider captchaApiProvider
        , IDbHealthCheck dbHealthCheck
        , ISystemConfigService systemConfigService, FieldsMappingService fieldMappingService
        , IPermissionService permissionService
        , IUserContextService userContext)
        {
            _commonService = commonService ?? throw new ArgumentNullException(nameof(commonService));
            _accountsService = accountsService ?? throw new ArgumentNullException(nameof(accountsService));

            _captchaValidatorService = captchaValidatorService;
            //_captchaApiProvider = captchaApiProvider;
            _dbHealthCheck = dbHealthCheck;
            _systemConfigService = systemConfigService;
            _fieldMappingService = fieldMappingService;

            _permissionService = permissionService;
            _userContext = userContext;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

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
                ModelState.AddModelError("", "驗證碼錯誤");
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
            var peoData = await _accountsService.AdminLoginAsync(model.UxID, model.MbrKey, clientIp);
            if (peoData != null)
            {
                // 1. 建立身份驗證 Claims (Cookie) 
                var claims = new List<Claim>
                {
                    //要於Cookie的值加在這裡
                    new Claim("ServiceMode", "ADMIN"),
                    new Claim("ServiceAccount", model.UxID),         //客服專用帳號
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

                    // 權限管理
                    new Claim("AduNo", peoData.adu_no.ToString()),
                    new Claim("AdlNo", peoData.adl_no.ToString()),
                    new Claim("AdlRank", peoData.adl_rank.ToString()),
                };

                // 1.1 建立權限列表
                var perList = await _permissionService.GetAdminPermissionFunction(peoData.acc_no);
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

                HttpContext.Session.Remove("LastLoginAccount");
                Response.Cookies.Delete("LastLoginAccount");

                // 2.5 Cookie，並根據 `RememberMe` 設定過期時間
                bool remember = model.RememberMe == true;
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = remember, //  記住我功能
                    ExpiresUtc = remember
                                 ? DateTime.UtcNow.AddDays(30)    // 記住我 => 30 天
                                 : DateTime.UtcNow.AddMinutes(60) // 一般登入 => 60 分鐘
                };


                // 3. 登入 Cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              principal,
                                              authProperties);
                HttpContext.User = principal;

                // 4. 寫入 Session
                HttpContext.Session.SetString("UserAccount", peoData.acc_login);
                HttpContext.Session.SetString("UserId", peoData.acc_no.ToString());
                HttpContext.Session.SetString("PeoUid", peoData.peo_uid.ToString());



                return Redirect(Url.Content("~/AdminPortal/Home/Index"));
            }
            #endregion
            else
            {

                ModelState.AddModelError("", "登入失敗，帳號或密碼錯誤！");
                return View(model);
            }
        }
    }
}
