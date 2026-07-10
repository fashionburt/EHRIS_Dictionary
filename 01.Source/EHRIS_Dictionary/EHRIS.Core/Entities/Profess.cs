using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("profess")]
public class Profess
{
    /// <summary>
    /// pty_no
    /// </summary>
    [Key]
    [Column("pro_no")]
    public int ProNo { get; set; }

    /// <summary>
    /// 編號
    /// </summary>
    [Required]
    [MaxLength(4)]
    [Column("pro_code", TypeName = "varchar(4)")]
    public string ProCode { get; set; }

    /// <summary>
    /// 類別名稱
    /// </summary>
    [Required]
    [Column("pro_name", TypeName = "nvarchar(4)")]
    [MaxLength(40)]
    public string ProName { get; set; }

    /// <summary>
    /// 狀態
    /// </summary>
    [Required]
    [MaxLength(80)]
    [Column("pro_english", TypeName = "varchar(80)")]
    public string ProEnglish { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Column("pro_order")]
    public int ProOrder { get; set; } = 99;

    /// <summary>
    /// 狀態
    /// </summary>
    [Required]
    [Column("pro_isManager", TypeName = "tinyint")]
    public byte ProIsManager { get; set; } = 0;


    /// <summary>
    /// 狀態
    /// </summary>
    [Required]
    [Column("pro_status", TypeName = "tinyint")]
    public byte ProStatus { get; set; } = 1;

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("pro_createname")]
    public string ProCreateName { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    [Required]
    [Column("pro_createtime")]
    public DateTime ProCreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("pro_modifyname")]
    public string ProModifyName { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    [Required]
    [Column("pro_modifytime")]
    public DateTime ProModifyTime { get; set; }
}