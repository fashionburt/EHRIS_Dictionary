using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories;
using EHRIS.Services.Common;
using EHRIS.Tools.Crypto;
using EHRIS.Tools.Extensions;
using EHRIS.Tools.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using MailMessage = EHRIS.Core.Entities.MailMessage;

namespace EHRIS.Services.Services;
public class AccountsService : IAccountsService
{
    private readonly IAccountsRepository _accountsRepo;
    private readonly ICommonService _commonService;

    public AccountsService(IAccountsRepository accountsRepo, ICommonService commonService)
    {
        _accountsRepo = accountsRepo;
        _commonService = commonService;
    }

    //public async Task<Account?> Login(string accLogin, string accPassword)
    //{
    //    var account = await _accountsRepo.GetAccountByLogin(accLogin);
    //    if (account == null || account.AccPainText != accPassword)
    //        return null; // 如果驗證失敗，回傳 `null`

    //    //var peopleinfo = await _accountsRepo.GetLoginUserInfoByLogin(accLogin);
    //    //GLoginUserInfo.LoadSystemInfo(peopleinfo);


    //    return account; // 回傳 `Account` 物件
    //}
    #region 登入
    public async Task<LoginUserInfo> LoginAsync(string uxID, string password, string clientIp)
    {
        var account = await _accountsRepo.GetAccountByLogin(uxID);

        if (account != null && PasswordHasher.VerifyPassword(password, account.AccPasswdSalt, _commonService.MasterKey, account.AccPasswdHash))
        {
            var peoData = await GetLoginUserInfoByLogin(uxID);
             
            // 呼叫 CommonService.LogOperateAsync 寫入行為操作紀錄
            await _commonService.LogOperateAsync(
                peoUid: peoData.peo_uid,
                depName: peoData.dep_name ?? "N/A",
                proName: peoData.pro_name ?? "N/A",
                peoName: peoData.peo_name ?? account.AccLogin,
                sfuNo: 1000,              // 功能編號 (自訂，例如登入功能代碼)
                sfuName: "一般登入",           // 功能名稱
                function: En_OperatorMode.登入, // 使用 Enum，不是 int

                memo: $"使用者 {peoData.peo_name ?? account.AccLogin} ({account.AccLogin}) 成功登入"
            );

            return peoData;
        }
        return null;
    }

    public async Task<LoginUserInfo> AdminLoginAsync(string uxID, string password, string clientIp)
    {
        var account = await _accountsRepo.GetAdminAccountByLogin(uxID);

        if (account != null && PasswordHasher.VerifyPassword(password, account.AduPasswdSalt, _commonService.MasterKey, account.AduPasswdHash))
        {
            var peoData = await _accountsRepo.GetAdminLoginUserInfoByLogin(uxID);

            try
            {
                var masterKey = Environment.GetEnvironmentVariable("EHRISKey") ?? _commonService.MasterKey;
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1. 取得並解密位階定義與使用者映射
                string levelJson = AESCrypto.Decrypt(await _accountsRepo.GetSystemLevelDataAsync(), masterKey);
                string mappingJson = AESCrypto.Decrypt(await _accountsRepo.GetUserLevelMappingAsync(), masterKey);

                var levels = JsonSerializer.Deserialize<List<SystemLevelDataDto>>(levelJson, options) ?? new();
                var mappings = JsonSerializer.Deserialize<List<UserLevelMappingDto>>(mappingJson, options) ?? new();

                // 2. 找出該使用者的位階編號與對應等級 (Rank)
                var adlNo = mappings.FirstOrDefault(m => m.AduNo == peoData.adu_no)?.AdlNo ?? 0;
                var userLevel = levels.FirstOrDefault(l => l.AdlNo == adlNo);

                // 3. 填入 LoginUserInfo 以供 Claims 使用
                peoData.adl_no = adlNo;
                peoData.adl_rank = userLevel?.AdlRank ?? 0;
            }
            catch { /* 解密失敗則維持預設 0 */ }

            // 呼叫 CommonService.LogOperateAsync 寫入行為操作紀錄
            await _commonService.LogOperateAsync(
                peoUid: peoData.peo_uid,
                depName: peoData.dep_name ?? "N/A",
                proName: peoData.pro_name ?? "N/A",
                peoName: peoData.peo_name ?? account.AduLogin,
                sfuNo: 1003,              // 功能編號 (自訂，例如登入功能代碼)
                sfuName: "後台登入",           // 功能名稱
                function: En_OperatorMode.登入, // 使用 Enum，不是 int

                memo: $"使用者 {peoData.peo_name ?? account.AduLogin} ({account.AduLogin}) 成功登入"
            );
            return peoData;
        }
        return null;
    }

