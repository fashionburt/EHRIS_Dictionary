using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;

namespace EHRIS.Services.Common;

public class OrgPeopleService : IOrgPeopleService
{
    private readonly IOrgPeopleRepository _repo;

    public OrgPeopleService(IOrgPeopleRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// 依 NodeType 解析「允許部門」並填入 filter.AllowedDeptNos（null = 不限制）。
    /// All / Automa（＝最大權限，不卡權限）→ null；Self → 自己部門；Parallel → 同層部門；
    /// Auth → 查權限表（預留）；Limit / AgentForMe2 → 暫不限制。
    /// </summary>
    private async Task PrepareScopeAsync(int accNo, OrgPeopleFilter f)
    {
        f.AllowedDeptNos = f.NodeType switch
        {
            NodeType.Self => new List<int> { f.SelfDepNo },
            NodeType.Parallel => await _repo.GetSiblingDeptNosAsync(f.SelfDepNo),
            NodeType.Auth => await _repo.GetAuthDeptNosAsync(accNo),
            NodeType.Automa => await ResolveAutomaAsync(accNo, f),
            _ => null   // All / Limit / AgentForMe2 → 不限制
        };
    }

    /// <summary>Automa（自動判斷）：依 supervise.sup_type → 1=全部 / 2=依授權(Auth) / 無=只自己部門。</summary>
    private async Task<List<int>?> ResolveAutomaAsync(int accNo, OrgPeopleFilter f)
    {
        var supType = await _repo.GetSupTypeAsync(accNo);
        if (supType == "1") return null;                                    // 最高權限 → 全部
        if (supType == "2") return await _repo.GetAuthDeptNosAsync(accNo);  // 依授權（Auth，目前預留=全部）
        return new List<int> { f.SelfDepNo };                              // 無 supervise → 只自己部門
    }

    public async Task<List<OrgDeptCountNode>> GetDeptTreeAsync(int accNo, OrgPeopleFilter filter)
    {
        await PrepareScopeAsync(accNo, filter);
        return await _repo.GetDeptTreeWithCountAsync(filter);
    }

    public async Task<(List<OrgPersonRow> rows, int total)> GetPeoplePageAsync(int accNo, OrgPeopleGridRequest req, OrgPeopleFilter filter)
    {
        // 效能防呆：全機關搜尋需有關鍵字、部門瀏覽需有部門；兩者皆不成立就不查（避免撈全庫人員）
        var hasDeptScope = !req.Global && req.DepNo > 0;
        var hasGlobalSearch = req.Global && !string.IsNullOrWhiteSpace(req.Q);
        if (!hasDeptScope && !hasGlobalSearch)
            return (new List<OrgPersonRow>(), 0);

        await PrepareScopeAsync(accNo, filter);
        var (field, dir) = ResolveOrder(req);
        return await _repo.GetPeoplePageAsync(
            req.DepNo, req.Global, req.Q, filter,
            req.Start, req.Length, field, dir);
    }

    public async Task<List<int>> ResolveAsync(int accNo, string? tokens, OrgPeopleFilter filter)
    {
        var (deptNos, uids, exclUids) = ParseTokens(tokens);

        var result = new HashSet<int>(uids);
        if (deptNos.Count > 0)
        {
            await PrepareScopeAsync(accNo, filter);   // 送出時也依 NodeType 限制範圍（安全）
            var deptPeople = await _repo.GetDeptPeopleUidsAsync(deptNos, filter);
            foreach (var u in deptPeople) result.Add(u);
        }
        result.ExceptWith(exclUids);
        return result.ToList();
    }

    public async Task<OrgPeoplePreload> ResolveDisplayAsync(int accNo, string? tokens, OrgPeopleFilter filter)
    {
        var preload = new OrgPeoplePreload();
        var (deptNos, uids, exclUids) = ParseTokens(tokens);

        if (deptNos.Count > 0)
        {
            var tree = await _repo.GetDeptTreeWithCountAsync(filter);
            var map = tree.ToDictionary(d => d.DepNo);
            foreach (var dn in deptNos)
                if (map.TryGetValue(dn, out var node)) preload.Depts.Add(node);
        }
        if (uids.Count > 0)
            preload.People = await _repo.GetPeopleByUidsAsync(uids);
        if (exclUids.Count > 0)
            preload.Excluded = await _repo.GetPeopleByUidsAsync(exclUids);

        return preload;
    }

    public async Task<List<OrgPersonRow>> GetDeptKeptAsync(int accNo, int depNo, OrgPeopleFilter filter, IEnumerable<int> excludeUids)
    {
        if (depNo <= 0) return new List<OrgPersonRow>();
        await PrepareScopeAsync(accNo, filter);
        var all = await _repo.GetDeptPeopleUidsAsync(new[] { depNo }, filter);
        var ex = new HashSet<int>(excludeUids ?? Enumerable.Empty<int>());
        var kept = all.Where(u => !ex.Contains(u)).ToList();
        if (kept.Count == 0) return new List<OrgPersonRow>();
        return await _repo.GetPeopleByUidsAsync(kept);
    }

    private static (List<int> deptNos, List<int> uids, List<int> exclUids) ParseTokens(string? tokens)
    {
        var deptNos = new List<int>();
        var uids = new List<int>();
        var exclUids = new List<int>();

        foreach (var raw in (tokens ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = raw.Trim();
            if (t.Length == 0) continue;

            if (t.StartsWith("d:")) { if (int.TryParse(t[2..], out var dn)) deptNos.Add(dn); }
            else if (t.StartsWith("x:")) { if (int.TryParse(t[2..], out var xu)) exclUids.Add(xu); }
            else if (int.TryParse(t, out var uid)) uids.Add(uid);
        }

        return (deptNos, uids, exclUids);
    }

    private static (string field, string dir) ResolveOrder(OrgPeopleGridRequest req)
    {
        var field = "bas_name";
        var dir = "asc";

        if (req.Orderby != null && req.Orderby.Count > 0 && req.Columns != null)
        {
            var o = req.Orderby[0];
            if (o.Column >= 0 && o.Column < req.Columns.Count)
            {
                field = (req.Columns[o.Column].Data ?? "").ToLowerInvariant() switch
                {
                    "proname" => "pro_name",
                    "ptyname" => "pty_name",
                    "depname" => "dep_name",
                    _ => "bas_name"
                };
            }
            dir = o.Dir;
        }

        return (field, dir);
    }
}
