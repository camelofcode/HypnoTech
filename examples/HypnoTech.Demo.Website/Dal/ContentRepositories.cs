using System.Collections.Generic;
using HypnoTech.Demo.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Demo.Website.Dal;

public interface IArticleRepository
{
    ICollection<Article> GetAll();

    Article GetBySlug(string slug);
}

public sealed class ArticleRepository : BaseMongoRepository, IArticleRepository
{
    public ArticleRepository(IOptions<MongoOptions> options)
        : base(options, "demoArticles")
    {
    }

    public ICollection<Article> GetAll() => GetAll<Article>();

    public Article GetBySlug(string slug) => GetOne<Article>(new BsonDocument("slug", slug));
}

public interface IArtistRepository
{
    ICollection<Artist> GetAll();
}

public sealed class ArtistRepository : BaseMongoRepository, IArtistRepository
{
    public ArtistRepository(IOptions<MongoOptions> options)
        : base(options, "demoArtists")
    {
    }

    public ICollection<Artist> GetAll() => GetAll<Artist>();
}

public interface IEventRepository
{
    ICollection<Event> GetAll();

    Event GetBySlug(string slug);
}

public sealed class EventRepository : BaseMongoRepository, IEventRepository
{
    public EventRepository(IOptions<MongoOptions> options)
        : base(options, "demoEvents")
    {
    }

    public ICollection<Event> GetAll() => GetAll<Event>();

    public Event GetBySlug(string slug) => GetOne<Event>(new BsonDocument("slug", slug));
}
