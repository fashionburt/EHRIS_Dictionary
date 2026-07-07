using EHRIS.Tools.Crypto;
using EHRIS.Tools.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;
using static EHRIS.Tools.Crypto.CredentialManager;

namespace EHRIS.Tools.Email
{
    public class EmailSenderService : IEmailSenderService
    {
        //private readonly IMailSettingsProvider _settingsProvider;
        private readonly ILoggerAdapter _logger;
        private readonly LogContextEnricher _enricher;
        private Dictionary<string, MailSettings> _map;
 
        public EmailSenderService(
        //IMailSettingsProvider settingsProvider,
        ILoggerAdapter logger,
        LogContextEnricher enricher)
        {
           //_settingsProvider = settingsProvider;
            _logger = logger;
            _enricher = enricher;
        }

        /// <summary>
        /// 從外面設定寄件帳號密碼
        /// </summary>
        /// <param name="map"></param>
        public void SetProvider(Dictionary<string, MailSettings> map)
        {
            _map = map;
        }

        /// <summary>
        /// 取得設定值
        /// </summary>
        /// <param name="senderKey">寄件索引值</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Task<MailSettings> GetSettingsAsync(string senderKey)
        { 
            if (!_map.TryGetValue(senderKey, out var settings))
                throw new InvalidOperationException($"找不到寄件人設定：{senderKey}");

            return Task.FromResult(settings);
        }

        /// <summary>
        /// 寄信
        /// </summary>
        /// <param name="key">加解密金鑰</param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="to"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task SendMailAsync(string key, string subject, string body, string to, string filePath = null)
        {
            var _settings = await GetSettingsAsync("default");

            CredentialManager manager = new CredentialManager(key);
            var credential = new AccountCredential
            {
                Account = _settings.AdminMail,
                EncryptedPassword = _settings.AdminPass
            };
             
            string paintText = manager.DecryptPassword(credential, _settings.AdminMail);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.AdminMail));

            foreach (var address in to.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(address))
                    message.To.Add(MailboxAddress.Parse(address));
            }

            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                builder.Attachments.Add(filePath);

            message.Body = builder.ToMessageBody();
            var logEvent = _enricher.CreateLogEvent("Email", "Send");
            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.SMTP_Server, _settings.SMTP_Port, _settings.SMTP_SSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await smtp.AuthenticateAsync(_settings.AdminMail, paintText);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.Info($"寄信成功：{subject} → {to}", logEvent);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"寄信失敗：{subject} → {to}", logEvent);
                throw; // 可選擇是否重新拋出
            }

            #region 舊的方法
            //using var smtp = new SmtpClient(_settings.SMTP_Server, _settings.SMTP_Port)
            //{
            //    EnableSsl = _settings.SMTP_SSL,
            //    Credentials = new NetworkCredential(_settings.AdminMail, _settings.AdminPass)
            //};

            //using var mail = new MailMessage
            //{
            //    From = new MailAddress(_settings.AdminMail, _settings.DisplayName, Encoding.UTF8),
            //    Subject = subject,
            //    Body = body,
            //    IsBodyHtml = true,
            //    SubjectEncoding = Encoding.UTF8,
            //    BodyEncoding = Encoding.UTF8,
            //    HeadersEncoding = Encoding.UTF8
            //};

            //foreach (var address in to.Split(';'))
            //{
            //    if (!string.IsNullOrWhiteSpace(address))
            //        mail.To.Add(address);
            //}

            //if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            //{
            //    var att = new Attachment(filePath, MediaTypeNames.Application.Octet)
            //    {
            //        NameEncoding = Encoding.UTF8,
            //        ContentDisposition = { DispositionType = DispositionTypeNames.Attachment }
            //    };
            //    mail.Attachments.Add(att);
            //}

            //var logEvent = _enricher.CreateLogEvent("Email", "Send");

            //try
            //{
            //    await smtp.SendMailAsync(mail);
            //    _logger.Info("寄信成功", logEvent);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex, $"寄信失敗：{subject}", logEvent);
            //    throw;
            //}
            #endregion
        }

        /// <summary>
        /// 取得信件加密密文(取資料庫用)
        /// </summary>
        /// <param name="key">加解密金鑰</param>
        /// <param name="account"></param>
        /// <param name="plainPassword"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string EncryptMailPwd(string key, string account, string plainPassword)
        {
            CredentialManager manager = new CredentialManager(key);
            var encrypted = manager.EncryptCredential(account, plainPassword);
            return encrypted.EncryptedPassword;
        }
    }
}
