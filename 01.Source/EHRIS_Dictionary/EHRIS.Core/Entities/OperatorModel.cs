using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EHRIS.Core.Entities;



/// <summary>
/// 異動資料行為
/// </summary>
public enum EventMode
{
    [DescriptionAttribute("AddEvent")]
    ADD = 1,

    [DescriptionAttribute("ModEvent")]
    EDIT = 2,

    [DescriptionAttribute("DelEvent")]
    DEL = 3,
}
public enum WhoExec
{
    [DescriptionAttribute("Personal")]
    Personal = 10,

    [DescriptionAttribute("Schedule")]
    Schedule = 20,
}

//[Keyless]
//public class PersonDepartmentDto
//{
//    public string bas_name { get; set; } = "";
//    public string Dep_Name { get; set; } = "";
//    public string Pro_Name { get; set; } = "";
//}

//[Keyless]
//public class EventOperatorDto
//{
//    /// <summary>
//    /// 修改人
//    /// </summary>
//    public int ExecUid = 0;
//    public int ExecSfuNo = 0;
//    public string _ExecProcName = "";
//    public string ExecProcName = "TextProc";
//    public EventMode OperMode = 0;
//    public WhoExec Who = 0;
//    public string FromIP = "";
//    /// <summary>
//    /// 被修改的人
//    /// 改人員資料時, 此欄位填被修改的人員peo_uid
//    /// </summary>
//    public int PevUid = 0;
//    public string PevDepName = "";
//    public string PevPeoName = "";
//    public string PevProName = "";
//}
//[Keyless]
//public class EventObjectDto
//{
//    public int PeoUID { get; set; }
//    public string TableName { get; set; } = "";
//    public string TablePK { get; set; } = "";

//    public string DepName { get; set; } = "";
//    public string PeoName { get; set; } = "";
//    public string ProName { get; set; } = "";

//    public List<ChangeColumn> content { get; } = new List<ChangeColumn>();
//}

[Serializable]
public class ChangeColumn
{
    public string ColumnDesc { get; set; } = "";
    public string Column { get; set; } = "";
    public string OriValue { get; set; } = "";
    public string NewValue { get; set; } = "";
}
[Table("personalevents")]
public class PersonalEvent
{
    [Key]
    public int pev_no { get; set; }
    public int pev_execuid { get; set; }
    public DateTime pev_execdate { get; set; }
    public DateTime pev_exectime { get; set; }
    public int pev_execsfuno { get; set; }
    public string pev_execprocname { get; set; } = "";
    public int pev_exectype { get; set; }  
    public string pev_execipaddress { get; set; } = "";
    public int pev_uid { get; set; }
    public string pev_depname { get; set; } = "";
    public string pev_peoname { get; set; } = "";
    public string pev_proname { get; set; } = "";
    public string pev_eventtype { get; set; } = "";
    public string pev_table { get; set; } = "";
    public string pev_pk { get; set; } = "";
    public string pev_content { get; set; } = "";

    public List<PersonalEventsDetail> Details { get; set; } = new();
}
[Table("personaleventsdetail")]
public class PersonalEventsDetail
{
    [Key]
    public int pvt_no { get; set; }
    public DateTime pvt_execdate { get; set;  }
    public string pvt_coldesc { get; set; } = "";
    public string pvt_column { get; set; } = "";
    public string pvt_orivalue { get; set; } = "";
    public string pvt_newvalue { get; set; } = "";

    public int pev_no { get; set; }
    public PersonalEvent Personalevent { get; set; }=new();
}
[Table("operates")]
public class Operates
{
    public long ope_no { get; set; } // bigint identity
    public DateTime ope_date { get; set; } = DateTime.Today;
    public int ope_peouid { get; set; }
    public string ope_depname { get; set; } = string.Empty;
    public string ope_proname { get; set; } = string.Empty;
    public string ope_peoname { get; set; } = string.Empty;
    public int ope_sfuno { get; set; }
    public string ope_sfuname { get; set; } = string.Empty;
    public DateTime ope_logintime { get; set; } = DateTime.Now;
    public int ope_function { get; set; }
    public string ope_method { get; set; } = string.Empty;
    public string ope_memo { get; set; } = string.Empty;
    public string ope_ipaddresss { get; set; } = string.Empty;
}
