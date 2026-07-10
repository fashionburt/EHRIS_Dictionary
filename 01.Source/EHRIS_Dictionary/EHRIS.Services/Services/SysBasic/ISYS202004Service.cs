using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic; 

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS202004Service
    {
        Task<DataTablesResponse<SYS202004ListViewModel>> GetProfessPagedListAsync(DataTablesRequest request);
        Task<SYS202004EditViewModel> GetProfessForEditAsync(int proNo);
        Task<(bool success, string message)> CreateProfessAsync(SYS202004UpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> UpdateProfessAsync(SYS202004UpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> SoftDeleteProfessAsync(int proNo, IDataLogger dataLogger, string userName);
    }
}