using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.AdminPortal;
using static EHRIS.Core.Models.LoginViewModel;

namespace EHRIS.Services.Services.AdminPortal;

public class ADS999001Service : IADS999001Service
{
    private readonly IADS999001Repository _repository;

    private static readonly List<BadgeOption> BadgeOptions = new List<BadgeOption>
    {
        new BadgeOption { TypeCode = 0, BadgeName = "緊急" },
        new BadgeOption { TypeCode = 1, BadgeName = "重要" },
        new BadgeOption { TypeCode = 2, BadgeName = "一般" }
    };

    public ADS999001Service(IADS999001Repository repository)
    {
        _repository = repository;
    }

    public string GetTypeName(byte typeCode)
    {
        return BadgeOptions.FirstOrDefault(x => x.TypeCode == typeCode)?.BadgeName ?? "一般";
    }

    public async Task<DataTablesResponse<ADS999001ListViewModel>> GetSysNoticePagedListAsync(DataTablesRequest request)
    {
        var result = await _repository.GetSysNoticePagedListAsync(request);
        foreach (var item in result.data)
        {
            item.SysnTypeName = GetTypeName(item.SysnType);
        }
        return result;
    }

    public async Task<ADS999001EditViewModel> GetSysNoticeForEditAsync(int sysnNo)
    {
        if (sysnNo == 0)
        {
            return new ADS999001EditViewModel
            {
                SysnType = 2,
                SysnPublicDt = DateTime.Today,
                SysnStartTime = DateTime.Today,
                SysnEndTime = DateTime.Today.AddDays(30),
                SysnTop = false,
                AllBadges = BadgeOptions
            };
        }

        var entity = await _repository.GetSysNoticeByNoAsync(sysnNo);
        if (entity == null) return null;

        return new ADS999001EditViewModel
        {
            SysnNo = entity.SysnNo,
            SysnType = entity.SysnType,
            SysnContent = entity.SysnContent,
            SysnPublicDt = entity.SysnPublicDt,
            SysnStartTime = entity.SysnStartTime,
            SysnEndTime = entity.SysnEndTime,
            SysnTop = entity.SysnTop,
            AllBadges = BadgeOptions
        };
    }

    public async Task<(bool success, string message)> CreateSysNoticeAsync(SysNoticeUpdateViewModel model, IDataLogger dataLogger, string userName)
    {
        try
        {
            if (model.SysnEndTime < model.SysnStartTime)
                return (false, "結束時間不可早於開始時間");

            bool res = await _repository.CreateSysNoticeAsync(model, userName, dataLogger);
            return res ? (true, "新增成功") : (false, "資料庫存取失敗");
        }
        catch (Exception ex) { return (false, $"發生錯誤：{ex.Message}"); }
    }

    public async Task<(bool success, string message)> UpdateSysNoticeAsync(SysNoticeUpdateViewModel model, IDataLogger dataLogger, string userName)
    {
        try
        {
            if (model.SysnEndTime < model.SysnStartTime)
                return (false, "結束時間不可早於開始時間");

            bool res = await _repository.UpdateSysNoticeAsync(model, userName, dataLogger);
            return res ? (true, "修改成功") : (false, "資料庫存取失敗");
        }
        catch (Exception ex) { return (false, $"發生錯誤：{ex.Message}"); }
    }

    public async Task<(bool success, string message)> SoftDeleteSysNoticeAsync(int sysnNo, IDataLogger dataLogger, string userName)
    {
        try
        {
            bool res = await _repository.SoftDeleteSysNoticeAsync(sysnNo, dataLogger);
            return res ? (true, "刪除成功") : (false, "刪除失敗");
        }
        catch (Exception ex) { return (false, $"發生錯誤：{ex.Message}"); }
    }

    public async Task<List<LoginNoticeViewModel>> GetLoginNoticesAsync(int topN = 10)
    {
        var list = await _repository.GetLoginNoticesAsync(topN);
        foreach (var item in list)
        {
            item.SysnTypeName = GetTypeName(item.SysnType);
            item.SysnTypeCode = item.SysnType switch
            {
                0 => "urgent",
                1 => "important",
                _ => "normal"
            };
        }
        return list;
    }
}