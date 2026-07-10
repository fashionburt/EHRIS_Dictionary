using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common
{
    public class SysVariableModel
    {
        /// <summary>
        /// svr_code
        /// </summary>
       
        public string SvrCode { get; set; } = "";

        /// <summary>
        /// sar_code
        /// </summary>
      
        public string SarCode { get; set; } = "";

        /// <summary>
        /// 順序
        /// </summary>
       
        public int SvrOrder { get; set; }


        /// <summary>
        /// 參數值
        /// </summary>
      
        public string SvrName { get; set; } = "";

        /// <summary>
        /// 狀態  0:不啟用  1:啟用
        /// </summary>
       
        public string SvrStatus { get; set; } = "";
    }
}
