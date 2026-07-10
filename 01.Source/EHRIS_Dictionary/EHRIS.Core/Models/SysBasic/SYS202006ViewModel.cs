namespace EHRIS.Core.Models.SysBasic
{
    public class SYS202006ListViewModel
    {
        public int HolNo { get; set; }
        public string HolCode { get; set; }
        public string HolName { get; set; }
        public string HolStatistics { get; set; }
        public string HolOfficial { get; set; }
        public int HolOrder { get; set; }
        public string HolModifyName { get; set; }
        public DateTime HolModifyTime { get; set; }
    }

    public class SYS202006UpdateViewModel
    {
        public int HolNo { get; set; }
        public string HolCode { get; set; }
        public string HolName { get; set; }
        public int HolOrder { get; set; }
        public string HolStatistics { get; set; }
        public string HolOfficial { get; set; }
    }

    public class SYS202006EditViewModel
    {
        public int HolNo { get; set; }
        public string HolCode { get; set; }
        public string HolName { get; set; }
        public int HolOrder { get; set; }
        public string HolStatistics { get; set; }
        public string HolOfficial { get; set; }
    }
}
