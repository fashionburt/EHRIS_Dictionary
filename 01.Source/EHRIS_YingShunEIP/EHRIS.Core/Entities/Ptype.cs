using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("ptype")]
public class PType
{
    /// <summary>
    /// pty_no
    /// </summary>
    [Key]
    [Column("pty_no")]
    public int PtyNo { get; set; }

    /// <summary>
    /// 編號
    /// </summary>
    [Required]
    [MaxLength(4)]
    [Column("pty_code", TypeName = "varchar(4)")]
    public string PtyCode { get; set; }

    /// <summary>
    /// 類別名稱
    /// </summary>
    [Required]
    [Column("pty_name")]
    [MaxLength(80)]
    public string PtyName { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Column("pty_order")]
    public int PtyOrder{ get; set; }
     
    /// <summary>
    /// 狀態
    /// </summary>
    [Required]
    [Column("pty_status", TypeName = "tinyint")]
    public byte PtyStatus { get; set; } = 1;

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("pty_createname")]
    public string PtyCreateName { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    [Required]
    [Column("pty_createtime")]
    public DateTime PtyCreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("pty_modifyname")]
    public string PtyModifyName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("pty_modifytime")]
    public DateTime PtyModifyTime { get; set; }
}