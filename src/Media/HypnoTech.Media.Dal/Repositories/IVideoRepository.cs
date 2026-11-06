using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IVideoRepository
    {
        ICollection<YouTubeVideo> GetVideos();
        Task<ICollection<YouTubeVideo>> GetVideosAsync(CancellationToken cancellationToken = default);
    }
}
