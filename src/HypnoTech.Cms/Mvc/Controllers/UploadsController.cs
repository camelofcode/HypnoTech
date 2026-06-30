using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Cms.Mvc.Controllers
{
    public class UploadsController : Controller
    {
        private readonly IUploadedFilesService _service;
        private readonly ICmsImageUploadService _imageUploadService;
        private readonly string _mainSiteUrl;

        public UploadsController(IUploadedFilesService service, ICmsImageUploadService imageUploadService, IConfigAdminRepository config)
        {
            _service = service;
            _imageUploadService = imageUploadService;
            _mainSiteUrl = config.GetConfig().MainSite.Url;
        }

        [Route("/Images/{section}/{*file}")]
        public IActionResult Images(string section, string file)
        {
            return _service.GetImage(section, file);
        }

        [Route("/Files/{section}/{*file}")]
        public IActionResult Files(string section, string file)
        {
            return _service.GetFile(section, file);
        }

        public IActionResult UploadRichTextImage(List<IFormFile> files)
        {
            var file = files[0];
            if (!new[] {"image/png", "image/jpeg"}.Contains(file.ContentType))
            {
                return Ok(new {error = "Image must be a .png or .jpg"});
            }
            return Ok(new { url =_imageUploadService.AddImage(file, "RichTextImages", 800, 800) } );
        }
    }
}