using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS202003Service
    {
        Task<DataTablesResponse<SYS202003ListViewModel>> GetPTypePagedListAsync(DataTablesRequest request);
        Task<SYS202003EditViewModel> GetPTypeForEditAsync(int ptyNo);
        Task<(bool success, string message)> CreatePTypeAsync(PTypeUpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> UpdatePTypeAsync(PTypeUpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> SoftDeletePTypeAsync(int ptyNo, IDataLogger dataLogger, string userName);
    }
}