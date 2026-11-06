using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ICmsImageUploadService
    {
        string AddImage(IFormFile file, string subdirectory, int maxHeight, int maxWidth);
        Task<string> AddImageAsync(IFormFile file, string subdirectory, int maxHeight, int maxWidth, CancellationToken cancellationToken = default);
        string RemoveImage(string name, string subdirectory);
    }
}
