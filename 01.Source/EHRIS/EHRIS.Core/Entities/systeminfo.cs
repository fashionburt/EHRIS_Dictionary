using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("systeminfo")]
public class SystemInfo
{
    /// <summary>
    /// 系統編號(公司唯一碼)(系統驗證用)
    /// </summary>
    [Key]
    [Column("syi_code")]
    public string SyiCode { get; set; }

    /// <summary>
    /// 系統名稱
    /// </summary>
    [Required]
    [Column("syi_name")]
    public string SyiName { get; set; }

    /// <summary>
    /// 系統驗證碼(系統驗證用)
    /// </summary>
    [Required]
    [Column("syi_pwdHash")]
    public string SyiPwdHash { get; set; }

    /// <summary>
    /// 系統驗證碼(系統驗證用)
    /// </summary>
    [Required]
    [Column("syi_pwdSalt")]
    public string SyiPwdSalt { get; set; }

    /// <summary>
    /// 網站標題名稱
    /// </summary>
    [Required]
    [Column("syi_title")]
    public string SyiTitle { get; set; }

    [Required]
    [Column("syi_subtitle")]
    public string SyiSubTitle { get; set; }

    /// <summary>
    /// Email Server
    /// </summary>
    [Required]
    [Column("syi_smtpServer")]
    public string SyiSmtpServer { get; set; }

    /// <summary>
    /// Email Port
    /// </summary>
    [Column("syi_smtpport")]
    public int SyiSmtpPort { get; set; }

    /// <summary>
    /// 是否SSL驗證 0 否  1是
    /// </summary>
    [Column("syi_smtpssl")]
    public bool SyiSmtpSsl { get; set; }

    /// <summary>
    /// 寄信電子信箱
    /// </summary>
    [Required]
    [Column("syi_emailaddr")]
    public string SyiEmailAddr { get; set; }

    /// <summary>
    /// 寄信電子信箱密碼(Base64)"
    /// </summary>
    [Required]
    [Column("syi_emailpwd")]
    public string SyiEmailPwd { get; set; }

    /// <summary>
    /// 寄信電子信箱密碼(Hash)
    /// </summary>
    [Required]
    [Column("syi_emailpwdHash")]
    public string SyiEmailPwdHash { get; set; }

    /// <summary>
    /// 加密版本號
    /// </summary>
    [Column("syi_emailpwdKeyVersion")]
    public int SyiEmailPwdKeyVersion { get; set; }

    [Column("syi_multiplemode")]
    public byte SyiMultipleMode { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("syi_createname")]
    public string SyiCreateName { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("syi_createtime")]
    public DateTime SyiCreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("syi_modifyname")]
    public string SyiModifyName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("syi_modifytime")]
    public DateTime SyiModifyTime {  get; set; }

    /// <summary>
    /// 權限設定
    /// </summary>
    [Column("syi_data")]
    public string SyiData { get; set; } = string.Empty;

    /// <summary>
    /// 權限關聯
    /// </summary>
    [Column("syi_dataHash")]
    public string SyiDataHash { get; set; } = string.Empty; 
}
