using EHRIS.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public interface IPermissionsRepository
    {
        Task<List<UserFuncPermissionViewModel>> GetFunctionPermissionByAccount(int acc_no);

        Task<List<UserFuncPermissionViewModel>> GetFunctionAdminPermissionByAccount(int adu_no);

        Task<List<UserPtypePermissionViewModel>> GetPtypePermissionByAccount(int acc_no);

        Task<List<UserPtypePermissionViewModel>> GetPtypePermissionByPersonType(int acc_no, List<string> personTypes);

        Task<bool> isSuperManByDep(int acc_no);

        Task<List<int>> GetDepNoPermissionByAccount(int acc_no);
         
    }
}
