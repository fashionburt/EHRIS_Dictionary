using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.AdminPortal
{
    public class ADS999003Repository : BaseRepository, IADS999003Repository
    {
        public ADS999003Repository(ApplicationDbContext context) : base(context) { }

        public async Task<List<AdminUserListViewModel>> GetUserListAsync()
        {
            string sql = @"
                SELECT 
                    u.adu_no AS AduNo, 
                    u.adu_login AS AduLogin, 
                    u.adu_displayname AS AduDisplayName,
                    u.adu_email AS AduEmail,
                    '' AS AdlName, 
                    u.adu_status AS AduStatus, 
                    u.adu_modifytime AS AduModifyTime,
                ISNULL(u.adu_modifyname, u.adu_createname) AS ModifyName,
                CONVERT(VARCHAR, ISNULL(u.adu_modifytime, u.adu_createtime), 120) AS ModifyTimeText,
                ISNULL(STUFF((
                SELECT ', ' + r.adr_rolename 
                FROM admin_userroles ur 
                JOIN admin_roles r ON ur.adr_no = r.adr_no 
                WHERE ur.adu_no = u.adu_no 
                FOR XML PATH('')), 1, 2, ''), '') AS RoleNames
                FROM admin_users u
                WHERE u.adu_status != 2
                ORDER BY u.adu_no DESC";

            return await _context.Database.SqlQueryRaw<AdminUserListViewModel>(sql).ToListAsync();
        }

        public async Task<string> GetSystemLevelDataAsync() =>
            await _context.Database.SqlQueryRaw<string>(
                "SELECT syi_data AS Value FROM systeminfo WHERE syi_code = '12008'"
            ).FirstOrDefaultAsync() ?? "";

        public async Task<string> GetUserLevelMappingAsync() =>
            await _context.Database.SqlQueryRaw<string>(
                "SELECT syi_dataHash AS Value FROM systeminfo WHERE syi_code = '12008'"
            ).FirstOrDefaultAsync() ?? "";

        public async Task<bool> AddUserAsync(AdminUsers user, List<int> roleIds, string cipherMapping, IDataLogger logger)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var trans = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.AdminUsers.AddAsync(user);
                    await _context.SaveChangesAsync();

                    if (roleIds?.Any() == true)
                        await _context.AdminUserRoles.AddRangeAsync(roleIds.Select(rid => new AdminUserRoles { AduNo = user.AduNo, AdrNo = rid }));

                    await _context.Database.ExecuteSqlRawAsync("UPDATE systeminfo SET syi_dataHash = @p0 WHERE syi_code = '12008'", cipherMapping);

                    await SaveChangesAsync(logger);
                    await trans.CommitAsync();
                    return true;
                }
                catch { await trans.RollbackAsync(); throw; }
            });
        }

        public async Task<bool> UpdateUserAsync(AdminUsers user, List<int> roleIds, bool updatePwd, string cipherMapping, IDataLogger logger)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var trans = await _context.Database.BeginTransactionAsync();
                try
                {
                    var exist = await _context.AdminUsers.FindAsync(user.AduNo);
                    if (exist == null) return false;

                    exist.AduDisplayName = user.AduDisplayName;
                    exist.AduEmail = user.AduEmail;
                    exist.AduStatus = user.AduStatus;
                    exist.AduModifyName = user.AduModifyName;
                    exist.AduModifyTime = user.AduModifyTime;

                    if (updatePwd) { exist.AduPasswdHash = user.AduPasswdHash; exist.AduPasswdSalt = user.AduPasswdSalt; }

                    _context.AdminUserRoles.RemoveRange(_context.AdminUserRoles.Where(ur => ur.AduNo == user.AduNo));
                    if (roleIds?.Any() == true)
                        await _context.AdminUserRoles.AddRangeAsync(roleIds.Select(rid => new AdminUserRoles { AduNo = user.AduNo, AdrNo = rid }));

                    await _context.Database.ExecuteSqlRawAsync("UPDATE systeminfo SET syi_dataHash = @p0 WHERE syi_code = '12008'", cipherMapping);

                    await SaveChangesAsync(logger);
                    await trans.CommitAsync();
                    return true;
                }
                catch { await trans.RollbackAsync(); throw; }
            });
        }

        public async Task<AdminUsers> GetUserByNoAsync(int aduNo) => await _context.AdminUsers.AsNoTracking().FirstOrDefaultAsync(u => u.AduNo == aduNo);
        public async Task<List<int>> GetUserRoleIdsAsync(int aduNo) => await _context.AdminUserRoles.Where(ur => ur.AduNo == aduNo).Select(ur => ur.AdrNo).ToListAsync();
        public async Task<bool> LoginExistsAsync(string login, int? exclude) => await _context.AdminUsers.AnyAsync(u => u.AduLogin == login && (exclude == null || u.AduNo != exclude));
        public async Task<bool> DeleteUserAsync(int aduNo, IDataLogger logger)
        {
            var u = await _context.AdminUsers.FindAsync(aduNo);
            if (u == null) return false;

            u.AduStatus = 2;
            u.AduModifyTime = DateTime.Now;

            return await SaveChangesAsync(logger) > 0;
        }
        public async Task<List<AdminRoles>> GetActiveRolesAsync() => await _context.AdminRoles.Where(r => r.AdrStatus == 1).OrderBy(r => r.AdrRoleName).AsNoTracking().ToListAsync();
    }
}