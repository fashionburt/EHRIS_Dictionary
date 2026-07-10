namespace EHRIS.Core.Models.Common;

/// <summary>
/// 共用 OrgSelector 元件使用的扁平樹節點 DTO。
/// </summary>
public class OrgNode
{
    /// <summary>節點 ID（部門為 dep_no 字串化；分組節點為 "u:" + 機關名）。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>父節點 ID；根節點為 null。</summary>
    public string? ParentId { get; set; }

    /// <summary>顯示名稱。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>排序順序。</summary>
    public int SortOrder { get; set; }

    /// <summary>是否為分組節點（機關層的虛擬節點，不會被計入表單提交的 ID 清單）。</summary>
    public bool IsGroup { get; set; }
}
