using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EHRIS.Core.Entities
{
    [Table("arguments_group")]
    public class ArgumentsGroup
    {
        /// <summary>
        /// 群組編碼
        /// </summary>
        [Key, MaxLength(5)]
        [Column("agr_group")]
        public string AgrGroup { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [Required, MaxLength(30)]
        [Column("agr_text")]
        public string AgrText { get; set; } = "";
         
        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("agr_createname")]
        public string AgrCreateName { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("agr_createtime")]
        public DateTime AgrCreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("agr_modifyname")]
        public string AgrModifyName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("agr_modifytime")]
        public DateTime AgrModifyTime { get; set; } = DateTime.Now;

    }
}
