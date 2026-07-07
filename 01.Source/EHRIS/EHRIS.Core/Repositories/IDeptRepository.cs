using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Repositories;

public interface IDeptRepository
{
    Task<List<DepartmentTree>> GetAllDepartment();
    Task<List<DepartmentTree>> GetAllUnitDepartment(int AccNo);
    Task<List<UnitTree>> GetAllUnit(int AccNo);
    Task<List<string>> GetDepartmentNameListAsync(List<int> depNoList);
    Task<List<string>> GetDepartmentNameByUniIdListAsync(List<string> uniIdList);
}
