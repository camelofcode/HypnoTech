using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Common.Models;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Cms.EntityFrameworkCore
{
    public sealed class SelectListRepository : ISelectListRepository
    {
        private readonly IHypnoTechDbContextFactory _contextFactory;

        public SelectListRepository(IHypnoTechDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<List<object>> GetSelectListItemsAsync(
            Type type,
            CancellationToken cancellationToken = default)
        {
            if (!ModelRegistry.Registrations.Any(registration => registration.Type == type))
            {
                throw new InvalidOperationException(
                    $"Type '{type.FullName}' is not registered as a HypnoTech model.");
            }

            var method = typeof(SelectListRepository)
                .GetMethod(nameof(GetAllAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                ?.MakeGenericMethod(type) ??
                throw new InvalidOperationException("Select-list query method could not be created.");
            return (Task<List<object>>)method.Invoke(this, [cancellationToken]);
        }

        private async Task<List<object>> GetAllAsync<T>(CancellationToken cancellationToken)
            where T : class
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return (await context.Set<T>().AsNoTracking().ToListAsync(cancellationToken))
                .Cast<object>()
                .ToList();
        }
    }
}
