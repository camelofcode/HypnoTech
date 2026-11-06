using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Media.Dal.MongoDb.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

public sealed partial class CmsRepositoryContractTests
{
    [Fact]
    public void Media_repositories_preserve_collections_bson_names_ordering_and_lookup()
    {
        RegisterMediaMappings();

        ForEachMediaTarget(options =>
        {
            var database = Database(options);
            var relatedId = ObjectId.GenerateNewId().ToString();
            database.GetCollection<BsonDocument>("audiotracks").InsertMany(
            [
                new AudioTrack
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Synthetic first track",
                    Path = "/Files/Synthetic/first.wav",
                    RelatedId = relatedId
                }.ToBsonDocument(),
                new AudioTrack
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Synthetic second track",
                    Path = "/Files/Synthetic/second.wav"
                }.ToBsonDocument()
            ]);
            database.GetCollection<BsonDocument>("videos").InsertOne(
                new YouTubeVideo
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Synthetic video",
                    YouTubeId = "synthetic-video-id"
                }.ToBsonDocument());
            database.GetCollection<BsonDocument>("photoalbums").InsertMany(
            [
                CreatePhotoAlbum("older-album", "Older album", new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)),
                CreatePhotoAlbum("newer-album", "Newer album", new DateTime(2026, 6, 7, 12, 0, 0, DateTimeKind.Utc))
            ]);

            var tracks = new AudioTrackRepository(options).GetTracksForPlayer();
            var videos = new VideoRepository(options).GetVideos();
            var photoAlbums = new PhotoAlbumRepository(options);

            Assert.Equal(
                ["Synthetic first track", "Synthetic second track"],
                tracks.Select(track => track.Title));
            Assert.Equal(relatedId, tracks.First().RelatedId);
            Assert.Equal("Synthetic video", Assert.Single(videos).Title);
            Assert.Equal(
                ["Newer album", "Older album"],
                photoAlbums.GetPhotoAlbums().Select(album => album.Title));
            Assert.Equal("Older album", photoAlbums.GetPhotoAlbum("older-album").Title);

            var audioDocument = database.GetCollection<BsonDocument>("audiotracks")
                .Find(Builders<BsonDocument>.Filter.Eq("title", "Synthetic first track"))
                .Single();
            Assert.True(audioDocument.Contains("related"));
            Assert.False(audioDocument.Contains(nameof(AudioTrack.RelatedId)));
            Assert.True(database.GetCollection<BsonDocument>("photoalbums")
                .Find(Builders<BsonDocument>.Filter.Eq("url", "newer-album"))
                .Single()
                .Contains("date"));
        });
    }

    [Fact]
    public async Task Media_async_repositories_match_the_synchronous_contract_and_honour_cancellation()
    {
        RegisterMediaMappings();

        await ForEachMediaTargetAsync(async options =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var database = Database(options);
            await database.GetCollection<BsonDocument>("audiotracks").InsertManyAsync(
            [
                new AudioTrack
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Async first track",
                    Path = "/Files/Synthetic/async-first.wav"
                }.ToBsonDocument(),
                new AudioTrack
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Async second track",
                    Path = "/Files/Synthetic/async-second.wav"
                }.ToBsonDocument()
            ], cancellationToken: cancellationToken);
            await database.GetCollection<BsonDocument>("videos").InsertOneAsync(
                new YouTubeVideo
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = "Async synthetic video",
                    YouTubeId = "async-synthetic-video-id"
                }.ToBsonDocument(),
                cancellationToken: cancellationToken);
            await database.GetCollection<BsonDocument>("photoalbums").InsertManyAsync(
            [
                CreatePhotoAlbum("async-older", "Async older", new DateTime(2026, 2, 3, 12, 0, 0, DateTimeKind.Utc)),
                CreatePhotoAlbum("async-newer", "Async newer", new DateTime(2026, 8, 9, 12, 0, 0, DateTimeKind.Utc))
            ], cancellationToken: cancellationToken);

            var audio = new AudioTrackRepository(options);
            var videos = new VideoRepository(options);
            var albums = new PhotoAlbumRepository(options);

            Assert.Equal(
                ["Async first track", "Async second track"],
                (await audio.GetTracksForPlayerAsync(cancellationToken)).Select(track => track.Title));
            Assert.Equal(
                "Async synthetic video",
                Assert.Single(await videos.GetVideosAsync(cancellationToken)).Title);
            Assert.Equal(
                ["Async newer", "Async older"],
                (await albums.GetPhotoAlbumsAsync(cancellationToken)).Select(album => album.Title));
            Assert.Equal(
                "Async older",
                (await albums.GetPhotoAlbumAsync("async-older", cancellationToken)).Title);

            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                audio.GetTracksForPlayerAsync(cancelled.Token));
        });
    }

    private void ForEachMediaTarget(Action<IOptions<MongoOptions>> assertion)
    {
        foreach (var target in _fixture.Targets)
        {
            var options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"ht_m_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
            });

            try
            {
                assertion(options);
            }
            catch (Exception exception)
            {
                throw new Xunit.Sdk.XunitException($"MongoDB target '{target.Name}' failed: {exception}");
            }
            finally
            {
                new MongoClient(options.Value.ConnectionString)
                    .DropDatabase(options.Value.Database);
            }
        }
    }

    private async Task ForEachMediaTargetAsync(Func<IOptions<MongoOptions>, Task> assertion)
    {
        foreach (var target in _fixture.Targets)
        {
            var options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"ht_ma_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
            });

            try
            {
                await assertion(options);
            }
            catch (Exception exception)
            {
                throw new Xunit.Sdk.XunitException($"MongoDB target '{target.Name}' failed: {exception}");
            }
            finally
            {
                await new MongoClient(options.Value.ConnectionString)
                    .DropDatabaseAsync(options.Value.Database);
            }
        }
    }

    private static void RegisterMediaMappings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Mongo:Server"] = "localhost",
                ["Mongo:Port"] = "27017",
                ["Mongo:Database"] = "hypnotech_media_mapping_tests",
                ["Media:AudioEnabled"] = "true",
                ["Media:YouTubeVideosEnabled"] = "true",
                ["Media:PhotoAlbumsEnabled"] = "true"
            })
            .Build();
        var services = new ServiceCollection();
        HypnoTech.Media.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForMedia(
            services,
            configuration);
    }

    private static BsonDocument CreatePhotoAlbum(string url, string title, DateTime date)
    {
        return new PhotoAlbum
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Url = url,
            Title = title,
            Date = date,
            Photos =
            [
                new Photo
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    ImagePath = $"/Images/Synthetic/{url}.png",
                    Caption = $"Synthetic cover for {title}"
                }
            ]
        }.ToBsonDocument();
    }
}
