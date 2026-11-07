using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IVideoService
    {
        Task<ICollection<YouTubeVideo>> GetVideosAsync(CancellationToken cancellationToken = default);
    }

    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _repository;

        public VideoService(IVideoRepository repository)
        {
            _repository = repository;
        }

        public Task<ICollection<YouTubeVideo>> GetVideosAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetVideosAsync(cancellationToken);
        }
    }
}
