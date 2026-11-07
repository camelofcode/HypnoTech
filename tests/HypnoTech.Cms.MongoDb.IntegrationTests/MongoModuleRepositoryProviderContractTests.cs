using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.MongoDb.Repositories;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Dal.MongoDb.Repositories;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Media.Dal.MongoDb.Repositories;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using HypnoTech.Persistence.ContractTests.Common;
using HypnoTech.Persistence.ContractTests.Ecommerce;
using HypnoTech.Persistence.ContractTests.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

[Collection(MongoPersistenceContractCollection.Name)]
public sealed class MongoCommonRepositoryProviderContractTests : CommonRepositoryProviderContractTests
{
    private readonly MongoCompatibilityFixture _fixture;

    public MongoCommonRepositoryProviderContractTests(MongoCompatibilityFixture fixture)
    {
        _fixture = fixture;
        MongoContractRegistration.RegisterCommon();
    }

    protected override IEnumerable<ICommonRepositoryContractContext> CreateContractContexts()
    {
        return _fixture.Targets.Select(target => new MongoCommonRepositoryContractContext(target));
    }
}

[Collection(MongoPersistenceContractCollection.Name)]
public sealed class MongoEcommerceRepositoryProviderContractTests : EcommerceRepositoryProviderContractTests
{
    private readonly MongoCompatibilityFixture _fixture;

    public MongoEcommerceRepositoryProviderContractTests(MongoCompatibilityFixture fixture)
    {
        _fixture = fixture;
        MongoContractRegistration.RegisterEcommerce();
    }

    protected override IEnumerable<IEcommerceRepositoryContractContext> CreateContractContexts()
    {
        return _fixture.Targets.Select(target => new MongoEcommerceRepositoryContractContext(target));
    }
}

[Collection(MongoPersistenceContractCollection.Name)]
public sealed class MongoMediaRepositoryProviderContractTests : MediaRepositoryProviderContractTests
{
    private readonly MongoCompatibilityFixture _fixture;

    public MongoMediaRepositoryProviderContractTests(MongoCompatibilityFixture fixture)
    {
        _fixture = fixture;
        MongoContractRegistration.RegisterMedia();
    }

    protected override IEnumerable<IMediaRepositoryContractContext> CreateContractContexts()
    {
        return _fixture.Targets.Select(target => new MongoMediaRepositoryContractContext(target));
    }
}

internal abstract class MongoRepositoryContractContext : IAsyncDisposable
{
    protected MongoRepositoryContractContext(MongoTarget target, string module)
    {
        ProviderName = $"MongoDB {target.Name}";
        Options = Microsoft.Extensions.Options.Options.Create(new MongoOptions
        {
            Server = target.Host,
            Port = target.Port,
            Database = $"ht_{module}_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
        });
        Database = new MongoClient(Options.Value.ConnectionString).GetDatabase(Options.Value.Database);
    }

    public string ProviderName { get; }

    protected IOptions<MongoOptions> Options { get; }

    protected IMongoDatabase Database { get; }

    public async ValueTask DisposeAsync()
    {
        await new MongoClient(Options.Value.ConnectionString).DropDatabaseAsync(Options.Value.Database);
    }

    protected static string EnsureIdentifier(string identifier)
    {
        return string.IsNullOrWhiteSpace(identifier) ? ObjectId.GenerateNewId().ToString() : identifier;
    }
}

