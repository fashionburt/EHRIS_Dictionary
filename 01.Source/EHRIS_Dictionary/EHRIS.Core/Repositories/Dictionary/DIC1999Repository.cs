using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace EHRIS.Core.Repositories.Dictionary;

public class DIC1999Repository : BaseRepository, IDIC1999Repository
{
    private readonly string _connectionTemplate;

    public DIC1999Repository(ApplicationDbContext context, IConfiguration config) : base(context)
    {
        _connectionTemplate = config.GetConnectionString("TargetServerTemplate") ?? "";
    }

    public async Task<DataTableResponse<DIC1999ViewModel>> GetPagedListAsync(DataTableRequest request, string serverIp, string clientIp)
    {
        var countSql = @"
        SELECT COUNT(*)
        FROM menu m
        INNER JOIN menu_access ma ON m.menu_id = ma.menu_id
        WHERE m.is_enabled = 1
          AND m.server_ip = @ServerIp
          AND ma.is_enabled = 1
          AND ma.client_ip = @ClientIp";

        var dataSql = @"
        SELECT m.menu_id, m.menu_name, m.menu_desc, m.is_enabled, m.sort_order
        FROM menu m
        INNER JOIN menu_access ma ON m.menu_id = ma.menu_id
        WHERE m.is_enabled = 1
          AND m.server_ip = @ServerIp
          AND ma.is_enabled = 1
          AND ma.client_ip = @ClientIp
        ORDER BY m.sort_order, m.menu_name
        OFFSET @Start ROWS FETCH NEXT @Length ROWS ONLY";

        using var conn = new Microsoft.Data.SqlClient.SqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync();

        int total;
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(countSql, conn))
        {
            cmd.Parameters.AddWithValue("@ServerIp", serverIp);
            cmd.Parameters.AddWithValue("@ClientIp", clientIp);
            total = (int)await cmd.ExecuteScalarAsync();
        }

