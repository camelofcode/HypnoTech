using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.EntityFrameworkCore;
using HypnoTech.Persistence.ContractTests.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

[Xunit.Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlCmsRepositoryProviderContractTests :
    CmsRepositoryProviderContractTests
{
    private readonly IReadOnlyCollection<IRelationalDatabaseFixture> _fixtures;

    public PostgreSqlCmsRepositoryProviderContractTests(
        PostgreSqlFixture postgreSql,
        SqlServerFixture sqlServer)
    {
        _fixtures = [postgreSql, sqlServer];
    }

    protected override IEnumerable<ICmsRepositoryContractContext> CreateContractContexts()
    {
        foreach (var fixture in _fixtures)
        {
            yield return PostgreSqlCmsRepositoryContractContext.CreateAsync(fixture)
                .GetAwaiter()
                .GetResult();
        }
    }

    private sealed class PostgreSqlCmsRepositoryContractContext : ICmsRepositoryContractContext
    {
        private readonly IRelationalDatabaseFixture _fixture;
        private readonly string _connectionString;
        private readonly IHypnoTechDbContextFactory _contextFactory;

        private PostgreSqlCmsRepositoryContractContext(
            IRelationalDatabaseFixture fixture,
            string connectionString,
            IHypnoTechDbContextFactory contextFactory)
        {
            _fixture = fixture;
            _connectionString = connectionString;
            _contextFactory = contextFactory;
            SelectListRepository = new SelectListRepository(contextFactory);
            SeedRepository = new SeedRepository(contextFactory);
        }

        public string ProviderName => _fixture.ProviderName;

        public ISelectListRepository SelectListRepository { get; }

        public ISeedRepository SeedRepository { get; }

        public static async Task<PostgreSqlCmsRepositoryContractContext> CreateAsync(
            IRelationalDatabaseFixture fixture)
        {
            var connectionString = await fixture.CreateDatabaseAsync();
            var options = fixture.Configure(
                    new DbContextOptionsBuilder<PostgreSqlCmsContractDbContext>(),
                    connectionString)
                .Options;
            var factory = new ContractDbContextFactory(options);
            await using (var context = await factory.CreateDbContextAsync())
            {
                await context.Database.EnsureCreatedAsync();
            }

            return new PostgreSqlCmsRepositoryContractContext(
                fixture,
                connectionString,
                new NeutralDbContextFactory(factory));
        }

        public string CreateIdentifier()
        {
            return Guid.NewGuid().ToString("N");
        }

        public ICmsRepository<TModel> CreateRepository<TModel>() where TModel : class
        {
            return new CmsRepository<TModel>(_contextFactory);
        }

        public async Task SeedChildAsync(
            CmsContractChild child,
            CancellationToken cancellationToken = default)
        {
            await AddAsync(child, cancellationToken);
        }

        public async Task SeedChoiceAsync(
            CmsContractChoice choice,
            CancellationToken cancellationToken = default)
        {
            await AddAsync(choice, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _fixture.DropDatabaseAsync(_connectionString);
        }

        private async Task AddAsync<TModel>(
            TModel model,
            CancellationToken cancellationToken)
            where TModel : class
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            context.Add(model);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed class PostgreSqlCmsContractDbContext : DbContext
    {
        public PostgreSqlCmsContractDbContext(
            DbContextOptions<PostgreSqlCmsContractDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddHypnoTechCmsModel();
        }
    }

    private sealed class ContractDbContextFactory :
        IDbContextFactory<PostgreSqlCmsContractDbContext>
    {
        private readonly DbContextOptions<PostgreSqlCmsContractDbContext> _options;

        public ContractDbContextFactory(
            DbContextOptions<PostgreSqlCmsContractDbContext> options)
        {
            _options = options;
        }

        public PostgreSqlCmsContractDbContext CreateDbContext()
        {
            return new PostgreSqlCmsContractDbContext(_options);
        }

        public Task<PostgreSqlCmsContractDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateDbContext());
        }
    }

    private sealed class NeutralDbContextFactory : IHypnoTechDbContextFactory
    {
        private readonly ContractDbContextFactory _factory;

        public NeutralDbContextFactory(ContractDbContextFactory factory)
        {
            _factory = factory;
        }

        public async ValueTask<DbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
        {
            return await _factory.CreateDbContextAsync(cancellationToken);
        }
    }
}
