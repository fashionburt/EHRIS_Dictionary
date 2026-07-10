using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("admin_functions")]
public class AdminFunctions
{
    /// <summary>
    /// sfu_no
    /// </summary>
    [Key]
	[Column("adf_no")]
	public int AdfNo { get; set; } 

    /// <summary>
    /// 功能名稱
    /// </summary>
    [Required]
    [Column("adf_name")]
    public string AdfName { get; set; }
       
    /// <summary>
	/// 功能程式路徑
	/// </summary>
    [Required]
    [Column("adf_controller")]
    public string AdfController { get; set; }

    [Required]
    [Column("adf_action")]
    public string AdfAction { get; set; }

    [Column("adf_icon")]
    public string AdfIcon { get; set; } = string.Empty;

    /// <summary>
	/// 父節點
	/// </summary>
    [Column("adf_parent")]
    public int AdfParent { get; set; }  //如果是第 3 層，指向上層子選單

    /// <summary>
	///  
	/// </summary>
    [Column("adf_order")]
    public int AdfOrder { get; set; }  //如果是第 3 層，指向上層子選單

    /// <summary>
	/// 是否啟用
	/// </summary>
    [Required]
    [Column("adf_status")]
    public byte Adfstatus { get; set; }

    [Required]
    [Column("ads_no")]
    public int AdsNo { get; set; }

    [NotMapped]
    public List<AdminFunctions>? SubFunctions { get; set; } // 第 3 層 選單 

    [Column("adf_Ins")]
    public byte AdfIns { get; set; }
    [Column("adf_Edi")]
    public byte AdfEdi { get; set; }
    [Column("adf_Del")]
    public byte AdfDel { get; set; }
}
