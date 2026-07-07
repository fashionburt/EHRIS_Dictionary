using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.AdminPortal;
using EHRIS.Tools.Email;
using EHRIS.Tools.Crypto;
using System.Text.Json;

namespace EHRIS.Services.Services.AdminPortal;

public class ADS999999Service : IADS999999Service
{
    private readonly IADS999999Repository _repository;
    private readonly IEmailSenderService _emailSender;

    public ADS999999Service(IADS999999Repository repository, IEmailSenderService emailSender)
    {
        _repository = repository;
        _emailSender = emailSender;
    }

    #region 平台設定
    public async Task<ADS999999ViewModel> GetSystemInfoAsync()
    {
        var info = await _repository.GetInfoAsync();
        if (info == null) return null;

        return new ADS999999ViewModel
        {
            SyiName = info.SyiName,
            SyiTitle = info.SyiTitle,
            SyiSubTitle = info.SyiSubTitle,
            SyiSmtpServer = info.SyiSmtpServer,
            SyiSmtpPort = info.SyiSmtpPort,
            SyiSmtpSsl = info.SyiSmtpSsl,
            SyiEmailAddr = info.SyiEmailAddr,
            SyiEmailPwdHash = info.SyiEmailPwdHash,
            SyiEmailPwdKeyVersion = info.SyiEmailPwdKeyVersion,
            SyiMultipleMode = info.SyiMultipleMode,
            ModifyName = info.SyiModifyName,
            ModifyTime = info.SyiModifyTime
        };
    }

    public async Task<bool> IsInitializedAsync()
    {
        return await _repository.HasDataAsync();
    }

    public async Task<(bool success, string message)> UpdateSystemInfoAsync(ADS999999ViewModel model, IDataLogger dataLogger)
    {
        try
        {
            var masterKey = Environment.GetEnvironmentVariable("EHRISKey") ?? "";

            if (!string.IsNullOrEmpty(model.SyiEmailPwdPlain))
            {
                model.SyiEmailPwd = _emailSender.EncryptMailPwd(masterKey, model.SyiEmailAddr, model.SyiEmailPwdPlain);

            }

            var result = await _repository.UpdateInfoAsync(model, dataLogger);
            return result ? (true, "系統資訊已安全更新") : (false, "更新失敗");
        }
        catch (Exception ex)
        {
            return (false, $"加密過程出錯: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> InitializeSystemAsync(string code, string name, string password, IDataLogger dataLogger)
    {
        try
        {
            if (await IsInitializedAsync()) return (false, "系統已初始化");

            await _repository.InitializeAsync(code, name, password, dataLogger);
            return (true, "初始化成功");
        }
        catch (Exception ex)
        {
            return (false, $"初始化失敗: {ex.Message}");
        }
    }
    #endregion

    #region 權限設定
    public async Task<LevelDataViewModel> GetLevelDataAsync()
    {
        var info = await _repository.GetInfoAsync();
        var model = new LevelDataViewModel();

        if (info == null || string.IsNullOrEmpty(info.SyiData))
        {
            return model;
        }

        try
        {
            var masterKey = Environment.GetEnvironmentVariable("EHRISKey") ?? "";
            string decryptedJson = AESCrypto.Decrypt(info.SyiData, masterKey);
            var levels = JsonSerializer.Deserialize<List<AdminLevelModel>>(decryptedJson);

            model.Levels = levels?.Where(x => x.Status != 2).OrderBy(x => x.No).ToList() ?? new();
        }
        catch
        {
            model.Levels = new();
        }

        return model;
    }

    public async Task<(bool success, string message)> UpdateLevelDataAsync(LevelDataViewModel model, IDataLogger dataLogger)
    {
        try
        {
            var masterKey = Environment.GetEnvironmentVariable("EHRISKey") ?? "";
            
            string jsonString = JsonSerializer.Serialize(model.Levels);
            string encryptedData = AESCrypto.Encrypt(jsonString, masterKey);

            var result = await _repository.UpdateLevelDataAsync(encryptedData, model.ModifyName, dataLogger);
            return result ? (true, "權限層級已安全加密儲存") : (false, "儲存失敗");
        }
        catch (Exception ex)
        {
            return (false, $"加密處理失敗: {ex.Message}");
        }
    }
    #endregion
}