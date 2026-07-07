using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS202002Service
    {
        Task<IEnumerable<SYS202002TreeViewModel>> GetAllFunctionsForTreeAsync();
        Task<IEnumerable<DropdownViewModel>> GetSystemsAsync();
        Task<IEnumerable<DropdownViewModel>> GetFunctionsBySystemAsync(int sysNo);
        Task<(bool Success, string Message)> CreateFunctionAsync(SYS202002CreateViewModel model, IDataLogger dataLogger, string userName);
        Task<SYS202002CreateViewModel> GetFunctionByIdAsync(int sfuNo);
        Task<(bool Success, string Message)> UpdateFunctionAsync(SYS202002CreateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool Success, string Message)> SoftDeleteFunctionAsync(int sfuNo, IDataLogger dataLogger, string userName);
    }
}