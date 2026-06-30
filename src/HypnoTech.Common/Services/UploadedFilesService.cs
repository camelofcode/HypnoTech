using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using HypnoTech.Common.Extensions;

namespace HypnoTech.Common.Services
{
    public class UploadedFilesService : IUploadedFilesService
    {
        private readonly string _basePath;

        public UploadedFilesService(IConfiguration configuration)
        {
            _basePath = configuration.UploadPath();
        }

        public FileResult GetImage(string section, string filename)
        {
            try
            {
                var file = File.ReadAllBytes(Path.Combine(_basePath, "Images", section, filename));
                return new FileContentResult(file, GetContentType(filename))
                {
                    FileDownloadName = filename
                };
            }
            catch
            {
                //TODO: Ana should we have an image not found?
                return null;
            }
        }

        public FileResult GetFile(string section, string filename)
        {
            try
            {
                var file = File.ReadAllBytes(Path.Combine(_basePath, "Files", section, filename));
                var contentType = GetContentType(filename);
                return new FileContentResult(file, contentType)
                {
                    FileDownloadName = filename,
                    EnableRangeProcessing = contentType.StartsWith("audio") || contentType.StartsWith("video")
                };
            }
            catch
            {
                return null;
            }
        }

        private string GetContentType(string filename)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
