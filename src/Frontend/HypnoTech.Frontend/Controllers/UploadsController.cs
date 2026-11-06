using System.Diagnostics;
using HypnoTech.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Frontend.Controllers
{
    public class UploadsController : Controller
    {
        private readonly IUploadedFilesService _service;

        public UploadsController(IUploadedFilesService service)
        {
            _service = service;
        }

        [ResponseCache(Duration = 31536000)]
        [Route("/Images/{section}/{*file}")]
        public IActionResult Images(string section, string file)
        {
            var result = _service.GetImage(section, file);
            return result == null ? NotFound() : result;
        }

        [ResponseCache(Duration = 31536000)]
        [Route("/Files/{section}/{*file}")]
        public IActionResult Files(string section, string file)
        {
            var result = _service.GetFile(section, file);
            return result == null ? NotFound() : result;
        }
    }
}
