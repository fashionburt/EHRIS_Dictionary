using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
using static EHRIS.Core.Models.LoginViewModel;

namespace EHRIS.Core.Repositories.AdminPortal
{
    public class ADS999001Repository : BaseRepository, IADS999001Repository
    {
        public ADS999001Repository(ApplicationDbContext context) : base(context) { }

        public async Task<DataTablesResponse<ADS999001ListViewModel>> GetSysNoticePagedListAsync(DataTablesRequest request)
        {
            var parameters = new List<SqlParameter>();
            var whereSql = new StringBuilder(" WHERE a.sysn_status = @status ");
            parameters.Add(new SqlParameter("@status", SqlDbType.Char, 1) { Value = "1" });

            if (request.filter != null)
            {
                if (request.filter.DateStart.HasValue)
                {
                    whereSql.Append(" AND a.sysn_publicdt >= @dateStart ");
                    parameters.Add(new SqlParameter("@dateStart", SqlDbType.Date) { Value = request.filter.DateStart.Value.Date });
                }
                if (request.filter.DateEnd.HasValue)
                {
                    whereSql.Append(" AND a.sysn_publicdt <= @dateEnd ");
                    parameters.Add(new SqlParameter("@dateEnd", SqlDbType.Date) { Value = request.filter.DateEnd.Value.Date });
                }
                if (request.filter.SysnType.HasValue)
                {
                    whereSql.Append(" AND a.sysn_type = @sysnType ");
                    parameters.Add(new SqlParameter("@sysnType", SqlDbType.TinyInt) { Value = request.filter.SysnType.Value });
                }
            }

            if (request.extraSearch != null
                && !string.IsNullOrWhiteSpace(request.extraSearch.searchValue)
                && request.extraSearch.columnIndexes != null
                && request.extraSearch.columnIndexes.Any())
            {
                var conds = new List<string>();
                foreach (var idx in request.extraSearch.columnIndexes)
                {
                    if (idx == 1) conds.Add("a.sysn_content LIKE @kw");
                }
                if (conds.Any())
                {
                    whereSql.Append(" AND (").Append(string.Join(" OR ", conds)).Append(") ");
                    parameters.Add(new SqlParameter("@kw", $"%{request.extraSearch.searchValue}%"));
                }
            }

            var orderBySql = "ORDER BY a.sysn_top DESC, a.sysn_publicdt DESC, a.sysn_no DESC";
            if (request.orderby != null && request.orderby.Any())
            {
                var order = request.orderby.First();
                var columnIndex = order.column;
                var sortColumn = request.Columns != null && columnIndex < request.Columns.Count
                    ? request.Columns[columnIndex].data
                    : null;
                var sortDirection = order.dir != null && order.dir.ToUpper() == "ASC" ? "ASC" : "DESC";

                var validSortColumns = new Dictionary<string, string>
                {
                    { "sysnType", "a.sysn_type" },
                    { "sysnTypeName", "a.sysn_type" },
                    { "sysnContent", "a.sysn_content" },
                    { "sysnPublicDt", "a.sysn_publicdt" },
                    { "sysnStartTime", "a.sysn_starttime" },
                    { "sysnEndTime", "a.sysn_endtime" },
                    { "sysnTop", "a.sysn_top" }
                };

                if (sortColumn != null && validSortColumns.ContainsKey(sortColumn))
                {
                    orderBySql = $"ORDER BY {validSortColumns[sortColumn]} {sortDirection}";
                }
            }

            var dataSql = $@"
                SELECT
                    a.sysn_no AS SysnNo,
                    a.sysn_type AS SysnType,
                    '' AS SysnTypeName,
                    a.sysn_content AS SysnContent,
                    a.sysn_publicdt AS SysnPublicDt,
                    a.sysn_starttime AS SysnStartTime,
                    a.sysn_endtime AS SysnEndTime,
                    a.sysn_top AS SysnTop,
                    a.sysn_modifyname AS SysnModifyName,
                    a.sysn_modifytime AS SysnModifyTime
                FROM sysnotice a
                {whereSql}
                {orderBySql}
                OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;";

            var dataParameters = new List<SqlParameter>(parameters);
            dataParameters.Add(new SqlParameter("@start", request.Start));
            dataParameters.Add(new SqlParameter("@length", request.Length));

            var data = await _context.Database.SqlQueryRaw<ADS999001ListViewModel>(dataSql, dataParameters.ToArray()).ToListAsync();

            var totalRecordsSql = "SELECT COUNT(*) AS Value FROM sysnotice WHERE sysn_status = @status";
            var totalRecordsParam = new SqlParameter("@status", SqlDbType.Char, 1) { Value = "1" };
            var totalRecords = await _context.Database.SqlQueryRaw<int>(totalRecordsSql, totalRecordsParam).SingleAsync();

            var filteredRecordsSql = $"SELECT COUNT(*) AS Value FROM sysnotice a {whereSql}";
            var filteredRecords = await _context.Database.SqlQueryRaw<int>(filteredRecordsSql, parameters.ToArray()).SingleAsync();

            return new DataTablesResponse<ADS999001ListViewModel>
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            };
        }

