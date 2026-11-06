using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ICmsRepository<TClass> where TClass : class
    {
        ICollection<TClass> GetAll<T>(string orderBy = null, bool descending = false);

        Task<ICollection<TClass>> GetAllAsync<T>(string orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        CmsListReponse<TClass> GetPaged<T>(CmsListRequest request);

        Task<CmsListReponse<TClass>> GetPagedAsync<T>(CmsListRequest request, CancellationToken cancellationToken = default);

        void MoveUp(string id);

        Task MoveUpAsync(string id, CancellationToken cancellationToken = default);

        void MoveDown(string id);

        Task MoveDownAsync(string id, CancellationToken cancellationToken = default);

        TClass GetById(string id);

        Task<TClass> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        void Add(TClass model);

        Task AddAsync(TClass model, CancellationToken cancellationToken = default);

        void Edit(TClass model);

        Task EditAsync(TClass model, CancellationToken cancellationToken = default);

        void Remove(string id);

        Task RemoveAsync(string id, CancellationToken cancellationToken = default);
    }
}
