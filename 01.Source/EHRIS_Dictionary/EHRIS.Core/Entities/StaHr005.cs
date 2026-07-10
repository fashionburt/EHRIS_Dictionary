using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("StaHr005")]

public class StaHr005
{
    [Key]
    [Column("sta005_no")]
    public Int64 Sta005No { get; set; }
    
    [Required]
    [Column("sta005_Unitid")]
    [MaxLength(20)]
    public string Sta005UnitId  { get; set; }

    [Column("sta005_udeno")]
    public int Sta005UdeNo { get; set; }

    [Column("sta005_ptyno")]
    public int Sta005PtyNo { get; set; }

    [Column("sta005_director")]
    public int Sta005Director { get; set; }

    [Column("sta005_sex")]
    public int Sta005Sex { get; set; }

    [Column("sta005_year")]
    public int Sta005Year { get; set; }

    [Column("sta005_month")]
    public int Sta005Month { get; set; }

    [Required]
    [Column("sta005_UnitName")]
    [MaxLength(100)]
    public string Sta005UnitName { get; set; }

    [Required]
    [Column("sta005_depname")]
    [MaxLength(100)]
    public string Sta005DepName { get; set; }

    [Required]
    [Column("sta005_ptyname")]
    [MaxLength(40)]
    public string Sta005PtyName { get; set; }

    [Column("sta005_people")]
    public int Sta005People { get; set; }

    [Required]
    [Column("sta005_createtime")]
    public DateTime Sta005CreateTime { get; set; } = DateTime.Now;

    [Required]
    [Column("sta005_modifytime")]
    public DateTime Sta005ModifyTime { get; set; } = DateTime.Now;

}
