using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;


namespace EHRIS.Core.Repositories;

public class SYS201002Repository : BaseRepository, ISYS201002Repository
{
   
    public SYS201002Repository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 抓所有角色
    /// </summary>
    /// <returns>角色列表</returns>
    public async Task<List<RoleListViewModel>> GetAllRoleList()
    {
        var roleList = await _context.Roles
            .Select(r => new RoleListViewModel
            {
                rol_no = r.RolNo,
                rol_name = r.RolName,
                rol_memo = r.RolMemo,
                rol_modifyname = r.RolModifyName,
                rol_modifytime = r.RolModifyTime,
                rol_open = r.RolOpen
            }).AsNoTracking().ToListAsync();
        if (roleList == null)
        {
            roleList = new List<RoleListViewModel>();
        }
        return roleList;

    }
    /// <summary>
    /// 依role.rol_no抓取單一筆角色資訊
    /// </summary>
    /// <param name="rol_no"></param>
    /// <returns>單一筆角色資訊</returns>
    public async Task<Role> GetRoleByNoAsync(int rol_no)
    {
        return await _context.Roles
            .AsNoTracking() 
            .FirstOrDefaultAsync(r => r.RolNo == rol_no);
    }
    /// <summary>
    /// 依role.rol_no或角色名稱搜尋是否資料存在
    /// </summary>
    /// <param name="rol_name">角色名稱</param>
    /// <param name="rol_no"></param>
    /// <returns>True:存在   False:不存在</returns>
    public async Task<bool> RoleNameExistsAsync(string rol_name, int? rol_no = null)
    {
        return await _context.Roles
            .AsNoTracking().AnyAsync(r => r.RolName == rol_name && (rol_no == null || r.RolNo != rol_no));
    }
    /// <summary>
    /// 依role.rol_no找角色是否被設定
    /// </summary>
    /// <param name="rol_no"></param>
    /// <returns>True:已被使用  False:未被使用</returns>
    public async Task<bool> RoleNameUsedAsync(int rol_no )
    {
        var blnRAu= await _context.RAuthoritys
            .AsNoTracking().AnyAsync(r => r.RolNo == rol_no);
        var blnRAc = await _context.RoleAccounts
            .AsNoTracking().AnyAsync(r => r.RolNo == rol_no);
        //任一table有關連rol_no都不能刪
        return blnRAu || blnRAc;
    }

