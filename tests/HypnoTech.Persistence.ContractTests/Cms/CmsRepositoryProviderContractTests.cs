using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Common;
using HypnoTech.Common.Models;
using Xunit;

namespace HypnoTech.Persistence.ContractTests.Cms;

public abstract class CmsRepositoryProviderContractTests : IDisposable
{
    protected CmsRepositoryProviderContractTests()
    {
        RegisterMappings();
    }

    protected abstract IEnumerable<ICmsRepositoryContractContext> CreateContractContexts();

    public void Dispose()
    {
        FormClassMapping.Maps.Clear();
        ModelRegistry.Registrations.RemoveAll(
            registration => registration.Type == typeof(CmsContractChoice));
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Crud_round_trip_preserves_values_and_generated_identity()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            var article = new CmsContractArticle
            {
                Heading = "Original",
                Summary = "Synthetic provider contract",
                Published = new DateTime(2026, 5, 6, 12, 0, 0, DateTimeKind.Utc)
            };

            await repository.AddAsync(article, TestContext.Current.CancellationToken);

            Assert.False(string.IsNullOrWhiteSpace(article.Id));
            var added = await repository.GetByIdAsync(article.Id, TestContext.Current.CancellationToken);
            Assert.Equal("Original", added.Heading);
            Assert.Equal(article.Published, added.Published);

            added.Heading = "Edited";
            await repository.EditAsync(added, TestContext.Current.CancellationToken);

            var all = await repository.GetAllAsync<CmsContractArticle>(
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("Edited", Assert.Single(all).Heading);

            await repository.RemoveAsync(article.Id, TestContext.Current.CancellationToken);
            Assert.Empty(await repository.GetAllAsync<CmsContractArticle>(
                cancellationToken: TestContext.Current.CancellationToken));
        });
    }

    [Fact]
    public async Task Adds_are_not_idempotent_and_generate_distinct_identifiers()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            var first = new CmsContractArticle { Heading = "Equivalent content" };
            var second = new CmsContractArticle { Heading = "Equivalent content" };

            await repository.AddAsync(first, TestContext.Current.CancellationToken);
            await repository.AddAsync(second, TestContext.Current.CancellationToken);

            Assert.NotEqual(first.Id, second.Id);
            Assert.Equal(2, (await repository.GetAllAsync<CmsContractArticle>(
                cancellationToken: TestContext.Current.CancellationToken)).Count);
        });
    }

    [Fact]
    public async Task Removing_an_already_removed_item_is_an_idempotent_no_op()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            var article = new CmsContractArticle { Heading = "Remove twice" };
            await repository.AddAsync(article, TestContext.Current.CancellationToken);

            await repository.RemoveAsync(article.Id, TestContext.Current.CancellationToken);
            await repository.RemoveAsync(article.Id, TestContext.Current.CancellationToken);

            Assert.Empty(await repository.GetAllAsync<CmsContractArticle>(
                cancellationToken: TestContext.Current.CancellationToken));
        });
    }

    [Fact]
    public async Task Paging_applies_default_and_explicit_sort_with_stable_page_counts()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            await AddArticlesAsync(repository,
                ("Charlie", 1),
                ("Alpha", 3),
                ("Echo", 2),
                ("Bravo", 5),
                ("Delta", 4));

            var defaultPage = await repository.GetPagedAsync<CmsContractArticle>(new CmsListRequest
            {
                Page = 1,
                PageSize = 2
            }, TestContext.Current.CancellationToken);
            var explicitPage = await repository.GetPagedAsync<CmsContractArticle>(new CmsListRequest
            {
                Page = 2,
                PageSize = 2,
                SortColumn = 0,
                Direction = SortType.Ascending
            }, TestContext.Current.CancellationToken);

            Assert.Equal(3, defaultPage.TotalPages);
            Assert.Equal(SortType.Descending, defaultPage.Direction);
            Assert.Equal(["Bravo", "Delta"], defaultPage.Items.Select(item => item.Heading));
            Assert.Equal(["Charlie", "Delta"], explicitPage.Items.Select(item => item.Heading));
        });
    }

    [Fact]
    public async Task Search_is_case_insensitive_literal_and_spans_searchable_members()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            await repository.AddAsync(
                new CmsContractArticle { Heading = "Literal [draft]", Summary = "ordinary" },
                TestContext.Current.CancellationToken);
            await repository.AddAsync(
                new CmsContractArticle { Heading = "Published", Summary = "Contains NEEDLE here" },
                TestContext.Current.CancellationToken);
            await repository.AddAsync(
                new CmsContractArticle { Heading = "Needle heading", Summary = "ordinary" },
                TestContext.Current.CancellationToken);

            var escaped = await repository.GetPagedAsync<CmsContractArticle>(
                new CmsListRequest { Search = "[draft]" },
                TestContext.Current.CancellationToken);
            var acrossMembers = await repository.GetPagedAsync<CmsContractArticle>(
                new CmsListRequest { Search = "needle" },
                TestContext.Current.CancellationToken);

            Assert.Equal("Literal [draft]", Assert.Single(escaped.Items).Heading);
            Assert.Equal(2, acrossMembers.Items.Count);
        });
    }

    [Fact]
    public async Task Search_with_no_searchable_members_returns_an_empty_page()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractNonSearchableDocument>();
            await repository.AddAsync(
                new CmsContractNonSearchableDocument { Name = "Present but not searchable" },
                TestContext.Current.CancellationToken);

            var result = await repository.GetPagedAsync<CmsContractNonSearchableDocument>(
                new CmsListRequest { Search = "Present" },
                TestContext.Current.CancellationToken);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalPages);
        });
    }

    [Fact]
    public async Task Ordered_movement_swaps_adjacent_items_and_boundaries_are_no_ops()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractOrderedDocument>();
            var first = new CmsContractOrderedDocument { Name = "First", Order = 1 };
            var second = new CmsContractOrderedDocument { Name = "Second", Order = 2 };
            var third = new CmsContractOrderedDocument { Name = "Third", Order = 3 };
            await repository.AddAsync(first, TestContext.Current.CancellationToken);
            await repository.AddAsync(second, TestContext.Current.CancellationToken);
            await repository.AddAsync(third, TestContext.Current.CancellationToken);

            await repository.MoveDownAsync(first.Id, TestContext.Current.CancellationToken);

            Assert.Equal(2, (await repository.GetByIdAsync(
                first.Id,
                TestContext.Current.CancellationToken)).Order);
            Assert.Equal(1, (await repository.GetByIdAsync(
                second.Id,
                TestContext.Current.CancellationToken)).Order);

            await repository.MoveUpAsync(second.Id, TestContext.Current.CancellationToken);
            await repository.MoveDownAsync(third.Id, TestContext.Current.CancellationToken);

            Assert.Equal(1, (await repository.GetByIdAsync(
                second.Id,
                TestContext.Current.CancellationToken)).Order);
            Assert.Equal(3, (await repository.GetByIdAsync(
                third.Id,
                TestContext.Current.CancellationToken)).Order);
        });
    }

    [Fact]
    public async Task Add_generates_unique_nested_list_and_gallery_identifiers()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            var article = new CmsContractArticle
            {
                Heading = "Nested identifiers",
                Sections =
                [
                    new CmsContractSection { Name = "First" },
                    new CmsContractSection { Name = "Second" }
                ],
                Photos =
                [
                    new CmsContractPhoto { Path = "/Images/one.png", Caption = "One" },
                    new CmsContractPhoto { Path = "/Images/two.png", Caption = "Two" }
                ]
            };

            await repository.AddAsync(article, TestContext.Current.CancellationToken);
            var loaded = await repository.GetByIdAsync(article.Id, TestContext.Current.CancellationToken);
            var identifiers = loaded.Sections.Select(section => section.Id)
                .Concat(loaded.Photos.Select(photo => photo.Id))
                .ToList();

            Assert.All(identifiers, identifier => Assert.False(string.IsNullOrWhiteSpace(identifier)));
            Assert.Equal(identifiers.Count, identifiers.Distinct(StringComparer.Ordinal).Count());
        });
    }

    [Fact]
    public async Task Edit_replaces_nested_list_and_gallery_values()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            var article = new CmsContractArticle
            {
                Heading = "Nested edit",
                Sections = [new CmsContractSection { Name = "Before" }],
                Photos = [new CmsContractPhoto { Path = "/Images/before.png", Caption = "Before" }]
            };
            await repository.AddAsync(article, TestContext.Current.CancellationToken);

            var loaded = await repository.GetByIdAsync(
                article.Id,
                TestContext.Current.CancellationToken);
            loaded.Sections = [new CmsContractSection { Name = "After" }];
            loaded.Photos =
            [
                new CmsContractPhoto { Path = "/Images/after.png", Caption = "After" },
                new CmsContractPhoto { Path = "/Images/second.png", Caption = "Second" }
            ];

            await repository.EditAsync(loaded, TestContext.Current.CancellationToken);

            var edited = await repository.GetByIdAsync(
                article.Id,
                TestContext.Current.CancellationToken);
            Assert.Equal("After", Assert.Single(edited.Sections).Name);
            Assert.Equal(["After", "Second"], edited.Photos.Select(photo => photo.Caption));
            Assert.All(
                edited.Sections.Select(section => section.Id)
                    .Concat(edited.Photos.Select(photo => photo.Id)),
                identifier => Assert.False(string.IsNullOrWhiteSpace(identifier)));
        });
    }

    [Fact]
    public async Task Registered_child_is_populated_and_a_null_reference_remains_unset()
    {
        await ForEachContextAsync(async context =>
        {
            var child = new CmsContractChild
            {
                Id = context.CreateIdentifier(),
                Name = "Referenced child"
            };
            await context.SeedChildAsync(child, TestContext.Current.CancellationToken);
            var repository = context.CreateRepository<CmsContractArticle>();
            var withChild = new CmsContractArticle
            {
                Heading = "Has child",
                ChildId = child.Id
            };
            var withoutChild = new CmsContractArticle { Heading = "No child" };
            await repository.AddAsync(withChild, TestContext.Current.CancellationToken);
            await repository.AddAsync(withoutChild, TestContext.Current.CancellationToken);

            var loadedWithChild = await repository.GetByIdAsync(
                withChild.Id,
                TestContext.Current.CancellationToken);
            var loadedWithoutChild = await repository.GetByIdAsync(
                withoutChild.Id,
                TestContext.Current.CancellationToken);

            Assert.Equal("Referenced child", loadedWithChild.Child.Name);
            Assert.Null(loadedWithoutChild.Child);
        });
    }

    [Fact]
    public async Task Select_lists_use_registered_model_types_and_preserve_values()
    {
        await ForEachContextAsync(async context =>
        {
            await context.SeedChoiceAsync(
                new CmsContractChoice
                {
                    Id = context.CreateIdentifier(),
                    Name = "Choice A"
                },
                TestContext.Current.CancellationToken);

            var choices = await context.SelectListRepository.GetSelectListItemsAsync(
                typeof(CmsContractChoice),
                TestContext.Current.CancellationToken);

            Assert.Equal("Choice A", Assert.IsType<CmsContractChoice>(Assert.Single(choices)).Name);
        });
    }

    [Fact]
    public async Task Seeding_populates_only_an_empty_logical_store()
    {
        var seedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SeedData");
        var seedName = $"provider-contract-{Guid.NewGuid():N}";
        var seedPath = Path.Combine(seedDirectory, $"{seedName}.json");
        Directory.CreateDirectory(seedDirectory);
        await File.WriteAllTextAsync(
            seedPath,
            "[{\"heading\":\"Seeded provider contract article\"}]",
            TestContext.Current.CancellationToken);

        try
        {
            await ForEachContextAsync(async context =>
            {
                await context.SeedRepository.SeedAsync<CmsContractArticle>(
                    "persistenceContractArticles",
                    seedName,
                    TestContext.Current.CancellationToken);
                await context.SeedRepository.SeedAsync<CmsContractArticle>(
                    "persistenceContractArticles",
                    seedName,
                    TestContext.Current.CancellationToken);

                var articles = await context.CreateRepository<CmsContractArticle>()
                    .GetAllAsync<CmsContractArticle>(
                        cancellationToken: TestContext.Current.CancellationToken);
                Assert.Equal(
                    "Seeded provider contract article",
                    Assert.Single(articles).Heading);
            });
        }
        finally
        {
            File.Delete(seedPath);
        }
    }

    [Fact]
    public async Task Operations_honour_an_already_cancelled_token()
    {
        await ForEachContextAsync(async context =>
        {
            var repository = context.CreateRepository<CmsContractArticle>();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                repository.GetAllAsync<CmsContractArticle>(cancellationToken: cancelled.Token));
        });
    }

    private async Task ForEachContextAsync(Func<ICmsRepositoryContractContext, Task> assertion)
    {
        foreach (var context in CreateContractContexts())
        {
            await using (context)
            {
                try
                {
                    await assertion(context);
                }
                catch (Exception exception) when (exception is not Xunit.Sdk.XunitException)
                {
                    throw new Xunit.Sdk.XunitException(
                        $"Persistence provider '{context.ProviderName}' failed: {exception}");
                }
            }
        }
    }

    private static async Task AddArticlesAsync(
        ICmsRepository<CmsContractArticle> repository,
        params (string Heading, int Day)[] values)
    {
        foreach (var value in values)
        {
            await repository.AddAsync(new CmsContractArticle
            {
                Heading = value.Heading,
                Published = new DateTime(2026, 1, value.Day, 12, 0, 0, DateTimeKind.Utc)
            }, TestContext.Current.CancellationToken);
        }
    }

    private static void RegisterMappings()
    {
        FormClassMapping.Maps.Clear();
        FormClassMapping.RegisterClassMapping<CmsContractArticle>(mapping =>
        {
            mapping.SetControllerName("PersistenceContractArticles");
            mapping.SetDisplayName("Persistence Contract Article");
            mapping.AddDataSource("persistenceContractArticles");
            mapping.MapHiddenInputMember(article => article.Id);
            mapping.MapTextInputMember(article => article.Heading).IncludeInTable(0).Searchable();
            mapping.MapTextAreaInputMember(article => article.Summary).Searchable();
            mapping.MapDateInputMember(article => article.Published).IncludeInTable(1);
            mapping.MapListInputMember<CmsContractSection, List<CmsContractSection>>(article => article.Sections)
                .MapMembers(section =>
                {
                    section.MapId(item => item.Id);
                    section.MapTextInputMember(item => item.Name, isNameField: true);
                });
            mapping.MapPhotoGalleryInputMember<CmsContractPhoto, List<CmsContractPhoto>>(article => article.Photos)
                .MapMembers(photo => photo.Id, photo => photo.Path, photo => photo.Caption);
            mapping.WithDefaultSort(article => article.Published, ascending: false);
            mapping.RegisterChild(
                article => article.Child,
                article => article.ChildId,
                "persistenceContractChildren");
        });
        FormClassMapping.RegisterClassMapping<CmsContractNonSearchableDocument>(mapping =>
        {
            mapping.SetDisplayName("Persistence Contract Non-searchable Document");
            mapping.AddDataSource("persistenceContractNonSearchableDocuments");
            mapping.MapHiddenInputMember(document => document.Id);
            mapping.MapTextInputMember(document => document.Name).IncludeInTable(0);
        });
        FormClassMapping.RegisterClassMapping<CmsContractOrderedDocument>(mapping =>
        {
            mapping.SetDisplayName("Persistence Contract Ordered Document");
            mapping.AddDataSource("persistenceContractOrderedDocuments");
            mapping.MapHiddenInputMember(document => document.Id);
            mapping.MapTextInputMember(document => document.Name).IncludeInTable(0);
            mapping.MapHiddenInputMember(document => document.Order);
            mapping.CanBeOrdered(document => document.Order);
        });
        ModelRegistry.Register(
            typeof(CmsContractChoice),
            "persistenceContractChoices",
            "Persistence Contract Choices");
    }
}
