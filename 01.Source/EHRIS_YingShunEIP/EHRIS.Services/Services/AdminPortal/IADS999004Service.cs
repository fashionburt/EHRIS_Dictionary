using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Services.AdminPortal;

public interface IADS999004Service
{
    Task<(IEnumerable<Ads999004ListViewModel> Data, int RecordsFiltered, int RecordsTotal)> GetListAsync(Ads999004DataTableRequest request);
    Task<List<ArgumentsGroup>> GetGroupListAsync();
    Task<Ads999004EditViewModel> GetByVariableAsync(string variable);
    Task<(bool success, string message)> AddAsync(Ads999004EditViewModel model, string user, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateAsync(Ads999004EditViewModel model, string user, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteAsync(string variable, string user, IDataLogger dataLogger);
}
