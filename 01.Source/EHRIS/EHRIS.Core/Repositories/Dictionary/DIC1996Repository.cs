using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.Dictionary;

public class DIC1996Repository : BaseRepository, IDIC1996Repository
{
    public DIC1996Repository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DataTableResponse<DIC1996ViewModel>> GetPagedListAsync(DIC1996Request request)
    {
        var query = _context.MarqueeAnnouncements.AsNoTracking();

        if (!string.IsNullOrEmpty(request.extraSearch?.searchValue))
        {
            var k = request.extraSearch.searchValue;
            query = query.Where(x => x.Message.Contains(k));
        }

        if (request.orderby != null && request.orderby.Any())
        {
            var sort = request.orderby[0];
            var col = request.columns[sort.column].data;
            var dir = sort.dir == "asc";

            query = col switch
            {
                "priority" => dir ? query.OrderBy(x => x.Priority) : query.OrderByDescending(x => x.Priority),
                "startDate_Text" => dir ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate),
                "endDate_Text" => dir ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate),
                _ => query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.StartDate)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.StartDate);
        }

        var total = await query.CountAsync();
        var data = await query
                    .Skip(request.start).Take(request.length)
                    .Select(x => new DIC1996ViewModel
                    {
                        Id = x.Id,
                        Message = x.Message ?? string.Empty,
                        IsEnabled = x.IsEnabled,
                        Priority = x.Priority,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate
                    }).ToListAsync();

        return new DataTableResponse<DIC1996ViewModel> { draw = request.draw, recordsTotal = total, recordsFiltered = total, data = data };
    }

    public async Task<MarqueeAnnouncement?> GetByIdAsync(int id) => await _context.MarqueeAnnouncements.FindAsync(id);

    public async Task<bool> AddAsync(MarqueeAnnouncement entity, string detail, IDataLogger dataLogger)
    {
        await _context.MarqueeAnnouncements.AddAsync(entity);
        _context.Logs.Add(new Log
        {
            DbKey = "",
            ServerIP = "",
            TableName = "marqueeannouncement",
            PkName = "",
            State = 10,
            Detail = detail,
            Date = DateTime.Now
        });
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> UpdateAsync(MarqueeAnnouncement entity, string detail, IDataLogger dataLogger)
    {
        _context.Logs.Add(new Log
        {
            DbKey = "",
            ServerIP = "",
            TableName = "marqueeannouncement",
            PkName = "",
            State = 20,
            Detail = detail,
            Date = DateTime.Now
        });
        _context.MarqueeAnnouncements.Update(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int id, string detail, IDataLogger dataLogger)
    {
        var entity = await _context.MarqueeAnnouncements.FindAsync(id);
        if (entity == null) return false;

        _context.Logs.Add(new Log
        {
            DbKey = "",
            ServerIP = "",
            TableName = "marqueeannouncement",
            PkName = "",
            State = 30,
            Detail = detail,
            Date = DateTime.Now
        });

        entity.IsEnabled = false;
        _context.MarqueeAnnouncements.Update(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }
}