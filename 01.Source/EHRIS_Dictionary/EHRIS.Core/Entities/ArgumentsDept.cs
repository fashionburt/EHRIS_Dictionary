using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EHRIS.Core.Entities
{
    [Table("arguments_dept")]
    public class ArgumentsDept
    {
        /// <summary>
        /// 流水號
        /// </summary>
        [Key]
        [Column("agd_no")]
        public int AgdNo { get; set; }

        /// <summary>
        /// 變數
        /// </summary>
        [Required]
        [Column("arg_variable")]
        public string ArgVariable { get; set; }

        /// <summary>
        /// 所屬單位
        /// </summary>
        [Column("dep_no")]
        public int DepNo { get; set; }
         
        /// <summary>
        /// 變數值-目前值
        /// </summary>
        [Required]
        [Column("agd_value")]
        public string AgdValue { get; set; }
          
        /// <summary>
        /// 啟用狀態
        /// </summary>
        [Required]
        [Column("agd_status")]
        public byte AgdStatus { get; set; } = 1;
         
        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("agd_createname")]
        public string AgdCreateName { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("agd_createtime")]
        public DateTime AgdCreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("agd_modifyname")]
        public string AgdModifyName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("agd_modifytime")]
        public DateTime AgdModifyTime { get; set; } = DateTime.Now;

    }
}
