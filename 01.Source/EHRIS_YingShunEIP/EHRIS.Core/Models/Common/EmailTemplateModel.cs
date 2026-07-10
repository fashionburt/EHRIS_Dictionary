
namespace EHRIS.Core.Models.Common;

public class EmailTemplateModel
{
    public int mat_no { get; set; } 
    public string mat_name { get; set; } = string.Empty;
    public string mat_subject { get; set; } = string.Empty;
    public string mat_content { get; set; } = string.Empty; 
}
