using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("sys")]
    public class Sys
    {
        /// <summary>
        /// sys_no
        /// </summary>
        [Key]
        [Column("sys_no")]
        public int SysNo { get; set; }

        /// <summary>
        /// sys_code
        /// </summary>
        [Required, StringLength(3)]
        [Column("sys_code")]
        public string SysCode { get; set; }

        /// <summary>
        /// 子系統名稱
        /// </summary>
        [Required, StringLength(50)]
        [Column("sys_name")]
        public string SysName { get; set; }

        /// <summary>
        /// 子系統類別
        /// </summary>
        [Required, StringLength(100)]
        [Column("sys_catalog")]
        public string SysCatalog { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column("sys_order")]
        public int SysOrder { get; set; }

        /// <summary>
        /// sys_default
        /// </summary>
        [Required]
        [Column("sys_default")]
        [StringLength(200)]
        public string SysDefault { get; set; }

        /// <summary>
        /// sys_defaltpic
        /// </summary>
        [Required]
        [Column("sys_defaltpic")]
        [StringLength(200)]
        public string SysDefaltPic { get; set; }

        /// <summary>
        /// sys_overpicture
        /// </summary>
        [Required]
        [Column("sys_overpicture")]
        [StringLength(200)]
        public string SysOverPicture { get; set; }

        /// <summary>
        /// 啟用狀態 0 不啟用 1啟用
        /// </summary>
        [Required, StringLength(1)]
        [Column("sys_status")]
        public byte SysStatus { get; set; }

        /// <summary>
        /// 系統建立類型 1 系統內建 2 功能模組
        /// </summary>
        [Required]
        [Column("sys_builtin")]
        public byte SysBuiltIn { get; set; }

        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("sys_createname")]
        [StringLength(400)]
        public string SysCreateName { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("sys_createtime")]
        public DateTime SysCreateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("sys_modifyname")]
        [StringLength(400)]
        public string SysModifyName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("sys_modifytime")]
        public DateTime SysModifyTime { get; set; }

       
    }

}
