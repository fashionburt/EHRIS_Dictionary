using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.Dictionary;

namespace EHRIS.Services.Dictionary;

public class DIC1999R01Service : IDIC1999R01Service
{
    private readonly IDIC1999R01Repository _repository;

    public DIC1999R01Service(IDIC1999R01Repository repository)
    {
        _repository = repository;
    }

    public async Task<DataTableResponse<DIC1999R01ViewModel>> GetDataTableAsync(DIC1999R01Request request, string serverIp, IDataLogger dataLogger)
    {
        if (!string.IsNullOrEmpty(request.DbKey))
        {
            await _repository.SyncTablesAsync(request.DbKey, serverIp, dataLogger);
        }
        return await _repository.GetPagedListAsync(request, serverIp);
    }

    public async Task<(bool success, string message)> UpdateDescriptionsAsync(string dbKey, string serverIp, List<DIC1999R01ViewModel> updates, IDataLogger dataLogger)
    {
        int count = 0;

        foreach (var item in updates)
        {
            var oldDesc = await _repository.GetSheetDescAsync(item.SheetId ?? 0, serverIp) ?? "";
            var newDesc = item.SheetDesc ?? "";

            if (oldDesc != newDesc)
            {
                var result = await _repository.UpdateDescriptionAsync(item, serverIp, dataLogger);
                if (result.success) count++;
            }
        }

        return count > 0 ? (true, $"成功更新 {count} 筆資料表描述") : (false, "沒有偵測到變更");
    }

    public async Task<(bool success, string message)> DeleteTableAsync(string dbKey, string serverIp, string tableName, int sheetId, IDataLogger dataLogger)
    {
        if (string.IsNullOrEmpty(dbKey) || string.IsNullOrEmpty(serverIp) || sheetId <= 0) return (false, "參數不完整");
        return await _repository.DeleteTableAsync(dbKey, serverIp, tableName, sheetId, dataLogger);
    }

    public async Task<(bool success, string message)> RenameTableAsync(string dbKey, string serverIp, int sheetId, string oldName, string newName, IDataLogger dataLogger)
    {
        if (string.IsNullOrWhiteSpace(newName) || oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "新名稱無效或與舊名稱相同");
        }
        return await _repository.RenameTableAsync(dbKey, serverIp, oldName, newName, sheetId, dataLogger);
    }

    public async Task<(bool success, string message)> CreateTableAsync(DIC1999R01CreateViewModel model, string serverIp, IDataLogger dataLogger)
    {
        if (string.IsNullOrEmpty(model.DbKey)) return (false, "未指定資料庫");
        return await _repository.CreatePhysicalTableAsync(model, serverIp, dataLogger);
    }
}