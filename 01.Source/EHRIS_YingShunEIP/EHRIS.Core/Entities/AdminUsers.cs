using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EHRIS.Core.Entities
{
    [Table("admin_users")]
    public class AdminUsers
    {
        [Key]
        [Column("adu_no")]
        public int AduNo { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        [Required, MaxLength(20)]
        [Column("adu_login")]
        public string AduLogin { get; set; } = "";

        /// <summary>
        /// 姓名
        /// </summary>
        [Column("adu_displayname")]
        public string AduDisplayName { get; set; } = "";
        
        [Required]
        [EmailAddress]
        [Column("adu_email")]
        public string AduEmail { get; set; } = "";

        /// <summary>
        ///  加密密碼
        /// </summary>
        [Required]
        [Column("adu_passwdHash")]
        public string? AduPasswdHash { get; set; }
        /// <summary>
        /// 加密密碼Slat值
        /// </summary>
        [Required]
        [Column("adu_passwdSalt")]
        public string? AduPasswdSalt { get; set; }

        /// <summary>
        /// 啟用狀態
        /// </summary>
        [Required]
        [Column("adu_status")]
        public byte AduStatus { get; set; } = 1;


        [Column("adu_createtime")]
        public DateTime AduCreateTime { get; set; } = DateTime.Now;

        [Column("adu_createname")]
        public string AduCreateName { get; set; } = "";

        [Column("adu_modifytime")]
        public DateTime? AduModifyTime { get; set; } = DateTime.Now;

        [Column("adu_modifyname")]
        public string? AduModifyName { get; set; }

    }
}
