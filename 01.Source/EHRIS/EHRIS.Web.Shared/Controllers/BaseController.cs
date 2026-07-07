using EHRIS.Security.Permission.Contracts;
using EHRIS.Security.Permission.Enums;
using EHRIS.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace EHRIS.Web.Shared.Controllers
{ 
    /// <summary>
    /// 提供功能代碼與操作行為的共用控制器基底類別
    /// </summary>
    public abstract class BaseController : Controller, IFunctionContextSetter
    {
        private readonly ICommonService _commonService; 

        protected string CurrentUser =>
       User?.Identity?.IsAuthenticated == true
           ? User.Identity.Name
           : "Anonymous";

        protected BaseController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        protected async Task SetBreadcrumbAsync(int sfuno, string actionName, string controllerName, string subViewTitle="")
        {
            var (SYSNAME, SFUMAINNAME, SFUNAME) = await _commonService.GetFunctionNamesAsync(sfuno);

            // 保存到屬性            
            SfuName = SFUNAME;
             
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                (SYSNAME, ""),
                (SFUMAINNAME, ""),
                (GetHeaderTitle(SFUNAME, subViewTitle), Url.Action(actionName, controllerName))
            };
             
            ViewData["Title"] = GetHeaderTitle(SFUNAME, subViewTitle);

        } 

        private string GetHeaderTitle(string baseTitle, string subTitle)
        {
            return string.IsNullOrWhiteSpace(subTitle)
                ? baseTitle
                : $"{baseTitle} - {subTitle}";
        }

        protected void SetError(string message)
        {
            TempData["SwalError"] = message;
        }

        protected void SetSuccess(string message)
        {
            TempData["SwalSuccess"] = message;
        }

        protected void SetWarning(string message)
        {
            TempData["SwalWarning"] = message;
        }


        /// <summary>
        /// 功能代碼（程式編號）
        /// </summary>
        protected int SfuNo { get; private set; }

        /// <summary>
        /// 功能名稱（SFUNAME）
        /// </summary>
        protected string SfuName { get; private set; }

        /// <summary>
        /// 目前操作行為（查詢、新增、修改、刪除等）
        /// </summary>
        protected FunctionAction CurrentAction { get; private set; }

        /// <summary>
        /// 設定目前操作的功能代碼與行為，並同步至 ViewBag
        /// </summary>
        /// <param name="sfuNo">功能代碼</param>
        /// <param name="action">操作行為</param>
        public void SetFunctionContext(int sfuNo, FunctionAction action)
        {
            SfuNo = sfuNo;
            CurrentAction = action;

            ViewBag.SfuNo = SfuNo;
            ViewBag.FunctionAction = action;
        }

        /// <summary>
        /// 檢查目前使用者是否擁有指定操作的權限
        /// </summary>
        /// <param name="action">操作行為</param>
        /// <returns>是否有權限</returns>
        public bool HasPermission(FunctionAction action)
        {
            var claimKey = GetClaimKey(action);
            return User.HasClaim(claimKey, "true");
        }

        /// <summary>
        /// 取得指定操作的 Claim 權限鍵值
        /// </summary>
        /// <param name="action">操作行為</param>
        /// <returns>Claim Key</returns>
        protected string GetClaimKey(FunctionAction action)
        {
            return $"FUNCTION:{SfuNo}:{action}";
        }
        /// <summary>
        /// 每個功能執行時, 都Log
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            
            base.OnActionExecuted(context);

            try
            {
                // 取得帳號
                var accLogin = User.Identity?.Name ?? "Unknown";

                // 取得 Controller/Action
                var page = $"{context.ActionDescriptor.RouteValues["controller"]}/{context.ActionDescriptor.RouteValues["action"]}";

                // 取得網路資訊
                var network = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

                // 訊息
                var message = context.HttpContext.Request.Method + " " + context.HttpContext.Request.Path;

              
                // 寫入 Serilog，使用 Map 按帳號分檔
                Log.Information(
                    "CONTROLLER_EXEC | Acc= {Acc} | IP={IP} | Path={Page} | Message={Message}",
                    
                    accLogin,
                    network,
                    page,
                    message
                );
            }
            catch
            {
                // 防呆，避免 Log 本身造成例外
            }
        }


    
       
        /// <summary>
        /// 某些要記錄的指定動作
        /// 例:LogCustomerAction("點擊查詢按鈕，日期:2025-12-18");
        /// </summary>
        /// <param name="message"></param>
        protected void LogCustomerAction(string message)
        {
            var accLogin = User.Identity?.Name ?? "Unknown";
            var page = $"{RouteData.Values["controller"]}/{RouteData.Values["action"]}";
            var network = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            Log.ForContext("AccLogin", accLogin)
               .ForContext("Page", page)
               .ForContext("Network", network)
               .Information("{Message}", message);
        }

    }


}
