using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Cms.MongoDb.Repositories;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

[Collection(MongoPersistenceContractCollection.Name)]
public sealed partial class CmsRepositoryContractTests : IDisposable
{
    private readonly MongoCompatibilityFixture _fixture;

    public CmsRepositoryContractTests(MongoCompatibilityFixture fixture)
    {
        _fixture = fixture;
        RegisterMappings();
    }

    public void Dispose()
    {
        FormClassMapping.Maps.Clear();
    }

    [Fact]
    public async Task Add_uses_persisted_bson_names_and_generates_nested_list_ids()
    {
        await ForEachTargetAsync(async (repository, options) =>
        {
            var article = new SyntheticArticle
            {
                Heading = "BSON contract",
                Sections = [new SyntheticSection { Name = "Generated ID" }],
                Photos = [new SyntheticPhoto { Path = "/Images/synthetic.png", Caption = "Synthetic" }]
            };
            await repository.AddAsync(article);
            var document = await Database(options)
                .GetCollection<BsonDocument>("syntheticArticles")
                .Find(Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(article.Id)))
                .SingleAsync();

            Assert.True(document.Contains("heading"));
            Assert.False(document.Contains(nameof(SyntheticArticle.Heading)));
            var section = document["sections"].AsBsonArray.Single().AsBsonDocument;
            var photo = document["photos"].AsBsonArray.Single().AsBsonDocument;
            Assert.True(section["_id"].IsObjectId);
            Assert.True(photo["_id"].IsObjectId);
        });
    }

    [Fact]
    public async Task GetById_populates_registered_child_from_legacy_reference()
    {
        FormClassMapping.GetMapping<SyntheticArticle>()
            .RegisterChild(article => article.Child, article => article.ChildId, "syntheticChildren");

        await ForEachTargetAsync(async (repository, options) =>
        {
            var childId = ObjectId.GenerateNewId();
            await Database(options).GetCollection<BsonDocument>("syntheticChildren").InsertOneAsync(new BsonDocument
            {
                { "_id", childId },
                { "name", "Referenced child" }
            });
            var article = new SyntheticArticle { Heading = "Has child", ChildId = childId.ToString() };
            await repository.AddAsync(article);

            var loaded = await repository.GetByIdAsync(article.Id);

            Assert.NotNull(loaded.Child);
            Assert.Equal("Referenced child", loaded.Child.Name);
        });
    }

    [Fact]
    public async Task Representative_legacy_document_with_missing_optional_fields_deserialises()
    {
        await ForEachTargetAsync(async (repository, options) =>
        {
            var id = ObjectId.GenerateNewId();
            await Database(options).GetCollection<BsonDocument>("syntheticArticles").InsertOneAsync(new BsonDocument
            {
                { "_id", id },
                { "heading", "Legacy minimum" }
            });

            var article = await repository.GetByIdAsync(id.ToString());

            Assert.Equal("Legacy minimum", article.Heading);
            Assert.Null(article.Summary);
            Assert.Empty(article.Sections);
        });
    }

    private async Task ForEachTargetAsync(
        Func<CmsRepository<SyntheticArticle>, IOptions<MongoOptions>, Task> assertion)
    {
        foreach (var target in _fixture.Targets)
        {
            var options = Options.Create(new MongoOptions
            {
                Server = target.Host,
                Port = target.Port,
                Database = $"hypnotech_{target.Name.Replace('-', '_').Replace('.', '_')}_{Guid.NewGuid():N}"
            });

            try
            {
                await assertion(new CmsRepository<SyntheticArticle>(options), options);
            }
            catch (Exception exception)
            {
                throw new Xunit.Sdk.XunitException($"MongoDB target '{target.Name}' failed: {exception}");
            }
            finally
            {
                await new MongoClient(options.Value.ConnectionString)
                    .DropDatabaseAsync(options.Value.Database);
            }
        }
    }

    private static IMongoDatabase Database(IOptions<MongoOptions> options)
    {
        return new MongoClient(options.Value.ConnectionString).GetDatabase(options.Value.Database);
    }

    private static void RegisterMappings()
    {
        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<SyntheticArticle>(mapping =>
        {
            mapping.SetControllerName("SyntheticArticles");
            mapping.SetDisplayName("Synthetic Article");
            mapping.AddDataSource("syntheticArticles");
            mapping.MapHiddenInputMember(article => article.Id);
            mapping.MapTextInputMember(article => article.Heading).IncludeInTable(0).Searchable();
            mapping.MapTextAreaInputMember(article => article.Summary).Searchable();
            mapping.MapDateInputMember(article => article.Published).IncludeInTable(1);
            mapping.MapListInputMember<SyntheticSection, List<SyntheticSection>>(article => article.Sections)
                .MapMembers(section =>
                {
                    section.MapId(item => item.Id);
                    section.MapTextInputMember(item => item.Name, isNameField: true);
                });
            mapping.MapPhotoGalleryInputMember<SyntheticPhoto, List<SyntheticPhoto>>(article => article.Photos)
                .MapMembers(photo => photo.Id, photo => photo.Path, photo => photo.Caption);
            mapping.WithDefaultSort(article => article.Published, ascending: false);
        });
    }

    public sealed class SyntheticArticle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("heading")]
        public string Heading { get; set; }

        [BsonElement("summary")]
        public string Summary { get; set; }

        [BsonElement("published")]
        public DateTime? Published { get; set; }

        [BsonElement("sections")]
        public List<SyntheticSection> Sections { get; set; } = new();

        [BsonElement("photos")]
        public List<SyntheticPhoto> Photos { get; set; } = new();

        [BsonElement("childId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChildId { get; set; }

        [BsonIgnore]
        public SyntheticChild Child { get; set; }
    }

    public sealed class SyntheticSection
    {
        [BsonElement("id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }

    public sealed class SyntheticChild
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }

    public sealed class SyntheticPhoto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("path")]
        public string Path { get; set; }

        [BsonElement("caption")]
        public string Caption { get; set; }
    }

}
