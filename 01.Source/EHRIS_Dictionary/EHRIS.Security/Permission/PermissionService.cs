using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;
using Microsoft.AspNetCore.Http;

namespace EHRIS.Security.Permission;

public class PermissionService : IPermissionService

{
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public PermissionService(IMenuRepository menuRepository, IPermissionsRepository permissionsRepository, IHttpContextAccessor httpContextAccessor)
    {
        _menuRepository = menuRepository;
        _permissionsRepository = permissionsRepository;
        _httpContextAccessor = httpContextAccessor; 
    }
    
    public bool HasPermission(int sfuNo, string action)
    {
        var claimKey = $"FUNCTION:{sfuNo}:{action}";
        return _httpContextAccessor.HttpContext?.User.HasClaim(claimKey, "true") ?? false;
    }

    public async Task<List<UserFuncPermissionViewModel>> GetPermissionFunction(int acc_no)
    {
        return await _permissionsRepository.GetFunctionPermissionByAccount(acc_no);
    }
      
    public async Task<List<UserFuncPermissionViewModel>> GetAdminPermissionFunction(int acc_no)
    {
        return await _permissionsRepository.GetFunctionAdminPermissionByAccount(acc_no);
    }

    public async Task<List<UserPtypePermissionViewModel>> GetPtypePermissionByPersonType(int acc_no, List<string> personTypes)
    {
        return await _permissionsRepository.GetPtypePermissionByPersonType(acc_no, personTypes);
    }

    public async Task<List<UserPtypePermissionViewModel>> GetPtypePermission(int acc_no)
    {
        return await _permissionsRepository.GetPtypePermissionByAccount(acc_no);
    }
     
    public async Task<List<int>> GetDepNoListByAuth(int acc_no)
    {
        return await _permissionsRepository.GetDepNoPermissionByAccount(acc_no);
    }
     
    public Task<List<int>> GetPtyNoListByAuth(int acc_no)
    {
        throw new NotImplementedException();
    }
      
    public Task<bool> isSuperMan(int acc_no)
    {
        throw new NotImplementedException();
    }

    public Task<bool> isSuperManByDep(int acc_no)
    {
        throw new NotImplementedException();
    }

    public Task<bool> isSuperManByPty(int acc_no)
    {
        throw new NotImplementedException();
    }

   
}
