using EHRIS.Tools.Email;
using EHRIS.Tools.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EHRIS.Tools.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTools(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddTransient<IEmailSenderService, EmailSenderService>();

            //// Crypto
            //services.AddSingleton<ICryptoService, AesGcmCryptoService>();
            //services.AddSingleton<Argon2Hasher>();

            // Logging
            services.AddSingleton<ILoggerAdapter, LoggerAdapter>();
            services.AddSingleton<LogContextEnricher>();

            //// Scheduling
            //services.AddSingleton<ISchedulerService, SchedulerService>();

            //// Common
            //services.AddSingleton<IClock, SystemClock>();

            return services;
        }
    }

}
