using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.Dictionary;
using EHRIS.Tools.Extensions;

namespace EHRIS.Services.Dictionary;

public class DIC1996Service : IDIC1996Service
{
    private readonly IDIC1996Repository _repository;

    public DIC1996Service(IDIC1996Repository repository)
    {
        _repository = repository;
    }

    public async Task<DataTableResponse<DIC1996ViewModel>> GetDataTableAsync(DIC1996Request request)
    {
        var res = await _repository.GetPagedListAsync(request);
        foreach (var item in res.data)
        {
            item.PriorityText = item.Priority switch
            {
                0 => "一般公告",
                1 => "維護公告",
                2 => "緊急公告",
                _ => "未知"
            };
            item.StartDate_Text = item.StartDate.ToRocDateTime("yyyy/MM/dd HH:mm");

            item.EndDate_Text = item.EndDate.HasValue
                            ? item.EndDate.Value.ToRocDateTime("yyyy/MM/dd HH:mm")
                            : "長期公告";
        }
        return res;
    }

    public async Task<DIC1996ViewModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        return new DIC1996ViewModel
        {
            Id = entity.Id,
            Message = entity.Message,
            IsEnabled = entity.IsEnabled,
            Priority = entity.Priority,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            StartDate_Text = entity.StartDate.ToString("yyyy-MM-ddTHH:mm"),
            EndDate_Text = entity.EndDate?.ToString("yyyy-MM-ddTHH:mm") ?? ""
        };
    }

    public async Task<(bool success, string message)> SaveAsync(DIC1996ViewModel model, IDataLogger dataLogger)
    {
        try
        {
            if (model.Id == 0)
            {
                var entity = new MarqueeAnnouncement
                {
                    Message = model.Message ?? "",
                    IsEnabled = model.IsEnabled,
                    Priority = model.Priority,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                };
                var detail = $"新增公告：「{entity.Message}」";
                var result = await _repository.AddAsync(entity, detail, dataLogger);
                return (result, result ? "儲存成功" : "儲存失敗");
            }
            else
            {
                var entity = await _repository.GetByIdAsync(model.Id);
                if (entity == null) return (false, "找不到資料");

                entity.Message = model.Message ?? "";
                entity.IsEnabled = model.IsEnabled;
                entity.Priority = model.Priority;
                entity.StartDate = model.StartDate;
                entity.EndDate = model.EndDate;

                var detail = $"修改公告：「{entity.Message}」";
                var result = await _repository.UpdateAsync(entity, detail, dataLogger);
                return (result, result ? "儲存成功" : "儲存失敗");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id, IDataLogger dataLogger)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return (false, "資料不存在");

            var detail = $"刪除公告：「{entity.Message}」";
            var result = await _repository.SoftDeleteAsync(id, detail, dataLogger);
            return (result, result ? "刪除成功" : "刪除失敗");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    public async Task<List<DIC1996ViewModel>> GetActiveAnnouncementsAsync(int take = 5)
    {
        var list = await _repository.GetActiveAnnouncementsAsync(take);
        var now = DateTime.Now;

        return list.Select(x => new DIC1996ViewModel
        {
            Id = x.Id,
            Message = x.Message,
            IsEnabled = x.IsEnabled,
            Priority = x.Priority,
            PriorityText = x.Priority switch
            {
                0 => "一般公告",
                1 => "維護公告",
                2 => "緊急公告",
                _ => "未知"
            },
            StartDate = x.StartDate,
            StartDate_Text = x.StartDate.ToString("yyyy-MM-dd"),
            IsNew = x.StartDate >= now.AddDays(-5)
        }).ToList();
    }
}