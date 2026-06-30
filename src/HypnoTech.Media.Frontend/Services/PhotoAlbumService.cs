using System.Collections.Generic;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IPhotoAlbumService
    {
        ICollection<PhotoAlbum> GetPhotoAlbums();
        PhotoAlbum GetPhotoAlbum(string url);
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

        public PhotoAlbum GetPhotoAlbum(string url)
        {
            return _repository.GetPhotoAlbum(url);
        }
    }
}
