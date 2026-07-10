using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("ComparedError")]
    public class ComparedError
    {
        /// <summary>
        /// cer_no
        /// </summary>
        [Key]
        [Column("cer_no")]
        public Guid CerNo { get; set; }

        /// <summary>
        /// cer_person_id
        /// </summary>
        [Column("cer_person_id")]
        public string CerPersonId { get; set; }

        /// <summary>
        /// cer_start_date
        /// </summary>
        [Column("cer_start_date")]
        public DateTime CerStartDate { get; set; }

        /// <summary>
        /// cer_start_time
        /// </summary>
        [Column("cer_start_time")]
        public TimeSpan CerStartTime { get; set; }

        /// <summary>
        /// cer_end_date
        /// </summary>
        [Column("cer_end_date")]
        public DateTime CerEndDate { get; set; }

        /// <summary>
        /// cer_end_time
        /// </summary>
        [Column("cer_end_time")]
        public TimeSpan CerEndTime { get; set; }

        /// <summary>
        /// cer_orgID
        /// </summary>
        [Column("cer_orgID")]
        public string CerOrgID { get; set; }

        /// <summary>
        /// cer_leave_type
        /// </summary>
        [Column("cer_leave_type")]
        public int CerLeaveType { get; set; }

        /// <summary>
        /// cer_memo
        /// </summary>
        [Column("cer_memo")]
        public string CerMemo { get; set; }

        /// <summary>
        /// cer_createtime
        /// </summary>
        [Column("cer_createtime")]
        public DateTime CerCreateTime { get; set; }
    }
}