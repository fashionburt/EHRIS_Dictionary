using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EHRIS.Core.Repositories.Dictionary;

public class DIC1998Repository : BaseRepository, IDIC1998Repository
{
    public DIC1998Repository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<DIC1998GroupViewModel>> GetGroupedListAsync(string clientIp, string serverIp, int? menuId)
    {
        var query = from ma in _context.Menu_Access.AsNoTracking()
                    join m in _context.Menus.AsNoTracking() on ma.MenuId equals m.MenuId
                    where ma.IsEnabled != 2
                    select new { ma, m };

        if (!string.IsNullOrEmpty(clientIp))
        {
            query = query.Where(x => x.ma.ClientIp == clientIp);
        }

        if (!string.IsNullOrEmpty(serverIp))
        {
            query = query.Where(x => x.m.ServerIP == serverIp);
        }

        if (menuId.HasValue && menuId.Value > 0)
        {
            query = query.Where(x => x.ma.MenuId == menuId.Value);
        }

        var rawData = await query.Select(x => new
        {
            x.ma.AccessId,
            x.ma.ClientIp,
            x.ma.CreateDate,
            x.m.MenuName,
            x.m.ServerIP
        }).ToListAsync();

        var grouped = rawData.GroupBy(x => new { x.ClientIp, x.ServerIP })
                             .Select(g => new DIC1998GroupViewModel
                             {
                                 ClientIp = g.Key.ClientIp,
                                 ServerIp = g.Key.ServerIP,
                                 Menus = g.Select(m => new AuthorizedMenuDto
                                 {
                                     AccessId = m.AccessId,
                                     MenuName = m.MenuName
                                 }).ToList(),
                                 CreateDate = g.Max(m => m.CreateDate).ToString("yyyy-MM-dd HH:mm:ss")
                             }).ToList();

        return grouped;
    }

    public async Task<DataTableResponse<DIC1998ViewModel>> GetPagedListAsync(DataTableRequest request)
    {
        var query = from ma in _context.Menu_Access.AsNoTracking()
                    join m in _context.Menus.AsNoTracking() on ma.MenuId equals m.MenuId
                    select new DIC1998ViewModel
                    {
                        AccessId = ma.AccessId,
                        ClientIp = ma.ClientIp,
                        MenuId = ma.MenuId,
                        MenuName = m.MenuName,
                        ServerIp = m.ServerIP,
                        IsEnabled = ma.IsEnabled,
                        CreateDate = ma.CreateDate
                    };

        var totalCount = await query.CountAsync();

        var pagedData = await query
            .OrderByDescending(a => a.CreateDate)
            .Skip(request.start)
            .Take(request.length)
            .ToListAsync();

        return new DataTableResponse<DIC1998ViewModel>
        {
            draw = request.draw,
            recordsTotal = totalCount,
            recordsFiltered = totalCount,
            data = pagedData
        };
    }

    public async Task<Menu_Access?> GetByIdAsync(int id)
    {
        return await _context.Menu_Access.FindAsync(id);
    }

    public async Task<bool> AnyAsync(Expression<Func<Menu_Access, bool>> predicate)
    {
        return await _context.Menu_Access.AnyAsync(predicate);
    }

    public async Task<bool> AddAsync(Menu_Access entity, string detail, IDataLogger dataLogger)
    {
        await _context.Menu_Access.AddAsync(entity);
        _context.Logs.Add(new Log
        {
            DbKey = entity.ClientIp,
            ServerIP = "",
            TableName = "",
            PkName = "",
            State = 10,
            Detail = detail,
            Date = DateTime.Now
        });
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> UpdateAsync(Menu_Access entity, string detail, IDataLogger dataLogger)
    {
        _context.Logs.Add(new Log
        {
            DbKey = entity.ClientIp,
            ServerIP = "",
            TableName = "",
            PkName = "",
            State = 20,
            Detail = detail,
            Date = DateTime.Now
        });
        _context.Menu_Access.Update(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> DeleteByIdAsync(int id, string detail, IDataLogger dataLogger)
    {
        var entity = await _context.Menu_Access.FindAsync(id);
        if (entity == null) return false;

        _context.Logs.Add(new Log
        {
            DbKey = entity.ClientIp,
            ServerIP = "",
            TableName = "",
            PkName = "",
            State = 30,
            Detail = detail,
            Date = DateTime.Now
        });

        entity.IsEnabled = 2;
        _context.Menu_Access.Update(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<List<Menu>> GetAvailableMenusAsync(string serverIp)
    {
        return await _context.Menus
            .AsNoTracking()
            .Where(m => m.ServerIP == serverIp && m.IsEnabled == 1)
            .OrderBy(m => m.MenuName)
            .ToListAsync();
    }
}