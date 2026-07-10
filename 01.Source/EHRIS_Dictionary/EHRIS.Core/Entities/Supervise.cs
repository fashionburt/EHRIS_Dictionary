using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("supervise")]
public class Supervise
{
    /// <summary>
    /// sup_no
    /// </summary>
    [Key]
    [Column("sup_no")]
    public int SupNo { get; set; }

    /// <summary>
    /// acc_no
    /// </summary>
    [Column("acc_no")]
    public int AccNo { get; set; }

    /// <summary>
    /// sup_type
    /// </summary>
    [Required]
    [MaxLength(1)]
    [Column("sup_type",TypeName = "char(1)")]
    public string SupType { get; set; }
}