using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EHRIS.Core.Entities
{
    /// <summary>
    /// 參數排程
    /// </summary>
    [Table("arguments_schedule")]
    public class ArgumentsSchedule
    {
        /// <summary>
        /// 流水號
        /// </summary>
        [Key]
        [Column("ags_no")]
        public int AgsNo { get; set; }

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
        [Column("ags_value")]
        public string AgsValue { get; set; }

        /// <summary>
        /// 排程啟用開始時間
        /// </summary>
        [Required]
        [Column("ags_starttime")]
        public DateTime AgsStartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 排程啟用結束時間
        /// </summary>
        [Required]
        [Column("ags_endtime")]
        public DateTime AgsEndTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 啟用狀態
        /// </summary>
        [Required]
        [Column("ags_status")]
        public byte AgsStatus { get; set; } = 1;
         
        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("ags_createname")]
        public string AgsCreateName { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("ags_createtime")]
        public DateTime AgsCreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("ags_modifyname")]
        public string AgsModifyName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("ags_modifytime")]
        public DateTime AgsModifyTime { get; set; } = DateTime.Now;

    }
}
