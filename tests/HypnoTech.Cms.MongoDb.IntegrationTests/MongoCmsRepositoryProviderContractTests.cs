using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.MongoDb.Repositories;
using HypnoTech.MongoDb;
using HypnoTech.Persistence.ContractTests.Cms;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

[Collection(MongoPersistenceContractCollection.Name)]
public sealed class MongoCmsRepositoryProviderContractTests :
    CmsRepositoryProviderContractTests
{
    private readonly MongoCompatibilityFixture _fixture;

    public MongoCmsRepositoryProviderContractTests(MongoCompatibilityFixture fixture)
    {
        _fixture = fixture;
        RegisterClassMaps();
    }

    protected override IEnumerable<ICmsRepositoryContractContext> CreateContractContexts()
    {
        foreach (var target in _fixture.Targets)
        {
            yield return new MongoCmsRepositoryContractContext(target);
        }
    }

    private static void RegisterClassMaps()
    {
        RegisterClassMap<CmsContractArticle>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Heading).SetElementName("heading");
            map.GetMemberMap(model => model.Summary).SetElementName("summary");
            map.GetMemberMap(model => model.Published).SetElementName("published");
            map.GetMemberMap(model => model.Sections).SetElementName("sections");
            map.GetMemberMap(model => model.Photos).SetElementName("photos");
            map.GetMemberMap(model => model.ChildId)
                .SetElementName("childId")
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.UnmapMember(model => model.Child);
        });
        RegisterClassMap<CmsContractSection>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Name).SetElementName("name");
        });
        RegisterClassMap<CmsContractPhoto>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Path).SetElementName("path");
            map.GetMemberMap(model => model.Caption).SetElementName("caption");
        });
        RegisterClassMap<CmsContractOrderedDocument>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Name).SetElementName("name");
            map.GetMemberMap(model => model.Order).SetElementName("order");
        });
        RegisterClassMap<CmsContractNonSearchableDocument>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Name).SetElementName("name");
        });
        RegisterClassMap<CmsContractChild>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Name).SetElementName("name");
        });
        RegisterClassMap<CmsContractChoice>(map =>
        {
            MapObjectId(map, model => model.Id);
            map.GetMemberMap(model => model.Name).SetElementName("name");
        });
    }

    private static void RegisterClassMap<TModel>(Action<BsonClassMap<TModel>> configure)
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(TModel)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<TModel>(map =>
        {
            map.AutoMap();
            configure(map);
        });
    }

    private static void MapObjectId<TModel>(
        BsonClassMap<TModel> map,
        System.Linq.Expressions.Expression<Func<TModel, string>> member)
    {
        map.MapIdMember(member).SetSerializer(new StringSerializer(BsonType.ObjectId));
    }

    private sealed class MongoCmsRepositoryContractContext : ICmsRepositoryContractContext
    {
        private readonly IOptions<MongoOptions> _options;

        public MongoCmsRepositoryContractContext(MongoTarget target)
        {
            ProviderName = $"MongoDB {target.Name}";
            _options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"htpc_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
            });
            SelectListRepository = new SelectListRepository(_options);
            SeedRepository = new SeedRepository(_options);
        }

        public string ProviderName { get; }

        public ISelectListRepository SelectListRepository { get; }

        public ISeedRepository SeedRepository { get; }

        public string CreateIdentifier()
        {
            return ObjectId.GenerateNewId().ToString();
        }

        public ICmsRepository<TModel> CreateRepository<TModel>() where TModel : class
        {
            return new CmsRepository<TModel>(_options);
        }

        public Task SeedChildAsync(
            CmsContractChild child,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return Database().GetCollection<CmsContractChild>("persistenceContractChildren")
                .InsertOneAsync(child, cancellationToken: cancellationToken);
        }

        public Task SeedChoiceAsync(
            CmsContractChoice choice,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return Database().GetCollection<CmsContractChoice>("persistenceContractChoices")
                .InsertOneAsync(choice, cancellationToken: cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await new MongoClient(_options.Value.ConnectionString)
                .DropDatabaseAsync(_options.Value.Database);
        }

        private IMongoDatabase Database()
        {
            return new MongoClient(_options.Value.ConnectionString).GetDatabase(_options.Value.Database);
        }
    }
}
