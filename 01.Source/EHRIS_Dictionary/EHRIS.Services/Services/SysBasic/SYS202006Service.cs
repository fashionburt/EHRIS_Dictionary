using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public class SYS202006Service : ISYS202006Service
    {
        private readonly ISYS202006Repository _repository;

        public SYS202006Service(ISYS202006Repository repository)
        {
            _repository = repository;
        }

        public async Task<DataTablesResponse<SYS202006ListViewModel>> GetHolidayPagedListAsync(DataTablesRequest request)
        {
            return await _repository.GetHolidayPagedListAsync(request);
        }

        public async Task<SYS202006EditViewModel> GetHolidayForEditAsync(int holNo)
        {
            if (holNo == 0) 
            {
                return new SYS202006EditViewModel
                {
                    HolNo = 0,
                    HolOrder = 0,
                    HolStatistics = "1", 
                    HolOfficial = "0"   
                };
            }

            var holiday = await _repository.GetHolidayByIdAsync(holNo);
            if (holiday == null) return null;

            return new SYS202006EditViewModel
            {
                HolNo = holiday.HolNo,
                HolCode = holiday.HolCode,
                HolName = holiday.HolName,
                HolOrder = holiday.HolOrder,
                HolStatistics = holiday.HolStatistics,
                HolOfficial = holiday.HolOfficial
            };
        }

        public async Task<(bool success, string message)> CreateHolidayAsync(SYS202006UpdateViewModel model, IDataLogger dataLogger, string userName)
        {
            if (string.IsNullOrWhiteSpace(model.HolCode))
            {
                return (false, "代號為必填。");
            }
            if (model.HolCode.Length == 1)
            {
                model.HolCode = model.HolCode.PadLeft(2, '0');
            }
            if (model.HolCode.Length != 2)
            {
                return (false, "代號長度必須為 2 位數。");
            }

            if (await _repository.CodeExistsAsync(model.HolCode, 0))
            {
                return (false, "代號已存在");
            }

            await _repository.CreateHolidayAsync(model, userName, dataLogger);
            return (true, "新增成功");
        }

        public async Task<(bool success, string message)> UpdateHolidayAsync(SYS202006UpdateViewModel model, IDataLogger dataLogger, string userName)
        {
            await _repository.UpdateHolidayAsync(model, userName, dataLogger);
            return (true, "修改成功");
        }

        public async Task<(bool success, string message)> SoftDeleteHolidayAsync(int holNo, IDataLogger dataLogger, string userName)
        {
            await _repository.SoftDeleteHolidayAsync(holNo, dataLogger);
            return (true, "刪除成功");
        }
    }
}