using System.ComponentModel.DataAnnotations;

namespace EHRIS.Core.Models.AdminPortal;
#region 平台設定

public class ADS999999ViewModel
{
    [Display(Name = "系統名稱")]
    public string SyiName { get; set; } = string.Empty;


    [Required(ErrorMessage = "請輸入網站標題名稱")]
    [Display(Name = "網站標題名稱")]
    public string SyiTitle { get; set; } = string.Empty;


    [Required(ErrorMessage = "請輸入網站副標題")]
    [Display(Name = "網站副標題")]
    public string SyiSubTitle { get; set; } = string.Empty;


    [Required(ErrorMessage = "請輸入Email Server")]
    [Display(Name = "Email Server")]
    public string SyiSmtpServer { get; set; } = string.Empty;


    [Display(Name = "Email Port")]
    public int SyiSmtpPort { get; set; }


    [Display(Name = "啟用SSL驗証")]
    public bool SyiSmtpSsl { get; set; }


    [Required(ErrorMessage = "請輸入電子信箱")]
    [EmailAddress(ErrorMessage = "格式錯誤")]
    [Display(Name = "寄信電子信箱")]
    public string SyiEmailAddr { get; set; } = string.Empty;


    [Display(Name = "寄信電子信箱密碼")]
    [DataType(DataType.Password)]
    public string? SyiEmailPwdPlain { get; set; } // 僅供畫面輸入使用

    public string SyiEmailPwd { get; set; } = string.Empty; // 儲存至資料庫的密文

    [Display(Name = "電子信箱密碼密文")]
    public string SyiEmailPwdHash { get; set; } = string.Empty;


    public int SyiEmailPwdKeyVersion { get; set; }


    public byte SyiMultipleMode { get; set; }


    public string ModifyName { get; set; } = string.Empty;


    public DateTime ModifyTime { get; set; }

}
#endregion

#region Levle
public class AdminLevelModel
{
    public int No { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }
    public int Status { get; set; } 
}

public class LevelDataViewModel
{
    public List<AdminLevelModel> Levels { get; set; } = new();

    public string ModifyName { get; set; } = string.Empty;
}
#endregion