using EHRIS.Core.DbContext;
using EHRIS.Core.Repositories;
using EHRIS.Tools.Crypto;


namespace EHRIS.Core.Models.Common;
public static class AppConfig 
{
    /// <summary>
    ///平台主標題
    /// </summary>
    public static string syi_title { get; set; }
    /// <summary>
    /// 平台次標題
    /// </summary>
    public static string syi_subtitle { get; set; }
    /// <summary>
    /// 平台單位編號(唯一碼), 認証用
    /// </summary>
    public static string syi_code { get; set; }
    /// <summary>
    /// 平台單位名稱
    /// </summary>
    public static string syi_name { get; set; }
    /// <summary>
    /// Email Server
    /// </summary>
    public static string syi_smtpServer { get; set; }
    /// <summary>
    /// Email Port
    /// </summary>
    public static int syi_smtpport { get; set; }
    /// <summary>
    /// 寄信信箱
    /// </summary>
    public static string syi_emailaddr { get; set; }
    /// <summary>
    /// 寄信是否加密
    /// </summary>
    public static bool syi_smtpssl { get; set; }
    /// <summary>
    /// 寄信密碼
    /// </summary>
    public static string syi_emailpwd { get; set; }
    /// <summary>
    /// 寄信加密密碼
    /// </summary>
    public static string syi_emailpwdHash { get; set; }
    /// <summary>
    /// 系統狀態訊息
    /// </summary>
    public static string config_message { get; set; }



    public static void LoadSystemInfo(ApplicationDbContext _dbContext , IDbHealthCheck _dbHealthCheck, string MasterKey)
    {
        config_message = "";
         var result = _dbHealthCheck.CanConnect();
        if (!result.Success)
        {
            config_message = "系統暫時無法使用";
        }
            

        var SystemConfig = _dbContext.SystemInfo.FirstOrDefault();
        if (SystemConfig == null)
        {
            config_message = "請確認系統設定";
        }
        else
        {
            syi_code = SystemConfig.SyiCode;
            syi_title = SystemConfig.SyiTitle;
            syi_subtitle = SystemConfig.SyiSubTitle;
            syi_name = SystemConfig.SyiName;
            syi_smtpServer = SystemConfig.SyiSmtpServer;
            syi_smtpport = SystemConfig.SyiSmtpPort;
            syi_emailaddr = SystemConfig.SyiEmailAddr;
            syi_smtpssl = SystemConfig.SyiSmtpSsl;
            syi_emailpwd = SystemConfig.SyiEmailPwd;
             
            if (!PasswordHasher.VerifyPassword(SystemConfig.SyiCode+ SystemConfig.SyiName, SystemConfig.SyiPwdSalt, MasterKey, SystemConfig.SyiPwdHash))
            {
                config_message = "系統尚未啟用，請洽系統管理員";
            }

        }

    }
}