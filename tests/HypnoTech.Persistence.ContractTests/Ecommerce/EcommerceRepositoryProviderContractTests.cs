using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Models;
using Xunit;

namespace HypnoTech.Persistence.ContractTests.Ecommerce;

public abstract class EcommerceRepositoryProviderContractTests
{
    protected abstract IEnumerable<IEcommerceRepositoryContractContext> CreateContractContexts();

    [Fact]
    public async Task Catalogue_groups_and_orders_items_and_supports_slug_lookup()
    {
        await ForEachContextAsync(async context =>
        {
            await SeedCatalogueAsync(context);

            var flattened = await context.MerchRepository.GetMerchFlattenedAsync(
                TestContext.Current.CancellationToken);
            var grouped = await context.MerchRepository.GetMerchAsync(
                TestContext.Current.CancellationToken);
            var audio = await context.MerchRepository.GetMerchItemAsync(
                "synthetic-audio",
                TestContext.Current.CancellationToken);

            Assert.Equal(["Synthetic print", "Synthetic audio"], flattened.Select(item => item.Name));
            Assert.Equal("Synthetic print", grouped.Single().SubCategories.First().Items.Single().Name);
            Assert.Equal("Synthetic audio", audio.Name);
        });
    }

    [Fact]
    public async Task Postage_profiles_preserve_values_and_protect_profiles_used_by_merch()
    {
        await ForEachContextAsync(async context =>
        {
            await SeedCatalogueAsync(context);
            var usedProfile = CreatePostageProfile("Used profile", 3.50m);
            var unusedProfile = CreatePostageProfile("Unused profile", 1.25m);
            await context.PostageProfileRepository.AddProfileAsync(
                usedProfile,
                TestContext.Current.CancellationToken);
            await context.PostageProfileRepository.AddProfileAsync(
                unusedProfile,
                TestContext.Current.CancellationToken);

            var profiles = await context.PostageProfileRepository.GetProfilesAsync(
                TestContext.Current.CancellationToken);
            Assert.False(profiles.Single(profile => profile.Name == "Used profile").CanDelete);
            Assert.True(profiles.Single(profile => profile.Name == "Unused profile").CanDelete);
            Assert.Contains(
                "cannot be removed",
                await context.PostageProfileRepository.RemoveProfileAsync(
                    usedProfile.Id,
                    TestContext.Current.CancellationToken));
            Assert.Null(await context.PostageProfileRepository.RemoveProfileAsync(
                unusedProfile.Id,
                TestContext.Current.CancellationToken));

            usedProfile.UnitedKingdom = new PostageRegion { Alone = 4.25m, WithOthers = 1.50m };
            await context.PostageProfileRepository.UpdateProfileAsync(
                usedProfile,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                4.25m,
                (await context.PostageProfileRepository.GetProfileAsync(
                    "Used profile",
                    TestContext.Current.CancellationToken)).UnitedKingdom.Alone);
        });
    }

    [Fact]
    public async Task Orders_preserve_lifecycle_country_enrichment_and_paging()
    {
        await ForEachContextAsync(async context =>
        {
            await context.SeedCountriesAsync(
            [
                new Country
                {
                    Code = "GB",
                    DisplayText = "United Kingdom",
                    Region = "UK"
                }
            ], TestContext.Current.CancellationToken);

            var completedOrder = CreateOrder("PAY-COMPLETE");
            await context.OrderRepository.SaveOrderAsync(
                completedOrder,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                "United Kingdom",
                (await context.OrderRepository.GetOrderAsync(
                    completedOrder.PaymentId,
                    TestContext.Current.CancellationToken)).Address.Country);

            var completed = await context.OrderRepository.CompleteOrderAsync(
                new PaymentCompletion
                {
                    PaymentId = completedOrder.PaymentId,
                    PayerId = "synthetic-payer",
                    SaleId = "synthetic-sale",
                    SaleUrl = "https://example.test/sale/synthetic-sale",
                    EmailAddress = "buyer@example.test"
                },
                TestContext.Current.CancellationToken);
            Assert.Equal("COMPLETE", completed.Status);
            Assert.Single((await context.OrderRepository.GetOrdersAsync(
                new PagedRequest { Page = 1, PageSize = 10 },
                TestContext.Current.CancellationToken)).Items);
            Assert.Equal(
                "SHIPPED",
                (await context.OrderRepository.MarkAsShippedAsync(
                    completedOrder.PaymentId,
                    TestContext.Current.CancellationToken)).Status);

            var cancelledOrder = CreateOrder("PAY-CANCELLED");
            await context.OrderRepository.SaveOrderAsync(
                cancelledOrder,
                TestContext.Current.CancellationToken);
            await context.OrderRepository.CancelOrderAsync(
                cancelledOrder.PaymentId,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                "CANCELLED",
                (await context.OrderRepository.GetOrderAsync(
                    cancelledOrder.PaymentId,
                    TestContext.Current.CancellationToken)).Status);
        });
    }

