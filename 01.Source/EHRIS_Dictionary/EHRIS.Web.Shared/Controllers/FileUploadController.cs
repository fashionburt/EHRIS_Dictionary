using EHRIS.Security.User;
using EHRIS.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.Controllers
{
    /// <summary>
    /// 共用檔案上傳元件，供所有表單的 FileUpload 元件使用
    /// </summary>
    [Authorize]
    [Route("[controller]/[action]")]
    public class FileUploadController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserContextService _userContext;

        public FileUploadController(IFileUploadService fileUploadService, IUserContextService userContext)
        {
            _fileUploadService = fileUploadService;
            _userContext = userContext;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string componentId)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "未選擇檔案" });

            var filUid = _userContext.PeoUID;
            var userAccount = _userContext.UserAccount ?? "unknown";
            var userName = _userContext.UserName ?? "";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            var result = await _fileUploadService.UploadAsync(
                file.OpenReadStream(), file.FileName, file.Length,
                filUid, userAccount, userName, ipAddress);

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFile([FromBody] RemoveFileRequest request)
        {
            var userName = _userContext.UserName ?? "";
            var success = await _fileUploadService.RemoveAsync(request.StoredName ?? "", userName);
            return Json(new { success });
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            var result = await _fileUploadService.DownloadFileAsync(fileName ?? "");
            if (result == null)
                return NotFound("檔案不存在");

            return File(result.Value.FileBytes!, "application/octet-stream", result.Value.OriginalName);
        }

        [HttpGet]
        public async Task<IActionResult> GetFileList(string? filIds)
        {
            if (string.IsNullOrWhiteSpace(filIds))
                return Json(new List<FileListItem>());

            var idList = filIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s.Trim(), out var id) ? id : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var files = await _fileUploadService.GetFileListByIdsAsync(idList);
            return Json(files);
        }

        public class RemoveFileRequest
        {
            public string? StoredName { get; set; }
            public string? ComponentId { get; set; }
        }
    }
}
