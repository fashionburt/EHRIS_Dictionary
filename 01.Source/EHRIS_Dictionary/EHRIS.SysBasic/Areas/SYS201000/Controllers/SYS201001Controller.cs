using EHRIS.Core.Models;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.SysBasic;
using EHRIS.Tools.ChangePassword;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.RegularExpressions;

namespace EHRIS.SysBasic.Controllers
{
    [Area("SYS201000")]
    [Route("/[controller]/[action]")]
    [RequireHttps]
    public class SYS201001Controller : BaseController
    {
        protected const int SFUNO = 201001;
        private readonly IUserContextService _userContext;
        private readonly ISYS201001Service _service;
        private readonly ICommonService _commonService;

        public SYS201001Controller(IUserContextService userContext, ISYS201001Service service, ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            
            _service = service;
            _commonService = commonService;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS201001()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS201001", "SYS201001");
            var unitList = await _service.GetUnitListAsync();
            ViewBag.UnitList = new SelectList(unitList, "DepNo", "DepName");
            var professList = await _service.GetProfessListAsync();
            ViewBag.ProfessList = new SelectList(professList, "ProNo", "ProName");
            var deptTree = await _commonService.GetAllUnitDeptList(_userContext.AccNO);

            ViewBag.CanInsert = HasPermission(FunctionAction.Insert);

            var viewModel = new SYS201001ViewModel
            {
                DepartmentTrees = deptTree
            };
            return PartialView("SYS201001", viewModel);
        }

