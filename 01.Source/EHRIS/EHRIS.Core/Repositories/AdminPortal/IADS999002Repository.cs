using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Core.Repositories.AdminPortal;

public interface IADS999002Repository : IBaseRepository
{
    Task<List<AdminRoleListViewModel>> GetAllRoleList();

    Task<AdminRoles> GetRoleByNoAsync(int adrNo);

    Task<bool> RoleNameExistsAsync(string adrRoleName, int? adrNo = null);

    Task<bool> RoleNameUsedAsync(int adrNo);

    Task<bool> AddRoleAsync(AdminRoles role, IDataLogger dataLogger);

    Task<bool> UpdateRoleAsync(AdminRoles role, IDataLogger dataLogger);

    Task<bool> DeleteRoleAsync(int adrNo, IDataLogger dataLogger);

    Task<List<AdminFunctionTreeViewModel>> GetAllFunctionPermissionAsync(int adrNo);

    Task<bool> SetRolePermissionAsync(AdminAuthorityViewModel roleAuthority, IDataLogger dataLogger);

    Task<List<AdminUserInRoleViewModel>> GetUsersInRoleAsync(int adrNo);
}