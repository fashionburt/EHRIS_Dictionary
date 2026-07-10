using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.Dictionary
{
    public class DIC1999R02ViewModel
    {
        public int RowId { get; set; }
        public int SheetId { get; set; }
        public string RowName { get; set; } = string.Empty;
        public string? RowDesc { get; set; }
        public string? RowRemark { get; set; }
        public string? DataType { get; set; }
        public int? Length { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public int SortOrder { get; set; }
        public string? LogAction { get; set; }
    }

    public class DIC1999R02Request : DataTableRequest
    {
        public string DbKey { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? Sid { get; set; }
    }

    public class DIC1999R02CreateViewModel
    {
        public string DbKey { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string? Sid { get; set; }
        public string DataType { get; set; } = "nvarchar";
        public int? Length { get; set; } = 50;
        public bool IsNullable { get; set; } = true;
        public string? Description { get; set; }
        public string? Remark { get; set; }
    }
}