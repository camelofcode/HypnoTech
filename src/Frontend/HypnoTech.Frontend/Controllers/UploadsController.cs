using System.Threading.Tasks;
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
        public async Task<IActionResult> Images(string section, string file)
        {
            var result = await _service.GetImageAsync(section, file, HttpContext.RequestAborted);
            return result == null ? NotFound() : result;
        }

        [ResponseCache(Duration = 31536000)]
        [Route("/Files/{section}/{*file}")]
        public async Task<IActionResult> Files(string section, string file)
        {
            var result = await _service.GetFileAsync(section, file, HttpContext.RequestAborted);
            return result == null ? NotFound() : result;
        }
    }
}
