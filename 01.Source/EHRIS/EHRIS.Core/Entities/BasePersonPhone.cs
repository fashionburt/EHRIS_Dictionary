using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("baseperson_phone")]
public class BasePersonPhone
{
    /// <summary>
    /// bsp_no
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("bsp_no")]
    public int BspNo { get; set; }
    /// <summary>
    /// bas_id
    /// </summary>
    [Column("bas_id")]
    public Guid BasId { get; set; }
    /// <summary>
    /// 電話種類
    /// </summary>
    [Required]
    [Column("bsp_phonetype")]
    [MaxLength(20)]
    public string BspPhoneType { get; set; }
    /// <summary>
    /// 區碼
    /// </summary>
    [Required]
    [Column("bsp_areacode")]
    [MaxLength(6)]
    public string BspAreaCode { get; set; }
    /// <summary>
    /// 電話
    /// </summary>
    [Required]
    [Column("bsp_phone")]
    [MaxLength(40)]
    public string BspPhone { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    [Column("bsp_order")]
    public int BspOrder { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("bsp_createtime")]
    public DateTime BspCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("bsp_createname")]
    [MaxLength(400)]
    public string BspCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("bsp_modifytime")]
    public DateTime? BspModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("bsp_modifyname")]
    [MaxLength(400)]
    public string? BspModifyName { get; set; }
}