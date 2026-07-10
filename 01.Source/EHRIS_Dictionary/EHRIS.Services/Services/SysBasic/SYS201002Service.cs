using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories;
using EHRIS.Services.Common;

namespace EHRIS.Services.Services;

public class SYS201002Service : ISYS201002Service
{
    private readonly ISYS201002Repository _sYS201002Repository;
    private readonly ICommonService _commonService;

    public SYS201002Service(ISYS201002Repository sYS201002Repository, ICommonService commonService)
    {
        _sYS201002Repository = sYS201002Repository;
        _commonService = commonService;
    }

    public async Task<List<RoleListViewModel>> GetRoleList()
    {

        return await _sYS201002Repository.GetAllRoleList();

    }
    public async Task<Role> GetRoleByNoAsync(int rol_no)
    {
        return await _sYS201002Repository.GetRoleByNoAsync(rol_no);
    }
    public async Task<(bool success, string message)> AddRoleAsync(Role role, string user, IDataLogger dataLogger)
    {
        try
        {
            if (await _sYS201002Repository.RoleNameExistsAsync(role.RolName))
                return (false, "角色名稱已存在");

            await _sYS201002Repository.AddRoleAsync(role, dataLogger);
            return (true, "新增成功");
        }
        catch (Exception ex)
        {
            return (false, "新增失敗: " + ex.Message);
        }
    }
    public async Task<(bool success, string message)> UpdateRoleAsync(Role role, string user, IDataLogger dataLogger)
    {
        try
        {
            if (await _sYS201002Repository.RoleNameExistsAsync(role.RolName, role.RolNo))
                return (false, "角色名稱已存在");

            await _sYS201002Repository.UpdateRoleAsync(role, dataLogger);
            return (true, "修改成功");
        }
        catch (Exception ex)
        {
            return (false, "修改失敗: " + ex.Message);
        }
    }


    public async Task<(bool success, string message)> DeleteRoleAsync(int rol_no, string userName, IDataLogger dataLogger)
    {
        try
        {
            if (await _sYS201002Repository.RoleNameUsedAsync(rol_no))
                return (false, "角色已有權限設定，無法刪除");

            await _sYS201002Repository.DeleteRoleAsync(rol_no, dataLogger);
            return (true, "刪除成功");
        }
        catch (Exception ex)
        {
            return (false, "刪除失敗: " + ex.Message);
        }
    }

    public async Task<List<FunctionTreeModel>> GetTreeAsync(int rolNo)
    {
        var flatList = await _sYS201002Repository.GetAllFunctionPermissionAsync(rolNo);
        var funTree = BuildTree(flatList, 0);
        return funTree;
    }

    private List<FunctionTreeModel> BuildTree(List<FunctionTreeModel> all, int parentId, HashSet<int> visited = null)
    {
        visited ??= new HashSet<int>();

        // 修正前：直接對 all 裡的原始物件呼叫 x.Children = ...（就地修改），
        //         同一個物件實例可能同時存在於不同節點的 Children 中，
        //         造成 JSON 序列化器跟著物件參照無限走下去（即使沒有真正的 C# 循環），
        //         最終超過 MaxDepth 並拋出 JsonException。
        //
        // 修正後：visited 過濾移到 Where（節點出現過就直接排除，不再進入 Select），
        //         並建立全新的 FunctionTreeModel 物件而非重用 all 裡的實例，
        //         確保每個樹節點都是獨立實例，序列化時不會出現共用參照或循環。
        return all
            .Where(x => x.ParentId == parentId && !visited.Contains(x.functionId))
            .Select(x =>
            {
                visited.Add(x.functionId);
                return new FunctionTreeModel
                {
                    functionId  = x.functionId,
                    ParentId    = x.ParentId,
                    functionName = x.functionName,
                    rauCheck    = x.rauCheck,
                    AddStatus   = x.AddStatus,
                    EditStatus  = x.EditStatus,
                    DelStatus   = x.DelStatus,
                    SfuIns      = x.SfuIns,
                    SfuEdi      = x.SfuEdi,
                    SfuDel      = x.SfuDel,
                    Children    = BuildTree(all, x.functionId, visited)
                };
            })
            .ToList();
    }
    public async Task<bool> SetRolePermissionAsync(AuthorityViewModel rolePermission, string userName, IDataLogger dataLogger)
    {
        return await _sYS201002Repository.SetRolePermissionAsync(rolePermission, dataLogger);
    }
    public async Task<List<PeopleListModel>> GetPeoplePermissionAsync(int rolNo)
    {
        return await _sYS201002Repository.GetPeoplePermissionAsync(rolNo);
    }
}
