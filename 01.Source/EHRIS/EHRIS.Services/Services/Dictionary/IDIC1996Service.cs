using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Dictionary;

public interface IDIC1996Service
{
    Task<DataTableResponse<DIC1996ViewModel>> GetDataTableAsync(DIC1996Request request);
    Task<DIC1996ViewModel?> GetByIdAsync(int id);
    Task<(bool success, string message)> SaveAsync(DIC1996ViewModel model, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteAsync(int id, IDataLogger dataLogger);
}