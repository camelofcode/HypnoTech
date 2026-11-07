using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ICmsRepository<TClass> where TClass : class
    {
        Task<ICollection<TClass>> GetAllAsync<T>(string orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        Task<CmsListReponse<TClass>> GetPagedAsync<T>(CmsListRequest request, CancellationToken cancellationToken = default);

        Task MoveUpAsync(string id, CancellationToken cancellationToken = default);

        Task MoveDownAsync(string id, CancellationToken cancellationToken = default);

        Task<TClass> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        Task AddAsync(TClass model, CancellationToken cancellationToken = default);

        Task EditAsync(TClass model, CancellationToken cancellationToken = default);

        Task RemoveAsync(string id, CancellationToken cancellationToken = default);
    }
}
