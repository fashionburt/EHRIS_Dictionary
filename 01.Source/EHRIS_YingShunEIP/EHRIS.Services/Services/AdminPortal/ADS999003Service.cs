using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.AdminPortal;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Tools.ChangePassword;
using EHRIS.Tools.Crypto;
using System.Text;
using System.Text.Json;

namespace EHRIS.Services.Services.AdminPortal;

public class ADS999003Service : IADS999003Service
{
    private readonly IADS999003Repository _repository;
    private readonly IUserContextService _userContext;
    private readonly ICommonService _commonService;

    public ADS999003Service(IADS999003Repository repository, IUserContextService userContext, ICommonService commonService)
    {
        _repository = repository;
        _userContext = userContext;
        _commonService = commonService;
    }


    #region 加解密
    private string GetMasterKey()
    {
        var key = Environment.GetEnvironmentVariable("EHRISKey");
        if (string.IsNullOrEmpty(key))
        {
            key = _commonService.MasterKey;
        }
        return key;
    }

    public async Task<List<SystemLevelDataDto>> GetDecryptedLevelsAsync()
    {
        var cipherText = await _repository.GetSystemLevelDataAsync();
        if (string.IsNullOrEmpty(cipherText)) return new();

        try
        {
            var json = AESCrypto.Decrypt(cipherText, GetMasterKey());
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var levels = JsonSerializer.Deserialize<List<SystemLevelDataDto>>(json, options);

            return levels?.Where(x => x.Status != 2).ToList() ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<UserLevelMappingDto>> GetDecryptedMappingsAsync()
    {
        var cipherText = await _repository.GetUserLevelMappingAsync();
        if (string.IsNullOrEmpty(cipherText)) return new();

        try
        {
            string json = AESCrypto.Decrypt(cipherText, GetMasterKey());
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<UserLevelMappingDto>>(json, options) ?? new();
        }
        catch
        {
            return new();
        }
    }
    #endregion



    public async Task<List<AdminUserListViewModel>> GetUserListAsync()
    {
        var users = await _repository.GetUserListAsync();
        var levels = await GetDecryptedLevelsAsync();
        var mappings = await GetDecryptedMappingsAsync();

        int currentRank = _userContext.AdlRank;

        return users.Select(u =>
        {
            var adlNo = mappings.FirstOrDefault(m => m.AduNo == u.AduNo)?.AdlNo ?? 0;
            var level = levels.FirstOrDefault(l => l.AdlNo == adlNo);
            u.AdlName = level?.AdlName ?? "無位階";
            return new { User = u, Rank = level?.AdlRank ?? 0 };
        })
        .Where(x => x.Rank <= currentRank)
        .Select(x => x.User)
        .ToList();
    }

    public async Task<(bool success, string message)> SaveUserAsync(AdminUserSaveViewModel model, IDataLogger logger)
    {
        try
        {
            if (await _repository.LoginExistsAsync(model.AduLogin, model.AduNo > 0 ? model.AduNo : null))
                return (false, "此帳號已被使用");

            bool isNew = model.AduNo == 0;
            bool updatePwd = !string.IsNullOrWhiteSpace(model.AduPassword);

            var user = new AdminUsers
            {
                AduNo = model.AduNo,
                AduLogin = model.AduLogin,
                AduDisplayName = model.AduDisplayName,
                AduEmail = model.AduEmail,
                AduStatus = model.AduStatus,
                AduModifyName = _userContext.UserName,
                AduModifyTime = DateTime.Now
            };

            if (updatePwd)
            {
                var valResult = PasswordValidator.ValidateStringRules(model.AduPassword, model.AduLogin);
                if (!valResult.IsValid) return (false, string.Join("\n", valResult.Errors));

                byte[] hKey = Encoding.UTF8.GetBytes(_commonService.MasterKey);
                var crypto = PasswordHasher.HashPassword(model.AduPassword, hKey);
                user.AduPasswdHash = crypto.Hash;
                user.AduPasswdSalt = crypto.Salt;
            }
            else if (isNew)
            {
                return (false, "新增帳號必須輸入密碼");
            }

            var mappings = await GetDecryptedMappingsAsync();
            mappings.RemoveAll(m => m.AduNo == model.AduNo);
            if (model.AduStatus != 2)
            {
                mappings.Add(new UserLevelMappingDto { AduNo = model.AduNo, AdlNo = model.AdlNo });
            }
            string jsonMapping = JsonSerializer.Serialize(mappings);
            string cipherMapping = AESCrypto.Encrypt(jsonMapping, GetMasterKey());

            bool res = isNew
                ? await _repository.AddUserAsync(user, model.RoleIds, cipherMapping, logger)
                : await _repository.UpdateUserAsync(user, model.RoleIds, updatePwd, cipherMapping, logger);

            return res ? (true, "儲存完成") : (false, "資料庫存取失敗");
        }
        catch (Exception ex) { return (false, $"發生錯誤：{ex.Message}"); }
    }

    public async Task<AdminUserSaveViewModel> GetUserByNoAsync(int aduNo)
    {
        var u = await _repository.GetUserByNoAsync(aduNo);
        if (u == null) return null;

        var mappings = await GetDecryptedMappingsAsync();
        var adlNo = mappings.FirstOrDefault(m => m.AduNo == aduNo)?.AdlNo ?? 0;

        return new AdminUserSaveViewModel
        {
            AduNo = u.AduNo,
            AduLogin = u.AduLogin,
            AduDisplayName = u.AduDisplayName,
            AduEmail = u.AduEmail,
            AdlNo = adlNo,
            AduStatus = u.AduStatus,
            RoleIds = await _repository.GetUserRoleIdsAsync(aduNo)
        };
    }

    public async Task<(bool success, string message)> DeleteUserAsync(int aduNo, IDataLogger logger)
    {
        return aduNo == _userContext.AduNo ? (false, "不可刪除本人") : (await _repository.DeleteUserAsync(aduNo, logger) ? (true, "刪除成功") : (false, "刪除失敗"));
    }

    public async Task<List<SystemLevelDataDto>> GetAvailableLevelsAsync()
    {
        var levels = await GetDecryptedLevelsAsync();

        return levels
            .Where(l => l.AdlRank <= _userContext.AdlRank)
            .OrderBy(l => l.AdlNo)
            .ToList();
    }

    public async Task<List<AdminRoles>> GetAvailableRolesAsync()
    {
        var roles = await _repository.GetActiveRolesAsync();
        if (_userContext.AdlRank < 99)
        {
            roles = roles.Where(r => r.AdrNo != 1 && r.AdrNo != 12).ToList();
        }
        return roles;
    }


}