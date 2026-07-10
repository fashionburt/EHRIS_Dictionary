using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Core.Repositories.SysBasic;
using EHRIS.Services.Common;
using EHRIS.Tools.ChangePassword;
using EHRIS.Tools.Crypto;
using Microsoft.Extensions.Caching.Memory; 
using System.Text;

namespace EHRIS.Services.Services.SysBasic
{
    public class SYS201001Service : ISYS201001Service
    {
        private readonly ISYS201001Repository _repository;
        private readonly ICommonService _commonService;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public SYS201001Service(ISYS201001Repository repository, ICommonService commonService, ApplicationDbContext context, IMemoryCache cache)
        {
            _repository = repository;
            _commonService = commonService;
            _context = context;
            _cache = cache;
        }

        private async Task<bool> IsLockedByZBufferAsync(int peoUid)
        {
            var lockedIds = await _cache.GetOrCreateAsync("Sys_UI_Ref_Buffer_Cache", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _repository.GetStaticLockPeoUidsAsync();
            });

            return lockedIds.Contains(peoUid);
        }

        public async Task<AdminDataTableResponse<AdminListViewModel>> GetAllAdminsForDataTableAsync(AdminDataTableRequest request)
        {
            var totalRecords = await _repository.GetTotalAdminCountAsync();
            var filteredData = await _repository.GetAllAdminsAsync(request);
            var recordsFiltered = await _repository.GetFilteredAdminCountAsync(request); 
            var sortedData = SortAdmins(filteredData, request); 
            var pagedData = sortedData.Skip(request.start).Take(request.length).ToList();

            return new AdminDataTableResponse<AdminListViewModel>
            {
                draw = request.draw,
                recordsTotal = totalRecords,
                recordsFiltered = recordsFiltered,
                data = pagedData
            };
        }

        private List<AdminListViewModel> SortAdmins(List<AdminListViewModel> admins, AdminDataTableRequest request)
        {
            if (request.orderby == null || !request.orderby.Any() || request.columns == null)
            {
                return admins.OrderBy(a => a.unit).ThenBy(a => a.name).ToList();
            }

            var orderInfo = request.orderby.First();
            var columnIndex = orderInfo.column;

            if (columnIndex < 0 || columnIndex >= request.columns.Count)
            {
                return admins;
            }

            var sortColumn = request.columns[columnIndex].data?.ToLower();
            var sortDirection = orderInfo.dir?.ToLower();

            Func<AdminListViewModel, object> keySelector;

            switch (sortColumn)
            {
                case "unit": keySelector = a => a.unit; break;
                case "name": keySelector = a => a.name; break;
                case "loginaccount": keySelector = a => a.loginAccount; break;
                case "accountstatus": keySelector = a => a.accountStatus; break;
                default: return admins;
            }

            if (sortDirection == "asc")
            {
                return admins.OrderBy(keySelector).ToList();
            }
            else
            {
                return admins.OrderByDescending(keySelector).ToList();
            }
        }
        public async Task<AdminDetailsViewModel> GetAdminDetailsAsync(int accNo) => await _repository.GetAdminDetailsAsync(accNo);
        public async Task<List<Departments>> GetUnitListAsync() => await _repository.GetUnitListAsync();
        public async Task<List<Role>> GetRoleListAsync() => await _repository.GetRoleListAsync();
        public async Task<List<PType>> GetActivePersonnelTypesAsync() => await _repository.GetActivePersonnelTypesAsync();
        public async Task<List<Profess>> GetProfessListAsync() => await _repository.GetProfessListAsync();

        public async Task<(bool success, string message)> AddAdminAsync(AdminUpdateModel model, string userName, IDataLogger dataLogger)
        {
            var validationResult = PasswordValidator.ValidateStringRules(model.password, model.loginAccount);
            if (!validationResult.IsValid)
            {
                return (false, string.Join("\n", validationResult.Errors));
            }

            if (await _repository.DoesAccountExistAsync(model.loginAccount))
            {
                return (false, "登入帳號已存在。");
            }
            byte[] hmacKey = Encoding.UTF8.GetBytes(_commonService.MasterKey);
            var (hash, salt) = PasswordHasher.HashPassword(model.password, hmacKey);
            try
            {
                await _repository.AddAdminAsync(model, hash, salt, userName, DateTime.Now, dataLogger);
                return (true, "新增成功。");
            }
            catch (Exception ex)
            {
                return (false, $"新增失敗：{ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateAdminAsync(AdminUpdateModel model, string userName, IDataLogger dataLogger)
        {
            var account = await _repository.GetAccountByIdAsync(model.acc_no);
            if (account == null) return (false, "找不到要更新的資料。");

            if (await IsLockedByZBufferAsync(account.PeoUid))
            {
                return (false, "系統管理者帳號，無法執行變更操作。");
            }

            if (await _repository.DoesAccountExistAsync(model.loginAccount, model.acc_no))
            {
                return (false, "此登入帳號已被其他使用者使用。");
            }
            string hash = account.AccPasswdHash;
            string salt = account.AccPasswdSalt;

            if (!string.IsNullOrEmpty(model.password))
            {
                var stringValidation = PasswordValidator.ValidateStringRules(model.password, model.loginAccount);
                if (!stringValidation.IsValid)
                {
                    return (false, string.Join("\n", stringValidation.Errors));
                }

                var minAgeValidation = PasswordValidator.ValidateMinAge(account.AccPwChange);
                if (!minAgeValidation.IsValid)
                {
                    return (false, string.Join("\n", minAgeValidation.Errors));
                }

                var recentHashes = await _repository.GetRecentPasswordHistoryAsync(model.acc_no, 3);

                if (PasswordHasher.IsPasswordReused(model.password, _commonService.MasterKey, recentHashes))
                {
                    return (false, "密碼不可與最近使用過前3組密碼相同。");
                }

                byte[] hmacKey = Encoding.UTF8.GetBytes(_commonService.MasterKey);
                (hash, salt) = PasswordHasher.HashPassword(model.password, hmacKey);
            }
            try
            {
                await _repository.UpdateAdminAsync(model, hash, salt, userName, DateTime.Now, dataLogger);
                return (true, "更新成功。");
            }
            catch (Exception ex)
            {
                return (false, $"更新失敗：{ex.Message}");
            }
        }

        public async Task<(bool success, string message)> SoftDeleteAdminAsync(int accNo, IDataLogger dataLogger, string userName)
        {
            var account = await _repository.GetAccountByIdAsync(accNo);
            if (account == null) return (false, "找不到要停用的資料。");

            if (await IsLockedByZBufferAsync(account.PeoUid))
            {
                return (false, "系統管理者帳號，無法刪除。");
            }

            var result = await _repository.SoftDeleteAdminAsync(accNo, userName, DateTime.Now, dataLogger);
            return result > 0 ? (true, "停用成功。") : (false, "停用失敗。");
        }
    }
}