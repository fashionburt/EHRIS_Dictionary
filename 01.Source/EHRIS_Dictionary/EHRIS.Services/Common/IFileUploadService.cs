namespace EHRIS.Services.Common
{
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string OriginalName { get; set; } = "";
        public string StoredName { get; set; } = "";
        public long FileSize { get; set; }
        public string UploadTime { get; set; } = "";
        public Guid FilId { get; set; }
    }

    public class FileListItem
    {
        public Guid FilId { get; set; }
        public string OriginalName { get; set; } = "";
        public string StoredName { get; set; } = "";
        public long FileSize { get; set; }
        public string UploadTime { get; set; } = "";
    }

    public interface IFileUploadService
    {
        /// <summary>上傳檔案並寫入 filedata</summary>
        Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, long fileLength,
            int filUid, string userAccount, string userName, string ipAddress);

        /// <summary>刪除檔案 (status 改 2)</summary>
        Task<bool> RemoveAsync(string storedName, string userName);

        /// <summary>取得檔案供下載</summary>
        Task<(byte[]? FileBytes, string OriginalName)?> DownloadFileAsync(string storedName);

        /// <summary>依 filUid 取得已上傳的檔案清單</summary>
        Task<List<FileListItem>> GetFileListAsync(int filUid);

        /// <summary>依 fil_id 清單取得已上傳的檔案</summary>
        Task<List<FileListItem>> GetFileListByIdsAsync(List<Guid> filIds);
    }
}
