using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common;

public  class FunctionTreeModel
{
    public int functionId { get; set; }
    public int ParentId { get; set; }  // 父節點
    public string functionName { get; set; } = string.Empty;
    public bool rauCheck { get; set; }
    /// <summary>
    /// 新增權限
    /// </summary>
    public byte AddStatus { get; set; }
    /// <summary>
    /// 編輯權限
    /// </summary>
    public byte EditStatus { get; set; }
    /// <summary>
    /// 刪除權限
    /// </summary>
    public byte DelStatus { get; set; }
    /// <summary>
    /// 是否有新增功能
    /// for 功能設定勾選使用(非權限)
    /// </summary>
    public bool SfuIns { get; set; }
    /// <summary>
    /// 是否有編輯功能
    /// for 功能設定勾選使用(非權限)
    /// </summary>
   
    public bool SfuEdi { get; set; }
    /// <summary>
    /// 是否有刪除功能
    /// for 功能設定勾選使用(非權限)
    /// </summary>
  
    public bool SfuDel { get; set; }
    public List<FunctionTreeModel> Children { get; set; } = new();
}
