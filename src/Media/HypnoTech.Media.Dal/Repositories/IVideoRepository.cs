using System.Collections.Generic;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IVideoRepository
    {
        ICollection<YouTubeVideo> GetVideos();
    }
}
