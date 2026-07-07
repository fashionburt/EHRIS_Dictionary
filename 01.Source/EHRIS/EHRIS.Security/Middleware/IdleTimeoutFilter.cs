using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

public class IdleTimeoutFilter : IActionFilter
{
    private readonly int _idleSeconds;

    public IdleTimeoutFilter(IConfiguration config)
    {
        _idleSeconds = config.GetValue<int>("IdleSettings:IdleTimeoutSeconds");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var http = context.HttpContext;
        var session = http.Session;

        var lastActivity = session.GetString("LastActivity");

        if (lastActivity != null)
        {
            if (DateTime.TryParse(lastActivity, out var dt))
            {
                if ((DateTime.Now - dt).TotalSeconds > _idleSeconds)
                {
                    session.Clear();

                    context.Result = new RedirectToActionResult("Index", "Home", new
                    {
                        timeout = 1
                    });
                    return;
                }
            }
        }

        session.SetString("LastActivity", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
