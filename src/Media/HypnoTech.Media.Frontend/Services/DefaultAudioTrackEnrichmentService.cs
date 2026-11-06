using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IAudioTrackEnrichmentService
    {
        ICollection<AudioTrack> EnrichAudioTracks(ICollection<AudioTrack> tracks);

        Task<ICollection<AudioTrack>> EnrichAudioTracksAsync(
            ICollection<AudioTrack> tracks,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(EnrichAudioTracks(tracks));
    }

    public class DefaultAudioTrackEnrichmentService : IAudioTrackEnrichmentService
    {
        public ICollection<AudioTrack> EnrichAudioTracks(ICollection<AudioTrack> tracks)
        {
            foreach (var audioTrack in tracks)
            {
                audioTrack.RelatedId = null;
            }

            return tracks;
        }
    }
}
