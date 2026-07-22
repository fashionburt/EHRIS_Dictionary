using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace EHRIS.Core.Repositories.Dictionary;

public class DIC1999R01Repository : BaseRepository, IDIC1999R01Repository
{
    private readonly string _connectionTemplate;

    public DIC1999R01Repository(ApplicationDbContext context, IConfiguration configuration) : base(context)
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

    public async Task<DataTableResponse<DIC1999R01ViewModel>> GetPagedListAsync(DIC1999R01Request request, string serverIp)
    {
        var keyword = request.extraSearch?.searchValue ?? "";

        var countSql = @"
        SELECT COUNT(*)
        FROM sheet s
        INNER JOIN menu m ON s.menu_id = m.menu_id AND s.server_ip = m.server_ip
        WHERE m.menu_name = @DbKey
          AND m.server_ip = @ServerIp
          AND s.sheet_name != 'sysdiagrams'
          AND (@Keyword = '' OR s.sheet_name LIKE '%' + @Keyword + '%' OR s.sheet_desc LIKE '%' + @Keyword + '%')";

        var dataSql = @"
        SELECT s.sheet_id, s.menu_id, s.sheet_name, s.sheet_desc
        FROM sheet s
        INNER JOIN menu m ON s.menu_id = m.menu_id AND s.server_ip = m.server_ip
        WHERE m.menu_name = @DbKey
          AND m.server_ip = @ServerIp
          AND s.sheet_name != 'sysdiagrams'
          AND (@Keyword = '' OR s.sheet_name LIKE '%' + @Keyword + '%' OR s.sheet_desc LIKE '%' + @Keyword + '%')
        ORDER BY s.sheet_name
        OFFSET @Start ROWS FETCH NEXT @Length ROWS ONLY";

        using var conn = new SqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync();

        int total;
        using (var cmd = new SqlCommand(countSql, conn))
        {
            cmd.Parameters.AddWithValue("@DbKey", request.DbKey ?? "");
            cmd.Parameters.AddWithValue("@ServerIp", serverIp);
            cmd.Parameters.AddWithValue("@Keyword", keyword);
            total = (int)await cmd.ExecuteScalarAsync();
        }

        var data = new List<DIC1999R01ViewModel>();
        using (var cmd = new SqlCommand(dataSql, conn))
        {
            cmd.Parameters.AddWithValue("@DbKey", request.DbKey ?? "");
            cmd.Parameters.AddWithValue("@ServerIp", serverIp);
            cmd.Parameters.AddWithValue("@Keyword", keyword);
            cmd.Parameters.AddWithValue("@Start", request.start);
            cmd.Parameters.AddWithValue("@Length", request.length);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                data.Add(new DIC1999R01ViewModel
                {
                    SheetId = reader.GetInt32(reader.GetOrdinal("sheet_id")),
                    MenuId = reader.GetInt32(reader.GetOrdinal("menu_id")),
                    TableName = reader.GetString(reader.GetOrdinal("sheet_name")),
                    OriginalTableName = reader.GetString(reader.GetOrdinal("sheet_name")),
                    SheetDesc = reader.IsDBNull(reader.GetOrdinal("sheet_desc")) ? null : reader.GetString(reader.GetOrdinal("sheet_desc")),
                    DbKey = request.DbKey
                });
            }
        }

