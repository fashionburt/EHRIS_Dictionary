using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("baseperson_bank")]
public class BasePersonBank
{
    /// <summary>
    /// bbn_no
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("bbn_no")]
    public int BbnNo { get; set; }
    /// <summary>
    /// bas_id
    /// </summary>
    [Column("bas_id")]
    public Guid BasId { get; set; }
    /// <summary>
    /// bbn_bancode
    /// </summary>
    [Required]
    [Column("bbn_bancode")]
    [MaxLength(10)]
    public string BbnBanCode { get; set; }
    /// <summary>
    /// bbn_bankaccount
    /// </summary>
    [Required]
    [Column("bbn_bankaccount")]
    [MaxLength(30)]
    public string BbnBankAccount { get; set; }
    /// <summary>
    /// bbn_bankuname
    /// </summary>
    [Required]
    [Column("bbn_bankuname")]
    [MaxLength(40)]
    public string BbnBankUName { get; set; }
    /// <summary>
    /// bbn_bankidcarno
    /// </summary>
    [Required]
    [Column("bbn_bankidcarno")]
    [MaxLength(32)]
    public string BbnBankIdCarNo { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("bbn_createtime")]
    public DateTime BbnCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("bbn_createname")]
    [MaxLength(400)]
    public string BbnCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("bbn_modifytime")]
    public DateTime? BbnModifyTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("bbn_modifyname")]
    [MaxLength(400)]
    public string? BbnModifyName { get; set; }
}