    public async Task<LoginUserInfo?> GetLoginUserInfoByLogin(string accLogin)
    {
        var peopleinfo = await _accountsRepo.GetLoginUserInfoByLogin(accLogin);

        return peopleinfo;
    } 

    public async Task<LoginUserInfo?> GetAdminUsersAsync(string aduLogin)
    { 
        var peopleinfo = await _accountsRepo.GetLoginUserInfoByLogin(aduLogin);

        return peopleinfo;
    }

    /// <summary>
    /// 查詢登入者帳號資料
    /// </summary>
    /// <param name="accLogin"></param>
    /// <returns></returns>
    public async Task<Account?> GetAccountAsync(string accLogin)
    {
        var account = await _accountsRepo.GetAccountByLogin(accLogin);
        if (account == null )
            return null; // 如果驗證失敗，回傳 `null`

        //var peopleinfo = await _accountsRepo.GetLoginUserInfoByLogin(accLogin);
        //GLoginUserInfo.LoadSystemInfo(peopleinfo);

        return account; // 回傳 `Account` 物件
    }
    #endregion

    public async Task<(int? accNo, string? basName, int? peo_uid)> GetAccountNoByEmail(string email)
    {
        return await _accountsRepo.GetAccountNoByEmail(email);
    }
    public async Task<AccountInfo> GetAccountNoByIdCard(string idCard, DateTime birthDay)
    {
        return await _accountsRepo.GetAccountNoByIdCard(idCard, birthDay);
    }

    //忘記密碼
    public async Task<(bool success, string message, string urlSafe)> ForgotPasswordAsync(ForgetPasswordModel request, string clientIp)
    {

        var existsAccount = await GetAccountNoByIdCard(request.IdCard, request.Birthday);
        if (existsAccount?.AccNo == null)
        {
            return (false, "您的電子郵件不存在，請確認是否正確", "");
        }

        WebDataLogger dataLogger = new WebDataLogger
        {
            ExecUID = existsAccount.PeoUid ?? 0,
            ExecSfuNo = 1001,
            ExecProName = "忘記密碼",
            ExecFromIP = clientIp,
            ToPeoUID = existsAccount.PeoUid ?? 0,
            EventType = En_DataEventMode.AddEvent,
        };

        var result = await AddForgetPassWord(
            existsAccount.AccNo.Value,
            existsAccount.BasName ?? "",
            existsAccount.PeoUid ?? 0,
            existsAccount.Email ?? "" ,
            clientIp
        );

        if (result.success)
        { 
            await _commonService.LogOperateAsync(
                peoUid: existsAccount.PeoUid ?? 0,
                depName: "",
                proName: "",
                peoName: existsAccount.BasName ?? $"身分證號{request.IdCard}",
                sfuNo: 1001,
                sfuName: "忘記密碼",
                function: En_OperatorMode.申請,
                memo: $"使用者 {existsAccount.BasName} ({existsAccount.AccNo}) 申請忘記密碼"
            );
        }

        return (result.success, result.success ? "系統已寄送重設密碼的連結" : result.message, result.urlSafe);
    }

