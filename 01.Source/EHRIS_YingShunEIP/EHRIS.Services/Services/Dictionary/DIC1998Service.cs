using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.Dictionary;

namespace EHRIS.Services.Dictionary;

public class DIC1998Service : IDIC1998Service
{
    private readonly IDIC1998Repository _repository;

    public DIC1998Service(IDIC1998Repository repository)
    {
        _repository = repository;
    }

    public async Task<DataTableResponse<DIC1998ViewModel>> GetDataTableAsync(DataTableRequest request)
    {
        return await _repository.GetPagedListAsync(request);
    }

    public async Task<DataTableResponse<DIC1998GroupViewModel>> GetGroupedDataTableAsync(DIC1998SearchModel request)
    {
        var allGroupedData = await _repository.GetGroupedListAsync(request.QueryClientIp, request.QueryServerIp, request.QueryMenuId);

        var totalCount = allGroupedData.Count;

        var pagedData = allGroupedData
            .Skip(request.start)
            .Take(request.length)
            .ToList();

        return new DataTableResponse<DIC1998GroupViewModel>
        {
            draw = request.draw,
            recordsTotal = totalCount,
            recordsFiltered = totalCount,
            data = pagedData
        };
    }

    public async Task<(bool success, string message)> SaveAsync(DIC1998SaveViewModel model, IDataLogger dataLogger)
    {
        if (model.AccessId > 0)
        {
            var existing = await _repository.GetByIdAsync(model.AccessId);
            if (existing == null) return (false, "找不到該筆授權資料");

            existing.ClientIp = model.ClientIp;
            existing.MenuId = model.MenuId;

            var detail = $"【{existing.ClientIp}】授權內容被「編輯」";
            var updateResult = await _repository.UpdateAsync(existing, detail, dataLogger);
            return updateResult ? (true, "編輯成功") : (false, "編輯失敗");
        }

        var isDuplicate = await _repository.AnyAsync(a => a.ClientIp == model.ClientIp && a.MenuId == model.MenuId && a.IsEnabled != 2);
        if (isDuplicate) return (false, "該 IP 已擁有此選單的授權");

        var deletedRecord = await _repository.GetDeletedRecordAsync(model.ClientIp, model.MenuId);
        if (deletedRecord != null)
        {
            deletedRecord.IsEnabled = 1;
            var reactivateDetail = $"【{deletedRecord.ClientIp}】授權被「重新啟用」";
            var reactivateResult = await _repository.UpdateAsync(deletedRecord, reactivateDetail, dataLogger);
            return reactivateResult ? (true, "已重新啟用授權") : (false, "重新啟用失敗");
        }

        var entity = new Menu_Access
        {
            ClientIp = model.ClientIp,
            MenuId = model.MenuId,
            IsEnabled = 1,
            CreateDate = DateTime.Now
        };

        var addDetail = $"【{entity.ClientIp}】被「新增」授權";
        var addResult = await _repository.AddAsync(entity, addDetail, dataLogger);
        return addResult ? (true, "新增授權成功") : (false, "新增失敗");
    }

    public async Task<(bool success, string message)> DeleteAsync(int accessId, IDataLogger dataLogger)
    {
        var entity = await _repository.GetByIdAsync(accessId);
        if (entity == null) return (false, "找不到該筆資料");

        var detail = $"【{entity.ClientIp}】授權被「刪除」";
        var result = await _repository.DeleteByIdAsync(accessId, detail, dataLogger);
        return result ? (true, "授權已移除") : (false, "移除失敗");
    }

    public async Task<List<Menu>> GetAvailableMenusAsync(string serverIp)
    {
        return await _repository.GetAvailableMenusAsync(serverIp);
    }
}