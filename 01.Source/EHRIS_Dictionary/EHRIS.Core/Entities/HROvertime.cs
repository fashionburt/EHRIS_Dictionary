using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_overtime")]

public class HROvertime
{
    /// <summary>
    /// ove_no
    /// </summary>
    [Key]
    [Column("ove_no")]
    public Int64 OveNo { get; set; }

    /// <summary>
    /// ove_guid
    /// </summary>
    [Column("ove_id")]    
    public Guid OveId { get; set; }

    /// <summary>
    /// ove_Unitid
    /// </summary>
    [Required]
    [Column("ove_Unitid")]
    [MaxLength(20)]
    public string OveUnitId { get; set; }

    /// <summary>
    /// ove_UnitName
    /// </summary>
    [Required]
    [Column("ove_UnitName")]
    [MaxLength(40)]
    public string OveUnitName { get; set; }


    /// <summary>
    /// ove_depno
    /// </summary>
    [Column("ove_depno")]
    public int OveDepNo { get;set; }


    /// <summary>
    /// ove_depid
    /// </summary>
    [Required]
    [Column("ove_depid")]
    [MaxLength(20)]
    public string OveDepId {  get; set; }

    /// <summary>
    /// ove_depname
    /// </summary>
    [Required]
    [Column("ove_depname")]
    [MaxLength(40)]
    public string OveDepName { get; set; }

    /// <summary>
    /// ove_ptyno
    /// </summary>
    [Column("ove_ptyno")]
    public int OvePtyNo { get; set; }

    /// <summary>
    /// ove_ptycode
    /// </summary>
    [Required]
    [Column("ove_ptycode")]
    [MaxLength(4)]
    public string OvePtyCode { get; set; }

    /// <summary>
    /// ove_ptyname
    /// </summary>
    [Required]
    [Column("ove_ptyname")]
    [MaxLength(40)]
    public string OvePtyName { get; set; }

    /// <summary>
    /// ove_prono
    /// </summary>
    [Column("ove_prono")]
    public int OveProNo { get; set; }


    /// <summary>
    /// ove_procode
    /// </summary>
    [Required]
    [Column("ove_procode")]
    [MaxLength(4)]
    public string OveProCode { get; set; }

    /// <summary>
    /// ove_proname
    /// </summary>
    [Required]
    [Column("ove_proname")]
    [MaxLength(40)]
    public string OveProName { get; set; }

    /// <summary>
    /// ove_director
    /// </summary>
    [Column("ove_director")]
    public int OveDirector { get; set; } = 0;

    /// <summary>
    /// ove_pleno
    /// </summary>
    [Column("ove_pleno")]
    public int OvePleNo { get; set; }

    /// <summary>
    /// ove_plecode
    /// </summary>
    [Required]
    [Column("ove_plecode")]
    [MaxLength(4)]
    public string OvePleCode { get; set; }

    /// <summary>
    /// ove_plename
    /// </summary>
    [Required]
    [Column("ove_plename")]
    [MaxLength(40)]
    public string OvePleName { get; set; }

    /// <summary>
    /// ove_peoname
    /// </summary>
    [Required]
    [Column("ove_peoname")]
    [MaxLength(20)]
    public string OvePeoName { get; set; }

    /// <summary>
    /// ove_idcard
    /// </summary>
    [Required]
    [Column("ove_idcard")]
    [MaxLength(20)]
    public string OveIdCard { get; set; }

    /// <summary>
    /// ove_sex
    /// </summary>
    [Column("ove_sex")]
    public int OveSex { get; set; } = 1;

    /// <summary>
    /// ove_age
    /// </summary>
    [Column("ove_age")]
    public int OveAge { get; set; } = 0;

    /// <summary>
    /// ove_originalid
    /// </summary>
    [Column("ove_originalid")]
    public int OveOriginalId { get; set; }

    /// <summary>
    /// ove_adate
    /// </summary>
    [Required]
    [Column("ove_adate")]
    public DateTime OveADate { get; set; }

    /// <summary>
    /// ove_dt01no
    /// </summary>
    [Column("ove_dt01no")]
    public int OveEt01No { get; set; }

    /// <summary>
    /// ove_sdate
    /// </summary>
    [Required]
    [Column("ove_sdate")]
    public DateTime OveSDate { get; set; }

