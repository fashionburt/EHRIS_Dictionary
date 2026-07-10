using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EHRIS.Core.Entities
{
    [Table("admin_roles")]
    public class AdminRoles
    {
        [Key]
        [Column("adr_no")]
        public int AdrNo { get; set; }

        /// <summary>
        /// 角色名稱
        /// </summary>
        [Required]
        [Column("adr_rolename")]
        public string AdrRoleName { get; set; }

        /// <summary>
        /// 角色備註
        /// </summary>

        [Column("adr_rolememo")]
        public string AdrRoleMemo { get; set; }

        [Required]
        [Column("adr_status")]
        public byte AdrStatus { get; set; }

        [Column("adr_createtime")]
        public DateTime AdrCreateTime { get; set; } = DateTime.Now;

        [Column("adr_createname")]
        public string AdrCreateName { get; set; } = "";

        [Column("adr_modifytime")]
        public DateTime AdrModifyTime { get; set; } = DateTime.Now;

        [Column("adr_modifyname")]
        public string AdrModifyName { get; set; } = "";

    }
}
