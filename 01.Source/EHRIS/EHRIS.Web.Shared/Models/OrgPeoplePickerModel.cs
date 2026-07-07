using EHRIS.Core.Models.Common;

namespace EHRIS.Web.Shared.Models;

/// <summary>
/// 人員選擇器（上區部門樹 + 下區 DataTable）共用元件設定模型。
/// </summary>
public class OrgPeoplePickerModel
{
    /// <summary>元件唯一識別碼（所有 DOM 元素 ID 前綴），一頁多個務必各自不同。</summary>
    public string ComponentId { get; set; } = "orgPeople";

    /// <summary>hidden input 的 name，表單送出讀此欄位（token 字串）。</summary>
    public string FieldName { get; set; } = "PeoIds";

    /// <summary>觸發按鈕文字。</summary>
    public string ButtonText { get; set; } = "選擇人員";

    /// <summary>Modal 標題。</summary>
    public string ModalTitle { get; set; } = "選擇人員";

    /// <summary>預設人員類別過濾 pty_no（null = 全部）。</summary>
    public int? PtyNo { get; set; }

    /// <summary>權限方式（根範圍策略）。目前 Automa=最大權限(不卡權限)；Self/Parallel/Auth 保留、Limit/AgentForMe2 未實作。</summary>
    public NodeType NodeType { get; set; } = NodeType.Automa;

    /// <summary>權限類型（已定義，權責邏輯待接）。</summary>
    public AuthType AuthType { get; set; } = AuthType.All;

    /// <summary>人員狀態（依 peo_jobtype：在職/離職/全部）。</summary>
    public PeopleStatus PeopleStatus { get; set; } = PeopleStatus.OnJob;

    /// <summary>是否顯示自己（False 排除登入者）。</summary>
    public PeopleShowSelf ShowSelf { get; set; } = PeopleShowSelf.True;

    /// <summary>是否顯示「職稱」欄。</summary>
    public bool ShowTitle { get; set; } = true;

    /// <summary>
    /// 是否允許複選（預設 true）。
    /// false（單選）時：選新的人自動取消舊的，且隱藏「部門全選 / 本頁全選」（單選無此概念）。
    /// </summary>
    public bool Multiple { get; set; } = true;

    /// <summary>初始選取的 token 字串（回顯），逗號分隔的 peo_uid / d:depNo / x:peo_uid。</summary>
    public string? SelectedTokens { get; set; }

    /// <summary>由 ViewComponent 從 SelectedTokens 解析填入（名稱回顯用），呼叫端不需設定。</summary>
    public OrgPeoplePreload? Preloaded { get; set; }
}
