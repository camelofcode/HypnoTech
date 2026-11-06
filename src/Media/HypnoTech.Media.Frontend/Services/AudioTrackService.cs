using System.Collections.Generic;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Media.Frontend.Services
{
    public interface IAudioTrackService
    {
        ICollection<AudioTrack> GetTracksForPlayer();
        Task<ICollection<AudioTrack>> GetTracksForPlayerAsync(CancellationToken cancellationToken = default);
    }

    public class AudioTrackService : IAudioTrackService
    {
        private readonly IAudioTrackRepository _repository;
        private readonly IAudioTrackEnrichmentService _audioTrackEnrichmentService;
        
        public AudioTrackService(IAudioTrackEnrichmentService audioTrackEnrichmentService, IAudioTrackRepository repository)
        {
            _audioTrackEnrichmentService = audioTrackEnrichmentService;
            _repository = repository;
        }

        public ICollection<AudioTrack> GetTracksForPlayer()
        {
            return _audioTrackEnrichmentService.EnrichAudioTracks(_repository.GetTracksForPlayer());
        }

        public async Task<ICollection<AudioTrack>> GetTracksForPlayerAsync(CancellationToken cancellationToken = default)
        {
            var tracks = await _repository.GetTracksForPlayerAsync(cancellationToken);
            return _audioTrackEnrichmentService.EnrichAudioTracks(tracks);
        }
    }
}
