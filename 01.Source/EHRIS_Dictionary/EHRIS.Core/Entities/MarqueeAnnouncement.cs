using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("marqueeannouncement")]
public class MarqueeAnnouncement
{
    /// <summary>
    /// Id
    /// </summary>
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    /// <summary>
    /// Message
    /// </summary>
    [Column("Message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// IsEnabled
    /// </summary>
    [Column("IsEnabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// StartDate
    /// </summary>
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// EndDate
    /// </summary>
    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Priority
    /// </summary>
    [Column("Priority")]
    public int Priority { get; set; }
}