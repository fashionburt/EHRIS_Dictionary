using EHRIS.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace EHRIS.Core.Models;

public class LoginViewModel
{
    /// <summary>
    /// 帳號
    /// </summary>
    [Required(ErrorMessage = "請輸入帳號")]
    [StringLength(20)]
    [Display(Name = "帳號：")]
    public string UxID { get; set; }

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "請輸入密碼")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度至少 6 碼")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼：")]
    public string MbrKey { get; set; }
    [Display(Name = "記住我")]
    public bool RememberMe { get; set; }
    [Required(ErrorMessage = "請輸入驗證碼")]
    
    public string DNTCaptchaText { get; set; }
    public string DNTCaptchaToken { get; set; }
    public string DNTCaptchaInputText { get; set; }

    /// <summary>
    /// 資訊公告
    /// </summary>
    public List<LoginNoticeViewModel> Notices { get; set; } = new();

    public class LoginNoticeViewModel
    {
        public int SysnNo { get; set; }
        public byte SysnType { get; set; }
        public string SysnTypeName { get; set; } = "";
        public string SysnTypeCode { get; set; } = "";
        public string SysnContent { get; set; } = "";
        public DateTime SysnPublicDt { get; set; }
        public bool SysnTop { get; set; }
    }

}
public class ForgetPasswordModel
{
    public string ForgetEmail { get; set; }
    public string IdCard { get; set; }
    public DateTime Birthday { get; set; }
    public string verifydata { get; set; }
    public string verificationCode { get; set; }
    public string newPassword { get; set; }
    public string confirmPassword { get; set; }
    public string DNTCaptchaText { get; set; }
    public string DNTCaptchaToken { get; set; }
    public string DNTCaptchaInputText { get; set; }
}
public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)] 
    public string OldPassword { get; set; }

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required, Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不符")]
    public string ConfirmPassword { get; set; }
}
