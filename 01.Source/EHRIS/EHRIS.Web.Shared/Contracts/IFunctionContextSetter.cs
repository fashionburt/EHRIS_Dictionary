using EHRIS.Security.Permission.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Web.Shared.Contracts
{
    /// <summary>
    /// 提供功能代碼與操作行為的上下文設定與權限檢查介面
    /// </summary>
    public interface IFunctionContextSetter
    {
        /// <summary>
        /// 設定目前操作的功能代碼與行為
        /// </summary>
        /// <param name="sfuNo">功能代碼</param>
        /// <param name="action">操作行為</param>
        void SetFunctionContext(int sfuNo, FunctionAction action);

        /// <summary>
        /// 檢查目前使用者是否擁有指定操作的權限
        /// </summary>
        /// <param name="action">操作行為</param>
        /// <returns>是否有權限</returns>
        bool HasPermission(FunctionAction action);
    }


}
