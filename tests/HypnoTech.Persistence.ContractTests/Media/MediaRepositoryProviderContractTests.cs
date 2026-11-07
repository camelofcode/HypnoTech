using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Models;
using Xunit;

namespace HypnoTech.Persistence.ContractTests.Media;

public abstract class MediaRepositoryProviderContractTests
{
    protected abstract IEnumerable<IMediaRepositoryContractContext> CreateContractContexts();

    [Fact]
    public async Task Media_queries_preserve_values_album_ordering_and_url_lookup()
    {
        await ForEachContextAsync(async context =>
        {
            var relatedId = context.CreateIdentifier();
            await context.SeedAudioTracksAsync(
            [
                new AudioTrack
                {
                    Title = "Synthetic first track",
                    Path = "/Files/Synthetic/first.wav",
                    RelatedId = relatedId
                },
                new AudioTrack
                {
                    Title = "Synthetic second track",
                    Path = "/Files/Synthetic/second.wav"
                }
            ], TestContext.Current.CancellationToken);
            await context.SeedVideosAsync(
            [
                new YouTubeVideo
                {
                    Title = "Synthetic video",
                    YouTubeId = "synthetic-video-id"
                }
            ], TestContext.Current.CancellationToken);
            await context.SeedPhotoAlbumsAsync(
            [
                CreatePhotoAlbum(
                    "older-album",
                    "Older album",
                    new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)),
                CreatePhotoAlbum(
                    "newer-album",
                    "Newer album",
                    new DateTime(2026, 6, 7, 12, 0, 0, DateTimeKind.Utc))
            ], TestContext.Current.CancellationToken);

            var tracks = await context.AudioTrackRepository.GetTracksForPlayerAsync(
                TestContext.Current.CancellationToken);
            var videos = await context.VideoRepository.GetVideosAsync(
                TestContext.Current.CancellationToken);
            var albums = await context.PhotoAlbumRepository.GetPhotoAlbumsAsync(
                TestContext.Current.CancellationToken);
            var older = await context.PhotoAlbumRepository.GetPhotoAlbumAsync(
                "older-album",
                TestContext.Current.CancellationToken);

            Assert.Equal(
                ["Synthetic first track", "Synthetic second track"],
                tracks.Select(track => track.Title));
            Assert.Equal(relatedId, tracks.First().RelatedId);
            Assert.Equal("Synthetic video", Assert.Single(videos).Title);
            Assert.Equal(["Newer album", "Older album"], albums.Select(album => album.Title));
            Assert.Equal("Older album", older.Title);
        });
    }

    [Fact]
    public async Task Media_queries_honour_an_already_cancelled_token()
    {
        await ForEachContextAsync(async context =>
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                context.AudioTrackRepository.GetTracksForPlayerAsync(cancelled.Token));
        });
    }

    private async Task ForEachContextAsync(Func<IMediaRepositoryContractContext, Task> assertion)
    {
        foreach (var context in CreateContractContexts())
        {
            await using (context)
            {
                try
                {
                    await assertion(context);
                }
                catch (Exception exception)
                {
                    throw new Xunit.Sdk.XunitException(
                        $"Persistence provider '{context.ProviderName}' failed: {exception}");
                }
            }
        }
    }

    private static PhotoAlbum CreatePhotoAlbum(string url, string title, DateTime date)
    {
        return new PhotoAlbum
        {
            Url = url,
            Title = title,
            Date = date,
            Photos =
            [
                new Photo
                {
                    ImagePath = $"/Images/Synthetic/{url}.png",
                    Caption = $"Synthetic cover for {title}"
                }
            ]
        };
    }
}
