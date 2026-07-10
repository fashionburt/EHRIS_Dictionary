using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("StaHr006")]

public class StaHr006
{
    [Key]
    [Column("sta006_no")]
    public Int64 Sta006No { get; set; }

    [Required]
    [Column("sta006_Unitid")]
    [MaxLength(20)]
    public string Sta006UnitId { get; set; }

    [Column("sta006_udeno")]
    public int Sta006UdeNo { get; set; }

    [Column("sta006_ptyno")]
    public int Sta006PtyNo { get; set; }

    [Column("sta006_director")]
    public int Sta006Director { get; set; }

    [Column("sta006_sex")]
    public int Sta006Sex { get; set; }

    [Column("sta006_year")]
    public int Sta006Year { get; set; }

    [Column("sta006_month")]
    public int Sta006Month { get; set; }

    [Required]
    [Column("sta006_UnitName")]
    [MaxLength(100)]
    public string Sta006UnitName { get; set; }

    [Required]
    [Column("sta006_depname")]
    [MaxLength(100)]
    public string Sta006DepName { get; set; }

    [Required]
    [Column("sta006_ptyname")]
    [MaxLength(40)]
    public string Sta006PtyName { get; set; }

    [Column("sta006_people")]
    public int Sta006People { get; set; }

    [Required]
    [Column("sta006_createtime")]
    public DateTime Sta006CreateTime { get; set; } = DateTime.Now;

    [Required]
    [Column("sta006_modifytime")]
    public DateTime Sta006ModifyTime { get; set; } = DateTime.Now;
}
