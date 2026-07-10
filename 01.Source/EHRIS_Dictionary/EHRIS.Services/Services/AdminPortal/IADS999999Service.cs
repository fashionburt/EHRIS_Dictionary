using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Services.AdminPortal;

public interface IADS999999Service
{
    #region 平台設定
    Task<ADS999999ViewModel> GetSystemInfoAsync();

    Task<bool> IsInitializedAsync();

    Task<(bool success, string message)> UpdateSystemInfoAsync(ADS999999ViewModel model, IDataLogger dataLogger);

    Task<(bool success, string message)> InitializeSystemAsync(string code, string name, string password, IDataLogger dataLogger);
    #endregion

    #region 權限設定
    Task<LevelDataViewModel> GetLevelDataAsync();
    Task<(bool success, string message)> UpdateLevelDataAsync(LevelDataViewModel model, IDataLogger dataLogger);
    #endregion
}