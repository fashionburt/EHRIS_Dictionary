using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EHRIS.Core.Entities
{
    [Table("arguments")]
    public class Arguments
    {
        /// <summary>
        /// 變數
        /// </summary>
        [Key]
        [Column("arg_variable")]
        public string ArgVariable { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [Required, MaxLength(50)]
        [Column("arg_describe")]
        public string ArgDescribe { get; set; } = "";

        /// <summary>
        /// 詳述
        /// </summary>
        [Required]
        [Column("arg_deatil")]
        public string ArgDeatil { get; set; } = "";

        /// <summary>
        ///  變數值-目前值
        /// </summary>
        [Required]
        [Column("arg_value")]
        public string ArgValue { get; set; }

        /// <summary>
        /// 變數值-預設值
        /// </summary>
        [Required]
        [Column("arg_defaultvalue")]
        public string ArgDefaultValue { get; set; }

        /// <summary>
        /// 變數來源
        /// </summary>
        [Required]
        [Column("arg_source")]
        public string ArgSource { get; set; }

        /// <summary>
        /// 是否多選
        /// </summary>
        [Required]
        [Column("arg_multisel")]
        public byte ArgMultiSel { get; set; }

        /// <summary>
        /// 分隔字元（arg_source=SPLITTEXT 時使用，空白=一字元拆一個）
        /// </summary>
        [Required]
        [Column("arg_splitchar")]
        public string ArgSplitChar { get; set; } = "";

        /// <summary>
        /// 變數群組 - arguments_group
        /// </summary>
        [Required]
        [Column("agr_group")]
        public string AgrGroup { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Required]
        [Column("arg_order")]
        public int ArgOrder { get; set; }

        /// <summary>
        /// 啟用狀態
        /// </summary>
        [Required]
        [Column("arg_status")]
        public byte ArgStatus { get; set; } = 1;

        /// <summary>
        /// 是否開放給人事使用
        /// </summary>
        [Required]
        [Column("arg_openmanager")]
        public byte ArgOpenManager { get; set; } = 0;

        /// <summary>
        /// 是否必填值
        /// </summary>
        [Required]
        [Column("arg_required")]
        public bool ArgRequired { get; set; } = false;

        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("arg_createname")]
        public string ArgCreateName { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("arg_createtime")]
        public DateTime ArgCreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("arg_modifyname")]
        public string ArgModifyName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("arg_modifytime")]
        public DateTime ArgModifyTime { get; set; } = DateTime.Now;

    }
}
