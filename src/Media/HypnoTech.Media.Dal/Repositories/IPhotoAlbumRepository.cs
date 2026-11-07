using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IPhotoAlbumRepository
    {
        Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default);
        Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default);
    }
}
