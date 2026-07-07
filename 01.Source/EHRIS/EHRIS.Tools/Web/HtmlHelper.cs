using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EHRIS.Tools.Web
{
    public class HtmlHelper
    {
        /// <summary>
        /// 對物件中的所有 string 屬性進行 HtmlEncode
        /// </summary>  
        public static T EncodeStrings<T>(T obj)
        {
            if (obj == null) return obj;

            if (obj is IEnumerable enumerable && !(obj is string))
            {
                foreach (var item in enumerable)
                {
                    EncodeStrings(item);
                }
                return obj;
            }

            var props = obj.GetType().GetProperties()
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(string))
                {
                    var value = (string)prop.GetValue(obj);
                    if (!string.IsNullOrEmpty(value))
                    {
                        prop.SetValue(obj, WebUtility.HtmlEncode(value));
                    }
                }
                else if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(DateTime))
                {
                    var nested = prop.GetValue(obj);
                    if (nested != null)
                    {
                        EncodeStrings(nested);
                    }
                }
            }

            return obj;
        }


    }
}
