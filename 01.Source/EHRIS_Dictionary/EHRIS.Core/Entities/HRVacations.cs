using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_vacations")]

public class HRVacations
{
    /// <summary>
    /// vac_no
    /// </summary>
    [Key]
    [Column("vac_no")]
    public Int64 VacNo { get; set; }

    /// <summary>
    /// vac_guid
    /// </summary>
    [Column("vac_id")]    
    public Guid VacId { get; set; }

    /// <summary>
    /// vac_Unitid
    /// </summary>
    [Required]
    [Column("vac_Unitid")]
    [MaxLength(20)]
    public string VacUnitId { get; set; }

    /// <summary>
    /// vac_UnitName
    /// </summary>
    [Required]
    [Column("vac_UnitName")]
    [MaxLength(40)]
    public string VacUnitName { get; set; }

    /// <summary>
    /// vac_depno
    /// </summary>
    [Column("vac_depno")]
    public int VacDepNo { get; set; }

    /// <summary>
    /// vac_depid
    /// </summary>
    [Required]
    [Column("vac_depid")]
    [MaxLength(20)]
    public string VacDepId { get; set; }

    /// <summary>
    /// vac_depname
    /// </summary>
    [Required]
    [Column("vac_depname")]
    [MaxLength(40)]
    public string VacDepName { get; set; }

    /// <summary>
    /// vac_ptyno
    /// </summary>
    [Column("vac_ptyno")]
    public int Vac_PtyNo { get; set; }

    /// <summary>
    /// vac_ptycode
    /// </summary>
    [Required]
    [Column("vac_ptycode")]
    [MaxLength(4)]
    public string VacPtyCode { get; set; }

    /// <summary>
    /// vac_ptyname
    /// </summary>
    [Required]
    [Column("vac_ptyname")]
    [MaxLength(40)]
    public string VacPtyName { get; set; }

    /// <summary>
    /// vac_prono
    /// </summary>
    [Column("vac_prono")]
    public int VacProNo { get; set; }

    /// <summary>
    /// vac_procode
    /// </summary>
    [Required]
    [Column("vac_procode")]
    [MaxLength(4)]
    public string VacProCode { get; set; }

    /// <summary>
    /// vac_proname
    /// </summary>
    [Required]
    [Column("vac_proname")]
    [MaxLength(40)]
    public string VacProName { get; set; }

    /// <summary>
    /// vac_director
    /// </summary>
    [Column("vac_director")]
    public int VacDirector { get; set; } = 0;

    /// <summary>
    /// vac_pleno
    /// </summary>
    [Column("vac_pleno")]
    public int VacPleNo { get; set; }

    /// <summary>
    /// vac_plecode
    /// </summary>
    [Required]
    [Column("vac_plecode")]
    [MaxLength(4)]
    public string VacPleCode { get; set; }

    /// <summary>
    /// vac_plename
    /// </summary>
    [Required]
    [Column("vac_plename")]
    [MaxLength(40)]
    public string VacPleName { get; set; }

    /// <summary>
    /// vac_peoname
    /// </summary>
    [Required]
    [Column("vac_peoname")]
    [MaxLength(20)]
    public string VacPeoName { get; set; }

    /// <summary>
    /// vac_idcard
    /// </summary>
    [Required]
    [Column("vac_idcard")]
    [MaxLength(20)]
    public string VacIdCard { get; set; }

    /// <summary>
    /// vac_sex
    /// </summary>
    [Column("vac_sex")]
    public int VacSex { get; set; } = 1;

    /// <summary>
    /// vac_age
    /// </summary>
    [Column("vac_age")]
    public int VacAge { get; set; } = 0;

    /// <summary>
    /// vac_originalid
    /// </summary>
    [Column("vac_originalid")]
    public int VacOriginalId { get; set; }

    /// <summary>
    /// vac_adate
    /// </summary>
    [Required]
    [Column("vac_adate")]
    public DateTime VacADate { get; set; }

    /// <summary>
    /// vac_holno
    /// </summary>
    [Column("vac_holno")]
    public int VacHolNo { get; set; }

    /// <summary>
    /// vac_holcode
    /// </summary>
    [Required]
    [Column("vac_holcode")]
    [MaxLength(2)]
    public string VacHolCode { get; set; }

    /// <summary>
    /// vac_holname
    /// </summary>
    [Required]
    [Column("vac_holname")]
    [MaxLength(50)]
    public string VacHolName { get; set; }

    /// <summary>
    /// vac_sdate
    /// </summary>
    [Required]
    [Column("vac_sdate")]
    public DateTime VacSDate { get; set; }

    /// <summary>
    /// vac_edate
    /// </summary>
    [Required]
    [Column("vac_edate")]
    public DateTime VacEDate { get; set; }

    /// <summary>
    /// vac_holiday
    /// </summary>
    [Required]
    [Column("vac_holiday")]
    [MaxLength(2)]
    public string VacHoliday { get; set; }

    /// <summary>
    /// vac_days
    /// </summary>
    [Column("vac_days")]
    public int VacDays { get; set; } = 0;

    /// <summary>
    /// vac_verify
    /// </summary>
    [Column("vac_verify")]
    public int VacVerify { get; set; }

    /// <summary>
    /// vac_reason
    /// </summary>
    [Required]
    [Column("vac_reason")]
    public string VacReason { get; set; }

    /// <summary>
    /// vac_place
    /// </summary>
    [Required]
    [Column("vac_place")]
    [MaxLength(200)]
    public string VacPlace { get; set; }

    /// <summary>
    /// vac_fly
    /// </summary>
    [Required]
    [Column("vac_fly")]
    [MaxLength(2)]
    public string VacFly { get; set; }

    /// <summary>
    /// vac_inside
    /// </summary>
    [Required]
    [Column("vac_inside")]
    [MaxLength(1)]
    public string VacInside { get; set; }

    /// <summary>
    /// vac_type
    /// </summary>
    [Required]
    [Column("vac_type")]
    [MaxLength(1)]
    public string VacType { get; set; }

    /// <summary>
    /// vac_useyear
    /// </summary>
    [Required]
    [Column("vac_useyear")]
    [MaxLength(4)]
    public string VacUseYear { get; set; }

    /// <summary>
    /// vac_memo
    /// </summary>
    [Required]
    [Column("vac_memo")]
    [MaxLength(20)]
    public string VacMemo { get; set; }
    
    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("vac_createtime")]
    public DateTime VacCreateTime { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("vac_createname")]
    [MaxLength(200)]
    public string VacCreateName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("vac_modifytime")]
    public DateTime VacModifyTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("vac_modifyname")]
    [MaxLength(200)]
    public string VacModifyName { get; set; }

    /// <summary>
    /// vac_processingtime
    /// </summary>    
    [Column("vac_processingtime")]
    public DateTime? VacProcessingTime { get; set; }

}