internal sealed class MongoCommonRepositoryContractContext :
    MongoRepositoryContractContext,
    ICommonRepositoryContractContext
{
    public MongoCommonRepositoryContractContext(MongoTarget target)
        : base(target, "c")
    {
        NavBarRepository = new NavBarRepository(Options);
        NavBarAdminRepository = new NavBarAdminRepository(Options);
        UserRepository = new UserRepository(Options);
        AccountRepository = new AccountRepository(Options);
        ConfigAdminRepository = new ConfigAdminRepository(Options);
        CountryRepository = new CountryRepository(Options);
        EmailTemplateRepository = new EmailTemplateRepository(Options);
        EmailTemplateAdminRepository = new EmailTemplateAdminRepository(Options);
        PasswordResetRepository = new PasswordResetRepository(Options);
        LoggingRepository = new LoggingRepository(Options);
        BackupRepository = new BackupRepository(Options);
    }

    public INavBarRepository NavBarRepository { get; }

    public INavBarAdminRepository NavBarAdminRepository { get; }

    public IUserRepository UserRepository { get; }

    public IAccountRepository AccountRepository { get; }

    public IConfigAdminRepository ConfigAdminRepository { get; }

    public ICountryRepository CountryRepository { get; }

    public IEmailTemplateRepository EmailTemplateRepository { get; }

    public IEmailTemplateAdminRepository EmailTemplateAdminRepository { get; }

    public IPasswordResetRepository PasswordResetRepository { get; }

    public ILoggingRepository LoggingRepository { get; }

    public IBackupRespository BackupRepository { get; }

    public Task SeedNavBarLinksAsync(
        IEnumerable<NavBarLink> links,
        CancellationToken cancellationToken = default)
    {
        var documents = links.Select(link =>
        {
            link.Id = EnsureIdentifier(link.Id);
            return link.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>("navbar")
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }

    public Task SeedAsync<T>(
        string logicalStore,
        IEnumerable<T> items,
        CancellationToken cancellationToken = default)
    {
        var documents = items.Select(item =>
        {
            var id = typeof(T).GetProperty("Id");
            if (id?.CanWrite == true && id.PropertyType == typeof(string))
            {
                id.SetValue(item, EnsureIdentifier((string)id.GetValue(item)));
            }

            return item.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>(logicalStore)
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }
}

internal sealed class MongoEcommerceRepositoryContractContext :
    MongoRepositoryContractContext,
    IEcommerceRepositoryContractContext
{
    public MongoEcommerceRepositoryContractContext(MongoTarget target)
        : base(target, "e")
    {
        MerchCategoryTypeRepository = new MerchCategoryTypeRepository(Options);
        MerchRepository = new MerchRepository(Options, MerchCategoryTypeRepository);
        PostageProfileRepository = new PostageProfileRepository(Options, MerchRepository);
        OrderRepository = new OrderRepository(Options, new CountryRepository(Options));
        DownloadCodeRepository = new DownloadCodeRepository(Options);
    }

    public IMerchCategoryTypeRepository MerchCategoryTypeRepository { get; }

    public IMerchRepository MerchRepository { get; }

    public IPostageProfileRepository PostageProfileRepository { get; }

    public IOrderRepository OrderRepository { get; }

    public IDownloadCodeRepository DownloadCodeRepository { get; }

    public Task SeedCountriesAsync(
        IEnumerable<Country> countries,
        CancellationToken cancellationToken = default)
    {
        return Database.GetCollection<BsonDocument>("countries")
            .InsertManyAsync(
                countries.Select(country => country.ToBsonDocument()),
                cancellationToken: cancellationToken);
    }

    public Task SeedMerchCategoryTypesAsync(
        IEnumerable<MerchCategoryType> categoryTypes,
        CancellationToken cancellationToken = default)
    {
        var documents = categoryTypes.Select(categoryType =>
        {
            categoryType.Id = EnsureIdentifier(categoryType.Id);
            return categoryType.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>("merchcategorytypes")
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }

    public Task SeedMerchItemsAsync(
        IEnumerable<MerchItem> items,
        CancellationToken cancellationToken = default)
    {
        var documents = items.Select(item =>
        {
            item.Id = EnsureIdentifier(item.Id);
            return item.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>("merches")
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }
}

internal sealed class MongoMediaRepositoryContractContext :
    MongoRepositoryContractContext,
    IMediaRepositoryContractContext
{
    public MongoMediaRepositoryContractContext(MongoTarget target)
        : base(target, "m")
    {
        AudioTrackRepository = new AudioTrackRepository(Options);
        VideoRepository = new VideoRepository(Options);
        PhotoAlbumRepository = new PhotoAlbumRepository(Options);
    }

    public IAudioTrackRepository AudioTrackRepository { get; }

    public IVideoRepository VideoRepository { get; }

    public IPhotoAlbumRepository PhotoAlbumRepository { get; }

    public string CreateIdentifier()
    {
        return ObjectId.GenerateNewId().ToString();
    }

    public Task SeedAudioTracksAsync(
        IEnumerable<AudioTrack> tracks,
        CancellationToken cancellationToken = default)
    {
        var documents = tracks.Select(track =>
        {
            track.Id = EnsureIdentifier(track.Id);
            return track.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>("audiotracks")
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }

    public Task SeedVideosAsync(
        IEnumerable<YouTubeVideo> videos,
        CancellationToken cancellationToken = default)
    {
        var documents = videos.Select(video =>
        {
            video.Id = EnsureIdentifier(video.Id);
            return video.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>("videos")
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }

    public Task SeedPhotoAlbumsAsync(
        IEnumerable<PhotoAlbum> albums,
        CancellationToken cancellationToken = default)
    {
        var documents = albums.Select(album =>
        {
            album.Id = EnsureIdentifier(album.Id);
            foreach (var photo in album.Photos)
            {
                photo.Id = EnsureIdentifier(photo.Id);
            }

            return album.ToBsonDocument();
        });
        return Database.GetCollection<BsonDocument>("photoalbums")
            .InsertManyAsync(documents, cancellationToken: cancellationToken);
    }
}

internal static class MongoContractRegistration
{
    public static void RegisterCommon()
    {
        var services = new ServiceCollection();
        HypnoTech.Common.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForCommon(
            services,
            Configuration());
    }

    public static void RegisterEcommerce()
    {
        RegisterCommon();
        var services = new ServiceCollection();
        HypnoTech.Ecommerce.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForEcommerce(
            services,
            Configuration());
    }

    public static void RegisterMedia()
    {
        var services = new ServiceCollection();
        HypnoTech.Media.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForMedia(
            services,
            Configuration(media: true));
    }

    private static IConfiguration Configuration(bool media = false)
    {
        var values = new Dictionary<string, string>
        {
            ["Mongo:Server"] = "localhost",
            ["Mongo:Port"] = "27017",
            ["Mongo:Database"] = "hypnotech_contract_mapping"
        };
        if (media)
        {
            values["Media:AudioEnabled"] = "true";
            values["Media:YouTubeVideosEnabled"] = "true";
            values["Media:PhotoAlbumsEnabled"] = "true";
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
