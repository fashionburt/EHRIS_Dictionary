using EHRIS.Core.Models.Common;
using System.Text.Json.Serialization;

namespace EHRIS.Core.Models.Dictionary;

public class DIC1998ViewModel
{
    [JsonPropertyName("accessId")]
    public int AccessId { get; set; }

    [JsonPropertyName("clientIp")]
    public string ClientIp { get; set; }

    [JsonPropertyName("menuId")]
    public int MenuId { get; set; }

    [JsonPropertyName("menuName")]
    public string MenuName { get; set; }

    [JsonPropertyName("serverIp")]
    public string ServerIp { get; set; }

    [JsonPropertyName("isEnabled")]
    public int IsEnabled { get; set; }

    [JsonPropertyName("isEnabledText")]
    public string IsEnabledText => IsEnabled switch
    {
        1 => "啟用",
        2 => "已刪除",
        _ => "未知"
    };

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }
}

public class DIC1998SaveViewModel
{
    public int AccessId { get; set; }
    public string ClientIp { get; set; }
    public int MenuId { get; set; }
}

public class DIC1998GroupViewModel
{
    [JsonPropertyName("clientIp")]
    public string ClientIp { get; set; }

    [JsonPropertyName("serverIp")]
    public string ServerIp { get; set; }

    [JsonPropertyName("menus")]
    public List<AuthorizedMenuDto> Menus { get; set; }

    [JsonPropertyName("createDate")]
    public string CreateDate { get; set; }
}

public class AuthorizedMenuDto
{
    [JsonPropertyName("accessId")]
    public int AccessId { get; set; }

    [JsonPropertyName("menuName")]
    public string MenuName { get; set; }
}

public class DIC1998SearchModel : DataTableRequest
{
    public string QueryClientIp { get; set; }
    public string QueryServerIp { get; set; }
    public int? QueryMenuId { get; set; }
}