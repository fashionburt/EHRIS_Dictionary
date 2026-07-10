using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Core.Repositories.Dictionary;

public interface IDIC1997Repository
{
    Task<DataTableResponse<DIC1997ViewModel>> GetPagedListAsync(DIC1997Request request, string serverIp);
    Task<List<string>> GetDbKeysAsync(string serverIp);
    Task<List<string>> GetTableNamesAsync(string serverIp, string dbKey, string? pkName = null);
    Task<List<string>> GetPkNamesAsync(string serverIp, string dbKey, string? tableName = null);
    Task<(bool success, string message)> UpdateFieldsAsync(string serverIp, string dbKey, string tableName, List<DIC1997UpdateModel> updates, IDataLogger dataLogger);
}