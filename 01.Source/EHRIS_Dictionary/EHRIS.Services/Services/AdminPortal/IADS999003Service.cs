using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Services.AdminPortal;

public interface IADS999003Service
{
    Task<List<SystemLevelDataDto>> GetDecryptedLevelsAsync();
    Task<List<UserLevelMappingDto>> GetDecryptedMappingsAsync();
    Task<List<AdminUserListViewModel>> GetUserListAsync();
    Task<(bool success, string message)> SaveUserAsync(AdminUserSaveViewModel model, IDataLogger logger);
    Task<AdminUserSaveViewModel> GetUserByNoAsync(int aduNo);
    Task<(bool success, string message)> DeleteUserAsync(int aduNo, IDataLogger logger);
    Task<List<SystemLevelDataDto>> GetAvailableLevelsAsync();
    Task<List<AdminRoles>> GetAvailableRolesAsync();
}