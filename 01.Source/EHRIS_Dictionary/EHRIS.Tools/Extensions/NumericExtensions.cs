using EHRIS.Tools.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class NumericExtensions
    {
        public enum NumberFormatStyle
        {
            /// <summary>
            /// 輸出結果: "1,234,568" 
            /// 四捨五入為整數，加上千位分隔
            /// </summary>
            Integer,
            /// <summary>
            /// 輸出結果: "1,234,567.89"
            /// 千位分隔 + 固定兩位小數
            /// </summary>
            Decimal2,
            /// <summary>
            /// 輸出結果: "1,234,567.89"
            /// 千位分隔 + 最多兩位小數（若是整數則不顯示小數）
            /// </summary>
            DecimalFlexible,
            /// <summary>
            /// 輸出結果: "85%"
            /// 自動乘以 100 並加上 %
            /// </summary>
            Percent,
            /// <summary>
            /// 輸出結果: "$1,234,567.89"
            /// 美元符號 + 千位分隔 + 兩位小數
            /// </summary>
            Currency 

        }

        public static string Format(this object value, NumberFormatStyle style)
        {
            if (value == null)
                return string.Empty;
             
            if (double.TryParse(value.ToString(), out double number))
            {
                return style switch
                {
                    NumberFormatStyle.Integer => number.ToString("#,##0"),
                    NumberFormatStyle.Decimal2 => number.ToString("#,##0.00"),
                    NumberFormatStyle.DecimalFlexible => number.ToString("#,##0.##"),
                    NumberFormatStyle.Percent => (number).ToString("0%"), // 若原始值是 0.85 → "85%"
                    NumberFormatStyle.Currency => number.ToString("$#,##0.00"),
                    _ => number.ToString()
                };
            }
             
            return value.ToString(); 
        }


    } 
}
 
