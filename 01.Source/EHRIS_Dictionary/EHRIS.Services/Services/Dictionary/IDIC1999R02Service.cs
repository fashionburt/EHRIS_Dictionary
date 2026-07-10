using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using System.Data;

namespace EHRIS.Services.Dictionary;

public interface IDIC1999R02Service
{
    Task<DataTableResponse<DIC1999R02ViewModel>> GetDataTableAsync(DIC1999R02Request request, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateFieldsAsync(string dbKey, string serverIp, string tableName, List<DIC1999R02ViewModel> updates, IDataLogger dataLogger);
    Task<(bool success, string message)> CreateColumnAsync(DIC1999R02CreateViewModel model, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteColumnAsync(string dbKey, string serverIp, string tableName, string columnName, int rowId, IDataLogger dataLogger);
    Task<(DataTable dataTable, List<string> pkList, List<string> fkList, Dictionary<string, string> colDescDict)> GetTableDetailAsync(string dbKey, string serverIp, string tableName, string keyword);
}