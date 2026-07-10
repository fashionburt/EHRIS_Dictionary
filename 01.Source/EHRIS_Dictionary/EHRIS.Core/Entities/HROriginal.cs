using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_original")]
[PrimaryKey(nameof(VacNo), nameof(OriNo))]
public class HROriginal
{
    /// <summary>
    /// vac_no
    /// </summary>
    [Key]
    [Column("vac_no")]
    public Int64 VacNo { get; set; }
    /// <summary>
    /// ori_no
    /// </summary>
    [Key]
    [Column("ori_no")]
    public int OriNo { get; set; }
    /// <summary>
    /// ori_sdate
    /// </summary>
    [Required]
    [Column("ori_sdate")]
    public DateTime OriSDate { get; set; }
    /// <summary>
    /// ori_stime
    /// </summary>
    [Required]
    [Column("ori_stime")]
    public DateTime OriSTime { get; set; }
    /// <summary>
    /// ori_etime
    /// </summary>
    [Required]
    [Column("ori_etime")]
    public DateTime OriETime { get; set; }
    /// <summary>
    /// ori_days
    /// </summary>
    [Column("ori_days")]
    public int OriDays {  get; set; }
    /// <summary>
    /// ori_rest
    /// </summary>
    [Column("ori_rest")]
    public int OriRest { get; set; }
    /// <summary>
    /// ori_allow
    /// </summary>
    [Column("ori_allow")]
    public int OriAllow { get; set; }


}
