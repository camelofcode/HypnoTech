using System.Collections.Generic;
using HypnoTech.Media.Models;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IAudioTrackEnrichmentService
    {
        ICollection<AudioTrack> EnrichAudioTracks(ICollection<AudioTrack> tracks);
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
