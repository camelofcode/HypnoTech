using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Persistence.ContractTests.Ecommerce;

public interface IEcommerceRepositoryContractContext : IAsyncDisposable
{
    string ProviderName { get; }

    IMerchCategoryTypeRepository MerchCategoryTypeRepository { get; }

    IMerchRepository MerchRepository { get; }

    IPostageProfileRepository PostageProfileRepository { get; }

    IOrderRepository OrderRepository { get; }

    IDownloadCodeRepository DownloadCodeRepository { get; }

    Task SeedCountriesAsync(
        IEnumerable<Country> countries,
        CancellationToken cancellationToken = default);

    Task SeedMerchCategoryTypesAsync(
        IEnumerable<MerchCategoryType> categoryTypes,
        CancellationToken cancellationToken = default);

    Task SeedMerchItemsAsync(
        IEnumerable<MerchItem> items,
        CancellationToken cancellationToken = default);
}
