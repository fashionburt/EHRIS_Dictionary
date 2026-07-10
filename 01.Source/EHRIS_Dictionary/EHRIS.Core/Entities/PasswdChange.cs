using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("passwdChange")]
public class PasswdChange
{
    /// <summary>
    /// pas_no
    /// </summary>
    [Key]
    [Column("pas_no")]
    public int PasNo {  get; set; }

    /// <summary>
    /// 密碼
    /// </summary>
    [Required]
    [Column("pas_paintext")]
    public string PasPainText {  get; set; }

    /// <summary>
    /// 密碼Hash值
    /// </summary>
    [Required]
    [Column("pas_passwdHash")]
    public string PasPasswdHash { get; set; }

    /// <summary>
    /// 密碼Salt
    /// </summary>
    [Required]
    [Column("pas_passwdSalt")]
    public string PasPasswdSalt {  get; set; }

    /// <summary>
    /// acc_no
    /// </summary>
    [Column("pas_accno")]
    public int PasAccNo {  get; set; }

    /// <summary>
    /// peo_uid
    /// </summary>
    [Column("pas_changeUID")]
    public int PasChangeUID { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("pas_datetime")]
    public DateTime PasDatetime { get; set; }

}
