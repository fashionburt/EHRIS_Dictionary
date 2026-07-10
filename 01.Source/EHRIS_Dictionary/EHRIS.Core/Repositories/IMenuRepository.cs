using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Repositories;

public interface IMenuRepository
{
    Task<List<MenuSysViewModel>> GetMenusByAccount(int accNo);

    Task<List<MenuSysViewModel>> GetAdminMenusByAccount(int aduNo);
    /// <summary>
    /// 取得功能名稱資料路徑
    /// </summary>
    /// <param name="sfuNo"></param>
    /// <returns></returns>
    Task<(string SysName, string SfuMainName, string SfuName)> GetFunctionNamesAsync(int sfuNo);

}