        #region 取得資料
        private Task<ErrorCheckUIDataViewModel> CheckUI(AdminUpdateModel model, bool isCreate)
        {
            var checkResult = new ErrorCheckUIDataViewModel();
            var errorMessages = new List<string>();

            if (isCreate || !string.IsNullOrEmpty(model.password))
            {
                var validationResult = PasswordValidator.ValidateStringRules(model.password, model.loginAccount);
                if (!validationResult.IsValid)
                {
                    errorMessages.AddRange(validationResult.Errors);
                }
            }

            if (model.proNo <= 0)
            {
                errorMessages.Add("職稱為必填項。");
            }
            if (model.unitId <= 0)
            {
                errorMessages.Add("單位為必填項。");
            }
            if (string.IsNullOrWhiteSpace(model.name))
            {
                errorMessages.Add("姓名為必填項。");
            }

            if (string.IsNullOrWhiteSpace(model.idNumber))
            {
                errorMessages.Add("身分證字號為必填項。");
            }
            else if (!IsValidTaiwanId(model.idNumber))
            {
                errorMessages.Add("身分證字號格式不正確。");
            }

            if (model.birthDate == default(DateTime))
            {
                errorMessages.Add("出生日期為必填項。");
            }
            if (string.IsNullOrWhiteSpace(model.loginAccount))
            {
                errorMessages.Add("登入帳號為必填項。");
            }
            if (!string.IsNullOrWhiteSpace(model.name) && model.name.Length > 50)
            {
                errorMessages.Add("姓名長度不可超過 50 個字元。");
            }


            if (errorMessages.Any())
            {
                checkResult.Result = false;
                checkResult.ErrorMessage = string.Join("\n", errorMessages);
            }

            return Task.FromResult(checkResult);
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetAllAdmins([FromBody] AdminDataTableRequest request)
        {
            var serviceResponse = await _service.GetAllAdminsForDataTableAsync(request);

            if (request.orderby != null && request.orderby.Any())
            {
                var firstOrder = request.orderby[0];
                string colName = request.columns[firstOrder.column].data;
                string dir = firstOrder.dir;
                serviceResponse.data = SortAdmins(serviceResponse.data.ToList(), colName, dir);
            }

            var responseData = serviceResponse.data.Select(a => new
            {
                unit = System.Net.WebUtility.HtmlEncode(a.unit ?? string.Empty),
                name = System.Net.WebUtility.HtmlEncode(a.name ?? string.Empty),
                loginAccount = System.Net.WebUtility.HtmlEncode(a.loginAccount ?? string.Empty),
                accountStatus = System.Net.WebUtility.HtmlEncode(a.accountStatus ?? string.Empty),
                editAction = GetEditButtons(a.acc_no, a.loginAccount),
                deleteAction = GetDelButtons(a.acc_no, a.loginAccount)
            });

            return Json(new
            {
                draw = serviceResponse.draw,
                recordsTotal = serviceResponse.recordsTotal,
                recordsFiltered = serviceResponse.recordsFiltered,
                data = responseData
            });
        }

        private List<AdminListViewModel> SortAdmins(List<AdminListViewModel> admins, string sortColumn, string sortDirection)
        {
            System.Func<AdminListViewModel, object> keySelector;
            switch (sortColumn)
            {
                case "unit": keySelector = a => a.unit; break;
                case "name": keySelector = a => a.name; break;
                case "loginAccount": keySelector = a => a.loginAccount; break;
                case "accountStatus": keySelector = a => a.accountStatus; break;
                default: return admins;
            }
            if (sortDirection == "asc") return admins.OrderBy(keySelector).ToList();
            else return admins.OrderByDescending(keySelector).ToList();
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetAdminDetails(int accNo)
        {
            var adminDetails = await _service.GetAdminDetailsAsync(accNo);
            if (adminDetails == null)
            {
                return Json(new { success = false, message = "找不到資料" });
            }

            adminDetails.name = System.Net.WebUtility.HtmlEncode(adminDetails.name ?? string.Empty);
            adminDetails.loginAccount = System.Net.WebUtility.HtmlEncode(adminDetails.loginAccount ?? string.Empty);
            adminDetails.idNumber = System.Net.WebUtility.HtmlEncode(adminDetails.idNumber ?? string.Empty);

            if (adminDetails.Emails != null)
            {
                foreach (var email in adminDetails.Emails)
                {
                    email.BseEmailType = System.Net.WebUtility.HtmlEncode(email.BseEmailType ?? string.Empty);
                    email.BseEmail = System.Net.WebUtility.HtmlEncode(email.BseEmail ?? string.Empty);
                }
            }

            return Json(new { success = true, data = adminDetails });
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetAllPersonnelTypes()
        {
            var types = await _service.GetActivePersonnelTypesAsync();
            return Ok(types.Select(p => new { p.PtyNo, p.PtyName }));
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _service.GetRoleListAsync();
            return Ok(roles.Select(r => new { value = r.RolNo, text = r.RolName }));
        }
        #endregion

        #region 操作行為
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> AddAdmin([FromBody] AdminUpdateModel model)
        {
            var checkResult = await CheckUI(model, isCreate: true);
            if (!checkResult.Result)
            {
                return BadRequest(new { success = false, message = checkResult.ErrorMessage });
            }

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "管理者管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };

            var result = await _service.AddAdminAsync(model, _userContext.UserName, dataLogger);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateAdmin([FromBody] AdminUpdateModel model)
        {
            if (model.acc_no > 0 && string.IsNullOrEmpty(model.password))
            {
                ModelState.Remove(nameof(model.password));
            }
            var checkResult = await CheckUI(model, isCreate: false);
            if (!ModelState.IsValid || !checkResult.Result)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (!checkResult.Result)
                {
                    errors.AddRange(checkResult.ErrorMessage.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)));
                }
                return BadRequest(new { success = false, message = string.Join("\n", errors.Distinct()) });
            }

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "管理者管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var result = await _service.UpdateAdminAsync(model, _userContext.UserName, dataLogger);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteAdmin(int accNo)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "管理者管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };

            var result = await _service.SoftDeleteAdminAsync(accNo, dataLogger, _userContext.UserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }
        #endregion

        #region 操作按鈕
        private string GetEditButtons(int id, string loginAccount)
        {
            var buttons = new StringBuilder();
            if (HasPermission(FunctionAction.Update))
            {
                buttons.Append($"<button type='button' class='icon-btn text-primary editAdmin' data-id='{id}' title='編輯'><i class='fa-regular fa-pen-to-square'></i></button>");
            }
            return buttons.ToString();
        }

        private string GetDelButtons(int id, string loginAccount = "")
        {
            var buttons = new StringBuilder();
            if (HasPermission(FunctionAction.Delete))
            {
                var nameAttr = string.IsNullOrEmpty(loginAccount) ? "" : $"data-name='{loginAccount}'";
                buttons.Append($"<button type='button' class='icon-btn text-danger deleteAdmin' data-id='{id}' {nameAttr} title='刪除'><i class='fa fa-trash'></i></button>");
            }
            return buttons.ToString();
        }
        #endregion

        private static bool IsValidTaiwanId(string id)
        {
            if (string.IsNullOrEmpty(id) || !Regex.IsMatch(id, @"^[A-Z][1289]\d{8}$"))
            {
                return false;
            }

            var letterMap = new Dictionary<char, int>
            {
                { 'A', 10 }, { 'B', 11 }, { 'C', 12 }, { 'D', 13 }, { 'E', 14 },
                { 'F', 15 }, { 'G', 16 }, { 'H', 17 }, { 'I', 34 }, { 'J', 18 },
                { 'K', 19 }, { 'L', 20 }, { 'M', 21 }, { 'N', 22 }, { 'O', 35 },
                { 'P', 23 }, { 'Q', 24 }, { 'R', 25 }, { 'S', 26 }, { 'T', 27 },
                { 'U', 28 }, { 'V', 29 }, { 'W', 32 }, { 'X', 30 }, { 'Y', 31 },
                { 'Z', 33 }
            };

            if (!letterMap.TryGetValue(id[0], out int p))
            {
                return false;
            }

            int sum = (p / 10) + (p % 10) * 9;

            for (int i = 1; i <= 8; i++)
            {
                sum += (id[i] - '0') * (8 - i + 1);
            }

            int lastDigit = id[9] - '0';
            int checkDigit = (10 - (sum % 10)) % 10;

            return lastDigit == checkDigit;
        }
    }
}