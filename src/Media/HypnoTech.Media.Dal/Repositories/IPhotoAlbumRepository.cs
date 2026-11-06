using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IPhotoAlbumRepository
    {
        ICollection<PhotoAlbum> GetPhotoAlbums();
        Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default);
        PhotoAlbum GetPhotoAlbum(string url);
        Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default);
    }
}
