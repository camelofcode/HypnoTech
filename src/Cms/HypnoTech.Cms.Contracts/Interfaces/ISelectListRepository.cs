using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ISelectListRepository
    {
        List<object> GetSelectListItems(Type type);

        Task<List<object>> GetSelectListItemsAsync(Type type, CancellationToken cancellationToken = default);
    }
}
