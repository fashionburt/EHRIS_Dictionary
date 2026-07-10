using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("statistic_leave")]
    [PrimaryKey(nameof(StlPersonId), nameof(StlStartDate), nameof(StlStartTime), nameof(StlEndDate), nameof(StlEndTime))]
    public class StatisticLeave
    {
        /// <summary>
        /// stl_personid
        /// </summary>
        [Key]
        [Column("stl_personid", Order = 0)]
        [Required]
        [MaxLength(20)]
        public string StlPersonId { get; set; } = string.Empty;

        /// <summary>
        /// stl_startdate
        /// </summary>
        [Key]
        [Column("stl_startdate", Order = 1)]
        [Required]
        public DateTime StlStartDate { get; set; }

        /// <summary>
        /// stl_starttime
        /// </summary>
        [Key]
        [Column("stl_starttime", Order = 2)]
        public TimeSpan StlStartTime { get; set; }

        /// <summary>
        /// stl_enddate
        /// </summary>
        [Required]
        [Column("stl_enddate")]
        public DateTime StlEndDate { get; set; }

        /// <summary>
        /// stl_endtime
        /// </summary>
        [Column("stl_endtime")]
        public TimeSpan StlEndTime { get; set; }

        /// <summary>
        /// stl_leavetype
        /// </summary>
        [Column("stl_leavetype")]
        public int StlLeaveType { get; set; }

        /// <summary>
        /// stl_reason
        /// </summary>
        [Required]
        [Column("stl_reason")]
        public string StlReason { get; set; } = string.Empty;

        /// <summary>
        /// stl_minutes
        /// </summary>
        [Column("stl_minutes")]
        public int StlMinutes { get; set; }

        /// <summary>
        /// stl_ddate
        /// </summary>
        [Required]
        [Column("stl_ddate")]
        public DateTime StlDDate { get; set; }

        /// <summary>
        /// stl_funeraltype
        /// </summary>
        [Column("stl_funeraltype")]
        public int StlFuneralType { get; set; }

        /// <summary>
        /// stl_foreigntype
        /// </summary>
        [Column("stl_foreigntype")]
        public int StlForeignType { get; set; }

        /// <summary>
        /// stl_location
        /// </summary>
        [Required]
        [Column("stl_location")]
        [MaxLength(100)]
        public string StlLocation { get; set; } = string.Empty;

        /// <summary>
        /// stl_officialtype
        /// </summary>
        [Column("stl_officialtype")]
        public int StlOfficialType { get; set; }

        /// <summary>
        /// stl_maternitytype
        /// </summary>
        [Column("stl_maternitytype")]
        public int StlMaternityType { get; set; }

        /// <summary>
        /// stl_createtime
        /// </summary>
        [Required]
        [Column("stl_createtime")]
        public DateTime StlCreateTime { get; set; }

        /// <summary>
        /// stl_modifytime
        /// </summary>
        [Required]
        [Column("stl_modifytime")]
        public DateTime StlModifyTime { get; set; }

        /// <summary>
        /// stl_orgid
        /// </summary>
        [Required]
        [Column("stl_orgid")]
        [MaxLength(50)]
        public string StlOrgId { get; set; } = string.Empty;

        /// <summary>
        /// stl_depid
        /// </summary>
        [Required]
        [Column("stl_depid")]
        [MaxLength(20)]
        public string StlDepId { get; set; } = string.Empty;

        /// <summary>
        /// stl_depno
        /// </summary>
        [Column("stl_depno")]
        public int StlDepNo { get; set; }
    }
}