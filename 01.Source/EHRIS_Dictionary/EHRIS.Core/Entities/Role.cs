using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("role")]

public class Role
{
    /// <summary>
    /// 角色代碼
    /// </summary>
    [Key]
    [Column("rol_no")]
    public int RolNo { get; set; }

    /// <summary>
    /// 角色名稱
    /// </summary>
    [Required]
    [Column("rol_name")] 
    public string RolName { get; set; }

    /// <summary>
    /// 角色備註
    /// </summary>
   
    [Column("rol_memo")] 
    public string RolMemo { get; set; }

    /// <summary>
    /// 是否開啟，其值0：關閉、1：開啟
    /// </summary>
    [Column("rol_open")] 
    public byte RolOpen { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
   
    [Column("rol_createname")] 
    public string RolCreateName { get; set; } = "";

    /// <summary>
    /// 建立時間
    /// </summary>
    
    [Column("rol_createtime")] 
    public DateTime RolCreateTime { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 修改人
    /// </summary>
    
    [Column("rol_modifyname")] 
    public string RolModifyName { get; set; } = "";

    /// <summary>
    /// 修改時間
    /// </summary>
    
    [Column("rol_modifytime")] 
    public DateTime RolModifyTime { get; set; } = DateTime.Now;

}
