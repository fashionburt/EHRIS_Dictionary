using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("ToBeCompared")]
    [PrimaryKey(nameof(TbcPersonId), nameof(TbcStartDate), nameof(TbcStartTime), nameof(TbcEndDate), nameof(TbcEndTime), nameof(TbcLeaveType))]
    public class ToBeCompared
    {
        /// <summary>
        /// tbc_person_id
        /// </summary>
        [Key]
        [Column("tbc_person_id", Order = 0)]
        public string TbcPersonId { get; set; }

        /// <summary>
        /// tbc_start_date
        /// </summary>
        [Key]
        [Column("tbc_start_date", Order = 1)]
        public DateTime TbcStartDate { get; set; }

        /// <summary>
        /// tbc_start_time
        /// </summary>
        [Key]
        [Column("tbc_start_time", Order = 2)]
        public TimeSpan TbcStartTime { get; set; }

        /// <summary>
        /// tbc_end_date
        /// </summary>
        [Key]
        [Column("tbc_end_date", Order = 3)]
        public DateTime TbcEndDate { get; set; }

        /// <summary>
        /// tbc_end_time
        /// </summary>
        [Key]
        [Column("tbc_end_time", Order = 4)]
        public TimeSpan TbcEndTime { get; set; }

        /// <summary>
        /// tbc_leave_type
        /// </summary>
        [Key]
        [Column("tbc_leave_type", Order = 5)]
        public int TbcLeaveType { get; set; }

        /// <summary>
        /// tbc_reason
        /// </summary>
        [Column("tbc_reason")]
        public string TbcReason { get; set; }

        /// <summary>
        /// tbc_minute
        /// </summary>
        [Column("tbc_minute")]
        public int TbcMinute { get; set; }

        /// <summary>
        /// tbc_d_date
        /// </summary>
        [Column("tbc_d_date")]
        public DateTime TbcDDate { get; set; }

        /// <summary>
        /// tbc_funeral_type
        /// </summary>
        [Column("tbc_funeral_type")]
        public int TbcFuneralType { get; set; }

        /// <summary>
        /// tbc_foreign_type
        /// </summary>
        [Column("tbc_foreign_type")]
        public int TbcForeignType { get; set; }

        /// <summary>
        /// tbc_location
        /// </summary>
        [Column("tbc_location")]
        public string TbcLocation { get; set; }

        /// <summary>
        /// tbc_official_type
        /// </summary>
        [Column("tbc_official_type")]
        public int TbcOfficialType { get; set; }

        /// <summary>
        /// tbc_maternity_type
        /// </summary>
        [Column("tbc_maternity_type")]
        public int TbcMaternityType { get; set; }

        /// <summary>
        /// tbc_overtime_type
        /// </summary>
        [Column("tbc_overtime_type")]
        public int TbcOvertimeType { get; set; }

        /// <summary>
        /// tbc_comp_minute
        /// </summary>
        [Column("tbc_comp_minute")]
        public int TbcCompMinute { get; set; }

        /// <summary>
        /// tbc_pay_minute
        /// </summary>
        [Column("tbc_pay_minute")]
        public int TbcPayMinute { get; set; }

        /// <summary>
        /// tbc_awards_minutes
        /// </summary>
        [Column("tbc_awards_minutes")]
        public int TbcAwardsMinutes { get; set; }

        /// <summary>
        /// tbc_orgID
        /// </summary>
        [Column("tbc_orgID")]
        public string TbcOrgID { get; set; }

        /// <summary>
        /// tbc_depdepid
        /// </summary>
        [Column("tbc_depdepid")]
        public string TbcDepDepId { get; set; }

        /// <summary>
        /// tbc_createtime
        /// </summary>
        [Column("tbc_createtime")]
        public DateTime TbcCreateTime { get; set; } = DateTime.Now;
    }
}