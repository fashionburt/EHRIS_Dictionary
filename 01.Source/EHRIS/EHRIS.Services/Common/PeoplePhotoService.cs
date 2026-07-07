using EHRIS.Core.Repositories;
using Microsoft.Extensions.Configuration;
using SkiaSharp;

namespace EHRIS.Services.Common
{
    public class PeoplePhotoService : IPeoplePhotoService
    {
        private const int DefaultMinWidth = 413;
        private const int DefaultMinHeight = 531;
        private const int DefaultMaxSizeMB = 2;

        private readonly string _uploadPath;
        private readonly IArgumentsRepository _argumentsRepo;

        public PeoplePhotoService(IConfiguration configuration, IArgumentsRepository argumentsRepo)
        {
            _uploadPath = configuration["UploadSettings:PeoplePhotoPath"]
                ?? configuration["UploadSettings:UploadPath"]
                ?? throw new InvalidOperationException("appsettings.json 未設定 UploadSettings:PeoplePhotoPath");

            _argumentsRepo = argumentsRepo;
        }

        /// <summary>
        /// 從參數 CMP_Photo_PixelDim 取得最小寬高（SPLITTEXT，第一個=Width，第二個=Height，例如 413:531）
        /// </summary>
        private async Task<(int MinWidth, int MinHeight)> GetMinPixelDimAsync()
        {
            var list = await _argumentsRepo.GetArgumentListAsync("CMP_Photo_PixelDim");
            var minWidth = list.Count > 0 && int.TryParse(list[0], out var w) ? w : DefaultMinWidth;
            var minHeight = list.Count > 1 && int.TryParse(list[1], out var h) ? h : DefaultMinHeight;
            return (minWidth, minHeight);
        }

        /// <summary>
        /// 從參數 CMP_Photo_MaxSize 取得檔案大小上限（單位 MB）
        /// </summary>
        private async Task<long> GetMaxSizeBytesAsync()
        {
            var value = await _argumentsRepo.GetArgumentAsync("CMP_Photo_MaxSize");
            var maxSizeMB = int.TryParse(value, out var mb) && mb > 0 ? mb : DefaultMaxSizeMB;
            return maxSizeMB * 1024L * 1024L;
        }

