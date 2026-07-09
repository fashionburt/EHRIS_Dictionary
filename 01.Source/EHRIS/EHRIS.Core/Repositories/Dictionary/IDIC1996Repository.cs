using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Core.Repositories.Dictionary;

public interface IDIC1996Repository
{
    Task<DataTableResponse<DIC1996ViewModel>> GetPagedListAsync(DIC1996Request request);
    Task<MarqueeAnnouncement?> GetByIdAsync(int id);
    Task<bool> AddAsync(MarqueeAnnouncement entity, string detail, IDataLogger dataLogger);
    Task<bool> UpdateAsync(MarqueeAnnouncement entity, string detail, IDataLogger dataLogger);
    Task<bool> SoftDeleteAsync(int id, string detail, IDataLogger dataLogger);
}