using EHRIS.Core.Models.Common;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("scheduleConfig")]

public class ScheduleConfig
{
    /// <summary>
    /// scc_no
    /// </summary>
    [Key]
    [Column("scc_no")]
    public int SccNo { get; set; }

    /// <summary>
    /// scc_name
    /// </summary>
    [Required]
    [Column("scc_name")]
    public string SccName { get; set; }

    /// <summary>
    /// scc_variable
    /// </summary>
    [Required]
    [Column("scc_variable")]
    public string SccVariable { get; set; }

    /// <summary>
    /// scc_value
    /// </summary>
    [Required]
    [Column("scc_value")]
    public string SccValue { get; set; }

    /// <summary>
    /// scc_default
    /// </summary>
    [Required]
    [Column("scc_default")]
    public string SccDefault { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("scc_createname")]
    public string SccCreateName { get; set; } = "";

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("scc_createtime")]
    public DateTime SccCreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("scc_modifyname")]
    public string SccModifyName { get; set; } = "";

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("scc_modifytime")]
    public DateTime SccModifyTime { get; set; } = DateTime.Now; 

}
