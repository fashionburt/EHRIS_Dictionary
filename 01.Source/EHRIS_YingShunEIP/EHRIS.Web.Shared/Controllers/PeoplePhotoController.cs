using EHRIS.Security.User;
using EHRIS.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class PeoplePhotoController : Controller
    {
        private readonly IPeoplePhotoService _photoService;
        private readonly IUserContextService _userContext;

        public PeoplePhotoController(IPeoplePhotoService photoService, IUserContextService userContext)
        {
            _photoService = photoService;
            _userContext = userContext;
        }

        /// <summary>上傳原圖，回傳暫存檔名 + 圖片預覽用 base64</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "請選擇照片" });

            var userAccount = _userContext.UserAccount ?? "unknown";
            var result = await _photoService.UploadOriginalAsync(
                file.OpenReadStream(), file.FileName, userAccount);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            // 回傳預覽用的圖片 URL
            var previewUrl = Url.Action("Preview", "PeoplePhoto", new { fileName = result.TempFileName });

            return Json(new
            {
                success = true,
                tempFileName = result.TempFileName,
                width = result.Width,
                height = result.Height,
                previewUrl
            });
        }

        /// <summary>預覽暫存原圖</summary>
        [HttpGet]
        public IActionResult Preview(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return NotFound();

            var filePath = _photoService.GetPhotoPath(fileName);
            if (string.IsNullOrEmpty(filePath))
                return NotFound();

            var ext = Path.GetExtension(fileName).ToLower();
            var contentType = ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "image/jpeg"
            };

            return PhysicalFile(filePath, contentType);
        }

        /// <summary>確認裁切，產生縮圖 + 浮水印，刪除原圖</summary>
        [HttpPost]
        public async Task<IActionResult> Crop([FromBody] CropRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.TempFileName))
                return Json(new { success = false, message = "缺少暫存檔名" });

            var userAccount = _userContext.UserAccount ?? "unknown";
            var watermarkText = request.WatermarkText ?? "";

            var result = await _photoService.CropAndProcessAsync(
                request.TempFileName,
                new CropArea
                {
                    X = request.X,
                    Y = request.Y,
                    Width = request.Width,
                    Height = request.Height
                },
                watermarkText,
                userAccount);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            // 回傳縮圖預覽
            var thumbnailUrl = Url.Action("Preview", "PeoplePhoto", new { fileName = result.ThumbnailFileName });
            var watermarkUrl = string.IsNullOrEmpty(result.WatermarkFileName)
                ? ""
                : Url.Action("Preview", "PeoplePhoto", new { fileName = result.WatermarkFileName });

            return Json(new
            {
                success = true,
                thumbnailFileName = result.ThumbnailFileName,
                watermarkFileName = result.WatermarkFileName,
                thumbnailUrl,
                watermarkUrl
            });
        }

        public class CropRequest
        {
            public string? TempFileName { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public string? WatermarkText { get; set; }
        }
    }
}
