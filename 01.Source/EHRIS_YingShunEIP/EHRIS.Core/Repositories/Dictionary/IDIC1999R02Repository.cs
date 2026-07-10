using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using System.Data;

namespace EHRIS.Core.Repositories.Dictionary;

public interface IDIC1999R02Repository
{
    Task<DataTableResponse<DIC1999R02ViewModel>> GetPagedListAsync(DIC1999R02Request request, string serverIp);
    Task<bool> SyncTableFieldsAsync(string dbKey, string serverIp, string tableName, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateFieldsAsync(List<DIC1999R02ViewModel> updates, string serverIp, IDataLogger dataLogger);
    Task<DIC1999R02ViewModel?> GetFieldOriginalDataAsync(int rowId, string serverIp);
    Task<(bool success, string message)> CreatePhysicalColumnAsync(DIC1999R02CreateViewModel model, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> DeletePhysicalColumnAsync(string dbKey, string serverIp, string tableName, string columnName, int rowId, IDataLogger dataLogger);
    Task<(DataTable dataTable, List<string> pkList, List<string> fkList, Dictionary<string, string> colDescDict)> GetTableDetailAsync(string dbKey, string serverIp, string tableName, string keyword);
}