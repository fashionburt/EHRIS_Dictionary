using DocumentFormat.OpenXml.Spreadsheet;
using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.AspNetCore.Http;

namespace EHRIS.Services.Services;

public interface IAccountsService
{
    //登入
    //Task<Account?> Login(string accLogin, string accPassword);
    Task<LoginUserInfo> LoginAsync(string uxID, string password, string clientIp);    
    Task<Account?> GetAccountAsync(string accLogin);

    //後台登入
    Task<LoginUserInfo> AdminLoginAsync(string uxID, string password, string clientIp);
    Task<LoginUserInfo?> GetAdminUsersAsync(string aduLogin);

    //登出
    Task LogoutAsync(int peo_uid, string dep_name, string pro_name, string peo_name, string acc_login, HttpContext httpContext);

    //忘記密碼
    Task<(bool success, string message, string urlSafe)> ForgotPasswordAsync(ForgetPasswordModel request, string clientIp);
    Task<AccountInfo> GetAccountNoByIdCard(string idCard, DateTime birthDay);


    Task<LoginUserInfo?> GetLoginUserInfoByLogin(string accLogin);

    Task<(int? accNo, string? basName, int? peo_uid)> GetAccountNoByEmail(string email);


    Task<(bool success, string message, string urlSafe)> AddForgetPassWord(int accno, string basName, int peo_uid, string forgetEmail, string clientIp);
    Task<ForgetPassWord?> GetForgetPassWordInfoByEmail(string email);
    Task<(bool result, string errMeg)> SetNewPassWordAsync(ForgetPassWord forgetInfo, string accPassword , IDataLogger dataLogger);
   Task<(bool result, string errMeg)> SetPersonalNewPassWordAsync(int Accno, string accPassword, IDataLogger dataLogger);
    Task<(bool IsValid, string ErrorMessage)> ValidatePassword(string password, int accno, bool checkOldPwd = false, string oldPassword = "");
    Task<People?> GetPeopleByAccno(int accno);

}
