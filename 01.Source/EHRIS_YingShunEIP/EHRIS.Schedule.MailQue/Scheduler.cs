using EHRIS.Core.Models.Common;
using EHRIS.Services.Common;
using EHRIS.Services.Services.Schedule;
using EHRIS.Tools.Crypto;
using EHRIS.Tools.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
    
namespace EHRIS.Schedule.MailQue
{
    public class Scheduler : BackgroundService 
    {
        private int _cachedIntervalSeconds = 10; // 預設值
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheRefreshInterval = TimeSpan.FromMinutes(1);

        private readonly ISystemConfigService _systemConfigService;

        private readonly ILogger<Scheduler> _logger;
        //private readonly IMailSettingsProvider _mailSettingsProvider;
        private readonly IEmailSenderService _emailSender;
        private readonly IMailQueService _emailQueService;
        private readonly ISchConfigService _config;
        private readonly SchedulerOptions _options;

        public Scheduler(ILogger<Scheduler> logger, 
            //IMailSettingsProvider mailSettingsProvider, 
            IEmailSenderService emailSender, 
            IMailQueService service, 
            ISchConfigService config, 
            IOptions<SchedulerOptions> options, 
            ISystemConfigService systemConfigService)
        { 
            _logger = logger;
            //_mailSettingsProvider = mailSettingsProvider;
            _emailSender = emailSender;
            _emailQueService = service;
            _config = config;
            _options = options.Value;

            _systemConfigService = systemConfigService;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("排程啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var interval = await GetIntervalFromDbAsync();
                    //_logger.LogInformation("目前排程間隔：{Interval} 秒", interval);

                    await DoWork();

                    await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "排程執行失敗");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // fallback 間隔
                }
            }

           // _logger.LogInformation("排程已停止"); 
        }

        private async Task<int> GetIntervalFromDbAsync()
        {
            var now = DateTime.UtcNow;

            if (now - _lastCacheUpdate < _cacheRefreshInterval)
            {
                return _cachedIntervalSeconds; // 使用快取
            }

            try
            {
                var configList = await _config.LoadSystemInfo("MailQue");
                foreach (var item in configList)
                {
                    //Console.WriteLine($"{item.scc_variable} = {item.scc_value}");
                    if (item.scc_variable.Trim() == "IntervalSeconds")
                    {
                        int IntervalSeconds = 10;
                        int.TryParse(item.scc_value, out IntervalSeconds);

                        _cachedIntervalSeconds = IntervalSeconds;
                        _lastCacheUpdate = now;
                    }
                }

                //if (config != null && config.IntervalSeconds > 0)
                //{
                //    _cachedIntervalSeconds = config.IntervalSeconds;
                //    _lastCacheUpdate = now;
                //}
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "無法更新排程間隔快取，使用上次快取值");
            }

            return _cachedIntervalSeconds;

        }


        private async Task DoWork()
        {
            var masterKey = Environment.GetEnvironmentVariable("EHRISKey");
            _systemConfigService.LoadSystemInfo(masterKey);

            var mailSettingsMap = new Dictionary<string, MailSettings>
            {
                ["default"] = new MailSettings
                {
                    SMTP_Server = AppConfig.syi_smtpServer,
                    SMTP_Port =AppConfig.syi_smtpport,
                    SMTP_SSL = AppConfig.syi_smtpssl,
                    AdminMail = AppConfig.syi_emailaddr,
                    AdminPass = AppConfig.syi_emailpwd,
                    DisplayName =  AppConfig.syi_name
                }
            };
             
            _emailSender.SetProvider(mailSettingsMap);
             
            //_logger.LogInformation("排程執行時間：{Time}", DateTime.Now);
            var dataList = await _emailQueService.GetMailSendQueList();
            _logger.LogInformation("共有 {Count} 筆待處理", dataList.Count);

            foreach (var data in dataList)
            {
                //寄信
                var subject = data.mai_subject;// "系統通知";
                var body = data.mai_content;//"<b>這是來自背景服務的通知</b>";
                var to = data.mai_email;

                try
                {
                    await _emailSender.SendMailAsync("Schedule.MailQue", subject, body, to);
                    await _emailQueService.UpdateMailMessageAsync(data.mai_no, "1", AppDomain.CurrentDomain.FriendlyName);

                    _logger.LogInformation("信件({mai_no}) 已送出", data.mai_no);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "信件({mai_no}) 已送出", data.mai_no);
                    throw;
                }

            }
        }

    }

}
