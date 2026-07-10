using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories;

public interface ISYS201002Repository : IBaseRepository
{
    /// <summary>
    /// 抓所有角色
    /// </summary>
    /// <returns>角色列表</returns>
    Task<List<RoleListViewModel>> GetAllRoleList();
    /// <summary>
    /// 依role.rol_no抓取單一筆角色資訊
    /// </summary>
    /// <param name="rol_no"></param>
    /// <returns>單一筆角色資訊</returns>
    Task<Role> GetRoleByNoAsync(int rol_no);
    /// <summary>
    /// 依role.rol_no或角色名稱搜尋是否資料存在
    /// </summary>
    /// <param name="rol_name">角色名稱</param>
    /// <param name="rol_no"></param>
    /// <returns>True:存在   False:不存在</returns>
    Task<bool> RoleNameExistsAsync(string rol_name, int? rol_no = null);
    /// <summary>
    /// 依role.rol_no找角色是否被設定
    /// </summary>
    /// <param name="rol_no"></param>
    /// <returns>True:已被使用  False:未被使用</returns>
    Task<bool> RoleNameUsedAsync(int rol_no);
    Task<bool> AddRoleAsync(Role role, IDataLogger dataLogger);
    Task<bool> UpdateRoleAsync(Role role, IDataLogger dataLogger);
    Task<bool> DeleteRoleAsync(int rol_no, IDataLogger dataLogger);

    Task<List<FunctionTreeModel>> GetAllFunctionPermissionAsync(int rolNo);
    Task<bool> SetRolePermissionAsync(AuthorityViewModel rolePermission, IDataLogger dataLogger);
    Task<List<PeopleListModel>> GetPeoplePermissionAsync(int rolNo);
}
