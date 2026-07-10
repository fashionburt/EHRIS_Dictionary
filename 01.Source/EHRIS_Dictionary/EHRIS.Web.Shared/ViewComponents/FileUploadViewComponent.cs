using EHRIS.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace EHRIS.Web.Shared.ViewComponents
{
    public class FileUploadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(FileUploadModel model)
        {
            return View(model);
        }
    }
}
