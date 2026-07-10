using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common
{
    /// <summary>
    /// 可查詢的人員類別權限
    /// </summary>
    public class UserPtypePermissionViewModel
    {
        public int PtyNo { get; set; }
        public string PtyCode { get; set; }
        public string PtyName { get; set; }
    }
}
