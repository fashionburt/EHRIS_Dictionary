using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("HR_wage")]

public class HRWage
{
    [Key]
    [Column("wag_no")]
    public Int64 WagNo { get; set; }

    [Column("wag_id")]    
    public Guid WagId { get; set; }

    [Required]
    [Column("wag_Unitid")]
    [MaxLength(20)]
    public string WagUnitId { get; set; }

    [Required]
    [Column("wag_UnitName")]
    [MaxLength(40)]
    public string WagUnitName { get; set; }

    [Column("wag_depno")]
    public int WagDepNo { get; set; }

    [Required]
    [Column("wag_depid")]
    [MaxLength(20)]
    public string WagDepId { get; set; }

    [Required]
    [Column("wag_depname")]
    [MaxLength(40)]
    public string WagDepName { get; set; }

    [Column("wag_ptyno")]
    public int WagPtyNo { get; set; }

    [Required]
    [Column("wag_ptycode")]
    [MaxLength(4)]
    public string WagPtyCode { get; set; }

    [Required]
    [Column("wag_ptyname")]
    [MaxLength(40)]
    public string WagPtyName { get; set; }

    [Column("wag_prono")]
    public int WagProNo { get; set; }

    [Required]
    [Column("wag_procode")]
    [MaxLength(4)]
    public string WagProCode { get; set; }

    [Required]
    [Column("wag_proname")]
    [MaxLength(40)]
    public string WagProName { get; set; }

    [Column("wag_director")]
    public int WagDirector { get; set; } = 0;

    [Column("wag_pleno")]
    public int WagPleNo { get; set; }

    [Required]
    [Column("wag_plecode")]
    [MaxLength(4)]
    public string WagPleCode { get; set; }

    [Required]
    [Column("wag_plename")]
    [MaxLength(40)]
    public string WagPleName { get; set; }

    [Required]
    [Column("wag_peoname")]
    [MaxLength(20)]
    public string WagPeoName { get; set; }

    [Required]
    [Column("wag_idcard")]
    [MaxLength(20)]
    public string WagIdCard { get; set; }

    [Column("wag_sex")]
    public int WagSex { get; set; } = 1;

    [Column("wag_age")]
    public int WagAge { get; set; } = 0;
    
    [Column("wag_originalid1")]
    public int WagOriginalId1 { get; set; }

    [Column("wag_originalid2")]
    public int WagOriginalId2 { get; set; }

    [Required]
    [Column("wag_applydate")]
    public DateTime WagApplyDate { get; set; }

    [Required]
    [Column("wag_cdate")]
    public DateTime WagCDate { get; set; }

    [Column("wag_verify")]
    public int WagVerify { get; set; }

    [Required]
    [Column("wag_type")]
    [MaxLength(1)]
    public string WagType { get; set; }

    [Required]
    [Column("wag_methods")]
    [MaxLength(2)]
    public string WagMethods { get; set; }

    [Required]
    [Column("wag_YM")]
    [MaxLength(10)]
    public string WagYM { get; set; }

    [Column("wag_totalmoney")]
    public int WagTotalMoney { get; set; } = 0;

    [Required]
    [Column("wag_where")]
    public DateTime WagWhere { get; set; }

    [Required]
    [Column("wag_sdate")]
    public DateTime WagSDate { get; set; }

    [Required]
    [Column("wag_edate")]
    public DateTime WagEDate { get; set; }

    [Required]
    [Column("wag_addin")]
    public DateTime WagAddIn { get; set; }

    [Required]
    [Column("wag_addout")]
    public DateTime WagAddOut { get; set; }

   
    [Column("wag_applyhour")]
    public decimal WagApplyHour { get; set; }

    [Column("wag_1d1hour")]
    public decimal Wag1D1Hour { get; set; }

    [Column("wag_4d3hour")]
    public decimal Wag4D3Hour { get; set; }

    [Column("wag_5d3hour")]
    public decimal Wag5D3Hour { get; set; }

    [Column("wag_2d1hour")]
    public decimal Wag2D1Hour { get; set; }

    [Column("wag_8d3hour")]
    public decimal Wag8D3Hour { get; set; }

    [Column("wag_dt01no")]
    public int WagDt01No { get; set; }

    [Required]
    [Column("wag_labor")]
    [MaxLength(1)]
    public string WagLabor { get; set; }

    [Column("wag_issuehour")]
    public decimal WagIssueHour { get; set; }

    [Column("wag_subtotal")]
    public decimal WagSubtotal { get; set; }

    [Required]
    [Column("wag_reason")]
    [MaxLength(500)]
    public string WagReason { get; set; }

    [Required]
    [Column("wag_ovetype")]
    [MaxLength(1)]
    public string WagOveType { get; set; }

    [Required]
    [Column("wag_exaname")]
    [MaxLength(100)]
    public string WagExaName { get; set; }

    [Required]
    [Column("wag_createtime")]
    public DateTime WagCreateTime { get; set; }

    [Required]
    [Column("wag_createname")]
    [MaxLength(200)]
    public string WagCreateName { get; set; }

    [Required]
    [Column("wag_modifytime")]
    public DateTime WagModifyTime { get; set; }

    [Required]
    [Column("wag_modifyname")]
    [MaxLength(200)]
    public string WagModifyName { get; set; }

}
