namespace EHRIS.Services.Common
{
    public class PhotoUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        /// <summary>原圖暫存檔名 (_up)</summary>
        public string TempFileName { get; set; } = "";
        /// <summary>原圖寬度</summary>
        public int Width { get; set; }
        /// <summary>原圖高度</summary>
        public int Height { get; set; }
    }

    public class PhotoCropResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        /// <summary>縮圖檔名 (_th)</summary>
        public string ThumbnailFileName { get; set; } = "";
        /// <summary>浮水印檔名 (_wm)</summary>
        public string WatermarkFileName { get; set; } = "";
    }

    public class CropArea
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public interface IPeoplePhotoService
    {
        /// <summary>上傳原圖，檢查像素，儲存 _up 檔</summary>
        Task<PhotoUploadResult> UploadOriginalAsync(Stream fileStream, string fileName,
            string userAccount);

        /// <summary>根據裁切區域產生縮圖 (_th) 和浮水印 (_wm)，刪除原圖 (_up)</summary>
        Task<PhotoCropResult> CropAndProcessAsync(string tempFileName, CropArea cropArea,
            string watermarkText, string userAccount);

        /// <summary>取得照片的實體路徑</summary>
        string GetPhotoPath(string fileName);
    }
}
