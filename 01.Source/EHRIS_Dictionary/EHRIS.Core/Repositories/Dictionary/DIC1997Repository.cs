using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.Dictionary;

public class DIC1997Repository : BaseRepository, IDIC1997Repository
{
    public DIC1997Repository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DataTableResponse<DIC1997ViewModel>> GetPagedListAsync(DIC1997Request request, string serverIp)
    {
        var query = _context.Logs.AsNoTracking()
                        .Where(x => x.ServerIP.Trim() == serverIp.Trim());

        if (!string.IsNullOrEmpty(request.DbKey)) query = query.Where(x => x.DbKey == request.DbKey);
        if (!string.IsNullOrEmpty(request.TableName)) query = query.Where(x => x.TableName == request.TableName);
        if (!string.IsNullOrEmpty(request.PkName)) query = query.Where(x => x.PkName == request.PkName);
        if (!string.IsNullOrEmpty(request.State) && int.TryParse(request.State, out int stateInt)) query = query.Where(x => x.State == stateInt);

        if (!string.IsNullOrEmpty(request.extraSearch?.searchValue))
        {
            var k = request.extraSearch.searchValue;
            query = query.Where(x => x.Detail.Contains(k) || x.DbKey.Contains(k) || x.TableName.Contains(k));
        }

        if (request.orderby != null && request.orderby.Any())
        {
            var sort = request.orderby[0];
            var col = request.columns[sort.column].data;
            var dir = sort.dir == "asc";

            query = col switch
            {
                "dbKey" => dir ? query.OrderBy(x => x.DbKey) : query.OrderByDescending(x => x.DbKey),
                "tableName" => dir ? query.OrderBy(x => x.TableName) : query.OrderByDescending(x => x.TableName),
                "pkName" => dir ? query.OrderBy(x => x.PkName) : query.OrderByDescending(x => x.PkName),
                "stateText" => dir ? query.OrderBy(x => x.State) : query.OrderByDescending(x => x.State),
                "detail" => dir ? query.OrderBy(x => x.Detail) : query.OrderByDescending(x => x.Detail),
                "dateText" => dir ? query.OrderBy(x => x.Date) : query.OrderByDescending(x => x.Date),
                _ => query.OrderByDescending(x => x.Date)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.Date);
        }

        var total = await query.CountAsync();
        var data = await query
            .Skip(request.start).Take(request.length)
            .Select(x => new DIC1997ViewModel
            {
                LogId = x.LogId,
                DbKey = x.DbKey,
                TableName = x.TableName,
                PkName = x.PkName,
                State = x.State.ToString(),
                Detail = x.Detail,
                Date = x.Date,
                ServerIP = x.ServerIP
            }).ToListAsync();

        return new DataTableResponse<DIC1997ViewModel> { draw = request.draw, recordsTotal = total, recordsFiltered = total, data = data };
    }

    public async Task<List<string>> GetDbKeysAsync(string serverIp) =>
            await _context.Logs
                .Where(x => x.ServerIP == serverIp && x.DbKey != null && x.DbKey != "" && x.DbKey != " ")
                .Select(x => x.DbKey!)
                .Distinct()
                .ToListAsync();

    public async Task<List<string>> GetTableNamesAsync(string serverIp, string dbKey, string? pkName = null)
    {
        var q = _context.Logs.Where(x => x.ServerIP == serverIp && x.DbKey == dbKey && x.TableName != null && x.TableName != "" && x.TableName != " ");
        if (!string.IsNullOrEmpty(pkName)) q = q.Where(x => x.PkName == pkName);
        return await q.Select(x => x.TableName!).Distinct().ToListAsync();
    }

    public async Task<List<string>> GetPkNamesAsync(string serverIp, string dbKey, string? tableName = null)
    {
        var q = _context.Logs.Where(x => x.ServerIP == serverIp && x.DbKey == dbKey && x.PkName != null && x.PkName != "" && x.PkName != " ");
        if (!string.IsNullOrEmpty(tableName)) q = q.Where(x => x.TableName == tableName);
        return await q.Select(x => x.PkName!).Distinct().ToListAsync();
    }

    public async Task<(bool success, string message)> UpdateFieldsAsync(string serverIp, string dbKey, string tableName, List<DIC1997UpdateModel> updates, IDataLogger dataLogger)
    {
        try
        {
            foreach (var item in updates)
            {
                var row = await _context.Rows.FirstOrDefaultAsync(x => x.RowId == item.RowId && x.ServerIP == serverIp);
                if (row != null)
                {
                    row.RowDesc = item.RowDesc;
                    row.RowRemark = item.RowRemark;
                }
            }
            await SaveChangesAsync(dataLogger);
            return (true, "更新完成");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}