using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories.SysBasic
{
    public interface ISYS202003Repository : IBaseRepository
    {
        Task<DataTablesResponse<SYS202003ListViewModel>> GetPTypePagedListAsync(DataTablesRequest request);
        Task<PType> GetPTypeByIdAsync(int ptyNo);
        Task<List<PersonTypeOption>> GetAllPersonTypeOptionsAsync();
        Task<List<string>> GetSelectedPersonTypesAsync(int ptyNo);
        Task<bool> CodeExistsAsync(string code, int currentId);

        Task CreatePTypeAndAssociationsAsync(PTypeUpdateViewModel model, string userName, IDataLogger dataLogger);
        Task UpdatePTypeAndAssociationsAsync(PTypeUpdateViewModel model, string userName, IDataLogger dataLogger);
        Task SoftDeletePTypeAsync(int ptyNo, IDataLogger dataLogger);
    }
}