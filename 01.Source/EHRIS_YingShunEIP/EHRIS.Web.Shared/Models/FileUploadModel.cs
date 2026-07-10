namespace EHRIS.Web.Shared.Models
{
    /// <summary>
    /// 上傳元件的設定模型
    /// </summary>
    public class FileUploadModel
    {
        /// <summary>元件的唯一識別碼，一頁多個上傳元件時用來區分</summary>
        public string ComponentId { get; set; } = "fileUpload";

        /// <summary>檔案大小上限 (MB)，預設 10</summary>
        public int MaxSizeMB { get; set; } = 10;

        /// <summary>是否允許多檔上傳，預設 false</summary>
        public bool Multiple { get; set; } = false;

        /// <summary>初始載入的 fil_id 清單 (編輯表單回顯用)，逗號分隔的 GUID 字串</summary>
        public string? FilIds { get; set; }

        /// <summary>hidden input 的 name 屬性，表單送出時可直接讀取</summary>
        public string FieldName { get; set; } = "FileIds";

        /// <summary>允許的副檔名 (含點號)，null 表示使用預設清單</summary>
        public List<string>? AllowedExtensions { get; set; }

        /// <summary>取得允許的副檔名 (accept 屬性用)</summary>
        public string GetAcceptString()
        {
            var list = AllowedExtensions ?? DefaultAllowedExtensions;
            return string.Join(",", list);
        }

        /// <summary>取得允許的副檔名清單</summary>
        public List<string> GetAllowedExtensions() => AllowedExtensions ?? DefaultAllowedExtensions;

        public long MaxSizeBytes => (long)MaxSizeMB * 1024 * 1024;

        public static readonly List<string> DefaultAllowedExtensions = new()
        {
            ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff", ".jfif",
            ".svgz", ".webp", ".ico", ".apng", ".avif",
            ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".pdf", ".txt", ".rar", ".zip", ".7z",
            ".odp", ".ods", ".odt", ".xps", ".csv"
        };

        public static readonly List<string> BlockedExtensions = new()
        {
            ".jsp", ".jspx", ".war", ".tar",
            ".php", ".php3", ".php4", ".php5", ".phtml",
            ".asp", ".aspx", ".asa", ".ashx", ".ascx",
            ".js", ".exe", ".dll", ".com", ".bat", ".cmd"
        };
    }

    public class UploadedFileInfo
    {
        /// <summary>原始檔名</summary>
        public string OriginalName { get; set; } = "";

        /// <summary>伺服器端儲存的檔名</summary>
        public string StoredName { get; set; } = "";

        /// <summary>檔案大小 (bytes)</summary>
        public long FileSize { get; set; }

        /// <summary>上傳時間</summary>
        public DateTime UploadTime { get; set; }

        /// <summary>格式化檔案大小</summary>
        public string FileSizeDisplay
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} B";
                if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F1} KB";
                return $"{FileSize / (1024.0 * 1024.0):F2} MB";
            }
        }
    }
}
