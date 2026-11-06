using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class MerchRepository : BaseMongoRepository, IMerchRepository
    {
        private readonly IMerchCategoryTypeRepository _merchCategoryTypeRepository;

        public MerchRepository(IOptions<MongoOptions> config, IMerchCategoryTypeRepository merchCategoryTypeRepository) : base(config, "merches")
        {
            _merchCategoryTypeRepository = merchCategoryTypeRepository;
        }

        public List<MerchCategory> GetMerch()
        {
            var allMerch = GetAll<MerchItem>().ToList();
            var orderedSubcategoryTypes = GetOrderedSubcategoryTypes(
                _merchCategoryTypeRepository.GetTypes());

            var groupedByCategory = allMerch
                .OrderBy(p => orderedSubcategoryTypes.IndexOf(p.SubCategory.ToLower()))
                .GroupBy(p => p.Category);

            return (from grouping in groupedByCategory
                let groupedBySubCategory = grouping.GroupBy(p => p.SubCategory)
                select new MerchCategory
                {
                    Name = grouping.Key,
                    SubCategories = groupedBySubCategory.Select(p => new MerchSubCategory
                        {
                            Name = p.Key,
                            Items = p.ToList()
                        })
                        .ToList()
                }).ToList();
        }

        public async Task<List<MerchCategory>> GetMerchAsync(CancellationToken cancellationToken = default)
        {
            var allMerch = (await GetAllAsync<MerchItem>(cancellationToken: cancellationToken)).ToList();
            var orderedSubcategoryTypes = GetOrderedSubcategoryTypes(
                await _merchCategoryTypeRepository.GetTypesAsync(cancellationToken));

            var groupedByCategory = allMerch
                .OrderBy(p => orderedSubcategoryTypes.IndexOf(p.SubCategory.ToLower()))
                .GroupBy(p => p.Category);

            return (from grouping in groupedByCategory
                    let groupedBySubCategory = grouping.GroupBy(p => p.SubCategory)
                    select new MerchCategory
                    {
                        Name = grouping.Key,
                        SubCategories = groupedBySubCategory.Select(p => new MerchSubCategory
                            {
                                Name = p.Key,
                                Items = p.ToList()
                            })
                            .ToList()
                    }).ToList();
        }

        public List<MerchItem> GetMerchFlattened()
        {
            var orderedSubcategoryTypes = GetOrderedSubcategoryTypes(
                _merchCategoryTypeRepository.GetTypes());
            return GetAll<MerchItem>()
                .OrderBy(p => orderedSubcategoryTypes.IndexOf(p.SubCategory.ToLower()))
                .ToList();
        }

        public async Task<List<MerchItem>> GetMerchFlattenedAsync(CancellationToken cancellationToken = default)
        {
            var orderedSubcategoryTypes = GetOrderedSubcategoryTypes(
                await _merchCategoryTypeRepository.GetTypesAsync(cancellationToken));
            return (await GetAllAsync<MerchItem>(cancellationToken: cancellationToken))
                .OrderBy(p => orderedSubcategoryTypes.IndexOf(p.SubCategory.ToLower()))
                .ToList();
        }

        public MerchItem GetMerchItem(string slug)
        {
            return GetOne<MerchItem>(new BsonDocument("slug", slug));
        }

        public Task<MerchItem> GetMerchItemAsync(string slug, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<MerchItem>(new BsonDocument("slug", slug), cancellationToken);
        }

        private static List<string> GetOrderedSubcategoryTypes(List<MerchCategoryType> types)
        {
            return types
                .OrderBy(category => category.Order)
                .SelectMany(category => category.SubcategoryTypes.OrderBy(subcategory => subcategory.Order))
                .Select(subcategory => subcategory.Name.ToLower())
                .ToList();
        }
    }
}
