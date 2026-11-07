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
    Task<ICollection<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Article> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class ArticleRepository : BaseMongoRepository, IArticleRepository
{
    public ArticleRepository(IOptions<MongoOptions> options)
        : base(options, "demoArticles")
    {
    }

    public Task<ICollection<Article>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Article>(cancellationToken: cancellationToken);

    public Task<Article> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        GetOneAsync<Article>(new BsonDocument("slug", slug), cancellationToken);
}

public interface IArtistRepository
{
    Task<ICollection<Artist>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class ArtistRepository : BaseMongoRepository, IArtistRepository
{
    public ArtistRepository(IOptions<MongoOptions> options)
        : base(options, "demoArtists")
    {
    }

    public Task<ICollection<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Artist>(cancellationToken: cancellationToken);
}

public interface IEventRepository
{
    Task<ICollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class EventRepository : BaseMongoRepository, IEventRepository
{
    public EventRepository(IOptions<MongoOptions> options)
        : base(options, "demoEvents")
    {
    }

    public Task<ICollection<Event>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Event>(cancellationToken: cancellationToken);

    public Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        GetOneAsync<Event>(new BsonDocument("slug", slug), cancellationToken);
}
