using EHRIS.Core.Models;
using EHRIS.Core.Models.SysBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories.Schedule
{
    public interface IMailQueRepository : IBaseRepository
    {
        Task<List<MailQueSendViewModel>> GetMailSendQueListAsync(string mai_status, int mai_errortimes);

        Task<Entities.MailMessage> GetMailMessageAsync(int mai_no);

        Task<bool> UpdateMailMessageAsync(Entities.MailMessage data);

        
    }
}
