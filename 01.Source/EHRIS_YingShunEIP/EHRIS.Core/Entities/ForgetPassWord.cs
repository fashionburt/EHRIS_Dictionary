using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("forgetPassWord")]
public class ForgetPassWord
{
    /// <summary>
    /// GUID
    /// </summary>
    [Key]
    [Column("fpw_id")]
    public Guid FpwId { get; set; }
    /// <summary>
    /// 帳號編號
    /// </summary>
    [Column("fpw_accno")]
    public int FpwAccno { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("fpw_cdate")]
    public DateTime FpwCdate { get; set; }
    /// <summary>
    /// 驗證碼
    /// </summary>
    [Required]
    [Column("fpw_code")]
    [MaxLength(10)]
    public string FpwCode { get; set; }
    /// <summary>
    /// 驗證碼到期時間
    /// </summary>
    [Required]
    [Column("fpw_codetime")]
    public DateTime FpwCodeTime { get; set; }
    /// <summary>
    /// 驗證狀態
    /// </summary>
    [Required]
    [Column("fpw_state", TypeName = "char(10)")]
    public string FpwState { get; set; }
    /// <summary>
    /// 密碼Token值
    /// </summary>
    [Required]
    [Column("fpw_token")]
    [MaxLength(100)]
    public string FpwToken { get; set; }
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("fpw_createtime")]
    public DateTime FpwCreateTime { get; set; }= DateTime.Now;
    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("fpw_createname")]
    [MaxLength(400)]
    public string FpwCreateName { get; set; }
    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("fpw_modifytime")]
    public DateTime? FpwModifyTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("fpw_modifyname")]
    [MaxLength(400)]
    public string? FpwModifyName { get; set; } 
}