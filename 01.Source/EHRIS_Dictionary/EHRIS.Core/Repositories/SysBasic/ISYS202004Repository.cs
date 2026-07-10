using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic; 

namespace EHRIS.Core.Repositories.SysBasic
{
    public interface ISYS202004Repository : IBaseRepository
    {
        Task<DataTablesResponse<SYS202004ListViewModel>> GetProfessPagedListAsync(DataTablesRequest request);
        Task<Profess> GetProfessByIdAsync(int proNo);
        Task<List<PersonTypeOption>> GetAllPersonTypeOptionsAsync();
        Task<List<string>> GetSelectedPersonTypesAsync(int proNo);
        Task<bool> CodeExistsAsync(string code, int currentId);
        Task CreateProfessAndAssociationsAsync(SYS202004UpdateViewModel model, string userName, IDataLogger dataLogger);
        Task UpdateProfessAndAssociationsAsync(SYS202004UpdateViewModel model, string userName, IDataLogger dataLogger);
        Task SoftDeleteProfessAsync(int proNo, IDataLogger dataLogger);
    }
}