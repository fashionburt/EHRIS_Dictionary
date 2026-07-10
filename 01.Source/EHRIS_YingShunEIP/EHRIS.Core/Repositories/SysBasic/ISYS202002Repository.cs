using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories.SysBasic
{
    public interface ISYS202002Repository : IBaseRepository
    {
        Task<IEnumerable<SYS202002TreeViewModel>> GetAllFunctionsForTreeAsync();
        Task<bool> FunctionExistsAsync(int sfuNo);
        Task<IEnumerable<int>> GetChildFunctionIdsAsync(int parentSfuNo);
        Task<IEnumerable<DropdownViewModel>> GetSystemsAsync();
        Task<IEnumerable<DropdownViewModel>> GetFunctionsBySystemAsync(int sysNo);
        Task<SYS202002CreateViewModel> GetFunctionByIdAsync(int sfuNo);
        Task<int> CreateFunctionAsync(SYS202002CreateViewModel model, string userName, IDataLogger dataLogger);

        Task<bool> UpdateFunctionAsync(SYS202002CreateViewModel model, string userName, IDataLogger dataLogger);

        Task<int> SoftDeleteFunctionsAsync(List<int> sfuNos, string userName, IDataLogger dataLogger);
        Task<int> UpdateChildrenStatusAsync(int sfuNo, byte status, string userName, IDataLogger dataLogger);
    }
}