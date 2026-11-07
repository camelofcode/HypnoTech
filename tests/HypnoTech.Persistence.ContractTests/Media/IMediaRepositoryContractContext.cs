using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;

namespace HypnoTech.Persistence.ContractTests.Media;

public interface IMediaRepositoryContractContext : IAsyncDisposable
{
    string ProviderName { get; }

    IAudioTrackRepository AudioTrackRepository { get; }

    IVideoRepository VideoRepository { get; }

    IPhotoAlbumRepository PhotoAlbumRepository { get; }

    string CreateIdentifier();

    Task SeedAudioTracksAsync(
        IEnumerable<AudioTrack> tracks,
        CancellationToken cancellationToken = default);

    Task SeedVideosAsync(
        IEnumerable<YouTubeVideo> videos,
        CancellationToken cancellationToken = default);

    Task SeedPhotoAlbumsAsync(
        IEnumerable<PhotoAlbum> albums,
        CancellationToken cancellationToken = default);
}
