using EHRIS.Core.Models;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.SysBasic;
using EHRIS.Tools.Extensions;
using EHRIS.Web.Shared.Controllers;
using EHRIS.Security.Permission.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Controllers
{
    [Area("SYS202000")]
    [Route("[controller]/[action]")]
    public class SYS202004Controller : BaseController
    {
        protected const int SFUNO = 202004;
        private readonly IUserContextService _userContext;
        private readonly ISYS202004Service _sys202004Service;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICommonService _commonService;

        public SYS202004Controller(IUserContextService userContext,
                                       ISYS202004Service sys202004Service,
                                       IAuthorizationService authorizationService,
                                       ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202004Service = sys202004Service;
            _authorizationService = authorizationService;
            _commonService = commonService;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202004()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202004", "SYS202004");
            ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202004");
        }

        #region 取得資料
        private Task<ErrorCheckUIDataViewModel> CheckUI(SYS202004UpdateViewModel model, bool isCreate)
        {
            var checkResult = new ErrorCheckUIDataViewModel();
            var errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(model.ProCode))
            {
                errorMessages.Add("代號為必填項。");
            }
            else if (isCreate)
            {
                if (model.ProCode.Length > 0 && model.ProCode.Length < 4)
                {
                    model.ProCode = model.ProCode.PadLeft(4, '0');
                }
                if (model.ProCode.Length != 4)
                {
                    errorMessages.Add("代號長度必須為 4 位數。");
                }
            }
            if (string.IsNullOrWhiteSpace(model.ProName))
            {
                errorMessages.Add("職稱中文為必填項。");
            }
            if (model.ProOrder < 0)
            {
                errorMessages.Add("排序不可為負數。");
            }
            if (string.IsNullOrWhiteSpace(model.ProIsManager) || (model.ProIsManager != "0" && model.ProIsManager != "1"))
            {
                errorMessages.Add("請選擇是否為主管。");
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
        public async Task<IActionResult> GetProfessData([FromBody] DataTablesRequest request)
        {
            var serviceResponse = await _sys202004Service.GetProfessPagedListAsync(request);
            var jsonData = new
            {
                draw = serviceResponse.draw,
                recordsTotal = serviceResponse.recordsTotal,
                recordsFiltered = serviceResponse.recordsFiltered,
                data = serviceResponse.data.Select(p => new
                {
                    p.ProCode,
                    p.ProName,
                    p.ProEnglish,
                    ProIsManager = (p.ProIsManager == "1") ? "是" : "否",
                    p.PersonTypes,
                    p.ProOrder,
                    p.ProModifyName,
                    ProModifyTime = p.ProModifyTime.ToRocDateTime(),
                    editAction = GetEditButtons(p.ProNo),
                    deleteAction = GetDelButtons(p.ProNo)
                }).ToList()
            };
            return Ok(jsonData);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetProfessForEdit(int id)
        {
            var viewModel = await _sys202004Service.GetProfessForEditAsync(id);
            if (viewModel == null)
            {
                return NotFound(new { success = false, message = "找不到指定的資料。" });
            }
            return Ok(new { success = true, data = viewModel });
        }
        #endregion

        #region 操作行為
        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> CreateProfess([FromBody] SYS202004UpdateViewModel model)
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
                ExecProName = "職稱資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202004Service.CreateProfessAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateProfess([FromBody] SYS202004UpdateViewModel model)
        {
            var checkResult = await CheckUI(model, isCreate: false);
            if (!checkResult.Result)
            {
                return BadRequest(new { success = false, message = checkResult.ErrorMessage });
            }

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "職稱資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202004Service.UpdateProfessAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteProfess(int id)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "職稱資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202004Service.SoftDeleteProfessAsync(id, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }
        #endregion

        #region 產生操作按鈕
        private string GetEditButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Update))
            {
                buttons += $"<button class='icon-btn text-primary editProfess' data-id='{id}' title='修改'><i class=\"fa-regular fa-pen-to-square\"></i></button> ";
            }
            return buttons;
        }
        private string GetDelButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Delete))
            {
                buttons += $"<button class='icon-btn text-danger deleteProfess' data-id='{id}' title='刪除'><i class=\"fa fa-trash\"></i></button>";
            }
            return buttons;
        }
        #endregion
    }
}