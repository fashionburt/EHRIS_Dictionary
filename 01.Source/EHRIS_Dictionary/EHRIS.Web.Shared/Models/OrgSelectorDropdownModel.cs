using EHRIS.Core.Models.Common;
using EHRIS.Services.Common;

namespace EHRIS.Web.Shared.Models;

/// <summary>
/// 下拉式組織樹（單選）共用元件設定模型。
/// 後端資料源與 OrgSelector modal 共用（同一個 OrgSelectorScope 與 /OrgSelector/Tree 端點）。
/// </summary>
public class OrgSelectorDropdownModel
{
    /// <summary>元件唯一識別碼（所有 DOM 元素 ID 前綴）。</summary>
    public string ComponentId { get; set; } = "orgDropdown";

    /// <summary>hidden input 的 name，表單送出時讀取（單一 ID）。</summary>
    public string FieldName { get; set; } = "DepId";

    /// <summary>
    /// 資料來源範圍，與 OrgSelector modal 相同：
    /// AuthorizedUnit（權責機關）/ AuthorizedDept（權責單位，預設）/ All（全部單位）。
    /// </summary>
    public OrgSelectorScope Scope { get; set; } = OrgSelectorScope.AuthorizedDept;

    /// <summary>未選取時顯示的提示文字。</summary>
    public string Placeholder { get; set; } = "請選擇單位";

    /// <summary>
    /// 是否顯示最上層「根節點」（dep_parentid=0 那層，例如根機關／市政府）。
    /// 主要用於 All（全部單位）模式：false 時隱藏該根節點、其子節點（局處層）上移為頂層。
    /// 只影響「最上層、非群組、且有子節點」的根節點；權責單位的機關群組層、權責機關清單不受影響。
    /// </summary>
    public bool ShowRoot { get; set; } = true;

    /// <summary>初始選取的單一 ID（編輯頁回顯用）。</summary>
    public string? SelectedId { get; set; }

    /// <summary>由 ViewComponent 從 DB 填入：對應 SelectedId 的節點（顯示名稱用），不需呼叫端設定。</summary>
    public OrgNode? PreloadedNode { get; set; }
}
