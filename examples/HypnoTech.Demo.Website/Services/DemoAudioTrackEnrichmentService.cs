using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.Media.Frontend.Services;
using HypnoTech.Media.Models;

namespace HypnoTech.Demo.Website.Services;

public sealed class DemoAudioTrackEnrichmentService : IAudioTrackEnrichmentService
{
    private readonly IArtistRepository _artists;

    public DemoAudioTrackEnrichmentService(IArtistRepository artists)
    {
        _artists = artists;
    }

    public ICollection<AudioTrack> EnrichAudioTracks(ICollection<AudioTrack> tracks)
    {
        var artists = _artists.GetAll().ToDictionary(artist => artist.Id);
        return EnrichAudioTracks(tracks, artists);
    }

    public async Task<ICollection<AudioTrack>> EnrichAudioTracksAsync(
        ICollection<AudioTrack> tracks,
        CancellationToken cancellationToken = default)
    {
        var artists = (await _artists.GetAllAsync(cancellationToken))
            .ToDictionary(artist => artist.Id);
        return EnrichAudioTracks(tracks, artists);
    }

    private static ICollection<AudioTrack> EnrichAudioTracks(
        ICollection<AudioTrack> tracks,
        IReadOnlyDictionary<string, Artist> artists)
    {
        foreach (var track in tracks)
        {
            if (track.RelatedId != null && artists.TryGetValue(track.RelatedId, out var artist))
            {
                track.RelatedName = artist.Name;
                track.ImagePath = artist.ImagePath;
            }
        }

        return tracks;
    }
}
