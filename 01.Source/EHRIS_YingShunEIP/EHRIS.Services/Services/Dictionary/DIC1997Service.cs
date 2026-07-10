using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.Dictionary;
using EHRIS.Tools.Extensions;

namespace EHRIS.Services.Dictionary;

public class DIC1997Service : IDIC1997Service
{
    private readonly IDIC1997Repository _repository;

    public DIC1997Service(IDIC1997Repository repository)
    {
        _repository = repository;
    }

    public async Task<DataTableResponse<DIC1997ViewModel>> GetDataTableAsync(DIC1997Request request, string serverIp)
    {
        var res = await _repository.GetPagedListAsync(request, serverIp);
        foreach (var item in res.data)
        {
            item.StateText = item.State switch
            {
                "10" => "新增",
                "11" => "申請",
                "20" => "更新",
                "21" => "密碼變更",
                "30" => "刪除",
                "40" => "查詢",
                "50" => "啟用",
                "51" => "關閉",
                "60" => "操作訊息",
                "70" => "登入",
                "71" => "登出",
                _ => "未知"
            };
            item.DateText = item.Date.ToRocDateTime();
        }
        return res;
    }

    public async Task<List<string>> GetDbKeysAsync(string serverIp)
        => await _repository.GetDbKeysAsync(serverIp);

    public async Task<List<string>> GetTableNamesAsync(string serverIp, string dbKey, string? pkName = null)
        => await _repository.GetTableNamesAsync(serverIp, dbKey, pkName);

    public async Task<List<string>> GetPkNamesAsync(string serverIp, string dbKey, string? tableName = null)
        => await _repository.GetPkNamesAsync(serverIp, dbKey, tableName);

    public async Task<(bool success, string message)> UpdateFieldsAsync(string serverIp, string dbKey, string tableName, List<DIC1997UpdateModel> updates, IDataLogger dataLogger)
    {
        return await _repository.UpdateFieldsAsync(serverIp, dbKey, tableName, updates, dataLogger);
    }
}