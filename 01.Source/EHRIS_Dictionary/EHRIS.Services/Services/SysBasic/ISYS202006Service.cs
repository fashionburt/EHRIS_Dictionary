using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS202006Service
    {
        Task<DataTablesResponse<SYS202006ListViewModel>> GetHolidayPagedListAsync(DataTablesRequest request);
        Task<SYS202006EditViewModel> GetHolidayForEditAsync(int holNo);
        Task<(bool success, string message)> CreateHolidayAsync(SYS202006UpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> UpdateHolidayAsync(SYS202006UpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> SoftDeleteHolidayAsync(int holNo, IDataLogger dataLogger, string userName);
    }
}