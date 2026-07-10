using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Services.AdminPortal;

public interface IADS999002Service
{
    Task<List<AdminRoleListViewModel>> GetRoleList();

    Task<AdminRoles> GetRoleByNoAsync(int adrNo);

    Task<(bool success, string message)> AddRoleAsync(AdminRoles role, IDataLogger dataLogger);

    Task<(bool success, string message)> UpdateRoleAsync(AdminRoles role, IDataLogger dataLogger);

    Task<(bool success, string message)> DeleteRoleAsync(int adrNo, IDataLogger dataLogger);

    Task<List<AdminFunctionTreeViewModel>> GetTreeAsync(int adrNo);

    Task<bool> SetRolePermissionAsync(AdminAuthorityViewModel rolePermission, IDataLogger dataLogger);

    Task<List<AdminUserInRoleViewModel>> GetUsersInRoleAsync(int adrNo);
}