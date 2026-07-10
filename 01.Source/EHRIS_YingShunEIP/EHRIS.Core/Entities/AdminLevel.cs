using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("admin_level")]
public class AdminLevel
{
    [Key]
    [Column("adl_no")]
    public int AdlNo { get; set; } 

    [Required]
    [Column("adl_name")]
    public string AdlName { get; set; }

    [Required]
    [Column("adl_rank")]
    public int AdlRank { get; set; }

    [Required]
    [Column("adl_status")]
    public int AdlStatus { get; set; }

    [Column("adl_createtime")]
    public DateTime AdlCreateTime { get; set; } = DateTime.Now;

    [Column("adl_createname")]
    public string AdlCreateName { get; set; } = "";

    [Column("adl_modifytime")]
    public DateTime AdlModifyTime { get; set; } = DateTime.Now;

    [Column("adl_modifyname")]
    public string AdlModifyName { get; set; } = "";
}