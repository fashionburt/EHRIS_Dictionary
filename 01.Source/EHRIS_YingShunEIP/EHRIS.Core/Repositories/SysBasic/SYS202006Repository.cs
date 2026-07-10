using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EHRIS.Core.Repositories.SysBasic
{
    public class SYS202006Repository : BaseRepository, ISYS202006Repository
    {
        public SYS202006Repository(ApplicationDbContext context) : base(context)
        {
        }

        private string BuildHolidaySearchWhereClause(ExtraSearch extraSearch)
        {
            if (extraSearch == null || string.IsNullOrEmpty(extraSearch.searchValue))
            {
                return string.Empty;
            }

            var searchConditions = new List<string>();
            bool searchSpecificColumns = extraSearch.columnIndexes != null && extraSearch.columnIndexes.Any();


            if (searchSpecificColumns)
            {
                if (extraSearch.columnIndexes.Contains(0)) searchConditions.Add("p.hol_code LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(1)) searchConditions.Add("p.hol_name LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(5)) searchConditions.Add("p.hol_modifyname LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(2)) searchConditions.Add("(CASE WHEN p.hol_statistics = '1' THEN N'是' ELSE N'否' END) LIKE @searchValue");
                if (extraSearch.columnIndexes.Contains(3)) searchConditions.Add("(CASE WHEN p.hol_official = '1' THEN N'是' ELSE N'否' END) LIKE @searchValue");
            }
            else
            {
                searchConditions.Add("p.hol_code LIKE @searchValue");
                searchConditions.Add("p.hol_name LIKE @searchValue");
                searchConditions.Add("(CASE WHEN p.hol_statistics = '1' THEN N'是' ELSE N'否' END) LIKE @searchValue");
                searchConditions.Add("(CASE WHEN p.hol_official = '1' THEN N'是' ELSE N'否' END) LIKE @searchValue");
                searchConditions.Add("p.hol_modifyname LIKE @searchValue");
            }

            if (searchConditions.Any())
            {
                return $" AND ({string.Join(" OR ", searchConditions)}) ";
            }

            return string.Empty;
        }

        public async Task<DataTablesResponse<SYS202006ListViewModel>> GetHolidayPagedListAsync(DataTablesRequest request)
        {
            var parameters = new List<SqlParameter>();
            var whereSql = new StringBuilder("WHERE p.hol_status = '1' ");

            var searchValue = request.extraSearch?.searchValue;
            if (!string.IsNullOrEmpty(searchValue))
            {
                parameters.Add(new SqlParameter("@searchValue", $"%{searchValue}%"));
                whereSql.Append(BuildHolidaySearchWhereClause(request.extraSearch));
            }

            var baseSql = $@" FROM HR_holiday p {whereSql}"; // baseSql includes FROM and WHERE
            var orderBySql = "ORDER BY p.hol_order ASC, p.hol_code ASC"; // Default order

            if (request.orderby != null && request.orderby.Any())
            {
                var order = request.orderby.First();
                var columnIndex = order.column;
                var sortColumn = request.Columns[columnIndex].data;
                var sortDirection = order.dir.ToUpper() == "ASC" ? "ASC" : "DESC";

                var validSortColumns = new Dictionary<string, string>
                 {
                     { "holCode", "p.hol_code" },
                     { "holName", "p.hol_name" },
                     { "holStatisticsDisplay", "p.hol_statistics" },
                     { "holOfficialDisplay", "p.hol_official" },
                     { "holOrder", "p.hol_order" },
                     { "holModifyName", "p.hol_modifyname" },
                     { "holModifyTime", "p.hol_modifytime" }
                 };

                if (validSortColumns.ContainsKey(sortColumn))
                {
                    orderBySql = $"ORDER BY {validSortColumns[sortColumn]} {sortDirection}";
                }
                else
                {
                    orderBySql = "ORDER BY p.hol_order ASC, p.hol_code ASC";
                }
            }

            var dataSql = $@"
                SELECT
                    p.hol_no AS HolNo,
                    p.hol_code AS HolCode,
                    p.hol_name AS HolName,
                    p.hol_statistics AS HolStatistics,
                    CASE WHEN p.hol_statistics = '1' THEN N'是' ELSE N'否' END AS HolStatisticsDisplay,
                    p.hol_official AS HolOfficial,
                    CASE WHEN p.hol_official = '1' THEN N'是' ELSE N'否' END AS HolOfficialDisplay,
                    p.hol_order AS HolOrder,
                    p.hol_modifyname AS HolModifyName,
                    p.hol_modifytime AS HolModifyTime
                {baseSql}
                {orderBySql}
                OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;";

            var totalRecordsSql = "SELECT COUNT(*) AS Value FROM HR_holiday WHERE hol_status = '1'";

            int totalRecords = await _context.Database.SqlQueryRaw<int>(totalRecordsSql).SingleAsync();

            int filteredRecords = totalRecords;
            if (!string.IsNullOrEmpty(searchValue))
            {
                var filteredRecordsSql = $"SELECT COUNT(*) AS Value {baseSql}";
                filteredRecords = await _context.Database.SqlQueryRaw<int>(filteredRecordsSql, parameters.ToArray()).SingleAsync();
            }

            var dataParameters = new List<SqlParameter>(parameters);
            dataParameters.Add(new SqlParameter("@start", request.Start));
            dataParameters.Add(new SqlParameter("@length", request.Length));

            var data = await _context.Database.SqlQueryRaw<SYS202006ListViewModel>(dataSql, dataParameters.ToArray()).ToListAsync();

            return new DataTablesResponse<SYS202006ListViewModel>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            };
        }

        public async Task<HRHoliday> GetHolidayByIdAsync(int holNo)
        {
            var sql = @"
                SELECT [hol_no], [hol_code], [hol_name], [hol_order], [hol_statistics], [hol_official], [hol_status], [hol_createtime], [hol_createname], [hol_modifytime], [hol_modifyname] 
                FROM HR_holiday 
                WHERE hol_no = @holNo";
            var param = new SqlParameter("@holNo", holNo);
            return await _context.HRHoliday.FromSqlRaw(sql, param).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int currentId)
        {
            var sql = @"
                SELECT 1 AS Value 
                FROM HR_holiday 
                WHERE hol_code = @code AND hol_no != @currentId AND hol_status = '1'";
            var parameters = new[]
            {
                new SqlParameter("@code", code),
                new SqlParameter("@currentId", currentId)
            };
            var result = await _context.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();
            return result == 1;
        }

        public async Task CreateHolidayAsync(SYS202006UpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var entity = new HRHoliday
            {
                HolCode = model.HolCode,
                HolName = model.HolName,
                HolOrder = model.HolOrder,
                HolStatistics = model.HolStatistics,
                HolOfficial = model.HolOfficial,
                HolStatus = "1",
                HolCreateTime = DateTime.Now,
                HolCreateName = userName,
                HolModifyTime = DateTime.Now,
                HolModifyName = userName
            };

            await _context.HRHoliday.AddAsync(entity);
            await SaveChangesAsync(dataLogger);
        }

        public async Task UpdateHolidayAsync(SYS202006UpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var holiday = await _context.HRHoliday.FirstOrDefaultAsync(x => x.HolNo == model.HolNo);

            if (holiday != null)
            {
                holiday.HolName = model.HolName;
                holiday.HolOrder = model.HolOrder;
                holiday.HolStatistics = model.HolStatistics;
                holiday.HolOfficial = model.HolOfficial;
                holiday.HolModifyTime = DateTime.Now;
                holiday.HolModifyName = userName;

                await SaveChangesAsync(dataLogger);
            }
        }

        public async Task SoftDeleteHolidayAsync(int holNo, IDataLogger dataLogger)
        {
            var holiday = await _context.HRHoliday.FirstOrDefaultAsync(x => x.HolNo == holNo);

            if (holiday != null)
            {
                holiday.HolStatus = "0";
                await SaveChangesAsync(dataLogger);
            }
        }
    }
}