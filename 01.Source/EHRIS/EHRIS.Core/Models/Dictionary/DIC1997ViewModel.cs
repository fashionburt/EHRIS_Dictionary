using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.Dictionary;

public class DIC1997ViewModel
{
    public int LogId { get; set; }
    public string? DbKey { get; set; }
    public string? TableName { get; set; }
    public string? PkName { get; set; }
    public string? State { get; set; }
    public string? StateText { get; set; }
    public string? Detail { get; set; }
    public DateTime Date { get; set; }
    public string? DateText { get; set; }
    public string? ServerIP { get; set; }
}

public class DIC1997Request : DataTableRequest
{
    public string? DbKey { get; set; }
    public string? TableName { get; set; }
    public string? PkName { get; set; }
    public string? State { get; set; }

    public string? Sid { get; set; }
}

public class DIC1997UpdateModel
{
    public int RowId { get; set; }
    public string? RowDesc { get; set; }
    public string? RowRemark { get; set; }
}