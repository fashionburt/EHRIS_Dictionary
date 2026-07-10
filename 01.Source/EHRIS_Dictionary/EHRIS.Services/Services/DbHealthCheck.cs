using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EHRIS.Core.Repositories;
using EHRIS.Core.Models.Common;

namespace EHRIS.Services.Services;

public class DbHealthCheck : IDbHealthCheck
{
    private readonly IConfiguration _config;

    public DbHealthCheck(IConfiguration config)
    {
        _config = config;
    }

    public ServiceResult<bool> CanConnect()
    {
        try
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return ServiceResult<bool>.Ok(true);
        }
        catch (SqlException)
        {
            return ServiceResult<bool>.Fail("資料庫無法連線，請聯絡系統管理員");
        }
        catch (Exception)
        {
            return ServiceResult<bool>.Fail("系統發生未知錯誤");
        }
    }
}
