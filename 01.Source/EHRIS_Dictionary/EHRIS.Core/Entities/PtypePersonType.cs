using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("ptype_persontype")]
public class PtypePersonType
{
    /// <summary>
    /// pty_no
    /// </summary>
    [Key]
    [Column("ptt_persontype")]
    public string PttPersonType { get; set; }

    /// <summary>
    /// 編號
    /// </summary>
    [Column("ptt_ptyno", TypeName = "int")]
    public int PttPtyNo { get; set; }

     
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("ptt_createname")]
    public string PtyCreateName { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    [Required]
    [Column("ptt_createtime")]
    public DateTime PtyCreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("ptt_modifyname")]
    public string PtyModifyName { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    [Required]
    [Column("ptt_modifytime")]
    public DateTime PtyModifyTime { get; set; }
}