using DNTCaptcha.Core;
using EHRIS.Core.DbContext;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Event;
using EHRIS.Core.Repositories.SysBasic;
using EHRIS.Core.Repositories.AdminPortal;
using EHRIS.Services.Services.SysBasic;
using EHRIS.Services.Services.AdminPortal;
using EHRIS.Security.Middleware;
using EHRIS.Security.Permission;
using EHRIS.Security.Permission.Filters;
using EHRIS.Security.User;
using EHRIS.Services.Common;
using EHRIS.Services.Services;
using EHRIS.Tools.Email;
using EHRIS.Tools.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;


// ---------------------- Serilog 設定----------------------
#region Serilog 設定
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Map(
        keySelector: logEvent =>
        {
            // 檢查 logEvent 是否有 AccLogin
            if (logEvent.Properties.TryGetValue("AccLogin", out var accProp))
            {
                var accLogin = accProp?.ToString().Trim('"');

                // 判斷空值或 "null"
                if (string.IsNullOrWhiteSpace(accLogin) || accLogin.Equals("null", StringComparison.OrdinalIgnoreCase))
                    return "Unknown";

                return accLogin;
            }

            return "Unknown";
        },
        configure: (acc_login, wt) =>
        {
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            wt.File(
                path: $"Logs/{dateFolder}/{acc_login}.log",
                 //rollingInterval: RollingInterval.Day,
                 //formatter: new Serilog.Formatting.Json.JsonFormatter(),
                 outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | {Message:lj}{NewLine}",
                retainedFileCountLimit: 30
            );
        }
    )
    //全域 Exception log（不分帳號）
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= Serilog.Events.LogEventLevel.Error)
        .WriteTo.File(
            path: $"Logs/{DateTime.Now:yyyy-MM-dd}/Exceptions.log",
            //rollingInterval: RollingInterval.Day,
            //formatter: new Serilog.Formatting.Json.JsonFormatter(),
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | {Message:lj}{NewLine}{Exception}",
            retainedFileCountLimit: 30
        )
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog 替代內建 Logger
builder.Host.UseSerilog();
#endregion

// ---------------------- MVC & Views ----------------------
// 註冊 HttpContextAccessor
builder.Services.AddHttpContextAccessor();  
#region MVC & Views
// 註冊 MVC Controller和View
var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    // 全域套用 RequireHttps
    options.Filters.Add<RequireHttpsAttribute>();

    // 套用授權功能 Filter
    options.Filters.Add<AuthorizeFunctionFilter>();

    // 套用閒置逾時 Filter
    options.AllowEmptyInputInBodyModelBinding = true;

});
// 掃描目前 AppDomain 裡所有組件
var assemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName.StartsWith("EHRIS."))
    .ToList();
//  確保 MVC 被啟用
foreach (var assembly in assemblies)
{
    mvcBuilder.AddApplicationPart(assembly);
}

builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    // 讓所有 View 都能共用 Web 專案的 Shared
    options.ViewLocationFormats.Add("~/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("~/Views/Shared/Tables/{0}.cshtml");
    options.ViewLocationFormats.Add("~/Views/_ViewImports.cshtml");
    options.ViewLocationFormats.Add("~/Views/_ViewStart.cshtml");

    // Area 下的 Controller 使用不同的路徑清單，需額外加入 Tables 路徑
    // {0}=ViewName  {1}=ControllerName  {2}=AreaName
    options.AreaViewLocationFormats.Add("/Views/Shared/Tables/{0}.cshtml");
});

// 註冊 DI by Singleton
builder.Services.AddSingleton<IDbHealthCheck, DbHealthCheck>();
builder.Services.AddSingleton<FieldsMappingService>();
// 註冊 DI by Scoped
builder.Services.AddScoped<IMenuRepository, MenuRepository>();


//權限
builder.Services.AddScoped<IPermissionsRepository, PermissionsRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<IArgumentsRepository, ArgumentsRepository>();


//操作紀錄
builder.Services.AddScoped<ILoggerAdapter, LoggerAdapter>();
//builder.Services.AddScoped<IEventObjectRepository, EventObjectRepository>();
builder.Services.AddScoped<LogContextEnricher>();

//共用服務
builder.Services.AddScoped<IOperatesRepository, OperatesRepository>();

builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IDeptRepository, DeptRepository>();
builder.Services.AddScoped<IPTypeRepository, PTypeRepository>();
builder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

