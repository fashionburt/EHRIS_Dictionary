using EHRIS.Services.Common;
using EHRIS.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.ViewComponents
{
    public class PeoplePhotoViewComponent : ViewComponent
    {
        private readonly ICommonService _commonService;

        public PeoplePhotoViewComponent(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public async Task<IViewComponentResult> InvokeAsync(PeoplePhotoModel model)
        {
            var list = await _commonService.GetArgumentListAsync("CMP_Photo_Extension");
            var extensions = list
                .SelectMany(v => v.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Select(e => e.TrimStart('.').ToLower())
                .Distinct()
                .ToList();

            if (extensions.Count == 0)
                extensions = new List<string> { "gif", "jpg", "jpeg", "png", "bmp" };

            model.AllowedExtensions = extensions;

            // 最小寬高從 CMP_Photo_PixelDim (SPLITTEXT) 讀取，第一個=Width，第二個=Height
            var pixelDim = await _commonService.GetArgumentListAsync("CMP_Photo_PixelDim");
            if (pixelDim.Count > 0 && int.TryParse(pixelDim[0], out var w))
                model.MinWidth = w;
            if (pixelDim.Count > 1 && int.TryParse(pixelDim[1], out var h))
                model.MinHeight = h;

            // 只有呼叫端「未指定」(null) 時才從 DB 讀；空字串代表明確不加浮水印，需尊重
            if (model.WatermarkText == null)
                model.WatermarkText = await _commonService.GetArgumentAsync("CMP_Photo_WatermarkText");

            return View(model);
        }
    }
}
