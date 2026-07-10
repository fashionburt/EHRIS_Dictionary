using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("statistic_overtime")]
    [PrimaryKey(nameof(StoPersonId), nameof(StoStartDate), nameof(StoStartTime), nameof(StoEndDate), nameof(StoEndTime))]
    public class StatisticOvertime
    {
        /// <summary>
        /// sto_personid
        /// </summary>
        [Key]
        [Column("sto_personid", Order = 0)]
        [Required]
        [MaxLength(20)]
        public string StoPersonId { get; set; } = string.Empty;

        /// <summary>
        /// sto_startdate
        /// </summary>
        [Key]
        [Column("sto_startdate", Order = 1)]
        [Required]
        public DateTime StoStartDate { get; set; }

        /// <summary>
        /// sto_starttime
        /// </summary>
        [Key]
        [Column("sto_starttime", Order = 2)]
        public TimeSpan StoStartTime { get; set; }

        /// <summary>
        /// sto_enddate
        /// </summary>
        [Required]
        [Column("sto_enddate")]
        public DateTime StoEndDate { get; set; }

        /// <summary>
        /// sto_endtime
        /// </summary>
        [Column("sto_endtime")]
        public TimeSpan StoEndTime { get; set; }

        /// <summary>
        // sto_reason
        /// </summary>
        [Required]
        [Column("sto_reason")]
        public string StoReason { get; set; } = string.Empty;

        /// <summary>
        /// sto_minutes
        /// </summary>
        [Column("sto_minutes")]
        public int StoMinutes { get; set; }

        /// <summary>
        /// sto_overtime_type
        /// </summary>
        [Column("sto_overtime_type")]
        public int StoOvertimeType { get; set; }

        /// <summary>
        /// sto_comp_minute
        /// </summary>
        [Column("sto_comp_minute")]
        public int StoCompMinute { get; set; }

        /// <summary>
        /// sto_pay_minute
        /// </summary>
        [Column("sto_pay_minute")]
        public int StoPayMinute { get; set; }

        /// <summary>
        /// sto_awards_minutes
        /// </summary>
        [Column("sto_awards_minutes")]
        public int StoAwardsMinutes { get; set; }

        /// <summary>
        /// sto_orgid
        /// </summary>
        [Required]
        [Column("sto_orgid")]
        [MaxLength(50)]
        public string StoOrgId { get; set; } = string.Empty;

        /// <summary>
        /// sto_depid
        /// </summary>
        [Required]
        [Column("sto_depid")]
        [MaxLength(20)]
        public string StoDepId { get; set; } = string.Empty;

        /// <summary>
        /// sto_depno
        /// </summary>
        [Column("sto_depno")]
        public int StoDepNo { get; set; }

        /// <summary>
        /// sto_createtime
        /// </summary>
        [Required]
        [Column("sto_createtime")]
        public DateTime StoCreateTime { get; set; }

        /// <summary>
        /// sto_modifytime
        /// </summary>
        [Required]
        [Column("sto_modifytime")]
        public DateTime StoModifyTime { get; set; }
    }
}