namespace EHRIS.Core.Models.SysBasic
{
    public class SYS202004ListViewModel
    {
        public int ProNo { get; set; }
        public string ProCode { get; set; }
        public string ProName { get; set; }
        public string ProEnglish { get; set; }
        public string ProIsManager { get; set; }
        public int ProOrder { get; set; }
        public string ProModifyName { get; set; }
        public DateTime ProModifyTime { get; set; }
        public string PersonTypes { get; set; }
    }
    public class SYS202004UpdateViewModel
    {
        public int ProNo { get; set; }
        public string ProCode { get; set; }
        public string ProName { get; set; }
        public string ProEnglish { get; set; }
        
        public string ProIsManager { get; set; }
        public int ProOrder { get; set; }
        public List<string> SelectedPersonTypes { get; set; }
    }
    public class SYS202004EditViewModel
    {
        public int ProNo { get; set; }
        public string ProCode { get; set; }
        public string ProName { get; set; }
        public string ProEnglish { get; set; }
        public string ProIsManager { get; set; }
        public int ProOrder { get; set; }
        public List<PersonTypeOption> AllPersonTypes { get; set; }
        public List<string> SelectedPersonTypes { get; set; }
    }
}
