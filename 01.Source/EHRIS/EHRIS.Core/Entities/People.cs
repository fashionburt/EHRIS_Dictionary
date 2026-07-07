using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("people")]
public class People
{
    /// <summary>
    /// peo_uid
    /// </summary>
    [Key]
    [Column("peo_uid")]
    public int PeoUid { get; set; }

    /// <summary>
    /// bas_id
    /// </summary>
    [Column("bas_id")]
    public Guid BasId { get; set; }

    /// <summary>
    /// 部門代碼
    /// </summary>
    [Column("dep_no")]
    public int DepNo { get; set; }

    /// <summary>
    /// 職稱代碼
    /// </summary>
    [Column("pro_no")]
    public int ProNo { get; set; }

    /// <summary>
    /// 人員類別代碼
    /// </summary>
    [Column("pty_no")]
    public int PtyNo { get; set; }

    /// <summary>
    /// 對到account.acc_no
    /// </summary>
    [Required]
    [Column("peo_account")]
    public string PeoAccount { get; set; }

    /// <summary>
    /// peo_jobtype
    /// </summary>
    [Column("peo_jobtype")]
    public int PeoJobType { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("peo_createtime")]
    public DateTime PeoCreateTime { get; set; }

    /// <summary>
    ///  建立人
    /// </summary>
    [Required]
    [Column("peo_createname")]
    public string PeoCreateName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("peo_modifytime")]
    public DateTime PeoModifyTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("peo_modifyname")]
    public string PeoModifyName { get; set; }

    // Navigation properties
    public BasePerson BasePerson { get; set; }
    public Departments Department { get; set; }
    public Account Account { get; set; }
}