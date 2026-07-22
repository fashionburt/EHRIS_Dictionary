using ClosedXML.Excel;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using EHRIS.Core.Repositories.Dictionary;
using System.Text;
using System.Text.Json;

namespace EHRIS.Services.Dictionary;

public class DIC1999Service : IDIC1999Service
{
    private readonly IDIC1999Repository _repository;

    public DIC1999Service(IDIC1999Repository repository)
    {
        _repository = repository;
    }

    public async Task<DataTableResponse<DIC1999ViewModel>> GetDataTableAsync(DataTableRequest request, string serverIp, string clientIp)
    {
        return await _repository.GetPagedListAsync(request, serverIp, clientIp);
    }

    public async Task<List<string>> GetAllDatabaseNamesAsync(string serverIp)
    {
        return await _repository.GetSystemDatabaseNamesAsync(serverIp);
    }

    public async Task<(bool success, string message)> AddDescriptionAsync(DIC1999ViewModel model, string serverIp, IDataLogger dataLogger)
    {
        var existing = await _repository.GetByNameAsync(model.MenuName ?? "", serverIp);

        if (existing != null)
        {
            if (existing.IsEnabled == 1) return (false, "此資料庫已在管理清單中");

            existing.IsEnabled = 1;
            existing.MenuDesc = model.MenuDesc;
            var detail = $"【{existing.MenuName}】({existing.ServerIP}) 被「重新啟用」並更新描述";
            return await _repository.UpdateAsync(existing, detail, dataLogger)
                ? (true, "已重新啟用並更新描述") : (false, "更新失敗");
        }

        var entity = new Menu
        {
            MenuName = model.MenuName,
            MenuDesc = model.MenuDesc,
            ServerIP = serverIp,
            IsEnabled = 1,
            SortOrder = 1
        };

        var addDetail = $"【{entity.MenuName}】({entity.ServerIP}) 被「新增」描述";
        var result = await _repository.AddAsync(entity, addDetail, dataLogger);
        return result ? (true, "新增成功") : (false, "新增失敗");
    }

    public async Task<(bool success, string message)> ToggleStatusAsync(int menuId, string serverIp, IDataLogger dataLogger)
    {
        var entity = await _repository.GetByIdAsync(menuId);
        if (entity == null || entity.ServerIP != serverIp) return (false, "找不到該筆資料或伺服器不符");

        entity.IsEnabled = entity.IsEnabled == 1 ? 0 : 1;
        var actionText = entity.IsEnabled == 1 ? "啟用" : "關閉";
        var detail = $"【{entity.MenuName}】({entity.ServerIP}) 被「{actionText}」";

        var result = await _repository.UpdateAsync(entity, detail, dataLogger);
        return result ? (true, "狀態已變更") : (false, "更新失敗");
    }

    public async Task<(bool success, string message)> DeleteAsync(int menuId, string serverIp, IDataLogger dataLogger)
    {
        var entity = await _repository.GetByIdAsync(menuId);
        if (entity == null || entity.ServerIP != serverIp) return (false, "找不到該筆資料或伺服器不符");

        var detail = $"【{entity.MenuName}】({entity.ServerIP}) 被「刪除」描述";
        var result = await _repository.DeleteByIdAsync(menuId, detail, dataLogger);
        return result ? (true, "刪除成功") : (false, "刪除失敗");
    }

    public async Task<(bool success, string message)> UpdateDescriptionsAsync(List<DIC1999ViewModel> updates, string serverIp, IDataLogger dataLogger)
    {
        if (updates == null || !updates.Any()) return (false, "無任何異動資料");

        foreach (var item in updates)
        {
            var entity = await _repository.GetByIdAsync(item.MenuId);
            if (entity != null && entity.ServerIP == serverIp)
            {
                entity.MenuDesc = item.MenuDesc;
                var detail = $"【{entity.MenuName}】描述更新為「{entity.MenuDesc ?? ""}」";
                await _repository.UpdateAsync(entity, detail, dataLogger);
            }
        }
        return (true, "儲存成功");
    }

