using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("log")]
public class Log
{
    /// <summary>
    /// log_id
    /// </summary>
    [Key]
    [Column("log_id")]
    public int LogId { get; set; }

    /// <summary>
    /// dbkey
    /// </summary>
    [Column("dbkey")]
    public string? DbKey { get; set; }

    /// <summary>
    /// tablename
    /// </summary>
    [Column("tablename")]
    public string? TableName { get; set; }

    /// <summary>
    /// pkName
    /// </summary>
    [Column("pkName")]
    public string? PkName { get; set; }

    /// <summary>
    /// state
    /// </summary>
    [Column("state")]
    public int State { get; set; }

    /// <summary>
    /// detail
    /// </summary>
    [Column("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// date
    /// </summary>
    [Required]
    [Column("date")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Column("server_ip")]
    public string ServerIP { get; set; } = string.Empty;
}