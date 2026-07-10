using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("profess_persontype")]
    public class ProfessPersonType
    {
        /// <summary>
        /// pro_no
        /// </summary>
        [Key]
        [Column("ptt_persontype")]
        public string PttPersonType { get; set; }

        /// <summary>
        /// 編號
        /// </summary>
        [Column("ptt_ptyno")]
        public int PttPtyNo { get; set; }

        /// <summary>
        /// 建立人
        /// </summary>
        [Required]
        [Column("ptt_createname")]
        public string PttCreateName { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        [Required]
        [Column("ptt_createtime")]
        public DateTime PttCreateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Required]
        [Column("ptt_modifyname")]
        public string PttModifyName { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        [Required]
        [Column("ptt_modifytime")]
        public DateTime PttModifyTime { get; set; }
    }
}
