using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;

using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace EHRIS.Core.Repositories;

public class DeptRepository :BaseRepository, IDeptRepository

{
    public DeptRepository(ApplicationDbContext context) : base(context)
    {

    }
    /// <summary>
    /// GetAllUnitDepartment  for  統計
    /// </summary>
    /// <returns></returns>
    public async Task<List<DepartmentTree>> GetAllUnitDepartment(int AccNo)
    {
        var allUnitDeptList = new List<DepartmentTree>();
        var superviseList = await _context.Supervise.Where(x => x.AccNo == AccNo)
                                        .AsNoTracking().FirstOrDefaultAsync();
        if (superviseList != null)
        {
            var sqlDept = "";
            if (superviseList.SupType.Equals("1"))
            {
                sqlDept = @"
                         select udp.ude_depno as Id ,uni_name AS Parent, dpt.dep_name as [Text], ut.uni_order as TreeOrder
                        from unit ut
                        inner join unit_depart udp on ut.uni_id = udp.uni_id and udp.ude_status='1'
                        inner join departments dpt on dpt.uni_id = udp.uni_id 
                                            and dpt.ude_no = udp.ude_no and dpt.dep_no = udp.ude_depno 
                                            and dpt.dep_status='1'
                        where ut.uni_status=1 
                        order by ut.uni_order
                        ";
            }
            else if (superviseList.SupType.Equals("2"))
            {
                sqlDept = @$"
                         select udp.ude_depno as Id ,uni_name AS Parent, dpt.dep_name as [Text], ut.uni_order as TreeOrder
                        from unit ut
                        inner join unit_depart udp on ut.uni_id = udp.uni_id and udp.ude_status='1'
                        inner join departments dpt on dpt.uni_id = udp.uni_id 
                                            and dpt.ude_no = udp.ude_no and dpt.dep_no = udp.ude_depno 
                                            and dpt.dep_status='1'
                        inner join department_category dc on dc.dep_no = dpt.dep_no 
                        inner join supervise sp on sp.sup_no = dc.sup_no and sp.sup_type='2'
                        where ut.uni_status=1 and sp.acc_no={AccNo}
                        order by ut.uni_order
                        ";
            }
                allUnitDeptList = await _context.DepartmentTrees
              .FromSqlRaw(sqlDept)
              .AsNoTracking()
              .ToListAsync();
        }
        return allUnitDeptList;


    }
    /// <summary>
    /// GetAllUnit  for  統計機關
    /// </summary>
    /// <returns></returns>
    public async Task<List<UnitTree>> GetAllUnit(int AccNo)
    {
        var allUnitList = new List<UnitTree>();
        var superviseList = await _context.Supervise.Where(x => x.AccNo == AccNo)
                                        .AsNoTracking().FirstOrDefaultAsync();

        if (superviseList != null)
        {
            var sqlUnit = "";
            if (superviseList.SupType.Equals("1"))
            {
                sqlUnit = @"
                           select distinct ut.uni_id as Id ,'' AS Parent, uni_name as [Text],ut.uni_order as TreeOrder
                            from unit ut
                            where ut.uni_status=1 
                            order by ut.uni_order
                            ";
            }else if (superviseList.SupType.Equals("2"))
            {
                sqlUnit = @$"
                           select distinct ut.uni_id as Id ,'' AS Parent, uni_name as [Text],ut.uni_order as TreeOrder
                            from unit ut
                            inner join unit_depart udp on ut.uni_id = udp.uni_id and udp.ude_status='1'
                            inner join departments dpt on dpt.uni_id = udp.uni_id 
                                                and dpt.ude_no = udp.ude_no and dpt.dep_no = udp.ude_depno 
                                                and dpt.dep_status='1'
                            inner join department_category dc on dc.dep_no = dpt.dep_no 
                            inner join supervise sp on sp.sup_no = dc.sup_no and sp.sup_type='2'
                            where ut.uni_status=1 and sp.acc_no={AccNo}
                            order by ut.uni_order
                            ";
            }

                allUnitList = await _context.unitTrees
                  .FromSqlRaw(sqlUnit)
                  .AsNoTracking()
                  .ToListAsync();
        }
        
        return allUnitList;


    }
    /// <summary>
    /// GetAllDepartment for 差勤
    /// </summary>
    /// <returns></returns>
    public async Task<List<DepartmentTree>> GetAllDepartment()
    {
        var sqlDept = @"
                        WITH DepartmentHierarchy AS (
                            -- 找到最上層部門 (dep_parentid = 0)
                            SELECT dep_no AS Id, '(' + isnull(dep_depid,'') +')' +dep_name AS Text, dep_parentid, CAST(NULL AS VARCHAR(MAX)) AS ParentPath,dep_order
                            FROM departments
                            WHERE dep_parentid = 0  AND dep_status = '1' 
                            UNION ALL
                            -- 遞迴處理子部門
                            SELECT d.dep_no,'(' + isnull(dep_depid,'') +')' + d.dep_name, d.dep_parentid, 
                                  CAST(isnull(h.ParentPath,'') + case when h.ParentPath is null then '' else '/' end + h.Text AS VARCHAR(MAX))   AS ParentPath,d.dep_order
                            FROM departments d
                            WHERE dep_status = '1' 
                            JOIN DepartmentHierarchy h ON d.dep_parentid = h.Id
                        )
                        SELECT Id, COALESCE(ParentPath,'') AS Parent, Text
                        FROM DepartmentHierarchy
                       WHERE NOT EXISTS (SELECT 1 FROM departments d WHERE d.dep_parentid = DepartmentHierarchy.Id)
                        ORDER BY Parent,dep_order;

                        ";
        var allDeptList = new List<DepartmentTree>();
        allDeptList = await _context.DepartmentTrees
          .FromSqlRaw(sqlDept)
          .AsNoTracking()
          .ToListAsync();
        return allDeptList;


    }
 
