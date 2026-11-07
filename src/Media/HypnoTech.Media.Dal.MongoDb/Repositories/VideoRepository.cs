using System.Collections.Generic;
using System.Linq;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Media.Dal.MongoDb.Repositories
{
    public class VideoRepository : BaseMongoRepository, IVideoRepository
    {
        public VideoRepository(IOptions<MongoOptions> config) : base(config, "videos")
        {
        }

        public async Task<ICollection<YouTubeVideo>> GetVideosAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<YouTubeVideo>(
                cancellationToken: cancellationToken)).ToList();
        }
    }
}
