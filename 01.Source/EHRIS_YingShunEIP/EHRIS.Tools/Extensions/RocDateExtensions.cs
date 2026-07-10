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
    public static class RocDateExtensions
    {
        /// <summary>
        /// 回傳民國年日期
        /// object date = "2025-09-16";
        /// date.ToRocDate();                    // ➜ "114-09-16"
        /// date.ToRocDate("ROC-yyyy/MM/dd");    // ➜ "114/09/16"
        /// date.ToRocDate("ROC-yyyy年MM月dd日");// ➜ "114年09月16日"
        /// </summary>
        /// <param name="value">DateTime日期或文字</param>
        /// <returns>說明：轉成 GGE-MM-DD 格式 如：2021-07-01 => 110-07-01</returns>
        public static string ToRocDate(this object value, string format = "ROC-yyyy-MM-dd")
        {
            if (value == null || string.IsNullOrWhiteSpace(format))
                return string.Empty;

            DateTime? date = value switch
            {
                DateTime dt => dt,
                string str => DateTime.TryParse(str, out var parsed) ? parsed : (DateTime?)null,
                _ => null
            };

            return date.HasValue ? date.Value.ToRocFormatted(format) : string.Empty;
        }


        /// <summary>
        /// 回傳民國年日期、時間(HH:mm)
        /// object dt = new DateTime(2025, 9, 16, 17, 0, 45); 
        /// dt.ToRocDateTime();                           // ➜ "114-09-16 17:00"
        /// dt.ToRocDateTime("ROC-yyyy-MM-dd HH:mm:ss");  // ➜ "114-09-16 17:00:45"
        /// dt.ToRocDateTime("ROC-yyyy年MM月dd日 HH:mm"); // ➜ "114年09月16日 17:00"
        /// </summary>
        /// <param name="value">DateTime日期或文字</param>
        /// <param name="format">時間格式</param>
        /// <returns>說明：轉成 GGE-MM-DD 格式 如：2021-07-01 17:00:25 => 110-07-01 17:00</returns>
        public static string ToRocDateTime(this object value, string format = "ROC-yyyy-MM-dd HH:mm")
        {
            if (value == null || string.IsNullOrWhiteSpace(format))
                return string.Empty;

            DateTime? date = value switch
            {
                DateTime dt => dt,
                string str => DateTime.TryParse(str, out var parsed) ? parsed : (DateTime?)null,
                _ => null
            };

            return date.HasValue ? date.Value.ToRocFormatted(format) : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToRocDateWeek(this object value, string format = "ROC-yyyy-MM-dd")
        {
            if (value == null || string.IsNullOrWhiteSpace(format))
                return string.Empty;

            DateTime? date = value switch
            {
                DateTime dt => dt,
                string str => DateTime.TryParse(str, out var parsed) ? parsed : (DateTime?)null,
                _ => null
            };

            return date.HasValue ? date.Value.ToRocDateWithWeek(format) : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToRocDateTimeWeek(this object value, string format = "ROC-yyyy-MM-dd HH:mm")
        {
            if (value == null)
                return string.Empty;

            DateTime? date = value switch
            {
                DateTime dt => dt,
                string str => DateTime.TryParse(str, out var parsed) ? parsed : (DateTime?)null,
                _ => null
            };

            return date.HasValue ? date.Value.ToRocDateTimeWithWeek(format) : string.Empty;
        }



        /// <summary>
        /// 回傳時間(HH:mm)
        /// object a = "2025-09-16 17:00:45";
        /// object b = new DateTime(2025, 9, 16, 17, 0, 45); 
        /// a.ToTime();                  // ➜ "17:00"
        /// b.ToTime("HH:mm:ss");        // ➜ "17:00:45"
        /// b.ToTime("hh:mm tt");        // ➜ "05:00 PM"
        /// b.ToTime("mm:ss");           // ➜ "00:45" 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToTime(this object value, string format = "HH:mm")
        {
            if (value == null || string.IsNullOrWhiteSpace(format))
                return string.Empty;

            DateTime? time = value switch
            {
                DateTime dt => dt,
                string str => DateTime.TryParse(str, out var parsed) ? parsed : (DateTime?)null,
                _ => null
            };

            return time.HasValue ? time.Value.ToString(format) : string.Empty;
        }



    }
}
