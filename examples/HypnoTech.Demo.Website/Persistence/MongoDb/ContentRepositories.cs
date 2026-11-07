using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Demo.Website.Persistence;

internal sealed class MongoArticleRepository : BaseMongoRepository, IArticleRepository
{
    public MongoArticleRepository(IOptions<MongoOptions> options)
        : base(options, "demoArticles")
    {
    }

    public Task<ICollection<Article>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Article>(cancellationToken: cancellationToken);

    public Task<Article> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        GetOneAsync<Article>(new BsonDocument("slug", slug), cancellationToken);
}

internal sealed class MongoArtistRepository : BaseMongoRepository, IArtistRepository
{
    public MongoArtistRepository(IOptions<MongoOptions> options)
        : base(options, "demoArtists")
    {
    }

    public Task<ICollection<Artist>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Artist>(cancellationToken: cancellationToken);
}

internal sealed class MongoEventRepository : BaseMongoRepository, IEventRepository
{
    public MongoEventRepository(IOptions<MongoOptions> options)
        : base(options, "demoEvents")
    {
    }

    public Task<ICollection<Event>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync<Event>(cancellationToken: cancellationToken);

    public Task<Event> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        GetOneAsync<Event>(new BsonDocument("slug", slug), cancellationToken);
}
