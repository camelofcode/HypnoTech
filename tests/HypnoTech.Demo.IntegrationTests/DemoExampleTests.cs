using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HypnoTech.Demo.Admin;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using DemoWebsiteProgram = HypnoTech.Demo.Website.Program;

namespace HypnoTech.Demo.IntegrationTests;

public sealed class DemoExampleTests : IClassFixture<DemoMongoFixture>
{
    private readonly DemoMongoFixture _mongo;

    public DemoExampleTests(DemoMongoFixture mongo)
    {
        _mongo = mongo;
    }

    [Fact]
    public void Public_example_projects_do_not_reference_cms_web_assemblies()
    {
        var websiteReferences = typeof(DemoWebsiteProgram).Assembly.GetReferencedAssemblies();
        var modelReferences = typeof(Article).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(
            websiteReferences,
            reference => reference.Name.StartsWith("HypnoTech.Cms", StringComparison.Ordinal));
        Assert.DoesNotContain(
            modelReferences,
            reference => reference.Name.StartsWith("HypnoTech.Cms", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Demo_hosts_seed_safe_content_and_preserve_the_navigation_response_contract()
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

            var websiteContentRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../examples/HypnoTech.Demo.Website"));
            Directory.SetCurrentDirectory(websiteContentRoot);
            using var websiteHost = DemoWebsiteProgram.CreateHostBuilder(
                [
                    $"--Mongo:Server={_mongo.Host}",
                    $"--Mongo:Port={_mongo.Port}",
                    $"--Mongo:Database={databaseName}",
                    $"--UploadPath={uploadPath}",
                    "--urls=http://127.0.0.1:0"
                ])
                .ConfigureWebHost(webBuilder => webBuilder
                    .UseContentRoot(websiteContentRoot)
                    .UseEnvironment(Environments.Development))
                .Build();

            await websiteHost.StartAsync(cancellationToken);
            var websiteAddress = websiteHost.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()
                .Addresses.Single();
            using var client = new HttpClient
            {
                BaseAddress = new Uri(websiteAddress.Replace("127.0.0.1", "localhost", StringComparison.Ordinal))
            };

            using var fullResponse = await client.GetAsync("/Articles", cancellationToken);
            var fullHtml = await fullResponse.Content.ReadAsStringAsync(cancellationToken);
            Assert.Equal(HttpStatusCode.OK, fullResponse.StatusCode);
            Assert.Equal("text/html", fullResponse.Content.Headers.ContentType?.MediaType);
            Assert.Contains("<!doctype html>", fullHtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("id=\"content\"", fullHtml, StringComparison.Ordinal);
            Assert.Contains("data-testid=\"persistent-basket\"", fullHtml, StringComparison.Ordinal);
            Assert.Contains("/_content/HypnoTech.Frontend/js/", fullHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("/_content/HypnoTech.Cms/js/", fullHtml, StringComparison.Ordinal);

            using var partialRequest = new HttpRequestMessage(HttpMethod.Get, "/Articles");
            partialRequest.Headers.TryAddWithoutValidation("X-Partial-Request", "1");
            using var partialResponse = await client.SendAsync(partialRequest, cancellationToken);
            var partialJson = await partialResponse.Content.ReadAsStringAsync(cancellationToken);
            Assert.Equal(HttpStatusCode.OK, partialResponse.StatusCode);
            Assert.Equal("application/json", partialResponse.Content.Headers.ContentType?.MediaType);
            using (var document = JsonDocument.Parse(partialJson))
            {
                var root = document.RootElement;
                Assert.Equal("Stories", root.GetProperty("title").GetString());
                Assert.Contains("data-testid=\"articles-list\"", root.GetProperty("html").GetString());
                Assert.DoesNotContain("<!doctype", root.GetProperty("html").GetString(), StringComparison.OrdinalIgnoreCase);
                Assert.True(root.TryGetProperty("script", out _));
                Assert.True(root.TryGetProperty("description", out _));
                Assert.Equal(
                    "/articles",
                    new Uri(root.GetProperty("canonicalUrl").GetString()).AbsolutePath);
            }

            using var imageResponse = await client.GetAsync(
                "/Images/Demo/artwork-rose.svg",
                cancellationToken);
            Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
            Assert.Equal("image/svg+xml", imageResponse.Content.Headers.ContentType?.MediaType);

            using var fileRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "/Files/Demo/lantern-motion.mp4");
            fileRequest.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 9);
            using var fileResponse = await client.SendAsync(fileRequest, cancellationToken);
            Assert.Equal(HttpStatusCode.PartialContent, fileResponse.StatusCode);
            Assert.Equal(10, (await fileResponse.Content.ReadAsByteArrayAsync(cancellationToken)).Length);

            using var navbarRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "/Api/ClearNavbarCache");
            navbarRequest.Headers.Add("X-Auth-Token", "demo-only-api-key");
            using var navbarResponse = await client.SendAsync(navbarRequest, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, navbarResponse.StatusCode);

            using var cartRequest = new HttpRequestMessage(HttpMethod.Get, "/Cart");
            cartRequest.Headers.TryAddWithoutValidation("X-Partial-Request", "1");
            using var cartResponse = await client.SendAsync(cartRequest, cancellationToken);
            var cartJson = await cartResponse.Content.ReadAsStringAsync(cancellationToken);
            using (var document = JsonDocument.Parse(cartJson))
            {
                Assert.Equal("demoCart", document.RootElement.GetProperty("script").GetString());
            }

            using var checkoutRequest = new HttpRequestMessage(HttpMethod.Get, "/Checkout");
            checkoutRequest.Headers.TryAddWithoutValidation("X-Partial-Request", "1");
            using var checkoutResponse = await client.SendAsync(checkoutRequest, cancellationToken);
            var checkoutHtml = await checkoutResponse.Content.ReadAsStringAsync(cancellationToken);
            Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
            Assert.Equal("text/html", checkoutResponse.Content.Headers.ContentType?.MediaType);
            Assert.Contains("<!doctype html>", checkoutHtml, StringComparison.OrdinalIgnoreCase);

            await websiteHost.StopAsync(cancellationToken);

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
