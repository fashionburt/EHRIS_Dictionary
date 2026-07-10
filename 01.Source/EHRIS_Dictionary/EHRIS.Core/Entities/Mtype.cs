using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("mtype")]
public class MType
{
    /// <summary>
    /// mty_no
    /// </summary>
    [Key]
    [Column("mty_no")]
    public int MtyNo { get; set; }

    /// <summary>
    /// acc_no
    /// </summary>
    [Column("acc_no")]
    public int AccNo { get; set; }

    /// <summary>
    /// pty_no
    /// </summary>
    [Column("pty_no")]
    public int PtyNo { get; set; }
}