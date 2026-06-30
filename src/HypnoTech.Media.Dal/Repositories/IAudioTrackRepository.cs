using System.Collections.Generic;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Dal.Repositories
{
    public interface IAudioTrackRepository
    {
        ICollection<AudioTrack> GetTracksForPlayer();
    }
}
