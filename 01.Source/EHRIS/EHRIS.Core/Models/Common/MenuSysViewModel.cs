using EHRIS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EHRIS.Core.Models.Common;

public class MenuSysViewModel
{
  
    public int sys_no { get; set; }
    public string sys_name { get; set; } = "";
    public string sys_shorten { get; set; } = "";
    public string? sys_catalog { get; set; }
    public int? sys_order { get; set; }
    public string? sys_default { get; set; }
    public string? sys_defaltpic { get; set; }
    public string? sys_overpicture { get; set; }
    public byte? sys_status { get; set; }
    public string? sys_createname { get; set; }
    public DateTime? sys_createtime { get; set; }
    public List<SysFuction>? ChildrenMenu { get; set; } //  子選單
}
