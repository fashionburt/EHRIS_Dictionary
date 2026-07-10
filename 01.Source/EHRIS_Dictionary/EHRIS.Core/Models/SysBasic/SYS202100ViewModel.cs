using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.SysBasic
{
    public class Sys202100DataTableRequest : DataTableRequest
    {
        public ExtraSearch extraSearch { get; set; }
    }

    public class Sys202100ListViewModel
    {
        public string ArgVariable { get; set; } = "";
        public string ArgDescribe { get; set; } = "";
        public string ArgValue { get; set; } = "";
        public string AgdValueDisplay { get; set; } = "";
    }

    public class Sys202100EditViewModel
    {
        public string ArgVariable { get; set; } = "";
        public string ArgDescribe { get; set; } = "";
    }

    public class Sys202100DeptListViewModel
    {
        public int AgdNo { get; set; }
        public string ArgVariable { get; set; } = "";
        public string ArgDescribe { get; set; } = "";
        public int DepNo { get; set; }
        public string DepName { get; set; } = "";
        public string ArgValue { get; set; } = "";
        public string AgdValue { get; set; } = "";
    }

    public class Sys202100DepOption
    {
        public int DepNo { get; set; }
        public string DepName { get; set; } = "";
    }

    public class Sys202100SchedViewModel
    {
        public int AgsNo { get; set; }
        public string ArgVariable { get; set; } = "";
        public int DepNo { get; set; }
        public string AgsValue { get; set; } = "";
        public DateTime? AgsStartTime { get; set; }
        public DateTime? AgsEndTime { get; set; }
    }


    public class SaveAllDeptRequest
    {
        public List<Sys202100DeptListViewModel> Depts { get; set; }
        public List<Sys202100SchedViewModel> Scheds { get; set; }
    }
}