using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.AdminPortal;

namespace EHRIS.Services.Services.AdminPortal;

public class ADS999002Service : IADS999002Service
{
    private readonly IADS999002Repository _repository;

    public ADS999002Service(IADS999002Repository repository)
    {
        _repository = repository;
    }

    public async Task<List<AdminRoleListViewModel>> GetRoleList()
    {
        return await _repository.GetAllRoleList();
    }

    public async Task<AdminRoles> GetRoleByNoAsync(int adrNo)
    {
        return await _repository.GetRoleByNoAsync(adrNo);
    }

    public async Task<(bool success, string message)> AddRoleAsync(AdminRoles role, IDataLogger dataLogger)
    {
        try
        {
            if (await _repository.RoleNameExistsAsync(role.AdrRoleName))
            {
                return (false, "角色名稱已存在");
            }

            await _repository.AddRoleAsync(role, dataLogger);
            return (true, "新增成功");
        }
        catch (Exception ex)
        {
            return (false, "新增失敗: " + ex.Message);
        }
    }

    public async Task<(bool success, string message)> UpdateRoleAsync(AdminRoles role, IDataLogger dataLogger)
    {
        try
        {
            if (await _repository.RoleNameExistsAsync(role.AdrRoleName, role.AdrNo))
            {
                return (false, "角色名稱已存在");
            }

            await _repository.UpdateRoleAsync(role, dataLogger);
            return (true, "修改成功");
        }
        catch (Exception ex)
        {
            return (false, "修改失敗: " + ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeleteRoleAsync(int adrNo, IDataLogger dataLogger)
    {
        try
        {
            if (await _repository.RoleNameUsedAsync(adrNo))
            {
                return (false, "此角色已被指派權限或有關連帳號，無法刪除");
            }

            await _repository.DeleteRoleAsync(adrNo, dataLogger);
            return (true, "刪除成功");
        }
        catch (Exception ex)
        {
            return (false, "刪除失敗: " + ex.Message);
        }
    }

    public async Task<List<AdminFunctionTreeViewModel>> GetTreeAsync(int adrNo)
    {
        var flatList = await _repository.GetAllFunctionPermissionAsync(adrNo);
        return BuildTree(flatList, 0);
    }

    private List<AdminFunctionTreeViewModel> BuildTree(List<AdminFunctionTreeViewModel> all, int parentId, HashSet<int> visited = null)
    {
        visited ??= new HashSet<int>();

        return all
            .Where(x => x.ParentId == parentId && !visited.Contains(x.FunctionId))
            .Select(x =>
            {
                visited.Add(x.FunctionId);
                return new AdminFunctionTreeViewModel
                {
                    FunctionId = x.FunctionId,
                    FunctionName = x.FunctionName,
                    ParentId = x.ParentId,
                    IsChecked = x.IsChecked,
                    AddStatus = x.AddStatus,
                    EditStatus = x.EditStatus,
                    DelStatus = x.DelStatus,
                    HasCreateAuth = x.HasCreateAuth,
                    HasEditAuth = x.HasEditAuth,
                    HasDeleteAuth = x.HasDeleteAuth,
                    Children = BuildTree(all, x.FunctionId, visited)
                };
            })
            .ToList();
    }

    public async Task<bool> SetRolePermissionAsync(AdminAuthorityViewModel rolePermission, IDataLogger dataLogger)
    {
        return await _repository.SetRolePermissionAsync(rolePermission, dataLogger);
    }

    public async Task<List<AdminUserInRoleViewModel>> GetUsersInRoleAsync(int adrNo)
    {
        return await _repository.GetUsersInRoleAsync(adrNo);
    }
}