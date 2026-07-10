using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("sysnotice")]
public class SysNotice
{
    /// <summary>
    /// 訊息公告主鍵
    /// </summary>
    [Key]
	[Column("sysn_no")]
	public int SysnNo { get; set; }

    /// <summary>
    /// 訊息公告種類
    /// 0：警急、1：重要、2：一般
    /// </summary>
    [Required]
    [Column("sysn_type", TypeName = "tinyint")]
    public byte SysnType { get; set; } = 2;

    /// <summary>
    /// 訊息公告日期
    /// </summary>
    [Required]
    [Column("sysn_publicdt")]
    public DateTime SysnPublicDt { get; set; }

    /// <summary>
    /// 訊息公告顯示開始時間
    /// </summary>
    [Required]
    [Column("sysn_starttime")]
    public DateTime SysnStartTime { get; set; }

    /// <summary>
    /// 訊息公告顯示結束時間
    /// </summary>
    [Required]
    [Column("sysn_endtime")]
    public DateTime SysnEndTime { get; set; }

    /// <summary>
    /// 訊息公告內容
    /// </summary>
    [Required]
    [Column("sysn_content")]
    public string SysnContent { get; set; }

    /// <summary>
    /// 訊息公告是否置頂
    /// 0：否、1：是
    /// </summary>
    [Required]
    [Column("sysn_top", TypeName = "bit")]
    public bool SysnTop { get; set; } = false;

    /// <summary>
    /// 訊息公告狀態
    /// </summary>
    [Required]
    [Column("sysn_status")]
    public string SysnStatus { get; set; }

    /// <summary>
    /// 訊息公告建立人
    /// </summary>
    [Required]
    [Column("sysn_createname")]
    public string SysnCreateName { get; set; }

    /// <summary>
    /// 訊息公告建立時間
    /// </summary>
    [Required]
    [Column("sysn_createtime")]
    public DateTime SysnCreateTime { get; set; }

    /// <summary>
    /// 訊息公告修編人
    /// </summary>
    [Required]
    [Column("sysn_modifyname")]
    public string SysnModifyName { get; set; }

    /// <summary>
    /// 訊息公告修編時間
    /// </summary>
    [Required]
    [Column("sysn_modifytime")]
    public DateTime SysnModifyTime { get; set; }
}
