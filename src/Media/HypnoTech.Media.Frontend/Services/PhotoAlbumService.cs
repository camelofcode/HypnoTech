using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IPhotoAlbumService
    {
        ICollection<PhotoAlbum> GetPhotoAlbums();
        Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default);
        PhotoAlbum GetPhotoAlbum(string url);
        Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default);
    }

    public class PhotoAlbumService : IPhotoAlbumService
    {
        private readonly IPhotoAlbumRepository _repository;

        public PhotoAlbumService(IPhotoAlbumRepository repository)
        {
            _repository = repository;
        }

        public ICollection<PhotoAlbum> GetPhotoAlbums()
        {
            return _repository.GetPhotoAlbums();
        }

        public Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetPhotoAlbumsAsync(cancellationToken);
        }

        public PhotoAlbum GetPhotoAlbum(string url)
        {
            return _repository.GetPhotoAlbum(url);
        }

        public Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default)
        {
            return _repository.GetPhotoAlbumAsync(url, cancellationToken);
        }
    }
}
