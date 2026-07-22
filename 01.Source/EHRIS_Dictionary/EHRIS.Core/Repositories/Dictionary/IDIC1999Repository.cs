using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;
using System.Linq.Expressions;

namespace EHRIS.Core.Repositories.Dictionary;

public interface IDIC1999Repository
{
    Task<DataTableResponse<DIC1999ViewModel>> GetPagedListAsync(DataTableRequest request, string serverIp, string clientIp);

    Task<List<string>> GetSystemDatabaseNamesAsync(string serverIp);

    Task<(string Name, string Desc)> GetDatabaseInfoAsync(int menuId, string serverIp);
    Task<List<(string TableName, string TableDesc)>> GetSheetMetadataAsync(int menuId, string serverIp);
    Task<List<(string TableName, string ColumnName, string RowDesc)>> GetRowMetadataAsync(int menuId, string serverIp);

    Task<List<MergedColumnInfo>> GetPhysicalSchemaAsync(string serverIp, string targetDbName, string tableName);
    Task<List<string>> GetPhysicalTablesAsync(string serverIp, string targetDbName);

    Task<bool> AnyAsync(Expression<Func<Menu, bool>> predicate);
    Task<Menu?> GetByIdAsync(int id);
    Task<Menu?> GetByNameAsync(string name, string serverIp);
    Task<bool> AddAsync(Menu entity, string detail, IDataLogger dataLogger);
    Task<bool> UpdateAsync(Menu entity, string detail, IDataLogger dataLogger);
    Task<bool> DeleteByIdAsync(int id, string detail, IDataLogger dataLogger);
    Task<List<(string TableName, string ColumnName, string DataType, int? Length, bool IsNullable, string KeyType)>> GetFullSchemaForWordExportAsync(string serverIp, string targetDbName);

}