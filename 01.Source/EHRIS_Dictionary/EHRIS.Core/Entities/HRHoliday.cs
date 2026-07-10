using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_holiday")]

public class HRHoliday
{
    /// <summary>
    /// hol_no
    /// </summary>
    [Key]
    [Column("hol_no")]
    public int HolNo { get; set; }
    /// <summary>
    /// hol_code
    /// </summary>
    [Required]
    [Column("hol_code")]
    [MaxLength(2)]
    public string HolCode { get; set; }
    /// <summary>
    /// hol_name
    /// </summary>
    [Required]
    [Column("hol_name")]
    [MaxLength(50)]
    public string HolName { get; set; }
    /// <summary>
    /// hol_order
    /// </summary>
    [Column("hol_order")]
    public int HolOrder { get; set; }

    /// <summary>
    /// 是否統計，0：否、1：是
    /// </summary>
    [Required]
    [Column("hol_statistics")]
    public string HolStatistics { get; set; }

    /// <summary>
    /// 是否為公務假別，0：否、1是
    /// 舉例：出差、公假......
    /// </summary>
    [Required]
    [Column("hol_official")]
    public string HolOfficial { get; set; }

    /// <summary>
    /// 是否啟用，1：啟用、2：刪除
    /// </summary>
    [Required]
    [Column("hol_status")]
    public string HolStatus { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("hol_createtime")]
    public DateTime HolCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("hol_createname")]
    [MaxLength(200)]
    public string HolCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("hol_modifytime")]
    public DateTime HolModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("hol_modifyname")]
    [MaxLength(200)]
    public string HolModifyName { get; set; }


}
