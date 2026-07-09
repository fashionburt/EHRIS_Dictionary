using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using System.Linq.Expressions;

namespace EHRIS.Core.Repositories.Dictionary;

public interface IDIC1998Repository
{
    Task<DataTableResponse<DIC1998ViewModel>> GetPagedListAsync(DataTableRequest request);
    Task<Menu_Access?> GetByIdAsync(int id);
    Task<Menu_Access?> GetDeletedRecordAsync(string clientIp, int menuId);
    Task<bool> AnyAsync(Expression<Func<Menu_Access, bool>> predicate);
    Task<bool> AddAsync(Menu_Access entity, string detail, IDataLogger dataLogger);
    Task<bool> UpdateAsync(Menu_Access entity, string detail, IDataLogger dataLogger);
    Task<bool> DeleteByIdAsync(int id, string detail, IDataLogger dataLogger);
    Task<List<Menu>> GetAvailableMenusAsync(string serverIp);

    Task<List<DIC1998GroupViewModel>> GetGroupedListAsync(string clientIp, string serverIp, int? menuId);
}