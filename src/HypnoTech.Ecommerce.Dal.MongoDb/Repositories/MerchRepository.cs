using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class MerchRepository : BaseMongoRepository, IMerchRepository
    {
        private readonly List<string> _orderedSubcategoryTypes = new List<string>();

        public MerchRepository(IOptions<MongoOptions> config, IMerchCategoryTypeRepository merchCategoryTypeRepository) : base(config, "merches")
        {
            var types = merchCategoryTypeRepository.GetTypes();
            foreach (var categoryType in types.OrderBy(p => p.Order))
            {
                foreach (var subcategoryType in categoryType.SubcategoryTypes.OrderBy(p => p.Order))
                {
                    _orderedSubcategoryTypes.Add(subcategoryType.Name.ToLower());
                }
            }
        }

        public List<MerchCategory> GetMerch()
        {
            var allMerch = GetAll<MerchItem>().ToList();

            var groupedByCategory = allMerch
                .OrderBy(p => _orderedSubcategoryTypes.IndexOf(p.SubCategory.ToLower()))
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
            return GetAll<MerchItem>()
                .OrderBy(p => _orderedSubcategoryTypes.IndexOf(p.SubCategory.ToLower()))
                .ToList();
        }

        public MerchItem GetMerchItem(string slug)
        {
            return GetOne<MerchItem>(new BsonDocument("slug", slug));
        }
    }
}
