using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using EHRIS.Core.Repositories;

namespace EHRIS.Security.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IDbHealthCheck dbHealthCheck)
    {
       
        // 白名單：不檢查靜態檔、Login API / 頁面
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/Error") ||
        path.StartsWith("/lib") ||
        path.StartsWith("/css") ||
        path.StartsWith("/js") )
        {
            await _next(context);
            return;
        }
        try
        {
            var result = dbHealthCheck.CanConnect();
            if (!result.Success)
            {
                // 將錯誤訊息存到 HttpContext Items
                context.Items["DbErrorMessage"] = result.Message;
            }

            await _next(context);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "資料庫連線錯誤");
            // 如果真的沒捕捉到，可存錯誤訊息
            context.Items["DbErrorMessage"] = "資料庫無法連線，請聯絡系統管理員";
            // 直接回原頁面，不要拋出例外
            context.Response.StatusCode = StatusCodes.Status200OK;
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未預期錯誤");
            context.Items["DbErrorMessage"] = "未預期錯誤";
            context.Response.StatusCode = StatusCodes.Status200OK;
            return;
        }
    }
}