        public async Task<SysNotice> GetSysNoticeByNoAsync(int sysnNo)
        {
            var sql = @"
                SELECT [sysn_no], [sysn_type], [sysn_publicdt], [sysn_starttime], [sysn_endtime],
                       [sysn_content], [sysn_top], [sysn_status],
                       [sysn_createname], [sysn_createtime], [sysn_modifyname], [sysn_modifytime]
                FROM sysnotice
                WHERE sysn_no = @sysnNo";
            var param = new SqlParameter("@sysnNo", sysnNo);
            return await _context.SysNotices.FromSqlRaw(sql, param).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<bool> CreateSysNoticeAsync(SysNoticeUpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var entity = new SysNotice
            {
                SysnType = model.SysnType,
                SysnContent = model.SysnContent ?? string.Empty,
                SysnPublicDt = model.SysnPublicDt,
                SysnStartTime = model.SysnStartTime,
                SysnEndTime = model.SysnEndTime,
                SysnTop = model.SysnTop,
                SysnStatus = "1",
                SysnCreateName = userName,
                SysnCreateTime = DateTime.Now,
                SysnModifyName = userName,
                SysnModifyTime = DateTime.Now
            };

            await _context.SysNotices.AddAsync(entity);
            return await SaveChangesAsync(dataLogger) > 0;
        }

        public async Task<bool> UpdateSysNoticeAsync(SysNoticeUpdateViewModel model, string userName, IDataLogger dataLogger)
        {
            var entity = await _context.SysNotices.FirstOrDefaultAsync(x => x.SysnNo == model.SysnNo);
            if (entity == null) return false;

            entity.SysnType = model.SysnType;
            entity.SysnContent = model.SysnContent ?? string.Empty;
            entity.SysnPublicDt = model.SysnPublicDt;
            entity.SysnStartTime = model.SysnStartTime;
            entity.SysnEndTime = model.SysnEndTime;
            entity.SysnTop = model.SysnTop;
            entity.SysnModifyName = userName;
            entity.SysnModifyTime = DateTime.Now;

            return await SaveChangesAsync(dataLogger) > 0;
        }

        public async Task<bool> SoftDeleteSysNoticeAsync(int sysnNo, IDataLogger dataLogger)
        {
            var entity = await _context.SysNotices.FirstOrDefaultAsync(x => x.SysnNo == sysnNo);
            if (entity == null) return false;

            entity.SysnStatus = "2";
            entity.SysnModifyTime = DateTime.Now;
            return await SaveChangesAsync(dataLogger) > 0;
        }

        public async Task<List<LoginNoticeViewModel>> GetLoginNoticesAsync(int topN)
        {
            var sql = @"
        SELECT TOP (@topN)
            a.sysn_no AS SysnNo,
            a.sysn_type AS SysnType,
            CAST('' AS NVARCHAR(20)) AS SysnTypeName,
            CAST('' AS NVARCHAR(20)) AS SysnTypeCode,
            a.sysn_content AS SysnContent,
            a.sysn_publicdt AS SysnPublicDt,
            a.sysn_top AS SysnTop
        FROM sysnotice a
        WHERE a.sysn_status = @status
          AND a.sysn_starttime <= @now
          AND a.sysn_endtime   >= @now
        ORDER BY a.sysn_top DESC, a.sysn_publicdt DESC, a.sysn_no DESC";

            var parameters = new[]
            {
        new SqlParameter("@topN", SqlDbType.Int) { Value = topN },
        new SqlParameter("@status", SqlDbType.Char, 1) { Value = "1" },
        new SqlParameter("@now", SqlDbType.DateTime) { Value = DateTime.Now }
    };

            return await _context.Database.SqlQueryRaw<LoginNoticeViewModel>(sql, parameters).ToListAsync();
        }
    }
}