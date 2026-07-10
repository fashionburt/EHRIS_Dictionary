using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("sheet")]
public class Sheet
{
    /// <summary>
    /// sheet_id
    /// </summary>
    [Key]
    [Column("sheet_id")]
    public int SheetId { get; set; }

    /// <summary>
    /// menu_id
    /// </summary>
    [Column("menu_id")]
    public int MenuId { get; set; }

    /// <summary>
    /// sheet_name
    /// </summary>
    [Column("sheet_name")]
    public string? SheetName { get; set; }

    /// <summary>
    /// sheet_desc
    /// </summary>
    [Column("sheet_desc")]
    public string? SheetDesc { get; set; }

    /// <summary>
    /// sort_order
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("server_ip")]
    public string ServerIP { get; set; } = string.Empty;
}