using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HypnoTech.Cms.EntityFrameworkCore
{
    public sealed class SeedRepository : ISeedRepository
    {
        private readonly IHypnoTechDbContextFactory _contextFactory;

        public SeedRepository(IHypnoTechDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task SeedAsync<T>(
            string collectionName,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            if (await ContainsAnyAsync<T>(context, cancellationToken))
            {
                return;
            }

            var json = await File.ReadAllTextAsync(
                Path.Combine("SeedData", $"{fileName}.json"),
                cancellationToken);
            var documents = JsonConvert.DeserializeObject<ICollection<T>>(json) ?? [];
            var persistenceOrder = DateTime.UtcNow.Ticks;
            foreach (var document in documents)
            {
                var id = document.GetType().GetProperty("Id");
                if (id?.PropertyType == typeof(string) && id.GetValue(document) == null)
                {
                    id.SetValue(document, Guid.NewGuid().ToString("N"));
                }

                var mapping = FormClassMapping.GetMapping<T>();
                if (mapping != null)
                {
                    foreach (var member in mapping.MemberMappings.OfType<GeneratedMemberMapping<T>>())
                    {
                        member.Generate(document);
                        document.GetType().GetProperty(member.MemberName)
                            ?.SetValue(document, member.GetPropertyValue(document));
                    }
                }

                var entry = context.Entry(document);
                if (entry.Metadata.FindProperty("PersistenceOrder") != null)
                {
                    entry.Property("PersistenceOrder").CurrentValue = persistenceOrder++;
                }

                context.Add(document);
                SetOwnedCollectionOrder(context, mapping, document);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static Task<bool> ContainsAnyAsync<T>(
            DbContext context,
            CancellationToken cancellationToken)
        {
            var method = typeof(SeedRepository)
                .GetMethod(nameof(ContainsAnyEntityAsync), BindingFlags.Static | BindingFlags.NonPublic)
                ?.MakeGenericMethod(typeof(T)) ??
                throw new InvalidOperationException("Seed query method could not be created.");
            return (Task<bool>)method.Invoke(null, [context, cancellationToken]);
        }

        private static Task<bool> ContainsAnyEntityAsync<TEntity>(
            DbContext context,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            return context.Set<TEntity>().AnyAsync(cancellationToken);
        }

        private static void SetOwnedCollectionOrder<T>(
            DbContext context,
            FormClassMapping mapping,
            T document)
        {
            if (mapping == null)
            {
                return;
            }

            foreach (var member in mapping.MemberMappings.Where(member =>
                         member is ListInputMapping || member is PhotoGalleryInputMapping))
            {
                if (member.GetPropertyValue(document) is not System.Collections.IEnumerable items)
                {
                    continue;
                }

                var order = 0;
                foreach (var item in items.Cast<object>())
                {
                    context.Entry(item).Property("PersistenceOrder").CurrentValue = order++;
                }
            }
        }
    }
}
