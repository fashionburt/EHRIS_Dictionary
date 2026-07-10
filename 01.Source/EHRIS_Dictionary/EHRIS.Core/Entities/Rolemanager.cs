using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("rolemanager")]
public class Rolemanager
{
    /// <summary>
    /// rm_no
    /// </summary>
    [Key]
    [Column("rm_no")]
    public int RmNo { get; set; }

    /// <summary>
    /// rol_no
    /// </summary>
    [Column("rol_no")]
    public int RolNo { get; set; }

    /// <summary>
    /// acc_no
    /// </summary>
    [Column("acc_no")]
    public int AccNo { get; set; }
}