    /// <summary>
    /// ove_edate
    /// </summary>
    [Required]
    [Column("ove_edate")]
    public DateTime OveEDate { get; set; }

    /// <summary>
    /// ove_days
    /// </summary>
    [Column("ove_days")]
    public int OveDays { get; set; } = 0;

    /// <summary>
    /// ove_verify
    /// </summary>
    [Column("ove_verify")]
    public int OveVerify { get; set; }

    /// <summary>
    /// ove_reason
    /// </summary>
    [Required]
    [Column("ove_reason")]
    [MaxLength(500)]
    public string OveReason { get; set; }

    /// <summary>
    /// ove_type
    /// </summary>
    [Required]
    [Column("ove_type")]
    [MaxLength(1)]
    public string OveType { get; set; }

    /// <summary>
    /// ove_brush
    /// </summary>
    [Column("ove_brush")]
    public int OveBrush { get; set; }

    /// <summary>
    /// ove_exaname
    /// </summary>
    [Required]
    [Column("ove_exaname")]
    [MaxLength(100)]
    public string OveExaName { get; set; }

    /// <summary>
    /// ove_exatype
    /// </summary>
    [Required]
    [Column("ove_exatype")]
    [MaxLength(1)]
    public string OveExaType { get; set; }

    /// <summary>
    /// ove_limitdate
    /// </summary>
    [Required]
    [Column("ove_limitdate")]
    public DateTime OveLimitDate { get; set; }

    /// <summary>
    /// ove_money
    /// </summary>
    [Column("ove_money")]
    public decimal OveMoney { get; set; }

    /// <summary>
    /// ove_labor
    /// </summary>
    [Required]
    [MaxLength(1)]
    [Column("ove_labor")]
    public string OveLabor { get; set; }

    /// <summary>
    /// ove_once
    /// </summary>
    [Required]
    [MaxLength(1)]
    [Column("ove_once")]
    public string OveOnce { get; set; }

    /// <summary>
    /// ove_where
    /// </summary>
    [Required]
    [Column("ove_where")]
    public DateTime OveWhere { get; set; }

    /// <summary>
    /// ove_holiday
    /// </summary>
    [Required]
    [Column("ove_holiday")]
    [MaxLength(1)]
    public string OveHoliday { get; set; }

    /// <summary>
    /// ove_indemnifying
    /// </summary>
    [Required]
    [Column("ove_indemnifying")]
    [MaxLength(1)]
    public string OveIndemnifying { get; set; }

    /// <summary>
    /// ove_noon
    /// </summary>
    [Required]
    [Column("ove_noon")]
    [MaxLength(1)]
    public string OveNoon { get; set; }

    /// <summary>
    /// ove_addin
    /// </summary>    
    [Column("ove_addin")]
    public DateTime? OveAddIn { get; set; }

    /// <summary>
    /// ove_addout
    /// </summary>    
    [Column("ove_addout")]
    public DateTime? OveAddOut { get; set; }

    /// <summary>
    /// ove_applymins
    /// </summary>
    [Column("ove_applymins")]
    public int OveApplyMins { get; set; } = 0;

    /// <summary>
    /// ove_signmins
    /// </summary>
    [Column("ove_signmins")]
    public int OveSignMins { get; set; } = 0;

    /// <summary>
    /// ove_checkmins
    /// </summary>
    [Column("ove_checkmins")]
    public int OveCheckMins { get; set; } = 0;

    /// <summary>
    /// ove_restmins
    /// </summary>
    [Column("ove_restmins")]
    public int OveRestMins { get; set; } = 0;

    /// <summary>
    /// ove_wagemins
    /// </summary>
    [Column("ove_wagemins")]
    public int OveWageMins { get; set; } = 0;

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("ove_createtime")]
    public DateTime OveCreateTime { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("ove_createname")]
    [MaxLength(200)]
    public string OveCreateName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("ove_modifytime")]
    public DateTime OveModifyTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("ove_modifyname")]
    [MaxLength(200)]
    public string OveModifyName { get; set; }

    /// <summary>
    /// ove_processingtime
    /// </summary>
    [Required]
    [Column("ove_processingtime")]
    public DateTime? OveProcessingTime { get; set; }

}
