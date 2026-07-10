using EHRIS.Core.DbContext;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Schedule;
using EHRIS.Schedule.MailQue;
using EHRIS.Services.Common;
using EHRIS.Services.Services;
using EHRIS.Services.Services.Schedule;
using EHRIS.Tools.Email;
using EHRIS.Tools.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
  
        services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")
    , sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDeptRepository, DeptRepository>();
        services.Configure<SchedulerOptions>(config.GetSection("Scheduler"));

        services.AddScoped<ILoadSystemConfigRepository, LoadSystemConfigRepository>();
        services.AddScoped<ISystemConfigService, SystemConfigService>();

        services.AddScoped<IDbHealthCheck, DbHealthCheck>();

        //寄件資料服務
        services.AddScoped<IMailQueService, MailQueService>();
        services.AddScoped<IMailQueRepository, MailQueRepository>();

        services.AddScoped<ISystemConfigService, SystemConfigService>();
        //共用服務
        services.AddScoped<ICommonService, CommonService>();

        //排程參數
        services.AddScoped<ISchConfigRepository, SchConfigRepository>();
        services.AddScoped<ISchConfigService, SchConfigService>();

        //寄信元件
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        //services.AddScoped<IMailSettingsProvider, MailSettingsProvider>(); 
        services.AddScoped<ILoggerAdapter, LoggerAdapter>();
        

        services.AddHttpContextAccessor(); // 若尚未加入，這是 LogContextEnricher 的依賴
        services.AddSingleton<LogContextEnricher>();
        services.AddMemoryCache();

        services.AddHostedService<Scheduler>();
    })
    
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });

builder.Build().Run();