using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common
{
    /// <summary>
    /// 可查詢的單位權限
    /// </summary>
    public class UserDepPermissionViewModel
    {
        public int DepNo { get; set; } 
        public string Dep_Name { get; set; }
    }
}
