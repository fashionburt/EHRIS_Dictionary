using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("StaParams")]
public class StaParams
{
    [Key]
    [Column("stp_no")]
    public int StpNo { get; set; }

    [Required]
    [Column("stp_name")]
    public string StpName { get; set; }

    [Required]
    [Column("stp_code")]
    public string StpCode {  get; set; }

    [Required]
    [Column("stp_type")]
    public string StpType { get; set; }

    [Required]
    [Column("stp_params1")]
    public string StpParams1 { get; set; }

    [Required]
    [Column("stp_params2")]
    public string StpParams2 { get; set; }

    [Column("stp_order")]
    public int StpOrder {  get; set; }

    [Required]
    [Column("stp_status")]
    public string StpStatus { get; set; }
}
