using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("StaHr005_DLeave")]

public class StaHr005DLeave
{
    [Key]
    [Column("sta005DL_no")]
    public Int64 Sta005DLNo { get; set; }

    [Column("sta005D_no")]
    public Int64 Sta005DNo { get; set; }

    [Required]
    [Column("sta005DL_holname")]
    [MaxLength(50)]
    public string sta005DLHolName { get; set; }

    [Column("sta005DL_holno")]
    public int sta005DLHolNo { get; set; }

    [Column("sta005DL_totalminutes")]
    public int sta005DLTotalMinutes { get; set; }

}
