using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.EntityFrameworkCore.Storage;

namespace EHRIS.Core.Repositories.SysBasic
{
    public interface ISYS202001Repository : IBaseRepository
    {
        Task<int> GetTotalDepartmentCountAsync();
        Task<(IEnumerable<Departments> Data, int RecordsFiltered)> GetDepartmentsFromSqlAsync(DepartmentDataTableRequest request);
        Task<(IEnumerable<Departments> Data, int RecordsFiltered, int RecordsTotal)> GetSubDepartmentsFromSqlAsync(int parentId, DepartmentDataTableRequest request);
        Task<List<string>> GetSiblingDeptCodesAsync(int parentId, string prefix);
        Task<bool> DepartmentCodeExistsAsync(string code);
        Task<int> GetNextUdeNoAsync(string uniId);

        Task<Departments> GetTrackedDepartmentByIdAsync(int id);
        Task<Unit> GetTrackedUnitByUniIdAsync(string uniId);
        Task<UnitDepart> GetTrackedUnitDepartByDepNoAsync(int depNo);
        Task<Departments> GetTrackedDepartmentByCodeAsync(string code);

        Task CreateDepartmentAsync(DepartmentUpdateModel model, string userName, IDataLogger dataLogger);
        Task<int> UpdateDepartmentAsync(DepartmentUpdateModel model, string userName, IDataLogger dataLogger);
        Task<int> SoftDeleteDepartmentAsync(int id, string userName, IDataLogger dataLogger);

        Task<IDbContextTransaction> BeginTransactionAsync();
        IExecutionStrategy CreateExecutionStrategy();
        Task<List<Departments>> GetAllDescendantDepartmentsAsync(int parentId);
    }
}