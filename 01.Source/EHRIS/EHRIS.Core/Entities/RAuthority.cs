using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("rauthority")]
public class RAuthority
{
    /// <summary>
    /// rau_no
    /// </summary>
    [Key]
    [Column("rau_no")]
    public int RauNo { get; set; }

    /// <summary>
    /// rol_no
    /// </summary>
    [Column("rol_no")]
    public int RolNo { get; set; }

    /// <summary>
    /// 對應role資料表
    /// </summary>
    [Column("sfu_no")]
    public int SfuNo { get; set; }

    /// <summary>
    /// rau_Ins
    /// </summary>
    [Column("rau_Ins")] 
    public byte RauIns { get; set; }

    /// <summary>
    /// rau_Edi
    /// </summary>
    [Column("rau_Edi")] 
    public byte RauEdi { get; set; }

    /// <summary>
    /// 是否刪除，其值0：否、1是
    /// </summary>
    [Column("rau_Del")] 
    public byte RauDel { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("rau_createname")]
    [MaxLength(200)]
    public string RauCreateName { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("rau_createtime")] 
    public DateTime RauCreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("rau_modifyname")]
    [MaxLength(200)]
    public string RauModifyName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("rau_modifytime")] 
    public DateTime RauModifyTime { get; set; }
}
