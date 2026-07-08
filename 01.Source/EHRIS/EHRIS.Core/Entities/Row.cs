using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("row")]
public class Row
{
    /// <summary>
    /// row_id
    /// </summary>
    [Key]
    [Column("row_id")]
    public int RowId { get; set; }

    /// <summary>
    /// sheet_id
    /// </summary>
    [Column("sheet_id")]
    public int SheetId { get; set; }

    /// <summary>
    /// row_name
    /// </summary>
    [Column("row_name")]
    public string? RowName { get; set; }

    /// <summary>
    /// row_desc
    /// </summary>
    [Column("row_desc")]
    public string? RowDesc { get; set; }

    /// <summary>
    /// sort_order
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// row_type
    /// </summary>
    [Column("row_type")]
    public string? RowType { get; set; }

    /// <summary>
    /// row_length
    /// </summary>
    [Column("row_length")]
    public int? RowLength { get; set; }

    /// <summary>
    /// row_null
    /// </summary>
    [Column("row_null")]
    public bool RowNull { get; set; }

    /// <summary>
    /// row_remark
    /// </summary>
    [Column("row_remark")]
    public string? RowRemark { get; set; }

    [Column("server_ip")]
    public string ServerIP { get; set; } = string.Empty;
}