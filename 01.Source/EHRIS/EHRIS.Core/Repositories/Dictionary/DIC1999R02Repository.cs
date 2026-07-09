using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EHRIS.Core.Repositories.Dictionary;

public class DIC1999R02Repository : BaseRepository, IDIC1999R02Repository
{
    private readonly string _connectionTemplate;

    public DIC1999R02Repository(ApplicationDbContext context, IConfiguration configuration) : base(context)
    {
        _connectionTemplate = configuration.GetConnectionString("TargetServerTemplate") ?? "";
    }

    private string GetTargetConnStr(string serverIp, string dbKey)
    {
        var builder = new SqlConnectionStringBuilder(string.Format(_connectionTemplate, serverIp))
        {
            InitialCatalog = dbKey
        };
        return builder.ConnectionString;
    }

    public async Task<DataTableResponse<DIC1999R02ViewModel>> GetPagedListAsync(DIC1999R02Request request, string serverIp)
    {
        var keyword = request.extraSearch?.searchValue ?? "";

        var countSql = @"
        SELECT COUNT(*)
        FROM [row] r
        INNER JOIN sheet s ON r.sheet_id = s.sheet_id AND r.server_ip = s.server_ip
        INNER JOIN menu m ON s.menu_id = m.menu_id AND s.server_ip = m.server_ip
        WHERE m.menu_name = @DbKey
          AND m.server_ip = @ServerIp
          AND s.sheet_name = @TableName
          AND (@Keyword = '' OR r.row_name LIKE '%' + @Keyword + '%' OR r.row_desc LIKE '%' + @Keyword + '%')";

        var dataSql = @"
        SELECT r.row_id, r.sheet_id, r.row_name, r.row_desc, r.row_remark, r.row_type, r.row_length, r.row_null, r.sort_order
        FROM [row] r
        INNER JOIN sheet s ON r.sheet_id = s.sheet_id AND r.server_ip = s.server_ip
        INNER JOIN menu m ON s.menu_id = m.menu_id AND s.server_ip = m.server_ip
        WHERE m.menu_name = @DbKey
          AND m.server_ip = @ServerIp
          AND s.sheet_name = @TableName
          AND (@Keyword = '' OR r.row_name LIKE '%' + @Keyword + '%' OR r.row_desc LIKE '%' + @Keyword + '%')
        ORDER BY r.sort_order, r.row_id
        OFFSET @Start ROWS FETCH NEXT @Length ROWS ONLY";

        var pkColumns = new List<string>();
        var fkColumns = new List<string>();

        using (var conn = new SqlConnection(GetTargetConnStr(serverIp, request.DbKey)))
        {
            await conn.OpenAsync();

            using (var cmd = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                                          WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 
                                          AND TABLE_NAME = @TableName", conn))
            {
                cmd.Parameters.AddWithValue("@TableName", request.TableName);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) pkColumns.Add(reader.GetString(0));
            }

            using (var cmd = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                                          WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsForeignKey') = 1 
                                          AND TABLE_NAME = @TableName", conn))
            {
                cmd.Parameters.AddWithValue("@TableName", request.TableName);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) fkColumns.Add(reader.GetString(0));
            }
        }

        using var localConn = new SqlConnection(_context.Database.GetConnectionString());
        await localConn.OpenAsync();

        int total;
        using (var cmd = new SqlCommand(countSql, localConn))
        {
            cmd.Parameters.AddWithValue("@DbKey", request.DbKey ?? "");
            cmd.Parameters.AddWithValue("@ServerIp", serverIp);
            cmd.Parameters.AddWithValue("@TableName", request.TableName ?? "");
            cmd.Parameters.AddWithValue("@Keyword", keyword);
            total = (int)await cmd.ExecuteScalarAsync();
        }

        var data = new List<DIC1999R02ViewModel>();
        using (var cmd = new SqlCommand(dataSql, localConn))
        {
            cmd.Parameters.AddWithValue("@DbKey", request.DbKey ?? "");
            cmd.Parameters.AddWithValue("@ServerIp", serverIp);
            cmd.Parameters.AddWithValue("@TableName", request.TableName ?? "");
            cmd.Parameters.AddWithValue("@Keyword", keyword);
            cmd.Parameters.AddWithValue("@Start", request.start);
            cmd.Parameters.AddWithValue("@Length", request.length);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var rowName = reader.GetString(reader.GetOrdinal("row_name"));
                data.Add(new DIC1999R02ViewModel
                {
                    RowId = reader.GetInt32(reader.GetOrdinal("row_id")),
                    SheetId = reader.GetInt32(reader.GetOrdinal("sheet_id")),
                    RowName = rowName,
                    RowDesc = reader.IsDBNull(reader.GetOrdinal("row_desc")) ? null : reader.GetString(reader.GetOrdinal("row_desc")),
                    RowRemark = reader.IsDBNull(reader.GetOrdinal("row_remark")) ? null : reader.GetString(reader.GetOrdinal("row_remark")),
                    DataType = reader.IsDBNull(reader.GetOrdinal("row_type")) ? null : reader.GetString(reader.GetOrdinal("row_type")),
                    Length = reader.IsDBNull(reader.GetOrdinal("row_length")) ? null : reader.GetInt32(reader.GetOrdinal("row_length")),
                    IsNullable = reader.GetBoolean(reader.GetOrdinal("row_null")),
                    IsPrimaryKey = pkColumns.Contains(rowName),
                    IsForeignKey = fkColumns.Contains(rowName)
                });
            }
        }