        var data = new List<DIC1999ViewModel>();
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dataSql, conn))
        {
            cmd.Parameters.AddWithValue("@ServerIp", serverIp);
            cmd.Parameters.AddWithValue("@ClientIp", clientIp);
            cmd.Parameters.AddWithValue("@Start", request.start);
            cmd.Parameters.AddWithValue("@Length", request.length);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                data.Add(new DIC1999ViewModel
                {
                    MenuId = reader.GetInt32(reader.GetOrdinal("menu_id")),
                    MenuName = reader.GetString(reader.GetOrdinal("menu_name")),
                    MenuDesc = reader.IsDBNull(reader.GetOrdinal("menu_desc")) ? null : reader.GetString(reader.GetOrdinal("menu_desc")),
                    IsEnabled = reader.GetInt32(reader.GetOrdinal("is_enabled")),
                    SortOrder = reader.GetInt32(reader.GetOrdinal("sort_order"))
                });
            }
        }

        return new DataTableResponse<DIC1999ViewModel>
        {
            draw = request.draw,
            recordsTotal = total,
            recordsFiltered = total,
            data = data
        };
    }

    public async Task<List<string>> GetSystemDatabaseNamesAsync(string serverIp)
    {
        var existingMenus = await _context.Menus
            .Where(m => m.IsEnabled == 1 && m.ServerIP == serverIp)
            .Select(m => m.MenuName)
            .ToListAsync();

        var targetConnStr = string.Format(_connectionTemplate, serverIp);

        var allDatabases = new List<string>();
        using (var conn = new Microsoft.Data.SqlClient.SqlConnection(targetConnStr))
        {
            await conn.OpenAsync();
            var sql = "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb', 'Resource', 'distribution') AND state = 0 ORDER BY name";
            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) allDatabases.Add(reader.GetString(0));
        }

        return allDatabases.Where(db => !existingMenus.Contains(db)).ToList();
    }

    public async Task<bool> AnyAsync(Expression<Func<Menu, bool>> predicate)
    {
        return await _context.Menus.AnyAsync(predicate);
    }

    public async Task<Menu?> GetByIdAsync(int id)
    {
        return await _context.Menus.FindAsync(id);
    }

    public async Task<bool> AddAsync(Menu entity, string detail, IDataLogger dataLogger)
    {
        await _context.Menus.AddAsync(entity);
        _context.Logs.Add(new Log
        {
            DbKey = entity.MenuName,
            ServerIP = entity.ServerIP,
            TableName = "",
            PkName = "",
            State = 10,
            Detail = detail,
            Date = DateTime.Now
        });
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> UpdateAsync(Menu entity, string detail, IDataLogger dataLogger)
    {
        _context.Logs.Add(new Log
        {
            DbKey = entity.MenuName,
            ServerIP = entity.ServerIP,
            TableName = "",
            PkName = "",
            State = 20,
            Detail = detail,
            Date = DateTime.Now
        });
        _context.Menus.Update(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<bool> DeleteByIdAsync(int id, string detail, IDataLogger dataLogger)
    {
        var entity = await _context.Menus.FindAsync(id);
        if (entity == null) return false;

        _context.Logs.Add(new Log
        {
            DbKey = entity.MenuName,
            ServerIP = entity.ServerIP,
            TableName = "",
            PkName = "",
            State = 30,
            Detail = detail,
            Date = DateTime.Now
        });

        entity.IsEnabled = 2;
        _context.Menus.Update(entity);
        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<Menu?> GetByNameAsync(string name, string serverIp)
    {
        return await _context.Menus.FirstOrDefaultAsync(a => a.MenuName == name && a.ServerIP == serverIp);
    }

    public async Task<(string Name, string Desc)> GetDatabaseInfoAsync(int menuId, string serverIp)
    {
        var data = await _context.Menus
            .Where(a => a.MenuId == menuId && a.ServerIP == serverIp)
            .Select(a => new { a.MenuName, a.MenuDesc })
            .FirstOrDefaultAsync();
        return (data?.MenuName ?? "", data?.MenuDesc ?? "");
    }

    public async Task<List<(string TableName, string TableDesc)>> GetSheetMetadataAsync(int menuId, string serverIp)
    {
        var sql = "SELECT sheet_name, sheet_desc FROM dbo.sheet WHERE menu_id = @menu_id AND server_ip = @server_ip";
        var result = new List<(string, string)>();
        using (var cmd = _context.Database.GetDbConnection().CreateCommand())
        {
            cmd.CommandText = sql;

            var pId = cmd.CreateParameter();
            pId.ParameterName = "@menu_id";
            pId.Value = menuId;
            cmd.Parameters.Add(pId);

            var pIp = cmd.CreateParameter();
            pIp.ParameterName = "@server_ip";
            pIp.Value = serverIp;
            cmd.Parameters.Add(pIp);

            if (cmd.Connection.State != System.Data.ConnectionState.Open) await cmd.Connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((reader.GetString(0), reader.IsDBNull(1) ? "" : reader.GetString(1)));
            }
        }
        return result;
    }

    public async Task<List<(string TableName, string ColumnName, string RowDesc)>> GetRowMetadataAsync(int menuId, string serverIp)
    {
        var sql = @"SELECT s.sheet_name, r.row_name, r.row_desc 
                FROM dbo.row r 
                JOIN dbo.sheet s ON r.sheet_id = s.sheet_id 
                WHERE s.menu_id = @menu_id AND s.server_ip = @server_ip AND r.server_ip = @server_ip";
        var result = new List<(string, string, string)>();
        using (var cmd = _context.Database.GetDbConnection().CreateCommand())
        {
            cmd.CommandText = sql;

            var pId = cmd.CreateParameter();
            pId.ParameterName = "@menu_id";
            pId.Value = menuId;
            cmd.Parameters.Add(pId);

            var pIp = cmd.CreateParameter();
            pIp.ParameterName = "@server_ip";
            pIp.Value = serverIp;
            cmd.Parameters.Add(pIp);

            if (cmd.Connection.State != System.Data.ConnectionState.Open) await cmd.Connection.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? "" : reader.GetString(2)));
            }
        }
        return result;
    }

    public async Task<List<string>> GetPhysicalTablesAsync(string serverIp, string targetDbName)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(string.Format(_connectionTemplate, serverIp))
        {
            InitialCatalog = targetDbName
        };
        var tables = new List<string>();
        using (var conn = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString))
        {
            await conn.OpenAsync();
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public async Task<List<MergedColumnInfo>> GetPhysicalSchemaAsync(string serverIp, string targetDbName, string tableName)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(string.Format(_connectionTemplate, serverIp))
        {
            InitialCatalog = targetDbName
        };
        var columns = new List<MergedColumnInfo>();
        using (var conn = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString))
        {
            await conn.OpenAsync();
            var sql = @"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE 
                    FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName ORDER BY ORDINAL_POSITION";
            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(new MergedColumnInfo
                {
                    ColumnName = reader.GetString(0),
                    DataType = reader.GetString(1),
                    MaxLength = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    IsNullable = reader.GetString(3) == "YES"
                });
            }
        }
        return columns;
    }
    public async Task<List<(string TableName, string ColumnName, string DataType, int? Length, bool IsNullable, string KeyType)>> GetFullSchemaForWordExportAsync(string serverIp, string targetDbName)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(string.Format(_connectionTemplate, serverIp))
        {
            InitialCatalog = targetDbName
        };

        var result = new List<(string TableName, string ColumnName, string DataType, int? Length, bool IsNullable, string KeyType)>();

        using var conn = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        var sql = @"
        SELECT 
            c.TABLE_NAME,
            c.COLUMN_NAME,
            c.DATA_TYPE,
            c.CHARACTER_MAXIMUM_LENGTH,
            c.IS_NULLABLE,
            CASE 
                WHEN pk.COLUMN_NAME IS NOT NULL THEN 'PK'
                WHEN fk.COLUMN_NAME IS NOT NULL THEN 'FK'
                ELSE ''
            END AS KEY_TYPE
        FROM INFORMATION_SCHEMA.COLUMNS c
        LEFT JOIN (
            SELECT ku.TABLE_NAME, ku.COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
            WHERE OBJECTPROPERTY(OBJECT_ID(ku.CONSTRAINT_SCHEMA + '.' + ku.CONSTRAINT_NAME), 'IsPrimaryKey') = 1
        ) pk ON pk.TABLE_NAME = c.TABLE_NAME AND pk.COLUMN_NAME = c.COLUMN_NAME
        LEFT JOIN (
            SELECT ku.TABLE_NAME, ku.COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
            WHERE OBJECTPROPERTY(OBJECT_ID(ku.CONSTRAINT_SCHEMA + '.' + ku.CONSTRAINT_NAME), 'IsForeignKey') = 1
        ) fk ON fk.TABLE_NAME = c.TABLE_NAME AND fk.COLUMN_NAME = c.COLUMN_NAME
        WHERE c.TABLE_NAME != 'sysdiagrams'
        ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION";

        using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3),
                reader.GetString(4) == "YES",
                reader.GetString(5)
            ));
        }

        return result;
    }
}