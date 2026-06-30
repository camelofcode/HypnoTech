using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using System.Collections.Generic;

namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ICmsRepository<TClass> where TClass : class
    {
        ICollection<TClass> GetAll<T>(string orderBy = null, bool descending = false);

        CmsListReponse<TClass> GetPaged<T>(CmsListRequest request);

        void MoveUp(string id);

        void MoveDown(string id);

        TClass GetById(string id);

        void Add(TClass model);

        void Edit(TClass model);

        void Remove(string id);
    }
}
