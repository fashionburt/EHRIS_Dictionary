using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
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
using System.Net;

namespace EHRIS.SysBasic.Controllers;

[Area("SYS201000")]
[Route("/[controller]/[action]")]
[RequireHttps]
public class SYS201002Controller : BaseController
{
    /// <summary>
    /// 設定此程式的程式編號
    /// </summary>
    protected const int SFUNO = 201002;
    private readonly IUserContextService _userContext;
    private readonly ISYS201002Service _sys201002Service;
    private readonly ICommonService _commonService;

    public SYS201002Controller(IUserContextService userContext, ISYS201002Service sys201002Service, ICommonService commonService) : base(commonService)
    {
        _userContext = userContext;
        _sys201002Service = sys201002Service;
        _commonService = commonService;
    }
    protected bool IsAjaxRequest()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
    [HttpGet]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)] //首次檢查權限
    public async Task<IActionResult> SYS201002()
    {
        //自動抓資料庫對應Breadcrumb
        await SetBreadcrumbAsync(SFUNO, "SYS201002", "SYS201002");

        ViewBag.AddStatus = HasPermission(FunctionAction.Insert);
        return PartialView("SYS201002");

    }

    #region 產生角色資料列表
    /// <summary>
        /// 取得角色列表
        /// </summary>
        /// <param name="request">來自前端的參數</param>
        /// <returns></returns>
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetAllRoleList([FromBody] DataTableRequest request)
    {
        var roles = await _sys201002Service.GetRoleList();
        if (roles == null)
        {
            roles = new List<RoleListViewModel>();
        }

        var total = roles.Count();
        var start = request?.start ?? 0;
        var length = request?.length ?? 10;

        var searchValue = request.extraSearch?.searchValue?.ToLower() ?? "";
        var searchColumns = request.extraSearch?.columnIndexes ?? new List<int>();

        if (request != null)
        {
            if (request.orderby != null && request.orderby.Count > 0)
            {
                var orderCol = request.orderby[0];
                var colName = request.columns[orderCol.column].data;

                if (orderCol.dir == "asc")
                {
                    roles = colName switch
                    {
                        "rol_no" => roles.OrderBy(r => r.rol_no).ToList(),
                        "rol_name" => roles.OrderBy(r => r.rol_name).ToList(),
                        "rol_memo" => roles.OrderBy(r => r.rol_memo).ToList(),
                        "role_open_text" => roles.OrderBy(r => r.rol_open).ToList(),
                        "rol_modifyname" => roles.OrderBy(r => r.rol_modifyname).ToList(),
                        "modifytime_text" => roles.OrderBy(r => r.rol_modifytime).ToList(),
                        _ => roles
                    };
                }
                else
                {
                    roles = colName switch
                    {
                        "rol_no" => roles.OrderByDescending(r => r.rol_no).ToList(),
                        "rol_name" => roles.OrderByDescending(r => r.rol_name).ToList(),
                        "rol_memo" => roles.OrderByDescending(r => r.rol_memo).ToList(),
                        "role_open_text" => roles.OrderByDescending(r => r.rol_open).ToList(),
                        "rol_modifyname" => roles.OrderByDescending(r => r.rol_modifyname).ToList(),
                        "modifytime_text" => roles.OrderByDescending(r => r.rol_modifytime).ToList(),
                        _ => roles
                    };
                }
            }
        }

        var filtered = string.IsNullOrEmpty(searchValue)
        ? roles
        : roles.Where(r =>
          searchColumns.Any(idx =>
            (idx == 0 && r.rol_name.ToLower().Contains(searchValue)) ||
            (idx == 1 && r.rol_memo.ToLower().Contains(searchValue)) ||
            (idx == 3 && r.rol_modifyname.ToLower().Contains(searchValue))
          )).ToList();

        var paged = filtered.Skip(start).Take(length).ToList();

        return Json(new
        {
            draw = request.draw,
            recordsTotal = roles.Count,
            recordsFiltered = filtered.Count(),
            data = paged.Select(r => new
            {
                rol_name = System.Net.WebUtility.HtmlEncode(r.rol_name ?? string.Empty),
                rol_memo = System.Net.WebUtility.HtmlEncode(r.rol_memo ?? string.Empty),
                role_open_text = r.rol_open == 1 ? "是" : "否",
                rol_modifyname = System.Net.WebUtility.HtmlEncode(r.rol_modifyname ?? string.Empty),
                modifytime_text = System.Net.WebUtility.HtmlEncode(r.rol_modifytime.ToRocDateTime()),
                peopleAction = GetPeopleButtons(r.rol_no, r.rol_name) ?? "",
                authorityAction = GetAuthorityButtons(r.rol_no, r.rol_name) ?? "",
                editAction = GetEditButtons(r.rol_no) ?? "",
                delAction = GetDelButtons(r.rol_no) ?? ""
            })
        });
    }

    #region 產生操作按鈕
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    private string GetPeopleButtons(int id, string rolName)
    {
        var buttons = "";
        if (HasPermission(FunctionAction.Update))
            buttons += $"<button class='icon-btn text-primary showPeople' data-id='{id}' data-rolname='{rolName}' ><i class=\"fa-solid fa-user-group\"></i></button> ";

        return buttons;
    }


    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    private string GetAuthorityButtons(int id, string rolName)
    {
        var buttons = "";
        if (HasPermission(FunctionAction.Update))
            buttons += $"<button class='icon-btn text-primary setRole' data-rolname='{rolName}' data-id='{id}'><i class=\"fa fa-cog\"></i></button> ";

        return buttons;
    }
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    private string GetEditButtons(int id)
    {
        var buttons = "";
        if (HasPermission(FunctionAction.Update))
            buttons += $"<button class='icon-btn text-primary editRole' data-id='{id}'><i class=\"fa-regular fa-pen-to-square\"></i></button> ";
        return buttons;
    }
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    private string GetDelButtons(int id)
    {
        var buttons = "";

        if (HasPermission(FunctionAction.Delete))
            buttons += $"<button class='icon-btn text-danger deleteRole' data-id='{id}'><i class=\"fa fa-trash\"></i></button>";
        return buttons;
    }
    #endregion
    #endregion 產生角色資料列表

    #region 抓單筆角色
    [HttpGet]
    public async Task<IActionResult> GetRoleByNo(int rol_no)
    {
        var role = await _sys201002Service.GetRoleByNoAsync(rol_no);
        if (role == null)
            return Json(new { success = false, message = "找不到資料" });

        role.RolName = System.Net.WebUtility.HtmlEncode(role.RolName ?? string.Empty);
        role.RolMemo = System.Net.WebUtility.HtmlEncode(role.RolMemo ?? string.Empty);
        role.RolCreateName = System.Net.WebUtility.HtmlEncode(role.RolCreateName ?? string.Empty);
        role.RolModifyName = System.Net.WebUtility.HtmlEncode(role.RolModifyName ?? string.Empty);

        return Json(new { success = true, data = role });
    }
    #endregion


    #region 新增角色
    [HttpPost]
    public async Task<IActionResult> AddRole([FromBody] Role role)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join("；", errors) });
        }
        string createUser = _userContext.UserName;
        role.RolCreateName = createUser;
        role.RolModifyName = createUser;
        var currentUserName = _userContext.UserName;

        WebDataLogger dataLogger = new WebDataLogger()
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "角色設定",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = En_DataEventMode.AddEvent
        };
        var result = await _sys201002Service.AddRoleAsync(role, currentUserName, dataLogger);

        if (!result.success)
        {
            return BadRequest(new { success = false, message = result.message });
        }
        return Ok(new { success = true, message = result.message });
    }
    #endregion
    #region 修改角色
    [HttpPost]
    public async Task<IActionResult> UpdateRole([FromBody] Role role)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "資料驗證失敗" });

        string createUser = _userContext.UserName;
        role.RolModifyName = createUser;
        role.RolModifyTime = DateTime.Now;
        var currentUserName = _userContext.UserName;

        WebDataLogger dataLogger = new WebDataLogger()
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "角色設定",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = En_DataEventMode.ModEvent
        };
        var result = await _sys201002Service.UpdateRoleAsync(role, currentUserName, dataLogger);

        if (!result.success)
        {
            return BadRequest(new { success = false, message = result.message });
        }
        return Ok(new { success = true, message = result.message });
    }

    #endregion
    #region 刪除角色
    [HttpPost]
    public async Task<IActionResult> DeleteRole([FromBody] Role role)
    {
        if (role == null) return BadRequest();
        var currentUserName = _userContext.UserName;

        WebDataLogger dataLogger = new WebDataLogger()
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "角色設定",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = En_DataEventMode.DelEvent
        };
        var result = await _sys201002Service.DeleteRoleAsync(role.RolNo, currentUserName, dataLogger);

        if (!result.success)
        {
            return BadRequest(new { success = false, message = result.message });
        }
        return Ok(new { success = true, message = result.message });
    }
    #endregion

    #region 權限設定
    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetPermission([FromBody] Role role)
    {
        var menuTree = await _sys201002Service.GetTreeAsync(role.RolNo);
        EncodeFunctionTree(menuTree);
        return Json(menuTree);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermission([FromBody] AuthorityViewModel request)
    {
        if (request == null) return BadRequest(new { success = false, message = "系統發生異常" });

        var currentUserName = _userContext.UserName;

        foreach (RolePermissionViewModel item in request.RolePermissions)
        {
            item.ModifyName = currentUserName;
            item.ModifyTime = DateTime.Now;
        }

        WebDataLogger dataLogger = new WebDataLogger()
        {
            ExecUID = _userContext.PeoUID,
            ExecSfuNo = SFUNO,
            ExecProName = "角色設定",
            ExecFromIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            ToPeoUID = _userContext.PeoUID,
            EventType = En_DataEventMode.ModEvent
        };

        var isSuccess = await _sys201002Service.SetRolePermissionAsync(request, currentUserName, dataLogger);

        if (isSuccess)
        {
            return Ok(new { success = true, message = "權限更新成功" });
        }
        else
        {
            return BadRequest(new { success = false, message = "權限更新失敗，請檢查後端日誌" });
        }
    }

    #endregion
    #region 人員明細

    [HttpPost]
    [AuthorizeFunction(SFUNO, FunctionAction.Query)]
    public async Task<IActionResult> GetPeople([FromBody] PeopleRequestViewModel request)
    {
        var roleId = 0;
        if (request != null)
        {
            roleId = request.rolNo;
        }

        var peopleList = await _sys201002Service.GetPeoplePermissionAsync(roleId);
        if (peopleList == null)
        {
            peopleList = new List<PeopleListModel>();
        }
        var total = peopleList.Count();
        var start = request?.start ?? 0;
        var length = request?.length ?? 10;

        var searchValue = request.extraSearch?.searchValue?.ToLower() ?? "";
        var searchColumns = request.extraSearch?.columnIndexes ?? new List<int>();

        if (request != null)
        {
            if (request.orderby != null && request.orderby.Count > 0)
            {
                var orderCol = request.orderby[0];
                var colName = request.columns[orderCol.column].data;

                if (orderCol.dir == "asc")
                {
                    peopleList = colName switch
                    {
                        "basName" => peopleList.OrderBy(r => r.BasName).ToList(),
                        "deptName" => peopleList.OrderBy(r => r.DeptName).ToList(),
                        "proName" => peopleList.OrderBy(r => r.ProName).ToList(),
                        _ => peopleList
                    };
                }
                else
                {
                    peopleList = colName switch
                    {
                        "basName" => peopleList.OrderByDescending(r => r.BasName).ToList(),
                        "deptName" => peopleList.OrderByDescending(r => r.DeptName).ToList(),
                        "proName" => peopleList.OrderByDescending(r => r.ProName).ToList(),

                        _ => peopleList
                    };
                }
            }
        }

        var filtered = string.IsNullOrEmpty(searchValue)
        ? peopleList
        : peopleList.Where(r =>
          searchColumns.Any(idx =>
            (idx == 0 && r.DeptName.ToLower().Contains(searchValue)) ||
            (idx == 1 && r.ProName.ToLower().Contains(searchValue)) ||
            (idx == 3 && r.BasName.ToLower().Contains(searchValue))
          )).ToList();

        var paged = filtered.Skip(start).Take(length).ToList();

        return Json(new
        {
            draw = request.draw,
            recordsTotal = peopleList.Count,
            recordsFiltered = filtered.Count(),
            data = paged.Select(r => new
            {
                DeptName = System.Net.WebUtility.HtmlEncode(r.DeptName ?? string.Empty),
                ProName = System.Net.WebUtility.HtmlEncode(r.ProName ?? string.Empty),
                BasName = System.Net.WebUtility.HtmlEncode(r.BasName ?? string.Empty),
            })
        });
    }
    #endregion

    private void EncodeFunctionTree(IEnumerable<FunctionTreeModel> nodes)
    {
        if (nodes == null) return;
        foreach (var node in nodes)
        {
            node.functionName = System.Net.WebUtility.HtmlEncode(node.functionName ?? string.Empty);
            if (node.Children != null && node.Children.Any())
            {
                EncodeFunctionTree(node.Children);
            }
        }
    }
}