    public async Task<(byte[] content, string fileName)> ExportExcelAsync(int menuId, string serverIp)
    {
        var dbInfo = await _repository.GetDatabaseInfoAsync(menuId, serverIp);
        if (string.IsNullOrEmpty(dbInfo.Name)) return (Array.Empty<byte>(), "");

        var physicalTables = await _repository.GetPhysicalTablesAsync(serverIp, dbInfo.Name);
        var sheetMeta = (await _repository.GetSheetMetadataAsync(menuId, serverIp)).ToDictionary(x => x.TableName, x => x.TableDesc, StringComparer.OrdinalIgnoreCase);
        var rowMeta = await _repository.GetRowMetadataAsync(menuId, serverIp);
        var rowMetaDict = rowMeta.GroupBy(x => x.TableName, StringComparer.OrdinalIgnoreCase)
                                 .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.ColumnName, x => x.RowDesc, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("資料庫描述");
        ws.Cell("A1").Value = "資料庫代號"; ws.Cell("B1").Value = dbInfo.Name;
        ws.Cell("A2").Value = "資料庫描述"; ws.Cell("B2").Value = dbInfo.Desc;
        var headers = new[] { "資料表名稱", "資料表描述", "欄位名稱", "欄位型態", "長度", "Null", "欄位描述" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];

        int rowIdx = 4;
        foreach (var tableName in physicalTables)
        {
            var columns = await _repository.GetPhysicalSchemaAsync(serverIp, dbInfo.Name, tableName);
            var tableDesc = sheetMeta.GetValueOrDefault(tableName, "");
            foreach (var col in columns)
            {
                ws.Cell(rowIdx, 1).Value = tableName;
                ws.Cell(rowIdx, 2).Value = tableDesc;
                ws.Cell(rowIdx, 3).Value = col.ColumnName;
                ws.Cell(rowIdx, 4).Value = col.DataType;
                ws.Cell(rowIdx, 5).Value = col.MaxLength?.ToString() ?? "";
                ws.Cell(rowIdx, 6).Value = col.IsNullable ? "YES" : "NO";
                ws.Cell(rowIdx, 7).Value = rowMetaDict.GetValueOrDefault(tableName)?.GetValueOrDefault(col.ColumnName, "") ?? "";
                rowIdx++;
            }
        }
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return (ms.ToArray(), $"{dbInfo.Name}.xlsx");
    }

    public async Task<(byte[] content, string fileName)> ExportJsonAsync(int menuId, string serverIp)
    {
        var dbInfo = await _repository.GetDatabaseInfoAsync(menuId, serverIp);
        if (string.IsNullOrEmpty(dbInfo.Name)) return (Array.Empty<byte>(), "");

        var physicalTables = await _repository.GetPhysicalTablesAsync(serverIp, dbInfo.Name);
        var sheetMeta = (await _repository.GetSheetMetadataAsync(menuId, serverIp)).ToDictionary(x => x.TableName, x => x.TableDesc, StringComparer.OrdinalIgnoreCase);
        var rowMeta = await _repository.GetRowMetadataAsync(menuId, serverIp);
        var rowMetaDict = rowMeta.GroupBy(x => x.TableName, StringComparer.OrdinalIgnoreCase)
                                 .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.ColumnName, x => x.RowDesc, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

        var jsonResult = new Dictionary<string, DIC1999R01ImportTableData>(StringComparer.OrdinalIgnoreCase);
        foreach (var tableName in physicalTables)
        {
            var columns = await _repository.GetPhysicalSchemaAsync(serverIp, dbInfo.Name, tableName);
            var tableDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var col in columns)
            {
                var desc = rowMetaDict.GetValueOrDefault(tableName)?.GetValueOrDefault(col.ColumnName, "");
                tableDict[col.ColumnName] = string.IsNullOrEmpty(desc) ? col.ColumnName : desc;
            }

            var tableDesc = sheetMeta.GetValueOrDefault(tableName, "");

            jsonResult[tableName] = new DIC1999R01ImportTableData
            {
                SheetDesc = string.IsNullOrEmpty(tableDesc) ? tableName : tableDesc,
                Columns = tableDict
            };
        }

