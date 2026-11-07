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
        Task<FileResult> GetImageAsync(string section, string filename, CancellationToken cancellationToken = default);
        Task<FileResult> GetFileAsync(string section, string filename, CancellationToken cancellationToken = default);
    }
}
