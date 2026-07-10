using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRIS.Tools.ChangePassword
{
    public static class PasswordValidator
    {
        /// <summary>
        /// 密碼長度限制
        /// </summary>
        private const int MinLength = 8;
        private const int MaxLength = 20;

        /// <summary>
        /// ★ 密碼最短使用期限 (天)
        /// </summary>
        private const int MinAgeDays = 1;

        /// <summary>
        /// 密碼最長使用期限 (天)
        /// </summary>
        private const int MaxAgeDays = 90;

        /// <summary>
        /// 驗證新密碼是否符合「字串本身」的規範 (長度、複雜度、不得同帳號)
        /// </summary>
        public static PasswordValidationResult ValidateStringRules(string newPassword, string accountName)
        {
            var result = new PasswordValidationResult();

            if (string.IsNullOrEmpty(newPassword))
            {
                result.Errors.Add("密碼不可為空");
                return result;
            }

            // 長度需介於 8-20 碼
            if (newPassword.Length < MinLength || newPassword.Length > MaxLength)
            {
                result.Errors.Add($"密碼長度需介於 {MinLength}-{MaxLength} 碼字元");
            }

            // 包含大寫字母、小寫字母、數字 (三種)
            bool hasUpper = newPassword.Any(char.IsUpper);
            bool hasLower = newPassword.Any(char.IsLower);
            bool hasDigit = newPassword.Any(char.IsDigit);

            //特殊字符（如 !, @, #, $, %, ^, &, *)
            bool hasSpeicalChar = newPassword.Any(c => !char.IsLetterOrDigit(c));
            //尚未加上
            //if(!hasUpper || !hasLower || !hasDigit || !hasSpeicalChar) result.Errors.Add("密碼須包含大寫字母、小寫字母、數字、特殊字符等四種");

            if (!hasUpper || !hasLower || !hasDigit)
            {
                result.Errors.Add("密碼須包含大寫字母、小寫字母、數字等三種");
            }

            // 密碼不可與帳號相同
            if (string.Equals(newPassword, accountName, StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("密碼不可與帳號相同");
            }

            return result;
        }

        /// <summary>
        /// 驗證「最短」使用期限 (1 天)
        /// </summary>
        public static PasswordValidationResult ValidateMinAge(DateTime? lastPasswordChangeDate)
        {
            var result = new PasswordValidationResult();
            if (lastPasswordChangeDate.HasValue)
            {
                // 確保比較基準一致 (全部轉為 UTC)
                var lastChangeUtc = lastPasswordChangeDate.Value.ToUniversalTime();

                if (DateTime.UtcNow.Subtract(lastChangeUtc).TotalDays < MinAgeDays)
                {
                    result.Errors.Add($"密碼最短使用期限為 {MinAgeDays} 天。若您已於今日換過密碼或恢復預設密碼，今日不可再變更密碼。");
                }
            }
            return result;
        }

        /// <summary>
        /// 檢查密碼是否已「過期」 (90 天)
        /// <summary>
        public static bool IsPasswordExpired(DateTime lastPasswordChangeDate)
        {
            // 確保比較基準一致 (全部轉為 UTC)
            var lastChangeUtc = lastPasswordChangeDate.ToUniversalTime();

            return DateTime.UtcNow.Subtract(lastChangeUtc).TotalDays > MaxAgeDays;
        }
    }

    /// <summary>
    /// 密碼驗證的結果物件
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// 檢查 Errors 列表是否為空，判斷是否驗證有效
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// 儲存所有驗證失敗的錯誤訊息
        /// </summary>
        public List<string> Errors { get; private set; } = new List<string>();
    }
}