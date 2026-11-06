using System.Collections.Generic;
using System.Linq;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Media.Dal.MongoDb.Repositories
{
    public class PhotoAlbumRepository : BaseMongoRepository, IPhotoAlbumRepository
    {
        public PhotoAlbumRepository(IOptions<MongoOptions> config) : base(config, "photoalbums")
        {
        }

        public ICollection<PhotoAlbum> GetPhotoAlbums()
        {
            return GetAll<PhotoAlbum>("date", true).ToList();
        }

        public async Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<PhotoAlbum>(
                "date",
                true,
                cancellationToken)).ToList();
        }

        public PhotoAlbum GetPhotoAlbum(string url)
        {
            return GetOne<PhotoAlbum>(new BsonDocument("url", url));
        }

        public Task<PhotoAlbum> GetPhotoAlbumAsync(string url, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<PhotoAlbum>(new BsonDocument("url", url), cancellationToken);
        }
    }
}
