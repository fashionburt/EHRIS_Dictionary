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
    public class SYS202003Controller : BaseController
    {
        protected const int SFUNO = 202003;
        private readonly IUserContextService _userContext;
        private readonly ISYS202003Service _sys202003Service;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICommonService _commonService;

        public SYS202003Controller(IUserContextService userContext,
                                       ISYS202003Service sys202003Service,
                                       IAuthorizationService authorizationService,
                                       ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202003Service = sys202003Service;
            _authorizationService = authorizationService;
            _commonService = commonService;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202003()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202003", "SYS202003");
            ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202003");
        }

        #region 取得資料
        private Task<ErrorCheckUIDataViewModel> CheckUI(PTypeUpdateViewModel model, bool isCreate)
        {
            var checkResult = new ErrorCheckUIDataViewModel();
            var errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(model.PtyCode))
            {
                errorMessages.Add("代號為必填項。");
            }
            else if (isCreate)
            {
                if (model.PtyCode.Length > 0 && model.PtyCode.Length < 2)
                {
                    model.PtyCode = model.PtyCode.PadLeft(2, '0');
                }
                if (model.PtyCode.Length != 2)
                {
                    errorMessages.Add("代號長度必須為 2 位數。");
                }
            }
            if (string.IsNullOrWhiteSpace(model.PtyName))
            {
                errorMessages.Add("人員類別為必填項。");
            }
            if (model.PtyOrder < 0)
            {
                errorMessages.Add("排序不可為負數。");
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
        public async Task<IActionResult> GetPTypeData([FromBody] DataTablesRequest request)
        {
            var serviceResponse = await _sys202003Service.GetPTypePagedListAsync(request);
            var jsonData = new
            {
                draw = serviceResponse.draw,
                recordsTotal = serviceResponse.recordsTotal,
                recordsFiltered = serviceResponse.recordsFiltered,
                data = serviceResponse.data.Select(p => new
                {
                    p.PtyCode,
                    p.PtyName,
                    p.PersonTypes,
                    p.PtyOrder,
                    p.PtyModifyName,
                    PtyModifyTime = p.PtyModifyTime.ToRocDateTime(),
                    editAction = GetEditButtons(p.PtyNo),
                    deleteAction = GetDelButtons(p.PtyNo)
                }).ToList()
            };
            return Ok(jsonData);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetPTypeForEdit(int id)
        {
            var viewModel = await _sys202003Service.GetPTypeForEditAsync(id);
            if (viewModel == null)
            {
                return NotFound(new { success = false, message = "找不到指定的資料。" });
            }
            return Ok(new { success = true, data = viewModel });
        }
        #endregion

        #region 操作紀錄
        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> CreatePType([FromBody] PTypeUpdateViewModel model)
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
                ExecProName = "人員類別管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202003Service.CreatePTypeAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdatePType([FromBody] PTypeUpdateViewModel model)
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
                ExecProName = "人員類別管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202003Service.UpdatePTypeAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeletePType(int id)
        {
            var currentUserName = _userContext.UserName;
            WebDataLogger dataLogger = new WebDataLogger
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人員類別管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };
            var result = await _sys202003Service.SoftDeletePTypeAsync(id, dataLogger, currentUserName);

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
                buttons += $"<button class='icon-btn text-primary editPType' data-id='{id}' title='修改'><i class=\"fa-regular fa-pen-to-square\"></i></button> ";
            }
            return buttons;
        }
        private string GetDelButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Delete))
            {
                buttons += $"<button class='icon-btn text-danger deletePType' data-id='{id}' title='刪除'><i class=\"fa fa-trash\"></i></button>";
            }
            return buttons;
        }
        #endregion
    }
}