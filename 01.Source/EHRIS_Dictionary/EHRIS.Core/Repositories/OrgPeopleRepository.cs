using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EHRIS.Core.DbContext;
using EHRIS.Core.Models.Common;
using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;

namespace EHRIS.Core.Repositories;

/// <summary>
/// 人員選擇器資料存取。人員狀態依 people.peo_jobtype（1=在職、2=離職），由 OrgPeopleFilter 控制。
/// 註：權責(supervise / NodeType / AuthType)過濾尚未套用，prototype 先回全機關。
/// </summary>
public class OrgPeopleRepository : BaseRepository, IOrgPeopleRepository
{
    public OrgPeopleRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>組人員共用 WHERE 片段（狀態 / 類別 / 排除自己），並把參數加入 sqlObj。</summary>
    private static string PeopleCond(SqlQueryObject sql, OrgPeopleFilter f, string alias)
    {
        var w = new StringBuilder();
        switch (f.Status)
        {
            case PeopleStatus.OnJob: w.Append($" AND {alias}.peo_jobtype='1' "); break;
            case PeopleStatus.StopJob: w.Append($" AND {alias}.peo_jobtype='2' "); break;
            default: w.Append($" AND {alias}.peo_jobtype IN ('1','2') "); break;   // All
        }
        if (f.PtyNo.HasValue)
        {
            w.Append($" AND {alias}.pty_no = @ptyNo ");
            sql.AddParameter(new SqlParameter("@ptyNo", f.PtyNo.Value));
        }
        if (!f.ShowSelf && f.SelfUid > 0)
        {
            w.Append($" AND {alias}.peo_uid <> @selfUid ");
            sql.AddParameter(new SqlParameter("@selfUid", f.SelfUid));
        }
        return w.ToString();
    }

    /// <summary>依 filter.AllowedDeptNos 加「dep_no IN (...)」限制；null = 全部、空 = 無資料。</summary>
    private static string AllowedDeptCond(SqlQueryObject sql, OrgPeopleFilter f, string alias)
    {
        if (f.AllowedDeptNos == null) return "";              // NodeType=All/Automa → 不限制
        if (f.AllowedDeptNos.Count == 0) return " AND 1=0 ";  // 有限制但空清單 → 無資料
        var ps = new List<string>();
        for (int i = 0; i < f.AllowedDeptNos.Count; i++)
        {
            ps.Add("@a" + i);
            sql.AddParameter(new SqlParameter("@a" + i, f.AllowedDeptNos[i]));
        }
        return $" AND {alias}.dep_no IN ({string.Join(",", ps)}) ";
    }

