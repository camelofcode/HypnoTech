using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Ecommerce.Dal.EntityFrameworkCore
{
    public sealed class MerchCategoryTypeRepository : IMerchCategoryTypeRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public MerchCategoryTypeRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<List<MerchCategoryType>> GetTypesAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var categories = await context.Set<MerchCategoryType>().AsNoTracking()
                .Include(category => category.SubcategoryTypes)
                .OrderBy(category => category.Order)
                .ToListAsync(cancellationToken);
            foreach (var category in categories)
            {
                category.SubcategoryTypes = category.SubcategoryTypes.OrderBy(item => item.Order).ToList();
            }

            return categories;
        }
    }

    public sealed class MerchRepository : IMerchRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        private readonly IMerchCategoryTypeRepository _categories;

        public MerchRepository(
            IHypnoTechDbContextFactory factory,
            IMerchCategoryTypeRepository categories)
        {
            _factory = factory;
            _categories = categories;
        }

        public async Task<List<MerchCategory>> GetMerchAsync(
            CancellationToken cancellationToken = default)
        {
            var all = await GetMerchFlattenedAsync(cancellationToken);
            return all.GroupBy(item => item.Category)
                .Select(category => new MerchCategory
                {
                    Name = category.Key,
                    SubCategories = category.GroupBy(item => item.SubCategory)
                        .Select(subcategory => new MerchSubCategory
                        {
                            Name = subcategory.Key,
                            Items = subcategory.ToList()
                        })
                        .ToList()
                })
                .ToList();
        }

        public async Task<List<MerchItem>> GetMerchFlattenedAsync(
            CancellationToken cancellationToken = default)
        {
            var orderedSubcategories = (await _categories.GetTypesAsync(cancellationToken))
                .SelectMany(category => category.SubcategoryTypes)
                .Select(subcategory => subcategory.Name.ToLower())
                .ToList();
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var items = await context.Set<MerchItem>().AsNoTracking()
                .Include(item => item.Options)
                .ToListAsync(cancellationToken);
            return items.OrderBy(item => orderedSubcategories.IndexOf(item.SubCategory.ToLower())).ToList();
        }

        public async Task<MerchItem> GetMerchItemAsync(
            string slug,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<MerchItem>().AsNoTracking()
                .Include(item => item.Options)
                .SingleOrDefaultAsync(item => item.Slug == slug, cancellationToken);
        }
    }

    public sealed class PostageProfileRepository : IPostageProfileRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        private readonly IMerchRepository _merch;

        public PostageProfileRepository(
            IHypnoTechDbContextFactory factory,
            IMerchRepository merch)
        {
            _factory = factory;
            _merch = merch;
        }

        public async Task<PostageProfile> GetProfileAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<PostageProfile>().AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.Name == name, cancellationToken);
        }

        public async Task<List<PostageProfile>> GetProfilesAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var profiles = await context.Set<PostageProfile>().AsNoTracking().ToListAsync(cancellationToken);
            var merch = await _merch.GetMerchFlattenedAsync(cancellationToken);
            foreach (var profile in profiles)
            {
                profile.CanDelete = merch.All(item => !string.Equals(
                    item.PostageProfile,
                    profile.Name,
                    StringComparison.InvariantCultureIgnoreCase));
            }

            return profiles;
        }

        public async Task AddProfileAsync(
            PostageProfile profile,
            CancellationToken cancellationToken = default)
        {
            profile.Id = Guid.NewGuid().ToString("N");
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            context.Add(profile);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PostageProfile> GetProfileForEditAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<PostageProfile>().AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.Id == id, cancellationToken);
        }

        public async Task UpdateProfileAsync(
            PostageProfile profile,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var existing = await context.Set<PostageProfile>()
                .SingleAsync(item => item.Id == profile.Id, cancellationToken);
            context.Entry(existing.UnitedKingdom).CurrentValues.SetValues(profile.UnitedKingdom);
            context.Entry(existing.EuropeanUnion).CurrentValues.SetValues(profile.EuropeanUnion);
            context.Entry(existing.RestOfWorld1).CurrentValues.SetValues(profile.RestOfWorld1);
            context.Entry(existing.RestOfWorld2).CurrentValues.SetValues(profile.RestOfWorld2);
            context.Entry(existing.RestOfWorld3).CurrentValues.SetValues(profile.RestOfWorld3);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> RemoveProfileAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var profile = await context.Set<PostageProfile>().AsNoTracking()
                .SingleAsync(item => item.Id == id, cancellationToken);
            var normalized = profile.Name.ToLower();
            if (await context.Set<MerchItem>().AnyAsync(
                    item => item.PostageProfile.ToLower() == normalized,
                    cancellationToken))
            {
                return $"'{profile.Name}' cannot be removed as it has merch associated. " +
                       "Move all merch to a different profile before removing.";
            }

            await context.Set<PostageProfile>().Where(item => item.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
            return null;
        }
    }
}
