using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS202100Service
    {
        Task<(IEnumerable<Sys202100ListViewModel> Data, int RecordsFiltered, int RecordsTotal)> GetListAsync(Sys202100DataTableRequest request);
        Task<Sys202100EditViewModel> GetByVariableAsync(string argVariable);
        Task<(bool success, string message)> UpdateAsync(Sys202100EditViewModel model, string user, IDataLogger dataLogger);
        Task<IEnumerable<Sys202100DeptListViewModel>> GetDeptListAsync(string argVariable);
        Task<(bool success, string message)> UpdateDeptAsync(Sys202100DeptListViewModel model, string user, IDataLogger dataLogger);
        Task<(bool success, string message)> DeleteDeptAsync(int agdNo, string user, IDataLogger dataLogger);
        Task<IEnumerable<Sys202100DepOption>> GetDepartmentOptionsAsync();
        Task<IEnumerable<Sys202100SchedViewModel>> GetSchedListAsync(string argVariable, int depNo);
        Task<(bool success, string message)> UpdateSchedAsync(Sys202100SchedViewModel model, string user, IDataLogger dataLogger);
        Task<(bool success, string message)> DeleteSchedAsync(int agsNo, string user, IDataLogger dataLogger);
        Task<(bool success, string message)> UpdateAllDeptAndSchedAsync(List<Sys202100DeptListViewModel> depts, List<Sys202100SchedViewModel> scheds, string user, IDataLogger dataLogger);
    }
}