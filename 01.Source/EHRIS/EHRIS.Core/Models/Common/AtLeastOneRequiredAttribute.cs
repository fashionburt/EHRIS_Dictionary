using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common
{
    /// <summary>
    /// 多欄位擇一驗證方式
    /// 範例：
    /// public class ContactViewModel
    /// {
    ///     public string Email  { get; set; }
    ///     public string Phone  { get; set; }
    ///     public string LineId  { get; set; }
    /// 
    ///     [AtLeastOneRequired("Email", "Phone", "LineId", ErrorMessage = "請至少填寫一種聯絡方式")]
    ///     public string ContactCheck { get; set; } // Dummy 屬性只用於驗証不使用
    /// }
    /// </summary>
    public class AtLeastOneRequiredAttribute : ValidationAttribute
    {
        private readonly string[] _propertyNames;

        public AtLeastOneRequiredAttribute(params string[] propertyNames)
        {
            _propertyNames = propertyNames;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var type = validationContext.ObjectType;
            var instance = validationContext.ObjectInstance;

            bool hasValue = _propertyNames.Any(name =>
            {
                var prop = type.GetProperty(name);
                if (prop == null) return false;

                var val = prop.GetValue(instance);
                return val is string str ? !string.IsNullOrWhiteSpace(str) : val != null;
            });

            return hasValue
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "至少填寫一個欄位", _propertyNames);
        }
    }

}
