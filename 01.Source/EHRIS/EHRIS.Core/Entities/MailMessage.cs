using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities;
[Table("mailmessage")]

public class MailMessage
{
    /// <summary>
    /// mai_no
    /// </summary>
    [Key]
    [Column("mai_no")]
    public int MaiNo { get; set; }

    /// <summary>
    /// 建立人
    /// </summary>
    [Column("peo_uid")]
    public  int PeoUid { get; set; }

    /// <summary>
    /// 寄信種類
    /// </summary>
    [Column("mai_type")]
    public  int MaiType { get; set; }

    /// <summary>
    /// 信件主旨
    /// </summary>
    [Required]
    [Column("mai_subject")]
    public  string MaiSubject { get; set; }

    /// <summary>
    /// 信件內容
    /// </summary>
    [Required]
    [Column("mai_content")]
    public string MaiContent { get; set; } = "";

    /// <summary>
    /// 收件人(可分號多人)
    /// </summary>
    [Required]
    [Column("mai_email")]
    public string MaiEmail { get; set; }

    /// <summary>
    /// 密件副本(可分號多人)
    /// </summary>
    [Required]
    [Column("mai_bcc")]
    public string MaiBCC { get; set; } = "";

    /// <summary>
    /// 副本(可分號多人)
    /// </summary>
    [Required]
    [Column("mai_cc")]
    public string MaiCC { get; set; }= "";

    /// <summary>
    /// 寄信成功時間
    /// </summary>
    [Required]
    [Column("mai_sendtime")]
    public DateTime? MaiSendTime { get; set; }

    /// <summary>
    /// 寄信狀態
    /// (0 未寄信 1寄信成功 2寄信失敗)
    /// </summary>
    [Required]
    [Column("mai_status")]
    [MaxLength(1)]
    public string MaiStatus { get; set; } = "0";

    /// <summary>
    /// 錯誤次數
    /// </summary>
    [Column("mai_errortimes")]
    public int MaiErrorTimes { get; set; } = 0;

    /// <summary>
    /// 建立人
    /// </summary>
    [Required]
    [Column("mai_createname")]
    public string MaiCreateName { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    [Required]
    [Column("mai_createtime")]
    public DateTime MaiCreateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [Required]
    [Column("mai_modifyname")]
    public string MaiModifyName { get; set; }

    /// <summary>
    /// 修改時間
    /// </summary>
    [Required]
    [Column("mai_modifytime")]
    public DateTime MaiModifyTime { get; set; } 

    //[AtLeastOneRequired("mai_email", "mai_bcc", "mai_cc", ErrorMessage = "請至少填寫一種收件人")]
    //public string EmailCheck { get; set; } // Dummy 屬性

}
