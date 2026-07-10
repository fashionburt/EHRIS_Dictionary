using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Enums;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services.SysBasic;
using EHRIS.Web.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Controllers
{
    [Area("SYS202000")]
    [Route("/[controller]/[action]")]
    public class SYS202100Controller : BaseController
    {
        protected const int SFUNO = 202100;
        private readonly IUserContextService _userContext;
        private readonly ISYS202100Service _sys202100Service;
        private readonly ICommonService _commonService;

        public SYS202100Controller(IUserContextService userContext, ISYS202100Service sys202100Service, ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202100Service = sys202100Service;
            _commonService = commonService;
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202100()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202100", "SYS202100");
            ViewBag.AddStatus = HasPermission(FunctionAction.Insert);

            dynamic model = new System.Dynamic.ExpandoObject();
            model.DepartmentTrees = await _commonService.GetAllUnitDeptList(_userContext.AccNO);

            return PartialView("SYS202100", model);
        }

        #region 取得資料
        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetList([FromBody] Sys202100DataTableRequest request)
        {
            var (data, recordsFiltered, recordsTotal) = await _sys202100Service.GetListAsync(request);

            return Json(new
            {
                draw = request.draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data.Select(r => new
                {
                    argVariable = System.Net.WebUtility.HtmlEncode(r.ArgVariable),
                    argDescribe = System.Net.WebUtility.HtmlEncode(r.ArgDescribe),
                    argValue = System.Net.WebUtility.HtmlEncode(r.ArgValue),
                    agdValueDisplay = System.Net.WebUtility.HtmlEncode(r.AgdValueDisplay),
                    deptAction = GetDeptButtons(r.ArgVariable) ?? "",
                    editAction = GetEditButtons(r.ArgVariable) ?? ""
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartmentOptions()
        {
            var data = await _sys202100Service.GetDepartmentOptionsAsync();
            return Json(new { success = true, data = data });
        }

        [HttpGet]
        public async Task<IActionResult> GetByVariable(string argVariable)
        {
            var data = await _sys202100Service.GetByVariableAsync(argVariable);
            if (data == null)
                return Json(new { success = false, message = "找不到資料" });

            var safeData = new
            {
                ArgVariable = System.Net.WebUtility.HtmlEncode(data.ArgVariable ?? string.Empty),
                ArgDescribe = System.Net.WebUtility.HtmlEncode(data.ArgDescribe ?? string.Empty)
            };

            return Json(new { success = true, data = safeData });
        }

        [HttpGet]
        public async Task<IActionResult> GetDeptList(string argVariable)
        {
            var data = await _sys202100Service.GetDeptListAsync(argVariable);
            return Json(new { success = true, data = data });
        }


        [HttpGet]
        public async Task<IActionResult> GetSchedList(string argVariable, int depNo)
        {
            var data = await _sys202100Service.GetSchedListAsync(argVariable, depNo);
            return Json(new { success = true, data = data });
        }
        #endregion

        #region 操作行為
        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateAllDept([FromBody] SaveAllDeptRequest request)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人事參數設定",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var result = await _sys202100Service.UpdateAllDeptAndSchedAsync(request.Depts, request.Scheds, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> Update([FromBody] Sys202100EditViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "資料驗證失敗" });

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人事參數設定",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var result = await _sys202100Service.UpdateAsync(model, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateDept([FromBody] Sys202100DeptListViewModel model)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人事參數設定",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var result = await _sys202100Service.UpdateDeptAsync(model, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteDept([FromBody] Sys202100DeptListViewModel model)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人事參數設定",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };

            var result = await _sys202100Service.DeleteDeptAsync(model.AgdNo, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateSched([FromBody] Sys202100SchedViewModel model)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人事參數設定",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var result = await _sys202100Service.UpdateSchedAsync(model, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteSched([FromBody] Sys202100SchedViewModel model)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "人事參數設定",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };

            var result = await _sys202100Service.DeleteSchedAsync(model.AgsNo, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 產生操作按鈕
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        private string GetDeptButtons(string id)
        {
            return $"<button class='icon-btn text-info openDept' data-id='{id}'><i class='fa fa-building'></i></button> ";
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        private string GetEditButtons(string id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Update))
                buttons += $"<button class='icon-btn text-primary editItem' data-id='{id}'><i class='fa-regular fa-pen-to-square'></i></button> ";
            return buttons;
        }
        #endregion
    }
}