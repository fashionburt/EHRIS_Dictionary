using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Core.Repositories.Dictionary;

public interface IDIC1999R01Repository
{
    Task<DataTableResponse<DIC1999R01ViewModel>> GetPagedListAsync(DIC1999R01Request request, string serverIp);
    Task<bool> SyncTablesAsync(string dbKey, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateDescriptionAsync(DIC1999R01ViewModel model, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteTableAsync(string dbKey, string serverIp, string tableName, int sheetId, IDataLogger dataLogger);
    Task<(bool success, string message)> RenameTableAsync(string dbKey, string serverIp, string oldName, string newName, int sheetId, IDataLogger dataLogger);
    Task<string?> GetSheetDescAsync(int sheetId, string serverIp);
    Task<(bool success, string message)> CreatePhysicalTableAsync(DIC1999R01CreateViewModel model, string serverIp, IDataLogger dataLogger);
    Task<(int tableCount, int columnCount)> ImportDictionaryAsync(string dbKey, string serverIp, Dictionary<string, Dictionary<string, string>> data, string detail, IDataLogger dataLogger);
}