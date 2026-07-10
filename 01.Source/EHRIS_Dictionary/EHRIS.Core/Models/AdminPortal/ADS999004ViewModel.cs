using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.SysBasic;

namespace EHRIS.Core.Models.AdminPortal;

public class Ads999004DataTableRequest
{
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }
    public List<DataTableColumn> columns { get; set; }
    public List<DataTableOrder> orderby { get; set; }
    public ExtraSearch extraSearch { get; set; }
}

public class Ads999004ListViewModel
{
    public string ArgVariable { get; set; }
    public string ArgDescribe { get; set; }
    public string ArgValue { get; set; }
    public string ArgDefaultValue { get; set; }
    public string ArgOpenManagerDisplay { get; set; }
    public string ArgModifyName { get; set; }
    public DateTime ArgModifyTime { get; set; }
}

public class Ads999004EditViewModel
{
    public string ArgVariable { get; set; }
    public string ArgDescribe { get; set; }
    public string ArgDeatil { get; set; }
    public string ArgValue { get; set; }
    public string ArgDefaultValue { get; set; }
    public string ArgSource { get; set; }
    public byte ArgMultiSel { get; set; }
    public string ArgSplitChar { get; set; }
    public string AgrGroup { get; set; }
    public int ArgOrder { get; set; }
    public byte ArgOpenManager { get; set; }
    public bool ArgRequired { get; set; }
}
