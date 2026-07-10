using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("unit")]
    public class Unit
    {
        /// <summary>
        /// uni_id
        /// </summary>
        [Key]
        [Column("uni_id")]
        [MaxLength(20)]
        public string UniId { get; set; }

        /// <summary>
        /// uni_name
        /// </summary>
        [Required]
        [Column("uni_name")]
        [MaxLength(200)]
        public string UniName { get; set; }

        /// <summary>
        /// uni_status
        /// </summary>
        [Required]
        [Column("uni_status")]
        [MaxLength(1)]
        public string UniStatus { get; set; }

        /// <summary>
        /// uni_order
        /// </summary>
        [Column("uni_order")]
        public int UniOrder { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("uni_createtime")]
        public DateTime UniCreateTime { get; set; }

        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("uni_createname")]
        [MaxLength(400)]
        public string UniCreateName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("uni_modifytime")]
        public DateTime UniModifyTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("uni_modifyname")]
        [MaxLength(400)]
        public string UniModifyName { get; set; }
    }
}