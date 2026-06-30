using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface ICountryRepository
    {
        List<Country> GetCountries();

        Country GetCountryByCode(string code);
    }
}