        return new DataTableResponse<DIC1999R01ViewModel>
        {
            draw = request.draw,
            recordsTotal = total,
            recordsFiltered = total,
            data = data
        };
    }

    public async Task<bool> SyncTablesAsync(string dbKey, string serverIp, IDataLogger dataLogger)
    {
        var menu = await _context.Menus.FirstOrDefaultAsync(x => x.MenuName == dbKey && x.ServerIP == serverIp);
        if (menu == null) return false;

        var physicalTables = new List<string>();
        using (var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey)))
        {
            await conn.OpenAsync();
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != 'sysdiagrams'";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) physicalTables.Add(reader.GetString(0));
        }

        var existingSheets = await _context.Sheets
            .Where(x => x.MenuId == menu.MenuId && x.ServerIP == serverIp)
            .Select(x => x.SheetName).ToListAsync();

        var newTables = physicalTables.Except(existingSheets, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var tableName in newTables)
        {
            _context.Sheets.Add(new Sheet
            {
                MenuId = menu.MenuId,
                ServerIP = serverIp,
                SheetName = tableName,
                SheetDesc = "",
                SortOrder = 1
            });

            _context.Logs.Add(new Log
            {
                DbKey = dbKey,
                ServerIP = serverIp,
                TableName = tableName,
                State = 10,
                Detail = $"【{tableName}】({serverIp}) 被同步新增至描述庫",
                Date = DateTime.Now
            });
        }

        if (newTables.Any()) await SaveChangesAsync(dataLogger);
        return true;
    }

    public async Task<(bool success, string message)> UpdateDescriptionAsync(DIC1999R01ViewModel model, string serverIp, IDataLogger dataLogger)
    {
        var entity = await _context.Sheets.FirstOrDefaultAsync(x => x.SheetId == model.SheetId && x.ServerIP == serverIp);
        if (entity == null) return (false, "找不到描述紀錄或伺服器不符");

        var originalDesc = entity.SheetDesc ?? "";

        var dbKey = await (from s in _context.Sheets
                           join m in _context.Menus on s.MenuId equals m.MenuId
                           where s.SheetId == model.SheetId && s.ServerIP == serverIp
                           select m.MenuName).FirstOrDefaultAsync();

        _context.Logs.Add(new Log
        {
            DbKey = dbKey ?? "",
            ServerIP = serverIp,
            TableName = entity.SheetName ?? "",
            PkName = "",
            State = 20,
            Detail = $"【{entity.SheetName}】描述更新：由「{originalDesc}」變更為「{model.SheetDesc ?? ""}」",
            Date = DateTime.Now
        });

        entity.SheetDesc = model.SheetDesc ?? "";
        await SaveChangesAsync(dataLogger);
        return (true, "更新成功");
    }

    public async Task<(bool success, string message)> DeleteTableAsync(string dbKey, string serverIp, string tableName, int sheetId, IDataLogger dataLogger)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            using (var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey)))
            {
                await conn.OpenAsync();
                var sql = $"DROP TABLE [{tableName}]";
                using var cmd = new SqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
            }

            _context.Logs.Add(new Log
            {
                DbKey = dbKey,
                ServerIP = serverIp,
                TableName = tableName,
                PkName = "",
                State = 30,
                Detail = $"【{tableName}】在伺服器 {serverIp} 被「刪除」實體表與描述資訊",
                Date = DateTime.Now
            });

            var rows = _context.Rows.Where(x => x.SheetId == sheetId && x.ServerIP == serverIp);
            _context.Rows.RemoveRange(rows);

            var sheet = await _context.Sheets.FirstOrDefaultAsync(x => x.SheetId == sheetId && x.ServerIP == serverIp);
            if (sheet != null) _context.Sheets.Remove(sheet);

            await SaveChangesAsync(dataLogger);
            await transaction.CommitAsync();
            return (true, "刪除成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"刪除失敗：{ex.Message}");
        }
    }

    public async Task<(bool success, string message)> RenameTableAsync(string dbKey, string serverIp, string oldName, string newName, int sheetId, IDataLogger dataLogger)
    {
        var sheet = await _context.Sheets.FirstOrDefaultAsync(x => x.SheetId == sheetId && x.ServerIP == serverIp);
        if (sheet == null) return (false, "找不到對應伺服器的描述紀錄");

        try
        {
            using (var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey)))
            {
                await conn.OpenAsync();
                var sql = $"EXEC sp_rename '{oldName}', '{newName}'";
                using var cmd = new SqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
            }

            _context.Logs.Add(new Log
            {
                DbKey = dbKey,
                ServerIP = serverIp,
                TableName = newName,
                PkName = "",
                State = 20,
                Detail = $"【{oldName}】在伺服器 {serverIp} 更名為「{newName}」",
                Date = DateTime.Now
            });

            sheet.SheetName = newName;
            await SaveChangesAsync(dataLogger);
            return (true, "更名成功");
        }
        catch (Exception ex)
        {
            return (false, $"物理更名失敗：{ex.Message}");
        }
    }

    public async Task<string?> GetSheetDescAsync(int sheetId, string serverIp)
    {
        return await _context.Sheets
            .Where(x => x.SheetId == sheetId && x.ServerIP == serverIp)
            .Select(x => x.SheetDesc)
            .FirstOrDefaultAsync();
    }

    public async Task<(bool success, string message)> CreatePhysicalTableAsync(DIC1999R01CreateViewModel model, string serverIp, IDataLogger dataLogger)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(x => x.MenuName == model.DbKey && x.ServerIP == serverIp);
            if (menu == null) return (false, "找不到對應的資料庫註冊資訊");

            using (var conn = new SqlConnection(GetTargetConnStr(serverIp, model.DbKey)))
            {
                await conn.OpenAsync();

                string identityStr = (model.PkType == "int" && model.PkIdentity) ? "IDENTITY(1,1)" : "";
                string createSql = $@"CREATE TABLE [{model.TableName}] ([{model.PkName}] {model.PkType} {identityStr} NOT NULL PRIMARY KEY)";
                using (var cmd = new SqlCommand(createSql, conn)) await cmd.ExecuteNonQueryAsync();

                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    string descSql = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@desc, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@tableName";
                    using var cmdDesc = new SqlCommand(descSql, conn);
                    cmdDesc.Parameters.AddWithValue("@desc", model.Description);
                    cmdDesc.Parameters.AddWithValue("@tableName", model.TableName);
                    await cmdDesc.ExecuteNonQueryAsync();
                }

                if (!string.IsNullOrWhiteSpace(model.PkDescription))
                {
                    string pkDescSql = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@desc, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@tableName, @level2type=N'COLUMN', @level2name=@pkName";
                    using var cmdPkDesc = new SqlCommand(pkDescSql, conn);
                    cmdPkDesc.Parameters.AddWithValue("@desc", model.PkDescription);
                    cmdPkDesc.Parameters.AddWithValue("@tableName", model.TableName);
                    cmdPkDesc.Parameters.AddWithValue("@pkName", model.PkName);
                    await cmdPkDesc.ExecuteNonQueryAsync();
                }
            }

            var sheet = new Sheet
            {
                MenuId = menu.MenuId,
                ServerIP = serverIp,
                SheetName = model.TableName,
                SheetDesc = model.Description ?? "",
                SortOrder = 1
            };
            _context.Sheets.Add(sheet);
            await SaveChangesAsync(dataLogger);

            var row = new Row
            {
                SheetId = sheet.SheetId,
                ServerIP = serverIp,
                RowName = model.PkName,
                RowDesc = model.PkDescription ?? "",
                RowType = model.PkType,
                RowLength = model.PkType == "int" ? 4 : (model.PkType == "uniqueidentifier" ? 16 : 0),
                RowNull = false,
                SortOrder = 1
            };
            _context.Rows.Add(row);

            _context.Logs.Add(new Log
            {
                DbKey = model.DbKey,
                TableName = model.TableName,
                PkName = "",
                State = 10,
                Detail = $"【{model.TableName}】被「新增」描述",
                Date = DateTime.Now,
                ServerIP = serverIp
            });

            await SaveChangesAsync(dataLogger);
            await transaction.CommitAsync();
            return (true, "資料表建立成功");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<(int tableCount, int columnCount)> ImportDictionaryAsync(string dbKey, string serverIp, Dictionary<string, DIC1999R01ImportTableData> data, string detail, IDataLogger dataLogger)
    {
        var sheets = await (from s in _context.Sheets
                            join m in _context.Menus on s.MenuId equals m.MenuId
                            where m.MenuName == dbKey && m.ServerIP == serverIp && s.ServerIP == serverIp
                            select s).ToListAsync();

        int tableCount = 0;
        int columnCount = 0;

        foreach (var sheetGroup in data)
        {
            var tableName = sheetGroup.Key;
            var sheet = sheets.FirstOrDefault(s => s.SheetName == tableName);
            if (sheet == null) continue;

            var physicalColumnNames = await GetPhysicalColumnNamesAsync(serverIp, dbKey, tableName);
            if (physicalColumnNames.Count == 0) continue;

            var rows = await SyncTableFieldsForImportAsync(dbKey, serverIp, tableName, sheet.SheetId, dataLogger);

            bool tableTouched = false;

            var sheetDesc = sheetGroup.Value.SheetDesc;
            if (!string.IsNullOrWhiteSpace(sheetDesc) && sheetDesc != tableName)
            {
                sheet.SheetDesc = sheetDesc;
                tableTouched = true;
            }

            if (sheetGroup.Value.Columns != null)
            {
                foreach (var col in sheetGroup.Value.Columns)
                {
                    var colName = col.Key;
                    var colDesc = col.Value;

                    if (string.IsNullOrWhiteSpace(colDesc)) continue;
                    if (colDesc == colName) continue;
                    if (!physicalColumnNames.Contains(colName)) continue;

                    var row = rows.FirstOrDefault(r => r.RowName == colName);
                    if (row == null) continue;

                    row.RowDesc = colDesc;
                    columnCount++;
                    tableTouched = true;
                }
            }

            if (tableTouched) tableCount++;
        }

        if (tableCount > 0)
        {
            _context.Logs.Add(new Log
            {
                DbKey = dbKey,
                ServerIP = serverIp,
                TableName = "",
                PkName = "",
                State = 20,
                Detail = detail,
                Date = DateTime.Now
            });
        }

        await SaveChangesAsync(dataLogger);
        return (tableCount, columnCount);
    }

    private async Task<HashSet<string>> GetPhysicalColumnNamesAsync(string serverIp, string dbKey, string tableName)
    {
        var columns = new HashSet<string>(StringComparer.Ordinal);
        using var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey));
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName", conn);
        cmd.Parameters.AddWithValue("@TableName", tableName);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) columns.Add(reader.GetString(0));
        return columns;
    }

    private async Task<List<Row>> SyncTableFieldsForImportAsync(string dbKey, string serverIp, string tableName, int sheetId, IDataLogger dataLogger)
    {
        var physicalFields = new List<(string RowName, string DataType, int? Length, bool IsNullable)>();

        using (var conn = new SqlConnection(GetTargetConnStr(serverIp, dbKey)))
        {
            await conn.OpenAsync();
            using var cmd = new SqlCommand(@"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, 
                                          CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END 
                                          FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName", conn);
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

        var existingRows = await _context.Rows.Where(x => x.SheetId == sheetId && x.ServerIP == serverIp).ToListAsync();
        var resultRows = new List<Row>();

        foreach (var field in physicalFields)
        {
            var row = existingRows.FirstOrDefault(r => r.RowName == field.RowName);
            if (row == null)
            {
                row = new Row
                {
                    SheetId = sheetId,
                    ServerIP = serverIp,
                    RowName = field.RowName,
                    RowDesc = "",
                    RowRemark = "",
                    RowType = field.DataType,
                    RowLength = field.Length,
                    RowNull = field.IsNullable,
                    SortOrder = 1
                };
                _context.Rows.Add(row);
            }
            else
            {
                row.RowType = field.DataType;
                row.RowLength = field.Length;
                row.RowNull = field.IsNullable;
            }

            resultRows.Add(row);
        }

        return resultRows;
    }
}