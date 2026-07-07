using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public class SYS202004Service : ISYS202004Service
    {
        private readonly ISYS202004Repository _repository;

        public SYS202004Service(ISYS202004Repository repository)
        {
            _repository = repository;
        }

        public async Task<DataTablesResponse<SYS202004ListViewModel>> GetProfessPagedListAsync(DataTablesRequest request)
        {
            return await _repository.GetProfessPagedListAsync(request);
        }

        public async Task<SYS202004EditViewModel> GetProfessForEditAsync(int proNo)
        {
            var allOptions = await _repository.GetAllPersonTypeOptionsAsync();

            if (proNo == 0)
            {
                return new SYS202004EditViewModel
                {
                    AllPersonTypes = allOptions,
                    ProIsManager = "0" 
                };
            }

            var profess = await _repository.GetProfessByIdAsync(proNo);
            if (profess == null) return null;

            var selectedOptions = await _repository.GetSelectedPersonTypesAsync(proNo);

            return new SYS202004EditViewModel
            {
                ProNo = profess.ProNo,
                ProCode = profess.ProCode,
                ProName = profess.ProName,
                ProEnglish = profess.ProEnglish,
                ProIsManager = profess.ProIsManager.ToString(),
                ProOrder = profess.ProOrder,
                AllPersonTypes = allOptions,
                SelectedPersonTypes = selectedOptions
            };
        }

        public async Task<(bool success, string message)> CreateProfessAsync(SYS202004UpdateViewModel model, IDataLogger dataLogger, string userName)
        {
            if (string.IsNullOrWhiteSpace(model.ProCode))
            {
                return (false, "職稱代號為必填。");
            }

            if (model.ProCode.Length > 0 && model.ProCode.Length < 4)
            {
                model.ProCode = model.ProCode.PadLeft(4, '0');
            }

            if (model.ProCode.Length != 4)
            {
                return (false, "職稱代號長度必須為 4 位數。");
            }

            if (await _repository.CodeExistsAsync(model.ProCode, 0))
            {
                return (false, "職稱代號已存在");
            }

            await _repository.CreateProfessAndAssociationsAsync(model, userName, dataLogger);
            return (true, "新增成功");
        }

        public async Task<(bool success, string message)> UpdateProfessAsync(SYS202004UpdateViewModel model, IDataLogger dataLogger, string userName)
        {
            if (await _repository.CodeExistsAsync(model.ProCode, model.ProNo))
            {
                return (false, "職稱代號已存在");
            }

            await _repository.UpdateProfessAndAssociationsAsync(model, userName, dataLogger);
            return (true, "修改成功");
        }

        public async Task<(bool success, string message)> SoftDeleteProfessAsync(int proNo, IDataLogger dataLogger, string userName)
        {
            await _repository.SoftDeleteProfessAsync(proNo, dataLogger);
            return (true, "刪除成功");
        }
    }
}