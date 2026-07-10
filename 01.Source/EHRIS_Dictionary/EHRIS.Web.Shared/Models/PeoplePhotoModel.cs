namespace EHRIS.Web.Shared.Models
{
    public class PeoplePhotoModel
    {
        /// <summary>元件識別碼</summary>
        public string ComponentId { get; set; } = "peoplePhoto";

        /// <summary>hidden input 的 name（存縮圖檔名）</summary>
        public string FieldName { get; set; } = "PhotoFileName";

        /// <summary>浮水印 hidden input 的 name（存浮水印檔名）</summary>
        public string WatermarkFieldName { get; set; } = "PhotoWatermarkFileName";

        /// <summary>浮水印文字。null = 未指定（由 ViewComponent 從 DB 讀取）；空字串 = 明確不加浮水印；其他值 = 使用該文字</summary>
        public string? WatermarkText { get; set; }

        /// <summary>初始照片檔名（編輯頁回顯用）</summary>
        public string? ExistingFileName { get; set; }

        /// <summary>初始浮水印照片檔名</summary>
        public string? ExistingWatermarkFileName { get; set; }

        /// <summary>允許的副檔名（由 ViewComponent 從 DB 填入，不含點號，如 gif, jpg）</summary>
        public List<string> AllowedExtensions { get; set; } = new();

        /// <summary>最小寬度（像素，由 ViewComponent 從 DB 填入）</summary>
        public int MinWidth { get; set; } = 413;

        /// <summary>最小高度（像素，由 ViewComponent 從 DB 填入）</summary>
        public int MinHeight { get; set; } = 531;
    }
}
