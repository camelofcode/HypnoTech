using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface ICountryRepository
    {
        List<Country> GetCountries();
        Task<List<Country>> GetCountriesAsync(CancellationToken cancellationToken = default);

        Country GetCountryByCode(string code);
        Task<Country> GetCountryByCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
