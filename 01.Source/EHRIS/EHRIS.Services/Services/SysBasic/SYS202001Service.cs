using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public class SYS202001Service : ISYS202001Service
    {
        private readonly ISYS202001Repository _repository;

        public SYS202001Service(ISYS202001Repository repository)
        {
            _repository = repository;
        }

        public async Task<DataTableResponse<DepartmentListViewModel>> GetDepartmentsForDataTableAsync(DepartmentDataTableRequest request)
        {
            var recordsTotal = await _repository.GetTotalDepartmentCountAsync();
            var (pagedData, recordsFiltered) = await _repository.GetDepartmentsFromSqlAsync(request);

            var data = pagedData.Select(d => new DepartmentListViewModel
            {
                deptId = d.DepNo,
                deptCode = d.DepDepId,
                deptName = d.DepName,
                deptOrder = d.DepOrder,
                deptModifyName = d.DepModifyName,
                deptModifyTime = d.DepModifyTime,
                dep_level = d.DepLevel
            }).ToList();

            return new DataTableResponse<DepartmentListViewModel>
            {
                draw = request.draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            };
        }

        public async Task<SYS202001EDTViewModel> GetEditDepartmentViewModelAsync(int id)
        {
            var department = await _repository.GetTrackedDepartmentByIdAsync(id);
            if (department == null) return null;

            string parentName = "無 (頂層)";
            if (department.DepParentId != 0)
            {
                var parentDept = await _repository.GetTrackedDepartmentByIdAsync(department.DepParentId);
                parentName = parentDept?.DepName ?? "未知";
            }

            return new SYS202001EDTViewModel
            {
                deptId = department.DepNo,
                deptCode = department.DepDepId,
                deptName = department.DepName,
                deptOrder = department.DepOrder,
                deptLevel = department.DepLevel,
                parentDeptName = parentName,
                parentId = department.DepParentId
            };
        }

        public async Task<DataTableResponse<DepartmentListViewModel>> GetSubDepartmentsForDataTableAsync(int parentId, DepartmentDataTableRequest request)
        {
            var (pagedData, recordsFiltered, recordsTotal) = await _repository.GetSubDepartmentsFromSqlAsync(parentId, request);
            var data = pagedData.Select(d => new DepartmentListViewModel
            {
                deptId = d.DepNo,
                deptCode = d.DepDepId,
                deptName = d.DepName,
                deptOrder = d.DepOrder,
                deptModifyName = d.DepModifyName,
                deptModifyTime = d.DepModifyTime,
                dep_level = d.DepLevel
            }).ToList();

            return new DataTableResponse<DepartmentListViewModel>
            {
                draw = request.draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            };
        }

        public async Task<(bool success, string message)> CreateDepartmentAsync(DepartmentUpdateModel model, IDataLogger dataLogger, string userName)
        {
            const int MAX_LEVEL = 100;
            var parentDept = await _repository.GetTrackedDepartmentByIdAsync(model.parentId);

            if (model.parentId != 0 && parentDept == null)
                return (false, "指定的上層部門不存在。");

            if (parentDept != null && parentDept.DepLevel >= MAX_LEVEL)
                return (false, $"無法新增，系統僅支援至 {MAX_LEVEL} 層部門。");

            if (await _repository.DepartmentCodeExistsAsync(model.deptCode))
                return (false, "有效的部門代碼已存在。");

            await _repository.CreateDepartmentAsync(model, userName, dataLogger);
            return (true, "部門新增成功！");
        }

        public async Task<(bool success, string message)> UpdateDepartmentAsync(DepartmentUpdateModel model, IDataLogger dataLogger, string userName)
        {
            if (string.Equals(model.deptCode?.Trim(), model.deptName?.Trim(), StringComparison.OrdinalIgnoreCase))
                return (false, "部門代碼與部門名稱不可相同！");

            var affected = await _repository.UpdateDepartmentAsync(model, userName, dataLogger);
            return affected > 0 ? (true, "部門資料更新成功！") : (false, "找不到要修改的部門資料！");
        }

        public async Task<(bool success, string message)> SoftDeleteDepartmentAsync(int id, IDataLogger dataLogger, string userName)
        {
            var affected = await _repository.SoftDeleteDepartmentAsync(id, userName, dataLogger);
            return affected > 0 ? (true, "部門及其所有子部門已刪除成功！") : (false, "找不到要刪除的部門資料！");
        }

        public async Task<DepartmentUpdateModel> GetDepartmentDetailsAsync(int id)
        {
            var dept = await _repository.GetTrackedDepartmentByIdAsync(id);
            if (dept == null) return null;

            return new DepartmentUpdateModel { deptId = dept.DepNo, deptCode = dept.DepDepId, deptName = dept.DepName, deptOrder = dept.DepOrder };
        }

        public async Task<string> GetSuggestedSubDeptCodeAsync(int parentId)
        {
            var parentDept = await _repository.GetTrackedDepartmentByIdAsync(parentId);
            if (parentDept == null) return "";

            string prefix = parentDept.DepDepId;
            var siblings = await _repository.GetSiblingDeptCodesAsync(parentId, prefix);

            int maxNumber = 0;
            foreach (var code in siblings)
            {
                if (code.Length <= prefix.Length) continue;
                string numberPart = code.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    if (currentNumber > maxNumber) maxNumber = currentNumber;
                }
            }
            return prefix + (maxNumber + 1).ToString("D2");
        }
    }
}