using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("acp_overtime")]
    public class AcpOvertime
    {
        /// <summary>
        /// sn
        /// </summary>
        [Key]
        [Column("sn")]
        public int Sn { get; set; }

        /// <summary>
        /// uuid
        /// </summary>
        [Column("uuid")]
        public string? Uuid { get; set; }

        /// <summary>
        /// person_id
        /// </summary>
        [Column("person_id")]
        public string? PersonId { get; set; }

        /// <summary>
        /// start_date
        /// </summary>
        [Required]
        [Column("start_date")]
        public string StartDate { get; set; } = "";

        /// <summary>
        /// end_date
        /// </summary>
        [Required]
        [Column("end_date")]
        public string EndDate { get; set; } = "";

        /// <summary>
        /// start_time
        /// </summary>
        [Required]
        [Column("start_time")]
        public string StartTime { get; set; } = "";

        /// <summary>
        /// end_time
        /// </summary>
        [Required]
        [Column("end_time")]
        public string EndTime { get; set; } = "";

        /// <summary>
        /// leave_type
        /// </summary>
        [Column("leave_type")]
        public byte LeaveType { get; set; }

        /// <summary>
        /// reason
        /// </summary>
        [Column("reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// minute
        /// </summary>
        [Column("minute")]
        public int Minute { get; set; }

        /// <summary>
        /// overtime_type
        /// </summary>
        [Column("overtime_type")]
        public byte OvertimeType { get; set; }

        /// <summary>
        /// comp_minute
        /// </summary>
        [Column("comp_minute")]
        public int CompMinute { get; set; }

        /// <summary>
        /// pay_minute
        /// </summary>
        [Column("pay_minute")]
        public int PayMinute { get; set; }

        /// <summary>
        /// awards_minutes
        /// </summary>
        [Column("awards_minutes")]
        public int AwardsMinutes { get; set; }

        /// <summary>
        /// date_type
        /// </summary>
        [Column("date_type")]
        public byte DateType { get; set; }

        /// <summary>
        /// B19SORCOD
        /// </summary>
        [Column("B19SORCOD")]
        public string? B19SORCOD { get; set; }

        /// <summary>
        /// B19TORGAN
        /// </summary>
        [Column("B19TORGAN")]
        public string? B19TORGAN { get; set; }

        /// <summary>
        /// B19TITCOD
        /// </summary>
        [Column("B19TITCOD")]
        public string? B19TITCOD { get; set; }

        /// <summary>
        /// B19SYSCOD
        /// </summary>
        [Column("B19SYSCOD")]
        public string? B19SYSCOD { get; set; }

        /// <summary>
        /// age
        /// </summary>
        [Column("age")]
        public int Age { get; set; }

        /// <summary>
        /// B19EPPESN
        /// </summary>
        [Column("B19EPPESN")]
        public string? B19EPPESN { get; set; }

        /// <summary>
        /// B19EPCHF
        /// </summary>
        [Column("B19EPCHF")]
        public string? B19EPCHF { get; set; }

        /// <summary>
        /// count_status
        /// </summary>
        [Column("count_status")]
        public byte CountStatus { get; set; }

        /// <summary>
        /// update_date_time
        /// </summary>
        [Column("update_date_time")]
        public DateTime UpdateDateTime { get; set; }

        /// <summary>
        /// B19TUNIT
        /// </summary>
        [Column("B19TUNIT")]
        public string B19TUNIT { get; set; }

        /// <summary>
        /// B19RCOD1B
        /// </summary>
        [Column("B19RCOD1B")]
        public string B19RCOD1B { get; set; }

        /// <summary>
        /// B19RCOD1E
        /// </summary>
        [Column("B19RCOD1E")]
        public string B19RCOD1E { get; set; }

        /// <summary>
        /// B19RCOD2B
        /// </summary>
        [Column("B19RCOD2B")]
        public string B19RCOD2B { get; set; }

        /// <summary>
        /// B19RCOD2E
        /// </summary>
        [Column("B19RCOD2E")]
        public string B19RCOD2E { get; set; }
    }
}