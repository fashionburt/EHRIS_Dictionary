using EHRIS.Core.Models.Common;

namespace EHRIS.Services.Common;

/// <summary>
/// OrgSelector 資料來源範圍，目前先以 STA301008 的兩種統計條件（權責機關 / 單位）,若後續要調整再做修改。
/// </summary>
public enum OrgSelectorScope
{
    /// <summary>
    /// 權責機關：登入者權責內可見的「機關」清單。
    /// 單層清單，節點 ID 為真實 uni_id，本身即為可選葉節點。
    /// </summary>
    AuthorizedUnit = 0,

    /// <summary>
    /// 權責單位：登入者權責內可見的「機關 → 部門」兩層。
    /// 機關層為虛擬分組節點（不提交），部門為可選葉節點，ID 為 dep_no。
    /// </summary>
    AuthorizedDept = 1
}

public interface IOrgSelectorService
{
    /// <summary>
    /// 取得樹節點清單（扁平結構，前端自行建樹）。
    /// </summary>
    /// <param name="scope">資料來源範圍：Authorized 走權責過濾、All 顯示全部部門</param>
    /// <param name="accNo">登入者 AccNo（Authorized 模式必須，All 模式忽略）</param>
    Task<List<OrgNode>> GetTreeAsync(OrgSelectorScope scope, int accNo);

    /// <summary>依 ID 清單回傳部門節點（編輯頁回顯名稱用）。</summary>
    Task<List<OrgNode>> GetNodesByIdsAsync(OrgSelectorScope scope, int accNo, IEnumerable<string> ids);
}
