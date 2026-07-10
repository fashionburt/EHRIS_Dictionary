using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Core.Repositories.AdminPortal;

public interface IADS999999Repository : IBaseRepository
{
    #region 平台管理
    Task<SystemInfo> GetInfoAsync();
    Task<bool> HasDataAsync();
    Task<bool> UpdateInfoAsync(ADS999999ViewModel model, IDataLogger dataLogger);
    Task<bool> InitializeAsync(string code, string name, string password, IDataLogger dataLogger);
    #endregion

    #region 權限管理
    Task<bool> UpdateLevelDataAsync(string encryptedData, string modifyName, IDataLogger dataLogger);
    #endregion
}