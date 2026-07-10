using EHRIS.Core.Models;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.SysBasic;
using EHRIS.Tools.Extensions;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Controllers
{
    [Area("SYS202000")]
    [Route("[controller]/[action]")]
    public class SYS202006Controller : BaseController
    {
        protected const int SFUNO = 202006;
        private readonly IUserContextService _userContext;
        private readonly ISYS202006Service _sys202006Service;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICommonService _commonService;

        public SYS202006Controller(IUserContextService userContext,
                                       ISYS202006Service sys202006Service,
                                       IAuthorizationService authorizationService,
                                       ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202006Service = sys202006Service;
            _authorizationService = authorizationService;
            _commonService = commonService;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202006()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202006", "SYS202006");
            ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202006");
        }


        #region 取資料
        private Task<ErrorCheckUIDataViewModel> CheckUI(SYS202006UpdateViewModel model, bool isCreate)
        {
            var checkResult = new ErrorCheckUIDataViewModel();
            var errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(model.HolCode))
            {
                errorMessages.Add("代號為必填項。");
            }
            else if (isCreate)
            {
                if (model.HolCode.Length > 0 && model.HolCode.Length < 2)
                {
                    model.HolCode = model.HolCode.PadLeft(2, '0');
                }
                if (model.HolCode.Length != 2)
                {
                    errorMessages.Add("代號長度必須為 2 位數。");
                }
            }

            if (string.IsNullOrWhiteSpace(model.HolName))
            {
                errorMessages.Add("假別名稱為必填項。");
            }
            if (model.HolOrder < 0)
            {
                errorMessages.Add("排序不可為負數。");
            }
            if (string.IsNullOrWhiteSpace(model.HolStatistics) || (model.HolStatistics != "0" && model.HolStatistics != "1"))
            {
                errorMessages.Add("請選擇是否統計。");
            }
            if (string.IsNullOrWhiteSpace(model.HolOfficial) || (model.HolOfficial != "0" && model.HolOfficial != "1"))
            {
                errorMessages.Add("請選擇是否為公務假別。");
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
        public async Task<IActionResult> GetHolidayData([FromBody] DataTablesRequest request)
        {
            var serviceResponse = await _sys202006Service.GetHolidayPagedListAsync(request);
            var jsonData = new
            {
                draw = serviceResponse.draw,
                recordsTotal = serviceResponse.recordsTotal,
                recordsFiltered = serviceResponse.recordsFiltered,
                data = serviceResponse.data.Select(p => new
                {
                    p.HolCode,
                    p.HolName,
                    HolStatisticsDisplay = (p.HolStatistics == "1") ? "是" : "否",
                    HolOfficialDisplay = (p.HolOfficial == "1") ? "是" : "否",
                    p.HolOrder,
                    p.HolModifyName,
                    HolModifyTime = p.HolModifyTime.ToRocDateTime(),
                    editAction = GetEditButtons(p.HolNo),
                    deleteAction = GetDelButtons(p.HolNo)
                }).ToList()
            };
            return Ok(jsonData);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetHolidayForEdit(int id)
        {
            var viewModel = await _sys202006Service.GetHolidayForEditAsync(id);
            if (viewModel == null)
            {
                return NotFound(new { success = false, message = "找不到指定的資料。" });
            }
            return Ok(new { success = true, data = viewModel });
        }
        #endregion

        #region 操作行為
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> CreateHoliday([FromBody] SYS202006UpdateViewModel model)
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
                ExecProName = "假別資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };

            var currentUserName = _userContext.UserName;
            var result = await _sys202006Service.CreateHolidayAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }
            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateHoliday([FromBody] SYS202006UpdateViewModel model)
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
                ExecProName = "假別資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var currentUserName = _userContext.UserName;
            var result = await _sys202006Service.UpdateHolidayAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }
            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "假別資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };

            var currentUserName = _userContext.UserName;
            var result = await _sys202006Service.SoftDeleteHolidayAsync(id, dataLogger, currentUserName);

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
                buttons += $"<button class='icon-btn text-primary editHoliday' data-id='{id}' title='修改'><i class=\"fa-regular fa-pen-to-square\"></i></button> ";
            }
            return buttons;
        }
        private string GetDelButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Delete))
            {
                buttons += $"<button class='icon-btn text-danger deleteHoliday' data-id='{id}' title='刪除'><i class=\"fa fa-trash\"></i></button>";
            }
            return buttons;
        }
        #endregion
    }
}