using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Core.Repositories.AdminPortal
{
    public interface IADS999003Repository : IBaseRepository
    {
        Task<List<AdminUserListViewModel>> GetUserListAsync();
        Task<string> GetSystemLevelDataAsync();
        Task<string> GetUserLevelMappingAsync();
        Task<bool> AddUserAsync(AdminUsers user, List<int> roleIds, string cipherMapping, IDataLogger logger);
        Task<bool> UpdateUserAsync(AdminUsers user, List<int> roleIds, bool updatePwd, string cipherMapping, IDataLogger logger);

        Task<AdminUsers> GetUserByNoAsync(int aduNo);
        Task<List<int>> GetUserRoleIdsAsync(int aduNo);
        Task<bool> LoginExistsAsync(string login, int? excludeNo);
        Task<bool> DeleteUserAsync(int aduNo, IDataLogger logger);
        Task<List<AdminRoles>> GetActiveRolesAsync();
    }
}