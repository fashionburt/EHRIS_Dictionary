using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public interface ISYS202001Service
    {
        Task<DataTableResponse<DepartmentListViewModel>> GetDepartmentsForDataTableAsync(DepartmentDataTableRequest request);
        Task<DataTableResponse<DepartmentListViewModel>> GetSubDepartmentsForDataTableAsync(int parentId, DepartmentDataTableRequest request);
        Task<SYS202001EDTViewModel> GetEditDepartmentViewModelAsync(int id);
        Task<(bool success, string message)> CreateDepartmentAsync(DepartmentUpdateModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> UpdateDepartmentAsync(DepartmentUpdateModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> SoftDeleteDepartmentAsync(int id, IDataLogger dataLogger, string userName);
        Task<DepartmentUpdateModel> GetDepartmentDetailsAsync(int id);
        Task<string> GetSuggestedSubDeptCodeAsync(int parentId);
    }
}