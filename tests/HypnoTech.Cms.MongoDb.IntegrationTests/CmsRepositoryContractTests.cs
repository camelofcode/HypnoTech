using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Cms.MongoDb.Repositories;
using HypnoTech.Common;
using HypnoTech.Common.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

public sealed partial class CmsRepositoryContractTests : IClassFixture<MongoCompatibilityFixture>, IDisposable
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
        ModelRegistry.Registrations.RemoveAll(registration => registration.Type == typeof(SyntheticChoice));
    }

    [Fact]
    public async Task Async_crud_paging_and_ordering_work_on_all_supported_targets()
    {
        await ForEachTargetAsync(async (repository, options) =>
        {
            var article = new SyntheticArticle
            {
                Heading = "Async original",
                Summary = "Synthetic async content",
                Published = new DateTime(2026, 5, 6, 12, 0, 0, DateTimeKind.Utc)
            };

            await repository.AddAsync(article);
            var added = await repository.GetByIdAsync(article.Id);
            added.Heading = "Async edited";
            await repository.EditAsync(added);

            var all = await repository.GetAllAsync<SyntheticArticle>();
            var page = await repository.GetPagedAsync<SyntheticArticle>(new CmsListRequest
            {
                Page = 1,
                PageSize = 10
            });

            Assert.Equal("Async edited", Assert.Single(all).Heading);
            Assert.Equal("Async edited", Assert.Single(page.Items).Heading);
            Assert.Equal(1, page.TotalPages);

            var orderedRepository = new CmsRepository<OrderedDocument>(options);
            var first = new OrderedDocument { Name = "Async first", Order = 1 };
            var second = new OrderedDocument { Name = "Async second", Order = 2 };
            await orderedRepository.AddAsync(first);
            await orderedRepository.AddAsync(second);
            await orderedRepository.MoveDownAsync(first.Id);

            Assert.Equal(2, (await orderedRepository.GetByIdAsync(first.Id)).Order);
            Assert.Equal(1, (await orderedRepository.GetByIdAsync(second.Id)).Order);

            await repository.RemoveAsync(article.Id);
            Assert.Empty(await repository.GetAllAsync<SyntheticArticle>());
        });
    }

    [Fact]
    public async Task Paging_default_sort_explicit_sort_and_total_pages_are_consistent()
    {
        await ForEachTargetAsync(async (repository, _) =>
        {
            await AddArticlesAsync(repository,
                ("Charlie", 1),
                ("Alpha", 3),
                ("Echo", 2),
                ("Bravo", 5),
                ("Delta", 4));

            var defaultPage = await repository.GetPagedAsync<SyntheticArticle>(new CmsListRequest
            {
                Page = 1,
                PageSize = 2
            });
            var explicitPage = await repository.GetPagedAsync<SyntheticArticle>(new CmsListRequest
            {
                Page = 2,
                PageSize = 2,
                SortColumn = 0,
                Direction = SortType.Ascending
            });

            Assert.Equal(3, defaultPage.TotalPages);
            Assert.Equal(["Bravo", "Delta"], defaultPage.Items.Select(item => item.Heading));
            Assert.Equal(["Charlie", "Delta"], explicitPage.Items.Select(item => item.Heading));
        });
    }

    [Fact]
    public async Task Search_is_case_insensitive_escaped_and_checks_all_searchable_members()
    {
        await ForEachTargetAsync(async (repository, _) =>
        {
            await repository.AddAsync(new SyntheticArticle { Heading = "Literal [draft]", Summary = "ordinary" });
            await repository.AddAsync(new SyntheticArticle { Heading = "Published", Summary = "Contains NEEDLE here" });
            await repository.AddAsync(new SyntheticArticle { Heading = "Needle heading", Summary = "ordinary" });

            var escaped = await repository.GetPagedAsync<SyntheticArticle>(new CmsListRequest { Search = "[draft]" });
            var acrossMembers = await repository.GetPagedAsync<SyntheticArticle>(new CmsListRequest { Search = "needle" });

            Assert.Equal("Literal [draft]", Assert.Single(escaped.Items).Heading);
            Assert.Equal(2, acrossMembers.Items.Count);
        });
    }

    [Fact]
    public async Task Search_with_no_searchable_members_returns_no_items()
    {
        await ForEachTargetAsync(async (_, options) =>
        {
            var repository = new CmsRepository<NonSearchableDocument>(options);
            await repository.AddAsync(new NonSearchableDocument { Name = "Present but not searchable" });

            var result = await repository.GetPagedAsync<NonSearchableDocument>(new CmsListRequest { Search = "Present" });

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalPages);
        });
    }

    [Fact]
    public async Task Ordered_movement_swaps_adjacent_items_and_boundary_moves_are_no_ops()
    {
        await ForEachTargetAsync(async (_, options) =>
        {
            var repository = new CmsRepository<OrderedDocument>(options);
            var first = new OrderedDocument { Name = "First", Order = 1 };
            var second = new OrderedDocument { Name = "Second", Order = 2 };
            var third = new OrderedDocument { Name = "Third", Order = 3 };
            await repository.AddAsync(first);
            await repository.AddAsync(second);
            await repository.AddAsync(third);

            await repository.MoveDownAsync(first.Id);

            Assert.Equal(2, (await repository.GetByIdAsync(first.Id)).Order);
            Assert.Equal(1, (await repository.GetByIdAsync(second.Id)).Order);
            await repository.MoveUpAsync(second.Id);
            await repository.MoveDownAsync(third.Id);

            Assert.Equal(1, (await repository.GetByIdAsync(second.Id)).Order);
            Assert.Equal(3, (await repository.GetByIdAsync(third.Id)).Order);
        });
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
    public async Task GetByIdAsync_populates_registered_child_from_legacy_reference()
    {
        FormClassMapping.GetMapping<SyntheticArticle>()
            .RegisterChild(article => article.Child, article => article.ChildId, "syntheticChildren");

        await ForEachTargetAsync(async (repository, options) =>
        {
            var childId = ObjectId.GenerateNewId();
            await Database(options).GetCollection<BsonDocument>("syntheticChildren").InsertOneAsync(
                new BsonDocument
                {
                    { "_id", childId },
                    { "name", "Async referenced child" }
                });
            var article = new SyntheticArticle { Heading = "Async child", ChildId = childId.ToString() };
            await repository.AddAsync(article);

            var loaded = await repository.GetByIdAsync(article.Id);

            Assert.NotNull(loaded.Child);
            Assert.Equal("Async referenced child", loaded.Child.Name);
        });
    }

    [Fact]
    public async Task Registered_child_with_null_reference_leaves_child_unset()
    {
        FormClassMapping.GetMapping<SyntheticArticle>()
            .RegisterChild(article => article.Child, article => article.ChildId, "syntheticChildren");

        await ForEachTargetAsync(async (repository, _) =>
        {
            var article = new SyntheticArticle { Heading = "No child reference" };
            await repository.AddAsync(article);

            var loaded = await repository.GetByIdAsync(article.Id);

            Assert.Null(loaded.Child);
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

    [Fact]
    public async Task Select_list_repository_uses_registered_collection_and_deserialises_items()
    {
        ModelRegistry.Register(typeof(SyntheticChoice), "syntheticChoices", "Synthetic Choices");

        await ForEachTargetAsync(async (_, options) =>
        {
            await Database(options).GetCollection<BsonDocument>("syntheticChoices").InsertOneAsync(
                new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "name", "Choice A" }
                });

            var repository = new SelectListRepository(options);
            var choices = await repository.GetSelectListItemsAsync(typeof(SyntheticChoice));

            Assert.Equal("Choice A", Assert.IsType<SyntheticChoice>(Assert.Single(choices)).Name);
        });
    }

    [Fact]
    public async Task SeedAsync_inserts_only_when_collection_is_empty()
    {
        var seedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SeedData");
        var seedName = $"async-characterisation-{Guid.NewGuid():N}";
        var seedPath = Path.Combine(seedDirectory, $"{seedName}.json");
        Directory.CreateDirectory(seedDirectory);
        await File.WriteAllTextAsync(
            seedPath,
            "[{\"heading\":\"Async seeded synthetic article\"}]",
            TestContext.Current.CancellationToken);

        try
        {
            await ForEachTargetAsync(async (_, options) =>
            {
                var repository = new SeedRepository(options);

                await repository.SeedAsync<SyntheticArticle>(
                    "asyncSeededSyntheticArticles",
                    seedName,
                    TestContext.Current.CancellationToken);
                await repository.SeedAsync<SyntheticArticle>(
                    "asyncSeededSyntheticArticles",
                    seedName,
                    TestContext.Current.CancellationToken);

                var documents = await Database(options)
                    .GetCollection<BsonDocument>("asyncSeededSyntheticArticles")
                    .Find(new BsonDocument())
                    .ToListAsync(TestContext.Current.CancellationToken);
                Assert.Equal(
                    "Async seeded synthetic article",
                    Assert.Single(documents)["heading"].AsString);
            });
        }
        finally
        {
            File.Delete(seedPath);
        }
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

    private static async Task AddArticlesAsync(
        CmsRepository<SyntheticArticle> repository,
        params (string Heading, int Day)[] values)
    {
        foreach (var value in values)
        {
            await repository.AddAsync(new SyntheticArticle
            {
                Heading = value.Heading,
                Published = new DateTime(2026, 1, value.Day, 12, 0, 0, DateTimeKind.Utc)
            });
        }
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
        FormClassMapping.RegisterClassMapping<NonSearchableDocument>(mapping =>
        {
            mapping.SetDisplayName("Non-searchable document");
            mapping.AddDataSource("nonSearchableDocuments");
            mapping.MapHiddenInputMember(document => document.Id);
            mapping.MapTextInputMember(document => document.Name).IncludeInTable(0);
        });
        FormClassMapping.RegisterClassMapping<OrderedDocument>(mapping =>
        {
            mapping.SetDisplayName("Ordered document");
            mapping.AddDataSource("orderedDocuments");
            mapping.MapHiddenInputMember(document => document.Id);
            mapping.MapTextInputMember(document => document.Name).IncludeInTable(0);
            mapping.MapHiddenInputMember(document => document.Order);
            mapping.CanBeOrdered(document => document.Order);
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

    public sealed class NonSearchableDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }

    public sealed class OrderedDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }
    }

    public sealed class SyntheticChoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}