        return new DataTableResponse<DIC1999R02ViewModel>
        {
            draw = request.draw,
            recordsTotal = total,
            recordsFiltered = total,
            data = data
        };
    }

    public async Task<bool> SyncTableFieldsAsync(string dbKey, string serverIp, string tableName, IDataLogger dataLogger)
    {
        var sheet = await _context.Sheets.FirstOrDefaultAsync(x => x.SheetName == tableName && x.ServerIP == serverIp &&
                                     _context.Menus.Any(m => m.MenuId == x.MenuId && m.MenuName == dbKey && m.ServerIP == serverIp));
        if (sheet == null) return false;

        var physicalFields = new List<(string RowName, string DataType, int? Length, bool IsNullable)>();

        using (var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey)))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand(@"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, 
                                              CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END 
                                              FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName", conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    physicalFields.Add((
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2),
                        reader.GetInt32(3) == 1
                    ));
                }
            }
        }

        var existingRows = await _context.Rows.Where(x => x.SheetId == sheet.SheetId && x.ServerIP == serverIp).ToListAsync();

        foreach (var field in physicalFields)
        {
            var row = existingRows.FirstOrDefault(r => r.RowName == field.RowName);
            if (row == null)
            {
                _context.Rows.Add(new Row
                {
                    SheetId = sheet.SheetId,
                    ServerIP = serverIp,
                    RowName = field.RowName,
                    RowDesc = "",
                    RowRemark = "",
                    RowType = field.DataType,
                    RowLength = field.Length,
                    RowNull = field.IsNullable,
                    SortOrder = 1
                });

                _context.Logs.Add(new Log
                {
                    DbKey = dbKey,
                    ServerIP = serverIp,
                    TableName = tableName,
                    PkName = field.RowName,
                    State = 10,
                    Detail = $"【{field.RowName}】在伺服器 {serverIp} 被同步新增描述",
                    Date = DateTime.Now
                });
            }
            else
            {
                row.RowType = field.DataType;
                row.RowLength = field.Length;
                row.RowNull = field.IsNullable;
            }
        }

        await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<(bool success, string message)> UpdateFieldsAsync(List<DIC1999R02ViewModel> updates, string serverIp, IDataLogger dataLogger)
    {
        var rowIds = updates.Select(u => u.RowId).ToList();
        var entities = await _context.Rows.Where(x => rowIds.Contains(x.RowId) && x.ServerIP == serverIp).ToListAsync();

        foreach (var item in updates)
        {
            var entity = entities.FirstOrDefault(x => x.RowId == item.RowId);
            if (entity != null)
            {
                entity.RowDesc = item.RowDesc;
                entity.RowRemark = item.RowRemark;
            }
        }

        await SaveChangesAsync(dataLogger);
        return (true, "更新成功");
    }

    public async Task<DIC1999R02ViewModel?> GetFieldOriginalDataAsync(int rowId, string serverIp)
    {
        return await _context.Rows.AsNoTracking()
            .Where(x => x.RowId == rowId && x.ServerIP == serverIp)
            .Select(x => new DIC1999R02ViewModel { RowDesc = x.RowDesc, RowRemark = x.RowRemark, RowName = x.RowName ?? "" })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool success, string message)> CreatePhysicalColumnAsync(DIC1999R02CreateViewModel model, string serverIp, IDataLogger dataLogger)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sheet = await _context.Sheets.FirstOrDefaultAsync(x => x.SheetName == model.TableName && x.ServerIP == serverIp &&
                                         _context.Menus.Any(m => m.MenuId == x.MenuId && m.MenuName == model.DbKey && m.ServerIP == serverIp));

            if (sheet == null) return (false, "找不到對應伺服器的資料表註冊資訊");

            using (var conn = new SqlConnection(GetTargetConnStr(serverIp, model.DbKey)))
            {
                await conn.OpenAsync();

                string lengthStr = (model.DataType.Contains("char") || model.DataType.Contains("binary"))
                                   ? $"({(model.Length == -1 ? "MAX" : model.Length.ToString())})" : "";
                string nullStr = model.IsNullable ? "NULL" : "NOT NULL";

                string alterSql = $"ALTER TABLE [{model.TableName}] ADD [{model.ColumnName}] {model.DataType}{lengthStr} {nullStr}";
                using (var cmd = new SqlCommand(alterSql, conn)) await cmd.ExecuteNonQueryAsync();

                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    string descSql = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@desc, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@tableName, @level2type=N'COLUMN', @level2name=@colName";
                    using var cmdDesc = new SqlCommand(descSql, conn);
                    cmdDesc.Parameters.AddWithValue("@desc", model.Description);
                    cmdDesc.Parameters.AddWithValue("@tableName", model.TableName);
                    cmdDesc.Parameters.AddWithValue("@colName", model.ColumnName);
                    await cmdDesc.ExecuteNonQueryAsync();
                }
            }

            var maxSort = await _context.Rows.Where(x => x.SheetId == sheet.SheetId && x.ServerIP == serverIp).MaxAsync(x => (int?)x.SortOrder) ?? 0;

            var row = new Row
            {
                SheetId = sheet.SheetId,
                ServerIP = serverIp,
                RowName = model.ColumnName,
                RowDesc = model.Description ?? "",
                RowRemark = model.Remark ?? "",
                RowType = model.DataType,
                RowLength = model.Length,
                RowNull = model.IsNullable,
                SortOrder = maxSort + 1
            };
            _context.Rows.Add(row);

            _context.Logs.Add(new Log
            {
                DbKey = model.DbKey,
                ServerIP = serverIp,
                TableName = model.TableName,
                PkName = model.ColumnName,
                State = 10,
                Detail = $"【{model.ColumnName}】於伺服器 {serverIp} 建立實體欄位並新增描述",
                Date = DateTime.Now
            });

            await SaveChangesAsync(dataLogger);
            await transaction.CommitAsync();
            return (true, "資料欄位建立成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeletePhysicalColumnAsync(string dbKey, string serverIp, string tableName, string columnName, int rowId, IDataLogger dataLogger)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            using (var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey)))
            {
                await conn.OpenAsync();
                string dropSql = $"ALTER TABLE [{tableName}] DROP COLUMN [{columnName}]";
                using (var cmd = new SqlCommand(dropSql, conn)) await cmd.ExecuteNonQueryAsync();
            }

            var row = await _context.Rows.FirstOrDefaultAsync(x => x.RowId == rowId && x.ServerIP == serverIp);
            if (row != null) _context.Rows.Remove(row);

            _context.Logs.Add(new Log
            {
                DbKey = dbKey,
                ServerIP = serverIp,
                TableName = tableName,
                PkName = columnName,
                State = 30,
                Detail = $"【{columnName}】於伺服器 {serverIp} 刪除實體欄位與描述",
                Date = DateTime.Now
            });

            await SaveChangesAsync(dataLogger);
            await transaction.CommitAsync();
            return (true, "資料欄位刪除成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<(DataTable dataTable, List<string> pkList, List<string> fkList, Dictionary<string, string> colDescDict)> GetTableDetailAsync(string dbKey, string serverIp, string tableName, string keyword)
    {
        using var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey));
        await conn.OpenAsync();

        var dataTable = new DataTable();
        string selectSql = $"SELECT TOP 200 * FROM [{tableName}]";
        using (var cmd = new SqlCommand(selectSql, conn))
        using (var reader = await cmd.ExecuteReaderAsync()) dataTable.Load(reader);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var filtered = dataTable.Clone();
            foreach (DataRow row in dataTable.Rows)
            {
                if (row.ItemArray.Any(x => x != null && x.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    filtered.ImportRow(row);
            }
            dataTable = filtered;
        }

        var pkList = new List<string>();
        string pkSql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                         WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 
                         AND TABLE_NAME = @t";
        using (var cmd = new SqlCommand(pkSql, conn))
        {
            cmd.Parameters.AddWithValue("@t", tableName);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) pkList.Add(reader.GetString(0));
        }

        var fkList = new List<string>();
        string fkSql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                         WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsForeignKey') = 1 
                         AND TABLE_NAME = @t";
        using (var cmd = new SqlCommand(fkSql, conn))
        {
            cmd.Parameters.AddWithValue("@t", tableName);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) fkList.Add(reader.GetString(0));
        }

        var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        var colDescDict = await _context.Rows.AsNoTracking()
            .Where(x => columnNames.Contains(x.RowName) && x.ServerIP == serverIp &&
                        _context.Sheets.Any(s => s.SheetId == x.SheetId && s.SheetName == tableName && s.ServerIP == serverIp))
            .ToDictionaryAsync(x => x.RowName, x => x.RowDesc ?? x.RowName);

        return (dataTable, pkList, fkList, colDescDict);
    }
}