using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.AdminPortal;

namespace EHRIS.Services.Services.AdminPortal;

public class ADS999004Service : IADS999004Service
{
    private readonly IADS999004Repository _repository;

    public ADS999004Service(IADS999004Repository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<Ads999004ListViewModel> Data, int RecordsFiltered, int RecordsTotal)>
        GetListAsync(Ads999004DataTableRequest request)
    {
        return await _repository.GetPagedListAsync(request);
    }

    public async Task<List<ArgumentsGroup>> GetGroupListAsync()
    {
        return await _repository.GetGroupListAsync();
    }

    public async Task<Ads999004EditViewModel> GetByVariableAsync(string variable)
    {
        var entity = await _repository.GetByVariableAsync(variable);
        if (entity == null) return null;

        return new Ads999004EditViewModel
        {
            ArgVariable = entity.ArgVariable,
            ArgDescribe = entity.ArgDescribe,
            ArgDeatil = entity.ArgDeatil,
            ArgValue = entity.ArgValue,
            ArgDefaultValue = entity.ArgDefaultValue,
            ArgSource = entity.ArgSource,
            ArgMultiSel = entity.ArgMultiSel,
            ArgSplitChar = entity.ArgSplitChar,
            AgrGroup = entity.AgrGroup,
            ArgOrder = entity.ArgOrder,
            ArgOpenManager = entity.ArgOpenManager,
            ArgRequired = entity.ArgRequired
        };
    }

    public async Task<(bool success, string message)> AddAsync(Ads999004EditViewModel model, string user, IDataLogger dataLogger)
    {
        try
        {
            if (await _repository.VariableExistsAsync(model.ArgVariable)) return (false, "參數變數已存在");

            var entity = MapToEntity(model, user);
            entity.ArgCreateName = user;
            entity.ArgCreateTime = DateTime.Now;

            await _repository.AddAsync(entity, dataLogger);
            return (true, "新增成功");
        }
        catch (Exception ex) { return (false, "新增失敗: " + ex.Message); }
    }

    public async Task<(bool success, string message)> UpdateAsync(Ads999004EditViewModel model, string user, IDataLogger dataLogger)
    {
        try
        {
            var existing = await _repository.GetByVariableAsync(model.ArgVariable);
            if (existing == null) return (false, "找不到資料");

            var entity = MapToEntity(model, user);
            entity.ArgCreateName = existing.ArgCreateName;
            entity.ArgCreateTime = existing.ArgCreateTime;

            await _repository.UpdateAsync(entity, dataLogger);
            return (true, "修改成功");
        }
        catch (Exception ex) { return (false, "修改失敗: " + ex.Message); }
    }

    public async Task<(bool success, string message)> DeleteAsync(string variable, string user, IDataLogger dataLogger)
    {
        try
        {
            var result = await _repository.SoftDeleteAsync(variable, user, dataLogger);
            return result ? (true, "刪除成功") : (false, "找不到資料");
        }
        catch (Exception ex) { return (false, "刪除失敗: " + ex.Message); }
    }

    private Arguments MapToEntity(Ads999004EditViewModel m, string user) => new()
    {
        ArgVariable = m.ArgVariable,
        ArgDescribe = m.ArgDescribe,
        ArgDeatil = m.ArgDeatil,
        ArgValue = m.ArgValue,
        ArgDefaultValue = m.ArgDefaultValue,
        ArgSource = m.ArgSource,
        ArgMultiSel = m.ArgMultiSel,
        ArgSplitChar = m.ArgSplitChar,
        AgrGroup = m.AgrGroup,
        ArgOrder = m.ArgOrder,
        ArgOpenManager = m.ArgOpenManager,
        ArgRequired = m.ArgRequired,
        ArgStatus = 1,
        ArgModifyName = user,
        ArgModifyTime = DateTime.Now
    };
}
