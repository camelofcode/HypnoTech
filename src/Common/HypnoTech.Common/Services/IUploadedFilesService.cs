using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Services
{
    public interface IUploadedFilesService
    {
        FileResult GetImage(string section, string filename);
        Task<FileResult> GetImageAsync(string section, string filename, CancellationToken cancellationToken = default);
        FileResult GetFile(string section, string filename);
        Task<FileResult> GetFileAsync(string section, string filename, CancellationToken cancellationToken = default);
    }
}