    /// <summary>
    /// 寫入忘記密碼table及mailmessage table
    /// </summary>
    /// <param name="accno">帳號</param>
    /// <param name="basName">人員姓名</param>
    /// <param name="peo_uid">people.peo_uid</param>
    /// <param name="forgetEmail">忘記密碼填的email</param>
    /// <returns></returns>
    public async Task<(bool success, string message,string urlSafe)> AddForgetPassWord(int accno, string basName, int peo_uid, string forgetEmail, string clientIp)
    {
        string token = _commonService.GenerateToken();
        string verificationCode = _commonService.GenerateVerificationCode(6);
        try
        {
            ForgetPassWord forgetPassWord = new ForgetPassWord();
            forgetPassWord.FpwAccno = accno;
            forgetPassWord.FpwToken = token;
            forgetPassWord.FpwCode = verificationCode;
            forgetPassWord.FpwState = "0";
            forgetPassWord.FpwCdate = DateTime.Now; //寫入時間
            forgetPassWord.FpwCodeTime = DateTime.Now.AddMinutes(10);  //到期時間
            forgetPassWord.FpwCreateTime = DateTime.Now;
            forgetPassWord.FpwCreateName = basName;
            forgetPassWord.FpwModifyTime = DateTime.Now;
            forgetPassWord.FpwModifyName = basName;

            #region 寫入mailmessage
            string tokendata = $"{forgetEmail}|{accno}|{token}";
            string encrypted = SecureEncryptor.Encrypt(tokendata, _commonService.MasterKey);
            string urlSafe = Uri.EscapeDataString(encrypted);
            
            //抓樣版  1:忘記密碼
            int mailType = 1;
            var mailTemplate = await _commonService.GetEmailTemplateAsync(mailType);
            //指定參數
            var mailParameters = new Dictionary<string, string>
                    {
                        { "SYI_NAME", AppConfig.syi_name },
                        { "FPW_CDDATE",  forgetPassWord.FpwCdate.ToRocDateTime() },
                        { "FPW_CODE", verificationCode },
                        { "FPW_CODETIME", forgetPassWord.FpwCodeTime.ToRocDateTime( ) }
                    };
            //替換參數
            string subject = _commonService.RenderTemplate(mailTemplate.mat_subject, mailParameters);
            string body = _commonService.RenderTemplate(mailTemplate.mat_content, mailParameters);

            //
            MailMessage mailMessage=new MailMessage();
            mailMessage.PeoUid=peo_uid;
            mailMessage.MaiType = mailType;
            mailMessage.MaiSubject = subject;
            mailMessage.MaiContent = body;
            mailMessage.MaiEmail = forgetEmail;
            mailMessage.MaiStatus = "0";
            mailMessage.MaiCreateName = basName;
            mailMessage.MaiCreateTime = DateTime.Now;
            mailMessage.MaiModifyName = basName;
            mailMessage.MaiModifyTime= DateTime.Now;

            #endregion
             
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = peo_uid,

                ExecSfuNo = 1001,
                ExecProName = "忘記密碼",

                ExecFromIP = clientIp,

                ToPeoUID = peo_uid,
                EventType = En_DataEventMode.AddEvent,
            };
            var addResult = await _accountsRepo.AddForgetPassWordAsync(forgetPassWord, mailMessage, dataLogger);

            
            // 呼叫 CommonService.LogOperateAsync 寫入行為操作紀錄
            await _commonService.LogOperateAsync(
                peoUid: peo_uid,
                depName: "",
                proName: "",
                peoName: basName ,
                sfuNo: 1001,              // 功能編號 (自訂，例如登出功能代碼)
                sfuName: "忘記密碼",           // 功能名稱
                function: En_OperatorMode.密碼變更, // 使用 Enum
                memo: $"使用者 {basName} ({accno}) 申請忘記密碼"
            );

            string returnMsg = "";
            if (string.IsNullOrEmpty(addResult.message))
            {
                returnMsg = "忘記密碼成功";

            }
            else
            {
                returnMsg = addResult.message;
            }
                return (addResult.success, returnMsg, urlSafe);
        }
        catch (Exception ex)
        {
            return (false, "忘記密碼失敗: " + ex.Message,"");
        }
    }
    public async Task<ForgetPassWord?> GetForgetPassWordInfoByEmail(string email)
    {
        var forgetInfo =await _accountsRepo.GetForgetPassWordInfoByEmailAsync(email);
        
        return forgetInfo;
    }
    /// <summary>
    /// 設定新密碼
    /// </summary>
    /// <param name="forgetInfo">ForgetPassWord 忘記密碼資訊</param>
    /// <param name="accPassword">新密碼</param>
    /// <returns></returns>
    public async Task<(bool result, string errMeg)> SetNewPassWordAsync(ForgetPassWord forgetInfo,string accPassword, IDataLogger dataLogger)
    {
        var SetNewPassword = (result: true, errMeg: "OK");
        //先檢查帳號和驗證碼
        if (await _accountsRepo.existVerifyCode(forgetInfo.FpwAccno, forgetInfo.FpwCode))
        {
            var acc_passwdHash = "";
            var acc_passwdSalt = "";
            //新密碼加密
            string hmacKeyStr = _commonService.MasterKey;
            byte[] hmacKey = Encoding.UTF8.GetBytes(hmacKeyStr);
            (string Hash, string Salt) hashPassword = PasswordHasher.HashPassword(accPassword, hmacKey);
            //設定新密碼
            SetNewPassword = await _accountsRepo.SetNewPassWordAsync(forgetInfo.FpwId
                                                , forgetInfo.FpwAccno, accPassword, hashPassword.Hash, hashPassword.Salt, dataLogger);

            var peoData = await _accountsRepo.GetUserInfoByAccount(forgetInfo.FpwAccno);
            await _commonService.LogOperateAsync(
              peoUid: peoData.peo_uid,
              depName: "",
              proName: "",
              peoName: peoData.peo_name,
              sfuNo: 1001,                 // 功能編號 (自訂，例如登出功能代碼)
              sfuName: "修改密碼",         // 功能名稱
              function: En_OperatorMode.密碼變更, // 使用 Enum
              memo: $"使用者 {peoData.peo_name} ({peoData.acc_login}) 申請修改密碼"
          );
        }
        else
        {
            SetNewPassword.result = false;
            SetNewPassword.errMeg = "驗證碼錯誤";
        }
        return SetNewPassword;
    }
    public async Task<(bool result, string errMeg)> SetPersonalNewPassWordAsync(int Accno, string accPassword, IDataLogger dataLogger)
    {
        var SetNewPassword = (result: true, errMeg: "OK");
       
            var acc_passwdHash = "";
            var acc_passwdSalt = "";
            //新密碼加密
            string hmacKeyStr = _commonService.MasterKey;
            byte[] hmacKey = Encoding.UTF8.GetBytes(hmacKeyStr);
            (string Hash, string Salt) hashPassword = PasswordHasher.HashPassword(accPassword, hmacKey);
            //設定新密碼
            SetNewPassword = await _accountsRepo.SetPersonalNewPassWordAsync(Accno, accPassword, hashPassword.Hash, hashPassword.Salt, dataLogger);

        var peoData = await _accountsRepo.GetUserInfo(Accno);
        await _commonService.LogOperateAsync(
          peoUid: peoData.peo_uid,
          depName: "",
          proName: "",
          peoName: peoData.peo_name,
          sfuNo: 1001,                 // 功能編號 (自訂，例如登出功能代碼)
          sfuName: "修改密碼",         // 功能名稱
          function: En_OperatorMode.密碼變更, // 使用 Enum
          memo: $"使用者 {peoData.peo_name} ({peoData.acc_login}) 申請修改密碼"
      );


        return SetNewPassword;
    }
    public  async Task<(bool IsValid, string ErrorMessage)> ValidatePassword(
        string password, int accno,bool checkOldPwd =false,string oldPassword ="")
    { 
        // 1. 長度
        if (password.Length < 8 || password.Length > 20)
            return (false, "密碼長度需在 8 到 20 碼之間");

        // 2. 字元類型檢查
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => "~!@#$".Contains(c));

        int categoryCount = new[] { hasUpper, hasLower, hasDigit, hasSpecial }.Count(x => x);
        if (categoryCount < 3)
            return (false, "密碼必須包含大小寫、數字、特殊符號(~!@#$)中至少三種");

        string accLogin = "";
        DateTime? lastChangedDate=null;
        
        var accountInfo = await _accountsRepo.GetAccountByAccno(accno);
        if (accountInfo != null)
        {
            accLogin = accountInfo.AccLogin;
            lastChangedDate = accountInfo.AccPwChange;
        }
        //變更密碼時, 要檢查舊密碼是否正確
        string hmacKeyStr = _commonService.MasterKey;
        if (checkOldPwd)
        {
            if (accountInfo != null)
            {
               if (! PasswordHasher.VerifyPassword(oldPassword, accountInfo.AccPasswdSalt, hmacKeyStr, accountInfo.AccPasswdHash))
                {
                    return (false, "請確認舊密碼");
                }
            }
            else
            {
                return (false, "查無舊密碼");
            }

        }
        // 3. 不可和帳號相同
        if (password.Equals(accLogin, StringComparison.OrdinalIgnoreCase))
            return (false, "密碼不可與帳號相同");

        // 4. 不可和前 3 組相同
        var lastPasswords = await _accountsRepo.GetLastPasswordAsync(accno);
       
        if (PasswordHasher.IsPasswordReused(password, hmacKeyStr, lastPasswords))
        {
            return (false, "密碼不可與前 3 組相同");
        }

      
        // 5. 最短使用期限 1 天
        if (lastChangedDate.HasValue && lastChangedDate.Value.AddDays(1) > DateTime.Now)
            return (false, "密碼最短使用期限為 1 天");

        return (true, "密碼驗證通過");
    }

    public async Task<People?> GetPeopleByAccno(int accno)
    {
        return await _accountsRepo.GetPeopleByAccno(accno);
    }

    public async Task LogoutAsync(int peo_uid, string dep_name, string pro_name, string peo_name, string acc_login, HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        httpContext.Session.Clear();

        if (httpContext.Request.Cookies["RememberMe"] == "False")
        {
            httpContext.Response.Cookies.Delete("UserName");
        }
         
        // 呼叫 CommonService.LogOperateAsync 寫入行為操作紀錄
        await _commonService.LogOperateAsync(
            peoUid: peo_uid,
            depName: dep_name ?? "N/A",
            proName: pro_name ?? "N/A",
            peoName: peo_name ?? acc_login,
            sfuNo: 1000,              // 功能編號 (自訂，例如登出功能代碼)
            sfuName: "一般登出",           // 功能名稱
            function: En_OperatorMode.登出, // 使用 Enum
            memo: $"使用者 {peo_name ?? acc_login} ({acc_login}) 成功登出"
        );
    }
}
