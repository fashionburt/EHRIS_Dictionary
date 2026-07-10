namespace EHRIS.Core.Models.Common;

/// <summary>
/// 人員選擇器：部門節點（含「直屬」在職人數），前端自行建樹。
/// </summary>
public class OrgDeptCountNode
{
    /// <summary>部門代碼 dep_no。</summary>
    public int DepNo { get; set; }

    /// <summary>父部門 dep_parentid；0 = 根。</summary>
    public int ParentId { get; set; }

    /// <summary>部門名稱。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>排序 dep_order。</summary>
    public int SortOrder { get; set; }

    /// <summary>該部門「直屬」在職人數（不含子部門）。</summary>
    public int PeopleCount { get; set; }
}

/// <summary>人員選擇器：人員資料列。</summary>
public class OrgPersonRow
{
    public int PeoUid { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>職稱 pro_name。</summary>
    public string ProName { get; set; } = string.Empty;
    /// <summary>人員類別 pty_name。</summary>
    public string PtyName { get; set; } = string.Empty;
    public int DepNo { get; set; }
    public string DepName { get; set; } = string.Empty;
}

/// <summary>
/// 人員 grid 的 DataTables server-side 請求（前端 OrgPeoplePicker.js 送出）。
/// </summary>
public class OrgPeopleGridRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } = 10;

    /// <summary>目前瀏覽的部門（dept 模式）。</summary>
    public int DepNo { get; set; }

    /// <summary>人員類別過濾 pty_no（null = 全部）。</summary>
    public int? PtyNo { get; set; }

    /// <summary>搜尋字（比對姓名）。</summary>
    public string? Q { get; set; }

    /// <summary>true = 跨全機關搜尋（忽略 DepNo）。</summary>
    public bool Global { get; set; }

    /// <summary>人員狀態（1=在職、2=離職、0=全部）。</summary>
    public int PeopleStatus { get; set; } = 1;

    /// <summary>是否顯示自己（false = 排除登入者）。</summary>
    public bool ShowSelf { get; set; } = true;

    /// <summary>權限方式（NodeType 的 int 值；4=Automa）。</summary>
    public int NodeType { get; set; } = (int)Common.NodeType.Automa;

    /// <summary>排序（DataTables 送出的欄位名為 orderby，對齊 createEhrisTable）。</summary>
    public List<OrgPeopleOrder>? Orderby { get; set; }
    public List<OrgPeopleColumn>? Columns { get; set; }
}

public class OrgPeopleOrder
{
    public int Column { get; set; }
    public string Dir { get; set; } = "asc";
}

public class OrgPeopleColumn
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// 「正面表列」用：取某部門「排除指定人員後」仍保留的在職人員（含姓名）。
/// 當使用者從全選中取消過半時，前端改存這些保留者的 peo_uid 而非一堆 x:。
/// </summary>
public class OrgDeptKeptRequest
{
    public int DepNo { get; set; }
    public int? PtyNo { get; set; }
    public int PeopleStatus { get; set; } = 1;
    public bool ShowSelf { get; set; } = true;
    public int NodeType { get; set; } = (int)Common.NodeType.Automa;
    public List<int>? Exclude { get; set; }
}

/// <summary>
/// 人員查詢共用篩選條件（由 Controller 依請求 + 登入者組出，傳給 Service/Repo）。
/// </summary>
public class OrgPeopleFilter
{
    /// <summary>人員類別過濾 pty_no（null = 全部）。</summary>
    public int? PtyNo { get; set; }

    /// <summary>人員狀態（依 peo_jobtype）。</summary>
    public PeopleStatus Status { get; set; } = PeopleStatus.OnJob;

    /// <summary>是否顯示自己（false 時以 SelfUid 排除）。</summary>
    public bool ShowSelf { get; set; } = true;

    /// <summary>登入者 peo_uid（ShowSelf=false 時用來排除自己）。</summary>
    public int SelfUid { get; set; }

    /// <summary>權限方式（根範圍策略）。</summary>
    public NodeType NodeType { get; set; } = NodeType.Automa;

    /// <summary>登入者所在部門 dep_no（Self / Parallel 用）。</summary>
    public int SelfDepNo { get; set; }

    /// <summary>依 NodeType 解析出的「允許部門」清單；null = 不限制（全部）。由 Service 填入。</summary>
    public List<int>? AllowedDeptNos { get; set; }

    public static OrgPeopleFilter From(int? ptyNo, int status, bool showSelf, int selfUid,
        int nodeType = (int)NodeType.Automa, int selfDepNo = 0) => new()
    {
        PtyNo = ptyNo,
        Status = System.Enum.IsDefined(typeof(PeopleStatus), status) ? (PeopleStatus)status : PeopleStatus.OnJob,
        ShowSelf = showSelf,
        SelfUid = selfUid,
        NodeType = System.Enum.IsDefined(typeof(NodeType), nodeType) ? (NodeType)nodeType : NodeType.Automa,
        SelfDepNo = selfDepNo
    };
}

/// <summary>
/// 回顯用：把 token 字串還原成可顯示的部門 / 人員 / 排除清單。
/// </summary>
public class OrgPeoplePreload
{
    /// <summary>整批選取的部門（d: token），含名稱與直屬在職人數。</summary>
    public List<OrgDeptCountNode> Depts { get; set; } = new();

    /// <summary>個別選取的人員（peo_uid token）。</summary>
    public List<OrgPersonRow> People { get; set; } = new();

    /// <summary>部門全選下被排除的人員（x: token），需 PeoUid + DepNo 即可。</summary>
    public List<OrgPersonRow> Excluded { get; set; } = new();
}
