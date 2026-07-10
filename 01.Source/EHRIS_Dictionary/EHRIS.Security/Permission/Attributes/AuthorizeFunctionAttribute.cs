using EHRIS.Security.Permission.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Security.Permission.Attributes
{
    /// <summary>
    /// 標記 Controller Action 所需的功能代碼與操作行為
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeFunctionAttribute : Attribute
    {
        public int SfuNO { get; }
        public FunctionAction Action { get; }

        public AuthorizeFunctionAttribute(int sfuNo, FunctionAction action)
        {
            SfuNO = sfuNo;
            Action = action;
        }
    }

}
