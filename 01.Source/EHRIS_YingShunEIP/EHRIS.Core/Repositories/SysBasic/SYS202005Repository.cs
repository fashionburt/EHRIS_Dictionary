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
    public class SYS202005Repository : BaseRepository, ISYS202005Repository
    {
        public SYS202005Repository(ApplicationDbContext context) : base(context)
        {
        }

        private string BuildSearchWhereClause(ExtraSearch extraSearch)
        {
            if (extraSearch == null ||
                string.IsNullOrEmpty(extraSearch.searchValue) ||
                extraSearch.columnIndexes == null ||
                !extraSearch.columnIndexes.Any())
            {
                return string.Empty;
            }

            var searchConditions = new List<string>();

            if (extraSearch.columnIndexes.Contains(0))
            {
                searchConditions.Add("ple_code LIKE @searchValue");
            }
            if (extraSearch.columnIndexes.Contains(1))
            {
                searchConditions.Add("ple_name LIKE @searchValue");
            }

            if (searchConditions.Any())
            {
                return $" AND ({string.Join(" OR ", searchConditions)}) ";
            }

            return string.Empty;
        }

        public async Task<(IEnumerable<Sys202005ListViewModel> Data, int RecordsFiltered, int RecordsTotal)>
            GetPagedListAsync(Sys202005DataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            var searchParam = new SqlParameter("@searchValue",
                string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%");

            var startParam = new SqlParameter("@start", request.start);
            var lengthParam = new SqlParameter("@length", request.length);

            var baseWhereClause = "WHERE ple_status = '1' ";

            var totalCountSql = $"SELECT COUNT(1) FROM HR_plevel {baseWhereClause}";
            var recordsTotal = (await _context.Database.SqlQueryRaw<int>(totalCountSql).ToListAsync()).Single();

            var searchWhereClause = new StringBuilder(baseWhereClause);
            searchWhereClause.Append(BuildSearchWhereClause(request.extraSearch));
            var searchWhereClauseString = searchWhereClause.ToString();

            var filteredCountSql = $"SELECT COUNT(1) FROM HR_plevel {searchWhereClauseString}";
            var recordsFiltered = (await _context.Database.SqlQueryRaw<int>(filteredCountSql, searchParam).ToListAsync()).Single();

            var orderByClause = "ORDER BY ple_modifytime DESC";
            if (request.orderby != null && request.orderby.Any())
            {
                var firstOrder = request.orderby.First();
                var columnName = request.columns[firstOrder.column].data;
                var direction = firstOrder.dir.ToUpper() == "ASC" ? "ASC" : "DESC";

                var allowedColumns = new Dictionary<string, string>
                {
                    { "pleCode", "ple_code" },
                    { "pleName", "ple_name" },
                    { "pleModifyName", "ple_modifyname" },
                    { "pleModifyTimeDisplay", "ple_modifytime" }
                };
                if (allowedColumns.ContainsKey(columnName))
                {
                    orderByClause = $"ORDER BY {allowedColumns[columnName]} {direction}";
                }
            }

            var dataSql = $@"
                SELECT 
                    ple_no AS PleNo, 
                    ple_code AS PleCode, 
                    ple_name AS PleName, 
                    ple_modifyname AS PleModifyName, 
                    ple_modifytime AS PleModifyTime
                FROM HR_plevel
                {searchWhereClauseString}
                {orderByClause}
                OFFSET @start ROWS
                FETCH NEXT @length ROWS ONLY";

            var data = await _context.Database.SqlQueryRaw<Sys202005ListViewModel>(dataSql, searchParam, startParam, lengthParam).ToListAsync();

            return (data, recordsFiltered, recordsTotal);
        }

        public async Task<HRPLevel> GetByNoAsync(int pleNo)
        {
            return await _context.HRPlevel
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PleNo == pleNo && r.PleStatus == "1");
        }

        public async Task<HRPLevel> GetTrackedByNoAsync(int pleNo)
        {
            var sql = "SELECT [ple_no], [pleT_no], [ple_code], [ple_name], [ple_status], [ple_createtime], [ple_createname], [ple_modifytime], [ple_modifyname] FROM HR_plevel WHERE ple_no = @pleNo";
            var idParam = new SqlParameter("@pleNo", pleNo);
            return await _context.HRPlevel.FromSqlRaw(sql, idParam).FirstOrDefaultAsync();
        }

        public async Task<bool> CodeExistsAsync(string pleCode, int? pleNo = null)
        {
            var sql = "SELECT 1 FROM HR_plevel WHERE ple_code = @code AND ple_status = '1' AND (@pleNo IS NULL OR ple_no != @pleNo)";
            var codeParam = new SqlParameter("@code", pleCode);
            var idParam = new SqlParameter("@pleNo", (object)pleNo ?? DBNull.Value);
            var result = await _context.Database.SqlQueryRaw<int>(sql, codeParam, idParam).ToListAsync();
            return result.Any();
        }

        public async Task<bool> AddAsync(HRPLevel entity, IDataLogger dataLogger)
        {
            await _context.HRPlevel.AddAsync(entity);
            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<bool> UpdateAsync(HRPLevel entity, IDataLogger dataLogger)
        {
            var existing = await GetTrackedByNoAsync(entity.PleNo);
            if (existing == null || existing.PleStatus == "2") return false;

            existing.PleCode = entity.PleCode;
            existing.PleName = entity.PleName;
            existing.PleModifyName = entity.PleModifyName;
            existing.PleModifyTime = entity.PleModifyTime;

            await SaveChangesAsync(dataLogger);
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int pleNo, string modifyUser, IDataLogger dataLogger)
        {
            var existing = await GetTrackedByNoAsync(pleNo);
            if (existing == null) return false;

            existing.PleStatus = "2";
            existing.PleModifyName = modifyUser;
            existing.PleModifyTime = DateTime.Now;

            await SaveChangesAsync(dataLogger);
            return true;
        }
    }
}