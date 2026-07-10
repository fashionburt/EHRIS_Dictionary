using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories.SysBasic
{
    public interface ISYS202006Repository : IBaseRepository
    {
        Task<DataTablesResponse<SYS202006ListViewModel>> GetHolidayPagedListAsync(DataTablesRequest request);
        Task<HRHoliday> GetHolidayByIdAsync(int holNo);
        Task<bool> CodeExistsAsync(string code, int currentId);

        Task CreateHolidayAsync(SYS202006UpdateViewModel model, string userName, IDataLogger dataLogger);
        Task UpdateHolidayAsync(SYS202006UpdateViewModel model, string userName, IDataLogger dataLogger);
        Task SoftDeleteHolidayAsync(int holNo, IDataLogger dataLogger);
    }
}
