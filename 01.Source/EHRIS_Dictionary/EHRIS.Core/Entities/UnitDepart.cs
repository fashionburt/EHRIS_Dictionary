using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("unit_depart")]
    [PrimaryKey(nameof(UniId), nameof(UdeNo))]
    public class UnitDepart
    {
        /// <summary>
        /// uni_id
        /// </summary>
        [Required]
        [Column("uni_id")]
        [MaxLength(20)]
        public string UniId { get; set; }

        /// <summary>
        /// ude_no
        /// </summary>
        [Column("ude_no")]
        public int UdeNo { get; set; }

        /// <summary>
        /// ude_depno
        /// </summary>
        [Column("ude_depno")]
        public int UdeDepNo { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        [Column("ude_order")]
        public int UdeOrder { get; set; }

        /// <summary>
        /// ude_status
        /// </summary>
        [Required]
        [Column("ude_status")]
        [MaxLength(1)]
        public string UdeStatus { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("ude_createtime")]
        public DateTime UdeCreateTime { get; set; }
        /// <summary>
        ///  建立人
        /// </summary>
        [Required]
        [Column("ude_createname")]
        [MaxLength(400)]
        public string UdeCreateName { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("ude_modifytime")]
        public DateTime UdeModifyTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("ude_modifyname")]
        [MaxLength(400)]
        public string UdeModifyName { get; set; }
    }
}