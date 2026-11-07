using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Ecommerce.Dal.EntityFrameworkCore
{
    public sealed class DownloadCodeRepository : IDownloadCodeRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public DownloadCodeRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<AssignDownloadCodesResponse> AssignDownloadCodesAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            var assigned = new List<AssignedDownloadCode>();
            var expired = new List<string>();
            foreach (var collectionId in order.DownloadCodeIds.Distinct())
            {
                var code = await AssignAsync(collectionId, order.EmailAddress, cancellationToken);
                if (code == null)
                {
                    continue;
                }

                assigned.Add(code.Value.Assigned);
                if (code.Value.Exhausted)
                {
                    expired.Add(code.Value.Assigned.Name);
                }
            }

            return new AssignDownloadCodesResponse
            {
                AssignedDownloadCodes = assigned,
                ExpiredCollections = expired
            };
        }

        public async Task<List<DownloadCodeCollection>> GetAllCodeCollectionsAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<DownloadCodeCollection>().AsNoTracking()
                .Include(collection => collection.Codes)
                .ToListAsync(cancellationToken);
        }

        public async Task AddCodeCollectionAsync(
            DownloadCodeCollection codes,
            CancellationToken cancellationToken = default)
        {
            codes.Id = System.Guid.NewGuid().ToString("N");
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            context.Add(codes);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DownloadCodeCollection> GetDownloadCodeCollectionAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<DownloadCodeCollection>().AsNoTracking()
                .Include(collection => collection.Codes)
                .SingleOrDefaultAsync(collection => collection.Id == id, cancellationToken);
        }

        public async Task<string> AssignDownloadCodeAsync(
            string collectionId,
            string emailAddress,
            CancellationToken cancellationToken = default)
        {
            return (await AssignAsync(collectionId, emailAddress, cancellationToken))?.Assigned.Code ??
                   string.Empty;
        }

        private async Task<(AssignedDownloadCode Assigned, bool Exhausted)?> AssignAsync(
            string collectionId,
            string emailAddress,
            CancellationToken cancellationToken)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var collection = await context.Set<DownloadCodeCollection>()
                .Include(item => item.Codes)
                .SingleAsync(item => item.Id == collectionId, cancellationToken);
            var code = collection.Codes.FirstOrDefault(item => !item.Assigned);
            if (code == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            code.AssignedTo = emailAddress;
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return (new AssignedDownloadCode
            {
                Code = code.Code,
                Name = collection.Name,
                RedemptionUrl = collection.RedemptionUrl
            }, collection.UnusedCodes == 0);
        }
    }
}
