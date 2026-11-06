using Microsoft.AspNetCore.Http;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ICmsImageUploadService
    {
        string AddImage(IFormFile file, string subdirectory, int maxHeight, int maxWidth);
        string RemoveImage(string name, string subdirectory);
    }
}
