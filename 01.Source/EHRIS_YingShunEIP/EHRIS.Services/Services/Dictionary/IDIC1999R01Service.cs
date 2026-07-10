using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Dictionary;

public interface IDIC1999R01Service
{
    Task<DataTableResponse<DIC1999R01ViewModel>> GetDataTableAsync(DIC1999R01Request request, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateDescriptionsAsync(string dbKey, string serverIp, List<DIC1999R01ViewModel> updates, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteTableAsync(string dbKey, string serverIp, string tableName, int sheetId, IDataLogger dataLogger);
    Task<(bool success, string message)> RenameTableAsync(string dbKey, string serverIp, int sheetId, string oldName, string newName, IDataLogger dataLogger);
    Task<(bool success, string message)> CreateTableAsync(DIC1999R01CreateViewModel model, string serverIp, IDataLogger dataLogger);
}