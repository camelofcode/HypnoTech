using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Demo.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Demo.Website.Dal;

public interface IArticleRepository
{
    ICollection<Article> GetAll();

    Task<ICollection<Article>> GetAllAsync(CancellationToken cancellationToken = default);

    Article GetBySlug(string slug);

    Task<Article> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class ArticleRepository : BaseMongoRepository, IArticleRepository
{
    public ArticleRepository(IOptions<MongoOptions> options)
        : base(options, "demoArticles")
    {
    }

    public ICollection<Article> GetAll() => GetAll<Article>();

    public Task<ICollection<Article>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Article>(cancellationToken: cancellationToken);

    public Article GetBySlug(string slug) => GetOne<Article>(new BsonDocument("slug", slug));

    public Task<Article> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        GetOneAsync<Article>(new BsonDocument("slug", slug), cancellationToken);
}

public interface IArtistRepository
{
    ICollection<Artist> GetAll();

    Task<ICollection<Artist>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class ArtistRepository : BaseMongoRepository, IArtistRepository
{
    public ArtistRepository(IOptions<MongoOptions> options)
        : base(options, "demoArtists")
    {
    }

    public ICollection<Artist> GetAll() => GetAll<Artist>();

    public Task<ICollection<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Artist>(cancellationToken: cancellationToken);
}

public interface IEventRepository
{
    ICollection<Event> GetAll();

    Task<ICollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);

    Event GetBySlug(string slug);

    Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class EventRepository : BaseMongoRepository, IEventRepository
{
    public EventRepository(IOptions<MongoOptions> options)
        : base(options, "demoEvents")
    {
    }

    public ICollection<Event> GetAll() => GetAll<Event>();

    public Task<ICollection<Event>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Event>(cancellationToken: cancellationToken);

    public Event GetBySlug(string slug) => GetOne<Event>(new BsonDocument("slug", slug));

    public Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        GetOneAsync<Event>(new BsonDocument("slug", slug), cancellationToken);
}
