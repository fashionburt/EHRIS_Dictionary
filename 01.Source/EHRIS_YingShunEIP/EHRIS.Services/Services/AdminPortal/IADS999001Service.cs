using EHRIS.Core.Models.AdminPortal;
using EHRIS.Core.Models.Event;
using static EHRIS.Core.Models.LoginViewModel;

namespace EHRIS.Services.Services.AdminPortal
{
    public interface IADS999001Service
    {
        Task<DataTablesResponse<ADS999001ListViewModel>> GetSysNoticePagedListAsync(DataTablesRequest request);
        Task<ADS999001EditViewModel> GetSysNoticeForEditAsync(int sysnNo);
        Task<(bool success, string message)> CreateSysNoticeAsync(SysNoticeUpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> UpdateSysNoticeAsync(SysNoticeUpdateViewModel model, IDataLogger dataLogger, string userName);
        Task<(bool success, string message)> SoftDeleteSysNoticeAsync(int sysnNo, IDataLogger dataLogger, string userName);
        string GetTypeName(byte typeCode);

        Task<List<LoginNoticeViewModel>> GetLoginNoticesAsync(int topN = 10);
    }
}