using System.Collections.Generic;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IVideoService
    {
        ICollection<YouTubeVideo> GetVideos();
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
    }
}
