using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.EntityFrameworkCore;
using HypnoTech.Persistence.ContractTests.Ecommerce;
using Microsoft.EntityFrameworkCore;
using CommonEf = HypnoTech.Common.Dal.EntityFrameworkCore;
using EcommerceEf = HypnoTech.Ecommerce.Dal.EntityFrameworkCore;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

[Xunit.Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlEcommerceRepositoryProviderContractTests :
    EcommerceRepositoryProviderContractTests
{
    private readonly IReadOnlyCollection<IRelationalDatabaseFixture> _fixtures;

    public PostgreSqlEcommerceRepositoryProviderContractTests(
        PostgreSqlFixture postgreSql,
        SqlServerFixture sqlServer)
    {
        _fixtures = [postgreSql, sqlServer];
    }

    protected override IEnumerable<IEcommerceRepositoryContractContext> CreateContractContexts()
    {
        foreach (var fixture in _fixtures)
        {
            yield return PostgreSqlEcommerceRepositoryContractContext.CreateAsync(fixture)
                .GetAwaiter()
                .GetResult();
        }
    }

    private sealed class PostgreSqlEcommerceRepositoryContractContext :
        IEcommerceRepositoryContractContext
    {
        private readonly IRelationalDatabaseFixture _fixture;
        private readonly string _connectionString;
        private readonly IHypnoTechDbContextFactory _contextFactory;

        private PostgreSqlEcommerceRepositoryContractContext(
            IRelationalDatabaseFixture fixture,
            string connectionString,
            IHypnoTechDbContextFactory contextFactory)
        {
            _fixture = fixture;
            _connectionString = connectionString;
            _contextFactory = contextFactory;

            MerchCategoryTypeRepository = new EcommerceEf.MerchCategoryTypeRepository(contextFactory);
            MerchRepository = new EcommerceEf.MerchRepository(
                contextFactory,
                MerchCategoryTypeRepository);
            PostageProfileRepository = new EcommerceEf.PostageProfileRepository(
                contextFactory,
                MerchRepository);
            OrderRepository = new EcommerceEf.OrderRepository(
                contextFactory,
                new CommonEf.CountryRepository(contextFactory));
            DownloadCodeRepository = new EcommerceEf.DownloadCodeRepository(contextFactory);
        }

        public string ProviderName => _fixture.ProviderName;
        public IMerchCategoryTypeRepository MerchCategoryTypeRepository { get; }
        public IMerchRepository MerchRepository { get; }
        public IPostageProfileRepository PostageProfileRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IDownloadCodeRepository DownloadCodeRepository { get; }

        public static async Task<PostgreSqlEcommerceRepositoryContractContext> CreateAsync(
            IRelationalDatabaseFixture fixture)
        {
            var connectionString = await fixture.CreateDatabaseAsync();
            var options = fixture.Configure(
                    new DbContextOptionsBuilder<PostgreSqlEcommerceContractDbContext>(),
                    connectionString)
                .Options;
            var factory = new EcommerceContractDbContextFactory(options);
            await using (var context = await factory.CreateDbContextAsync())
            {
                await context.Database.EnsureCreatedAsync();
            }

            return new PostgreSqlEcommerceRepositoryContractContext(
                fixture,
                connectionString,
                new NeutralEcommerceDbContextFactory(factory));
        }

        public Task SeedCountriesAsync(
            IEnumerable<Country> countries,
            CancellationToken cancellationToken = default) => AddRangeAsync(countries, cancellationToken);

        public Task SeedMerchCategoryTypesAsync(
            IEnumerable<MerchCategoryType> categoryTypes,
            CancellationToken cancellationToken = default)
        {
            var types = categoryTypes.ToList();
            SetIds(types);
            return AddRangeAsync(types, cancellationToken);
        }

        public Task SeedMerchItemsAsync(
            IEnumerable<MerchItem> items,
            CancellationToken cancellationToken = default)
        {
            var merch = items.ToList();
            SetIds(merch);
            foreach (var option in merch.SelectMany(item => item.Options))
            {
                option.Id ??= Guid.NewGuid().ToString("N");
            }

            return AddRangeAsync(merch, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _fixture.DropDatabaseAsync(_connectionString);
        }

        private async Task AddRangeAsync<T>(
            IEnumerable<T> items,
            CancellationToken cancellationToken)
            where T : class
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            context.AddRange(items.Cast<object>());
            await context.SaveChangesAsync(cancellationToken);
        }

        private static void SetIds<T>(IEnumerable<T> items)
        {
            var id = typeof(T).GetProperty("Id");
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty((string)id.GetValue(item)))
                {
                    id.SetValue(item, Guid.NewGuid().ToString("N"));
                }
            }
        }
    }

    private sealed class PostgreSqlEcommerceContractDbContext : DbContext
    {
        public PostgreSqlEcommerceContractDbContext(
            DbContextOptions<PostgreSqlEcommerceContractDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CommonEf.CommonModelBuilderExtensions.AddHypnoTechCommonModel(modelBuilder);
            EcommerceEf.EcommerceModelBuilderExtensions.AddHypnoTechEcommerceModel(modelBuilder);
        }
    }

    private sealed class EcommerceContractDbContextFactory :
        IDbContextFactory<PostgreSqlEcommerceContractDbContext>
    {
        private readonly DbContextOptions<PostgreSqlEcommerceContractDbContext> _options;
        public EcommerceContractDbContextFactory(
            DbContextOptions<PostgreSqlEcommerceContractDbContext> options) => _options = options;
        public PostgreSqlEcommerceContractDbContext CreateDbContext() => new(_options);
        public Task<PostgreSqlEcommerceContractDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }

    private sealed class NeutralEcommerceDbContextFactory : IHypnoTechDbContextFactory
    {
        private readonly EcommerceContractDbContextFactory _factory;
        public NeutralEcommerceDbContextFactory(EcommerceContractDbContextFactory factory) =>
            _factory = factory;
        public async ValueTask<DbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            await _factory.CreateDbContextAsync(cancellationToken);
    }
}
