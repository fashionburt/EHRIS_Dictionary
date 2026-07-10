using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities; 
[Table("accounts")]
public class Account
{
    /// <summary>
    /// acc_no
    /// </summary>
    [Key]
    [Column("acc_no")] 
    public int AccNo { get; set; }
    /// <summary>
    /// 職員編號
    /// </summary>
    [Column("peo_uid")] 
    public int PeoUid { get; set; }
    /// <summary>
    /// 帳號
    /// </summary>
    [Required, MaxLength(20)]
    [Column("acc_login")] 
    public string AccLogin { get; set; } = "";
    /// <summary>
    /// 密碼
    /// </summary>
    [Required]
    [Column("acc_paintext")] 
    public required string AccPainText { get; set; }
    /// <summary>
    /// 啟用狀態
    /// </summary>
    [Required]
    [Column("acc_status")] 
    public byte AccStatus { get; set; } = 1;
    /// <summary>
    /// 是否為第一次登入
    /// </summary>
    [Required]
    [Column("acc_firstlogin")] 
    public byte Acc_FirstLogin { get; set; } = 1;
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("acc_createtime")] 
    public DateTime AccCreateTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("acc_createname")] 
    public string? AccCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("acc_modifytime")] 
    public DateTime AccModifyTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("acc_modifyname")] 
    public string? AccModifyName { get; set; }
    /// <summary>
    ///  加密密碼
    /// </summary>
    [Required]
    [Column("acc_passwdHash")] 
    public string? AccPasswdHash { get; set; }
    /// <summary>
    /// 加密密碼Slat值
    /// </summary>
    [Required]
    [Column("acc_passwdSalt")] 
    public string? AccPasswdSalt { get; set; }
    /// <summary>
    /// 密碼修改時間
    /// </summary>
    [Required]
    [Column("acc_pwchange")] 
    public DateTime AccPwChange { get; set; }
}