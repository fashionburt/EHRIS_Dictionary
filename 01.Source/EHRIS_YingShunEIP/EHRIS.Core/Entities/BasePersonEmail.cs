using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("baseperson_email")]
public class BasePersonEmail
{
    /// <summary>
    /// bse_no
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("bse_no")]
    public int BseNo { get; set; }
    /// <summary>
    /// bas_id
    /// </summary>
    [Column("bas_id")]
    public Guid BasId { get; set; }
    /// <summary>
    /// 電子信箱種類
    /// </summary>
    [Required]
    [Column("bse_emailtype")]
    [MaxLength(20)]
    public string BseEmailType { get; set; }
    /// <summary>
    /// 電子信箱
    /// </summary>
    [Required]
    [Column("bse_email")]
    [MaxLength(400)]
    public string BseEmail { get; set; }
    /// <summary>
    /// 電子信箱排序
    /// </summary>
    [Column("bse_order")]
    public int BseOrder { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("bse_createtime")]
    public DateTime BseCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("bse_createname")]
    [MaxLength(400)]
    public string BseCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("bse_modifytime")]
    public DateTime? BseModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("bse_modifyname")]
    [MaxLength(400)]
    public string? BseModifyName { get; set; }
}