using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EHRIS.Core.Repositories.SysBasic
{
    public class SYS202001Repository : BaseRepository, ISYS202001Repository
    {
        public SYS202001Repository(ApplicationDbContext context, AuditDbContext aduitContext) : base(context)
        {
        }

        #region 讀取與查詢

        public async Task<int> GetTotalDepartmentCountAsync()
        {
            return await _context.Department.CountAsync(x => x.DepStatus == "1" && x.DepLevel == 1);
        }

        public async Task<(IEnumerable<Departments> Data, int RecordsFiltered)> GetDepartmentsFromSqlAsync(DepartmentDataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            IQueryable<Departments> query = _context.Department.Where(d => d.DepStatus == "1" && d.DepLevel == 1);

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(d => d.DepDepId.Contains(searchValue) || d.DepName.Contains(searchValue) || d.DepModifyName.Contains(searchValue));
            }

            int recordsFiltered = await query.CountAsync();

            if (request.orderby != null && request.orderby.Any())
            {
                var sortInfo = request.orderby.First();
                var colIndex = sortInfo.column;
                var colData = request.columns[colIndex].data;
                var isAsc = sortInfo.dir.ToLower() == "asc";

                var sortMap = new Dictionary<string, Func<IQueryable<Departments>, bool, IOrderedQueryable<Departments>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "deptCode", (q, asc) => asc ? q.OrderBy(x => x.DepDepId) : q.OrderByDescending(x => x.DepDepId) },
            { "deptName", (q, asc) => asc ? q.OrderBy(x => x.DepName) : q.OrderByDescending(x => x.DepName) },
            { "deptOrder", (q, asc) => asc ? q.OrderBy(x => x.DepOrder) : q.OrderByDescending(x => x.DepOrder) },
            { "deptModifyName", (q, asc) => asc ? q.OrderBy(x => x.DepModifyName) : q.OrderByDescending(x => x.DepModifyName) },
            { "deptModifyTime", (q, asc) => asc ? q.OrderBy(x => x.DepModifyTime) : q.OrderByDescending(x => x.DepModifyTime) }
        };

                if (sortMap.ContainsKey(colData))
                {
                    query = sortMap[colData](query, isAsc);
                }
                else
                {
                    query = query.OrderBy(d => d.DepOrder).ThenBy(d => d.DepDepId);
                }
            }
            else
            {
                query = query.OrderBy(d => d.DepOrder).ThenBy(d => d.DepDepId);
            }

            var data = await query.Skip(request.start).Take(request.length).ToListAsync();
            return (data, recordsFiltered);
        }

        public async Task<(IEnumerable<Departments> Data, int RecordsFiltered, int RecordsTotal)> GetSubDepartmentsFromSqlAsync(int parentId, DepartmentDataTableRequest request)
        {
            var searchValue = request.extraSearch?.searchValue;
            IQueryable<Departments> query = _context.Department.Where(d => d.DepParentId == parentId && d.DepStatus == "1");
            int recordsTotal = await _context.Department.CountAsync(d => d.DepParentId == parentId && d.DepStatus == "1");

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(d => d.DepDepId.Contains(searchValue) || d.DepName.Contains(searchValue) || d.DepModifyName.Contains(searchValue));
            }

            int recordsFiltered = await query.CountAsync();

            if (request.orderby != null && request.orderby.Any())
            {
                var sortInfo = request.orderby.First();
                var colData = request.columns[sortInfo.column].data;
                var isAsc = sortInfo.dir.ToLower() == "asc";

                var sortMap = new Dictionary<string, Func<IQueryable<Departments>, bool, IOrderedQueryable<Departments>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "deptCode", (q, asc) => asc ? q.OrderBy(x => x.DepDepId) : q.OrderByDescending(x => x.DepDepId) },
            { "deptName", (q, asc) => asc ? q.OrderBy(x => x.DepName) : q.OrderByDescending(x => x.DepName) },
            { "deptOrder", (q, asc) => asc ? q.OrderBy(x => x.DepOrder) : q.OrderByDescending(x => x.DepOrder) },
            { "deptModifyName", (q, asc) => asc ? q.OrderBy(x => x.DepModifyName) : q.OrderByDescending(x => x.DepModifyName) },
            { "deptModifyTime", (q, asc) => asc ? q.OrderBy(x => x.DepModifyTime) : q.OrderByDescending(x => x.DepModifyTime) }
        };

                query = sortMap.ContainsKey(colData) ? sortMap[colData](query, isAsc) : query.OrderBy(d => d.DepOrder).ThenBy(d => d.DepDepId);
            }
            else
            {
                query = query.OrderBy(d => d.DepOrder).ThenBy(d => d.DepDepId);
            }

            var data = await query.Skip(request.start).Take(request.length).ToListAsync();
            return (data, recordsFiltered, recordsTotal);
        }

        public async Task<List<string>> GetSiblingDeptCodesAsync(int parentId, string prefix)
        {
            return await _context.Department
                .Where(x => x.DepParentId == parentId && x.DepDepId.StartsWith(prefix))
                .Select(x => x.DepDepId)
                .ToListAsync();
        }

        public async Task<bool> DepartmentCodeExistsAsync(string code)
        {
            return await _context.Department.AnyAsync(x => x.DepDepId == code && x.DepStatus == "1");
        }

        public async Task<int> GetNextUdeNoAsync(string uniId)
        {
            var maxNo = await _context.UnitDepart
                .Where(x => x.UniId == uniId)
                .MaxAsync(x => (int?)x.UdeNo) ?? 0;
            return maxNo + 1;
        }

        #endregion

        #region 實體追蹤 (用於異動)

        public async Task<Departments> GetTrackedDepartmentByIdAsync(int id)
        {
            return await _context.Department.FirstOrDefaultAsync(x => x.DepNo == id);
        }

        public async Task<Unit> GetTrackedUnitByUniIdAsync(string uniId)
        {
            return await _context.Unit.FirstOrDefaultAsync(x => x.UniId == uniId);
        }

        public async Task<UnitDepart> GetTrackedUnitDepartByDepNoAsync(int depNo)
        {
            return await _context.UnitDepart.FirstOrDefaultAsync(x => x.UdeDepNo == depNo);
        }

        public async Task<Departments> GetTrackedDepartmentByCodeAsync(string code)
        {
            return await _context.Department.FirstOrDefaultAsync(x => x.DepDepId == code);
        }
        public async Task<List<Departments>> GetAllDescendantDepartmentsAsync(int parentId)
        {
            var result = new List<Departments>();
            var directChildren = await _context.Department
                .Where(x => x.DepParentId == parentId && x.DepStatus == "1")
                .ToListAsync();

            foreach (var child in directChildren)
            {
                result.Add(child);
                var descendants = await GetAllDescendantDepartmentsAsync(child.DepNo);
                result.AddRange(descendants);
            }

            return result;
        }

        #endregion

        #region 實體異動邏輯 (支援 DataLogger)

        public async Task CreateDepartmentAsync(DepartmentUpdateModel model, string userName, IDataLogger dataLogger)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var currentTime = DateTime.Now;
                    string effectiveUniId;

                    var parentDept = await GetTrackedDepartmentByIdAsync(model.parentId);
                    int newLevel = (parentDept?.DepLevel ?? 0) + 1;

                    if (parentDept == null || parentDept.DepLevel == 0)
                    {
                        effectiveUniId = model.deptCode;
                        var unit = await GetTrackedUnitByUniIdAsync(effectiveUniId);
                        if (unit == null)
                        {
                            unit = new Unit { UniId = effectiveUniId, UniName = model.deptName, UniStatus = "1", UniOrder = model.deptOrder ?? 0, UniCreateName = userName, UniCreateTime = currentTime, UniModifyName = userName, UniModifyTime = currentTime };
                            await _context.Unit.AddAsync(unit);
                        }
                    }
                    else
                    {
                        effectiveUniId = parentDept.UniId;
                    }

                    int nextUdeNo = await GetNextUdeNoAsync(effectiveUniId);
                    var newDept = new Departments
                    {
                        DepDepId = model.deptCode,
                        DepName = model.deptName,
                        DepParentId = model.parentId,
                        DepLevel = newLevel,
                        DepOrder = model.deptOrder ?? 0,
                        UniId = effectiveUniId,
                        UdeNo = nextUdeNo,
                        DepStatus = "1",
                        DepCode = "2",
                        DepIntroduce = "",
                        DepCreateTime = currentTime,
                        DepModifyTime = currentTime,
                        DepCreateName = userName,
                        DepModifyName = userName
                    };

                    await _context.Department.AddAsync(newDept);
                    await _context.SaveChangesAsync();

                    var newUnitDepart = new UnitDepart
                    {
                        UniId = effectiveUniId,
                        UdeNo = nextUdeNo,
                        UdeDepNo = newDept.DepNo,
                        UdeOrder = model.deptOrder ?? 0,
                        UdeStatus = "1",
                        UdeCreateName = userName,
                        UdeCreateTime = currentTime,
                        UdeModifyName = userName,
                        UdeModifyTime = currentTime
                    };
                    await _context.UnitDepart.AddAsync(newUnitDepart);

                    await SaveChangesAsync(dataLogger);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<int> UpdateDepartmentAsync(DepartmentUpdateModel model, string userName, IDataLogger dataLogger)
        {
            var dept = await GetTrackedDepartmentByIdAsync(model.deptId);
            if (dept == null) return 0;

            var currentTime = DateTime.Now;

            dept.DepName = model.deptName;
            dept.DepOrder = model.deptOrder ?? 0;
            dept.DepModifyName = userName;
            dept.DepModifyTime = currentTime;

            if (dept.DepLevel == 1)
            {
                var unit = await GetTrackedUnitByUniIdAsync(dept.DepDepId);
                if (unit != null)
                {
                    unit.UniName = model.deptName;
                    unit.UniOrder = model.deptOrder ?? 0;
                    unit.UniModifyName = userName;
                    unit.UniModifyTime = currentTime;
                }
            }

            var ude = await GetTrackedUnitDepartByDepNoAsync(model.deptId);
            if (ude != null)
            {
                ude.UdeOrder = model.deptOrder ?? 0;
                ude.UdeModifyName = userName;
                ude.UdeModifyTime = currentTime;
            }

            return await SaveChangesAsync(dataLogger);
        }

        public async Task<int> SoftDeleteDepartmentAsync(int id, string userName, IDataLogger dataLogger)
        {
            var dept = await GetTrackedDepartmentByIdAsync(id);
            if (dept == null) return 0;

            var currentTime = DateTime.Now;

            dept.DepStatus = "2";
            dept.DepModifyName = userName;
            dept.DepModifyTime = currentTime;

            var ude = await GetTrackedUnitDepartByDepNoAsync(id);
            if (ude != null)
            {
                ude.UdeStatus = "2";
                ude.UdeModifyName = userName;
                ude.UdeModifyTime = currentTime;
            }

            if (dept.DepLevel == 1)
            {
                var unit = await GetTrackedUnitByUniIdAsync(dept.DepDepId);
                if (unit != null)
                {
                    unit.UniStatus = "2";
                    unit.UniModifyName = userName;
                    unit.UniModifyTime = currentTime;
                }
            }

            var descendants = await GetAllDescendantDepartmentsAsync(id);
            foreach (var child in descendants)
            {
                child.DepStatus = "2";
                child.DepModifyName = userName;
                child.DepModifyTime = currentTime;

                var childUde = await GetTrackedUnitDepartByDepNoAsync(child.DepNo);
                if (childUde != null)
                {
                    childUde.UdeStatus = "2";
                    childUde.UdeModifyName = userName;
                    childUde.UdeModifyTime = currentTime;
                }

                if (child.DepLevel == 1)
                {
                    var childUnit = await GetTrackedUnitByUniIdAsync(child.DepDepId);
                    if (childUnit != null)
                    {
                        childUnit.UniStatus = "2";
                        childUnit.UniModifyName = userName;
                        childUnit.UniModifyTime = currentTime;
                    }
                }
            }

            return await SaveChangesAsync(dataLogger);
        }


        public async Task<Departments> GetDepartmentByIdAsync(int id)
        {
            return await _context.Department.AsNoTracking().FirstOrDefaultAsync(x => x.DepNo == id);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }

        #endregion
    }
}