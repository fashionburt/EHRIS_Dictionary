using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("sysfuction")]
public class SysFuction
{
    /// <summary>
    /// sfu_no
    /// </summary>
    [Key]
	[Column("sfu_no")]
	public int SfuNo { get; set; }

    /// <summary>
    /// sys_no
    /// </summary>
    [Column("sys_no")]
    public int SysNo { get; set; } // 關聯 `sys_no` 作為父選單 ID

    /// <summary>
    /// 功能名稱
    /// </summary>
    [Required]
    [Column("sfu_name")]
    public string SfuName { get; set; }

    /// <summary>
    /// 顯示功能名稱
    /// </summary>
    [Required]
    [Column("sfu_disname")]
    public string SfuDisName { get; set; }

    /// <summary>
    /// 功能簡稱
    /// </summary>
    [Required]
    [Column("sfu_disshorten")]
    public string SfuShorten { get; set; }

    /// <summary>
    /// sfu_catalog
    /// </summary>
    [Required]
    [Column("sfu_catalog")]
    public string SfuCatalog { get; set; }

    /// <summary>
	/// 順序
	/// </summary>
    [Column("sfu_order")]
    public int SfuOrder { get; set; }

    /// <summary>
	/// 功能程式路徑
	/// </summary>
    [Required]
    [Column("sfu_path")]
    public string SfuPath { get; set; }

    /// <summary>
	/// sfu_defaltpic
	/// </summary>
    [Required]
    [Column("sfu_defaltpic")]
    public string SfuDefaltPic { get; set; }

    /// <summary>
	/// sfu_overpicture
	/// </summary>
    [Required]
    [Column("sfu_overpicture")]
    public string SfuOverPicture { get; set; }

    /// <summary>
	/// 父節點
	/// </summary>
    [Column("sfu_parent")]
    public int SfuParent { get; set; }  //如果是第 3 層，指向上層子選單

    /// <summary>
	/// 是否啟用
	/// </summary>
    [Required]
    [Column("sfu_status")]
    public byte SfuStatus { get; set; }

    /// <summary>
    /// 系統建立類型 1 系統內建 2 功能模組
    /// </summary>
    [Required]
    [Column("sfu_builtin")]
    public byte SfuBuiltIn { get; set; }

    /// <summary>
	/// sfu_version
	/// </summary>
    [Required]
    [Column("sfu_version")]
    public string SfuVersion { get; set; }
    /// <summary>
    /// 是否有新增功能
    /// for 功能設定勾選使用(非權限)
    /// </summary>
    [Column("sfu_Ins")]
    public byte SfuIns { get; set; }
    /// <summary>
    /// 是否有編輯功能
    /// for 功能設定勾選使用(非權限)
    /// </summary>
    [Column("sfu_Edi")]
    public byte SfuEdi { get; set; }
    /// <summary>
    /// 是否有刪除功能
    /// for 功能設定勾選使用(非權限)
    /// </summary>
    [Column("sfu_Del")]
    public byte SfuDel { get; set; }


    /// <summary>
	/// 建立人
	/// </summary>
    [Required]
    [Column("sfu_createname")]
    public string SfuCreateName { get; set; }

    /// <summary>
	/// 建立時間
	/// </summary>
    [Required]
    [Column("sfu_createtime")]
    public DateTime SfuCreateTime { get; set; }

    /// <summary>
	/// 修改人
	/// </summary>
    [Required]
    [Column("sfu_modifyname")]
    public string SfuModifyName { get; set; }

    /// <summary>
	/// 修改時間
	/// </summary>
    [Required]
    [Column("sfu_modifytime")]
    public DateTime SfuModifyTime { get; set; }


	public List<SysFuction>? SubFunctions { get; set; } // 第 3 層 選單

    
}
