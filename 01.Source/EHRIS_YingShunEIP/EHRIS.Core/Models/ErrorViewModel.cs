namespace EHRIS.Core.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public class ErrorCheckUIDataViewModel
{
    public bool Result = true;

    public string ErrorMessage = string.Empty;
}
