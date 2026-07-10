using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Common
{
    public interface IFileExportService
    {
        IActionResult CreateDownloadFile(byte[] fileBytes, string fileExtension, string baseFileName = "統計表");
    }
}
