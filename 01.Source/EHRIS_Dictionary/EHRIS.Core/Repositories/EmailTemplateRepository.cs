using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories
{
    public class EmailTemplateRepository: BaseRepository,IEmailTemplateRepository
    {
        public EmailTemplateRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<EmailTemplateModel?> GetEmailTemplateAsync(int templateId)
        {
            var mailTemplate = await _context.mailType.Where(x=>x.MatNo==templateId && x.MatStatus=="1")
                .Select(x => new EmailTemplateModel
                {
                    mat_no = x.MatNo,
                    mat_name = x.MatName,
                    mat_subject = x.MatSubject,
                    mat_content = x.MatContent
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return mailTemplate;
        }

        
    }
}