builder.Services.AddScoped<ILoadSystemConfigRepository, LoadSystemConfigRepository>();
builder.Services.AddScoped<ISystemConfigService, SystemConfigService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<ISysVariableRepository, SysVariableRepository>();
builder.Services.AddScoped<IArgumentsRepository, ArgumentsRepository>();
builder.Services.AddScoped<IFileExportService, FileExportService>();
builder.Services.AddScoped<IFileDataRepository, FileDataRepository>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IPeoplePhotoService, PeoplePhotoService>();
builder.Services.AddScoped<IOrgSelectorService, OrgSelectorService>();
builder.Services.AddScoped<EHRIS.Core.Repositories.IOrgPeopleRepository, EHRIS.Core.Repositories.OrgPeopleRepository>();
builder.Services.AddScoped<EHRIS.Services.Common.IOrgPeopleService, EHRIS.Services.Common.OrgPeopleService>();


//登入服務
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();

#region SysBasic基本設定
//SysBasic
builder.Services.AddScoped<ISYS201001Repository, SYS201001Repository>();
builder.Services.AddScoped<ISYS201002Repository, SYS201002Repository>();
builder.Services.AddScoped<ISYS202001Repository, SYS202001Repository>();
builder.Services.AddScoped<ISYS202002Repository, SYS202002Repository>();
builder.Services.AddScoped<ISYS202003Repository, SYS202003Repository>();
builder.Services.AddScoped<ISYS202004Repository, SYS202004Repository>();
builder.Services.AddScoped<ISYS202005Repository, SYS202005Repository>();
builder.Services.AddScoped<ISYS202006Repository, SYS202006Repository>();
builder.Services.AddScoped<ISYS202100Repository, SYS202100Repository>();

builder.Services.AddScoped<ISYS201001Service, SYS201001Service>();
builder.Services.AddScoped<ISYS201002Service, SYS201002Service>();
builder.Services.AddScoped<ISYS202001Service, SYS202001Service>();
builder.Services.AddScoped<ISYS202002Service, SYS202002Service>();
builder.Services.AddScoped<ISYS202003Service, SYS202003Service>();
builder.Services.AddScoped<ISYS202004Service, SYS202004Service>();
builder.Services.AddScoped<ISYS202005Service, SYS202005Service>();
builder.Services.AddScoped<ISYS202006Service, SYS202006Service>();
builder.Services.AddScoped<ISYS202100Service, SYS202100Service>();
#endregion

#region AdminPortal後台管理
//AdminPortal
builder.Services.AddScoped<IADS999001Repository, ADS999001Repository>();
builder.Services.AddScoped<IADS999002Repository, ADS999002Repository>();
builder.Services.AddScoped<IADS999003Repository, ADS999003Repository>();
builder.Services.AddScoped<IADS999004Repository, ADS999004Repository>();
builder.Services.AddScoped<IADS999999Repository, ADS999999Repository>();

builder.Services.AddScoped<IADS999001Service, ADS999001Service>();
builder.Services.AddScoped<IADS999002Service, ADS999002Service>();
builder.Services.AddScoped<IADS999003Service, ADS999003Service>();
builder.Services.AddScoped<IADS999004Service, ADS999004Service>();
builder.Services.AddScoped<IADS999999Service, ADS999999Service>();
#endregion

#region Dictionary字典管理
//builder.Services.AddScoped<IDIC1999Repository, DIC1999Repository>();
//builder.Services.AddScoped<IDIC1999R01Repository, DIC1999R01Repository>();
//builder.Services.AddScoped<IDIC1999R02Repository, DIC1999R02Repository>();
//builder.Services.AddScoped<IDIC1998Repository, DIC1998Repository>();
//builder.Services.AddScoped<IDIC1997Repository, DIC1997Repository>();
//builder.Services.AddScoped<IDIC1996Repository, DIC1996Repository>();

//builder.Services.AddScoped<IDIC1999Service, DIC1999Service>();
//builder.Services.AddScoped<IDIC1999R01Service, DIC1999R01Service>();
//builder.Services.AddScoped<IDIC1999R02Service, DIC1999R02Service>();
//builder.Services.AddScoped<IDIC1998Service, DIC1998Service>();
//builder.Services.AddScoped<IDIC1997Service, DIC1997Service>();
//builder.Services.AddScoped<IDIC1996Service, DIC1996Service>();
#endregion

// 設定 DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
    , sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )));

//操作紀錄DbContext 
builder.Services.AddDbContext<AuditDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("LogConnection")));



#endregion
// ---------------------- Session ----------------------
#region Session
// Session Timeout
int sessionMinutes = builder.Configuration.GetValue<int>("Session:TimeoutMinutes");

