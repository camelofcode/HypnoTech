using System;
using System.IO;
using System.Threading.Tasks;
using HypnoTech.Demo.Admin;
using HypnoTech.Demo.Website.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Demo.IntegrationTests;

public sealed class DemoExampleTests : IClassFixture<DemoMongoFixture>
{
    private readonly DemoMongoFixture _mongo;

    public DemoExampleTests(DemoMongoFixture mongo)
    {
        _mongo = mongo;
    }

    [Fact]
    public async Task Admin_host_seeds_only_synthetic_content_and_hashes_the_documented_password()
    {
        var databaseName = $"hypnotech_demo_{Guid.NewGuid():N}";
        var uploadPath = Path.Combine(Path.GetTempPath(), $"hypnotech-demo-uploads-{Guid.NewGuid():N}");
        var originalDirectory = Directory.GetCurrentDirectory();
        var cancellationToken = TestContext.Current.CancellationToken;

        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            using var host = Program.CreateHostBuilder(
                [
                    $"--Mongo:Server={_mongo.Host}",
                    $"--Mongo:Port={_mongo.Port}",
                    $"--Mongo:Database={databaseName}",
                    $"--UploadPath={uploadPath}",
                    "--Demo:SeedOnStartup=true",
                    "--urls=http://127.0.0.1:0"
                ])
                .ConfigureWebHost(webBuilder => webBuilder
                    .UseContentRoot(AppContext.BaseDirectory)
                    .UseEnvironment(Environments.Development))
                .Build();

            await host.StartAsync(cancellationToken);
            await host.StopAsync(cancellationToken);

            var database = new MongoClient($"mongodb://{_mongo.Host}:{_mongo.Port}")
                .GetDatabase(databaseName);
            var user = await database.GetCollection<BsonDocument>("adminusers")
                .Find(Builders<BsonDocument>.Filter.Eq("username", DemoSeeder.AdminUsername))
                .SingleAsync(cancellationToken);

            Assert.StartsWith("$2", user["password"].AsString);
            Assert.DoesNotContain(DemoSeeder.AdminPassword, user.ToJson());
            Assert.Equal(2, await database.GetCollection<BsonDocument>("demoArticles")
                .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken));
            Assert.Equal(3, await database.GetCollection<BsonDocument>("demoArtists")
                .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken));
            Assert.Equal(2, await database.GetCollection<BsonDocument>("demoEvents")
                .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken));
            Assert.NotEmpty(await database.GetCollection<BsonDocument>("merches")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(cancellationToken));
            Assert.NotEmpty(await database.GetCollection<BsonDocument>("audiotracks")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(cancellationToken));
            Assert.NotEmpty(await database.GetCollection<BsonDocument>("photoalbums")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(cancellationToken));
            Assert.Single(await database.GetCollection<BsonDocument>("videos")
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(cancellationToken));
            Assert.Equal(5, await database.GetCollection<BsonDocument>("navbar")
                .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken));
            Assert.True(File.Exists(Path.Combine(uploadPath, "Images", "Demo", "artwork-rose.svg")));
            Assert.True(File.Exists(Path.Combine(uploadPath, "Files", "Demo", "lantern-motion.mp4")));

            await database.Client.DropDatabaseAsync(databaseName, cancellationToken);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            if (Directory.Exists(uploadPath))
            {
                Directory.Delete(uploadPath, true);
            }
        }
    }

    [Theory]
    [InlineData("/Checkout/Pay")]
    [InlineData("/checkout/pay/anything")]
    [InlineData("/Checkout/Complete")]
    [InlineData("/Checkout/Cancelled")]
    public void Real_payment_paths_are_blocked(string path)
    {
        Assert.True(DemoPaymentSafety.IsRealPaymentPath(path));
    }

    [Theory]
    [InlineData("/Basket/GetDetails")]
    [InlineData("/Checkout/GetDetails")]
    [InlineData("/Checkout")]
    [InlineData("/Shop")]
    public void Safe_basket_and_checkout_paths_remain_available(string path)
    {
        Assert.False(DemoPaymentSafety.IsRealPaymentPath(path));
    }
}
