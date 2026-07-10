using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EHRIS.Security.Middleware;
public class NetworkStabilityMiddleware
{
    private readonly RequestDelegate _next;

    //全站共用
    private static readonly ConcurrentDictionary<string, DateTime> _lastRequest
        = new();

    public NetworkStabilityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        //重複請求判斷
        if (IsRepeatedRequest(context))
        {
            LogUnstable(context, "RepeatedRequest");
        }

        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
            LogUnstable(context, "RequestCanceled");
            throw;
        }
        finally
        {
            sw.Stop();

            //Request比軟久的判斷
            if (sw.ElapsedMilliseconds > 3000)
            {
                LogUnstable(context, $"SlowRequest {sw.ElapsedMilliseconds}ms");
            }
        }
    }

   
    private bool IsRepeatedRequest(HttpContext context)
    {
        // 沒登入則不用判斷
        var acc = context.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(acc))
            return false;

        // 同帳號 + 同路徑
        var key = $"{acc}:{context.Request.Path}";

        var now = DateTime.Now;

        if (_lastRequest.TryGetValue(key, out var last)
            && (now - last).TotalSeconds < 2)
        {
            return true;
        }

        _lastRequest[key] = now;
        return false;
    }

    private void LogUnstable(HttpContext context, string reason)
    {
        var acc = context.User?.Identity?.Name ?? "Anonymous";
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var path = context.Request.Path;

        Serilog.Log.Information(
            "NETWORK_UNSTABLE | Acc={Acc} | IP={IP} | Path={Path} | Reason={Reason}",
            acc, ip, path, reason
        );
    }
}
