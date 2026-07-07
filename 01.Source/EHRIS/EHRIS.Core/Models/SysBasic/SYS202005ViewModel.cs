using EHRIS.Core.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Models.SysBasic;


public class Sys202005ListViewModel
{
    public int PleNo { get; set; }
    public string PleCode { get; set; } = "";
    public string PleName { get; set; } = "";
    public string PleModifyName { get; set; } = "";
    public DateTime PleModifyTime { get; set; }

    [NotMapped] // 告知 EF Core 此屬性非資料庫欄位，請勿對應
    public string PleModifyTimeDisplay { get; set; } = "";
}

public class Sys202005EditViewModel
{
    public int PleNo { get; set; }

    [Required(ErrorMessage = "代碼不可為空")]
    [MaxLength(4)]
    public string PleCode { get; set; } = "";

    [Required(ErrorMessage = "職等不可為空")]
    [MaxLength(100)]
    public string PleName { get; set; } = "";
}


public class Sys202005DataTableRequest
{
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }
    public List<DataTableRequestColumn> columns { get; set; }
    public List<DataTableRequestOrder> orderby { get; set; }
    public ExtraSearch extraSearch { get; set; }

}

public class DataTableRequestColumn
{
    public string data { get; set; }
    public string name { get; set; }
    public bool searchable { get; set; }
    public bool orderable { get; set; }
}

public class DataTableRequestOrder
{
    public int column { get; set; }
    public string dir { get; set; }
}