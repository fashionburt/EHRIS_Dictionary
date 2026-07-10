using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services;
using EHRIS.Tools.Extensions;
using EHRIS.Tools.Web;
using EHRIS.Web.Shared.Controllers;
using EHRIS.Security.Permission.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Controllers
{
    [Area("SYS202000")]
    [Route("/[controller]/[action]")]
    public class SYS202005Controller : BaseController
    {
        protected const int SFUNO = 202005;
        private readonly IUserContextService _userContext;
        private readonly ISYS202005Service _sys202005Service;
        private readonly ICommonService _commonService;

        public SYS202005Controller(IUserContextService userContext, ISYS202005Service sys202005Service, ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202005Service = sys202005Service;
            _commonService = commonService;
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202005()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202005", "SYS202005");
            ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202005");
        }

        #region 產生職等資料列表

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetList([FromBody] Sys202005DataTableRequest request)
        {
            var (data, recordsFiltered, recordsTotal) = await _sys202005Service.GetListAsync(request);

            return Json(new
            {
                draw = request.draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data.Select(r => new
                {
                    pleCode = r.PleCode,
                    pleName = r.PleName,
                    pleModifyName = r.PleModifyName,
                    pleModifyTimeDisplay = r.PleModifyTime.ToRocDateTime(),
                    editAction = GetEditButtons(r.PleNo) ?? "",
                    delAction = GetDelButtons(r.PleNo) ?? ""
                })
            });
        }
        #endregion

        #region 抓單筆職等

        [HttpGet]
        public async Task<IActionResult> GetByNo(int pleNo)
        {
            var data = await _sys202005Service.GetByNoAsync(pleNo);
            if (data == null)
                return Json(new { success = false, message = "找不到資料" });

            var safeData = new
            {
                PleNo = data.PleNo,
                PleCode = System.Net.WebUtility.HtmlEncode(data.PleCode ?? string.Empty),
                PleName = System.Net.WebUtility.HtmlEncode(data.PleName ?? string.Empty),
            };

            return Json(new { success = true, data = safeData });
        }

        #endregion

        #region 新增職等
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> Add([FromBody] Sys202005EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("；", errors) });
            }

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "職等資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };

            var result = await _sys202005Service.AddAsync(model, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 修改職等
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> Update([FromBody] Sys202005EditViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "資料驗證失敗" });

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "職等資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };

            var result = await _sys202005Service.UpdateAsync(model, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 刪除職等

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] Sys202005EditViewModel model)
        {
            if (model == null) return BadRequest();

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "職等資料管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };
            var result = await _sys202005Service.DeleteAsync(model.PleNo, _userContext.UserName, dataLogger);
            return Json(new { success = result.success, message = result.message });
        }

        #endregion

        #region 產生操作按鈕

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        private string GetEditButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Update))
                buttons += $"<button class='icon-btn text-primary editItem' data-id='{id}'><i class=\"fa-regular fa-pen-to-square\"></i></button> ";
            return buttons;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        private string GetDelButtons(int id)
        {
            var buttons = "";
            if (HasPermission(FunctionAction.Delete))
                buttons += $"<button class='icon-btn text-danger deleteItem' data-id='{id}'><i class=\"fa fa-trash\"></i></button>";
            return buttons;
        }

        #endregion
    }
}