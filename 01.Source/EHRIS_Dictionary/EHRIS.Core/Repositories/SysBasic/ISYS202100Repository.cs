using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Repositories
{
    public interface ISYS202100Repository : IBaseRepository
    {
        Task<(IEnumerable<Sys202100ListViewModel> Data, int RecordsFiltered, int RecordsTotal)> GetPagedListAsync(Sys202100DataTableRequest request);
        Task<Arguments> GetByVariableAsync(string argVariable);
        Task<bool> UpdateAsync(Arguments entity, IDataLogger dataLogger);
        Task<IEnumerable<Sys202100DeptListViewModel>> GetDeptListAsync(string argVariable);
        Task<bool> UpdateDeptAsync(ArgumentsDept deptEntity, IDataLogger dataLogger);
        Task<bool> SoftDeleteDeptAsync(int agdNo, string modifyUser, IDataLogger dataLogger);
        Task<IEnumerable<Sys202100DepOption>> GetDepartmentOptionsAsync();
        Task<IEnumerable<Sys202100SchedViewModel>> GetSchedListAsync(string argVariable, int depNo);
        Task<bool> UpdateSchedAsync(ArgumentsSchedule scheduleEntity, IDataLogger dataLogger);
        Task<bool> SoftDeleteSchedAsync(int agsNo, string modifyUser, IDataLogger dataLogger);
        Task<bool> SoftDeleteSchedByAgdNoAsync(int agdNo, string modifyUser, IDataLogger dataLogger);
    }
}