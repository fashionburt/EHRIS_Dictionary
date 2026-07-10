using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Repositories;

public interface IOrgPeopleRepository
{
    /// <summary>部門樹（含每部門的直屬人數，依 filter 狀態/類別/排除自己）。</summary>
    Task<List<OrgDeptCountNode>> GetDeptTreeWithCountAsync(OrgPeopleFilter filter);

    /// <summary>人員分頁（DataTables server-side）。回傳當頁資料與符合條件總筆數。</summary>
    Task<(List<OrgPersonRow> rows, int total)> GetPeoplePageAsync(
        int depNo, bool global, string? q, OrgPeopleFilter filter,
        int start, int length, string orderField, string orderDir);

    /// <summary>展開多個部門 → 其直屬人員 peo_uid（送出時把 d: token 還原成人員）。</summary>
    Task<List<int>> GetDeptPeopleUidsAsync(IEnumerable<int> depNos, OrgPeopleFilter filter);

    /// <summary>依 peo_uid 清單取人員資料（回顯用，不套狀態/自己過濾，離職者也回名稱）。</summary>
    Task<List<OrgPersonRow>> GetPeopleByUidsAsync(IEnumerable<int> uids);

    /// <summary>平行部門：與指定部門同一父層（同 dep_parentid）的啟用部門（含自己）。</summary>
    Task<List<int>> GetSiblingDeptNosAsync(int selfDepNo);

    /// <summary>查權限表 → 授權部門（Auth）。目前預留，回 null=不限制。</summary>
    Task<List<int>?> GetAuthDeptNosAsync(int accNo);

    /// <summary>取登入者 supervise.sup_type（"1"/"2"/null）。Automa 自動判斷用。</summary>
    Task<string?> GetSupTypeAsync(int accNo);
}
