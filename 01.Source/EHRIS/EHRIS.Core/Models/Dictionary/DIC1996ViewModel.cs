using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.Dictionary;

public class DIC1996ViewModel
{
    public int Id { get; set; }
    public string? Message { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string? PriorityText { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string StartDate_Text { get; set; } = string.Empty;
    public string EndDate_Text { get; set; } = string.Empty;
    public string? EditAction { get; set; }
    public string? DelAction { get; set; }
}

public class DIC1996Request : DataTableRequest
{
    public string? Message { get; set; }
    public int? Priority { get; set; }
    public bool? IsEnabled { get; set; }
}