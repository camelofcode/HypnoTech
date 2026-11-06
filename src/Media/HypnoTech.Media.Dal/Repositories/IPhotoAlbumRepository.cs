using System.Collections.Generic;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IPhotoAlbumRepository
    {
        ICollection<PhotoAlbum> GetPhotoAlbums();
        PhotoAlbum GetPhotoAlbum(string url);
    }
}
