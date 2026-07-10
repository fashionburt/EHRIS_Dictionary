using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("StaHr006_Details")]

public class StaHr006Details
{
    [Key]
    [Column("sta006D_no")]
    public Int64 Sta006DNo { get; set; }

    [Column("sta006_no")]
    public Int64 Sta006No { get; set; }

    [Column("stp_no")]
    public int StpNo { get; set; }

    [Column("sta006D_people")]
    public int Sta006DPeople { get; set; }
   
}
