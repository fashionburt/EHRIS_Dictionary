using EHRIS.Tools.Crypto;
using EHRIS.Tools.Web;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System.Text;


namespace EHRIS.Core.Repositories.Event;

public class FieldsMappingService
{
    private static Dictionary<string, Dictionary<string, string>> _fieldMappings =
           new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    private static DateTime _lastLoadedTime = DateTime.MinValue;
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(60);
    private static readonly object _lock = new();
    private static readonly string _filePath = PathHelper.ResolvePath("~/Json/FieldsMapping.enc"); 

    private static readonly FileSystemWatcher _watcher;
    private static volatile bool _isLoaded;
    static FieldsMappingService()
    {
        LoadMappings();

        var dir = Path.GetDirectoryName(_filePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        _watcher = new FileSystemWatcher(dir)
        {
            Filter = Path.GetFileName(_filePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };


        _watcher.Changed += (s, e) => LoadMappings();
        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// 取得欄位名稱
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="fieldName">欄名</param>
    /// <returns>欄位說明</returns>
    public string GetDisplayName(string tableName, string fieldName)
    {
        // 檢查快取是否過期
        if (DateTime.UtcNow - _lastLoadedTime > _cacheDuration)
        {
            LoadMappings();
        }

        lock (_lock)
        {
            Dictionary<string, string> fields;
            string displayName;
            if (_fieldMappings.TryGetValue(tableName, out fields) &&
                fields.TryGetValue(fieldName, out displayName))
            {
                return displayName;
            }
        }

        return tableName + "." + fieldName;

    }

    private static void LoadMappings()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _fieldMappings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                    return;
                }

                // 讀取加密檔
                var encrypted = File.ReadAllText(_filePath, Encoding.UTF8);

                // 從環境變數取得 MasterKey
                var masterKey = Environment.GetEnvironmentVariable("EHRISKey");
                if (string.IsNullOrWhiteSpace(masterKey))
                {
                    throw new InvalidOperationException("未設定環境變數 EHRISKey，無法解密 FieldsMapping.enc");
                }

                // 可選的 AAD（例如專案名稱）
                var aad = "EHRIS";// Environment.GetEnvironmentVariable("EHRISAAD");


                // 解密成 JSON
                string json;
                if (!string.IsNullOrWhiteSpace(aad))
                    json = SecureEncryptor.Decrypt(encrypted, masterKey, aad);
                else
                    json = SecureEncryptor.Decrypt(encrypted, masterKey);

                // 反序列化
                var mappings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

                if (mappings != null)
                {
                    _fieldMappings = mappings;
                    _lastLoadedTime = DateTime.UtcNow;
                    _isLoaded = true;
                }

            }
            catch (Exception ex)
            {
                // TODO: log error
                // Console.Error.WriteLine("[FieldsMappingService] Failed to load mappings: " + ex.Message);
                Console.Error.WriteLine("[FieldsMappingService] Failed to load mappings: " + ex.Message);
                _isLoaded = false;

            }
        }
    }
     

}
