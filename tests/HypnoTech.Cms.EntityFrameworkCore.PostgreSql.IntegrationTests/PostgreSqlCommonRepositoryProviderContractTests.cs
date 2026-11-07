using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.EntityFrameworkCore;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using HypnoTech.EntityFrameworkCore;
using HypnoTech.Persistence.ContractTests.Common;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

[Xunit.Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlCommonRepositoryProviderContractTests :
    CommonRepositoryProviderContractTests
{
    private readonly IReadOnlyCollection<IRelationalDatabaseFixture> _fixtures;

    public PostgreSqlCommonRepositoryProviderContractTests(
        PostgreSqlFixture postgreSql,
        SqlServerFixture sqlServer)
    {
        _fixtures = [postgreSql, sqlServer];
        ModelRegistry.Register(typeof(Country), "countries", "Countries");
        ModelRegistry.Register(typeof(Config), "config", "Config");
        ModelRegistry.Register(typeof(EmailTemplate), "emailtemplates", "Email Templates");
    }

    protected override IEnumerable<ICommonRepositoryContractContext> CreateContractContexts()
    {
        foreach (var fixture in _fixtures)
        {
            yield return PostgreSqlCommonRepositoryContractContext.CreateAsync(fixture)
                .GetAwaiter()
                .GetResult();
        }
    }

    private sealed class PostgreSqlCommonRepositoryContractContext :
        ICommonRepositoryContractContext
    {
        private readonly IRelationalDatabaseFixture _fixture;
        private readonly string _connectionString;
        private readonly IHypnoTechDbContextFactory _contextFactory;

        private PostgreSqlCommonRepositoryContractContext(
            IRelationalDatabaseFixture fixture,
            string connectionString,
            IHypnoTechDbContextFactory contextFactory)
        {
            _fixture = fixture;
            _connectionString = connectionString;
            _contextFactory = contextFactory;
            NavBarRepository = new NavBarRepository(contextFactory);
            NavBarAdminRepository = new NavBarAdminRepository(contextFactory);
            UserRepository = new UserRepository(contextFactory);
            AccountRepository = new AccountRepository(contextFactory);
            ConfigAdminRepository = new ConfigAdminRepository(contextFactory);
            CountryRepository = new CountryRepository(contextFactory);
            EmailTemplateRepository = new EmailTemplateRepository(contextFactory);
            EmailTemplateAdminRepository = new EmailTemplateAdminRepository(contextFactory);
            PasswordResetRepository = new PasswordResetRepository(contextFactory);
            LoggingRepository = new LoggingRepository(contextFactory);
            BackupRepository = new BackupRepository(contextFactory);
        }

        public string ProviderName => _fixture.ProviderName;
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

        public static async Task<PostgreSqlCommonRepositoryContractContext> CreateAsync(
            IRelationalDatabaseFixture fixture)
        {
            var connectionString = await fixture.CreateDatabaseAsync();
            var options = fixture.Configure(
                    new DbContextOptionsBuilder<PostgreSqlCommonContractDbContext>(),
                    connectionString)
                .Options;
            var factory = new CommonContractDbContextFactory(options);
            await using (var context = await factory.CreateDbContextAsync())
            {
                await context.Database.EnsureCreatedAsync();
            }

            return new PostgreSqlCommonRepositoryContractContext(
                fixture,
                connectionString,
                new NeutralCommonDbContextFactory(factory));
        }

        public Task SeedNavBarLinksAsync(
            IEnumerable<NavBarLink> links,
            CancellationToken cancellationToken = default)
        {
            return SeedAsync("navbar", links, cancellationToken);
        }

        public async Task SeedAsync<T>(
            string logicalStore,
            IEnumerable<T> items,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            foreach (var item in items)
            {
                var id = typeof(T).GetProperty("Id");
                if (id?.PropertyType == typeof(string) && string.IsNullOrEmpty((string)id.GetValue(item)))
                {
                    id.SetValue(item, Guid.NewGuid().ToString("N"));
                }
            }

            context.AddRange(items.Cast<object>());
            await context.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _fixture.DropDatabaseAsync(_connectionString);
        }
    }

    private sealed class PostgreSqlCommonContractDbContext : DbContext
    {
        public PostgreSqlCommonContractDbContext(
            DbContextOptions<PostgreSqlCommonContractDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddHypnoTechCommonModel();
        }
    }

    private sealed class CommonContractDbContextFactory :
        IDbContextFactory<PostgreSqlCommonContractDbContext>
    {
        private readonly DbContextOptions<PostgreSqlCommonContractDbContext> _options;
        public CommonContractDbContextFactory(
            DbContextOptions<PostgreSqlCommonContractDbContext> options) => _options = options;
        public PostgreSqlCommonContractDbContext CreateDbContext() => new(_options);
        public Task<PostgreSqlCommonContractDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }

    private sealed class NeutralCommonDbContextFactory : IHypnoTechDbContextFactory
    {
        private readonly CommonContractDbContextFactory _factory;
        public NeutralCommonDbContextFactory(CommonContractDbContextFactory factory) => _factory = factory;
        public async ValueTask<DbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            await _factory.CreateDbContextAsync(cancellationToken);
    }
}
