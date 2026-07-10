using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.Dictionary;
using System.Data;

namespace EHRIS.Services.Dictionary;

public class DIC1999R02Service : IDIC1999R02Service
{
    private readonly IDIC1999R02Repository _repository;

    public DIC1999R02Service(IDIC1999R02Repository repository)
    {
        _repository = repository;
    }

    public async Task<DataTableResponse<DIC1999R02ViewModel>> GetDataTableAsync(DIC1999R02Request request, string serverIp, IDataLogger dataLogger)
    {
        await _repository.SyncTableFieldsAsync(request.DbKey, serverIp, request.TableName, dataLogger);
        return await _repository.GetPagedListAsync(request, serverIp);
    }

    public async Task<(bool success, string message)> UpdateFieldsAsync(string dbKey, string serverIp, string tableName, List<DIC1999R02ViewModel> updates, IDataLogger dataLogger)
    {
        var validUpdates = new List<DIC1999R02ViewModel>();

        foreach (var item in updates)
        {
            var original = await _repository.GetFieldOriginalDataAsync(item.RowId, serverIp);
            if (original == null) continue;

            bool isDescChanged = (original.RowDesc ?? "").Trim() != (item.RowDesc ?? "").Trim();
            bool isRemarkChanged = (original.RowRemark ?? "").Trim() != (item.RowRemark ?? "").Trim();

            if (isDescChanged || isRemarkChanged)
            {
                validUpdates.Add(item);
            }
        }

        if (!validUpdates.Any()) return (false, "資料未變動");

        return await _repository.UpdateFieldsAsync(validUpdates, serverIp, dataLogger);
    }

    public async Task<(bool success, string message)> CreateColumnAsync(DIC1999R02CreateViewModel model, string serverIp, IDataLogger dataLogger)
    {
        return await _repository.CreatePhysicalColumnAsync(model, serverIp, dataLogger);
    }

    public async Task<(bool success, string message)> DeleteColumnAsync(string dbKey, string serverIp, string tableName, string columnName, int rowId, IDataLogger dataLogger)
    {
        return await _repository.DeletePhysicalColumnAsync(dbKey, serverIp, tableName, columnName, rowId, dataLogger);
    }

    public async Task<(DataTable dataTable, List<string> pkList, List<string> fkList, Dictionary<string, string> colDescDict)> GetTableDetailAsync(string dbKey, string serverIp, string tableName, string keyword)
    {
        return await _repository.GetTableDetailAsync(dbKey, serverIp, tableName, keyword);
    }
}