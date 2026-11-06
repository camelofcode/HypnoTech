using System.Collections.Generic;
using System.Linq;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Demo.Models;
using HypnoTech.Media.Frontend.Services;
using HypnoTech.Media.Models;

namespace HypnoTech.Demo.Website.Services;

public sealed class DemoAudioTrackEnrichmentService : IAudioTrackEnrichmentService
{
    private readonly ICmsRepository<Artist> _artists;

    public DemoAudioTrackEnrichmentService(ICmsRepository<Artist> artists)
    {
        _artists = artists;
    }

    public ICollection<AudioTrack> EnrichAudioTracks(ICollection<AudioTrack> tracks)
    {
        var artists = _artists.GetAll<Artist>().ToDictionary(artist => artist.Id);
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