    public async Task<List<string>> GetDepartmentNameListAsync(List<int> depNoList)
    { 
        SqlQueryObject sqlObj = new SqlQueryObject();

        var sqlDept = @"SELECT dep_name  
                        FROM departments
                        WHERE {0} AND dep_status = '1' 
                        ORDER BY dep_order ";

        List<string> DEPNOStrList = new List<string>();

        string depNOStr = "";
        int idx = 0;
        foreach (int depNo in depNoList)
        { 
            sqlObj.AddParameter(new SqlParameter("@DEPNO" + idx, depNo));
            DEPNOStrList.Add("@DEPNO" + idx.ToString());
            idx++;
        }

        depNOStr = $" departments.dep_no IN ({string.Join(",", DEPNOStrList)})";
        sqlObj.Sql = string.Format(sqlDept, depNOStr);

        var depNameList = await SQLQueryAsync<string>(sqlObj, reader => reader["dep_name"].ToString());

        return depNameList;
    }

    public async Task<List<string>> GetDepartmentNameByUniIdListAsync(List<string> uniIdList)
    {
        SqlQueryObject sqlObj = new SqlQueryObject();

        var sqlDept = @"SELECT dep_name  
                        FROM departments
                        WHERE {0} AND dep_status = '1'
                        ORDER BY dep_order ";

        List<string> UNIIDStrList = new List<string>();

        string uniIDStr = "";
        int idx = 0;
        foreach (string uni_id in uniIdList)
        {
            sqlObj.AddParameter(new SqlParameter("@UNIID" + idx, uni_id.Trim()));
            UNIIDStrList.Add("@UNIID" + idx.ToString());
            idx++;
        }

        uniIDStr = $" departments.dep_depid IN ({string.Join(",", UNIIDStrList)})";
        sqlObj.Sql = string.Format(sqlDept, uniIDStr);

        var depNameList = await SQLQueryAsync<string>(sqlObj, reader => reader["dep_name"].ToString());

        return depNameList;
    }
}
