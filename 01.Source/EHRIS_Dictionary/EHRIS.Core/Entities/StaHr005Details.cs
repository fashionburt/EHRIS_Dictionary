using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("StaHr005_Details")]

public class StaHr005Details
{
    [Key]
    [Column("sta005D_no")]
    public Int64 Sta005DNo { get; set; }

    [Column("sta005_no")]
    public Int64 Sta005No { get; set; }

    [Required]
    [Column("sta005D_idcard")]
    [MaxLength(20)]
    public string Sta005DIDCard { get; set; }

    [Required]
    [Column("sta005D_peoname")]
    [MaxLength(40)]
    public string Sta005DPeoName { get; set; }

    [Column("sta005D_prono")]
    public int Sta005DProNo { get; set; }

    [Required]
    [Column("sta005D_proname")]
    [MaxLength(40)]
    public string Sta005DProName { get; set; }

    [Column("sta005D_age")]
    public int Sta005DAge { get; set; }

    [Required]
    [Column("sta005D_arrivedate")]
    public DateTime Sta005DArriveDate { get; set; }

    [Column("sta005D_workdays")]
    public int Sta005DWorkDays { get; set; }

    [Column("sta005D_flexdays")]
    public int Sta005DFlexDays { get; set; }

    [Column("sta005D_overtime")]
    public int Sta005DOverTime { get; set; }

}
