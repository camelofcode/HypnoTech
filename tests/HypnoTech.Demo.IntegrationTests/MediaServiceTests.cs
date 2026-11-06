using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Frontend.Services;
using HypnoTech.Media.Models;
using Xunit;

namespace HypnoTech.Demo.IntegrationTests;

public sealed class MediaServiceTests
{
    [Fact]
    public async Task GetTracksForPlayerAsync_awaits_the_repository_and_preserves_enrichment()
    {
        var repository = new StubAudioTrackRepository(
        [
            new AudioTrack
            {
                Id = "synthetic-track",
                Title = "Synthetic track",
                Path = "/Files/Synthetic/track.wav",
                RelatedId = "synthetic-related-item"
            }
        ]);
        var service = new AudioTrackService(
            new DefaultAudioTrackEnrichmentService(),
            repository);

        var tracks = await service.GetTracksForPlayerAsync(
            TestContext.Current.CancellationToken);

        var track = Assert.Single(tracks);
        Assert.Equal("Synthetic track", track.Title);
        Assert.Null(track.RelatedId);
        Assert.Equal(TestContext.Current.CancellationToken, repository.CancellationToken);
    }

    private sealed class StubAudioTrackRepository : IAudioTrackRepository
    {
        private readonly ICollection<AudioTrack> _tracks;

        public StubAudioTrackRepository(ICollection<AudioTrack> tracks)
        {
            _tracks = tracks;
        }

        public CancellationToken CancellationToken { get; private set; }

        public ICollection<AudioTrack> GetTracksForPlayer() => _tracks;

        public Task<ICollection<AudioTrack>> GetTracksForPlayerAsync(
            CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            return Task.FromResult(_tracks);
        }
    }
}
