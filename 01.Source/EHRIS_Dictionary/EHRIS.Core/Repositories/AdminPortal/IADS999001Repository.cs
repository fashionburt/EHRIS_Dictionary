using EHRIS.Core.Entities;
using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using static EHRIS.Core.Models.LoginViewModel;

namespace EHRIS.Core.Repositories.AdminPortal
{
    public interface IADS999001Repository : IBaseRepository
    {
        Task<DataTablesResponse<ADS999001ListViewModel>> GetSysNoticePagedListAsync(DataTablesRequest request);
        Task<SysNotice> GetSysNoticeByNoAsync(int sysnNo);
        Task<bool> CreateSysNoticeAsync(SysNoticeUpdateViewModel model, string userName, IDataLogger dataLogger);
        Task<bool> UpdateSysNoticeAsync(SysNoticeUpdateViewModel model, string userName, IDataLogger dataLogger);
        Task<bool> SoftDeleteSysNoticeAsync(int sysnNo, IDataLogger dataLogger);
        Task<List<LoginNoticeViewModel>> GetLoginNoticesAsync(int topN);

    }
}