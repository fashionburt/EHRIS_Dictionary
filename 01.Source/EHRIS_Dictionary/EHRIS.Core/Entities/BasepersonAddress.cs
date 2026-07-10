using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("baseperson_address")]
public class BasePersonAddress
{
    /// <summary>
    /// bsa_no
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("bsa_no")]
    public int BsaNo { get; set; }
    /// <summary>
    /// bas_id
    /// </summary>
    [Column("bas_id")]
    public Guid BasId { get; set; }
    /// <summary>
    /// 地址種類
    /// </summary>
    [Required]
    [Column("bsa_addrtype")]
    [MaxLength(20)]
    public string BsaAddrType { get; set; }
    /// <summary>
    /// 郵遞區號
    /// </summary>
    [Required]
    [Column("bsa_postalcode")]
    [MaxLength(6)]
    public string BsaPostalCode { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    [Required]
    [Column("bsa_address")]
    [MaxLength(400)]
    public string BsaAddress { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    [Column("bsa_order")]
    public int BsaOrder { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("bsa_createtime")]
    public DateTime BsaCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("bsa_createname")]
    [MaxLength(400)]
    public string BsaCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("bsa_modifytime")]
    public DateTime? BsaModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("bsa_modifyname")]
    [MaxLength(400)]
    public string? BsaModifyName { get; set; }
}