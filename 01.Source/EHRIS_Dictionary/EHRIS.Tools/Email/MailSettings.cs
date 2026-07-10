using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Email
{
    public class MailSettings
    {
        public string SMTP_Server { get; set; } = "";
        public int SMTP_Port { get; set; } = 25;
        public bool SMTP_SSL { get; set; } = false;
        public string AdminMail { get; set; } = "";
        public string AdminPass { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }
}