    [Fact]
    public async Task Download_codes_are_assigned_once_and_report_exhausted_collections()
    {
        await ForEachContextAsync(async context =>
        {
            var collection = new DownloadCodeCollection
            {
                Name = "Synthetic album",
                RedemptionUrl = "https://example.test/redeem",
                Codes =
                [
                    new DownloadCode { Code = "SYNTHETIC-ONE" },
                    new DownloadCode { Code = "SYNTHETIC-TWO" }
                ]
            };
            await context.DownloadCodeRepository.AddCodeCollectionAsync(
                collection,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                "SYNTHETIC-ONE",
                await context.DownloadCodeRepository.AssignDownloadCodeAsync(
                    collection.Id,
                    "first@example.test",
                    TestContext.Current.CancellationToken));

            var assignment = await context.DownloadCodeRepository.AssignDownloadCodesAsync(
                new Order
                {
                    EmailAddress = "second@example.test",
                    DownloadCodeIds = [collection.Id, collection.Id]
                },
                TestContext.Current.CancellationToken);
            Assert.Equal("SYNTHETIC-TWO", Assert.Single(assignment.AssignedDownloadCodes).Code);
            Assert.Equal("Synthetic album", Assert.Single(assignment.ExpiredCollections));
        });
    }

    [Fact]
    public async Task Ecommerce_queries_honour_an_already_cancelled_token()
    {
        await ForEachContextAsync(async context =>
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                context.MerchCategoryTypeRepository.GetTypesAsync(cancelled.Token));
        });
    }

    private async Task ForEachContextAsync(Func<IEcommerceRepositoryContractContext, Task> assertion)
    {
        foreach (var context in CreateContractContexts())
        {
            await using (context)
            {
                try
                {
                    await assertion(context);
                }
                catch (Exception exception)
                {
                    throw new Xunit.Sdk.XunitException(
                        $"Persistence provider '{context.ProviderName}' failed: {exception}");
                }
            }
        }
    }

    private static async Task SeedCatalogueAsync(IEcommerceRepositoryContractContext context)
    {
        await context.SeedMerchCategoryTypesAsync(
        [
            new MerchCategoryType
            {
                Name = "Shop",
                Order = 1,
                SubcategoryTypes =
                [
                    new MerchSubcategoryType { Name = "Prints", Order = 1 },
                    new MerchSubcategoryType { Name = "Audio", Order = 2 }
                ]
            }
        ], TestContext.Current.CancellationToken);
        await context.SeedMerchItemsAsync(
        [
            new MerchItem
            {
                Name = "Synthetic audio",
                Category = "Shop",
                SubCategory = "Audio",
                Price = "6.50",
                PostageProfile = "Used profile",
                Slug = "synthetic-audio",
                Options = []
            },
            new MerchItem
            {
                Name = "Synthetic print",
                Category = "Shop",
                SubCategory = "Prints",
                Price = "14.00",
                PostageProfile = "Used profile",
                Slug = "synthetic-print",
                Options = []
            }
        ], TestContext.Current.CancellationToken);
    }

    private static PostageProfile CreatePostageProfile(string name, decimal alone)
    {
        return new PostageProfile
        {
            Name = name,
            UnitedKingdom = new PostageRegion { Alone = alone, WithOthers = 1m },
            EuropeanUnion = new PostageRegion { Alone = alone, WithOthers = 1m },
            RestOfWorld1 = new PostageRegion { Alone = alone, WithOthers = 1m },
            RestOfWorld2 = new PostageRegion { Alone = alone, WithOthers = 1m },
            RestOfWorld3 = new PostageRegion { Alone = alone, WithOthers = 1m }
        };
    }

    private static Order CreateOrder(string paymentId)
    {
        return new Order
        {
            DateTime = new DateTime(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc),
            PaymentId = paymentId,
            Status = "CREATED",
            Address = new PostageAddress { CountryCode = "GB" },
            Items = [],
            DownloadCodeIds = []
        };
    }
}
