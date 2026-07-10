using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories;

public interface IAccountsRepository : IBaseRepository
{
    Task<Account?> GetAccountByLogin(string accLogin);
    Task<AdminUsers?> GetAdminAccountByLogin(string aduLogin);
    Task<Account?> GetAccountByAccno(int accno);
    Task<LoginUserInfo?> GetLoginUserInfoByLogin(string accLogin);
    Task<LoginUserInfo?> GetAdminLoginUserInfoByLogin(string accLogin);

    Task<string> GetSystemLevelDataAsync();
    Task<string> GetUserLevelMappingAsync();


    Task<(int? accNo, string? basName, int? peo_uid)> GetAccountNoByEmail(string email);

    Task<AccountInfo> GetAccountNoByIdCard(string idCard, DateTime birthDay);

    //Task<EventObjectDto> GetEventDeptInfo(int uid);
    Task<(bool success, string message)> AddForgetPassWordAsync(ForgetPassWord forgetPassWord, Entities.MailMessage mailMessage, IDataLogger dataLogger);
    Task<ForgetPassWord?> GetForgetPassWordInfoByEmailAsync(string email);
    Task<List<(string, string)>> GetLastPasswordAsync(int accno);
    Task<bool> existVerifyCode(int accno, string verifyCode);
    Task<(bool result, string errMeg)> SetNewPassWordAsync(Guid fpw_id, int accno, string accPassword, string acc_passwdHash, string acc_passwdSalt, IDataLogger dataLogger);
    Task<(bool result, string errMeg)> SetPersonalNewPassWordAsync(int accno, string accPassword, string acc_passwdHash, string acc_passwdSalt, IDataLogger dataLogger);
    //Task CreateAccount(Account account);
    //Task InsertLog(Account account);
    Task<People?> GetPeopleByAccno(int accno);


}