    public async Task<List<OrgDeptCountNode>> GetDeptTreeWithCountAsync(OrgPeopleFilter filter)
    {
        var sqlObj = new SqlQueryObject();
        var cond = PeopleCond(sqlObj, filter, "p");
        var allowed = AllowedDeptCond(sqlObj, filter, "d");

        sqlObj.Sql = $@"
            SELECT d.dep_no AS DepNo, ISNULL(d.dep_parentid,0) AS ParentId,
                   d.dep_name AS Name, ISNULL(d.dep_order,0) AS SortOrder,
                   (SELECT COUNT(*) FROM people p
                      WHERE p.dep_no = d.dep_no {cond}) AS PeopleCount
            FROM departments d
            WHERE d.dep_status='1' {allowed}
            ORDER BY d.dep_order";

        return await SQLQueryAsync(sqlObj, r => new OrgDeptCountNode
        {
            DepNo = Convert.ToInt32(r["DepNo"]),
            ParentId = r["ParentId"] == DBNull.Value ? 0 : Convert.ToInt32(r["ParentId"]),
            Name = r["Name"]?.ToString() ?? "",
            SortOrder = r["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(r["SortOrder"]),
            PeopleCount = Convert.ToInt32(r["PeopleCount"])
        });
    }

    public async Task<(List<OrgPersonRow> rows, int total)> GetPeoplePageAsync(
        int depNo, bool global, string? q, OrgPeopleFilter filter,
        int start, int length, string orderField, string orderDir)
    {
        var countObj = new SqlQueryObject();
        var dataObj = new SqlQueryObject();

        var whereC = new StringBuilder(" WHERE 1=1 ");
        var whereD = new StringBuilder(" WHERE 1=1 ");
        whereC.Append(PeopleCond(countObj, filter, "p")); whereC.Append(AllowedDeptCond(countObj, filter, "p"));
        whereD.Append(PeopleCond(dataObj, filter, "p")); whereD.Append(AllowedDeptCond(dataObj, filter, "p"));

        if (!global && depNo > 0)
        {
            whereC.Append(" AND p.dep_no = @depNo "); countObj.AddParameter(new SqlParameter("@depNo", depNo));
            whereD.Append(" AND p.dep_no = @depNo "); dataObj.AddParameter(new SqlParameter("@depNo", depNo));
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            whereC.Append(" AND b.bas_name LIKE @q "); countObj.AddParameter(new SqlParameter("@q", "%" + q.Trim() + "%"));
            whereD.Append(" AND b.bas_name LIKE @q "); dataObj.AddParameter(new SqlParameter("@q", "%" + q.Trim() + "%"));
        }

        const string fromCore = @"
            FROM people p
            INNER JOIN baseperson b ON b.bas_id = p.bas_id ";

        countObj.Sql = "SELECT COUNT(*) " + fromCore + whereC;
        int total = await QuerySingleAsync(countObj, row => row[0] == DBNull.Value ? 0 : Convert.ToInt32(row[0]));

        var orderCol = orderField switch
        {
            "pro_name" => "pf.pro_name",
            "pty_name" => "pt.pty_name",
            "dep_name" => "d.dep_name",
            _ => "b.bas_name"
        };
        var dir = string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        dataObj.AddParameter(new SqlParameter("@start", start < 0 ? 0 : start));
        dataObj.AddParameter(new SqlParameter("@length", length <= 0 ? 10 : length));
        dataObj.Sql = $@"
            SELECT p.peo_uid AS PeoUid, b.bas_name AS Name,
                   ISNULL(pf.pro_name,'') AS ProName, ISNULL(pt.pty_name,'') AS PtyName,
                   p.dep_no AS DepNo, ISNULL(d.dep_name,'') AS DepName
            {fromCore}
            LEFT JOIN profess pf ON pf.pro_no = p.pro_no AND pf.pro_status='1'
            LEFT JOIN ptype pt ON pt.pty_no = p.pty_no
            LEFT JOIN departments d ON d.dep_no = p.dep_no
            {whereD}
            ORDER BY {orderCol} {dir}, p.peo_uid
            OFFSET @start ROWS FETCH NEXT @length ROWS ONLY";

        var rows = await SQLQueryAsync(dataObj, r => new OrgPersonRow
        {
            PeoUid = Convert.ToInt32(r["PeoUid"]),
            Name = r["Name"]?.ToString() ?? "",
            ProName = r["ProName"]?.ToString() ?? "",
            PtyName = r["PtyName"]?.ToString() ?? "",
            DepNo = Convert.ToInt32(r["DepNo"]),
            DepName = r["DepName"]?.ToString() ?? ""
        });

        return (rows, total);
    }

    public async Task<List<int>> GetDeptPeopleUidsAsync(IEnumerable<int> depNos, OrgPeopleFilter filter)
    {
        var list = depNos?.Distinct().ToList() ?? new List<int>();
        if (list.Count == 0) return new List<int>();

        var sqlObj = new SqlQueryObject();
        var inParams = new List<string>();
        for (int i = 0; i < list.Count; i++)
        {
            inParams.Add("@d" + i);
            sqlObj.AddParameter(new SqlParameter("@d" + i, list[i]));
        }
        var cond = PeopleCond(sqlObj, filter, "p");
        var allowed = AllowedDeptCond(sqlObj, filter, "p");

        sqlObj.Sql = $@"
            SELECT p.peo_uid AS PeoUid
            FROM people p
            WHERE p.dep_no IN ({string.Join(",", inParams)}) {cond} {allowed}";

        return await SQLQueryAsync(sqlObj, r => Convert.ToInt32(r["PeoUid"]));
    }

    /// <summary>平行部門：與指定部門同一父層（同 dep_parentid）的所有啟用部門 dep_no（含自己）。</summary>
    public async Task<List<int>> GetSiblingDeptNosAsync(int selfDepNo)
    {
        if (selfDepNo <= 0) return new List<int>();
        var sqlObj = new SqlQueryObject();
        sqlObj.AddParameter(new SqlParameter("@self", selfDepNo));
        sqlObj.Sql = @"
            SELECT dep_no FROM departments
            WHERE dep_status='1'
              AND dep_parentid = (SELECT dep_parentid FROM departments WHERE dep_no=@self)";
        return await SQLQueryAsync(sqlObj, r => Convert.ToInt32(r["dep_no"]));
    }

    /// <summary>
    /// 查權限表（supervise / department_category）→ 登入者授權的部門 dep_no。
    /// TODO(權責)：sup_type=1 → 全部(回 null)；sup_type=2 → 依 department_category.sup_no 對應部門。
    /// 目前先回 null（不限制），待資料源方向定案後實作。
    /// </summary>
    /// <summary>取登入者的 supervise.sup_type（"1"最高 / "2"依授權 / null 無記錄）。Automa 自動判斷用。</summary>
    public async Task<string?> GetSupTypeAsync(int accNo)
    {
        var sqlObj = new SqlQueryObject();
        sqlObj.AddParameter(new SqlParameter("@accNo", accNo));
        sqlObj.Sql = "SELECT TOP 1 sup_type FROM supervise WHERE acc_no = @accNo";
        var list = await SQLQueryAsync(sqlObj, r => r["sup_type"]?.ToString());
        return list.Count > 0 ? list[0]?.Trim() : null;
    }

    public Task<List<int>?> GetAuthDeptNosAsync(int accNo)
    {
        // 預留位置：實作範例
        //   var supType = (select sup_type from supervise where acc_no=@accNo);
        //   if (supType == "1") return null;                       // 最高權限 → 全部
        //   else return (select dc.dep_no from department_category dc
        //                join supervise sp on sp.sup_no=dc.sup_no and sp.acc_no=@accNo and sp.sup_type='2');
        return Task.FromResult<List<int>?>(null);
    }

    public async Task<List<OrgPersonRow>> GetPeopleByUidsAsync(IEnumerable<int> uids)
    {
        var list = uids?.Distinct().ToList() ?? new List<int>();
        if (list.Count == 0) return new List<OrgPersonRow>();

        var sqlObj = new SqlQueryObject();
        var inParams = new List<string>();
        for (int i = 0; i < list.Count; i++)
        {
            inParams.Add("@u" + i);
            sqlObj.AddParameter(new SqlParameter("@u" + i, list[i]));
        }

        // 回顯：不套狀態/自己過濾，離職者也回名稱
        sqlObj.Sql = $@"
            SELECT p.peo_uid AS PeoUid, b.bas_name AS Name,
                   ISNULL(pf.pro_name,'') AS ProName, ISNULL(pt.pty_name,'') AS PtyName,
                   p.dep_no AS DepNo, ISNULL(d.dep_name,'') AS DepName
            FROM people p
            INNER JOIN baseperson b ON b.bas_id = p.bas_id
            LEFT JOIN profess pf ON pf.pro_no = p.pro_no AND pf.pro_status='1'
            LEFT JOIN ptype pt ON pt.pty_no = p.pty_no
            LEFT JOIN departments d ON d.dep_no = p.dep_no
            WHERE p.peo_uid IN ({string.Join(",", inParams)})";

        return await SQLQueryAsync(sqlObj, r => new OrgPersonRow
        {
            PeoUid = Convert.ToInt32(r["PeoUid"]),
            Name = r["Name"]?.ToString() ?? "",
            ProName = r["ProName"]?.ToString() ?? "",
            PtyName = r["PtyName"]?.ToString() ?? "",
            DepNo = Convert.ToInt32(r["DepNo"]),
            DepName = r["DepName"]?.ToString() ?? ""
        });
    }
}
