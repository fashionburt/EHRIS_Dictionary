using EHRIS.Core.Models.Common;
using System.Text.Json.Serialization;

namespace EHRIS.Core.Models.AdminPortal
{
    public class AdminRoleListViewModel
    {
        public int AdrNo { get; set; }
        public string AdrRoleName { get; set; } = string.Empty;
        public string AdrRoleMemo { get; set; } = string.Empty;
        public byte AdrStatus { get; set; }
        public string StatusText => AdrStatus == 1 ? "啟用" : "停用";
        public string ModifyName { get; set; } = string.Empty;
        public DateTime ModifyTime { get; set; }
    }
    public class AdminAuthorityViewModel
    {
        [JsonPropertyName("adrNo")] 
        public int AdrNo { get; set; }

        [JsonPropertyName("rolePermissions")]
        public List<AdminRolePermissionViewModel> RolePermissions { get; set; } = new();
    }
    public class AdminRolePermissionViewModel
    {
        [JsonPropertyName("adrNo")]
        public int AdrNo { get; set; }
        [JsonPropertyName("adfNo")]
        public int AdfNo { get; set; }
        [JsonPropertyName("funcType")]
        public string FuncType { get; set; } = string.Empty;
        [JsonPropertyName("parentAdfNo")]
        public int ParentAdfNo { get; set; }
        [JsonPropertyName("isChecked")]
        public bool IsChecked { get; set; }
        [JsonPropertyName("canCreate")]
        public bool CanCreate { get; set; }
        [JsonPropertyName("canEdit")]
        public bool CanEdit { get; set; }
        [JsonPropertyName("canDelete")]
        public bool CanDelete { get; set; }
        public string ModifyName { get; set; } = string.Empty;
        public DateTime ModifyTime { get; set; }
    }
    public class AdminFunctionTreeViewModel
    {
        public int FunctionId { get; set; }
        public string FunctionName { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public bool IsChecked { get; set; }
        public byte AddStatus { get; set; }
        public byte EditStatus { get; set; }
        public byte DelStatus { get; set; }
        public bool HasCreateAuth { get; set; }
        public bool HasEditAuth { get; set; }
        public bool HasDeleteAuth { get; set; }
        public List<AdminFunctionTreeViewModel> Children { get; set; } = new();
    }
    public class AdminRoleRequestViewModel
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        [JsonPropertyName("adrNo")]
        public int AdrNo { get; set; } 
        public List<DataTableRequestColumn> columns { get; set; } = new();
        public List<DataTableRequestOrder> orderby { get; set; } = new();
        public ExtraSearch extraSearch { get; set; } = new();
    }
    public class AdminUserInRoleViewModel
    {
        public int AduNo { get; set; }

        [JsonPropertyName("aduLogin")] 
        public string AduLogin { get; set; } = string.Empty;

        [JsonPropertyName("aduDisplayName")]
        public string AduDisplayName { get; set; } = string.Empty;

        [JsonPropertyName("aduStatusText")] 
        public string AduStatusText => AduStatus == 1 ? "正常" : "鎖定";

        public byte AduStatus { get; set; }
    }
}