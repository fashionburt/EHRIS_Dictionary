namespace EHRIS.Core.Models.Dictionary;

public class DIC1999ViewModel
{
    public int MenuId { get; set; }
    public string? ServerIP { get; set; }
    public string? MenuName { get; set; }
    public string? MenuDesc { get; set; }
    public int IsEnabled { get; set; }
    public string IsEnabledText => IsEnabled switch
    {
        1 => "啟用",
        0 => "關閉",
        2 => "已刪除",
        _ => "未知"
    };
    public int SortOrder { get; set; }
}

public class MergedTableInfo
{
    public string TableName { get; set; } = string.Empty;
    public string TableDesc { get; set; } = string.Empty;
    public List<MergedColumnInfo> Columns { get; set; } = new();
}

public class MergedColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public string Description { get; set; } = string.Empty;
}