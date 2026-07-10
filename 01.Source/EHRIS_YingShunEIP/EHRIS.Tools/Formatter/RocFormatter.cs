using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Tools.Formatter
{
    public static class RocFormatter
    {
        private static readonly string[] WeekDays =
        { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };


        public static string ToRocString(this DateTime date)
        {
            int rocYear = date.Year - 1911;
            if (rocYear <= 0)
                return date.ToString("yyyy-MM-dd"); // fallback for pre-ROC dates

            return $"{rocYear:000}-{date:MM-dd}";
        } 

        public static string ToRocDateTimeString(this DateTime dateTime)
        {
            int rocYear = dateTime.Year - 1911;
            if (rocYear <= 0)
                return dateTime.ToString("yyyy-MM-dd HH:mm"); // fallback for pre-ROC dates

            return $"{rocYear:000}-{dateTime:MM-dd HH:mm}";
        }

        public static string ToRocDateWithWeek(this DateTime dateTime, string format)
        {
            return $"{dateTime.ToRocFormatted(format)} ({WeekDays[(int)dateTime.DayOfWeek]})";
        }  

        public static string ToRocDateTimeWithWeek(this DateTime dateTime, string format)
        {
            return $"{dateTime.ToRocFormatted(format)} ({WeekDays[(int)dateTime.DayOfWeek]})";
        }

        public static string ToRocFormatted(this DateTime dateTime, string format)
        {
            int rocYear = dateTime.Year - 1911;
            if (rocYear <= 0)
                return dateTime.ToString(format.Replace("ROC-yyyy", "yyyy")); // fallback for pre-ROC

            return dateTime.ToString(format.Replace("ROC-yyyy", $"{rocYear:000}"));
        } 
    }
}
