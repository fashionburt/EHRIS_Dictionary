using EHRIS.Core.Models.Common;

namespace EHRIS.Services.Common;

public interface IOrgPeopleService
{
    /// <summary>部門樹（含直屬人數，依 filter）。accNo 預留給權責過濾。</summary>
    Task<List<OrgDeptCountNode>> GetDeptTreeAsync(int accNo, OrgPeopleFilter filter);

    /// <summary>人員分頁（DataTables server-side）。</summary>
    Task<(List<OrgPersonRow> rows, int total)> GetPeoplePageAsync(int accNo, OrgPeopleGridRequest req, OrgPeopleFilter filter);

    /// <summary>
    /// 把 token 字串還原成最終 peo_uid 清單（送出時用）。
    /// token：個別 peo_uid / d:depNo（整部門）/ x:peo_uid（部門全選下的排除）。
    /// filter 需與元件設定一致（狀態/類別/排除自己），d: 展開才會對應正確人員。
    /// </summary>
    Task<List<int>> ResolveAsync(int accNo, string? tokens, OrgPeopleFilter filter);

    /// <summary>回顯用：把 token 還原成可顯示的部門 / 人員 / 排除清單（含名稱）。</summary>
    Task<OrgPeoplePreload> ResolveDisplayAsync(int accNo, string? tokens, OrgPeopleFilter filter);

    /// <summary>「正面表列」：某部門排除指定人員後仍保留的人員（含姓名，依 filter）。</summary>
    Task<List<OrgPersonRow>> GetDeptKeptAsync(int accNo, int depNo, OrgPeopleFilter filter, IEnumerable<int> excludeUids);
}
