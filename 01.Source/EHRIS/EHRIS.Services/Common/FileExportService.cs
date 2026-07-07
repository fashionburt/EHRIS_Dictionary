using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Common
{
    using EHRIS.Tools.Extensions;
    using EHRIS.Tools.Formatter;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;

    public class FileExportService : IFileExportService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileExportService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult CreateDownloadFile(byte[] fileBytes, string fileExtension, string baseFileName = "統計表")
        {
            var rocDate = DateTime.Now.ToString("yyyyMMddhhmmss");
            var fileName = $"{baseFileName}_{rocDate}.{fileExtension}";
            var encodedFileName = Uri.EscapeDataString(fileName);
            var contentType = GetContentType(fileExtension);

            // 設定 Content-Disposition 標頭（支援中文檔名）
            _httpContextAccessor.HttpContext?.Response?.Headers?.Add("Content-Disposition",
                $"attachment; filename=\"download.{fileExtension}\"; filename*=UTF-8''{encodedFileName}");

            return new FileContentResult(fileBytes, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
