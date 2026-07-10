using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("roleaccount")]
    public class RoleAccount
    {
        /// <summary>
        /// rac_no
        /// </summary>
        [Key]
        [Column("rac_no")]
        public int RacNo { get; set; }

        /// <summary>
        /// RolNo
        /// </summary>
        [Column("rol_no")]
        public int RolNo { get; set; }

        /// <summary>
        /// acc_no
        /// </summary>
        [Column("acc_no")]
        public int AccNo { get; set; }

        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("rac_createname")]
        [MaxLength(200)]
        public string RacCreateName { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Required]
        [Column("rac_createtime")]
        public DateTime RacCreateTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("rac_modifyname")]
        [MaxLength(200)]
        public string RacModifyName { get; set; } = string.Empty;

        /// <summary>
        /// 修改時間
        /// </summary>
        [Required]
        [Column("rac_modifytime")]
        public DateTime RacModifyTime { get; set; } = DateTime.Now;
        /*
        // 關聯 account 與 role
        [ForeignKey("acc_no")]
        public virtual Account Account { get; set; }

        [ForeignKey("rol_no")]
        public virtual Role Role { get; set; }
        */
    }
}
