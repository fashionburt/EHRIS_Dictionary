using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common
{
    /// <summary>
    /// 可查詢的功能權限
    /// </summary>
    public class UserFuncPermissionViewModel
    {
        public int SfuNo { get; set; }
        public bool CanQuery { get; set; }

        public bool CanCreate { get; set; } 
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }
}
