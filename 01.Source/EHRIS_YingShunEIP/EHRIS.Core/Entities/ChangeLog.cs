using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("ChangeLog")]
public class ChangeLog
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string TableName { get; set; } = "";
    [Required]
    public string KeyValue { get; set; } = "";
    [Required]
    public string Action { get; set; } = ""; // INSERT / UPDATE / DELETE
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int ExecUid { get; set; }
    public DateTime ExecTime { get; set; } = DateTime.Now;
}
