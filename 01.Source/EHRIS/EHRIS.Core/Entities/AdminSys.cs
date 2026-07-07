using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("admin_sys")]
    public class AdminSys
    {
        /// <summary>
        /// sys_no
        /// </summary>
        [Key]
        [Column("ads_no")]
        public int AdsNo { get; set; }

        /// <summary>
        /// sys_code
        /// </summary>
        [Required, StringLength(3)]
        [Column("ads_code")]
        public string AdsCode { get; set; }

        /// <summary>
        /// 子系統名稱
        /// </summary>
        [Required, StringLength(50)]
        [Column("ads_name")]
        public string AdsName { get; set; } 

        /// <summary>
        /// 排序
        /// </summary>
        [Column("ads_order")]
        public int AdsOrder { get; set; }
         
        /// <summary>
        /// 啟用狀態 0 不啟用 1啟用
        /// </summary>
        [Required, StringLength(1)]
        [Column("ads_status")]
        public byte AdsStatus { get; set; }
         

       
    }

}
