using EHRIS.Security.Permission.Attributes;
using EHRIS.Security.Permission.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Security.Permission.Filters
{
    /// <summary>
    /// 根據 AuthorizeFunctionAttribute 自動設定功能上下文並檢查權限
    /// </summary>
    public class AuthorizeFunctionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller as IFunctionContextSetter;
            if (controller == null)
            {
                await next();
                return;
            }

            var methodInfo = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
            var attr = methodInfo?.GetCustomAttributes(typeof(AuthorizeFunctionAttribute), false)
                                 .Cast<AuthorizeFunctionAttribute>()
                                 .FirstOrDefault();

            if (attr == null)
            {
                await next();
                return;
            }

            controller.SetFunctionContext(attr.SfuNO, attr.Action);

            if (!controller.HasPermission(attr.Action))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            await next();
        }
    }


}
