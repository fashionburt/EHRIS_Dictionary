using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NPOI.HPSF;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;
using System;
using System.Security.Principal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace EHRIS.Core.Repositories;

public class AccountsRepository : BaseRepository, IAccountsRepository
{

    public AccountsRepository(ApplicationDbContext context, AuditDbContext aduitContext) : base(context)
    {

    }
    /// <summary>
    /// GetAccountByLogin
    /// 依帳號取得Account資訊
    /// </summary>
    /// <param name="accLogin">登入帳號</param>
    /// <returns></returns>
    public async Task<Account?> GetAccountByLogin(string accLogin)
    {
        return await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccLogin == accLogin);
    }

    /// <summary>
    /// GetAdminAccountByLogin
    /// 依帳號取得後台AdminUsers資訊
    /// </summary>
    /// <param name="aduLogin"></param>
    /// <returns></returns>
    public async Task<AdminUsers?> GetAdminAccountByLogin(string aduLogin)
    {
        return await _context.AdminUsers.AsNoTracking().FirstOrDefaultAsync(a => a.AduLogin == aduLogin);
    }
    /// <summary>
    /// GetAccountByAccno
    /// 依accno取得Account資訊
    /// </summary>
    /// <param name="accno">accno</param>
    /// <returns></returns>
    public async Task<Account?> GetAccountByAccno(int accno)
    {
        return await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccNo == accno);
    }
    /// <summary>
    /// 依accno取得People
    /// </summary>
    /// <param name="accno"></param>
    /// <returns></returns>
    public async Task<People?> GetPeopleByAccno(int accno)
    {
        return await (from p in _context.Peoples
                      join a in _context.Accounts on p.PeoUid equals a.PeoUid
                      where a.AccNo == accno
                      select p).AsNoTracking().FirstOrDefaultAsync();
    }
    /// <summary>
    /// GetLoginUserInfoByLogin 
    /// 依登入帳號取得登入者資訊
    /// </summary>
    /// <param name="accLogin">登入帳號</param>
    /// <returns></returns>
    public async Task<LoginUserInfo?> GetLoginUserInfoByLogin(string accLogin)
    {
         //未來可以改成SQL查詢
        var loginInfo = await (from acc in _context.Accounts
                               join peo in _context.Peoples on acc.PeoUid equals peo.PeoUid
                               join bas in _context.Basepersons on peo.BasId equals bas.BasId
                               
                               //單位
                               join dep in _context.Department on peo.DepNo equals dep.DepNo 
                               join uni in _context.Unit on dep.UniId equals uni.UniId into luni
                               from uni in luni.DefaultIfEmpty()
                               //join depParent in _context.Department on dep.DepParentId equals depParent.DepNo into ldepParent
                               //from depParent in ldepParent.DefaultIfEmpty() //保留先不用

                               //人員類別
                               join pty in _context.PType on peo.PtyNo equals pty.PtyNo into lpty
                               from pty in lpty.DefaultIfEmpty()
                               
                               //職稱
                               join pro in _context.Profess on peo.ProNo equals pro.ProNo into lpro
                               from pro in lpro.DefaultIfEmpty()

                               where acc.AccLogin == accLogin

                               && acc.AccStatus == 1 //只找啟用人員
                               && peo.PeoJobType == 1 //只找在職人員
                               
                               select new LoginUserInfo
                               {
                                   acc_no = acc.AccNo,
                                   acc_login = accLogin,
                                    
                                   peo_uid = peo.PeoUid,
                                   peo_name = bas.BasName,

                                   dep_no = peo.DepNo,
                                   dep_name = dep.DepName,
                                   uni_name = uni!= null? uni.UniName: "",

                                   pro_no = peo.ProNo ,
                                   pro_name = pro != null ? pro.ProName : "",

                                   pty_no = peo.PtyNo ,
                                   pty_name = pty != null ? pty.PtyName : ""


                               }).AsNoTracking().FirstOrDefaultAsync();

        if (loginInfo == null)
        {
            loginInfo = new LoginUserInfo { };
        }
        return loginInfo;
    }

    /// <summary>
    /// GetAdminLoginUserInfoByLogin 
    /// 依登入帳號取得登入者資訊
    /// </summary>
    /// <param name="aduLogin">登入帳號</param>
    /// <returns></returns>
    public async Task<LoginUserInfo?> GetAdminLoginUserInfoByLogin(string aduLogin)
    {
        var loginInfo = await (from adu in _context.AdminUsers
                               where adu.AduLogin == aduLogin

                               && adu.AduStatus == 1  // 只找啟用人員 

                               select new LoginUserInfo
                               {
                                   acc_no = adu.AduNo,
                                   acc_login = aduLogin,

                                   peo_uid = 0,
                                   peo_name = adu.AduDisplayName,

                                   dep_no = 0,
                                   dep_name = "客服單位",
                                   uni_name = "客服單位",

                                   pro_no = 0,
                                   pro_name = "",

                                   pty_no = 0,
                                   pty_name = "",

                                   adu_no = adu.AduNo,
                                   adl_rank = 0,
                               }).AsNoTracking().FirstOrDefaultAsync();

        if (loginInfo == null)
        {
            loginInfo = new LoginUserInfo { };
        }
        return loginInfo;
    }
    #region 新增 systeminfo 存取方法
    /// <summary>
    /// 取得系統等級加密資訊 (syi_data)
    /// </summary>
    public async Task<string> GetSystemLevelDataAsync() =>
        await _context.Database.SqlQueryRaw<string>("SELECT syi_data AS Value FROM systeminfo WHERE syi_code = '12008'").FirstOrDefaultAsync() ?? "";
    /// <summary>
    /// 取得使用者等級映射加密資訊 (syi_dataHash)
    /// </summary>
    public async Task<string> GetUserLevelMappingAsync() =>
        await _context.Database.SqlQueryRaw<string>("SELECT syi_dataHash AS Value FROM systeminfo WHERE syi_code = '12008'").FirstOrDefaultAsync() ?? "";
    #endregion





    /// <summary>
    /// GetAccountNoByEmail 檢查email是否存在於有效帳號內
    /// </summary>
    /// <param name="email">Email</param>
    /// <returns></returns>
    public async Task<(int? accNo, string? basName, int? peo_uid)> GetAccountNoByEmail(string email)
    {

        var existAccount = await (from a in _context.BasePersonEmail
                                  join f in _context.Basepersons on a.BasId equals f.BasId
                                  join b in _context.Peoples on a.BasId equals b.BasId
                                  join c in _context.Accounts on b.PeoUid equals c.PeoUid
                                  where c.AccStatus == 1 && a.BseEmail == email
                                  select new { c.AccNo, f.BasName, b.PeoUid }).AsNoTracking().FirstOrDefaultAsync();


        if (existAccount == null)
            return (null, null, null);

        return (existAccount.AccNo, existAccount.BasName, existAccount.PeoUid);
    }

    public async Task<AccountInfo> GetAccountNoByIdCard(string idCard, DateTime birthDay)
    {
        
        //var existAccount = await (from a in _context.BasePersonEmail
        //                          join f in _context.Basepersons on a.BasId equals f.BasId
        //                          join b in _context.Peoples on a.BasId equals b.BasId
        //                          join c in _context.Accounts on b.PeoUid equals c.PeoUid
        //                          where c.AccStatus == "1" && f.BasIdCard == idCard && f.BasBirthday == birthDay
        //                          select new { c.AccNo, f.BasName, b.PeoUid, a.BseEmail, a.BseOrder }).AsNoTracking().OrderBy(x => x.BseOrder).FirstOrDefaultAsync();


        //if (existAccount == null)
        //    return (null, null, null, "");

        //return (existAccount.AccNo, existAccount.BasName, existAccount.PeoUid, existAccount.BseEmail);

        var existAccount = await (from a in _context.BasePersonEmail
                                  join f in _context.Basepersons on a.BasId equals f.BasId
                                  join b in _context.Peoples on a.BasId equals b.BasId
                                  join c in _context.Accounts on b.PeoUid equals c.PeoUid
                                  where c.AccStatus == 1 && f.BasIdCard == idCard && f.BasBirthday == birthDay
                                  
                                  select new AccountInfo
                                  {
                                      AccNo = c.AccNo,
                                      BasName = f.BasName,
                                      PeoUid = b.PeoUid,
                                      Email = a.BseEmail,
                                      BseOrder = a.BseOrder,
                                  }).AsNoTracking().OrderBy(x => x.BseOrder).FirstOrDefaultAsync();

        return existAccount; // 這裡就可能是 null

    }
    //public async Task<EventObjectDto> GetEventDeptInfo(int uid)
    //{

    //    EventObjectDto eventObjectDto = new EventObjectDto();
    //    var dt = await _context.PersonDepartmentDtos
    //                .FromSqlInterpolated($@"
    //                        SELECT baseperson.bas_name, departments.dep_name, 'profess' as pro_name 
    //                        FROM people
    //                        INNER JOIN baseperson on baseperson.bas_id = people.bas_id
    //                        INNER JOIN departments ON people.dep_no = departments.dep_no
    //                        WHERE people.peo_uid = {uid}").AsNoTracking()
    //                .FirstOrDefaultAsync();

    //    if (dt != null)
    //    {
    //        eventObjectDto.DepName = dt.Dep_Name;
    //        eventObjectDto.PeoName = dt.bas_name;
    //        eventObjectDto.ProName = dt.Pro_Name;
    //    }

    //    return eventObjectDto;
    //}

    public async Task<(bool success, string message)> AddForgetPassWordAsync(ForgetPassWord forgetPassWord
        , Entities.MailMessage mailMessage, IDataLogger dataLogger)
    {

        try
        {
            
            var forgetInfo = await _context.ForgetPassWords.Where(x => x.FpwAccno == forgetPassWord.FpwAccno
                                                                        && x.FpwState == "0").ToListAsync();
            foreach (var item in forgetInfo)
            {
                item.FpwState = "2";
                item.FpwModifyName = forgetPassWord.FpwAccno.ToString();
                item.FpwModifyTime = DateTime.Now;
            }
            await _context.ForgetPassWords.AddAsync(forgetPassWord);
            await _context.MailMessage.AddAsync(mailMessage);


            await SaveChangesAsync(dataLogger);
            return (true, "忘記密碼成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException?.Message);
            return (false, "忘記密碼失敗: " + ex.Message);
        }
    }
    public async Task<ForgetPassWord?> GetForgetPassWordInfoByEmailAsync(string email)
    {
        var forgetInfo = await (from f in _context.ForgetPassWords
                                join a in _context.Accounts on f.FpwAccno equals a.AccNo
                                join p in _context.Peoples on a.PeoUid equals p.PeoUid
                                join b in _context.BasePersonEmail on p.BasId equals b.BasId
                                where a.AccStatus == 1 && b.BseEmail == email && f.FpwState == "0"
                                select f).OrderByDescending(x => x.FpwCreateTime).AsNoTracking().FirstOrDefaultAsync();


        return forgetInfo;
    }
    /// <summary>
    /// 取最後3組的密碼
    /// </summary>
    /// <param name="accno"></param>
    /// <returns></returns>
    public async Task<List<(string, string)>> GetLastPasswordAsync(int accno)
    {
        var lastPasswords = await _context.PasswdChanges
            .AsNoTracking()
            .Where(x => x.PasAccNo == accno)
            .OrderByDescending(x => x.PasDatetime)
            .Take(3)
            .Select(x => new ValueTuple<string, string>(x.PasPasswdHash, x.PasPasswdSalt))
            .ToListAsync();

        return lastPasswords;
    }

    public async Task<bool> existVerifyCode(int accno, string verifyCode)
    {
        var existCode = false;
        var existAccount = await _context.ForgetPassWords.Where(x => x.FpwAccno == accno
                                                                    && x.FpwState == "0").OrderByDescending(y => y.FpwCreateTime).AsNoTracking().FirstOrDefaultAsync();

        if (existAccount != null)
        {
            if (existAccount.FpwCode == verifyCode)
            {
                existCode = true;
            }
        }
        return existCode;

    }

    public async Task<(bool result, string errMeg)> SetNewPassWordAsync(Guid fpw_id, int accno, string accPassword, string acc_passwdHash, string acc_passwdSalt, IDataLogger dataLogger)
    {
        var setInfo = (result: true, errMeg: "OK");
        var account = await _context.Accounts.Where(x => x.AccNo == accno).FirstOrDefaultAsync();
        var forgetInfo = await _context.ForgetPassWords.Where(x => x.FpwId == fpw_id).FirstOrDefaultAsync();
        var basePersonInfo = await (from b in _context.Basepersons
                                    join p in _context.Peoples on b.BasId equals p.BasId
                                    join a in _context.Accounts on p.PeoUid equals a.PeoUid
                                    where a.AccStatus == 1
                                    select new { b.BasId, b.BasName })
                                    .AsNoTracking().OrderByDescending(x => x.BasId).FirstOrDefaultAsync();
        if (account != null && forgetInfo != null)
        {
            try
            {
                account.AccPainText = accPassword;
                account.AccPasswdHash = acc_passwdHash;
                account.AccPasswdSalt = acc_passwdSalt;
                account.AccPwChange = DateTime.Now;
                account.AccModifyTime = DateTime.Now;
                account.AccModifyName = basePersonInfo.BasName;

                forgetInfo.FpwState = "1";
                forgetInfo.FpwModifyTime = DateTime.Now;
                forgetInfo.FpwModifyName = basePersonInfo.BasName;

                PasswdChange passwdChange = new PasswdChange();

                passwdChange.PasPainText = accPassword;
                passwdChange.PasPasswdHash = acc_passwdHash;
                passwdChange.PasPasswdSalt = acc_passwdSalt;
                passwdChange.PasAccNo = accno;
                passwdChange.PasChangeUID = account.PeoUid;
                passwdChange.PasDatetime = DateTime.Now;

                _context.Update(account);
                _context.Update(forgetInfo);
                _context.PasswdChanges.Add(passwdChange);

                await SaveChangesAsync(dataLogger);
            }
            catch (Exception ex)
            {
                setInfo.result = false;
                setInfo.errMeg = string.Format("系統錯誤:{0}", ex.Message);
            }

        }
        else
        {
            setInfo.result = false;
            setInfo.errMeg = "帳號不存在";
        }
        return setInfo;
    }
    public async Task<(bool result, string errMeg)> SetPersonalNewPassWordAsync( int accno, string accPassword, string acc_passwdHash, string acc_passwdSalt, IDataLogger dataLogger)
    {
        var setInfo = (result: true, errMeg: "OK");
        var account = await _context.Accounts.Where(x => x.AccNo == accno).FirstOrDefaultAsync();
        var basePersonInfo = await (from b in _context.Basepersons
                                    join p in _context.Peoples on b.BasId equals p.BasId
                                    join a in _context.Accounts on p.PeoUid equals a.PeoUid
                                    where a.AccStatus == 1
                                    select new { b.BasId, b.BasName })
                                    .AsNoTracking().OrderByDescending(x => x.BasId).FirstOrDefaultAsync();
        if (account != null )
        {
            try
            {
                account.AccPainText = accPassword;
                account.AccPasswdHash = acc_passwdHash;
                account.AccPasswdSalt = acc_passwdSalt;
                account.AccPwChange = DateTime.Now;
                account.AccModifyTime = DateTime.Now;
                account.AccModifyName = basePersonInfo.BasName;


                PasswdChange passwdChange = new PasswdChange();

                passwdChange.PasPainText = accPassword;
                passwdChange.PasPasswdHash = acc_passwdHash;
                passwdChange.PasPasswdSalt = acc_passwdSalt;
                passwdChange.PasAccNo = accno;
                passwdChange.PasChangeUID = account.PeoUid;
                passwdChange.PasDatetime = DateTime.Now;

                _context.Update(account);
               
                _context.PasswdChanges.Add(passwdChange);

                await SaveChangesAsync(dataLogger);
            }
            catch (Exception ex)
            {
                setInfo.result = false;
                setInfo.errMeg = string.Format("系統錯誤:{0}", ex.Message);
            }

        }
        else
        {
            setInfo.result = false;
            setInfo.errMeg = "帳號不存在";
        }
        return setInfo;
    }

    //public async Task InsertLog(Account account)
    //{
    //    var person = _context.Peoples.FirstOrDefault(p => p.PeoUid == account.PeoUid);
    //    if (person != null)
    //    {
    //        //測試寫入異動資料
    //        //person.peo_memo = $"異動帳號：{account.acc_login}，時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    //        //person.peo_addr = $"時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    //        //_dbContext.SaveChanges();
    //    }
    //}
}
