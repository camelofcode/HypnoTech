using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.EntityFrameworkCore;
using HypnoTech.Media.Dal.EntityFrameworkCore;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.Persistence.ContractTests.Media;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

[Xunit.Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlMediaRepositoryProviderContractTests :
    MediaRepositoryProviderContractTests
{
    private readonly IReadOnlyCollection<IRelationalDatabaseFixture> _fixtures;

    public PostgreSqlMediaRepositoryProviderContractTests(
        PostgreSqlFixture postgreSql,
        SqlServerFixture sqlServer)
    {
        _fixtures = [postgreSql, sqlServer];
    }

    protected override IEnumerable<IMediaRepositoryContractContext> CreateContractContexts()
    {
        foreach (var fixture in _fixtures)
        {
            yield return PostgreSqlMediaRepositoryContractContext.CreateAsync(fixture)
                .GetAwaiter()
                .GetResult();
        }
    }

    private sealed class PostgreSqlMediaRepositoryContractContext :
        IMediaRepositoryContractContext
    {
        private readonly IRelationalDatabaseFixture _fixture;
        private readonly string _connectionString;
        private readonly IHypnoTechDbContextFactory _contextFactory;

        private PostgreSqlMediaRepositoryContractContext(
            IRelationalDatabaseFixture fixture,
            string connectionString,
            IHypnoTechDbContextFactory contextFactory)
        {
            _fixture = fixture;
            _connectionString = connectionString;
            _contextFactory = contextFactory;
            AudioTrackRepository = new AudioTrackRepository(contextFactory);
            VideoRepository = new VideoRepository(contextFactory);
            PhotoAlbumRepository = new PhotoAlbumRepository(contextFactory);
        }

        public string ProviderName => _fixture.ProviderName;
        public IAudioTrackRepository AudioTrackRepository { get; }
        public IVideoRepository VideoRepository { get; }
        public IPhotoAlbumRepository PhotoAlbumRepository { get; }

        public static async Task<PostgreSqlMediaRepositoryContractContext> CreateAsync(
            IRelationalDatabaseFixture fixture)
        {
            var connectionString = await fixture.CreateDatabaseAsync();
            var options = fixture.Configure(
                    new DbContextOptionsBuilder<PostgreSqlMediaContractDbContext>(),
                    connectionString)
                .Options;
            var factory = new MediaContractDbContextFactory(options);
            await using (var context = await factory.CreateDbContextAsync())
            {
                await context.Database.EnsureCreatedAsync();
            }

            return new PostgreSqlMediaRepositoryContractContext(
                fixture,
                connectionString,
                new NeutralMediaDbContextFactory(factory));
        }

        public string CreateIdentifier() => Guid.NewGuid().ToString("N");

        public Task SeedAudioTracksAsync(
            IEnumerable<AudioTrack> tracks,
            CancellationToken cancellationToken = default) => SeedAsync(tracks, cancellationToken);

        public Task SeedVideosAsync(
            IEnumerable<YouTubeVideo> videos,
            CancellationToken cancellationToken = default) => SeedAsync(videos, cancellationToken);

        public Task SeedPhotoAlbumsAsync(
            IEnumerable<PhotoAlbum> albums,
            CancellationToken cancellationToken = default)
        {
            var values = albums.ToList();
            foreach (var photo in values.SelectMany(album => album.Photos))
            {
                photo.Id ??= CreateIdentifier();
            }

            return SeedAsync(values, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _fixture.DropDatabaseAsync(_connectionString);
        }

        private async Task SeedAsync<T>(
            IEnumerable<T> items,
            CancellationToken cancellationToken)
            where T : class
        {
            var values = items.ToList();
            var id = typeof(T).GetProperty("Id");
            foreach (var item in values)
            {
                if (string.IsNullOrEmpty((string)id.GetValue(item)))
                {
                    id.SetValue(item, CreateIdentifier());
                }
            }

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            context.AddRange(values.Cast<object>());
            if (typeof(T) == typeof(AudioTrack) || typeof(T) == typeof(YouTubeVideo))
            {
                long order = 1;
                foreach (var item in values)
                {
                    context.Entry(item).Property<long>("PersistenceOrder").CurrentValue = order++;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class PostgreSqlMediaContractDbContext : DbContext
    {
        public PostgreSqlMediaContractDbContext(
            DbContextOptions<PostgreSqlMediaContractDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddHypnoTechMediaModel();
        }
    }

    private sealed class MediaContractDbContextFactory :
        IDbContextFactory<PostgreSqlMediaContractDbContext>
    {
        private readonly DbContextOptions<PostgreSqlMediaContractDbContext> _options;
        public MediaContractDbContextFactory(
            DbContextOptions<PostgreSqlMediaContractDbContext> options) => _options = options;
        public PostgreSqlMediaContractDbContext CreateDbContext() => new(_options);
        public Task<PostgreSqlMediaContractDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }

    private sealed class NeutralMediaDbContextFactory : IHypnoTechDbContextFactory
    {
        private readonly MediaContractDbContextFactory _factory;
        public NeutralMediaDbContextFactory(MediaContractDbContextFactory factory) => _factory = factory;
        public async ValueTask<DbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            await _factory.CreateDbContextAsync(cancellationToken);
    }
}