    public async Task<bool> AddRoleAsync(Role role, IDataLogger dataLogger)
    {
        await _context.Roles.AddAsync(role);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> UpdateRoleAsync(Role role, IDataLogger dataLogger)
    {
        var existing = await _context.Roles.FindAsync(role.RolNo);
        if (existing == null) return false;

        existing.RolName = role.RolName;
        existing.RolMemo = role.RolMemo;
        existing.RolOpen = role.RolOpen;
        existing.RolModifyName = role.RolModifyName;
        existing.RolModifyTime = role.RolModifyTime;

        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> DeleteRoleAsync(int rol_no, IDataLogger dataLogger)
    {
        var role = await _context.Roles.FindAsync(rol_no);
        if (role == null) return false;

        _context.Roles.Remove(role);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<List<FunctionTreeModel>> GetAllFunctionPermissionAsync(int rolNo)
    {
        string sql = @"
            select s.sys_no as functionId, s.sys_name as functionName, 0 as ParentId
                , cast(0 as bit) as rauCheck , cast(0 as tinyint) as AddStatus, cast(0 as tinyint) as EditStatus, cast(0 as tinyint) as DelStatus
                , cast(0 as bit) as SfuIns , cast(0 as bit) as SfuEdi, cast(0 as bit) as SfuDel
                from sys s 
                where s.sys_status='1'
            union
            select sf.sfu_no as functionId,sf.sfu_name as functionName
                , case when sf.sfu_parent=0 then s.sys_no else sf.sfu_parent end as ParentId
                , cast(case when ra.rau_no is not null then 1 else 0 end as bit) as rauCheck
                , cast(isnull(ra.rau_Ins,0) as tinyint) as AddStatus
                , cast(isnull(ra.rau_Edi,0) as tinyint) as EditStatus, cast(isnull(ra.rau_Del,0) as tinyint) as DelStatus
                , cast(isnull(sf.sfu_Ins , 0) as bit) as SfuIns
                , cast(isnull(sf.sfu_Edi , 0) as bit) as SfuEdi
                , cast(isnull(sf.sfu_Del , 0) as bit) as SfuDel
                from sysfuction sf
                inner join sys s on s.sys_no = sf.sys_no and s.sys_status='1'
                left join rauthority ra on ra.sfu_no = sf.sfu_no   and ra.rol_no = @rolNo
                left join role r on ra.rol_no = r.rol_no and r.rol_open=1
                where sf.sfu_status='1' ";

        var rolNoParam = new SqlParameter("@rolNo", rolNo);

        return await _context.Database.SqlQueryRaw<FunctionTreeModel>(sql, rolNoParam).ToListAsync();
    }
    /// <summary>
    /// 角色管理-權限設定
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public async Task<bool> SetRolePermissionAsync(AuthorityViewModel roleAuthority, IDataLogger dataLogger)
    {
        var isVaild = true;
        var currentRolNo = 0;
        try
        {
            if (roleAuthority != null && roleAuthority.RolePermissions != null)
            {
                currentRolNo = roleAuthority.RolNo;
                foreach (RolePermissionViewModel rolePermission in roleAuthority.RolePermissions)
                {
                    if (rolePermission.ParentSfuNo == 0)
                    {
                        continue;
                    }

                    if ("func".Equals(rolePermission.FuncType, StringComparison.OrdinalIgnoreCase))
                    {
                        var parentFunc = await _context.RAuthoritys
                               .Where(x => x.RolNo == currentRolNo && x.SfuNo == rolePermission.SfuNo)
                               .OrderBy(x => x.RauNo)
                               .FirstOrDefaultAsync();

                        if (parentFunc != null)
                        {
                            if (!rolePermission.isChecked)
                            {
                                _context.RAuthoritys.Remove(parentFunc);

                                List<int> sameLevelFun = await (from ra in _context.RAuthoritys
                                                                join sf in _context.SysFuction on ra.SfuNo equals sf.SfuNo
                                                                where ra.RolNo == currentRolNo
                                                                && sf.SfuParent == rolePermission.ParentSfuNo
                                                                && ra.SfuNo != rolePermission.SfuNo
                                                                select sf.SfuParent)
                                                  .Distinct()
                                                  .ToListAsync();

                                if (sameLevelFun is null || sameLevelFun.Count == 0)
                                {
                                    if (rolePermission.ParentSfuNo != 0)
                                    {
                                        var delPFunc = await _context.RAuthoritys
                                                  .Where(x => x.RolNo == currentRolNo && x.SfuNo == rolePermission.ParentSfuNo)
                                                  .OrderBy(x => x.RauNo)
                                                  .FirstOrDefaultAsync();
                                        if (delPFunc != null)
                                            _context.RAuthoritys.Remove(delPFunc);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (rolePermission.isChecked)
                            {
                                var existsInSfu = await _context.SysFuction.AnyAsync(x => x.SfuNo == rolePermission.SfuNo);
                                if (existsInSfu)
                                {
                                    var newParentFunc = new RAuthority
                                    {
                                        SfuNo = rolePermission.SfuNo,
                                        RolNo = currentRolNo,
                                        RauIns = 0,
                                        RauEdi = 0,
                                        RauDel = 0,
                                        RauCreateName = rolePermission.ModifyName,
                                        RauCreateTime = rolePermission.ModifyTime,
                                        RauModifyName = rolePermission.ModifyName,
                                        RauModifyTime = rolePermission.ModifyTime
                                    };
                                    await _context.RAuthoritys.AddAsync(newParentFunc);
                                }
                            }
                        }
                    }
                    else if ("sub".Equals(rolePermission.FuncType, StringComparison.OrdinalIgnoreCase))
                    {
                        var childFunc = await _context.RAuthoritys
                                .Where(x => x.RolNo == currentRolNo && x.SfuNo == rolePermission.SfuNo)
                                .OrderBy(x => x.RauNo)
                                .FirstOrDefaultAsync();

                        if (childFunc != null)
                        {
                            if (rolePermission.isChecked)
                            {
                                childFunc.RauIns = Convert.ToByte(rolePermission.RauIns);
                                childFunc.RauEdi = Convert.ToByte(rolePermission.RauEdi);
                                childFunc.RauDel = Convert.ToByte(rolePermission.RauDel);
                                childFunc.RauModifyName = rolePermission.ModifyName;
                                childFunc.RauModifyTime = rolePermission.ModifyTime;
                            }
                            else
                            {
                                _context.RAuthoritys.Remove(childFunc);
                            }
                        }
                        else
                        {
                            if (rolePermission.isChecked)
                            {
                                var existsInSfu = await _context.SysFuction.AnyAsync(x => x.SfuNo == rolePermission.SfuNo);
                                if (existsInSfu)
                                {
                                    var newChildFunc = new RAuthority
                                    {
                                        SfuNo = rolePermission.SfuNo,
                                        RolNo = currentRolNo,
                                        RauIns = Convert.ToByte(rolePermission.RauIns),
                                        RauEdi = Convert.ToByte(rolePermission.RauEdi),
                                        RauDel = Convert.ToByte(rolePermission.RauDel),
                                        RauCreateName = rolePermission.ModifyName,
                                        RauCreateTime = rolePermission.ModifyTime,
                                        RauModifyName = rolePermission.ModifyName,
                                        RauModifyTime = rolePermission.ModifyTime
                                    };
                                    await _context.RAuthoritys.AddAsync(newChildFunc);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                isVaild = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}:{1} {2}", DateTime.Now, ex.Message, ex.InnerException);
            isVaild = false;
        }
        await SaveChangesAsync(dataLogger);

        return isVaild;
    }

    public async Task<List<PeopleListModel>> GetPeoplePermissionAsync(int rolNo)
    {
        string sql = @"
            select ra.acc_no as AccNo, p.peo_uid as PeoUid, b.bas_name as BasName
                , isnull(pf.pro_name,'') as ProName, isnull(dpt.dep_name,'') as DeptName
                from roleaccount ra
                INNER join accounts a on ra.acc_no = a.acc_no and a.acc_status='1'
                INNER join people p on a.peo_uid = p.peo_uid
                INNER join baseperson b on p.bas_id = b.bas_id 
                LEFT join departments dpt on dpt.dep_no = p.dep_no  and dpt.dep_status='1'
                LEFT join profess pf on p.pro_no = pf.pro_no and pf.pro_status='1'
                where ra.rol_no = @rolNo ";

        var rolNoParam = new SqlParameter("@rolNo", rolNo);

        return await _context.Database.SqlQueryRaw<PeopleListModel>(sql, rolNoParam).ToListAsync();
    }
}