//註冊 Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionMinutes); // Session 過期時間
    options.Cookie.HttpOnly = true; // 防止 JavaScript 存取
    options.Cookie.IsEssential = true;
});
#endregion
// ---------------------- Authentication & Authorization ----------------------
#region Authentication & Authorization
//  設定 Cookie 認證（這要在 builder.Build() 之前）
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {

        options.LoginPath = "/Account/Login";  //  未登入時導向
        options.LogoutPath = "/Account/Logout"; //登出時導向
        options.AccessDeniedPath= "/Account/AccessDenied"; // 無權限時導向

        // 如果驗證失敗，自動依照 PathBase 導向正確的 Login
        options.Events.OnRedirectToLogin = context =>
        {
            var pathBase = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : "";
            var redirectUri = context.RedirectUri;

            if (!string.IsNullOrEmpty(pathBase) && !redirectUri.StartsWith(pathBase))
            {
                redirectUri = pathBase + redirectUri;
            }

            context.Response.Redirect(redirectUri);
            return Task.CompletedTask;
        };


        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); //  設定 Cookie 失效時間
        options.SlidingExpiration = true; // 讓登入時間根據使用者行為動態延長
        options.Cookie.HttpOnly = true;             // 防止 JS 存取
       // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS 如果不需要用aspx, 使用這行
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;// (執行aspx用)需要設定為 None，否則第三方應用無法讀取
       options.Cookie.SameSite = SameSiteMode.Lax; // CSRF 防護（可調整）
    });

// 授權
builder.Services.AddAuthorization(options =>
{
    // 這裡定義 Policy
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddMyAppAuthorization();


#endregion
// ---------------------- DNT Captcha ----------------------
#region 驗證碼

//註冊驗證碼
var env = builder.Environment;
builder.Services.AddDNTCaptcha(options =>
{
    options.UseCookieStorageProvider()
           .AbsoluteExpiration(minutes: 5)  //設定驗證碼過期時間
           .ShowThousandsSeparators(false)
           .WithEncryptionKey("EHRISSecureCaptchaKey")
           .UseCustomFont(Path.Combine(env.WebRootPath, "fonts", "arial.ttf"))
           .InputNames(
               new DNTCaptchaComponent
               {
                   CaptchaInputName = "DNTCaptchaInputText",
                   CaptchaHiddenInputName = "DNTCaptchaText",
                   CaptchaHiddenTokenName = "DNTCaptchaToken"

               })
           .Identifier("dntCaptcha")
           ;
});

#endregion

var app = builder.Build();
// ---------------------- Middleware ----------------------
#region  Middleware
// 全域例外處理
app.UseMiddleware<EHRIS.Security.Middleware.ExceptionMiddleware>();
app.UseMiddleware<EHRIS.Security.Middleware.RequestSanitizerMiddleware>();
app.UseMiddleware<EHRIS.Security.Middleware.NetworkStabilityMiddleware>();





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            var ex = feature?.Error;

            Log
                .ForContext("LogType", "CustomerCare")
                .ForContext("UserAction", "UnhandledException")
                .ForContext("Page", context.Request.Path)
                .Error(ex, "系統錯誤");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("系統發生錯誤");
        });
    });
    app.UseHsts();
}
#endregion
// DB Health Check Middleware
#region DB Health Check Middleware
var whiteList = new[] { "/Account/login", "/" };
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    if (whiteList.Any(x => path.StartsWith(x)))
    {
        await next();
        return;
    }
    var dbHealth = context.RequestServices.GetRequiredService<IDbHealthCheck>();
    if (!dbHealth.CanConnect().Success)
    {
        // 傳回一個最小 HTML，避免 Layout 嘗試查資料庫
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(@"
             <!DOCTYPE html>
            <html>
            <head>
                <title>系統錯誤</title>
                
            </head>
            <body>
                <script>
                    Swal.fire({
                        icon: 'error',
                        title: '資料庫無法連線',
                        text: '請稍後再試',
                        confirmButtonText: '確定'
                    });
                </script>
            </body>
            </html>");
    }
    else
    {
        await next();
    }
});
#endregion
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var contentType = ctx.Context.Response.ContentType;
        if (!string.IsNullOrEmpty(contentType) && !contentType.Contains("charset"))
        {
            if (contentType.StartsWith("text/") || contentType == "application/json")
            {
                ctx.Context.Response.Headers["Content-Type"] = contentType + "; charset=utf-8";
            }
        }
    }
});

app.UseRouting();
app.UseSession(); //開啟 Session  在 app.UseRouting() 之後，UseAuthorization() 之前

//app.UseUserActivity();      // 記錄使用者行為  app.UseMiddleware<UserActivityMiddleware>();

// 加入驗證與授權中介軟體（順序很重要）
app.UseAuthentication(); //開啟驗證
app.UseAuthorization();  // 檢查 [Authorize]

app.UseMiddleware<UserActivityMiddleware>();

app.MapControllers();
//確保預設路由為 Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");




app.Run();
