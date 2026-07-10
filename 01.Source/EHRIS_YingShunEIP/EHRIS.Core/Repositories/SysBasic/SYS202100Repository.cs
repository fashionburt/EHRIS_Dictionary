using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EHRIS.Core.Repositories
{
    public class SYS202100Repository : BaseRepository, ISYS202100Repository
    {
        public SYS202100Repository(ApplicationDbContext context) : base(context)
        {
        }

        private string BuildSearchWhereClause(ExtraSearch extraSearch)
        {
            if (extraSearch == null || string.IsNullOrEmpty(extraSearch.searchValue) || extraSearch.columnIndexes == null || !extraSearch.columnIndexes.Any())
            {
                return string.Empty;
            }

            var searchConditions = new List<string>();

            if (extraSearch.columnIndexes.Contains(0))
            {
                searchConditions.Add("arg_variable LIKE @searchValue");
            }
            if (extraSearch.columnIndexes.Contains(1))
            {
                searchConditions.Add("arg_describe LIKE @searchValue");
            }

            if (searchConditions.Any())
            {
                return $" AND ({string.Join(" OR ", searchConditions)}) ";
            }

            return string.Empty;
        }

        public async Task<(IEnumerable<Sys202100ListViewModel> Data, int RecordsFiltered, int RecordsTotal)> GetPagedListAsync(Sys202100DataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            var searchParam = new SqlParameter("@searchValue", string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%");

            var startParam = new SqlParameter("@start", request.start);
            var lengthParam = new SqlParameter("@length", request.length);

            var baseWhereClause = "WHERE arg_status = 1 AND arg_openmanager = 1 ";

            var totalCountSql = $"SELECT COUNT(1) FROM arguments {baseWhereClause}";
            var recordsTotal = (await _context.Database.SqlQueryRaw<int>(totalCountSql).ToListAsync()).Single();

            var searchWhereClause = new StringBuilder(baseWhereClause);
            searchWhereClause.Append(BuildSearchWhereClause(request.extraSearch));
            var searchWhereClauseString = searchWhereClause.ToString();

            var filteredCountSql = $"SELECT COUNT(1) FROM arguments {searchWhereClauseString}";
            var recordsFiltered = (await _context.Database.SqlQueryRaw<int>(filteredCountSql, searchParam).ToListAsync()).Single();

            var orderByClause = "ORDER BY arg_order ASC";
            if (request.orderby != null && request.orderby.Any())
            {
                var firstOrder = request.orderby.First();
                var columnName = request.columns[firstOrder.column].data;
                var direction = firstOrder.dir.ToUpper() == "ASC" ? "ASC" : "DESC";

                var allowedColumns = new Dictionary<string, string>
                {
                    { "argVariable", "arg_variable" },
                    { "argDescribe", "arg_describe" },
                    { "argValue", "arg_value" }
                };
                if (allowedColumns.ContainsKey(columnName))
                {
                    orderByClause = $"ORDER BY {allowedColumns[columnName]} {direction}";
                }
            }

            var dataSql = $@"
            SELECT 
                arg_variable AS ArgVariable, 
                arg_describe AS ArgDescribe, 
                arg_value AS ArgValue,
                ISNULL((SELECT TOP 1 agd_value FROM arguments_dept WHERE arg_variable = a.arg_variable AND agd_status = 1), '未設定') AS AgdValueDisplay
            FROM arguments a
            {searchWhereClauseString}
            {orderByClause}
            OFFSET @start ROWS
            FETCH NEXT @length ROWS ONLY";

            var data = await _context.Database.SqlQueryRaw<Sys202100ListViewModel>(dataSql, searchParam, startParam, lengthParam).ToListAsync();

            return (data, recordsFiltered, recordsTotal);
        }

        public async Task<Arguments> GetByVariableAsync(string argVariable)
        {
            return await _context.Set<Arguments>().AsNoTracking().FirstOrDefaultAsync(r => r.ArgVariable == argVariable && r.ArgStatus == 1);
        }

        public async Task<bool> UpdateAsync(Arguments entity, IDataLogger dataLogger)
        {
            var existing = await _context.Set<Arguments>().FirstOrDefaultAsync(x => x.ArgVariable == entity.ArgVariable);
            if (existing == null || existing.ArgStatus == 2) return false;

            existing.ArgDescribe = entity.ArgDescribe ?? "";
            existing.ArgModifyName = entity.ArgModifyName ?? "";
            existing.ArgModifyTime = DateTime.Now;

            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<IEnumerable<Sys202100DeptListViewModel>> GetDeptListAsync(string argVariable)
        {
            var sql = @"
            SELECT 
                d.agd_no AS AgdNo,
                d.arg_variable AS ArgVariable,
                a.arg_describe AS ArgDescribe,
                d.dep_no AS DepNo,
                ISNULL(dp.dep_name, CASE WHEN d.dep_no = 1 THEN '全公司' ELSE '' END) AS DepName,
                a.arg_value AS ArgValue,
                d.agd_value AS AgdValue
            FROM arguments_dept d
            INNER JOIN arguments a ON d.arg_variable = a.arg_variable
            LEFT JOIN departments dp ON d.dep_no = dp.dep_no
            WHERE d.arg_variable = @argVariable AND d.agd_status = 1";

            var param = new SqlParameter("@argVariable", argVariable);
            return await _context.Database.SqlQueryRaw<Sys202100DeptListViewModel>(sql, param).ToListAsync();
        }

        public async Task<bool> UpdateDeptAsync(ArgumentsDept deptEntity, IDataLogger dataLogger)
        {
            if (deptEntity.AgdNo == 0)
            {
                var existingDept = await _context.Set<ArgumentsDept>()
                    .FirstOrDefaultAsync(x => x.ArgVariable == deptEntity.ArgVariable && x.DepNo == deptEntity.DepNo);

                if (existingDept != null)
                {
                    existingDept.AgdValue = deptEntity.AgdValue ?? "";
                    existingDept.AgdStatus = 1;
                    existingDept.AgdModifyName = deptEntity.AgdModifyName ?? "";
                    existingDept.AgdModifyTime = DateTime.Now;
                }
                else
                {
                    deptEntity.AgdStatus = 1;
                    deptEntity.AgdValue = deptEntity.AgdValue ?? "";
                    deptEntity.AgdCreateName = deptEntity.AgdModifyName ?? "";
                    deptEntity.AgdCreateTime = DateTime.Now;
                    deptEntity.AgdModifyName = deptEntity.AgdModifyName ?? "";
                    deptEntity.AgdModifyTime = DateTime.Now;
                    await _context.Set<ArgumentsDept>().AddAsync(deptEntity);
                }
            }
            else
            {
                var existingDept = await _context.Set<ArgumentsDept>().FirstOrDefaultAsync(x => x.AgdNo == deptEntity.AgdNo);
                if (existingDept != null)
                {
                    existingDept.AgdValue = deptEntity.AgdValue ?? "";
                    existingDept.AgdModifyName = deptEntity.AgdModifyName ?? "";
                    existingDept.AgdModifyTime = DateTime.Now;
                }
            }

            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<bool> SoftDeleteDeptAsync(int agdNo, string modifyUser, IDataLogger dataLogger)
        {
            var existingDept = await _context.Set<ArgumentsDept>().FirstOrDefaultAsync(x => x.AgdNo == agdNo);
            if (existingDept == null) return false;

            existingDept.AgdStatus = 2;
            existingDept.AgdModifyName = modifyUser ?? "";
            existingDept.AgdModifyTime = DateTime.Now;

            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<IEnumerable<Sys202100DepOption>> GetDepartmentOptionsAsync()
        {
            var sql = "SELECT dep_no AS DepNo, dep_name AS DepName FROM departments WHERE dep_status = 1 ORDER BY dep_order ASC";
            return await _context.Database.SqlQueryRaw<Sys202100DepOption>(sql).ToListAsync();
        }

        public async Task<IEnumerable<Sys202100SchedViewModel>> GetSchedListAsync(string argVariable, int depNo)
        {
            var sql = @"
            SELECT 
                ags_no AS AgsNo,
                arg_variable AS ArgVariable,
                dep_no AS DepNo,
                ags_value AS AgsValue,
                ags_starttime AS AgsStartTime,
                ags_endtime AS AgsEndTime
            FROM arguments_schedule
            WHERE arg_variable = @argVariable AND dep_no = @depNo AND ags_status = 1
            ORDER BY ags_starttime ASC";

            var pVariable = new SqlParameter("@argVariable", argVariable);
            var pDepNo = new SqlParameter("@depNo", depNo);

            return await _context.Database.SqlQueryRaw<Sys202100SchedViewModel>(sql, pVariable, pDepNo).ToListAsync();
        }

        public async Task<bool> UpdateSchedAsync(ArgumentsSchedule scheduleEntity, IDataLogger dataLogger)
        {
            if (scheduleEntity.AgsNo == 0)
            {
                scheduleEntity.AgsStatus = 1;
                scheduleEntity.AgsValue = scheduleEntity.AgsValue ?? "";
                scheduleEntity.AgsCreateName = scheduleEntity.AgsModifyName ?? "";
                scheduleEntity.AgsCreateTime = DateTime.Now;
                scheduleEntity.AgsModifyName = scheduleEntity.AgsModifyName ?? "";
                scheduleEntity.AgsModifyTime = DateTime.Now;
                await _context.Set<ArgumentsSchedule>().AddAsync(scheduleEntity);
            }
            else
            {
                var existingSchedule = await _context.Set<ArgumentsSchedule>().FirstOrDefaultAsync(x => x.AgsNo == scheduleEntity.AgsNo);
                if (existingSchedule != null)
                {
                    existingSchedule.AgsValue = scheduleEntity.AgsValue ?? "";
                    existingSchedule.AgsStartTime = scheduleEntity.AgsStartTime;
                    existingSchedule.AgsEndTime = scheduleEntity.AgsEndTime;
                    existingSchedule.AgsModifyName = scheduleEntity.AgsModifyName ?? "";
                    existingSchedule.AgsModifyTime = DateTime.Now;
                }
            }

            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<bool> SoftDeleteSchedAsync(int agsNo, string modifyUser, IDataLogger dataLogger)
        {
            var existingSchedule = await _context.Set<ArgumentsSchedule>().FirstOrDefaultAsync(x => x.AgsNo == agsNo);
            if (existingSchedule == null) return false;

            existingSchedule.AgsStatus = 2;
            existingSchedule.AgsModifyName = modifyUser ?? "";
            existingSchedule.AgsModifyTime = DateTime.Now;

            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<bool> SoftDeleteSchedByAgdNoAsync(int agdNo, string modifyUser, IDataLogger dataLogger)
        {
            var dept = await _context.Set<ArgumentsDept>().AsNoTracking().FirstOrDefaultAsync(x => x.AgdNo == agdNo);
            if (dept == null) return false;

            var schedules = await _context.Set<ArgumentsSchedule>()
                .Where(x => x.ArgVariable == dept.ArgVariable && x.DepNo == dept.DepNo && x.AgsStatus == 1)
                .ToListAsync();

            foreach (var sched in schedules)
            {
                sched.AgsStatus = 2;
                sched.AgsModifyName = modifyUser ?? "";
                sched.AgsModifyTime = DateTime.Now;
            }

            await SaveChangesAsync(dataLogger);
            return true;
        }

    }
}