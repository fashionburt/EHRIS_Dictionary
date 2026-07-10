using EHRIS.Core.DbContext;
using EHRIS.Core.Repositories;


namespace EHRIS.Core.Models.Common;
/// <summary>
/// 使用者登入資訊
/// </summary>
public class LoginUserInfo
{

    public int acc_no { get; set; }
    public string acc_login { get; set; }

    public int peo_uid { get; set; }
    public string peo_name { get; set; }

    public int dep_Lv1No { get; set; }
    public int dep_no { get; set; }
    public string dep_name { get; set; }
    public string uni_name { get; set; }

    public int pty_no { get; set; }
    public string? pty_name { get; set; }
    public int pro_no { get; set; }
    public string? pro_name { get; set; }

    #region 後台管理
    public int adl_no { get; set; } = 0;
    public int adu_no { get; set; } = 0;
    public int adl_rank { get; set; } = 0;
    #endregion
}