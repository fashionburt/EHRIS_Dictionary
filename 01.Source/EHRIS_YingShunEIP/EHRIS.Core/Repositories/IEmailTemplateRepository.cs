using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Repositories
{
    public interface IEmailTemplateRepository
    {
        Task<EmailTemplateModel?> GetEmailTemplateAsync(int templateId);
        
    }
}
