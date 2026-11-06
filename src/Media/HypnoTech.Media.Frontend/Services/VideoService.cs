using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IVideoService
    {
        ICollection<YouTubeVideo> GetVideos();
        Task<ICollection<YouTubeVideo>> GetVideosAsync(CancellationToken cancellationToken = default);
    }

    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _repository;

        public VideoService(IVideoRepository repository)
        {
            _repository = repository;
        }

        public ICollection<YouTubeVideo> GetVideos()
        {
            return _repository.GetVideos();
        }

        public Task<ICollection<YouTubeVideo>> GetVideosAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetVideosAsync(cancellationToken);
        }
    }
}
