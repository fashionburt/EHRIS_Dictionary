using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories
{
    public interface ISYS202005Repository : IBaseRepository
    {
        Task<(IEnumerable<Sys202005ListViewModel> Data, int RecordsFiltered, int RecordsTotal)>
            GetPagedListAsync(Sys202005DataTableRequest request);

        Task<HRPLevel> GetByNoAsync(int pleNo);

        Task<HRPLevel> GetTrackedByNoAsync(int pleNo);

        Task<bool> CodeExistsAsync(string pleCode, int? pleNo = null);

        Task<bool> AddAsync(HRPLevel entity, IDataLogger dataLogger);

        Task<bool> UpdateAsync(HRPLevel entity, IDataLogger dataLogger);

        Task<bool> SoftDeleteAsync(int pleNo, string modifyUser, IDataLogger dataLogger);
    }
}