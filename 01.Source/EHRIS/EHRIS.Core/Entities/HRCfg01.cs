using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_cfg01")]
[PrimaryKey(nameof(Cfg01Year), nameof(Cfg01Month))]

public class HRCfg01
{
    /// <summary>
    /// cfg01_year
    /// </summary>
    [Key]
    [Column("cfg01_year")]
    public int Cfg01Year { get; set; }
    /// <summary>
    /// cfg01_month
    /// </summary>
    [Column("cfg01_month")]
    public int Cfg01Month { get; set; }
    /// <summary>
    /// cfg01_days1
    /// </summary>
    [Column("cfg01_days1")]
    public int Cfg01Days1 { get; set; }
    /// <summary>
    /// cfg01_days2
    /// </summary>
    [Column("cfg01_days2")]
    public int Cfg01Days2 { get; set; }

}
