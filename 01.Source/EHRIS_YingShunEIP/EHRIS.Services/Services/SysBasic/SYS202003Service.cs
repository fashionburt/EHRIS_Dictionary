using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories.SysBasic;

namespace EHRIS.Services.Services.SysBasic
{
    public class SYS202003Service : ISYS202003Service
    {
        private readonly ISYS202003Repository _repository;

        public SYS202003Service(ISYS202003Repository repository)
        {
            _repository = repository;
        }

        public async Task<DataTablesResponse<SYS202003ListViewModel>> GetPTypePagedListAsync(DataTablesRequest request)
        {
            return await _repository.GetPTypePagedListAsync(request);
        }

        public async Task<SYS202003EditViewModel> GetPTypeForEditAsync(int ptyNo)
        {
            var allOptions = await _repository.GetAllPersonTypeOptionsAsync();

            if (ptyNo == 0)
            {
                return new SYS202003EditViewModel
                {
                    AllPersonTypes = allOptions
                };
            }

            var ptype = await _repository.GetPTypeByIdAsync(ptyNo);
            if (ptype == null) return null;

            var selectedOptions = await _repository.GetSelectedPersonTypesAsync(ptyNo);

            return new SYS202003EditViewModel
            {
                PtyNo = ptype.PtyNo,
                PtyCode = ptype.PtyCode,
                PtyName = ptype.PtyName,
                PtyOrder = ptype.PtyOrder,
                AllPersonTypes = allOptions,
                SelectedPersonTypes = selectedOptions
            };
        }

        public async Task<(bool success, string message)> CreatePTypeAsync(PTypeUpdateViewModel model, IDataLogger dataLogger, string userName)
        {
            if (!int.TryParse(model.PtyCode, out int ptyNoValue))
            {
                return (false, "職稱代號必須為數字。");
            }

            if (model.PtyCode.Length == 1)
            {
                model.PtyCode = model.PtyCode.PadLeft(2, '0');
            }

            if (await _repository.CodeExistsAsync(model.PtyCode, 0))
            {
                return (false, "職稱代號已存在");
            }


            await _repository.CreatePTypeAndAssociationsAsync(model, userName, dataLogger);
            return (true, "新增成功");
        }

        public async Task<(bool success, string message)> UpdatePTypeAsync(PTypeUpdateViewModel model, IDataLogger dataLogger, string userName)
        {
            if (await _repository.CodeExistsAsync(model.PtyCode, model.PtyNo))
            {
                return (false, "類別代碼已存在");
            }

            await _repository.UpdatePTypeAndAssociationsAsync(model, userName, dataLogger);
            return (true, "修改成功");
        }

        public async Task<(bool success, string message)> SoftDeletePTypeAsync(int ptyNo, IDataLogger dataLogger, string userName)
        {
            await _repository.SoftDeletePTypeAsync(ptyNo, dataLogger);
            return (true, "刪除成功");
        }
    }
}