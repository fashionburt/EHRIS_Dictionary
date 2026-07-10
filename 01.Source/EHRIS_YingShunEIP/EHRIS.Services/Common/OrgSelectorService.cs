using EHRIS.Core.Models.Common;

namespace EHRIS.Services.Common;

public class OrgSelectorService : IOrgSelectorService
{
    private readonly ICommonService _commonService;
    private const string GroupPrefix = "u:";

    public OrgSelectorService(ICommonService commonService)
    {
        _commonService = commonService;
    }

    public Task<List<OrgNode>> GetTreeAsync(OrgSelectorScope scope, int accNo) => scope switch
    {
        OrgSelectorScope.AuthorizedUnit => GetAuthorizedUnitTreeAsync(accNo),
        OrgSelectorScope.AuthorizedDept => GetAuthorizedDeptTreeAsync(accNo),
        _ => Task.FromResult(new List<OrgNode>())
    };

    public async Task<List<OrgNode>> GetNodesByIdsAsync(OrgSelectorScope scope, int accNo, IEnumerable<string> ids)
    {
        var idSet = new HashSet<string>(ids.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (idSet.Count == 0) return new List<OrgNode>();
        var all = await GetTreeAsync(scope, accNo);
        return all.Where(n => !n.IsGroup && idSet.Contains(n.Id)).ToList();
    }

    /// <summary>
    /// 權責機關
    /// 回傳單層機關清單，節點 ID 為真實 uni_id、依 uni_order 排序，機關本身即為可選葉節點。
    /// </summary>
    private async Task<List<OrgNode>> GetAuthorizedUnitTreeAsync(int accNo)
    {
        var units = await _commonService.GetAllUnitList(accNo);

        return units
            .Select(u => new OrgNode
            {
                Id = u.Id,            // uni_id
                ParentId = null,      // 單層，無父節點
                Name = u.Text,        // uni_name
                SortOrder = u.TreeOrder,
                IsGroup = false       // 機關本身即為可選葉節點
            })
            .ToList();
    }

    /// <summary>
    /// 權責單位
    /// 回傳 (Id:int dep_no, Parent:機關名, Text:部門名)，組合為「機關虛擬節點 → 部門節點」兩層樹。
    /// </summary>
    private async Task<List<OrgNode>> GetAuthorizedDeptTreeAsync(int accNo)
    {
        var raw = await _commonService.GetAllUnitDeptList(accNo);

        var result = new List<OrgNode>();
        var groupSeen = new Dictionary<string, int>();

        foreach (var row in raw)
        {
            var unitName = row.Parent ?? "";
            if (!groupSeen.ContainsKey(unitName))
                groupSeen[unitName] = groupSeen.Count;

            result.Add(new OrgNode
            {
                Id = row.Id.ToString(),
                ParentId = GroupPrefix + unitName,
                Name = row.Text,
                SortOrder = groupSeen[unitName] * 10000 + result.Count,
                IsGroup = false
            });
        }

        var groupNodes = groupSeen.Select(kv => new OrgNode
        {
            Id = GroupPrefix + kv.Key,
            ParentId = null,
            Name = kv.Key,
            SortOrder = kv.Value,
            IsGroup = true
        });

        return groupNodes.Concat(result).ToList();
    }
}
