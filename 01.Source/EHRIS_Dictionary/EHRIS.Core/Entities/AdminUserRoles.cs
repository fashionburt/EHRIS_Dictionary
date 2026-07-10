using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("admin_userroles")]
    public class AdminUserRoles
    {
        /// <summary>
        /// aur_no
        /// </summary>
        [Key]
        [Column("aur_no")]
        public int AurNo { get; set; }

        /// <summary>
        /// adr_no
        /// </summary>
        [Column("adr_no")]
        public int AdrNo { get; set; }

        /// <summary>
        /// adu_no
        /// </summary>
        [Column("adu_no")]
        public int AduNo { get; set; }
         
    }
}
