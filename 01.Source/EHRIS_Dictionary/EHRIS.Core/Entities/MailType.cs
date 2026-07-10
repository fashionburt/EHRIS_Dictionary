using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("mailType")]

public class MailType
{
    /// <summary>
    /// mat_no
    /// </summary>
    [Key]
    [Column("mat_no")]
    public int MatNo { get; set; }

    /// <summary>
    /// 寄信種類名稱
    /// </summary>
    [Required]
    [Column("mat_name")]
    public string MatName { get; set; }

    /// <summary>
    /// mat_subject
    /// </summary>
    [Required]
    [Column("mat_subject")]
    public string MatSubject { get; set; }

    /// <summary>
    /// mat_content
    /// </summary>
    [Required]
    [Column("mat_content")]
    public string MatContent { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Column("mat_order")]
    public int MatOrder { get; set; }

    /// <summary>
    /// 0 不啟用 1 啟用
    /// </summary>
    [Required]
    [Column("mat_status")]
    public string MatStatus { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("mat_createname")]
    public string MatCreateName { get; set; } = "";

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("mat_createtime")]
    public DateTime MatCreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("mat_modifyname")]
    public string MatModifyName { get; set; } = "";

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("mat_modifytime")]
    public DateTime MatModifyTime { get; set; } = DateTime.Now;

}
