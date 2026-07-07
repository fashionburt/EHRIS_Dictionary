using DocumentFormat.OpenXml.Presentation;
using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Models.SysBasic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories.Schedule
{
    public class MailQueRepository :  BaseRepository, IMailQueRepository
    {
        public MailQueRepository(ApplicationDbContext context, IUnitOfWork unit) : base(context, unit) { }
         
        /// <summary>
        /// 取得信件清單(不可異動)
        /// </summary>
        /// <param name="mai_status"></param>
        /// <returns></returns>
        private IQueryable<MailMessage> GetMailQueQueryableNoTracking(string mai_status)
        {
            var query = _context.MailMessage.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(mai_status))
                query = query.Where(x => x.MaiStatus == mai_status);

            return query;
        }


        /// <summary>
        /// 取得寄送的信件清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<MailQueSendViewModel>> GetMailSendQueListAsync(string mai_status, int mai_errortimes)
        {
            return await GetMailQueQueryableNoTracking(mai_status)
                .Where(x => x.MaiErrorTimes <= mai_errortimes)
                .Select(x => new MailQueSendViewModel
                {
                    mai_no = x.MaiNo,
                    mai_email = x.MaiEmail,
                    mai_bcc = x.MaiBCC,
                    mai_cc = x.MaiCC,
                    mai_subject = x.MaiSubject,
                    mai_content = x.MaiContent,
                    mai_errortimes = x.MaiErrorTimes
                }).ToListAsync();
        }
          
        public async Task<bool> UpdateMailMessageAsync(MailMessage data)
        {
            var mail = await _context.MailMessage.FirstOrDefaultAsync(x => x.MaiNo == data.MaiNo);
            if (mail == null) return false;
             
            var dto = new ScheduleDataLogger
            {  
                ToPeoUID = 0,
                EventType = En_DataEventMode.AddEvent 
            };

            _context.Entry(mail).CurrentValues.SetValues(data);
            await SaveChangesAsync(dto); // 使用 BaseRepository 的 UnitOfWork

            return true;
        }
         
        public async Task<MailMessage?> GetMailMessageAsync(int mai_no)
        {
            return await _context.MailMessage.FirstOrDefaultAsync(x => x.MaiNo == mai_no);
        }

    }
}
