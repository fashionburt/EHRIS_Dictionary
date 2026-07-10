using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EHRIS.Tools.Formatter
{
    /// <summary>
    /// 提供常用的格式轉換工具，例如物件轉整數、轉字串等。
    /// </summary>
    public static class FormatterUtil
    {
        #region 整數轉換 
        /// <summary>
        /// 將任意物件轉換為整數，若無法轉換則回傳 0。
        /// 支援 decimal、int、double、string 等常見格式。
        /// </summary>
        public static int ToInt(this object value)
        {
            if (value == null)
                return 0;

            switch (value)
            {
                case int i:
                    return i;
                case decimal d:
                    return Decimal.ToInt32(d);
                case double db:
                    return (int)db;
                case float f:
                    return (int)f;
                case string s when int.TryParse(s, out int result):
                    return result;
                default:
                    if (decimal.TryParse(value.ToString(), out decimal temp))
                        return (int)temp;
                    return 0;
            }
        }
        #endregion

        #region 數字轉中文（一般與金額）

        //public static string ToChineseNumerals(this string str, bool useFinancialChars = false)
        //{
        //    return Regex.Replace(str, "\\d+", m =>
        //    {
        //        int value = int.Parse(m.Value);
        //        return value.ToChineseNumerals(useFinancialChars);
        //    });
        //}

        //public static string ToChineseNumerals(this int value, bool useFinancialChars = false)
        //{
        //    return ((decimal)value).ToChineseNumerals(useFinancialChars);
        //}

        //public static string ToChineseNumerals(this decimal value, bool useFinancialChars = false)
        //{
        //    string formatted = EastAsiaNumericFormatter.FormatWithCulture(
        //        useFinancialChars ? "L" : "Ln",
        //        value,
        //        null,
        //        new CultureInfo("zh-TW"));

        //    string pattern = useFinancialChars ? "[^壹貳參肆伍陸柒捌玖]拾" : "[^一二三四五六七八九]十";
        //    string one = useFinancialChars ? "壹" : "一";

        //    return Regex.Replace(formatted, pattern, m =>
        //        m.Value.Substring(0, 1) + one + m.Value.Substring(1));
        //}

        public static string ToChineseCurrencyRaw(this string value)
        {
            string result = "";
            for (int i = 0; i < value.Length; i++)
            {
                result += value[i] switch
                {
                    '0' => "零",
                    '1' => "壹",
                    '2' => "貳",
                    '3' => "參",
                    '4' => "肆",
                    '5' => "伍",
                    '6' => "陸",
                    '7' => "柒",
                    '8' => "捌",
                    '9' => "玖",
                    _ => ""
                };

                int pos = value.Length - i;
                result += pos switch
                {
                    12 => "仟",
                    11 => "佰",
                    10 => "拾",
                    9 => "億",
                    8 => "仟",
                    7 => "佰",
                    6 => "拾",
                    5 => "萬",
                    4 => "仟",
                    3 => "佰",
                    2 => "拾",
                    1 => "元整",
                    _ => ""
                };
            }
            return result;
        }

        #endregion

        #region 金額格式化

        public static string ToCurrency(this object value)
        {
            int intValue = value.ToInt();
            return intValue == 0 ? "0" : string.Format("{0:N0}", intValue);
        }

        //public static string ToCurrencyText(this object value, CurrencyType type)
        //{
        //    string raw = value?.ToString() ?? "0";
        //    string integerPart = "0", decimalPart = "";

        //    if (raw.Contains("."))
        //    {
        //        var parts = raw.Split('.');
        //        integerPart = parts[0];
        //        decimalPart = parts[1];
        //    }
        //    else
        //    {
        //        integerPart = raw;
        //    }

        //    string result = GetCurrencyChinese(integerPart, true, "圓", type) +
        //                    GetCurrencyChinese(decimalPart, false, "", type);

        //    if (!result.Contains("分") && !result.Contains("角"))
        //        result += " 元整";

        //    result = SystemChineseConverter.ToTraditional(result);

        //    if (type == CurrencyType.RMB)
        //        result = result.Replace("參", "叁");

        //    return result;
        //}

        private static string GetCurrencyChinese(string number, bool isInteger, string unit, CurrencyType type)
        {
            string[] numerals = { "零", "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖" };
            string[] units = { "厘", "分", "角", "", "拾", "佰", "仟", "萬" };
            int offset = isInteger ? 4 : 1;

            if (!isInteger)
            {
                number = number.PadRight(3, '0').Substring(0, 3);
            }
            else
            {
                if (number.Length >= 9)
                    return GetCurrencyChinese(number[..^8], true, "億", type) +
                           GetCurrencyChinese(number[^8..], true, "圓", type);
                if (number.Length >= 5)
                    return GetCurrencyChinese(number[..^4], true, "萬", type) +
                           GetCurrencyChinese(number[^4..], true, "圓", type);
            }

            string result = "";
            bool hasZero = false;

            for (int i = 0; i < number.Length; i++)
            {
                int digit = int.Parse(number[i].ToString());
                int unitIndex = number.Length - i + offset - 2;

                if (digit == 0)
                {
                    hasZero = true;
                    continue;
                }

                if (hasZero)
                {
                    if (type == CurrencyType.RMB)
                        result += "零";
                    hasZero = false;
                }

                result += numerals[digit] + units[unitIndex];
            }

            return isInteger ? result + unit : result;
        }

        public enum CurrencyType
        {
            NTD,
            RMB
        }

        #endregion

        #region 日期轉換

        public static DateTime ToDateTime(this string dateString)
        {
            string[] formats = {
                "yyyy/M/d tt hh:mm:ss", "yyyy/MM/dd tt hh:mm:ss",
                "yyyy/MM/dd HH:mm:ss", "yyyy/M/d HH:mm:ss",
                "yyyy/M/d", "yyyy/MM/dd", "yyyy-MM-dd",
                "yyyy-MM-dd HHmm", "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-M-d HH:mm:ss", "yyyy-M-d HH:mm:ss.fff",
                "yyyy-M-d", "MM/dd/yyyy", "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss.fff"
            };

            try
            {
                return DateTime.ParseExact(dateString.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            }
            catch
            {
                string fallback = dateString.Replace("-", "/");
                var culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                return DateTime.Parse(fallback, culture);
            }
        }

        #endregion


    }
}
