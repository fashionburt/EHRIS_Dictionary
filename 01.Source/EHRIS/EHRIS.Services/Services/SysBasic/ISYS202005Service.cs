using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services
{
    public interface ISYS202005Service
    {
        Task<(IEnumerable<Sys202005ListViewModel> Data, int RecordsFiltered, int RecordsTotal)>GetListAsync(Sys202005DataTableRequest request);

        Task<Sys202005EditViewModel> GetByNoAsync(int pleNo);

        Task<(bool success, string message)> AddAsync(Sys202005EditViewModel model, string user, IDataLogger dataLogger);

        Task<(bool success, string message)> UpdateAsync(Sys202005EditViewModel model, string user, IDataLogger dataLogger);

        Task<(bool success, string message)> DeleteAsync(int pleNo, string user, IDataLogger dataLogger);
    }
}