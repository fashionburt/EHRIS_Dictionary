using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories;
using EHRIS.Services.Common;

namespace EHRIS.Services.Services
{
    public class SYS202005Service : ISYS202005Service
    {
        private readonly ISYS202005Repository _repository;
        private readonly ICommonService _commonService;

        public SYS202005Service(ISYS202005Repository repository, ICommonService commonService)
        {
            _repository = repository;
            _commonService = commonService;
        }

        public async Task<(IEnumerable<Sys202005ListViewModel> Data, int RecordsFiltered, int RecordsTotal)>
            GetListAsync(Sys202005DataTableRequest request)
        {
            return await _repository.GetPagedListAsync(request);
        }

        public async Task<Sys202005EditViewModel> GetByNoAsync(int pleNo)
        {
            var entity = await _repository.GetByNoAsync(pleNo);
            if (entity == null) return null;

            return new Sys202005EditViewModel
            {
                PleNo = entity.PleNo,
                PleCode = entity.PleCode,
                PleName = entity.PleName
            };
        }

        public async Task<(bool success, string message)> AddAsync(Sys202005EditViewModel model, string user, IDataLogger dataLogger)
        {
            try
            {
                if (await _repository.CodeExistsAsync(model.PleCode))
                {
                    return (false, "代碼已存在");
                }

                var entity = new HRPLevel
                {
                    PleTNo = 0,
                    PleCode = model.PleCode,
                    PleName = model.PleName,
                    PleStatus = "1",
                    PleCreateTime = DateTime.Now,
                    PleCreateName = user,
                    PleModifyTime = DateTime.Now,
                    PleModifyName = user
                };

                await _repository.AddAsync(entity, dataLogger);
                return (true, "新增成功");
            }
            catch (Exception ex)
            {
                return (false, "新增失敗: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> UpdateAsync(Sys202005EditViewModel model, string user, IDataLogger dataLogger)
        {
            try
            {
                if (await _repository.CodeExistsAsync(model.PleCode, model.PleNo))
                {
                    return (false, "代碼已存在");
                }

                var entity = new HRPLevel
                {
                    PleNo = model.PleNo,
                    PleCode = model.PleCode,
                    PleName = model.PleName,
                    PleModifyName = user,
                    PleModifyTime = DateTime.Now
                };

                await _repository.UpdateAsync(entity, dataLogger);
                return (true, "修改成功");
            }
            catch (Exception ex)
            {
                return (false, "修改失敗: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int pleNo, string user, IDataLogger dataLogger)
        {
            try
            {
                await _repository.SoftDeleteAsync(pleNo, user, dataLogger);
                return (true, "刪除成功");
            }
            catch (Exception ex)
            {
                return (false, "刪除失敗: " + ex.Message);
            }
        }
    }
}