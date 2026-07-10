using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("sysvariable")]
[PrimaryKey(nameof(SvrCode), nameof(SarCode))]

public class SysVariable
{
    /// <summary>
    /// svr_code
    /// </summary>
    [Key]
    [Column("svr_code", TypeName = "varchar(20)")]
    public string SvrCode { get; set; } = "";

    /// <summary>
    /// sar_code
    /// </summary>
    [Key]
    [Column("sar_code", TypeName = "varchar(20)")]
    public string SarCode { get; set; } = "";

    /// <summary>
    /// 順序
    /// </summary>
    [Column("svr_order")]
    public int SvrOrder { get; set; }


    /// <summary>
    /// 參數值
    /// </summary>
    [Required]
    [Column("svr_name")]
    [MaxLength(50)]
    public string SvrName { get; set; } = "";

    /// <summary>
    /// 狀態  0:不啟用  1:啟用
    /// </summary>
    [Required]
    [Column("svr_status", TypeName = "char(1)")]
    public string SvrStatus { get; set; } = "";

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("svr_createtime")]
    public DateTime SvrCreateTime { get; set; }= DateTime.Now;

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("svr_createname")]
    [MaxLength(200)]
    public string SvrCreateName { get; set; } = "";

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("svr_modifytime")]
    public DateTime  SvrModifyTime { get; set; }= DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("svr_modifyname")]
    [MaxLength(200)]
    public string SvrModifyName { get; set; } = "";
}
