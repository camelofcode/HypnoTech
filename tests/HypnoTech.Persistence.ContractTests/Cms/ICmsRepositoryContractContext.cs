using System;
using HypnoTech.Cms.Contracts.Interfaces;

namespace HypnoTech.Persistence.ContractTests.Cms;

public interface ICmsRepositoryContractContext : IAsyncDisposable
{
    string ProviderName { get; }

    ISelectListRepository SelectListRepository { get; }

    ISeedRepository SeedRepository { get; }

    string CreateIdentifier();

    ICmsRepository<TModel> CreateRepository<TModel>() where TModel : class;

    System.Threading.Tasks.Task SeedChildAsync(
        CmsContractChild child,
        System.Threading.CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task SeedChoiceAsync(
        CmsContractChoice choice,
        System.Threading.CancellationToken cancellationToken = default);
}
