using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;

using EHRIS.Tools.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace EHRIS.Core.Repositories;

public class SysVariableRepository : BaseRepository, ISysVariableRepository

{
    public SysVariableRepository(ApplicationDbContext context) : base(context)
    {

    }

    /// <summary>
    /// 統計表用參數
    /// </summary>
    /// <param name="stp_code"></param>
    /// <returns></returns>
    public async Task<List<StaParamsViewModel>> GetStaParamsAsync(string stp_code)
    {
        var sql = @"     SELECT stp_name, stp_params1, stp_params2
                         FROM StaParams
                         WHERE stp_status='1' AND stp_code=@STPCODE
                         ORDER BY stp_order
                        ";

        SqlQueryObject sqlObj = new SqlQueryObject();
        sqlObj.Sql = sql;

        sqlObj.AddParameter(new SqlParameter("@STPCODE", stp_code));

        var dataList = await SQLQueryAsync<StaParamsViewModel>(sqlObj,
              reader => new StaParamsViewModel
              {
                  StpName = reader["stp_name"].ToString(),
                  StpStartRange = reader["stp_params1"].ToString(),
                  StpEndRange = reader["stp_params2"].ToString()
              });


        return dataList;
    }

    /// <summary>
    /// GetAllUnitDepartment  for  統計
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysVariable>> GetSysVariableBySarVodeCodeAsync(string sarCode)
    {

        return  await _context.SysVariable
                        .Where(x=>x.SarCode == sarCode &&  x.SvrStatus=="1" )
                        .OrderBy(x=>x.SvrOrder)
                        .AsNoTracking()
                       .ToListAsync();
    }
   

}
