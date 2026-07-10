using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHRIS.Core.Models.AdminPortal;

public class AdminUserListViewModel
{
    public int AduNo { get; set; }
    public string AduLogin { get; set; } = "";
    public string AduDisplayName { get; set; } = "";
    public string RoleNames { get; set; } = "";
    public string AdlName { get; set; } = "";
    public byte AduStatus { get; set; }
    public string StatusText => AduStatus == 1 ? "啟用" : "停用";
    public string ModifyName { get; set; } = "";
    public string ModifyTimeText { get; set; } = "";
    public string AduEmail { get; set; } = "";
    public DateTime? AduModifyTime { get; set; } = null;
}

public class AdminUserSaveViewModel
{
    public int AduNo { get; set; }
    [Required] public string AduLogin { get; set; } = "";
    [Required] public string AduDisplayName { get; set; } = "";
    [Required, EmailAddress] public string AduEmail { get; set; } = "";
    public string AduPassword { get; set; } = "";
    [Required] public int AdlNo { get; set; }
    public byte AduStatus { get; set; }
    public List<int> RoleIds { get; set; } = new();
}

public class AdminUserRequestViewModel : AdminRoleRequestViewModel { }

public class AdminPasswordHistoryDto
{
    public string Hash { get; set; } = "";
    public string Salt { get; set; } = "";
}

public class SystemLevelDataDto
{
    [JsonPropertyName("No")]
    public int AdlNo { get; set; }

    [JsonPropertyName("Name")]
    public string AdlName { get; set; } = "";

    [JsonPropertyName("Rank")]
    public int AdlRank { get; set; }

    [JsonPropertyName("Status")]
    public int Status { get; set; }
}

public class UserLevelMappingDto
{
    public int AduNo { get; set; }
    public int AdlNo { get; set; }
}