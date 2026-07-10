using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("departments")]
public class Departments
{
    /// <summary>
    /// dep_no
    /// </summary>
    [Key]
    [Column("dep_no")]
    public int DepNo { get; set; }
    /// <summary>
    /// dep_depid
    /// </summary>
    [Required]
    [Column("dep_depid")]
    [MaxLength(20)]
    public string DepDepId { get; set; }
    /// <summary>
    /// 父層編號
    /// </summary>
    [Column("dep_parentid")]
    public int DepParentId { get; set; }
    /// <summary>
    /// 單位名稱
    /// </summary>
    [Required]
    [Column("dep_name")]
    [MaxLength(40)]
    public string DepName { get; set; }
    /// <summary>
    /// 單位層級
    /// </summary>
    [Column("dep_level")]
    public int DepLevel { get; set; }
    /// <summary>
    /// 單位編號
    /// </summary>
    [Required]
    [Column("dep_code")]
    [MaxLength(32)]
    public string DepCode { get; set; }
    /// <summary>
    /// 單位排序
    /// </summary>
    [Column("dep_order")]
    public int DepOrder { get; set; }
    /// <summary>
    /// 啟用狀態
    /// </summary>
    [Required]
    [Column("dep_status")]
    [MaxLength(1)]
    public string DepStatus { get; set; }
    /// <summary>
    /// 單位介紹
    /// </summary>
    [Required]
    [Column("dep_introduce")]
    public string DepIntroduce { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("dep_createtime")]
    public DateTime DepCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("dep_createname")]
    [MaxLength(200)]
    public string DepCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("dep_modifytime")]
    public DateTime DepModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("dep_modifyname")]
    [MaxLength(200)]
    public string DepModifyName { get; set; }
    /// <summary>
    /// uni_id
    /// </summary>
    [Required]
    [Column("uni_id")]
    public string UniId { get; set; }
    /// <summary>
    /// ude_no
    /// </summary>
    [Required]
    [Column("ude_no")]
    public int? UdeNo { get; set; }

}