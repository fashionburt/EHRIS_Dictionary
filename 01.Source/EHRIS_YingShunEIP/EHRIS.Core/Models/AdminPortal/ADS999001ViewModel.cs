using EHRIS.Core.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace EHRIS.Core.Models.AdminPortal
{
    public class ADS999001ListViewModel
    {
        public int SysnNo { get; set; }
        public byte SysnType { get; set; }
        public string SysnTypeName { get; set; } = "";
        public string SysnContent { get; set; } = "";
        public DateTime SysnPublicDt { get; set; }
        public DateTime SysnStartTime { get; set; }
        public DateTime SysnEndTime { get; set; }
        public bool SysnTop { get; set; }
        public string SysnModifyName { get; set; } = "";
        public DateTime SysnModifyTime { get; set; }
    }

    public class ADS999001EditViewModel
    {
        public int SysnNo { get; set; }
        public byte SysnType { get; set; } = 2;
        public string SysnContent { get; set; } = "";
        public DateTime SysnPublicDt { get; set; } = DateTime.Today;
        public DateTime SysnStartTime { get; set; } = DateTime.Today;
        public DateTime SysnEndTime { get; set; } = DateTime.Today.AddDays(30);
        public bool SysnTop { get; set; } = false;
        public List<BadgeOption> AllBadges { get; set; } = new List<BadgeOption>();
    }

    public class BadgeOption
    {
        public byte TypeCode { get; set; }
        public string BadgeName { get; set; } = "";
    }

    public class SysNoticeUpdateViewModel
    {
        public int SysnNo { get; set; }

        [Required]
        public byte SysnType { get; set; }

        [Required]
        [MaxLength(2000)]
        public string SysnContent { get; set; } = "";

        [Required]
        public DateTime SysnPublicDt { get; set; }

        [Required]
        public DateTime SysnStartTime { get; set; }

        [Required]
        public DateTime SysnEndTime { get; set; }

        public bool SysnTop { get; set; } = false;
    }

    public class ADS999001QueryViewModel
    {
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public byte? SysnType { get; set; }
    }

    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search? Search { get; set; }
        public List<ADS999001Column>? Columns { get; set; }
        public List<ADS999001Order>? orderby { get; set; }
        public ExtraSearch extraSearch { get; set; }
        public ADS999001QueryViewModel? filter { get; set; }
    }

    public class Search
    {
        public string? Value { get; set; }
        public bool Regex { get; set; }
    }

    public class ADS999001Column
    {
        public string data { get; set; } = "";
        public string name { get; set; } = "";
        public bool searchable { get; set; }
        public bool orderable { get; set; }
    }

    public class ADS999001Order
    {
        public int column { get; set; }
        public string dir { get; set; } = "";
    }

    public class DataTablesResponse<T>
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; } = new();
    }
}