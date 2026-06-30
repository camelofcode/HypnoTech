using System.Collections.Generic;
using System.Linq;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;

namespace HypnoTech.Media.Dal.MongoDb.Repositories
{
    public class VideoRepository : BaseMongoRepository, IVideoRepository
    {
        public VideoRepository(IOptions<MongoOptions> config) : base(config, "videos")
        {
        }

        public ICollection<YouTubeVideo> GetVideos()
        {
            return GetAll<YouTubeVideo>().ToList();
        }
    }
}
