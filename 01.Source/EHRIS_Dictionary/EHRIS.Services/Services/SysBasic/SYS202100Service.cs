using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories;
using EHRIS.Services.Common;
using EHRIS.Services.Services.SysBasic;

namespace EHRIS.Services.Services
{
    public class SYS202100Service : ISYS202100Service
    {
        private readonly ISYS202100Repository _repository;

        public SYS202100Service(ISYS202100Repository repository)
        {
            _repository = repository;
        }

        public async Task<(IEnumerable<Sys202100ListViewModel> Data, int RecordsFiltered, int RecordsTotal)> GetListAsync(Sys202100DataTableRequest request)
        {
            return await _repository.GetPagedListAsync(request);
        }

        public async Task<Sys202100EditViewModel> GetByVariableAsync(string argVariable)
        {
            var entity = await _repository.GetByVariableAsync(argVariable);
            if (entity == null) return null;

            return new Sys202100EditViewModel
            {
                ArgVariable = entity.ArgVariable ?? "",
                ArgDescribe = entity.ArgDescribe ?? ""
            };
        }

        public async Task<(bool success, string message)> UpdateAsync(Sys202100EditViewModel model, string user, IDataLogger dataLogger)
        {
            try
            {
                var entity = new Arguments
                {
                    ArgVariable = model.ArgVariable ?? "",
                    ArgDescribe = model.ArgDescribe ?? "",
                    ArgModifyName = user ?? "",
                    ArgModifyTime = DateTime.Now
                };

                var result = await _repository.UpdateAsync(entity, dataLogger);
                return result ? (true, "修改成功") : (false, "修改失敗");
            }
            catch (Exception ex)
            {
                return (false, "修改失敗: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Sys202100DeptListViewModel>> GetDeptListAsync(string argVariable)
        {
            return await _repository.GetDeptListAsync(argVariable);
        }

        public async Task<(bool success, string message)> UpdateDeptAsync(Sys202100DeptListViewModel model, string user, IDataLogger dataLogger)
        {
            try
            {
                var deptEntity = new ArgumentsDept
                {
                    AgdNo = model.AgdNo,
                    ArgVariable = model.ArgVariable ?? "",
                    DepNo = model.DepNo,
                    AgdValue = model.AgdValue ?? "",
                    AgdModifyName = user ?? "",
                    AgdModifyTime = DateTime.Now
                };

                var result = await _repository.UpdateDeptAsync(deptEntity, dataLogger);
                return result ? (true, "儲存成功") : (false, "儲存失敗");
            }
            catch (Exception ex)
            {
                return (false, "儲存失敗: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteDeptAsync(int agdNo, string user, IDataLogger dataLogger)
        {
            try
            {
                var result = await _repository.SoftDeleteDeptAsync(agdNo, user, dataLogger);
                if (result)
                {
                    await _repository.SoftDeleteSchedByAgdNoAsync(agdNo, user, dataLogger);
                }
                return result ? (true, "刪除成功") : (false, "刪除失敗");
            }
            catch (Exception ex)
            {
                return (false, "刪除失敗: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Sys202100DepOption>> GetDepartmentOptionsAsync()
        {
            return await _repository.GetDepartmentOptionsAsync();
        }

        public async Task<IEnumerable<Sys202100SchedViewModel>> GetSchedListAsync(string argVariable, int depNo)
        {
            return await _repository.GetSchedListAsync(argVariable, depNo);
        }

        public async Task<(bool success, string message)> UpdateSchedAsync(Sys202100SchedViewModel model, string user, IDataLogger dataLogger)
        {
            try
            {
                if (!model.AgsStartTime.HasValue || !model.AgsEndTime.HasValue)
                {
                    return (false, "請填寫完整的開始與結束時間");
                }

                if (model.AgsStartTime.Value >= model.AgsEndTime.Value)
                {
                    return (false, "結束時間必須大於開始時間");
                }

                var existingScheds = await _repository.GetSchedListAsync(model.ArgVariable, model.DepNo);
                foreach (var sched in existingScheds)
                {
                    if (sched.AgsNo != model.AgsNo)
                    {
                        if (model.AgsStartTime.Value < sched.AgsEndTime.Value && model.AgsEndTime.Value > sched.AgsStartTime.Value)
                        {
                            return (false, "排程設定時間不可重疊！");
                        }
                    }
                }

                var scheduleEntity = new ArgumentsSchedule
                {
                    AgsNo = model.AgsNo,
                    ArgVariable = model.ArgVariable ?? "",
                    DepNo = model.DepNo,
                    AgsValue = model.AgsValue ?? "",
                    AgsStartTime = model.AgsStartTime.Value,
                    AgsEndTime = model.AgsEndTime.Value,
                    AgsModifyName = user ?? "",
                    AgsModifyTime = DateTime.Now
                };

                var result = await _repository.UpdateSchedAsync(scheduleEntity, dataLogger);
                return result ? (true, "儲存成功") : (false, "儲存失敗");
            }
            catch (Exception ex)
            {
                return (false, "儲存失敗: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteSchedAsync(int agsNo, string user, IDataLogger dataLogger)
        {
            try
            {
                var result = await _repository.SoftDeleteSchedAsync(agsNo, user, dataLogger);
                return result ? (true, "刪除成功") : (false, "刪除失敗");
            }
            catch (Exception ex)
            {
                return (false, "刪除失敗: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> UpdateAllDeptAndSchedAsync(List<Sys202100DeptListViewModel> depts, List<Sys202100SchedViewModel> scheds, string user, IDataLogger dataLogger)
        {
            try
            {
                foreach (var dept in depts)
                {
                    var deptEntity = new ArgumentsDept
                    {
                        AgdNo = dept.AgdNo,
                        ArgVariable = dept.ArgVariable ?? "",
                        DepNo = dept.DepNo,
                        AgdValue = dept.AgdValue ?? "",
                        AgdModifyName = user ?? "",
                        AgdModifyTime = DateTime.Now
                    };

                    await _repository.UpdateDeptAsync(deptEntity, dataLogger);
                }

                if (scheds != null && scheds.Any())
                {
                    foreach (var sched in scheds)
                    {
                        if (!sched.AgsStartTime.HasValue || !sched.AgsEndTime.HasValue)
                        {
                            continue;
                        }

                        if (sched.AgsStartTime.Value >= sched.AgsEndTime.Value)
                        {
                            continue;
                        }

                        var scheduleEntity = new ArgumentsSchedule
                        {
                            AgsNo = sched.AgsNo,
                            ArgVariable = sched.ArgVariable ?? "",
                            DepNo = sched.DepNo,
                            AgsValue = sched.AgsValue ?? "",
                            AgsStartTime = sched.AgsStartTime.Value,
                            AgsEndTime = sched.AgsEndTime.Value,
                            AgsModifyName = user ?? "",
                            AgsModifyTime = DateTime.Now
                        };
                        await _repository.UpdateSchedAsync(scheduleEntity, dataLogger);
                    }
                }

                return (true, "儲存成功");
            }
            catch (Exception ex)
            {
                return (false, "儲存失敗: " + ex.Message);
            }
        }
    }
}