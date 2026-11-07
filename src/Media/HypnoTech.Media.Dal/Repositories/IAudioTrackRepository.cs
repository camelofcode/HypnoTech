using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IAudioTrackRepository
    {
        Task<ICollection<AudioTrack>> GetTracksForPlayerAsync(CancellationToken cancellationToken = default);
    }
}
