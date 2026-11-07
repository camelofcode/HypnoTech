using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Demo.Website.Persistence;

internal sealed class EntityFrameworkCoreArticleRepository : IArticleRepository
{
    private readonly IHypnoTechDbContextFactory _factory;

    public EntityFrameworkCoreArticleRepository(IHypnoTechDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<ICollection<Article>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Set<Article>().AsNoTracking()
            .OrderByDescending(article => article.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Article> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Set<Article>().AsNoTracking()
            .SingleOrDefaultAsync(article => article.Slug == slug, cancellationToken);
    }
}

internal sealed class EntityFrameworkCoreArtistRepository : IArtistRepository
{
    private readonly IHypnoTechDbContextFactory _factory;

    public EntityFrameworkCoreArtistRepository(IHypnoTechDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<ICollection<Artist>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Set<Artist>().AsNoTracking()
            .OrderBy(artist => artist.Order)
            .ToListAsync(cancellationToken);
    }
}

internal sealed class EntityFrameworkCoreEventRepository : IEventRepository
{
    private readonly IHypnoTechDbContextFactory _factory;

    public EntityFrameworkCoreEventRepository(IHypnoTechDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<ICollection<Event>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var items = await context.Set<Event>().AsNoTracking()
            .Include(item => item.Schedule)
            .OrderBy(item => item.StartsAt)
            .ToListAsync(cancellationToken);
        OrderSchedules(items);
        return items;
    }

    public async Task<Event> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        var item = await context.Set<Event>().AsNoTracking()
            .Include(entry => entry.Schedule)
            .SingleOrDefaultAsync(entry => entry.Slug == slug, cancellationToken);
        if (item != null)
        {
            item.Schedule = item.Schedule.OrderBy(entry => entry.Order).ToList();
        }

        return item;
    }

    private static void OrderSchedules(IEnumerable<Event> items)
    {
        foreach (var item in items)
        {
            item.Schedule = item.Schedule.OrderBy(entry => entry.Order).ToList();
        }
    }
}
