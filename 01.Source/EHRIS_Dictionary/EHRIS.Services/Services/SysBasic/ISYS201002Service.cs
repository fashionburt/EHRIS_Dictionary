using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services;

public interface ISYS201002Service
{
    Task<List<RoleListViewModel>> GetRoleList();
    Task<Role> GetRoleByNoAsync(int rol_no);
    Task<(bool success, string message)> AddRoleAsync(Role role, string userName, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateRoleAsync(Role role, string userName, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteRoleAsync(int rol_no, string userName, IDataLogger dataLogger);
    Task<List<FunctionTreeModel>> GetTreeAsync(int rolNo);
    Task<bool> SetRolePermissionAsync(AuthorityViewModel rolePermission, string userName, IDataLogger dataLogger);
    Task<List<PeopleListModel>> GetPeoplePermissionAsync(int rolNo);

}
