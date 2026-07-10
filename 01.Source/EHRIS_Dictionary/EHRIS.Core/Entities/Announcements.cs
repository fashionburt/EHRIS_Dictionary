using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("announcements")]
public class Announcements
{
    /// <summary>
    /// acc_no
    /// </summary>
    [Key]
    [Column("ann_no")]
    public int AnnNo { get; set; }
    /// <summary>
    /// 公告類型
    /// </summary>
    [Column("ann_type")]
    public string AnnType { get; set; }
    /// <summary>
    /// 公告急迫性
    /// </summary>
    [Column("ann_badge")]
    public string AnnBadge { get; set; }
    /// <summary>
    /// 公告標題
    /// </summary>
    [Required, MaxLength(200)]
    [Column("ann_title")]
    public string AnnTitle { get; set; }

    [Required, MaxLength(500)]
    [Column("ann_content")]
    public string AnnContent { get; set; }
    /// <summary>
    /// 公告日期
    /// </summary>
    [Required]
    [Column("ann_date")]
    public DateTime AnnDate { get; set; } = DateTime.Now;
    /// <summary>
    /// 啟用狀態
    /// </summary>
    [Required]
    [Column("ann_status")]
    public byte Annstatus { get; set; } = 1; 
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("ann_createtime")]
    public DateTime AnnCreateTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("ann_createname")]
    public string? AnnCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("ann_modifytime")]
    public DateTime AnnModifyTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("ann_modifyname")]
    public string? AnnModifyName { get; set; }
     
}