        private string GetUploadDirectory(string userAccount)
        {
            var dateDir = DateTime.Now.ToString("yyyyMMdd");
            var dir = Path.Combine(_uploadPath, dateDir, userAccount);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        private async Task<HashSet<string>> GetAllowedExtensionsAsync()
        {
            var list = await _argumentsRepo.GetArgumentListAsync("CMP_Photo_Extension");
            // DB 值可能是 "gif,jpg,jpeg,png,bmp" 或 "gif jpg jpeg png bmp" 等格式
            // 統一加上 . 前綴，確保比對一致
            var extensions = list
                .SelectMany(v => v.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Select(e => e.StartsWith('.') ? e : "." + e)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return extensions.Count > 0 ? extensions : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".gif", ".jpg", ".jpeg", ".png", ".bmp" };
        }

        public async Task<PhotoUploadResult> UploadOriginalAsync(Stream fileStream, string fileName,
            string userAccount)
        {
            var allowedExtensions = await GetAllowedExtensionsAsync();
            var ext = Path.GetExtension(fileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                return new PhotoUploadResult { Success = false, Message = $"只允許 {string.Join(", ", allowedExtensions.Select(e => e.TrimStart('.').ToUpper()))} 格式的照片" };

            var maxSizeBytes = await GetMaxSizeBytesAsync();
            if (fileStream.Length > maxSizeBytes)
                return new PhotoUploadResult { Success = false, Message = $"檔案大小不可超過 {maxSizeBytes / 1024 / 1024} MB" };

            var uploadDir = GetUploadDirectory(userAccount);
            var timestamp = DateTime.Now.ToString("yyyyMMddhhmmss");
            var prefix = timestamp + "_" + userAccount;
            var upFileName = prefix + "_up" + ext;
            var upFilePath = Path.Combine(uploadDir, upFileName);

            // 儲存原圖
            using (var fs = new FileStream(upFilePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }

            // 檢查像素
            using var bitmap = SKBitmap.Decode(upFilePath);
            if (bitmap == null)
            {
                File.Delete(upFilePath);
                return new PhotoUploadResult { Success = false, Message = "無法讀取圖片檔案" };
            }

            var (minWidth, minHeight) = await GetMinPixelDimAsync();
            if (bitmap.Width < minWidth || bitmap.Height < minHeight)
            {
                File.Delete(upFilePath);
                return new PhotoUploadResult
                {
                    Success = false,
                    Message = $"照片解析度不足，寬度至少需達 {minWidth} 像素（目前 {bitmap.Width}），高度至少需達 {minHeight} 像素（目前 {bitmap.Height}）"
                };
            }

            return new PhotoUploadResult
            {
                Success = true,
                TempFileName = upFileName,
                Width = bitmap.Width,
                Height = bitmap.Height
            };
        }

        public Task<PhotoCropResult> CropAndProcessAsync(string tempFileName, CropArea cropArea,
            string watermarkText, string userAccount)
        {
            var uploadDir = GetUploadDirectory(userAccount);
            var upFilePath = Path.Combine(uploadDir, tempFileName);

            if (!File.Exists(upFilePath))
                return Task.FromResult(new PhotoCropResult { Success = false, Message = "找不到暫存原圖" });

            var prefix = Path.GetFileNameWithoutExtension(tempFileName).Replace("_up", "");
            var ext = Path.GetExtension(tempFileName);

            using var original = SKBitmap.Decode(upFilePath);
            if (original == null)
                return Task.FromResult(new PhotoCropResult { Success = false, Message = "無法讀取原圖" });

            // 1. 裁切
            var cropRect = new SKRectI(
                (int)cropArea.X,
                (int)cropArea.Y,
                (int)(cropArea.X + cropArea.Width),
                (int)(cropArea.Y + cropArea.Height)
            );

            // 確保不超出邊界
            cropRect.Left = Math.Max(0, cropRect.Left);
            cropRect.Top = Math.Max(0, cropRect.Top);
            cropRect.Right = Math.Min(original.Width, cropRect.Right);
            cropRect.Bottom = Math.Min(original.Height, cropRect.Bottom);

            using var cropped = new SKBitmap(cropRect.Width, cropRect.Height);
            using (var canvas = new SKCanvas(cropped))
            {
                canvas.DrawBitmap(original, cropRect, new SKRect(0, 0, cropRect.Width, cropRect.Height));
            }

            // 2. 儲存裁切結果（保留原始解析度）
            var thFileName = prefix + "_IMG" + ext;
            var thFilePath = Path.Combine(uploadDir, thFileName);
            SaveBitmap(cropped, thFilePath, ext);

            // 3. 浮水印 (_wm)
            var wmFileName = "";
            if (!string.IsNullOrWhiteSpace(watermarkText))
            {
                using var wmBitmap = cropped.Copy();
                AddWatermark(wmBitmap, watermarkText);
                wmFileName = prefix + "_WM" + ext;
                var wmFilePath = Path.Combine(uploadDir, wmFileName);
                SaveBitmap(wmBitmap, wmFilePath, ext);
            }

            // 4. 刪除原圖
            File.Delete(upFilePath);

            return Task.FromResult(new PhotoCropResult
            {
                Success = true,
                ThumbnailFileName = thFileName,
                WatermarkFileName = wmFileName
            });
        }

        public string GetPhotoPath(string fileName)
        {
            // 搜尋所有日期子目錄
            if (!Directory.Exists(_uploadPath)) return "";

            foreach (var dir in Directory.GetDirectories(_uploadPath, "*", SearchOption.AllDirectories))
            {
                var filePath = Path.Combine(dir, fileName);
                if (File.Exists(filePath)) return filePath;
            }
            return "";
        }

        private static void SaveBitmap(SKBitmap bitmap, string filePath, string ext)
        {
            // BMP/GIF 格式 SkiaSharp 不一定支援 Encode，統一存為 PNG 或 JPEG
            var format = ext.ToLower() switch
            {
                ".png" => SKEncodedImageFormat.Png,
                ".gif" => SKEncodedImageFormat.Png,  // GIF 轉 PNG 避免 Encode 失敗
                ".bmp" => SKEncodedImageFormat.Png,  // BMP 轉 PNG 避免 Encode 失敗
                _ => SKEncodedImageFormat.Jpeg
            };

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(format, 90);
            if (data == null)
                throw new InvalidOperationException($"無法編碼圖片格式：{ext}");
            using var fs = new FileStream(filePath, FileMode.Create);
            data.SaveTo(fs);
        }

        private static void AddWatermark(SKBitmap bitmap, string text)
        {
            using var canvas = new SKCanvas(bitmap);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Center
            };

            // 使用標楷體，若不存在則 fallback
            paint.Typeface = SKTypeface.FromFamilyName("DFKai-SB", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                ?? SKTypeface.FromFamilyName("Microsoft JhengHei", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                ?? SKTypeface.Default;

            // 計算對角線長度，字型大小約為對角線的 1/4（讓文字明顯）
            var diagonal = (float)Math.Sqrt(bitmap.Width * bitmap.Width + bitmap.Height * bitmap.Height);
            paint.TextSize = diagonal / (text.Length + 2);

            // 確保文字不會太大或太小
            paint.TextSize = Math.Clamp(paint.TextSize, 16f, diagonal * 0.15f);

            // 旋轉角度：左下到右上 -45 度
            float cx = bitmap.Width / 2f;
            float cy = bitmap.Height / 2f;

            canvas.Save();
            canvas.RotateDegrees(-45, cx, cy);

            // 陰影層（黑色半透明）
            paint.Color = new SKColor(0, 0, 0, 80);
            canvas.DrawText(text, cx + 2, cy + 2, paint);

            // 文字層（白色半透明）
            paint.Color = new SKColor(255, 255, 255, 120);
            canvas.DrawText(text, cx, cy, paint);

            canvas.Restore();
        }
    }
}
