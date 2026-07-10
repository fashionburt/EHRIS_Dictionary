using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_wkrecord")]

public class HRWkRecord
{
    [Key]
    [Column("wkr_no")]
    public Int64 WkrNo { get; set; }

    [Required]
    [Column("wkr_id")]
    public Guid WkrId { get; set; }

    [Required]
    [Column("wkr_Unitid")]
    [MaxLength(20)]
    public string WkrUnitID { get; set; }

    [Required]
    [Column("wkr_UnitName")]
    [MaxLength(40)]
    public string WkrUnitName { get; set; }

    [Column("wkr_depno")]
    public int WkrDepNo { get; set; }

    [Required]
    [Column("wkr_depid")]
    [MaxLength(20)]
    public string WkrDepID { get; set; }

    [Required]
    [Column("wkr_depname")]
    [MaxLength(40)]
    public string WkrDepName { get; set; }

    [Column("wkr_ptyno")]
    public int WkrPtyNo { get; set; }

    [Required]
    [Column("wkr_ptycode")]
    [MaxLength(4)]
    public string WkrPtyCode { get; set; }

    [Required]
    [Column("wkr_ptyname")]
    [MaxLength(40)]
    public string WkrPtyName { get; set; }

    [Column("wkr_prono")]
    public int WkrProNo { get; set; }

    [Required]
    [Column("wkr_procode")]
    [MaxLength(4)]
    public string WkrProCode { get; set; }

    [Required]
    [Column("wkr_proname")]
    [MaxLength(40)]
    public string WkrProName { get; set; }

    [Column("wkr_director")]
    public int WkrDirector { get; set; } = 0;

    [Column("wkr_pleno")]
    public int WkrPleNo { get; set; }

    [Required]
    [Column("wkr_plecode")]
    [MaxLength(4)]
    public string WkrPleCode { get; set; }

    [Required]
    [Column("wkr_plename")]
    [MaxLength(40)]
    public string WkrPleName { get; set; }

    [Required]
    [Column("wkr_peoname")]
    [MaxLength(20)]
    public string WkrPeoName { get; set; }

    [Column("wkr_idcard")]
    [MaxLength(20)]
    public string WkrIdCard { get; set; }

    [Required]
    [Column("wkr_arrivedate")]
    public DateTime WkrArriveDate { get; set; }

    [Column("wkr_sex")]
    public int WkrSex { get; set; } = 1;

    [Column("wkr_age")]
    public int WkrAge { get; set; } = 0;

    [Column("wkr_originalid")]
    public Int64 WkrOriginalID { get; set; }

    [Required]
    [Column("wkr_date")]
    [MaxLength(20)]
    public string WkrDate { get; set; }

    [Column("wkr_dt01no")]
    public int WkrDT01No { get; set; }

    [Required]
    [Column("wkr_inseat")]
    [MaxLength(30)]
    public string WkrInSeat { get; set; }

    [Column("wkr_intime")]
    public DateTime? WkrInTime { get; set; }

    [Column("wkr_midtime")]
    public DateTime? WkrMidTime { get; set; }

    [Column("wkr_outtime")]
    public DateTime? WkrOutTime { get; set; }

    [Column("wkr_inps")]
    public int WkrInPS { get; set; } = 0;

    [Column("wkr_outps")]
    public int WkrOutPS { get; set; } = 0;

    [Column("wkr_tlwork")]
    public int WkrTLWork { get; set; } = 0;

    [Required]
    [Column("wkr_befoream")]
    public DateTime WkrBeforeAM { get; set; }

    [Required]
    [Column("wkr_amst")]
    public DateTime WkrAMST { get; set; }

    [Required]
    [Column("wkr_amet")]
    public DateTime WkrAMET { get; set; }

    [Required]
    [Column("wkr_amelast")]
    public DateTime WkrAMELast { get; set; }

    [Required]
    [Column("wkr_pmst")]
    public DateTime WkrPMST { get; set; }

    [Required]
    [Column("wkr_beforepm")]
    public DateTime WkrBeforePM { get; set; }

    [Required]
    [Column("wkr_pmet")]
    public DateTime WkrPMET { get; set; }

    [Required]
    [Column("wkr_pmslast")]
    public DateTime WkrPMSLast { get; set; }

    [Required]
    [Column("wkr_pmelast")]
    public DateTime WkrPMELast { get; set; }

    [Required]
    [Column("wkr_stime")]
    public DateTime WkrSTime { get; set; }

    [Required]
    [Column("wkr_otime")]
    public DateTime WkrOTime { get; set; }

    [Required]
    [Column("wkr_mtime")]
    public DateTime WkrMTime { get; set; }

    [Required]
    [Column("wkr_etime")]
    public DateTime WkrETime { get; set; }

    [Column("wkr_totalhour")]
    public int WkrTotalHour { get; set; }

    [Required]
    [Column("wkr_holiday")]
    [MaxLength(1)]
    public string WkrHoliday { get; set; }

    [Column("wkr_resthour")]
    public int WkrRestHour { get; set; }

    [Required]
    [Column("wkr_type")]
    [MaxLength(1)]
    public string WkrType { get; set; }

    [Required]
    [Column("wkr_shiftclass")]
    [MaxLength(1)]
    public string WkrShiftClass { get; set; }

    [Required]
    [Column("wkr_message")]
    [MaxLength(50)]
    public string WkrMessage { get; set; }

    [Required]
    [Column("wkr_createtime")]
    public DateTime WkrCreateTime { get; set; }

    [Required]
    [Column("wkr_createname")]
    [MaxLength(200)]
    public string WkrCreateName { get; set; }

    [Required]
    [Column("wkr_modifytime")]
    public DateTime WkrModifyTime { get; set; }

    [Required]
    [Column("wkr_modifyname")]
    [MaxLength(200)]
    public string WkrModifyName { get; set; }

    [Column("wkr_publicdays")]
    public int WkrPublicDays { get; set; } = 0;

    [Column("wkr_generaldays")]
    public int WkrGeneralDays { get; set; } = 0;

    [Column("wkr_overtime")]
    public int WkrOverTime { get; set; } = 0;

    [Column("wkr_cardmins")]
    public int WkrCardMins { get; set; }

    [Required]
    [Column("wkr_flextime")]
    [MaxLength(1)]
    public string WkrFlexTime { get; set; } = "0";


}

