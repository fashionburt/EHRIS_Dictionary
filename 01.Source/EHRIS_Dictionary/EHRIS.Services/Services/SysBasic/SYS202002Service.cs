using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public class SYS202002Service : ISYS202002Service
    {
        private readonly ISYS202002Repository _sysfuctionRepository;
        private readonly ApplicationDbContext _context; 

        public SYS202002Service(ISYS202002Repository sysfuctionRepository, ApplicationDbContext context)
        {
            _sysfuctionRepository = sysfuctionRepository;
            _context = context;
        }
        public async Task<IEnumerable<SYS202002TreeViewModel>> GetAllFunctionsForTreeAsync()
        {
            return await _sysfuctionRepository.GetAllFunctionsForTreeAsync();
        }

        public async Task<IEnumerable<DropdownViewModel>> GetSystemsAsync()
        {
            return await _sysfuctionRepository.GetSystemsAsync();
        }

        public async Task<IEnumerable<DropdownViewModel>> GetFunctionsBySystemAsync(int sysNo)
        {
            return await _sysfuctionRepository.GetFunctionsBySystemAsync(sysNo);
        }

        public async Task<SYS202002CreateViewModel> GetFunctionByIdAsync(int sfuNo)
        {
            return await _sysfuctionRepository.GetFunctionByIdAsync(sfuNo);
        }

        public async Task<(bool Success, string Message)> CreateFunctionAsync(SYS202002CreateViewModel model, IDataLogger dataLogger, string userName)
        {
            if (await _sysfuctionRepository.FunctionExistsAsync(model.SfuNo))
            {
                return (false, "新增失敗：系統編號已存在。");
            }

            var currentTime = DateTime.Now;
            model.CreateName = userName;
            model.CreateTime = currentTime;
            model.ModifyName = userName;
            model.ModifyTime = currentTime;
            model.SfuVersion = "";

            try
            {
                await _sysfuctionRepository.CreateFunctionAsync(model, userName, dataLogger);
                return (true, "新增成功！");
            }
            catch (Exception ex)
            {
                return (false, $"新增失敗：{ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateFunctionAsync(SYS202002CreateViewModel model, IDataLogger dataLogger, string userName)
        {
            model.ModifyName = userName;
            model.ModifyTime = DateTime.Now;
            try
            {
                bool isSuccess = await _sysfuctionRepository.UpdateFunctionAsync(model, userName, dataLogger);

                if (!isSuccess)
                {
                    return (false, $"更新失敗：找不到編號為 {model.SfuNo} 的資料。");
                }

                if (model.SfuStatus == 0)
                {
                    await _sysfuctionRepository.UpdateChildrenStatusAsync(model.SfuNo, model.SfuStatus, userName, dataLogger);
                }

                return (true, "更新成功！");
            }
            catch (Exception ex)
            {
                return (false, $"資料庫發生錯誤：{ex.Message}");
            }
        }


        public async Task<(bool Success, string Message)> SoftDeleteFunctionAsync(int sfuNo, IDataLogger dataLogger, string userName)
        {
            var targetFunction = await _sysfuctionRepository.GetFunctionByIdAsync(sfuNo);
            if (targetFunction == null)
            {
                return (false, "刪除失敗，找不到指定的資料。");
            }

            var childIds = await _sysfuctionRepository.GetChildFunctionIdsAsync(sfuNo);
            var idsToDelete = childIds.ToList();
            idsToDelete.Add(sfuNo);

            try
            {
                var affectedRows = await _sysfuctionRepository.SoftDeleteFunctionsAsync(idsToDelete, userName, dataLogger);
                if (affectedRows > 0)
                {
                    return (true, $"刪除成功！共影響 {affectedRows} 筆資料。");
                }
                return (false, "刪除失敗。");
            }
            catch (Exception ex)
            {
                return (false, $"刪除失敗：{ex.Message}");
            }
        }


    }
}