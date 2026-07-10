using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;

[Table("filedata")]
public class FileData
{
    [Key]
    [Column("fil_id")]
    public Guid FilId { get; set; } = Guid.NewGuid();

    /// <summary>對應表單的 ID</summary>
    [Column("fil_uid")]
    public int FilUid { get; set; }

    /// <summary>上傳日期</summary>
    [Column("fil_date")]
    public DateTime FilDate { get; set; } = DateTime.Now;

    /// <summary>檔案放置目錄</summary>
    [Required]
    [Column("fil_dirPath")]
    public string FilDirPath { get; set; } = "";

    /// <summary>檔案名稱</summary>
    [Required]
    [Column("fil_fileName")]
    public string FilFileName { get; set; } = "";

    /// <summary>上傳的 IP</summary>
    [Required, MaxLength(250)]
    [Column("fil_ipaddress")]
    public string FilIpAddress { get; set; } = "";

    /// <summary>狀態：1=上傳, 2=刪除</summary>
    [Column("fil_status")]
    public byte FilStatus { get; set; } = 1;

    /// <summary>建立者</summary>
    [Required, MaxLength(200)]
    [Column("fil_createname")]
    public string FilCreateName { get; set; } = "";

    /// <summary>建立日期</summary>
    [Column("fil_createtime")]
    public DateTime FilCreateTime { get; set; } = DateTime.Now;

    /// <summary>異動者</summary>
    [Required, MaxLength(200)]
    [Column("fil_modifyname")]
    public string FilModifyName { get; set; } = "";

    /// <summary>異動日期</summary>
    [Column("fil_modifytime")]
    public DateTime FilModifyTime { get; set; } = DateTime.Now;
}
