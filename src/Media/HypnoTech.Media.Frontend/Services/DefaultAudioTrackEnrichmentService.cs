using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IAudioTrackEnrichmentService
    {
        Task<ICollection<AudioTrack>> EnrichAudioTracksAsync(
            ICollection<AudioTrack> tracks,
            CancellationToken cancellationToken = default);
    }

    public class DefaultAudioTrackEnrichmentService : IAudioTrackEnrichmentService
    {
        public Task<ICollection<AudioTrack>> EnrichAudioTracksAsync(
            ICollection<AudioTrack> tracks,
            CancellationToken cancellationToken = default)
        {
            foreach (var audioTrack in tracks)
            {
                audioTrack.RelatedId = null;
            }

            return Task.FromResult(tracks);
        }
    }
}
