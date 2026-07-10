using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;

using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace EHRIS.Core.Repositories;

public class MenuRepository : BaseRepository, IMenuRepository

{

    public MenuRepository(ApplicationDbContext dbContext) : base(dbContext)
    {

    }
    /// <summary>
    /// 取得功能名稱資料路徑
    /// </summary>
    /// <param name="sfuNo"></param>
    /// <returns></returns>
    public async Task<(string SysName, string SfuMainName, string SfuName)> GetFunctionNamesAsync(int sfuNo)
    {
        var sql = @"SELECT 
                  [sysfuction].sfu_name,
                  ISNULL([mainSfu].sfu_name, '') AS mainSfu_name,
                  ISNULL([sys].sys_name, '') AS sys_name
                  
                  FROM [sysfuction]
                  LEFT JOIN [sysfuction] AS mainSfu ON sysfuction.sfu_parent = mainSfu.sfu_no
                  LEFT JOIN [sys] ON [sysfuction].sys_no = [sys].sys_no
                  WHERE [sysfuction].sfu_no = @SFUNO";

        SqlQueryObject sqlObj = new SqlQueryObject();
        sqlObj.Sql = sql;
        sqlObj.AddParameter(new SqlParameter("@SFUNO", sfuNo));

        var resultList = await SQLQueryAsync<(string SysName, string SfuMainName, string SfuName)>(
                         sqlObj,
                         reader => new ValueTuple<string, string, string>(
                             reader["sys_name"].ToString(),
                             reader["mainSfu_name"].ToString(),
                             reader["sfu_name"].ToString()
                         )
                     );


        if (resultList.Any())
        {
            return resultList.First();
        }
        else
        {
            return (SysName: "", SfuMainName: "", SfuName: "");
        }

    }

    public async Task<List<MenuSysViewModel>> GetMenusByAccount(int accNo)
    {
        List<SysFuction> allFunctions;
        allFunctions = await (
                       from b in _context.SysFuction
                       join c in _context.RAuthoritys on b.SfuNo equals c.SfuNo

                       join d in _context.Roles on c.RolNo equals d.RolNo
                       where d.RolOpen == 1
                       join e in _context.RoleAccounts on d.RolNo equals e.RolNo
                       where e.AccNo == accNo && b.SfuStatus == 1 && b.SfuBuiltIn == 2
                       where b.SfuStatus == 1 && b.SfuBuiltIn == 2
                       select b
                   )
                   .Distinct().AsNoTracking()
                   .OrderBy(m => m.SfuOrder)
                   .ToListAsync();

        List<SysFuction> accountFunctios = await (
                        from b in _context.SysFuction
                        join c in _context.AAuthoritys on b.SfuNo equals c.SfuNo

                        where c.AauNo == accNo && b.SfuStatus == 1 && b.SfuBuiltIn == 2
                        select b
                    )
                    .Distinct().AsNoTracking()
                    .OrderBy(m => m.SfuOrder)
                    .ToListAsync();

        allFunctions.Union(accountFunctios).ToList();

        var menuList = allFunctions
                    .Where(f => f.SfuParent == 0)
                    .Select(f => new MenuSysViewModel
                    {
                        sys_no = f.SfuNo,
                        sys_name = f.SfuDisName,
                        sys_shorten = f.SfuShorten,
                        sys_catalog = f.SfuCatalog,
                        sys_order = f.SfuOrder,
                        sys_path = f.SfuPath,
                        sys_defaltpic = f.SfuDefaltPic,
                        sys_overpicture = f.SfuOverPicture,
                        sys_status = f.SfuStatus,
                        sys_createname = f.SfuCreateName,
                        sys_createtime = f.SfuCreateTime
                    })
                    .Distinct()
                    .OrderByDescending(m => m.sys_order)
                    .ToList();

        return menuList;
    }



    public async Task<List<MenuSysViewModel>> GetAdminMenusByAccount(int aduNo)
    {
        //依角色抓有權限的功能 (rauthority)
        List<AdminFunctions> allFunctions;
        allFunctions = await (
                       from b in _context.AdminFunctions
                       join c in _context.AdminRoleFunctions on b.AdfNo equals c.AdfNo

                       join d in _context.AdminRoles on c.AdrNo equals d.AdrNo
                       where d.AdrStatus == 1

                       join e in _context.AdminUserRoles on d.AdrNo equals e.AdrNo
                       where e.AduNo == aduNo && b.Adfstatus == 1
                       where b.Adfstatus == 1
                       select b
                   )
                   .Distinct().AsNoTracking()
                   .OrderBy(m => m.AdfOrder)
                   .ToListAsync();//讀取所有子選單



        //往回抓最上層系統(sys)
        var sysNos = allFunctions.Select(f => f.AdsNo).Distinct().ToList();
        List<MenuSysViewModel> allParents = new List<MenuSysViewModel>();

        allParents = await _context.AdminSys
                        .Where(m => m.AdsStatus == 1  && sysNos.Contains(m.AdsNo))
                        .Select(x => new MenuSysViewModel
                        {
                            sys_no = x.AdsNo,
                            sys_name = x.AdsName,

                            sys_catalog = "",
                            sys_order = x.AdsOrder,
                            sys_default = "",
                            sys_defaltpic = "",
                            sys_overpicture = "",
                            sys_status = x.AdsStatus,
                        }
                        )
                        .Distinct()
                        .AsNoTracking()
                        .OrderBy(a => a.sys_order)
                        .ToListAsync();


        var menuHierarchy = allParents
                    .Select(m => new MenuSysViewModel
                    {
                        sys_no = m.sys_no,
                        sys_name = m.sys_name,

                        sys_order = m.sys_order,
                        sys_defaltpic = m.sys_defaltpic,
                        sys_overpicture = m.sys_overpicture,
                        ChildrenMenu = allFunctions
                            .Where(f => f.AdsNo == m.sys_no && f.AdfParent == 0) // 依系統分類，暫不過濾 AdfParent（debug 用）
                            .Select(f => new SysFuction
                            {
                                SfuNo = f.AdfNo,
                                SfuName = f.AdfName,
                                SfuShorten = f.AdfName,
                                SysNo = f.AdsNo,
                                SfuPath = f.AdfController,  // 功能路徑
                                SfuOrder = f.AdfOrder,
                                SubFunctions = allFunctions
                                    .Where(sub => sub.AdfParent == f.AdfNo) // 取得第 3 層選單
                                    .Select(sub => new SysFuction
                                    {
                                        SfuNo = sub.AdfNo,
                                        SfuName = sub.AdfName,
                                        SysNo = f.AdsNo,
                                        SfuPath = sub.AdfController,  // 功能路徑
                                        SfuOrder = sub.AdfOrder
                                    }).OrderBy(o => o.SfuOrder).ToList()
                            }).OrderBy(o => o.SfuOrder).ToList()
                    }).OrderBy(o => o.sys_order).ToList();
        return menuHierarchy;


    }
}
