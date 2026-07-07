using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.AdminPortal;

namespace EHRIS.Core.Repositories.AdminPortal;

public interface IADS999004Repository
{
    Task<(IEnumerable<Ads999004ListViewModel> Data, int RecordsFiltered, int RecordsTotal)> GetPagedListAsync(Ads999004DataTableRequest request);
    Task<List<ArgumentsGroup>> GetGroupListAsync();
    Task<Arguments> GetByVariableAsync(string variable);
    Task<bool> VariableExistsAsync(string variable);
    Task AddAsync(Arguments entity, IDataLogger dataLogger);
    Task UpdateAsync(Arguments entity, IDataLogger dataLogger);
    Task<bool> SoftDeleteAsync(string variable, string user, IDataLogger dataLogger);
}
