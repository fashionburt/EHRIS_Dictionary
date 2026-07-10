using System.ComponentModel.DataAnnotations;

namespace EHRIS.Core.Models;

/// <summary>
/// 信件佇列
/// </summary>
public class MailQueSendViewModel
{ 
    /// <summary>
    /// 信件編號
    /// </summary>
    public int mai_no { get; set; }

    /// <summary>
    /// 收件人(以,分開)
    /// </summary>
    public string mai_email { get; set; }
    /// <summary>
    /// 密件副本(以,分開)
    /// </summary>
    public string mai_bcc { get; set; } = "";
    /// <summary>
    /// 副本(以,分開)
    /// </summary>
    public string mai_cc { get; set; } = "";

    /// <summary>
    /// 信件主旨
    /// </summary>
    public string mai_subject { get; set; }
     
    /// <summary>
    /// 信件內容(可HTML格式)
    /// </summary> 
    public string mai_content { get; set; } = "";

    /// <summary>
    /// 錯誤次數
    /// </summary>
    public int mai_errortimes { get; set; } 
}
