using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.SysBasic;


public class RoleListViewModel
{
    public int rol_no { get; set; }
    public string rol_name { get; set; } = "";
    public string rol_memo { get; set; } = "";
    public byte rol_open { get; set; }
    public string rol_modifyname { get; set; } = "";
    public DateTime rol_modifytime { get; set; }
}

public class AuthorityViewModel
{
    /// <summary>
    /// 角色編號
    /// </summary>
    public int RolNo { get; set; }



    /// <summary>
    /// 權限項目清單
    /// </summary>
    //[BindNever]
    public List<RolePermissionViewModel> RolePermissions { get; set; } = new();
}
public class RolePermissionViewModel
{
   

   
    public int RolNo { get; set; }

    public string FuncType { get; set; }
    public int SfuNo { get; set; }
    public bool isChecked { get; set; }
    public int ParentSfuNo { get; set; }

    public bool RauIns { get; set; }

    
    public bool RauEdi { get; set; }

    
    public bool RauDel { get; set; }
    public string ModifyName {  get; set; }
    public DateTime ModifyTime { get; set; }
}

public class PeopleRequestViewModel
{
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }
    public string search { get; set; }
    public int rolNo { get; set; }
    public List<DataTableRequestColumn> columns { get; set; }
    public List<DataTableRequestOrder> orderby { get; set; }
    public ExtraSearch extraSearch { get; set; }

}
