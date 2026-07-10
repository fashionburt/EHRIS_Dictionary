using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_plevel")]

public class HRPLevel
{
    /// <summary>
    /// ple_no
    /// </summary>
    [Key]
    [Column("ple_no")]
    public int PleNo { get; set; }

    /// <summary>
    /// pleT_no
    /// </summary>
    [Column("pleT_no")]
    public int PleTNo { get; set; }

    /// <summary>
    /// ple_code
    /// </summary>
    [Required]
    [Column("ple_code")]
    [MaxLength(4)]
    public string PleCode { get; set; }

    /// <summary>
    /// ple_name
    /// </summary>
    [Required]
    [Column("ple_name")]
    [MaxLength(50)]
    public string PleName { get; set; }

    /// <summary>
    /// 是否啟用，1：啟用、2：刪除
    /// </summary>
    [Required]
    [Column("ple_status")]
    public string PleStatus { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("ple_createtime")]
    public DateTime PleCreateTime { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("ple_createname")]
    [MaxLength(200)]
    public string PleCreateName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("ple_modifytime")]
    public DateTime PleModifyTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("ple_modifyname")]
    [MaxLength(200)]
    public string PleModifyName { get; set; }

}
