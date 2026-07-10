using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("baseperson")]
public class BasePerson
{
    /// <summary>
    /// 自然人唯一ID
    /// </summary>
    [Key]
    [Column("bas_id")] 
    public Guid BasId { get; set; }
    /// <summary>
    /// 姓名
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("bas_name")] 
    public string BasName { get; set; }
    /// <summary>
    /// 性別
    /// </summary>
    [Column("bas_sex")] 
    public bool BasSex { get; set; }
    /// <summary>
    /// 身分證號
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("bas_idcard")] 
    public string BasIdCard { get; set; }
    /// <summary>
    /// 是否為本國人
    /// </summary>
    [Column("bas_isnational")]
    public bool BasIsNational { get; set; }
    /// <summary>
    /// 生日
    /// </summary>
    [Required]
    [Column("bas_birthday")] 
    public DateTime BasBirthday { get; set; }
    /// <summary>
    /// 照片
    /// </summary>
    [Required]
    [Column("bas_photo")] 
    public string BasPhoto {  get; set; }
    /// <summary>
    /// 照片浮水印
    /// </summary>
    [Required]
    [Column("bas_photo_watermark")] 
    public string BasPhotoWatermark {  get; set; }
    /// <summary>
    /// 備註
    /// </summary>
    [Required]
    [Column("bas_remark")] 
    public string BasRemark { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("bas_createtime")] 
    public DateTime BasCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("bas_createname")] 
    public string BasCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("bas_modifytime")] 
    public DateTime BasModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("bas_modifyname")] 
    public string BasModifyName { get; set; }
}