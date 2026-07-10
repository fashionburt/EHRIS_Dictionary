using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using System.Diagnostics;

namespace EHRIS.Security.Middleware;

/*
 * 中介：UserActivityMiddleware
 * 用途：記錄使用者行為
 */
public class UserActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
     
    public UserActivityMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }
     
    public async Task InvokeAsync(HttpContext context)
    {
        var accLogin =
            context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Unknown";

        var sw = Stopwatch.StartNew();
        var startTime = DateTime.Now;
        var completed = false;

        
        using (LogContext.PushProperty("AccLogin", accLogin))
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            try
            {
                await _next(context);
                completed = true;
            }
            catch (Exception ex)
            {
                //Exception 
                Log.Error(ex,
                    "REQUEST_EXCEPTION | Path={Path} | Method={Method}",
                    context.Request.Path,
                    context.Request.Method
                );
                throw;
            }
            finally
            {
                sw.Stop();

                //正常完成
                if (completed)
                {
                    LogAction(context, startTime, sw.ElapsedMilliseconds, "Completed");
                }

                // NETWORK_UNSTABLE
                if (sw.ElapsedMilliseconds > 3000)
                {
                    LogNetworkUnstable(
                        context,
                        $"SlowRequest {sw.ElapsedMilliseconds}ms"
                    );
                }
            }
        }
       
    }
    private void LogAction(HttpContext context, DateTime start, long elapsedMs, string status)
    {
        var acc = context.User?.Identity?.Name ?? "Anonymous";
        var path = context.Request.Path;
        var ip = context.Connection.RemoteIpAddress?.ToString();

        Log.Information(
            "ACTION_TRACE | Acc={Acc} | Path={Path} | IP={IP} | Start={Start} | Elapsed={Elapsed}ms | Status={Status}",
            acc, path, ip, start.ToString("yyyy-MM-dd HH:mm:ss.fff"), elapsedMs, status
        );
    }

    private void LogNetworkUnstable(HttpContext context, string reason)
    {
        var acc = context.User?.Identity?.Name ?? "Anonymous";
        var path = context.Request.Path;

        Serilog.Log.Information(
            "NETWORK_UNSTABLE | Acc={Acc} | Path={Path} | Reason={Reason}",
            acc, path, reason
        );
    }
}

