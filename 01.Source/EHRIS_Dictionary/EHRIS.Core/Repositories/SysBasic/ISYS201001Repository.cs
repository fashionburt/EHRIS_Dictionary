using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories.SysBasic
{
    public interface ISYS201001Repository : IBaseRepository
    {
        Task<List<AdminListViewModel>> GetAllAdminsAsync(AdminDataTableRequest request);
        Task<AdminDetailsViewModel> GetAdminDetailsAsync(int accNo);
        Task<int> GetTotalAdminCountAsync();
        Task<int> GetFilteredAdminCountAsync(AdminDataTableRequest request);
        Task<Account> GetAccountByIdAsync(int id);
        Task<bool> DoesAccountExistAsync(string loginAccount, int excludeAccNo = 0);
        Task<List<(string Hash, string Salt)>> GetRecentPasswordHistoryAsync(int accNo, int count);
        Task<int> AddAdminAsync(AdminUpdateModel model, string hashedPassword, string salt, string modifierName, DateTime processTime, IDataLogger dataLogger);
        Task<int> UpdateAdminAsync(AdminUpdateModel model, string hashedPassword, string salt, string modifierName, DateTime processTime, IDataLogger dataLogger);
        Task<int> SoftDeleteAdminAsync(int accNo, string modifierName, DateTime processTime, IDataLogger dataLogger);
        Task<List<Departments>> GetUnitListAsync();
        Task<List<Role>> GetRoleListAsync();
        Task<List<PType>> GetActivePersonnelTypesAsync();
        Task<List<Profess>> GetProfessListAsync();
        Task<HashSet<int>> GetStaticLockPeoUidsAsync();
    }
}