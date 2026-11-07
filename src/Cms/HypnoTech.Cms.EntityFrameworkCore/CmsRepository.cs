using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Dtos.Response;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Common;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Cms.EntityFrameworkCore
{
    public sealed class CmsRepository<TClass> : ICmsRepository<TClass>
        where TClass : class
    {
        private readonly IHypnoTechDbContextFactory _contextFactory;

        public CmsRepository(IHypnoTechDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task MoveUpAsync(string id, CancellationToken cancellationToken = default)
        {
            return MoveAsync(id, -1, cancellationToken);
        }

        public Task MoveDownAsync(string id, CancellationToken cancellationToken = default)
        {
            return MoveAsync(id, 1, cancellationToken);
        }

        public async Task<TClass> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var mapping = FormClassMapping.GetMapping<TClass>();
            var model = await QueryWithChildren(context, mapping, tracking: true)
                .SingleOrDefaultAsync(
                    model => EF.Property<string>(model, "Id") == id,
                    cancellationToken);
            OrderOwnedCollections(context, mapping, model);
            return model;
        }

        public async Task AddAsync(TClass model, CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            mapping.SetId(model, Guid.NewGuid().ToString("N"));
            SetNestedIds(model, mapping);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            SetPersistenceOrder(context, model, DateTime.UtcNow.Ticks);
            context.Add(model);
            SetOwnedCollectionOrder(context, mapping, model);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task EditAsync(TClass model, CancellationToken cancellationToken = default)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            SetNestedIds(model, mapping);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var id = mapping.GetId(model);
            var existing = await QueryWithChildren(context, mapping, tracking: true)
                .SingleAsync(
                    item => EF.Property<string>(item, "Id") == id,
                    cancellationToken);

            context.Entry(existing).CurrentValues.SetValues(model);
            ReplaceOwnedCollections(context, mapping, existing, model);
            context.ChangeTracker.DetectChanges();
            SetOwnedCollectionOrder(context, mapping, existing);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var mapping = FormClassMapping.GetMapping<TClass>();
            var existing = await QueryWithChildren(context, mapping, tracking: true)
                .SingleOrDefaultAsync(
                    model => EF.Property<string>(model, "Id") == id,
                    cancellationToken);
            if (existing == null)
            {
                return;
            }

            context.Remove(existing);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ICollection<TClass>> GetAllAsync<T>(
            string orderBy = null,
            bool descending = false,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var mapping = FormClassMapping.GetMapping<TClass>();
            var query = QueryWithChildren(context, mapping, tracking: true);
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                query = ApplyOrder(query, orderBy, descending);
            }

            var items = await query.ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                OrderOwnedCollections(context, mapping, item);
            }

            return items;
        }

        public async Task<CmsListReponse<TClass>> GetPagedAsync<T>(
            CmsListRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var mapping = FormClassMapping.GetMapping<TClass>();
            ApplyDefaultSortIfUnset(request, mapping);

            var query = QueryWithChildren(context, mapping, tracking: true);
            query = ApplySearch(query, request, mapping);
            var count = await query.CountAsync(cancellationToken);

            if (mapping.Ordered)
            {
                query = ApplyOrder(query, mapping.OrderProperty, descending: false);
            }
            else
            {
                var column = mapping.MemberMappings.First(member => member.TableOrder == request.SortColumn);
                if (request.Direction == SortType.Unsorted)
                {
                    request.Direction = column.MemberType == typeof(DateTime) ||
                                        column.MemberType == typeof(DateTime?)
                        ? SortType.Descending
                        : SortType.Ascending;
                }

                query = ApplyOrder(query, column.MemberName, request.IsDescending);
            }

            var items = await query.Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                OrderOwnedCollections(context, mapping, item);
            }

            return new CmsListReponse<TClass>
            {
                SortColumn = request.SortColumn ?? 0,
                Direction = request.Direction,
                Search = request.Search,
                CurrentPage = request.Page,
                Items = items,
                TotalPages = (int)Math.Ceiling(count / (decimal)request.PageSize)
            };
        }

        private async Task MoveAsync(
            string id,
            int offset,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.Ordered)
            {
                throw new InvalidOperationException("The CMS model is not configured for ordering.");
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            var set = context.Set<TClass>();
            var current = await set.SingleAsync(
                model => EF.Property<string>(model, "Id") == id,
                cancellationToken);
            var orderProperty = mapping.ClassType.GetProperty(mapping.OrderProperty) ??
                                throw new InvalidOperationException("Order property is unavailable.");
            var currentOrder = (int)orderProperty.GetValue(current);
            var targetOrder = currentOrder + offset;
            var adjacent = await set.SingleOrDefaultAsync(
                model => EF.Property<int>(model, mapping.OrderProperty) == targetOrder,
                cancellationToken);
            if (adjacent == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            orderProperty.SetValue(current, targetOrder);
            orderProperty.SetValue(adjacent, currentOrder);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private static IQueryable<TClass> QueryWithChildren(
            DbContext context,
            FormClassMapping mapping,
            bool tracking = false)
        {
            IQueryable<TClass> query = context.Set<TClass>();
            foreach (var child in mapping.Children)
            {
                query = query.Include(child.TypeInfo.Name);
            }

            return tracking ? query : query.AsNoTracking();
        }

        private static IQueryable<TClass> ApplySearch(
            IQueryable<TClass> query,
            CmsListRequest request,
            FormClassMapping mapping)
        {
            if (!request.HasSearch)
            {
                return query;
            }

            var searchable = mapping.MemberMappings
                .Where(member =>
                    member.IsSearchable &&
                    member.MemberType == typeof(string) &&
                    !member.AccessPath.Contains('.', StringComparison.Ordinal))
                .ToList();
            if (searchable.Count == 0)
            {
                return query.Where(_ => false);
            }

            var parameter = Expression.Parameter(typeof(TClass), "model");
            Expression body = null;
            var search = Expression.Constant(request.Search.ToLowerInvariant());
            var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
            var contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);

            foreach (var member in searchable)
            {
                var property = Expression.Property(parameter, member.MemberName);
                var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
                var lowered = Expression.Call(property, toLower);
                var match = Expression.Call(lowered, contains, search);
                var candidate = Expression.AndAlso(notNull, match);
                body = body == null ? candidate : Expression.OrElse(body, candidate);
            }

            return query.Where(Expression.Lambda<Func<TClass, bool>>(body, parameter));
        }

        private static IQueryable<TClass> ApplyOrder(
            IQueryable<TClass> query,
            string propertyName,
            bool descending)
        {
            var property = typeof(TClass).GetProperty(propertyName) ??
                           throw new InvalidOperationException(
                               $"CMS sort property '{propertyName}' was not found on '{typeof(TClass).FullName}'.");
            var parameter = Expression.Parameter(typeof(TClass), "model");
            var selector = Expression.Lambda(Expression.Property(parameter, property), parameter);
            var method = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var call = Expression.Call(
                typeof(Queryable),
                method,
                [typeof(TClass), property.PropertyType],
                query.Expression,
                Expression.Quote(selector));
            return query.Provider.CreateQuery<TClass>(call);
        }

        private static void ApplyDefaultSortIfUnset(
            CmsListRequest request,
            FormClassMapping mapping)
        {
            if (request.SortColumn.HasValue)
            {
                return;
            }

            if (string.IsNullOrEmpty(mapping.SortProperty))
            {
                request.SortColumn = 0;
                return;
            }

            var member = mapping.GetMember(mapping.SortProperty);
            request.SortColumn = member.TableOrder ?? mapping.MemberMappings.ToList().IndexOf(member);
            request.Direction = mapping.SortAscending ? SortType.Ascending : SortType.Descending;
        }

        private static void SetNestedIds(TClass model, FormClassMapping mapping)
        {
            foreach (var member in mapping.MemberMappings.OfType<ListInputMapping>())
            {
                member.SetIds(member.GetPropertyValue(model), () => Guid.NewGuid().ToString("N"));
            }

            foreach (var member in mapping.MemberMappings.OfType<PhotoGalleryInputMapping>())
            {
                member.SetIds(member.GetPropertyValue(model), () => Guid.NewGuid().ToString("N"));
            }
        }

        private static void ReplaceOwnedCollections(
            DbContext context,
            FormClassMapping mapping,
            TClass existing,
            TClass replacement)
        {
            foreach (var member in mapping.MemberMappings.Where(member =>
                         member is ListInputMapping || member is PhotoGalleryInputMapping))
            {
                var property = mapping.ClassType.GetProperty(member.MemberName);
                if (property == null)
                {
                    continue;
                }

                if (property.GetValue(existing) is IEnumerable oldItems)
                {
                    context.RemoveRange(oldItems.Cast<object>());
                }

                property.SetValue(existing, property.GetValue(replacement));
            }
        }

        private static void SetPersistenceOrder(DbContext context, object model, long value)
        {
            var entry = context.Entry(model);
            if (entry.Metadata.FindProperty("PersistenceOrder") != null)
            {
                entry.Property("PersistenceOrder").CurrentValue = value;
            }
        }

        private static void SetOwnedCollectionOrder(
            DbContext context,
            FormClassMapping mapping,
            TClass model)
        {
            foreach (var member in mapping.MemberMappings.Where(member =>
                         member is ListInputMapping || member is PhotoGalleryInputMapping))
            {
                if (member.GetPropertyValue(model) is not IEnumerable items)
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

        private static void OrderOwnedCollections(
            DbContext context,
            FormClassMapping mapping,
            TClass model)
        {
            if (model == null)
            {
                return;
            }

            foreach (var member in mapping.MemberMappings.Where(member =>
                         member is ListInputMapping || member is PhotoGalleryInputMapping))
            {
                if (member.GetPropertyValue(model) is not IList items)
                {
                    continue;
                }

                var ordered = items.Cast<object>()
                    .OrderBy(item => Convert.ToInt32(
                        context.Entry(item).Property("PersistenceOrder").CurrentValue))
                    .ToList();
                items.Clear();
                foreach (var item in ordered)
                {
                    items.Add(item);
                }
            }
        }
    }
}
