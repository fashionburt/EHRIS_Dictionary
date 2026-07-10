using EHRIS.Core.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace EHRIS.Core.Models.SysBasic
{
    public class SYS202003ListViewModel
    {
        public int PtyNo { get; set; }
        public string PtyCode { get; set; }
        public string PtyName { get; set; }
        public string PersonTypes { get; set; }
        public int PtyOrder { get; set; }
        public string PtyModifyName { get; set; }
        public System.DateTime PtyModifyTime { get; set; }
    }

    public class SYS202003EditViewModel
    {
        public int PtyNo { get; set; }
        public string PtyCode { get; set; }
        public string PtyName { get; set; }
        public int PtyOrder { get; set; }
        public List<PersonTypeOption> AllPersonTypes { get; set; } = new List<PersonTypeOption>();
        public List<string> SelectedPersonTypes { get; set; } = new List<string>();
    }

    public class PersonTypeOption
    {
        public string SvrCode { get; set; }
        public string SvrName { get; set; }
    }

    public class PTypeUpdateViewModel
    {
        public int PtyNo { get; set; }

        [Required]
        [MaxLength(4)]
        public string PtyCode { get; set; }

        [Required]
        [MaxLength(80)]
        public string PtyName { get; set; }

        [Required]
        public int? PtyOrder { get; set; }

        public List<string> SelectedPersonTypes { get; set; } = new List<string>();
    }

    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search? Search { get; set; }
        public List<SYS202003Column>? Columns { get; set; }
        public List<SYS202003Order>? orderby { get; set; }
        public ExtraSearch extraSearch { get; set; }
    }

    public class Search
    {
        public string? Value { get; set; }
        public bool Regex { get; set; }
    }

    public class SYS202003Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
    }

    public class SYS202003Order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class DataTablesResponse<T>
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }
    }
}