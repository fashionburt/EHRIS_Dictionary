using EHRIS.Core.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHRIS.Core.Models.SysBasic
{
    public class SYS202002ParentViewModel
    {
        public int SfuNo { get; set; }
        public string SysName { get; set; }
        public string SfuName { get; set; }
        public string SfuStatus { get; set; }
    }

    public class SYS202002ChildViewModel
    {
        public int SfuNo { get; set; }
        public string SfuName { get; set; }
        public string SfuStatus { get; set; }
        public int SfuParent { get; set; }
    }

    public class SYS202002TreeViewModel
    {
        [JsonPropertyName("id")]
        public int SfuNo { get; set; }

        [JsonPropertyName("pid")]
        public int? SfuParent { get; set; }

        public string SysName { get; set; }
        public string SfuName { get; set; }
        public byte SfuStatus { get; set; }

        public byte SfuIns { get; set; }
        public byte SfuEdi { get; set; }
        public byte SfuDel { get; set; }

        public string EditAction { get; set; }
        public string DeleteAction { get; set; }
    }

    public class SYS202002CreateViewModel
    {
        [Display(Name = "系統編號")]
        [Range(0, int.MaxValue, ErrorMessage = "系統編號不可為負數。")]
        public int SfuNo { get; set; }

        [Display(Name = "主系統分類")]
        [Required(ErrorMessage = "主系統分類為必填項。")]
        public int? SysNo { get; set; }

        [Display(Name = "上層功能分類")]
        public int SfuParent { get; set; }

        [Display(Name = "系統名稱")]
        [Required(ErrorMessage = "系統名稱為必填項。")]
        [StringLength(100, ErrorMessage = "系統名稱長度不可超過 100 個字元。")]
        public string SfuName { get; set; }

        [Display(Name = "系統目標")]
        [StringLength(100, ErrorMessage = "系統目標長度不可超過 100 個字元。")]
        public string SfuCatalog { get; set; }

        [Display(Name = "系統狀態")]
        [Required(ErrorMessage = "系統狀態為必填項。")]
        public byte SfuStatus { get; set; }

        [Display(Name = "排列順序")]
        [Range(0, int.MaxValue, ErrorMessage = "排列順序不可為負數。")]
        public int SfuOrder { get; set; }

        [Display(Name = "預設頁面")]
        [StringLength(200, ErrorMessage = "預設頁面長度不可超過 200 個字元。")]
        public string SfuPath { get; set; }

        [Display(Name = "允許新增")]
        public byte SfuIns { get; set; }

        [Display(Name = "允許編輯")]
        public byte SfuEdi { get; set; }

        [Display(Name = "允許刪除")]
        public byte SfuDel { get; set; }

        public string CreateName { get; set; }

        public DateTime? CreateTime { get; set; }

        public string ModifyName { get; set; }

        public DateTime? ModifyTime { get; set; }
        public string SfuVersion { get; set; }
    }


    public class DropdownViewModel
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
    public class SYS202002ListViewModel
    {
        public int SfuNo { get; set; }
        public string SysName { get; set; }
        public string SfuName { get; set; }
        public byte SfuIns { get; set; }
        public byte SfuEdi { get; set; }
        public byte SfuDel { get; set; }
        public string SfuStatus { get; set; }
        public string EditAction { get; set; } = string.Empty;
        public string DeleteAction { get; set; } = string.Empty;

    }

    public class BootstrapTableRequest
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
        public string Sort { get; set; }
        public string Order { get; set; }
        public ExtraSearch ExtraSearch { get; set; }     }

    public class DataTablesRequest202002ListViewModel
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public DtSearch? search { get; set; }
        public List<DtColumn>? columns { get; set; }
        public List<DtOrder>? orderby { get; set; }         public ExtraSearch? extraSearch { get; set; }
    }

    public class DtSearch
    {
        public string? value { get; set; }
        public bool regex { get; set; }
    }

    public class DtColumn
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
    }

    public class DtOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

}