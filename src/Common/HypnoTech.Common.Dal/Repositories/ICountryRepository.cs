using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetCountriesAsync(CancellationToken cancellationToken = default);
        Task<Country> GetCountryByCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
