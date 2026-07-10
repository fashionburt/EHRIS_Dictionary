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
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.SysBasic.Controllers
{
    [Area("SYS202000")]
    [Route("[controller]/[action]")]
    public class SYS202001Controller : BaseController
    {
        protected const int SFUNO = 202001;
        private readonly IUserContextService _userContext;
        private readonly ISYS202001Service _sys202001Service;
        private readonly ICommonService _commonService;

        public SYS202001Controller(IUserContextService userContext, ISYS202001Service sys202001Service, ICommonService commonService) : base(commonService)
        {
            _userContext = userContext;
            _sys202001Service = sys202001Service;
            _commonService = commonService;
        }

        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> SYS202001()
        {
            await SetBreadcrumbAsync(SFUNO, "SYS202001", "SYS202001");
            ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202001");
        }

        #region 取得資料
        private Task<ErrorCheckUIDataViewModel> CheckUI(DepartmentUpdateModel model)
        {
            var checkResult = new ErrorCheckUIDataViewModel();
            var errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(model.deptCode))
            {
                errorMessages.Add("部門代碼為必填項。");
            }
            if (string.IsNullOrWhiteSpace(model.deptName))
            {
                errorMessages.Add("部門名稱為必填項。");
            }
            if (model.deptOrder < 0)
            {
                errorMessages.Add("排序編號不可為負數。");
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
        public async Task<IActionResult> GetDepartments([FromBody] DepartmentDataTableRequest request)
        {
            var serviceResponse = await _sys202001Service.GetDepartmentsForDataTableAsync(request);
            var jsonData = new
            {
                draw = serviceResponse.draw,
                recordsTotal = serviceResponse.recordsTotal,
                recordsFiltered = serviceResponse.recordsFiltered,
                data = serviceResponse.data.Select(d => new
                {
                    d.deptId,
                    d.deptCode,
                    d.deptName,
                    d.deptOrder,
                    d.deptModifyName,
                    deptModifyTime = d.deptModifyTime.ToRocDateTime(),
                    editAction = GetEditButton(d.deptId, "btn-edit"),
                    deleteAction = GetDeleteButton(d.deptId, d.deptName, "btn-delete")
                }).ToList()
            };
            return Ok(jsonData);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetEditDepartmentPartial(int id)
        {
            var viewModel = await _sys202001Service.GetEditDepartmentViewModelAsync(id);
            if (viewModel == null)
            {
                return Content("<p class='text-center text-danger'>無法載入部門資料。</p>");
            }
            viewModel.CanUpdate = HasPermission(FunctionAction.Update);
            viewModel.CanInsert = HasPermission(FunctionAction.Insert);
            return PartialView("SYS202001EDT", viewModel);
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetSubDepartments([FromBody] DepartmentDataTableRequest request, [FromQuery] int parentId)
        {
            var serviceResponse = await _sys202001Service.GetSubDepartmentsForDataTableAsync(parentId, request);
            const int MAX_LEVEL = 100;
            var jsonData = new
            {
                draw = serviceResponse.draw,
                recordsTotal = serviceResponse.recordsTotal,
                recordsFiltered = serviceResponse.recordsFiltered,
                data = serviceResponse.data.Select(d => new
                {
                    d.deptId,
                    d.deptCode,
                    d.deptName,
                    d.deptOrder,
                    d.deptModifyName,
                    deptModifyTime = d.deptModifyTime.ToRocDateTime(),
                    editAction = GetSubEditButton(d.deptId, d.dep_level),
                    deleteAction = GetDeleteButton(d.deptId, d.deptName, "btn-delete-sub")
                }).ToList()
            };
            return Ok(jsonData);
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetDepartmentDetails(int id)
        {
            var details = await _sys202001Service.GetDepartmentDetailsAsync(id);
            if (details == null)
            {
                return NotFound(new { success = false, message = "找不到指定的部門資料。" });
            }
            return Ok(new { success = true, data = details });
        }

        [HttpGet]
        [AuthorizeFunction(SFUNO, FunctionAction.Query)]
        public async Task<IActionResult> GetSuggestedSubDeptCode(int parentId)
        {
            var suggestedCode = await _sys202001Service.GetSuggestedSubDeptCodeAsync(parentId);
            if (string.IsNullOrEmpty(suggestedCode))
            {
                return Ok(new { success = false, message = "無法產生建議代碼 (可能已達上限或父部門不存在)。" });
            }
            return Ok(new { success = true, data = suggestedCode });
        }
        #endregion

        #region 操作行為
        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Insert)]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentUpdateModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "提交的資料無效或格式錯誤。" });
            }

            var checkResult = await CheckUI(model);
            if (!checkResult.Result)
            {
                return BadRequest(new { success = false, message = checkResult.ErrorMessage });
            }

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "部門資訊管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.AddEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202001Service.CreateDepartmentAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Update)]
        public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentUpdateModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "提交的資料無效或格式錯誤。" });
            }

            var checkResult = await CheckUI(model);
            if (!checkResult.Result)
            {
                return BadRequest(new { success = false, message = checkResult.ErrorMessage });
            }

            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "部門資訊管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.ModEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202001Service.UpdateDepartmentAsync(model, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }

        [HttpPost]
        [AuthorizeFunction(SFUNO, FunctionAction.Delete)]
        public async Task<IActionResult> DeleteDepartment([FromBody] DeleteRequestModel request)
        {
            WebDataLogger dataLogger = new WebDataLogger()
            {
                ExecUID = _userContext.PeoUID,
                ExecSfuNo = SFUNO,
                ExecProName = "部門資訊管理",
                ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                ToPeoUID = _userContext.PeoUID,
                EventType = En_DataEventMode.DelEvent
            };
            var currentUserName = _userContext.UserName;
            var result = await _sys202001Service.SoftDeleteDepartmentAsync(request.id, dataLogger, currentUserName);

            if (!result.success)
            {
                return BadRequest(new { success = false, message = result.message });
            }

            return Ok(new { success = true, message = result.message });
        }
        #endregion

        #region 產生標準操作按鈕
        private string GetEditButton(int id, string cssClass = "btn-edit")
        {
            if (!HasPermission(FunctionAction.Update)) return "";
            return $"<button type='button' class='icon-btn text-primary {cssClass}' data-id='{id}' title='修改'><i class=\"fa-regular fa-pen-to-square\"></i></button>";
        }
        private string GetDeleteButton(int id, string name = "", string cssClass = "btn-delete")
        {
            if (!HasPermission(FunctionAction.Delete)) return "";
            var nameAttribute = string.IsNullOrEmpty(name) ? "" : $"data-name='{name}'";
            return $"<button type='button' class='icon-btn text-danger {cssClass}' data-id='{id}' {nameAttribute} title='刪除'><i class=\"fa fa-trash\"></i></button>";
        }
        private string GetSubEditButton(int id, int level)
        {
            if (!HasPermission(FunctionAction.Update)) return "";
            const int MAX_LEVEL = 100; // 最大層級，根據實際需求調整，controller、Service相同
            if (level >= MAX_LEVEL)
            {
                return $"<button type='button' class='icon-btn text-primary btn-edit-sub' data-id='{id}' title='修改'><i class=\"fa-regular fa-pen-to-square\"></i></button>";
            }
            else
            {
                return $"<button type='button' class='icon-btn text-primary btn-drill-down' data-id='{id}' title='修改並檢視子部門'><i class=\"fa-regular fa-pen-to-square\"></i></button>";
            }
        }
        #endregion
    }
}