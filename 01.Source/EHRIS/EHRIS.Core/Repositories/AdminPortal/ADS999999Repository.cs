using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.AdminPortal;

public class ADS999999Repository : BaseRepository, IADS999999Repository
{
    public ADS999999Repository(ApplicationDbContext context) : base(context) { }

    #region 平台管理
    public async Task<SystemInfo> GetInfoAsync()
    {
        return await _context.SystemInfo.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<bool> HasDataAsync()
    {
        return await _context.SystemInfo.AnyAsync();
    }

    public async Task<bool> UpdateInfoAsync(ADS999999ViewModel model, IDataLogger dataLogger)
    {
        var entity = await _context.SystemInfo.FirstOrDefaultAsync();
        if (entity == null) return false;

        entity.SyiTitle = model.SyiTitle;
        entity.SyiSubTitle = model.SyiSubTitle;
        entity.SyiSmtpServer = model.SyiSmtpServer;
        entity.SyiSmtpPort = model.SyiSmtpPort;
        entity.SyiSmtpSsl = model.SyiSmtpSsl;
        entity.SyiEmailAddr = model.SyiEmailAddr;
        entity.SyiEmailPwd = model.SyiEmailPwd;
        entity.SyiEmailPwdHash = model.SyiEmailPwdHash;
        entity.SyiEmailPwdKeyVersion = model.SyiEmailPwdKeyVersion;
        entity.SyiMultipleMode = model.SyiMultipleMode;
        entity.SyiModifyName = model.ModifyName;
        entity.SyiModifyTime = DateTime.Now;

        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> InitializeAsync(string code, string name, string password, IDataLogger dataLogger)
    {
        var entity = new SystemInfo
        {
            SyiCode = code,
            SyiName = name,
            SyiPwdHash = password,
            SyiPwdSalt = Guid.NewGuid().ToString(),
            SyiTitle = name,
            SyiSubTitle = string.Empty,
            SyiEmailAddr = string.Empty,
            SyiEmailPwd = string.Empty,
            SyiEmailPwdHash = string.Empty,
            SyiCreateName = "System",
            SyiCreateTime = DateTime.Now,
            SyiModifyName = "System",
            SyiModifyTime = DateTime.Now
        };

        await _context.SystemInfo.AddAsync(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }
    #endregion

    #region 權限管理
    public async Task<bool> UpdateLevelDataAsync(string encryptedData, string modifyName, IDataLogger dataLogger)
    {
        var entity = await _context.SystemInfo.FirstOrDefaultAsync();
        if (entity == null) return false;

        entity.SyiData = encryptedData;
        entity.SyiModifyName = modifyName;
        entity.SyiModifyTime = DateTime.Now;

        await SaveChangesAsync(dataLogger);
        return true;
    }
    #endregion
}