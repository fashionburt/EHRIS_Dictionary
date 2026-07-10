using EHRIS.Core.Entities;
using EHRIS.Core.Repositories;
using Microsoft.Extensions.Configuration;

namespace EHRIS.Services.Common
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileDataRepository _fileDataRepository;
        private readonly string _uploadPath;

        private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jsp", ".jspx", ".war", ".tar",
            ".php", ".php3", ".php4", ".php5", ".phtml",
            ".asp", ".aspx", ".asa", ".ashx", ".ascx",
            ".js", ".exe", ".dll", ".com", ".bat", ".cmd"
        };

        public FileUploadService(IFileDataRepository fileDataRepository, IConfiguration configuration)
        {
            _fileDataRepository = fileDataRepository;
            _uploadPath = configuration["UploadSettings:UploadPath"]
                ?? throw new InvalidOperationException("appsettings.json 未設定 UploadSettings:UploadPath");
        }

        /// <summary>取得上傳目錄：根目錄/日期/帳號/</summary>
        private string GetUploadDirectory(string userAccount)
        {
            var dateDir = DateTime.Now.ToString("yyyyMMdd");
            var uploadDir = Path.Combine(_uploadPath, dateDir, userAccount);
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);
            return uploadDir;
        }

        /// <summary>從儲存檔名還原原始檔名 (去掉 yyyyMMddHHmmss_ 前綴)</summary>
        private static string GetOriginalName(string storedName)
        {
            var idx = storedName.IndexOf('_');
            return idx >= 0 && idx < storedName.Length - 1
                ? storedName[(idx + 1)..]
                : storedName;
        }

        public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, long fileLength,
            int filUid, string userAccount, string userName, string ipAddress)
        {
            // 副檔名檢查
            var ext = Path.GetExtension(fileName).ToLower();
            if (BlockedExtensions.Contains(ext))
                return new FileUploadResult { Success = false, Message = "不允許的檔案類型" };

            var uploadDir = GetUploadDirectory(userAccount);
            var now = DateTime.Now;

            // 儲存檔名：yyyyMMddHHmmss_原始檔名
            var storedName = now.ToString("yyyyMMddHHmmss_") + fileName;
            var filePath = Path.Combine(uploadDir, storedName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }

            // 寫入 filedata
            var fileData = new FileData
            {
                FilUid = filUid,
                FilDate = now,
                FilDirPath = uploadDir,
                FilFileName = storedName,
                FilIpAddress = ipAddress,
                FilStatus = 1,
                FilCreateName = userName,
                FilCreateTime = now,
                FilModifyName = userName,
                FilModifyTime = now
            };
            await _fileDataRepository.AddFileData(fileData);

            return new FileUploadResult
            {
                Success = true,
                OriginalName = fileName,
                StoredName = storedName,
                FileSize = fileLength,
                UploadTime = now.ToString("yyyy-MM-dd HH:mm:ss"),
                FilId = fileData.FilId
            };
        }

        public async Task<bool> RemoveAsync(string storedName, string userName)
        {
            var fileData = await _fileDataRepository.GetByStoredName(storedName);
            if (fileData == null) return false;

            // 刪除實體檔案
            var filePath = Path.Combine(fileData.FilDirPath, fileData.FilFileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            // 更新 status = 2
            fileData.FilStatus = 2;
            fileData.FilModifyName = userName;
            fileData.FilModifyTime = DateTime.Now;
            await _fileDataRepository.UpdateFileData(fileData);

            return true;
        }

        public async Task<(byte[]? FileBytes, string OriginalName)?> DownloadFileAsync(string storedName)
        {
            var fileData = await _fileDataRepository.GetByStoredName(storedName);
            if (fileData == null) return null;

            var filePath = Path.Combine(fileData.FilDirPath, fileData.FilFileName);
            if (!File.Exists(filePath)) return null;

            var bytes = File.ReadAllBytes(filePath);
            var originalName = GetOriginalName(fileData.FilFileName);
            return (bytes, originalName);
        }

        public async Task<List<FileListItem>> GetFileListAsync(int filUid)
        {
            var files = await _fileDataRepository.GetFileListByUid(filUid);

            return files.Select(f =>
            {
                var filePath = Path.Combine(f.FilDirPath, f.FilFileName);
                long fileSize = 0;
                if (File.Exists(filePath))
                    fileSize = new FileInfo(filePath).Length;

                return new FileListItem
                {
                    FilId = f.FilId,
                    OriginalName = GetOriginalName(f.FilFileName),
                    StoredName = f.FilFileName,
                    FileSize = fileSize,
                    UploadTime = f.FilCreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }).ToList();
        }

        public async Task<List<FileListItem>> GetFileListByIdsAsync(List<Guid> filIds)
        {
            if (filIds == null || filIds.Count == 0) return new List<FileListItem>();

            var files = await _fileDataRepository.GetFileListByIds(filIds);

            return files.Select(f =>
            {
                var filePath = Path.Combine(f.FilDirPath, f.FilFileName);
                long fileSize = 0;
                if (File.Exists(filePath))
                    fileSize = new FileInfo(filePath).Length;

                return new FileListItem
                {
                    FilId = f.FilId,
                    OriginalName = GetOriginalName(f.FilFileName),
                    StoredName = f.FilFileName,
                    FileSize = fileSize,
                    UploadTime = f.FilCreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }).ToList();
        }
    }
}
