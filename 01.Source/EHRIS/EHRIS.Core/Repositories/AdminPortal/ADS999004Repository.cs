using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EHRIS.Core.Repositories.AdminPortal
{
    public class ADS999004Repository : BaseRepository, IADS999004Repository
    {
        public ADS999004Repository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Ads999004ListViewModel> Data, int RecordsFiltered, int RecordsTotal)>
            GetPagedListAsync(Ads999004DataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            var searchParam = new SqlParameter("@searchValue", string.IsNullOrEmpty(searchValue) ? (object)DBNull.Value : $"%{searchValue}%");
            var startParam = new SqlParameter("@start", request.start);
            var lengthParam = new SqlParameter("@length", request.length);

            var baseWhere = "WHERE arg_status = 1 ";
            var totalCountSql = $"SELECT COUNT(1) FROM arguments {baseWhere}";
            var recordsTotal = (await _context.Database.SqlQueryRaw<int>(totalCountSql).ToListAsync()).Single();

            var searchWhere = new StringBuilder(baseWhere);
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchWhere.Append(" AND (arg_variable LIKE @searchValue OR arg_describe LIKE @searchValue) ");
            }

            var filteredCountSql = $"SELECT COUNT(1) FROM arguments {searchWhere}";
            var recordsFiltered = (await _context.Database.SqlQueryRaw<int>(filteredCountSql, searchParam).ToListAsync()).Single();

            var sortColumn = "agr_group, arg_order";
            if (request.orderby != null && request.orderby.Any())
            {
                var order = request.orderby[0];
                sortColumn = order.column switch
                {
                    0 => "arg_describe",
                    1 => "arg_variable",
                    6 => "arg_modifytime",
                    _ => "agr_group, arg_order"
                };
                sortColumn += (order.dir == "desc" ? " DESC" : " ASC");
            }

            var dataSql = $@"
                SELECT 
                    arg_variable AS ArgVariable, 
                    arg_describe AS ArgDescribe, 
                    arg_value AS ArgValue, 
                    arg_defaultvalue AS ArgDefaultValue,
                    CASE WHEN arg_openmanager = 1 THEN '是' ELSE '否' END AS ArgOpenManagerDisplay,
                    arg_modifyname AS ArgModifyName,
                    arg_modifytime AS ArgModifyTime
                FROM arguments
                {searchWhere}
                ORDER BY {sortColumn}
                OFFSET @start ROWS FETCH NEXT @length ROWS ONLY";

            var data = await _context.Database.SqlQueryRaw<Ads999004ListViewModel>(dataSql, searchParam, startParam, lengthParam).ToListAsync();
            return (data, recordsFiltered, recordsTotal);
        }

        public async Task<List<ArgumentsGroup>> GetGroupListAsync()
        {
            return await _context.ArgumentsGroup.OrderBy(x => x.AgrGroup).ToListAsync();
        }

        public async Task<Arguments> GetByVariableAsync(string variable)
        {
            return await _context.Arguments.AsNoTracking().FirstOrDefaultAsync(x => x.ArgVariable == variable && x.ArgStatus == 1);
        }

        public async Task<bool> VariableExistsAsync(string variable)
        {
            return await _context.Arguments.AnyAsync(x => x.ArgVariable == variable && x.ArgStatus == 1);
        }

        public async Task AddAsync(Arguments entity, IDataLogger dataLogger)
        {
            await _context.Arguments.AddAsync(entity);
            await SaveChangesAsync(dataLogger);
        }

        public async Task UpdateAsync(Arguments entity, IDataLogger dataLogger)
        {
            var existing = await _context.Arguments.FirstOrDefaultAsync(x => x.ArgVariable == entity.ArgVariable);
            if (existing == null) return;

            _context.Entry(existing).CurrentValues.SetValues(entity);
            await SaveChangesAsync(dataLogger);
        }

        public async Task<bool> SoftDeleteAsync(string variable, string user, IDataLogger dataLogger)
        {
            var existing = await _context.Arguments.FirstOrDefaultAsync(x => x.ArgVariable == variable);
            if (existing == null) return false;

            existing.ArgStatus = 2;
            existing.ArgModifyName = user;
            existing.ArgModifyTime = DateTime.Now;

            var schedules = await _context.ArgumentsSchedule
                .Where(x => x.ArgVariable == variable && x.AgsStatus != 2)
                .ToListAsync();

            foreach (var s in schedules)
            {
                s.AgsStatus = 2;
                s.AgsModifyName = user;
                s.AgsModifyTime = DateTime.Now;
            }

            var depts = await _context.ArgumentsDept
                .Where(x => x.ArgVariable == variable && x.AgdStatus != 2)
                .ToListAsync();

            foreach (var d in depts)
            {
                d.AgdStatus = 2;
                d.AgdModifyName = user;
                d.AgdModifyTime = DateTime.Now;
            }

            await SaveChangesAsync(dataLogger);
            return true;
        }
    }
}