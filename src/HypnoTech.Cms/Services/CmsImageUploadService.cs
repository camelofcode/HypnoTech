using System.IO;
using System.Linq;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Helpers;
using HypnoTech.Cms.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HypnoTech.Cms.Services
{
    public class CmsImageUploadService : ICmsImageUploadService
    {
        private readonly string _basePath;

        public CmsImageUploadService(IConfiguration configuration)
        {
            _basePath = configuration.UploadPath();
        }

        public string AddImage(IFormFile file, string subdirectory, int maxHeight, int maxWidth)
        {
            var directory = Path.Combine(_basePath, "Images", subdirectory);
            Directory.CreateDirectory(directory);
            var filePath = FileHelper.GetDeduplicatedPath(Path.Combine(directory, file.FileName));

            if (file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();

                    File.WriteAllBytes(filePath, fileBytes.Scale(maxWidth, maxHeight));
                }
            }

            return $"/Images/{subdirectory}/{file.FileName.Replace("\\", "/")}";
        }

        public string RemoveImage(string name, string subdirectory)
        {
            var path = Path.Combine(_basePath, subdirectory, name);

            return path;
        }
    }
}
