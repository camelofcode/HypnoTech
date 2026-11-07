using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.MongoDb.Repositories;
using HypnoTech.Common.Models.Settings;
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
    public async Task Navbar_async_repository_preserves_enabled_order_and_honours_cancellation()
    {
        RegisterCommonMappings();

        await ForEachNavbarTargetAsync(async options =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var collection = Database(options).GetCollection<BsonDocument>("navbar");
            await collection.InsertManyAsync(
            [
                new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "key", "later" },
                    { "text", "Later" },
                    { "order", 2 },
                    { "enabled", true }
                },
                new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "key", "disabled" },
                    { "text", "Disabled" },
                    { "order", 0 },
                    { "enabled", false }
                },
                new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "key", "earlier" },
                    { "text", "Earlier" },
                    { "order", 1 },
                    { "enabled", true }
                }
            ], cancellationToken: cancellationToken);
            var repository = new NavBarRepository(options);

            var links = await repository.GetEnabledNavBarLinksAsync(cancellationToken);

            Assert.Equal(["earlier", "later"], links.Select(link => link.Key));
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                repository.GetEnabledNavBarLinksAsync(cancelled.Token));
        });
    }

    private static void RegisterCommonMappings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Mongo:Server"] = "localhost",
                ["Mongo:Port"] = "27017",
                ["Mongo:Database"] = "hypnotech_common_mapping_tests"
            })
            .Build();
        var services = new ServiceCollection();
        HypnoTech.Common.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForCommon(
            services,
            configuration);
    }

    private async Task ForEachNavbarTargetAsync(Func<IOptions<MongoOptions>, Task> assertion)
    {
        foreach (var target in _fixture.Targets)
        {
            var options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"ht_nav_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
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
