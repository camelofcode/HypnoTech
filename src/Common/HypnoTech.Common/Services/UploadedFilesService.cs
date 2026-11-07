using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HypnoTech.Common.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Services
{
    public class UploadedFilesService : IUploadedFilesService
    {
        private readonly string _basePath;
        private readonly ILogger<UploadedFilesService> _logger;

        public UploadedFilesService(IConfiguration configuration, ILogger<UploadedFilesService> logger)
        {
            _basePath = configuration.UploadPath();
            _logger = logger;
        }

        public Task<FileResult> GetImageAsync(string section, string filename, CancellationToken cancellationToken = default)
        {
            return GetFileResultAsync("Images", section, filename, false, cancellationToken);
        }

        public Task<FileResult> GetFileAsync(string section, string filename, CancellationToken cancellationToken = default)
        {
            return GetFileResultAsync("Files", section, filename, true, cancellationToken);
        }

        private async Task<FileResult> GetFileResultAsync(
            string category,
            string section,
            string filename,
            bool enableRangeProcessing,
            CancellationToken cancellationToken)
        {
            var path = Path.GetFullPath(Path.Combine(_basePath, category, section, filename));

            try
            {
                var file = await File.ReadAllBytesAsync(path, cancellationToken);
                var contentType = GetContentType(filename);
                return new FileContentResult(file, contentType)
                {
                    FileDownloadName = filename,
                    EnableRangeProcessing = enableRangeProcessing &&
                                            (contentType.StartsWith("audio") || contentType.StartsWith("video"))
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception,
                    "Could not serve uploaded {Category} file from {Path}. UploadPath resolved to {UploadPath}.",
                    category, path, _basePath);
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
