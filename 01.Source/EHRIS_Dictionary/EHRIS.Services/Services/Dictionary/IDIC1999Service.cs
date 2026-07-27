using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Dictionary;
using EHRIS.Core.Models.Event;

namespace EHRIS.Services.Dictionary;

public interface IDIC1999Service
{
    Task<DataTableResponse<DIC1999ViewModel>> GetDataTableAsync(DataTableRequest request, string serverIp, string clientIp);
    Task<List<string>> GetAllDatabaseNamesAsync(string serverIp);
    Task<(bool success, string message)> AddDescriptionAsync(DIC1999ViewModel model, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> UpdateDescriptionsAsync(List<DIC1999ViewModel> updates, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> DeleteAsync(int menuId, string serverIp, IDataLogger dataLogger);
    Task<(bool success, string message)> ToggleStatusAsync(int menuId, string serverIp, IDataLogger dataLogger);
    Task<(byte[] content, string fileName)> ExportExcelAsync(int menuId, string serverIp);
    Task<(byte[] content, string fileName)> ExportJsonAsync(int menuId, string serverIp);
    Task<(byte[] content, string fileName)> ExportWordAsync(int menuId, string serverIp, List<string>? selectedTables = null);

    Task<List<(string TableName, string TableDesc)>> GetTableListAsync(int menuId, string serverIp);
}