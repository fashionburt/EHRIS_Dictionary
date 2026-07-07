using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.Extensions
{
    /// <summary>
    /// Controller 擴充方法：搭配 _SwalMessages.cshtml 自動彈出 SweetAlert2 訊息
    /// <para>使用方式：this.AlertError("帳號與密碼不可為空！");</para>
    /// <para>使用方式：this.AlertSuccess("儲存成功！");</para>
    /// <para>使用方式：this.AlertInfo("即將導向首頁", "/Home/Index");</para>
    /// </summary>
    public static class AlertExtensions
    {
        private const string KeyError      = "SwalError";
        private const string KeySuccess    = "SwalSuccess";
        private const string KeyWarning    = "SwalWarning";
        private const string KeyInfo       = "SwalInfo";
        private const string KeyRedirect   = "SwalRedirectUrl";

        /// <summary>錯誤訊息（紅色 icon）</summary>
        public static void AlertError(this Controller controller, string message, string? redirectUrl = null)
        {
            controller.TempData[KeyError] = message;
            if (redirectUrl != null) controller.TempData[KeyRedirect] = redirectUrl;
        }

        /// <summary>成功訊息（綠色 icon）</summary>
        public static void AlertSuccess(this Controller controller, string message, string? redirectUrl = null)
        {
            controller.TempData[KeySuccess] = message;
            if (redirectUrl != null) controller.TempData[KeyRedirect] = redirectUrl;
        }

        /// <summary>警告訊息（黃色 icon）</summary>
        public static void AlertWarning(this Controller controller, string message, string? redirectUrl = null)
        {
            controller.TempData[KeyWarning] = message;
            if (redirectUrl != null) controller.TempData[KeyRedirect] = redirectUrl;
        }

        /// <summary>提示訊息（藍色 icon）</summary>
        public static void AlertInfo(this Controller controller, string message, string? redirectUrl = null)
        {
            controller.TempData[KeyInfo] = message;
            if (redirectUrl != null) controller.TempData[KeyRedirect] = redirectUrl;
        }
    }
}
