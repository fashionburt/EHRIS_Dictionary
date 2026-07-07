using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.SysBasic
{
    public class SYS202002Repository : BaseRepository, ISYS202002Repository
    {
        public SYS202002Repository(ApplicationDbContext context, AuditDbContext aduitContext) : base(context)
        {

        }

        public async Task<IEnumerable<SYS202002TreeViewModel>> GetAllFunctionsForTreeAsync()
        {
            var sql = @"
                SELECT 
                    sf.sfu_no       AS SfuNo,
                CASE 
                WHEN sf.sfu_parent = 0 OR sf.sfu_parent IS NULL THEN NULL 
                ELSE sf.sfu_parent 
                END             AS SfuParent,
                ISNULL(s.sys_name, '') AS SysName,
                    sf.sfu_name     AS SfuName,
                    sf.sfu_status   AS SfuStatus,
                CAST(sf.sfu_Ins AS TINYINT) AS SfuIns,
                CAST(sf.sfu_Edi AS TINYINT) AS SfuEdi,
                CAST(sf.sfu_Del AS TINYINT) AS SfuDel,
                ''              AS EditAction,   
                ''              AS DeleteAction  
                FROM 
                    sysfuction sf
                LEFT JOIN 
                    sys s ON sf.sys_no = s.sys_no
                WHERE
                    sf.sfu_status != 2
                ORDER BY
                SysName ASC,      
                SfuParent ASC,     
                sf.sfu_order ASC,   
                SfuNo ASC";

            return await _context.Database
                .SqlQueryRaw<SYS202002TreeViewModel>(sql)
                .ToListAsync();
        }


        public async Task<bool> FunctionExistsAsync(int sfuNo)
        {
            var sql = @"
                SELECT CAST(
                    CASE WHEN EXISTS (
                        SELECT 1 FROM sysfuction 
                        WHERE sfu_no = @SfuNo) 
                    THEN 1 ELSE 0 END 
                    AS BIT) AS Value";
            var sfuNoParam = new SqlParameter("@SfuNo", sfuNo);

            return await _context.Database.SqlQueryRaw<bool>(sql, sfuNoParam).SingleAsync();
        }

        public async Task<IEnumerable<int>> GetChildFunctionIdsAsync(int parentSfuNo)
        {
            var sql = @"
                WITH FunctionHierarchy AS
                (
                    SELECT sfu_no FROM sysfuction WHERE sfu_parent = @ParentSfuNo
                    UNION ALL
                    SELECT f.sfu_no FROM sysfuction f
                    INNER JOIN FunctionHierarchy h ON f.sfu_parent = h.sfu_no
                )
                SELECT sfu_no FROM FunctionHierarchy;";

            var parentSfuNoParam = new SqlParameter("@ParentSfuNo", parentSfuNo);
            return await _context.Database.SqlQueryRaw<int>(sql, parentSfuNoParam).ToListAsync();
        }

        public async Task<SYS202002CreateViewModel> GetFunctionByIdAsync(int sfuNo)
        {
            var sql = @"
            SELECT 
                sfu_no          AS SfuNo,
                sys_no          AS SysNo,
                sfu_name        AS SfuName,
                sfu_catalog     AS SfuCatalog,
                sfu_order       AS SfuOrder,
                sfu_path        AS SfuPath,
                sfu_parent      AS SfuParent,
                sfu_status      AS SfuStatus,
                sfu_Ins         AS SfuIns, 
                sfu_Edi         AS SfuEdi,
                sfu_Del         AS SfuDel,
                sfu_createname  AS CreateName,
                sfu_createtime  AS CreateTime,
                sfu_modifyname  AS ModifyName,
                sfu_modifytime  AS ModifyTime,
                sfu_version     AS SfuVersion
            FROM sysfuction WHERE sfu_no = @SfuNo";

            var sfuNoParam = new SqlParameter("@SfuNo", sfuNo);
            return await _context.Database.SqlQueryRaw<SYS202002CreateViewModel>(sql, sfuNoParam).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DropdownViewModel>> GetSystemsAsync()
        {
            var sql = @"
                SELECT sys_no AS Value, sys_name AS Text 
                FROM sys 
                WHERE sys_status = '1' 
                ORDER BY sys_order;";
            return await _context.Database.SqlQueryRaw<DropdownViewModel>(sql).ToListAsync();
        }

        public async Task<IEnumerable<DropdownViewModel>> GetFunctionsBySystemAsync(int sysNo)
        {
            var sql = @"
                SELECT 
                    sfu_no AS Value, 
                    sfu_name AS Text 
                FROM sysfuction 
                WHERE sys_no = @SysNo 
                AND (sfu_parent = 0 OR sfu_parent IS NULL) 
                ORDER BY sfu_order;";
            var sysNoParam = new SqlParameter("@SysNo", sysNo);

            return await _context.Database.SqlQueryRaw<DropdownViewModel>(sql, sysNoParam).ToListAsync();
        }

        public async Task<int> CreateFunctionAsync(SYS202002CreateViewModel model, string userName, IDataLogger dataLogger)
        {
            var sql = @"
        INSERT INTO sysfuction 
        (sfu_no, sys_no, sfu_name, sfu_disname, sfu_disshorten, sfu_catalog, sfu_order, sfu_path, 
         sfu_defaltpic, sfu_overpicture, sfu_parent, sfu_status, sfu_builtin, sfu_version, 
         sfu_Ins, sfu_Edi, sfu_Del, sfu_createname, sfu_createtime, sfu_modifyname, sfu_modifytime)
        VALUES 
        (@SfuNo, @SysNo, @SfuName, @SfuName, @SfuName, @SfuCatalog, @SfuOrder, @SfuPath, 
         '', '', @SfuParent, @SfuStatus, 2, @SfuVersion, 
         @SfuIns, @SfuEdi, @SfuDel, @UserName, GETDATE(), @UserName, GETDATE())";

            var parameters = new[]
            {
        new SqlParameter("@SfuNo", model.SfuNo),
        new SqlParameter("@SysNo", model.SysNo ?? 0),
        new SqlParameter("@SfuName", model.SfuName),
        new SqlParameter("@SfuCatalog", model.SfuCatalog ?? ""),
        new SqlParameter("@SfuOrder", model.SfuOrder),
        new SqlParameter("@SfuPath", model.SfuPath ?? ""),
        new SqlParameter("@SfuParent", model.SfuParent),
        new SqlParameter("@SfuStatus", model.SfuStatus),
        new SqlParameter("@SfuVersion", model.SfuVersion ?? "1.0.0"),
        new SqlParameter("@SfuIns", model.SfuIns),
        new SqlParameter("@SfuEdi", model.SfuEdi),
        new SqlParameter("@SfuDel", model.SfuDel),
        new SqlParameter("@UserName", userName)
    };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            return await SaveChangesAsync(dataLogger);
        }

        public async Task<bool> UpdateFunctionAsync(SYS202002CreateViewModel model, string userName, IDataLogger dataLogger)
        {
            var sql = @"
        UPDATE sysfuction 
        SET sys_no = @SysNo,
            sfu_name = @SfuName,
            sfu_disname = @SfuName,
            sfu_catalog = @SfuCatalog,
            sfu_order = @SfuOrder,
            sfu_path = @SfuPath,
            sfu_parent = @SfuParent,
            sfu_status = @SfuStatus,
            sfu_Ins = @SfuIns,
            sfu_Edi = @SfuEdi,
            sfu_Del = @SfuDel,
            sfu_modifyname = @UserName,
            sfu_modifytime = GETDATE()
        WHERE sfu_no = @SfuNo";

            var parameters = new[]
            {
        new SqlParameter("@SysNo", model.SysNo ?? 0),
        new SqlParameter("@SfuName", model.SfuName),
        new SqlParameter("@SfuCatalog", model.SfuCatalog ?? ""),
        new SqlParameter("@SfuOrder", model.SfuOrder),
        new SqlParameter("@SfuPath", model.SfuPath ?? ""),
        new SqlParameter("@SfuParent", model.SfuParent),
        new SqlParameter("@SfuStatus", model.SfuStatus),
        new SqlParameter("@SfuIns", model.SfuIns),
        new SqlParameter("@SfuEdi", model.SfuEdi),
        new SqlParameter("@SfuDel", model.SfuDel),
        new SqlParameter("@UserName", userName),
        new SqlParameter("@SfuNo", model.SfuNo)
    };
            var affectedRows = await _context.Database.ExecuteSqlRawAsync(sql, parameters);

            if (affectedRows > 0)
            {
                await SaveChangesAsync(dataLogger);
            }

            return affectedRows > 0;
        }

        public async Task<int> SoftDeleteFunctionsAsync(List<int> sfuNos, string userName, IDataLogger dataLogger)
        {
            if (sfuNos == null || !sfuNos.Any()) return 0;

            var sql = $@"
        UPDATE sysfuction 
        SET sfu_status = 2,
            sfu_modifyname = @UserName,
            sfu_modifytime = GETDATE()
        WHERE sfu_no IN ({string.Join(",", sfuNos)})
        AND sfu_status != 2";

            var parameter = new SqlParameter("@UserName", userName);

            var result = await _context.Database.ExecuteSqlRawAsync(sql, parameter);
            await SaveChangesAsync(dataLogger);
            return result;
        }

        public async Task<int> UpdateChildrenStatusAsync(int sfuNo, byte status, string userName, IDataLogger dataLogger)
        {
            var sql = @"
        WITH FunctionHierarchy AS
        (
            SELECT sfu_no FROM sysfuction WHERE sfu_parent = @SfuNo
            UNION ALL
            SELECT f.sfu_no FROM sysfuction f
            INNER JOIN FunctionHierarchy h ON f.sfu_parent = h.sfu_no
        )
        UPDATE f
        SET f.sfu_status = @Status,
            f.sfu_modifyname = @UserName,
            f.sfu_modifytime = GETDATE()
        FROM sysfuction f
        INNER JOIN FunctionHierarchy h ON f.sfu_no = h.sfu_no
        WHERE f.sfu_status != 2";

            var parameters = new[]
            {
                new SqlParameter("@SfuNo", sfuNo),
                new SqlParameter("@Status", status),
                new SqlParameter("@UserName", userName)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            await SaveChangesAsync(dataLogger);
            return result;
        }



    }
}