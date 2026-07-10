using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("aauthority")]
public class AAuthority
{
    /// <summary>
    /// aau_no
    /// </summary>
    [Key]
    [Column("aau_no")]
    public int AauNo { get; set; }
    /// <summary>
    /// 帳號編號
    /// </summary>
    [Column("acc_no")]
    public int AccNo { get; set; }
    /// <summary>
    /// 功能編號
    /// </summary>
    [Column("sfu_no")]
    public int SfuNo { get; set; }
    /// <summary>
    /// 新增權限
    /// </summary>
    [Column("aau_Ins")]
    public byte AauIns { get; set; }
    /// <summary>
    /// 修改權限
    /// </summary>
    [Column("aau_Edi")]
    public byte AauEdi { get; set; }
    /// <summary>
    /// 刪除權限
    /// </summary>
    [Column("aau_Del")] 
    public byte AauDel { get; set; }
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("aau_createname")] 
    public string AauCreateName { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("aau_createtime")]
    public DateTime AauCreateTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("aau_modifyname")] 
    public string AauModifyName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("aau_modifytime")]
    public DateTime AauModifyTime { get; set; } = DateTime.Now;
}
