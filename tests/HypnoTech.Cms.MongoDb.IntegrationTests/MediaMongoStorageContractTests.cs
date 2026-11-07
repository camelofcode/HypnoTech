using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

public sealed partial class CmsRepositoryContractTests
{
    [Fact]
    public async Task Media_models_preserve_legacy_bson_names()
    {
        MongoContractRegistration.RegisterMedia();

        await ForEachMediaStorageTargetAsync(async options =>
        {
            var database = Database(options);
            var relatedId = ObjectId.GenerateNewId().ToString();
            var track = new AudioTrack
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = "Synthetic track",
                Path = "/Files/Synthetic/track.wav",
                RelatedId = relatedId
            };
            var album = new PhotoAlbum
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Url = "synthetic-album",
                Title = "Synthetic album",
                Date = new DateTime(2026, 6, 7, 12, 0, 0, DateTimeKind.Utc),
                Photos =
                [
                    new Photo
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        ImagePath = "/Images/Synthetic/album.png",
                        Caption = "Synthetic cover"
                    }
                ]
            };

            await database.GetCollection<BsonDocument>("audiotracks")
                .InsertOneAsync(track.ToBsonDocument(), cancellationToken: TestContext.Current.CancellationToken);
            await database.GetCollection<BsonDocument>("photoalbums")
                .InsertOneAsync(album.ToBsonDocument(), cancellationToken: TestContext.Current.CancellationToken);

            var audioDocument = await database.GetCollection<BsonDocument>("audiotracks")
                .Find(Builders<BsonDocument>.Filter.Eq("title", track.Title))
                .SingleAsync(TestContext.Current.CancellationToken);
            var albumDocument = await database.GetCollection<BsonDocument>("photoalbums")
                .Find(Builders<BsonDocument>.Filter.Eq("url", album.Url))
                .SingleAsync(TestContext.Current.CancellationToken);

            Assert.True(audioDocument.Contains("related"));
            Assert.False(audioDocument.Contains(nameof(AudioTrack.RelatedId)));
            Assert.True(albumDocument.Contains("date"));
        });
    }

    private async Task ForEachMediaStorageTargetAsync(Func<IOptions<MongoOptions>, Task> assertion)
    {
        foreach (var target in _fixture.Targets)
        {
            var options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"ht_ms_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
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
}
