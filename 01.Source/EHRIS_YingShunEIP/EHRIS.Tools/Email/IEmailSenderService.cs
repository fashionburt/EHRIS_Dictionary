using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Email
{
    public interface IEmailSenderService
    {
        void SetProvider(Dictionary<string, MailSettings> map);

        string EncryptMailPwd(string key, string account, string plainPassword);

        Task SendMailAsync(string key, string subject, string body, string to, string filePath = null);
    }
}