        var jsonString = JsonSerializer.Serialize(jsonResult, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        return (Encoding.UTF8.GetBytes(jsonString), $"{dbInfo.Desc}({dbInfo.Name}).json");
    }
    public async Task<(byte[] content, string fileName)> ExportWordAsync(int menuId, string serverIp)
    {
        var dbInfo = await _repository.GetDatabaseInfoAsync(menuId, serverIp);
        if (string.IsNullOrEmpty(dbInfo.Name)) return (Array.Empty<byte>(), "");

        var sheetMeta = (await _repository.GetSheetMetadataAsync(menuId, serverIp)).ToDictionary(x => x.TableName, x => x.TableDesc, StringComparer.OrdinalIgnoreCase);
        var rowMeta = await _repository.GetRowMetadataAsync(menuId, serverIp);
        var rowMetaDict = rowMeta.GroupBy(x => x.TableName, StringComparer.OrdinalIgnoreCase)
                                 .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.ColumnName, x => x.RowDesc, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

        var schema = await _repository.GetFullSchemaForWordExportAsync(serverIp, dbInfo.Name);
        var tableGroups = schema.GroupBy(x => x.TableName, StringComparer.OrdinalIgnoreCase)
                                 .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                                 .ToList();

        var doc = new EHRIS.Tools.Office.Word.WordGenerater();

        string? currentSection = null;

        foreach (var tableGroup in tableGroups)
        {
            var tableName = tableGroup.Key;
            var firstLetter = tableName.Substring(0, 1).ToUpperInvariant();

            if (firstLetter != currentSection)
            {
                currentSection = firstLetter;
                doc.AddHeading(currentSection, 1);
            }

            var tableDesc = sheetMeta.GetValueOrDefault(tableName, "");

            var headerColumns = new List<EHRIS.Tools.Office.Package.OpenXmlWordHelper.WordTableColumn>
        {
            new() { Header = "序號", WidthDxa = 700 },
            new() { Header = "欄位(中文)", WidthDxa = 2600 },
            new() { Header = "欄位(英文)", WidthDxa = 2200 },
            new() { Header = "型態", WidthDxa = 1300 },
            new() { Header = "長度", WidthDxa = 700 },
            new() { Header = "Null", WidthDxa = 700 },
            new() { Header = "鍵值", WidthDxa = 700 }
        };

            var rows = new List<List<EHRIS.Tools.Office.Package.OpenXmlWordHelper.WordTableCellSpec>>
        {
            new()
            {
                new() { Text = "資料表名稱", ColSpan = 2, Bold = true },
                new() { Text = $"{tableDesc}【{tableName}】", ColSpan = 5, Bold = true }
            },
            new()
            {
                new() { Text = "序號" },
                new() { Text = "欄位(中文)" },
                new() { Text = "欄位(英文)" },
                new() { Text = "型態" },
                new() { Text = "長度" },
                new() { Text = "Null" },
                new() { Text = "鍵值" }
            }
        };

            int seq = 1;
            var colDescDict = rowMetaDict.GetValueOrDefault(tableName);

            foreach (var col in tableGroup)
            {
                rows.Add(new List<EHRIS.Tools.Office.Package.OpenXmlWordHelper.WordTableCellSpec>
            {
                new() { Text = seq.ToString() },
                new() { Text = colDescDict?.GetValueOrDefault(col.ColumnName, "") ?? "" },
                new() { Text = col.ColumnName },
                new() { Text = col.DataType },
                new() { Text = col.Length?.ToString() ?? "" },
                new() { Text = col.IsNullable ? "YES" : "No" },
                new() { Text = col.KeyType }
            });
                seq++;
            }

            doc.AddTable(headerColumns, rows);
            doc.AddParagraph("");
        }

        var (stream, contentType, extension) = doc.Export();
        return (stream.ToArray(), $"{dbInfo.Name}.{extension}");
    }
}