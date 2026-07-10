using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("admin_rolefunctions")]
public class AdminRoleFunctions
{
    /// <summary>
    /// rau_no
    /// </summary>
    [Key]
    [Column("arf_no")]
    public int ArfNo { get; set; }

    /// <summary>
    /// adr_no
    /// </summary>
    [Column("adr_no")]
    public int AdrNo { get; set; }

    /// <summary>
    /// 對應role資料表
    /// </summary>
    [Column("adf_no")]
    public int AdfNo { get; set; }

    /// <summary>
    /// arf_cancreate
    /// </summary>
    [Column("arf_cancreate")] 
    public bool ArfCanCreate { get; set; }

    /// <summary>
    /// arf_canedit
    /// </summary>
    [Column("arf_canedit")] 
    public bool ArfCanEdit { get; set; }

    /// <summary>
    /// arf_candelete
    /// </summary>
    [Column("arf_candelete")] 
    public bool ArfCanDelete { get; set; }
     
}
