using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic; 
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.SysBasic;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Controllers;
using EHRIS.Security.Permission.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Controllers
{
        [Area("SYS202000")]
    [Route("[controller]/[action]")]
    public class SYS202002Controller : BaseController
    {
                                protected const int SFUNO = 202002; 
        private readonly IUserContextService _userContext;
        private readonly ISYS202002Service _sys202002Service;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICommonService _commonService;

        public SYS202002Controller(IUserContextService userContext,
            ISYS202002Service sys202002Service,
            IAuthorizationService authorizationService, ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202002Service = sys202002Service;
            _authorizationService = authorizationService;
            _commonService = commonService;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202002()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202002", "SYS202002");
            ViewBag.CanInsert = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202002");
        }

        #region 取得資料
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetDataForTree()
        {
            var allFunctions = (await _sys202002Service.GetAllFunctionsForTreeAsync()).ToList();

            foreach (var func in allFunctions)
            {
                func.SysName = System.Net.WebUtility.HtmlEncode(func.SysName ?? string.Empty);
                func.SfuName = System.Net.WebUtility.HtmlEncode(func.SfuName ?? string.Empty);
                func.EditAction = GetEditButtons(func.SfuNo);
                func.DeleteAction = GetDelButtons(func.SfuNo);
            }

            return Json(allFunctions);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetSystems()
        {
            var data = await _sys202002Service.GetSystemsAsync();
            var safeData = data.Select(x => new DropdownViewModel
            {
                Value = x.Value,
                Text = System.Net.WebUtility.HtmlEncode(x.Text ?? string.Empty)
            });
            return Json(safeData);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetFunctionsBySystem(int sysNo)
        {
            var data = await _sys202002Service.GetFunctionsBySystemAsync(sysNo);
            var safeData = data.Select(x => new DropdownViewModel
            {
                Value = x.Value,
                Text = System.Net.WebUtility.HtmlEncode(x.Text ?? string.Empty)
            });
            return Json(safeData);
        }

        [HttpGet]
        public async Task<IActionResult> GetFunctionDetails(int id)
        {
            var data = await _sys202002Service.GetFunctionByIdAsync(id);
            if (data == null)
            {
                return NotFound();
            }

            data.SfuName = System.Net.WebUtility.HtmlEncode(data.SfuName ?? string.Empty);
            data.SfuCatalog = System.Net.WebUtility.HtmlEncode(data.SfuCatalog ?? string.Empty);
            data.SfuPath = System.Net.WebUtility.HtmlEncode(data.SfuPath ?? string.Empty);
            data.CreateName = System.Net.WebUtility.HtmlEncode(data.CreateName ?? string.Empty);
            data.ModifyName = System.Net.WebUtility.HtmlEncode(data.ModifyName ?? string.Empty);

            return Json(data);
        }
        #endregion

        #region 操作行為
        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> CreateFunction([FromBody] SYS202002CreateViewModel model)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "功能管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202002Service.CreateFunctionAsync(model, dataLogger, currentUserName);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateFunction([FromBody] SYS202002CreateViewModel model)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "功能管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202002Service.UpdateFunctionAsync(model, dataLogger, currentUserName);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteFunction([FromForm] int id)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "功能管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202002Service.SoftDeleteFunctionAsync(id, dataLogger, currentUserName);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
        #endregion

        #region 產生操作按鈕

        private string GetEditButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Update))
            {
                buttons += $"<button class='icon-btn text-primary editFunction' data-id='{id}' title='修改'><i class=\"fa-regular fa-pen-to-square\"></i></button> ";
            }
            return buttons;
        }

        private string GetDelButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Delete))
            {
                buttons += $"<button class='icon-btn text-danger deleteFunction' data-id='{id}' title='刪除'><i class=\"fa fa-trash\"></i></button>";
            }
            return buttons;
        }
        #endregion

    }
}