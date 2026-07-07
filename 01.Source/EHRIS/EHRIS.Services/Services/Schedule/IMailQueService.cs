using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Models.SysBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Services.Schedule
{
    public interface IMailQueService
    {
        Task<List<MailQueSendViewModel>> GetMailSendQueList();

        Task<(bool success, string message)> UpdateMailMessageAsync(int mai_no, string mai_status, string who);
    }
}
