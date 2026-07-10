using EHRIS.Core.Models.Common;
using EHRIS.Services.Common;

namespace EHRIS.Web.Shared.Models;

/// <summary>
/// OrgSelector 共用元件的設定模型。
/// </summary>
public class OrgSelectorModel
{
    /// <summary>元件的唯一識別碼，一頁多個元件時用來區分（會作為所有 DOM 元素 ID 前綴）。</summary>
    public string ComponentId { get; set; } = "orgSelector";

    /// <summary>hidden input 的 name 屬性，表單送出時可直接讀取（逗號分隔 ID 清單）。</summary>
    public string FieldName { get; set; } = "DepIds";

    /// <summary>
    /// 資料來源範圍：
    /// AuthorizedUnit = 權責機關
    /// AuthorizedDept = 權責單位
    /// </summary>
    public OrgSelectorScope Scope { get; set; } = OrgSelectorScope.AuthorizedDept;

    /// <summary>觸發按鈕文字。</summary>
    public string ButtonText { get; set; } = "選擇單位";

    /// <summary>Modal 標題。</summary>
    public string ModalTitle { get; set; } = "選擇單位";

    /// <summary>「包含子節點」開關預設值。</summary>
    public bool IncludeChildrenDefault { get; set; } = true;

    /// <summary>初始選取的 ID 清單（編輯頁回顯用），逗號分隔 dep_no。</summary>
    public string? SelectedIds { get; set; }

    /// <summary>由 ViewComponent 從 DB 填入：對應 SelectedIds 的名稱清單，用於 chip 顯示。不需呼叫端設定。</summary>
    public List<OrgNode> PreloadedNodes { get; set; } = new();

    /// <summary>是否允許複選（預設 true）。false 時勾選新節點會自動取消舊節點。</summary>
    public bool Multiple { get; set; } = true;
}
