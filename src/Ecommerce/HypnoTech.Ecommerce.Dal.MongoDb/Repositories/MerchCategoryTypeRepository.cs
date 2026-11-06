using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class MerchCategoryTypeRepository : BaseMongoRepository, IMerchCategoryTypeRepository
    {
        public MerchCategoryTypeRepository(IOptions<MongoOptions> config) : base(config, "merchcategorytypes")
        {
        }

        public List<MerchCategoryType> GetTypes()
        {
            return GetAll<MerchCategoryType>().ToList();
        }

        public async Task<List<MerchCategoryType>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<MerchCategoryType>(cancellationToken: cancellationToken)).ToList();
        }
    }
}
