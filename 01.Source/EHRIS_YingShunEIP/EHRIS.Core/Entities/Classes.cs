using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("classes")]
public class Classes
{
    [Key]
    [Column("cla_no")]
    public int ClaNo { get; set; }

    [Column("cla_number")]
    public string ClaNumber { get; set; }

    [Column("cla_name")]
    public string ClaName { get; set; }

    [Column("cla_daystart")]
    public string ClaDaystart { get; set; }

    [Column("cla_dayend")]
    public string ClaDayend { get; set; }

    [Column("cla_stime")]
    public string ClaStime { get; set; }
    [Column("cla_etime")]
    public string ClaEtime { get; set; }

    [Column("cla_otime")]
    public string ClaOtime { get; set; }

    [Column("cla_mtime")]
    public string ClaMtime { get; set; }

    [Column("cla_befoream")]
    public string ClaBefoream { get; set; }

    [Column("cla_amst")]
    public string ClaAmst { get; set; }

    [Column("cla_amelast")]
    public string ClaAmelast { get; set; }
    [Column("cla_amet")]
    public string ClaAmet { get; set; }

    [Column("cla_pmst")]
    public string ClaPmst { get; set; }

    [Column("cla_pmslast")]
    public string ClaPmslast { get; set; }

    [Column("cla_beforepm")]
    public string ClaBeforepm { get; set; }

    [Column("cla_pmet")]
    public string ClaPmet { get; set; }

    [Column("cla_pmelast")]
    public string ClaPmelast { get; set; }
    [Column("cla_overtime")]
    public string ClaOvertime { get; set; }

    [Column("cla_beforework")]
    public string ClaBeforework { get; set; }

    [Column("cla_afterwork")]
    public string ClaAfterwork { get; set; }

    [Column("cla_open")]
    public string ClaOpen { get; set; }

    [Column("cla_noon")]
    public string ClaNoon { get; set; }

    [Column("cla_end")]
    public string ClaEnd { get; set; }

    [Column("cla_amhour")]
    public int ClaAmhour { get; set; }

    [Column("cla_pmhour")]
    public int ClaPmhour { get; set; }

    [Column("cla_totalhour")]
    public int ClaTotalhour { get; set; }

    [Column("cla_another")]
    public bool ClaAnother { get; set; }

    [Column("cla_isauth")]
    public string ClaIsauth { get; set; }

    [Column("cla_status")]
    public string ClaStatus { get; set; }

    [Column("cla_createname")]
    public string ClaCreatename { get; set; }

    [Column("cla_createtime")]
    public DateTime ClaCreatetime { get; set; }

    [Column("cla_modifname")]
    public string ClaModifname { get; set; }

    [Column("cla_modifytime")]
    public DateTime ClaModifytime { get; set; } 

}
