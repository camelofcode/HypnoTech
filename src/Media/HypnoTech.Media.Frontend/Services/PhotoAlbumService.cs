using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IPhotoAlbumService
    {
        Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default);
        Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default);
    }

    public class PhotoAlbumService : IPhotoAlbumService
    {
        private readonly IPhotoAlbumRepository _repository;

        public PhotoAlbumService(IPhotoAlbumRepository repository)
        {
            _repository = repository;
        }

        public Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetPhotoAlbumsAsync(cancellationToken);
        }

        public Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default)
        {
            return _repository.GetPhotoAlbumAsync(url, cancellationToken);
        }
    }
}
