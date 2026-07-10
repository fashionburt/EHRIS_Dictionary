using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using EHRIS.Tools.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EHRIS.Core.Repositories.AdminPortal;

public class ADS999002Repository : BaseRepository, IADS999002Repository
{
    public ADS999002Repository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<AdminRoleListViewModel>> GetAllRoleList()
    {
        var roleList = await _context.AdminRoles
            .Where(r => r.AdrStatus == 1)
            .Select(r => new AdminRoleListViewModel
            {
                AdrNo = r.AdrNo,
                AdrRoleName = r.AdrRoleName,
                AdrRoleMemo = r.AdrRoleMemo,
                AdrStatus = r.AdrStatus,
                ModifyTime = DateTime.Now
            }).AsNoTracking().ToListAsync();

        return roleList ?? new List<AdminRoleListViewModel>();
    }

    public async Task<AdminRoles> GetRoleByNoAsync(int adrNo)
    {
        return await _context.AdminRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.AdrNo == adrNo);
    }

    public async Task<bool> RoleNameExistsAsync(string adrRoleName, int? adrNo = null)
    {
        return await _context.AdminRoles
            .AsNoTracking()
            .AnyAsync(r => r.AdrRoleName == adrRoleName && (adrNo == null || r.AdrNo != adrNo));
    }

    public async Task<bool> RoleNameUsedAsync(int adrNo)
    {
        var hasFunctions = await _context.AdminRoleFunctions.AsNoTracking().AnyAsync(r => r.AdrNo == adrNo);
        var hasUsers = await _context.AdminUserRoles.AsNoTracking().AnyAsync(r => r.AdrNo == adrNo);
        return hasFunctions || hasUsers;
    }

    public async Task<bool> AddRoleAsync(AdminRoles role, IDataLogger dataLogger)
    {
        await _context.AdminRoles.AddAsync(role);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> UpdateRoleAsync(AdminRoles role, IDataLogger dataLogger)
    {
        var existing = await _context.AdminRoles.FindAsync(role.AdrNo);
        if (existing == null) return false;

        existing.AdrRoleName = role.AdrRoleName;
        existing.AdrRoleMemo = role.AdrRoleMemo;
        existing.AdrStatus = role.AdrStatus;

        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> DeleteRoleAsync(int adrNo, IDataLogger dataLogger)
    {
        var role = await _context.AdminRoles.FindAsync(adrNo);
        if (role == null) return false;

        role.AdrStatus = 2;
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<List<AdminFunctionTreeViewModel>> GetAllFunctionPermissionAsync(int adrNo)
    {
        StringBuilder sbSQL = new StringBuilder();
        sbSQL.Append(@$"
            SELECT s.ads_no AS functionId, s.ads_name AS functionName, 0 AS parentId
                , 0 AS rauCheck , 0 AS rau_Ins, 0 AS rau_Edi, 0 AS rau_Del
                , 0 AS SfuIns , 0 AS SfuEdi, 0 AS SfuDel
                FROM admin_sys s 
                WHERE s.ads_status = 1
            UNION
            SELECT f.adf_no AS functionId, f.adf_name AS functionName
                , CASE WHEN f.adf_parent = 0 THEN s.ads_no ELSE f.adf_parent END AS parentId
                , CASE WHEN rf.arf_no IS NOT NULL THEN 1 ELSE 0 END AS rauCheck
                , ISNULL(rf.arf_cancreate, 0) AS rau_Ins
                , ISNULL(rf.arf_canedit, 0) AS rau_Edi
                , ISNULL(rf.arf_candelete, 0) AS rau_Del
                , f.adf_Ins AS SfuIns, f.adf_Edi AS SfuEdi, f.adf_Del AS SfuDel
                FROM admin_functions f
                INNER JOIN admin_sys s ON s.ads_no = f.ads_no AND s.ads_status = 1
                LEFT JOIN admin_rolefunctions rf ON rf.adf_no = f.adf_no AND rf.adr_no = {adrNo}
                WHERE f.adf_status = 1");

        var sqlObj = new SqlQueryObject { Sql = sbSQL.ToString() };
        var rawData = await SQLQueryRawAsync(sqlObj);

        return rawData.Select(row => new AdminFunctionTreeViewModel
        {
            FunctionId = Convert.ToInt32(row[0]),
            FunctionName = row[1]?.ToString() ?? string.Empty,
            ParentId = Convert.ToInt32(row[2]),
            IsChecked = Convert.ToBoolean(row[3]),
            HasCreateAuth = Convert.ToBoolean(row[4]),
            HasEditAuth = Convert.ToBoolean(row[5]),
            HasDeleteAuth = Convert.ToBoolean(row[6]),
            AddStatus = Convert.ToByte(row[7]),
            EditStatus = Convert.ToByte(row[8]),
            DelStatus = Convert.ToByte(row[9])
        }).ToList();
    }


    public async Task<bool> SetRolePermissionAsync(AdminAuthorityViewModel roleAuthority, IDataLogger dataLogger)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentAdrNo = roleAuthority.AdrNo;
                var permissions = roleAuthority.RolePermissions;

                var oldPermissions = await _context.AdminRoleFunctions
                    .Where(x => x.AdrNo == currentAdrNo)
                    .ToListAsync();

                _context.AdminRoleFunctions.RemoveRange(oldPermissions);

                foreach (var item in permissions.Where(x => x.IsChecked && x.FuncType == "func"))
                {
                    var newPermission = new AdminRoleFunctions
                    {
                        AdrNo = currentAdrNo,
                        AdfNo = item.AdfNo,
                        ArfCanCreate = item.CanCreate,
                        ArfCanEdit = item.CanEdit,
                        ArfCanDelete = item.CanDelete
                    };
                    await _context.AdminRoleFunctions.AddAsync(newPermission);
                }

                await SaveChangesAsync(dataLogger);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        });
    }

    public async Task<List<AdminUserInRoleViewModel>> GetUsersInRoleAsync(int adrNo)
    {
        var query = from aur in _context.AdminUserRoles
                    join adu in _context.AdminUsers on aur.AduNo equals adu.AduNo
                    where aur.AdrNo == adrNo
                    select new AdminUserInRoleViewModel
                    {
                        AduNo = adu.AduNo,
                        AduLogin = adu.AduLogin,
                        AduDisplayName = adu.AduDisplayName,
                        AduStatus = adu.AduStatus
                    };

        return await query.AsNoTracking().ToListAsync();
    }
}