using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Dictionary;

public interface IDIC1998Service
{
    Task<DataTableResponse<DIC1998ViewModel>> GetDataTableAsync(DataTableRequest request);
    Task<(bool success, string message)> SaveAsync(DIC1998SaveViewModel model, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteAsync(int accessId, IDataLogger dataLogger);
    Task<List<Menu>> GetAvailableMenusAsync(string serverIp);

    Task<DataTableResponse<DIC1998GroupViewModel>> GetGroupedDataTableAsync(DIC1998SearchModel request);
}