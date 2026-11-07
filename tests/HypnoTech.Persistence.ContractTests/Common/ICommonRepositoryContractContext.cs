using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Persistence.ContractTests.Common;

public interface ICommonRepositoryContractContext : IAsyncDisposable
{
    string ProviderName { get; }

    INavBarRepository NavBarRepository { get; }

    INavBarAdminRepository NavBarAdminRepository { get; }

    IUserRepository UserRepository { get; }

    IAccountRepository AccountRepository { get; }

    IConfigAdminRepository ConfigAdminRepository { get; }

    ICountryRepository CountryRepository { get; }

    IEmailTemplateRepository EmailTemplateRepository { get; }

    IEmailTemplateAdminRepository EmailTemplateAdminRepository { get; }

    IPasswordResetRepository PasswordResetRepository { get; }

    ILoggingRepository LoggingRepository { get; }

    IBackupRespository BackupRepository { get; }

    Task SeedNavBarLinksAsync(
        IEnumerable<NavBarLink> links,
        CancellationToken cancellationToken = default);

    Task SeedAsync<T>(
        string logicalStore,
        IEnumerable<T> items,
        CancellationToken cancellationToken = default);
}
