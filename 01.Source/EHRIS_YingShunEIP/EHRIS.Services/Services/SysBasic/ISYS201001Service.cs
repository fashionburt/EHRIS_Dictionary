using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS201001Service
    {
        Task<AdminDataTableResponse<AdminListViewModel>> GetAllAdminsForDataTableAsync(AdminDataTableRequest request);
        Task<AdminDetailsViewModel> GetAdminDetailsAsync(int accNo);
        Task<(bool success, string message)> AddAdminAsync(AdminUpdateModel model, string userName, IDataLogger dataLogger);
        Task<(bool success, string message)> UpdateAdminAsync(AdminUpdateModel model, string userName, IDataLogger dataLogger);
        Task<(bool success, string message)> SoftDeleteAdminAsync(int accNo, IDataLogger dataLogger, string userName);
        Task<List<Departments>> GetUnitListAsync();
        Task<List<Role>> GetRoleListAsync();
        Task<List<PType>> GetActivePersonnelTypesAsync();
        Task<List<Profess>> GetProfessListAsync();
    }
}