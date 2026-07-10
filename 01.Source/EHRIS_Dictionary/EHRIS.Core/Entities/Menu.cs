using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("menu")]
public class Menu
{
    [Key]
    [Column("menu_id")]
    public int MenuId { get; set; }
    [Column("server_ip")]
    public string ServerIP { get; set; } = string.Empty;
    [Column("menu_name")]
    public string? MenuName { get; set; }

    [Column("menu_desc")]
    public string? MenuDesc { get; set; }

    [Column("is_enabled")]
    public int IsEnabled { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }
}