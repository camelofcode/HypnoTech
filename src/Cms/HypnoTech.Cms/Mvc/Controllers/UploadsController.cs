using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Mvc.Controllers
{
    public class UploadsController : Controller
    {
        private readonly IUploadedFilesService _service;
        private readonly ICmsImageUploadService _imageUploadService;

        public UploadsController(IUploadedFilesService service, ICmsImageUploadService imageUploadService, IConfigAdminRepository config)
        {
            _service = service;
            _imageUploadService = imageUploadService;
        }

        [Route("/Images/{section}/{*file}")]
        public async Task<IActionResult> Images(string section, string file)
        {
            var result = await _service.GetImageAsync(section, file, HttpContext.RequestAborted);
            return result == null ? NotFound() : result;
        }

        [Route("/Files/{section}/{*file}")]
        public async Task<IActionResult> Files(string section, string file)
        {
            var result = await _service.GetFileAsync(section, file, HttpContext.RequestAborted);
            return result == null ? NotFound() : result;
        }

        public async Task<IActionResult> UploadRichTextImage(List<IFormFile> files)
        {
            var file = files[0];
            if (!new[] {"image/png", "image/jpeg"}.Contains(file.ContentType))
            {
                return Ok(new {error = "Image must be a .png or .jpg"});
            }
            var url = await _imageUploadService.AddImageAsync(
                file,
                "RichTextImages",
                800,
                800,
                HttpContext.RequestAborted);
            return Ok(new { url } );
        }
    }
}
