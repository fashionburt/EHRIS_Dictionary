using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("menu_access")]
public class Menu_Access
{
    [Key]
    [Column("access_id")]
    public int AccessId { get; set; }

    [Column("client_ip")]
    public string ClientIp { get; set; }

    [Column("menu_id")]
    public int MenuId { get; set; }

    [Column("is_enabled")]
    public int IsEnabled { get; set; }

    [Required]
    [Column("create_date")]
    public DateTime CreateDate { get; set; } = DateTime.Now;
}