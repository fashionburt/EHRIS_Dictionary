using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("menu_access")]
public class Menu_Access
{
    [Key]
    [Column("access_id")]
    public int Access_Id { get; set; }

    [Column("client_ip")]
    public string Client_Ip { get; set; }

    [Column("menu_id")]
    public int Menu_Id { get; set; }

    [Column("is_enabled")]
    public int Is_Enabled { get; set; }

    [Required]
    [Column("create_date")]
    public DateTime Create_Date { get; set; } = DateTime.Now;
}
